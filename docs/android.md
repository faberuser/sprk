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

## Planned: game assets downloaded from the server

Goal: the APK installs on its own and downloads the game patch (asset bundles, tables,
localization, video) from the server it connects to, instead of `adb push`. Later the
Windows client can use the same path instead of shipping the patch with each install.

What already exists:

- **The game's own downloader** (`NGameAsset.NPatch.PatchDownloader`, driven by
  `StateBase_PatchDownload` at login). It reads `<cdn>/LastBuildVersion.txt`, then
  `<cdn>/<version>/patch.json`, compares it with the local
  `Documents/Patch/<platform>/PatchInfo/patch.json`, and downloads the missing
  archives from `<cdn>/<version>/<archive>`. `patch.json` lists archives (`.cb` files),
  each with its entries (path under the patch folder, length, MD5, CRC, download
  category and priority); categories allow background and on-demand downloads, and
  archives are grouped by quality tier (`NoSpec`, …).
- **The CDN URL** comes from the server (`CdnProxyUrl` = `<server>/cdn/`). The server
  currently answers `LastBuildVersion.txt` with `1` and `patch.json` with no archives
  (`server/src/api/system/cdn.rs`), so the client downloads nothing and uses what is
  installed.
- **The launcher's signed update system** (`client-updates/`, release manifests with
  SHA-256 per file) keeps the Windows install's managed files in sync. It is separate
  from the game's patch downloader.

Work to do:

1. **Per-platform asset sets.** The client uses the folder name `StandaloneWindows` on
   both platforms (`DefinePlatform.PlatformName`), but the bundles differ: Android needs
   the Android bundles (ETC/ASTC textures, GLES shaders), Windows the Windows ones.
   The client has to tell the server its platform (CDN path or request field), and the
   server serves a matching `patch.json` and archive set.
2. **Building the archives.** A tool that packs the patch folder (Android bundle set or
   Windows bundle set, plus the shared tables, localization, PlainAsset and video) into
   `.cb` archives and writes `patch.json`, with `LastBuildVersion` bumped per release.
   Split by quality tier and language so a device downloads only its tier and language,
   and mark the minimum needed to reach the lobby as the first category.
3. **Serving them.** Host the archives on the server (`/cdn/<version>/...`) or on
   external storage it redirects to. The Android set is about 8.6 GB and the Windows set
   about 19 GB, so downloads must be resumable and verified (the entries carry MD5/CRC;
   consider signing `patch.json` as the launcher manifests are).
4. **Repacked built-in content.** The ~420 MB of repacked scenes, Resources and
   shaders (`sprk/content/` on the device) either ships inside the APK or becomes part of
   the downloaded set.
5. **Windows.** Point the Windows client at the same mechanism, then decide how it
   coexists with the launcher's updates (for example, the launcher keeps managing code
   and tables while the game downloads asset bundles).
6. **Docker deployment.** Storage for the archives in the server image or a volume, and
   the version pinning that already ties each server release to its client release.

## Distribution

- The package id is `com.sprk.hybridprobe` and the APK is debug-signed. Changing the id
  makes a new app (the 8.6 GB patch folder and saved logins must be set up again).
- Device content arrives by `adb push` (see the planned download above); there is no CI
  build and no link to the launcher's update system.
- Remote servers must serve `host.json` over HTTPS (plain HTTP only works for loopback),
  so a phone away from the PC needs an HTTPS server.
- `client/android/HybridBootstrap/.utmp/` (Unity build cache) was committed in
  `7f4fa7e1` and is now ignored; remove it from the index.
