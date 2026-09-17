use super::*;
use rand::seq::SliceRandom;

fn definition(s: &AppState, id: i64) -> Result<(&Value, &Value)> {
    let cfg = &s.tables.live.rules["Summons"][id.to_string()];
    if cfg.is_null() || !active(cfg) {
        return Err(rule("GachaDataNotFound"));
    }
    Ok((row(s, "NewEquipGacha", &[("Index", id)])?, cfg))
}
async fn state(db: &mut SqliteConnection, a: i64, id: i64) -> Result<Value> {
    let mut v = get(db, a, "summon", id).await?;
    if v.is_null() {
        v = json!({"GachaIndex":id,"GachaCount":0,"Claimed":0,"DailyFree":0,"LastFree":0,"Step":1,"CompletedStep":0,"CompletedCount":0,"IsTakeLastReward":false,"Day":day()});
    }
    if n(&v, "Day") != day() {
        v["Day"] = json!(day());
        v["DailyFree"] = json!(0);
    }
    Ok(v)
}
fn free(d: &Value, v: &Value) -> Value {
    json!({"GachaIndex":d["Index"],"LastGachaTime":if n(v,"LastFree")>0{json!(time(n(v,"LastFree")))}else{Value::Null},"LastGachaResetTime":time(day()*86400),"RemainDayGachaChance":(n(d,"DailyMaxFree")-n(v,"DailyFree")).max(0),"ResetRemainTime":((day()+1)*86400-now()).max(0)})
}
fn info(d: &Value, cfg: &Value) -> Value {
    json!({"GachaIndex":d["Index"],"BeginTime":time(n(cfg,"Begin")),"EndTime":time(if n(cfg,"End")>0{n(cfg,"End")}else{4102444800}),"OnSale":true,"Discount":n(cfg,"Discount"),"Durational":n(d,"Durational"),"SaleResetTime":time((day()+1)*86400),"ProgressResetTime":time(4102444800),"IsUTCBeginTime":1,"IsUTCEndTime":1,"IsUTCProgressResetTime":1,"RemainNumberOfDiscount":0})
}
pub(super) async fn snapshot(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let (mut infos, mut frees, mut ceilings, mut pets, mut starters, mut steps) =
        (vec![], vec![], vec![], vec![], vec![], vec![]);
    if let Some(defs) = s.tables.live.rules["Summons"].as_object() {
        for key in defs.keys() {
            let Ok(id) = key.parse() else {
                continue;
            };
            let Ok((d, cfg)) = definition(s, id) else {
                continue;
            };
            let v = state(db, a, id).await?;
            infos.push(info(d, cfg));
            frees.push(free(d, &v));
            let count = json!({"GachaIndex":id,"GachaCount":n(&v,"GachaCount")-n(&v,"Claimed")});
            if n(d, "Type") == 6 {
                pets.push(count);
            } else {
                ceilings.push(count);
            }
            if n(d, "Type") == 5 {
                steps.push(json!({"GachaIndex":id,"CompletedStep":v["CompletedStep"],"CompletedCount":v["CompletedCount"],"ProgressTime":v["ProgressTime"]}));
            }
            if n(cfg, "StarterCount") > 0 {
                starters.push(json!({"GachaIndex":id,"GachaCount":n(&v,"StarterDraws"),"IsTakeLastReward":v["IsTakeLastReward"]}));
            }
        }
    }
    Ok(
        json!({"EquipGachaInfos":infos,"freeEquipGachaInfos":frees,"GachaCeilingCountInfos":ceilings,"PetGachaCountInfos":pets,"StarterPickupEquipGachaInfos":starters,"StepUpEquipGachaInfos":steps}),
    )
}
fn pool<'a>(cfg: &'a Value, r: &Request, bonus: bool) -> Result<&'a str> {
    let category = int(r, "HighGachaCategory")?;
    let key = if bonus { "BonusGroup" } else { "Group" };
    if category > 0 {
        if let Some(v) = cfg["Categories"][category.to_string()][key].as_str() {
            return Ok(v);
        }
        return Err(rule("GachaItemGroupCodeDataNotFound"));
    }
    cfg[key]
        .as_str()
        .or_else(|| cfg["Group"].as_str())
        .ok_or_else(|| rule("GachaItemGroupCodeDataNotFound"))
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    if matches!(action, "get_equip_gacha" | "get_free_equip_gacha_info") {
        return snapshot(db, s, a).await;
    }
    let id = int(r, "GachaIndex")?;
    let (d, cfg) = definition(s, id)?;
    let mut v = state(db, a, id).await?;
    let mut out = json!({});
    let mut rewards = Rewards::default();
    if action == "get_gacha_item_group_code" {
        return Ok(
            json!({"ItemGroupCode":pool(cfg,r,false)?,"BonusItemGroupCode":pool(cfg,r,true)?,"ItemGroupCodeNameKey":"","BonusItemGroupCodeNameKey":""}),
        );
    }
    if matches!(
        action,
        "equip_gacha_ceiling_reward"
            | "pet_gacha_roof_reward"
            | "get_starter_pickup_equip_gacha_last_reward"
    ) {
        let starter = action == "get_starter_pickup_equip_gacha_last_reward";
        let pet = action == "pet_gacha_roof_reward";
        if !starter && pet != (n(d, "Type") == 6) {
            return Err(rule("InvalidValue"));
        }
        let threshold = n(
            cfg,
            if starter {
                "StarterCount"
            } else {
                "CeilingCount"
            },
        );
        if threshold <= 0
            || if starter {
                n(&v, "StarterDraws")
            } else {
                n(&v, "GachaCount") - n(&v, "Claimed")
            } < threshold
            || starter && v["IsTakeLastReward"] == true
        {
            return Err(rule("NotEnoughCount"));
        }
        if starter {
            v["IsTakeLastReward"] = json!(true);
        } else {
            v["Claimed"] = json!(n(&v, "Claimed") + threshold);
        }
        let item = n(
            cfg,
            if starter {
                "StarterItem"
            } else {
                "CeilingItem"
            },
        );
        if starter && n(cfg, "StarterRewardIndex") > 0 {
            reward(db, s, a, n(cfg, "StarterRewardIndex"), &mut rewards).await?;
        } else {
            item::give(db, s, a, item as i32, 1, 0, 0, &mut rewards).await?;
        }
        out["ItemResults"] = json!(rewards.items);
        out["RewardResult"] = item::reward_response(db, s, a, rewards).await?;
    } else if matches!(action, "exec_equip_gacha" | "exec_pet_gacha") {
        if n(cfg, "StarterCount") > 0 && n(&v, "StarterDraws") >= n(cfg, "StarterCount") {
            return Err(rule("StartPickupLimitOver"));
        }
        let is_pet = n(d, "Type") == 6;
        if action == "exec_pet_gacha" && !is_pet {
            return Err(rule("GachaDataNotFound"));
        }
        let ticket = int(r, "ItemIndex")?;
        let is_free = flag(r, "Free")?;
        if ticket > 0 && is_free {
            return Err(rule("InvalidValue"));
        }
        let step = if n(d, "Type") == 5 {
            Some(row(
                s,
                "StepUpGachaReward",
                &[("Index", id), ("Step", n(&v, "Step"))],
            )?)
        } else {
            None
        };
        if is_free {
            if n(d, "DailyMaxFree") <= n(&v, "DailyFree")
                || n(&v, "LastFree") + n(d, "FreeCoolTime") > now()
            {
                return Err(rule("NotFreeTime"));
            }
            v["DailyFree"] = json!(n(&v, "DailyFree") + 1);
            v["LastFree"] = json!(now());
        } else if ticket > 0 {
            let t = row(s, "GachaSelectItem", &[("ItemIndex", ticket)])?;
            if !t["GachaIndices"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(id)))
                || n(t, "GachaCategoryType") > 0
                    && n(t, "GachaCategoryType") != int(r, "HighGachaCategory")?
            {
                return Err(rule("ItemTypeMismatch"));
            }
            out["ItemResult"] = item::consume(db, a, ticket as i32, 1).await?;
        } else {
            if d["TicketOnly"] == true {
                return Err(rule("ItemNotOwned"));
            }
            let discount = step
                .map(|v| n(v, "Discount"))
                .unwrap_or(n(cfg, "Discount"))
                .clamp(0, 100);
            let gold = n(d, "OpenGold") * (100 - discount) / 100;
            let gem = n(d, "OpenGem") * (100 - discount) / 100;
            if gold + gem <= 0 {
                return Err(rule("InvalidCost"));
            }
            if gold > 0 {
                rewards
                    .currencies
                    .push(hero::currency(db, a, "Gold", -gold).await?);
            }
            if gem > 0 {
                rewards
                    .currencies
                    .push(hero::currency(db, a, "Gem", -gem).await?);
            }
            if n(d, "Mileage") > 0 {
                rewards.currencies.push(
                    hero::currency(db, a, "Mileage", n(d, "Mileage") * (100 - discount) / 100)
                        .await?,
                );
            }
        }
        let selected = ids(r, "HeroIndices", 10, true)?;
        if n(d, "PickupHeroCount") > 0 {
            if selected.len() as i64 != n(d, "PickupHeroCount")
                || selected.iter().any(|id| {
                    !d["PickupHeroIndices"]
                        .as_array()
                        .is_some_and(|v| v.contains(&json!(id)))
                })
            {
                return Err(rule("InvalidValue"));
            }
        } else if !selected.is_empty() {
            return Err(rule("InvalidValue"));
        }
        let count = n(d, "Bid") + n(d, "BonusBid");
        if !(1..=100).contains(&count) {
            return Err(rule("InvalidValue"));
        }
        let mut items = vec![];
        let mut pet_results = vec![];
        for i in 0..count {
            let pity =
                n(cfg, "PityEvery") > 0 && (n(&v, "GachaCount") + i + 1) % n(cfg, "PityEvery") == 0;
            let mut code = if pity {
                cfg["PityGroup"]
                    .as_str()
                    .ok_or_else(|| rule("GachaDataNotFound"))?
            } else {
                pool(cfg, r, i >= n(d, "Bid"))?
            }
            .to_owned();
            if !selected.is_empty() && (i >= n(d, "Bid") || pity) {
                let chosen = *selected.choose(&mut rand::thread_rng()).unwrap();
                code = cfg["HeroGroups"][chosen.to_string()]
                    .as_str()
                    .ok_or_else(|| rule("GachaDataNotFound"))?
                    .to_owned();
            }
            let (index, c, star, custom) = s
                .tables
                .roll_item_from_group_code(&code, &[])
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            if s.tables
                .live
                .find("Pet", &[("Index", index as i64)])
                .is_some()
            {
                for _ in 0..c {
                    let pet = pets::add(db, s, a, index as i64).await?;
                    items.push(json!({"ItemResult":pet["PetSoulResults"].as_array().and_then(|v|v.first()),"EquipItemResult":null,"PetItemResult":pet["PetResult"],"StaminaResult":null}));
                    pet_results.push(pet);
                }
            } else {
                let mut roll = Rewards::default();
                item::give(db, s, a, index, c, star, custom, &mut roll).await?;
                items.push(json!({"ItemResult":roll.items.first(),"EquipItemResult":roll.equipment.first(),"PetItemResult":null,"StaminaResult":null}));
            }
        }
        v["GachaCount"] = json!(n(&v, "GachaCount") + count);
        if n(cfg, "StarterCount") > 0 {
            v["StarterDraws"] = json!(n(&v, "StarterDraws") + 1);
        }
        if is_pet {
            pets::increment(db, a, 3, count).await?;
            out["PetMiscInfos"] = json!(list(db, a, "pet_misc").await?);
        }
        v["ProgressTime"] = json!(time(now()));
        if let Some(step) = step {
            v["CompletedStep"] = step["Step"].clone();
            v["CompletedCount"] = json!(n(&v, "CompletedCount") + 1);
            v["Step"] = step["NextStep"].clone();
            let mut rr = Rewards::default();
            reward(db, s, a, n(step, "RewardIndex"), &mut rr).await?;
            out["StepUpRewardInfo"] = item::reward_response(db, s, a, rr).await?;
        }
        out["CurrencyResults"] = json!(rewards.currencies);
        out["GachaItemResults"] = json!(items);
        out["PetGachaItemResults"] = json!(pet_results);
        out["GachaInfo"] = free(d, &v);
        out["EquipGachaInfo"] = info(d, cfg);
        crate::api::progression::record(db, a, "ExecEquipGacha", id, 0, count).await?;
    } else {
        return Err(rule("Fail"));
    }
    put(db, a, "summon", id, &v).await?;
    let snap = snapshot(db, s, a).await?;
    merge(&mut out, snap.clone());
    out["PetGachaCountInfo"] =
        json!({"GachaIndex":id,"GachaCount":n(&v,"GachaCount")-n(&v,"Claimed")});
    out["StarterPickupEquipGachaInfo"] = snap["StarterPickupEquipGachaInfos"]
        .as_array()
        .and_then(|v| v.iter().find(|v| n(v, "GachaIndex") == id))
        .cloned()
        .unwrap_or(Value::Null);
    out["StepUpEquipGachaInfo"] = snap["StepUpEquipGachaInfos"]
        .as_array()
        .and_then(|v| v.iter().find(|v| n(v, "GachaIndex") == id))
        .cloned()
        .unwrap_or(Value::Null);
    Ok(out)
}
