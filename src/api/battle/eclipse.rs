//! The client's native EclipseContext/OfflinePlay lifecycle. Online entry still
//! requires the separate battle transport; it is never redirected here.
use super::*;

fn stages(s: &AppState) -> Result<Vec<Value>> {
    let mut rows: Vec<_> = s
        .tables
        .battle
        .rows("CampaignDungeon")
        .iter()
        .filter(|v| n(v, "BattleType") == 36)
        .map(|v| json!({"ChapterIndex":v["ChapterIndex"],"DungeonIndex":v["DungeonIndex"]}))
        .collect();
    rows.sort_by_key(|v| (n(v, "ChapterIndex"), n(v, "DungeonIndex")));
    if rows.is_empty() {
        return Err(rule("DungeonNotFound"));
    }
    Ok(rows)
}

// The native updater advances through each stage, then repeats the final stage
// with an increasing creature level (NCampaignState.WavePause).
fn position(s: &AppState, wave: i64) -> Result<Value> {
    let stages = stages(s)?;
    let mut remaining = wave;
    for (i, stage) in stages.iter().enumerate() {
        let count = s
            .tables
            .battle
            .rows("EclipseWave")
            .iter()
            .filter(|v| {
                v["ChapterIndex"] == stage["ChapterIndex"]
                    && v["DungeonIndex"] == stage["DungeonIndex"]
                    && n(v, "Difficulty") == 0
                    && v["Scenario"] == false
            })
            .map(|v| n(v, "WaveIndex"))
            .max()
            .unwrap_or(0);
        if count == 0 || remaining <= 0 {
            return Err(rule("DungeonNotFound"));
        }
        if remaining <= count || i + 1 == stages.len() {
            return Ok(
                json!({"ChapterIndex":stage["ChapterIndex"],"DungeonIndex":stage["DungeonIndex"],
                "WaveIndex":(remaining-1)%count+1,"CreatureLevel":(remaining-1)/count}),
            );
        }
        remaining -= count;
    }
    Err(rule("DungeonNotFound"))
}

