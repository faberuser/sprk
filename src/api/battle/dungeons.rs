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
    let mut result = charge_reserved(db, s, a, kind, cost).await?;
    if kind == 1 && cost > 0 {
        result["RaiderExpResult"] = stamina_exp(db, s, a, cost).await?;
    }
    Ok(result)
}

// Deduct a validated cost without awarding stamina EXP twice.
pub(super) async fn charge_reserved(
    db: &mut SqliteConnection, s: &AppState, a: i64, kind: i64, cost: i64,
) -> Result<Value> {
    if cost < 0 {
        return Err(rule("InvalidCost"));
    }
    if cost>0 && kind>0 && !matches!(kind,3|4|7) {
        let held=super::entry_costs::held(db,a,"Stamina",kind).await?;
        if held>0 {
            let current=Box::pin(crate::api::account::stamina::snapshot(db,s,a,kind)).await?;
            if n(&current,"NewValue")-held<cost {return Err(rule(if kind==1 {"NotEnoughStamina"}else{"NotEnoughDungeonKey"}));}
        }
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
    if kind == 2 || kind == 20 {
        return crate::api::community::tickets(db, s, a, name, -cost).await;
    }
    if kind == 10 {
        return crate::api::community::guild_ticket(db, s, a, -cost).await;
    }
    if kind == 15 {
        return crate::api::community::conquest_tickets(db,s,a,cost).await;
    }
    if kind == 1 {
        let before = crate::api::account::stamina::chicken(db, s, a).await?;
        sqlx::query("UPDATE user_info SET stamina=stamina-? WHERE account_id=? AND stamina>=?")
            .bind(cost).bind(a).bind(cost).execute(&mut *db).await?
            .rows_affected().eq(&1).then_some(()).ok_or_else(||rule("NotEnoughStamina"))?;
        let mut result = crate::api::account::stamina::chicken(db, s, a).await?;
        result["AddValue"] = json!(n(&before,"AddValue") - cost + n(&result,"AddValue"));
        return Ok(result);
    }
    if kind == 9 {
        let def = s.tables.services.find("Stamina", &[("StaminaType", kind)])
            .ok_or_else(|| rule("InvalidStaminaType"))?;
        let mut info = get(db, a, "key", kind).await?;
        // Older saves retained only Day/Count. Recover the last known spend
        // time when its response agrees with that balance; otherwise start
        // a timer now without inventing elapsed recovery time.
        if !info.is_null() && n(&info, "RechargeTime") <= 0 {
            if let Some(raw) = sqlx::query_scalar::<_, String>(
                "SELECT begin_response FROM battle_runs WHERE account=?")
                .bind(a).fetch_optional(&mut *db).await? {
                let begin: Value = read_json(&raw)?;
                let previous = &begin["StaminaResult"];
                if previous["Type"] == name && n(previous, "NewValue") == n(&info, "Count") {
                    if let Some(stamp) = previous["StaminaRechargeTime"].as_str()
                        .and_then(|v| chrono::NaiveDateTime::parse_from_str(v, "%Y-%m-%d %H:%M:%S").ok()) {
                        if info["Day"] == stamp.date().to_string() {
                            info["RechargeTime"] = json!(stamp.and_utc().timestamp());
                        }
                    }
                }
            }
        }
        let (info, result) = tower_key_at(&info, def, Utc::now(), cost)?;
        put(db, a, "key", kind, &info).await?;
        return Ok(result);
    }
    if kind == 26 {
        // Devourer entries accumulate; the native table specifies 3 per day,
        // not a daily replacement balance like ordinary dungeon keys.
        let def = s.tables.services.find("Stamina", &[("StaminaType", kind)])
            .ok_or_else(|| rule("InvalidStaminaType"))?;
        let info = get(db, a, "key", kind).await?;
        let (info, result) = shakmeh_key_at(&info, def, Utc::now(), cost)?;
        put(db, a, "key", kind, &info).await?;
        return Ok(result);
    }
    if let Some(def) = s.tables.services.find("Stamina", &[("StaminaType", kind)]) {
        let overrides = &s.tables.battle.rules["EntryRechargeOverrides"][name];
        let initial = overrides["GrantCount"].as_i64()
            .or_else(|| def["ResetCount"].as_array().and_then(|v|v.get(
                if v.len()==7 { Utc::now().weekday().num_days_from_sunday() as usize } else { 0 }
            )).and_then(Value::as_i64))
            .or_else(|| s.tables.battle.rules["DungeonKeyDailyDefaults"][name].as_i64()).unwrap_or(0);
        let mut info = get(db, a, "key", kind).await?;
        let column = match kind { 11 => Some("world_boss_ticket"), 13 => Some("sword2"), _ => None };
        if let Some(column) = column {
            let count:i64 = sqlx::query_scalar(&format!("SELECT {column} FROM user_info WHERE account_id=?"))
                .bind(a).fetch_one(&mut *db).await?;
            let initial_record=info.is_null();
            if initial_record { info = json!({"Day":day()}); }
            info["Count"] = json!(if initial_record {count.max(initial)}else{count});
        }
        let (info, result) = crate::api::account::recharge::at(&info, def, overrides, initial, Utc::now(), cost)?;
        if let Some(column) = column {
            sqlx::query(&format!("UPDATE user_info SET {column}=? WHERE account_id=?"))
                .bind(n(&info,"Count")).bind(a).execute(&mut *db).await?;
        }
        put(db, a, "key", kind, &info).await?;
        return Ok(result);
    }
    if !matches!(kind,3 | 4 | 7) {
        let initial=s.tables.battle.rules["DungeonKeyDailyDefaults"][name].as_i64().unwrap_or(0);
        let def=json!({"AttributeName":name,"UpdateType":2,"ResetCount":[initial],"MaxCountValue":initial.to_string()});
        let info=get(db,a,"key",kind).await?;
        let (info,result)=crate::api::account::recharge::at(&info,&def,&Value::Null,initial,Utc::now(),cost)?;
        put(db,a,"key",kind,&info).await?;
        return Ok(result);
    }
    let currency = match kind { 3 => "Gold", 4 => "Gem", _ => "Mileage" };
    let value = n(&hero::currency(db, a, currency, -cost).await?, "NewValue");
    Ok(
        json!({"Type":name,"AddValue":-cost,"NewValue":value,"StaminaRechargeTime":time(now()),"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}),
    )
}

fn tower_key_at(
    info: &Value, def: &Value, now: chrono::DateTime<Utc>, cost: i64,
) -> Result<(Value, Value)> {
    // Pre-Doomsday tower help: one admission every two hours. StaminaType 9
    // uses Regen with a fixed maximum of 80; ResetCount is not a daily grant.
    const INTERVAL: i64 = 2 * 3600;
    let cap = def["MaxCountValue"].as_str().and_then(|v| v.parse::<i64>().ok())
        .filter(|v| *v > 0).ok_or_else(|| rule("InvalidStaminaType"))?;
    let initial = def["ResetCount"].as_array().and_then(|v| v.first())
        .and_then(Value::as_i64).filter(|v| *v >= 0)
        .ok_or_else(|| rule("InvalidStaminaType"))?;
    let timestamp = now.timestamp();
    let old = if info.is_null() { initial.min(cap) } else { n(info, "Count").max(0) };
    let last = n(info, "RechargeTime");
    let last = if last <= 0 || last > timestamp { timestamp } else { last };
    let added = ((timestamp - last) / INTERVAL).min((cap - old).max(0));
    let available = old + added;
    if cost < 0 || cost > available {
        return Err(rule("NotEnoughDungeonKey"));
    }
    // Do not bank recovery while full. Spending from full starts a new timer.
    let anchor = if available >= cap { timestamp } else { last + added * INTERVAL };
    let value = available - cost;
    let next = if value < cap { INTERVAL - (timestamp - anchor) } else { 0 };
    let full = if value < cap { next + (cap - value - 1) * INTERVAL } else { 0 };
    Ok((
        json!({"Day":now.date_naive().to_string(),"Count":value,
            "RechargeTime":anchor,"RechargeCount":n(info,"RechargeCount")}),
        json!({"Type":"ChallengeTowerKey","AddValue":added-cost,"NewValue":value,
            "StaminaRechargeTime":time(anchor),"NextRechargeRemainTime":next,
            "FullRechargeRemainTime":full,"RechargeCount":n(info,"RechargeCount"),"IsHide":false}),
    ))
}

fn shakmeh_key_at(
    info: &Value, def: &Value, now: chrono::DateTime<Utc>, cost: i64,
) -> Result<(Value, Value)> {
    let daily = def["ResetCount"].as_array().and_then(|v| v.first())
        .and_then(Value::as_i64).filter(|v| *v > 0)
        .ok_or_else(|| rule("InvalidStaminaType"))?;
    let cap = def["MaxCountValue"].as_str().and_then(|v| v.parse::<i64>().ok())
        .filter(|v| *v > 0).ok_or_else(|| rule("InvalidStaminaType"))?;
    let today = now.date_naive();
    // Adopt legacy Day/Count records without replacing their remaining charges.
    let last = info["Day"].as_str()
        .and_then(|v| chrono::NaiveDate::parse_from_str(v, "%Y-%m-%d").ok())
        .unwrap_or(today);
    let old = if info.is_null() { daily.min(cap) } else { n(info, "Count").max(0) };
    let elapsed = (today - last).num_days().max(0);
    let added = elapsed.saturating_mul(daily).min((cap - old).max(0));
    let available = old + added;
    if cost > available {
        return Err(rule("NotEnoughDungeonKey"));
    }
    let value = available - cost;
    // All emulator daily resets use UTC. Keep a future anchor on clock rollback
    // so the same day's automatic grant cannot be issued twice.
    let anchor = last.max(today);
    let next_at = (anchor + Duration::days(1)).and_hms_opt(0, 0, 0).unwrap().and_utc();
    let next = (next_at - now).num_seconds();
    let days_to_full = ((cap - value).max(0) + daily - 1) / daily;
    let full = if days_to_full == 0 { 0 } else { next + (days_to_full - 1) * 86400 };
    Ok((
        json!({"Day":anchor.to_string(),"Count":value,"RechargeCount":0}),
        json!({"Type":"ShakmehMiddleBossKey","AddValue":added-cost,"NewValue":value,
            "StaminaRechargeTime":time(now.timestamp()),"NextRechargeRemainTime":next,
            "FullRechargeRemainTime":full,"RechargeCount":0,"IsHide":false}),
    ))
}

#[cfg(test)]
mod tower_charge_tests {
    use super::*;

    fn at(info: &Value, stamp: &str, cost: i64) -> (Value, Value) {
        tower_key_at(info, &json!({"ResetCount":[5],"MaxCountValue":"80"}),
            stamp.parse().unwrap(), cost).unwrap()
    }

    #[test]
    fn tower_recovery_keeps_partial_time_and_catches_up_offline_without_daily_reset() {
        let start = "2026-10-05T23:00:00Z";
        let legacy = json!({"Day":"2026-10-05","Count":0,"RechargeCount":0});
        let (saved, first) = at(&legacy, start, 0);
        assert_eq!(first["NewValue"], 0);
        assert_eq!(first["NextRechargeRemainTime"], 7200);
        assert_eq!(first["FullRechargeRemainTime"], 80 * 7200);
        let (_, before) = at(&saved, "2026-10-06T00:59:59Z", 0);
        assert_eq!(before["NewValue"], 0);
        assert_eq!(before["NextRechargeRemainTime"], 1);
        let (saved, recovered) = at(&saved, "2026-10-06T01:00:00Z", 0);
        assert_eq!(recovered["NewValue"], 1);
        assert_eq!(recovered["AddValue"], 1);
        let (saved, offline) = at(&saved, "2026-10-06T06:30:00Z", 1);
        assert_eq!(offline["NewValue"], 2);
        assert_eq!(offline["AddValue"], 1);
        assert_eq!(offline["NextRechargeRemainTime"], 1800);
        assert_eq!(offline["FullRechargeRemainTime"], 1800 + 77 * 7200);
        assert_eq!(at(&saved, "2026-10-06T06:30:00Z", 0).1["AddValue"], 0);
        assert!(tower_key_at(&saved, &json!({"ResetCount":[5],"MaxCountValue":"80"}),
            "2026-10-06T06:30:00Z".parse().unwrap(), 3).is_err());
    }

    #[test]
    fn tower_recovery_caps_preserves_overflow_and_starts_timer_on_spending_from_full() {
        let (saved, _) = at(&json!({"Count":79}), "2026-10-05T00:00:00Z", 0);
        let (saved, full) = at(&saved, "2026-10-06T00:00:00Z", 0);
        assert_eq!(full["NewValue"], 80);
        assert_eq!(full["AddValue"], 1);
        assert_eq!(full["NextRechargeRemainTime"], 0);
        assert_eq!(full["FullRechargeRemainTime"], 0);
        let (saved, spent) = at(&saved, "2026-10-08T00:00:00Z", 1);
        assert_eq!(spent["NewValue"], 79);
        assert_eq!(spent["NextRechargeRemainTime"], 7200);
        assert_eq!(at(&saved, "2026-10-08T00:00:01Z", 0).1["NewValue"], 79);
        let (saved, _) = at(&json!({"Count":85}), "2026-10-05T00:00:00Z", 0);
        let (_, overflow) = at(&saved, "2026-10-08T00:00:00Z", 0);
        assert_eq!(overflow["NewValue"], 85);
        assert_eq!(overflow["AddValue"], 0);
        let (_, spent) = at(&saved, "2026-10-08T00:00:00Z", 6);
        assert_eq!(spent["NewValue"], 79);
        assert_eq!(spent["NextRechargeRemainTime"], 7200);
        assert_eq!(at(&Value::Null, "2026-10-05T00:00:00Z", 0).1["NewValue"], 5);
        assert_eq!(at(&json!({"Count":9,"RechargeTime":1791244800}), "2026-10-05T00:00:00Z", 0).1["NewValue"], 9);
    }
}

#[cfg(test)]
mod shakmeh_charge_tests {
    use super::*;

    fn at(info: &Value, stamp: &str, cost: i64) -> (Value, Value) {
        let def = json!({"ResetCount":[3],"MaxCountValue":"60"});
        shakmeh_key_at(info, &def, stamp.parse().unwrap(), cost).unwrap()
    }

    #[test]
    fn shakmeh_daily_charges_preserve_legacy_balance_and_count_down_to_reset() {
        let legacy = json!({"Day":"2026-10-05","Count":4,"RechargeCount":0});
        let (saved, result) = at(&legacy, "2026-10-05T17:00:00Z", 0);
        assert_eq!(saved, legacy);
        assert_eq!(result["NewValue"], 4);
        assert_eq!(result["AddValue"], 0);
        assert_eq!(result["NextRechargeRemainTime"], 7 * 3600);
        assert_eq!(result["FullRechargeRemainTime"], 7 * 3600 + 18 * 86400);
        let (saved, result) = at(&saved, "2026-10-06T00:00:00Z", 0);
        assert_eq!(result["NewValue"], 7);
        assert_eq!(result["AddValue"], 3);
        assert_eq!(result["NextRechargeRemainTime"], 86400);
        assert_eq!(at(&saved, "2026-10-06T00:00:01Z", 0).1["AddValue"], 0);
        let (_, spent) = at(&saved, "2026-10-06T12:00:00Z", 1);
        assert_eq!(spent["NewValue"], 6);
        assert_eq!(spent["AddValue"], -1);
        assert_eq!(spent["NextRechargeRemainTime"], 12 * 3600);
    }

    #[test]
    fn shakmeh_daily_charges_catch_up_offline_stop_at_cap_and_preserve_overflow() {
        for (count, expected, added) in [(4, 13, 9), (59, 60, 1), (60, 60, 0), (65, 65, 0)] {
            let old = json!({"Day":"2026-10-02","Count":count});
            let (saved, result) = at(&old, "2026-10-05T00:00:00Z", 0);
            assert_eq!(result["NewValue"], expected);
            assert_eq!(result["AddValue"], added);
            assert_eq!(saved["Day"], "2026-10-05");
            if expected >= 60 { assert_eq!(result["FullRechargeRemainTime"], 0); }
        }
        let (_, new) = at(&Value::Null, "2026-10-05T00:00:00Z", 0);
        assert_eq!(new["NewValue"], 3);
        let full = json!({"Day":"2026-10-02","Count":60});
        let (saved, spent) = at(&full, "2026-10-05T00:00:00Z", 1);
        assert_eq!(spent["NewValue"], 59);
        assert_eq!(spent["FullRechargeRemainTime"], 86400);
        assert_eq!(at(&saved, "2026-10-05T00:00:01Z", 0).1["NewValue"], 59);
        assert_eq!(at(&saved, "2026-10-06T00:00:00Z", 0).1["NewValue"], 60);
        let future = json!({"Day":"2026-10-06","Count":4,"RechargeCount":0});
        let (saved, result) = at(&future, "2026-10-05T00:00:00Z", 0);
        assert_eq!(saved, future);
        assert_eq!(result["NewValue"], 4);
    }
}
pub(super) async fn stamina_exp(db: &mut SqliteConnection, s: &AppState, a: i64, cost: i64) -> Result<Value> {
    let rate = settings(s, "RaiderExpPerStamina", 200);
    let amount = cost.checked_mul(rate).filter(|v| *v >= 0 && *v <= i32::MAX as i64)
        .ok_or_else(|| rule("InvalidCost"))?;
    let mut rewards = Rewards::default();
    tutorial::team_exp(db, s, a, amount, &mut rewards).await?;
    Ok(json!(rewards.team_exp.first()))
}
pub(super) async fn key_snapshot(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let mut out = vec![];
    for k in 5..KEYS.len() {
        if s.tables.services.find("Stamina", &[("StaminaType", k as i64)]).is_some()
            || s.tables.battle.rules["DungeonKeyDailyDefaults"]
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
// God King's Temple story completion is shared by Trials and Eclipse.
pub(super) async fn require_godking_unlock(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let progress = campaign::progress(db, s, a, 65, 11).await?;
    if n(&progress, "FirstRewardedDiff") == 0 {
        return Err(rule("NotCompletedReqDungeon"));
    }
    Ok(())
}

// Original ContentsOpenCondition/SubQuest rows: 11580, 11790 and 11900.
pub(super) async fn require_late_raid_unlock(db: &mut SqliteConnection, s: &AppState, a: i64, battle_type: i64) -> Result<()> {
    let (chapter, dungeon) = match battle_type {
        40 | 45 | 46 => (10, 9),
        41 | 42 => (10, 30),
        47 => (11, 10), // Displayed as X-10.
        _ => return Ok(()),
    };
    if n(&campaign::progress(db, s, a, chapter, dungeon).await?, "FirstRewardedDiff") == 0 {
        return Err(rule("NotCompletedReqDungeon"));
    }
    Ok(())
}

pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    d: &Value,
) -> Result<()> {
    let battle_type = n(d, "BattleType");
    require_late_raid_unlock(db, s, a, battle_type).await?;
    if battle_type == 48 {
        karma::validate(db,s,a,r).await?;
    }
    if battle_type == 47 && int(r, "RaidIndex")? <= 0 {
        return Err(rule("DungeonNotFound"));
    }
    if matches!(
        battle_type,
        4 | 9 | 20 | 30 | 32 | 36 | 43 | 44
    ) {
        return Err(rule("ContentsDisabled"));
    }
    if matches!(battle_type, 12 | 29) {
        // Dragon solo variants still use BattleType.Raid in the client table.
        // Only their native offline route can use this campaign lifecycle.
        let raid = raid_data(s, r)?;
        let solo_type = if battle_type == 29 { 3 } else { 1 };
        if n(raid, "Type") != solo_type || raid["IsOnlineSingle"] != false {
            return Err(rule("ContentsDisabled"));
        }
        if battle_type == 29 {
            let condition = raid["OpenCondition"].as_array().filter(|v| v.len() == 2)
                .ok_or_else(|| rule("NotOpenedDungeon"))?;
            let index = condition[0].as_i64().unwrap_or(0);
            let required = condition[1].as_i64().unwrap_or(i64::MAX);
            let cleared = get(db, a, "raid", index).await?;
            let solo_index = s.tables.battle.rows("Raid").iter()
                .find(|v| n(v, "Index") == index).map(|v| n(v, "SingleRaidIndex")).unwrap_or(0);
            let solo_cleared = get(db, a, "raid", solo_index).await?;
            if n(&cleared, "RaidLevel").max(n(&solo_cleared, "RaidLevel")) < required {
                return Err(rule("NotOpenedDungeon"));
            }
            let shared = n(raid, "ClearRaidIndex");
            let progress = get(db, a, "raid", shared).await?;
            let minimum = s.tables.battle.rows("Raid").iter()
                .filter(|v| n(v, "Index") == shared).map(|v| n(v, "Level")).min()
                .ok_or_else(|| rule("NotOpenedDungeon"))?;
            if n(raid, "Level") > n(&progress, "RaidLevel").max(minimum) {
                return Err(rule("NotOpenedDungeon"));
            }
        }
    }
    if battle_type == 39 {
        crate::api::live::validate_purchase_dungeon(
            db,
            s,
            a,
            n(d, "ChapterIndex"),
            n(d, "DungeonIndex"),
        )
        .await?;
    }
    if battle_type == 33 {
        let raid = raid_data(s, r)?;
        if n(raid, "Type") != 13 || raid["IsOnlineSingle"] != false {
            return Err(rule("ContentsDisabled"));
        }
        let condition = raid["OpenCondition"].as_array().filter(|v| v.len() == 2)
            .ok_or_else(|| rule("NotOpenedDungeon"))?;
        let chapter = condition[0].as_i64().unwrap_or(0);
        let dungeon = condition[1].as_i64().unwrap_or(0);
        let min_diff = s.tables.tutorials.dungeon_difficulty(chapter as i32, dungeon as i32);
        if chapter <= 0 || dungeon <= 0 || n(&campaign::progress(db,s,a,chapter,dungeon).await?, "FirstRewardedDiff") >> min_diff == 0 {
            return Err(rule("NotCompletedReqDungeon"));
        }
        let shared = n(raid, "ClearRaidIndex");
        let progress = get(db, a, "raid", shared).await?;
        if n(raid, "Level") > n(&progress, "RaidLevel").max(1) {
            return Err(rule("NotOpenedDungeon"));
        }
    }
    if matches!(battle_type, 40 | 41 | 42 | 45 | 46) {
        let raid = raid_data(s, r)?;
        let solo = if matches!(battle_type, 45 | 46) { 17 } else { 15 };
        if n(raid, "Type") != solo || raid["IsOnlineSingle"] != false {
            return Err(rule("ContentsDisabled"));
        }
        let condition = raid["OpenCondition"].as_array().filter(|v| v.len() == 2)
            .ok_or_else(|| rule("NotOpenedDungeon"))?;
        let chapter = condition[0].as_i64().unwrap_or(0);
        let dungeon = condition[1].as_i64().unwrap_or(0);
        // Native IsCompleted(Easy) resolves Easy to this stage's minimum
        // difficulty; 10-9 starts at Normal, while 10-30 starts at Easy.
        let min_diff = s.tables.tutorials.dungeon_difficulty(chapter as i32, dungeon as i32);
        if chapter <= 0 || dungeon <= 0
            || n(&campaign::progress(db, s, a, chapter, dungeon).await?, "FirstRewardedDiff") >> min_diff == 0
        {
            return Err(rule("NotCompletedReqDungeon"));
        }
        let shared = n(raid, "ClearRaidIndex");
        let progress = get(db, a, "raid", shared).await?;
        if n(raid, "Level") > n(&progress, "RaidLevel").max(1) {
            return Err(rule("NotOpenedDungeon"));
        }
    }
    if battle_type == 16 {
        super::super::community::raid_validate(db, s, a, r).await?;
    }
    if battle_type == 26 {
        let ban=super::super::community::conquest_validate(db,s,a,r).await?;
        // Conquest's session bans live on its rotating dungeon definition.
        // RaidData.BanIndex is deliberately zero in the archived table.
        let definition=s.tables.arena_guild.find("BanRule",&[("Index",ban)])
            .ok_or_else(||rule("DungeonNotFound"))?;
        for id in campaign::selected_heroes(s,r)? {
            let hero=row(s,"BattleHero",&[("Index",id)])?;
            if definition["BanValue2"].as_str().unwrap_or("").split(',').any(|code|code==hero["CodeName"].as_str().unwrap_or("")) {
                return Err(rule("NotAvailableHero"));
            }
        }
    }
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
        // The Portal's native boss shortcuts require the story's first final
        // battle (601). That battle itself opens after the story battle 210,
        // whose first-clear reward fills the Wrath gauge.
        let story_complete = n(&campaign::progress(db, s, a, 50000, 601).await?, "FirstRewardedDiff") > 0;
        let first_story_final = n(d, "DungeonIndex") == 601
            && boolean(r, "ScenarioDungeon", false)?
            && n(&campaign::progress(db, s, a, 50000, 210).await?, "FirstRewardedDiff") > 0;
        if !story_complete && !first_story_final {
            return Err(rule("NotCompletedReqDungeon"));
        }
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
    if let Some(group) = s.tables
        .battle
        .find("GodkingTrialGroup", &[("ChapterIndex", c)])
    {
        require_godking_unlock(db, s, a).await?;
        restrictions::party(s, r, n(group, "BanRuleIndex"))?;
        let g = get(db, a, "godking", c).await?;
        // A Slate opens one trial until victory. Calendar resets replenish
        // tickets; they do not invalidate an already-paid dungeon.
        if n(&g, "IsOpen") != 1 {
            return Err(rule("GodkingTrialDungeonNotOpened"));
        }
        super::entry_costs::restore_godking_gate(db,s,a,c).await?;
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
        super::treasure::validate(db, s, a, d).await?;
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
            let opened = get(db, a, "punishment_open", n(def, "GroupIndex")).await?;
            if n(&opened, "IsOpen") != 1
                || n(&opened, "DungeonType") != 2
                || opened["Day"] != day()
                || n(&opened, "GroupIndex") != n(def, "GroupIndex")
                || n(&opened, "OpenLevel") != int(r, "RaidLevel")?
            {
                return Err(rule("RaidNotStarted"));
            }
            let clear = get(
                db,
                a,
                "punishment_raid",
                campaign::key(int(r, "RaidIndex")?, int(r, "RaidLevel")?),
            )
            .await?;
            if !clear.is_null() {
                return Err(rule("AlreadyCompleted"));
            }
            let cleared = punishment_clears(db, s, a, n(def, "GroupIndex")).await?;
            // The original dragon nodes use PunishmentClearDungeonCount < 2.
            if n(def, "GroupIndex") == 101001 && n(def, "Boss") == 0 && cleared.len() >= 2 {
                return Err(rule("NotOpenedDungeon"));
            }
            let mut expected = Vec::new();
            if n(def, "Boss") == 1 {
                for (raid, affix, wanted) in if n(def, "GroupIndex") == 101001 {
                    vec![
                        (1001, 5001, false),
                        (1002, 5002, false),
                        (1003, 5003, false),
                        (1004, 5004, false),
                    ]
                } else {
                    vec![(1006, 6001, true), (1007, 6002, true)]
                } {
                    if cleared.iter().any(|v| n(v, "RaidIndex") == raid) == wanted {
                        expected.push(affix);
                    }
                }
            }
            let mut supplied = ids(r, "AffixIndices", 32)?;
            supplied.sort();
            expected.sort();
            if supplied != expected {
                return Err(rule("InvalidAffix"));
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
    if n(campaign::dungeon(s, r)?, "BattleType") == 27 {
        entry["TreasurePeriod"] = super::treasure::info(db, s, a).await?["Period"].clone();
    }
    if let Some(f) = floor(s, r) {
        let cost = if entry["DeferredEntryCosts"]==true {
            super::entry_costs::charge_currency(db,s,a,entry,"Currency",match n(f,"BattleCostType") {2=>3,3=>4,6=>7,_=>0},n(f,"BattleStartCost")+n(f,"BattleEndCost")).await?
        } else {tower_cost(db,a,n(f,"BattleCostType"),n(f,"BattleStartCost")+n(f,"BattleEndCost")).await?};
        if !cost.is_null() {
            if entry["DeferredEntryCosts"]!=true {super::entry_costs::note(entry,"Currency",match n(f,"BattleCostType") {2=>3,3=>4,6=>7,_=>0}, n(f,"BattleStartCost")+n(f,"BattleEndCost"));}
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
    if n(dungeon, "BattleType") == 16 {
        super::super::community::raid_enter(db, s, a, r, entry, out).await?;
    }
    if n(dungeon, "BattleType") == 38 {
        let def = row(
            s,
            "ShakmehDungeon",
            &[("DungeonIndex", int(r, "DungeonIndex")?)],
        )?;
        if n(def, "ShakmehIndex") == 2 {
            entry["ShakmehStoryFinal"] = json!(boolean(r, "ScenarioDungeon", false)?
                && n(&campaign::progress(db, s, a, 50000, 601).await?, "FirstRewardedDiff") == 0);
            let (_, max) = shakmeh_gauge(db, s, a).await?;
            let cost = super::entry_costs::charge_currency(db,s,a,entry,"ShakmehGauge",0,max).await?;
            out["CurrencyResults2"] = json!([cost.clone()]);
            // ApplyBeginCampaignResponse ignores CurrencyResults2. The common
            // response handler applies ReservedCurrencyResults immediately.
            out["ReservedCurrencyResults"] = json!([cost]);
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
    if entry["ShakmehStoryFinal"] == true && !won && !entry["VictoryEntryCosts"].is_array() {
        // Farming is locked until this introduction is won. Preserve its
        // retry path instead of leaving a new player with an empty gauge.
        let (gauge, max) = shakmeh_gauge(db, s, a).await?;
        out["CurrencyResults"] = json!([hero::currency(db, a, "ShakmehMiddleBossPoint", max - gauge).await?]);
    }
    if entry["ShakmehFinal"] == true && won {
        put(db, a, "shakmeh_passive", 0, &json!([])).await?;
    }
    if entry["ShakmehFinal"] == true {
        // Also synchronize at settlement, including failure and the story
        // introduction's refund. This reports the balance without charging again.
        let snapshot = hero::currency(db, a, "ShakmehMiddleBossPoint", 0).await?;
        let mut updates = out["ReservedCurrencyResults"].as_array().cloned().unwrap_or_default();
        updates.push(snapshot);
        out["ReservedCurrencyResults"] = json!(updates);
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
    if let Some(stage) = s.tables
        .battle
        .find("UnderPrisonDungeon", &[("ChapterIndex", c), ("DungeonIndex", d)])
    {
        let mut p = under(db, s, a, c).await?;
        p["CompletedCount"] = json!(n(&p, "CompletedCount") + 1);
        p["Diff"] = json!(n(&p, "Diff").max(n(stage, "Diff")));
        p["LastCompletedTime"] = json!(time(now()));
        put(db, a, "under_prison", c, &p).await?;
        out["UnderPrisonInfos"] = json!([p]);
    }
    if let Some(g) = s
        .tables
        .battle
        .find("GodkingTrial", &[("ChapterIndex", c), ("DungeonIndex", d)])
    {
        reward_index(db, s, a, n(g, "RewardIndex"), &mut extra).await?;
        let mut state = get(db, a, "godking", c).await?;
        state["IsOpen"] = json!(0);
        state["IsOpened"] = json!(false);
        put(db, a, "godking", c, &state).await?;
        out["GodkingTrialDungeonInfo"] = state;
    }
    if s.tables.battle.find(
        "TreasureHouseDungeon",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    ).is_some() {
        if entry["TreasurePeriod"] != super::treasure::info(db, s, a).await?["Period"] {
            return Err(rule("TreasurehouseInitializeEnterPopup"));
        }
        // CampaignDungeon already grants this monster's drop reward.
        super::treasure::clear(db, s, a, out).await?;
    }
    if int(r, "RaidIndex")? > 0 {
        let data = raid_data(s, r)?;
        let mut index = n(data, "Index");
        let mut level = n(data, "Level");
        if matches!(n(data, "Type"), 1 | 3 | 13 | 15 | 17) {
            // RaidHelper reads the shared index and treats RaidLevel as the
            // highest selectable stage, not the last stage just completed.
            if n(data, "ClearRaidIndex") > 0 { index = n(data, "ClearRaidIndex"); }
            level = s.tables.battle.rows("Raid").iter()
                .filter(|v| n(v, "Index") == index && n(v, "Level") > level
                    && (n(data,"Type")!=13 || v["IsOpen"] == true))
                .map(|v| n(v, "Level")).min().unwrap_or(level);
            level = level.max(n(&get(db, a, "raid", index).await?, "RaidLevel"));
        }
        let info = json!({"RaidIndex":index,"RaidLevel":level,"BattleScore":end.number("TotalDamage",0)?,"CreatedTime":time(now()),"CompletedTime":time(now())});
        put(db, a, "raid", index, &info).await?;
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
        let definition = row(
            s,
            "PunishmentRaid",
            &[("RaidIndex", id), ("RaidLevel", level)],
        )?;
        let group = n(definition, "GroupIndex");
        let mut p = get(db, a, "punishment_open", group).await?;
        if n(definition, "Boss") == 1 {
            p["ClearLevel"] = json!(n(&p, "ClearLevel").max(level));
            p["ClearCount"] = json!(n(&p, "ClearCount") + 1);
            p["IsOpen"] = json!(0);
        }
        put(db, a, "punishment_open", group, &p).await?;
        let raid = raid_data(s, r)?;
        let check = n(raid, "ClearRaidIndex");
        put(db,a,"punishment_raid",campaign::key(id,level),&json!({"RaidIndex":id,"RaidLevel":level,"ClearCheckIndex":check,"ClearedTime":time(now())})).await?;
        let clears = punishment_clears(db, s, a, group).await?;
        let selected = s
            .tables
            .battle
            .rows("PunishmentRaidReward")
            .iter()
            .filter(|v| {
                n(v, "RaidIndex") == id
                    && n(v, "RaidLevel") == level
                    && (v["Active"] == true || n(v, "RewardType") == 2)
                    && v["ClearCheckIndex"].as_array().is_some_and(|checks| {
                        checks
                            .iter()
                            .all(|c| clears.iter().any(|v| v["ClearCheckIndex"] == *c))
                    })
            })
            .max_by_key(|v| v["ClearCheckIndex"].as_array().map_or(0, Vec::len));
        if let Some(selected) = selected {
            for index in selected["RewardIndex"].as_array().into_iter().flatten() {
                reward_index(db, s, a, index.as_i64().unwrap_or(0), &mut extra).await?;
            }
        }
        out["PunishmentRaidInfos"] = json!(clears);
        out["OpenPunishmentRaidInfo"] = p;
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
    // Aegina's Protection applies to both bosses throughout the cycle.
    // The archive's help text specifies a new roll after defeating the final boss.
    if passives.as_array().is_none_or(|v| v.is_empty()) {
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
        "ReservedCurrencyResults",
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
pub(super) async fn under(db: &mut SqliteConnection, s: &AppState, a: i64, c: i64) -> Result<Value> {
    let mut p = get(db, a, "under_prison", c).await?;
    if p.is_null() {
        p = json!({"ChapterIndex":c,"CompletedCount":0,"Diff":0,"MaxTryCount":settings(s,"UnderPrisonMaxDailyAttempts",3),"LastCompletedTime":null,"Day":day()});
    }
    if p["Day"] != day() {
        p["Day"] = json!(day());
        p["CompletedCount"] = json!(0);
    }
    // Older servers stored campaign difficulty (usually zero), not Stockade's
    // stage tier. Recover only tiers backed by successful, persisted clears.
    let cleared: Vec<i64> = sqlx::query_scalar(
        "SELECT dungeon_id FROM campaign_progress WHERE account_id=? AND chapter_id=? AND clear_count>0",
    ).bind(a).bind(c).fetch_all(&mut *db).await?;
    for dungeon in cleared {
        if let Some(stage) = s.tables.battle.find(
            "UnderPrisonDungeon", &[("ChapterIndex", c), ("DungeonIndex", dungeon)],
        ) {
            p["Diff"] = json!(n(&p, "Diff").max(n(stage, "Diff")));
        }
    }
    put(db, a, "under_prison", c, &p).await?;
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
            let request = Request(std::collections::HashMap::from([(
                "StaminaType".into(),
                "UndergroundPrisonKey".into(),
            )]));
            let result =
                crate::api::account::stamina::execute(db, s, a, &request, "user/recharge_stamina")
                    .await?;
            reward.currencies.push(result["CurrencyResult"].clone());
            out["UndergroundPrisonKeyResult"] = result["StaminaResult"].clone();
        }
        "get_treasure_house_info" | "reset_treasure_house_info" => {
            if action.starts_with("reset") {
                let current = super::treasure::info(db, s, a).await?;
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
                super::treasure::reroll(db, s, a).await?;
                reward.currencies.push(super::treasure::snapshot(db, a).await?);
            }
            out["PlayerTreasureHouseInfo"] = super::treasure::info(db, s, a).await?;
            if action.starts_with("get") {
                out["CurrencyResult"] = super::treasure::snapshot(db, a).await?;
            }
        }
        "open_godking_trial_dungeon" => {
            require_godking_unlock(db, s, a).await?;
            let c = int(r, "ChapterIndex")?;
            row(s, "GodkingTrialGroup", &[("ChapterIndex", c)])?;
            // A paid gate stays selected until victory (including retries after loss).
            for current in list(db, a, "godking").await? {
                if n(&current, "IsOpen") == 1 {
                    return Err(rule("AlreadyCompleted"));
                }
            }
            out["StaminaResult"] = charge(db,s,a,21,1).await?;
            let v = json!({"ChapterIndex":c,"IsOpen":1,"IsOpened":true,"RunVersion":2,"OpenedTime":time(now()),"NextResetRemainTime":-1,"Day":day()});
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
            if int(r,"DungeonType")?==1 { return karma::opening(db,s,a,r,action).await; }
            let group = int(r, "GroupIndex")?;
            let mut p = get(db, a, "punishment_open", group).await?;
            if action != "get_punishment_raid_info" {
                let active: bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=0 AND started>=?)")
                    .bind(a).bind(now()-settings(s,"BattleExpirySeconds",14400)).fetch_one(&mut *db).await?;
                if active {
                    return Err(rule("AlreadyOnBattleHero"));
                }
            }
            if action == "open_punishment_raid" {
                require_late_raid_unlock(db, s, a, 47).await?;
                let group = int(r, "GroupIndex")?;
                let level = int(r, "Level")?;
                let kind = int(r, "DungeonType")?;
                let group_data = row(
                    s,
                    "PunishmentGroup",
                    &[("GroupIndex", group), ("PunishmentDungeonType", kind)],
                )?;
                // The extraction has punishment definitions whose Raid rows are absent.
                // An unplayable group must not consume opening stamina.
                if kind != 2
                    || !s
                        .tables
                        .battle
                        .rows("PunishmentRaid")
                        .iter()
                        .filter(|v| n(v, "GroupIndex") == group && n(v, "RaidLevel") == level)
                        .all(|v| {
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
                let clears = if p["Day"] == day() && n(&p, "GroupIndex") == group {
                    n(&p, "ClearCount")
                } else {
                    0
                };
                let previous_level = if p["Day"] == day() && n(&p, "GroupIndex") == group {
                    n(&p, "ClearLevel")
                } else {
                    0
                };
                let increases = group_data["StaminaIncreaseCount"].as_array();
                let cost = (0..clears).try_fold(n(group_data, "OpenStaminaCount"), |cost, i| {
                    let increase = increases
                        .and_then(|v| v.get((i as usize).min(v.len().saturating_sub(1))))
                        .and_then(Value::as_i64)
                        .unwrap_or(n(group_data, "StaminaIncreaseRate"));
                    cost.checked_add(increase)
                        .ok_or_else(|| rule("InvalidCost"))
                })?;
                super::entry_costs::release_gate(db,s,a,"punishment_open",group,&mut p).await?;
                out["StaminaResult"] = super::entry_costs::opening_charge(db,s,a,1,cost).await?;
                for definition in s
                    .tables
                    .battle
                    .rows("PunishmentRaid")
                    .iter()
                    .filter(|v| n(v, "GroupIndex") == group)
                {
                    sqlx::query("DELETE FROM battle_state WHERE account=? AND kind='punishment_raid' AND idx=?")
                        .bind(a).bind(campaign::key(n(definition,"RaidIndex"),n(definition,"RaidLevel"))).execute(&mut *db).await?;
                }
                p = json!({"GroupIndex":group,"DungeonType":kind,"IsOpen":1,"OpenedTime":time(now()),"ExpireTime":time((now()/86400+1)*86400),"OpenLevel":level,"ClearLevel":previous_level,"ClearCount":clears,"Day":day()});
                super::entry_costs::mark_gate(db,a,&mut p,1,cost).await?;
                put(db, a, "punishment_open", group, &p).await?;
            }
            if action == "reset_punishment_raid" {
                let idx=n(&p,"GroupIndex");
                super::entry_costs::release_gate(db,s,a,"punishment_open",idx,&mut p).await?;
                if n(&p, "GroupIndex") != int(r, "GroupIndex")?
                    || n(&p, "DungeonType") != int(r, "DungeonType")?
                {
                    return Err(rule("DungeonNotFound"));
                }
                p["IsOpen"] = json!(0);
                put(db, a, "punishment_open", group, &p).await?;
            }
            out["OpenPunishmentRaidInfo"] = p;
            out["PunishmentRaidInfos"] = json!(list(db, a, "punishment_raid").await?);
            if out["StaminaResult"].is_null() {
                out["StaminaResult"] = charge(db, s, a, 1, 0).await?;
            }
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    let v = rewards(db, s, a, reward).await?;
    append_rewards(&mut out, &v);
    Ok(out)
}

pub(super) async fn punishment_clears(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    group: i64,
) -> Result<Vec<Value>> {
    Ok(list(db, a, "punishment_raid")
        .await?
        .into_iter()
        .filter(|v| {
            s.tables
                .battle
                .find(
                    "PunishmentRaid",
                    &[
                        ("RaidIndex", n(v, "RaidIndex")),
                        ("RaidLevel", n(v, "RaidLevel")),
                    ],
                )
                .is_some_and(|def| n(def, "GroupIndex") == group)
        })
        .collect())
}
