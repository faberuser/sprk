# SPRK Launcher

Windows desktop launcher for the existing King's Raid client. All source stays
in `sprk-server`; the published executable goes beside `King's Raid.exe`.

On opening the launcher, it checks the current manifest, hashes only the files
managed by that manifest, downloads changed/missing files, installs the complete
release, and starts the game. It does not scan/download the entire 20 GB client.
It shows progress and offers Retry after an error. The game must be closed while
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

The build script copies the complete icon group from the client's `King's Raid.exe`
into `launcher/Assets/KingsRaid.ico` and embeds it in the launcher. The launcher
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

`AutoLaunch=false` keeps the window open after checking. Clicking Play checks
again under the installation lock, then starts the game. The configured game
path is relative to the launcher directory; that directory is also the game's
working directory, regardless of where a shortcut is launched from.

## Signed releases for your homelab

Generate keys once (OpenSSL is needed on the publishing machine):

```powershell
python scripts/create_update_signing_key.py
```

Keep the private key on your publishing machine and back it up. The generated
`update-signing` directory and PEM files are ignored by Git and excluded from
Docker images. Do not copy the private key into the game client or public update
directory. Keep the same key for future releases so existing launchers trust them.

Build/configure the launcher using your public HTTPS endpoint and public key:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/build_launcher.ps1 -ManifestUrl "https://raid.example.com/updates/stable/manifest.json" -PublicKeyFile update-signing/public.pem
```

Publish each tested client release with that private key:

```powershell
python scripts/publish_client_update.py --client ../sprk-client --output client-updates --version 1.0.0 --signing-key update-signing/private.pem
```

Upload the complete release directory before atomically replacing the channel
manifest. The launcher checks the RSA-SHA256 signature before deciding which
files to download or remove. It refuses unsigned or incorrectly signed releases
when a public key is configured. Non-local update URLs require HTTPS and a public
key; HTTP/unsigned development is limited to localhost. Redirects are disabled,
and file URLs must remain on the configured update server's origin.

See [update hosting](../docs/client-updates.md) for `SPRK_SERVICE_MODE`, release
selection, the server endpoints, and Portainer configuration.

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
