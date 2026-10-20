use super::*;
use crate::models::item::ItemGrant;

async fn participation_reward(db: &mut SqliteConnection, s: &AppState, a: i64, id: i64, damage: i64, reward: &mut Rewards) -> Result<()> {
    let tier=s.tables.battle.rows("WorldBossScoreReward").iter().find(|r|
        n(r,"WorldBossIndex")==id && damage>=n(r,"MinScore") &&
        (n(r,"MaxScore")==0 || damage<=n(r,"MaxScore")))
        .ok_or_else(||rule("ItemDataNotFound"))?;
    let data=s.tables.get_reward(n(tier,"RewardIndex") as i32).ok_or_else(||rule("ItemDataNotFound"))?;
    tutorial::currency(db,a,"Gold",data.roll_gold(),reward).await?;
    tutorial::currency(db,a,"Gem",data.roll_gem(),reward).await?;
    tutorial::currency(db,a,"WorldBossPoint",n(tier,"WorldBossPoint"),reward).await?;
    for drop in data.roll_items(&s.tables.reward_string_pool) {
        let (code,filter)=crate::tables::parse_item_code(&drop.item_code);
        if s.tables.item_groups.contains(&code) {
            // Two random boxes means two independent draws, not one roll whose
            // quantity is multiplied by two (which forces identical rarities).
            for _ in 0..drop.count {
                let (index,count,star,_)=s.tables.roll_item_from_group_code(&code,&filter)
                    .ok_or_else(||rule("ItemDataNotFound"))?;
                item::give(db,s,a,ItemGrant {index,count,star,custom:0},reward).await?;
            }
        } else {
            let index=s.tables.get_item_index(&code).ok_or_else(||rule("ItemDataNotFound"))?;
            item::give(db,s,a,ItemGrant {index,count:drop.count,star:drop.star_min,custom:drop.custom_option_index},reward).await?;
        }
    }
    Ok(())
}
pub(super) fn match_season(s: &AppState, r: &Request) -> Result<Value> {
    let arena = int(r, "ArenaType")?;
    if !matches!(arena, 0 | 5) {
        return Err(rule("ContentsDisabled"));
    }
    let (number, start, end) = season(s);
    Ok(
        json!({"SeasonData":{"Index":number,"Name":"Local season","Begin":time(start),"End":time(end),"Description":"","SeasonIndex":number,"ShowSeasonIndex":number,"ArenaType":if arena==5{"Ordeal"}else{"Normal"},"MatchSeasonType":"Regular","IsTakeReward":false,"PlayTimeBegin":"00:00:00","PlayTimeEnd":"23:59:59","LimitedHeroStar":0,"LimitedHeroLevel":0,"LimitedHeroCount":4,"ActiveTier":true}}),
    )
}
pub(super) fn season(s: &AppState) -> (i64, i64, i64) {
    let epoch = s.tables.battle.rules["SeasonEpoch"]
        .as_str()
        .and_then(|v| chrono::NaiveDateTime::parse_from_str(v, "%Y-%m-%d %H:%M:%S").ok())
        .map(|v| v.and_utc().timestamp())
        .unwrap_or(1767571200);
    let length = settings(s, "SeasonDays", 7).max(1) * 86400;
    let number = ((now() - epoch).max(0) / length) + 1;
    (
        number,
        epoch + (number - 1) * length,
        epoch + number * length,
    )
}
fn world_boss_season(s: &AppState) -> (i64,i64,i64) {
    let (_,start,end)=season(s);
    let offset=s.tables.services.rows("Constant").iter()
        .find(|v|v["Key"]=="WorldBossGlobalUtcOffSetHour")
        .and_then(|v|v["Value"].as_str()).and_then(|v|v.parse::<i64>().ok()).unwrap_or(9)*3600;
    let length=end-start;
    let (number,_,_)=season(s);
    if now()>=end-offset {(number+1,end-offset,end+length-offset)}
    else {(number,start-offset,end-offset)}
}
fn boss_season(s: &AppState, family: &str, id: i64) -> Result<(i64,i64,i64)> {
    Ok(if family=="world_boss" && source(s,family,id)?["IsGlobal"]==true {world_boss_season(s)} else {season(s)})
}
fn world_boss_day(at:i64)->String {
    // Recovered WorldBossGlobalDailyRewardResetTime=15:00:00 UTC.
    chrono::DateTime::from_timestamp(at+9*3600,0).unwrap().date_naive().to_string()
}
fn source<'a>(s: &'a AppState, family: &str, id: i64) -> Result<&'a Value> {
    let name = if family == "event_world_boss" {
        "EventWorldBoss"
    } else {
        "WorldBoss"
    };
    let fields = if family == "event_world_boss" {
        vec![("Index", id), ("BossLevel", 1)]
    } else {
        vec![("Index", id)]
    };
    row(s, name, &fields)
}
// Native WorldBoss consumes the first entry and resolves its previous season
// through NextIndex. Legacy local and special daily rows are not the live cycle.
fn active_world_boss(s: &AppState, season: i64) -> Result<i64> {
    let start = settings(s, "WorldBossRotationStart", 3);
    let mut cycle = Vec::new();
    let mut id = start;
    loop {
        if cycle.contains(&id) {
            if id != start { return Err(rule("WorldBossNotActive")); }
            break;
        }
        let def = source(s, "world_boss", id)?;
        if def["IsGlobal"] != true || n(def, "NextIndex") <= 0 {
            return Err(rule("WorldBossNotActive"));
        }
        cycle.push(id);
        id = n(def, "NextIndex");
    }
    Ok(cycle[((season - 1).max(0) as usize) % cycle.len()])
}
// Global World Boss is a timed score competition in the recovered tables:
// DealBattleDuration=518400, NormalBattleDuration=0, KillRewardIndex=0.
// Keep positive HP for the native persistent-HP protocol during scoring; it is
// a per-battle compatibility value, not a finite pool consumed by players.
fn refresh_global_boss(v: &mut Value, def: &Value, start: i64, season_end: i64, at: i64) {
    let end = start.saturating_add(n(def, "DealBattleDuration").max(0)).min(season_end);
    let active = at >= start && at < end;
    v["MaxHp"] = json!(n(v, "MaxHp").max(1));
    v["MonsterHp0"] = json!(if active { n(v, "MaxHp") } else { 0 });
    v["Status"] = json!(if active { "ActiveDeal" } else { "Ended" });
    v["CreatedTime"] = json!(time(start));
    v["BeginTime"] = json!(time(start));
    v["EndedTime"] = json!(time(end));
    v["EndTime"] = json!(time(end));
    v["NextOpenTime"] = json!(time(season_end));
    v["BattleCompletedTime"] = if active { Value::Null } else { json!(time(end)) };
}

