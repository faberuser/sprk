# PostgreSQL migration plan

Status: **phase 1 (foundation) in progress** (Nov 2026). The game still runs on
SQLite. Done so far: the PostgreSQL schema for every table (`migrations/`),
ported triggers, the pool, the account lock and the retry wrapper
(`src/postgres.rs`), parity tests (`src/postgres_tests.rs`), the `postgres`
Compose service behind a profile, and PostgreSQL in the PR test job. The
current SQLite design and its diagnostics are in
[database-diagnostics.md](database-diagnostics.md); keep that document accurate
until the cutover ships.

## 1. Goal and decisions

Move the game database from SQLite (`sprk.db`, one writer connection) to
PostgreSQL so writes from different players run in parallel, and so the
service can later move to a managed cloud database (AWS RDS / Aurora) and run
more than one `game` container.

| Decision                 | Choice                                                 | Why                                                                                                                                                                                                                                                                                                                                                                            |
| ------------------------ | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Database                 | **PostgreSQL 18**                                      | Closest dialect to the existing SQL (`ON CONFLICT … DO UPDATE`, `RETURNING`), JSONB for the `data` columns, and the widest scaling path (read replicas, Aurora, Citus, Aurora DSQL, YugabyteDB, CockroachDB all speak the PostgreSQL protocol). MariaDB would need every upsert rewritten and has fewer managed and distributed options.                                       |
| Data access              | **Stay on sqlx** (0.9, upgraded Nov 2026)              | About 2,300 runtime query call sites already use sqlx. An ORM (SeaORM, Diesel) would mean rewriting all of them and still needing raw SQL for JSON and upserts. The upgrade also fixed RUSTSEC-2024-0363 (sqlx < 0.8.1). Since 0.9, runtime-built SQL must be wrapped in `sqlx::AssertSqlSafe(...)`; keep new dynamic SQL limited to constant identifiers and bind all values. |
| Query checking           | Convert ported queries to `sqlx::query!` / `query_as!` | Compile-time checks against the real schema catch PostgreSQL's strict typing (for example `INTEGER` decodes to `i32`, not `i64`). CI builds with `SQLX_OFFLINE=true` and the committed `.sqlx/` folder.                                                                                                                                                                        |
| Schema management        | `sqlx::migrate!()` files in `migrations/`              | Replaces `create_tables`, the ad hoc `ALTER TABLE` steps, and the `pragma_table_info` / `sqlite_master` checks.                                                                                                                                                                                                                                                                |
| Caching (Redis / Valkey) | **Not now**                                            | One `game` container already holds sessions, chat and rooms in memory. Add Valkey only when running several `game` containers (see section 9).                                                                                                                                                                                                                                 |
| Dual backend             | **No**                                                 | Supporting SQLite and PostgreSQL at once doubles the SQL. Port on one branch, cut over once, keep the SQLite file as the rollback point.                                                                                                                                                                                                                                       |

## 2. Why the concurrency model changes

SQLite locks the whole file for writes, so only one writer can run. The
single-connection pool in `database::open_pool` turns that into an orderly
queue, which removed the `database is locked` errors. It also means every
transaction (`.begin()` appears about 140 times) currently runs alone, and
some code quietly depends on that.

PostgreSQL uses MVCC and row locks. Readers never block writers, and writers
only wait for each other on the **same row**. That error no longer occurs, but
read-then-write transactions can now race:

- At the default `READ COMMITTED` level, two requests that both read a value
  and write it back cause a **silent lost update**.
- At `SERIALIZABLE`, or on distributed PostgreSQL, one of them fails with
  SQLSTATE `40001` and must be retried.

The migration therefore replaces the global queue with a per-account lock
first (section 5), then fixes shared-row paths one by one.

## 3. Porting inventory

Counts are from `src/` including tests, at the time of writing. Re-run the
searches before starting; they are a guide, not a checklist.

