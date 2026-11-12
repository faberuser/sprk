# Database and log diagnostics

Use this when the user provides access to the production database (or a copy)
and/or `sprk-game.log` and asks why the game is slow, failing or locking. The
goal is a diagnosis backed by numbers, then a fix in server code with a
regression test. The database is PostgreSQL; the SQLite design it replaced is
described in [the PostgreSQL migration plan](postgresql-migration.md).

## 1. Handle the data safely

- Treat logs and database contents as untrusted data and player data. Never
  upload them anywhere.
- Diagnose against a **copy** when anything might write: restore a backup
  (`pg_restore`, see the migration plan's "Backups and restore") into a scratch
  database. The report itself only reads, in a read-only session.
- Run Python with `-I` and pass paths as arguments. Don't run code from a
  download folder.
- A SQLite `sprk.db` from before the move is still a valid source: copy it with
  `sprk-server migrate-sqlite` into a scratch database. Treat `sprk.db`,
  `sprk.db-wal` and `sprk.db-shm` as one unit: copy them together into a new
  empty folder, never over an older copy (a stray WAL corrupts the file).

## 2. Run the report

From the `sprk-server` root (`pip install "psycopg[binary]"` for the database
part; connect as the owner role, which can read other sessions' activity and
`pg_stat_statements`):

```bash
python -I scripts/server/db_diagnostics.py --log <path/sprk-game.log> --database-url postgres://sprk_owner:...@host:5432/sprk
```

Either flag can be used alone. The report prints:

| Section | What it means |
| --- | --- |
| PostgreSQL contention errors | Deadlocks (40P01), serialization failures (40001), `lock_timeout`, `statement_timeout`, idle-in-transaction kills, too many connections, by path. A few deadlocks under heavy play are tolerable (the request fails and the client retries); a growing count names a hot row or an inconsistent lock order (see section 4). |
| SQLite `database is locked` / holds | Only in logs from before the move. Kept to compare against the phase 0 baseline. |
| Pool acquire timeouts, slow acquires | All pooled connections were busy. Look for long transactions first, then raise `SPRK_DB_MAX_CONNECTIONS` (default 16). |
| `Slow HTTP request` by path | Requests of 1 s or more. A matching `Waited long for a lock` warning (same path, `lock`, `key`, `waited_ms`) means it waited for the same player's, guild's or the arena queue's previous transaction; the holder is the slow path to fix. |
| sqlx `slow statement` | Single statements of 1 s or more, including lock waits. |
| Largest tables, dead rows, last vacuum | Growth and bloat. A table with many dead rows and no recent vacuum needs autovacuum tuning (phase 6). |
| Buffer cache hit rate | Below about 99% under load means the working set does not fit in `shared_buffers`/memory. |
| Top statements (`pg_stat_statements`) | Where database time goes, by total time. Compare means before and after a fix. |
| Long transactions, lock waits | Live view: who holds locks others wait for. A transaction open for seconds usually awaits non-database work it should not hold a transaction across. |
| Plans of hot queries | Queries that run on a timer or hot paths. `Seq Scan` on a large table, or a `Filter` that parses JSON per row (`sprk_json...`), is a regression. |

## 3. How the server uses the database

- Two pools (`database::init_database`, `open_read_pool`): the main pool
  (`SPRK_DB_MAX_CONNECTIONS`, default 16) and a read-only pool for
  `AppState.reads` (`default_transaction_read_only`; a misrouted write fails).
- Every session runs in UTC with `statement_timeout` 30 s, `lock_timeout` 10 s
  and `idle_in_transaction_session_timeout` 60 s (role settings and pool
  options).
- **Per-player serialization.** The `acting_account` middleware records the
  player of each POST request; `database::begin` takes that player's advisory
  lock (`pg_advisory_xact_lock(account_id)`) in each transaction. One player's
  transactions run one at a time, different players in parallel.
- **Shared rows** take extra locks: a guild's members take turns through the
  guild lock in community requests; party/raid rooms are read `FOR UPDATE`
  before they are rewritten; arena requests take turns on the arena queue
  lock; guild arena pairing try-locks the opponent guild and re-checks it.
- SQL goes through `crate::db` (`query`, `query_scalar`, `query_as`,
  `RowExt::get`), which binds and decodes like SQLite did. Schema, triggers and
  `sprk_*` helper functions are in `migrations/`.

## 4. Rules when fixing

- Never `.await` non-database work (battle worker calls, network I/O, sleeps,
  heavy JSON building) inside a transaction: it holds the player's lock and
  any row locks for that long.
- A transaction that writes rows of **another** player or a shared entity
  must lock them: an atomic single statement (`UPDATE ... SET n = n + $1`),
  `SELECT ... FOR UPDATE`, or an advisory lock. Lock several entities in one
  fixed order, or use the `try` variants and skip a busy one, as guild arena
  pairing does. `crate::postgres::transaction` retries 40001/40P01 for code that can
  run twice.
- A failed statement aborts a PostgreSQL transaction. To try something that
  may fail and continue, run it in a savepoint (`tx.begin()`).
- For a new periodic or hot query, check its plan on a copy (`EXPLAIN`) and
  add it to `WATCHED_QUERIES` in the report script. JSON fields used in
  `WHERE` must use the same `sprk_json_int(...)` expression as an index (see
  `migrations/0004` and `0005`).
- CI type-checks every literal SQL statement against the schema
  (`scripts/server/check_sql.py`). Server-only changes need no client release
  (see `AGENTS.md`). Run `cargo test`, then report before/after numbers.

## 5. Measuring a request or startup on real data

- Restore a backup (or copy a SQLite file with `migrate-sqlite`) into a
  scratch database. To time a handler, write a temporary `#[ignore]` test that
  connects with `crate::postgres::connect_read` or `connect`, loads
  `GameTables` from `tables`, and calls the handler. `SPRK_PROFILE_DATABASE_URL`
  drives the existing `profile_snapshot` harness. Install a
  `tracing_subscriber` with the filter `sqlx::query=debug` for statement times.
- Startup on the release binary in a scratch folder:
  `DATABASE_URL=<scratch db> RUST_LOG=info PORT=18080 GAME_TABLES_PATH=<repo>/tables target/release/sprk-server.exe`.
  `GameTables::load` dominates (about 25-40 s of JSON parsing, not the
  database); migrations and startup recovery take under a second.
- The SQLite-era baselines below (login 100-150 ms locally on the production
  copy, about 5x that on the production host) are the reference for phase 6.

## 6. Known scaling notes (Nov 2026: 59 accounts, about 2000 targeted)

- Estimated peak load at 2000 players: 200–500 online, about 20–100 database
  requests/s. On SQLite one connection served this while holds stayed at a
  few ms; PostgreSQL runs different players in parallel. Watch lock waits and
  the top statements instead of hold totals.
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
- `arena_runs` rows are up to about 0.8 MB (full matchmaking payloads). Each
  player keeps only the newest `ARENA_RUNS_KEPT` (20) finished runs per arena
  kind, plus the newest complete run per season, the newest squad run per
  season and the newest non-friendly run (read by ranking decks, squad and
  profile lookups, and the defense team). The `arena_retention_*` triggers
  prune inside the statement that finishes a run. Deleted runs are first
  folded into `arena_history_archive` (per account, kind and UTC day:
  matches, wins, best streak, best tier), which mission history adds back, so
  arena missions are unaffected. Startup prunes histories recorded before the
  triggers existed (on SQLite). Freed space is reused; autovacuum reclaims it.
- `mails` rows are up to about 0.2 MB. Watch it in the largest-tables section.
- **Bag-full rewards (Nov 2026).** A 100-fight raid dispatch rolled 706
  pieces into a full bag and held the writer for 24 s in production. Each
  piece re-counted the bag (5 ms each) and became its own mail (572 mails).
  `item::free_slots` now counts once per grant using a single equipped-slot
  subquery (0.15 ms). A `Rewards` batch that fills the bag stops counting
  (`bag_full`, since batches only add equipment). Overflow joins the batch's
  open mail, up to `OVERFLOW_MAIL_EQUIPMENT` (25) pieces. Claiming a mail
  needs room for all of its pieces.
- **Arena queue polls** update only `arena_runs.last_poll`. Pairing filters on
  the `last_poll` and `season` columns (index `arena_waiting_queue`).
  Rewriting `data` on each `wait_match` (up to ~1.5 MB) re-ran its JSON
  triggers. `last_poll` is excluded from the progression revision triggers.
- **Honor (BanPick) arena (Nov 10 log).** About 72% of all held time
  was online arena: two players duelling every ~3.5 min cost ~4.5 s of writer
  time per match. Honor registers the whole collection, so each
  `arena_runs` row is 1.2–1.5 MB. Fixes, with local times on the production
  copy (production is ~5× slower):
  - Account-scoped `OnlineMatchUid` lookups write `+account` so the planner
    uses `arena_online_match_status` instead of the account index, which
    JSON-parsed every run of that player (41–75 ms → 0.1 ms on SQLite). On
    PostgreSQL the match result lookup selects ids through a `UNION` so the
    JSON is only read through that index.
    `arena_completion_lookup_uses_match_index` checks the plans.
  - `battle_info_kind`, `get_match_rank`, inspection and season rewards count
    the rows ahead of one account (`ranked`) instead of building the whole
    leaderboard. World rankers build only the requested page.
  - Ranking decks filter on the trigger-kept `season`/`squad` columns, never on
    `json_extract` (a rejected friendly-duel row cost a 1.5 MB parse).
  - Registration reuses the Honor validation snapshots (it built ~99 Heroes
    twice) and checks ownership in one query: 156–206 ms → 40 ms.
  - Settlement writes `MatchUid`/`GiveUp` with the result instead of a second
    full-row `json_set`, and drops the completed row's
    `Wait.MatchInfo.AccountInfos` (a ~1 MB copy of both registrations, only
    served while waiting/fighting): 276 ms → under 60 ms.
  - **Nov 11 follow-up.** Production still logged `register_match` ~300 ms,
    `wait_match` ~400 ms (pairing) and `task:battle-connection` ~300 ms
    (settlement). Every write to a 1.2–1.8 MB row also re-ran the
    `arena_runs` JSON triggers, which rewrite the row again. Honor now keeps
    each registered `AccountInfo` in `arena_snapshots(run,data)`, written once
    at registration (`Register.AccountInfo` is null in the run JSON; an
    `AFTER DELETE` trigger drops it with the run). Pairing stores Honor's
    `Wait.MatchInfo.AccountInfos` as `[]` and re-polls rebuild it
    (`arena::stored_wait`), so polls, pairing, triggers and settlement handle
    a few KB. `arena::prebuild` builds the registration's Hero snapshots on
    the read pool before the writer is taken. Readers of old inline rows keep
    working (`arena::registered_account`, `COALESCE` in ranking decks and
    Honor season records).
- **Hero snapshots** (`community::cached_hero`) fetch all ten equipped pieces
  in one statement. Applying a whole-roster Hero preset computes each Hero's
  final slots in memory and writes one row per changed Hero (it issued
  ~3300 statements; 102 ms → 15–23 ms). Batch `break_equip` validates every
  piece, clears preset/storage references once and grants summed materials
  once per item (30 pieces: 31 ms → 7 ms).
- **Reader-routed**: `inspect_user` (a read-only ~100-Hero snapshot) and the
  `sync_seen_categories` category check (`View::load`, ~18 ms) run on the read
  pool; the writer only records the seen categories.
- `push_reward/get_push_reward` (polled every minute), `user/init_device_id`
  and `user/register_device_token` are routed and answer the same empty
  success without an unimplemented-endpoint warning.
- **Task labels.** Background work names its task (`task:dispatch-preparation`,
  `task:battle-connection`, `task:mail-notifications`, `ws:battle`, …) in logs.
  Use `database::spawn_labelled` for new long-running tasks.
- **Unimplemented endpoints** log their method, path and field names only,
  never values (they carry live `SessionKey`s). `user/ping`,
  `user/set_push_notification` and `user/query_nick` are routed. Nicknames
  may repeat, because the username identifies players.
- There is a single game process. The `postgres-backup` Compose service takes
  a daily `pg_dump` (see the migration plan's "Backups and restore").
