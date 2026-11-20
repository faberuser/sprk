# Game assets

Players get the game in two parts, the same way on Windows and Android:

- **Code** (Managed DLLs, tables, localization, video, `client/overrides`) comes from
  releases ([client updates](client-updates.md)). SprkLauncher installs it on Windows,
  the APK's bootstrap on Android ([android.md](android.md)).
- **Assets** (asset bundles, PlainAsset, LoginPresentation, …) come from the game's own
  patch downloader (`NGameAsset.NPatch.PatchDownloader`) at login, from the update
  service. It downloads only the player's quality tier and language, and fetches most
  content in the background after the lobby.

The Unity player and its data files (`King's Raid.exe`, `level*`, `sharedassets*`, …)
are neither: a new Windows player starts from the client ZIP, an Android player from
the APK. The ZIP does not need the asset bundles; the game downloads them.

## How the game downloads

With `SPRK_ASSETS_CDN_URL` set on the game server (the update service's public
`/updates/assets/` URL), `host.json` gives each platform `<SPRK_ASSETS_CDN_URL>/<platform>/`
as its patch CDN. Without it, every platform gets the empty `/cdn/` patch and nothing is
downloaded, as in the shipped game.

1. The client reads `LastBuildVersion.txt`. When it differs from the version it has,
   it downloads `<n>/patch.json`, the list of every asset with its length, MD5, quality
   tier, language and download category.
2. It downloads each listed file whose MD5 or length differs from its local record
   (`patch.bjs`): category 0 before the lobby, the rest in the background. A file whose
   record differs but whose installed copy already has the listed length and MD5 is
   recorded instead: installations carry a record from the old live game that does not
   match files installed since (an existing Windows install would otherwise download
   about 3.8 GiB of files it already has).
3. Each file is written to `<file>.part`, checked against length and MD5, then moved
   into place and recorded. A failed or corrupt download is retried and never replaces
   a file.

The client downloads only from this service (a CDN URL containing `/updates/assets/`).
Installations carry the old live game's `PatchInfo/patch.json`, which the game reuses
as the remote list while the versions match; with the empty `/cdn/` it would otherwise
try to download archives no server has. Publish versions above 1, that list's version.
The client picks its `host.json` entry by the platform it runs on (`Android`, `iOS`,
`StandaloneWindows`).

## Storage and endpoints

```text
<SPRK_UPDATES_DIR>/assets/
  objects/<sha256>                       file bytes, stored once across versions and platforms
  <platform>/current.txt                 active version
  <platform>/versions/<n>/patch.json     the game's patch list (one Raw archive per file)
  <platform>/versions/<n>/index.json     archive name -> sha256
  <platform>/versions/<n>/set.json       path, size, sha256 (which objects the version keeps)
```

| Endpoint                                              | Behavior                                |
| ----------------------------------------------------- | --------------------------------------- |
| `GET /updates/assets/<platform>/LastBuildVersion.txt` | Active version, `no-store`              |
| `GET /updates/assets/<platform>/<n>/patch.json`       | Patch list, immutable                   |
| `GET /updates/assets/<platform>/<n>/<archive name>`   | File, immutable, with `Range` and `HEAD` |

Versions and objects are never modified; a new version adds only the files that
changed. Rolling back means writing an older number into `current.txt`.

**Cleanup.** When the update service starts, it keeps each platform's active version
and the two newest others, then deletes objects that no kept version uses. Objects
changed in the last 24 hours are never deleted, and cleanup is skipped while
`.sync-lock` or a `.publish-lock` exists or when any version cannot be read.

## Publish

Windows assets come from the installed client, Android assets from `../sprk-android`
(built by `client/android/dev.py assets`, which converts changed Windows bundles):

```powershell
python client/scripts/release/publish_assets.py --client ../sprk-client
python client/scripts/release/publish_assets.py --client ../sprk-android --platform Android --patch-prefix "" --metadata "../sprk-client/King's Raid_Data/Documents/Patch/StandaloneWindows/patch.json"
```

Each publishes the next version of the patch folder into `client-updates/assets`. It
leaves out what releases install (`TableData`, `TableJit`, `LocalizationJit`, `Video`,
`SPRK-Native*.json`) and the game's own patch records (`patch.json`, `patch.bjs`,
`PatchInfo/`). Files a release also carries, such as an overridden bundle, stay in the
list with the same content: the bundle loader only loads files its list names.
Download categories come from the original patch list (`--metadata` for Android, whose
bundles have the same names). Only changed files add objects.

Upload to the host's update storage:

```powershell
python client/scripts/release/sync_assets.py --dest <host SPRK_UPDATES_DIR>/assets
```

It copies objects first (hash-checked), then version folders, then `current.txt`, under
`.sync-lock`, and never overwrites anything. With rsync or FileZilla, keep that order.
Files must be writable by UID/GID 10001. Players get the new version at their next
login; no deployment is needed.

## Hosting notes

- Disk: about 19 GB for Windows plus 11 GB for Android; later versions add only
  changed files.
- Bandwidth: a new player downloads their tier (about 4.5–6.6 GB on Android). Rate-limit
  the update container or shape traffic so downloads do not lag the game.
- Cloudflare: check its terms before serving gigabytes per player through its proxy or
  a tunnel; files over 512 MB are not cached on the free plan.
- Backups: the host volume is the only served copy of the objects. Keep the local store.