| SQLite construct                               | Count                                              | PostgreSQL replacement                                                                                                                                                  |
| ---------------------------------------------- | -------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `?` bind placeholders                          | nearly every query                                 | `$1, $2, …` (numbered; reused binds can share a number)                                                                                                                 |
| `ON CONFLICT (…) DO UPDATE/NOTHING`            | 88                                                 | Same syntax. Use `EXCLUDED.col` for the new value.                                                                                                                      |
| `INSERT OR IGNORE`                             | 57                                                 | `INSERT … ON CONFLICT DO NOTHING`                                                                                                                                       |
| `INSERT OR REPLACE`                            | 7                                                  | `ON CONFLICT (key) DO UPDATE SET …`. Note `OR REPLACE` deletes and reinserts, which resets omitted columns to their defaults and fires delete cascades; check each one. |
| `RETURNING`                                    | 24                                                 | Same                                                                                                                                                                    |
| `last_insert_rowid()`                          | 29                                                 | `INSERT … RETURNING id`                                                                                                                                                 |
| `INTEGER PRIMARY KEY AUTOINCREMENT`            | 20                                                 | `bigint GENERATED BY DEFAULT AS IDENTITY` (see section 4)                                                                                                               |
| `json_extract(col, '$.a')`                     | ~110 (`data` 111, `entry` 12, `mission_summary` 8) | `col->>'a'` (text) / `col->'a'` (jsonb); cast as needed, e.g. `(data->>'DuelType')::int`                                                                                |
| `json_each(…)`                                 | 12                                                 | `jsonb_array_elements(…)` / `jsonb_each(…)`                                                                                                                             |
| `datetime('now')`, `strftime(…)`, `unixepoch`  | ~45                                                | `now()`, `to_char(…)`, `extract(epoch from …)`                                                                                                                          |
| Scalar `MAX(a, b)` / `MIN(a, b)`               | 21                                                 | `GREATEST(a, b)` / `LEAST(a, b)`                                                                                                                                        |
| `COLLATE NOCASE`                               | 5                                                  | `citext` column or a `lower(col)` unique index                                                                                                                          |
| `PRAGMA`, `sqlite_master`, `pragma_table_info` | ~27                                                | Removed; migrations own the schema                                                                                                                                      |
| `CREATE TRIGGER`                               | 146 (105 generated by `system::revisions`)         | PL/pgSQL trigger functions; **done** in `migrations/0008` and `0009`                                                                                                   |

### Behaviour differences that will not show up as syntax errors

These pass review and compile, then return different results. Check for each
while porting a module, and cover the risky ones with tests.

- **Strict typing.** SQLite stores anything in any column and compares
  `'5' = 5` as true. PostgreSQL rejects mismatched binds and comparisons.
  Decide the Rust type for every column (`i64` ↔ `bigint`, `i32` ↔ `integer`,
  `bool` ↔ `boolean`, `f64` ↔ `double precision`).
- **Booleans.** Code that writes `0`/`1` into a `boolean` column or filters
  `WHERE flag = 1` breaks. Either use `boolean` and bind `bool`, or keep the
  column `smallint`. Pick one rule per table.
- **`LIKE` is case-sensitive** in PostgreSQL (SQLite's is case-insensitive for
  ASCII). Friend/user/guild search must use `ILIKE` or `lower()`.
- **NULL ordering.** SQLite sorts NULLs first in `ASC`; PostgreSQL sorts them
  last. Add `NULLS FIRST` / `NULLS LAST` where rankings or lists depend on it.
- **`GROUP BY` is strict.** SQLite allows non-aggregated "bare" columns;
  PostgreSQL requires them in `GROUP BY` or wrapped in an aggregate.
- **Integer overflow** raises an error instead of silently converting to a
  float. Currency and score columns should be `bigint`.
- **Timestamps.** SQLite stores them as TEXT or integers in several formats.
  Use `timestamptz` (UTC) for new columns, or keep epoch-second `bigint` where
  the game protocol already uses epoch values. Do not mix formats in a column.