async fn start_wave(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<i64> {
    let record = n(&get(db, a, "eclipse_record", 0).await?, "BestWave");
    Ok(n(
        row(s, "EclipseStart", &[("RecordWave", record)])?,
        "StartWave",
    )
    .max(1))
}

pub(super) async fn info(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    decks: Vec<Value>,
) -> Result<Value> {
    let run = get(db, a, "eclipse", 0).await?;
    let wave = start_wave(db, s, a).await?;
    let pos = position(s, wave)?;
    let active = run["IsPlayEclipse"] == true;
    let record = get(db, a, "eclipse_record", 0).await?;
    Ok(json!({"EclipseInfo":{
        "MatchIndex":n(&run,"MatchIndex"),"IsDeckSave":!decks.is_empty(),"IsPlayEclipse":active,
        "MaxWaveIndex":n(&record,"BestWave"),"CurrentWaveIndex":if active{n(&run,"TeamWave")}else{wave},
        "DeckIndex":if active{n(&run,"DeckIndex")}else{1},"LastDeckIndex":n(&run,"LastDeckIndex"),
        "ChapterIndex":if active{run["StartPosition"]["ChapterIndex"].clone()}else{pos["ChapterIndex"].clone()},
        "DungeonIndex":if active{run["StartPosition"]["DungeonIndex"].clone()}else{pos["DungeonIndex"].clone()},
        "ExpireTime":if active{run["ExpireTime"].clone()}else{Value::Null},"DeckResults":decks,
        "EclipseStaminaResult":dungeons::charge(db,s,a,22,0).await?,
        "EclipsePointResult":hero::currency(db,a,"EclipsePoint",0).await?
    }}))
}

pub(super) async fn ensure_available(db: &mut SqliteConnection, a: i64) -> Result<()> {
    if get(db, a, "eclipse", 0).await?["IsPlayEclipse"] == true {
        return Err(rule("AlreadyOnBattleHero"));
    }
    Ok(())
}

pub(super) async fn begin(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    dungeons::require_godking_unlock(db, s, a).await?;
    rooms::validate_battle(db, s, a, r).await?;
    if s.tables.services.rules["RequireBattleService"] == true {
        return Err(rule("BattleServerNotFound"));
    }
    if campaign::difficulty(r)? != 0 || boolean(r, "ScenarioDungeon", false)? {
        return Err(rule("InvalidDiff"));
    }
    let mut run = get(db, a, "eclipse", 0).await?;
    let continuing = run["IsPlayEclipse"] == true;
    let requested_tickets = r.number("EnterTicketCount", 1)?;
    // Native automatic team handoff can send zero from its uninitialized
    // reward cache. The active run already owns its paid ticket multiplier.
    let tickets = if continuing && requested_tickets == 0 {
        n(&run, "EnterTicketCount")
    } else { requested_tickets };
    let speed = r.number("OnlineGameSpeedRatio", 1)?;
    if !(1..=s.tables.hero_shop.constant("MaxEclipseTicket", 5)).contains(&tickets)
        || !(1..=3).contains(&speed)
    {
        return Err(rule("Fail"));
    }
    if continuing {
        if n(&run, "Expires") < now() || n(&run, "EnterTicketCount") != tickets {
            return Err(rule("DungeonNotFound"));
        }
    } else {
        let wave = start_wave(db, s, a).await?;
        let pos = position(s, wave)?;
        // Validate the entry coordinates before any currency mutation.
        if int(r, "ChapterIndex")? != n(&pos, "ChapterIndex")
            || int(r, "DungeonIndex")? != n(&pos, "DungeonIndex")
        {
            return Err(rule("DungeonNotFound"));
        }
        let active: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=0 AND started>=?)")
            .bind(a).bind(now()-settings(s,"BattleExpirySeconds",14400)).fetch_one(&mut *db).await?;
        if active {
            return Err(rule("AlreadyOnBattleHero"));
        }
        let decks = list(db, a, "eclipse_deck").await?;
        if decks.is_empty() {
            return Err(rule("EmptyHeroIndices"));
        }
        let mut seen = BTreeSet::new();
        let mut snapshot = vec![];
        for (i, deck) in decks.iter().enumerate() {
            let ids = special::eclipse_heroes(&deck["HeroIndices"])?;
            if ids.is_empty() || n(deck, "DeckIndex") != i as i64 + 1 {
                return Err(rule("EmptyHeroIndices"));
            }
            let mut heroes = vec![];
            for id in ids {
                if !seen.insert(id) {
                    return Err(rule("DuplicatedHero"));
                }
                heroes.push(special::cached_hero(db, a, id as i32).await?);
            }
            snapshot.push(json!({"DeckIndex":i+1,"HeroInfos":heroes,"HeroConditionInfos":[]}));
        }
        dispatch::ensure_available(db, a, &seen.into_iter().collect::<Vec<_>>(), None).await?;
        // Keep identities unique even after a GM reset deletes battle_state.
        let match_id = sqlx::query("INSERT INTO eclipse_run_ids(account) VALUES(?)")
            .bind(a)
            .execute(&mut *db)
            .await?
            .last_insert_rowid();
        if match_id > i32::MAX as i64 {
            return Err(rule("Fail"));
        }
        let stamina = dungeons::charge(db, s, a, 22, tickets).await?;
        run = json!({"MatchIndex":match_id,"IsPlayEclipse":true,"DeckIndex":1,"LastDeckIndex":snapshot.len(),
            "StartPosition":pos,"StartWaveIndex":wave,"MaxWaveIndex":0,"TeamWave":wave,"EnterTicketCount":tickets,
            "OnlineGameSpeedRatio":speed,"Expires":now()+settings(s,"BattleExpirySeconds",14400),
            "ExpireTime":time(now()+settings(s,"BattleExpirySeconds",14400)),"DeckList":snapshot,
            "Status":"BattleStart","StaminaResult":stamina,"Records":[],"TeamActive":false});
        for mut deck in decks {
            deck["ClearMaxWaveIndex"] = json!(0);
            put(db, a, "eclipse_deck", n(&deck, "DeckIndex"), &deck).await?;
        }
    }
    let pos = &run["StartPosition"];
    if int(r, "ChapterIndex")? != n(pos, "ChapterIndex")
        || int(r, "DungeonIndex")? != n(pos, "DungeonIndex")
    {
        return Err(rule("DungeonNotFound"));
    }
    if run["TeamActive"] == true {
        if n(&run, "OnlineGameSpeedRatio") != speed {
            return Err(rule("Fail"));
        }
        return Ok(run["BeginResponse"].clone());
    }
    if n(&run, "DeckIndex") > n(&run, "LastDeckIndex") || run["Status"] == "BattleEnd" {
        return Err(rule("AlreadyCompleted"));
    }
    let mut out = response(s, "campaign/begin_campaign");
    out["StaminaResult"] = if continuing {
        dungeons::charge(db, s, a, 22, 0).await?
    } else {
        run["StaminaResult"].clone()
    };
    out["EclipseBattleInfo"] = json!({"MatchIndex":run["MatchIndex"],"DeckList":run["DeckList"],"DungeonList":stages(s)?,
        "CurrentDeckIndex":run["DeckIndex"],"CurrentChapterIndex":pos["ChapterIndex"],"CurrentDungeonIndex":pos["DungeonIndex"],
        "CurrentWaveIndex":pos["WaveIndex"],"MaxWaveIndex":run["StartWaveIndex"],"CurrentCreatureLevel":pos["CreatureLevel"],
        "OnlineGameSpeedRatio":speed,"GuildSkills":[],"AccountBuffs":[],"ClassBuffDataBases":[],"ExtraStatDataBases":[],"PetStatDataBases":[]});
    run["TeamActive"] = json!(true);
    run["OnlineGameSpeedRatio"] = json!(speed);
    run["TeamStarted"] = json!(now());
    run["TeamWave"] = run["StartWaveIndex"].clone();
    run["LastReport"] = Value::Null;
    run["EndResponse"] = Value::Null;
    run["Status"] = json!("BattleStart");
    run["BeginResponse"] = out.clone();
    put(db, a, "eclipse", 0, &run).await?;
    Ok(out)
}

