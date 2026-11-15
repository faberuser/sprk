//! Transaction-scoped reads of keyed state rows (`battle_state`,
//! `extension_state`, `community_state`). Snapshots such as the lobby read a
//! row per key type, one statement each; `load` reads the kinds they use in one
//! statement per table and `scope` serves the helpers' `get` from it, while
//! their `put` writes through to both.
//!
//! Use it only inside one transaction that holds the owner's account lock, and
//! only for kinds that are written solely through the helpers' `put` (no raw
//! SQL), so the prefetched rows cannot go stale.
use crate::error::{Result, ServerError};
use sqlx::PgConnection;
use std::{cell::RefCell, collections::{BTreeMap, HashMap}};

#[derive(Clone, Copy, PartialEq, Eq, Hash, Debug)]
pub(crate) enum Table {
    Battle,
    Extension,
    Community,
}

/// (table, owner, kind) -> idx -> data, in idx order. A prefetched kind
/// without a row for an idx means the row does not exist.
#[derive(Default)]
pub(crate) struct Rows(HashMap<(Table, i64, String), BTreeMap<i64, String>>);

tokio::task_local! {
    static ROWS: RefCell<Rows>;
}

impl Rows {
    /// Add every row of `kinds` owned by `owner` in `table`.
    pub(crate) async fn load(mut self, db: &mut PgConnection, table: Table, owner: i64, kinds: &[&str]) -> Result<Self> {
        let sql = match table {
            Table::Battle => "SELECT kind,idx,data FROM battle_state WHERE account=$1 AND kind = ANY(ARRAY(SELECT jsonb_array_elements_text($2::jsonb)))",
            Table::Extension => "SELECT kind,idx,data FROM extension_state WHERE account_id=$1 AND kind = ANY(ARRAY(SELECT jsonb_array_elements_text($2::jsonb)))",
            Table::Community => "SELECT kind,idx,data FROM community_state WHERE owner=$1 AND kind = ANY(ARRAY(SELECT jsonb_array_elements_text($2::jsonb)))",
        };
        for kind in kinds { self.0.entry((table, owner, (*kind).to_owned())).or_default(); }
        let list = serde_json::to_string(kinds).map_err(|e| ServerError::Internal(e.to_string()))?;
        for (kind, idx, data) in crate::db::query_as::<(String, i64, String)>(sql).bind(owner).bind(list).fetch_all(db).await? {
            self.0.entry((table, owner, kind)).or_default().insert(idx, data);
        }
        Ok(self)
    }
}

/// Run `future` with `rows` serving the state helpers' reads.
pub(crate) async fn scope<F: std::future::Future>(rows: Rows, future: F) -> F::Output {
    ROWS.scope(RefCell::new(rows), future).await
}

/// `Some(row)` when the kind is prefetched in this scope (`None` row: absent).
pub(crate) fn get(table: Table, owner: i64, kind: &str, idx: i64) -> Option<Option<String>> {
    ROWS.try_with(|rows| {
        rows.borrow().0.get(&(table, owner, kind.to_owned())).map(|by_idx| by_idx.get(&idx).cloned())
    }).ok().flatten()
}

/// Every row of a prefetched kind in idx order (`ORDER BY idx`), or None.
pub(crate) fn list(table: Table, owner: i64, kind: &str) -> Option<Vec<String>> {
    ROWS.try_with(|rows| {
        rows.borrow().0.get(&(table, owner, kind.to_owned())).map(|by_idx| by_idx.values().cloned().collect())
    }).ok().flatten()
}

/// Record a write of a prefetched kind (other kinds are ignored).
pub(crate) fn put(table: Table, owner: i64, kind: &str, idx: i64, data: String) {
    let _ = ROWS.try_with(|rows| {
        if let Some(by_idx) = rows.borrow_mut().0.get_mut(&(table, owner, kind.to_owned())) {
            by_idx.insert(idx, data);
        }
    });
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn prefetched_rows_serve_reads_and_follow_writes_inside_the_scope_only() {
        let db = crate::database::test_pool().await;
        let mut conn = db.acquire().await.unwrap();
        let a: i64 = crate::db::query_scalar("INSERT INTO accounts(login_id) VALUES('prefetch-test') RETURNING account_id").fetch_one(&mut *conn).await.unwrap();
        crate::api::battle::put(&mut conn, a, "key", 5, &serde_json::json!({"Count":1})).await.unwrap();
        let rows = Rows::default().load(&mut conn, Table::Battle, a, &["key"]).await.unwrap();
        scope(rows, async {
            assert_eq!(get(Table::Battle, a, "key", 5), Some(Some(r#"{"Count":1}"#.to_owned())));
            assert_eq!(get(Table::Battle, a, "key", 6), Some(None), "a prefetched kind without a row");
            assert_eq!(get(Table::Battle, a, "tower", 5), None, "kinds not prefetched read the database");
            assert_eq!(get(Table::Battle, a + 1, "key", 5), None, "other owners read the database");
            crate::api::battle::put(&mut conn, a, "key", 6, &serde_json::json!({"Count":2})).await.unwrap();
            assert_eq!(crate::api::battle::get(&mut conn, a, "key", 6).await.unwrap()["Count"], 2);
            assert_eq!(list(Table::Battle, a, "key"), Some(vec![r#"{"Count":1}"#.to_owned(), r#"{"Count":2}"#.to_owned()]), "idx order");
        }).await;
        assert_eq!(get(Table::Battle, a, "key", 5), None, "nothing outside the scope");
        let stored: String = crate::db::query_scalar("SELECT data FROM battle_state WHERE account=$1 AND kind='key' AND idx=6").bind(a).fetch_one(&mut *conn).await.unwrap();
        assert_eq!(stored, r#"{"Count":2}"#, "writes still reach the database");
    }
}
