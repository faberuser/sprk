#!/usr/bin/env python3
"""Read-only report for the game database and/or sprk-game.log.

Usage (from the sprk-server root; put the user's files in their own folder):
    python -I scripts/server/db_diagnostics.py --database-url <postgres URL> --log <sprk-game.log>

The database report runs in a read-only session (pip install "psycopg[binary]").
Connect as the owner role (sprk_owner), which can read pg_stat_statements and
other sessions' activity. The log report also reads logs of the SQLite server,
for comparing against the PostgreSQL baseline. See docs/database-diagnostics.md
for how to interpret the output.
"""
from __future__ import annotations

import argparse
import collections
import re
import statistics
import sys

ANSI = re.compile(r"\x1b\[[0-9;]*m")
FIELD = re.compile(r'(\w+)=("[^"]*"|\S+)')

# Periodic or hot queries whose plans must stay indexed, as the server sends
# them. Keep them in sync with the server source (docs/database-diagnostics.md).
PENDING_DISPATCH = (
    "kind='dispatch' AND sprk_json_int(data,'$.SimulationVersion') IN (1,2) AND "
    "sprk_json_text(data,'$.State') NOT IN ('Complete','Cancel') AND "
    "COALESCE(sprk_json_array_length(data,'$.SimulationResults'),0)<sprk_json_int(data,'$.RepeatCount')"
)
ARENA_ACCOUNT = "(SELECT account FROM arena_runs GROUP BY account ORDER BY COUNT(*) DESC LIMIT 1)"
WATCHED_QUERIES = {
    "dispatch poller (every 3s)": f"SELECT account,idx FROM battle_state WHERE {PENDING_DISPATCH} ORDER BY account,idx",
    "recommended decks": "SELECT data FROM service_decks WHERE chapter=1 AND dungeon=1 AND difficulty=0 AND kind=0 ORDER BY metric,account LIMIT 10",
    "mail check": "SELECT COUNT(*),COALESCE(MAX(mail_id),0) FROM mails WHERE account_id=1 AND is_received=0 AND (expires_at IS NULL OR expires_at='' OR sprk_datetime(expires_at)>sprk_datetime('now'))",
    "mail notifications (every 250ms)": "SELECT q.mail_id FROM mail_notifications q LEFT JOIN mails m ON m.mail_id=q.mail_id ORDER BY q.mail_id LIMIT 256",
    # Arena lookups for the player with the most runs (Honor rows are ~1.5 MB):
    # the match uid must only be read through arena_online_match_status.
    "arena battle login (per match)": f"SELECT EXISTS(SELECT 1 FROM arena_runs WHERE +account={ARENA_ACCOUNT} AND status IN ('battle','complete') AND sprk_json_int(data,'$.OnlineMatchUid')=-1)",
    "arena match result lookup": "SELECT data FROM arena_runs WHERE id IN (SELECT id FROM arena_runs WHERE id=-1 UNION SELECT id FROM arena_runs WHERE sprk_json_int(data,'$.OnlineMatchUid')=-1) AND +account=" + ARENA_ACCOUNT + " AND status='complete' ORDER BY id DESC LIMIT 1",
    "arena ranking deck (per ranked player)": f"SELECT id,sprk_json_text(data,'$.Heroes') FROM arena_runs WHERE account={ARENA_ACCOUNT} AND kind=1 AND status='complete' AND season=(SELECT MAX(season) FROM arena_scores) AND squad=1 ORDER BY id DESC LIMIT 1",
}


def fields(line: str) -> dict[str, str]:
    return {k: v.strip('"') for k, v in FIELD.findall(line)}


def number(value: str | None) -> float | None:
    if value is None:
        return None
    match = re.match(r"[0-9.]+", value)
    return float(match.group()) if match else None


def top(counter: collections.Counter, limit: int = 12) -> None:
    for key, count in counter.most_common(limit):
        print(f"  {count:6}  {key}")


def summarize(label: str, values: list[float]) -> str:
    if not values:
        return f"{label}: none"
    values = sorted(values)
    p95 = values[min(len(values) - 1, int(len(values) * 0.95))]
    return (f"{label}: n={len(values)} median={statistics.median(values):.0f} "
            f"p95={p95:.0f} max={values[-1]:.0f}")