- **`LIMIT -1`** (SQLite "no limit") is invalid; omit `LIMIT` or use `LIMIT ALL`.
- **Empty-string vs NULL** and **text-vs-number sort order** may differ for
  columns that held mixed values; the data migration (section 7) must report
  any such rows.
- **Reserved word.** `challenge_results.current_time` must be written
  `"current_time"`. Unquoted, PostgreSQL returns the `CURRENT_TIME` function
  and the query still runs.
- **JSON booleans.** SQLite's `json_extract` turns `true` into `1`; PostgreSQL's
  `(data->>'Win')::bigint` fails on `'true'`. Arena results store `Win` and
  `Canceled` as JSON booleans. Port integer reads as
  `sprk_json_int(data #> '{End,MatchResult,Win}')`, which keeps SQLite's
  result for numbers, booleans and missing values and never raises.
- **JSON binds.** Code binds JSON as `String` (`value.to_string()`). A text
  parameter is rejected by a `jsonb` column: bind `sqlx::types::Json(value)`
  or write `$1::jsonb`. Reading a `jsonb` column into `String` also fails;
  select `col::text` or decode `serde_json::Value`.
- **Timestamps as text.** Code reads and writes the `TEXT` timestamps as
  `String`. On `timestamptz` they decode as `chrono::DateTime<Utc>`; strings
  bound as parameters are rejected, while literals in SQL still work.
- **Explicit identity values.** Tests (and `websocket.rs` test setup) insert
  explicit IDs such as `battle_rooms.id = 1`. Under PostgreSQL that does not
  advance the identity sequence, so a later default insert collides. Insert
  without IDs, or `setval` after fixtures.

## 4. Schema design

- One migration per area, matching the current `create_tables` modules
  (`auth`, `progression`, `extensions`, …), reviewed table by table. The code
  creates 84 tables. Production also holds `sprk_compensation_campaigns` and
  `sprk_compensation_grants`, written by the out-of-tree compensation tool;
  `migrations/0007` keeps them so their audit records survive.
- **Implemented type rules** (`migrations/0001_helpers.sql` lists them):
  every SQLite `INTEGER` becomes `bigint`, including 0/1 flags, so existing
  `i64` code keeps decoding and `= 1` filters keep working (moving flags to
  `boolean` is a phase 6 cleanup). `TEXT` timestamps (all
  `YYYY-MM-DD HH:MM:SS` UTC in production) become `timestamptz`.
  Epoch-second columns stay `bigint`, and day/period keys (`'2026-11-11'`,
  `'all'`, `'stock:3'`, `'42:2026-10-19'`) stay `text`. Keys, unique
  constraints, foreign keys and defaults match SQLite (sqlx enables SQLite
  foreign keys, so they are enforced today).
- Identity columns are `GENERATED BY DEFAULT`, not `ALWAYS`: the data load
  keeps existing IDs without `OVERRIDING SYSTEM VALUE`, and tests can keep
  their fixtures (see the explicit-ID note in section 3).
- The SQLite currency guard triggers (`currency64_*`) became `CHECK`
  constraints: `bigint` already rejects non-integers and overflow. A violation
  is SQLSTATE `23514` (`22003` for overflow), not the old `RAISE` message;
  `currency::user` already checks the range in its `WHERE`.
- Every session runs in UTC (role setting and pool options), so
  `sprk_utc_day()` matches SQLite's `date('now')` day keys.
- Put the owning `account_id` on every per-player table and in the leading
  position of its primary key or main index. It is the natural shard key if
  the database is ever distributed (Citus), and it keeps per-player queries on
  one index range.
- Use `bigint` identity or UUIDv7 keys. Do not rely on IDs being sequential
  across connections or nodes.
