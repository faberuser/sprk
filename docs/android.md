# Android build: open work

What is left before the Android build (the HybridCLR prototype in `client/android/`) is
more than a test build. How to build, install and test it is in
[client/README.md](../client/README.md#android).

Current state: on BlueStacks it starts in about 3 seconds, logs in (local server or a
server added in the in-app list), reaches the lobby at the display's refresh rate
(144 fps measured) and plays campaign battles.

## Needs testing

- **Real phones.** Everything so far ran on BlueStacks only (x86 host, ARM translation).
  Check startup, rendering, memory and thermals on ARM phones and other screen sizes.
- **World-map pinch zoom.** Now zooms by the change in finger distance (the old code
  averaged both fingers' movement, which cancels out for a pinch). Not tried on a touch
  screen yet.
- **Battles beyond the campaign.** PvP, co-op, dispatch, long sessions. Battle frame
  times have not been measured (`measure_battle.sh` taps stage 1-4, whose position on
  the map depends on progress).
- **Windows after the shared-code changes.** The Android changes in `client/managed`
  are runtime-guarded and keep Windows behaviour, but have not been run through
  [client automation](client-automation.md). The login-logo fallback in
  `SprkLoginPresentation` and the world-map pinch change are not in a Windows release
  yet.

## Rendering and content

- Some UI panels render as solid black boxes (the attendance reward banner, a shop
  panel); probably render textures or effect shaders.
- About 474 bundles exist only in the Windows set (skill effects, pets, scenario voice,
  `NguiAtlas_Icon_Item_Item9`, …). They need Android builds or a conversion; until then
  that content fails to load. (The four Windows-only "heroes" `Ybb`, `lisa`, `capy` and
  `testtest` are Mixamo-rigged test models that nothing references, not released heroes.)
- The Android data has only the High/Mid quality tiers and the English, Portuguese and
  Spanish language bundles; the in-game **Low** graphics setting and the other languages
  look for bundles that are not on the device.
- 6 of the 245 GLES shaders report as unsupported. The Vuplex web-view shaders have no
  Android build (web views are Windows-only).
- `resources.assets` keeps the Windows texture formats (DXT/BC). BlueStacks shows them;
  phones without S3TC need checking and probably transcoding in the repacker.
- 9 MonoBehaviour script types are repacked with base fields only, because UnityPy cannot
  generate their fields (the repacker lists them); their serialized settings load as
  defaults.

## Input

- Mouse-wheel zoom does nothing on BlueStacks: the game reads the wheel through Unity's
  legacy input, which does not receive it there (switching the app to the classic
  Android Activity did not help). Low priority while phones are the target.

## Performance

- All game code is interpreted by HybridCLR. Options if battles are slow: incremental GC
  (`PlayerSettings.gcIncremental`, currently off), or compiling the game assemblies into
  the player ahead of time.
- Scene loads stall for 0.4–1.7 s while bundles, atlases and models load (as on Windows).
- An x86_64 build for emulators was set aside; BlueStacks runs the ARM64 build through
  its translation layer.

## Game code

- `LoginEncrypter` cannot decode the server certificate under IL2CPP (*"Unable to decode
  certificate"*). Login works because SPRK uses token authentication, but the encrypted
  login id is empty. Decoding the PEM to DER first did not help.
- Saved logins and the server list are stored unencrypted in the app's private storage;
  the Android Keystore would be the next hardening step.

## Game updates and assets from the server

Goal: the APK installs on its own and gets everything else from the server it connects
to, instead of `adb push`. Windows does this through SprkLauncher
([game assets](game-assets.md)).

**Done: code and platform-neutral data.** After the server picker, the bootstrap
(`SprkUpdater`, rules in `SprkUpdate.cs`) installs from the server's signed release
manifest, the same one SprkLauncher uses:

- game assemblies (`Assembly-CSharp`, every `Sprk*.dll`) into `sprk/hot/`, so new
  features (for example a new `Sprk*` assembly) need no new APK;
- tables, localization, `PlainAsset`, `LoginPresentation`, video and the other
  platform-neutral patch files into `Documents/Patch/StandaloneWindows/` (about 750 MB);
- not Windows asset bundles, and not the game's patch records (`PatchInfo/`,
  `patch.json`, `patch.bjs`), which the game's downloader owns on Android.

The manifest is verified with `config/client-update-public.pem`, built into the APK
(unsigned manifests only from 127.0.0.1). A server's update URL defaults to
`<its origin>/updates/stable/manifest.json` (Local: port 8081); an added server can set
another. Downloads resume, run four at a time and are hash-checked before they replace a
file; unchanged files are not re-hashed (size and write time). If the server cannot be
reached, the player can retry or play the installed version. Unattended dev runs
(`server:local`) skip the update, so `dev.py hot` and `push_patch.sh` keep working.
`dotnet run --project client/android/UpdateTests` checks the rules against the local
release. Not run on a device yet: the APK build needs the repacked content first.

**Next: Android asset bundles** through the game's own patch downloader
(`PatchDownloader`, fixed to download and verify Raw archives), published as an
`Android` asset set (`publish_assets.py --platform Android --patch-prefix ""`) and served
when the game server sets `SPRK_ASSETS_CDN_URL`. The downloader filters by quality tier
and language and can fetch most content in the background. The original Android set
(`Pre-Doomsday/.../Patch/Android/Assetbundle`, 7.8 GiB) needs per tier, English:

| Tier     | Download | Coverage                                             |
| -------- | -------- | ---------------------------------------------------- |
| Original | 6.25 GiB | complete except the ~285 Windows-only bundles        |
| Mid      | 4.38 GiB | complete except the ~300 Windows-only bundles        |
| Low      | (2.1 GiB)| no `LowSpec`/`LowLargeSpec` folders: 8,014 files missing |

Language bundles are about 30 MB each. Until a Low set exists, map Low to Mid (the game
recommends Low below 2.2 GB RAM and on devices without ASTC). The repacked built-in
content (`sprk/content/`, ~420 MB) still comes with `push_content.sh`.

## Distribution

- The package id is `com.sprk.hybridprobe` and the APK is debug-signed. Changing the id
  makes a new app (the 8.6 GB patch folder and saved logins must be set up again).
- Device content arrives by `adb push` (see the planned download above); there is no CI
  build and no link to the launcher's update system.
- Remote servers must serve `host.json` over HTTPS (plain HTTP only works for loopback),
  so a phone away from the PC needs an HTTPS server.
- `client/android/HybridBootstrap/.utmp/` (Unity build cache) was committed in
  `7f4fa7e1` and is now ignored; remove it from the index.
