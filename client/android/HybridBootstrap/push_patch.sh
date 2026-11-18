#!/usr/bin/env bash
# Populate the game's patch folder on the device. The SPRK code keeps the
# "StandaloneWindows" folder name on every platform; on Android its root is the app's
# external files dir (UnityCache.persistentDataPath).
#   tables / localization / plain assets / video: from the installed Windows client
#     (platform-neutral data, including SPRK's table changes)
#   Assetbundle: the original Android bundle set (GLES shaders, ETC textures) with its
#     own BundlePack indexes. (LoginPresentation is SPRK json/png/music, platform-neutral.)
# Incremental: a top-level item already present on the device is not pushed again
# unless --force is given. Never modifies the sources.
#
# usage: client/android/HybridBootstrap/push_patch.sh [--force] [adb] [serial]
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
FORCE=0; [ "${1:-}" = --force ] && { FORCE=1; shift; }
ADB="${1:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"
SERIAL="${2:-emulator-5554}"
ROOT="$(cd "$HERE/../../../.." && pwd)"
WIN="$ROOT/sprk-client/King's Raid_Data/Documents/Patch/StandaloneWindows"
ANDROID="$ROOT/Pre-Doomsday/com.vespainteractive.KingsRaid/files/Documents/Patch/Android/Assetbundle"
DEST=/sdcard/Android/data/com.sprk.hybridprobe/files/Documents/Patch/StandaloneWindows
NEUTRAL=(AiScript ContentsDefine LocalizationJit LoginPresentation PatchInfo PlainAsset TableData TableJit Video patch.json patch.bjs SPRK-NativeCombat.json SPRK-NativeData.json SPRK-NativeLocalization.json)

export MSYS_NO_PATHCONV=1
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
adb() { "$ADB" -s "$SERIAL" "$@"; }
# This BlueStacks image refuses adb push's own directory creation under
# Android/data/<pkg> ("secure_mkdirs failed"), but `adb shell mkdir -p` works:
# create the whole tree first.
mkdirs() {
  local src="$1" dest="$2"
  # Relative names after a cd keep each command under adb's 4 KB service-name limit.
  (cd "$(dirname "$src")" && find "$(basename "$src")" -type d) |
    while mapfile -t -n 20 batch && [ ${#batch[@]} -gt 0 ]; do
      adb shell "cd '$dest' && mkdir -p $(printf "'%s' " "${batch[@]}")" >/dev/null </dev/null
    done
}
push_tree() { [ -d "$1" ] && mkdirs "$1" "$2"; adb push "$(win "$1")" "$2/" >/dev/null; }
present() { adb shell "[ -e '$1' ] && echo yes || true" 2>/dev/null | tr -d '\r'; }
adb shell mkdir -p "$DEST" >/dev/null
for item in "${NEUTRAL[@]}"; do
  [ -e "$WIN/$item" ] || continue
  if [ $FORCE = 0 ] && [ "$(present "$DEST/$item")" = yes ]; then echo "skip $item"; continue; fi
  echo "push $item"; push_tree "$WIN/$item" "$DEST"
done
if [ $FORCE = 0 ] && [ "$(present "$DEST/Assetbundle/BundlePack.bin")" = yes ]; then
  echo "skip Assetbundle"
else
  echo "push Assetbundle (android set)"; push_tree "$ANDROID" "$DEST"
fi
# The Android set's BundlePack.bin is the old client's format ("Unexpected binary read" in
# LoaderPlaying.LoadPackFromBinary); the current code reads the Windows index, whose bundle
# names the Android files share.
adb push "$(win "$WIN/Assetbundle/BundlePack.bin")" "$DEST/Assetbundle/BundlePack.bin" >/dev/null
adb shell du -sh "$DEST" | tr -d '\r'
