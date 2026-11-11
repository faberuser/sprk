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
- `migrations/`: PostgreSQL schema, triggers and SQL helper functions
- `launcher/`: Windows launcher and its checks
- `scripts/`: client patching, publishing, and local server tools
- `tests/`: Python tool and Windows process-lifetime checks

See [the scripts inventory](scripts/README.md) for active CI and development tools.

## Run a local server (Windows)

You need [Rust 1.99+](https://rustup.rs), Python, and
[Docker Desktop](https://www.docker.com/products/docker-desktop/) running, plus
a patched game client (see [Patching the client](#patching-the-client)) and a
published local client update (`client-updates/stable/manifest.json`, see
[client updates](docs/client-updates.md)).

1. Optional: copy `.env.example` to `.env` and set `SPRK_GM_KEY` to enable GM
   tools on `http://127.0.0.1:8082`.
2. Run `start_server.bat`.
3. Choose **Local** in SprkLauncher.

`start_server.bat` builds the release server and starts the update service
(port 8081) and the game (port 8080). Close the window or press Ctrl+C to stop
both.

### The local database

The game stores its data in PostgreSQL. Unless `DATABASE_URL` is set (in `.env`
or the terminal), `start_server.bat` runs it for you in the Docker container
`sprk-postgres-local` on `127.0.0.1:54320`. The data lives in the Docker volume
of the same name and survives restarts.

If a `sprk.db` from the earlier SQLite version exists, the first start copies
it into PostgreSQL (report in `target\local-server\sqlite-copy-report.json`).
After that `sprk.db` is no longer used.

```powershell
# SQL shell on the local database
docker exec -it sprk-postgres-local psql -U postgres -d sprk

# Start over (copies sprk.db again on the next start): stop the server, then
docker rm -f sprk-postgres-local
docker volume rm sprk-postgres-local
```

## Host with Docker Compose

Use Docker Engine with the Compose plugin on Linux, or Docker Desktop on
Windows. The published `linux/amd64` image contains the server, tables and
battle worker; Rust is not needed. From the repository root, in a Linux shell
(WSL on Windows):

```bash
cp .env.example .env

# Host directories: client releases (UID/GID 10001) and database backups (UID 999).
sudo mkdir -p /srv/sprk/client-updates /srv/sprk/db-backups
sudo chown 10001:10001 /srv/sprk/client-updates
sudo chown 999:999 /srv/sprk/db-backups
```

Edit `.env`. At least set:

- `SPRK_GAME_HOST`: the address players reach, such as `192.168.1.10:8080`
  (`SPRK_GAME_HTTPS=true` behind an HTTPS reverse proxy).
- `SPRK_DB_SUPERUSER_PASSWORD`, `SPRK_DB_OWNER_PASSWORD`, `SPRK_DB_APP_PASSWORD`:
  three different random passwords (letters, digits, `-`, `_`). They apply when
  the database volume is first created.
- `SPRK_GM_KEY`: a random key of at least 32 characters, or empty to disable GM.

Then start and check:

```bash
docker compose up -d
docker compose ps
curl --fail http://localhost:8080/health
curl --fail http://localhost:8080/health/battle
curl --fail http://localhost:8081/health
```

| Service           | Address                 | Data                                                 |
| ----------------- | ----------------------- | ---------------------------------------------------- |
| `game`            | `http://localhost:8080` | service state in the `game-data` volume              |
| `updates`         | `http://localhost:8081` | signed client releases in `SPRK_UPDATES_HOST_DIR`    |
| `postgres`        | internal only           | game database in the `postgres-data` volume          |
| `postgres-backup` | none                    | nightly `pg_dump` files in `SPRK_DB_BACKUP_HOST_DIR` |

The update service downloads and verifies its client release before
listening, so its first start can take several minutes. Chat and battle use
`/ws/chat` and `/ws/battle` on the game port; the GM listener uses port 8082
on loopback.

```bash
docker compose pull && docker compose up -d   # update to the latest image
docker compose down                           # stop, keeping all data
```

Never add `--volumes` to `docker compose down` unless you mean to delete the
database. For Portainer, reverse proxies and local image builds, see
[client updates and Docker hosting](docs/client-updates.md#portainer--docker)
and [the CI release workflow](docs/github-client-updates.md). Database
backups, restores and moving from SQLite are in
[the PostgreSQL migration guide](docs/postgresql-migration.md).

## Development

### Build

```bash
cargo build --release   # target/release/sprk-server(.exe)
```

For VS Code, install the analyzer and standard-library sources for your
toolchain, then run **rust-analyzer: Restart server**:

```bash
rustup component add rust-analyzer rust-src
```

The workspace settings use this toolchain's `rust-analyzer` from `PATH`, and the
parent SPRK workspace links `sprk-server/Cargo.toml`.

### Run the binary directly

```bash
docker run -d --name sprk-postgres-dev -e POSTGRES_PASSWORD=dev \
  -p 127.0.0.1:5432:5432 postgres:18 -c max_connections=300

DATABASE_URL=postgres://postgres:dev@127.0.0.1:5432/postgres cargo run --release
```

At startup the server applies the migrations in `migrations/`, then listens on
`http://0.0.0.0:8080`.

| Variable                 | Meaning                                                     |
| ------------------------ | ----------------------------------------------------------- |
| `DATABASE_URL`           | game database (required in `game` and `all` modes)          |
| `DATABASE_MIGRATION_URL` | owner role for migrations; defaults to `DATABASE_URL`       |
| `SPRK_SERVICE_MODE`      | `game` (default), `updates` (client updates only), or `all` |
| `PORT`                   | HTTP port, default 8080                                     |

The server refuses to start when a `sprk.db` from the SQLite version sits next
to an empty database. Copy it first:
`DATABASE_URL=... sprk-server migrate-sqlite sprk.db`.

### Tests

Tests need a PostgreSQL server where the role can create databases, such as the
`sprk-postgres-dev` container above:

```bash
SPRK_TEST_POSTGRES_URL=postgres://postgres:dev@127.0.0.1:5432/postgres cargo test
```

The first run builds a migrated template database; each test clones it into its
own database, and leftovers are dropped on the next run. Parallel tests open many
connections, hence `-c max_connections=300`.

## Patching the client

Patch `Assembly-CSharp.dll` and `resources.assets` in one command by pointing
the patcher at the game client folder:

```batch
scripts\client\patch.bat "D:\path\to\game\client"
scripts\client\patch.bat --restore "D:\path\to\game\client"
```

Or in PowerShell:

```powershell
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client"
.\scripts\client\patch.ps1 -ClientPath "D:\path\to\game\client" -Restore
```

This runs the DLL patcher (`scripts/DllPatcher`, which keeps
`Assembly-CSharp.dll.backup_before_patch` from its first run) and points
`resources.assets` at the local server (`http://127.0.0.1:8080`).

`DllPatcher` and `DllDisasm` also take the client folder directly:

```bash
dotnet run --project scripts/DllPatcher -- "D:\path\to\game\client"
dotnet run --project scripts/DllDisasm -- "D:\path\to\game\client"
```

## Credits

Thanks to Shamomylle/keirakoro/zanezonair for providing clients for this project.
