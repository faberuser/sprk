#!/usr/bin/env python3
"""Copy a game database from the old SQLite version (sprk.db) into PostgreSQL.

For servers that still have a sprk.db from before the PostgreSQL move. Start
the current server once against the target database (it creates the schema),
stop it, then run (pip install "psycopg[binary]"):

    python -I server/scripts/database/copy_sqlite_to_postgres.py sprk.db --database-url <postgres URL>

The default URL is the database start_server.bat runs. The copy refuses a
database that already has accounts, runs in one transaction (all or nothing),
does not re-run the gameplay triggers (the rows already carry their derived
values), continues IDs after the highest one SQLite issued, and compares every
table's rows afterwards. sprk.db is opened read-only and left unchanged.
"""
from __future__ import annotations

import argparse
import os
import sqlite3
import sys
from pathlib import Path

DEFAULT_URL = "postgres://postgres:sprk-local@127.0.0.1:54320/sprk"


def text(value):
    """A SQLite value as COPY text: whole floats become integers."""
    if value is None:
        return None
    if isinstance(value, float) and value.is_integer():
        return str(int(value))
    if isinstance(value, bytes):
        return value.decode("utf-8")
    return str(value)


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("sqlite", type=Path, help="the old sprk.db (with its -wal/-shm files next to it, if any)")
    parser.add_argument("--database-url", default=os.environ.get("DATABASE_URL", DEFAULT_URL),
                        help="target PostgreSQL database (default: DATABASE_URL, else the start_server.bat database)")
    args = parser.parse_args()
    try:
        import psycopg
    except ImportError:
        sys.exit('This script needs psycopg: pip install "psycopg[binary]"')
    if not args.sqlite.is_file():
        sys.exit(f"{args.sqlite} does not exist")

    source = sqlite3.connect(f"{args.sqlite.resolve().as_uri()}?mode=ro", uri=True)
    problems = [row[0] for row in source.execute("PRAGMA integrity_check") if row[0] != "ok"]
    if any("index" not in p for p in problems):
        sys.exit("SQLite integrity_check failed:\n  " + "\n  ".join(problems))
    source_tables = [row[0] for row in source.execute(
        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'")]

    with psycopg.connect(args.database_url) as db:
        has_schema = db.execute("SELECT to_regclass('public.accounts') IS NOT NULL").fetchone()[0]
        if not has_schema:
            sys.exit("The target database has no game schema. Start the server against it once, stop it, then rerun.")
        accounts = db.execute("SELECT COUNT(*) FROM accounts").fetchone()[0]
        if accounts:
            sys.exit(f"The target database already has {accounts} accounts. Copy only into a new, empty one.")

        columns: dict[str, list[str]] = {}
        for table, column in db.execute(
                "SELECT c.table_name, c.column_name FROM information_schema.columns c "
                "JOIN information_schema.tables t USING (table_schema, table_name) "
                "WHERE c.table_schema='public' AND t.table_type='BASE TABLE' AND c.table_name NOT LIKE '\\_sqlx%' "
                "ORDER BY c.table_name, c.ordinal_position"):
            columns.setdefault(table, []).append(column)
        unknown = [t for t in source_tables if t not in columns]
        if unknown:
            sys.exit(f"SQLite tables the PostgreSQL schema does not have: {unknown}")

        # Parents before children, so foreign keys hold while loading.
        edges = db.execute(
            "SELECT conrelid::regclass::text, confrelid::regclass::text FROM pg_constraint "
            "WHERE contype='f' AND connamespace='public'::regnamespace AND conrelid<>confrelid").fetchall()
        order, done = [], set()
        while len(done) < len(columns):
            ready = [t for t in sorted(columns) if t not in done
                     and all(parent.strip('"') in done for child, parent in edges if child.strip('"') == t)]
            if not ready:
                sys.exit("Foreign keys form a cycle")
            order += ready
            done.update(ready)

        for table in columns:
            db.execute(f'ALTER TABLE "{table}" DISABLE TRIGGER USER')
        total = 0
        for table in [t for t in order if t in source_tables]:
            source_columns = [row[1] for row in source.execute(f'PRAGMA table_info("{table}")')]
            extra = [c for c in source_columns if c not in columns[table]]
            if extra:
                sys.exit(f"{table}: SQLite columns the PostgreSQL schema does not have: {extra}")
            missing = [c for c in columns[table] if c not in source_columns]
            if missing:
                print(f"  note: {table} has no {missing} in SQLite; they get their defaults")
            names = ",".join(f'"{c}"' for c in source_columns)
            # Rows the schema or a first server start created are replaced by the copy.
            db.execute(f'DELETE FROM "{table}"')
            rows = [tuple(text(v) for v in row) for row in source.execute(f'SELECT {names} FROM "{table}"')]
            with db.cursor().copy(f'COPY "{table}" ({names}) FROM STDIN') as copy:
                for row in rows:
                    copy.write_row(row)
            copied = db.execute(
                f'SELECT {",".join(f"""CAST("{c}" AS text)""" for c in source_columns)} FROM "{table}"').fetchall()
            if sorted(copied, key=repr) != sorted(rows, key=repr):
                sys.exit(f"{table}: rows differ after loading; nothing was copied")
            total += len(rows)
            print(f"  {table}: {len(rows)} rows")

        # Continue every identity after the highest ID SQLite ever issued
        # (sqlite_sequence remembers deleted rows), so no ID is reused.
        issued = {}
        if source.execute("SELECT 1 FROM sqlite_master WHERE name='sqlite_sequence'").fetchone():
            issued = dict(source.execute("SELECT name, seq FROM sqlite_sequence"))
        for table, column in db.execute(
                "SELECT table_name, column_name FROM information_schema.columns "
                "WHERE table_schema='public' AND is_identity='YES'").fetchall():
            highest = db.execute(f'SELECT COALESCE(MAX("{column}"), 0) FROM "{table}"').fetchone()[0]
            last = max(highest, int(issued.get(table) or 0))
            if last > 0:
                db.execute("SELECT setval(pg_get_serial_sequence(%s, %s), %s)", (f'"{table}"', column, last))

        for table in columns:
            db.execute(f'ALTER TABLE "{table}" ENABLE TRIGGER USER')
    for problem in problems:
        print(f"  SQLite index warning (PostgreSQL builds its own indexes): {problem}")
    print(f"Copied {total} rows in {len(source_tables)} tables from {args.sqlite}; every table matches.")


if __name__ == "__main__":
    main()
