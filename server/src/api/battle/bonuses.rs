//! Reward bonuses shared by battle settlement and its regression tests.
use super::*;

pub(crate) async fn calculate(
    db: &mut PgConnection,
    s: &AppState,
    a: i64,
    battle: i64,
) -> Result<(i64, i64)> {
    let honor_gold = crate::api::inventory::honor_buff::bonus(db, s).await?;
    let guild_bonus = crate::api::community::guild_reward_boost(db, s, a).await?;
    crate::api::inventory::item::normalize_booster_timers(db, s, a).await?;
    let active: Vec<i32> = crate::db::query_scalar(
        "SELECT item_index FROM item_boosters WHERE account_id=$1 AND end_time>sprk_datetime('now')",
    )
    .bind(a)
    .fetch_all(&mut *db)
    .await?;
    let (mut gold, mut exp) = (0, 0);
    for id in active {
        if let Some(b) = s.tables.inventory.booster_effect(id) {
            if b["BattleTypes"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(battle)))
            {
                match n(b, "Type") {
                    1 => exp += n(b, "Value"),
                    3 => gold += n(b, "Value"),
                    _ => {}
                }
            }
        }
    }
    let costumes: Vec<i32> =
        crate::db::query_scalar("SELECT costume_index FROM costumes WHERE account_id=$1")
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
    Ok((gold + guild_bonus.0 + honor_gold, exp + guild_bonus.1))
}

/// Dispatch stamina saving, as the native dispatch window shows it: the sum of
/// `SaveDispatchStamina` booster abilities on owned Legend costumes.
pub(crate) async fn dispatch_stamina_discount(
    db: &mut PgConnection,
    s: &AppState,
    a: i64,
) -> Result<i64> {
    const LEGEND: i64 = 2;
    const BOOSTER: i64 = 1;
    let costumes: Vec<i32> =
        crate::db::query_scalar("SELECT costume_index FROM costumes WHERE account_id=$1")
            .bind(a)
            .fetch_all(db)
            .await?;
    let mut rate = 0;
    for id in costumes {
        let Some(c) = s.tables.hero_shop.costumes.get(&id) else { continue };
        if n(c, "CostumeType") != LEGEND { continue; }
        for i in 1..=3 {
            let v = &c[format!("AbilityValue{i}")];
            if n(c, &format!("AbilityType{i}")) == BOOSTER && v[0] == "SaveDispatchStamina" {
                rate += v[1].as_str().and_then(|s| s.parse::<i64>().ok()).unwrap_or(0);
            }
        }
    }
    Ok(rate.clamp(0, 100))
}
