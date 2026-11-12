//! Currency arithmetic stays within bigint: overflow fails the statement instead of wrapping.
use crate::{error::Result, api::inventory::item::rule};
use sqlx::PgConnection;


pub(crate) fn balance(old: i64, delta: i64) -> Result<i64> {
    if old < 0 { return Err(rule("InvalidCurrency")); }
    old.checked_add(delta).filter(|v| *v >= 0).ok_or_else(|| rule("CurrencyOverflow"))
}

/// Call only with a column selected by server code, never request text.
pub(crate) async fn user(db: &mut PgConnection, account: i64, column: &str, delta: i64, held: i64) -> Result<i64> {
    let minimum = if delta < 0 {
        delta.checked_neg().and_then(|v| v.checked_add(held)).ok_or_else(|| rule("NotEnoughCurrency"))?
    } else { 0 };
    let maximum = i64::MAX - delta.max(0);
    let value: Option<i64> = crate::db::query_scalar(sqlx::AssertSqlSafe(format!(
        "UPDATE user_info SET {column}={column}+$1 WHERE account_id=$2 AND {column} BETWEEN $3 AND $4 RETURNING {column}"
    ))).bind(delta).bind(account).bind(minimum).bind(maximum).fetch_optional(db).await?;
    value.ok_or_else(|| rule(&format!("NotEnough{}", column.split('_').map(|part| {
        let mut chars = part.chars();
        chars.next().map(|c| c.to_uppercase().collect::<String>() + chars.as_str()).unwrap_or_default()
    }).collect::<String>())))
}
