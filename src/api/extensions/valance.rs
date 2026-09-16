use super::*;
use rand::Rng;

pub(super) fn unique_options(state: &AppState, data: &Value, eq: &mut EquipItemInfo) -> Result<()> {
    let mut pool: BTreeSet<i64> = data["UniqueOptionIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
        .collect();
    for group in data["UniqueOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            pool.extend(ids.iter().map(|i| *i as i64));
        }
    }
    let count = if n(data, "EquipGrade") == 1 { 2 } else { 1 };
    let mut v = json!(eq);
    for i in 1..=count {
        let (id, step) = equipment::option(state, &pool.iter().copied().collect::<Vec<_>>())?;
        v[format!("ExtraOptionIndex{i}")] = json!(id);
        v[format!("ExtraOptionStep{i}")] = json!(step);
        if data["EnableDuplicationUniqueOption"] != true {
            pool.remove(&id);
        }
    }
    *eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
    Ok(())
}
pub(crate) fn select_unique(
    state: &AppState,
    eq: &mut EquipItemInfo,
    chosen: &[i32],
) -> Result<()> {
    let data = meta(state, eq.item_index)?;
    let count = if n(data, "EquipGrade") == 1 { 2 } else { 1 };
    if n(data, "EquipType") != 1 || chosen.len() != count {
        return Err(rule("NoAvailableOption"));
    }
    let mut pool: BTreeSet<i64> = data["UniqueOptionIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
        .collect();
    for group in data["UniqueOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            pool.extend(ids.iter().map(|v| *v as i64));
        }
    }
    let mut v = json!(eq);
    for (slot, id) in chosen.iter().enumerate() {
        if !pool.contains(&(*id as i64)) {
            return Err(rule("NoAvailableOption"));
        }
        let option = state
            .tables
            .inventory
            .options
            .get(id)
            .ok_or_else(|| rule("NoAvailableOption"))?;
        v[format!("ExtraOptionIndex{}", slot + 1)] = json!(id);
        v[format!("ExtraOptionStep{}", slot + 1)] = json!(n(option, "Steps"));
        if data["EnableDuplicationUniqueOption"] != true {
            pool.remove(&(*id as i64));
        }
    }
    v["Identified"] = json!(1);
    *eq = serde_json::from_value(v).map_err(|_| rule("NoAvailableOption"))?;
    Ok(())
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "valance_craft" {
        return craft(db, state, account, req).await;
    }
    let mut out = item::success();
    if action == "valance_identified" {
        let slots = ids(req, "EquipItemSlotIndices")?;
        if slots.is_empty()
            || slots.len() as i64
                > state
                    .tables
                    .hero_shop
                    .constant("MaxValanceIdentifiedCount", 10)
        {
            return Err(rule("InvalidItemCount"));
        }
        let mut cost = 0;
        let mut results = vec![];
        let mut identified = vec![];
        for slot in slots {
            let mut eq = equip(db, account, slot).await?;
            let data = meta(state, eq.item_index)?;
            if eq.identified != 0 || n(data, "EquipType") != 1 {
                return Err(rule("AlreadyIdentified"));
            }
            cost += n(
                row(
                    state,
                    "ValanceIdentified",
                    &[("DetailIndex", n(data, "DetailIndex"))],
                )?,
                "ReqGold",
            );
            unique_options(state, data, &mut eq)?;
            eq.identified = 1;
            save_equip(db, account, &eq).await?;
            let mut v = json!(eq);
            v["EquipItemSlotIndex"] = json!(eq.slot_index);
            identified.push(v);
            results.push(eq);
        }
        out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
        out["EquipItemResults"] = json!(results);
        out["EquipItemIdentifiedInfos"] = json!(identified);
        return Ok(out);
    }
    // The extracted tier-upgrade request has no target or material fields.
    if action == "valance_tier_upgrade" {
        return Err(rule("Fail"));
    }
    let mut eq = equip(db, account, req.number("EquipItemSlotIndex", 0)?).await?;
    let data = meta(state, eq.item_index)?;
    if n(data, "EquipType") != 1 || eq.identified == 0 {
        return Err(rule("InvalidEquipItem"));
    }
    match action {
        "valance_awaken" => {
            let r = row(
                state,
                "ValanceAwaken",
                &[
                    ("Tier", n(data, "Tier")),
                    ("DetailIndex", n(data, "DetailIndex")),
                    ("CreatureTagType", data["TagType"][0].as_i64().unwrap_or(0)),
                    ("Star", eq.star as i64),
                ],
            )?;
            out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
            out["ItemResults"] = json!([item::consume(
                db,
                account,
                n(r, "ItemIndex1") as i32,
                n(r, "ItemCount1") as i32
            )
            .await?]);
            eq.star += 1;
            eq.upgrade_star_fail_bonus = 0;
            out["Success"] = json!(true);
            super::super::progression::record(db, account, "AwakenGear", 0, 0, 1).await?;
        }
        "valance_enchant" => {
            let item_id = req.number("ConsumeItemIndex", 0)?;
            let r = row(
                state,
                "ValanceEnchant",
                &[
                    ("ItemIndex", item_id),
                    ("PartType", n(data, "PartType")),
                    ("SubType", n(data, "SubType")),
                ],
            )?;
            let pool = state
                .tables
                .inventory
                .option_groups
                .get(&(n(r, "OptionGroupIndex") as i32))
                .ok_or_else(|| rule("InvalidOption"))?;
            let rows: Vec<_> = state
                .tables
                .extensions
                .rows("ValanceEnchantOptionCount")
                .iter()
                .filter(|v| n(v, "ItemIndex") == item_id)
                .collect();
            let total: i64 = rows.iter().map(|v| n(v, "Rate")).sum();
            if total <= 0 {
                return Err(rule("ItemDataNotFound"));
            }
            let mut roll = rand::thread_rng().gen_range(0..total);
            let count = n(
                rows.into_iter()
                    .find(|v| {
                        roll -= n(v, "Rate");
                        roll < 0
                    })
                    .unwrap(),
                "OptionCount",
            );
            if !(1..=3).contains(&count) {
                return Err(rule("InvalidOption"));
            }
            let mut pending =
                json!({"ConsumeItemIndex":item_id,"EnchantEquipItemSlotIndex":eq.slot_index});
            let mut candidates: Vec<_> = pool.iter().map(|i| *i as i64).collect();
            for i in 1..=3 {
                let (id, step) = if i <= count {
                    equipment::option(state, &candidates)?
                } else {
                    (0, 0)
                };
                candidates.retain(|v| *v != id);
                pending[format!("EnchantOptionIndex{i}")] = json!(id);
                pending[format!("EnchantOptionStep{i}")] = json!(step);
            }
            equipment::pending(db, account, eq.slot_index, "valance_enchant", &pending).await?;
            out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
            out["ItemResultInfo"] = item::consume(db, account, item_id as i32, 1).await?;
            out["NewValanceEnchantOptionInfo"] = pending;
            super::super::progression::record(db, account, "Enchant", 0, 0, 1).await?;
        }
        "confirm_valance_enchant" => {
            let pending:String=sqlx::query_scalar("SELECT data FROM equipment_pending WHERE account_id=? AND slot_index=? AND kind='valance_enchant'")
                .bind(account).bind(eq.slot_index).fetch_optional(&mut *db).await?.ok_or_else(||rule("UnconfirmedOption"))?;
            let p: Value = serde_json::from_str(&pending).map_err(|_| rule("InvalidOption"))?;
            if matches!(req.text("IsNew"), "true" | "True" | "1") {
                let mut v = json!(eq);
                for i in 1..=3 {
                    for field in ["Index", "Step"] {
                        let key = format!("EnchantOption{field}{i}");
                        v[&key] = p[&key].clone();
                    }
                }
                eq = serde_json::from_value(v).map_err(|_| rule("InvalidOption"))?;
            }
            sqlx::query("DELETE FROM equipment_pending WHERE account_id=? AND slot_index=? AND kind='valance_enchant'").bind(account).bind(eq.slot_index).execute(&mut *db).await?;
        }
        _ => return Err(rule("Fail")),
    }
    save_equip(db, account, &eq).await?;
    out["ResultEquipItem"] = json!(eq);
    Ok(out)
}
async fn craft(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let r = row(
        state,
        "ValanceCraft",
        &[
            ("SetType", req.number("SetType", 0)?),
            ("CreatureTagType", req.number("CreatureTagType", 0)?),
            ("EquipPartType", req.number("PartType", 0)?),
            ("EquipItemSubType", req.number("SubPartType", 0)?),
        ],
    )?;
    let chosen = ids(req, "SelectEquipOptionIndices")?;
    let mut allowed = BTreeSet::new();
    for group in r["EquipOptionGroupIndex"]
        .as_array()
        .into_iter()
        .flatten()
        .filter_map(Value::as_i64)
    {
        if let Some(ids) = state.tables.inventory.option_groups.get(&(group as i32)) {
            allowed.extend(ids.iter().map(|v| *v as i64));
        }
    }
    if chosen.iter().any(|id| !allowed.contains(id)) {
        return Err(rule("InvalidOption"));
    }
    let mut out = item::success();
    out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
    let mut removed = vec![
        item::consume(
            db,
            account,
            n(r, "MaterialItemIndex") as i32,
            (n(r, "MaterialItemCount")
                + if chosen.is_empty() {
                    0
                } else {
                    n(r, "AdditionalMaterialItemCount")
                }) as i32,
        )
        .await?,
    ];
    if !chosen.is_empty() {
        removed.push(
            item::consume(
                db,
                account,
                n(r, "SubMaterialItemIndex") as i32,
                n(r, "SubMaterialItemCount") as i32,
            )
            .await?,
        );
    }
    let (id, count, star, _) = state
        .tables
        .roll_item_from_group_code(r["ResultItemGroupCode"].as_str().unwrap_or(""), &[])
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    if count != 1 {
        return Err(rule("InvalidRewardData"));
    }
    let mut rewards = Rewards::default();
    item::give(db, state, account, id, 1, star, 0, &mut rewards).await?;
    let eq = rewards
        .equipment
        .get_mut(0)
        .ok_or_else(|| rule("InvalidRewardData"))?;
    if !chosen.is_empty() {
        item::make_options(
            state,
            id,
            eq,
            &chosen.iter().map(|v| *v as i32).collect::<Vec<_>>(),
        )?;
    }
    eq.identified = 0;
    save_equip(db, account, eq).await?;
    out["EquipItemResult"] = json!(eq);
    out["ItemResults"] = json!(removed);
    out["IsAdvanced"] = json!(n(meta(state, id)?, "EquipGrade") == 1);
    Ok(out)
}
