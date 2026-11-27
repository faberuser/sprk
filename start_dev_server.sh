#!/usr/bin/env bash
# Build and run a test server from this checkout (for example the development
# branch) on a Linux host with Docker, without CI. It runs the same Docker
# Compose stack as production, from an image built here:
#
#   1. Battle worker: pinned runtime + pinned baseline client release
#      + client/overrides/files + code compiled from client/managed
#      (server/scripts/worker/development_worker.py, run in a .NET container).
#   2. Image sprk:development (server compiled from source, with that worker).
#   3. docker compose up with that image; configuration comes from .env.
#
# Players' game files are not built here: upload a client release from your
# computer into SPRK_UPDATES_HOST_DIR (client-updates). The update service of
# this image never downloads releases from GitHub.
#
# Builds use committed files only (git HEAD): commit or pull first.
#
#   ./start_dev_server.sh             build everything, then (re)start the stack
#   ./start_dev_server.sh --no-build  restart the stack with the last built image
set -euo pipefail

project="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd -- "$project"
die() { printf 'Error: %s\n' "$*" >&2; exit 1; }

build=true
case "${1:-}" in
    '') ;;
    --no-build) build=false ;;
    -h|--help) sed -n '2,19p' "$0" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) die "Unknown option $1 (use --no-build or --help)" ;;
esac

command -v docker >/dev/null 2>&1 || die 'Docker is required.'
docker compose version >/dev/null 2>&1 || die 'The Docker Compose plugin is required.'
command -v git >/dev/null 2>&1 || die 'git is required.'
command -v curl >/dev/null 2>&1 || die 'curl is required.'
[[ -f .env ]] || die 'Create .env first (copy .env.example and fill it in).'

image='sprk:development'
work="$project/target/dev-server"
mkdir -p -- "$work/downloads"

if $build; then
    commit="$(git rev-parse --short=12 HEAD)"
    branch="$(git rev-parse --abbrev-ref HEAD)"
    echo "Building $branch at $commit (committed files only)."
    [[ -z "$(git status --porcelain --untracked-files=no)" ]] \
        || echo 'Note: uncommitted changes are not included in this build.'

    # Inputs pinned in the repository; their hashes and the release signature
    # are verified by the worker build. Public releases: no login needed.
    pinned() { sed -nE "s/^ *\"$1\": *\"([^\"]+)\".*/\1/p" "$2" | head -n 1; }
    runtime_repo="$(pinned repository config/worker-runtime.json)"
    runtime_tag="$(pinned tag config/worker-runtime.json)"
    runtime_asset="$(pinned asset config/worker-runtime.json)"
    baseline_tag="$(sed -nE 's/^ *"tag": *"([^"]+)".*/\1/p' client/scripts/release/client_baseline.py)"
    baseline_repo="$(sed -nE 's/^ *"repository": *"([^"]+)".*/\1/p' client/scripts/release/client_baseline.py)"
    [[ -n "$runtime_tag" && -n "$baseline_tag" ]] || die 'Could not read the pinned worker inputs.'
    fetch() { # repo tag asset -> cached file path
        local target="$work/downloads/$2-$3"
        if [[ ! -s "$target" ]]; then
            echo "Downloading $2/$3..." >&2
            curl -fsSL --retry 3 -o "$target.partial" "https://github.com/$1/releases/download/$2/$3" \
                || die "Download failed: $1 $2 $3"
            mv -- "$target.partial" "$target"
        fi
        printf '%s' "$target"
    }
    runtime="$(fetch "$runtime_repo" "$runtime_tag" "$runtime_asset")"
    baseline="$(fetch "$baseline_repo" "$baseline_tag" client-update.tar.gz)"

    # The .NET SDK must match client/global.json exactly (a newer feature band
    # refuses to build), so the worker is compiled in that SDK's container.
    sdk="$(sed -nE 's/.*"version": *"([^"]+)".*/\1/p' client/global.json | head -n 1)"
    [[ -n "$sdk" ]] || die 'Could not read the .NET SDK version from client/global.json.'
    echo "Preparing .NET $sdk build tools..."
    docker build -q -t "sprk-dev-tools:$sdk" - >/dev/null <<EOF || die 'Could not prepare the .NET build image.'
FROM mcr.microsoft.com/dotnet/sdk:$sdk
RUN apt-get update && apt-get install -y --no-install-recommends python3 && rm -rf /var/lib/apt/lists/*
EOF

    echo 'Assembling the battle worker (baseline + overrides + compiled game code)...'
    rm -rf -- "$work/worker" "$work/worker.new"
    mkdir -p -- "$work/worker.new"
    # Build from a clean copy of HEAD inside the container: nothing in this
    # checkout is written, and every file in the result is owned by this user.
    git archive --format=tar HEAD | docker run --rm -i \
        --user "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp -e DOTNET_NOLOGO=1 \
        -v "$runtime:/inputs/runtime.tar.gz:ro" -v "$baseline:/inputs/baseline.tar.gz:ro" \
        -v "$work/worker.new:/output" \
        "sprk-dev-tools:$sdk" bash -c '
            set -e
            mkdir /tmp/repo && tar -x -C /tmp/repo && cd /tmp/repo
            python3 server/scripts/worker/development_worker.py --output /tmp/worker \
                --runtime /inputs/runtime.tar.gz --baseline /inputs/baseline.tar.gz
            cp -a /tmp/worker/. /output/' \
        || die 'Battle worker build failed; the running server was not changed.'
    mv -- "$work/worker.new" "$work/worker"

    echo "Building image $image (compiling the server; the first build takes a while)..."
    # The Dockerfile reads build-input/worker; swap it in only for this build.
    staged="$project/build-input/worker"
    previous="$project/build-input/worker.before-dev-build"
    [[ ! -e "$previous" ]] || die "Remove the leftover $previous first."
    mkdir -p -- "$project/build-input"
    [[ ! -e "$staged" ]] || mv -- "$staged" "$previous"
    mv -- "$work/worker" "$staged"
    restore() {
        [[ -d "$staged" ]] && mv -- "$staged" "$work/worker"
        [[ -e "$previous" ]] && mv -- "$previous" "$staged"
        return 0
    }
    if ! docker build --target production \
            --label "org.opencontainers.image.revision=$(git rev-parse HEAD)" \
            -t "$image" "$project"; then
        restore; die 'Image build failed; the running server was not changed.'
    fi
    restore
fi

docker image inspect "$image" >/dev/null 2>&1 || die "No $image yet: run without --no-build first."

# The game image is local: never let Compose look for it in a registry. Fetch
# the database image separately, since --pull never would not download it.
echo 'Starting the stack...'
docker compose pull --quiet postgres postgres-backup
SPRK_IMAGE="$image" docker compose up -d --pull never --remove-orphans

port="$(sed -nE 's/^SPRK_GAME_PORT=([0-9]+).*/\1/p' .env | tail -n 1)"
port="${port:-8080}"
echo "Waiting for the game server and battle workers on port $port..."
for ((attempt = 0; attempt < 180; attempt++)); do
    if curl -fsS --max-time 2 -o /dev/null "http://127.0.0.1:$port/health/battle" 2>/dev/null; then
        echo "Ready: $image ($(docker image inspect -f '{{index .Config.Labels "org.opencontainers.image.revision"}}' "$image" | cut -c1-12))."
        echo 'Logs: docker compose logs -f game   Worker log: docker compose exec game tail -f /data/conquest-worker.log'
        exit 0
    fi
    sleep 2
done
die "The server did not become ready within 6 minutes. Check: docker compose logs game"
