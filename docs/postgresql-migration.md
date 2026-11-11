# PostgreSQL migration

Status (Nov 2026): **phases 1–5 are implemented** on the `postgres-migration`
branch and verified in tests and a rehearsal on a production copy. Production
still runs the SQLite build until the cutover in section 7. What remains needs
the operator: the phase 0 baseline, a play-through with the real client on a
rehearsal, a load test, the cutover itself, and phase 6 tuning (section 10).

Diagnosis and tuning of the running database: [database-diagnostics.md](database-diagnostics.md).

## 1. Goal and decisions

Move the game database from SQLite (`sprk.db`, one writer connection) to
PostgreSQL so writes from different players run in parallel, and so the
service can later move to a managed cloud database (AWS RDS / Aurora) and run
more than one `game` container.

| Decision | Choice | Why |
| --- | --- | --- |
| Database | **PostgreSQL 18** | Closest dialect to the existing SQL (`ON CONFLICT`, `RETURNING`), and the widest scaling path (replicas, Aurora, Citus and other PostgreSQL-compatible databases). |
| Data access | **sqlx 0.9 through `crate::db`** | About 2,350 query call sites already used sqlx. `crate::db` keeps SQLite's binding and decoding rules (section 2), so call sites did not change type by type. |
| Query checking | **`PREPARE` sweep in CI** instead of `query!` macros | `scripts/server/check_sql.py` type-checks every literal statement against the migrated schema (2,134 statements). Converting all call sites to macros would have rewritten every query for the same protection. |
| Column types | **SQLite's storage types**: `bigint` and `text` (JSON and timestamps included) | Rust code binds and reads `i64` and `String`; changing types would fail at runtime on untested paths. Per-column `jsonb`/`timestamptz` conversion is phase 6 work. |
| Schema management | `sqlx::migrate!()` files in `migrations/`, applied at startup with the owner role | Replaces `create_tables`, the ad hoc `ALTER TABLE` steps and the `PRAGMA` checks. |
| Concurrency | Per-player advisory lock in every request transaction, plus locks on shared rows (section 3) | Same guarantees as the single writer for one player and for shared rows; different players in parallel. |
| Caching (Redis / Valkey) | **Not now** | One `game` container holds sessions, chat and rooms in memory. Add it only for several `game` containers (section 9). |
| Dual backend | **No** | One branch, one cutover. The SQLite file and image are the rollback point. |

## 2. How the port works (for developers)

### SQL dialect

All SQL is PostgreSQL with `$1, $2, …` placeholders. SQLite functions became
`sprk_*` helpers in `migrations/0001_helpers.sql` with SQLite's semantics:

| SQLite | PostgreSQL |
| --- | --- |
| `datetime('now')`, `datetime(x, '-5 seconds')` | `sprk_datetime('now')`, `sprk_datetime(x, '-5 seconds')` (text `YYYY-MM-DD HH:MM:SS`, UTC, per statement) |
| `date('now')`, `date(x, '-1 day')` | `sprk_date(...)`; `sprk_utc_day()`; column default `sprk_now()` |
| `strftime('%s', x)`, `unixepoch()` | `sprk_epoch(x)` (truncated seconds, like SQLite) |
| `json_extract(d, '$.a.b')` | `sprk_json_int` / `sprk_json_text` / `sprk_json_real` / `sprk_json` (jsonb), picked per field from the types found in production data |
| `json_set`, `json_remove`, `json_type`, `json_array_length`, `json_each` | `sprk_json_set` (one path; nest for several), `sprk_json_remove`, `sprk_json_type`, `sprk_json_array_length`, `sprk_json_each`; `SELECT value FROM json_each(?)` → `SELECT sprk_json_ints($n)` |
| `json(?)` inside `json_set` | `($n)::jsonb` |
| `INSERT OR IGNORE` / `OR REPLACE` | `ON CONFLICT DO NOTHING` / `ON CONFLICT (...) DO UPDATE SET …` |
| `last_insert_rowid()` | `INSERT … RETURNING id` |
| scalar `MAX(a,b)`, `MIN(a,b)` | `GREATEST`, `LEAST` (they ignore NULLs; SQLite returned NULL) |
| `SUM(x)` | `SUM(x)::bigint` (PostgreSQL returns `numeric`) |
| `a IS b`, `a IS NOT b` | `IS NOT DISTINCT FROM`, `IS DISTINCT FROM` |
| `LIKE`, `COLLATE NOCASE` | `ILIKE`, `lower(a) = lower(b)` |
| `PRAGMA`, `sqlite_master` | `information_schema`; migrations own the schema |

