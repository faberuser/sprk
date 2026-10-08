use super::*;

fn dispatch_raid<'a>(s: &'a AppState, d: &Value) -> Result<Option<&'a Value>> {
    let kind = match n(d, "BattleType") {
        12 => 1,
        29 => 3,
        40..=42 => 15,
        _ => return Ok(None),
    };
    let mut matches = s.tables.battle.rows("Raid").iter().filter(|raid| {
        n(raid, "ChapterIndex") == n(d, "ChapterIndex")
            && n(raid, "DungeonIndex") == n(d, "DungeonIndex")
            && n(raid, "Type") == kind
            && raid["IsOnlineSingle"] == false
            && raid["IsOpen"] == true
    });
    let raid = matches.next().ok_or_else(|| rule("ContentsDisabled"))?;
    if matches.next().is_some() {
        return Err(rule("ContentsDisabled"));
    }
    Ok(Some(raid))
}
pub(super) async fn ensure_available(
    db: &mut SqliteConnection,
    a: i64,
    party: &[i64],
    except: Option<i64>,
) -> Result<()> {
    super::eclipse::ensure_available(db, a).await?;
    super::super::community::ensure_available(db, a, party).await?;
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
// Native ServerResult is a list of per-run integers, not a serialized reward response.
fn result_string(results: &[Value]) -> String {
    results
        .iter()
        .map(|v| n(v, "Outcome").to_string())
        .collect::<Vec<_>>()
        .join(",")
}
fn advance(info: &mut Value, stamp: i64) {
    let Some(results) = info["SimulationResults"].as_array().cloned() else {
        return;
    };
    if matches!(info["State"].as_str(), Some("Complete" | "Cancel")) {
        return;
    }
    let elapsed = (stamp - n(info, "StartTimestamp")).max(0) * 1000;
    let mut deadline = 0;
    let completed = if stamp >= n(info, "FinishTimestamp") {
        results.len()
    } else {
        results
            .iter()
            .take_while(|v| {
                deadline += n(v, "TimeMs");
                deadline <= elapsed
            })
            .count()
    };
    let wins = results[..completed]
        .iter()
        .filter(|v| n(v, "Outcome") == 1)
        .count();
    info["WinCount"] = json!(wins);
    info["LoseCount"] = json!(completed - wins);
    info["ServerResult"] = json!(result_string(&results[..completed]));
    info["State"] = json!(if completed == results.len() {
        "ReadyToComplete"
    } else {
        "Battle"
    });
}
async fn simulation_result(db: &mut SqliteConnection, a: i64, r: &Request) -> Result<Value> {
    let slot = int(r, "SlotIndex")?;
    let mut info = get(db, a, "dispatch", slot).await?;
    if n(&info, "SimulationVersion") != 1
        || info["SimulationJobId"] != r.text("SimulationJobId")
        || matches!(info["State"].as_str(), Some("Complete" | "Cancel"))
    {
        return Err(rule("DispatchBattleNotFound"));
    }
    let outcomes: Vec<i64> = read_json(r.text("Results")).map_err(|_| rule("InvalidCount"))?;
    let times: Vec<i64> = read_json(r.text("TimesMs")).map_err(|_| rule("InvalidCount"))?;
    if outcomes.len() != n(&info, "RepeatCount") as usize
        || times.len() != outcomes.len()
        || outcomes.iter().any(|v| !matches!(v, 0 | 1))
        || times.iter().any(|v| !(1..=3600000).contains(v))
    {
        return Err(rule("InvalidCount"));
    }
    let results = json!(
        outcomes
            .iter()
            .zip(&times)
            .map(|(outcome, time)| json!({"Outcome":outcome,"TimeMs":time}))
            .collect::<Vec<_>>()
    );
    if info["SimulationResults"].is_array() {
        if info["SimulationResults"] != results {
            return Err(rule("AlreadyFinishDispatchBattle"));
        }
    } else {
        let total: i64 = times.iter().sum();
        let start = now();
        let finish = start + (total + 999) / 1000;
        info["SimulationResults"] = results;
        info["ClientResult"] = json!(
            outcomes
                .iter()
                .map(ToString::to_string)
                .collect::<Vec<_>>()
                .join(",")
        );
        info["ClientResultTimeMs"] = json!((total / outcomes.len() as i64).max(1));
        info["StartTimestamp"] = json!(start);
        info["FinishTimestamp"] = json!(finish);
        info["BeginTime"] = json!(time(start));
        info["CompleteTime"] = json!(time(finish));
        info["ServerResult"] = json!("");
        info["State"] = json!("Battle");
    }
    advance(&mut info, now());
    put(db, a, "dispatch", slot, &info).await?;
    Ok(json!({"DispatchInfo":info}))
}
pub(super) async fn snapshot(db: &mut SqliteConnection, a: i64) -> Result<Vec<Value>> {
    let mut runs = list(db, a, "dispatch").await?;
    // Native clients reserve every hero present in this snapshot, regardless of
    // state. Keep terminal records in storage for duplicate-request protection.
    runs.retain(|run| !matches!(run["State"].as_str(), Some("Complete" | "Cancel")));
    for run in &mut runs {
        if n(run, "SimulationVersion") == 1 {
            advance(run, now());
            put(db, a, "dispatch", n(run, "SlotIndex"), run).await?;
        } else if run["State"] == "Battle" && n(run, "FinishTimestamp") <= now() {
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
        if !r.text("SimulationJobId").is_empty() {
            return simulation_result(db, a, r).await;
        }
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
        let mut request = Request(parameters);
        let d = campaign::dungeon(s, &request)?;
        // Native dispatch requests identify the dungeon, but omit raid fields.
        // Derive them from the solo raid table instead of trusting caller extras.
        if let Some(raid) = dispatch_raid(s, d)? {
            request
                .0
                .insert("RaidIndex".into(), n(raid, "Index").to_string());
            request
                .0
                .insert("RaidLevel".into(), n(raid, "Level").to_string());
        }
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
        if !matches!(n(d, "BattleType"), 1 | 2 | 10 | 12 | 29 | 40..=42)
            || !matches!(n(d, "ReqStaminaType"), 0 | 1)
        {
            return Err(rule("ContentsDisabled"));
        }
        campaign::validate(db, s, a, &request, &party).await?;
        let progress =
            campaign::progress(db, s, a, n(d, "ChapterIndex"), n(d, "DungeonIndex")).await?;
        if n(&progress, "FirstRewardedDiff") & (1 << diff) == 0 {
            return Err(rule("NotCompletedDungeon"));
        }
        let count = int(r, "RepeatCount")?;
        if count < 1 || count > settings(s, "DispatchMaxRepeat", 200) {
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
        let mut receipt = if entry_costs::enabled(db, a).await? {
            entry_costs::deferred_receipt()
        } else {
            json!({})
        };
        let stamina = if receipt["DeferredEntryCosts"] == true {
            let result =
                entry_costs::charge(db, s, a, &mut receipt, n(d, "ReqStaminaType"), cost).await?;
            // Offline jobs keep their budget until collection or cancellation.
            sqlx::query("UPDATE battle_entry_holds SET expires=? WHERE account=? AND owner=?")
                .bind(i64::MAX)
                .bind(a)
                .bind(receipt["EntryCostReservation"].as_str().unwrap())
                .execute(&mut *db)
                .await?;
            result
        } else {
            dungeons::charge_reserved(db, s, a, n(d, "ReqStaminaType"), cost).await?
        };
        let finish = now() + settings(s, "DispatchSecondsPerBattle", 60).max(1) * count;
        request.0.remove("SessionKey");
        let mut info = json!({"SlotIndex":slot,"ChapterIndex":n(d,"ChapterIndex"),"DungeonIndex":n(d,"DungeonIndex"),"Difficulty":diff,"DeckIndex":int(r,"DeckIndex")?,"RepeatCount":count,"HeroIndices":json!(party).to_string(),"BeginTime":time(now()),"CompleteTime":time(finish),"ClientResult":null,"ClientResultTimeMs":0,"ServerResult":null,"State":"Battle","WinCount":0,"LoseCount":0,"StaminaDiscountRate":0,"FinishTimestamp":finish,"StartTimestamp":now(),"Cost":cost,"CostType":n(d,"ReqStaminaType"),"Request":request.0});
        info["SimulationVersion"] = json!(1);
        info["SimulationJobId"] = json!(uuid::Uuid::new_v4().to_string());
        info["State"] = json!("Simulation");
        if receipt["DeferredEntryCosts"] == true {
            info["EntryCostHold"] = receipt;
        }
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
    advance(&mut info, now());
    if !cancel && (info["State"] == "Simulation" || now() < n(&info, "FinishTimestamp")) {
        return Err(rule("NotCompletedDungeon"));
    }
    let total = n(&info, "RepeatCount");
    let count = if n(&info, "SimulationVersion") == 1 {
        n(&info, "WinCount") + n(&info, "LoseCount")
    } else if cancel {
        ((now() - n(&info, "StartTimestamp")) / settings(s, "DispatchSecondsPerBattle", 60).max(1))
            .clamp(0, total)
    } else {
        total
    };
    let wins = if n(&info, "SimulationVersion") == 1 {
        n(&info, "WinCount")
    } else {
        count
    };
    let losses = count - wins;
    let req = request(&info);
    let heroes: Vec<i64> = read_json(info["HeroIndices"].as_str().unwrap_or("[]"))?;
    let mut out = if wins > 0 {
        campaign::complete(db, s, a, &req, &heroes, 3, wins).await?
    } else {
        item::success()
    };
    if wins > 0 {
        if let Some(raid) = dispatch_raid(s, campaign::dungeon(s, &req)?)? {
            let mut extra = Rewards::default();
            for _ in 0..wins {
                reward_index(db, s, a, n(raid, "IndividualReward"), &mut extra).await?;
            }
            dungeons::append_rewards(&mut out, &rewards(db, s, a, extra).await?);
        }
    }
    let deferred = info["EntryCostHold"]["DeferredEntryCosts"] == true;
    if deferred {
        let mut receipt = info["EntryCostHold"].clone();
        if let Some(costs) = receipt["VictoryEntryCosts"].as_array_mut() {
            for cost in costs {
                cost["Amount"] = json!(n(cost, "Amount") / total * wins);
            }
        }
        entry_costs::settle(db, s, a, &mut receipt, wins > 0, &mut out).await?;
        info["EntryCostHold"] = receipt;
        if !out["ExpResult2"].is_null() {
            out["ExpResult"] = out["ExpResult2"].clone();
        }
    }
    if !deferred && wins < total {
        let refund = n(&info, "Cost") / total * (total - wins);
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
    // The native dispatch response has no equipment field. Deliver rolled gear
    // through mail so collection updates the bag through its supported protocol.
    let mut equipment: Vec<crate::models::equip::EquipItemInfo> = read_value(
        out["EquipItemResults"]
            .as_array()
            .cloned()
            .map(Value::Array)
            .unwrap_or(json!([])),
    )?;
    for equip in &mut equipment {
        sqlx::query("DELETE FROM equip_items WHERE account_id=? AND slot_index=?")
            .bind(a)
            .bind(equip.slot_index)
            .execute(&mut *db)
            .await?;
        equip.slot_index = 0;
        equip.uid = String::new();
    }
    for attachments in equipment.chunks(100) {
        sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_equipment) VALUES(?,'System','Dispatch equipment rewards','Equipment earned by your dispatch. Claim it here to add it to your bag.',?)")
            .bind(a).bind(json!(attachments).to_string()).execute(&mut *db).await?;
    }
    out["EquipItemResults"] = json!([]);
    out["EquipItemInfos"] = json!([]);
    if !deferred && n(&info, "CostType") == 1 && wins > 0 {
        out["ExpResult"] = dungeons::stamina_exp(db, s, a, n(&info, "Cost") / total * wins).await?;
    }
    out["TeamExpResult"] = out["ExpResult"].clone();
    info["State"] = json!(if cancel { "Cancel" } else { "Complete" });
    info["WinCount"] = json!(wins);
    info["LoseCount"] = json!(losses);
    info["CompleteTime"] = json!(time(now()));
    if n(&info, "SimulationVersion") != 1 {
        info["ServerResult"] = json!(vec!["1"; count as usize].join(","));
    }
    info["CollectionResponse"] = out.clone();
    if !out["StaminaResult"].is_null() {
        out["EclipseStaminaResult"] = out["StaminaResult"].clone();
    }
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
