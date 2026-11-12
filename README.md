# SPRK

A private server for a real-time RPG mobile game, with the game's client code
maintained as source. This repository holds both halves:

- **`server/`**: the game server, written in Rust, with a PostgreSQL database.
- **`client/`**: the game's C# code, the Windows launcher, and the tools that
  build and publish client updates.

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

## Which section do you need?

| You want to… | Read | You need to install |
| --- | --- | --- |
| Play on your own PC | [Play on your own computer](#play-on-your-own-computer) | Docker Desktop |
| Run a server for friends or the public | [Host a server for other players](#host-a-server-for-other-players) | Docker on a Linux machine |
| Change the server or the game | [Contributing](#contributing) | Rust, .NET, Python, Docker |

Players never build anything: the server comes as a ready-made Docker image and
the game updates itself through its launcher.

## Play on your own computer

### What you need

- A 64-bit Windows PC (Intel or AMD) with a few GB of free memory. The server
  also runs on Linux; the game itself needs Windows.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), installed
  and running.
- The SPRK game client folder, which contains `King's Raid.exe` and
  `SprkLauncher.exe`. It is shared as a ZIP; extract it anywhere.

### First-time setup

1. Download this repository: on GitHub, click **Code**, then **Download ZIP**,
   and extract it, for example to `C:\sprk`. You only need it for its
   `docker-compose.yml` and `.env.example` files.
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

   The first start downloads the server and the current game update, which can
   take several minutes. It is ready when <http://localhost:8080/health> and
   <http://localhost:8081/health> both open in a browser.
5. Start `SprkLauncher.exe` from the game folder, choose the **Local** server
   and play. The first time, the launcher brings the game files up to date from
   your server.

### Everyday use

Run these in the same folder:

```bash
docker compose stop                            # stop the server
docker compose start                           # start it again
docker compose pull && docker compose up -d    # update to the latest version
```

Docker Desktop restarts the server when your computer starts, as long as Docker
Desktop itself starts. After updating the server, start the game through
`SprkLauncher.exe` again so it picks up the matching game update.

Your game data stays in Docker between restarts and updates. A backup is written
to `data\db-backups` once a day; keep that folder. Never run
`docker compose down --volumes` or delete the `postgres-data` volume in Docker
Desktop: that deletes your accounts.

If something does not work, `docker compose ps` shows which part is not
running and `docker compose logs game` shows the server's messages.

### Upgrading from the SQLite version

Older versions kept the game data in a `sprk.db` file, which the current server
no longer reads. To bring it over once:

1. Start the current server once so it creates the new database, then stop the
   game with `docker compose stop game`.
2. Copy the data. The old file is still in the `game-data` volume and the
   database has no published port, so run the copy in a container on the
   stack's network. Volume and network names start with your folder's name
   (see `docker volume ls`); this example assumes the folder is `sprk`:

   ```bash
   docker run --rm --network sprk_default -v sprk_game-data:/data -v "$PWD/server/scripts/database":/w:ro \
     python:3.12-slim sh -c 'pip install -q "psycopg[binary]" && python -I /w/copy_sqlite_to_postgres.py /data/sprk.db --database-url "postgres://sprk_owner:<SPRK_DB_OWNER_PASSWORD>@postgres:5432/sprk"'
   ```

3. Start the game again with `docker compose start game`.

The copy only goes into a database without accounts, is all or nothing, checks
every table afterwards and leaves `sprk.db` unchanged.

## Host a server for other players

This is the same Compose setup on an always-on Linux machine with Docker Engine
and the Compose plugin, or a Portainer stack using `docker-compose.yml`.

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

Every setting is explained in [`.env.example`](.env.example). Then start and check:

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
listener stays on port 8082 of `SPRK_GM_BIND_IP` (loopback by default); never
expose it publicly. Players add your server in SprkLauncher (**Manage Servers**)
with your game address (`.../host.json`) and update address
(`.../updates/stable/manifest.json`).

Each server image is pinned to the game update it was built with and installs
it on startup, so updating the image updates the game for everyone.

More detail:

- [Client updates and Docker hosting](docs/client-updates.md#portainer--docker):
  Portainer, reverse proxies and HTTPS.
- [GitHub CI and client updates](docs/github-client-updates.md): how the image
  and client releases are built and deployed.
- [The database guide](docs/database-diagnostics.md): backups, restores,
  settings and diagnostics.

## Contributing

### How the pieces fit

```text
client/managed  (C# source) ──build──▶ Assembly-CSharp.dll + Sprk*.dll ─┐
pinned client release (Unity player, assets, tables) ───────────────────┼─▶ signed client update
client/overrides/files (changed assets/tables) ─────────────────────────┘        │
                                                                                  ▼
server/  (Rust) ──build──▶ sprk-server ──▶ Docker image (pins that update) ──▶ players' launchers
```

The Unity player and the 19 GB of game assets are never source and never live in
this repository. Only the game's own C# code (`Assembly-CSharp.dll`) and the
SPRK feature assemblies (`Sprk*.dll`) are built here; everything else comes
from an existing game folder or the pinned release.

### Repository layout

| Path | Contents |
| --- | --- |
| [`server/`](server/README.md) | Rust server, PostgreSQL migrations, tests, GM CLI and server scripts |
| [`client/`](client/README.md) | Game C# source, launcher, client build/release/automation scripts |
| `tables/` | Game tables the server loads |
| `config/` | Client-update public key and shared configuration |
| `docs/` | Guides: updates, CI, database, automation and more |
| `.github/workflows/` | Pull-request checks and the build-and-deploy pipeline |
| `Dockerfile`, `Dockerfile.tests`, `docker-bake.hcl`, `docker-compose.yml` | Server image, test image and the Compose stack |
| `start_server.bat` | Local Windows development server |

Generated output goes to `target/` (Rust builds, automation runs) and
`artifacts/` (client builds); both are ignored by Git.

### Requirements

| Tool | Needed for |
| --- | --- |
| Git | Everything |
| [Rust 1.99+](https://rustup.rs) | The server |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | The local database, and building images |
| Python 3.11+ | Client build and release scripts, server tools, `server/gm.py` |
| [.NET SDK 10.0.1xx](https://dotnet.microsoft.com/download) (pinned in `client/global.json`) | The client code and the launcher |
| Windows | Building and running the client; the server builds on any OS |

### Set up a workspace

Clone this repository next to a game folder. The scripts look for the game in
`../sprk-client` by default:

```text
<workspace>/
  sprk/          this repository
  sprk-client/   the game folder from the client ZIP
```

Run every command from the repository root. Start the game once through its
launcher on the public server so its files match the current release.

### Work on the server

```bash
cargo build                 # debug build
start_server.bat            # Windows: database, update service and game on localhost
```

On a fresh clone, the first `start_server.bat` downloads the newest published
client update from GitHub, so the launcher's **Local** server works right away.

[`server/README.md`](server/README.md) covers the local development server,
configuration, tests, the native battle worker and GM tools.

### Work on the client

```powershell
python client/scripts/build/build_managed_client.py                 # compile (about 40 s)
python client/scripts/build/sandbox_client.py create                # separate test copy of the game
python client/scripts/build/build_managed_client.py --install ../sprk-client-test
```

[`client/README.md`](client/README.md) covers editing the game code, testing it
safely, the launcher, replacing assets and tables, and publishing updates.

### Tests and checks

```bash
cargo test --locked                                                  # needs SPRK_TEST_POSTGRES_URL, see server/README.md
python -m unittest discover -s client/scripts -t . -p "test_*.py"
python -m unittest discover -s server/scripts -t . -p "test_*.py"
dotnet run --project client/launcher/SprkLauncher.Tests
```

Pull requests into `main` run the server tests, SQL checks, script tests,
native battle tests under Wine, a server image smoke test and a complete client
release build. See [the scripts inventory](docs/scripts.md) for every tool.

### Branches and releases

Commit to `development` (directly or by merging a feature branch), then open a
pull request from `development` into `main`. Merging it builds and publishes the
client update and the server image, then redeploys the public server; see
[GitHub CI and client updates](docs/github-client-updates.md). Nothing is
uploaded by hand.

### Documentation

| Guide | Topic |
| --- | --- |
| [docs/client-updates.md](docs/client-updates.md) | Update service, local publishing, Docker hosting |
| [docs/github-client-updates.md](docs/github-client-updates.md) | CI pipeline, release baseline, signing |
| [docs/client-automation.md](docs/client-automation.md) | Driving the real game in tests |
| [docs/database-diagnostics.md](docs/database-diagnostics.md) | PostgreSQL operations and performance |
| [docs/scripts.md](docs/scripts.md) | Every script and its purpose |
| [docs/damage-formula.md](docs/damage-formula.md) | Combat damage calculation |

## Credits

Thanks to Shamomylle/keirakoro/zanezonair for providing clients for this project.
