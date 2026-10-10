# SPRK Launcher

Windows desktop launcher for the existing King's Raid client. All source stays
in `sprk-server`; the published executable goes beside `King's Raid.exe`.

On opening the launcher, choose **Local** or **Public**, then click **Update and
Play**. It checks that profile's manifest, hashes only the managed files,
downloads changed/missing files, installs the release, and starts the game.
It does not scan/download the entire 20 GB client.
It shows progress and offers Retry after an error. If a game window is already
running on the same profile, clicking Play starts another window using the installed
files. Close all game windows before switching profiles. Updates are deferred
until all game windows are closed. Enable Unity multiple-window
support with `python scripts/enable_multiple_windows.py ../sprk-client --install`.
The game must be closed while
updating. A failed update never automatically starts an unchecked client.

## Build and install

Development requires Windows, .NET SDK 10, and Python 3.10+. Players do not need to install
.NET: the build script publishes a compressed, self-contained Windows x64 EXE.

Run from the server repository:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build_launcher.ps1
```

The default destination is `../sprk-client`. Override it with `-ClientPath`.
`sprk-launcher.json` is created alongside the EXE; a normal rebuild preserves an
existing configuration. Explicit `-ManifestUrl`/`-PublicKeyFile` options write
a new configuration. Ship both files with the initial client, and make the
player's desktop shortcut point to `SprkLauncher.exe`.

The build script uses the checked-in `launcher/Assets/KingsRaid.ico`, or refreshes
it from the client's EXE when `scripts/extract_exe_icon.py` is available. The launcher
window uses that icon too. Configurations produced by older builds with a
PowerShell object around `ManifestPublicKeyPem` are repaired automatically on launch.

Local unsigned development configuration:

```json
{
  "ManifestUrl": "http://127.0.0.1:8081/updates/stable/manifest.json",
  "GameExecutable": "King's Raid.exe",
  "ManifestPublicKeyPem": "",
  "AutoLaunch": true
}
```

The profile selector always waits for **Update and Play**, so you can choose a
server before any update starts. This explicit button checks under the installation
lock and starts the game even if a legacy config has `AutoLaunch=false`. The configured game
path is relative to the launcher directory; that directory is also the game's
working directory, regardless of where a shortcut is launched from.

## Signed releases for your homelab

Generate keys once (OpenSSL is needed on the publishing machine):

```powershell
python scripts/create_update_signing_key.py
```

Keep the private key on your publishing machine and back it up. The generated
`client-updates/update-signing` directory and PEM files are ignored by Git and excluded from
Docker images. Keep this subfolder on the publishing machine; upload only
release directories and channel manifests to the public server. The SPRK update
endpoint does not serve files from this signing subfolder. Keep the same key for future releases so existing launchers trust them.

Build/configure the launcher using your public HTTPS endpoint and public key:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build_launcher.ps1 -ManifestUrl "https://raid.example.com/updates/stable/manifest.json" -PublicKeyFile client-updates/update-signing/public.pem
```

Publish each tested client release with that private key:

```powershell
python scripts/publish_client_update.py --client ../sprk-client --output client-updates --version 1.0.0 --signing-key client-updates/update-signing/private.pem
```

Upload the complete release directory before atomically replacing the channel
manifest. The launcher checks the RSA-SHA256 signature before deciding which
files to download or remove. It refuses unsigned or incorrectly signed releases
when a public key is configured. Non-local update URLs require HTTPS and a public
key; HTTP/unsigned development is limited to localhost. Redirects are disabled,
and file URLs must remain on the configured update server's origin.

See [update hosting](../docs/client-updates.md) for `SPRK_SERVICE_MODE`, release
selection, the server endpoints, and Portainer configuration.

## Local and Public profiles

The launcher remembers the profile used when you click **Update and Play**.
Its default profiles are:

| Profile | Game bootstrap | Update manifest |
| --- | --- | --- |
| Local | `http://127.0.0.1:8080/host.json` | `http://127.0.0.1:8081/updates/stable/manifest.json` |
| Public | `https://play.krinfo.net/host.json` | `https://updates.krinfo.net/updates/stable/manifest.json` |

`sprk-launcher.json` stores `SelectedProfile` and a `Profiles` array. Each profile
has `Id`, `Name`, `HostUrl`, and `ManifestUrl`. The top-level
`ManifestPublicKeyPem` is shared unless a profile defines its own
`ManifestPublicKeyPem`. Existing standard localhost/public configurations acquire
the default profiles automatically. Custom legacy update URLs are preserved in
an `Existing server` profile; set its `HostUrl` before playing. Headless updates
continue to work without a game URL.

The launcher passes `SPRK_HOST_URL` to the game process. Install the accompanying
client patch once, with all game windows closed:

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --accounts-only
```

The patch changes the final bootstrap request after the native debug override,
without rewriting shared assets or global environment variables. Starting the
game EXE directly retains its original bootstrap behavior. Saved accounts are
encrypted as before and stored separately by server origin under `SprkServers`
inside the game's persistent data directory. Existing accounts are imported
only into the matching origin's store, leaving the legacy store intact.

Publish this patched client to both update endpoints before using either
profile. The launcher refuses to launch a release without the profile hook,
preventing an older update from silently sending a Public launch to localhost.
Distribute the new launcher EXE separately to existing players.

Both profiles share installed game files. Switching checks hashes against the
selected server, even if both manifests have the same version string. Different
builds may require downloads each time. Close all game windows before switching;
additional windows on the same profile can use the already installed files.

For automation, use `SprkLauncher.exe --update-only --profile local` or
`--profile public`. Headless mode updates without launching the game or changing
the remembered UI selection.

## Interrupted updates

The launcher holds an exclusive installation lock, stages every download before
modifying game files, checks size and SHA-256, and keeps partial downloads under
`.sprk-launcher/downloads` for resume on retry. A transaction journal and backups
under `.sprk-launcher` let the next launch restore an interrupted installation
before checking for new updates. Installation is not cancelled halfway through
a commit; closing waits for it to finish safely. Committed updates survive cleanup
interruptions. The updater checks that the game is closed before downloading and
again before installing.

Paths cannot escape the client directory, use Windows device names, pass through
symlinks/junctions, or replace the launcher/configuration. Published manifests
cannot contain duplicate paths, conflicting deletions, or file/directory clashes.
The initial client must be in a directory the player can write to. Errors appear
in the window and `.sprk-launcher/launcher.log`.

The launcher itself is distributed separately; updating its running EXE would
require a separate replacement helper. This launcher updates the game binaries,
table data, localization, and any other files selected by the publisher.

## Validation

```powershell
dotnet run --project launcher/SprkLauncher.Tests -c Release
python -m unittest discover -s scripts -p test_publish_client_update.py
python scripts/test_launcher_e2e.py
```

The last command requires the packaged launcher, a compiled debug
`sprk-server.exe`, and generated signing keys. It runs only in temporary client
directories, uses the real server's update-only endpoint, verifies OpenSSL/.NET
signature compatibility, resumes a JIT download, installs/removes files, rejects
a forged signature, and confirms unchanged files need no download. It never
starts the game. `SprkLauncher.exe --update-only` is a headless update mode for
these checks; it returns a nonzero exit code and writes `sprk-launcher-error.log`
on failure.