Rules that do not show up as syntax errors:

- In `ON CONFLICT … DO UPDATE`, qualify the existing row: `SET n = t.n + excluded.n`
  (an unqualified `n` is ambiguous in PostgreSQL).
- `challenge_results."current_time"` must be quoted; unquoted it is the
  `CURRENT_TIME` function.
- `GROUP BY` is strict: group by the primary key to select other columns.
- `ORDER BY` puts NULLs last in ascending order (SQLite: first).
- A failed statement aborts the transaction; use a savepoint (`tx.begin()`)
  to try something and continue.
- JSON fields compared in `WHERE` must use the same expression as their index
  (`sprk_json_int(data,'$.OnlineMatchUid')`).

### `crate::db`

`crate::db::query`, `query_scalar`, `query_as` and `RowExt::get`/`try_get`
replace their sqlx counterparts everywhere outside the database plumbing:

- **Binds:** every integer width and `bool` binds as `bigint` (SQLite stored
  booleans as 0/1), JSON values as text. One parameter type per SQL text also
  avoids sqlx's prepared-statement cache error ("incorrect binary data format")
  when two call sites bind `i32` and `i64` to the same statement.
- **Decodes:** any integer column into any integer type (range-checked), into
  `bool` (non-zero) and `f64`; JSON text into `serde_json::Value`.

### Triggers

The 146 SQLite triggers are PL/pgSQL functions in `migrations/0008` (progression
metrics, play records, mail notifications, arena upkeep and retention) and
`0009` (response-cache revisions). Counters saturate at the `bigint` maximum
instead of failing the gameplay write. The currency guards became `CHECK`
constraints. `ported_triggers_match_sqlite_triggers` replays the same writes on
the final SQLite schema (`tests/fixtures/sqlite_schema.sql`) and on PostgreSQL
and compares every derived row.

### Tests

