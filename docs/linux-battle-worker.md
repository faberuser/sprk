# Linux battle worker

Challenge Raid, Guild Conquest and other online battles require the original
Unity combat engine. The Linux/amd64 game image runs the Windows player under
Debian's Wine with Xvfb, `-batchmode` and `-nographics`. It uses the same signed
client release pinned to the server image. No Windows host, display, GPU or
additional public port is required.

## Build inputs

The existing client-update archive contains managed code and baked tables, but
not the Unity player, embedded Mono runtime and startup scenes. These stable
runtime files are a separate immutable GitHub release, pinned by SHA-256 in
`config/worker-runtime.json`. The seed tool explicitly selects engine and scene
files; it excludes launcher profiles, accounts, logs, service keys, managed code
and the large downloaded asset cache.

One-time runtime preparation, from the repository root:

```powershell
python scripts/linux_worker.py seed --client ../sprk-client --archive target/worker-runtime.tar.gz
```

This creates the archive and updates the tracked pin. Publish that archive under
the exact tag in the pin before merging the deployment changes. Use a new tag
for changed runtime files, never replace an existing asset. No signing private
key belongs in this archive or the container. Routine client patches do not
require rebuilding this runtime seed.

For CI, `scripts/linux_worker.py build` downloads the pinned runtime and the
image's exact client release. It verifies the runtime hash, signed manifest,
every client file's size/hash and the tag/version relationship. Only a complete
verified directory is activated as `build-input/worker`. The image copies it to
`/opt/sprk-worker`. Failed or unsafe archives cannot become image inputs.

Local image validation (use a compatible published client tag):

```powershell
python scripts/linux_worker.py build --tag client-<version>
docker build -t sprk-linux-worker:test .
python scripts/smoke_linux_worker.py sprk-linux-worker:test
docker build --target native-tests -t sprk-native-tests:test .
docker run --rm --init sprk-native-tests:test conquest_native_tcp_battle --ignored --nocapture
docker run --rm --init sprk-native-tests:test native_challenge_party_combat_and_settlement --ignored --nocapture
```

`--runtime <archive>` and `--release <archive>` allow local archives, with the
same verification. Choose a fresh `--output` directory for repeat assembly;
Docker expects the final input at `build-input/worker`. Preparation does not
modify the installed Windows client or the local stable update manifest.

CI builds and loads the image, boots an isolated game container, and requires
worker readiness and native Challenge Raid/Conquest combat tests before pushing
either GHCR tag or invoking Portainer. The
smoke container has no production database or published ports and is removed
after the check. Updates mode does not start Wine or generate a worker key.

## Startup and diagnosis

The entrypoint preserves an existing `BATTLE_SERVICE_KEY` or key file. Otherwise
it creates `/data/conquest-service.key` with private permissions in the existing
game-data volume. The Rust supervisor passes the key through the worker's
environment and restarts the worker on exit. Keep `BATTLE_BIND=127.0.0.1`; port
9002 must not be published. The worker uses an ephemeral Wine prefix under
`/tmp`, and logs to `/data/conquest-worker.log`.

`GET /health` remains the HTTP liveness check. `GET /health/battle` returns 200
only with a connected worker supporting Arena and Challenge Raid; otherwise it
returns 503. It exposes no keys or player data. The game container healthcheck
requires both endpoints and gives Wine three minutes to initialize. The
updates container checks HTTP liveness only.

Successful game logs contain `Native battle worker authenticated` with
`arena_version=1` and `challenge_raid_version=1`. If the container stays
unhealthy, inspect `/data/conquest-worker.log` along with the game logs. A
listening battle socket alone does not mean the engine is available.

The existing Portainer stack and game-data volume remain in place. Deploy the
new image through the normal development PR/main CI workflow. Restarting a game
container cancels active battles, so deploy between sessions. Confirm readiness
and enter Challenge Raid and Guild Conquest after the rollout.
