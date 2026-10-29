# Client update service

The Windows launcher is implemented under `launcher/`. See
[launcher setup](../launcher/README.md) to build `SprkLauncher.exe`, configure
its update URL, and distribute it alongside the client.

The same `sprk-server` executable/image runs either service, or both. Select the
mode before starting the process:

| Variable            | Default          | Purpose                                                                   |
| ------------------- | ---------------- | ------------------------------------------------------------------------- |
| `SPRK_SERVICE_MODE` | `game`           | `game`, `updates`, or `all`; invalid values fail startup                  |
| `SPRK_UPDATES_DIR`  | `client-updates` | Existing directory containing published releases; used in `updates`/`all` |
| `PORT`              | `8080`           | HTTP listening port for this process                                      |

`game` initializes the game database, tables, chat/battle listeners, and any
configured native worker. `updates` skips all of them. `all` initializes both
services; update requests stay outside the game's state/middleware. `/health`
works in every mode. Existing installations default to game mode.

## Publish a client release

To replace manual SFTP uploads with verified, startup-only downloads from GitHub
Releases, see [GitHub client update sync](github-client-updates.md). Each server
image pins its compatible client release; running servers do not poll for changes.

Run from the repository root with Python 3.10+:

```powershell
python scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version 1.0.0 --signing-key client-updates/update-signing/private.pem
```

The default selection includes all files under:

- `King's Raid_Data/app.info` (Unity startup company/product identity)
- `King's Raid_Data/Managed`
- `King's Raid_Data/Documents/Patch/StandaloneWindows/TableData`
- `King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit`
- `King's Raid_Data/Documents/Patch/StandaloneWindows/LocalizationJit`
- `King's Raid_Data/Documents/Patch/StandaloneWindows/Video`

Normal client launches write `%USERPROFILE%\AppData\LocalLow\sprk\King's Raid\Player.log`.
The full patcher enables native `usePlayerLog` and aligns `app.info` with the
`sprk` PlayerSettings identity; Unity chooses its initial log path from this file.
Use `--player-logging-only` for a focused installation and
`--verify-player-logging` to verify both settings. Restart the game after installing.
`python scripts/client/verify_player_logging.py` proves the default path with an
isolated native client and no `-logFile` argument.

Generate release-signing keys once with
`python scripts/release/create_update_signing_key.py`. Keep `client-updates/update-signing/private.pem`
on the publishing machine and give the launcher the public key through its
configuration. OpenSSL is needed on the publishing machine, not on players'
machines. Unsigned publishing is supported for localhost development only; omit
`--signing-key` and leave the launcher's public key empty in that case.

Backup (`.backup`, `.before-*`), staged (`.patched`, `.eye-patched`, `.staged`,
`*-staged`), log, and temporary files are excluded.
Symlinks/junctions in the input are rejected. Stop client patching/baking while
publishing so the snapshot represents one tested build.

For other code/assets, use repeated `--include` options. These **replace** the
defaults, so include the default directories too if they should stay managed:

```powershell
python scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version 1.0.1 --signing-key client-updates/update-signing/private.pem --include "King's Raid_Data/Managed" --include "King's Raid_Data/Documents/Patch/StandaloneWindows/TableData" --include "King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit" --include "King's Raid_Data/Documents/Patch/StandaloneWindows/LocalizationJit" --include "King's Raid_Data/resources.assets"
```

The first version distributed to players should include every file the launcher
will manage. Publish a new version identifier every time; an existing release
cannot be overwritten. Keep the managed selection consistent between releases:
paths removed from it become deletions in the next manifest.

When present, `SPRK-NativeData.json`, `SPRK-NativeCombat.json` and
`SPRK-NativeLocalization.json` under the native
patch directory are included automatically, so table changes and their integrity
metadata are installed together.

Each release contains a complete snapshot of the selected files. A launcher
compares local SHA-256 hashes and downloads only changed or missing files, even
when a player skips versions. The publisher copies all selected files to the
host's release storage; it does not copy the entire 20 GB client or create binary
deltas. Player-side hash comparison and installation belong to the launcher.

Publishing writes to a hidden staging directory, renames the complete release
into place, and atomically replaces the channel manifest last. A publisher lock
prevents simultaneous releases from racing. If interrupted before finishing,
the old manifest remains usable. A crashed publisher may leave `.publish-lock`
or staging directories; remove those only after its process has stopped. A
complete release can remain unreferenced if publishing its channel fails; use
a new version or explicitly recover that release, rather than overwrite it.

Storage layout:

```text
client-updates/
  stable/manifest.json
  releases/
    1.0.0/
      manifest.json
      files/King's Raid_Data/...
```

Never modify published release files. GitHub startup sync retains the active
release and the two most recently published other local releases, deleting older
recognized releases immediately after successful activation, without a grace
period. GitHub release assets remain available. These artifacts are ignored by
Git and excluded from the Docker image.

## Endpoints and launcher contract

| Endpoint                                        | Behavior                                            |
| ----------------------------------------------- | --------------------------------------------------- |
| `GET /updates/stable/manifest.json`             | Current channel manifest, `Cache-Control: no-store` |
| `GET /updates/releases/1.0.0/manifest.json`     | Immutable release manifest                          |
| `GET /updates/releases/1.0.0/files/{file_path}` | Immutable release file, streamed in bounded chunks  |

`HEAD`, byte `Range` requests for resume, and `Last-Modified` conditional
requests are supported. Unknown files/directories return 404; write methods
return 405. Downloads are confined to the configured storage directory, including
checks against symlinks/junctions escaping it. No directory listing is exposed.
The update endpoint is publicly readable and has no game login requirement.

