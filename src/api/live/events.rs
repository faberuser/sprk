use super::*;
use rand::Rng;

async fn progress(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let cfg = event(s)?;
    let season = n(cfg, "Season");
    let mut v = get(db, a, "event_step", season).await?;
    if v.is_null() {
        v = json!({"DailyStep":0,"DailyCount":0,"TotalCount":0,"MissionCount":0,"MissionStep":0,"GroupTotalStep":0,"Day":day()});
    }
    if n(&v, "Day") != day() {
        v["DailyStep"] = json!(0);
        v["DailyCount"] = json!(0);
        v["Day"] = json!(day());
    }
    let total = n(&get(db, 0, "event_group", season).await?, "Total");
    v["GroupTotalCount"] = json!(total);
    let rows = s.tables.live.rows("EventStep");
    let current = rows
        .iter()
        .filter(|r| n(r, "Type") == 2 && n(r, "TargetScore") <= total)
        .map(|r| n(r, "Step"))
        .max()
        .unwrap_or(0);
    let target = rows
        .iter()
        .filter(|r| n(r, "Type") == 2 && n(r, "Step") == current + 1)
        .map(|r| n(r, "TargetScore"))
        .next()
        .unwrap_or(total);
    v["GroupCurrentStep"] = json!(current);
    v["GroupTargetScore"] = json!(target);
    v["GroupRemainScore"] = json!((target - total).max(0));
    Ok(v)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let cfg = event(s)?;
    let season = n(cfg, "Season");
    let mut out = json!({});
    let mut rewards = Rewards::default();
    match action {
        "event_calendar" => {
            let version = cfg["Version"].as_str().unwrap_or("local-1");
            out = json!({"EventCalendarVersion":version,"NewEvent":r.text("EventCalendarVersion")!=version,"EventCalendarDatas":cfg["Calendar"].as_array().cloned().unwrap_or_default()});
        }
        "get_event_step_info" | "put_event_step" | "get_event_step_reward" => {
            let mut v = progress(db, s, a).await?;
            if action == "put_event_step" {
                let amount = n(cfg, "Contribution");
                if amount <= 0 || n(&v, "DailyCount") + amount > n(cfg, "DailyContributionLimit") {
                    return Err(rule("InvalidValue"));
                }
                rewards
                    .currencies
                    .push(hero::currency(db, a, "GrowWorldTreePoint", -amount).await?);
                for k in ["DailyCount", "TotalCount"] {
                    v[k] = json!(n(&v, k) + amount);
                }
                v["MissionCount"] = json!(n(&v, "MissionCount") + 1);
                put(
                    db,
                    0,
                    "event_group",
                    season,
                    &json!({"Total":n(&v,"GroupTotalCount")+amount}),
                )
                .await?;
            }
            if action == "get_event_step_reward" {
                let typ = int(r, "EventStepType")?;
                let (field, score) = match typ {
                    0 => ("DailyStep", n(&v, "DailyCount")),
                    1 => ("MissionStep", n(&v, "MissionCount")),
                    2 => ("GroupTotalStep", n(&v, "GroupTotalCount")),
                    _ => return Err(rule("InvalidValue")),
                };
                let data = row(s, "EventStep", &[("Type", typ), ("Step", n(&v, field) + 1)])?;
                if score < n(data, "TargetScore") || (typ == 2 && n(&v, "TotalCount") == 0) {
                    return Err(rule("NotEnoughPoint"));
                }
                reward(db, s, a, n(data, "RewardIndex"), &mut rewards).await?;
                v[field] = json!(n(data, "Step"));
                out["RewardStep"] = v[field].clone();
            }
            put(db, a, "event_step", season, &v).await?;
            out["EventStepInfo"] = progress(db, s, a).await?;
            out["EventStepGroupServerInfos"] = json!(s
                .tables
                .live
                .rows("EventStep")
                .iter()
                .filter(|v| n(v, "Type") == 2)
                .map(|v| json!({"Step":v["Step"],"TargetScore":v["TargetScore"]}))
                .collect::<Vec<_>>());
            merge(&mut out, item::reward_response(db, s, a, rewards).await?);
        }
        "event_craft_item" => {
            let id = int(r, "CraftIndex")?;
            let count = item::positive(r, "CraftCount")? as i64;
            let data = row(s, "EventCraft", &[("Index", id)])?;
            if !cfg["CraftGroups"]
                .as_array()
                .is_some_and(|v| v.contains(&data["ShowGroup"]))
            {
                return Err(rule("ContentsDisabled"));
            }
            let mut v = get(db, a, "event_craft", id).await?;
            let old = if n(&v, "Season") == season {
                n(&v, "CraftCount")
            } else {
                0
            };
            if old + count > i32::MAX as i64
                || n(data, "CraftCount") > 0 && old + count > n(data, "CraftCount")
            {
                return Err(rule("InvalidItemCount"));
            }
            let mut consumed = vec![];
            for i in 1..=8 {
                let code = data[format!("MaterialItemCode{i}")].as_str().unwrap_or("");
                let cost = n(data, &format!("MaterialItemCount{i}"));
                if !code.is_empty() && cost > 0 {
                    consumed.push(consume_code(db, s, a, code, cost * count).await?);
                }
            }
            for _ in 0..count {
                reward(db, s, a, n(data, "RewardIndex"), &mut rewards).await?;
            }
            v = json!({"Index":id,"ShowGroup":data["ShowGroup"],"CraftCount":old+count,"UpdatedTime":time(now()),"Season":season});
            put(db, a, "event_craft", id, &v).await?;
            out = json!({"ItemResults":consumed,"RewardResult":item::reward_response(db,s,a,rewards).await?,"EventCraftItemInfo":v});
        }
        "reward_event_roulette" => {
            let id = int(r, "RouletteIndex")?;
            let count = item::positive(r, "RouletteCount")? as i64;
            if !cfg["RouletteIndices"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(id)))
            {
                return Err(rule("ContentsDisabled"));
            }
            let data = row(s, "EventRoulette", &[("Index", id)])?;
            let mut removed = vec![];
            let mut currencies = vec![];
            if n(data, "Type") == 0 {
                currencies.push(
                    charge(
                        db,
                        s,
                        a,
                        n(data, "CurrencyType"),
                        n(data, "CurrencyValue") * count,
                    )
                    .await?,
                );
            } else {
                removed.push(
                    consume_code(
                        db,
                        s,
                        a,
                        data["ItemCode"]
                            .as_str()
                            .ok_or_else(|| rule("ItemDataNotFound"))?,
                        n(data, "ItemCount") * count,
                    )
                    .await?,
                );
            }
            for _ in 0..count {
                group(
                    db,
                    s,
                    a,
                    data["DropItemGroupCode"]
                        .as_str()
                        .ok_or_else(|| rule("ItemDataNotFound"))?,
                    &mut rewards,
                )
                .await?;
            }
            out = json!({"CurrencyResults":currencies,"ItemResults":removed,"RewardResult":item::reward_response(db,s,a,rewards).await?});
        }
        "event_forge" | "event_exchange_equip" => {
            if cfg["ForgeEnabled"] != true {
                return Err(rule("ContentsDisabled"));
            }
            let slots = if action == "event_forge" {
                vec![int(r, "EquipItemSlotIndex")?]
            } else {
                ids(r, "EquipItemSlotIndices", 100, true)?
            };
            if slots.is_empty() {
                return Err(rule("InvalidValue"));
            }
            let mut removed = vec![];
            let mut consumed = vec![];
            for slot in slots {
                let mut eq = crate::api::extensions::material(db, a, slot).await?;
                let level = if action == "event_forge" {
                    int(r, "UpgradeLevel")?
                } else {
                    eq.level as i64
                };
                let data = s
                    .tables
                    .live
                    .rows("EventForge")
                    .iter()
                    .find(|v| {
                        n(v, "EnchantLevel") == level
                            && v["ItemCode"]
                                .as_str()
                                .and_then(|v| s.tables.get_item_index(v))
                                == Some(eq.item_index)
                    })
                    .ok_or_else(|| rule("InvalidValue"))?;
                if action == "event_exchange_equip" {
                    reward(db, s, a, n(data, "ExchangeRewardIndex"), &mut rewards).await?;
                    removed.push(slot);
                } else {
                    if level != eq.level as i64 + 1 || level > n(data, "MaxEnchantLevel") {
                        return Err(rule("InvalidValue"));
                    }
                    out["CurrencyResult"] =
                        hero::currency(db, a, "Gold", -n(data, "RequireGoldCost")).await?;
                    consumed.push(
                        consume_code(
                            db,
                            s,
                            a,
                            data["RequireItem"].as_str().unwrap_or(""),
                            n(data, "RequireItemCount"),
                        )
                        .await?,
                    );
                    let fixed = flag(r, "IsFixed")?;
                    if fixed && n(data, "SuccessfulItemCount") > 0 {
                        consumed.push(
                            consume_code(
                                db,
                                s,
                                a,
                                data["SuccessfulItemCode"].as_str().unwrap_or(""),
                                n(data, "SuccessfulItemCount"),
                            )
                            .await?,
                        );
                    }
                    let roll = rand::thread_rng().gen_range(0..1000);
                    let success = fixed || roll < n(data, "SuccessRatio");
                    out["Success"] = json!(success);
                    if success {
                        eq.level = level as i32;
                        crate::api::extensions::save_equip(db, a, &eq).await?;
                        out["ResultEquipItem"] = json!(eq);
                    } else if roll >= n(data, "SuccessRatio") + n(data, "FailRatio")
                        && n(data, "BreakRatio") > 0
                    {
                        removed.push(slot);
                        let code = data["BreakItemCode"].as_str().unwrap_or("");
                        if !code.is_empty() {
                            let id = s
                                .tables
                                .get_item_index(code)
                                .ok_or_else(|| rule("ItemDataNotFound"))?;
                            item::give(db, s, a, id, 1, 0, 0, &mut rewards).await?;
                        }
                    } else {
                        out["ResultEquipItem"] = json!(eq);
                    }
                }
            }
            for slot in &removed {
                sqlx::query("DELETE FROM equip_items WHERE account_id=? AND slot_index=?")
                    .bind(a)
                    .bind(slot)
                    .execute(&mut *db)
                    .await?;
            }
            out["RemovedEquipItemSlotIndices"] = json!(removed);
            out["RemovedItemResultInfos"] = json!(consumed);
            out["ItemResults"] = json!(rewards.items);
        }
        _ => return Err(rule("Fail")),
    }
    Ok(out)
}