fn dungeon_info(run: &Value) -> Value {
    json!({"MatchIndex":run["MatchIndex"],"ChapterIndex":run["StartPosition"]["ChapterIndex"],
        "DungeonIndex":run["StartPosition"]["DungeonIndex"],"WaveIndex":run["TeamWave"],"Status":run["Status"],
        "DeckIndex":run["DeckIndex"],"StartWaveIndex":run["StartWaveIndex"],"MaxWaveIndex":run["MaxWaveIndex"],
        "CurrentCreatureLevel":run["StartPosition"]["CreatureLevel"],"ExpireTime":run["ExpireTime"],
        "OnlineGameSpeedRatio":run["OnlineGameSpeedRatio"],"EnterTicketCount":run["EnterTicketCount"],
        "RewardedTime":run["RewardedTime"],"UpdatedTime":time(now())})
}

pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let mut run = get(db, a, "eclipse", 0).await?;
    if run.is_null() || int(r, "MatchIndex")? != n(&run, "MatchIndex") {
        return Err(rule("DungeonNotFound"));
    }
    if action == "save_eclipse_result" {
        if r.number("AccountId", a)? != a || n(&run, "Expires") < now() {
            return Err(rule("Fail"));
        }
        let fields = [
            "PlayTime",
            "TotalDamage",
            "CurrentDeckIndex",
            "CurrentChapterIndex",
            "CurrentDungeonIndex",
            "CurrentWaveIndex",
            "MaxWaveIndex",
            "CurrentCreatureLevel",
            "LastStatus",
        ];
        let mut report = json!({});
        for key in fields {
            // Native SaveEclipseResult.TotalDamage is a signed 64-bit long.
            // Maxed parties can exceed Int32.MaxValue during the first team.
            let value = if key == "TotalDamage" {
                r.number(key, 0)?
            } else {
                int(r, key)?
            };
            if value < 0 { return Err(rule("InvalidRequest")); }
            report[key] = json!(value);
        }
        if run["LastReport"] == report {
            return Ok(item::success());
        }
        if run["IsPlayEclipse"] != true || run["TeamActive"] != true {
            return Err(rule("Fail"));
        }
        let status = n(&report, "LastStatus");
        let next = matches!(status, 7 | 9);
        let deck = n(&run, "DeckIndex");
        let max = n(&report, "MaxWaveIndex");
        let cap = s
            .tables
            .battle
            .rows("EclipseStart")
            .iter()
            .map(|v| n(v, "RecordWave"))
            .max()
            .unwrap_or(0)
            + 1;
        let elapsed = (now() - n(&run, "TeamStarted") + 5).max(1) * n(&run, "OnlineGameSpeedRatio");
        // EclipseContext sends its initial coordinates even after advancing;
        // do not confuse those fields with authoritative progress. They must
        // nevertheless describe a real wave in the supplied client tables.
        let valid_wave = s.tables.battle.rows("EclipseWave").iter().any(|v| {
            v["ChapterIndex"] == report["CurrentChapterIndex"]
                && v["DungeonIndex"] == report["CurrentDungeonIndex"]
                && v["WaveIndex"] == report["CurrentWaveIndex"]
                && n(v, "Difficulty") == 0
                && v["Scenario"] == false
        });
        if !matches!(status, 1 | 2 | 3 | 7 | 9)
            || n(&report, "CurrentDeckIndex") != deck + i64::from(next)
            || max < n(&run, "TeamWave")
            || max > cap
            || max - n(&run, "StartWaveIndex") > elapsed
            || n(&report, "PlayTime") > elapsed
            || !valid_wave
            || n(&report, "CurrentCreatureLevel") > n(&position(s, max)?, "CreatureLevel")
            || n(&report, "PlayTime") < n(&run["LastReport"], "PlayTime")
            || n(&report, "TotalDamage") < n(&run["LastReport"], "TotalDamage")
            || (status == 3 && deck != n(&run, "LastDeckIndex"))
            || (status == 7 && deck >= n(&run, "LastDeckIndex"))
        {
            return Err(rule("Fail"));
        }
        run["TeamWave"] = json!(max);
        run["MaxWaveIndex"] = json!(n(&run, "MaxWaveIndex").max(max - 1));
        run["LastReport"] = report;
        run["Status"] = json!(match status {
            1 => "BattleStart",
            2 => "BattleContinue",
            3 => "BattleEnd",
            7 => "BattleTeamEndOff",
            _ => "BattleOut",
        });
        if matches!(status, 3 | 7 | 9) {
            run["TeamActive"] = json!(false);
            run["Records"]
                .as_array_mut()
                .ok_or_else(|| rule("Fail"))?
                .push(json!({"DeckIndex":deck,"ClearWave":max-1}));
            run["DeckIndex"] = json!(deck + i64::from(next));
            let mut saved = get(db, a, "eclipse_deck", deck).await?;
            saved["ClearMaxWaveIndex"] = json!(max - 1);
            put(db, a, "eclipse_deck", deck, &saved).await?;
        }
        put(db, a, "eclipse", 0, &run).await?;
        return Ok(item::success());
    }
    if matches!(action, "give_up_eclipse_dungeon" | "end_eclipse") {
        if run["IsPlayEclipse"] != true {
            return Err(rule("AlreadyCompleted"));
        }
        if action == "end_eclipse" && run["Status"] != "BattleEnd" {
            return Err(rule("Fail"));
        }
        if action == "give_up_eclipse_dungeon" {
            run["Status"] = json!("BattleGiveUp");
        }
        return settle(db, s, a, &mut run).await;
    }
    Err(rule("Fail"))
}

