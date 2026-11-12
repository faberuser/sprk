# Scripts

Run commands from the repository root. Scripts belong to the product they
serve; CI and Docker orchestration files stay at the root.

| Directory | Purpose |
| --- | --- |
| `client/scripts/build/` | Compile the game and feature assemblies from `client/managed`; test sandbox |
| `client/scripts/release/` | Build, sign, publish and select client updates; pin the asset baseline |
| `client/scripts/automation/` | Drive the real client against an isolated server; scenarios |
| `client/scripts/launcher/` | Build the launcher and install it into a game folder |
| `client/scripts/android/` | Android conversion and installation |
| `server/scripts/database/` | SQL checking, database diagnostics and the SQLite copy tool |
| `server/scripts/local/` | Local server startup and process lifetime |
| `server/scripts/worker/` | Battle worker preparation and image smoke checks |
| `server/scripts/performance/` | HTTP latency measurements |
| `server/scripts/docker/` | Container entrypoints, health checks and image build steps |

Main entry points:

```powershell
python client/scripts/build/build_managed_client.py --help          # compile, optionally install
python client/scripts/build/sandbox_client.py --help                # separate test copy of the game
python client/scripts/release/build_client_release.py --help        # complete signed release (CI)
python client/scripts/release/publish_client_update.py --help       # local release from the installed client
python client/scripts/automation/client_automation.py --help        # real-client tests
python server/scripts/worker/linux_worker.py --help
python server/gm.py --help
```

Tests:

```powershell
python -m unittest discover -s client/scripts -t . -p "test_*.py"
python -m unittest discover -s server/scripts -t . -p "test_*.py"
dotnet run --project client/launcher/SprkLauncher.Tests
cargo test --locked
```

Client build and release scripts use only the Python standard library.
`client/scripts/requirements-client.txt` is for the Android tools. Python imports follow the
directories, for example `client.scripts.release` and `server.scripts.worker`.
The root `start_server.bat` calls `server/scripts/local/start_local_server.ps1`.
