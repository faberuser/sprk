//! Native 2022 affix Challenge Raid. Rotation dates are SPRK policy; stages,
//! affix points and reward tiers come from the archived client tables.
use super::*;
#[cfg(test)]
#[path = "challenge_tests.rs"]
mod tests;

fn definition(s: &AppState, season: i64) -> Result<&Value> {
    let rotation = s.tables.battle.rules["ChallengeRaidRotation"].as_array()
        .filter(|v| !v.is_empty()).ok_or_else(|| rule("NotFoundSeasonData"))?;
    let id = rotation[((season - 1).max(0) as usize) % rotation.len()].as_i64()
        .ok_or_else(|| rule("NotFoundSeasonData"))?;
    row(s, "ChallengeRaid", &[("Index", id)])
}

pub(crate) fn events(s: &AppState) -> Vec<Value> {
    let (season, start, end) = seasons::season(s);
    let mut result = vec![];
    // Include the following week so a connected client can transition without
    // relogging. The native event manager filters by time.
    for offset in 0..=1 {
        let Ok(def) = definition(s, season + offset) else { continue; };
        for (i, bonus) in ["ChallengeRaidNotice", "ChallengeRaidSeason", "ChallengeRaid"].iter().enumerate() {
            result.push(json!({"Id":8000000+(season+offset)*10+i as i64,
                "DayOfWeek":"","BeginTime":time(start+offset*(end-start)),"EndTime":time(end+offset*(end-start)),
                "BeginHour":0,"BeginMinute":0,"EndHour":0,"EndMinute":0,"DungeonType":[],
                "BonusType":bonus,"Condition":n(def,"Index").to_string(),"ItemCode":"","ItemIndices":[],
                "BonusPercentage":0,"Title":null,"Message":[],"LocalInfo":[],
                "MoveTaskType":"None","MoveTaskValue":"","MinLevel":0,"BuffPosition":"None"}));
        }
    }
    result
}

pub(super) fn require_active(s: &AppState, raid: &Value) -> Result<i64> {
    let def = definition(s, seasons::season(s).0)?;
    if n(def,"RaidIndex") != n(raid,"Index") || !matches!(n(raid,"Type"),7|9) {
        return Err(rule("NotFoundSeasonData"));
    }
    Ok(n(def,"Index"))
}

fn points(s: &AppState, r: &Request) -> Result<i64> {
    let d = campaign::dungeon(s,r)?;
    let diff = campaign::difficulty(r)?;
    let group = campaign::field(d,"AffixGroupIndex",diff);
    let selected = ids(r,"AffixIndices",128)?;
    let mut total = 0;
    for id in selected {
        let affix = row(s,"ChallengeAffix",&[("Index",id)])?;
        if n(affix,"GroupIndex") != group || n(affix,"ShowType") != 0 {
            return Err(rule("InvalidAffix"));
        }
        total += n(affix,"Point");
    }
    // Advantages must be paid for by penalties, even before the clear bonus.
    if group <= 0 || total < 0 { return Err(rule("InvalidAffix")); }
    Ok((campaign::field(d,"ClearPoint",diff)+total)*campaign::field(d,"AffixMultiple",diff)/1000)
}

pub(super) fn validate(s: &AppState, r: &Request, service: bool) -> Result<()> {
    let raid = dungeons::raid_data(s,r)?;
    require_active(s,raid)?;
    if n(raid,"Type")==7 && !service { return Err(rule("BattleServerNotFound")); }
    if n(raid,"Type")==9 && service { return Err(rule("InvalidValue")); }
    if campaign::difficulty(r)? != 0 { return Err(rule("InvalidDiff")); }
    points(s,r)?;
    if !service { validate_affix_party(s,r)?; }
    Ok(())
}

pub(super) fn validate_affix_party(s: &AppState, r: &Request) -> Result<()> {
    for id in ids(r,"AffixIndices",128)? {
        let affix = row(s,"ChallengeAffix",&[("Index",id)])?;
        restrictions::party(s,r,n(affix,"BanRuleIndex"))?;
    }
    Ok(())
}