Manifest format (SHA-256 shortened here for readability):

```json
{
    "schema_version": 1,
    "version": "1.0.0",
    "channel": "stable",
    "published_at": "2026-10-08T00:00:00+00:00",
    "files": [
        {
            "path": "King's Raid_Data/Managed/SprkDispatch.dll",
            "size": 12345,
            "sha256": "<64 lowercase hexadecimal characters>",
            "url": "/updates/releases/1.0.0/files/King%27s%20Raid_Data/Managed/SprkDispatch.dll"
        }
    ],
    "deleted_files": []
}
```

`path` is relative to the installed client root, using `/`. `url` is a percent
encoded path on the update server's origin. `deleted_files` carries previously
managed removals forward so a launcher can also handle skipped versions. Readded
files are removed from that deletion list. The launcher validates paths,
verifies sizes/hashes, stages the complete update, and applies it while the game
is closed. With `--signing-key`, the publisher adds a `signature` object with
`algorithm: "RSA-SHA256"`, a base64 `payload` of the original JSON bytes, and a
base64 `value` containing the RSA PKCS#1 v1.5 SHA-256 signature. A launcher with
a configured public key verifies the signature and uses only that payload for
installation decisions. The outer convenience fields are not trusted for
installation. Hosted update URLs require HTTPS and a configured public key;
localhost HTTP can be used for development.

## Run locally

Publish at least one fixture/release, then run an update-only process:

```powershell
$env:SPRK_SERVICE_MODE = "updates"
$env:SPRK_UPDATES_DIR = "client-updates"
$env:PORT = "8081"
cargo run --locked
```

Check `http://127.0.0.1:8081/health` and
`http://127.0.0.1:8081/updates/stable/manifest.json`. For both services in one
process, use `SPRK_SERVICE_MODE=all`; for normal game startup, use `game` or unset
the variable. Paths are relative to the process's working directory. The server
does not automatically load `.env` files; Compose does, or set process variables
directly.

## Portainer / Docker

Use the single `docker-compose.yml` file for Docker Standalone and Portainer.
It pulls the shared `SPRK_IMAGE` for both services, defaulting to
`ghcr.io/faberuser/sprk:latest`. GitHub Actions builds the image and triggers the
Portainer webhook; see [GitHub CI and client updates](github-client-updates.md).
The former `docker-compose.ghcr.yml` has been removed. If an existing Git stack
uses that filename, change its Compose path to `docker-compose.yml` while keeping
the same stack name and volumes.

`pull_policy: always` refreshes the image on deployment. Enable Portainer's
**Re-pull image** option for webhook redeployment. Portainer needs registry access
but no Rust toolchain or source build step. Set stack variables from `.env.example`.
If `SPRK_IMAGE` was previously set to a local tag, update it to the full GHCR path.

Create `/srv/sprk/client-updates` on the host and make it writable by container
UID/GID `10001`. The update service downloads its pinned release at startup and
retains the active release and two recent previous releases there, pruning older
recognized releases immediately after successful startup sync. For an image without a pinned release, manually
published artifacts can still be placed in the same directory.

For local development, build the Dockerfile explicitly and use the same Compose
file with a local image and pulling disabled. For example, in PowerShell:

```powershell
docker build -t sprk-server:local .
$env:SPRK_IMAGE = "sprk-server:local"
docker compose up -d --pull never
```

An image built without `CLIENT_UPDATE_TAG` has automatic client sync disabled.
See [Docker's Compose CLI](https://docs.docker.com/reference/cli/docker/compose/up/)
for the `--pull` option. Override `SPRK_UPDATES_HOST_DIR` for a local host path
that already exists and contains any manually published client updates.

- `game`: HTTP and player WebSockets on host port 8080; database and
  optional service configuration persist in the `game-data` volume at `/data`.
- `updates`: HTTP on host port 8081; published artifacts mounted at
  `/client-updates`; no database volume. Default limits are one CPU and 256 MB.

Set `SPRK_GAME_HOST` to the game HTTP hostname/port advertised to players.
Chat and battle use `/ws/chat` and `/ws/battle` on that same host. Private TCP
listeners are bound to container loopback and have no published ports. Tables
are included at `/app/tables`; a read-only mount can override that directory if
you maintain server tables outside the image.

Use your HTTPS reverse proxy to route `/updates/` (preserving the path) to the
update container and game HTTP requests to the game container. On the stack's
network these are `updates:8080` and `game:8080`; a separate proxy stack must join
the same network, or route through the published host ports. TLS terminates at
the proxy. Publishing new release files needs no container rebuild or restart.
If copying releases from another machine, transfer the full release directory
first, then atomically replace `stable/manifest.json` on the host.

For a game API behind HTTPS, set `SPRK_GAME_HTTPS=true`. The advertised login/CDN
defaults then use HTTPS, and player sockets use WSS. `LOGIN_SERVER` can override
the login URL (include a trailing `/`). See [WebSocket hosting](websocket-hosting.md)
for the client patch and Cloudflare Tunnel configuration.

CPU/memory limits do not reserve your home upload bandwidth. Configure download
rate limits or router traffic shaping separately if updates interfere with play.

## Validation

```sh
cargo test --locked
python -m unittest discover -s scripts -t . -p 'test_*client*.py'
docker compose -f docker-compose.yml config --quiet
```

The tests cover actual startup in all modes, resumable/HEAD downloads, router
and middleware separation, unsafe paths, complete manifests, skipped-version
removals, immutable versions, and interrupted publishing.
