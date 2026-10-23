# SPRK development instructions

## Reproducible client changes and CI deployment

Follow [GitHub client releases](docs/github-client-updates.md) for CI inputs,
isolated validation, signing and deployment.

- Keep client changes reproducible from tracked inputs. Implement DLL patches
  and embedded resources under `scripts/DllPatcher`; new patches and their
  verifiers must run through its full build path (`--build-release`, including
  `BuildRelease.cs`). Existing patch fixes use their existing calls. Track any
  additional transformation tools and invoke them from the release build.
- Put replacement tables/assets under `client-overrides/files/` using
  client-relative paths. Include matching `SPRK-Native*.json` integrity metadata
  whenever the changed data requires it. Changes only in `../sprk-client` or
  ignored local release folders do not become CI inputs automatically.
- Routine patches do not require a baseline change, a manual CI release version,
  or `client-build.json`. `scripts/release/client_baseline.py` pins the fixed distributed
  snapshot; use the baseline preparation tool only when deliberately replacing
  that snapshot.
- Preserve complete telemetry cleanup. The Python release builder and
  `scripts/client/patch.ps1` run `scripts/client/remove_client_telemetry.py` before full DLL
  patching, while original type definitions are available for serialized asset
  cleanup. Keep its dependencies in `scripts/client/requirements-client.txt`. Managed
  DLL stripping alone does not replace the serialized asset and SDK cleanup.
- For explicitly source-only work or CI/build-tool changes that do not install
  client assets, validate relevant checks in a fresh isolated workspace. For a
  complete signed build use `scripts/release/build_client_release.py` without
  `--publish`, with a unique work path/version and the existing signing key.
  Do not change the installed client or normal local stable manifest for that
  validation. Report that no installed-client release was generated. This does
  not waive the local release requirement below when installed files change.
- The normal deployment path is commit to `development`, PR, then merge to
  `main`. GitHub-hosted runners build, verify, sign and publish the client update,
  push the matching server image to GHCR, and trigger Portainer. No routine
  SFTP upload or SSH deployment step is needed. A local signed release alone
  does not publish a GitHub release or deploy the host.
- The updates container downloads only its image-pinned client release at
  startup, verifies it before activation, and keeps the active release plus two
  previous releases. Preserve startup-only pinning so older server code cannot
  automatically serve a newer incompatible client. Its `/client-updates` mount
  must be writable by container UID/GID `10001`.

## Required client update after client changes

When a task changes installed game client code, DLLs, TableData, TableJit,
LocalizationJit or other launcher-managed assets, **generate a new signed client
update before reporting the task complete**. Updating patcher source alone does
not update the installed client. Apply and verify the intended patch first, then
publish the actual resulting client files. The user's request to implement a
client change includes authorization to generate its local update artifacts;
do not ask for separate permission to run the local publisher.

Use the existing workflow described in [docs/client-updates.md](docs/client-updates.md):

1. Run commands from the `sprk-server` repository root. The installed client is
   normally `../sprk-client`; confirm it exists before patching.
2. Preserve existing client patches. Use the appropriate focused patch mode and
   validate the final installed files. If King’s Raid is running and prevents
   installation, prepare/stage the fix, ask the user to close it, and continue
   after closure. Never publish the unchanged client as if the staged fix were
   installed. If the user requests source-only or stage-only work, respect that
   scope and explicitly report that no installed-client release was generated.
3. Choose a **new, unique** release identifier such as
   `local-YYYYMMDD-description`, using the user's local date. Add a time or numeric
   suffix when necessary. Inspect `client-updates/releases` and the current
   `client-updates/stable/manifest.json`; never overwrite an existing release.
4. Publish after all intended client changes are installed and relevant checks
   pass. Use the existing signing key:

   ```powershell
   python scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --channel stable --version <new-release-id> --signing-key client-updates/update-signing/private.pem
   ```

   Replace `<new-release-id>` with the chosen identifier. The publisher creates
   `client-updates/releases/<new-release-id>/` and updates
   `client-updates/stable/manifest.json` atomically.
5. Preserve the managed file selection across releases. Normally use the default
   includes. If modifying additional assets, inspect the publisher's
   `DEFAULT_INCLUDES` and the previous manifest, then supply **all** required
   includes plus the extra assets. Repeated `--include` arguments replace defaults;
   publishing only the changed DLL would mark other managed files for deletion.
   Review `deleted_files` and ensure deletions are intentional. A complete managed
   snapshot is expected; the launcher downloads only changed or missing files.
6. Verify that the stable and release manifests match, the signature verifies
   with the tracked `config/client-update-public.pem`, and each release file matches
   its manifest size and SHA-256 hash. Confirm the changed installed files appear
   in the release with their current hashes. Do not edit signed manifests by hand.
7. In the final response report the generated release ID, checks performed, and
   any restart needed. Distinguish the local installed-client release from a
   published GitHub release and a completed Portainer deployment. Public rollout
   normally uses the tracked inputs and CI path above. If the user explicitly
   requests manual hosting, identify the release folder and updated
   `stable/manifest.json` to upload; do not claim a deployment from local
   artifact creation alone.

Never print or commit private signing-key contents, regenerate/rotate existing
keys to bypass a problem, or silently publish unsigned updates. If required files,
the signing key, installation or publishing are blocked, report the specific
blocker and what remains unfinished. Keep existing releases available and do not
delete unrelated work or artifacts.

Server-only or documentation-only changes do not require a client release unless
they also change installed client files. Batch related patches in one task into
one final verified release unless the user requests separate releases.