pub(super) fn enter(s: &AppState, r: &Request, entry: &mut Value) -> Result<()> {
    if !matches!(n(campaign::dungeon(s,r)?,"BattleType"),25|30) { return Ok(()); }
    entry["ChallengeSeason"] = json!(seasons::season(s).0);
    entry["ChallengeIndex"] = json!(require_active(s,dungeons::raid_data(s,r)?)?);
    entry["ChallengePoints"] = json!(points(s,r)?);
    Ok(())
}

pub(super) async fn rankers(db: &mut SqliteConnection, id: i64, season: i64) -> Result<Vec<Value>> {
    let rows = sqlx::query("SELECT c.*,a.nick,u.avatar_hero_index FROM challenge_results c JOIN accounts a ON a.account_id=c.account JOIN user_info u ON u.account_id=c.account WHERE challenge=? AND season=? ORDER BY score DESC,battle_time,account")
        .bind(id).bind(season).fetch_all(db).await?;
    Ok(rows.iter().enumerate().map(|(i,r)| json!({"Rank":i,"AccountId":r.get::<i64,_>("account"),
        "Nick":r.get::<String,_>("nick"),"AvatarHeroIndex":r.get::<i64,_>("avatar_hero_index"),
        "Score":r.get::<i64,_>("score"),"BattleTime":r.get::<i64,_>("battle_time"),
        "CurrentScore":r.get::<i64,_>("current_score"),"CurrentBattleTime":r.get::<i64,_>("current_time")})).collect())
}

async fn mine(db: &mut SqliteConnection, a: i64, id: i64, season: i64) -> Result<Value> {
    if let Some(me) = rankers(db,id,season).await?.into_iter().find(|v| n(v,"AccountId")==a) { return Ok(me); }
    let r = sqlx::query("SELECT a.nick,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE account_id=?")
        .bind(a).fetch_one(db).await?;
    Ok(json!({"Rank":-1,"AccountId":a,"Nick":r.get::<String,_>("nick"),"AvatarHeroIndex":r.get::<i64,_>("avatar_hero_index"),
        "Score":0,"CurrentScore":0,"BattleTime":0,"CurrentBattleTime":0}))
}

pub(super) async fn finish(db: &mut SqliteConnection, s: &AppState, a: i64, end: &Request,
    entry: &Value, elapsed: i64, out: &mut Value) -> Result<()> {
    let id = n(entry,"ChallengeIndex");
    if id == 0 { return Ok(()); }
    let season = n(entry,"ChallengeSeason");
    if season != seasons::season(s).0 { return Err(rule("NotFoundSeasonData")); }
    // Native ranking labels consume milliseconds; PureBattleTime is rounded
    // seconds and is zero on defeat, while ElapsedBattleTime retains precision.
    let won = boolean(end,"Completed",false)?;
    let battle_time = end.number("ElapsedBattleTime",end.number("PureBattleTime",0)?.saturating_mul(1000))?;
    if battle_time < 0 || (won && battle_time == 0) || battle_time > elapsed.max(0)*4000+10000 || battle_time > 14400000 {
        return Err(rule("ChallengeRaidScoreError"));
    }
    let score = if won { n(entry,"ChallengePoints") } else { 0 };
    if won {
        sqlx::query("INSERT INTO challenge_results VALUES(?,?,?,?,?,?,?) ON CONFLICT(season,challenge,account) DO UPDATE SET score=MAX(score,excluded.score),battle_time=CASE WHEN excluded.score>score OR (excluded.score=score AND excluded.battle_time<battle_time) THEN excluded.battle_time ELSE battle_time END,current_score=excluded.current_score,current_time=excluded.current_time")
            .bind(season).bind(id).bind(a).bind(score).bind(battle_time).bind(score).bind(battle_time).execute(&mut *db).await?;
        if let Some(team) = entry["ChallengeTeam"].as_array() {
            let mut team = team.clone();
            team.sort_by_key(|v| v.as_i64().unwrap_or(0));
            let mut players = vec![];
            for account in &team { players.push(mine(db,account.as_i64().ok_or_else(||rule("InvalidValue"))?,id,season).await?); }
            let request = Request(read_value(entry["Request"].clone())?);
            let data = json!({"RankerInfos":players,"Score":score,"BattleTime":battle_time,
                "RaidLevel":int(&request,"RaidLevel")?,"AffixIndices":ids(&request,"AffixIndices",128)?});
            sqlx::query("INSERT INTO challenge_teams VALUES(?,?,?,?,?,?) ON CONFLICT(season,challenge,team) DO UPDATE SET score=excluded.score,battle_time=excluded.battle_time,data=excluded.data WHERE excluded.score>score OR (excluded.score=score AND excluded.battle_time<battle_time)")
                .bind(season).bind(id).bind(json!(team).to_string()).bind(score).bind(battle_time).bind(data.to_string()).execute(&mut *db).await?;
        }
    }
    let mut me = mine(db,a,id,season).await?;
    me["CurrentScore"] = json!(score);
    me["CurrentBattleTime"] = json!(battle_time);
    out["ChallengeRaidRankerInfo"] = me;
    Ok(())
}

