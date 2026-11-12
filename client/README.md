# SPRK client

The game's own C# code and the SPRK features built on top of it, the Windows
launcher, and the tools that build, test and publish client updates.

Run every command from the repository root.

## What is source and what is not

A game folder (`sprk-client`) is a Unity player plus assets. Only two kinds of
file in it are built from this directory:

| Built here | Comes from an existing game folder or release |
| --- | --- |
| `King's Raid_Data/Managed/Assembly-CSharp.dll` (the game's code) | `King's Raid.exe`, `UnityPlayer.dll`, `MonoBleedingEdge/`, `D3D12/` |
| `King's Raid_Data/Managed/Sprk*.dll` (SPRK features) | Unity, Mono and third-party DLLs in `Managed/` |
| `SprkLauncher.exe` | Scenes, asset bundles, tables, `King's Raid_Data/Documents/` (19 GB) |

## Layout

| Path | Contents |
| --- | --- |
| `managed/Assembly-CSharp/` | The game's C# code |
| `managed/Sprk*/` | SPRK feature assemblies, one DLL per directory |
| `managed/Directory.Build.props` | Compiler settings shared by every project |
| `managed/references.lock.json` | SHA-256 pins of the game runtime DLLs the code compiles against |
| `launcher/` | `SprkLauncher.exe` (server profiles, signed updates) and its tests |
| `overrides/files/` | Replacement assets and tables, by client-relative path (empty until needed) |
| `scripts/build/` | Compile `managed/`; create and remove test copies of the game |
| `scripts/release/` | Build, sign, publish and pin client updates |
| `scripts/automation/` | Drive the real game in tests |
| `scripts/launcher/`, `scripts/android/` | Launcher installation; Android conversion |
| `tools/ManagedSource/` | Read-only DLL inspection and comparison |
| `global.json` | .NET SDK version |

## Requirements

- Windows.
- [.NET SDK 10.0.1xx](https://dotnet.microsoft.com/download) (`global.json`
  accepts any 10.0.1xx patch).
- Python 3.11+. The build and release scripts need no extra packages.
- A game folder at `../sprk-client`, next to this repository, kept current by
  starting it once through its launcher. The build reads the game's runtime
  DLLs from it and checks each against `managed/references.lock.json`.

## Build

```powershell
python client/scripts/build/build_managed_client.py
```

This compiles all 48 assemblies in about 40 seconds into
`artifacts/client-build/client-code/King's Raid_Data/Managed/`. Add
`--install <game folder>` to copy them into a game folder (close the game
first), `--references <Managed folder>` if the game is elsewhere, and
`--work <folder>` for another build directory.

## Test your changes safely

Test in a separate copy of the game, not in `../sprk-client`:

```powershell
python client/scripts/build/sandbox_client.py create      # makes ../sprk-client-test, about 2 minutes
python client/scripts/build/build_managed_client.py --install ../sprk-client-test   # after each change
python client/scripts/build/sandbox_client.py remove      # when done
```

The sandbox copies the player files (1.2 GB), shares the real game's
`King's Raid_Data/Documents` through a junction, and installs DLLs built from
source. To play it:

1. Start the local server with `start_server.bat` (see the
   [server README](../server/README.md)).
2. Close the normal game; only one copy can run at a time.
3. Run `Play (local server).bat` in the sandbox.

`Play (public server).bat` uses your real account on the public server.

- Never start `SprkLauncher.exe` for the sandbox. It would replace the test
  DLLs with the published ones. The `.bat` files set `SPRK_HOST_URL`, which is
  all the launcher normally adds.
- Delete the sandbox with `sandbox_client.py remove` only. A plain recursive
  delete can follow the junction and delete the real game's assets.
- Saved settings and logins are shared with the real game: Unity keeps them
  per Windows user.

For repeatable checks, [client automation](../docs/client-automation.md) starts
an isolated server and database and drives the game through scenarios (login,
lobby, shops, battles). Point it at the sandbox with `--client ../sprk-client-test`.

## Change the game

Edit the `.cs` files under `managed/` and rebuild. The source is ordinary
maintained code: never regenerate it from a DLL or patch a compiled DLL.

- **New feature assembly.** Create `managed/SprkName/SprkName.csproj` containing
  `<Project Sdk="Microsoft.NET.Sdk" />`, add its source and call it from the game
  code. The build, release and launcher pick it up automatically. Deleting a
  directory removes its DLL from the next release.
- **Serialized fields.** Unity loads components and assets by field name and
  declaration order. In `MonoBehaviour`, `ScriptableObject` and `[Serializable]`
  types, do not reorder, rename, retype or remove existing fields; add new ones
  at the end or mark them `[NonSerialized]`.
- **Names.** Keep assembly, namespace and type names that assets, reflection or
  the server refer to.
- **Server agreement.** Some server tests read client source, for example the
  read-only request list in `managed/SprkNetworkPerformance/SprkNetworkPerformance.cs`.
- **Logs.** Info-level `Debug.Log` output does not reach the game log; write
  diagnostics to a file when testing.

### How the build works

Projects compile against the game's own runtime DLLs, never the .NET SDK's
libraries: `references.lock.json` pins those 144 files, and the build refuses
any other version. The `netstandard2.1` target only enables SDK tooling.

The game calls the feature assemblies and they call the game. The build
resolves that cycle without any previously built DLL:

1. Compile all source into one temporary assembly.
2. Compile the feature assemblies against it, then the game against them.
3. Compile the final feature assemblies, then the final game, against those.

The output is deterministic: the same source and SDK give byte-identical DLLs.
After one complete build, an IDE or `dotnet build` on a single project works too,
using the references staged in `artifacts/client-build/references`.

## Change assets or tables

There is no source form for assets or tables. To ship a changed file, put its
final version under `overrides/files/` at its client-relative path, for example
`overrides/files/King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit/ItemTable.jit`,
together with any `SPRK-Native*.json` integrity file that covers it. Server-side
game tables are separate and live in `../tables/`.

## Launcher

```powershell
client/scripts/launcher/build_launcher.ps1 -ClientPath ../sprk-client   # build and install SprkLauncher.exe
dotnet run --project client/launcher/SprkLauncher.Tests                 # checks
```

The launcher keeps server profiles in `sprk-launcher.json` next to the game,
checks the selected server's signed manifest, downloads only changed files and
starts the game with that server's address in `SPRK_HOST_URL`.

## Publish client updates

### Through CI (normal)

Merging into `main` builds the update from tracked inputs only:

```text
pinned published release (player, assets, tables, runtime DLLs)   scripts/release/client_baseline.py
  + overrides/files/
  + DLLs compiled from managed/
  = signed manifest + client-update.tar.gz, published as a GitHub release
```

The matching server image is pinned to that release and installs it at startup.
Check the complete build locally without a signing key or any upload:

```powershell
python client/scripts/release/build_client_release.py --work artifacts/release-check --version check-1 --check-only
```

Details: [GitHub CI and client updates](../docs/github-client-updates.md).
`scripts/release/pin_client_baseline.py` moves the baseline to a newer published
release; routine changes never need it.

### Locally (for start_server.bat)

The local update service serves whatever was last published from your game
folder. After installing new DLLs into `../sprk-client`, publish it:

```powershell
python client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version local-YYYYMMDD-name --signing-key client-updates/update-signing/private.pem
```

Use a new version name every time. The default selection keeps every file the
previous release managed. See [client updates](../docs/client-updates.md),
including unsigned publishing for contributors without the signing key.

### New players

New players still need a complete game folder (the client ZIP). Any recent one
works: the launcher brings the managed files up to the current release on its
first start. Refresh the ZIP occasionally to shorten that first update.

## Tests

```powershell
python -m unittest discover -s client/scripts -t . -p "test_*.py"
dotnet run --project client/launcher/SprkLauncher.Tests
```

## Inspect a DLL

The source is the reference, but to look at a compiled or third-party DLL:

```powershell
cd client
dotnet run --project tools/ManagedSource -- inspect <assembly.dll> <full-type-name> [method-name]
```

See [tools/README.md](tools/README.md) for comparing two builds.

## History

`managed/` was recovered once, with ILSpy, from the last release built by the
former DLL patcher (`client-ci-2795d44b755a-43152391931-1`), then repaired until
it compiled. The patcher and the recovery records were deleted afterwards; git
history still has them (`git show 470d93a2:client/legacy/README.md`).

Checked against that release on 2026-11-12: all 15,350 types keep the same
serialized field layout, base types and interfaces; login, lobby, hero,
inventory and Special Shop screens and a campaign battle worked on the real game
with no new errors; the server's native battle worker test passed. One
intentional difference: some currency methods now use checked 32-bit
conversions, so a value above that range throws instead of wrapping.
