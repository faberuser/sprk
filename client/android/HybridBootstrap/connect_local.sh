#!/usr/bin/env bash
# The Android build reaches the local server through 127.0.0.1 on the device, forwarded to
# this PC by adb. The forwarding is lost whenever adb or BlueStacks restarts; rerun this.
# usage: client/android/HybridBootstrap/connect_local.sh [adb] [serial]
ADB="${1:-/c/Program Files/BlueStacks_nxt/HD-Adb.exe}"; SERIAL="${2:-emulator-5554}"
"$ADB" connect 127.0.0.1:5555 >/dev/null 2>&1
for port in 8080 8081 9001 9002; do "$ADB" -s "$SERIAL" reverse tcp:$port tcp:$port >/dev/null; done
"$ADB" -s "$SERIAL" reverse --list