#[cfg(test)]
mod global_schedule_tests {
    use super::*;

    #[test]
    fn world_boss_global_schedule_uses_six_days_and_repairs_only_active_period() {
        let start = 1_767_571_200_i64;
        let end = start + 7 * 86400;
        let def = json!({"DealBattleDuration":518400,"NormalBattleDuration":0});
        let original = json!({"MaxHp":1000000000000_i64,"MonsterHp0":0,"Status":"Ended",
            "TotalDamage":6164131807809_i64,"RewardedTime":"saved-reward","DailyRewarding":1});
        for at in [start, start + 518399] {
            let mut v = original.clone();
            refresh_global_boss(&mut v,&def,start,end,at);
            assert_eq!(v["Status"],"ActiveDeal");
            assert!(n(&v,"MonsterHp0")>0);
            assert_eq!(v["TotalDamage"],original["TotalDamage"]);
            assert_eq!(v["RewardedTime"],original["RewardedTime"]);
            assert_eq!(v["DailyRewarding"],1);
            assert_eq!(v["EndTime"],time(start+518400));
            assert_eq!(v["NextOpenTime"],time(end));
            let before=v.clone();
            refresh_global_boss(&mut v,&def,start,end,at);
            assert_eq!(v,before,"Repair must be idempotent");
        }
        for at in [start-1,start+518400,end-1,end] {
            let mut v=original.clone();
            refresh_global_boss(&mut v,&def,start,end,at);
            assert_eq!(v["Status"],"Ended");
            assert_eq!(v["MonsterHp0"],0);
        }
        let mut v=original;
        refresh_global_boss(&mut v,&def,end,end+7*86400,end);
        assert_eq!(v["Status"],"ActiveDeal");
    }
}

