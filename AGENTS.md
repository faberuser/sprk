# SPRK development instructions

## Repository layout

One repository holds the server and the client. Run every command from the
repository root.

| Path | Contents |
| --- | --- |
| `server/` | Rust server (`src/`), PostgreSQL `migrations/`, `tests/`, `gm.py`, `scripts/` |
| `client/managed/` | Game (`Assembly-CSharp`) and SPRK feature assemblies (`Sprk*`) as C# source |
| `client/launcher/` | Windows launcher and its tests |
| `client/overrides/files/` | Optional replacement assets/tables, by client-relative path |
| `client/scripts/` | `build/`, `release/`, `automation/`, `launcher/`, `android/` |
| `client/tools/` | Read-only DLL inspection |
| `tables/`, `config/` | Server game tables and shared configuration |
| `docs/`, `.github/`, `Dockerfile*`, `docker-compose.yml` | Documentation, CI and container orchestration |

Generated output goes to `target/` (Cargo, automation runs) or `artifacts/`
(client builds); both are ignored. The installed game is a separate folder,
normally `../sprk-client`. It holds the Unity player and assets, which are
never source: only `Assembly-CSharp.dll` and `Sprk*.dll` are built here.
See [client/README.md](client/README.md) and [the scripts inventory](docs/scripts.md).

## Shell and efficient inspection

- On Windows, prefer Git Bash when available for routine repository navigation,
  `git`, `rg`, and text inspection. Discover Git Bash from the local Git for
  Windows installation and verify the executable; bare `bash` may resolve to
  the Windows/WSL launcher. On Linux/macOS, use the available native shell.
  Start Bash without profiles where possible
  (`--noprofile --norc`) to avoid prompt noise. If the command tool cannot select
  Git Bash directly, invoke that executable from PowerShell; for complex commands,
  pipe a literal here-string to `bash.exe --noprofile --norc -s` to avoid nested
  quoting and unintended PowerShell expansion.
- Use repository-relative paths where possible. Discover client/reference-data
  locations from the current workspace, configuration, or user-provided paths;
  do not assume a drive letter or checkout location. Use `cygpath` when path
  conversion is needed and quote paths containing spaces or apostrophes.
  When invoking native Windows tools, use the path format they expect; Git Bash
  is not a Linux environment. Fall back to PowerShell when path conversion or
  native-tool compatibility makes it simpler.
- Use PowerShell for existing `.ps1` workflows, Windows services/process APIs,
  and Windows-specific administration. For deletion or moving files on Windows,
  use native PowerShell cmdlets with `-LiteralPath`; verify resolved targets
  before recursive operations. Keep each file operation within one shell.
- Reduce tool output: search scoped paths with `rg`, list candidate files before
  reading them, and return relevant line ranges or matches. `client/managed`
  has over 12,000 files; never scan or dump it broadly, and avoid recursive
  scans of client assets.
- Game code is source: read and search `client/managed` directly. Inspect a
  compiled or third-party DLL only when needed, with `client/tools/ManagedSource`
  ([usage](client/tools/README.md)); do not dump raw DLL bytes or whole
  assemblies into chat.

## Database performance and diagnostics

When given a game log, a database dump, a diagnostics report or a slow, failing
or locking server, follow [the database guide](docs/database-diagnostics.md):
handle the files as player data, load dumps only into a scratch container, run
`server/scripts/database/db_diagnostics.py`, and fix the cause in server code with a
regression test and before/after numbers.

## Changing the client

- Change game behavior by editing C# under `client/managed`. A new feature
  assembly is a new `client/managed/SprkName/SprkName.csproj` directory; the
  build and release include it automatically.
- `client/managed` is maintained source. Never regenerate it by decompiling a
  DLL, and never patch a compiled DLL.
- Unity serializes components by field declaration order and name. In classes
  derived from `MonoBehaviour`/`ScriptableObject` and in `[Serializable]` types,
  do not reorder, rename, retype or remove existing serialized fields; append
  new ones at the end or mark them `[NonSerialized]`.
- Keep assembly names, namespaces and type names that game assets, reflection
  or the server's battle worker refer to.
- Compile with `python client/scripts/build/build_managed_client.py`. It must
  finish without errors before anything is installed or committed. The server
  test `client_parallel_reads_are_server_observations` reads
  `client/managed/SprkNetworkPerformance/SprkNetworkPerformance.cs`; keep its
  `parallelReads` list a plain initializer.
- Put a replacement table or asset, in its final form, under
  `client/overrides/files/` using its client-relative path, with matching
  `SPRK-Native*.json` integrity metadata when the changed data requires it.
  Changes made only in `../sprk-client` or in ignored folders never reach CI.
- `client/scripts/release/client_baseline.py` pins the published release that
  supplies assets and runtime DLLs. Routine changes never touch it; use
  `pin_client_baseline.py` only when deliberately adopting a newer release.
