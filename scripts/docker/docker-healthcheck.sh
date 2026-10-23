#!/bin/sh
set -eu
base="http://127.0.0.1:${PORT:-8080}"
curl --fail --silent "$base/health" >/dev/null
case "${SPRK_SERVICE_MODE:-game}" in
  game|all) curl --fail --silent "$base/health/battle" >/dev/null ;;
esac
