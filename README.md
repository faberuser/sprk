# sprk

A Rust implementation of a server emulator for a real-time RPG mobile game client.

## Disclaimer

This project is for educational and preservation purposes only.

- All trademarks, copyrights, and other intellectual property related to the original game and its associated franchise belong to their respective owners.
- This repository does not include any copyrighted game assets, binaries, or master data.
- Use this software at your own risk. The authors assume no responsibility for any damages or legal consequences resulting from its use.

## Features

- Accounts, quests, and rewards
- Heroes, equipment, and customization
- Inventory, crafting, and shops
- Events, summons, and pets
- Dungeons, raids, and multiplayer
- Arena, guilds, and community
- Replays, rankings, and supporting services
- Configuration and development tools

## Source layout

- `src/api/`: account, gameplay, GM, and protocol handlers
- `src/models/` and `src/tables/`: client response types and table loading
- `src/updates.rs`: launcher update delivery
- `launcher/`: Windows launcher and its checks
- `scripts/`: client patching, publishing, and local server tools
- `tests/`: Python tool and Windows process-lifetime checks

See [the scripts inventory](scripts/README.md) for active CI and development tools.

## Building the Server

### Prerequisites

- Rust 1.93+ ([Install from https://rustup.rs](https://rust-lang.org/tools/install/))
- SQLite ([bundled with the project](https://www.sqlite.org/download.html))

### VS Code Rust analysis

Install the analyzer and standard-library sources for your active Rust toolchain:

```bash
rustup component add rust-analyzer rust-src
```

The workspace settings use this toolchain's `rust-analyzer` from `PATH` instead
of the extension's bundled version, keeping analysis compatible with the compiler.
The parent SPRK workspace explicitly links `sprk-server/Cargo.toml`. After changing
these settings, run **rust-analyzer: Restart server** in VS Code.

### Build Steps

```bash
# Build in release mode
cargo build --release

# The binary will be at target/release/sprk-server.exe (Windows)
# Or target/release/sprk-server (Linux/Mac)
```

### Running the Server

`SPRK_SERVICE_MODE=game` (the default) runs the game services. Use `updates` to
serve client update manifests/files without initializing the game, or `all` to
run both. See [client updates and Portainer setup](docs/client-updates.md) for the
publishing script, download endpoints, and shared-image container stack.

```bash
# Run the server (default port 8080)
cargo run --release

# Or run the binary directly
./target/release/sprk-server
```

The server will:

1. Create a SQLite database file (`sprk.db`) on first run
2. Initialize all required tables
3. Start listening on `http://0.0.0.0:8080`

### Running with Docker Compose

Use Docker Engine with the Compose plugin on Linux, or Docker Desktop with Linux
containers on Windows. The image runs on `linux/amd64` and includes the server,
tables, and battle worker; a local Rust installation is not required.

Run the following from the repository root in a Linux shell (WSL on Windows):

```bash
cp .env.example .env

# The updates container runs as UID/GID 10001 and needs a writable host directory.
sudo mkdir -p /srv/sprk/client-updates
sudo chown 10001:10001 /srv/sprk/client-updates
```

Edit `.env` before starting:

- `SPRK_IMAGE`: shared image for both services; defaults to `ghcr.io/faberuser/sprk:latest`.
- `SPRK_GAME_HOST`: hostname/IP and HTTP port reachable by players, such as
  `192.168.1.10:8080`. Set `SPRK_GAME_HTTPS=true` when using an HTTPS reverse proxy.
- `SPRK_UPDATES_HOST_DIR`: existing host directory created above. If you change
  it, create that directory and grant UID/GID `10001` write access there.
- `SPRK_GM_KEY`: replace the example value with a random key of at least 32
  characters, or leave it empty to disable GM access. Keep `SPRK_GM_BIND_IP`
  at `127.0.0.1` for local administration.

```bash
docker compose config --quiet
docker compose up -d
docker compose ps
docker compose logs -f game updates
```

Compose pulls the published image and starts two services:

| Service | Default host address | Persistent storage |
| --- | --- | --- |
| `game` | `http://localhost:8080` | SQLite database and service state in the `game-data` volume at `/data` |
| `updates` | `http://localhost:8081` | Signed client releases in the host directory mounted at `/client-updates` |

The update service downloads and verifies the image-pinned client release before
listening; its initial startup can take several minutes. Check readiness with:

```bash
curl --fail http://localhost:8080/health
curl --fail http://localhost:8080/health/battle
curl --fail http://localhost:8081/health
```

Use `SPRK_GAME_PORT` and `SPRK_UPDATE_PORT` in `.env` to change the published
ports. Player chat and battle use `/ws/chat` and `/ws/battle` on the game HTTP
port. The GM listener uses host port `8082` on loopback by default.

```bash
# Pull and apply the latest configured image.
docker compose pull
docker compose up -d

# Stop and remove containers while keeping persistent data.
docker compose down
```

Do not add `--volumes` to `docker compose down` unless you intend to delete the
game database volume. For Portainer, reverse proxy setup, and local image builds,
see [client updates and Docker hosting](docs/client-updates.md#portainer--docker)
and [the CI release workflow](docs/github-client-updates.md). Local image builds
also require preparing `build-input/worker` before running `docker build`.

### Patching the Client

Use the unified patcher to patch both `Assembly-CSharp.dll` and `resources.assets` in one command — just point it at your game client root folder:

```batch
scripts\client\patch.bat "D:\path\to\game\client"
scripts\client\patch.bat --restore "D:\path\to\game\client"
```

Or via PowerShell:

```powershell
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client"
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client" -Restore
```

This will:

1. Run the DLL patcher (`scripts/DllPatcher`) — creates a backup `Assembly-CSharp.dll.backup_before_patch` on first run
2. Patch `resources.assets` to redirect the query host URL to your local server (`http://127.0.0.1:8080`)
3. Both done in one command with just the client folder path

#### DLL Patcher CLI

`DllPatcher` and `DllDisasm` also accept the client root as a CLI argument:

```bash
dotnet run --project scripts/DllPatcher -- "D:\path\to\game\client"
dotnet run --project scripts/DllDisasm -- "D:\path\to\game\client"
```

## Credits

Thanks to Shamomylle/keirakoro/zanezonair for providing clients for this project.
