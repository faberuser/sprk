# Scripts

Run commands from the `sprk-server` repository root unless noted otherwise.

| Directory | Purpose | Main entry points |
| --- | --- | --- |
| [release/](release/) | Signed client snapshots, CI builds, baseline preparation, packaging and regression tests | `build_client_release.py`, `publish_client_update.py`, `prepare_client_baseline.py` |
| [client/](client/) | Manual Windows client patching, telemetry cleanup and UnityPy dependencies | `patch.ps1`, `patch.bat`, `remove_client_telemetry.py`, `requirements-client.txt` |
| [android/](android/) | Android APK builds, content conversion, installation and bundled adapters | `build_android_accounts.py`, `build_android_endpoint_test.py`, `convert_android_bundle_pack.py`, `install_android_content.py` |
| [automation/](automation/) | Opt-in real-client gameplay automation and smoke scenarios | `client_automation.py`, `scenarios/` |
| [server/](server/) | Local service startup, process cleanup and Windows/Linux battle worker preparation | `start_local_server.ps1`, `prepare_local_worker.py`, `linux_worker.py`, `smoke_linux_worker.py` |
| [docker/](docker/) | Container startup, battle worker execution and health checks | `docker-entrypoint.sh`, `docker-battle-worker.sh`, `docker-healthcheck.sh` |
| [launcher/](launcher/) | Windows launcher build and installation | `build_launcher.ps1` |
| [DllPatcher/](DllPatcher/) | Reproducible client patches, embedded resources, verifiers and self-tests | `dotnet run --project scripts/DllPatcher -- ...` |
| [DllDisasm/](DllDisasm/) | Read-only managed DLL inspection | `dotnet run --project scripts/DllDisasm -- ...` |

The former root scripts now live in these directories; use their new paths.
Python tools support direct execution, for example:

```powershell
python -m pip install -r scripts/client/requirements-client.txt
python scripts/release/build_client_release.py --help
python scripts/release/publish_client_update.py --help
python scripts/automation/client_automation.py --help
python scripts/server/linux_worker.py --help
```

Python tools also support package invocation, such as
`python -m scripts.release.build_client_release --help`. Shared imports use the
`scripts` package. Discover release and worker regression tests recursively with:

```powershell
python -m unittest discover -s scripts -t . -p 'test_*client*.py'
```

`start_server.bat` at the repository root calls `server/start_local_server.ps1`.
Docker and GitHub Actions use the organized paths directly.

See [CI client releases](../docs/github-client-updates.md),
[local publishing](../docs/client-updates.md),
[client automation](../docs/client-automation.md), and
[Linux battle workers](../docs/linux-battle-worker.md) for their workflows.
