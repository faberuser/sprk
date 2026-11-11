# Cleanup after the PostgreSQL migration

Code, files and docs that exist only to move from SQLite to PostgreSQL. Remove
them once the move is finished, so nobody maintains a second database path.
The migration itself is described in [postgresql-migration.md](postgresql-migration.md).

## When

All of these are true:

- Production has run on PostgreSQL for the whole rollback window, and the
  rollback image tag is no longer needed.
- The final production `sprk.db` (with `-wal`/`-shm`) is archived somewhere
  outside the server, or you have decided to delete it.
- The phase 0 baseline was compared with PostgreSQL numbers (the SQLite log
  sections of the diagnostics script are only needed for that).
- No one needs to copy another SQLite file: every local and test setup has
  already been copied (`sprk-postgres-local`, automation seeds).

## 1. Remove the SQLite copy path

| What | Where | Notes |
| --- | --- | --- |
| Copy tool | `src/migrate_sqlite.rs`, `mod migrate_sqlite` and the `migrate-sqlite` dispatch in `src/main.rs` | Its two tests go with it. |
| Startup guard | `refuse_unmigrated_sqlite` and `SPRK_SQLITE_PATH` in `src/database.rs` | Stops the server when `sprk.db` sits next to an empty database. |
| Test helper | `remove_sqlite_file` in `src/database.rs` | Only used by the copy tool's tests. |
| Parity tests | In `src/postgres_tests.rs`: `sqlite_schema`, `migrations_cover_every_sqlite_column`, `ported_triggers_match_sqlite_triggers`, `trigger_script`, `DERIVED`, `row_strings` | Keep the other tests in that file (constraints, locks, retry, read pool, acting account). |
| Schema fixture | `tests/fixtures/sqlite_schema.sql`; its line in `.gitattributes`; `COPY tests/fixtures` in `Dockerfile.tests`; the `tests/` lines in `.dockerignore` | Delete the folder if nothing else uses it. |
| sqlx SQLite driver | `"sqlite"` in the sqlx features in `Cargo.toml` | Last, after everything above compiles without it. `Cargo.lock` then drops `sqlx-sqlite` and `libsqlite3-sys`. |
| SQL check exclusions | `check_sql.py` skips `postgres_tests.rs` and `migrate_sqlite.rs` | Remove the skips once those SQLite queries are gone. |
| Local launcher | The `migrate-sqlite --if-empty` step in `scripts/server/start_local_server.ps1` | The PostgreSQL container part stays. |
| Automation seeds | `--seed-db` in `scripts/automation/client_automation.py` and `docs/client-automation.md` | Replace with a PostgreSQL seed, e.g. `CREATE DATABASE ... TEMPLATE <seed db>` or `pg_restore` of a dump. |
| Diagnostics | The "SQLite-era signals" block in `report_log` of `scripts/server/db_diagnostics.py` | Only after the phase 0 baseline comparison. |
| Docs | `README.md` (the `sprk.db` copy note under "The local database" and "Run the binary directly"), the `docker-compose.yml` comment on the `game-data` volume, section 7 of `postgresql-migration.md` | Turn `postgresql-migration.md` into a short operations guide (schema rules, concurrency, backups); the porting tables can go. |
| Data files | `/data/sprk.db*` in the `game-data` volume on the host; local `sprk.db*` files | After archiving. `game-data` keeps the worker key and other non-database state. |

## 2. Reword SQLite references in comments

These comments explain code in terms of the old single SQLite writer. The code
is correct; only the wording is dated. Reword them to describe PostgreSQL:

- `src/api/progression/cache.rs` (lines about "SQLite transaction" / "write lock")
- `src/api/tutorial/mod.rs` ("acquires the SQLite write lock")
- `src/api/battle/party_messages.rs` ("Obtain the SQLite writer")
- `src/api/battle/cooperative.rs` (two "waiting on SQLite" comments)
- `src/api/battle/mod.rs` (partial index note)
- `src/api/community/arena.rs` (`+account` note)
- `src/currency.rs` (module comment about REAL overflow)
- `src/database_tests.rs` (`SQLITE_READONLY` comment)
- `src/main.rs` ("Configuration lives beside sprk.db")
- `docs/database-diagnostics.md`, section 6 (dated SQLite measurements; keep
  them as history or move them to a changelog)

Comments that compare the new behaviour with the old writer on purpose
(`src/database.rs`, `src/postgres.rs`, `src/api/community/mod.rs`,
`src/api/system/middleware.rs`, `src/db.rs`) can stay or be shortened.

## 3. Compatibility layer (phase 6, not plain cleanup)

These keep the ported code behaving like it did on SQLite. They are in daily
use, so removing them means changing types and queries, one area at a time,
with tests:

- **`crate::db` lenient binds and decodes** (`src/db.rs`): needed while columns
  are `bigint` and Rust fields are `i32`/`u8`/`bool`. Shrinks as columns get
  native types (`boolean`, `integer`) and Rust types match.
- **`sprk_*` SQL helpers** (`migrations/0001_helpers.sql`): `sprk_json_*` stay
  while JSON is stored as `text`; converting a column to `jsonb` lets its
  queries use `->`/`->>` directly. `sprk_datetime`/`sprk_date`/`sprk_epoch`
  stay while timestamps are `text`. Never drop a helper that an index,
  trigger or older migration uses; add a new migration instead.
- **`bigint` 0/1 flags** → `boolean`, **text timestamps** → `timestamptz`,
  **JSON text** → `jsonb`: see phase 6 in the migration doc.

## 4. Keep

Not migration leftovers: `migrations/` (never edit or delete applied files),
the test database harness in `src/database.rs`, `scripts/server/check_sql.py`,
the account/guild/arena locks and retry wrapper in `src/postgres.rs`, the
`acting_account` middleware, the backup service, and the
`sprk_compensation_*` audit tables.

## Check when done

```bash
cargo build && SPRK_TEST_POSTGRES_URL=... cargo test
python3 -I scripts/server/check_sql.py --psql "docker exec -i <container> psql -U postgres"
git grep -niE "sqlite|sprk\.db|migrate-sqlite"   # only history and docs on purpose
```