pub(super) async fn end_campaign(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let mut run = get(db, a, "eclipse", 0).await?;
    let current = position(s, n(&run, "TeamWave").max(1))?;
    let valid_coordinates = [&run["StartPosition"], &current].iter().any(|v| {
        n(v, "ChapterIndex") == r.number("ChapterIndex", 0).unwrap_or(0)
            && n(v, "DungeonIndex") == r.number("DungeonIndex", 0).unwrap_or(0)
    });
    if run.is_null()
        || campaign::difficulty(r)? != 0
        || boolean(r, "ScenarioDungeon", false)?
        || !valid_coordinates
    {
        return Err(rule("DungeonNotFound"));
    }
    if run["TeamActive"] == true
        || !matches!(
            run["Status"].as_str(),
            Some("BattleEnd" | "BattleTeamEndOff" | "BattleOut")
        )
    {
        return Err(rule("NotCompletedBattle"));
    }
    if !run["EndResponse"].is_null() {
        return Ok(run["EndResponse"].clone());
    }
    let mut out = response(s, "campaign/end_campaign");
    if run["Status"] == "BattleEnd" || n(&run, "DeckIndex") > n(&run, "LastDeckIndex") {
        merge(&mut out, settle(db, s, a, &mut run).await?);
    } else {
        out["EclipseDungeonInfo"] = dungeon_info(&run);
    }
    out["EclipseStaminaResult"] = dungeons::charge(db, s, a, 22, 0).await?;
    run["EndResponse"] = out.clone();
    put(db, a, "eclipse", 0, &run).await?;
    Ok(out)
}

