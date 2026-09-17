use super::*;

pub(crate) async fn add(db: &mut SqliteConnection, s: &AppState, a: i64, id: i64) -> Result<Value> {
    let data = row(s, "Pet", &[("Index", id)])?;
    let old = get(db, a, "pet", id).await?;
    if !old.is_null() {
        let grade = n(data, "Grade");
        let code = if grade == 6 {
            row(s, "PetAwaken", &[("PetIndex", id), ("Star", 1)])?["ItemCode1"]
                .as_str()
                .ok_or_else(|| rule("PetSoulDataError"))?
        } else {
            match grade {
                3 => "ITEM_PETSOUL_COMMON_RARE",
                4 => "ITEM_PETSOUL_COMMON_HERO",
                5 => "ITEM_PETSOUL_COMMON_ANCIENT",
                _ => return Err(rule("PetSoulDataError")),
            }
        };
        let index = s
            .tables
            .get_item_index(code)
            .ok_or_else(|| rule("PetSoulDataError"))?;
        let mut rewards = Rewards::default();
        item::give(db, s, a, index, 1, 0, 0, &mut rewards).await?;
        return Ok(json!({"PetResult":null,"PetSoulResults":rewards.items}));
    }
    let v = json!({"PetIndex":id,"Star":0,"CreatedTime":time(now()),"HappinessPoint":0,"LastPlayTime":null,"LastPatTime":null,"LastFeedTime":null,"MaxRewardedTime":null,"FullPoint":0,"Status":0,"EndPenaltyTime":null});
    put(db, a, "pet", id, &v).await?;
    Ok(json!({"PetResult":v,"PetSoulResults":[]}))
}
async fn owned(db: &mut SqliteConnection, a: i64, id: i64) -> Result<Value> {
    let v = get(db, a, "pet", id).await?;
    if v.is_null() {
        return Err(rule("PetNotOwned"));
    }
    Ok(v)
}
fn available(v: &Value) -> Result<()> {
    if n(v, "Status") == 2 || timestamp(&v["EndPenaltyTime"]) > now() {
        return Err(rule("InvalidPetStatus"));
    }
    Ok(())
}
async fn misc(db: &mut SqliteConnection, a: i64, key: i64, value: String) -> Result<()> {
    put(
        db,
        a,
        "pet_misc",
        key,
        &json!({"MiscKey":key,"MiscValue":value,"UpdatedTime":time(now())}),
    )
    .await
}
pub(super) async fn increment(
    db: &mut SqliteConnection,
    a: i64,
    key: i64,
    count: i64,
) -> Result<()> {
    let old = get(db, a, "pet_misc", key).await?;
    let old = old["MiscValue"]
        .as_str()
        .and_then(|v| v.parse::<i64>().ok())
        .unwrap_or(0);
    misc(db, a, key, (old + count).to_string()).await
}
async fn initialize(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    if s.tables.live.rows("PetIncubatorSlot").is_empty() {
        return Ok(());
    }
    let d = row(s, "PetIncubatorSlot", &[("OpenNum", 1)])?;
    let slot = n(d, "Index");
    if get(db, a, "pet_slot", slot).await?.is_null() {
        open_slot(db, a, d).await?;
    }
    Ok(())
}
async fn open_slot(db: &mut SqliteConnection, a: i64, d: &Value) -> Result<()> {
    let slot = n(d, "Index");
    let id = n(d, "DefaultIndex");
    put(
        db,
        a,
        "pet_slot",
        slot,
        &json!({"SlotIndex":slot,"MountingIndex":id,"SetItemIndex":0,"CompletedTime":null}),
    )
    .await?;
    put(
        db,
        a,
        "pet_incubator",
        slot * 100000 + id,
        &json!({"SlotIndex":slot,"IncubatorIndex":id,"CreatedTime":time(now())}),
    )
    .await
}
pub(super) async fn snapshot(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    initialize(db, s, a).await?;
    Ok(
        json!({"PetInfos":list(db,a,"pet").await?,"PetMiscInfos":list(db,a,"pet_misc").await?,"PetIncubatorSlotInfos":list(db,a,"pet_slot").await?,"PetIncubatorInfos":list(db,a,"pet_incubator").await?,"PetExploreInfos":list(db,a,"pet_explore").await?}),
    )
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    initialize(db, s, a).await?;
    let mut out = json!({});
    let mut rewards = Rewards::default();
    match action {
        "get_pet_house_info" | "get_pet_misc_info" | "get_pet_explore_list" => {
            return snapshot(db, s, a).await
        }
        "change_pet_layout" => {
            let pets: Vec<i64> =
                serde_json::from_str(r.text("PetIndices")).map_err(|_| rule("InvalidValue"))?;
            let mut seen = std::collections::BTreeSet::new();
            if pets.len() > setting(s, "PetLayoutMax", 8) as usize {
                return Err(rule("InvalidValue"));
            }
            for id in &pets {
                if *id < 0 || (*id > 0 && !seen.insert(*id)) {
                    return Err(rule("InvalidValue"));
                }
                if *id > 0 {
                    available(&owned(db, a, *id).await?)?;
                }
            }
            misc(
                db,
                a,
                2,
                pets.iter()
                    .map(ToString::to_string)
                    .collect::<Vec<_>>()
                    .join(","),
            )
            .await?;
        }
        "change_pet_avatar" => {
            let avatar = int(r, "AvatarPetIndex")?;
            let id = int(r, "PetIndex")?;
            for i in [avatar, id] {
                if i > 0 {
                    available(&owned(db, a, i).await?)?;
                }
            }
            for mut p in list(db, a, "pet").await? {
                if n(&p, "Status") == 1 {
                    p["Status"] = json!(0);
                    put(db, a, "pet", n(&p, "PetIndex"), &p).await?;
                }
            }
            if id > 0 {
                let mut p = owned(db, a, id).await?;
                p["Status"] = json!(1);
                put(db, a, "pet", id, &p).await?;
            }
            misc(db, a, 1, avatar.to_string()).await?;
        }
        "egg_supplier" => {
            let v = get(db, a, "pet_supplier", 0).await?;
            let cooldown = setting(s, "EggSupplierSeconds", 86400);
            if n(&v, "Next") > now() {
                return Err(rule("NotYet"));
            }
            let id = setting(s, "EggSupplierItem", 7500010);
            item::give(db, s, a, id as i32, 1, 0, 0, &mut rewards).await?;
            put(db, a, "pet_supplier", 0, &json!({"Next":now()+cooldown})).await?;
            misc(db, a, 5, time(now() + cooldown)).await?;
            out["ItemResults"] = json!(rewards.items);
        }
        "feed_the_pet" | "pet_interactive" | "pet_happy_gift" | "pet_awaken" => {
            let id = int(r, "PetIndex")?;
            let mut p = owned(db, a, id).await?;
            available(&p)?;
            let data = row(s, "Pet", &[("Index", id)])?;
            let grade = n(data, "Grade");
            let care = row(s, "PetTierInteractive", &[("Grade", grade)])?;
            if action == "pet_awaken" {
                if grade != 6 || n(&p, "Star") >= 5 {
                    return Err(rule("CannotAwakenMore"));
                }
                let d = row(
                    s,
                    "PetAwaken",
                    &[("PetIndex", id), ("Star", n(&p, "Star") + 1)],
                )?;
                rewards.items.push(
                    consume_code(
                        db,
                        s,
                        a,
                        d["ItemCode1"].as_str().unwrap_or(""),
                        n(d, "ItemCount1"),
                    )
                    .await?,
                );
                rewards
                    .currencies
                    .push(hero::currency(db, a, "Gold", -n(d, "ReqGold")).await?);
                p["Star"] = json!(n(&p, "Star") + 1);
            } else if action == "feed_the_pet" {
                let count = item::positive(r, "FeedCount")? as i64;
                let value = s.tables.hero_shop.constant("PetFoodValue", 25);
                if n(&p, "FullPoint") >= n(data, "MaxFood")
                    || count > (n(data, "MaxFood") - n(&p, "FullPoint") + value - 1) / value
                {
                    return Err(rule("InvalidValue"));
                }
                out["StaminaResult"] = crate::api::battle::charge_key(db, s, a, 27, count).await?;
                p["FullPoint"] =
                    json!((n(&p, "FullPoint") + count * value).min(n(data, "MaxFood")));
            } else if action == "pet_happy_gift" {
                if n(&p, "HappinessPoint") < n(care, "PetMaxHappinessPoint")
                    || timestamp(&p["MaxRewardedTime"]) + n(care, "SleepCoolTimeSec") > now()
                {
                    return Err(rule("NotYet"));
                }
                reward(db, s, a, n(care, "RewardIndex"), &mut rewards).await?;
                p["HappinessPoint"] = json!(0);
                p["MaxRewardedTime"] = json!(time(now()));
            } else {
                let typ = int(r, "ActionType")?;
                let count = r.number("ActionCount", 1)?;
                if !(1..=4).contains(&count) {
                    return Err(rule("InvalidValue"));
                }
                let (field, cool, add) = match typ {
                    0 => (
                        "LastFeedTime",
                        "PetFeedCoolTimeSec",
                        "PetFeedAddHappinessPoint",
                    ),
                    1 => (
                        "LastPatTime",
                        "PetPatCoolTimeSec",
                        "PetPatAddHappinessPoint",
                    ),
                    2 => (
                        "LastPlayTime",
                        "PetPlayCoolTimeSec",
                        "PetPlayAddHappinesPoint",
                    ),
                    _ => return Err(rule("InvalidValue")),
                };
                if timestamp(&p[field]) + n(care, cool) > now()
                    || timestamp(&p["MaxRewardedTime"]) + n(care, "SleepCoolTimeSec") > now()
                    || n(&p, "HappinessPoint") >= n(care, "PetMaxHappinessPoint")
                {
                    return Err(rule("NotYet"));
                }
                if typ == 0 {
                    out["StaminaResult"] =
                        crate::api::battle::charge_key(db, s, a, 27, count).await?;
                }
                p[field] = json!(time(now()));
                p["HappinessPoint"] = json!((n(&p, "HappinessPoint") + n(care, add) * count)
                    .min(n(care, "PetMaxHappinessPoint")));
            }
            put(db, a, "pet", id, &p).await?;
            out["PetResult"] = p.clone();
            out["PetResultInfo"] = p;
            merge(&mut out, item::reward_response(db, s, a, rewards).await?);
        }
        "pet_incubator_slot_expansion" => {
            let count = list(db, a, "pet_slot").await?.len() as i64;
            let d = row(s, "PetIncubatorSlot", &[("OpenNum", count + 1)])?;
            rewards
                .currencies
                .push(charge(db, s, a, n(d, "BuyCurrencyType"), n(d, "BuyCurrencyValue")).await?);
            if n(d, "Mileage") > 0 {
                rewards
                    .currencies
                    .push(hero::currency(db, a, "Mileage", n(d, "Mileage")).await?);
            }
            open_slot(db, a, d).await?;
            out = snapshot(db, s, a).await?;
            out["CurrencyResults"] = json!(rewards.currencies);
        }
        "buy_pet_incubator"
        | "set_pet_incubator"
        | "set_egg_in_pet_incubator"
        | "unset_egg_in_pet_incubator"
        | "get_egg_rewards" => {
            let slot = int(r, "SlotIndex")?;
            let id = int(r, "IncubatorIndex")?;
            let mut v = get(db, a, "pet_slot", slot).await?;
            if v.is_null() {
                return Err(rule("InvalidValue"));
            }
            let d = row(s, "PetIncubator", &[("Index", id)])?;
            let key = slot * 100000 + id;
            if action == "buy_pet_incubator" {
                if !get(db, a, "pet_incubator", key).await?.is_null() {
                    return Err(rule("AlreadyOwned"));
                }
                if n(d, "BuyCurrencyValue") <= 0 {
                    return Err(rule("InvalidValue"));
                }
                rewards.currencies.push(
                    charge(db, s, a, n(d, "BuyCurrencyType"), n(d, "BuyCurrencyValue")).await?,
                );
                if n(d, "Mileage") > 0 {
                    rewards
                        .currencies
                        .push(hero::currency(db, a, "Mileage", n(d, "Mileage")).await?);
                }
                let info = json!({"SlotIndex":slot,"IncubatorIndex":id,"CreatedTime":time(now())});
                put(db, a, "pet_incubator", key, &info).await?;
                out["PetIncubatorInfo"] = info;
                out["CurrencyResults"] = json!(rewards.currencies);
            } else {
                if get(db, a, "pet_incubator", key).await?.is_null() {
                    return Err(rule("ItemNotOwned"));
                }
                if action == "set_pet_incubator" {
                    if n(&v, "SetItemIndex") != 0 {
                        return Err(rule("InvalidValue"));
                    }
                    v["MountingIndex"] = json!(id);
                } else {
                    if n(&v, "MountingIndex") != id {
                        return Err(rule("InvalidValue"));
                    }
                    if action == "set_egg_in_pet_incubator" {
                        if n(&v, "SetItemIndex") != 0 {
                            return Err(rule("InvalidValue"));
                        }
                        let egg = int(r, "ItemIndex")?;
                        let e = row(s, "PetEgg", &[("Index", egg)])?;
                        out["ItemResult"] = item::consume(db, a, egg as i32, 1).await?;
                        let duration =
                            n(e, "ReqTime") * (100 - n(d, "TimePer")).clamp(1, 100) / 100;
                        v["SetItemIndex"] = json!(egg);
                        v["CompletedTime"] = json!(time(now() + duration));
                    } else {
                        let egg = n(&v, "SetItemIndex");
                        if egg == 0 || (int(r, "ItemIndex")? > 0 && int(r, "ItemIndex")? != egg) {
                            return Err(rule("InvalidValue"));
                        }
                        if action == "get_egg_rewards" {
                            if timestamp(&v["CompletedTime"]) > now() {
                                return Err(rule("NotYet"));
                            }
                            let e = row(s, "PetEgg", &[("Index", egg)])?;
                            let (pet, _, _, _) = s
                                .tables
                                .roll_item_from_group_code(
                                    e["ItemGroupCode"].as_str().unwrap_or(""),
                                    &[],
                                )
                                .ok_or_else(|| rule("PetDataNotFound"))?;
                            out["PetAddResultInfos"] = json!([add(db, s, a, pet as i64).await?]);
                            increment(db, a, 6, 1).await?;
                        } else {
                            item::give(db, s, a, egg as i32, 1, 0, 0, &mut rewards).await?;
                            out["ItemResult"] = json!(rewards.items.first());
                        }
                        v["SetItemIndex"] = json!(0);
                        v["CompletedTime"] = Value::Null;
                    }
                }
                put(db, a, "pet_slot", slot, &v).await?;
                out["PetIncubatorSlotInfo"] = v;
            }
        }
        "pet_upgrade_tier" => {
            let souls = ids(r, "PetSouls", 100, false)?;
            if souls.is_empty() {
                return Err(rule("InvalidValue"));
            }
            let grade = n(item::data(s, souls[0] as i32)?, "Grade");
            let d = row(s, "PetUpgradeTier", &[("Grade", grade)])?;
            if souls.len() as i64 != n(d, "ItemCount") {
                return Err(rule("InvalidValue"));
            }
            let valid = s.tables.live.rules["PetSoulItems"]
                .as_array()
                .ok_or_else(|| rule("PetSoulDataError"))?;
            let mut counts = std::collections::BTreeMap::new();
            for id in souls {
                if !valid.contains(&json!(id)) || n(item::data(s, id as i32)?, "Grade") != grade {
                    return Err(rule("InvalidValue"));
                }
                *counts.entry(id).or_insert(0) += 1;
            }
            for (id, c) in counts {
                rewards
                    .items
                    .push(item::consume(db, a, id as i32, c).await?);
            }
            out["RemovedItemResults"] = json!(rewards.items);
            out["CurrencyResults"] =
                json!([hero::currency(db, a, "Gold", -n(d, "ReqGold")).await?]);
            let code = s.tables.live.rules["PetTierGroups"][grade.to_string()]
                .as_str()
                .ok_or_else(|| rule("PetDataNotFound"))?;
            let (id, _, _, _) = s
                .tables
                .roll_item_from_group_code(code, &[])
                .ok_or_else(|| rule("PetDataNotFound"))?;
            let result = add(db, s, a, id as i64).await?;
            increment(db, a, 7, 1).await?;
            out["PetResult"] = result["PetResult"].clone();
            out["ItemResults"] = result["PetSoulResults"].clone();
        }
        "pet_start_explore" | "pet_cancel_explore" | "pet_end_explore" => {
            let id = int(r, "ExploreIndex")?;
            let deck = int(r, "DeckIndex")?;
            let d = row(s, "PetAdventure", &[("Index", id)])?;
            let rd = row(s, "PetAdventureReward", &[("Index", id)])?;
            if deck < 1 || deck > setting(s, "PetExploreDecks", 3) {
                return Err(rule("InvalidValue"));
            }
            let key = id * 100 + deck;
            let mut v = get(db, a, "pet_explore", key).await?;
            let mut results = vec![];
            if action == "pet_start_explore" {
                if !v.is_null() && v["PetIndices"].as_str().is_some_and(|v| !v.is_empty()) {
                    return Err(rule("InvalidValue"));
                }
                let pets = ids(r, "PetIndices", n(d, "MaxPetCount") as usize, true)?;
                if pets.is_empty() {
                    return Err(rule("InvalidValue"));
                }
                let mut grade = 0;
                for id in &pets {
                    let mut p = owned(db, a, *id).await?;
                    available(&p)?;
                    if n(&p, "Status") == 1 || n(&p, "FullPoint") < n(d, "ReqFood") {
                        return Err(rule("InvalidValue"));
                    }
                    p["FullPoint"] = json!(n(&p, "FullPoint") - n(d, "ReqFood"));
                    p["Status"] = json!(2);
                    grade = grade.max(n(row(s, "Pet", &[("Index", *id)])?, "Grade"));
                    put(db, a, "pet", *id, &p).await?;
                    results.push(p);
                }
                v = json!({"ExploreIndex":id,"DeckIndex":deck,"PetIndices":pets.iter().map(ToString::to_string).collect::<Vec<_>>().join(","),"BeginTime":time(now()),"EndTime":time(now()+n(rd,"Time")),"RewardCount":setting(s,"PetExploreRewardPerPet",1)*pets.len() as i64,"RewardRate":100,"RewardGrade":grade});
            } else {
                let pets: Vec<i64> = v["PetIndices"]
                    .as_str()
                    .unwrap_or("")
                    .split(',')
                    .filter_map(|v| v.parse().ok())
                    .collect();
                if pets.is_empty() {
                    return Err(rule("InvalidValue"));
                }
                if action == "pet_end_explore" {
                    increment(db, a, 8, 1).await?;
                    if timestamp(&v["EndTime"]) > now() {
                        return Err(rule("NotYet"));
                    }
                    item::give(
                        db,
                        s,
                        a,
                        n(rd, "RewardItemIndex") as i32,
                        n(&v, "RewardCount") as i32,
                        0,
                        0,
                        &mut rewards,
                    )
                    .await?;
                }
                for id in pets {
                    let mut p = owned(db, a, id).await?;
                    p["Status"] = json!(0);
                    p["EndPenaltyTime"] = json!(time(
                        now()
                            + if action == "pet_cancel_explore" {
                                n(rd, "PenaltyTime")
                            } else {
                                0
                            }
                    ));
                    put(db, a, "pet", id, &p).await?;
                    results.push(p);
                }
                v["PetIndices"] = json!("");
                v["EndTime"] = Value::Null;
                v["RewardCount"] = json!(0);
            }
            put(db, a, "pet_explore", key, &v).await?;
            out["PetExploreInfo"] = v;
            out["PetResultInfos"] = json!(results);
            merge(&mut out, item::reward_response(db, s, a, rewards).await?);
        }
        _ => return Err(rule("Fail")),
    }
    out["PetMiscInfos"] = json!(list(db, a, "pet_misc").await?);
    Ok(out)
}