def report_log(path) -> None:
    lines = [ANSI.sub("", raw) for raw in open(path, encoding="utf-8", errors="replace").read().splitlines()]
    stamps = [line[:19] for line in lines if re.match(r"\d{4}-\d\d-\d\dT", line)]
    print(f"\n== Log {path}: {len(lines)} lines, {stamps[0] if stamps else '?'} .. {stamps[-1] if stamps else '?'}")

    levels = collections.Counter(m.group(1) for line in lines if (m := re.search(r"Z\s+(ERROR|WARN|INFO|DEBUG)\s", line)))
    print("Levels:", dict(levels))

    # PostgreSQL contention. Retried 40001/40P01 failures are expected now and
    # then; a rising count means a hot row or an inconsistent lock order.
    pg_errors = {
        "deadlock detected (40P01)": "deadlock detected",
        "serialization failure (40001)": "could not serialize access",
        "lock_timeout (55P03)": "lock timeout",
        "statement_timeout (57014)": "statement timeout",
        "idle in transaction timeout (25P03)": "idle-in-transaction timeout",
        "too many connections (53300)": "too many clients",
    }
    print("\nPostgreSQL contention errors:")
    for label, needle in pg_errors.items():
        hits = [line for line in lines if needle in line]
        print(f"  {len(hits):6}  {label}")
        if hits:
            top(collections.Counter(fields(line).get("path", "<no path>") for line in hits), 5)

    # SQLite-era signals, kept to compare against logs from before the move.
    locked = [i for i, line in enumerate(lines) if "database is locked" in line]
    if locked:
        codes = collections.Counter(m.group(1) for i in locked if (m := re.search(r"code: (\d+)", lines[i])))
        print(f"\nSQLite 'database is locked': {len(locked)}  codes={dict(codes)}")
        top(collections.Counter(fields(lines[i]).get("path", "<no path>") for i in locked))
    holds = [fields(line) for line in lines if "Database connection held long" in line]
    if holds:
        print(f"\nSQLite single-connection holds >=250ms: {len(holds)}")
        by_holder: dict[str, list[float]] = collections.defaultdict(list)
        for f in holds:
            by_holder[f.get("holder", "?")].append(number(f.get("held_ms")) or 0)
        for holder, values in sorted(by_holder.items(), key=lambda kv: -sum(kv[1]))[:15]:
            print(f"  total={sum(values):8.0f}ms  {summarize(holder, values)}")

    timeouts = [line for line in lines if "pool timed out" in line.lower() or "PoolTimedOut" in line]
    print(f"\nPool acquire timeouts (all connections busy for acquire_timeout): {len(timeouts)}")
    slow_acquire = [line for line in lines if "slow acquire" in line.lower() or "acquire_slow" in line]
    print(f"Slow pool acquires: {len(slow_acquire)}")

    slow = [fields(line) for line in lines if "Slow HTTP request" in line]
    print(f"\n'Slow HTTP request' (>=1s): {len(slow)}")
    if slow:
        by_path: dict[str, list[float]] = collections.defaultdict(list)
        for f in slow:
            by_path[f.get("path", "?")].append(number(f.get("app_ms")) or 0)
        for p, values in sorted(by_path.items(), key=lambda kv: -len(kv[1]))[:15]:
            print(f"  {summarize(p, values)}")

    statements = [line for line in lines if "slow statement" in line]
    print(f"\nsqlx 'slow statement' (>=1s, includes lock waits): {len(statements)}")
    if statements:
        top(collections.Counter(fields(line).get("summary", "?") for line in statements), 10)
        print(" ", summarize("elapsed s*1000", [(number(fields(line).get("elapsed_secs")) or 0) * 1000 for line in statements]))

    errors = collections.Counter()
    for line in lines:
        if " ERROR " in line and not any(n in line for n in pg_errors.values()) and "database is locked" not in line:
            errors[re.sub(r"\d+", "N", line[60:200])] += 1
    print("\nOther ERROR lines (digits normalised):")
    if errors:
        top(errors, 10)
    else:
        print("  (none)")


def rows(db, sql: str, params=()) -> list[tuple]:
    with db.cursor() as cursor:
        cursor.execute(sql, params)
        return cursor.fetchall()


