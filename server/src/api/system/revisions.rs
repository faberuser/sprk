//! Transactional invalidation also covers background jobs and external DB writers.
//! Revisions are per account: a change bumps the account(s) whose responses
//! depend on that row. Progression data read across accounts (world boss
//! ranking, credential links) bumps the global `('progression', 0)` revision,
//! which every account's progression cache also checks.
//!
//! The triggers that maintain the revisions are in
//! migrations/0009_response_revisions.sql.
use crate::error::Result;
use sqlx::PgConnection;

pub(crate) async fn get(db: &mut PgConnection, scope: &str, account: i64) -> Result<i64> {
    Ok(crate::db::query_scalar("SELECT revision FROM response_revisions WHERE scope=$1 AND account=$2")
        .bind(scope).bind(account).fetch_optional(db).await?.unwrap_or(0))
}

/// The account's own progression revision and the global one it also depends on.
pub(crate) async fn progression(db: &mut PgConnection, account: i64) -> Result<(i64, i64)> {
    Ok(crate::db::query_as("SELECT COALESCE((SELECT revision FROM response_revisions WHERE scope='progression' AND account=$1),0),COALESCE((SELECT revision FROM response_revisions WHERE scope='progression' AND account=0),0)")
        .bind(account).fetch_one(db).await?)
}
