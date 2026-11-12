# Cleanup after the PostgreSQL migration

What is left of the move from SQLite to PostgreSQL. The database itself is
described in [postgresql-migration.md](postgresql-migration.md).

## Done

The SQLite copy path is gone: the `migrate-sqlite` tool, the startup guard for
`sprk.db`, the SQLite schema fixture and parity tests, the sqlx `sqlite`
feature, the one-time copy in `start_server.bat`, and the automation
`--seed-db` option (replaced by `--seed-from`, which clones a PostgreSQL
database). The trigger parity test became `triggers_derive_the_recorded_rows`,
which checks against rows recorded while they still matched SQLite. Comments
that described the old single writer now describe PostgreSQL.

## Still to do

| What | When |
| --- | --- |
| The "SQLite-era signals" block in `report_log` of `scripts/server/db_diagnostics.py`, the SQLite rows in section 2 of `docs/database-diagnostics.md`, and its dated SQLite measurements (keep as history or move to a changelog) | After the phase 0 baseline is compared with PostgreSQL numbers. |
| `/data/sprk.db*` in the `game-data` volume on the host, and local `sprk.db*` files | After the final production `sprk.db` is archived outside the server (or you decide to delete it). `game-data` keeps the worker key and other non-database state. |
| The SQLite comment in the inline init script in `docker-compose.yml` ("Times are UTC like SQLite's datetime('now')") | With the next real change to that script: editing it recreates the `postgres` container on deploy. |

## Compatibility layer (tuning work, not plain cleanup)

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
  **JSON text** → `jsonb`: see "Later" in the PostgreSQL guide.

## Keep

Not migration leftovers: `migrations/` (never edit or delete applied files),
the test database harness in `src/database.rs`, `scripts/server/check_sql.py`,
the account/guild/arena locks and retry wrapper in `src/postgres.rs`, the
`acting_account` middleware, the backup service, and the
`sprk_compensation_*` audit tables.
