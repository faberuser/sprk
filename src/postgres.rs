//! PostgreSQL connection settings, migrations, locks and the retry wrapper
//! (docs/database-diagnostics.md).

use futures_util::future::BoxFuture;
use sqlx::{
    postgres::{PgConnectOptions, PgPoolOptions},
    PgConnection, PgPool,
};
use std::{str::FromStr, time::Duration};

/// Schema migrations in `migrations/`, embedded at compile time. They run at
/// game startup with the owner role, before the server accepts traffic.
pub(crate) static MIGRATOR: sqlx::migrate::Migrator = sqlx::migrate!();

/// Default pool size: roughly 2-4x the database's CPU cores in total.
const DEFAULT_MAX_CONNECTIONS: u32 = 16;

/// Session settings applied to every connection. The role should carry the
/// same timeouts (`ALTER ROLE ... SET`); these keep local and test databases
/// equivalent. Stored timestamps are UTC text.
fn connect_options(url: &str, read_only: bool) -> Result<PgConnectOptions, sqlx::Error> {
    let mut settings = vec![
        ("timezone", "UTC"),
        // A stuck request must not hold row or advisory locks indefinitely.
        ("statement_timeout", "30s"),
        ("lock_timeout", "10s"),
        ("idle_in_transaction_session_timeout", "60s"),
    ];
    // Endpoints routed to the read pool must never write; PostgreSQL rejects
    // a write there instead of racing.
    if read_only {
        settings.push(("default_transaction_read_only", "on"));
    }
    Ok(PgConnectOptions::from_str(url)?.options(settings))
}

fn max_connections() -> u32 {
    std::env::var("SPRK_DB_MAX_CONNECTIONS").ok()
        .and_then(|v| v.parse().ok())
        .filter(|n| *n > 0)
        .unwrap_or(DEFAULT_MAX_CONNECTIONS)
}

/// Open the read-write pool.
pub(crate) async fn connect(url: &str) -> Result<PgPool, sqlx::Error> {
    PgPoolOptions::new()
        .max_connections(max_connections())
        .acquire_timeout(Duration::from_secs(30))
        .connect_with(connect_options(url, false)?)
        .await
}

/// Open the read-only pool used for `AppState.reads`.
pub(crate) async fn connect_read(url: &str) -> Result<PgPool, sqlx::Error> {
    PgPoolOptions::new()
        .max_connections(max_connections())
        .acquire_timeout(Duration::from_secs(30))
        .connect_with(connect_options(url, true)?)
        .await
}

/// One-connection pool for startup work such as migrations.
pub(crate) async fn connect_single(url: &str) -> Result<PgPool, sqlx::Error> {
    PgPoolOptions::new()
        .max_connections(1)
        .acquire_timeout(Duration::from_secs(30))
        .connect_with(connect_options(url, false)?)
        .await
}

/// Apply pending migrations. Run with the owner role; the app role only has
/// SELECT/INSERT/UPDATE/DELETE.
pub(crate) async fn migrate(pool: &PgPool) -> Result<(), sqlx::migrate::MigrateError> {
    MIGRATOR.run(pool).await
}

/// Serialize mutating requests of one account. Requests for different
/// accounts run in parallel.
///
/// Take it as the first statement of the request's transaction; PostgreSQL
/// releases it at commit or rollback. It lives in the database, so it also
/// works across several `game` containers. Advisory locks keyed by a single
/// bigint are reserved for accounts; use the two-int4 form for anything else.
pub(crate) async fn lock_account(conn: &mut PgConnection, account_id: i64) -> Result<(), sqlx::Error> {
    let start = std::time::Instant::now();
    sqlx::query("SELECT pg_advisory_xact_lock($1)")
        .bind(account_id)
        .execute(conn)
        .await?;
    warn_slow_wait("account", account_id, start);
    Ok(())
}

/// A lock wait this long delays the request noticeably.
const SLOW_LOCK_WAIT: Duration = Duration::from_millis(250);

/// Log a long wait for one of the locks above, with the request that waited.
/// The holder is usually the same player's (or guild's) previous request,
/// which then shows as the slow path to optimize.
fn warn_slow_wait(lock: &'static str, key: i64, start: std::time::Instant) {
    let waited = start.elapsed();
    if waited >= SLOW_LOCK_WAIT {
        let path = crate::database::REQUEST_PATH.try_with(|p| p.to_string()).unwrap_or_else(|_| "background".into());
        tracing::warn!(lock, key, path, waited_ms = waited.as_millis() as u64, "Waited long for a lock");
    }
}

/// Lock several accounts (cross-player writes such as friend points or
/// duels) in ascending order, so two requests never wait on each other in
/// opposite order.
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
pub(crate) async fn lock_accounts(conn: &mut PgConnection, accounts: &[i64]) -> Result<(), sqlx::Error> {
    let mut accounts = accounts.to_vec();
    accounts.sort_unstable();
    accounts.dedup();
    for account in accounts {
        lock_account(conn, account).await?;
    }
    Ok(())
}

