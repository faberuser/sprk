use super::*;
use chrono::{Datelike, Duration, Utc};
const KEYS: &[&str] = &[
    "None",
    "Chicken",
    "Sword",
    "Gold",
    "Gem",
    "UndergroundPrisonKey",
    "UndergroundLabyrinthKey",
    "Mileage",
    "HideoutKey",
    "ChallengeTowerKey",
    "GuildRaidTicket",
    "WorldBossTicket",
    "EventDungeonTicket",
    "Sword2",
    "MazeKey",
    "GuildSuppressKey",
    "TreasureHouseKey",
    "EventWorldBossTicket",
    "WorldBossGlobalTicket",
    "ConquestKey",
    "GuildArenaKey",
    "GodkingTrialKey",
    "EclipseKey",
    "EventHeroDungeonTicket",
    "EventDungeonGoldKey",
    "PrisonKey",
    "ShakmehMiddleBossKey",
    "PetFeedKey",
    "Sword3",
    "TechnoEnchantKey",
    "PunishmentRaidKey",
];
pub(crate) async fn charge(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: i64,
    cost: i64,
) -> Result<Value> {
    if cost < 0 {
        return Err(rule("InvalidCost"));
    }
    if kind <= 0 {
        if cost != 0 {
            return Err(rule("InvalidCost"));
        }
        return Ok(Value::Null);
    }
    let name = *KEYS
        .get(kind as usize)
        .ok_or_else(|| rule("NotEnoughStamina"))?;
    let value = if kind == 1 {
        sqlx::query_scalar::<_,i64>("UPDATE user_info SET stamina=stamina-? WHERE account_id=? AND stamina>=? RETURNING stamina").bind(cost).bind(a).bind(cost).fetch_optional(&mut *db).await?.ok_or_else(||rule("NotEnoughStamina"))?
    } else if matches!(kind, 3 | 4 | 7) {
        let kind = if kind == 3 {
            "Gold"
        } else if kind == 4 {
            "Gem"
        } else {
            "Mileage"
        };
        let r = hero::currency(db, a, kind, -cost).await?;
        n(&r, "NewValue")
    } else {
        let mut info = get(db, a, "key", kind).await?;
        let default = s.tables.battle.rules["DungeonKeyDailyDefaults"][name]
            .as_i64()
            .unwrap_or(0);
        if info.is_null() || info["Day"] != day() {
            if kind == 11 {
                // Keep purchased/recovered tickets in the existing currency column.
                sqlx::query("UPDATE user_info SET world_boss_ticket=MAX(world_boss_ticket,?) WHERE account_id=?").bind(default).bind(a).execute(&mut *db).await?;
            }
            info = json!({"Day":day(),"Count":default,"RechargeCount":0});
        }
        if kind == 11 {
            info["Count"] = json!(
                sqlx::query_scalar::<_, i64>(
                    "SELECT world_boss_ticket FROM user_info WHERE account_id=?"
                )
                .bind(a)
                .fetch_one(&mut *db)
                .await?
            );
        }
        let count = n(&info, "Count");
        if cost > count {
            return Err(rule("NotEnoughDungeonKey"));
        }
        info["Count"] = json!(count - cost);
        if kind == 11 {
            sqlx::query("UPDATE user_info SET world_boss_ticket=? WHERE account_id=?")
                .bind(count - cost)
                .bind(a)
                .execute(&mut *db)
                .await?;
        }
        put(db, a, "key", kind, &info).await?;
        count - cost
    };
    Ok(
        json!({"Type":name,"AddValue":-cost,"NewValue":value,"StaminaRechargeTime":time(now()),"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}),
    )
}
pub(super) async fn key_snapshot(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let mut out = vec![];
    for k in 5..KEYS.len() {
        if s.tables.battle.rules["DungeonKeyDailyDefaults"]
            .get(KEYS[k])
            .is_some()
        {
            out.push(charge(db, s, a, k as i64, 0).await?);
        }
    }
    Ok(json!(out))
}
fn period(r: &Value) -> String {
    let date = Utc::now().date_naive();
    if r["DailyReset"] == true {
        return date.to_string();
    }
    if r["WeeklyReset"] == true {
        return (date - Duration::days(date.weekday().num_days_from_monday() as i64)).to_string();
    }
    if let Some(days) = r["MonthlyResetDate"].as_array() {
        for n in 0..=62 {
            let d = date - Duration::days(n);
            if days.iter().any(|v| v.as_u64() == Some(d.day() as u64)) {
                return d.to_string();
            }
        }
    }
    "all".into()
}
async fn tower(db: &mut SqliteConnection, s: &AppState, a: i64, id: i64) -> Result<Value> {
    let data = row(s, "Tower", &[("TowerIndex", id)])?;
    let p = period(data);
    let mut info = get(db, a, "tower", id).await?;
    if info.is_null() || info["Period"] != p {
        info = json!({"TowerIndex":id,"CompletedFloor":0,"CompletedTime":null,"ResetCount":0,"ResetTime":null,"ReceiveRewardTime":null,"Period":p});
        put(db, a, "tower", id, &info).await?;
        put(db, a, "tower_heroes", id, &json!([])).await?;
        put(db, a, "tower_creature_stage", id, &Value::Null).await?;
    }
    Ok(info)
}
pub(super) async fn tower_list(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
) -> Result<Vec<Value>> {
    let mut out = vec![];
    for r in s.tables.battle.rows("Tower") {
        out.push(tower(db, s, a, n(r, "TowerIndex")).await?);
    }
    Ok(out)
}
fn floor<'a>(s: &'a AppState, r: &Request) -> Option<&'a Value> {
    let c = r.number("ChapterIndex", 0).ok()?;
    let d = r.number("DungeonIndex", 0).ok()?;
    s.tables.battle.rows("TowerFloor").iter().find(|v| {
        n(v, "ChapterIndex") == c
            && (n(v, "DungeonIndex") == d
                || v["DungeonIndices"]
                    .as_array()
                    .is_some_and(|x| x.contains(&json!(d))))
    })
}
pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    d: &Value,
) -> Result<()> {
    let battle_type = n(d, "BattleType");
    if matches!(
        battle_type,
        4 | 9 | 12 | 20 | 26 | 29 | 30 | 32 | 36 | 43 | 44 | 48
    ) {
        return Err(rule("ContentsDisabled"));
    }
    if battle_type == 39 {
        crate::api::live::validate_purchase_dungeon(db,s,a,n(d,"ChapterIndex"),n(d,"DungeonIndex")).await?;
    }
    if battle_type == 16 { super::super::community::raid_validate(db,s,a,r).await?; }
    if battle_type == 15 && int(r, "WorldBossIndex")? == 0 {
        return Err(rule("MissingWorldBossIndex"));
    }
    if battle_type == 19 && int(r, "EventWorldBossIndex")? == 0 {
        return Err(rule("MissingEventWorldBossIndex"));
    }
    if matches!(battle_type, 21 | 25 | 33 | 40 | 41 | 42 | 45 | 46 | 47)
        && int(r, "RaidIndex")? == 0
    {
        return Err(rule("MissingRaidIndex"));
    }
    if matches!(battle_type, 17 | 31) {
        let kind = if battle_type == 17 {
            "hideout"
        } else {
            "conquest"
        };
        let p = daily_attempts(db, a, kind, n(d, "ChapterIndex")).await?;
        let max = s.tables.hero_shop.constant(
            if battle_type == 17 {
                "MaxHideoutDungeonTryCount"
            } else {
                "MaxConquestDungeonTryCount"
            },
            3,
        );
        if n(&p, "CompletedCount") >= max {
            return Err(rule("MaxDailyCount"));
        }
    }
    if let Some(f) = floor(s, r) {
        let id = n(f, "TowerIndex");
        if int(r, "TowerIndex")? != id || int(r, "TowerFloor")? != n(f, "Floor") {
            return Err(rule("MissingTowerIndex"));
        }
        let t = tower(db, s, a, id).await?;
        let data = row(s, "Tower", &[("TowerIndex", id)])?;
        restrictions::party(s, r, n(f, "ReqClass"))?;
        if carries_creatures(s, id)? && !r.0.contains_key("SweepCount") {
            let previous = get(db, a, "tower_heroes", id).await?;
            let mut party = ids(r, "HeroIndices", 32)?;
            party.extend(ids(r, "GroupHeroIndices", 32)?);
            if previous
                .as_array()
                .into_iter()
                .flatten()
                .any(|v| n(v, "TeamId") == 0 && n(v, "Hp") == 0 && party.contains(&n(v, "Index")))
            {
                return Err(rule("NotAvailableHero"));
            }
        }
        if let Some(open) = f["OpenTime"].as_str().filter(|v| !v.is_empty()) {
            let open = chrono::NaiveDateTime::parse_from_str(open, "%Y-%m-%d %H:%M:%S")
                .map_err(|_| rule("CannotEnterTowerFloor"))?
                .and_utc()
                .timestamp();
            if now() < open {
                return Err(rule("CannotEnterTowerFloor"));
            }
        }
        if n(f, "Floor") > n(&t, "CompletedFloor") + 1
            || data["RepeatableFloor"] != true && n(f, "Floor") <= n(&t, "CompletedFloor")
        {
            return Err(rule("CannotEnterTowerFloor"));
        }
        let level: i64 = sqlx::query_scalar("SELECT team_level FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&mut *db)
            .await?;
        if level < n(f, "ReqTeamLevel") {
            return Err(rule("TowerTeamLevelLimit"));
        }
        if n(f, "ReqChapterIndex") > 0
            && n(
                &campaign::progress(db, s, a, n(f, "ReqChapterIndex"), n(f, "ReqDungeonIndex"))
                    .await?,
                "FirstRewardedDiff",
            ) == 0
        {
            return Err(rule("TowerReqDungeonNotCompleted"));
        }
    } else if int(r, "TowerIndex")? > 0 {
        return Err(rule("TowerDungeonMismatch"));
    }
    let c = n(d, "ChapterIndex");
    if battle_type == 38 {
        let def = row(
            s,
            "ShakmehDungeon",
            &[("DungeonIndex", n(d, "DungeonIndex"))],
        )?;
        restrictions::party(s, r, n(def, "BanRuleIndex"))?;
        let gauge = shakmeh_gauge(db, s, a).await?;
        if (n(def, "ShakmehIndex") == 1 && gauge.0 >= gauge.1)
            || (n(def, "ShakmehIndex") == 2 && gauge.0 < gauge.1)
        {
            return Err(rule("NotEnoughCurrency"));
        }
        if let Some(condition) = def["OpenCondition"].as_array() {
            if condition.len() == 2 {
                let previous = campaign::progress(
                    db,
                    s,
                    a,
                    condition[0].as_i64().unwrap_or(0),
                    condition[1].as_i64().unwrap_or(0),
                )
                .await?;
                if n(&previous, "FirstRewardedDiff") == 0 {
                    return Err(rule("NotCompletedReqDungeon"));
                }
            }
        }
    }
    if s.tables
        .battle
        .find("GodkingTrialGroup", &[("ChapterIndex", c)])
        .is_some()
    {
        let g = get(db, a, "godking", c).await?;
        if n(&g, "IsOpen") != 1 || g["Day"] != day() {
            return Err(rule("GodkingTrialDungeonNotOpened"));
        }
        let cleared: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_reward_claims WHERE account=? AND kind='godking_clear' AND idx=? AND period=?)")
            .bind(a).bind(campaign::key(c,n(d,"DungeonIndex"))).bind(day()).fetch_one(&mut *db).await?;
        if cleared {
            return Err(rule("AlreadyCompleted"));
        }
    }
    if s.tables
        .battle
        .rows("UnderPrisonDungeon")
        .iter()
        .any(|v| n(v, "ChapterIndex") == c)
    {
        let p = under(db, s, a, c).await?;
        if n(&p, "CompletedCount") >= n(&p, "MaxTryCount") {
            return Err(rule("ExceededDailyEntryCount"));
        }
    }
    if s.tables
        .battle
        .rows("TreasureHouseDungeon")
        .iter()
        .any(|v| n(v, "ChapterIndex") == c)
    {
        let info = treasure(db, s, a).await?;
        let target = row(s, "TreasureHouseDungeon", &[("Index", n(&info, "Index"))])?;
        if n(target, "DungeonIndex") != n(d, "DungeonIndex") || n(&info, "ClearCount") >= 1 {
            return Err(rule("TreasurehouseInitializeEnterPopup"));
        }
    }
    if let Some(dow) = s.tables.battle.find("DOWDungeon", &[("DungeonIndex", c)]) {
        let today = Utc::now().weekday().number_from_monday();
        if !dow["OpenDOWs"]
            .as_array()
            .is_some_and(|v| v.contains(&json!(today)))
        {
            return Err(rule("NotOpenedDungeon"));
        }
    }
    if int(r, "RaidIndex")? > 0 {
        let raid = raid_data(s, r)?;
        restrictions::party(s, r, n(raid, "BanIndex"))?;
        let mut party = ids(r, "HeroIndices", 32)?;
        party.extend(ids(r, "GroupHeroIndices", 32)?);
        for id in party {
            if n(&hero::info(db, a, id as i32).await?, "Level") < n(raid, "ReqHeroLevel") {
                return Err(rule("NotAvailableHero"));
            }
        }
        if raid["IsOpen"] != true
            || n(raid, "ChapterIndex") != c
            || n(raid, "DungeonIndex") != n(d, "DungeonIndex")
        {
            return Err(rule("RaidDungeonMismatch"));
        }
        if battle_type == 47 {
            let def = row(
                s,
                "PunishmentRaid",
                &[
                    ("RaidIndex", int(r, "RaidIndex")?),
                    ("RaidLevel", int(r, "RaidLevel")?),
                ],
            )?;
            let opened = get(db, a, "punishment_open", 0).await?;
            if n(&opened, "IsOpen") != 1
                || opened["Day"] != day()
                || n(&opened, "GroupIndex") != n(def, "GroupIndex")
                || n(&opened, "OpenLevel") != int(r, "RaidLevel")?
            {
                return Err(rule("RaidNotStarted"));
            }
        }
    }
    seasons::validate(db, s, a, r).await
}
async fn tower_cost(db: &mut SqliteConnection, a: i64, kind: i64, amount: i64) -> Result<Value> {
    if amount == 0 {
        return Ok(Value::Null);
    }
    hero::currency(
        db,
        a,
        match kind {
            2 => "Gold",
            3 => "Gem",
            6 => "Mileage",
            _ => return Err(rule("NotEnoughTowerCost")),
        },
        -amount,
    )
    .await
}
pub(super) async fn enter(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
    out: &mut Value,
) -> Result<()> {
    if let Some(f) = floor(s, r) {
        let cost = tower_cost(
            db,
            a,
            n(f, "BattleCostType"),
            n(f, "BattleStartCost") + n(f, "BattleEndCost"),
        )
        .await?;
        if !cost.is_null() {
            out["CurrencyResults2"] = json!([cost]);
        }
        entry["TowerPeriod"] = json!(period(row(
            s,
            "Tower",
            &[("TowerIndex", n(f, "TowerIndex"))]
        )?));
        if carries_creatures(s, n(f, "TowerIndex"))? {
            let previous = get(db, a, "tower_heroes", n(f, "TowerIndex")).await?;
            let stage = get(db, a, "tower_creature_stage", n(f, "TowerIndex")).await?;
            let mut party = ids(r, "HeroIndices", 32)?;
            party.extend(ids(r, "GroupHeroIndices", 32)?);
            out["TowerIntialUserHeroes"] = json!(previous
                .as_array()
                .into_iter()
                .flatten()
                .filter(|v| n(v, "TeamId") == 0 && party.contains(&n(v, "Index")))
                .cloned()
                .collect::<Vec<_>>());
            if n(&stage, "Floor") == n(f, "Floor") {
                out["TowerIntialEnemyHeroes"] = json!(previous
                    .as_array()
                    .into_iter()
                    .flatten()
                    .filter(|v| n(v, "TeamId") == 1)
                    .cloned()
                    .collect::<Vec<_>>());
            }
        }
    }
    let dungeon = campaign::dungeon(s, r)?;
    if n(dungeon,"BattleType")==16 {super::super::community::raid_enter(db,s,a,r,entry,out).await?;}
    if n(dungeon, "BattleType") == 38 {
        let def = row(
            s,
            "ShakmehDungeon",
            &[("DungeonIndex", int(r, "DungeonIndex")?)],
        )?;
        if n(def, "ShakmehIndex") == 2 {
            let (_, max) = shakmeh_gauge(db, s, a).await?;
            out["CurrencyResults2"] =
                json!([hero::currency(db, a, "ShakmehMiddleBossPoint", -max).await?]);
            entry["ShakmehFinal"] = json!(true);
        }
    }
    if int(r, "WorldBossIndex")? > 0 || int(r, "EventWorldBossIndex")? > 0 {
        seasons::enter(db, s, a, r, entry, out).await?;
    }
    Ok(())
}
pub(super) async fn finish(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    end: &Request,
    entry: &Value,
    won: bool,
    out: &mut Value,
) -> Result<()> {
    if entry["ShakmehFinal"] == true {
        put(db, a, "shakmeh_passive", 0, &json!([])).await?;
    }
    if let Some(f) = floor(s, r) {
        let id = n(f, "TowerIndex");
        if carries_creatures(s, id)? && !end.text("CreatureInfoString").is_empty() {
            let mut party = ids(r, "HeroIndices", 32)?;
            party.extend(ids(r, "GroupHeroIndices", 32)?);
            let reported = restrictions::creatures(end, &party)?;
            let current = tower(db, s, a, id).await?;
            if current["Period"] != entry["TowerPeriod"] {
                return Err(rule("CannotEnterTowerFloor"));
            }
            let mut stored = get(db, a, "tower_heroes", id)
                .await?
                .as_array()
                .cloned()
                .unwrap_or_default();
            stored.retain(|v| n(v, "TeamId") == 0 && !party.contains(&n(v, "Index")));
            stored.extend(reported.into_iter().filter(|v| !won || n(v, "TeamId") == 0));
            put(db, a, "tower_heroes", id, &json!(stored)).await?;
            put(
                db,
                a,
                "tower_creature_stage",
                id,
                &json!({"Floor":n(f,"Floor")}),
            )
            .await?;
        }
    }
    if !won {
        return Ok(());
    }
    let c = int(r, "ChapterIndex")?;
    let d = int(r, "DungeonIndex")?;
    let mut extra = Rewards::default();
    let dungeon = campaign::dungeon(s, r)?;
    let bt = n(dungeon, "BattleType");
    if matches!(bt, 17 | 31) {
        let kind = if bt == 17 { "hideout" } else { "conquest" };
        let mut p = daily_attempts(db, a, kind, c).await?;
        p["CompletedCount"] = json!(n(&p, "CompletedCount") + 1);
        p["LastCompletedTime"] = json!(time(now()));
        put(db, a, kind, c, &p).await?;
        out[if bt == 17 {
            "HideoutDungeonResult"
        } else {
            "ConquestDungeonResult"
        }] = p;
    }
    if let Some(f) = floor(s, r) {
        let mut t = tower(db, s, a, n(f, "TowerIndex")).await?;
        if t["Period"] != entry["TowerPeriod"] {
            return Err(rule("CannotEnterTowerFloor"));
        }
        let first = n(&t, "CompletedFloor") < n(f, "Floor");
        t["CompletedFloor"] = json!(n(&t, "CompletedFloor").max(n(f, "Floor")));
        t["CompletedTime"] = json!(time(now()));
        put(db, a, "tower", n(f, "TowerIndex"), &t).await?;
        for reward in f[if first {
            "RewardIndex"
        } else {
            "RepeatRewardIndex"
        }]
        .as_array()
        .into_iter()
        .flatten()
        {
            reward_index(db, s, a, reward.as_i64().unwrap_or(0), &mut extra).await?;
        }
        out["TowerInfos"] = json!([t]);
    }
    if s.tables
        .battle
        .rows("UnderPrisonDungeon")
        .iter()
        .any(|v| n(v, "ChapterIndex") == c)
    {
        let mut p = under(db, s, a, c).await?;
        p["CompletedCount"] = json!(n(&p, "CompletedCount") + 1);
        p["Diff"] = json!(n(&p, "Diff").max(campaign::difficulty(r)?));
        p["LastCompletedTime"] = json!(time(now()));
        put(db, a, "under_prison", c, &p).await?;
        out["UnderPrisonInfos"] = json!([p]);
    }
    if let Some(g) = s
        .tables
        .battle
        .find("GodkingTrial", &[("ChapterIndex", c), ("DungeonIndex", d)])
    {
        claim(db, a, "godking_clear", campaign::key(c, d), &day()).await?;
        reward_index(db, s, a, n(g, "RewardIndex"), &mut extra).await?;
        out["GodkingTrialDungeonInfo"] = get(db, a, "godking", c).await?;
    }
    if let Some(t) = s.tables.battle.find(
        "TreasureHouseDungeon",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    ) {
        let mut info = treasure(db, s, a).await?;
        info["ClearCount"] = json!(1);
        info["ClearedTime"] = json!(time(now()));
        put(db, a, "treasure", 0, &info).await?;
        reward_index(db, s, a, n(t, "RewardIndex"), &mut extra).await?;
    }
    if int(r, "RaidIndex")? > 0 {
        let data = raid_data(s, r)?;
        let info = json!({"RaidIndex":n(data,"Index"),"RaidLevel":n(data,"Level"),"BattleScore":end.number("TotalDamage",0)?,"CreatedTime":time(now()),"CompletedTime":time(now())});
        put(db, a, "raid", n(data, "Index"), &info).await?;
        reward_index(db, s, a, n(data, "IndividualReward"), &mut extra).await?;
        out["CompletedRaidInfo"] = info;
    }
    if bt == 38 {
        let def = row(s, "ShakmehDungeon", &[("DungeonIndex", d)])?;
        let p = get(db, a, "shakmeh_clear", campaign::key(c, d)).await?;
        if p.is_null() {
            reward_index(db, s, a, n(def, "FirstClearRewardIndex"), &mut extra).await?;
        }
        reward_index(db, s, a, n(def, "RewardIndex"), &mut extra).await?;
        shakmeh_passives(db, s, a).await?;
        put(db,a,"shakmeh_clear",campaign::key(c,d),&json!({"Level":n(def,"Level"),"ShakmehIndex":n(def,"ShakmehIndex"),"ClearedTime":time(now())})).await?;
    }
    if bt == 47 {
        let id = int(r, "RaidIndex")?;
        let level = int(r, "RaidLevel")?;
        let mut p = get(db, a, "punishment_open", 0).await?;
        p["ClearLevel"] = json!(n(&p, "ClearLevel").max(level));
        p["ClearCount"] = json!(n(&p, "ClearCount") + 1);
        put(db, a, "punishment_open", 0, &p).await?;
        put(db,a,"punishment_raid",campaign::key(id,level),&json!({"RaidIndex":id,"RaidLevel":level,"ClearCheckIndex":id,"ClearedTime":time(now())})).await?;
    }
    let v = rewards(db, s, a, extra).await?;
    append_rewards(out, &v);
    Ok(())
}
fn carries_creatures(s: &AppState, id: i64) -> Result<bool> {
    let def = row(s, "Tower", &[("TowerIndex", id)])?;
    Ok(s.tables.battle.rules["TowerCarryCreatureTypes"]
        .as_array()
        .is_some_and(|v| v.contains(&def["Type"])))
}
async fn shakmeh_gauge(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<(i64, i64)> {
    let max = n(row(s, "CurrencyType", &[("CurrencyType", 48)])?, "MaxValue");
    if max <= 0 {
        return Err(rule("ItemDataNotFound"));
    }
    let value=sqlx::query_scalar::<_,i64>("SELECT COALESCE((SELECT value FROM battle_currencies WHERE account=? AND kind='ShakmehMiddleBossPoint'),0)").bind(a).fetch_one(db).await?;
    Ok((value, max))
}
pub(super) async fn shakmeh_passives(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
) -> Result<Value> {
    let mut passives = get(db, a, "shakmeh_passive", 0).await?;
    let (gauge, max) = shakmeh_gauge(db, s, a).await?;
    if gauge >= max && passives.as_array().is_none_or(|v| v.is_empty()) {
        let def = row(s, "ShakmehBoss", &[("BossType", 2)])?;
        let mut choices = def["SequenceSkillProjectileGroup"]
            .as_array()
            .cloned()
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        choices.sort_by_key(|v| v.as_i64());
        choices.dedup();
        use rand::seq::SliceRandom;
        choices.shuffle(&mut rand::thread_rng());
        choices.truncate(
            s.tables
                .hero_shop
                .constant("ShakmehBossPassiveSkillNumber", 1)
                .max(0) as usize,
        );
        passives = json!(choices);
        put(db, a, "shakmeh_passive", 0, &passives).await?;
    }
    if !passives.is_array() {
        passives = json!([]);
    }
    Ok(passives)
}
pub(super) fn append_rewards(out: &mut Value, v: &Value) {
    for key in [
        "CurrencyResults",
        "ItemResults",
        "EquipItemInfos",
        "EquipItemResults",
        "HeroInfos",
    ] {
        let mut values = out[key].as_array().cloned().unwrap_or_default();
        values.extend(v[key].as_array().into_iter().flatten().cloned());
        out[key] = json!(values);
    }
}
fn raid_data<'a>(s: &'a AppState, r: &Request) -> Result<&'a Value> {
    row(
        s,
        "Raid",
        &[
            ("Index", int(r, "RaidIndex")?),
            ("Level", int(r, "RaidLevel")?),
        ],
    )
}
async fn daily_attempts(db: &mut SqliteConnection, a: i64, kind: &str, c: i64) -> Result<Value> {
    let mut p = get(db, a, kind, c).await?;
    if p.is_null() || p["Day"] != day() {
        p = json!({"ChapterIndex":c,"Day":day(),"ResetCount":0,"CompletedCount":0,"LastCompletedTime":null});
    }
    Ok(p)
}
async fn under(db: &mut SqliteConnection, s: &AppState, a: i64, c: i64) -> Result<Value> {
    let mut p = get(db, a, "under_prison", c).await?;
    if p.is_null() {
        p = json!({"ChapterIndex":c,"CompletedCount":0,"Diff":0,"MaxTryCount":settings(s,"UnderPrisonMaxDailyAttempts",3),"LastCompletedTime":null,"Day":day()});
    }
    if p["Day"] != day() {
        p["Day"] = json!(day());
        p["CompletedCount"] = json!(0);
    }
    put(db, a, "under_prison", c, &p).await?;
    Ok(p)
}
async fn treasure(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let mut p = get(db, a, "treasure", 0).await?;
    if p.is_null() || p["Day"] != day() {
        let choices = s.tables.battle.rows("TreasureHouseDungeon");
        let total = choices.iter().map(|v| n(v, "Ratio").max(0)).sum::<i64>();
        if total <= 0 {
            return Err(rule("DungeonNotFound"));
        }
        let mut roll = (rand::random::<u64>() % (total as u64)) as i64;
        let t = choices
            .iter()
            .find(|v| {
                roll -= n(v, "Ratio").max(0);
                roll < 0
            })
            .unwrap();
        p = json!({"Index":n(t,"Index"),"ClearCount":0,"ClearedTime":null,"UpdatedTime":time(now()),"CreatedTime":time(now()),"ResetTime":time(now()),"NextResetTime":format!("{} 00:00:00",Utc::now().date_naive()+Duration::days(1)),"Day":day()});
        put(db, a, "treasure", 0, &p).await?;
    }
    Ok(p)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    let action = path.rsplit('/').next().unwrap();
    let mut out = item::success();
    let mut reward = Rewards::default();
    match action {
        "get_tower_hero_info" => {
            tower(db, s, a, int(r, "TowerIndex")?).await?;
            let v = get(db, a, "tower_heroes", int(r, "TowerIndex")?).await?;
            out["TowerHeroInfos"] = if v.is_array() { v } else { json!([]) };
        }
        "get_tower_npc_info" => {
            let id = int(r, "TowerIndex")?;
            let t = tower(db, s, a, id).await?;
            let data = row(s, "Tower", &[("TowerIndex", id)])?;
            out["TowerNpcInfos"] = json!([]);
            if data["UseNpc"] == true {
                let mut cache = get(db, a, "tower_npcs", id).await?;
                if cache["Period"] != t["Period"] || cache["ResetCount"] != t["ResetCount"] {
                    let floors = s
                        .tables
                        .battle
                        .rows("TowerFloor")
                        .iter()
                        .filter(|f| n(f, "TowerIndex") == id)
                        .map(|f| n(f, "Floor"))
                        .max()
                        .unwrap_or(0);
                    let mut opponents = Vec::new();
                    for _ in 0..floors {
                        opponents.push(special::opponent(db, a, 0).await?);
                    }
                    cache = json!({"Period":t["Period"],"ResetCount":t["ResetCount"],"Opponents":opponents});
                    put(db, a, "tower_npcs", id, &cache).await?;
                }
                out["TowerNpcInfos"] = cache["Opponents"].clone();
            }
        }
        "receive_tower_reward" | "reset_tower" => {
            let id = int(r, "TowerIndex")?;
            let data = row(s, "Tower", &[("TowerIndex", id)])?;
            let mut t = tower(db, s, a, id).await?;
            if action == "receive_tower_reward" {
                let max = s
                    .tables
                    .battle
                    .rows("TowerFloor")
                    .iter()
                    .filter(|f| n(f, "TowerIndex") == id)
                    .map(|f| n(f, "Floor"))
                    .max()
                    .unwrap_or(i64::MAX);
                if n(&t, "CompletedFloor") < max || !t["ReceiveRewardTime"].is_null() {
                    return Err(rule("AlreadyCompleted"));
                }
                for v in data["CompleteRewardIndex"].as_array().into_iter().flatten() {
                    reward_index(db, s, a, v.as_i64().unwrap_or(0), &mut reward).await?;
                }
                t["ReceiveRewardTime"] = json!(time(now()));
            } else {
                let active: Option<String> = sqlx::query_scalar(
                    "SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND started>=?",
                )
                .bind(a)
                .bind(now() - settings(s, "BattleExpirySeconds", 14400))
                .fetch_optional(&mut *db)
                .await?;
                if let Some(active) = active {
                    let active: Value = read_json(&active)?;
                    let request = Request(
                        serde_json::from_value(active["Request"].clone())
                            .map_err(|_| rule("Fail"))?,
                    );
                    if floor(s, &request).is_some_and(|f| n(f, "TowerIndex") == id) {
                        return Err(rule("AlreadyOnBattleHero"));
                    }
                }
                let count = n(&t, "ResetCount");
                let creatures = get(db, a, "tower_heroes", id).await?;
                let attempted = creatures.as_array().is_some_and(|v| !v.is_empty());
                if count >= n(data, "MaxResetCount") || n(&t, "CompletedFloor") == 0 && !attempted {
                    return Err(rule("AlreadyCompleted"));
                }
                let costs = data["ResetCost"]
                    .as_array()
                    .ok_or_else(|| rule("InvalidCost"))?;
                let cost = costs
                    .get(count as usize)
                    .or(costs.last())
                    .and_then(Value::as_i64)
                    .ok_or_else(|| rule("InvalidCost"))?;
                out["CurrencyResults2"] =
                    json!([tower_cost(db, a, n(data, "ResetCostType"), cost).await?]);
                t["CompletedFloor"] = json!(0);
                t["CompletedTime"] = Value::Null;
                t["ResetCount"] = json!(count + 1);
                t["ResetTime"] = json!(time(now()));
                t["ReceiveRewardTime"] = Value::Null;
                put(db, a, "tower_heroes", id, &json!([])).await?;
                put(db, a, "tower_creature_stage", id, &Value::Null).await?;
            }
            put(db, a, "tower", id, &t).await?;
            out["TowerInfo"] = t;
        }
        "get_maze_tower_info" => {
            let k = charge(db, s, a, 14, 0).await?;
            out["MazeKey"] = k["NewValue"].clone();
            out["MazeKeyRechargeTime"] = k["StaminaRechargeTime"].clone();
            out["MazeTowerRewardStep"] = get(db, a, "maze_step", 0)
                .await?
                .get("Step")
                .cloned()
                .unwrap_or(json!(0));
        }
        "receive_maze_tower_reward" => {
            let id = int(r, "MazeTowerRewardIndex")?;
            let def = s.tables.battle.rules["MazeTowerRewards"]
                .as_array()
                .into_iter()
                .flatten()
                .find(|v| n(v, "Index") == id)
                .ok_or_else(|| rule("ContentsDisabled"))?;
            for req in def["Towers"].as_array().into_iter().flatten() {
                if n(
                    &tower(db, s, a, n(req, "TowerIndex")).await?,
                    "CompletedFloor",
                ) < n(req, "Floor")
                {
                    return Err(rule("NotCompletedDungeon"));
                }
            }
            claim(db, a, "maze", id, &day()).await?;
            reward_index(db, s, a, n(def, "RewardIndex"), &mut reward).await?;
            put(db, a, "maze_step", 0, &json!({"Step":id})).await?;
        }
        "get_dow_dungeon_info" => {
            let date = Utc::now().date_naive();
            let dow = date.weekday().number_from_monday();
            out["DOWDungeonInfos"]=json!(s.tables.battle.rows("DOWDungeon").iter().filter(|v|v["OpenDOWs"].as_array().is_some_and(|x|x.contains(&json!(dow)))).map(|v|json!({"DungeonIndex":n(v,"DungeonIndex"),"Diff":0,"StartTime":format!("{date} 00:00:00"),"EndTime":format!("{} 00:00:00",date+Duration::days(1))})).collect::<Vec<_>>());
            let k = charge(db, s, a, 5, 0).await?;
            out["UndergroundPrisonKey"] = k["NewValue"].clone();
            out["UndergroundPrisonKeyRechargeTime"] = k["StaminaRechargeTime"].clone();
        }
        "get_under_prison_info" => {
            let mut infos = vec![];
            let chapters = s
                .tables
                .battle
                .rows("UnderPrisonDungeon")
                .iter()
                .map(|v| n(v, "ChapterIndex"))
                .collect::<BTreeSet<_>>();
            for c in chapters {
                infos.push(under(db, s, a, c).await?);
            }
            let k = charge(db, s, a, 25, 0).await?;
            out["UnderPrisonInfos"] = json!(infos);
            out["UnderPrisonKey"] = k["NewValue"].clone();
            out["UnderPrisonKeyRechargeTime"] = k["StaminaRechargeTime"].clone();
        }
        "recharge_underground_prison_key" => {
            let c = int(r, "DungeonIndex")?;
            row(s, "DOWDungeon", &[("DungeonIndex", c)])?;
            let price = settings(s, "PrisonRechargeGem", 100);
            reward
                .currencies
                .push(hero::currency(db, a, "Gem", -price).await?);
            charge(db, s, a, 5, 0).await?;
            let mut k = get(db, a, "key", 5).await?;
            if n(&k, "RechargeCount") >= 3 {
                return Err(rule("AlreadyCompleted"));
            }
            k["Count"] = json!(n(&k, "Count") + 1);
            k["RechargeCount"] = json!(n(&k, "RechargeCount") + 1);
            put(db, a, "key", 5, &k).await?;
            out["UndergroundPrisonKeyResult"] = charge(db, s, a, 5, 0).await?;
        }
        "get_treasure_house_info" | "reset_treasure_house_info" => {
            if action.starts_with("reset") {
                let current = treasure(db, s, a).await?;
                if n(&current, "ClearCount") > 0 {
                    return Err(rule("AlreadyCompleted"));
                }
                let def = s
                    .tables
                    .battle
                    .rows("TreasureHouseInfo")
                    .first()
                    .ok_or_else(|| rule("DungeonNotFound"))?;
                reward.currencies.push(
                    hero::currency(
                        db,
                        a,
                        if n(def, "ChangeCurrencyType") == 2 {
                            "Gem"
                        } else {
                            "Gold"
                        },
                        -n(def, "ChangeCurrencyCost"),
                    )
                    .await?,
                );
                put(db, a, "treasure", 0, &Value::Null).await?;
            }
            out["PlayerTreasureHouseInfo"] = treasure(db, s, a).await?;
        }
        "open_godking_trial_dungeon" => {
            let c = int(r, "ChapterIndex")?;
            row(s, "GodkingTrialGroup", &[("ChapterIndex", c)])?;
            let current = get(db, a, "godking", c).await?;
            if current["Day"] == day() && n(&current, "IsOpen") == 1 {
                return Err(rule("AlreadyCompleted"));
            }
            out["StaminaResult"] = charge(db, s, a, 21, 1).await?;
            for mut old in list(db, a, "godking").await? {
                old["IsOpen"] = json!(0);
                old["IsOpened"] = json!(false);
                put(db, a, "godking", n(&old, "ChapterIndex"), &old).await?;
            }
            let v = json!({"ChapterIndex":c,"IsOpen":1,"IsOpened":true,"OpenedTime":time(now()),"NextResetRemainTime":86400-now().rem_euclid(86400),"Day":day()});
            put(db, a, "godking", c, &v).await?;
            out["GodkingTrialDungeonInfo"] = v;
        }
        "reset_hideout_dungeon" | "reset_conquest_dungeon" => {
            let c = int(r, "ChapterIndex")?;
            let (kind, bt, prefix) = if action.contains("hideout") {
                ("hideout", 17, "Hideout")
            } else {
                ("conquest", 31, "Conquest")
            };
            if !s
                .tables
                .battle
                .rows("CampaignDungeon")
                .iter()
                .any(|v| n(v, "ChapterIndex") == c && n(v, "BattleType") == bt)
            {
                return Err(rule("DungeonNotFound"));
            }
            let mut p = daily_attempts(db, a, kind, c).await?;
            if n(&p, "CompletedCount") == 0 {
                return Err(rule("AlreadyCompleted"));
            }
            let count = n(&p, "ResetCount") as usize;
            let costs: Vec<i64> = s
                .tables
                .hero_shop
                .constants
                .get(&format!("Reset{prefix}DungeonGem"))
                .and_then(|v| serde_json::from_str(v).ok())
                .ok_or_else(|| rule("InvalidCost"))?;
            let cost = *costs.get(count).ok_or_else(|| rule("AlreadyCompleted"))?;
            let currency = hero::currency(db, a, "Gem", -cost).await?;
            let (sys, pay): (i64, i64) =
                sqlx::query_as("SELECT gem,pay_gem FROM user_info WHERE account_id=?")
                    .bind(a)
                    .fetch_one(&mut *db)
                    .await?;
            out["GemResult"] = json!({"AddValue":-cost,"NewSysGem":sys,"NewPayGem":pay});
            reward.currencies.push(currency);
            p["ResetCount"] = json!(count + 1);
            p["CompletedCount"] = json!(0);
            put(db, a, kind, c, &p).await?;
            out[format!("{prefix}DungeonInfo")] = p;
        }
        "reset_event_dungeon" => return Err(rule("ContentsDisabled")),
        "reset_raid" | "reset_all_raid" => {
            let ids = if action == "reset_raid" {
                vec![int(r, "RaidIndex")?]
            } else {
                list(db, a, "raid")
                    .await?
                    .iter()
                    .map(|v| n(v, "RaidIndex"))
                    .collect()
            };
            let mut infos = vec![];
            for id in ids {
                let mut info = get(db, a, "raid", id).await?;
                if info.is_null() || info["CompletedTime"].is_null() {
                    return Err(rule("AlreadyCompleted"));
                }
                let def = row(
                    s,
                    "Raid",
                    &[("Index", id), ("Level", n(&info, "RaidLevel"))],
                )?;
                let currency = hero::currency(db, a, "Gem", -n(def, "ResetGem")).await?;
                out["CurrencyResult"] = currency.clone();
                reward.currencies.push(currency);
                info["CompletedTime"] = Value::Null;
                put(db, a, "raid", id, &info).await?;
                infos.push(info);
            }
            out["RaidResult"] = if action == "reset_raid" {
                infos.first().cloned().unwrap_or(Value::Null)
            } else {
                json!({"LastRaidResetTime":time(now()),"RaidInfos":infos})
            };
        }
        "get_passive_info" => {
            out["PassiveInfos"] = shakmeh_passives(db, s, a).await?;
        }
        "get_punishment_raid_info" | "open_punishment_raid" | "reset_punishment_raid" => {
            let mut p = get(db, a, "punishment_open", 0).await?;
            if action == "open_punishment_raid" {
                let group = int(r, "GroupIndex")?;
                let level = int(r, "Level")?;
                let kind = int(r, "DungeonType")?;
                row(
                    s,
                    "PunishmentGroup",
                    &[("GroupIndex", group), ("PunishmentDungeonType", kind)],
                )?;
                // The extraction has punishment definitions whose Raid rows are absent.
                // An unplayable group must not consume its opening key.
                if kind != 0
                    || !s
                        .tables
                        .battle
                        .rows("PunishmentRaid")
                        .iter()
                        .filter(|v| n(v, "GroupIndex") == group && n(v, "RaidLevel") == level)
                        .any(|v| {
                            s.tables
                                .battle
                                .find("Raid", &[("Index", n(v, "RaidIndex")), ("Level", level)])
                                .is_some_and(|raid| raid["IsOpen"] == true)
                        })
                {
                    return Err(rule("ContentsDisabled"));
                }
                if !s
                    .tables
                    .battle
                    .rows("PunishmentRaid")
                    .iter()
                    .any(|v| n(v, "GroupIndex") == group && n(v, "RaidLevel") == level)
                {
                    return Err(rule("DungeonNotFound"));
                }
                if !p.is_null() && p["Day"] == day() && n(&p, "IsOpen") == 1 {
                    return Err(rule("AlreadyCompleted"));
                }
                out["StaminaResult"] = charge(db, s, a, 30, 1).await?;
                p = json!({"GroupIndex":group,"DungeonType":int(r,"DungeonType")?,"IsOpen":1,"OpenedTime":time(now()),"ExpireTime":time(now()+86400),"OpenLevel":level,"ClearLevel":0,"ClearCount":0,"Day":day()});
                put(db, a, "punishment_open", 0, &p).await?;
            }
            if action == "reset_punishment_raid" {
                if n(&p, "GroupIndex") != int(r, "GroupIndex")?
                    || n(&p, "DungeonType") != int(r, "DungeonType")?
                {
                    return Err(rule("DungeonNotFound"));
                }
                p["IsOpen"] = json!(0);
                put(db, a, "punishment_open", 0, &p).await?;
            }
            out["OpenPunishmentRaidInfo"] = p;
            out["PunishmentRaidInfos"] = json!(list(db, a, "punishment_raid").await?);
            if out["StaminaResult"].is_null() {
                out["StaminaResult"] = charge(db, s, a, 30, 0).await?;
            }
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    let v = rewards(db, s, a, reward).await?;
    append_rewards(&mut out, &v);
    Ok(out)
}
