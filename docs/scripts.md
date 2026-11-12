# Product scripts

Run commands from the repository root. CI and Docker orchestration files stay
at root; executable scripts are owned by their product.

| Directory | Purpose |
| --- | --- |
| `client/scripts/build/` | Maintained source compilation and verified API reference packaging |
| `client/scripts/release/` | Signed updates, immutable baselines, packaging and release tests |
| `client/scripts/legacy/` | Existing patch entrypoints, telemetry removal and storage asset cleanup |
| `client/scripts/recovery/` | Isolated source recovery and inventory experiments |
| `client/scripts/verification/` | Optional Unity runtime probes |
| `client/scripts/launcher/` | Launcher build and installation |
| `client/scripts/automation/` | Real-client gameplay automation and scenarios |
| `client/scripts/android/` | Android conversion and installation |
| `server/scripts/database/` | SQL checking, database diagnostics and data copy tools |
| `server/scripts/local/` | Local server startup and process lifetime management |
| `server/scripts/worker/` | Battle worker preparation and image smoke checks |
| `server/scripts/performance/` | HTTP latency measurements |
| `server/scripts/docker/` | Container entrypoints, battle-worker execution and health checks |

Legacy assembly tools are in `client/tools/`; see its README for their roles.
Dependencies shared by client tools are pinned in `client/scripts/requirements-client.txt`.

```powershell
python client/scripts/build/build_managed_client.py --help
python client/scripts/release/build_client_release.py --help
python server/scripts/worker/linux_worker.py --help
python -m unittest discover -s client/scripts -t . -p 'test_*.py'
python -m unittest discover -s server/scripts -t . -p 'test_*.py'
```

Python imports follow the directories, for example `client.scripts.release` and
`server.scripts.worker`. The root `start_server.bat` calls
`server/scripts/local/start_local_server.ps1`.
