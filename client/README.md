# SPRK client

The game's own C# code and the SPRK features built on top of it, the Windows
launcher, and the tools that build, test and publish client updates.

Run every command from the repository root.

## What is source and what is not

A game folder (`sprk-client`) is a Unity player plus assets. Only two kinds of
file in it are built from this directory:

| Built here                                                       | Comes from an existing game folder or release                        |
| ---------------------------------------------------------------- | -------------------------------------------------------------------- |
| `King's Raid_Data/Managed/Assembly-CSharp.dll` (the game's code) | `King's Raid.exe`, `UnityPlayer.dll`, `MonoBleedingEdge/`, `D3D12/`  |
| `King's Raid_Data/Managed/Sprk*.dll` (SPRK features)             | Unity, Mono and third-party DLLs in `Managed/`                       |
| `SprkLauncher.exe`                                               | Scenes, asset bundles, tables, `King's Raid_Data/Documents/` (19 GB) |

## Layout

| Path                            | Contents                                                                    |
| ------------------------------- | --------------------------------------------------------------------------- |
| `managed/Assembly-CSharp/`      | The game's C# code                                                          |
| `managed/Sprk*/`                | SPRK feature assemblies, one DLL per directory                              |
| `managed/Directory.Build.props` | Compiler settings shared by every project                                   |
| `managed/references.lock.json`  | SHA-256 pins of the game runtime DLLs the code compiles against             |
| `launcher/`                     | `SprkLauncher.exe` (server profiles, signed updates) and its tests          |
| `overrides/files/`              | Replacement assets and tables, by client-relative path (empty until needed) |
| `dev.py`                        | Everyday commands: build, test, install, clean                              |
| `scripts/build/`                | Compiler and test-copy code used by `dev.py` and CI                         |
| `scripts/release/`              | Build, sign, publish and pin client updates                                 |
| `scripts/automation/`           | Drive the real game in tests                                                |
| `scripts/launcher/`             | Launcher installation                                                       |
| `android/`                      | Android build of the same code (HybridCLR): `android/dev.py`, Unity project |
| `tools/ManagedSource/`          | Read-only DLL inspection and comparison                                     |
| `global.json`                   | .NET SDK version                                                            |

## Requirements

- Windows.
- [.NET SDK 10.0.1xx](https://dotnet.microsoft.com/download) (`global.json`
  accepts any 10.0.1xx patch).
- Python 3.11+. The build and release scripts need no extra packages.
- A game folder at `../sprk-client`, next to this repository, kept current by
  starting it once through its launcher. The build reads the game's runtime
  DLLs from it and checks each against `managed/references.lock.json`.

## Everyday commands

One script covers the whole loop:

```powershell
python client/dev.py test       # build, then start a separate test copy of the game
python client/dev.py install    # build, put it into your real game, publish a local release
                                #   and new asset versions when assets changed
python client/dev.py build      # only compile, to check for errors
python client/dev.py clean      # delete the test copy
```

| Command   | Changes your real game? | How you play afterwards         |
| --------- | ----------------------- | ------------------------------- |
| `test`    | No                      | It starts the test copy for you |
| `install` | Yes                     | SprkLauncher, server **Local**  |

For the Android build, use `python client/android/dev.py` (see [Android](#android)).

### Try a change: `test`

1. Start the local server with `start_server.bat` (see the
   [server README](../server/README.md)).
2. Close the game if it is open; only one copy can run at a time.
3. Run `python client/dev.py test`. Rerun it after each change.

The first run creates `../sprk-client-test`: the player files are copied
(1.2 GB) and the 19 GB `King's Raid_Data/Documents` folder is shared with your
real game through a link. Each run then compiles the code (about 40 seconds),
puts the DLLs into the test copy and starts it on the local server; the test
copy never connects to the public server. Add `--no-build` to start it again
without compiling.

The test copy has no launcher on purpose: the launcher always updates before
playing, which would replace your new DLLs with the published ones.

Delete the test copy only with `python client/dev.py clean`. It refuses while
the game runs and never follows the link, whereas a plain recursive delete can
reach the real game's assets. Saved settings and logins are shared with the
real game, because Unity keeps them per Windows user.

### Keep a change: `install`

`python client/dev.py install` compiles the code, copies the DLLs into
`../sprk-client` and publishes them as a new local release. Then play as usual:
SprkLauncher on the **Local** server finds nothing to replace, because the
release matches your game. Without the project's signing key the release is
unsigned; the command explains the one-time launcher setting this needs.

When an override changed the game's assets (an asset bundle), `install` also publishes
new Windows and Android asset versions into `client-updates/assets`, so the local server
serves them. `--no-assets` skips this. Players get the same change after the merge, with
no upload: the update service applies the release's asset overlay
([Game assets](../docs/game-assets.md#edited-assets-reach-players-through-ci)).

Close the local server before `install`: its battle worker runs the real game and keeps
the DLLs open, and the copy then fails with `[Errno 22] Invalid argument`.

To share a change with players, commit it; CI builds the public release (see
[Publish client updates](#publish-client-updates)).

### Options

`--game <folder>` uses a game folder other than `../sprk-client`, for example
`python client/dev.py --game D:\games\sprk-client test`. The scripts under
`scripts/build/` are the building blocks `dev.py` and CI use;
`build_managed_client.py --help` lists their lower-level options.

For repeatable checks, [client automation](../docs/client-automation.md) starts
an isolated server and database and drives the game through scenarios (login,
lobby, shops, battles). Point it at the test copy with `--client ../sprk-client-test`.

## Change the game

Edit the `.cs` files under `managed/` and rebuild. Never patch a compiled DLL.

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
game tables are separate and live in `../server/tables/`.

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

To clean up `overrides/files/`, never just delete it: the next release would
ship the baseline's old files again, and launchers would put them back on
players' machines. Once a release containing the overrides is published, pin
it as the baseline and delete those overrides in the same pull request:

```bash
python client/scripts/release/pin_client_baseline.py --tag client-<that version>
```

The files stay in every release (now from the baseline), so they do not belong
in `scripts/release/unmanaged_files.txt`.

### Locally (for start_server.bat)

The local update service serves the newest release in `client-updates/`. On a
fresh clone `start_server.bat` downloads the public one; `python client/dev.py
install` publishes your own build. To publish by hand, for example after
changing assets:

```powershell
python client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --version local-YYYYMMDD-name --signing-key client-updates/update-signing/private.pem
```

Use a new version name every time. The default selection keeps every file the
previous release managed. See [client updates](../docs/client-updates.md),
including unsigned publishing for contributors without the signing key.

### Files releases no longer manage

`scripts/release/unmanaged_files.txt` lists client files that releases stop
managing. A listed file is left out of every new release but is **not** marked
deleted, so the launcher leaves each player's existing copy alone; it just can
no longer update or repair it. Use it to keep big, finished assets (for
example restored models or login backgrounds) out of every release once all
players and the current client ZIP have them. The file explains the conditions,
including running the native battle tests first, because the server's battle
worker is built from the release too. Remove a line to manage a file again.

## Android

`android/` builds the same code for Android: a small Unity 6000.0.53f1 player
(`android/HybridBootstrap/`) loads the compiled game DLLs at startup and runs them with
[HybridCLR](https://github.com/focus-creative-games/hybridclr). The game's built-in scenes
and Resources are repacked from the Windows client into Android bundles, and the device
gets a patch folder with the Windows tables, localization and video plus the original
Android asset bundles. It is a prototype; open work is listed in
[docs/android.md](../docs/android.md).

### Requirements

- Unity **6000.0.53f1** with Android Build Support, installed through Unity Hub. **Hub
  must be running and signed in** during builds; the editor gets its licence from Hub.
- `pip install -r client/scripts/requirements-client.txt` (UnityPy, Pillow).
- Next to the repository: `../sprk-client` (the Windows client) and `../sprk-apk`, the
  pinned original Android inputs: `kings-raid-4.88.8.apk` (for `content` only) and
  `Assetbundle/` (the original Android bundles, from `Documents/Patch/Android/Assetbundle`).
  `../sprk-android` is the maintained Android asset set built from them.
- A device: BlueStacks with ADB enabled (`HD-Adb.exe`, serial `emulator-5554`, the
  default) or a phone over USB (`--adb <adb.exe> --serial <id>`, see `adb devices`).
- The local server (`start_server.bat`).

### Commands

`android/dev.py` is the Android counterpart of `dev.py`:

```powershell
python client/android/dev.py content --apk <path>\kings-raid-4.88.8.apk  # once, and after Windows player-data or repacker changes
python client/android/dev.py build          # compile the game code, build the APK
python client/android/dev.py test           # build, install, push missing device data, start on the local server
python client/android/dev.py test --picker  # same, but choose the server on the device
python client/android/dev.py hot            # C# only: compile, push the DLLs, restart (no APK rebuild)
python client/android/dev.py connect        # restore the adb forwarding to the local server
```

| Change                                       | Run                                             |
| -------------------------------------------- | ----------------------------------------------- |
| C# in `managed/`                             | `hot`                                           |
| Bootstrap or Unity project (`android/...`)   | `test`                                          |
| Repacker or the Windows client's player data | `content --apk ...`, then `test`                |
| Tables or localization in the Windows client | `android/HybridBootstrap/push_patch.sh --force` |

The first `test` on a device pushes the 8.6 GB patch folder (about 4 minutes on
BlueStacks); later runs skip what is already there. `hot` puts the DLLs in
`files/sprk/hot/` on the device, where the bootstrap prefers them over the APK's copies;
`test` removes them again. `dev.py` finds Unity through Unity Hub (`--unity` or
`SPRK_UNITY` override it) and runs the device scripts with Git Bash. Unity logs go to
`artifacts/android-bootstrap/`, with the APK (`apk/sprk-hybrid-probe.apk`), the repacked
content (`content/`) and the reference bundles (`templates/`).

### Servers and accounts

The game reads its server from `SPRK_HOST_URL`; SprkLauncher sets it on Windows, and on
Android the bootstrap sets it before any game code runs:

- With `--picker` the app opens on a server list: **Local** plus servers added on the
  device (name and `host.json` URL), like SprkLauncher's. Remote URLs must be
  `https://`; plain `http://` only works for 127.0.0.1, as in the game. The list is kept
  in the app's private storage, and saved logins are kept per server.
- Without it (`server:local` in the startup index, written by `dev.py test`), the app
  goes straight to Local, so unattended runs never wait for input.
- After **Play**, the app updates the game code and platform-neutral data from that
  server's signed release, like SprkLauncher (see
  [docs/android.md](../docs/android.md#game-updates-and-assets-from-the-server)). An
  added server can set its update URL; empty means `<origin>/updates/stable/manifest.json`.
  Unattended runs skip the update, so files pushed by `dev.py hot` stay in place.

**Local** is `http://127.0.0.1:8080/host.json` on the device, forwarded to this PC by
`adb reverse` (emulator or USB). The forwarding is lost whenever adb restarts; if the
title screen says _"Cannot read Host files due to bad internet connection"_, run
`python client/android/dev.py connect`.

Accounts: _Create an Account_ registers on the server you picked. New accounts start in
the tutorial; _Skip Tutorial_ leads to the lobby. To drive the UI from the PC use
`adb shell input tap X Y` and `adb shell input text ...`; do not send `keyevent 111`
(Escape), which Android treats as Back (the game asks to exit).

### Logs

- `files/sprk-probe.txt` on the device
  (`adb shell cat /sdcard/Android/data/com.sprk.hybridprobe/files/sprk-probe.txt`):
  startup steps with timestamps, the first occurrence of each error with its stack, and a
  `perf` line every 5 s (fps, frame times, frames over 50 ms, GCs, heap, target fps).
- logcat: `adb logcat -v time Unity:V CRASH:E DEBUG:E '*:S'`; `dev.py test` saves it to
  `artifacts/android-bootstrap/probe-logcat.txt`.
- Switches, as lines in the device's `files/sprk/index.txt`: `debug:overlay` (log on
  screen), `debug:probes` (startup self-tests), `server:local` (skip the server list).

Normal at startup: _"… Table Cannot Load"_ before the patch step, the
`VuplexWebViewWindows` DLL error and _"Unable to decode certificate"_.

### Writing code that runs on both

The same DLLs ship on Windows and Android, so platform differences are runtime checks
(`Application.platform`), never `#if`:

- Put a Windows-only native import (`[DllImport("user32.dll")]` and similar) in a method
  Android never calls. HybridCLR binds imports when it prepares the calling method, so a
  check inside that method comes too late (see `DeviceHelper.GetWindowsGeoName`).
- Reach the patch folder through `UnityCache.GetDownloadedAssetPath(...)`, not
  `Application.dataPath`; on Android the player data is the read-only APK.
- `Application.targetFrameRate = -1` means 30 fps on mobile; desktop uses VSync.

### Scripts

In `android/HybridBootstrap/` (what `dev.py` runs):

| Script                              | Purpose                                                                       |
| ----------------------------------- | ----------------------------------------------------------------------------- |
| `sync_inputs.py`                    | Game DLLs, AOT plugins from `../sprk-client`, app icon into the Unity project |
| `content/extract_apk_data.py`       | Original APK player data → `artifacts/android-bootstrap/apk-data`             |
| `content/repack_content.py`         | Windows player data → Android content bundles and `resources-map.json`        |
| `content/patch_player_resources.py` | Run by the build: the player's Resources and shader-name tables               |
| `push_patch.sh`, `push_content.sh`  | Patch folder; content bundles and startup index                               |
| `push_hot.sh`                       | Game DLLs as overrides                                                        |
| `connect_local.sh`                  | `adb reverse` for 8080, 9001 and 9002                                         |
| `run_probe.sh [--install]`          | Install, forward, launch, wait, print the probe log and errors, screenshot    |
| `measure_battle.sh <label>`         | Saved-account login, lobby and stage 1-4 frame-time sample                    |

The Unity build entry points are `SprkAndroidBuild.Build` and
`SprkContentTemplates.Build` (`android/HybridBootstrap/Assets/SprkBootstrap/Editor/`);
the repacker's docstrings explain the asset-bundle rules it follows.

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

### New players

New players still need a complete game folder (the client). Any recent one
works: the launcher brings the managed files up to the current release on its
first start. Refresh the client occasionally to shorten that first update.

## Compatibility

The source build was checked against client release
`client-ci-2795d44b755a-43152391931-1` on 2026-11-12: all 15,350 types keep the same
serialized field layout, base types and interfaces; login, lobby, hero,
inventory and Special Shop screens and a campaign battle worked on the real game
with no new errors; the server's native battle worker test passed. One
intentional difference: some currency methods now use checked 32-bit
conversions, so a value above that range throws instead of wrapping.