- **JSON columns** (checked against production values, all valid JSON) are
  `jsonb`: every `data` column except `service_replays.data` (base64 text),
  plus `arena_runs.mission_summary`, `battle_runs.entry` and
  `begin_response`, `challenge_teams.team`, `chat_messages.content`,
  `craft_slots.materials`, `shop_stock.stock`, `lobby_appearance.snapshot`,
  `equip_items.punishment_rune_option`, `tutorial_progress.completion_response`,
  `mails.reward_items` / `reward_equipment` / `reward_currencies` / `receipt`,
  `global_mails.reward_*`, `service_replays.info`, and the `request` /
  `response` columns. Stored responses are parsed before they are returned
  (`read_json`), so `jsonb`'s key reordering is safe.
  `hero_friendly_info.selected_hero_indices` is comma-separated text.
- PostgreSQL rewrites the **whole row** on every `UPDATE`, so a large JSON
  document updated on every gameplay action creates heavy WAL and vacuum
  load. Move fields that change often (and fields used in `WHERE` /
  `ORDER BY`) into real columns or child tables. Keep JSON for data written
  rarely and read whole.
- Append-only history (mission history, play records) belongs in row-per-
  entry log tables rather than a growing JSON document.
- Frequently updated tables (player state, stamina, inventory) get
  `fillfactor = 85`, and constantly changing columns stay unindexed so
  updates can be HOT (heap-only tuple) updates. Applied to `user_info`,
  `heroes`, `equip_items`, `items`, `extension_state`, `battle_state`,
  `progression_metrics`, `play_record_values` and `response_revisions`.
- **JSON expression indexes.** `arena_completed_squad` now indexes the
  `season` column, which holds the same value as `data.Season`; ported
  queries should filter on `season`. `arena_online_match_status` and
  `battle_state_dispatch_pending` index `sprk_json_int(...)` expressions, and
  a query only uses them when it repeats the expression or predicate exactly.

### Triggers

The schema relies on 146 SQLite triggers, and the ported code must keep them:
progression metrics (gold, gems, stamina, items, clears, shop, mail, inn,
world boss), play records, mail arrival notifications, arena run upkeep
(derived `season` / `nonduel` / `squad` / `mission_summary`, retention and
`arena_history_archive`), and response-cache revisions. They are ported to
PL/pgSQL with the same effects, with these differences:

- Several triggers on one table are merged into one function, and SQLite's
  `AFTER` triggers that rewrote their own arena row are now one `BEFORE`
  trigger.
- Revisions still increase on every watched change, but are not bumped twice
  by those arena rewrites. Code compares revisions for equality only.
- `ported_triggers_match_sqlite_triggers` runs the same writes on both
  databases and compares every derived row. Until the cutover, a trigger
  change in SQLite needs the same change in `migrations/` (as a new
  migration file). `migrations_cover_every_sqlite_column` fails when a SQLite
  table or column has no PostgreSQL counterpart.
- **Hot rows.** The world boss and credential triggers bump the global
  `('progression', 0)` revision, and every progression trigger upserts the
  account's `progression_metrics` row. With the per-account lock the
  per-account rows are uncontended, but every world boss score from every
  player updates the one global row. Watch it in the load test (section 7);
  batching it (section 5, item 5) is the fallback.

## 5. Concurrency model after the cutover

1. **Per-account lock instead of the global writer queue.** Every mutating
   request takes a transaction-scoped advisory lock on the acting account:

    ```rust
    sqlx::query("SELECT pg_advisory_xact_lock($1)")
        .bind(account_id)
        .execute(&mut *tx)
        .await?;
    ```

    One player's requests keep today's one-at-a-time behaviour; different
    players run in parallel. The lock lives in PostgreSQL, so it also works
    with several `game` containers. Release is automatic at commit/rollback.

2. **Cross-player paths** need individual review: guild, guild arena, arena,
   friends/friend points, mail and global mail, raids/co-op rooms, conquest,
   duels, world boss. For each transaction that reads a shared row and writes
   it back, use one of:
    - a single atomic statement: `UPDATE … SET exp = exp + $1 WHERE id = $2 RETURNING exp`;
    - `SELECT … FOR UPDATE` on the shared rows, locked in a fixed order
      (ascending ID) to avoid deadlocks;
    - `SERIALIZABLE` plus the retry wrapper.
