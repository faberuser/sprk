# Android endpoint test

The archived APK named `KingsRaid 9-1-2022.apk` identifies itself as Android
`04.88.008` (manifest version `4.88.8`). It contains x86 native libraries only.
Use a compatible x86 Android test environment. These APKs do not add ARM support.

## Prepared artifacts

Builds started on October 21, 2026, Bangkok time:

| Build directory under `target/android-endpoint-tests/` | QueryHost |
| --- | --- |
| `20261021-local-01` | `http://127.0.0.1:8080/host.json` |
| `20261021-hosted-01` | `https://play.krinfo.net/host.json` |

Each build has `signed/sprk-android-endpoint-aligned-debugSigned.apk`, a
`build-report.json`, and `signature-verification.txt`. Install the signed APK.
Convenient copies are also available in the workspace's `../android-test-apks/`
folder as `SPRK-Local-Signed.apk` and `SPRK-Hosted-Signed.apk`.
Do not install `sprk-android-endpoint-unsigned.apk`: it is an intermediate build.
BlueStacks' October 22 install log confirmed that selecting this intermediate
causes `INSTALL_PARSE_FAILED_NO_CERTIFICATES` (status -103), displayed as a generic
installation error. That attempt did not reach game startup.
Both builds use the same development certificate. They keep the original package
ID, so use a clean emulator instance rather than replacing an original-signed
installation containing player data. The development signature cannot update an
installation signed by the original publisher.

Only the Config TextAsset's `QueryHost` changes. All 4,742 non-signature ZIP
entries are compared against the original after signing; only that asset differs.
The replacement JSON has trailing whitespace to preserve the serialized string
length and every Unity byte offset. All other config fields remain identical.
The source APK and extracted archive are not modified.

The APK is zipaligned and signatures v1/v2/v3 verify. The verifier reports 45
warnings about retained META-INF library metadata not covered by JAR/v1 signing;
v2/v3 APK signature verification succeeds. These are development test builds,
not production release certificates or Windows launcher updates.

## Run a local test later

Run the SPRK game server with its normal local configuration. Its `/host.json`
defaults to advertising `127.0.0.1:8080` and already contains an Android platform
entry. If overriding server environment variables, ensure returned game and login
addresses are reachable from the Android environment. This APK already declares
`usesCleartextTraffic=true`, so no manifest edit is needed for local HTTP.

With an x86-compatible Android environment connected to ADB, run from this repo:

```powershell
adb install target/android-endpoint-tests/20261021-local-01/signed/sprk-android-endpoint-aligned-debugSigned.apk
adb reverse tcp:8080 tcp:8080
adb shell am start -n com.vespainteractive.KingsRaid/com.vespa.kingsraid.MainActivity
adb logcat -v threadtime > target/android-endpoint-tests/android-logcat.txt
```

Port reversal makes the Android loopback address reach this PC's server. Reapply
it after reconnecting the environment. Capture server request logs as well.

The archived downloaded assets are separate from the APK, under
`../Pre-Doomsday/com.vespainteractive.KingsRaid/files/`. They may need restoring
into the test app's external files directory before it can get beyond patching.
On an emulator permitting ADB writes to external app data, after installation:

```powershell
adb push ../Pre-Doomsday/com.vespainteractive.KingsRaid /sdcard/Android/data/
```

If Android blocks that directory, restore through the emulator's supported file
transfer/root facilities; do not assume the APK includes those downloaded assets.

The hosted build needs no local port reversal, but an initial probe to its host
URL returned HTTP 403. Access from Android is unverified. Authentication, protocol
compatibility, original telemetry and AppGuard protection are unchanged. AppGuard
may reject modified/re-signed packages in other environments. Successful login
is a later compatibility milestone, not a verified property of this endpoint patch.

## BlueStacks connection verified October 22

The local signed build installed and launched on BlueStacks Pie64 (Android 9),
whose ABI list includes x86. Its initial host-file error was a connection refusal
to Android's `127.0.0.1:8080`, recorded in Unity's Android log. SPRK itself was
already serving `/host.json` successfully on the PC.

BlueStacks Settings > Advanced > Android Debug Bridge must be enabled and saved;
when disabled, the bundled ADB listed the emulator but rejected reverse forwarding
with `error: closed`. After the user enabled it, this command succeeded:

```powershell
& 'C:\Program Files\BlueStacks_nxt\HD-Adb.exe' -s emulator-5554 reverse tcp:8080 tcp:8080
```

The reverse mapping was verified and an HTTP probe through Android's forwarded
port returned HTTP 200 with SPRK's host JSON. Restarting the game advanced past
the host error to its Terms of Service and Privacy Policy screen. The original
CloudFront agreement pages failed DNS resolution. No agreement was accepted and
login has not been tested. No new APK was needed for this connection fix.

Evidence is saved under `target/android-endpoint-tests/20261022-connection-check/`
(`http-check.json`, `android-logcat.txt`, and `screen.png`). Reapply the reverse
mapping after restarting BlueStacks or reconnecting ADB.

## Reproduce

Install `scripts/requirements-client.txt` and Java. Obtain
[`uber-apk-signer-1.3.0.jar`](https://github.com/patrickfav/uber-apk-signer/releases/tag/v1.3.0)
and its published checksum file. The builder requires signer SHA-256
`e1299fd6fcf4da527dd53735b56127e8ea922a321128123b9c32d619bba1d835`.
The local copy is under `target/android-tools/`.

```powershell
python scripts/build_android_endpoint_test.py --source "../Pre-Doomsday/KingsRaid 9-1-2022.apk" --host-url http://127.0.0.1:8080/host.json --signer target/android-tools/uber-apk-signer-1.3.0.jar --output target/android-endpoint-tests/new-unique-local-build
```

Use a new output directory for every build. The script refuses an existing
directory and unexpected original configs. It verifies the signer checksum,
signature, ZIP integrity, final config, unchanged remaining payload, and unchanged
source APK before writing the build report. This workflow prepares separate APK
artifacts; it does not install or alter the launcher-managed Windows client.
