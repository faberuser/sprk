use super::*;
pub(super) async fn ensure_available(
    db: &mut SqliteConnection,
    a: i64,
    party: &[i64],
    except: Option<i64>,
) -> Result<()> {
    super::super::community::ensure_available(db,a,party).await?;
    for v in list(db, a, "dispatch").await? {
        if except == Some(n(&v, "SlotIndex"))
            || matches!(v["State"].as_str(), Some("Complete" | "Cancel"))
        {
            continue;
        }
        let ids: Vec<i64> = read_json(v["HeroIndices"].as_str().unwrap_or("[]"))?;
        if ids.iter().any(|id| party.contains(id)) {
            return Err(rule("AlreadyOnBattleHero"));
        }
    }
    Ok(())
}
fn request(v: &Value) -> Request {
    Request(read_value(v["Request"].clone()).unwrap_or_default())
}
pub(super) async fn snapshot(db: &mut SqliteConnection, a: i64) -> Result<Vec<Value>> {
    let mut runs = list(db, a, "dispatch").await?;
    for run in &mut runs {
        if run["State"] == "Battle" && n(run, "FinishTimestamp") <= now() {
            run["State"] = json!("ReadyToComplete");
            put(db, a, "dispatch", n(run, "SlotIndex"), run).await?;
        }
    }
    Ok(runs)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    if action == "get_dispatch_list" {
        return Ok(json!({"DispatchBattleInfos":snapshot(db,a).await?}));
    }
    if action == "complete_calculate_dispatch_result" {
        let next = snapshot(db, a)
            .await?
            .into_iter()
            .find(|v| v["State"] == "ReadyToComplete");
        return Ok(json!({"DispatchInfo":next}));
    }
    if action == "start_dispatch" {
        let mut parameters = r.0.clone();
        parameters.insert(
            "DungeonDifficulty".into(),
            int(r, "Difficulty")?.to_string(),
        );
        let request = Request(parameters);
        let d = campaign::dungeon(s, &request)?;
        let diff = campaign::difficulty(&request)?;
        if campaign::field(d, "DispatchType", diff) == 0 {
            return Err(rule("DungeonNotFound"));
        }
        let party = ids(r, "HeroIndices", 32)?;
        let active: Option<String> = sqlx::query_scalar(
            "SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND started>=?",
        )
        .bind(a)
        .bind(now() - settings(s, "BattleExpirySeconds", 14400))
        .fetch_optional(&mut *db)
        .await?;
        if let Some(active) = active {
            let active: Value = read_json(&active)?;
            if active["Heroes"]
                .as_array()
                .is_some_and(|v| party.iter().any(|h| v.contains(&json!(h))))
            {
                return Err(rule("AlreadyOnBattleHero"));
            }
        }
        // Timed dispatch currently uses campaign drops and refunds ordinary stamina.
        if !matches!(n(d, "BattleType"), 1 | 2 | 10) || !matches!(n(d, "ReqStaminaType"), 0 | 1) {
            return Err(rule("ContentsDisabled"));
        }
        campaign::validate(db, s, a, &request, &party).await?;
        let progress =
            campaign::progress(db, s, a, n(d, "ChapterIndex"), n(d, "DungeonIndex")).await?;
        if n(&progress, "FirstRewardedDiff") & (1 << diff) == 0 {
            return Err(rule("NotCompletedDungeon"));
        }
        let count = int(r, "RepeatCount")?;
        if count < 1 || count > settings(s, "DispatchMaxRepeat", 100) {
            return Err(rule("InvalidCount"));
        }
        let existing = list(db, a, "dispatch").await?;
        let slot = (1..=settings(s, "DispatchMaxSlots", 5))
            .find(|slot| {
                !existing.iter().any(|v| {
                    n(v, "SlotIndex") == *slot
                        && !matches!(v["State"].as_str(), Some("Complete" | "Cancel"))
                })
            })
            .ok_or_else(|| rule("AlreadyOnBattleHero"))?;
        let cost = campaign::stamina_cost(d, diff) * count;
        let stamina = dungeons::charge(db, s, a, n(d, "ReqStaminaType"), cost).await?;
        let finish = now() + settings(s, "DispatchSecondsPerBattle", 60).max(1) * count;
        let info = json!({"SlotIndex":slot,"ChapterIndex":n(d,"ChapterIndex"),"DungeonIndex":n(d,"DungeonIndex"),"Difficulty":diff,"DeckIndex":int(r,"DeckIndex")?,"RepeatCount":count,"HeroIndices":json!(party).to_string(),"BeginTime":time(now()),"CompleteTime":time(finish),"ClientResult":null,"ClientResultTimeMs":0,"ServerResult":null,"State":"Battle","WinCount":0,"LoseCount":0,"StaminaDiscountRate":0,"FinishTimestamp":finish,"StartTimestamp":now(),"Cost":cost,"CostType":n(d,"ReqStaminaType"),"Request":request.0});
        put(db, a, "dispatch", slot, &info).await?;
        return Ok(json!({"DispatchBattleInfo":info,"StaminaResult":stamina}));
    }
    let slot = int(r, "SlotIndex")?;
    let mut info = get(db, a, "dispatch", slot).await?;
    if info.is_null() {
        return Err(rule("DispatchBattleNotFound"));
    }
    if matches!(info["State"].as_str(), Some("Complete" | "Cancel")) {
        return Err(rule("AlreadyFinishDispatchBattle"));
    }
    let cancel = action == "cancel_dispatch";
    if !cancel && now() < n(&info, "FinishTimestamp") {
        return Err(rule("NotCompletedDungeon"));
    }
    let total = n(&info, "RepeatCount");
    let count = if cancel {
        ((now() - n(&info, "StartTimestamp")) / settings(s, "DispatchSecondsPerBattle", 60).max(1))
            .clamp(0, total)
    } else {
        total
    };
    let req = request(&info);
    let heroes: Vec<i64> = read_json(info["HeroIndices"].as_str().unwrap_or("[]"))?;
    let mut out = if count > 0 {
        campaign::complete(db, s, a, &req, &heroes, 3, count).await?
    } else {
        item::success()
    };
    if cancel && count < total {
        let refund = n(&info, "Cost") / total * (total - count);
        let kind = n(&info, "CostType");
        if kind == 1 {
            let value: i64 = sqlx::query_scalar(
                "UPDATE user_info SET stamina=stamina+? WHERE account_id=? RETURNING stamina",
            )
            .bind(refund)
            .bind(a)
            .fetch_one(&mut *db)
            .await?;
            out["StaminaResult"] = json!({"Type":"Chicken","AddValue":refund,"NewValue":value});
        } else if refund > 0 {
            return Err(rule("NotEnoughCurrency"));
        }
    }
    out["TeamExpResult"] = out["ExpResult"].clone();
    info["State"] = json!(if cancel { "Cancel" } else { "Complete" });
    info["WinCount"] = json!(count);
    info["CompleteTime"] = json!(time(now()));
    info["ServerResult"] = json!(out.to_string());
    put(db, a, "dispatch", slot, &info).await?;
    out["DispatchBattleInfo"] = info;
    Ok(out)
}
pub(super) async fn sweep(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let d = campaign::dungeon(s, r)?;
    let diff = campaign::difficulty(r)?;
    let count = int(r, "SweepCount")?;
    if n(d, "SweepDungeonType") == 0 || count < 1 || count > 100 {
        return Err(rule("DungeonNotFound"));
    }
    // Eclipse needs battle-service proof before it can establish a sweep record.
    if !matches!(n(d, "BattleType"), 14 | 22 | 38) {
        return Err(rule("ContentsDisabled"));
    }
    let p = campaign::progress(db, s, a, n(d, "ChapterIndex"), n(d, "DungeonIndex")).await?;
    if n(&p, "FirstRewardedDiff") & (1 << diff) == 0 {
        return Err(rule("NotCompletedDungeon"));
    }
    let tickets = n(d, "SweepTicketCount") * count;
    let max = n(d, "SweepTicketMaxCount");
    if max > 0 && count > max {
        return Err(rule("InvalidCount"));
    }
    let removed = if tickets > 0 {
        Some(
            item::consume(
                db,
                a,
                s.tables.hero_shop.constant("SweepTicketItemIndex", 120125) as i32,
                tickets as i32,
            )
            .await?,
        )
    } else {
        None
    };
    let cost = (campaign::stamina_cost(d, diff) + n(d, "AdvancedReqStamina")) * count;
    let stamina = dungeons::charge(db, s, a, n(d, "ReqStaminaType"), cost).await?;
    let mut params = r.0.clone();
    if let Some(f) = s.tables.battle.find(
        "TowerFloor",
        &[
            ("ChapterIndex", n(d, "ChapterIndex")),
            ("DungeonIndex", n(d, "DungeonIndex")),
        ],
    ) {
        params.insert("TowerIndex".into(), n(f, "TowerIndex").to_string());
        params.insert("TowerFloor".into(), n(f, "Floor").to_string());
    }
    let request = Request(params);
    let mut out = item::success();
    for _ in 0..count {
        dungeons::validate(db, s, a, &request, d).await?;
        let mut entry = json!({});
        let mut start = json!({});
        dungeons::enter(db, s, a, &request, &mut entry, &mut start).await?;
        let mut result = campaign::complete(db, s, a, &request, &[], 3, 1).await?;
        dungeons::finish(
            db,
            s,
            a,
            &request,
            &Request(Default::default()),
            &entry,
            true,
            &mut result,
        )
        .await?;
        dungeons::append_rewards(&mut out, &result);
        out["CampaignResults"] = result["CampaignResults"].clone();
        if let Some(tower) = result["TowerInfos"].as_array().and_then(|v| v.first()) {
            out["TowerInfoResult"] = tower.clone();
        }
    }
    out["StaminaResult"] = stamina;
    out["StartDungeonIndex"] = d["DungeonIndex"].clone();
    out["EndDungeonIndex"] = d["DungeonIndex"].clone();
    if let Some(v) = removed {
        out["ItemResults"].as_array_mut().unwrap().push(v);
    }
    Ok(out)
}
