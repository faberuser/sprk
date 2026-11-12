# SPRK client

The game's managed code, built from source. Server code lives in `../server`.

| Path | Contents |
| --- | --- |
| `managed/Assembly-CSharp/` | The game's C# code |
| `managed/Sprk*/` | SPRK feature assemblies, one DLL per directory |
| `launcher/` | `SprkLauncher.exe` (profiles, signed updates) and its tests |
| `overrides/files/` | Optional replacement assets and tables, by client-relative path |
| `scripts/build/` | Compiles `managed/`; creates test copies of the game |
| `scripts/release/` | Signed update building, publishing and baseline pin |
| `scripts/automation/` | Drives the real client for tests |
| `scripts/launcher/`, `scripts/android/` | Launcher installation; Android conversion |
| `tools/ManagedSource/` | Read-only DLL inspection and build verification |

What is **not** source and never built here: the Unity player
(`King's Raid.exe`, `UnityPlayer.dll`, `MonoBleedingEdge/`), native plugins,
Unity/Mono/third-party DLLs, scenes, asset bundles, tables and the rest of
`King's Raid_Data/`. A game folder supplies those as they are. The build replaces
only `Assembly-CSharp.dll` and `Sprk*.dll` in `King's Raid_Data/Managed/`.

## Build, install, play

Requirements: Windows, Python 3.11+, the .NET SDK selected by `global.json`
(10.0.1xx), and an installed game folder (default `../sprk-client`, next to this
repository). Run commands from the repository root.

```powershell
# Compile all 48 assemblies into artifacts/client-build (about 40 seconds)
python client/scripts/build/build_managed_client.py

# Compile and copy the DLLs into the game folder (close the game first)
python client/scripts/build/build_managed_client.py --install ../sprk-client
```

Then start the game through the launcher as usual. Use `--references` when the
runtime DLLs are somewhere else, and `--work` for a different build directory.

To change the game, edit the `.cs` files in `managed/` and rebuild. To add a
feature assembly, create `managed/SprkName/SprkName.csproj` containing
`<Project Sdk="Microsoft.NET.Sdk" />` plus its source; the build, the release
and the launcher pick it up automatically. Deleting a directory removes its DLL
from the next release.

### Test without touching your game

```powershell
python client/scripts/build/sandbox_client.py create      # ../sprk-client-test, about 2 minutes
python client/scripts/build/build_managed_client.py --install ../sprk-client-test   # after each change
python client/scripts/build/sandbox_client.py remove      # when done
```

The sandbox copies the player files (1.2 GB), shares the real game's 19 GB
`King's Raid_Data/Documents` through a junction and installs the source-built
DLLs. Start the local server (`start_server.bat`), close the normal game, then
run `Play (local server).bat` in the sandbox. `Play (public server).bat` uses
your real account on the public server.

- Never start `SprkLauncher.exe` for the sandbox; it would replace the test DLLs
  with the published ones. The `.bat` files set `SPRK_HOST_URL`, which is all
  the launcher normally adds.
- Delete the sandbox with `sandbox_client.py remove` only. A plain recursive
  delete can follow the junction and delete the real game's assets.
- Saved settings and logins are shared with the real game (Unity keeps them
  per Windows user), and only one game instance can run at a time.

After installing changed DLLs into the real game, publish a local signed update so the local update
server and the launcher agree with the game folder; see
[client updates](../docs/client-updates.md).

### How the build works

`managed/Directory.Build.props` holds the shared compiler settings. Projects
compile against the game's own runtime DLLs, not the .NET SDK's:
`managed/references.lock.json` pins those 144 files by SHA-256, and the build
refuses anything else. The `netstandard2.1` label only enables SDK tooling.

The game calls the feature assemblies and they call the game. The build breaks
that cycle without any previously built DLL:

1. Compile all source into one temporary assembly.
2. Compile the feature assemblies against it, then the game against them.
3. Compile the final feature assemblies, then the final game, against those.

Output is deterministic: the same source and SDK produce byte-identical DLLs.
An IDE or `dotnet build` on one project works after one complete build, using
the references staged in `artifacts/client-build/references`.

## Releases

`scripts/release/build_client_release.py` assembles a complete signed update in
an isolated folder:

```text
pinned published release (assets, tables, runtime DLLs)   scripts/release/client_baseline.py
  + overrides/files/                                      replacement assets
  + DLLs compiled from managed/
  = signed manifest + client-update.tar.gz
```

CI runs it on every merge to `main`, publishes the result as a GitHub release
and deploys the matching server; see [GitHub client releases](../docs/github-client-updates.md).
Check a release locally without the signing key or any upload:

```powershell
python client/scripts/release/build_client_release.py --work artifacts/release-check --version check-1 --check-only
```

To ship a changed asset or table, put the file under `overrides/files/` at its
client-relative path, for example
`overrides/files/King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit/ItemTable.jit`,
together with any `SPRK-Native*.json` integrity metadata that covers it.

New players still need a complete game folder (the Google Drive ZIP). It only
has to contain the player and assets; the launcher brings its managed files up
to the current release on first start.

## Launcher

```powershell
client/scripts/launcher/build_launcher.ps1 -ClientPath ../sprk-client
dotnet run --project client/launcher/SprkLauncher.Tests
```

## Tests

```powershell
python -m unittest discover -s client/scripts -t . -p "test_*.py"
```

[Client automation](../docs/client-automation.md) starts an isolated server and
database and drives the real game (login, lobby, shops, battles).

## Where the source came from

`managed/` was recovered once from the last DLL-patched release
(`client-ci-2795d44b755a-43152391931-1`) with ILSpy and repaired until it
compiled. It is now ordinary maintained source: never regenerate it from a DLL.
The retired DLL patcher, its scripts and the recovery records were deleted
afterwards; git history still has them (`git show 470d93a2:client/legacy/README.md`). Checked on 2026-11-12 against that release:

- every type keeps the same serialized field layout, base type and interfaces
  (15,350 types), with matching assembly identities and embedded resources;
- on the real client: login, lobby, hero, inventory and Special Shop screens,
  and a campaign battle through victory and rewards, with no new errors;
- the server's native battle worker test (`conquest_native_tcp_battle`) passes
  with these DLLs.

Known differences from the patched DLLs: explicit interface methods that the old
64-bit currency patch left with mismatched `int`/`long` signatures now match
their interfaces and use checked conversions (a value above the 32-bit range
throws instead of wrapping), and compiler-generated closure and backing-field
names differ. Features not covered by the checks above should be play-tested
before a public release.