pub(super) async fn execute(db: &mut SqliteConnection, s: &AppState, a: i64, r: &Request, action: &str) -> Result<Value> {
    settle(db,s,a).await?;
    let season = seasons::season(s).0;
    let id = int(r,"RaidIndex")?; // Native request uses ChallengeRaidIndex, not the underlying raid.
    if id != n(definition(s,season)?,"Index") { return Err(rule("NotFoundSeasonData")); }
    let mut out = item::success();
    if action.contains("multi_play") {
        let rows: Vec<String> = sqlx::query_scalar("SELECT data FROM challenge_teams WHERE season=? AND challenge=? ORDER BY score DESC,battle_time,team LIMIT 100")
            .bind(season).bind(id).fetch_all(db).await?;
        let mut ranks = vec![];
        for (i,raw) in rows.iter().enumerate() { let mut v: Value = read_json(raw)?; v["Rank"]=json!(i); ranks.push(v); }
        out["RankerInfos"] = json!(ranks);
    } else if action.ends_with("ranker_list") {
        let page = int(r,"PageNo")?;
        if !(0..=10000).contains(&page) { return Err(rule("NotFoundSeasonData")); }
        out["RankerInfos"] = json!(rankers(db,id,season).await?.into_iter().skip(page as usize*10).take(10).collect::<Vec<_>>());
    } else { out["RankInfo"] = mine(db,a,id,season).await?; }
    Ok(out)
}

pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let records: Vec<(i64,i64)> = sqlx::query_as("SELECT season,challenge FROM challenge_results WHERE account=? AND season<? AND NOT EXISTS(SELECT 1 FROM battle_reward_claims WHERE account=? AND kind='challenge_native_season' AND idx=challenge AND period=CAST(season AS TEXT))")
        .bind(a).bind(seasons::season(s).0).bind(a).fetch_all(&mut *db).await?;
    for (season,id) in records {
        let ranks = rankers(db,id,season).await?;
        let Some(me) = ranks.iter().find(|v|n(v,"AccountId")==a) else {continue;};
        let rank = n(me,"Rank")+1;
        let reward = s.tables.battle.rows("ChallengeRaidReward").iter().find(|r| {
            let pos = if n(r,"Percentage")>0 {(rank*100+ranks.len() as i64-1)/ranks.len() as i64} else {rank};
            n(r,"ChallengeRaidIndex")==id && pos>=n(r,"MinRank") && (n(r,"MaxRank")==0 || pos<=n(r,"MaxRank"))
        });
        // The recovered 700xx tables define ranking prizes only for solo weeks.
        let Some(reward) = reward else {
            claim(db,a,"challenge_native_season",id,&season.to_string()).await?;
            continue;
        };
        seasons::deliver(db,s,a,seasons::SeasonClaim {kind:"challenge_native_season",id,period:&season.to_string()},
            n(reward,"RewardIndex"),"Challenge Raid season ranking reward").await?;
    }
    Ok(())
}
