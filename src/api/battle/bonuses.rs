//! Reward bonuses shared by battle settlement and its regression tests.
use super::*;

pub(crate) async fn calculate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    battle: i64,
) -> Result<(i64, i64)> {
    let guild_bonus = crate::api::community::guild_reward_boost(db, s, a).await?;
    let active: Vec<i32> = sqlx::query_scalar(
        "SELECT item_index FROM item_boosters WHERE account_id=? AND end_time>datetime('now')",
    )
    .bind(a)
    .fetch_all(&mut *db)
    .await?;
    let (mut gold, mut exp) = (0, 0);
    for id in active {
        if let Some(b) = s.tables.inventory.boosters.get(&id) {
            if b["BattleTypes"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(battle)))
            {
                match n(b, "Type") {
                    1 => exp = exp.max(n(b, "Value")),
                    3 => gold = gold.max(n(b, "Value")),
                    _ => {}
                }
            }
        }
    }
    let costumes: Vec<i32> =
        sqlx::query_scalar("SELECT costume_index FROM costumes WHERE account_id=?")
            .bind(a)
            .fetch_all(db)
            .await?;
    for id in costumes {
        if let Some(c) = s.tables.hero_shop.costumes.get(&id) {
            for i in 1..=3 {
                if n(c, &format!("AbilityType{i}")) == 1 {
                    let v = &c[format!("AbilityValue{i}")];
                    let amount = v[1]
                        .as_str()
                        .and_then(|s| s.parse::<i64>().ok())
                        .unwrap_or(0);
                    match v[0].as_str().unwrap_or("") {
                        "BonusGold" => gold += amount,
                        "BonusExp" => exp += amount,
                        _ => {}
                    }
                }
            }
        }
    }
    Ok((gold + guild_bonus.0, exp + guild_bonus.1))
}
