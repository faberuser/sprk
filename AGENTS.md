# SPRK development instructions

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
   python scripts/publish_client_update.py --client ../sprk-client --output client-updates --channel stable --version <new-release-id> --signing-key client-updates/update-signing/private.pem
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
   with `client-updates/update-signing/public.pem`, and each release file matches
   its manifest size and SHA-256 hash. Confirm the changed installed files appear
   in the release with their current hashes. Do not edit signed manifests by hand.
7. In the final response report the generated release ID, checks performed, and
   any restart needed. For public players, identify the new release folder and
   updated `stable/manifest.json` to upload; creating local artifacts does not
   mean the homelab update host has been deployed.

Never print or commit private signing-key contents, regenerate/rotate existing
keys to bypass a problem, or silently publish unsigned updates. If required files,
the signing key, installation or publishing are blocked, report the specific
blocker and what remains unfinished. Keep existing releases available and do not
delete unrelated work or artifacts.

Server-only or documentation-only changes do not require a client release unless
they also change installed client files. Batch related patches in one task into
one final verified release unless the user requests separate releases.
