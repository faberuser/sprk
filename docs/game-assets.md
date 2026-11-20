# Game assets

Code releases ([client updates](client-updates.md)) manage what this repository builds or
overrides: Managed DLLs, tables, localization, overrides. Everything else in the client
(the Unity player and data files, asset bundles, PlainAsset, LoginPresentation, …, about
19 GB) is an **asset**. Assets are stored once, by hash, in the update service's object
store, and every release lists them. As a result:

- SprkLauncher installs a complete game, code and assets, into an empty folder, as one
  release in one transaction. The game itself downloads nothing on Windows.
- Each release, and therefore each server image, pins its exact asset set.
- A release archive contains only the code files (~570 MB); assets are never copied into
  `releases/`.

## Release manifests

An asset entry in a release manifest has the URL `/updates/assets/objects/<sha256>`
instead of `/updates/releases/<version>/files/<path>`:

```json
{"path": "UnityPlayer.dll", "size": 34078720, "sha256": "<hash>", "url": "/updates/assets/objects/<hash>"}
```

The manifest's `asset_set` field names the pinned set (`assets-<n>`). If a code file and
an asset have the same path, the code file wins. An asset that a newer set drops becomes a
deletion, like any other managed file.

## Storage

```text
<SPRK_UPDATES_DIR>/
  releases/<version>/                     code releases (archive contents only)
  assets/objects/<sha256>                 asset bytes, stored once
  assets/<platform>/versions/<n>/set.json     asset set (path, size, sha256)
  assets/<platform>/versions/<n>/patch.json   game patch list (Android, see below)
  assets/<platform>/versions/<n>/index.json   game archive name -> sha256
  assets/<platform>/current.txt               game's active version
```

| Endpoint                                            | Behavior                                 |
| --------------------------------------------------- | ---------------------------------------- |
| `GET /updates/assets/objects/<sha256>`              | Immutable object, with `Range` and `HEAD` |
| `GET /updates/assets/<platform>/LastBuildVersion.txt` | Game's active version, `no-store`       |
| `GET /updates/assets/<platform>/<n>/patch.json`     | Game patch list, immutable               |
| `GET /updates/assets/<platform>/<n>/<archive name>` | Game archive, immutable                  |

**Startup check.** When the update service installs its pinned release, it checks that
every asset object the release lists is present with the right size. If one is missing,
startup fails, the previous releases stay on disk and nothing is served. Hashes were
checked when publishing and uploading, and the launcher verifies every download.

**Cleanup.** After the release cleanup (active release plus two others), the update
service keeps each platform's active asset version and the two newest others. It then
deletes objects that no release on disk and no kept version uses. Objects changed in the
last 24 hours are never deleted. Cleanup also skips deletion while `.sync-lock` or a
`.publish-lock` exists, or when any release or set cannot be read.

## Publish an asset set

Assets rarely change. When they do, for example a new Unity build or new bundles:

1. **Publish locally:**

   ```powershell
   python client/scripts/release/publish_assets.py --client ../sprk-client
   ```

   This hashes the client folder and leaves out:
   - what every code release builds (`Managed`, `TableData`, `TableJit`,
     `LocalizationJit`, `Video`, `SPRK-Native*.json`, the Unity startup files and the
     privacy file);
   - the launcher's own files;
   - the game's own patch records (`patch.json`, `patch.bjs`, `PatchInfo/`), backup
     copies and partial downloads.

   Files a code release also carries forward, such as overridden bundles, stay in the
   set. The release lists its own copy instead, so whatever a release's history, no file
   drops out of both lists.

   It then stores new objects in `client-updates/assets` and writes the pin
   `client/assets/StandaloneWindows.json`. Only changed files add objects.
2. **Upload** to the host's update storage, before deploying:

   ```powershell
   python client/scripts/release/sync_assets.py --dest <host SPRK_UPDATES_DIR>/assets
   ```

   It copies objects first (hash-checked), then version folders, then `current.txt`,
   under `.sync-lock`, and never overwrites anything. With rsync, keep the same order.
   Files must be writable by UID/GID 10001.
3. **Commit the pin** (`client/assets/StandaloneWindows.json`) and merge as usual. The
   pin is a CI input: the merge builds a release listing the new set, and the server
   image pins that release. Local releases (`client/dev.py install`,
   `publish_client_update.py`) list the pinned set too; `--no-asset-set` leaves it out.

If step 3 is deployed before step 2 finishes, the update service refuses to start, and
the game service is not affected. Finish the upload and restart the update container.

The first launch after a launcher upgrade hashes every installed file once (in
parallel). Later launches re-hash only files whose size or write time changed
(`.sprk-launcher/file-hashes.json`; delete it to force a full check).

## Game downloader (Android)

Platforms without the launcher use the game's own patch downloader. With
`SPRK_ASSETS_CDN_URL` set on the game server (the update service's `/updates/assets/`
URL), `host.json` points Android and iOS at `<SPRK_ASSETS_CDN_URL>/<platform>/`. Windows
always gets the empty `/cdn/` patch.

The client downloads only when its CDN is this asset service (a URL containing
`/updates/assets/`): installations carry the old live game's `PatchInfo/patch.json`,
which the game reuses as the remote list while the versions match, so with the legacy
empty `/cdn/` (Windows) it downloads nothing, as the shipped game did. Publish Android
versions above 1, the version of that carried list. The client reads
`LastBuildVersion.txt`, downloads `<n>/patch.json` when the version changed, and downloads every archive whose MD5 or length differs from its local record,
filtered by quality tier and language. Each Raw archive is written to `<file>.part`,
checked, then moved into place. Publish the Android set (`../sprk-android`, built by
`client/android/dev.py assets`) after rebuilding it:

```powershell
python client/scripts/release/publish_assets.py --client ../sprk-android --platform Android --patch-prefix "" --no-pin --metadata "../sprk-client/King's Raid_Data/Documents/Patch/StandaloneWindows/patch.json"
```

`--metadata` copies the download categories (needed before the lobby, or in the
background) from the Windows client's original list, which names the same bundles.
`--no-pin`: code releases do not list Android bundles; the game's downloader reads
`Android/versions/<n>/patch.json`. Upload with `sync_assets.py` as for Windows, then set
`SPRK_ASSETS_CDN_URL` for the game service (`https://<host>/updates/assets/`).

This path has not been run in the game. Downloads restart a file instead of resuming it,
and `patch.json` is not signed.

## Hosting notes

- Disk: about 19 GB for the first set; later sets add only changed files.
- Bandwidth: a new player downloads about 19 GB, with four parallel downloads. Rate-limit
  the update container or shape traffic so installs do not lag the game.
- Cloudflare: check its terms before serving gigabytes per player through its proxy or a
  tunnel; files over 512 MB are not cached on the free plan. Object storage (for example
  R2) for `objects/` is a later option.
- Backups: the host volume is the only served copy of the objects. Keep the local store.
