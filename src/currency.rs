//! Currency arithmetic must stay integral in SQLite (overflow otherwise becomes REAL).
use crate::{error::Result, api::inventory::item::rule};
use sqlx::SqliteConnection;

/// SQLite columns already store signed 64-bit integers. Enforce that direct
/// updates cannot promote overflowing balances to REAL or create negatives.
pub(crate) async fn migrate(pool: &crate::database::DbPool) -> anyhow::Result<()> {
    let columns = ["gold", "gem", "pay_gem", "pvp_coin", "stamina", "sword", "sword2",
        "royal_point", "raid_point", "event_dungeon_point", "mileage", "friendship_point",
        "guild_raid_ticket", "world_boss_ticket"];
    let invalid = columns.iter().map(|c| format!("typeof(NEW.{c})!='integer' OR NEW.{c}<0")).collect::<Vec<_>>().join(" OR ");
    for event in ["INSERT", "UPDATE"] {
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS currency64_user_{event} BEFORE {event} ON user_info WHEN {invalid} OR NEW.gem>9223372036854775807-NEW.pay_gem BEGIN SELECT RAISE(ABORT,'Invalid 64-bit currency balance'); END"))
            .execute(pool).await?;
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS currency64_battle_{event} BEFORE {event} ON battle_currencies WHEN typeof(NEW.value)!='integer' OR NEW.value<0 BEGIN SELECT RAISE(ABORT,'Invalid 64-bit currency balance'); END"))
            .execute(pool).await?;
    }
    // Audit without modifying player balances. A corrupt earlier balance needs
    // explicit recovery; silently coercing REAL to INTEGER would lose currency.
    let invalid = invalid.replace("NEW.", "");
    let bad: i64 = sqlx::query_scalar(&format!("SELECT COUNT(*) FROM user_info WHERE {invalid} OR gem>9223372036854775807-pay_gem"))
        .fetch_one(pool).await?;
    let bad_events: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM battle_currencies WHERE typeof(value)!='integer' OR value<0").fetch_one(pool).await?;
    anyhow::ensure!(bad == 0 && bad_events == 0, "Invalid existing currency balances: {bad} user rows, {bad_events} event rows");
    Ok(())
}

pub(crate) fn balance(old: i64, delta: i64) -> Result<i64> {
    if old < 0 { return Err(rule("InvalidCurrency")); }
    old.checked_add(delta).filter(|v| *v >= 0).ok_or_else(|| rule("CurrencyOverflow"))
}

/// Call only with a column selected by server code, never request text.
pub(crate) async fn user(db: &mut SqliteConnection, account: i64, column: &str, delta: i64, held: i64) -> Result<i64> {
    let minimum = if delta < 0 {
        delta.checked_neg().and_then(|v| v.checked_add(held)).ok_or_else(|| rule("NotEnoughCurrency"))?
    } else { 0 };
    let maximum = i64::MAX - delta.max(0);
    let value: Option<i64> = sqlx::query_scalar(&format!(
        "UPDATE user_info SET {column}={column}+? WHERE account_id=? AND typeof({column})='integer' AND {column} BETWEEN ? AND ? RETURNING {column}"
    )).bind(delta).bind(account).bind(minimum).bind(maximum).fetch_optional(db).await?;
    value.ok_or_else(|| rule(&format!("NotEnough{}", column.split('_').map(|part| {
        let mut chars = part.chars();
        chars.next().map(|c| c.to_uppercase().collect::<String>() + chars.as_str()).unwrap_or_default()
    }).collect::<String>())))
}
