use super::*;

pub(super) fn dungeon<'a>(s: &'a AppState, r: &Request) -> Result<&'a Value> {
    let c = int(r, "ChapterIndex")?;
    let d = int(r, "DungeonIndex")?;
    row(
        s,
        "CampaignDungeon",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    )
}
pub(super) fn difficulty(r: &Request) -> Result<i64> {
    let d = r.number("DungeonDifficulty", 1)?;
    if !(0..=3).contains(&d) {
        return Err(rule("InvalidDiff"));
    }
    Ok(d)
}
pub(super) fn field(d: &Value, key: &str, diff: i64) -> i64 {
    n(
        d,
        &format!(
            "{key}_{}",
            ["Easy", "Normal", "Hard", "Hell"][diff as usize]
        ),
    )
}
pub(super) fn key(c: i64, d: i64) -> i64 {
    (c << 32) | d
}
pub(super) async fn progress(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    c: i64,
    d: i64,
) -> Result<Value> {
    let mut p = get(db, a, "dungeon", key(c, d)).await?;
    if p.is_null() {
        let old=sqlx::query("SELECT clear_count,best_star,completed_time FROM campaign_progress WHERE account_id=? AND chapter_id=? AND dungeon_id=?").bind(a).bind(c).bind(d).fetch_optional(&mut *db).await?;
        let count = old
            .as_ref()
            .map(|r| r.get::<i64, _>("clear_count"))
            .unwrap_or(0);
        let diff = s.tables.tutorials.dungeon_difficulty(c as i32, d as i32) as i64;
        let star = old
            .as_ref()
            .map(|r| r.get::<i64, _>("best_star"))
            .unwrap_or(0);
        p = json!({"ChapterIndex":c,"DungeonIndex":d,"MaxStar":if count>0{diff*10+star}else{0},"FirstRewardedDiff":if count>0{1<<diff}else{0},"ScenarioComplete":if count>0{1}else{0},"CompletedTime":old.as_ref().and_then(|r|r.get::<Option<String>,_>("completed_time")),"DailyCompletedCount":0,"ResetCount":0,"Day":day()});
    }
    if p["Day"] != day() {
        p["DailyCompletedCount"] = json!(0);
        p["ResetCount"] = json!(0);
        p["Day"] = json!(day());
    }
    Ok(p)
}
pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    heroes: &[i64],
) -> Result<()> {
    let d = dungeon(s, r)?;
    let c = n(d, "ChapterIndex");
    let ch = row(s, "CampaignChapter", &[("Index", c)])?;
    let diff = difficulty(r)?;
    if ch["IsOpen"] != true && !(n(d,"BattleType")==16 && s.tables.arena_guild.rules["EnableLegacyGuildRaids"]==true) {
        return Err(rule("NotOpenedDungeon"));
    }
    if diff > n(ch, "MaxDifficulty") && d["NoDifficulty"] != true {
        return Err(rule("InvalidDiff"));
    }
    let req_c = n(ch, "ReqChapterIndex");
    let req_d = n(ch, "ReqDungeonIndex");
    if req_c > 0
        && req_d > 0
        && n(
            &progress(db, s, a, req_c, req_d).await?,
            "FirstRewardedDiff",
        ) == 0
    {
        return Err(rule("NotCompletedReqDungeon"));
    }
    if heroes.is_empty() {
        return Err(rule("EmptyHeroIndices"));
    }
    let max = if n(d, "MainSquardCount") > 0 {
        n(d, "MainSquardCount") + n(d, "SubSquardCount")
    } else {
        4
    };
    if heroes.len() as i64 > max {
        return Err(rule("NotMatchHeroIndices"));
    }
    owned(db, a, heroes).await?;
    dispatch::ensure_available(db, a, heroes, None).await?;
    let p = progress(db, s, a, c, n(d, "DungeonIndex")).await?;
    if n(&p, "FirstRewardedDiff") == 0 && matches!(n(d, "BattleType"), 1 | 2 | 10) {
        let predecessors = s
            .tables
            .battle
            .rows("CampaignDungeon")
            .iter()
            .filter(|v| {
                n(v, "ChapterIndex") == c && n(v, "NextDungeonIndex") == n(d, "DungeonIndex")
            })
            .collect::<Vec<_>>();
        if !predecessors.is_empty() {
            let mut unlocked = false;
            for previous in predecessors {
                unlocked |= n(
                    &progress(db, s, a, c, n(previous, "DungeonIndex")).await?,
                    "FirstRewardedDiff",
                ) > 0;
            }
            if !unlocked {
                return Err(rule("CannotVisitDungeon"));
            }
        }
    }
    let limit = n(ch, "DailyEnterMaxCount");
    if limit > 0 && n(&p, "DailyCompletedCount") >= limit {
        return Err(rule("MaxDailyCount"));
    }
    // Closed/unsupported multiplayer and mode requests must pass their own state checks.
    dungeons::validate(db, s, a, r, d).await?;
    rooms::validate_battle(db, s, a, r).await?;
    Ok(())
}
pub(super) async fn begin(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let mut party = ids(r, "HeroIndices", 32)?;
    let group = ids(r, "GroupHeroIndices", 32)?;
    party.extend(group);
    if party.iter().collect::<BTreeSet<_>>().len() != party.len() {
        return Err(rule("DuplicatedHero"));
    }
    let mut entry = json!({"ChapterIndex":int(r,"ChapterIndex")?,"DungeonIndex":int(r,"DungeonIndex")?,"DungeonDifficulty":difficulty(r)?,"ScenarioDungeon":boolean(r,"ScenarioDungeon",false)?,"Heroes":party,"Request":r.0});
    let previous = sqlx::query(
        "SELECT started,completed,entry,begin_response FROM battle_runs WHERE account=?",
    )
    .bind(a)
    .fetch_optional(&mut *db)
    .await?;
    if let Some(old) = previous {
        if old.get::<i64, _>("completed") == 0
            && now() - old.get::<i64, _>("started") <= settings(s, "BattleExpirySeconds", 14400)
        {
            let saved: Value = read_json(&old.get::<String, _>("entry"))?;
            if [
                "ChapterIndex",
                "DungeonIndex",
                "DungeonDifficulty",
                "ScenarioDungeon",
                "Heroes",
            ]
            .iter()
            .all(|k| saved[*k] == entry[*k])
            {
                return Ok(read_json(&old.get::<String, _>("begin_response"))?);
            }
            return Err(rule("AlreadyOnBattleHero"));
        }
    }
    validate(db, s, a, r, &party).await?;
    let leader = int(r, "LeaderHeroIndex")?;
    if leader > 0 && !party.contains(&leader) {
        return Err(rule("HeroNotFound"));
    }
    let d = dungeon(s, r)?;
    let mut out = response(s, "campaign/begin_campaign");
    let cost = stamina_cost(d, difficulty(r)?);
    out["StaminaResult"] = dungeons::charge(db, s, a, n(d, "ReqStaminaType"), cost).await?;
    dungeons::enter(db, s, a, r, &mut entry, &mut out).await?;
    let selected = selected_codes(r, "SelectedRewardItemCodes")?;
    if !selected.is_empty() {
        validate_selection(s, n(d, "ChapterIndex"), n(d, "DungeonIndex"), &selected)?;
    }
    entry["Selected"] = json!(selected);
    let run_id = uuid::Uuid::new_v4().to_string();
    entry["RunId"] = json!(run_id);
    entry["ServiceRequired"] = json!(s.tables.services.rules["RequireBattleService"]==true);
    entry["DeckSnapshot"] = crate::api::services::records::snapshot(db,a,&party).await?;
    out["RunId"] = json!(run_id);
    sqlx::query("INSERT INTO battle_runs(account,run_id,started,completed,entry,begin_response) VALUES(?,?,?,0,?,?) ON CONFLICT(account) DO UPDATE SET run_id=excluded.run_id,started=excluded.started,completed=0,entry=excluded.entry,begin_response=excluded.begin_response").bind(a).bind(&run_id).bind(now()).bind(entry.to_string()).bind(out.to_string()).execute(&mut *db).await?;
    super::super::progression::record(
        db,
        a,
        "EnterDungeon",
        int(r, "ChapterIndex")?,
        int(r, "DungeonIndex")?,
        1,
    )
    .await?;
    Ok(out)
}
pub(super) fn stamina_cost(d: &Value, diff: i64) -> i64 {
    let cost = field(d, "ReqStamina", diff);
    if cost > 0 {
        cost
    } else {
        n(d, "ReqStamina")
    }
}
pub(super) async fn visit(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    let d = dungeon(s, r)?;
    let c = n(d, "ChapterIndex");
    let di = n(d, "DungeonIndex");
    let mut p = progress(db, s, a, c, di).await?;
    if action == "complete_scenario_dungeon" {
        if n(&p, "FirstRewardedDiff") == 0 {
            return Err(rule("NotCompletedDungeon"));
        }
        p["ScenarioComplete"] = json!(1);
    } else {
        p["VisitedTime"] = json!(time(now()));
        sqlx::query("INSERT OR IGNORE INTO campaign_progress(account_id,chapter_id,dungeon_id,is_unlocked) VALUES(?,?,?,1)").bind(a).bind(c).bind(di).execute(&mut *db).await?;
    }
    put(db, a, "dungeon", key(c, di), &p).await?;
    Ok(json!({"DungeonInfo":p}))
}
pub(super) async fn end(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request)->Result<Value>{
    end_inner(db,s,a,r,false).await
}
pub(crate) async fn end_authoritative(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request)->Result<Value>{
    end_inner(db,s,a,r,true).await
}
async fn end_inner(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    trusted: bool,
) -> Result<Value> {
    let saved = sqlx::query("SELECT * FROM battle_runs WHERE account=?")
        .bind(a)
        .fetch_optional(&mut *db)
        .await?
        .ok_or_else(|| rule("UserCampaignInfoNotFound"))?;
    if now() - saved.get::<i64, _>("started") > settings(s, "BattleExpirySeconds", 14400) {
        return Err(rule("UserCampaignInfoNotFound"));
    }
    let entry: Value = read_json(&saved.get::<String, _>("entry"))?;
    for (key, code) in [
        ("ChapterIndex", "WrongChapterIndex"),
        ("DungeonIndex", "WrongDungeonIndex"),
    ] {
        if int(r, key)? != n(&entry, key) {
            return Err(rule(code));
        }
    }
    if difficulty(r)? != n(&entry, "DungeonDifficulty")
        || boolean(r, "ScenarioDungeon", false)?
            != entry["ScenarioDungeon"].as_bool().unwrap_or(false)
    {
        return Err(rule("DifficultyMismatch"));
    }
    if saved.get::<i64,_>("completed") != 0 {
        if entry["ServiceOwned"]==true {
            let result:Option<String>=sqlx::query_scalar("SELECT response FROM service_results WHERE run=? AND account=?").bind(saved.get::<String,_>("run_id")).bind(a).fetch_optional(&mut *db).await?;
            if let Some(result)=result{return read_json(&result);}
        }
        return Err(rule("AlreadyCompleted"));
    }
    if !trusted && (entry["ServiceOwned"]==true || entry["ServiceRequired"]==true){return Err(rule("NotCompletedBattle"));}
    let completed = boolean(r, "Completed", false)?;
    let star = r.number("Star", if completed { 3 } else { 0 })?;
    if !(0..=3).contains(&star) || completed && star == 0 {
        return Err(rule("InvalidStar"));
    }
    let party: Vec<i64> = read_value(entry["Heroes"].clone())?;
    let alive = ids(r, "AliveHeroIndices", 32)?;
    if alive.iter().any(|v| !party.contains(v)) {
        return Err(rule("HeroNotFound"));
    }
    if completed && r.0.contains_key("AliveHeroIndices") && alive.is_empty() {
        return Err(rule("AllHeroesDied"));
    }
    let elapsed = now() - saved.get::<i64, _>("started");
    if r.number("PureBattleTime", 0)? < 0 || r.number("TotalDamage", 0)? < 0 {
        return Err(rule("ModulatedData"));
    }
    let request = Request(read_value(entry["Request"].clone())?);
    if n(&entry,"GuildId")>0 {
        let mut out=item::success();
        super::super::community::raid_finish(db,s,a,r,&entry,&mut out).await?;
        sqlx::query("UPDATE battle_runs SET completed=1 WHERE account=? AND completed=0").bind(a).execute(db).await?;
        return Ok(out);
    }
    let mut out = if completed {
        complete(db, s, a, &request, &party, star, 1).await?
    } else {
        item::success()
    };
    dungeons::finish(db, s, a, &request, r, &entry, completed, &mut out).await?;
    seasons::finish(db, s, a, &request, r, elapsed, &mut out).await?;
    rooms::finish(db, s, a, &request, completed, &mut out).await?;
    if completed {crate::api::services::records::record_clear(db,a,&entry,r.number("PureBattleTime",elapsed)?).await?;}
    sqlx::query("UPDATE battle_runs SET completed=1 WHERE account=? AND completed=0")
        .bind(a)
        .execute(db)
        .await?;
    Ok(out)
}
pub(super) async fn complete(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    party: &[i64],
    star: i64,
    count: i64,
) -> Result<Value> {
    let d = dungeon(s, r)?;
    let c = n(d, "ChapterIndex");
    let di = n(d, "DungeonIndex");
    let diff = difficulty(r)?;
    let mut p = progress(db, s, a, c, di).await?;
    let mut reward = Rewards::default();
    for _ in 0..count {
        reward_index(db, s, a, field(d, "DropRewardIndex", diff), &mut reward).await?;
    }
    let first = n(&p, "FirstRewardedDiff") & (1 << diff) == 0;
    if first {
        reward_index(
            db,
            s,
            a,
            field(d, "FirstClearRewardIndex", diff),
            &mut reward,
        )
        .await?;
        if n(&p, "FirstRewardedDiff") == 0 {
            reward_index(db, s, a, n(d, "FirstRewardIndex"), &mut reward).await?;
        }
        tutorial::currency(db, a, "Gem", field(d, "FirstClearGem", diff), &mut reward).await?;
    }
    let (gold_boost, exp_boost) = boost(db, s, a, n(d, "BattleType")).await?;
    let base_gold = reward
        .currencies
        .iter()
        .filter(|v| v["CurrencyType"] == "Gold")
        .map(|v| n(v, "AddValue"))
        .sum::<i64>();
    tutorial::currency(db, a, "Gold", base_gold * gold_boost / 100, &mut reward).await?;
    let selected = selected_codes(r, "SelectedRewardItemCodes")?;
    if let Some(def) = s
        .tables
        .battle
        .find("SelectReward", &[("ChapterIndex", c), ("DungeonIndex", di)])
    {
        if selected.is_empty() {
            let pending = get(db, a, "selected_reward", key(c, di)).await?;
            put(
                db,
                a,
                "selected_reward",
                key(c, di),
                &json!({"Count":n(&pending,"Count")+count}),
            )
            .await?;
        } else {
            grant_selection(db, s, a, def, &selected, count, &mut reward).await?;
        }
    }
    let base_exp = field(d, "CreatureExp", diff) * count;
    let mut hero_exp = vec![];
    let mut flasks = vec![];
    let mut flask_items = vec![];
    for id in party {
        let h = hero::info(db, a, *id as i32).await?;
        let old = n(&h, "Level") as i32;
        let cap = s
            .tables
            .tutorials
            .support
            .hero_stars
            .iter()
            .find(|v| {
                v.star == n(&h, "Star") as i32 && v.transcended == n(&h, "Transcended") as i32
            })
            .map(|v| v.max_hero_level)
            .unwrap_or(old)
            .max(old);
        let exp = base_exp + base_exp * exp_boost / 100;
        let (level, new_exp) = if old >= cap {
            (old, n(&h, "Exp"))
        } else {
            crate::tables::add_exp(
                &s.tables.tutorials.support.hero_levels,
                old,
                n(&h, "Exp"),
                exp,
                cap,
            )
        };
        sqlx::query("UPDATE heroes SET level=?,exp=? WHERE account_id=? AND hero_index=?")
            .bind(level)
            .bind(new_exp)
            .bind(a)
            .bind(id)
            .execute(&mut *db)
            .await?;
        if old >= cap {
            if let Some((f, items)) =
                super::super::extensions::consumables::fill_flask(db, s, a, *id as i32, exp).await?
            {
                flasks.push(f);
                flask_items.extend(items);
            }
        }
        hero_exp.push(json!({"HeroIndex":id,"OldLevel":old,"NewLevel":level,"NewValue":new_exp,"AddValue":base_exp,"AddBoosterValue":exp-base_exp,"AddTeamLevelValue":0}));
    }
    p["MaxStar"] = json!(n(&p, "MaxStar").max(diff * 10 + star));
    p["FirstRewardedDiff"] = json!(n(&p, "FirstRewardedDiff") | (1 << diff));
    p["CompletedTime"] = json!(time(now()));
    p["DailyCompletedCount"] = json!(n(&p, "DailyCompletedCount") + count);
    if boolean(r, "ScenarioDungeon", false)? {
        p["ScenarioComplete"] = json!(1);
    }
    put(db, a, "dungeon", key(c, di), &p).await?;
    let sweep_type = n(d, "SweepDungeonType");
    if sweep_type > 0 {
        let previous = get(db, a, "top_clear", sweep_type).await?;
        if di > n(&previous, "DungeonIndex") {
            put(
                db,
                a,
                "top_clear",
                sweep_type,
                &json!({"SweepDungeonType":sweep_type,"DungeonIndex":di}),
            )
            .await?;
        }
    }
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star,is_unlocked,completed_time) VALUES(?,?,?,?,?,1,?) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=clear_count+excluded.clear_count,best_star=MAX(best_star,excluded.best_star),completed_time=excluded.completed_time").bind(a).bind(c).bind(di).bind(count).bind(star).bind(time(now())).execute(&mut *db).await?;

    let mut out = rewards(db, s, a, reward).await?;
    out["CampaignResults"] = json!([p]);
    out["HeroExpResults"] = json!(hero_exp);
    out["FlaskResults"] = json!(flasks);
    out["FlaskItemResults"] = json!(flask_items);
    out["SelectedRewardItemCodes"] = json!(selected);
    Ok(out)
}
async fn boost(db: &mut SqliteConnection, s: &AppState, a: i64, battle: i64) -> Result<(i64, i64)> {
    let active: Vec<i32> = sqlx::query_scalar(
        "SELECT item_index FROM item_boosters WHERE account_id=? AND end_time>datetime('now')",
    )
    .bind(a)
    .fetch_all(&mut *db)
    .await?;
    let (mut gold, mut exp) = (0, 0);
    for id in active {
        if let Some(b) = s.tables.inventory.boosters.get(&id) {
            if b["BattleTypes"]
                .as_array()
                .is_some_and(|v| v.contains(&json!(battle)))
            {
                match n(b, "Type") {
                    1 => exp = exp.max(n(b, "Value")),
                    3 => gold = gold.max(n(b, "Value")),
                    _ => {}
                }
            }
        }
    }
    let costumes: Vec<i32> =
        sqlx::query_scalar("SELECT costume_index FROM costumes WHERE account_id=?")
            .bind(a)
            .fetch_all(db)
            .await?;
    for id in costumes {
        if let Some(c) = s.tables.hero_shop.costumes.get(&id) {
            for i in 1..=3 {
                if n(c, &format!("AbilityType{i}")) == 1 {
                    let v = &c[format!("AbilityValue{i}")];
                    let amount = v[1]
                        .as_str()
                        .and_then(|s| s.parse::<i64>().ok())
                        .unwrap_or(0);
                    match v[0].as_str().unwrap_or("") {
                        "BonusGold" => gold += amount,
                        "BonusExp" => exp += amount,
                        _ => {}
                    }
                }
            }
        }
    }
    Ok((gold, exp))
}
fn selected_codes(r: &Request, key: &str) -> Result<Vec<String>> {
    if matches!(r.text(key), "" | "null") {
        return Ok(vec![]);
    }
    let codes: Vec<String> = read_json(r.text(key)).map_err(|_| rule("InvalidRequest"))?;
    if codes.len() > 28 || codes.iter().collect::<BTreeSet<_>>().len() != codes.len() {
        return Err(rule("InvalidRequest"));
    }
    Ok(codes)
}
fn validate_selection(s: &AppState, c: i64, d: i64, codes: &[String]) -> Result<()> {
    let def = row(
        s,
        "SelectReward",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    )?;
    if codes.len() as i64 != n(def, "SelectCount")
        || codes
            .iter()
            .any(|code| !(1..=28).any(|i| def[format!("Item{i}Code")] == *code))
    {
        return Err(rule("InvalidRequest"));
    }
    Ok(())
}
async fn grant_selection(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    def: &Value,
    codes: &[String],
    count: i64,
    r: &mut Rewards,
) -> Result<()> {
    validate_selection(s, n(def, "ChapterIndex"), n(def, "DungeonIndex"), codes)?;
    for code in codes {
        let i = (1..=28)
            .find(|i| def[format!("Item{i}Code")] == *code)
            .unwrap();
        let id = s
            .tables
            .get_item_index(code)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        let amount = n(def, &format!("Item{i}Count")) * count;
        item::give(
            db,
            s,
            a,
            id,
            i32::try_from(amount).map_err(|_| rule("InvalidItemCount"))?,
            n(def, &format!("Item{i}Star")) as i32,
            0,
            r,
        )
        .await?;
    }
    Ok(())
}
pub(super) async fn select_reward(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let c = int(r, "ChapterIndex")?;
    let d = int(r, "DungeonIndex")?;
    let pending = get(db, a, "selected_reward", key(c, d)).await?;
    let count = n(&pending, "Count");
    if count <= 0 {
        return Err(rule("AlreadyCompleted"));
    }
    let def = row(
        s,
        "SelectReward",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    )?;
    let codes = selected_codes(r, "ItemCodes")?;
    let mut reward = Rewards::default();
    grant_selection(db, s, a, def, &codes, count, &mut reward).await?;
    put(db, a, "selected_reward", key(c, d), &json!({"Count":0})).await?;
    rewards(db, s, a, reward).await
}
