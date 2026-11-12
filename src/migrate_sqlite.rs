//! `sprk-server migrate-sqlite <sprk.db>`: copy the SQLite game database into
//! an empty PostgreSQL database (docs/postgresql-migration.md, section 7).
//!
//! The copy runs in one PostgreSQL transaction and either loads everything or
//! nothing. Gameplay triggers are disabled while loading (rows already carry
//! their derived values) and foreign keys stay enforced, so tables load parents
//! first. Identity sequences continue after the highest ID SQLite ever issued.
//! Every row is checksummed on both sides; any difference aborts the copy.
use anyhow::{bail, ensure, Context};
use sqlx::{
    sqlite::{SqliteConnectOptions, SqliteRow},
    Column as _, Connection, PgConnection, Row, SqliteConnection, TypeInfo, ValueRef,
};
use std::{
    collections::{BTreeMap, BTreeSet},
    hash::{Hash, Hasher},
    path::{Path, PathBuf},
};

pub(crate) struct Options {
    pub sqlite: PathBuf,
    /// Exit successfully without copying when PostgreSQL already has accounts.
    pub if_empty: bool,
    /// Optional JSON report path.
    pub report: Option<PathBuf>,
}

pub(crate) fn parse_args(args: &[String]) -> anyhow::Result<Options> {
    let mut sqlite = None;
    let mut if_empty = false;
    let mut report = None;
    let mut it = args.iter();
    while let Some(arg) = it.next() {
        match arg.as_str() {
            "--if-empty" => if_empty = true,
            "--report" => report = Some(PathBuf::from(it.next().context("--report needs a path")?)),
            other if other.starts_with("--") => bail!("Unknown option {other}"),
            other if sqlite.is_none() => sqlite = Some(PathBuf::from(other)),
            other => bail!("Unexpected argument {other}"),
        }
    }
    Ok(Options {
        sqlite: sqlite.context("Usage: sprk-server migrate-sqlite <sprk.db> [--if-empty] [--report <file.json>]")?,
        if_empty,
        report,
    })
}

#[derive(serde::Serialize, Default, Debug)]
struct TableReport {
    rows: u64,
    checksum: String,
}

#[derive(serde::Serialize, Default, Debug)]
struct Report {
    source: String,
    integrity_warnings: Vec<String>,
    tables: BTreeMap<String, TableReport>,
    sequences: BTreeMap<String, i64>,
}

pub(crate) async fn run(options: Options) -> anyhow::Result<()> {
    let url = std::env::var("DATABASE_MIGRATION_URL").or_else(|_| std::env::var("DATABASE_URL"))
        .context("Set DATABASE_URL (or DATABASE_MIGRATION_URL) to the target PostgreSQL database")?;
    let pool = crate::postgres::connect_single(&url).await?;
    crate::postgres::migrate(&pool).await?;
    let mut pg = pool.acquire().await?.detach();
    pool.close().await;

    let accounts: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM accounts").fetch_one(&mut pg).await?;
    if accounts > 0 {
        if options.if_empty {
            println!("PostgreSQL already holds {accounts} accounts; nothing to copy.");
            return Ok(());
        }
        bail!("The PostgreSQL database already holds {accounts} accounts. Copy only into an empty database.");
    }
    let report = copy(&options.sqlite, &mut pg).await?;
    let text = serde_json::to_string_pretty(&report)?;
    if let Some(path) = &options.report {
        std::fs::write(path, &text).with_context(|| format!("Writing {}", path.display()))?;
    }
    let rows: u64 = report.tables.values().map(|t| t.rows).sum();
    println!("Copied {rows} rows in {} tables from {}; checksums match.", report.tables.len(), options.sqlite.display());
    for warning in &report.integrity_warnings {
        println!("SQLite integrity warning (indexes are rebuilt by the load): {warning}");
    }
    Ok(())
}