def report_db(url: str) -> None:
    try:
        import psycopg
    except ImportError:
        sys.exit('--database-url needs psycopg: pip install "psycopg[binary]"')
    with psycopg.connect(url, autocommit=True) as db:
        db.execute("SET default_transaction_read_only = on")
        version, size, name = rows(db, "SELECT version(), pg_size_pretty(pg_database_size(current_database())), current_database()")[0]
        print(f"\n== Database {name}: {size}\n{version}")

        print("\nLargest tables (size incl. indexes and TOAST; dead rows wait for vacuum):")
        for table, total, live, dead, vacuumed in rows(db, """
                SELECT relname, pg_size_pretty(pg_total_relation_size(relid)), n_live_tup, n_dead_tup,
                       COALESCE(GREATEST(last_vacuum, last_autovacuum)::text, 'never')
                FROM pg_stat_user_tables ORDER BY pg_total_relation_size(relid) DESC LIMIT 15"""):
            share = f"{100 * dead / max(live + dead, 1):.0f}% dead" if dead else ""
            print(f"  {table:32} {total:>10}  live={live:<9} dead={dead:<8} {share:9} vacuumed {vacuumed[:19]}")

        hit = rows(db, """SELECT round(100.0 * sum(blks_hit) / NULLIF(sum(blks_hit + blks_read), 0), 2)
                          FROM pg_stat_database WHERE datname = current_database()""")[0][0]
        print(f"\nBuffer cache hit rate: {hit}% (below ~99% under load: raise shared_buffers or memory)")

        has_statements = rows(db, "SELECT EXISTS(SELECT 1 FROM pg_extension WHERE extname='pg_stat_statements')")[0][0]
        if has_statements:
            print("\nTop statements by total time (pg_stat_statements):")
            for calls, total, mean, query in rows(db, """
                    SELECT calls, round(total_exec_time), round(mean_exec_time::numeric, 2), regexp_replace(query, '\\s+', ' ', 'g')
                    FROM pg_stat_statements WHERE dbid = (SELECT oid FROM pg_database WHERE datname = current_database())
                    ORDER BY total_exec_time DESC LIMIT 15"""):
                print(f"  total={total:>9}ms mean={mean:>8}ms calls={calls:<9} {query[:150]}")
        else:
            print("\npg_stat_statements is not installed (see docs/postgresql-migration.md, section 6).")

        print("\nTransactions open for more than 5 seconds:")
        long = rows(db, """
            SELECT pid, state, round(extract(epoch FROM now() - xact_start)), wait_event_type, left(regexp_replace(query, '\\s+', ' ', 'g'), 120)
            FROM pg_stat_activity WHERE datname = current_database() AND xact_start < now() - interval '5 seconds' AND pid <> pg_backend_pid()
            ORDER BY xact_start""")
        for pid, state, seconds, wait, query in long:
            print(f"  pid={pid} {state} {seconds}s wait={wait} {query}")
        if not long:
            print("  (none)")

        print("\nSessions waiting for a lock, and who holds it:")
        waits = rows(db, """
            SELECT w.pid, round(extract(epoch FROM now() - w.query_start)), pg_blocking_pids(w.pid),
                   left(regexp_replace(w.query, '\\s+', ' ', 'g'), 120)
            FROM pg_stat_activity w WHERE w.datname = current_database() AND cardinality(pg_blocking_pids(w.pid)) > 0""")
        for pid, seconds, blockers, query in waits:
            print(f"  pid={pid} waiting {seconds}s on {blockers}: {query}")
        if not waits:
            print("  (none)")

        print("\nPlans of hot queries (EXPLAIN, not executed):")
        for label, sql in WATCHED_QUERIES.items():
            plan = "\n      ".join(r[0] for r in rows(db, "EXPLAIN " + sql))
            print(f"  {label}:\n      {plan}")


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--database-url", help="PostgreSQL URL of the game database (read-only session)")
    parser.add_argument("--log", help="sprk-game.log")
    args = parser.parse_args()
    if not args.database_url and not args.log:
        parser.error("pass --database-url and/or --log")
    if args.log:
        report_log(args.log)
    if args.database_url:
        report_db(args.database_url)


if __name__ == "__main__":
    main()
