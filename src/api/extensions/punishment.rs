use super::*;
use rand::Rng;

pub(crate) async fn attached(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<bool> {
    Ok(sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM hero_details h,json_each(h.data,'$.HeroRunePageInfos') r WHERE h.account_id=? AND json_extract(r.value,'$.EquipItemSlotIndex')=?)").bind(account).bind(slot).fetch_one(db).await?)
}
pub(crate) async fn available(
    db: &mut SqliteConnection,
    account: i64,
    slot: i64,
) -> Result<EquipItemInfo> {
    let r = sqlx::query(
        "SELECT * FROM equip_items WHERE account_id=? AND slot_index=? AND inventory_type IN (0,3)",
    )
    .bind(account)
    .bind(slot)
    .fetch_optional(&mut *db)
    .await?
    .ok_or_else(|| rule("EquipItemNotFound"))?;
    let eq = EquipItemInfo::from_row(&r);
    if eq.locked != 0 || attached(db, account, slot).await? {
        return Err(rule("LockedEquip"));
    }
    Ok(eq)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let settings = get(db, account, "punishment_storage", 0).await?;
    let extend = n(&settings, "Extend");
    let storage = state
        .tables
        .inventory
        .extensions
        .iter()
        .find(|r| n(r, "InventoryType") == 3)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    if action == "extend_punishment_rune_storage_slot" {
        let cost = n(storage, "EquipItemExtendPrice");
        if req.number("ReqGem", -1)? != cost || extend >= n(storage, "EquipItemExtendCount") {
            return Err(rule("InvalidPrice"));
        }
        let currency = hero::currency(db, account, "Gem", -cost).await?;
        put(
            db,
            account,
            "punishment_storage",
            0,
            &json!({"Extend":extend+1}),
        )
        .await?;
        return Ok(
            json!({"BaseResult":"Success","Result":"Success","CurrencyResult":currency,"PunishmentRuneStorageExtend":extend+1}),
        );
    }
    let id = req.number("PunishmentRuneIndex", 0)?;
    let data = row(state, "PunishmentRune", &[("PunishmentRuneIndex", id)])?;
    let owned:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=? AND punishment_rune_option IS NOT NULL").bind(account).fetch_one(&mut *db).await?;
    let max = state
        .tables
        .hero_shop
        .constant("MaxPunishmentRuneItemCount", 100)
        + extend * n(storage, "EquipItemExtendSize");
    if owned >= max {
        return Err(rule("EquipItemFull"));
    }
    let cost = row(
        state,
        "PunishmentRuneItemCount",
        &[("PunishmentRuneCount", owned)],
    )?;
    let ingredients: Vec<i64> = serde_json::from_str(req.text("IngredientRuneIndices"))
        .map_err(|_| rule("InvalidMaterial"))?;
    if ingredients.is_empty() || ingredients.len() > 100 {
        return Err(rule("InvalidMaterial"));
    }
    let tags: Vec<String> = serde_json::from_str(
        state
            .tables
            .hero_shop
            .constants
            .get("PunishmentRuneIngredientRuneTags")
            .ok_or_else(|| rule("ItemDataNotFound"))?,
    )
    .map_err(|_| rule("ItemDataNotFound"))?;
    let gauges: Vec<i64> = serde_json::from_str(
        state
            .tables
            .hero_shop
            .constants
            .get("PunishmentRuneIngredientRuneGaugePerRune")
            .ok_or_else(|| rule("ItemDataNotFound"))?,
    )
    .map_err(|_| rule("ItemDataNotFound"))?;
    let mut gauge = 0;
    let mut counts = std::collections::BTreeMap::<i64, i32>::new();
    for id in ingredients {
        let r = row(state, "RuneItem", &[("ItemIndex", id)])?;
        let amount = tags
            .iter()
            .enumerate()
            .filter(|(_, tag)| r["Tag"].as_array().is_some_and(|v| v.contains(&json!(tag))))
            .filter_map(|(i, _)| gauges.get(i))
            .max()
            .copied()
            .ok_or_else(|| rule("InvalidMaterial"))?;
        gauge += amount;
        *counts.entry(id).or_default() += 1;
    }
    if gauge
        != state
            .tables
            .hero_shop
            .constant("PunishmentRuneMaxGauge", 1000)
    {
        return Err(rule("InvalidMaterial"));
    }
    for (id, count) in [
        (n(data, "VoidRuneIndex"), 1),
        (n(data, "LegendRuneIndex"), 1),
        (
            n(data, "ApostleBloodIndex"),
            n(cost, "ApostleBloodCount") as i32,
        ),
        (
            state
                .tables
                .hero_shop
                .constant("RuneFragmentItemIndex", 45005),
            n(cost, "RuneFragmentCount") as i32,
        ),
    ] {
        *counts.entry(id).or_default() += count;
    }
    let mut removed = vec![];
    for (id, count) in counts {
        removed.push(item::consume(db, account, id as i32, count).await?);
    }
    let mut rewards = Rewards::default();
    super::super::tutorial::equipment(
        db,
        account,
        EquipItemInfo::new(0, id as i32, 0, state.server_time_str()),
        &mut rewards,
    )
    .await?;
    let eq = &mut rewards.equipment[0];
    eq.inventory_type = 3;
    let option = row(
        state,
        "PunishmentRuneOption",
        &[("Index", n(data, "MainOptionIndex"))],
    )?;
    let mut stats = json!({"EquipItemSlotIndex":eq.slot_index,"MainOptionIndex":n(data,"MainOptionIndex"),"MainOptionValue":rand::thread_rng().gen_range(n(option,"OptionValueMin0")..=n(option,"OptionValueMax0"))});
    for i in 1..=3 {
        stats[format!("ExtraSkillIndex{i}")] = data[format!("ExtraSkillIndex{i}")].clone();
        stats[format!("ExtraSkillValue{i}")] = json!(rand::thread_rng().gen_range(
            n(data, &format!("ExtraSkillValue{i}Min"))..=n(data, &format!("ExtraSkillValue{i}Max"))
        ));
    }
    eq.punishment_rune_option = Some(stats);
    save_equip(db, account, eq).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","ItemResults":removed,"EquipItemResult":eq}),
    )
}
pub(crate) async fn dismantle(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    rewards: &mut Rewards,
) -> Result<()> {
    let slots = ids(req, "EquipItemSlotIndices")?;
    let drop = row(
        state,
        "PunishmentRuneItemBreak",
        &[(
            "BreakIndex",
            state
                .tables
                .hero_shop
                .constant("PunishmentRuneBreakIndex", 1),
        )],
    )?;
    for slot in slots {
        let eq = available(db, account, slot).await?;
        row(
            state,
            "PunishmentRune",
            &[("PunishmentRuneIndex", eq.item_index as i64)],
        )?;
        for i in 1..=5 {
            let max = n(drop, &format!("MaxItemCount{i}"));
            if max > 0 {
                let count =
                    rand::thread_rng().gen_range(n(drop, &format!("MinItemCount{i}"))..=max);
                item::give(
                    db,
                    state,
                    account,
                    n(drop, &format!("ItemIndex{i}")) as i32,
                    count as i32,
                    0,
                    0,
                    rewards,
                )
                .await?;
            }
        }
        remove_equip(db, account, eq.slot_index).await?;
    }
    Ok(())
}
