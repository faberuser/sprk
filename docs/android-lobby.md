# Android local lobby build

The archived x86 APK now reaches the Orvel lobby using SPRK password accounts.
The test uses the original Android build1558 downloaded content. Localization
is retained: restoring the complete archive fixed the initial missing
`Localization_MonthlyStoryEnglish.csv` error. No Windows localization or Windows
asset bundles are copied into Android.

The APK and archive use different metadata layouts. The archive's
`Assetbundle/BundlePack.bin` holds indexed BJS records; the APK expects a
consolidated `Assetbundle/BundlePack.binv2`. The conversion tool assembles the
original records and sprite links, then uses the installed Windows client's
native JsonBin serializer to write the expected format. It only reads Windows
files. The original Android `.bin`, localization, and actual bundles remain
unchanged. Shared sprite aliases follow archive declaration order, with later
declarations taking priority; the conversion report records their count.

`scripts/android/lobby.js` also adapts loopback WebSocket addresses to the
advertised native TCP port, adds the current authenticated game session to the
old chat LoginReq serializer (the same fix as Windows PatchChatSession), resolves
existing local bundles when the server's empty CDN index omits them, and skips
the unavailable external-store billing initialization. Authentication stays
server-validated. Missing assets still fail; the adapter does not fabricate them
or force the bundle-ready flag.

## Reproduce

Run from `sprk-server`; Java, Python dependencies from
`scripts/requirements-client.txt`, and .NET 10 SDK must be available:

```powershell
python scripts/build_android_accounts.py --output target/android-lobby/new-apk
python scripts/convert_android_bundle_pack.py --output target/android-lobby/new-pack
```

The converter pins the original Android pack hash. Its report records the native
serializer DLL hash and verifies the entire decoded output against the assembled
source tree (float32 precision tolerance: relative 2e-6, absolute 1e-6).

Prepare a separate staging directory with this structure:

```text
client/
  SPRK.apk                         # signed APK from new-apk/signed/
  files/Documents/Patch/Android/   # complete archived Android directory
    Assetbundle/BundlePack.binv2   # added output from new-pack/
```

Publish a unique signed Android snapshot, using a separate output directory from
the Windows update channel:

```powershell
python scripts/publish_client_update.py --client <client> --output target/android-client-updates --channel stable --version <unique-version> --signing-key client-updates/update-signing/private.pem --include SPRK.apk --include files/Documents/Patch/Android
python scripts/install_android_content.py --release target/android-client-updates/releases/<unique-version> --report-dir target/android-lobby/<verification>
```

The installer verifies the RSA manifest and every local file before installing.
It restores the complete content tree, verifies the installed APK hash, then
checks every downloaded file on Android. `--verify-only` skips installation and
copying, but still force-stops the app for a consistent verification. Legacy
BlueStacks ADB needs directories precreated before pushing; the installer does
this automatically. Retain the original archive as the reproducible content
input. Original APK signing certificates are unavailable, so this uses the same
development APK certificate as the earlier account test.

For the normal local server, enable BlueStacks ADB and apply these mappings after
each emulator restart (use the server's actual advertised chat/battle ports):

```powershell
& 'C:\Program Files\BlueStacks_nxt\HD-Adb.exe' -s emulator-5554 reverse tcp:8080 tcp:8080
& 'C:\Program Files\BlueStacks_nxt\HD-Adb.exe' -s emulator-5554 reverse tcp:9001 tcp:9001
& 'C:\Program Files\BlueStacks_nxt\HD-Adb.exe' -s emulator-5554 reverse tcp:9002 tcp:9002
```

This remains a local x86 test build. Remote WebSocket/TLS transport and ARM
support are not implemented. Obsolete remote shop-banner URLs can fail; those
failures did not prevent the lobby. Combat and the full game feature set have
not been validated by this lobby test.

## October 22, 2026 verification

- BlueStacks Pie64 / Android 9: password sign-in, native `/user/login`, authenticated
  native chat, restored bundle loading, intro video, Orvel town and tutorial.
- Converted 9,339 indexed metadata records and 19,785 sprite links; checked all
  4,012,430 values after native binary roundtrip.
- Final APK built from a fresh decode; ZIP integrity, unchanged original payloads,
  zipalignment and v1/v2/v3 APK signatures verified by the builder.
- Stable and release manifests match; RSA signature and all 23,844 release files
  verified. The installed APK and all 23,843 external Android files match their
  signed snapshot hashes. Normal local ADB mappings were restored after testing.

Friendly APK: `../android-test-apks/SPRK-Local-Lobby-Signed.apk`.
Final build and screenshot evidence: `target/android-lobby/20261022-lobby-final/`.
Signed local snapshot: `local-20261022-android-lobby-02`, under
`target/android-client-updates/releases/`; its verification report is under
`target/android-lobby/20261022-release-verification/`.
This is a separate local Android artifact, not a GitHub release or deployment.