- Test source-built DLLs in a sandbox before installing them into the real
  game: `python client/scripts/build/sandbox_client.py create` makes
  `../sprk-client-test` (player files copied, the 19 GB `Documents` folder
  shared through a junction, DLLs built from source). Rebuild into it with
  `build_managed_client.py --install ../sprk-client-test`. Start it with its
  `Play (local server).bat`, never with SprkLauncher, which would restore the
  published DLLs. Delete it only with `sandbox_client.py remove`: a plain
  recursive delete can follow the junction and destroy the real assets.
- Verify behavior on the real client with
  [client automation](docs/client-automation.md) where a change is user-visible
  (`--client ../sprk-client-test` for the sandbox): it runs an isolated server
  and database. Never point a test client at the public server; set
  `SPRK_HOST_URL` to the local `host.json` for manual runs.

## CI and deployment

Follow [GitHub client releases](docs/github-client-updates.md) for CI inputs,
isolated validation, signing and deployment.

- The normal deployment path is commit to `development`, PR, then merge to
  `main`. GitHub-hosted runners compile the client source, sign and publish the
  client update, push the matching server image to GHCR, and trigger Portainer.
  No routine SFTP upload or SSH deployment step is needed. A local signed release
  alone does not publish a GitHub release or deploy the host.
- For source-only work or CI/build-tool changes that do not install client
  files, validate with
  `python client/scripts/release/build_client_release.py --work <new folder> --version <unique id> --check-only`.
  It builds the complete release with a throwaway key and changes neither the
  installed client nor the local stable manifest. Report that no
  installed-client release was generated.
- The updates container downloads only its image-pinned client release at
  startup, verifies it before activation, and keeps the active release plus two
  previous releases. Preserve startup-only pinning so older server code cannot
  automatically serve a newer incompatible client. Its `/client-updates` mount
  must be writable by container UID/GID `10001`.

## Required client update after client changes

When a task changes the installed game client (compiled DLLs, TableData,
TableJit, LocalizationJit or other launcher-managed assets), **generate a new
signed client update before reporting the task complete**. Editing source alone
does not update the installed client. Build and install first, then publish the
actual resulting client files. The user's request to implement a client change
includes authorization to generate its local update artifacts; do not ask for
separate permission to run the local publisher.

Use the workflow described in [docs/client-updates.md](docs/client-updates.md):

1. The installed client is normally `../sprk-client`; confirm it exists.
2. Build and install with
   `python client/scripts/build/build_managed_client.py --install ../sprk-client`,
   then validate the installed client. The user authorizes closing the local
   server and King's Raid client when installation or validation requires it;
   no separate confirmation is needed. Never publish the unchanged client as if
   the change were installed. If the user requests source-only work, respect
   that scope and explicitly report that no installed-client release was generated.
3. Choose a **new, unique** release identifier such as
   `local-YYYYMMDD-description`, using the user's local date. Add a time or numeric
   suffix when necessary. Inspect `client-updates/releases` and the current
   `client-updates/stable/manifest.json`; never overwrite an existing release.
4. Publish after all intended client changes are installed and relevant checks
   pass. Use the existing signing key:

    ```powershell
    python client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --channel stable --version <new-release-id> --signing-key client-updates/update-signing/private.pem
    ```

    The publisher creates `client-updates/releases/<new-release-id>/` and updates
    `client-updates/stable/manifest.json` atomically.

5. Use the default selection. It keeps every file the previous release managed
   that still exists in the client, so nothing is dropped by accident. Repeated
   `--include` arguments replace the defaults and publish only that scope, which
   marks every other managed file for deletion. The publisher snapshots the
   installed client: compare the new manifest with the previous one
   (added, removed, changed, `deleted_files`) and confirm every difference
   belongs to this task.
6. Verify that the stable and release manifests match, the signature verifies
   with the tracked `config/client-update-public.pem`, and each release file matches
   its manifest size and SHA-256 hash. Confirm the changed installed files appear
   in the release with their current hashes. Do not edit signed manifests by hand.
7. In the final response report the generated release ID, checks performed, and
   any restart needed. Distinguish the local installed-client release from a
   published GitHub release and a completed Portainer deployment. Public rollout
   uses the tracked inputs and CI path above. If the user explicitly
   requests manual hosting, identify the release folder and updated
   `stable/manifest.json` to upload; do not claim a deployment from local
   artifact creation alone.

Never print or commit private signing-key contents, regenerate/rotate existing
keys to bypass a problem, or silently publish unsigned updates. If required files,
the signing key, installation or publishing are blocked, report the specific
blocker and what remains unfinished. Keep existing releases available and do not
delete unrelated work or artifacts.

Server-only or documentation-only changes do not require a client release unless
they also change installed client files. Batch related changes in one task into
one final verified release unless the user requests separate releases.
