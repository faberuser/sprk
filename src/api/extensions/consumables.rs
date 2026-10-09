use super::*;

pub(crate) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    if matches!(action, "use_flask_item" | "cancel_flask_item") {
        return flask(db, state, account, req, action).await;
    }
    let id = item::item_index(req, "ItemIndex")?;
    let mut out = item::success();
    match action {
        "use_nick_change_item" | "use_guild_name_change_item" => {
            let guild = action == "use_guild_name_change_item";
            if n(item::data(state, id)?, "Type") != if guild { 25 } else { 24 } {
                return Err(rule("ItemTypeMismatch"));
            }
            let name = req.text(if guild { "Name" } else { "Nick" }).trim();
            let min = state.tables.hero_shop.constant(
                if guild {
                    "MinGuildNameLength"
                } else {
                    "MinNickLength"
                },
                2,
            );
            let max = state.tables.hero_shop.constant(
                if guild {
                    "MaxGuildNameLength"
                } else {
                    "MaxNickLength"
                },
                12,
            );
            if (name.chars().count() as i64) < min {
                return Err(rule("TooShortName"));
            }
            if name.chars().count() as i64 > max {
                return Err(rule("TooLongName"));
            }
            if name
                .chars()
                .any(|c| c.is_control() || matches!(c, '<' | '>' | '[' | ']'))
            {
                return Err(rule("InvalidName"));
            }
            if guild {
                let guild_id: i64 =
                    sqlx::query_scalar("SELECT guild_id FROM guilds WHERE master_account_id=?")
                        .bind(account)
                        .fetch_optional(&mut *db)
                        .await?
                        .ok_or_else(|| rule("NotGuildMaster"))?;
                let duplicate: bool = sqlx::query_scalar(
                    "SELECT EXISTS(SELECT 1 FROM guilds WHERE name=? COLLATE NOCASE)",
                )
                .bind(name)
                .fetch_one(&mut *db)
                .await?;
                if duplicate {
                    return Err(rule("AlreadyExistName"));
                }
                sqlx::query("UPDATE guilds SET name=? WHERE guild_id=?")
                    .bind(name)
                    .bind(guild_id)
                    .execute(&mut *db)
                    .await?;
            } else {
                let duplicate: bool = sqlx::query_scalar(
                    "SELECT EXISTS(SELECT 1 FROM accounts WHERE nick=? COLLATE NOCASE)",
                )
                .bind(name)
                .fetch_one(&mut *db)
                .await?;
                if duplicate {
                    return Err(rule("AlreadyExistName"));
                }
                sqlx::query("UPDATE accounts SET nick=? WHERE account_id=?")
                    .bind(name)
                    .bind(account)
                    .execute(&mut *db)
                    .await?;
            }
            out["ItemResult"] = item::consume(db, account, id, 1).await?;
        }
        "use_accessory_select_item" => {
            let selector = row(state, "AccessorySelectItem", &[("ItemIndex", id as i64)])?;
            let accessory = req.number("AccessoryCostumeIndex", 0)?;
            if accessory != n(selector, "AccessoryIndex") {
                return Err(rule("InvalidCostume"));
            }
            let data = row(state, "AccessoryCostume", &[("Index", accessory)])?;
            let eligible = |hero_id: i32| {
                !selector["TargetHero"]
                    .as_array()
                    .is_some_and(|v| !v.is_empty() && !v.contains(&json!(hero_id)))
                    && !data["UnableHero"]
                        .as_array()
                        .is_some_and(|v| v.contains(&json!(hero_id)))
            };
            out["ItemResult"] = item::consume(db, account, id, 1).await?;
            if matches!(req.text("GetOtherReward"), "true" | "True" | "1") {
                // The client sends HeroIndex=0 here and checks the entire eligible roster,
                // including unrecruited heroes, before offering the alternative reward.
                let mut eligible_count = 0;
                for (&hero_id, hero_data) in &state.tables.hero_shop.heroes {
                    if matches!(n(hero_data, "OpenType"), 1 | 3 | 4 | 6) && eligible(hero_id) {
                        eligible_count += 1;
                        if get(
                            db,
                            account,
                            "accessory",
                            cosmetics::accessory_key(hero_id, accessory)?,
                        )
                        .await?
                        .is_null()
                        {
                            return Err(rule("AllCostumeNotOwned"));
                        }
                    }
                }
                if eligible_count == 0 || n(selector, "AllOwnedRewardIndex") <= 0 {
                    return Err(rule("AllCostumeNotOwned"));
                }
                let mut rewards = Rewards::default();
                item::reward(
                    db,
                    state,
                    account,
                    n(selector, "AllOwnedRewardIndex") as i32,
                    &mut rewards,
                )
                .await?;
                let r = item::reward_response(db, state, account, rewards).await?;
                for key in ["CurrencyResults", "EquipItemResults", "ItemResults"] {
                    out[key] = r[key].clone();
                }
            } else {
                let hero_id = item::item_index(req, "HeroIndex")?;
                let h = hero::info(db, account, hero_id).await?;
                if !eligible(hero_id) {
                    return Err(rule("NotCorrectHero"));
                }
                let ownership_key = cosmetics::accessory_key(hero_id, accessory)?;
                let old = get(db, account, "accessory", ownership_key).await?;
                if !old.is_null() {
                    return Err(rule("AlreadyHaveCostume"));
                }
                let raw: Value = serde_json::from_str(req.text("PositionInfo"))
                    .map_err(|_| rule("InvalidCostume"))?;
                let encoded = json!({accessory.to_string():raw}).to_string();
                cosmetics::positions(&encoded)?;
                let info = json!({"HeroIndex":hero_id,"AccessoryCostumeIndex":accessory,"PositionInfo":raw.to_string(),"CreatedTime":state.server_time_str()});
                put(db, account, "accessory", ownership_key, &info).await?;
                let mut appearance = hero::appearance(&h);
                let part = n(data, "PartType");
                if !(1..=6).contains(&part) {
                    return Err(rule("InvalidCostume"));
                }
                appearance[format!("AccessoryCostumeIndex{part}")] = json!(accessory);
                let mut details = hero::details(db, account, hero_id).await?;
                details[format!("AccessoryCostumeIndex{part}")] = json!(accessory);
                hero::save_details(db, account, hero_id, &details).await?;
                out["PlayerAccessoryCostumeInfo"] = info;
                out["HeroCostumeResultInfo"] = appearance;
                out["CurrencyResults"] = json!([]);
                out["EquipItemResults"] = json!([]);
                out["ItemResults"] = json!([]);
            }
        }
        "use_pet_select_item" => {
            let selector = row(state, "PetSelectItem", &[("ItemIndex", id as i64)])?;
            let pet = req.number("PetIndex", 0)?;
            if !selector["PetIndices"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(pet)))
            {
                return Err(rule("InvalidPet"));
            }
            if !get(db, account, "pet", pet).await?.is_null() {
                return Err(rule("AlreadyHavePet"));
            }
            let info = json!({"PetIndex":pet,"Star":n(selector,"PetStar"),"CreatedTime":state.server_time_str(),"HappinessPoint":0,"FullPoint":0,"Status":0});
            let consumed = item::consume(db, account, id, 1).await?;
            put(db, account, "pet", pet, &info).await?;
            out["ItemResults"] = json!([consumed]);
            out["ItemUseResults"] = json!([]);
            out["PetAddResultInfo"] = json!({"PetSoulResults":[],"PetResult":info});
        }
        "awaken_transition" | "soul_weapon_transition_ticket" => {
            return transition(db, state, account, req, action, id).await;
        }
        "soul_weapon_ability_ticket" => {
            return ability(db, state, account, req, id).await;
        }
        "use_recipe_item" => {
            // RecipeItemTable is absent in this extraction; configured rows use explicit costs/rewards.
            let recipe = row(state, "RecipeItem", &[("ItemIndex", id as i64)])?;
            let mut removed = vec![item::consume(db, account, id, 1).await?];
            for material in recipe["Materials"].as_array().into_iter().flatten() {
                removed.push(
                    item::consume(
                        db,
                        account,
                        n(material, "ItemIndex") as i32,
                        n(material, "Count") as i32,
                    )
                    .await?,
                );
            }
            out["CurrencyResult"] =
                hero::currency(db, account, "Gold", -n(recipe, "ReqGold")).await?;
            let mut rewards = Rewards::default();
            item::reward(
                db,
                state,
                account,
                n(recipe, "RewardIndex") as i32,
                &mut rewards,
            )
            .await?;
            removed.extend(rewards.items);
            out["ItemResults"] = json!(removed);
            out["EquipItemInfos"] = json!(rewards.equipment);
        }
        _ => return Err(rule("Fail")),
    }
    Ok(out)
}
async fn transition(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
    id: i32,
) -> Result<Value> {
    let ticket = row(state, "EquipTransitionTicket", &[("ItemIndex", id as i64)])?;
    let soul = action == "soul_weapon_transition_ticket";
    if n(ticket, "Type") != if soul { 1 } else { 0 } {
        return Err(rule("ItemTypeMismatch"));
    }
    let source = req.number("SourceEquipItemSlotIndex", 0)?;
    let target = req.number("TargetEquipItemSlotIndex", 0)?;
    if source == target {
        return Err(rule("InvalidEquipItem"));
    }
    let mut a = equip(db, account, source).await?;
    let mut b = equip(db, account, target).await?;
    for (eq, key) in [(&a, "FixedGiveItem"), (&b, "FixedGetItem")] {
        let detail = n(meta(state, eq.item_index)?, "DetailIndex");
        if eq.locked != 0
            || eq.star > 5
            || !ticket[key]
                .as_array()
                .is_some_and(|v| v.contains(&json!(eq.item_index)))
            || !ticket["SelectAbleDetailIndex"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(detail)))
        {
            return Err(rule("InvalidEquipItem"));
        }
        let pending: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=? AND slot_index=?)",
        )
        .bind(account)
        .bind(eq.slot_index)
        .fetch_one(&mut *db)
        .await?;
        if pending {
            return Err(rule("UnconfirmedOption"));
        }
    }
    let excluded = |eq: &EquipItemInfo| {
        ticket["ExceptItemGroup1"]
            .as_array()
            .is_some_and(|v| v.contains(&json!(eq.item_index)))
    };
    if excluded(&a) != excluded(&b) {
        return Err(rule("InvalidEquipItem"));
    }
    let mut out = item::success();
    if soul {
        let mut first = get(db, account, "soul", source).await?;
        let mut second = get(db, account, "soul", target).await?;
        if first.is_null() || first["PendingRenew"] == true || second["PendingRenew"] == true {
            return Err(rule("InvalidEquipItem"));
        }
        if second.is_null() {
            return Err(rule("SoulWeaponNotFound"));
        }
        first["EquipItemSlotIndex"] = json!(target);
        first["ItemIndex"] = json!(b.item_index);
        second["EquipItemSlotIndex"] = json!(source);
        second["ItemIndex"] = json!(a.item_index);
        soul::stats(state, &mut first)?;
        soul::stats(state, &mut second)?;
        put(db, account, "soul", target, &first).await?;
        put(db, account, "soul", source, &second).await?;
        out["SourceSoulWeaponInfo"] = second;
        out["TargetSoulWeaponInfo"] = first;
    } else {
        if (a.star as i64) < n(ticket, "LeastStar") || b.star >= a.star {
            return Err(rule("InvalidEquipItem"));
        }
        std::mem::swap(&mut a.star, &mut b.star);
        std::mem::swap(
            &mut a.upgrade_star_fail_bonus,
            &mut b.upgrade_star_fail_bonus,
        );
        save_equip(db, account, &a).await?;
        save_equip(db, account, &b).await?;
        out["SourceEquipItemResult"] = json!(a);
        out["TargetEquipItemResult"] = json!(b);
    }
    out["ItemResult"] = item::consume(db, account, id, 1).await?;
    Ok(out)
}
async fn ability(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    id: i32,
) -> Result<Value> {
    let ticket = row(
        state,
        "SoulWeaponAbilityTicket",
        &[("ItemIndex", id as i64)],
    )?;
    let slot = req.number("TargetEquipItemSlotIndex", 0)?;
    let eq = equip(db, account, slot).await?;
    if ticket["FixedEquipItemIndex"]
        .as_array()
        .is_some_and(|v| !v.is_empty() && !v.contains(&json!(eq.item_index)))
    {
        return Err(rule("InvalidEquipItem"));
    }
    let mut info = get(db, account, "soul", slot).await?;
    if info.is_null() || info["PendingRenew"] == true {
        return Err(rule("SoulWeaponNotFound"));
    }
    let kind = n(ticket, "ItemType");
    let grade = n(ticket, "SoulWeaponGrade");
    let level = n(ticket, "EtherReinforceLevel");
    if (kind == 1 && grade <= n(&info, "Grade"))
        || (kind == 2 && level <= n(&info, "ReinforceLevel"))
        || (kind == 3 && (grade < n(&info, "Grade") || level < n(&info, "ReinforceLevel")))
    {
        return Err(rule("InvalidEquipItem"));
    }
    let tag = meta(state, eq.item_index)?["TagType"][0]
        .as_i64()
        .unwrap_or(0);
    let refund = row(
        state,
        "SoulWeaponAbilityReward",
        &[
            ("Type", kind),
            ("TagType", tag),
            ("Grade", n(&info, "Grade")),
            ("ReinforceLevel", n(&info, "ReinforceLevel")),
        ],
    )?;
    let mut rewards = Rewards::default();
    for (i, code) in refund["ItemCodeList"]
        .as_array()
        .into_iter()
        .flatten()
        .enumerate()
    {
        let item_id = state
            .tables
            .hero_shop
            .items
            .iter()
            .find(|(_, v)| v["Code"] == *code)
            .map(|(id, _)| *id)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        item::give(
            db,
            state,
            account,
            item_id,
            refund["ItemCountList"][i].as_i64().unwrap_or(0) as i32,
            0,
            0,
            &mut rewards,
        )
        .await?;
    }
    if kind == 1 || kind == 3 {
        row(state, "SoulWeaponUpgrade", &[("Grade", grade)])?;
        info["Grade"] = json!(grade);
    }
    if kind == 2 || kind == 3 {
        row(state, "SoulWeaponReinforce", &[("ReinforceLevel", level)])?;
        info["ReinforceLevel"] = json!(level);
        info["Exp"] = json!(0);
        info["ReinforceRatio"] = json!(0);
    }
    if !(1..=3).contains(&kind) {
        return Err(rule("ItemTypeMismatch"));
    }
    soul::stats(state, &mut info)?;
    let consumed = item::consume(db, account, id, 1).await?;
    put(db, account, "soul", slot, &info).await?;
    Ok(
        json!({"BaseResult":"Success","Result":"Success","ItemResult":consumed,"ItemRefundsResults":rewards.items,"TargetSoulWeaponInfo":info}),
    )
}
async fn flask(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let hero_id = item::item_index(req, "HeroIndex")?;
    hero::info(db, account, hero_id).await?;
    let mut details = hero::details(db, account, hero_id).await?;
    let mut out = item::success();
    if action == "cancel_flask_item" {
        let old = n(&details, "FlaskItemIndex");
        let data = row(state, "FlaskItem", &[("ItemIndex", old)])?;
        if data["IsUnEquip"] != true {
            return Err(rule("CannotUnequip"));
        }
        let mut rewards = Rewards::default();
        item::give(db, state, account, old as i32, 1, 0, 0, &mut rewards).await?;
        out["ItemResult"] = rewards.items.first().cloned().unwrap_or(Value::Null);
        details["FlaskItemIndex"] = json!(0);
        details["FlaskExp"] = json!(0);
    } else {
        if n(&details, "FlaskItemIndex") != 0 {
            return Err(rule("AlreadyEquipped"));
        }
        let id = item::item_index(req, "ItemIndex")?;
        let data = row(state, "FlaskItem", &[("ItemIndex", id as i64)])?;
        if n(data, "SourceType") != 1 || n(data, "RewardType") != 1 {
            return Err(rule("InvalidItemType"));
        }
        out["ItemResult"] = item::consume(db, account, id, 1).await?;
        details["FlaskItemIndex"] = json!(id);
        details["FlaskExp"] = json!(0);
    }
    hero::save_details(db, account, hero_id, &details).await?;
    out["FlaskResult"] = json!({"HeroIndex":hero_id,"ItemIndex":n(&details,"FlaskItemIndex"),"AddExp":0,"AddExpRatio":0,"NewExp":n(&details,"FlaskExp")});
    Ok(out)
}

