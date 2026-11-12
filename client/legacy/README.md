# Retired client patching tools

Nothing here is built, tested or run by CI or by the release pipeline. Client
code is now compiled from [`../managed`](../README.md); change it there.

| Path | What it was |
| --- | --- |
| `DllPatcher/` | IL patcher that turned the original game DLL into the SPRK client, with its verifiers and embedded helper source (`Resources/*.cs.txt`) |
| `scripts/patch.ps1`, `patch.bat` | One-command patching of a game folder |
| `scripts/remove_client_telemetry.py` | Removal of telemetry SDKs and their serialized assets (needs `../scripts/requirements-client.txt`) |
| `scripts/isolate_client_storage.py` | Storage isolation asset edit used by the patcher |
| `recovery/` | Hashes of the DLLs the source was recovered from and the repairs applied to the decompiler output |

Why it is kept: the patcher's comments and verifiers document why each game
change exists, and its `Native*.cs` and `Patches/*` data steps are the only
record of how the shipped tables, localization and integrity metadata were
generated. Those generated files are part of the pinned client release now.
A table or asset that needs to change is placed, already in its final form,
under `../overrides/files/`; there is no source-based table generator yet.

The patcher expects the original, unpatched game DLL and does not work on a
source-built client. Delete this directory once nothing in it is still needed
for reference.
