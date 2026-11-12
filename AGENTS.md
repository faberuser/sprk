# SPRK agent instructions

Read this before changing anything. The [README](README.md),
[server/README.md](server/README.md) and [client/README.md](client/README.md)
explain the project for people; this file adds the rules agents must follow.

## Orientation

One repository holds the server and the client. Run every command from the
repository root.

| Path | Contents |
| --- | --- |
| `server/` | Rust server (`src/`), PostgreSQL `migrations/`, `tests/`, `gm.py`, `scripts/` |
| `client/managed/` | The game's C# code (`Assembly-CSharp`) and SPRK feature assemblies (`Sprk*`) |
| `client/launcher/` | Windows launcher and its tests |
| `client/overrides/files/` | Replacement assets/tables, by client-relative path |
| `client/scripts/` | `build/`, `release/`, `automation/`, `launcher/`, `android/` |
| `client/tools/` | Read-only DLL inspection |
| `tables/`, `config/` | Server game tables; client-update public key and shared configuration |
| `docs/`, `.github/`, `Dockerfile*`, `docker-compose.yml` | Guides, CI and containers ([scripts inventory](docs/scripts.md)) |

Folders outside the repository:

| Path | What it is | Rule |
| --- | --- | --- |
| `../sprk-client` | The user's installed game (Unity player + 19 GB of assets) | Change only when a task installs client changes; then publish a release (below) |
| `../sprk-client-test` | Test copy made by `client/dev.py test` | Use for testing; delete only with `client/dev.py clean` |
| `client-updates/` | Local update releases and the signing key | Never edit releases or manifests by hand |
| `target/`, `artifacts/` | Generated builds and test runs (ignored) | Safe to regenerate |

Only `Assembly-CSharp.dll`, `Sprk*.dll` and `SprkLauncher.exe` are built from
this repository. The Unity player, assets and tables are never source.

## Ground rules

- `client/managed` is the source of truth. Change behavior there; never patch a
  compiled DLL.
- Never point a test client at the public server. Use the local server, the
  test copy started by `client/dev.py test`, client automation, or set
  `SPRK_HOST_URL=http://127.0.0.1:8080/host.json`.
- Never print or commit private keys, `.env` secrets, player databases or
  game assets. Never rotate keys to bypass a problem.
- Never edit an applied file in `server/migrations/`; add a new migration.
- Pushes to `main` deploy to the public server. Never push or merge to `main`
  unless the user explicitly asks for that step.
- Keep existing releases, backups and unrelated work. Report blockers plainly
  instead of working around a deliberate restriction.

## Shell and inspection

- On Windows, prefer Git Bash for navigation, `git`, `rg` and text inspection.
  Discover it from the Git for Windows installation; bare `bash` may be the WSL
  launcher. Start it with `--noprofile --norc`. If the command tool cannot select
  Git Bash, pipe a literal here-string to `bash.exe --noprofile --norc -s` from
  PowerShell. On Linux/macOS, use the native shell.
- Use repository-relative paths. Discover the game and other folders from the
  workspace or configuration; do not assume a drive letter. Quote paths: game
  paths contain spaces and an apostrophe (`King's Raid_Data`). Use `cygpath` when
  native Windows tools need Windows paths.
- Use PowerShell for `.ps1` workflows, Windows process/service APIs, and for
  deleting or moving files (`-LiteralPath`, after checking the resolved target).
  Never delete recursively through a junction: the sandbox links to the real
  game's assets.
- Keep output small. `client/managed` has over 12,000 files: search it with
  scoped `rg`, read line ranges, and never dump it or the game's asset folders.
- Read and search `client/managed` for game behavior. Inspect a compiled or
  third-party DLL only when needed, with `client/tools/ManagedSource`
  ([usage](client/tools/README.md)); never paste raw DLL bytes or whole assemblies.

## Server work

- Build and test from the root: `cargo build`, and
  `SPRK_TEST_POSTGRES_URL=<server with CREATEDB> cargo test --locked`
  (see [server/README.md](server/README.md)). Add a regression test for each fix.
- The local stack is `start_server.bat` (database in the Docker container
  `sprk-postgres-local`, game on 8080, updates on 8081, GM on 8082). When
  `client-updates/stable/manifest.json` is missing, its first start downloads
  and verifies the newest published client release from GitHub; it never
  replaces existing local releases.
- For a game log, database dump, diagnostics report or a slow, failing or
  locking server, follow [the database guide](docs/database-diagnostics.md):
  treat the files as player data, load dumps only into a scratch container,
  run `server/scripts/database/db_diagnostics.py`, and fix the cause in server
  code with a regression test and before/after numbers.
