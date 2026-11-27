#!/usr/bin/env bash
# Linux/macOS counterpart of start_local_server.ps1 (called by start_server.sh).
# Builds once, then runs the local game (8080), update (8081) and GM (8082) services.
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
project="$(cd -- "$script_dir/../../.." && pwd)"
cd -- "$project"

die() { printf 'Error: %s\n' "$*" >&2; exit 1; }

python_bin="${PYTHON:-}"
if [[ -z "$python_bin" ]]; then
    if command -v python3 >/dev/null 2>&1; then python_bin=python3; else python_bin=python; fi
fi
command -v "$python_bin" >/dev/null 2>&1 || die 'Python 3 is required (set PYTHON to override).'
command -v cargo >/dev/null 2>&1 || die 'cargo is required to build the server.'
command -v curl >/dev/null 2>&1 || die 'curl is required for readiness checks.'

# Load .env literally (same rules as import_local_env.ps1): no shell evaluation,
# and nonempty variables already set in the terminal take precedence.
import_local_env() {
    local path="$1" line name value quote line_number=0
    [[ -f "$path" ]] || return 0
    local assign_re='^[[:space:]]*(export[[:space:]]+)?([A-Za-z_][A-Za-z0-9_]*)[[:space:]]*=[[:space:]]*(.*)$'
    while IFS= read -r line || [[ -n "$line" ]]; do
        line_number=$((line_number + 1))
        line="${line%$'\r'}"
        [[ "$line" =~ ^[[:space:]]*(#.*)?$ ]] && continue
        [[ "$line" =~ $assign_re ]] || die "Invalid .env assignment at line $line_number."
        name="${BASH_REMATCH[2]}"
        value="${BASH_REMATCH[3]}"
        value="${value%"${value##*[![:space:]]}"}"
        if [[ "$value" == \"* || "$value" == \'* ]]; then
            quote="${value:0:1}"
            local quoted_re="^${quote}([^${quote}]*)${quote}[[:space:]]*(#.*)?$"
            [[ "$value" =~ $quoted_re ]] || die "Invalid .env quoted value at line $line_number. Use a single-line value."
            value="${BASH_REMATCH[1]}"
        else
            [[ "$value" =~ ^(.*[^[:space:]])?[[:space:]]+#.*$ ]] && value="${BASH_REMATCH[1]}"
            value="${value%"${value##*[![:space:]]}"}"
        fi
        if [[ -z "${!name:-}" ]]; then
            export "$name=$value"
        fi
    done < "$path"
}
import_local_env "$project/.env"

updates="$project/client-updates"
# Without a local client update (a fresh clone), the update service downloads the
# newest published release from GitHub once and verifies its signature. Existing
# local releases are never replaced: this only happens when none exists.
github_tag=''
github_repo="${SPRK_UPDATES_GITHUB_REPO:-faberuser/sprk}"
if [[ ! -f "$updates/stable/manifest.json" ]]; then
    echo "No local client update yet. Looking up the newest client release of $github_repo on GitHub..."
    if ! github_tag="$("$python_bin" "$project/client/scripts/release/latest_client_release.py" --repo "$github_repo" | tail -n 1)" \
        || [[ -z "${github_tag//[[:space:]]/}" ]]; then
        die "Could not find a published client release on GitHub ($github_repo). Check the internet connection, or publish
a local client update: $python_bin client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version local-1"
    fi
    github_tag="$(printf '%s' "$github_tag" | tr -d '[:space:]')"
    mkdir -p -- "$updates"
fi

port_in_use() {
    if command -v ss >/dev/null 2>&1; then
        ss -Hltn "sport = :$1" 2>/dev/null | grep -q .
    else
        (exec 3<>"/dev/tcp/127.0.0.1/$1") 2>/dev/null
    fi
}
for port in 8080 8081; do
    if port_in_use "$port"; then
        die "Port $port is already in use. Close the existing local server before running start_server.sh."
    fi
done

# The native worker is a generated view of the installed client. Recreate it
# after target cleanup, before paying for a build or starting any services.
"$python_bin" "$project/server/scripts/worker/prepare_local_worker.py" \
    || die 'Battle worker preparation failed; no services were started.'

echo 'Starting server...'
cargo build --release || die 'Server build failed; no services were started.'
executable="$project/target/release/sprk-server"
logs="$project/target/local-server"
mkdir -p -- "$logs"

# Game database: PostgreSQL. Without DATABASE_URL (in .env or the terminal),
# run a loopback-only server in Docker with a persistent volume.
if [[ -z "${DATABASE_URL:-}" ]]; then
    container='sprk-postgres-local'
    db_port="${SPRK_LOCAL_DB_PORT:-54320}"
    command -v docker >/dev/null 2>&1 \
        || die 'Docker is needed for the local PostgreSQL database. Install Docker or set DATABASE_URL.'
    status="$(docker ps -a --filter "name=^${container}\$" --format '{{.Status}}')" \
        || die 'Docker is not reachable. Start the Docker daemon (and check you may use it without sudo) or set DATABASE_URL.'
    if [[ -z "$status" ]]; then
        docker run -d --name "$container" -e POSTGRES_PASSWORD=sprk-local -e POSTGRES_DB=sprk \
            -p "127.0.0.1:${db_port}:5432" -v sprk-postgres-local:/var/lib/postgresql postgres:18 >/dev/null \
            || die 'Could not start the local PostgreSQL container.'
    elif [[ "$status" != Up* ]]; then
        docker start "$container" >/dev/null || die 'Could not start the local PostgreSQL container.'
    fi
    # Probe TCP: during first-time initialization the server listens on its socket only.
    ready=false
    for ((attempt = 0; attempt < 120; attempt++)); do
        if docker exec "$container" pg_isready -h 127.0.0.1 -U postgres -d sprk >/dev/null 2>&1; then
            ready=true; break
        fi
        sleep 0.5
    done
    $ready || die 'The local PostgreSQL container did not become ready.'
    export DATABASE_URL="postgres://postgres:sprk-local@127.0.0.1:${db_port}/sprk"
fi

update_pid=''
cleanup() {
    if [[ -n "$update_pid" ]] && kill -0 "$update_pid" 2>/dev/null; then
        kill "$update_pid" 2>/dev/null || true
        wait "$update_pid" 2>/dev/null || true
    fi
}
trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM HUP

# Readiness checks run every 250 ms: 10 seconds normally, 30 minutes for a download.
ready_attempts=40
update_env=(SPRK_UPDATES_DIR="$updates" SPRK_SERVICE_MODE=updates PORT=8081)
if [[ -n "$github_tag" ]]; then
    # Only the update service syncs from GitHub; later starts serve the local copy.
    update_env+=(
        SPRK_UPDATES_GITHUB_REPO="$github_repo"
        SPRK_UPDATES_GITHUB_TAG="$github_tag"
        SPRK_UPDATES_PUBLIC_KEY_FILE="$project/config/client-update-public.pem"
    )
    ready_attempts=7200
    echo "Downloading and verifying client release $github_tag (about 400 MB, first start only)..."
fi
env "${update_env[@]}" "$executable" >"$logs/updates.log" 2>"$logs/updates-error.log" </dev/null &
update_pid=$!
unset SPRK_UPDATES_GITHUB_TAG SPRK_UPDATES_GITHUB_REPO SPRK_UPDATES_PUBLIC_KEY_FILE

ready=false
for ((attempt = 0; attempt < ready_attempts; attempt++)); do
    kill -0 "$update_pid" 2>/dev/null || die "Update service exited. Check $logs/updates-error.log"
    if curl -fsS --max-time 2 -o /dev/null 'http://127.0.0.1:8081/updates/stable/manifest.json' 2>/dev/null; then
        ready=true; break
    fi
    sleep 0.25
done
$ready || die 'Local update service did not become ready on port 8081.'

export SPRK_UPDATES_DIR="$updates"
export SPRK_SERVICE_MODE=game
export PORT=8080
export SERVER_HOST='127.0.0.1:8080'
export SERVER_HTTPS=false
export SERVER_NAME=Local
export LOGIN_SERVER='http://127.0.0.1:8080/'
export GAME_TABLES_PATH="$project/server/tables"
export CHAT_BIND=127.0.0.1
export BATTLE_BIND=127.0.0.1
export SPRK_WEBSOCKET_ORIGIN='ws://127.0.0.1:8080'
export SPRK_GM_BIND='127.0.0.1:8082'
export RUST_LOG="${RUST_LOG:-info,tower_http=debug}"
# Clients download game assets from the local asset store (docs/game-assets.md).
# Android needs it: without a patch CDN it has no asset list and stops at
# "Ready AssetBundle". Set SPRK_ASSETS_CDN_URL=off to serve the empty /cdn/ patch.
if [[ "${SPRK_ASSETS_CDN_URL:-}" == off ]]; then
    unset SPRK_ASSETS_CDN_URL
elif [[ -z "${SPRK_ASSETS_CDN_URL:-}" && -d "$updates/assets" ]]; then
    export SPRK_ASSETS_CDN_URL='http://127.0.0.1:8081/updates/assets/'
fi

echo 'Game:    http://127.0.0.1:8080'
echo 'Updates: http://127.0.0.1:8081/updates/stable/manifest.json'
[[ -n "${SPRK_ASSETS_CDN_URL:-}" ]] && echo "Assets:  $SPRK_ASSETS_CDN_URL"
if [[ -n "${SPRK_GM_KEY:-}" ]]; then
    echo 'GM:      http://127.0.0.1:8082 (administrator key required)'
else
    echo 'GM disabled: set SPRK_GM_KEY in .env to enable it.'
fi
echo 'Press Ctrl+C to stop all local services.'

set +e
"$executable"
code=$?
set -e
if [[ $code -ne 0 && $code -ne 130 ]]; then
    die "Game server exited with code $code."
fi
