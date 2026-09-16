use super::*;
use rand::Rng;

pub(super) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if action == "soul_break" {
        return break_soul(db, state, account, req).await;
    }
    if action == "soul_weapon_limit_break" {
        return limit_break(db, state, account, req).await;
    }
    if matches!(
        action,
        "restore_soul_stone" | "get_soul_stone_mileage_reward" | "confirm_soul_stone"
    ) {
        return restore(db, state, account, req, action).await;
    }
    let slot = req.number(
        "EquipItemSlotIndex",
        req.number("TargetEquipItemSlotIndex", 0)?,
    )?;
    let eq = equip(db, account, slot).await?;
    let data = meta(state, eq.item_index)?;
    if data["IsOpenSoulWeapon"] != true {
        return Err(rule("InvalidEquipItem"));
    }
    let mut info = get(db, account, "soul", slot).await?;
    let mut out = item::success();
    if action == "soul_weapon_liberation" {
        if !info.is_null() {
            return Err(rule("AlreadyLiberated"));
        }
        let price = row(
            state,
            "SoulWeaponUpgradePrice",
            &[("ItemIndex", eq.item_index as i64), ("Grade", 0)],
        )?;
        out["CurrencyResult"] = hero::currency(db, account, "Gold", -n(price, "ReqGold")).await?;
        out["RemovedItemResultInfos"] = json!([item::consume(
            db,
            account,
            n(price, "MaterialItemIndex") as i32,
            n(price, "MaterialItemCount") as i32
        )
        .await?]);
        info = json!({"EquipItemSlotIndex":slot,"ItemIndex":eq.item_index,"Grade":0,"ReinforceLevel":0,"ReinforceRatio":0,"Exp":0,"OptionRatio1":500,"OptionRatio2":500,"OptionBonusRatio1":0,"OptionBonusRatio2":0,"RenewOptionCount":0,"CreatedTime":state.server_time_str()});
    } else {
        if info.is_null() {
            return Err(rule("SoulWeaponNotFound"));
        }
        match action {
            "soul_weapon_upgrade" => {
                let grade = n(&info, "Grade") + 1;
                row(state, "SoulWeaponUpgrade", &[("Grade", grade)])?;
                if n(&info, "ReinforceLevel")
                    < grade * state.tables.hero_shop.constant("SoulWeaponUpgradeRatio", 5)
                {
                    return Err(rule("NotEnoughReinforceLevel"));
                }
                let price = row(
                    state,
                    "SoulWeaponUpgradePrice",
                    &[("ItemIndex", eq.item_index as i64), ("Grade", grade)],
                )?;
                out["CurrencyResult"] =
                    hero::currency(db, account, "Gold", -n(price, "ReqGold")).await?;
                out["RemovedItemResultInfo"] = item::consume(
                    db,
                    account,
                    n(price, "MaterialItemIndex") as i32,
                    n(price, "MaterialItemCount") as i32,
                )
                .await?;
                info["Grade"] = json!(grade);
            }
            "soul_weapon_ether_injection" => {
                let level = n(&info, "ReinforceLevel");
                row(
                    state,
                    "SoulWeaponReinforce",
                    &[("ReinforceLevel", level + 1)],
                )?;
                let ruledata = row(state, "SoulWeaponReinforce", &[("ReinforceLevel", level)])?;
                let pairs = equipment::item_pairs(req, "InjectionItemInfos")?;
                if pairs.is_empty() {
                    return Err(rule("InvalidMaterial"));
                }
                let mut cost = 0;
                let mut exp = n(&info, "Exp");
                let mut rate = n(&info, "ReinforceRatio");
                let mut consumed = vec![];
                for (id, count) in pairs {
                    let ether = row(state, "SoulWeaponEther", &[("ItemIndex", id)])?;
                    if ruledata["UnAvailableItemCode"]
                        .as_array()
                        .is_some_and(|a| a.contains(&ether["ItemCode"]))
                    {
                        return Err(rule("InvalidMaterial"));
                    }
                    let bonus = row(
                        state,
                        "SoulWeaponEtherRate",
                        &[("ReinforceLevel", level), ("ItemIndex", id)],
                    )?;
                    cost += n(ether, "ReqGold") * count;
                    exp += n(ether, "ExpPoints") * count;
                    rate += n(bonus, "AddedBonusRate") * count;
                    consumed.push(item::consume(db, account, id as i32, count as i32).await?);
                }
                let max = state
                    .tables
                    .hero_shop
                    .constant("SoulWeaponReinforceMaxRate", 100000);
                if n(&info, "Exp") >= n(ruledata, "RequiredExp")
                    && n(&info, "ReinforceRatio") >= max
                {
                    return Err(rule("AlreadyMax"));
                }
                out["CurrencyResult"] = hero::currency(db, account, "Gold", -cost).await?;
                out["RemovedItemResultInfos"] = json!(consumed);
                info["Exp"] = json!(exp.min(n(ruledata, "RequiredExp")));
                info["ReinforceRatio"] = json!(rate.min(max));
            }
            "soul_weapon_reinforce" => {
                let level = n(&info, "ReinforceLevel");
                row(
                    state,
                    "SoulWeaponReinforce",
                    &[("ReinforceLevel", level + 1)],
                )?;
                let r = row(state, "SoulWeaponReinforce", &[("ReinforceLevel", level)])?;
                if n(&info, "Exp") < n(r, "RequiredExp")
                    || n(&info, "ReinforceRatio") < n(r, "SuccessRateMark") * 10
                {
                    return Err(rule("NotEnoughExp"));
                }
                out["CurrencyResult"] =
                    hero::currency(db, account, "Gold", -n(r, "ReqGold")).await?;
                let success = roll(
                    n(&info, "ReinforceRatio"),
                    state
                        .tables
                        .hero_shop
                        .constant("SoulWeaponReinforceMaxRate", 100000),
                );
                if success {
                    info["ReinforceLevel"] = json!(level + 1);
                }
                info["Exp"] = json!(0);
                info["ReinforceRatio"] = json!(0);
                out["ReinforceSuccess"] = json!(success);
            }
            "soul_weapon_renew_option" => {
                if info["PendingRenew"] == true {
                    return Err(rule("UnconfirmedOption"));
                }
                let coupon = req.number("CouponItemIndex", 0)?;
                if coupon > 0 {
                    if coupon
                        != state
                            .tables
                            .hero_shop
                            .constant("SoulWeaponRenewTicketItemIndex", 2202)
                    {
                        return Err(rule("InvalidMaterial"));
                    }
                    out["RemovedItemResultInfos"] =
                        json!([item::consume(db, account, coupon as i32, 1).await?]);
                } else {
                    let prices: Vec<i64> = serde_json::from_str(
                        state
                            .tables
                            .hero_shop
                            .constants
                            .get("SoulWeaponRenewGem")
                            .ok_or_else(|| rule("ItemDataNotFound"))?,
                    )
                    .map_err(|_| rule("ItemDataNotFound"))?;
                    let price = *prices
                        .get(
                            (n(&info, "RenewOptionCount") as usize)
                                .min(prices.len().saturating_sub(1)),
                        )
                        .ok_or_else(|| rule("ItemDataNotFound"))?;
                    out["CurrencyResult"] = hero::currency(db, account, "Gem", -price).await?;
                    out["RemovedItemResultInfos"] = json!([]);
                }
                let first = rand::thread_rng().gen_range(300..=700);
                info["RenewOptionRatio1"] = json!(first);
                info["RenewOptionRatio2"] = json!(1000 - first);
                for i in 1..=2 {
                    info[format!("RenewOptionBonusRatio{i}")] =
                        json!(rand::thread_rng().gen_range(0..=200));
                }
                info["RenewOptionCount"] = json!(n(&info, "RenewOptionCount") + 1);
                info["PendingRenew"] = json!(true);
                super::super::progression::record(db, account, "SoulWeaponRenewOption", 0, 0, 1)
                    .await?;
            }
            "soul_weapon_confirm_renew_option" => {
                if info["PendingRenew"] != true {
                    return Err(rule("UnconfirmedOption"));
                }
                if matches!(req.text("IsNew"), "true" | "True" | "1") {
                    for key in [
                        "OptionRatio1",
                        "OptionRatio2",
                        "OptionBonusRatio1",
                        "OptionBonusRatio2",
                    ] {
                        info[key] = info[format!("Renew{key}")].clone();
                    }
                }
                for key in [
                    "OptionRatio1",
                    "OptionRatio2",
                    "OptionBonusRatio1",
                    "OptionBonusRatio2",
                ] {
                    info[format!("Renew{key}")] = json!(0);
                }
                info["PendingRenew"] = json!(false);
            }
            "soul_weapon_transition" => {
                let target = req.number("ReceiveEquipItemSlotIndex", 0)?;
                if target == slot || info["PendingRenew"] == true {
                    return Err(rule("InvalidEquipItem"));
                }
                let other = equip(db, account, target).await?;
                if eq.locked != 0
                    || other.locked != 0
                    || other.item_index != eq.item_index
                    || !get(db, account, "soul", target).await?.is_null()
                {
                    return Err(rule("InvalidEquipItem"));
                }
                out["CurrencyResult"] = hero::currency(
                    db,
                    account,
                    "Gold",
                    -state
                        .tables
                        .hero_shop
                        .constant("SoulWeaponTransition", 10000000),
                )
                .await?;
                info["EquipItemSlotIndex"] = json!(target);
                put(db, account, "soul", target, &info).await?;
                sqlx::query(
                    "DELETE FROM extension_state WHERE account_id=? AND kind='soul' AND idx=?",
                )
                .bind(account)
                .bind(slot)
                .execute(db)
                .await?;
                out["ResultSoulWeaponInfo"] = info;
                return Ok(out);
            }
            _ => return Err(rule("Fail")),
        }
    }
    stats(state, &mut info)?;
    put(db, account, "soul", slot, &info).await?;
    out["ResultSoulWeaponInfo"] = info;
    Ok(out)
}
pub(super) fn stats(state: &AppState, info: &mut Value) -> Result<()> {
    let data = meta(state, n(info, "ItemIndex") as i32)?;
    let option = row(
        state,
        "SoulWeaponOption",
        &[("TagType", data["TagType"][0].as_i64().unwrap_or(0))],
    )?;
    let reinforce = row(
        state,
        "SoulWeaponReinforce",
        &[("ReinforceLevel", n(info, "ReinforceLevel"))],
    )?;
    let upgrade = row(state, "SoulWeaponUpgrade", &[("Grade", n(info, "Grade"))])?;
    let factor = (1000 + n(reinforce, "StatRate")) * (1000 + n(upgrade, "UpgradeRate")) / 1000;
    for i in 1..=2 {
        let base = n(info, &format!("OptionRatio{i}"))
            * n(option, &format!("OptionRatioPerValue{i}"))
            * factor
            / 1000
            / 1000;
        info[format!("OptionStat{i}")] = json!(base);
        info[format!("BonusOptionStat{i}")] =
            json!(base * n(info, &format!("OptionBonusRatio{i}")) / 1000);
    }
    Ok(())
}