pub(crate) async fn fill_flask(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    hero_id: i32,
    exp: i64,
) -> Result<Option<(Value, Vec<Value>)>> {
    let mut details = hero::details(db, account, hero_id).await?;
    let id = n(&details, "FlaskItemIndex");
    if id == 0 || exp <= 0 {
        return Ok(None);
    }
    let r = row(state, "FlaskItem", &[("ItemIndex", id)])?;
    if n(r, "SourceType") != 1 || n(r, "FlaskExp") <= 0 {
        return Ok(None);
    }
    let before = n(&details, "FlaskExp");
    let after = (before + exp).min(n(r, "FlaskExp"));
    let mut rewards = Rewards::default();
    let mut filled = 0;
    if after >= n(r, "FlaskExp") {
        item::give(
            db,
            state,
            account,
            n(r, "PotionItemIndex") as i32,
            1,
            0,
            0,
            &mut rewards,
        )
        .await?;
        details["FlaskItemIndex"] = json!(0);
        details["FlaskExp"] = json!(0);
        filled = 1;
    } else {
        details["FlaskExp"] = json!(after);
    }
    hero::save_details(db, account, hero_id, &details).await?;
    Ok(Some((
        json!({"FilledFlaskIndex":n(r,"PotionItemIndex"),"FilledFlaskCount":filled,"Info":{"HeroIndex":hero_id,"ItemIndex":n(&details,"FlaskItemIndex"),"AddExp":after-before,"AddExpRatio":0,"NewExp":n(&details,"FlaskExp")}}),
        rewards.items,
    )))
}
