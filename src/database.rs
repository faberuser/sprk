//! Game database: PostgreSQL. Schema lives in `migrations/`; connection
//! settings, the account lock and the retry wrapper in `crate::postgres`.
use std::{path::Path, sync::Arc};

pub type DbPool = sqlx::PgPool;

tokio::task_local! {
    /// HTTP path of the request running in this task, for slow-request logs.
    pub(crate) static REQUEST_PATH: Arc<str>;
    /// The player this request acts for (set by the acting_account middleware).
    pub(crate) static ACTING_ACCOUNT: i64;
}

/// Begin a write transaction. Within a player's request it first takes that
/// player's account lock, so one player's transactions run one at a time (as
/// on the single SQLite writer) while different players run in parallel.
/// Background work has no acting account and takes explicit locks instead.
pub(crate) async fn begin(pool: &DbPool) -> Result<sqlx::Transaction<'static, sqlx::Postgres>, sqlx::Error> {
    let mut tx = pool.begin().await?;
    if let Ok(account) = ACTING_ACCOUNT.try_with(|a| *a) {
        crate::postgres::lock_account(&mut tx, account).await?;
    }
    Ok(tx)
}

/// Spawn background work labelled `label` in logs rather than "background"
/// (task-locals do not cross spawns).
pub(crate) fn spawn_labelled<F>(label: &'static str, work: F) -> tokio::task::JoinHandle<F::Output>
where F: std::future::Future + Send + 'static, F::Output: Send + 'static {
    tokio::spawn(REQUEST_PATH.scope(Arc::from(label), work))
}

fn database_url() -> anyhow::Result<String> {
    std::env::var("DATABASE_URL").map_err(|_| anyhow::anyhow!(
        "DATABASE_URL is not set; it must name the game's PostgreSQL database (see docs/postgresql-migration.md)"))
}

/// Run pending migrations with the owner role (DATABASE_MIGRATION_URL, or
/// DATABASE_URL when one role does both), then open the app pool.
pub async fn init_database() -> anyhow::Result<DbPool> {
    let url = database_url()?;
    let migration_url = std::env::var("DATABASE_MIGRATION_URL").unwrap_or_else(|_| url.clone());
    let owner = crate::postgres::connect_single(&migration_url).await?;
    crate::postgres::migrate(&owner).await?;
    owner.close().await;

    let pool = crate::postgres::connect(&url).await?;
    refuse_unmigrated_sqlite(&pool).await?;
    tracing::info!("Database initialized successfully");
    Ok(pool)
}

/// The SQLite file is the only copy of the player data until the copy tool
/// has loaded it. Starting on an empty PostgreSQL database next to it would
/// let players create accounts whose IDs collide with the import.
async fn refuse_unmigrated_sqlite(pool: &DbPool) -> anyhow::Result<()> {
    let sqlite = std::env::var("SPRK_SQLITE_PATH").unwrap_or_else(|_| "sprk.db".into());
    if !Path::new(&sqlite).exists() {
        return Ok(());
    }
    let accounts: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM accounts").fetch_one(pool).await?;
    anyhow::ensure!(accounts > 0,
        "{sqlite} exists but the PostgreSQL database has no accounts. Copy it with `sprk-server migrate-sqlite {sqlite}` \
         before starting the game, or move the file away if it is not needed.");
    Ok(())
}

/// Read-only pool for `AppState.reads`: same server for now, a replica later.
pub(crate) async fn open_read_pool() -> anyhow::Result<DbPool> {
    let url = std::env::var("DATABASE_READ_URL").or_else(|_| database_url())?;
    Ok(crate::postgres::connect_read(&url).await?)
}

/// Connections per test pool.
#[cfg(test)]
pub(crate) const TEST_POOL_SIZE: u32 = 8;

/// Delete a test SQLite file. Windows can hold the handle briefly after the
/// pool closes; retry only sharing violations.
#[cfg(test)]
pub(crate) async fn remove_sqlite_file(path: &Path) {
    for attempt in 0..20 {
        match std::fs::remove_file(path) {
            Ok(()) => return,
            Err(error) if cfg!(windows) && matches!(error.raw_os_error(), Some(32 | 33)) && attempt < 19 => {
                tokio::time::sleep(std::time::Duration::from_millis(25)).await;
            }
            Err(error) => panic!("Could not remove {}: {error}", path.display()),
        }
    }
}

/// Fresh, migrated database for one test, cloned from a template that is
/// migrated once per schema version. Needs SPRK_TEST_POSTGRES_URL: a server
/// where the role may create databases (README: PostgreSQL).
#[cfg(test)]
pub(crate) async fn test_pool() -> DbPool {
    test_support::database(false).await
}

/// Make writes fail, to test rollback paths: `timing` is e.g.
/// "BEFORE INSERT ON mails", `when` an optional row condition.
#[cfg(test)]
pub(crate) async fn fail_trigger(db: &DbPool, name: &str, timing: &str, when: Option<&str>, message: &str) {
    let when = when.map(|w| format!("WHEN ({w})")).unwrap_or_default();
    sqlx::raw_sql(sqlx::AssertSqlSafe(format!(
        "CREATE OR REPLACE FUNCTION sprk_test_fail() RETURNS trigger LANGUAGE plpgsql AS $f$ BEGIN RAISE EXCEPTION '%', TG_ARGV[0]; END $f$; \
         CREATE TRIGGER {name} {timing} FOR EACH ROW {when} EXECUTE FUNCTION sprk_test_fail('{}')", message.replace('\'', "''"))))
        .execute(db).await.unwrap();
}

/// Rows written so far in this test database, like SQLite's total_changes().
/// The first call installs a counting trigger on every table.
#[cfg(test)]
pub(crate) async fn row_changes(db: &DbPool) -> i64 {
    sqlx::raw_sql(r#"
        CREATE SEQUENCE IF NOT EXISTS sprk_test_writes;
        CREATE OR REPLACE FUNCTION sprk_test_count_write() RETURNS trigger LANGUAGE plpgsql
            AS $f$ BEGIN PERFORM nextval('sprk_test_writes'); RETURN NULL; END $f$;
        DO $d$ DECLARE t text; BEGIN
            FOR t IN SELECT c.relname FROM pg_class c JOIN pg_namespace n ON n.oid = c.relnamespace
                     WHERE n.nspname = 'public' AND c.relkind = 'r' AND c.relname NOT LIKE '!_sqlx%' ESCAPE '!'
                       AND NOT EXISTS (SELECT 1 FROM pg_trigger g WHERE g.tgrelid = c.oid AND g.tgname = 'sprk_test_count_write')
            LOOP
                EXECUTE format('CREATE TRIGGER sprk_test_count_write AFTER INSERT OR UPDATE OR DELETE ON %I FOR EACH ROW EXECUTE FUNCTION sprk_test_count_write()', t);
            END LOOP;
        END $d$;"#).execute(db).await.unwrap();
    crate::db::query_scalar("SELECT CASE WHEN is_called THEN last_value ELSE 0 END FROM sprk_test_writes")
        .fetch_one(db).await.unwrap()
}

/// The plan of `sql` with every `$n` bound to 1 and sequential scans
/// disabled, so tiny test tables still show whether an index is usable.
#[cfg(test)]
pub(crate) async fn explain(db: &DbPool, sql: &str) -> String {
    let params = sql.match_indices('$')
        .filter_map(|(i, _)| sql[i + 1..].split(|c: char| !c.is_ascii_digit()).next()?.parse::<usize>().ok())
        .max().unwrap_or(0);
    let mut tx = db.begin().await.unwrap();
    sqlx::query("SET LOCAL enable_seqscan = off").execute(&mut *tx).await.unwrap();
    let mut query = crate::db::query_scalar::<String>(sqlx::AssertSqlSafe(format!("EXPLAIN {sql}")));
    for _ in 0..params { query = query.bind(1_i64); }
    let plan = query.fetch_all(&mut *tx).await.unwrap().join("
");
    tx.rollback().await.unwrap();
    plan
}

/// Remove a trigger made by `fail_trigger`.
#[cfg(test)]
pub(crate) async fn drop_fail_trigger(db: &DbPool, name: &str, table: &str) {
    sqlx::query(sqlx::AssertSqlSafe(format!("DROP TRIGGER {name} ON {table}"))).execute(db).await.unwrap();
}

/// Close `db` and open a new pool on the same database ("server restart").
#[cfg(test)]
pub(crate) async fn reconnect(db: DbPool) -> DbPool {
    let options = db.connect_options().as_ref().clone();
    db.close().await;
    sqlx::postgres::PgPoolOptions::new().max_connections(8).connect_with(options).await.unwrap()
}

/// Read-only pool on the same test database as `db`.
#[cfg(test)]
pub(crate) async fn test_read_pool(db: &DbPool) -> DbPool {
    let options = db.connect_options().as_ref().clone().options([("default_transaction_read_only", "on")]);
    sqlx::postgres::PgPoolOptions::new().max_connections(2).connect_with(options).await.unwrap()
}

#[cfg(test)]
pub(crate) mod test_support {
    use sqlx::{postgres::{PgConnectOptions, PgPoolOptions}, AssertSqlSafe, Connection, PgConnection};
    use std::{str::FromStr, sync::atomic::{AtomicU64, Ordering}};
    use tokio::sync::OnceCell;

    static TEMPLATE: OnceCell<String> = OnceCell::const_new();
    static NEXT: AtomicU64 = AtomicU64::new(0);

    pub(crate) fn admin_options() -> PgConnectOptions {
        let url = std::env::var("SPRK_TEST_POSTGRES_URL").expect(
            "SPRK_TEST_POSTGRES_URL is not set. Tests need a PostgreSQL server where the role can create \
             databases, e.g. postgres://postgres:dev@127.0.0.1:5432/postgres (README: PostgreSQL)");
        PgConnectOptions::from_str(&url).expect("valid SPRK_TEST_POSTGRES_URL")
    }

    /// Template name derived from the migrations, so a schema change builds
    /// a new template and an unchanged one is reused across test runs.
    fn template_name() -> String {
        let mut hash: u64 = 0xcbf29ce484222325;
        for m in crate::postgres::MIGRATOR.iter() {
            for b in m.version.to_le_bytes().iter().chain(m.checksum.iter()) {
                hash = (hash ^ u64::from(*b)).wrapping_mul(0x100000001b3);
            }
        }
        format!("sprk_tpl_{hash:016x}")
    }

    async fn template() -> &'static str {
        TEMPLATE.get_or_init(|| async {
            let name = template_name();
            let mut admin = PgConnection::connect_with(&admin_options()).await.expect("connect to SPRK_TEST_POSTGRES_URL");
            // Serialize template creation across concurrent test processes.
            sqlx::query("SELECT pg_advisory_lock(hashtext('sprk test template'))").execute(&mut admin).await.unwrap();
            let exists: bool = crate::db::query_scalar("SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname=$1)")
                .bind(&name).fetch_one(&mut admin).await.unwrap();
            if !exists {
                let building = format!("{name}_build");
                sqlx::query(AssertSqlSafe(format!("DROP DATABASE IF EXISTS {building}"))).execute(&mut admin).await.unwrap();
                sqlx::query(AssertSqlSafe(format!("CREATE DATABASE {building}"))).execute(&mut admin).await.unwrap();
                let mut build = PgConnection::connect_with(&admin_options().database(&building)).await.unwrap();
                crate::postgres::MIGRATOR.run_direct(None, &mut build, false).await.expect("migrations apply to an empty database");
                build.close().await.unwrap();
                // Rename last, so other processes never clone a half-built template.
                sqlx::query(AssertSqlSafe(format!("ALTER DATABASE {building} RENAME TO {name}"))).execute(&mut admin).await.unwrap();
            }
            // Leftovers of earlier runs (a panicking test cannot drop its
            // database). Databases still in use by another run are skipped.
            let pid = std::process::id();
            let stale: Vec<String> = crate::db::query_scalar(
                "SELECT datname FROM pg_database WHERE (datname LIKE 'sprk\\_t\\_%' AND datname NOT LIKE $1) \
                 OR (datname LIKE 'sprk\\_tpl\\_%' AND datname<>$2)")
                .bind(format!("sprk\\_t\\_{pid}\\_%")).bind(&name).fetch_all(&mut admin).await.unwrap();
            for db in stale {
                let _ = sqlx::query(AssertSqlSafe(format!("DROP DATABASE IF EXISTS {db}"))).execute(&mut admin).await;
            }
            sqlx::query("SELECT pg_advisory_unlock(hashtext('sprk test template'))").execute(&mut admin).await.unwrap();
            admin.close().await.unwrap();
            name
        }).await
    }

    pub(crate) async fn database(read_only: bool) -> crate::database::DbPool {
        let template = template().await;
        let name = format!("sprk_t_{}_{}", std::process::id(), NEXT.fetch_add(1, Ordering::Relaxed));
        let mut admin = PgConnection::connect_with(&admin_options()).await.unwrap();
        sqlx::query(AssertSqlSafe(format!("CREATE DATABASE {name} TEMPLATE {template}"))).execute(&mut admin).await.unwrap();
        admin.close().await.unwrap();
        let mut settings = vec![("timezone", "UTC")];
        if read_only { settings.push(("default_transaction_read_only", "on")); }
        PgPoolOptions::new()
            .max_connections(crate::database::TEST_POOL_SIZE)
            .connect_with(admin_options().database(&name).options(settings))
            .await
            .unwrap()
    }
}
