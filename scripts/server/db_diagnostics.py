#!/usr/bin/env python3
"""Read-only report for a production sprk.db and/or sprk-game.log.

Usage (from the sprk-server root; put the user's files in their own folder):
    python -I scripts/server/db_diagnostics.py --db <copy/of/sprk.db> --log <sprk-game.log>

The database is opened with ``mode=ro&immutable=1``: nothing is written, and a
-wal file next to it is ignored (ask for the server to be stopped, or for the
-wal/-shm files too, when the newest commits matter). See
docs/database-diagnostics.md for how to interpret the output.
"""
from __future__ import annotations

import argparse
import collections
import re
import sqlite3
import statistics
import sys
import time
from pathlib import Path

ANSI = re.compile(r"\x1b\[[0-9;]*m")
FIELD = re.compile(r'(\w+)=("[^"]*"|\S+)')

# Periodic or hot queries whose plans must stay indexed. Keep them in sync with
# the server source (see docs/database-diagnostics.md).
PENDING_DISPATCH = (
    "kind='dispatch' AND json_extract(data,'$.SimulationVersion') IN (1,2) AND "
    "json_extract(data,'$.State') NOT IN ('Complete','Cancel') AND "
    "COALESCE(json_array_length(json_extract(data,'$.SimulationResults')),0)"
    "<json_extract(data,'$.RepeatCount')"
)
WATCHED_QUERIES = {
    "dispatch poller (every 3s)": f"SELECT account,idx FROM battle_state WHERE {PENDING_DISPATCH} ORDER BY account,idx",
    "recommended decks": "SELECT data FROM service_decks WHERE chapter=1 AND dungeon=1 AND difficulty=0 AND kind=0 ORDER BY metric,account LIMIT 10",
    "mail check": "SELECT COUNT(*),COALESCE(MAX(mail_id),0) FROM mails WHERE account_id=1 AND is_received=0 AND (expires_at IS NULL OR expires_at='' OR datetime(expires_at)>datetime('now'))",
    "mail notifications (every 250ms)": "SELECT q.mail_id FROM mail_notifications q LEFT JOIN mails m ON m.mail_id=q.mail_id ORDER BY q.mail_id LIMIT 256",
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


def report_log(path: Path) -> None:
    lines = [ANSI.sub("", raw) for raw in path.read_text(encoding="utf-8", errors="replace").splitlines()]
    stamps = [line[:19] for line in lines if re.match(r"\d{4}-\d\d-\d\dT", line)]
    print(f"\n== Log {path.name}: {len(lines)} lines, {stamps[0] if stamps else '?'} .. {stamps[-1] if stamps else '?'}")

    levels = collections.Counter(m.group(1) for line in lines if (m := re.search(r"Z\s+(ERROR|WARN|INFO|DEBUG)\s", line)))
    print("Levels:", dict(levels))

    locked = [i for i, line in enumerate(lines) if "database is locked" in line]
    codes = collections.Counter(m.group(1) for i in locked if (m := re.search(r"code: (\d+)", lines[i])))
    print(f"\n'database is locked': {len(locked)}  codes={dict(codes)}  (5=SQLITE_BUSY, 517=BUSY_SNAPSHOT, 6=LOCKED)")
    if locked:
        latencies = []
        for i in locked:
            for line in lines[i:i + 3]:
                if (m := re.search(r"latency=(\d+) ms", line)):
                    latencies.append(float(m.group(1)))
                    break
        print(" ", summarize("failure latency ms", latencies))
        print("  <50ms = transaction upgrade conflict (busy_timeout never applied); ~busy_timeout = lock wait expired")
        top(collections.Counter(fields(lines[i]).get("path", "<no path>") for i in locked))

    timeouts = [line for line in lines if "pool timed out" in line.lower() or "PoolTimedOut" in line]
    print(f"\nPool acquire timeouts (queued > acquire_timeout): {len(timeouts)}")

    holds = [fields(line) for line in lines if "Database connection held long" in line]
    print(f"\n'Database connection held long' (>=250ms on the single connection): {len(holds)}")
    if holds:
        by_holder: dict[str, list[float]] = collections.defaultdict(list)
        for f in holds:
            by_holder[f.get("holder", "?")].append(number(f.get("held_ms")) or 0)
        for holder, values in sorted(by_holder.items(), key=lambda kv: -sum(kv[1]))[:15]:
            print(f"  total={sum(values):8.0f}ms  {summarize(holder, values)}")

    slow = [fields(line) for line in lines if "Slow HTTP request" in line]
    print(f"\n'Slow HTTP request' (>=1s incl. queue wait): {len(slow)}")
    if slow:
        by_path: dict[str, list[float]] = collections.defaultdict(list)
        for f in slow:
            by_path[f.get("path", "?")].append(number(f.get("app_ms")) or 0)
        for p, values in sorted(by_path.items(), key=lambda kv: -len(kv[1]))[:15]:
            print(f"  {summarize(p, values)}")

    statements = [line for line in lines if "slow statement" in line]
    print(f"\nsqlx 'slow statement' (>=1s, includes lock/queue waits inside SQLite): {len(statements)}")
    if statements:
        top(collections.Counter(fields(line).get("summary", "?") for line in statements), 10)
        print(" ", summarize("elapsed s*1000", [(number(fields(line).get("elapsed_secs")) or 0) * 1000 for line in statements]))

    errors = collections.Counter()
    for line in lines:
        if " ERROR " in line and "database is locked" not in line and "on_failure" not in line:
            errors[re.sub(r"\d+", "N", line[60:200])] += 1
    print("\nOther ERROR lines (digits normalised):")
    if errors:
        top(errors, 10)
    else:
        print("  (none)")


def report_db(path: Path) -> None:
    uri = path.resolve().as_uri() + "?mode=ro&immutable=1"
    db = sqlite3.connect(uri, uri=True)
    header = path.open("rb").read(20)
    size_mb = path.stat().st_size / 1e6
    print(f"\n== Database {path.name}: {size_mb:.1f} MB, SQLite {sqlite3.sqlite_version}, "
          f"journal={'WAL' if header[18:20] == b'\x02\x02' else 'rollback'}")
    for pragma in ("page_size", "page_count", "freelist_count"):
        print(f"  {pragma}={db.execute(f'PRAGMA {pragma}').fetchone()[0]}")
    wal = Path(str(path) + "-wal")
    if wal.exists():
        print(f"  WAL file present ({wal.stat().st_size / 1e6:.1f} MB) - ignored by immutable read")

    try:
        accounts = db.execute("SELECT COUNT(*) FROM accounts").fetchone()[0]
        print(f"  accounts={accounts}")
    except sqlite3.Error:
        accounts = 0

    print("\nLargest tables (bytes of column data):")
    sizes = []
    for (table,) in db.execute("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'"):
        cols = [r[1] for r in db.execute(f'PRAGMA table_info("{table}")')]
        expr = "+".join(f'COALESCE(LENGTH("{c}"),0)' for c in cols) or "0"
        total, biggest, rows = db.execute(f'SELECT SUM({expr}),MAX({expr}),COUNT(*) FROM "{table}"').fetchone()
        sizes.append((total or 0, biggest or 0, rows, table))
    for total, biggest, rows, table in sorted(sizes, reverse=True)[:12]:
        per_account = f"  {total / accounts / 1e3:8.1f} KB/account" if accounts else ""
        print(f"  {table:32} {total / 1e6:8.1f} MB  rows={rows:7}  max_row={biggest / 1e3:8.1f} KB{per_account}")

    print("\nIndexes on watched tables:")
    for table in ("service_decks", "battle_state", "mails", "mail_notifications"):
        names = [r[0] for r in db.execute("SELECT name FROM sqlite_master WHERE type='index' AND tbl_name=?", (table,))]
        print(f"  {table}: {names}")

    print("\nWatched query plans and timings (second run, warm cache):")
    for label, sql in WATCHED_QUERIES.items():
        try:
            plan = "; ".join(r[3] for r in db.execute("EXPLAIN QUERY PLAN " + sql))
            db.execute(sql).fetchall()
            start = time.perf_counter()
            rows = db.execute(sql).fetchall()
            ms = (time.perf_counter() - start) * 1000
            print(f"  {label}: {ms:.2f} ms rows={len(rows)}\n      plan: {plan}")
        except sqlite3.Error as error:
            print(f"  {label}: {error}")

    print("\nDispatch preparation (in-progress jobs): simulated fight time prepared vs real time elapsed")
    print("  BEHIND = the round counter stalls and the deadline keeps moving; healthy jobs stay ok.")
    try:
        import json
        from datetime import datetime, timezone
        snapshot = db.execute("SELECT MAX(last_login) FROM accounts").fetchone()[0]
        now = datetime.fromisoformat(snapshot).replace(tzinfo=timezone.utc).timestamp() if snapshot else time.time()
        print(f"  'now' = newest account login in the file: {snapshot} UTC")
        for account, slot, raw in db.execute("SELECT account,idx,data FROM battle_state WHERE kind='dispatch'"):
            job = json.loads(raw)
            sims = job.get("SimulationResults") or []
            if job.get("State") != "Battle" or not sims:
                continue
            prepared = sum(x.get("TimeMs", 0) for x in sims) / 1000
            elapsed = now - job.get("StartTimestamp", now)
            done = len(sims) >= job.get("RepeatCount", 0)
            speed = prepared / elapsed if elapsed > 0 else 0
            status = "ok" if done or prepared >= elapsed else f"BEHIND (preparing at {speed:.2f}x real time)"
            print(f"  acct={account} slot={slot} stage={job.get('ChapterIndex')}-{job.get('DungeonIndex')} "
                  f"prepared {len(sims)}/{job.get('RepeatCount')} = {prepared / 3600:.2f}h of fights, "
                  f"elapsed {elapsed / 3600:.2f}h: {status}")
    except (sqlite3.Error, ValueError, KeyError) as error:
        print(f"  dispatch check failed: {error}")

    try:
        over = db.execute(
            "SELECT COUNT(*) FROM (SELECT ROW_NUMBER() OVER (PARTITION BY chapter,dungeon,difficulty,kind "
            "ORDER BY metric,account) AS rank FROM service_decks) WHERE rank>10").fetchone()[0]
        print(f"\nservice_decks rows beyond the served top 10 (should be 0 after startup migration): {over}")
    except sqlite3.Error as error:
        print(f"\nservice_decks check failed: {error}")


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--db", type=Path, help="copy of sprk.db (opened read-only, immutable)")
    parser.add_argument("--log", type=Path, help="sprk-game.log")
    args = parser.parse_args()
    if not args.db and not args.log:
        parser.error("pass --db and/or --log")
    if args.log:
        report_log(args.log)
    if args.db:
        report_db(args.db)


if __name__ == "__main__":
    main()
