#!/bin/sh
set -eu

case "${SPRK_SERVICE_MODE:-game}" in
  game|all)
    # The native worker shares only this container's private TCP listener.
    # Persist its authentication key beside the DB; never bake it into an image.
    if [ -z "${BATTLE_SERVICE_KEY:-}" ]; then
      export BATTLE_SERVICE_KEY_FILE="${BATTLE_SERVICE_KEY_FILE:-/data/conquest-service.key}"
      if [ ! -e "$BATTLE_SERVICE_KEY_FILE" ]; then
        umask 077
        temporary=$(mktemp "${BATTLE_SERVICE_KEY_FILE}.XXXXXX")
        openssl rand -hex 32 > "$temporary"
        # An existing key wins if two starts race. Never rotate it on restart.
        ln "$temporary" "$BATTLE_SERVICE_KEY_FILE" 2>/dev/null || test -s "$BATTLE_SERVICE_KEY_FILE"
        rm -f "$temporary"
      fi
      test -s "$BATTLE_SERVICE_KEY_FILE"
    fi
    export BATTLE_WORKER_EXECUTABLE="${BATTLE_WORKER_EXECUTABLE:-/usr/local/bin/sprk-battle-worker}"
    ;;
esac
exec /usr/local/bin/sprk-server "$@"
