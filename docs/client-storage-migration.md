# Client storage isolation and migration retirement

SPRK permanently uses company `sprk`, product `King's Raid`. Unity's Windows
persistent data directory is `%USERPROFILE%\AppData\LocalLow\sprk\King's Raid`;
PlayerPrefs are under `HKCU\Software\sprk\King's Raid`. The official client's
`masangsoft` directory and registry key are read-only migration sources.

## Release record

- First local signed release: `local-20261025-client-storage-isolation`.
- Public deployment date/version: **fill in when CI actually deploys this change**.
- Storage/report schema: 1.
- Legacy migration currently enabled: `SprkStorageMigration.LegacyMigrationEnabled`.
- Local artifact creation is not evidence of public rollout or player migration.

## What is copied, and when

The first normal game launch copies only `sprk-accounts.dat` and the stores under
`SprkServers/<64-hex server key>/sprk-accounts.dat`, plus existing preferences with
prefixes `Sprk.Graphics.`, `Sprk.Window.`, `KingsRaid_SystemOption_`, and
`KingsRaid_GameOption_`. Account files retain their original DPAPI encryption and
remain usable by the same Windows user. Existing destination files/values win.
Session locks, browser caches, logs, test profiles, other files and login-related
registry values are not copied. Sources are never deleted, renamed or updated.

Normal completion writes both:

- `sprk-storage-migration-v1.complete` in the new persistent data directory;
- `Sprk.Storage.MigrationV1Receipt` in the new registry key.

Each receipt contains a random profile installation ID, schema, UTC completion
time and counts only. A receipt is checked before opening any old source.
Subsequent changes made by the official client are not imported. The registry
receipt prevents reimport if the new folder/marker is removed; it recreates the
file receipt without reading legacy data. A fresh install with no legacy data
also gets a completion receipt, so a later official-client install is ignored.
Deleting **both** receipts deliberately removes this protection; never advise
players to do that as a generic troubleshooting step.

Migration is serialized with a local file lock. Failed copies do not receive a
completion receipt; a subsequent attempt resumes without overwriting existing
new data. Corrupt receipts fail closed and do not trigger another import.
Automation profiles and battle workers do not migrate or report real profiles.

## Checking adoption

After successful migration/fresh initialization, the game reports schema 1 once
per authenticated session to `/client/storage_status`, retrying failed reports
without blocking login. The server writes `client_storage_reports`. It receives
an opaque random installation ID, schema, source-present flag and copy counts;
it does not receive Windows paths/usernames, hardware IDs, credentials or file
contents. Reports are operational self-reports, not an anti-cheat or update gate.

Deploy the server addition before distributing the client update. An older
server can ignore reports; the migration remains local and works without it.
Run against the hosted database or a recent consistent read-only backup:

```powershell
python scripts/client/storage_migration_status.py --database sprk.db --active-days 30
python scripts/client/storage_migration_status.py --database sprk.db --active-days 90 --missing --json
```

The script opens SQLite read-only and never creates a missing database. Missing
entries list account IDs and last-login times, not login names or credentials.
Use the hosted database: a local development database says nothing about hosted
players. Zero missing accounts means every account active in that time window
has reported at least one successful profile, **not** that every offline player,
computer or Windows profile has migrated. A missing report can mean an old
client, an offline profile, failed migration, or a reporting/network failure.

Equivalent SQL for operators who cannot run the Python tool:

```sql
SELECT a.account_id, a.last_login
FROM accounts a
WHERE a.last_login >= datetime('now','-90 days')
  AND NOT EXISTS (
    SELECT 1 FROM client_storage_reports r
    WHERE r.account_id=a.account_id AND r.schema_version>=1
  )
ORDER BY a.last_login DESC;
```

## When retirement is appropriate

1. Record the real public release date/version above. Confirm reports arrive
   from the hosted server after deployment; a successful local test is insufficient.
2. Check both 30-day and 90-day activity windows. Investigate remaining active
   accounts and migration failures. Retain a dated read-only report as evidence.
3. Allow at least 90 days after public deployment as a starting grace period,
   longer if players commonly return after extended absences. This is an operator
   policy, not proof that offline users have migrated. Confirm the current client
   is the distributed version and check known multi-PC users separately.
4. Explicitly decide how returning, unobserved profiles will recover saved login
   lists and preferences. Their server-side characters/items are unaffected;
   they can sign in again, but automatic preservation ends when migration is
   retired. Keep an archived migration-capable build or support procedure.
5. Obtain the operator's go-ahead to retire legacy import, recording the decision,
   date and evidence here. Do not retire automatically based on a percentage or
   elapsed time alone.

## Exact cleanup path for the next agent

Read `AGENTS.md` and `sprk-server/AGENTS.md` first. Keep unrelated work intact.

1. Set `LegacyMigrationEnabled=false` in
   `scripts/DllPatcher/Resources/StorageMigration.cs.txt` for a reviewed retirement
   release. This disables source reads/copying while retaining permanent storage
   isolation, completion receipts and reporting. Adjust tests to cover this mode.
2. After that release is verified, remove the dead legacy-copy branch,
   `CopyAccounts`, legacy preference enumeration/copy methods and the old source
   path. Keep the public `Ensure`/`StartReporter` facade or update every caller in
   `Automation.cs.txt`; do not remove the helper blindly.
3. **Keep** company conversion in `scripts/client/isolate_client_storage.py`,
   its `ClientStorage.cs` installer/full-build verifier, and both native timing
   fingerprints in `BattleGraphics.cs`. CI still starts from the pinned baseline
   with the original company. Removing conversion would reintroduce interference.
4. Keep existing completion receipts. Never delete or move players' `masangsoft`
   data or registry keys: the official client may own them by then. Do not copy
   them again to clean up. Account-store `.sessions` locks are live resources.
5. Reporting can be retained for rollout history. If removing it later, remove
   the reporter component, server route/module/migration hook and operator tool
   together. Do not drop the audit table or user data without separate approval.
6. Run storage unit checks, native two-launch probe and a fresh full client build.
   Install and verify the focused patch, then create and verify a **new** signed
   update with the existing key and the complete previous managed-file selection.
   Use the normal development/PR/main CI workflow for public deployment.
7. Update this document with the retirement release, tests, remaining compatibility
   path and whether public deployment actually completed.

## Reproduction and validation

```powershell
dotnet run --project scripts/DllPatcher -- ../sprk-client --self-test-client-storage
dotnet run --project scripts/DllPatcher -- ../sprk-client --client-storage-only --stage-only
python scripts/client/verify_client_storage.py
cargo test storage_migration -- --test-threads=1
```

The Windows native probe uses a private GUID-scoped source registry key and dummy
account file. It proves Unity uses `sprk`, restores native PlayerPrefs before use,
and ignores later source edits across a restart. It never imports the real old
profile; temporary probe registry values are removed after both launches.
The pure migration tests cover source preservation, destination precedence,
allowlisted preferences, empty-source first launch, marker/tombstone recovery,
interrupted copies, corrupt receipts and concurrent starts. Full `--build-release`
installs and verifies this patch and runs these checks' portable core tests.