async fn boss(db: &mut SqliteConnection, s: &AppState, family: &str, id: i64) -> Result<Value> {
    let def = source(s, family, id)?;
    if family == "event_world_boss" && def["IsOpen"] != true {
        return Err(rule("WorldBossNotActive"));
    }
    let (season, start, end) = boss_season(s,family,id)?;
    let key = campaign::key(id, season);
    let mut v = get(db, 0, family, key).await?;
    if v.is_null() {
        let hp = if family == "event_world_boss" {
            n(def, "MaxHp")
        } else {
            settings(s, "WorldBossMaxHp", 1000000000000)
        };
        v = json!({"Index":id,"Season":season,"MonsterHp0":hp,"MaxHp":hp,"TotalDamage":0,"Status":"ActiveDeal","CreatedTime":time(start),"EndedTime":time(end),"BeginTime":time(start),"EndTime":time(end),"NextOpenTime":time(end),"BossLevel":1,"OrderNo":1,"DailyBossKillCount":0,"RewardedTime":null,"DailyRewardedTime":null,"DailyRewarding":0,"BattleCompletedTime":null});
        put(db, 0, family, key, &v).await?;
    }
    if family == "world_boss" && def["IsGlobal"] == true {
        let previous = v.clone();
        refresh_global_boss(&mut v, def, start, end, now());
        // This also repairs bosses incorrectly ended by the former HP pool,
        // without changing scores, total damage, claim ledgers or rewards.
        if v != previous { put(db, 0, family, key, &v).await?; }
    }
    Ok(v)
}
pub(super) async fn validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<()> {
    for (family, key) in [
        ("world_boss", "WorldBossIndex"),
        ("event_world_boss", "EventWorldBossIndex"),
    ] {
        let id = int(r, key)?;
        if id == 0 {
            continue;
        }
        let def = source(s, family, id)?;
        if n(def, "ChapterIndex") != int(r, "ChapterIndex")?
            || n(def, "DungeonIndex") != int(r, "DungeonIndex")?
        {
            return Err(rule("WrongWorldBossDungeon"));
        }
        let v = boss(db, s, family, id).await?;
        if n(&v, "MonsterHp0") <= 0 {
            return Err(rule("WorldBossNotActive"));
        }
        let highest: i64 =
            sqlx::query_scalar("SELECT COALESCE(MAX(level),0) FROM heroes WHERE account_id=?")
                .bind(a)
                .fetch_one(&mut *db)
                .await?;
        if highest < n(def, "ReqMaxHeroLevel") {
            return Err(rule("NotAvailableHero"));
        }
    }
    Ok(())
}
pub(super) async fn enter(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
    out: &mut Value,
) -> Result<()> {
    for (family, key, field) in [
        ("world_boss", "WorldBossIndex", "WorldBossInfo"),
        (
            "event_world_boss",
            "EventWorldBossIndex",
            "EventWorldBossInfo",
        ),
    ] {
        let id = int(r, key)?;
        if id > 0 {
            let definition = source(s, family, id)?;
            let key_type = if family == "event_world_boss" {
                17
            } else if definition["IsGlobal"] == true {
                18
            } else {
                11
            };
            let dungeon = campaign::dungeon(s, r)?;
            if n(dungeon, "ReqStaminaType") != key_type
                || campaign::stamina_cost(dungeon, campaign::difficulty(r)?) == 0
            {
                out["StaminaResult"] =
                    dungeons::charge(db, s, a, key_type, settings(s, "BossTicketsPerBattle", 1))
                        .await?;
            }
            let v = boss(db, s, family, id).await?;
            entry["BossSeason"] = v["Season"].clone();
            out[field] = v;
        }
    }
    Ok(())
}
async fn score(
    db: &mut SqliteConnection,
    family: &str,
    id: i64,
    season: i64,
    a: i64,
    damage: i64,
    battle_time: i64,
    date: &str,
) -> Result<()> {
    sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score,battle_time) VALUES(?,?,?,?,?,?,?) ON CONFLICT(family,boss,season,account,day) DO UPDATE SET score=score+excluded.score,battle_time=battle_time+excluded.battle_time").bind(family).bind(id).bind(season).bind(a).bind(date).bind(damage).bind(battle_time).execute(db).await?;
    Ok(())
}
// The native campaign client leaves TotalDamage unset and reports received
// damage on the persistent enemy at wave slot zero instead.
fn native_world_boss_damage(end: &Request) -> Result<i64> {
    // WebServiceRequester escapes strings before WWWForm escapes them again.
    // Accept direct JSON from API callers, or exactly one remaining URL layer.
    let raw = end.text("CreatureInfoString");
    let creatures: Vec<Value> = serde_json::from_str(raw).or_else(|_| {
        let decoded = urlencoding::decode(raw)
            .map_err(<serde_json::Error as serde::de::Error>::custom)?;
        serde_json::from_str(&decoded)
    }).map_err(|_| rule("WorldBossHpError"))?;
    if creatures.len() > 512 { return Err(rule("WorldBossHpError")); }
    let mut boss_damage = None;
    for creature in creatures {
        let integer = |field: &str| creature[field].as_i64()
            .or_else(|| creature[field].as_str().and_then(|v|v.parse().ok())).unwrap_or(-1);
        let key = creature["Key"].as_str().unwrap_or("");
        let parts: Vec<&str> = key.split('_').collect();
        if integer("TeamId") != 1 || parts.len()!=3 || parts[1]!="0" { continue; }
        if parts[2]!="1" || parts[0].parse::<i64>().ok()!=Some(integer("Index")) || integer("Index")<=0 || boss_damage.is_some() {
            return Err(rule("WorldBossHpError"));
        }
        boss_damage = Some(creature["GivedDamage"].as_i64()
            .or_else(|| creature["GivedDamage"].as_str().and_then(|v|v.parse().ok()))
            .filter(|v|*v>=0).ok_or_else(||rule("WorldBossHpError"))?);
    }
    boss_damage.ok_or_else(||rule("WorldBossHpError"))
}