async fn break_soul(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let slots = ids(req, "EquipItemSlotIndices")?;
    if slots.is_empty() {
        return Err(rule("InvalidMaterial"));
    }
    let mut rewards = Rewards::default();
    for slot in slots {
        let eq = equip(db, account, slot).await?;
        let info = get(db, account, "soul", slot).await?;
        if info.is_null() || info["PendingRenew"] == true {
            return Err(rule("SoulWeaponNotFound"));
        }
        // Removing the soul record inside this transaction lets the ordinary material checks apply.
        sqlx::query("DELETE FROM extension_state WHERE account_id=? AND kind='soul' AND idx=?")
            .bind(account)
            .bind(slot)
            .execute(&mut *db)
            .await?;
        material(db, account, slot).await?;
        let tag = meta(state, eq.item_index)?["TagType"][0]
            .as_i64()
            .unwrap_or(0);
        let r = row(
            state,
            "SoulBreak",
            &[
                ("TagType", tag),
                ("Grade", n(&info, "Grade")),
                ("ReinforceLevel", n(&info, "ReinforceLevel")),
            ],
        )?;
        for reward in r["Rewards"].as_array().into_iter().flatten() {
            let count = rand::thread_rng().gen_range(n(reward, "Min")..=n(reward, "Max"));
            if count > 0 {
                item::give(
                    db,
                    state,
                    account,
                    n(reward, "ItemIndex") as i32,
                    count as i32,
                    0,
                    0,
                    &mut rewards,
                )
                .await?;
            }
        }
        remove_equip(db, account, eq.slot_index).await?;
    }
    Ok(json!({"BaseResult":"Success","Result":"Success","ItemResults":rewards.items}))
}
async fn limit_break(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
) -> Result<Value> {
    let slot = req.number("EquipItemSlotIndex", 0)?;
    let mut eq = equip(db, account, slot).await?;
    if get(db, account, "soul", slot).await?.is_null() {
        return Err(rule("SoulWeaponNotFound"));
    }
    let data = meta(state, eq.item_index)?;
    // ContentsDefine.SoulWeaponLimitBreak was not shipped with the extracted tables.
    // An administrator can supply its native fields without changing the handler.
    let r = row(
        state,
        "SoulWeaponLimitBreak",
        &[
            ("DetailIndex", n(data, "DetailIndex")),
            ("Star", eq.star as i64 + 1),
        ],
    )?;
    let materials = ids(req, "MaterialSlotIndices")?;
    if materials.len() as i64 != n(r, "WeaponUniqueCount") {
        return Err(rule("InvalidMaterial"));
    }
    for material_slot in &materials {
        if *material_slot == slot {
            return Err(rule("InvalidMaterial"));
        }
        let m = material(db, account, *material_slot).await?;
        if m.item_index != eq.item_index {
            return Err(rule("InvalidMaterial"));
        }
    }
    let stone = n(
        row(
            state,
            "SoulWeaponUpgradePrice",
            &[("ItemIndex", eq.item_index as i64), ("Grade", 0)],
        )?,
        "MaterialItemIndex",
    );
    let expected: std::collections::BTreeMap<_, _> = [
        (stone, n(r, "SoulStoneCount")),
        (n(r, "TransStoneIndex"), n(r, "TransStoneCount")),
    ]
    .into_iter()
    .filter(|(_, c)| *c > 0)
    .collect();
    let sent: std::collections::BTreeMap<_, _> = equipment::item_pairs(req, "MaterialItemInfors")?
        .into_iter()
        .collect();
    if expected != sent {
        return Err(rule("InvalidMaterial"));
    }
    let currency = hero::currency(
        db,
        account,
        "Gold",
        -n(r, "GoldCount") * materials.len() as i64,
    )
    .await?;
    let mut removed = vec![];
    for (id, count) in expected {
        removed.push(item::consume(db, account, id as i32, count as i32).await?);
    }
    for slot in &materials {
        remove_equip(db, account, *slot as i32).await?;
    }
    let success = roll(
        n(r, "SuccessRatio") + eq.upgrade_star_fail_bonus as i64,
        100,
    );
    if success {
        eq.star += 1;
        eq.upgrade_star_fail_bonus = 0;
    } else {
        eq.upgrade_star_fail_bonus += n(r, "FailBonusRatio") as i32;
    }
    save_equip(db, account, &eq).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","Success":success,"ResultEquipItem":eq,"CurrencyResult":currency,"ItemResults":[],"RemovedEquipItemSlotIndices":materials,"RemovedItemResultInfos":removed}),
    )
}