- Some server tests read client source (for example the `parallelReads` list in
  `client/managed/SprkNetworkPerformance/SprkNetworkPerformance.cs`). Keep such
  lists plain initializers and run those tests after changing either side.

## Client work

- Change behavior by editing C# under `client/managed`. A new feature assembly
  is a new `client/managed/SprkName/SprkName.csproj` directory; the build and
  release include it automatically.
- Unity loads components by field name and declaration order. In
  `MonoBehaviour`, `ScriptableObject` and `[Serializable]` types, never reorder,
  rename, retype or remove existing fields; append new ones or mark them
  `[NonSerialized]`. Keep assembly, namespace and type names that assets,
  reflection or the server refer to.
- Use `client/dev.py` for the loop: `build` compiles (it must finish without
  errors before anything is installed or committed), `test` builds into the test
  copy `../sprk-client-test` and starts it on the local server, `install`
  builds into `../sprk-client` and publishes a local release, `clean` deletes
  the test copy.
- Try changes with `test` before `install`. Never start the test copy with
  SprkLauncher (it restores the published DLLs) and never delete it other than
  with `clean`: it links to the real game's assets.
- Verify user-visible changes on the real game with
  [client automation](docs/client-automation.md)
  (`--client ../sprk-client-test`); it runs an isolated server and database.
- Assets and tables have no source form. Put a changed file, in final form,
  under `client/overrides/files/` at its client-relative path, with matching
  `SPRK-Native*.json` integrity metadata when the data requires it. Changes
  made only in `../sprk-client` or in ignored folders never reach CI.
- `client/scripts/release/client_baseline.py` pins the published release that
  supplies assets and runtime DLLs. Change it only with `pin_client_baseline.py`
  when deliberately adopting a newer release.

## CI and deployment

Details: [GitHub CI and client updates](docs/github-client-updates.md).

- Flow: commit to `development`, open a PR into `main`, merge. The merge builds
  the client from tracked inputs, signs and publishes it as a GitHub release,
  builds the matching server image, pushes it to GHCR and redeploys through
  Portainer. Nothing is uploaded by hand. A local release is not a deployment.
- For source-only, CI or build-tool work, validate the complete release without
  touching the installed game or local manifest:
  `python client/scripts/release/build_client_release.py --work <new folder> --version <unique id> --check-only`.
  Report that no installed-client release was generated.
- Each server image pins its client release and installs it only at startup,
  keeping the active release and two previous ones. Preserve this pinning so an
  older server never serves a newer, incompatible client. The `/client-updates`
  mount must be writable by UID/GID `10001`.

## Required local release after installing client changes

When a task changes the installed game (`../sprk-client`: compiled DLLs,
TableData, TableJit, LocalizationJit or other launcher-managed files),
**publish a new signed local release before reporting the task complete**, so
the local update server and launcher agree with the game folder. A request to
implement a client change authorizes this; do not ask separately. If the user
asks for source-only work, do not install, and say that no installed-client
release was generated.

1. Confirm `../sprk-client` exists. Close the local server and the game when
   needed; the user authorizes this.
2. Build and install with
   `python client/scripts/build/build_managed_client.py --install ../sprk-client`
   (or `python client/dev.py install`, which also performs steps 3-4 with a
   timestamped release ID), then validate the installed game. Never publish an
   unchanged client as if the change were installed.
3. Choose a new release ID, `local-YYYYMMDD-description` with the user's local
   date (add a suffix if needed). Check `client-updates/releases` and
   `client-updates/stable/manifest.json`; never overwrite a release.
4. Publish with the default selection and the existing key:

   ```powershell
   python client/scripts/release/publish_client_update.py --client ../sprk-client --output client-updates --channel stable --version <release-id> --signing-key client-updates/update-signing/private.pem
   ```

   The default keeps every file the previous release managed. `--include`
   replaces that selection and would mark every other managed file deleted.
5. Compare the new manifest with the previous one (added, removed, changed,
   `deleted_files`). The publisher snapshots the whole installed game, so every
   difference must belong to this task.
6. Verify that `stable/manifest.json` equals the release's manifest, the
   signature verifies with `config/client-update-public.pem`, and every release
   file matches its manifest size and SHA-256, including the changed files.
7. Report the release ID, the checks and any restart needed. Distinguish this
   local release from a GitHub release and from a Portainer deployment. If the
   user asks for manual hosting, name the release folder and manifest to upload.

Batch related changes into one release unless asked otherwise. Server-only or
documentation-only changes need no client release. If the key, the game folder
or publishing is unavailable, report exactly what is blocked and unfinished;
never publish unsigned to work around it.