pub(super) async fn finish(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    end: &Request,
    elapsed: i64,
    out: &mut Value,
) -> Result<()> {
    let damage = end.number("TotalDamage", 0)?;
    for (family, key, field) in [
        ("world_boss", "WorldBossIndex", "WorldBossInfo"),
        (
            "event_world_boss",
            "EventWorldBossIndex",
            "EventWorldBossInfo",
        ),
    ] {
        let id = int(r, key)?;
        if id == 0 {
            continue;
        }
        let saved: String = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?")
            .bind(a)
            .fetch_one(&mut *db)
            .await?;
        let entry: Value = read_json(&saved)?;
        let current_season=boss_season(s,family,id)?.0;
        if n(&entry, "BossSeason") != current_season {
            return Err(rule("WorldBossSeasonChanged"));
        }
        let damage = if damage == 0 && family == "world_boss" {
            native_world_boss_damage(end)?
        } else { damage };
        if damage <= 0
            || damage
                > elapsed.max(1).saturating_mul(settings(
                    s,
                    "WorldBossMaxDamagePerSecond",
                    1000000000,
                ))
        {
            return Err(rule("WorldBossHpError"));
        }
        let mut v = boss(db, s, family, id).await?;
        let dealt = damage.min(n(&v, "MonsterHp0"));
        if dealt <= 0 {
            return Err(rule("WorldBossNotActive"));
        }
        let global = family == "world_boss" && source(s, family, id)?["IsGlobal"] == true;
        // Legacy and event bosses keep their finite HP behavior. Global bosses
        // retain their per-battle HP and accept every validated scoring attempt.
        let credited = if family == "world_boss" { damage } else { dealt };
        if !global { v["MonsterHp0"] = json!(n(&v, "MonsterHp0") - dealt); }
        v["TotalDamage"] = json!(n(&v, "TotalDamage")
            .checked_add(credited)
            .ok_or_else(|| rule("WorldBossHpError"))?);
        let killed = !global && n(&v, "MonsterHp0") == 0;
        if killed {
            v["Status"] = json!("Ended");
            v["BattleCompletedTime"] = json!(time(now()));
        }
        put(db, 0, family, campaign::key(id, current_season), &v).await?;
        // The score ledger also drives ordinary UTC daily quests. Keep that
        // calendar intact; World Boss's 15:00 reset has its own daily snapshot.
        score(db, family, id, current_season, a, credited, elapsed,&day()).await?;
        if global {
            let previous=get(db,a,"world_boss_daily_score",id).await?;
            let date=world_boss_day(now());
            let same=previous["Day"]==date && n(&previous,"Season")==current_season;
            let score=if same {n(&previous,"Score")} else {0};
            let count=if same {n(&previous,"Count")} else {0};
            put(db,a,"world_boss_daily_score",id,&json!({"Day":date,"Season":current_season,
                "Score":score.checked_add(credited).ok_or_else(||rule("WorldBossHpError"))?,"Count":count+1})).await?;
        }
        if family == "world_boss" {
            let mut rank = rankers(db, family, id, current_season).await?
                .into_iter().find(|v| n(v,"AccountId")==a).ok_or_else(||rule("InvalidRank"))?;
            rank["Rank"] = json!(n(&rank,"Rank")-1);
            out["WorldBossRankInfo"] = rank;
        }
        out[field] = v;
        out[if family == "world_boss" {
            "WorldBossKilledByMe"
        } else {
            "EventWorldBossKilledByMe"
        }] = json!(killed);
        let mut reward = Rewards::default();
        let def = source(s, family, id)?;
        if global { participation_reward(db,s,a,id,damage,&mut reward).await?; }
        if family == "event_world_boss" {
            let claim_kind = format!("{family}_participant");
            let exists:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_reward_claims WHERE account=? AND kind=? AND idx=? AND period=?)").bind(a).bind(&claim_kind).bind(id).bind(season(s).0.to_string()).fetch_one(&mut *db).await?;
            if !exists {
                claim(db, a, &claim_kind, id, &season(s).0.to_string()).await?;
                reward_index(db, s, a, n(def, "ParticipantRewardIndex"), &mut reward).await?;
            }
        }
        if killed {
            reward_index(db, s, a, n(def, "KillRewardIndex"), &mut reward).await?;
        }
        dungeons::append_rewards(out, &rewards(db, s, a, reward).await?);
    }
    let raid = int(r, "RaidIndex")?;
    if raid > 0
        && s.tables
            .battle
            .rows("ChallengeRaid")
            .iter()
            .any(|v| n(v, "RaidIndex") == raid)
        && boolean(end, "Completed", false)?
    {
        if damage < 0
            || damage
                > elapsed.max(1).saturating_mul(settings(
                    s,
                    "WorldBossMaxDamagePerSecond",
                    1000000000,
                ))
        {
            return Err(rule("ChallengeRaidScoreError"));
        }
        score(
            db,
            "challenge",
            raid,
            season(s).0,
            a,
            damage.max(1),
            elapsed,
            &day(),
        )
        .await?;
        out["ChallengeRaidRankerInfo"] = rankers(db, "challenge", raid, season(s).0)
            .await?
            .into_iter()
            .find(|v| n(v, "AccountId") == a)
            .unwrap_or(Value::Null);
    }
    Ok(())
}
async fn rankers(
    db: &mut SqliteConnection,
    family: &str,
    id: i64,
    season: i64,
) -> Result<Vec<Value>> {
    let rows=sqlx::query("SELECT b.account,SUM(b.score) score,SUM(b.battle_time) battle_time,a.nick,u.avatar_hero_index FROM battle_scores b JOIN accounts a ON a.account_id=b.account JOIN user_info u ON u.account_id=b.account WHERE b.family=? AND b.boss=? AND b.season=? GROUP BY b.account ORDER BY score DESC,b.account").bind(family).bind(id).bind(season).fetch_all(db).await?;
    Ok(rows.iter().enumerate().map(|(i,r)|json!({"Rank":i+1,"AccountId":r.get::<i64,_>("account"),"Nick":r.get::<String,_>("nick"),"Score":r.get::<i64,_>("score"),"CurrentScore":r.get::<i64,_>("score"),"BattleTime":r.get::<i64,_>("battle_time"),"CurrentBattleTime":r.get::<i64,_>("battle_time"),"AvatarHeroIndex":r.get::<i64,_>("avatar_hero_index"),"CountryCode":"","ServerGroup":"Local"})).collect())
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    let (family, action) = path.split_once('/').unwrap();
    settle(db, s, a).await?;
    let mut out = item::success();
    if action.ends_with("get_world_boss_info") || action.ends_with("get_event_world_boss_info") {
        let mut infos = vec![];
        let name = if family == "world_boss" {
            "WorldBoss"
        } else {
            "EventWorldBoss"
        };
        let ids = if family == "world_boss" {
            BTreeSet::from([active_world_boss(s, world_boss_season(s).0)?])
        } else { s
            .tables
            .battle
            .rows(name)
            .iter()
            .filter(|v| family == "world_boss" || v["IsOpen"] == true && n(v, "BossLevel") == 1)
            .map(|v| n(v, "Index"))
            .collect::<BTreeSet<_>>() };
        for id in ids {
            infos.push(boss(db, s, family, id).await?);
        }
        out[if family == "world_boss" {
            "WorldBossInfos"
        } else {
            "EventWorldBossInfos"
        }] = json!(infos);
        return Ok(out);
    }
    let id = int(
        r,
        if family == "raid" {
            "RaidIndex"
        } else if family == "event_world_boss" {
            "EventWorldBossIndex"
        } else {
            "WorldBossIndex"
        },
    )?;
    let current = if family=="world_boss" {boss_season(s,family,id)?.0} else {season(s).0};
    let season = r.number("Season", current)?;
    if season < 1 || season > current {
        return Err(rule("NotFoundSeasonData"));
    }
    if action.ends_with("_hp") {
        out["Hp"] = boss(db, s, family, id).await?["MonsterHp0"].clone();
        return Ok(out);
    }
    if action.contains("daily_score") || action.contains("score_info") {
        let mut score:i64=sqlx::query_scalar("SELECT COALESCE(SUM(score),0) FROM battle_scores WHERE family=? AND boss=? AND season=? AND account=? AND (?=0 OR day=?)").bind(family).bind(id).bind(season).bind(a).bind(if action.contains("daily"){1}else{0}).bind(day()).fetch_one(&mut *db).await?;
        if family=="world_boss" && source(s,family,id)?["IsGlobal"]==true && action.contains("daily") {
            let daily=get(db,a,"world_boss_daily_score",id).await?;
            score=if daily["Day"]==world_boss_day(now()) && n(&daily,"Season")==season {n(&daily,"Score")} else {0};
        }
        out[if action.contains("daily") {
            "WorldBossDailyScore"
        } else {
            "ScoreInfo"
        }] = json!({"AccountId":a,"WorldBossIndex":id,"Season":season,"Score":score,"IsGetDailyParticipantReward":if score>0 {1}else{0},"GetDailyAchievementStep":0,"UpdateTime":time(now())});
        return Ok(out);
    }
    if action.contains("rank") {
        let kind = if family == "raid" {
            "challenge"
        } else {
            family
        };
        let mut rankings = rankers(db, kind, id, season).await?;
        // Native World Boss displays Rank + 1; -1 is its unranked sentinel.
        // Keep the internal one-based ranks used by reward settlement unchanged.
        if family == "world_boss" {
            for rank in &mut rankings { rank["Rank"] = json!(n(rank, "Rank") - 1); }
        }
        if action.contains("ranker_list") {
            let page = int(r, "PageNo")?;
            if page > 100 {
                return Err(rule("InvalidRank"));
            }
            out["RankerInfos"] = json!(rankings
                .into_iter()
                .skip(page as usize * 100)
                .take(100)
                .collect::<Vec<_>>());
        } else {
            out["TotalRankerCount"] = json!(rankings.len());
            out["RankInfo"] = rankings
                .into_iter()
                .find(|v| n(v, "AccountId") == a)
                .unwrap_or(Value::Null);
            if family == "world_boss" && out["RankInfo"].is_null() {
                let player = sqlx::query("SELECT a.nick,a.country_code,u.avatar_hero_index FROM accounts a JOIN user_info u ON u.account_id=a.account_id WHERE a.account_id=?")
                    .bind(a).fetch_one(&mut *db).await?;
                out["RankInfo"] = json!({"Rank":-1,"AccountId":a,"Nick":player.get::<String,_>("nick"),
                    "Score":0,"AvatarHeroIndex":player.get::<i64,_>("avatar_hero_index"),
                    "CountryCode":player.get::<Option<String>,_>("country_code").unwrap_or_default(),"ServerGroup":"Local"});
            }
        }
        return Ok(out);
    }
    Err(rule("ContentsDisabled"))
}