`cargo test` needs `SPRK_TEST_POSTGRES_URL` (a role that may create databases;
see `README.md`). The first test builds a migrated template database; each test
clones it (`database::test_pool`). Helpers: `reconnect` (server restart),
`test_read_pool`, `row_changes` (SQLite's `total_changes()`), `fail_trigger`
(make writes fail), `explain` (plans with sequential scans disabled). CI runs
the suite, the native Wine tests and the image smoke test against PostgreSQL
service containers, plus `check_sql.py`.

## 3. Concurrency model

SQLite ran one transaction at a time. PostgreSQL runs them in parallel, so
read-then-write code can lose updates or deadlock unless it locks.

1. **Per-player serialization.** The `acting_account` middleware records the
   player of each POST request with a valid session; `database::begin` (used
   for every write transaction of a request) takes that player's
   `pg_advisory_xact_lock(account_id)`. One player's transactions run in turn,
   as on the single writer; different players run in parallel. The lock lives
   in PostgreSQL, so it also holds across several `game` containers. It is
   held per transaction, not per request, so non-database work never blocks
   the player's other requests.
2. **Background, chat and battle work** has no acting account. The places that
   reserved the SQLite writer now take explicit account locks (co-op and arena
   settlement on the room master, mail, party messages, coupons, inventory
   initialization).
3. **Shared rows.**
   - Guild content: community requests of a guild member take the guild's
     lock (`postgres::lock_guild`) after the account lock.
   - Party/raid rooms: the room row is read `FOR UPDATE` before the JSON is
     rewritten.
   - Arena: matchmaking writes both players' queue entries and swords, so
     arena requests and arena settlements take one arena queue lock
     (`postgres::lock_arena_queue`) after the account lock and run in turn,
     as they did on the single writer; a queue entry that changed anyway
     aborts the pairing.
   - Guild arena pairing locks the opponent guild with a `try` lock
     (skipping a busy one instead of waiting, which could deadlock) and
     re-reads it.
   - Counters shared across players (guild conquest keys, honor buff,
     friendship points) are single atomic statements.
4. **Retry wrapper.** `postgres::transaction(pool, Isolation, closure)` reruns
   on 40001/40P01. Request handlers do not use it yet (they have side effects
   inside transactions); a deadlock fails one request, which the client
   retries. Watch the counts in the diagnostics report.
5. **Hot rows to watch:** the global `('progression', 0)` revision (bumped by
   every world boss score and credential change) and per-guild rows during
   guild raids. Phase 6 may batch them.

Tests: `account_lock_serializes_one_account_only`,
`request_transactions_serialize_per_acting_account`,
`acting_account_middleware_prevents_lost_updates_within_one_player`,
`transaction_retries_serialization_failures`, and the existing concurrent claim
and purchase tests (mail, coupons, inventory, heroes, arena polls), which now
run on separate connections.

## 4. Schema

- `migrations/0001`–`0009`: helpers, then tables per area (core, progression,
  battle, community, services, compensation audit), gameplay triggers and
  revision triggers. Column names, keys, defaults and foreign keys match the
  final SQLite schema; all 542 columns of production were checked.
- `INTEGER` → `bigint` (flags stay 0/1), `TEXT` → `text`. Identity columns are
  `GENERATED BY DEFAULT` so the copy and test fixtures can keep explicit IDs.
- Production also holds `sprk_compensation_campaigns` / `_grants` (written by
  the out-of-tree compensation tool); `0007` keeps them.
- Frequently updated tables use `fillfactor = 85`. JSON expression indexes:
  `arena_online_match_status`, `battle_state_dispatch_pending`.
- Helpers call each other schema-qualified: index maintenance runs with
  `search_path = pg_catalog`.
- sqlx checksums migration files: `.gitattributes` pins them to LF. Never edit
  an applied migration; add a new one. New migrations must stay compatible
  with the image still running during a deploy (add columns before use; drop
  later).

## 5. Running it

- `DATABASE_URL` (app role) is required. `DATABASE_MIGRATION_URL` (owner role)
  applies migrations at startup; without it `DATABASE_URL` is used.
  `DATABASE_READ_URL` optionally points the read pool elsewhere (a replica
  later). `SPRK_DB_MAX_CONNECTIONS` sizes each pool (default 16).
- Sessions run in UTC with `statement_timeout` 30 s, `lock_timeout` 10 s and
  `idle_in_transaction_session_timeout` 60 s. The read pool sets
  `default_transaction_read_only`.
- The server refuses to start when `sprk.db` (or `SPRK_SQLITE_PATH`) exists
  next to a database without accounts, so a deploy can never open the game on
  an empty database before the copy.
- Locally, `start_server.bat` starts a loopback PostgreSQL container
  (`sprk-postgres-local`, port 54320, volume `sprk-postgres-local`) unless
  `DATABASE_URL` is set, and copies the local `sprk.db` into it once
  (`migrate-sqlite --if-empty`).

## 6. Infrastructure

### Compose / Portainer

`docker-compose.yml` runs `postgres` (PostgreSQL 18, volume `postgres-data`,
no published port, `pg_stat_statements`), `postgres-backup` and `game` (waits
for a healthy `postgres`). On first start of the volume,
`config/postgres/initdb/10-sprk-roles.sh` creates the `sprk` database and two
roles: `sprk_owner` (owns the schema, runs migrations and backups) and
`sprk_app` (data access only, not a superuser).

Portainer stack variables to set **before** the cutover (use long random
URL-safe passwords, letters, digits, `-` and `_`):

| Variable | Use |
| --- | --- |
| `SPRK_DB_SUPERUSER_PASSWORD` | bootstrap superuser (maintenance only) |
| `SPRK_DB_OWNER_PASSWORD` | `sprk_owner`: migrations, backups, diagnostics |
| `SPRK_DB_APP_PASSWORD` | `sprk_app`: the game |
| `SPRK_DB_BACKUP_HOST_DIR` | host backup directory, default `/srv/sprk/db-backups` (create it, owned by UID 999) |
| `SPRK_DB_SHARED_BUFFERS` | about 25% of the memory available to PostgreSQL (default 512MB) |

The passwords take effect only when the volume is first created; change them
later with `ALTER ROLE … PASSWORD`. Add PgBouncer (transaction mode, 1.21 or
later) only for several `game` containers.

### Backups and restore

`postgres-backup` writes `sprk-<UTC time>.dump` (`pg_dump -Fc`) daily at
`SPRK_DB_BACKUP_HOUR_UTC` (default 04:00), and at start when the newest dump is
over a day old. After each successful dump it keeps the newest
`SPRK_DB_BACKUP_KEEP` files (default 14); a failed dump deletes nothing. The
dump of the production copy is 24 MB.

For an off-host copy, mount a NAS (or other storage) on the host and set
`SPRK_DB_BACKUP_COPY_HOST_DIR` to that directory, writable by UID 999. Every
run copies the dumps it is missing there (so it catches up after a NAS outage)
and keeps the newest `SPRK_DB_BACKUP_COPY_KEEP` (default: same as
`SPRK_DB_BACKUP_KEEP`). A failed copy only logs a warning. Docker never creates
that directory: if the NAS is not mounted, `postgres-backup` does not start,
rather than writing "NAS" copies onto the local disk.
Copy the directory off the host as well; for point-in-time recovery add WAL
archiving (pgBackRest or WAL-G) later. On AWS, RDS/Aurora backups replace this.

Restore (tested on the production copy: identical row counts in all tables,
triggers and sequences intact):

```bash
docker compose stop game
docker compose exec -T postgres pg_restore -U sprk_owner -d sprk --clean --if-exists \
    --no-owner --single-transaction --exit-on-error < /srv/sprk/db-backups/sprk-<time>.dump
docker compose start game
```

This replaces the contents of the existing `sprk` database in one transaction
(a failure leaves it unchanged) and keeps the roles, their settings and the
grants from the init script; restored objects belong to `sprk_owner`, whose
default privileges give `sprk_app` access. Restore a backup into a scratch
database now and then; a backup that has never been restored is not a backup.

### CI (GitHub Actions)

`test-pr.yml` runs PostgreSQL service containers for the server tests and the
native Wine tests, and the image smoke test starts its own. `check_sql.py` runs
after the tests. `build-and-deploy.yml` is unchanged: migrations run at `game`
startup.

## 7. Data migration and cutover

### The copy tool

`sprk-server migrate-sqlite <sprk.db> [--report file.json] [--if-empty]`
(`src/migrate_sqlite.rs`) copies a SQLite database into the empty PostgreSQL
database named by `DATABASE_MIGRATION_URL` or `DATABASE_URL`:

- Opens the SQLite file read-only and runs `PRAGMA integrity_check`. Index-only
  damage is reported and tolerated (PostgreSQL builds its own indexes and its
  keys reject duplicates); anything else stops the copy.
- Checks that every SQLite table and column exists in the schema.
- Loads all tables in one transaction with `COPY`, parents first, with
  gameplay triggers disabled (rows already carry their derived values).
- Continues every identity sequence after the highest ID SQLite ever issued
  (`sqlite_sequence`), so IDs of deleted rows are never reused.
- Reads every table back and compares row counts and per-row checksums; any
  difference rolls everything back.

The source must have been opened by the **final SQLite build** (its startup
migrations and backfills are not repeated). Production is on that build until
the cutover.

Rehearsal on the production copy of 11 Nov 2026: 326,741 rows in 86 tables in
13 s, checksums equal; the server then started on it and served health checks.
That copy was taken while the server ran and `integrity_check` reported wrong
entry counts in the primary-key indexes of `play_record_values` and
`progression_metrics` (no duplicate rows). Run
`sqlite3 -readonly sprk.db "PRAGMA integrity_check;"` on production before the
cutover; take the final copy with the server stopped.

### Rehearse

1. Copy production's `sprk.db`, `-wal` and `-shm` together into a new folder
   (or `sqlite3 sprk.db ".backup copy.db"`).
2. Copy it into a scratch PostgreSQL database with `migrate-sqlite --report`.
3. Start a server on it and play through login, summon, campaign, co-op,
   guild, arena and mail with the real client (or
   `scripts/automation/client_automation.py launch --seed-db <copy>`).
4. Load test with many simulated accounts; check the diagnostics report for
   lock waits, deadlocks and slow statements (phase 0 baseline for comparison).

### Cutover

1. Before the day: set the Portainer variables (section 6), create the backup
   directory, run the rehearsal, decide the rollback window. Note the current
   image tag.
2. Announce maintenance. Stop `game`.
3. Back up `/data/sprk.db` with its `-wal`/`-shm` (the server is stopped, so
   the copy is consistent) and keep it as the rollback point.
4. Merge the PR. CI deploys the new image; `postgres` starts and creates the
   roles. `game` starts, migrates the empty schema and then **refuses to run**
   because `sprk.db` is still present; that is expected.
5. Copy the data:
   `docker compose run --rm game migrate-sqlite /data/sprk.db --report /data/sqlite-copy-report.json`.
   Keep the report.
6. `docker compose up -d game`. Check `/health` and `/health/battle`, log in,
   check a few accounts and the mailbox, then reopen.
7. Leave `sprk.db` in place for the rollback window (the guard is satisfied
   once PostgreSQL has accounts), then move it to cold storage.

### Rollback

Before reopening: redeploy the previous image tag; it opens the untouched
`sprk.db`. After reopening, new data exists only in PostgreSQL, so rolling back
loses it; decide the window in advance.

## 8. Tools

| Tool | Status |
| --- | --- |
| `gm.py maxheroes` | Clones a PostgreSQL database (`--source-url`, `--output-db`) and edits the clone. Needs `psycopg`. The other `gm.py` commands use the GM HTTP API and are unchanged. |
| `scripts/server/db_diagnostics.py` | `--database-url`: sizes, dead rows, cache hit rate, `pg_stat_statements`, long transactions, lock waits, plans. `--log` also reads SQLite-era logs. |
| `scripts/automation/client_automation.py` | Each run gets its own `sprk_auto_*` database (`--seed-db` copies a SQLite file into it); `assert-db` runs read-only. Needs `psycopg`. |
| `scripts/server/start_local_server.ps1` | Local PostgreSQL container and one-time copy of the local `sprk.db`. |
| `scripts/server/check_sql.py` | `PREPARE` sweep of all literal SQL (CI). |

## 9. Later: several game containers and the cloud

- **Several `game` containers.** The database is no longer the blocker (its
  locks are database-wide); in-process state is: sessions, chat, co-op/battle
  rooms and the Wine battle worker. Options: sticky routing so a room lives on
  one instance, plus Valkey/Redis for sessions and pub/sub. Add PgBouncer then.
- **AWS.** RDS or Aurora PostgreSQL needs only new URLs, RDS Proxy as the
  pooler and `DATABASE_READ_URL` pointing at a reader. Distributed PostgreSQL
  (Aurora DSQL and others) needs the retry wrapper on request transactions and
  a check of its feature limits (advisory locks, triggers) against the schema.

## 10. Phases

| Phase | Work | Status |
| --- | --- | --- |
| 0. Baseline | `db_diagnostics.py --log` on a recent production log; record latency | **Operator.** Do it before the cutover; the script still reads SQLite-era logs. |
| 1. Foundation | PostgreSQL in Compose and CI, schema for all tables, retry wrapper, account lock | Done |
| 2. Port | All queries, `crate::db`, triggers, tests on PostgreSQL | Done: 735 tests, 2,134 statements type-checked |
| 3. Concurrency | Per-player lock on request transactions, shared-row locks, concurrency tests | Done; watch deadlock counts in production |
| 4. Tools | `gm.py`, diagnostics, automation, local launcher, backups and restore | Done; restore tested on the production copy |
| 5. Data and cutover | Copy tool, validation, rehearsal | Tool and rehearsal done. **Operator:** real-client play-through, load test, cutover |
| 6. Tuning | Hot JSON fields to columns or `jsonb`, `timestamptz`, booleans, `synchronous_commit` for low-value writes, batching hot counters, autovacuum | After the cutover, against the phase 0 baseline |
| 7. Cleanup | Remove the SQLite copy path and migration-only code | After the rollback window; checklist in [postgresql-cleanup.md](postgresql-cleanup.md) |
