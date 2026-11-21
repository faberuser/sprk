#!/usr/bin/env bash
# Write an index override that mounts the repacked built-in content and starts the given
# scene (unless SPRK_SERVER_PICKER=1, it also skips the server picker for the local server).
# The content itself is in the APK (SprkAndroidBuild.StageContent). Copies pushed by older
# versions of this script would take precedence over the APK's, so they are removed.
#
# usage: [SPRK_SERVER_PICKER=1] client/android/HybridBootstrap/push_content.sh [scene=Login] [adb] [serial]
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
SCENE="${1:-Login}"
ADB="${2:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"
SERIAL="${3:-emulator-5554}"
PKG=com.sprk.hybridprobe
DEST="/sdcard/Android/data/$PKG/files/sprk"
# Mount order: shaders before anything that references them, scenes last.
FILES=(sprk-shaders.unity3d sprk-ggm.unity3d sprk-resources.unity3d sprk-scenes.unity3d)

export MSYS_NO_PATHCONV=1
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
adb() { "$ADB" -s "$SERIAL" "$@"; }
adb shell mkdir -p "$DEST" >/dev/null
adb shell rm -rf "$DEST/content" >/dev/null
index="$(mktemp)"
grep -v '^bundle:\|^content:\|^start:' "$HERE/Assets/StreamingAssets/sprk/index.txt" > "$index"
for f in "${FILES[@]}"; do
  echo "content:$f" >> "$index"
done
echo "start:$SCENE" >> "$index"
# Unattended runs use the local server; SPRK_SERVER_PICKER=1 shows the server picker instead.
[ "${SPRK_SERVER_PICKER:-0}" = 1 ] || echo "server:local" >> "$index"
adb push "$(win "$index")" "$DEST/index.txt" >/dev/null
rm -f "$index"
echo "index for ${#FILES[@]} content bundles (in the APK), start scene $SCENE"
