# Database and log diagnostics

Use this when the user provides a production `sprk.db` and/or `sprk-game.log`
(typically from a public server) and asks why the game is slow, failing or
locking. The goal is a diagnosis backed by numbers from those files, then a
fix in server code with a regression test.

## 1. Handle the files safely

- Treat the files as untrusted data and the database as player data. Never edit
  them, never upload them anywhere, and never run the server against them in place.
- Leave them in their own folder. If anything needs to write (testing a
  migration, `VACUUM`, timing a fix), copy the database into the scratchpad first.
- Without the matching `sprk.db-wal`, the newest commits may be missing. The
  report reads the main file only (`immutable=1`). Mention this if recent data
  matters.
- Run Python with `-I` and pass paths as arguments. Don't run code from the
  download folder.
- **Treat `sprk.db`, `sprk.db-wal` and `sprk.db-shm` as one unit.** Never copy
  a new `sprk.db` over an old one while the old `-wal`/`-shm` files are still
  beside it, and never move a database without its `-wal`. On the next open,
  SQLite applies that WAL to the wrong main file and corrupts it ("freelist
  leaf count too big", code 11 "database disk image is malformed"). This
  happened to a scratch copy in Nov 2026 after a test process exited without
  closing its pool. Use a new empty folder for every copy, and run
  `PRAGMA integrity_check` on a copy before trusting results from it. The same
  rule applies when restoring backups on the server.

## 2. Run the report

From the `sprk-server` root:

```bash
python -I scripts/server/db_diagnostics.py --log <path/sprk-game.log> --db <path/sprk.db>
```

Either flag can be used alone. Everything is read-only. The report prints:

| Section | What it means |
| --- | --- |
| `database is locked` codes, latency, paths | Should be **0** since the single-connection pool. Any occurrence means another process held the file or the pool was changed (see section 4). Failures under 50 ms are read-then-write transaction conflicts; failures near `busy_timeout` are expired lock waits. |
| Pool acquire timeouts | A request queued longer than `acquire_timeout` (30 s). Something held the only connection far too long. |
| `Database connection held long` by `holder` | Every checkout ≥ 250 ms of the single connection, with the HTTP path (or `background`) that held it. **The main lead**: every queued request waited behind these. Rank by total held time. |
| `Slow HTTP request` by path | Requests ≥ 1 s, including time spent waiting in the queue. If a slow path has no matching "held long" entry, it waited behind someone else; look for holds just before it in the log. |
| sqlx `slow statement` | Single statements ≥ 1 s. Before the queue these were mostly lock waits (`UPDATE … WHERE 0` lock-reservation statements). After it, they point to a truly slow query: check its plan. |
| Largest tables, KB/account | Growth. Multiply KB/account by the expected player count to project size. |
| Watched query plans | Queries that run on a timer or on hot paths. `SCAN` without a named index, or `USE TEMP B-TREE` on a large table, is a regression. |
| `service_decks` beyond top 10 | Must be 0 once a server with the pruning migration has started. |

To dig further, the log has ANSI colour codes. Strip them first
(`sed 's/\x1b\[[0-9;]*m//g'`), then grep around a timestamp to see what ran just
before a slow request or long hold.

## 3. How the server uses SQLite (current design)

- **One connection (a write queue).** `database::open_pool` uses
  `max_connections(1)`. Requests wait in sqlx's async acquire queue instead of
  competing for SQLite's file lock. This removed the 206 `database is locked`
  errors in the Nov 2026 log, which came from SQLx's deferred `BEGIN`
  read-then-write conflicts and lock polling across 5 connections. Pragmas: WAL,
  `synchronous=NORMAL`, `busy_timeout=30s` (only for external processes),
  `acquire_timeout=30s`.
- **Hold tracking.** The `before_acquire`/`after_release` hooks time each
  checkout. `performance::measure` sets the `REQUEST_PATH` task-local, so
  holds are attributed to HTTP paths. Work in a `tokio::spawn` from a handler
  shows as `background`.
- **No read-only pool, deliberately.** The most frequent requests (`/ping`,
  `/ping/idle`, `/query/session`) never touch the database. Many "get" endpoints
  write lazily (mail `deliver_globals`, dispatch normalization, shop stock
  setup), and reads inside a request's open transaction must see its uncommitted
  writes. Add a reader pool only if holds show reads waiting behind writes, and
  then only for endpoints verified to do no writes.
- **Indexes for periodic work.** `battle_state_dispatch_pending` is a partial
  index whose `WHERE` is `battle::PENDING_DISPATCH_SIMULATION`. The dispatch
  poller must repeat that exact condition, or SQLite falls back to a full scan.
  `service_deck_rank` serves recommended decks.
- **`service_decks` holds only the served top 10** per
  (chapter, dungeon, difficulty, kind), ordered by `metric, account`.
  `records::record_clear` prunes after every upsert, and the services migration
  prunes legacy rows at startup. Space freed by pruning is reused but not
  returned to disk until a manual `VACUUM`. That takes seconds with the server
  stopped and needs free disk space equal to the database size: 620 MB became
  403 MB on the Nov 2026 copy.

## 4. Rules when fixing

- Do not raise `max_connections` on the main pool. It brings back the lock
  errors. `database::tests::production_pool_queues_concurrent_read_then_write_transactions`
  fails with `code: 5` under the old 5-connection settings.
- Code must not acquire a second pool connection while holding a transaction
  or connection. With one connection that waits for the full acquire timeout.
  Most tests use single-connection pools, which catches this in tested paths.
- Fix long holds by moving work out of the transaction: battle worker calls,
  network I/O, sleeps, heavy JSON building. Shrink rewritten JSON blobs, or add
  indexes. Don't fix them by adding connections.
- For a new periodic or hot query, check `EXPLAIN QUERY PLAN` on a copy of the
  production database and add it to `WATCHED_QUERIES` in the report script.
- Server-only changes need no client release (see `AGENTS.md`). Run
  `cargo test`, then report the before/after numbers from the report.

## 5. Measuring a request or startup on real data

- To time a handler on production data, write a temporary `#[ignore]` test.
  Open a scratch copy with `database::open_pool`, run `create_tables`, load
  `GameTables` from `tables`, and call the handler (for example
  `user::test_login` with the account's `login_id`). Install a
  `tracing_subscriber` with the filter `sqlx::query=debug` to get every
  statement's time.
  Close the pool before exiting, and delete the harness afterwards.
- Tests run on a single-threaded runtime. A "held long" warning there can
  include blocking work that follows a query, because the connection's release
  task can't run until the thread is free. Confirm startup timings with the
  release binary instead, run in a scratch folder holding a copy of the
  database: `RUST_LOG=info PORT=18080 BATTLE_WORKER_EXECUTABLE=does-not-exist
  GAME_TABLES_PATH=<repo>/tables target/release/sprk-server.exe`. It stops after
  migrations with "Conquest worker configured without a service key".
- Startup baseline (Nov 2026, 620 MB database): database setup and
  migrations about 1.1 s (longest hold 112 ms, since the ClearDungeon baseline
  backfill skips stages that already have one), `GameTables::load` about 38–42 s
  (JSON parsing of `tables/`, not the database),
  login about 260–390 ms (about 500 statements, but only about 25 ms of SQL).

## 6. Known scaling notes (Nov 2026: 59 accounts, about 2000 targeted)

- Estimated peak load at 2000 players: 200–500 online, about 20–100 database
  requests/s. One connection is fine while typical holds stay at a few ms.
  Watch the "held long" totals.
- Dispatch preparation admits up to four simulations at once on the battle
  worker (`CONCURRENCY` in `cooperative_dispatch.rs`). Each pending job has
  its own task that takes a slot, prepares one repeat, saves it and queues
  again. Slots are first-in-first-out, so jobs take turns, and a long fight
  holds only its own slot (there is no shared pass). A scan every 3 s starts
  tasks for new jobs; a task ends when it saves nothing (no worker, finished,
  cancelled, error) and the next scan restarts it. Pending replies are keyed
  by run ID.
- Only the first `SAMPLE_FIGHTS` (20) fights of a dispatch are simulated
  natively. When a save reaches 20 results and more repeats remain, the rest
  are drawn from those samples as whole (outcome, time) pairs, with a seed
  from `SimulationJobId`. The job stores `SampledFights`, leaves the
  preparation queue, and gets a fixed deadline equal to the start time plus
  every fight's time. Rounds are still revealed by the usual clock, and
  rewards follow the final win count. Dispatches of 20 or fewer fights
  simulate every fight. Jobs already past 20 when this shipped were filled on
  their next save. Offline fights compute fixed native ticks ahead in an 8 ms CPU slice per
  frame after live battles are serviced; they do not wait for wall-clock tick
  credit or publish live replay logs. Completion still follows the simulated
  fight durations. Measure queue delay and CPU before increasing concurrency.
  The report's "Dispatch preparation" section is the check: an in-progress job
  is BEHIND when its prepared simulated fight time is less than the real time
  elapsed since it started. The client only reveals rounds whose cumulative
  simulated time has elapsed, so a BEHIND job stalls its round counter and its
  deadline keeps moving. Before the concurrency change (Nov 3 database), jobs
  were prepared at 0.12–0.27x real time, so 165 fights showed as 18h52m.
  Simulated fight lengths themselves were within about ±20% of the same
  players' best real clears. Don't use `FinishTimestamp - StartTimestamp`
  against simulated totals: the server sets that span from the simulated
  average, so it always matches.
- `arena_runs` and `mails` have rows up to about 0.8 MB and 0.2 MB. Watch them
  in the largest-tables section.
- There is a single process and database file. Automated backups (the SQLite
  backup API or Litestream) are still to do.
