# PostgreSQL

The game database is PostgreSQL 18. This guide covers how the code talks to it,
the concurrency rules, the schema, and how it is run, backed up and restored.
Diagnosis and tuning of the running database:
[database-diagnostics.md](database-diagnostics.md).

The game moved here from SQLite (`sprk.db`, one writer connection) in November
2026 so that writes from different players run in parallel, and so the service
can later use a managed cloud database and more than one `game` container. The
file keeps its old name because an applied migration refers to it.

## 1. Decisions

| Decision | Choice | Why |
| --- | --- | --- |
| Data access | **sqlx 0.9 through `crate::db`** | `crate::db` keeps the binding and decoding rules the code was written for (section 2), so call sites need not change type by type. |
| Query checking | **`PREPARE` sweep in CI** instead of `query!` macros | `scripts/server/check_sql.py` type-checks every literal statement against the migrated schema. |
| Column types | **`bigint` and `text`** (JSON and timestamps included) | What the code binds and reads today. Per-column `jsonb`/`timestamptz`/`boolean` is tuning work (section 8). |
| Schema management | `sqlx::migrate!()` files in `migrations/`, applied at startup with the owner role | One source of truth, checksummed. |
| Concurrency | Per-player advisory lock in every request transaction, plus locks on shared rows (section 3) | One player's requests run in turn; different players run in parallel. |
| Caching (Redis / Valkey) | **Not now** | One `game` container holds sessions, chat and rooms in memory. Add it only for several `game` containers (section 7). |

## 2. Writing SQL

All SQL is PostgreSQL with `$1, $2, …` placeholders. Shared helpers live in
`migrations/0001_helpers.sql`:

| Need | Helper |
| --- | --- |
| Current or shifted time as text `YYYY-MM-DD HH:MM:SS` (UTC) | `sprk_datetime('now')`, `sprk_datetime(x, '-5 seconds')`; column default `sprk_now()` |
| Dates | `sprk_date(...)`, `sprk_utc_day()` |
| Epoch seconds of a text timestamp | `sprk_epoch(x)` (truncated) |
| Read a JSON field from a text column | `sprk_json_int` / `sprk_json_text` / `sprk_json_real` / `sprk_json` (jsonb), with `'$.a.b'` paths |
| Change JSON | `sprk_json_set` (one path; nest for several), `sprk_json_remove` |
| Inspect JSON | `sprk_json_type`, `sprk_json_array_length`, `sprk_json_each`, `sprk_json_ints($n)` (a JSON array of integers as rows) |

Rules that do not show up as syntax errors:

- In `ON CONFLICT … DO UPDATE`, qualify the existing row: `SET n = t.n + excluded.n`
  (an unqualified `n` is ambiguous).
- `challenge_results."current_time"` must be quoted; unquoted it is the
  `CURRENT_TIME` function.
- `GROUP BY` is strict: group by the primary key to select other columns.
- `SUM(x)` returns `numeric`: write `SUM(x)::bigint`.
- `GREATEST`/`LEAST` ignore NULLs; `ORDER BY` puts NULLs last in ascending order.
- A failed statement aborts the transaction; use a savepoint (`tx.begin()`)
  to try something and continue.
- JSON fields compared in `WHERE` must use the same expression as their index
  (`sprk_json_int(data,'$.OnlineMatchUid')`).

### `crate::db`

`crate::db::query`, `query_scalar`, `query_as` and `RowExt::get`/`try_get`
replace their sqlx counterparts everywhere outside the database plumbing:

- **Binds:** every integer width and `bool` binds as `bigint` (flags are 0/1),
  JSON values as text. One parameter type per SQL text also avoids sqlx's
  prepared-statement cache error ("incorrect binary data format") when two call
  sites bind `i32` and `i64` to the same statement.
- **Decodes:** any integer column into any integer type (range-checked), into
  `bool` (non-zero) and `f64`; JSON text into `serde_json::Value`.

### Triggers

Gameplay triggers are PL/pgSQL functions in `migrations/0008` (progression
metrics, play records, mail notifications, arena upkeep and retention) and
`0009` (response-cache revisions). Counters saturate at the `bigint` maximum
instead of failing the gameplay write. Currency limits are `CHECK` constraints.
`triggers_derive_the_recorded_rows` replays a fixed set of writes and compares
every derived row with `tests/fixtures/trigger_rows.txt`; to change trigger
output on purpose, rerun it with `SPRK_RECORD_TRIGGER_ROWS=1` and review the diff.

### Tests

`cargo test` needs `SPRK_TEST_POSTGRES_URL` (a role that may create databases;
see `README.md`). The first test builds a migrated template database; each test
clones it (`database::test_pool`). Helpers: `reconnect` (server restart),
`test_read_pool`, `row_changes` (rows written so far), `fail_trigger` (make
writes fail), `explain` (plans with sequential scans disabled). CI runs the
suite, the native Wine tests and the image smoke test against PostgreSQL
service containers, plus `check_sql.py`.

## 3. Concurrency model

Transactions run in parallel, so read-then-write code can lose updates or
deadlock unless it locks.

1. **Per-player serialization.** The `acting_account` middleware records the
   player of each POST request with a valid session; `database::begin` (used
   for every write transaction of a request) takes that player's
   `pg_advisory_xact_lock(account_id)`. One player's transactions run in turn;
   different players run in parallel. The lock lives in PostgreSQL, so it also
   holds across several `game` containers. It is held per transaction, not per
   request, so non-database work never blocks the player's other requests.
2. **Background, chat and battle work** has no acting account and takes
   explicit account locks (co-op and arena settlement on the room master, mail,
   party messages, coupons, inventory initialization).