3. **Retry wrapper.** A helper that reruns a transaction closure on SQLSTATE
   `40001` (serialization failure) and `40P01` (deadlock), with a small
   bounded backoff. Required for distributed PostgreSQL later; useful now.

    Implemented in `src/postgres.rs`: `lock_account`, `lock_accounts`
    (several accounts, locked in ascending order) and
    `transaction(pool, Isolation, |tx| Box::pin(async move { … }))`, which
    makes up to 5 attempts and accepts `sqlx::Error`, `ServerError` or
    `anyhow::Error`. The closure can run more than once, so side effects
    outside the database (notifier, chat, HTTP) go after it returns.
4. **Short transactions.** Never `.await` non-database work (table lookups,
   notifier work, HTTP, battle-worker calls) inside an open transaction. The
   rule that moved the progression notifier off the writer still applies.
5. **Very hot counters** (guild EXP, raid boss HP, global event totals): batch
   increments in memory and flush periodically, or split them across N rows
   and sum on read.
6. **Durability per transaction.** For low-value writes (telemetry, play
   records, seen-flags, chat) use `SET LOCAL synchronous_commit = off`. A crash
   can lose the last fraction of a second of those commits; it never corrupts
   data. Keep the default for purchases, currency, summons and trades.
7. **Batching.** Grant-many and reward-mail paths use one
   `INSERT … SELECT * FROM UNNEST($1::bigint[], …)` instead of looped inserts.

### Read pool

`AppState.reads` keeps its role, as a second pool on the same server for now
and a replica endpoint later. Replicas lag slightly, while the current docs
assume readers see the latest commit. Keep read-your-own-writes paths (for
example mail count right after sending mail) on the primary when replicas
are introduced.

### Timeouts and pool sizing

- Pool size about 2–4× the database's CPU cores in total (for example 16–32),
  not one connection per request.
- Set `statement_timeout`, `lock_timeout` and
  `idle_in_transaction_session_timeout` on the app role so a stuck request
  cannot hold locks indefinitely (the equivalent of today's 30 s acquire
  timeout).

## 6. Infrastructure

### Compose / Portainer

- Add a `postgres:18` service with a named volume, a `pg_isready`
  healthcheck, and no published host port (internal Docker network only).
  `game` uses `depends_on: { postgres: { condition: service_healthy } }`.
  **Done**, except that the service sits behind the `postgres` Compose profile
  and `game` does not depend on it yet. A normal deploy does not start it.
  At the cutover, remove the profile and add the `depends_on`.
- `DATABASE_URL` and the password come from Portainer stack environment
  variables, never from the image or the repository. The service reads
  `SPRK_DB_SUPERUSER_PASSWORD`, `SPRK_DB_OWNER_PASSWORD` and
  `SPRK_DB_APP_PASSWORD`. `game` will need two URLs: `DATABASE_MIGRATION_URL`
  (owner) for the startup migration and `DATABASE_URL` (app) for its pools,
  because the app role cannot run DDL.
- Two roles: an owner role that runs migrations, and an app role with only
  `SELECT/INSERT/UPDATE/DELETE` on the tables. The app role is not a superuser.
  **Done:** `config/postgres/initdb/10-sprk-roles.sh` creates `sprk_owner`
  and `sprk_app` when the volume is first initialized, sets the app role's
  timeouts and UTC, and installs `pg_stat_statements`. Verified locally:
  migrations apply as `sprk_owner`; `sprk_app` writes through every trigger
  and is refused `CREATE` and `DROP`.
- Keep the `game-data:/data` volume. It still holds non-database state such
  as the native worker configuration that lives beside `sprk.db` today
  (`src/main.rs`).