// Settle closed daily/season records lazily into the existing persistent mailbox.
// The ledger and mail are written in the same transaction, including after reconnect.
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let legacy_bosses=json!(s.tables.battle.rows("WorldBoss").iter()
        .filter(|r|r["IsGlobal"]!=true).map(|r|n(r,"Index")).collect::<Vec<_>>()).to_string();
    let records=sqlx::query("SELECT family,boss,season,day,score FROM battle_scores b WHERE account=? AND ((family='world_boss' AND boss IN (SELECT CAST(value AS INTEGER) FROM json_each(?)) AND day<? AND NOT EXISTS(SELECT 1 FROM battle_reward_claims c WHERE c.account=b.account AND c.kind='world_boss_daily' AND c.idx=b.boss AND c.period=CAST(b.season AS TEXT)||':'||b.day)) OR (family IN ('world_boss','challenge') AND season<=? AND NOT EXISTS(SELECT 1 FROM battle_reward_claims c WHERE c.account=b.account AND c.kind=b.family||'_season' AND c.idx=b.boss AND c.period=CAST(b.season AS TEXT)))) ORDER BY season,day LIMIT 500").bind(a).bind(legacy_bosses).bind(day()).bind(world_boss_season(s).0.max(season(s).0)).fetch_all(&mut *db).await?;
    for record in records {
        let family: String = record.get("family");
        let id: i64 = record.get("boss");
        let old: i64 = record.get("season");
        let date: String = record.get("day");
        let score: i64 = record.get("score");
        let global=family=="world_boss" && source(s,&family,id)?["IsGlobal"]==true;
        // These are participation drops, already paid at the end of each
        // global attempt. Do not pay another set as a next-day score mail.
        if family == "world_boss" && !global && date < day() {
            if let Some(reward) = s
                .tables
                .battle
                .rows("WorldBossScoreReward")
                .iter()
                .find(|v| {
                    n(v, "WorldBossIndex") == id
                        && score >= n(v, "MinScore")
                        && (n(v,"MaxScore")==0 || score <= n(v, "MaxScore"))
                })
            {
                deliver(
                    db,
                    s,
                    a,
                    SeasonClaim {
                        kind: "world_boss_daily",
                        id,
                        period: &format!("{old}:{date}"),
                    },
                    n(reward, "RewardIndex"),
                    "World boss daily reward",
                )
                .await?;
            }
        }
        let current=if global {world_boss_season(s)} else {season(s)};
        let closed=old<current.0 || (global && old==current.0 &&
            now()>=current.1+n(source(s,&family,id)?,"DealBattleDuration"));
        if !closed || family == "event_world_boss" { continue; }
        let ranks = rankers(db, &family, id, old).await?;
        let Some(me) = ranks.iter().find(|v| n(v, "AccountId") == a) else {
            continue;
        };
        let rank = n(me, "Rank");
        let data = if family == "challenge" {
            "ChallengeRaidReward"
        } else if global {
            "WorldBossGlobalReward"
        } else {
            "WorldBossReward"
        };
        if let Some(reward) = s.tables.battle.rows(data).iter().find(|v| {
            if family == "challenge" && n(v, "ChallengeRaidIndex") != id {
                return false;
            }
            let position = if n(v, "Percentage") > 0 {
                (rank * 100 + ranks.len() as i64 - 1) / ranks.len() as i64
            } else {
                rank
            };
            position >= n(v, "MinRank") && position <= n(v, "MaxRank")
        }) {
            deliver(
                db,
                s,
                a,
                SeasonClaim {
                    kind: &format!("{family}_season"),
                    id,
                    period: &old.to_string(),
                },
                n(reward, "RewardIndex"),
                "Battle season ranking reward",
            )
            .await?;
        }
    }
    Ok(())
}
struct SeasonClaim<'a> {
    kind: &'a str,
    id: i64,
    period: &'a str,
}

