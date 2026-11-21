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
OUT="$HERE/../../../../sprk-android/bootstrap"
mkdir -p "$OUT"
adb() { "$ADB" -s "$SERIAL" "$@"; }
win() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }

if [ $INSTALL = 1 ]; then
  adb install -r "$(win "$OUT/apk/sprk-hybrid-probe.apk")" | tail -1
fi
adb shell am force-stop $PKG
# The game reaches the local server on 127.0.0.1; forward it (and chat 9001 / battle 9002)
# to the isolated local stack on this PC.
for port in 8080 8081 9001 9002; do adb reverse tcp:$port tcp:$port >/dev/null; done
adb logcat -G 64M >/dev/null 2>&1
adb logcat -c
log="$OUT/probe-logcat.txt"
probe=/sdcard/Android/data/$PKG/files/sprk-probe.txt
adb shell rm -f $probe
adb shell monkey -p $PKG -c android.intent.category.LAUNCHER 1 >/dev/null 2>&1   # launcher activity, whatever its class
# Wait for the bootstrap's result or a crash. (No background `adb logcat` stream: killing
# it from Git Bash leaves the native adb process holding the output open.)
for _ in $(seq 1 60); do
  sleep 2
  adb shell cat $probe 2>/dev/null | grep -q "RESULT:\|server picker" && break
  adb logcat -d CRASH:E '*:S' 2>/dev/null | grep -q "SIGSEGV\|SIGABRT" && break
done
sleep 2
adb logcat -d -v time Unity:V CRASH:E DEBUG:E AndroidRuntime:E '*:S' > "$log" 2>/dev/null
adb exec-out screencap -p > "$OUT/probe-screen.png"
echo "== probe report"
adb shell cat /sdcard/Android/data/$PKG/files/sprk-probe.txt
echo "== crash / errors"
grep -a "CRASH.*signal\|CRASH.*Cause\|E/Unity" "$log" | grep -av "^\s*$" | cut -c1-220 | sort | uniq -c | sort -rn | head -15
echo "logcat: $log  screenshot: $OUT/probe-screen.png"