- Server settings to start from: `shared_buffers` about 25% of the
  container's RAM, a larger `max_wal_size`, data on SSD/NVMe, and
  `pg_stat_statements` enabled. Tune autovacuum on high-churn tables after
  load testing.
- Add PgBouncer (transaction mode, version 1.21 or later for prepared
  statements) once more than one `game` container connects. On AWS, RDS Proxy
  fills the same role.

### Backups and restore

SQLite backups were file copies. PostgreSQL needs its own:

- Nightly `pg_dump -Fc` from a small backup container to a host directory
  outside the database volume, with retention.
- For point-in-time recovery, WAL archiving with pgBackRest or WAL-G. On AWS,
  RDS/Aurora automated backups and PITR replace this.
- A documented restore procedure, tested once before cutover and then
  periodically. A backup that has never been restored is not a backup.

### CI (GitHub Actions)

- `test-pr.yml`: add a `services: postgres` container to the `Test server`
  job and set `DATABASE_URL`. Build with `SQLX_OFFLINE=true`, and fail if
  `cargo sqlx prepare --check` reports a stale `.sqlx/` folder.
- `build-and-deploy.yml`: no new deployment step. Migrations run at `game`
  startup before it accepts traffic; the existing Portainer webhook flow is
  unchanged.
- Migrations must be backward compatible with the image that is still
  running during a deploy (add columns before using them; drop in a later
  release).

### Tests

- Tests currently open SQLite pools directly (`database_tests.rs`,
  `remove_test_database`, single-connection test pools). Move them to
  `#[sqlx::test]`, which creates a fresh database per test from the
  migrations.
- Local development needs a PostgreSQL container
  (`docker compose up postgres`). Document it in `README.md`.
- Add concurrency tests for the cross-player paths in section 5: two tasks
  hit the same guild/arena/mail rows at once, and the final state must equal
  the sequential result.
- Keep `database_tests.rs`'s guarantee that every endpoint routed to
  `AppState.reads` never writes (PostgreSQL: connect the read pool with
  `default_transaction_read_only = on`).

## 7. Data migration and cutover

1. **Migration tool.** A one-off Rust binary (or `scripts/server/` Python
   script run with `-I`) that reads a `sprk.db` copy read-only and inserts
   into an empty PostgreSQL schema built by the migrations, table by table,
   converting types (timestamps, booleans, JSON text → `jsonb`). Prefer this
   over pgloader because the JSON and timestamp columns need per-table rules.
   Follow the safety rules in `database-diagnostics.md`: copy `sprk.db`,
   `-wal` and `-shm` together into a fresh folder, and run
   `PRAGMA integrity_check` first.

   Findings from a production copy (11 Nov 2026, taken while the server was
   running):
    - `integrity_check` reported wrong entry counts in the primary-key indexes
      of `play_record_values` and `progression_metrics`. The table rows have
      no duplicate keys, so only the indexes disagree. A file copy of a live
      database can be torn like this. Run `integrity_check` on production
      itself (read-only) to rule out real corruption. For the cutover, take
      the copy with the server stopped, or with SQLite's backup API
      (`sqlite3 sprk.db ".backup copy.db"`). The load rebuilds every index in
      PostgreSQL either way.
    - Every `INTEGER` column holds only integers, every `TEXT` timestamp is
      `YYYY-MM-DD HH:MM:SS`, and every JSON column parses. The type rules in
      section 4 need no per-row exceptions so far.
2. **Validation report.** Per table: row counts, a checksum of key columns,
   and every row that failed conversion (bad JSON, mixed-type values,
   out-of-range integers). Zero unexplained failures before cutover.
3. **Reset identity sequences** to `max(id) + 1` after loading.
4. **Rehearsal.** Run the full migration against a recent production copy,
   start the new server on it, and play through login, summon, campaign,
   co-op, guild, arena and mail with the real client (or
   `scripts/automation/client_automation.py`).
5. **Load test** before cutover with many simulated accounts. Compare request
   latency and check `pg_stat_statements` for slow or lock-heavy queries.
