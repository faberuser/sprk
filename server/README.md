# SPRK server

The game server: a single Rust binary, `sprk-server`, backed by PostgreSQL. The
same binary also serves client updates to the launcher. Players don't need
anything here; they use the Docker image (see the [root README](../README.md)).

Run every command from the repository root. The root `Cargo.toml` is a
workspace containing this package, so builds go to the shared `target/`.

## Layout

| Path | Contents |
| --- | --- |
| `src/api/` | Request handlers by area: account, gameplay, battle, community, GM, system |
| `src/models/`, `src/tables/` | Response types and game-table loading |
| `src/updates/` | Client update service (manifests, release sync from GitHub) |
| `migrations/` | PostgreSQL schema, triggers and SQL functions, applied at startup |
| `tests/` | Integration tests and their fixtures |
| `gm.py` | GM command-line tool |
| `scripts/local/` | `start_server.bat` implementation |
| `scripts/database/` | SQL type checks, diagnostics, SQLite→PostgreSQL copy |
| `scripts/worker/` | Native battle worker preparation and image smoke test |
| `scripts/docker/` | Container entrypoint, health check, image build helpers |
| `scripts/performance/` | HTTP latency measurement |

The server also reads `../tables/` (game tables) and `../config/`.

## Requirements

- [Rust 1.99+](https://rustup.rs). For VS Code, also run
  `rustup component add rust-analyzer rust-src` and open the repository root.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) for
  PostgreSQL 18.
- Python 3.11+ for the scripts and `gm.py` (database tools also need
  `pip install "psycopg[binary]"`).
- A game folder at `../sprk-client` to play against the server
  (see the [client README](../client/README.md)).

## Run a local development server (Windows)

```powershell
start_server.bat
```

It builds the release server, starts PostgreSQL in Docker, and runs two
processes until you close the window or press Ctrl+C:

| Service | Address |
| --- | --- |
| Game | `http://127.0.0.1:8080` |
| Client updates | `http://127.0.0.1:8081/updates/stable/manifest.json` |
| GM (only when `SPRK_GM_KEY` is set) | `http://127.0.0.1:8082` |

Start the game with SprkLauncher and choose **Local**, or test a source-built
client with `python client/dev.py test`.

Before the first start:

- **Database.** Nothing to do: unless `DATABASE_URL` is set, the script runs
  PostgreSQL in the Docker container `sprk-postgres-local` on `127.0.0.1:54320`,
  with its data in the volume of the same name.
- **Client update.** Nothing to do on a fresh clone: when
  `client-updates/stable/manifest.json` does not exist, the first start
  downloads the newest published client release from GitHub (about 400 MB),
  verifies its signature with `config/client-update-public.pem` and serves it.
  The launcher's **Local** profile accepts it as is. Later starts reuse the
  local copy and never replace existing local releases. Set
  `SPRK_UPDATES_GITHUB_REPO` (default `faberuser/sprk`) to use a fork.
- **Your own client changes.** To serve DLLs you built and installed, publish a
  local release from your game folder ([details](../docs/client-updates.md)):

  ```powershell
  python client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version local-YYYYMMDD-name
  ```

  Without the project's signing key this release is unsigned. The launcher
  accepts that only for a loopback server whose profile has no public key: in
  **Manage Servers**, edit **Local**, untick **Use the launcher's default public
  key** and leave the key empty. Maintainers sign with
  `--signing-key client-updates/update-signing/private.pem`.
- **Second checkout or worktree.** `client-updates/`, `.env` and
  `conquest-service.*` are local files that Git ignores, so a new checkout starts
  without them and downloads the release again. To keep one shared set of local
  releases (and the signing key), link the existing folder instead:
  `New-Item -ItemType Junction -Path client-updates -Target <other checkout>\client-updates`.
- **Optional settings.** Copy `.env.example` to `.env`. The script reads
  `SPRK_GM_KEY`, `DATABASE_URL`, `SPRK_LOCAL_DB_PORT` and
  `SPRK_UPDATES_GITHUB_REPO` from it.

Useful database commands:

