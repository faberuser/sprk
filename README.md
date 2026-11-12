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

## Contents

- [Play on your own computer](#play-on-your-own-computer): run your own server
  with Docker, no programming tools needed.
- [Host a server for other players](#host-a-server-for-other-players): the same
  setup on a Linux server or Portainer.
- [Contributing](#contributing): build, run and test the server from source.

## Play on your own computer

The server runs in [Docker](https://www.docker.com/products/docker-desktop/),
which downloads everything it needs. You do not need to build anything.

You need:

- A 64-bit Windows or Linux PC (Intel or AMD) with a few GB of free memory.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), installed
  and running.
- The game client with `SprkLauncher.exe` (Windows).

### First-time setup

1. Download this repository: on GitHub, click **Code**, then **Download ZIP**,
   and extract it, for example to `C:\sprk`.
2. In that folder, create two empty folders: `data\client-updates` and
   `data\db-backups`.
3. Copy `.env.example` to a new file named `.env`, open it in a text editor
   (Notepad is fine) and change these lines:

   ```ini
   SPRK_DB_SUPERUSER_PASSWORD=type_a_long_random_password_here
   SPRK_DB_OWNER_PASSWORD=type_a_different_one_here
   SPRK_DB_APP_PASSWORD=and_a_third_one_here
   SPRK_DB_BACKUP_HOST_DIR=./data/db-backups
   SPRK_UPDATES_HOST_DIR=./data/client-updates
   SPRK_GM_KEY=
   ```

   Use three different passwords made of letters, digits, `-` and `_`. You never
   need to type them again; just do not change them later.
4. Open a terminal in the folder (in Windows Explorer, right-click the folder
   and choose **Open in Terminal**) and run:

   ```bash
   docker compose up -d
   ```

   The first start downloads the server and the game files, which can take
   several minutes. It is ready when <http://localhost:8080/health> and
   <http://localhost:8081/health> both open in a browser.
5. Start `SprkLauncher.exe`, choose the **Local** server and play. The launcher
   downloads the game files from your server the first time.

### Everyday use

Run these in the same folder:

```bash
docker compose stop                            # stop the server
docker compose start                           # start it again
docker compose pull && docker compose up -d    # update to the latest version
```

Docker Desktop restarts the server automatically when your computer starts, as
long as Docker Desktop itself starts.

Your game data stays in Docker between restarts and updates. A backup is written
to `data\db-backups` once a day; keep that folder. Never run
`docker compose down --volumes` or delete the `postgres-data` volume in Docker
Desktop: that deletes your accounts.

If something does not work, `docker compose ps` shows which part is not
running and `docker compose logs game` shows the server's messages.

## Host a server for other players

This is the same Compose setup on an always-on Linux server, with Docker Engine
and the Compose plugin (or a Portainer stack using `docker-compose.yml`).

```bash
cp .env.example .env

# Host folders: client releases (UID/GID 10001) and database backups (UID 999).
sudo mkdir -p /srv/sprk/client-updates /srv/sprk/db-backups
sudo chown 10001:10001 /srv/sprk/client-updates
sudo chown 999:999 /srv/sprk/db-backups
```

In `.env` (or the Portainer stack variables), set at least:

- `SPRK_GAME_HOST`: the address players reach, such as `192.168.1.10:8080`, or
  your domain with `SPRK_GAME_HTTPS=true` behind an HTTPS reverse proxy.
- The three `SPRK_DB_*_PASSWORD` values, as above. They apply when the database
  volume is first created.
- `SPRK_GM_KEY`: a random key of at least 32 characters for GM tools, or empty.

Then start and check:

```bash
docker compose up -d
curl --fail http://localhost:8080/health
curl --fail http://localhost:8080/health/battle
curl --fail http://localhost:8081/health
```

| Service           | Address                 | Data                                                 |
| ----------------- | ----------------------- | ---------------------------------------------------- |
| `game`            | `http://localhost:8080` | service state in the `game-data` volume              |
| `updates`         | `http://localhost:8081` | signed client releases in `SPRK_UPDATES_HOST_DIR`    |
| `postgres`        | internal only           | game database in the `postgres-data` volume          |
| `postgres-backup` | none                    | daily `pg_dump` files in `SPRK_DB_BACKUP_HOST_DIR`, optionally copied to `SPRK_DB_BACKUP_COPY_HOST_DIR` |

Chat and battle use `/ws/chat` and `/ws/battle` on the game port. The GM
listener stays on loopback port 8082; never expose it. Players add your server
in SprkLauncher with your `SPRK_GAME_HOST` and update service addresses.

More detail:

- [Client updates and Docker hosting](docs/client-updates.md#portainer--docker):
  Portainer, reverse proxies and HTTPS.
- [GitHub CI and client updates](docs/github-client-updates.md): how the image
  and client releases are built.
- [The PostgreSQL guide](docs/postgresql-migration.md): backups, restores and
  moving from the older SQLite `sprk.db`.

## Contributing

### Tools

- [Rust 1.99+](https://rustup.rs) for the server.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) for the
  database (and Docker image builds).
- Python 3 for the scripts in `scripts/` and `gm.py`.
- The [.NET SDK](https://dotnet.microsoft.com/download) for the client patcher
  and the launcher.

For VS Code, install the analyzer and standard-library sources for your
toolchain, then run **rust-analyzer: Restart server**:

```bash
rustup component add rust-analyzer rust-src
```

The workspace settings use this toolchain's `rust-analyzer` from `PATH`, and the
parent SPRK workspace links `sprk-server/Cargo.toml`.

### Source layout

- `src/api/`: account, gameplay, GM, and protocol handlers
- `src/models/` and `src/tables/`: client response types and table loading
- `src/updates.rs`: launcher update delivery
- `migrations/`: PostgreSQL schema, triggers and SQL helper functions
- `launcher/`: Windows launcher and its checks
- `scripts/`: client patching, publishing, and local server tools
- `tests/`: Python tool and Windows process-lifetime checks

See [the scripts inventory](scripts/README.md) for active CI and development tools.

### Run a development server (Windows)

Besides the tools above you need a patched game client (see
[Patching the client](#patching-the-client)) and a published local client update
(`client-updates/stable/manifest.json`, see [client updates](docs/client-updates.md)).

1. Optional: copy `.env.example` to `.env` and set `SPRK_GM_KEY` to enable GM
   tools on `http://127.0.0.1:8082`.
2. Run `start_server.bat`.
3. Choose **Local** in SprkLauncher.

`start_server.bat` builds the release server and starts the update service
(port 8081) and the game (port 8080). Close the window or press Ctrl+C to stop
both.

The game stores its data in PostgreSQL. Unless `DATABASE_URL` is set (in `.env`
or the terminal), `start_server.bat` runs it for you in the Docker container
`sprk-postgres-local` on `127.0.0.1:54320`. The data lives in the Docker volume
of the same name and survives restarts. If a `sprk.db` from the earlier SQLite
version exists, the first start copies it into PostgreSQL (report in
`target\local-server\sqlite-copy-report.json`); after that `sprk.db` is no
longer used.

```powershell
# SQL shell on the local database
docker exec -it sprk-postgres-local psql -U postgres -d sprk

# Start over (copies sprk.db again on the next start): stop the server, then
docker rm -f sprk-postgres-local
docker volume rm sprk-postgres-local
```

### Build and run the binary directly

```bash
cargo build --release   # target/release/sprk-server(.exe)

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

To try the Compose stack with your own build, prepare `build-input/worker`
(the battle worker), then build the image:

```bash
docker build -t sprk-server:local .
SPRK_IMAGE=sprk-server:local docker compose up -d --pull never
```

### Tests

Tests need a PostgreSQL server where the role can create databases, such as the
`sprk-postgres-dev` container above:

```bash
SPRK_TEST_POSTGRES_URL=postgres://postgres:dev@127.0.0.1:5432/postgres cargo test
```

The first run builds a migrated template database; each test clones it into its
own database, and leftovers are dropped on the next run. Parallel tests open many
connections, hence `-c max_connections=300`. Pull requests also run the checks in
`.github/workflows/test-pr.yml`.

### Patching the client

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