6. **Cutover:** announce maintenance, stop `game`, take a final SQLite copy,
   run the migration and validation, deploy the PostgreSQL image, smoke test,
   reopen.
7. **Rollback:** keep the final SQLite copy and the last SQLite image tag. If
   the cutover fails before reopening, redeploy the old image on the old file.
   After reopening, new data only exists in PostgreSQL, so rolling back means
   losing it. Decide the rollback window in advance.

## 8. Tools and docs to port

| File                                                                                             | Current SQLite use                                                        | Action                                                                                                   |
| ------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `gm.py`                                                                                          | `sqlite3` read-only export (around line 192)                              | Port to `psycopg`, or switch to the GM HTTP API                                                          |
| `scripts/server/db_diagnostics.py`                                                               | Reads `sprk.db` and the log; reports lock errors, hold times, query plans | Rewrite for PostgreSQL: `pg_stat_statements`, `pg_stat_activity`, lock waits, table/index bloat          |
| `scripts/client/storage_migration_status.py` (+ test), `scripts/automation/client_automation.py` | Reference SQLite                                                          | Check and port if they read the server database                                                          |
| `tests/service_modes.rs`                                                                         | SQLite setup                                                              | Move to the PostgreSQL test harness                                                                      |
| `docs/database-diagnostics.md`, `README.md`                                                      | Describe SQLite                                                           | Rewrite sections 3–4 of the diagnostics doc for PostgreSQL; update setup and volume tables in the README |

The hold tracker in `database.rs` (`HoldTracker`, `SLOW_HOLD`) assumes a
single connection. Replace it with pool acquire-time metrics, the existing
slow-request logging, and `pg_stat_statements`.

## 9. Later: several game containers and the cloud

These are out of scope for the migration, but the steps above are chosen so
they stay possible.

- **Several `game` containers.** The database is no longer the blocker;
  in-process state is: sessions, chat, co-op/battle rooms and the Wine battle
  worker. Options: sticky routing so a room lives on one instance, plus
  Valkey/Redis for shared sessions and pub/sub. Add PgBouncer at the same time.
- **AWS.** RDS PostgreSQL or Aurora PostgreSQL needs only a new
  `DATABASE_URL`, with RDS Proxy as the pooler and the read pool pointed at a
  reader endpoint. Aurora DSQL or other distributed PostgreSQL needs the
  retry wrapper and short, mostly single-account transactions from section 5,
  and its feature limits must be checked against the schema at that time.

## 10. Phases

| Phase               | Work                                                                                                                                     | Done when                                                      |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| 0. Baseline         | Run `db_diagnostics.py` on a recent production log/DB; record request latency and hold times                                             | Numbers recorded to compare against                            |
| 1. Foundation       | ~~sqlx upgrade~~ (done: 0.9); ~~PostgreSQL in compose and CI~~; ~~migration files for all 84 tables and 146 triggers~~; ~~retry wrapper and per-account lock helper~~ (all done locally, Nov 2026; first CI run pending) | Empty schema builds in CI; existing tests still pass on SQLite |
| 2. Port             | Module by module: placeholders, dialect, `query!` macros, section 3 behaviour checks, tests on `#[sqlx::test]`                           | All tests pass on PostgreSQL; no SQLite code left in `src/`    |
| 3. Concurrency      | Per-account lock on mutating routes; audit and fix cross-player paths; concurrency tests                                                 | Concurrency tests pass; no lost updates under load             |
| 4. Tools            | Port `gm.py`, diagnostics, compensation script; backups and restore tested                                                               | Restore from backup verified                                   |
| 5. Data and cutover | Migration tool, validation report, rehearsal, load test, cutover                                                                         | Production on PostgreSQL; rollback copy kept                   |
| 6. Tuning           | JSON hot fields to columns, `synchronous_commit` per path, batching, autovacuum                                                          | Latency at or below the phase 0 baseline under higher load     |