```powershell
docker exec -it sprk-postgres-local psql -U postgres -d sprk    # SQL shell

# Start over with an empty database: stop the server, then
docker rm -f sprk-postgres-local
docker volume rm sprk-postgres-local
```

## Run the binary directly

```bash
docker run -d --name sprk-postgres-dev -e POSTGRES_PASSWORD=dev \
  -p 127.0.0.1:5432:5432 postgres:18 -c max_connections=300

DATABASE_URL=postgres://postgres:dev@127.0.0.1:5432/postgres GAME_TABLES_PATH=tables cargo run --release
```

At startup the server applies `migrations/`, then listens on
`http://0.0.0.0:8080`. Main settings:

| Variable | Meaning |
| --- | --- |
| `DATABASE_URL` | Game database (required in `game` and `all` modes) |
| `DATABASE_MIGRATION_URL` | Owner role for migrations; defaults to `DATABASE_URL` |
| `SPRK_SERVICE_MODE` | `game` (default), `updates` (client updates only) or `all` |
| `PORT` | HTTP port, default 8080 |
| `GAME_TABLES_PATH` | Game tables directory |
| `SPRK_UPDATES_DIR` | Published client releases, in `updates`/`all` mode |
| `SERVER_HOST`, `SERVER_HTTPS` | Address the server advertises to clients |
| `SPRK_GM_KEY`, `SPRK_GM_BIND` | GM access key and listener address |
| `RUST_LOG` | Log filter, for example `info,tower_http=debug` |

`start_server.bat` (`scripts/local/start_local_server.ps1`) shows a complete
local configuration; `.env.example` and `docker-compose.yml` show the hosted one.

## Native battle worker

Some battles (Conquest, co-op raids, dragon and story parties) are simulated by
a headless copy of the game, the battle worker, which connects back to the
server. In Docker the image includes a Linux/Wine worker built from the pinned
client release. Locally it is optional and configured in the repository root:

- `conquest-service.json`: `{"Executable": "<path to a game .exe>"}`, or the
  `BATTLE_WORKER_EXECUTABLE` variable;
- `conquest-service.key`: a random shared secret, or `BATTLE_SERVICE_KEY`.

`start_server.bat` recreates the worker at
`target/client-automation/clients/battle-worker/` from `../sprk-client` when the
configuration points there and it is missing. Without a worker, those battle
types are unavailable.

## Tests

Tests need a PostgreSQL server where the role can create databases, such as the
`sprk-postgres-dev` container above:

```bash
SPRK_TEST_POSTGRES_URL=postgres://postgres:dev@127.0.0.1:5432/postgres cargo test --locked
python -m unittest discover -s server/scripts -t . -p "test_*.py"
```

The first run builds a migrated template database; each test clones it into its
own database, and leftovers are dropped on the next run. Parallel tests open
many connections, hence `-c max_connections=300`.

Native battle tests are ignored by default. Run them against a game executable:

```bash
CONQUEST_NATIVE_EXECUTABLE=<path to a game .exe> cargo test --locked conquest_native_tcp_battle -- --ignored
```

Some tests read client files to keep both sides in agreement, for example
`client/managed/SprkNetworkPerformance/SprkNetworkPerformance.cs`. Schema
changes go in a new file in `migrations/`; never edit an applied migration.

## GM tools

With `SPRK_GM_KEY` set, the GM listener runs on port 8082. `gm.py` talks to it:

```bash
python server/gm.py server http://127.0.0.1:8082     # save the GM address
python server/gm.py players <name>                   # find an account
python server/gm.py --help                           # all commands
```

Never expose the GM port publicly.

## Docker image

CI builds the image; see [GitHub CI and client updates](../docs/github-client-updates.md).
To try the Compose stack with your own build, prepare `build-input/worker` from
a published client release with
`python server/scripts/worker/linux_worker.py build --tag client-<version>`, then:

```bash
docker build -t sprk-server:local .
SPRK_IMAGE=sprk-server:local docker compose up -d --pull never
```

Database operations, backups and performance work are covered in
[the database guide](../docs/database-diagnostics.md).
