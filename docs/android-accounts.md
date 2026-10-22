# Android SPRK account test build

The Android 04.88.008 x86 client now has a username/password sign-in and account
creation form backed by the same SPRK endpoints used by the Windows client.
The provider button table is hidden; provider/debug-account click handlers open
the SPRK form. The server selector in the form uses the advertised server list.

The adapter posts JSON credentials to `/api/auth/login` or `/api/auth/register`,
then verifies the access token with `/api/auth/token/verify`. Only the returned
account identity and game ticket enter the native game login state. The normal
`/user/login` request still consumes a server-validated, identity-bound ticket.
No server authentication rules were relaxed and no server source was changed.

Passwords and refresh tokens are not saved. This first implementation requires
signing in again after restarting the app; Windows' saved-account selector and
account-management UI are not ported yet. Password confirmation is required only
when creating an account. Failed authentication keeps the form open and clears
the password for retry.

## Build and installation

Run from `sprk-server` with Java and `scripts/requirements-client.txt` installed:

```powershell
python scripts/build_android_accounts.py --output target/android-accounts/new-unique-build
```

The default source is the untouched archived APK in `../Pre-Doomsday/`; its
SHA-256 is pinned. The default bootstrap URL is `http://127.0.0.1:8080/host.json`.
`--host-url` selects another host URL that fits the original serialized config.
The builder downloads checksum-pinned upstream dependencies under
`target/android-tools/`, decodes into the new build directory, patches, rebuilds,
signs, and verifies the APK. `--decoded` can reuse a disposable analysis decode
for development; the final October 22 build used a fresh decode.

Install the APK under the build's `signed/` directory. The final local artifact
is also copied to `../android-test-apks/SPRK-Local-Accounts-Signed.apk`.
It uses the same development certificate and package ID as the earlier endpoint
test, so it can update that installation with `adb install -r` and retain data.
It is not signed with the original publisher's certificate and cannot update an
original-signed installation.

For BlueStacks, enable ADB in Settings > Advanced and map the local game port:

```powershell
& 'C:\Program Files\BlueStacks_nxt\HD-Adb.exe' -s emulator-5554 reverse tcp:8080 tcp:8080
```

Reapply the mapping after reconnecting/restarting BlueStacks. This remains an
x86 Android build; no ARM libraries were added. The Windows client and its signed
update channel are unchanged.

## Implementation and verification

`scripts/android/accounts.js` supplies the native Android form and IL2CPP login
adapter. The unmodified Frida Gadget 16.7.19 x86 library loads that local bundled
script together with frida-il2cpp-bridge 0.9.1. There is no debugger listener,
remote script URL, or external script dependency at runtime. Package notices
and the bridge's MIT license are included in `assets/sprk/`.

The existing MainActivity static initializer loads the adapter. The obsolete
AppGuard Application wrapper is replaced with plain MultiDex startup, and its
Unity facade is inert. Runtime testing showed AppGuard's
`com.inca.security.IiiiIiiIii.run` calling `Process.killProcess` after opening the
custom form; disabling that old integration prevents the shutdown. This does
not replace SPRK's server authentication. Original SDK files and unrelated
telemetry cleanup are outside this initial account adapter.

The builder preserves the binary manifest, original native game libraries,
metadata, and all other APK payloads. Exactly three original payload entries
change: `classes.dex`, `classes2.dex`, and the startup Config asset. The account
runtime and license notices are added. It verifies final payload bytes against
their intended inputs, checks every other entry against the source, checks ZIP
integrity, verifies zipalignment and v1/v2/v3 signatures, and hashes the result.
`build-report.json` records those checks and dependency hashes. The retained
META-INF library metadata generates the same 45 JAR/v1 warnings as the endpoint
APK; v2/v3 verification succeeds.

Tested October 22, 2026 in BlueStacks Pie64 / Android 9, using a fresh isolated
SPRK process, separate database, fixture accounts, and temporary port forwarding:

- An incorrect password returned HTTP 401 and left the form available for retry.
- Correct credentials returned HTTP 200 from password login and token verification.
- Native `/user/login` returned HTTP 200 and created the expected fixture save.
- Mismatched confirmation was rejected by the form.
- Account creation returned HTTP 200, followed by verified native game login.

The initial account-only test reached table loading and reported missing
`Localization_MonthlyStoryEnglish.csv`. Restoring the full archived Android
content resolved that error without deleting or replacing localization. The
subsequent lobby build reaches Orvel; see [Android lobby setup](android-lobby.md).
The external downloaded assets remain separate from the APK.

Test evidence is under `target/android-accounts/20261022-auth-test/` and the final
build directory `target/android-accounts/20261022-accounts-local-01/`. The test
database and private fixture credentials stay in ignored build storage. The
test-only ADB mapping is restored to the normal local server after verification.
