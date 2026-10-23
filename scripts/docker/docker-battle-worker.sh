#!/bin/sh
set -eu
# The Rust supervisor supplies the key, host, port and bootstrap URL and
# restarts this process on exit. Wine and Xvfb run as the unprivileged game UID.
export WINEPREFIX=/tmp/sprk-worker-wine
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
exec xvfb-run -a /usr/lib/wine/wine64 "King's Raid.exe" \
  -batchmode -nographics -logFile "Z:$log"
