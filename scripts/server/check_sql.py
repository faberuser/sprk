#!/usr/bin/env python3
"""Type-check every literal SQL statement in src/ against the PostgreSQL schema.

Builds a scratch database from migrations/, then PREPAREs each statement
(syntax, tables, columns, operator types, GROUP BY, ambiguity) without running
it. Statements assembled with format!() are left to the tests.

Usage (psql reached through any command, e.g. a container):
    python3 -I scripts/server/check_sql.py --psql "docker exec -i <container> psql -U postgres"
Exits non-zero when a statement fails to prepare.
"""
import argparse
import pathlib
import re
import shlex
import subprocess
import sys
import uuid

ROOT = pathlib.Path(__file__).resolve().parents[2]
QUERY = re.compile(
    r'(?:crate::db::query|crate::db::query_scalar|crate::db::query_as|sqlx::query|sqlx::query_scalar|sqlx::query_as)'
    r'(?:::<[^>]*>)?\(\s*(?:sqlx::AssertSqlSafe\()?(r(#*)"(.*?)"\2|"((?:[^"\\]|\\.)*)")', re.S)
# Statements on objects that only exist at test time (created by test helpers
# and by the lock tests' scratch table).
SKIP = ("sprk_test_writes", "FROM counter", "INTO counter", "UPDATE counter", "TABLE counter")


def unescape(body: str) -> str:
    body = re.sub(r'\\\n\s*', '', body)
    return body.replace('\\"', '"').replace('\\n', '\n').replace('\\t', '\t').replace('\\\\', '\\')


def statements():
    for path in sorted((ROOT / "src").rglob("*.rs")):
        text = path.read_text(encoding="utf-8")
        for m in QUERY.finditer(text):
            sql = m.group(3) if m.group(3) is not None else unescape(m.group(4))
            if re.search(r'\{[a-z_][\w.:?]*\}', sql) or not re.match(r'\s*(SELECT|INSERT|UPDATE|DELETE|WITH)\b', sql, re.I):
                continue
            if any(s in sql for s in SKIP):
                continue
            line = text.count("\n", 0, m.start()) + 1
            yield f"{path.relative_to(ROOT)}:{line}", sql.replace("{{", "{").replace("}}", "}")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--psql", required=True, help="command that runs psql against the server (connected to any database)")
    args = parser.parse_args()
    psql = shlex.split(args.psql)
    database = f"sprk_sql_check_{uuid.uuid4().hex[:8]}"
    run = lambda sql, *extra: subprocess.run(psql + list(extra), input=sql, text=True, capture_output=True, encoding="utf-8")

    created = run(f"CREATE DATABASE {database};")
    if created.returncode != 0:
        print(created.stderr, file=sys.stderr)
        return 2
    try:
        schema = "\\set ON_ERROR_STOP on\nBEGIN;\n" + "\n".join(
            p.read_text(encoding="utf-8") for p in sorted((ROOT / "migrations").glob("*.sql"))) + "\nCOMMIT;\n"
        applied = run(schema, "-q", "-d", database)
        if applied.returncode != 0:
            print("migrations failed:\n" + applied.stderr, file=sys.stderr)
            return 2
        script, count = [], 0
        for count, (location, sql) in enumerate(statements(), 1):
            body = f"PREPARE s{count} AS {sql}".replace("$q$", "$ q $")
            script.append(f"DO $do$ BEGIN EXECUTE $q${body}$q$; EXECUTE 'DEALLOCATE s{count}'; "
                          f"EXCEPTION WHEN OTHERS THEN RAISE NOTICE 'FAIL {location}: %', SQLERRM; END $do$;")
        checked = run("\n".join(script), "-q", "-d", database)
        failures = [line.split("NOTICE:", 1)[1].strip() for line in checked.stderr.splitlines() if "NOTICE:  FAIL" in line]
        for failure in failures:
            print(failure)
        print(f"{count} statements checked, {len(failures)} failed")
        return 1 if failures or checked.returncode != 0 else 0
    finally:
        run(f"DROP DATABASE IF EXISTS {database} WITH (FORCE);")


if __name__ == "__main__":
    sys.exit(main())