async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64, run: &mut Value) -> Result<Value> {
    claim(db, a, "eclipse", n(run, "MatchIndex"), "run").await?;
    let mut records = run["Records"].as_array().cloned().unwrap_or_default();
    if run["TeamActive"] == true {
        records.push(json!({"DeckIndex":run["DeckIndex"],"ClearWave":n(run,"TeamWave")-1}));
    }
    let last = s
        .tables
        .battle
        .rows("EclipseReward")
        .iter()
        .map(|v| n(v, "Wave"))
        .max()
        .ok_or_else(|| rule("Fail"))?;
    let repeat = s
        .tables
        .hero_shop
        .constant("EclipseRewardRepeatFactor", 10)
        .clamp(1, last);
    let mut reward = Rewards::default();
    for _ in 0..n(run, "EnterTicketCount") {
        for record in &records {
            let cleared = n(record, "ClearWave");
            if cleared < n(run, "StartWaveIndex") {
                continue;
            }
            for wave in 1..=cleared {
                let index = if wave <= last {
                    wave
                } else {
                    last - repeat + 1 + (wave - last - 1) % repeat
                };
                reward_index(
                    db,
                    s,
                    a,
                    n(row(s, "EclipseReward", &[("Wave", index)])?, "NormalReward"),
                    &mut reward,
                )
                .await?;
            }
        }
        let best = n(run, "MaxWaveIndex");
        if best >= n(run, "StartWaveIndex") {
            // Native best-team preview uses the highest team's single reward row.
            if let Some(row) = s.tables.battle.find("EclipseReward", &[("Wave", best)]) {
                reward_index(db, s, a, n(row, "BestTeamReward"), &mut reward).await?;
            }
        }
    }
    let previous = get(db, a, "eclipse_record", 0).await?;
    put(
        db,
        a,
        "eclipse_record",
        0,
        &json!({"BestWave":n(&previous,"BestWave").max(n(run,"MaxWaveIndex"))}),
    )
    .await?;
    run["IsPlayEclipse"] = json!(false);
    run["TeamActive"] = json!(false);
    run["RewardedTime"] = json!(time(now()));
    let mut out = rewards(db, s, a, reward).await?;
    out["EclipseDungeonInfo"] = dungeon_info(run);
    put(db, a, "eclipse", 0, run).await?;
    Ok(out)
}