/// Advisory lock classes for shared, non-account entities (two-int4 key form,
/// separate from the account keyspace).
const GUILD_LOCK: i32 = 1;
const ARENA_QUEUE_LOCK: i32 = 2;

/// Serialize arena matchmaking and settlement: they write both players'
/// queue entries and swords. Take it after the account lock, never before.
pub(crate) async fn lock_arena_queue(conn: &mut PgConnection) -> Result<(), sqlx::Error> {
    let start = std::time::Instant::now();
    sqlx::query("SELECT pg_advisory_xact_lock($1, 0)").bind(ARENA_QUEUE_LOCK).execute(conn).await?;
    warn_slow_wait("arena queue", 0, start);
    Ok(())
}

/// Serialize read-modify-write of a guild's shared rows (guild_*
/// community_state, member list, guild arena/raid/conquest state). Take it
/// after the account lock, never before.
pub(crate) async fn lock_guild(conn: &mut PgConnection, guild_id: i64) -> Result<(), sqlx::Error> {
    let start = std::time::Instant::now();
    sqlx::query("SELECT pg_advisory_xact_lock($1, $2)")
        .bind(GUILD_LOCK)
        .bind(i32::try_from(guild_id).map_err(|_| sqlx::Error::Protocol(format!("guild id {guild_id} out of range")))?)
        .execute(conn)
        .await?;
    warn_slow_wait("guild", guild_id, start);
    Ok(())
}

/// Like `lock_guild`, but returns false instead of waiting (for a second
/// guild, such as a guild arena opponent: waiting could deadlock).
pub(crate) async fn try_lock_guild(conn: &mut PgConnection, guild_id: i64) -> Result<bool, sqlx::Error> {
    sqlx::query_scalar("SELECT pg_try_advisory_xact_lock($1, $2)")
        .bind(GUILD_LOCK)
        .bind(i32::try_from(guild_id).map_err(|_| sqlx::Error::Protocol(format!("guild id {guild_id} out of range")))?)
        .fetch_one(conn)
        .await
}

/// SQLSTATE 40001 (serialization failure) or 40P01 (deadlock detected): the
/// transaction did nothing and can run again.
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
pub(crate) fn is_retryable(error: &sqlx::Error) -> bool {
    matches!(
        error.as_database_error().and_then(|e| e.code()).as_deref(),
        Some("40001" | "40P01")
    )
}

/// Errors that may carry a database error, so the retry wrapper can see it.
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
pub(crate) trait DatabaseFailure {
    fn database_error(&self) -> Option<&sqlx::Error>;
}

impl DatabaseFailure for sqlx::Error {
    fn database_error(&self) -> Option<&sqlx::Error> { Some(self) }
}

impl DatabaseFailure for crate::error::ServerError {
    fn database_error(&self) -> Option<&sqlx::Error> {
        match self {
            crate::error::ServerError::Database(e) => Some(e),
            _ => None,
        }
    }
}

impl DatabaseFailure for anyhow::Error {
    fn database_error(&self) -> Option<&sqlx::Error> { self.downcast_ref() }
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
pub(crate) enum Isolation {
    /// PostgreSQL's default. Pair read-then-write paths with `lock_account`,
    /// `SELECT ... FOR UPDATE` or a single atomic UPDATE.
    ReadCommitted,
    /// Conflicting transactions fail with 40001 and are retried.
    Serializable,
}

/// Attempts per transaction, including the first.
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
const MAX_ATTEMPTS: u32 = 5;

/// Run `work` in a transaction and commit it, rerunning the whole
/// transaction on serialization failures and deadlocks with a short bounded
/// backoff. `work` may run several times, so it must not have effects
/// outside the database (notifications, HTTP, channel sends); do those after
/// this returns. Keep it short: no table lookups or other awaits on
/// non-database work while the transaction is open.
#[cfg_attr(not(test), allow(dead_code))] // building block; see docs section 3
pub(crate) async fn transaction<T, E, F>(pool: &PgPool, isolation: Isolation, mut work: F) -> Result<T, E>
where
    F: for<'c> FnMut(&'c mut PgConnection) -> BoxFuture<'c, Result<T, E>>,
    E: From<sqlx::Error> + DatabaseFailure,
{
    let mut attempt = 1;
    loop {
        let result = async {
            let mut tx = pool.begin().await?;
            if isolation == Isolation::Serializable {
                sqlx::query("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE").execute(&mut *tx).await?;
            }
            let value = work(&mut *tx).await?;
            tx.commit().await?;
            Ok::<T, E>(value)
        }.await;
        match result {
            Err(error) if attempt < MAX_ATTEMPTS && error.database_error().is_some_and(is_retryable) => {
                // 5, 10, 20, 40 ms plus up to the same again in jitter, so
                // conflicting requests do not retry in lockstep.
                let base = 5u64 << (attempt - 1);
                let jitter = rand::random::<u64>() % (base + 1);
                tracing::debug!(attempt, "Retrying database transaction after a serialization failure");
                tokio::time::sleep(Duration::from_millis(base + jitter)).await;
                attempt += 1;
            }
            result => return result,
        }
    }
}

#[cfg(test)]
#[path = "postgres_tests.rs"]
mod tests;