async fn restore(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let mut progress = get(db, account, "soul_restore", 0).await?;
    if progress.is_null() {
        progress = json!({"Mileage":0,"Choices":[]});
    }
    let mut rewards = Rewards::default();
    let mut removed = vec![];
    let mut out = item::success();
    if action == "confirm_soul_stone" {
        let id = req.number("SoulStoneItemIndex", 0)?;
        if !progress["Choices"]
            .as_array()
            .is_some_and(|a| a.contains(&json!(id)))
        {
            return Err(rule("InvalidSoulStoneIndex"));
        }
        item::give(db, state, account, id as i32, 1, 0, 0, &mut rewards).await?;
        progress["Choices"] = json!([]);
        let r = item::reward_response(db, state, account, rewards).await?;
        out["ResultSoulStoneItemInfo"] = r["ItemResults"].get(0).cloned().unwrap_or(Value::Null);
    } else {
        if progress["Choices"]
            .as_array()
            .is_some_and(|a| !a.is_empty())
        {
            return Err(rule("UnconfirmedSoulStone"));
        }
        let index = req.number("SoulStoneRestoreIndex", 0)?;
        let r = state
            .tables
            .extensions
            .rows("SoulStoneRestore")
            .get(usize::try_from(index - 1).map_err(|_| rule("InvalidSoulStoneRestoreIndex"))?)
            .ok_or_else(|| rule("InvalidSoulStoneRestoreIndex"))?;
        if action == "get_soul_stone_mileage_reward" {
            let max = state.tables.hero_shop.constant("MaxSoulStoneMileage", 20);
            if n(&progress, "Mileage") < max {
                return Err(rule("NotEnoughMileage"));
            }
            item::reward(
                db,
                state,
                account,
                state
                    .tables
                    .hero_shop
                    .constant("SoulStoneMileageRewardIndex", 65001) as i32,
                &mut rewards,
            )
            .await?;
            progress["Mileage"] = json!(n(&progress, "Mileage") - max);
        } else {
            removed.push(
                item::consume(
                    db,
                    account,
                    n(r, "MaterialItemIndex") as i32,
                    n(r, "MaterialItemCount") as i32,
                )
                .await?,
            );
            let group = r["SoulStoneItemGroup"].as_str().unwrap_or("");
            let local = state
                .tables
                .extensions
                .find("LocalSoulStoneRestore", &[("Index", index)]);
            let mut local_pool = BTreeSet::new();
            if group.is_empty() {
                let config = local.ok_or_else(|| rule("ItemDataNotFound"))?;
                local_pool.extend(
                    config["ItemIndices"]
                        .as_array()
                        .into_iter()
                        .flatten()
                        .filter_map(Value::as_i64)
                        .map(|v| v as i32),
                );
                if config["UseLiberationStonePool"] == true {
                    for price in state
                        .tables
                        .extensions
                        .rows("SoulWeaponUpgradePrice")
                        .iter()
                        .filter(|v| n(v, "Grade") == 0)
                    {
                        let data = meta(state, n(price, "ItemIndex") as i32)?;
                        let npc = data["CreatureIndex"]
                            .as_array()
                            .into_iter()
                            .flatten()
                            .filter_map(Value::as_i64)
                            .any(|id| {
                                state
                                    .tables
                                    .extensions
                                    .rows("NPCFriendlyPoint")
                                    .iter()
                                    .any(|r| n(r, "HeroIndex") == id)
                            });
                        if !npc || config["IncludeNpcHeroes"] == true {
                            local_pool.insert(n(price, "MaterialItemIndex") as i32);
                        }
                    }
                }
            }
            let mut choices = BTreeSet::new();
            let needed = state.tables.hero_shop.constant("SelectSoulStoneCount", 3);
            for _ in 0..100 {
                if choices.len() as i64 >= needed {
                    break;
                }
                let id = if group.is_empty() {
                    let pool: Vec<_> = local_pool
                        .iter()
                        .copied()
                        .filter(|id| !choices.contains(id))
                        .collect();
                    if pool.is_empty() {
                        return Err(rule("ItemDataNotFound"));
                    }
                    pool[rand::thread_rng().gen_range(0..pool.len())]
                } else {
                    state
                        .tables
                        .roll_item_from_group_code(group, &[])
                        .ok_or_else(|| rule("ItemDataNotFound"))?
                        .0
                };
                if state.tables.items.reward_item(id).is_none() {
                    return Err(rule("ItemDataNotFound"));
                }
                choices.insert(id);
            }
            if choices.len() as i64 != needed {
                return Err(rule("ItemDataNotFound"));
            }
            progress["Choices"] = json!(choices);
            progress["Mileage"] = json!(n(&progress, "Mileage") + 1);
            super::super::progression::record(db, account, "SoulRestore", 0, 0, 1).await?;
        }
        let r = item::reward_response(db, state, account, rewards).await?;
        out["CurrencyResults"] = r["CurrencyResults"].clone();
        out["ItemResults"] = r["ItemResults"].clone();
        out["EquipItemResults"] = r["EquipItemResults"].clone();
        out["RemovedItemResultInfos"] = json!(removed);
        out["SoulStoneMileage"] = progress["Mileage"].clone();
        out["SelectSoulStoneIndices"] = progress["Choices"].clone();
    }
    put(db, account, "soul_restore", 0, &progress).await?;
    Ok(out)
}
