#!/usr/bin/env bash
# Install (optional) and launch the HybridCLR probe on a device/emulator, wait for
# it to finish or crash, and print the probe report plus any crash header.
#
# usage: client/android/HybridBootstrap/run_probe.sh [--install] [adb] [serial]
set -uo pipefail
export MSYS_NO_PATHCONV=1
HERE="$(cd "$(dirname "$0")" && pwd)"
INSTALL=0; [ "${1:-}" = --install ] && { INSTALL=1; shift; }
ADB="${1:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"
SERIAL="${2:-emulator-5554}"
PKG=com.sprk.hybridprobe
OUT="$HERE/../../../artifacts/android-bootstrap"
mkdir -p "$OUT"
adb() { "$ADB" -s "$SERIAL" "$@"; }
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }

if [ $INSTALL = 1 ]; then
  adb install -r "$(win "$HERE/Build/sprk-hybrid-probe.apk")" | tail -1
fi
adb shell am force-stop $PKG
adb logcat -G 64M >/dev/null 2>&1
adb logcat -c
log="$OUT/probe-logcat.txt"
adb logcat -v time Unity:V CRASH:E DEBUG:E AndroidRuntime:E '*:S' > "$log" &
cat_pid=$!
adb shell am start -n $PKG/com.unity3d.player.UnityPlayerGameActivity >/dev/null
for _ in $(seq 1 60); do
  sleep 2
  grep -q "RESULT:\|SIGSEGV\|SIGABRT" "$log" && break
done
sleep 2
adb exec-out screencap -p > "$OUT/probe-screen.png"
kill $cat_pid 2>/dev/null
echo "== probe report"
adb shell cat /sdcard/Android/data/$PKG/files/sprk-probe.txt
echo "== crash / errors"
grep -a "CRASH.*signal\|CRASH.*Cause\|E/Unity" "$log" | grep -av "^\s*$" | cut -c1-220 | sort | uniq -c | sort -rn | head -15
echo "logcat: $log  screenshot: $OUT/probe-screen.png"
