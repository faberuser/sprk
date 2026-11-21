#!/usr/bin/env bash
# Push a small, fixed sample of original asset bundles (Android 2018 builds and
# Windows 2020.3 builds of the same assets) to the probe app and extend its
# index so the next launch runs the content probe. Never modifies the sources.
#
# usage: client/android/HybridBootstrap/push_bundle_probe.sh [android|windows|both] [adb] [serial]
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
WHICH="${1:-android}"
ADB="${2:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"
SERIAL="${3:-emulator-5554}"
PLATFORMS=(android windows); [ "$WHICH" != both ] && PLATFORMS=("$WHICH")
PKG=com.sprk.hybridprobe
ROOT="$(cd "$HERE/../../../.." && pwd)"
# The maintained Android bundle set (client/android/dev.py assets); before it is built,
# the original Android bundles pinned in ../sprk-android/base-apk.
ANDROID="$ROOT/sprk-android/build/Assetbundle"
[ -d "$ANDROID" ] || ANDROID="$ROOT/sprk-android/base-apk/Assetbundle"
WINDOWS="$ROOT/sprk-client/King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle"
DEST="/sdcard/Android/data/$PKG/files/sprk"

# Dependency order: shaders first, then the model bundle, its animations, the prefab.
SAMPLE=(Shader.unity3d OriginalSpec/Creature_Boss_Altigma_Star0.unity3d Creature_Boss_Altigma_Animation.unity3d Creature_Boss_Altigma.unity3d)

# Git Bash would rewrite device paths like /sdcard/... into Windows paths.
export MSYS_NO_PATHCONV=1
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
adb() { "$ADB" -s "$SERIAL" "$@"; }
index="$(mktemp)"
grep -v '^bundle:' "$HERE/Assets/StreamingAssets/sprk/index.txt" > "$index"
for platform in "${PLATFORMS[@]}"; do
  src="$ANDROID"; [ "$platform" = windows ] && src="$WINDOWS"
  for rel in "${SAMPLE[@]}"; do
    # The root Android Shader.unity3d is a stale Unity 5.6 build; the 2018.4 client used Unity_2018/.
    [ "$platform" = android ] && [ "$rel" = Shader.unity3d ] && rel=Unity_2018/Shader.unity3d
    adb shell mkdir -p "$DEST/bundles/$platform/$(dirname "$rel")" >/dev/null
    adb push "$(win "$src/$rel")" "$DEST/bundles/$platform/$rel" >/dev/null
    echo "bundle:$platform/$rel" >> "$index"
  done
done
adb push "$(win "$index")" "$DEST/index.txt" >/dev/null
rm -f "$index"
echo "pushed ${#SAMPLE[@]} bundles for: ${PLATFORMS[*]} (index override)"