3. **Shared rows.**
   - Guild content: community requests of a guild member take the guild's
     lock (`postgres::lock_guild`) after the account lock.
   - Party/raid rooms: the room row is read `FOR UPDATE` before the JSON is
     rewritten.
   - Arena: matchmaking writes both players' queue entries and swords, so
     arena requests and arena settlements take one arena queue lock
     (`postgres::lock_arena_queue`) after the account lock and run in turn; a
     queue entry that changed anyway aborts the pairing.
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
   guild raids.

Lock waits of 250 ms or more are logged as "Waited long for a lock".

Tests: `account_lock_serializes_one_account_only`,
`request_transactions_serialize_per_acting_account`,
`acting_account_middleware_prevents_lost_updates_within_one_player`,
`transaction_retries_serialization_failures`, and the concurrent claim and
purchase tests (mail, coupons, inventory, heroes, arena polls).

## 4. Schema

- `migrations/0001`–`0009`: helpers, then tables per area (core, progression,
  battle, community, services, compensation audit), gameplay triggers and
  revision triggers.
- Integers are `bigint` (flags 0/1), everything else `text`. Identity columns
  are `GENERATED BY DEFAULT`, so fixtures can keep explicit IDs.
- `sprk_compensation_campaigns` / `_grants` are written by the out-of-tree
  compensation tool; `0007` keeps them.
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
  later). `SPRK_DB_MAX_CONNECTIONS` sizes each pool, main and read alike
  (default 16, so up to 32 connections); PostgreSQL allows 100 in total, so
  keep it at 45 or below unless `max_connections` is raised.
- Sessions run in UTC with `statement_timeout` 30 s, `lock_timeout` 10 s and
  `idle_in_transaction_session_timeout` 60 s. The read pool sets
  `default_transaction_read_only`.
- Locally, `start_server.bat` starts a loopback PostgreSQL container
  (`sprk-postgres-local`, port 54320, volume `sprk-postgres-local`) unless
  `DATABASE_URL` is set.

## 6. Infrastructure

### Compose / Portainer

`docker-compose.yml` runs `postgres` (PostgreSQL 18, volume `postgres-data`,
no published port, `pg_stat_statements`), `postgres-backup` and `game` (waits
for a healthy `postgres`). On first start of the volume, the init script
(`sprk-postgres-init` under `configs:` in `docker-compose.yml`) creates the
`sprk` database and two roles: `sprk_owner` (owns the schema, runs migrations
and backups) and `sprk_app` (data access only, not a superuser). The init and
backup scripts are inline in the compose file because Portainer git stacks
cannot bind-mount files from the repository; this needs Docker Compose 2.23.1
or newer. If the volume was created without them (roles missing), create the
roles by running that script by hand in the `postgres` container.

Stack variables (long random URL-safe passwords: letters, digits, `-` and `_`):

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
`SPRK_DB_BACKUP_KEEP` files (default 14); a failed dump deletes nothing. A
production dump is about 24 MB.

For an off-host copy, mount a NAS (or other storage) on the host and set
`SPRK_DB_BACKUP_COPY_HOST_DIR` to that directory, writable by UID 999. Every
run copies the dumps it is missing there (so it catches up after a NAS outage)
and keeps the newest `SPRK_DB_BACKUP_COPY_KEEP` (default: same as
`SPRK_DB_BACKUP_KEEP`). A failed copy only logs a warning. Docker never creates
that directory: if the NAS is not mounted, `postgres-backup` does not start,
rather than writing "NAS" copies onto the local disk. For point-in-time
recovery add WAL archiving (pgBackRest or WAL-G) later. On AWS, RDS/Aurora
backups replace this.

A manual dump at any time:

```bash
docker compose exec -T postgres pg_dump -U postgres -Fc sprk > sprk-manual.dump
```

Restore:

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
after the tests. Migrations run at `game` startup, so deploys need no extra step.

## 7. Tools

| Tool | Use |
| --- | --- |
| `gm.py maxheroes` | Clones a PostgreSQL database (`--source-url`, `--output-db`) and edits the clone. Needs `psycopg`. The other `gm.py` commands use the GM HTTP API. |
| `scripts/server/db_diagnostics.py` | `--database-url`: sizes, dead rows, cache hit rate, `pg_stat_statements`, long transactions, lock waits, plans. `--log`: slow requests and lock waits from server logs. |
| `scripts/automation/client_automation.py` | Each run gets its own `sprk_auto_*` database (`--seed-from` clones an existing one); `assert-db` runs read-only. Needs `psycopg`. |
| `scripts/server/start_local_server.ps1` | Local PostgreSQL container for `start_server.bat`. |
| `scripts/server/check_sql.py` | `PREPARE` sweep of all literal SQL (CI). |

## 8. Later

- **Tuning.** Hot JSON fields to columns or `jsonb`, `timestamptz`, `boolean`
  flags, `synchronous_commit = off` for low-value writes, batching hot
  counters, autovacuum settings. Measure first with the diagnostics report.
  The compatibility layer that remains (`crate::db`, the `sprk_*` helpers) is
  listed in [postgresql-cleanup.md](postgresql-cleanup.md).
- **Several `game` containers.** The database is no longer the blocker (its
  locks are database-wide); in-process state is: sessions, chat, co-op/battle
  rooms and the Wine battle worker. Options: sticky routing so a room lives on
  one instance, plus Valkey/Redis for sessions and pub/sub. Add PgBouncer then.
- **AWS.** RDS or Aurora PostgreSQL needs only new URLs, RDS Proxy as the
  pooler and `DATABASE_READ_URL` pointing at a reader. Distributed PostgreSQL
  (Aurora DSQL and others) needs the retry wrapper on request transactions and
  a check of its feature limits (advisory locks, triggers) against the schema.