async fn copy(path: &Path, pg: &mut PgConnection) -> anyhow::Result<Report> {
    ensure!(path.is_file(), "{} does not exist", path.display());
    let mut sqlite = SqliteConnection::connect_with(
        &SqliteConnectOptions::new().filename(path).read_only(true)).await
        .with_context(|| format!("Opening {}", path.display()))?;
    let mut report = Report { source: path.display().to_string(), ..Default::default() };

    // Index-only damage is tolerated: PostgreSQL builds its own indexes and
    // its keys reject any duplicate row. Anything else stops the copy.
    let problems: Vec<String> = sqlx::query_scalar("PRAGMA integrity_check").fetch_all(&mut sqlite).await?;
    for problem in problems.into_iter().filter(|p| p != "ok") {
        let index_only = problem.starts_with("wrong # of entries in index") || problem.contains("missing from index");
        ensure!(index_only, "SQLite integrity_check failed: {problem}");
        report.integrity_warnings.push(problem);
    }

    let source_tables: BTreeSet<String> = sqlx::query_scalar(
        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'")
        .fetch_all(&mut sqlite).await?.into_iter().collect();
    let target = target_schema(pg).await?;
    let unknown: Vec<_> = source_tables.iter().filter(|t| !target.contains_key(*t)).collect();
    ensure!(unknown.is_empty(), "SQLite tables without a PostgreSQL table: {unknown:?}");

    let mut tx = pg.begin().await?;
    // Rows carry the values the SQLite triggers derived; do not derive again.
    for table in target.keys() {
        sqlx::query(sqlx::AssertSqlSafe(format!("ALTER TABLE \"{table}\" DISABLE TRIGGER USER"))).execute(&mut *tx).await?;
    }
    // The schema seeds the global revision row; the copy brings its own.
    sqlx::query("DELETE FROM response_revisions").execute(&mut *tx).await?;

    for table in load_order(&mut tx, &target).await? {
        if !source_tables.contains(&table) {
            continue;
        }
        let columns = &target[&table];
        let source_columns: BTreeSet<String> = sqlx::query_scalar(sqlx::AssertSqlSafe(format!(
            "SELECT name FROM pragma_table_info('{table}')"))).fetch_all(&mut sqlite).await?.into_iter().collect();
        let expected: BTreeSet<String> = columns.iter().cloned().collect();
        ensure!(source_columns == expected,
            "Column mismatch in {table}: SQLite {source_columns:?}, PostgreSQL {expected:?}");
        let table_report = copy_table(&mut sqlite, &mut tx, &table, columns).await
            .with_context(|| format!("Copying {table}"))?;
        report.tables.insert(table, table_report);
    }

    report.sequences = reset_sequences(&mut sqlite, &mut tx).await?;
    for table in target.keys() {
        sqlx::query(sqlx::AssertSqlSafe(format!("ALTER TABLE \"{table}\" ENABLE TRIGGER USER"))).execute(&mut *tx).await?;
    }
    tx.commit().await?;
    sqlx::query("ANALYZE").execute(&mut *pg).await?;
    Ok(report)
}

/// PostgreSQL tables and their columns in ordinal order.
async fn target_schema(pg: &mut PgConnection) -> anyhow::Result<BTreeMap<String, Vec<String>>> {
    let rows: Vec<(String, String)> = sqlx::query_as(
        "SELECT c.table_name::text, c.column_name::text FROM information_schema.columns c \
         JOIN information_schema.tables t ON t.table_schema=c.table_schema AND t.table_name=c.table_name \
         WHERE c.table_schema='public' AND t.table_type='BASE TABLE' \
         AND c.table_name NOT LIKE '!_sqlx%' ESCAPE '!' ORDER BY c.table_name, c.ordinal_position")
        .fetch_all(&mut *pg).await?;
    let mut tables: BTreeMap<String, Vec<String>> = BTreeMap::new();
    for (table, column) in rows {
        tables.entry(table).or_default().push(column);
    }
    Ok(tables)
}

/// Tables ordered so every foreign key's target loads first.
async fn load_order(pg: &mut PgConnection, tables: &BTreeMap<String, Vec<String>>) -> anyhow::Result<Vec<String>> {
    let edges: Vec<(String, String)> = sqlx::query_as(
        "SELECT c.conrelid::regclass::text, c.confrelid::regclass::text FROM pg_constraint c \
         JOIN pg_namespace n ON n.oid=c.connamespace WHERE c.contype='f' AND n.nspname='public' AND c.conrelid<>c.confrelid")
        .fetch_all(&mut *pg).await?;
    let mut order = vec![];
    let mut done = BTreeSet::new();
    while done.len() < tables.len() {
        let ready: Vec<String> = tables.keys()
            .filter(|t| !done.contains(*t))
            .filter(|t| edges.iter().all(|(child, parent)| child != *t || done.contains(parent)))
            .cloned().collect();
        ensure!(!ready.is_empty(), "Foreign keys form a cycle");
        for table in ready {
            done.insert(table.clone());
            order.push(table);
        }
    }
    Ok(order)
}

/// A SQLite value rendered for COPY's text format.
fn copy_text(row: &SqliteRow, index: usize, out: &mut String) -> anyhow::Result<()> {
    let value = row.try_get_raw(index)?;
    if value.is_null() {
        out.push_str("\\N");
        return Ok(());
    }
    let kind = value.type_info().name().to_owned();
    match kind.as_str() {
        "INTEGER" => out.push_str(&row.try_get::<i64, _>(index)?.to_string()),
        "TEXT" => {
            for c in row.try_get::<String, _>(index)?.chars() {
                match c {
                    '\\' => out.push_str("\\\\"),
                    '\n' => out.push_str("\\n"),
                    '\r' => out.push_str("\\r"),
                    '\t' => out.push_str("\\t"),
                    c => out.push(c),
                }
            }
        }
        other => bail!("column {} holds a {other} value; only integers, text and NULL are expected",
            row.columns()[index].name()),
    }
    Ok(())
}

/// Order-independent checksum: the sum of each row's 64-bit hash.
fn row_hash(line: &str) -> u64 {
    let mut hasher = std::collections::hash_map::DefaultHasher::new();
    line.hash(&mut hasher);
    hasher.finish()
}

async fn copy_table(sqlite: &mut SqliteConnection, pg: &mut PgConnection, table: &str, columns: &[String]) -> anyhow::Result<TableReport> {
    use futures_util::TryStreamExt;
    let list = columns.iter().map(|c| format!("\"{c}\"")).collect::<Vec<_>>().join(",");
    let mut copy = pg.copy_in_raw(&format!("COPY \"{table}\" ({list}) FROM STDIN")).await?;
    let mut rows = sqlx::query(sqlx::AssertSqlSafe(format!("SELECT {list} FROM \"{table}\""))).fetch(&mut *sqlite);
    let (mut count, mut sum, mut buffer) = (0u64, 0u64, String::new());
    while let Some(row) = rows.try_next().await? {
        let mut line = String::new();
        for i in 0..columns.len() {
            if i > 0 { line.push('\t'); }
            copy_text(&row, i, &mut line).with_context(|| format!("row {}", count + 1))?;
        }
        sum = sum.wrapping_add(row_hash(&line));
        buffer.push_str(&line);
        buffer.push('\n');
        count += 1;
        if buffer.len() > 1 << 20 {
            copy.send(buffer.as_bytes()).await?;
            buffer.clear();
        }
    }
    drop(rows);
    if !buffer.is_empty() {
        copy.send(buffer.as_bytes()).await?;
    }
    let loaded = copy.finish().await?;
    ensure!(loaded == count, "{table}: read {count} rows but loaded {loaded}");

    // Read every row back exactly as COPY renders it and compare.
    let mut out = pg.copy_out_raw(&format!("COPY \"{table}\" ({list}) TO STDOUT")).await?;
    let (mut back, mut back_sum, mut pending) = (0u64, 0u64, Vec::<u8>::new());
    while let Some(chunk) = out.try_next().await? {
        pending.extend_from_slice(&chunk);
        while let Some(end) = pending.iter().position(|b| *b == b'\n') {
            let line = String::from_utf8(pending.drain(..=end).collect())?;
            back_sum = back_sum.wrapping_add(row_hash(line.trim_end_matches('\n')));
            back += 1;
        }
    }
    ensure!(back == count && back_sum == sum, "{table}: PostgreSQL rows differ from SQLite after loading");
    Ok(TableReport { rows: count, checksum: format!("{sum:016x}") })
}

/// Continue every identity after the highest ID SQLite issued, including IDs
/// of deleted rows (sqlite_sequence), so no ID is ever reused.
async fn reset_sequences(sqlite: &mut SqliteConnection, pg: &mut PgConnection) -> anyhow::Result<BTreeMap<String, i64>> {
    let issued: BTreeMap<String, i64> = sqlx::query_as::<_, (String, i64)>("SELECT name, seq FROM sqlite_sequence")
        .fetch_all(&mut *sqlite).await.unwrap_or_default().into_iter().collect();
    let identities: Vec<(String, String)> = sqlx::query_as(
        "SELECT table_name::text, column_name::text FROM information_schema.columns \
         WHERE table_schema='public' AND is_identity='YES' ORDER BY table_name")
        .fetch_all(&mut *pg).await?;
    let mut result = BTreeMap::new();
    for (table, column) in identities {
        let max: Option<i64> = sqlx::query_scalar(sqlx::AssertSqlSafe(format!("SELECT max(\"{column}\") FROM \"{table}\"")))
            .fetch_one(&mut *pg).await?;
        let last = max.unwrap_or(0).max(issued.get(&table).copied().unwrap_or(0));
        if last > 0 {
            sqlx::query("SELECT setval(pg_get_serial_sequence($1, $2), $3)")
                .bind(format!("\"{table}\"")).bind(&column).bind(last).execute(&mut *pg).await?;
            result.insert(table, last);
        }
    }
    Ok(result)
}

#[cfg(test)]
mod tests {
    use super::*;

    /// A SQLite database with the final SQLite schema and a few rows that
    /// exercise escaping, NULLs, triggers and identity continuation.
    async fn source() -> (PathBuf, sqlx::SqlitePool) {
        let path = std::env::temp_dir().join(format!("sprk-copy-{}.sqlite", uuid::Uuid::new_v4()));
        let pool = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1)
            .connect_with(SqliteConnectOptions::new().filename(&path).create_if_missing(true)).await.unwrap();
        sqlx::raw_sql(include_str!("../tests/fixtures/sqlite_schema.sql")).execute(&pool).await.unwrap();
        for sql in [
            "INSERT INTO accounts(account_id,login_id,nick) VALUES(1,'a','Tab\tand\nnewline \\ backslash'),(2,'b',NULL)",
            "INSERT INTO user_info(account_id,gold) VALUES(1,100),(2,5)",
            "UPDATE user_info SET gold=40 WHERE account_id=1",
            "INSERT INTO mails(account_id,title,content) VALUES(1,'hi','x'),(1,'later','y')",
            "DELETE FROM mails WHERE title='later'",
            "INSERT INTO arena_runs(account,kind,started,status,data) VALUES(1,0,1,'complete','{\"Season\":3,\"Heroes\":[1]}')",
        ] {
            sqlx::query(sqlx::AssertSqlSafe(sql)).execute(&pool).await.unwrap();
        }
        (path, pool)
    }

    #[tokio::test]
    async fn copies_every_row_exactly_once_without_replaying_triggers() {
        let (path, sqlite) = source().await;
        let expected_metrics: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM progression_metrics").fetch_one(&sqlite).await.unwrap();
        let expected_notifications: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mail_notifications").fetch_one(&sqlite).await.unwrap();
        sqlite.close().await;

        let db = crate::database::test_pool().await;
        // Views in public (production's pg_stat_statements) are not copy targets.
        sqlx::query("CREATE VIEW extension_view AS SELECT 1 AS x").execute(&db).await.unwrap();
        let mut pg = db.acquire().await.unwrap().detach();
        let report = copy(&path, &mut pg).await.unwrap();
        assert_eq!(report.tables["accounts"].rows, 2);
        let nick: String = sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id=1").fetch_one(&db).await.unwrap();
        assert_eq!(nick, "Tab\tand\nnewline \\ backslash");
        // Derived rows were copied, not derived a second time.
        let metrics: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM progression_metrics").fetch_one(&db).await.unwrap();
        let notifications: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mail_notifications").fetch_one(&db).await.unwrap();
        assert_eq!((metrics, notifications), (expected_metrics, expected_notifications));
        // The deleted mail's ID is never reused; triggers are active again.
        let mail: i64 = sqlx::query_scalar("INSERT INTO mails(account_id,title) VALUES(1,'new') RETURNING mail_id").fetch_one(&db).await.unwrap();
        assert_eq!(mail, 3);
        let notified: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mail_notifications WHERE mail_id=3").fetch_one(&db).await.unwrap();
        assert_eq!(notified, 1);
        let account: i64 = sqlx::query_scalar("INSERT INTO accounts(login_id) VALUES('c') RETURNING account_id").fetch_one(&db).await.unwrap();
        assert_eq!(account, 3);
        drop(pg);
        crate::database::remove_sqlite_file(&path).await;
    }

    #[tokio::test]
    async fn refuses_unknown_tables_and_rolls_back() {
        let (path, sqlite) = source().await;
        sqlx::query("CREATE TABLE unexpected(x INTEGER)").execute(&sqlite).await.unwrap();
        sqlite.close().await;
        let db = crate::database::test_pool().await;
        let mut pg = db.acquire().await.unwrap().detach();
        let error = copy(&path, &mut pg).await.unwrap_err();
        assert!(error.to_string().contains("unexpected"), "{error}");
        let accounts: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM accounts").fetch_one(&db).await.unwrap();
        assert_eq!(accounts, 0);
        drop(pg);
        crate::database::remove_sqlite_file(&path).await;
    }
}
