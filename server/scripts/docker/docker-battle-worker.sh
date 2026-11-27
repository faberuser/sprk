#!/bin/sh
set -eu
# The Rust supervisor supplies the key, host, port and bootstrap URL and
# restarts this process on exit. Wine and Xvfb run as the unprivileged game UID.
# With BATTLE_WORKERS > 1 several workers start at the same moment. Each gets
# its own Wine prefix (one wineserver each) and X display: started together on
# a shared prefix, one fails with "error opening file descriptor for wineserver
# tmpdir file", and `xvfb-run -a` can pick the same free display twice.
worker="${SPRK_BATTLE_WORKER:-1}"
case "$worker" in ''|*[!0-9]*) echo "SPRK_BATTLE_WORKER must be a number" >&2; exit 1 ;; esac
export WINEPREFIX="/tmp/sprk-worker-wine-$worker"
export WINEARCH=win64
export WINEDEBUG=-all
export WINEDLLOVERRIDES='mscoree,mshtml,winemenubuilder.exe=d'
mkdir -p "$WINEPREFIX"
chmod 700 "$WINEPREFIX"
log=/data/conquest-worker.log
while [ "$#" -gt 0 ]; do
  case "$1" in
    -logFile) log="$2"; shift 2 ;;
    *) shift ;;
  esac
done
cd /opt/sprk-worker
exec xvfb-run -n "$((98 + worker))" /usr/lib/wine/wine64 "King's Raid.exe" \
  -batchmode -nographics -logFile "Z:$log"
