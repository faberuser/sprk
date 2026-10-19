# Scripts

## Client releases

| File                                                           | Purpose                                                                                        |
| -------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- |
| `build_client_release.py`                                      | CI/local isolated baseline download, patching, verification, signing, and GitHub publishing    |
| `prepare_client_baseline.py`                                   | Prepare a new immutable baseline when the distributed client changes                           |
| `client_baseline.py`                                           | Automatically maintained fixed baseline reference; no routine edits                            |
| `publish_client_update.py`                                     | Generate a signed snapshot from an installed/patched client; required for local client changes |
| `client_update_bundle.py`                                      | Shared signature verification and archive packaging; imported by CI tools                      |
| `create_update_signing_key.py`                                 | Bootstrap keys for a new installation; keep existing keys for this installation                |
| `test_build_client_release.py`, `test_client_update_bundle.py` | Build-input and packaging regression checks used by CI                                         |

See [CI client releases](../docs/github-client-updates.md) and
[local publishing](../docs/client-updates.md).

## Development tools

| File/directory                                   | Purpose                                                           |
| ------------------------------------------------ | ----------------------------------------------------------------- |
| `DllPatcher/`                                    | Client patch source, embedded resources, verifiers and self-tests |
| `DllDisasm/`                                     | Read-only DLL inspection tool documented in the repository README |
| `patch.ps1`, `patch.bat`                         | Manual DLL/assets patching and restore entry points               |
| `requirements-client.txt`                        | Pinned UnityPy dependency for client asset cleanup                |
| `build_launcher.ps1`                             | Build/install the Windows launcher                                |
| `start_local_server.ps1`                         | Start local game/update services; called by `../start_server.bat` |
| `import_local_env.ps1`, `LocalServerLifetime.cs` | Local server environment loading and child-process cleanup        |
| `client_automation.py`, `client-scenarios/`      | Opt-in real-client development and gameplay smoke scenarios       |

The standalone WebSocket probe was removed because its test driver no longer
exists. The standalone client uploader was replaced by the CI build/publish
entry point; its reusable verification/packaging code remains in the shared
module. Local development tools remain available independently of CI.