async fn deliver(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    identity: SeasonClaim<'_>,
    reward: i64,
    title: &str,
) -> Result<()> {
    let SeasonClaim { kind, id, period } = identity;
    let exists:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_reward_claims WHERE account=? AND kind=? AND idx=? AND period=?)").bind(a).bind(kind).bind(id).bind(period).fetch_one(&mut *db).await?;
    if exists {
        return Ok(());
    }
    let data = s
        .tables
        .get_reward(reward as i32)
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    let mut items = vec![];
    let mut currencies = vec![];
    for drop in data.roll_items(&s.tables.reward_string_pool) {
        let (code, filter) = crate::tables::parse_item_code(&drop.item_code);
        if code == "WorldBossPoint" || code == "RaidPoint" {
            currencies.push(json!({"CurrencyType":code,"Amount":drop.count}));
            continue;
        }
        let resolved = if let Some(index) = s.tables.get_item_index(&code) {
            Some((index, drop.count, drop.star_min))
        } else {
            s.tables
                .roll_item_from_group_code(&code, &filter)
                .map(|(i, c, star, _)| (i, c * drop.count, star))
        };
        let Some((index, count, star)) = resolved else {
            return Err(rule("ItemDataNotFound"));
        };
        if star != 0 || drop.custom_option_index != 0 {
            return Err(rule("ItemDataNotFound"));
        }
        items.push(json!({"ItemIndex":index,"ItemCount":count}));
    }
    claim(db, a, kind, id, period).await?;
    let expiry=if kind=="world_boss_season" && source(s,"world_boss",id)?["IsGlobal"]==true {
        s.tables.services.rows("Constant").iter().find(|v|v["Key"]=="WorldBossGlobalRewardMailExpireHour")
            .and_then(|v|v["Value"].as_str()).and_then(|v|v.parse::<i64>().ok()).unwrap_or(72)*3600
    } else {7*86400};
    sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_gold,reward_gem,reward_items,reward_currencies,expires_at) VALUES(?,'System',?,?,?,?,?,?,?)").bind(a).bind(title).bind(format!("Reward for boss {id}, period {period}.")).bind(data.roll_gold()).bind(data.roll_gem()).bind(json!(items).to_string()).bind(json!(currencies).to_string()).bind(time(now()+expiry)).execute(db).await?;
    Ok(())
}
