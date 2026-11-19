#!/usr/bin/env bash
# Push freshly built game assemblies (client/dev.py build output) as hot-update overrides,
# so a C# change is testable without rebuilding the APK. The bootstrap prefers
# persistentDataPath/sprk/hot/<Name>.dll.bytes over the copies inside the APK.
# Rebuild the APK when AOT generic usage changes (HybridCLR metadata) or to drop overrides.
#
# usage: client/android/HybridBootstrap/push_hot.sh [adb] [serial]
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
ADB="${1:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"
SERIAL="${2:-emulator-5554}"
SRC="$HERE/../../../artifacts/client-build/client-code/King's Raid_Data/Managed"
DEST=/sdcard/Android/data/com.sprk.hybridprobe/files/sprk/hot
export MSYS_NO_PATHCONV=1
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }
stage="$(mktemp -d)"
tr -d '\r' < "$HERE/HotUpdate/hot-update.txt" | while read -r name; do
  if [ -n "$name" ]; then cp "$SRC/$name.dll" "$stage/$name.dll.bytes"; fi
done
"$ADB" -s "$SERIAL" shell mkdir -p "$DEST" >/dev/null
"$ADB" -s "$SERIAL" push "$(win "$stage")/." "$DEST/" >/dev/null
echo "pushed $(ls "$stage" | wc -l) hot assemblies"
rm -rf "$stage"
