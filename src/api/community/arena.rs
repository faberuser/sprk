use super::*;
#[path = "arena_online.rs"]
pub(crate) mod online;
#[path = "arena_honor_rewards.rs"]
pub(crate) mod honor_rewards;
#[path = "arena_daily_rewards.rs"]
mod daily_rewards;
pub(crate) async fn defense_heroes(db: &mut SqliteConnection, a: i64) -> Result<Vec<i64>> {
    let saved: Option<String> = sqlx::query_scalar("SELECT data FROM arena_runs WHERE account=? AND kind=0 AND json_extract(data,'$.DuelType') IS NOT 1 ORDER BY id DESC LIMIT 1")
        .bind(a).fetch_optional(&mut *db).await?;
    if let Some(saved) = saved {
        let saved: Value = parse(&saved)?;
        let mut heroes = vec![];
        for id in saved["Heroes"].as_array().into_iter().flatten().filter_map(Value::as_i64).take(4) {
            let owned: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)")
                .bind(a).bind(id).fetch_one(&mut *db).await?;
            if owned && !heroes.contains(&id) { heroes.push(id); }
        }
        if !heroes.is_empty() { return Ok(heroes); }
    }
    Ok(sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? ORDER BY RANDOM() LIMIT 4")
        .bind(a).fetch_all(db).await?)
}
pub(crate) fn season(s: &AppState) -> (i64, i64, i64) {
    season_at(s, now())
}
fn season_at(s: &AppState, at: i64) -> (i64, i64, i64) {
    let epoch = s.tables.arena_guild.rules["SeasonEpoch"]
        .as_str()
        .and_then(|v| chrono::NaiveDateTime::parse_from_str(v, "%Y-%m-%d %H:%M:%S").ok())
        .map(|v| v.and_utc().timestamp())
        .unwrap_or(1767571200);
    let length = settings(s, "SeasonDays", 7).clamp(1, 365) * 86400;
    let index = (at - epoch).max(0) / length + 1;
    (index, epoch + (index - 1) * length, epoch + index * length)
}
pub(super) fn season_info(s: &AppState, kind: i64) -> Value {
    let (index, start, end) = season(s);
    let name = [
        "Normal",
        "BanPick",
        "WorldBanPick",
        "GuildArena",
        "Arena3",
        "Ordeal",
        "Lucky",
    ]
    .get(kind as usize)
    .copied()
    .unwrap_or("Normal");
    json!({"Index":index,"Name":"Local arena season","Begin":time(start),"End":time(end),"Description":"","SeasonIndex":index,"ShowSeasonIndex":index,"ArenaType":name,"MatchSeasonType":"Regular","IsTakeReward":false,"PlayTimeBegin":"00:00:00","PlayTimeEnd":"23:59:59","LimitedHeroStar":0,"LimitedHeroLevel":0,"LimitedHeroCount":if kind==1{constant(s,"MatchingBanPickHeroCountMin",12)}else{4},"ActiveTier":true})
}
pub(crate) async fn match_tickets(db:&mut SqliteConnection,s:&AppState,a:i64,kind:i64,amount:i64)->Result<Value> {
    if kind==1 {
        if amount>0 {crate::api::account::stamina::refund(db,s,a,13,amount).await}
        else {crate::api::battle::charge_key(db,s,a,13,-amount).await}
    } else {tickets(db,s,a,"Sword",amount).await}
}
pub(crate) async fn tickets(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: &str,
    amount: i64,
) -> Result<Value> {
    let mut v = get(db, a, "tickets", if kind == "Sword" { 0 } else { 1 }).await?;
    if kind == "Sword" {
        let current:i64 = sqlx::query_scalar("SELECT sword FROM user_info WHERE account_id=?")
            .bind(a).fetch_one(&mut *db).await?;
        v["Count"] = json!(current);
        let def = s.tables.services.find("Stamina", &[("StaminaType",2)])
            .ok_or_else(||rule("InvalidStaminaType"))?;
        let overrides = &s.tables.battle.rules["EntryRechargeOverrides"][kind];
        let (mut saved, mut result) = crate::api::account::recharge::at(
            &v, def, overrides, current, chrono::Utc::now(), (-amount).max(0))?;
        if amount > 0 {
            let value = n(&saved,"Count").checked_add(amount)
                .ok_or_else(||rule("MaxStamina"))?;
            saved["Count"] = json!(value);
            let added = n(&result,"AddValue") + amount;
            let refreshed = crate::api::account::recharge::at(&saved,def,overrides,value,chrono::Utc::now(),0)?;
            saved = refreshed.0; result = refreshed.1;
            result["AddValue"] = json!(added);
        }
        saved["Value"] = saved["Count"].clone();
        sqlx::query("UPDATE user_info SET sword=? WHERE account_id=?")
            .bind(n(&saved,"Count")).bind(a).execute(&mut *db).await?;
        put(db,a,"tickets",0,&saved).await?;
        return Ok(result);
    }
    let defaults = settings(
        s,
        if kind == "Sword" {
            "ArenaDailyTickets"
        } else {
            "GuildArenaDailyTickets"
        },
        5,
    );
    let current = if kind == "Sword" {
        sqlx::query_scalar::<_, i64>("SELECT sword FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&mut *db)
            .await?
    } else {
        n(&v, "Value")
    };
    let current = if v["Day"] != day() {
        current.max(defaults)
    } else {
        current
    };
    let value = current
        .checked_add(amount)
        .filter(|v| *v >= 0)
        .ok_or_else(|| {
            rule(if kind == "Sword" {
                "NotEnoughSword"
            } else {
                "NotEnoughGuildArenaKey"
            })
        })?;
    v = json!({"Day":day(),"Value":value});
    put(db, a, "tickets", if kind == "Sword" { 0 } else { 1 }, &v).await?;
    if kind == "Sword" {
        sqlx::query("UPDATE user_info SET sword=? WHERE account_id=?")
            .bind(value)
            .bind(a)
            .execute(db)
            .await?;
    }
    Ok(
        json!({"Type":kind,"AddValue":amount,"NewValue":value,"StaminaRechargeTime":time(now()),"NextRechargeRemainTime":0,"FullRechargeRemainTime":0,"RechargeCount":0,"IsHide":false}),
    )
}
pub(super) async fn owned(db: &mut SqliteConnection, a: i64, heroes: &[i64]) -> Result<()> {
    if heroes.is_empty() {
        return Err(rule("NoHeroes"));
    }
    // One query for the whole selection (Honor registers up to 200 Heroes):
    // every requested Hero must exist exactly once for this account.
    let counts: std::collections::HashMap<i64, i64> = sqlx::query_as(
        "SELECT hero_index,count(*) FROM heroes WHERE account_id=? AND hero_index IN (SELECT value FROM json_each(?)) GROUP BY hero_index",
    )
    .bind(a)
    .bind(json!(heroes).to_string())
    .fetch_all(&mut *db)
    .await?
    .into_iter()
    .collect();
    if heroes.iter().any(|id| counts.get(id) != Some(&1)) {
        return Err(rule("InvalidHero"));
    }
    let active: Option<String> = sqlx::query_scalar(
        "SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND started>=?",
    )
    .bind(a)
    .bind(now() - 14400)
    .fetch_optional(&mut *db)
    .await?;
    if let Some(v) = active {
        let v: Value = parse(&v)?;
        if v["Heroes"]
            .as_array()
            .is_some_and(|v| heroes.iter().any(|id| v.contains(&json!(id))))
        {
            return Err(rule("CannotPlayInBattle"));
        }
    }
    let dispatches: Vec<String> =
        sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind='dispatch'")
            .bind(a)
            .fetch_all(db)
            .await?;
    for v in dispatches {
        let v: Value = parse(&v)?;
        if matches!(v["State"].as_str(), Some("Complete" | "Cancel")) {
            continue;
        }
        let ids: Vec<i64> = parse(v["HeroIndices"].as_str().unwrap_or("[]"))?;
        if ids.iter().any(|id| heroes.contains(id)) {
            return Err(rule("CannotPlayInBattle"));
        }
    }
    Ok(())
}
pub(super) async fn account(
    db: &mut SqliteConnection,
    a: i64,
    heroes: &[i64],
    npc: bool,
) -> Result<Value> {
    account_with(db, a, heroes, npc, Default::default()).await
}
/// `account`, reusing Hero snapshots the caller already built in this
/// transaction (Honor validation snapshots the whole registered collection).
async fn account_with(
    db: &mut SqliteConnection,
    a: i64,
    heroes: &[i64],
    npc: bool,
    mut built: std::collections::HashMap<i64, Value>,
) -> Result<Value> {
    let u = user(db, a).await?;
    let mut map = serde_json::Map::new();
    for id in heroes {
        let hero = match built.remove(id) {
            Some(hero) => hero,
            None => cached_hero(db, a, *id).await?,
        };
        map.insert(id.to_string(), hero);
    }
    Ok(
        json!({"UserInfo":u,"HeroInfos":map,"AiHeroInfos":{},"GroupHeroInfos":{},"DeckInfos":{},"Host":"","ServerGroup":"local","MatchServerHost":"","IsNpc":npc,"ArenaType":"Normal","TeamType":if npc{"Right"}else{"Left"},"Mmr":1000,"FightingPower":0,"Rank":0,"LeaderHeroIndex":heroes.first().copied().unwrap_or(0),"GuildInfo":[],"GuildSkills":[],"AccountBuffs":[],"ClassBuffDataBases":[],"GuildArenaBuffDataBases":[],"ExtraStatDataBases":[],"PetStatDataBases":[],"BattleInfo":null,"AvgFightingPower":0,"LuckyDeckIndex":0}),
    )
}
async fn ensure_kind(db: &mut SqliteConnection, s: &AppState, a: i64, kind: i64) -> Result<()> {
    sqlx::query("INSERT OR IGNORE INTO arena_scores(account,kind,season,score) VALUES(?,?,?,?)")
        .bind(a)
        .bind(kind)
        .bind(season(s).0)
        .bind(constant(s, "InitialMatchScore", 1000))
        .execute(db)
        .await?;
    Ok(())
}
fn tier(s: &AppState, kind: i64, rank: i64, score: i64) -> i64 {
    s.tables
        .arena_guild
        .rows(if kind == 1 { "GlobalBanPickTier" } else { "MatchTier" })
        .iter()
        .find(|v| {
            rank >= n(v, "MinRank")
                && (n(v, "MaxRank") == 0 || rank <= n(v, "MaxRank"))
                && score >= n(v, "MinScore")
                && score <= n(v, "MaxScore")
        })
        .map(|v| n(v, "Index"))
        .unwrap_or_else(|| {
            if kind == 1 { return 11; }
            s.tables
                .arena_guild
                .rows("MatchReward")
                .iter()
                .find(|v| rank >= n(v, "MinRank") && rank <= n(v, "MaxRank"))
                .map(|v| n(v, "TierIndex"))
                .unwrap_or(11)
        })
}
/// The leaderboard's ordering. Rows past `RANKED_LIMIT` are not ranked.
const RANKED_ORDER: &str = "ORDER BY score DESC,wins DESC,account";
const RANKED_LIMIT: i64 = 10000;
/// The whole leaderboard; the reference the paged/single-row helpers match.
#[cfg(test)]
async fn ranking_kind(db: &mut SqliteConnection, s: &AppState, season: i64, kind: i64) -> Result<Vec<Value>> {
    ranking_page(db, s, season, kind, 0, RANKED_LIMIT - 1).await
}
/// Zero-based, inclusive leaderboard rows `from..=to`, identical to the same
/// slice of `ranking_kind` without building every other player's row.
async fn ranking_page(db: &mut SqliteConnection, s: &AppState, season: i64, kind: i64, from: i64, to: i64) -> Result<Vec<Value>> {
    let to = to.min(RANKED_LIMIT - 1);
    if from > to { return Ok(vec![]); }
    let rows: Vec<(i64,i64,i64,i64)> = sqlx::query_as(&format!("SELECT account,score,wins,losses FROM arena_scores WHERE kind=? AND season=? {RANKED_ORDER} LIMIT ? OFFSET ?"))
        .bind(kind).bind(season).bind(to - from + 1).bind(from).fetch_all(&mut *db).await?;
    let mut out = vec![];
    for (i, (account, score, wins, losses)) in rows.into_iter().enumerate() {
        out.push(ranked_row(db, s, season, kind, from + i as i64, account, score, wins, losses).await?);
    }
    Ok(out)
}
#[allow(clippy::too_many_arguments)]
async fn ranked_row(db: &mut SqliteConnection, s: &AppState, season: i64, kind: i64, rank: i64, account: i64, score: i64, wins: i64, losses: i64) -> Result<Value> {
    let mut u = user(db, account).await?;
    // Native Arena ranks are zero-based; table tiers and rewards are one-based.
    merge(
        &mut u,
        json!({"Rank":rank,"TotalRank":rank,"TierRank":rank,"TierIndex":tier(s,kind,rank+1,score),"MatchScore":score,"SeasonWin":wins,"SeasonLose":losses,"ServerGroup":"local","CountryCode":"US"}),
    );
    u["UserInfo"] = u.clone();
    u["HeroInfos"] = ranking_deck(db, account, season, kind).await?;
    Ok(u)
}
/// One account's leaderboard row (as in `ranking_kind`), or None when it is
/// not ranked. Counts the rows ahead of it instead of building the board.
async fn ranked(db: &mut SqliteConnection, s: &AppState, season: i64, kind: i64, a: i64) -> Result<Option<Value>> {
    let Some((score, wins, losses)) = sqlx::query_as::<_,(i64,i64,i64)>("SELECT score,wins,losses FROM arena_scores WHERE account=? AND kind=? AND season=?")
        .bind(a).bind(kind).bind(season).fetch_optional(&mut *db).await? else { return Ok(None); };
    let rank: i64 = sqlx::query_scalar("SELECT count(*) FROM arena_scores WHERE kind=? AND season=? AND (score>? OR (score=? AND (wins>? OR (wins=? AND account<?))))")
        .bind(kind).bind(season).bind(score).bind(score).bind(wins).bind(wins).bind(a).fetch_one(&mut *db).await?;
    if rank >= RANKED_LIMIT { return Ok(None); }
    Ok(Some(ranked_row(db, s, season, kind, rank, a, score, wins, losses).await?))
}
/// Number of ranked players (`ranking_kind(..).len()`).
async fn ranked_count(db: &mut SqliteConnection, season: i64, kind: i64) -> Result<i64> {
    let count: i64 = sqlx::query_scalar("SELECT count(*) FROM arena_scores WHERE kind=? AND season=?")
        .bind(kind).bind(season).fetch_one(db).await?;
    Ok(count.min(RANKED_LIMIT))
}
async fn ranking_deck(db: &mut SqliteConnection, account: i64, season: i64, kind: i64) -> Result<Value> {
    // Honor registration contains the eligible collection, not the final four.
    // Its draft persists the actual combat formation in Heroes before loading.
    // Never substitute another league/season, a friendly duel or a random team.
    // Honor rows hold whole collections (~1.5 MB). Filter on the trigger-kept
    // `season`/`squad` columns (squad = non-friendly with 1-4 Heroes) so no
    // rejected row is JSON-parsed, then extract only the formation's Heroes.
    let saved: Option<(i64,String)> = sqlx::query_as("SELECT id,json_extract(data,'$.Heroes') FROM arena_runs WHERE account=? AND kind=? AND status='complete' AND season=? AND squad=1 ORDER BY id DESC LIMIT 1")
        .bind(account).bind(kind).bind(season).fetch_optional(&mut *db).await?;
    let Some((run, heroes)) = saved else { return Ok(json!([])); };
    let heroes: Value = parse(&heroes)?;
    let mut seen = std::collections::BTreeSet::new();
    let ids: Vec<i64> = heroes.as_array().into_iter().flatten().filter_map(Value::as_i64)
        .filter(|id| *id > 0 && seen.insert(*id)).collect();
    if ids.is_empty() { return Ok(json!([])); }
    // json_extract with two or more paths returns a JSON array (null for a
    // missing Hero). The leading placeholder path keeps a single Hero an array.
    let paths = ids.iter().map(|_| "?").collect::<Vec<_>>().join(",");
    let sql = format!("SELECT json_extract(data,'$.__none__',{paths}) FROM arena_runs WHERE id=?");
    let mut query = sqlx::query_scalar::<_, String>(&sql);
    for id in &ids { query = query.bind(format!("$.Register.AccountInfo.HeroInfos.\"{id}\"")); }
    let registered: Value = parse(&query.bind(run).fetch_one(&mut *db).await?)?;
    let mut result = vec![];
    for (i, id) in ids.into_iter().enumerate() {
        let hero = &registered[i + 1];
        if !hero.is_null() {
            result.push(json!({"Index":id,"Star":hero["Star"],"Level":hero["Level"]}));
        } else if let Some((star,level)) = sqlx::query_as::<_,(i64,i64)>("SELECT star,level FROM heroes WHERE account_id=? AND hero_index=?")
            .bind(account).bind(id).fetch_optional(&mut *db).await? {
            result.push(json!({"Index":id,"Star":star,"Level":level}));
        }
    }
    Ok(json!(result))
}
pub(super) async fn battle_info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    battle_info_kind(db, s, a, 0).await
}
pub(super) async fn inspection_info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    // Inspecting another player must not enroll them in the current season.
    let season = season(s).0;
    let rank = ranked(db, s, season, 0, a).await?;
    let data: Option<String> = sqlx::query_scalar("SELECT data FROM arena_scores WHERE account=? AND kind=0 AND season=?")
        .bind(a).bind(season).fetch_optional(&mut *db).await?;
    let data: Value = data.map(|v|parse(&v)).transpose()?.unwrap_or_else(||json!({}));
    let mut out = rank.unwrap_or_else(||json!({"Rank":-1,"TierIndex":0,"MatchScore":0,"SeasonWin":0,"SeasonLose":0}));
    out["SuccessiveWin"] = json!(n(&data,"SuccessiveWin"));
    Ok(out)
}
pub(crate) async fn battle_info_kind(db: &mut SqliteConnection, s: &AppState, a: i64, kind: i64) -> Result<Value> {
    ensure_kind(db, s, a, kind).await?;
    let rank = ranked(db, s, season(s).0, kind, a)
        .await?
        .ok_or_else(|| rule("MatchResultNotFound"))?;
    let data: String =
        sqlx::query_scalar("SELECT data FROM arena_scores WHERE account=? AND kind=? AND season=?")
            .bind(a)
            .bind(kind)
            .bind(season(s).0)
            .fetch_one(db)
            .await?;
    let mut out: Value = parse(&data)?;
    merge(&mut out, rank);
    out["WorldTierIndex"] = out["TierIndex"].clone();
    Ok(out)
}
pub(super) async fn execute(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request,action:&str)->Result<Value>{
    execute_inner(db,s,a,r,action,false).await
}
pub(crate) async fn service_result(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request)->Result<Value>{
    complete_match(db,s,a,r,true).await
}
async fn execute_inner(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
    trusted: bool,
) -> Result<Value> {
    // Native polls, cancellation and result requests omit ArenaType. Resolve
    // their durable run instead of accidentally operating on Victory's partition.
    let kind = if matches!(action,"wait_match"|"cancel_match"|"get_match_result") {
        let uid=r.number("MatchUid",0)?;
        // Separate statements so each uses its index. `+account` keeps SQLite
        // on the match index instead of JSON-parsing every one of the
        // account's runs (Honor rows are ~1.5 MB each).
        let found=if uid==0 {
            sqlx::query_scalar::<_,i64>("SELECT kind FROM arena_runs WHERE account=? AND (status IN ('waiting','battle') OR (status='canceled' AND json_extract(data,'$.QueueCancelReason') IS NOT NULL)) ORDER BY id DESC LIMIT 1")
                .bind(a).fetch_optional(&mut *db).await?
        } else {
            sqlx::query_scalar::<_,i64>("SELECT kind FROM arena_runs WHERE +account=? AND (id=? OR json_extract(data,'$.OnlineMatchUid')=?) ORDER BY id DESC LIMIT 1")
                .bind(a).bind(uid).bind(uid).fetch_optional(&mut *db).await?
        };
        found.unwrap_or(int(r,"ArenaType")?)
    } else {int(r,"ArenaType")?};
    if action == "get_season_info" {
        if kind == 5 {
            return Ok(json!({"SeasonData":super::super::battle::match_calendar(s)?}));
        }
        return Ok(json!({"SeasonData":season_info(s,kind)}));
    }
    if !(0..=1).contains(&kind) {
        return Err(rule("ContentsDisabled"));
    }
    honor_rewards::settle(db, s, a).await?;
    ensure_kind(db, s, a, kind).await?;
    sqlx::query("UPDATE arena_runs SET status='expired' WHERE account=? AND status IN ('waiting','battle') AND json_extract(data,'$.OnlineMatchUid') IS NULL AND started<?").bind(a).bind(now()-settings(s,"ArenaMatchExpirySeconds",900)).execute(&mut *db).await?;
    let mut out = item::success();
    match action {
        "get_match_rank" => {
            let target = r.number("RankerAccountId", a)?;
            let target = if target == 0 { a } else { target };
            let rank = ranked(db, s, season(s).0, kind, target).await?;
            let total = ranked_count(db, season(s).0, kind).await?;
            let position = rank.as_ref().map(|v| n(v, "Rank")).unwrap_or(0);
            out[if kind == 1 { "BattleBanPickInfo" } else { "BattleInfo" }] = if target == a {
                battle_info_kind(db, s, a, kind).await?
            } else {
                rank.ok_or_else(|| rule("MatchResultNotFound"))?
            };
            out["RankResult"] =
                json!({"TierRank":position,"TotalRank":position,"TotalRankerCount":total});
        }
        "get_match_ranker" | "get_server_ranker" | "get_world_ranker" => {
            let old = r.number("SeasonIndex", season(s).0)?;
            let old = if old == 0 { season(s).0 } else { old };
            // The client sends zero-based, inclusive page boundaries.
            let from = r.number("StartRank", 0)?.max(0);
            let to = r.number("EndRank", from + 99)?.max(from);
            if to - from >= 1000 {
                return Err(rule("RankError"));
            }
            let ranks = ranking_page(db, s, old, kind, from, to).await?;
            out[if action == "get_match_ranker" {
                "MatchRankers"
            } else {
                "Rankers"
            }] = json!(ranks);
        }
        "get_server_group_ranker" => {
            let score: i64 = sqlx::query_scalar(
                "SELECT COALESCE(SUM(score),0) FROM arena_scores WHERE kind=? AND season=?",
            )
            .bind(kind)
            .bind(season(s).0)
            .fetch_one(db)
            .await?;
            out["Rankers"] = json!([{"ServerGroup":"local","Rank":0,"Score":score}]);
        }
        "register_match" => {
            let duel=int(r,"DuelType")?;
            let group=r.number("GroupId",0)?;
            // A friendly duel registers in the league its invitation named.
            if !(0..=1).contains(&duel) || (duel==1 && super::duel::league(group)!=kind) {
                return Err(rule("ContentsDisabled"));
            }
            let invited=if duel==1 {super::duel::opponent(db,a,group).await?} else {0};
            if n(&user(db, a).await?, "TeamLevel") < constant(s, "ArenaOpenTeamLevel", 10) {
                return Err(rule("ContentsDisabled"));
            }
            let heroes = ids(r, "HeroIndices", if kind==1{200}else{4})?;
            owned(db, a, &heroes).await?;
            // Honor snapshots its whole collection here; registration reuses
            // them instead of rebuilding every Hero (~30 queries each).
            let mut built = std::collections::HashMap::new();
            if kind==1 {
                if heroes.len() < constant(s,"MatchingBanPickHeroCountMin",12) as usize {return Err(rule("NoHeroes"));}
                for id in &heroes {
                    let h=cached_hero(db,a,*id).await?;
                    if n(&h,"Level")<constant(s,"MatchingBanPickHeroLevelMin",1) || n(&h,"Star")<constant(s,"MatchingBanPickHeroStarMin",1) {return Err(rule("InvalidHero"));}
                    built.insert(*id,h);
                }
                // Friendly duels never consume or require Honor tickets.
                if duel==0 && n(&match_tickets(db,s,a,kind,0).await?,"NewValue")<1 {return Err(rule("NotEnoughSword"));}
            }
            let leader = int(r, "LeaderHeroIndex")?;
            if kind==0 && leader > 0 && !heroes.contains(&leader) {
                return Err(rule("InvalidHero"));
            }
            let active = sqlx::query(
                "SELECT data FROM arena_runs WHERE account=? AND status IN ('waiting','battle')",
            )
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
            if let Some(active) = active {
                let data: Value = parse(&active.get::<String, _>("data"))?;
                if data["Heroes"] == json!(heroes) && n(&data, "Leader") == leader && n(&data,"DuelType")==duel && n(&data,"GroupId")==group {
                    return Ok(data["Register"].clone());
                }
                return Err(rule("WaitMore"));
            }
            super::ensure_available(db, a, &heroes).await?;
            let mut mine = account_with(db, a, &heroes, false, built).await?;
            mine["LeaderHeroIndex"] = json!(if leader == 0 { heroes[0] } else { leader });
            mine["ArenaType"]=json!(if kind==1{"BanPick"}else{"Normal"});
            mine["BattleInfo"] = battle_info_kind(db, s, a,kind).await?;
            mine["Rank"] = mine["BattleInfo"]["Rank"].clone();
            mine["TierIndex"] = mine["BattleInfo"]["TierIndex"].clone();
            out["AccountInfo"] = mine;
            out["SwordResult"] = match_tickets(db, s, a,kind,0).await?;
            // Honor registers the eligible collection, not a saved party deck.
            // Applying it as DeckInfo would replace the client's four-hero deck.
            out["DeckInfo"] = if kind==1 {Value::Null} else {
                json!({"HeroIndices":heroes,"LeaderHeroIndex":if leader==0{heroes[0]}else{leader}})
            };
            let data = json!({"Kind":kind,"DuelType":duel,"GroupId":group,"InvitedOpponent":invited,"Heroes":heroes,"Leader":leader,"Season":season(s).0,"Register":out,"Expires":now()+settings(s,"ArenaMatchExpirySeconds",900)});
            sqlx::query("INSERT INTO arena_runs(account,kind,started,status,data,last_poll) VALUES(?,?,?,'waiting',?,?)").bind(a).bind(kind).bind(now()).bind(data.to_string()).bind(now()).execute(db).await?;
        }
        "cancel_match" => {
            if online::cancel_account(db, s, a).await? {
                out["SwordResult"] = match_tickets(db, s, a,kind,0).await?;
                return Ok(out);
            }
            sqlx::query("UPDATE arena_runs SET status='canceled' WHERE account=? AND status IN ('waiting','battle')").bind(a).execute(&mut *db).await?;
            out["SwordResult"] = match_tickets(db, s, a,kind,0).await?;
        }
        "wait_match" => {
            let run = sqlx::query(
                "SELECT * FROM arena_runs WHERE account=? AND status IN ('waiting','battle')",
            )
            .bind(a)
            .fetch_optional(&mut *db)
            .await?;
            let Some(run)=run else {
                let last:Option<(String,String)>=sqlx::query_as("SELECT status,data FROM arena_runs WHERE account=? AND kind=? ORDER BY id DESC LIMIT 1")
                    .bind(a).bind(kind).fetch_optional(&mut *db).await?;
                if let Some((status,raw))=last {
                    if status=="canceled" && parse::<Value>(&raw)?["QueueCancelReason"].is_string() {
                        out["Result"]=json!("Canceled");
                        out["SwordResult"]=match_tickets(db,s,a,kind,0).await?;
                        return Ok(out);
                    }
                }
                return Err(rule("WaiterNotFound"));
            };
            let id = run.get::<i64, _>("id");
            let mut data: Value = parse(&run.get::<String, _>("data"))?;
            // Pairing wins over a simultaneous bot fallback/cancel poll.
            if data["OnlineMatchUid"].is_i64() {
                return Ok(data["Wait"].clone());
            }
            if flag(r, "ReserveCancel")? {
                sqlx::query("UPDATE arena_runs SET status='canceled' WHERE id=?")
                    .bind(id)
                    .execute(&mut *db)
                    .await?;
                out["Result"] = json!("Canceled");
                out["SwordResult"] = match_tickets(db, s, a,kind,0).await?;
                return Ok(out);
            }
            if run.get::<String, _>("status") == "battle" {
                return Ok(data["Wait"].clone());
            }
            sqlx::query("UPDATE arena_runs SET last_poll=? WHERE id=?")
                .bind(now()).bind(id).execute(&mut *db).await?;
            if let Some(matched) = online::pair(db, s, a, id, &data).await? {
                return Ok(matched);
            }
            if kind==1 || n(&data,"DuelType")==1 || !flag(r, "PlayOfflineMatch")? {
                out["Result"] = json!("WaitMore");
                return Ok(out);
            }
            let selected:Option<i64>=sqlx::query_scalar("SELECT account_id FROM heroes WHERE account_id!=? GROUP BY account_id ORDER BY RANDOM() LIMIT 1").bind(a).fetch_optional(&mut *db).await?;
            let opponent = selected.unwrap_or(a);
            let heroes=defense_heroes(db,opponent).await?;
            out["Result"] = json!("WaitMore");
            out["MatchedNpcInfo"] = account(db, opponent, &heroes, true).await?;
            if opponent == a {
                out["MatchedNpcInfo"]["UserInfo"]["AccountId"] = json!(-a);
            }
            // The native battle-end callback reads this snapshot when saving
            // the opponent's score and tier in the arena replay.
            let opponent_battle = battle_info(db, s, opponent).await?;
            out["MatchedNpcInfo"]["TierIndex"] = opponent_battle["TierIndex"].clone();
            out["MatchedNpcInfo"]["BattleInfo"] = opponent_battle;
            out["ChapterIndex"] = json!(1000);
            out["DungeonIndex"] = json!(1);
            out["SwordResult"] = tickets(db, s, a, "Sword", -1).await?;
            out["BattleInfo"] = battle_info(db, s, a).await?;
            data["Wait"] = out.clone();
            data["Opponent"] = json!(opponent);
            data["Started"] = json!(now());
            sqlx::query("UPDATE arena_runs SET status='battle',data=? WHERE id=?")
                .bind(data.to_string())
                .bind(id)
                .execute(db)
                .await?;
        }
        "set_offline_match_result" => {
            out = complete_match(db,s,a,r,trusted).await?;
        }
        "get_match_result" => {
            let id = r.number("MatchUid", 0)?;
            let data: Option<String> = sqlx::query_scalar(
                "SELECT data FROM arena_runs WHERE (id=? OR json_extract(data,'$.OnlineMatchUid')=?) AND +account=? AND status='complete' ORDER BY id DESC LIMIT 1",
            )
            .bind(id)
            .bind(id)
            .bind(a)
            .fetch_optional(db)
            .await?;
            let data: Value = parse(&data.ok_or_else(|| rule("MatchResultNotFound"))?)?;
            out = data["End"].clone();
            out["CurrencyResults"] = json!([]);
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    Ok(out)
}
async fn rank_mail(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    rank: i64,
    kind: &str,
    period: &str,
) -> Result<()> {
    let inserted = sqlx::query(
        "INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,?,0,?)",
    )
    .bind(a)
    .bind(kind)
    .bind(period)
    .execute(&mut *db)
    .await?
    .rows_affected();
    if inserted == 0 {
        return Ok(());
    }
    let def = s
        .tables
        .arena_guild
        .rows("MatchReward")
        .iter()
        .find(|v| rank >= n(v, "MinRank") && rank <= n(v, "MaxRank"))
        .ok_or_else(|| rule("ItemDataNotFound"))?;
    let daily = kind == "arena_daily_reward";
    let amount = n(def, if daily { "DailyReward" } else { "SeasonReward" });
    let currency = s.tables.arena_guild.rules[if daily {
        "ArenaDailyRewardCurrency"
    } else {
        "ArenaSeasonRewardCurrency"
    }]
    .as_str()
    .unwrap_or(if daily { "PvpCoin" } else { "Gem" });
    if !matches!(currency, "PvpCoin" | "Gem" | "Gold") {
        return Err(rule("InvalidCost"));
    }
    let mut items = vec![];
    let code = def[if daily {
        "DailyRewardExpItemCode"
    } else {
        "SeasonRewardExpItemCode"
    }]
    .as_str()
    .unwrap_or("");
    let count = n(
        def,
        if daily {
            "DailyRewardExpItemCount"
        } else {
            "SeasonRewardExpItemCount"
        },
    );
    if count > 0 {
        let id = s
            .tables
            .get_item_index(code)
            .ok_or_else(|| rule("ItemDataNotFound"))?;
        if !crate::models::item::disabled_reward(id) {
            items.push(json!({"ItemIndex":id,"ItemCount":count}));
        }
    }
    let currencies = if amount > 0 {
        json!([{"CurrencyType":currency,"Amount":amount}])
    } else {
        json!([])
    };
    sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_items,reward_currencies,expires_at) VALUES(?,'Arena',?,?,?, ?,?)").bind(a).bind(if daily{"Arena daily reward"}else{"Arena season reward"}).bind(format!("Rank {rank}, period {period}")).bind(json!(items).to_string()).bind(currencies.to_string()).bind(time(now()+72*3600)).execute(db).await?;
    Ok(())
}
pub(super) async fn pending(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<bool> {
    if honor_rewards::pending(db, s, a).await? || !daily_rewards::due(db, s, a, now()).await?.is_empty() {
        return Ok(true);
    }
    Ok(sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM arena_scores q WHERE account=? AND kind=0 AND season<? AND wins+losses>0 AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=q.account AND c.kind='arena_season_reward' AND c.period=CAST(q.season AS TEXT)))")
        .bind(a).bind(season(s).0).fetch_one(db).await?)
}
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    honor_rewards::settle(db, s, a).await?;
    for (day, rank) in daily_rewards::due(db, s, a, now()).await? {
        rank_mail(
            db,
            s,
            a,
            rank,
            "arena_daily_reward",
            &day,
        )
        .await?;
    }
    let seasons:Vec<i64>=sqlx::query_scalar("SELECT season FROM arena_scores q WHERE account=? AND kind=0 AND season<? AND wins+losses>0 AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=q.account AND c.kind='arena_season_reward' AND c.period=CAST(q.season AS TEXT)) ORDER BY season LIMIT 100").bind(a).bind(season(s).0).fetch_all(&mut *db).await?;
    for old in seasons {
        let rank = ranked(db, s, old, 0, a)
            .await?
            .map(|v| n(&v, "Rank") + 1)
            .ok_or_else(|| rule("MatchResultNotFound"))?;
        rank_mail(db, s, a, rank, "arena_season_reward", &old.to_string()).await?;
    }
    Ok(())
}

async fn complete_match(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request,trusted:bool)->Result<Value> {
    let kind=int(r,"ArenaType")?;
    if !(0..=1).contains(&kind) {return Err(rule("ContentsDisabled"));}
    let mut out=item::success();
    let run = sqlx::query("SELECT * FROM arena_runs WHERE account=? AND status='battle'")
        .bind(a)
        .fetch_optional(&mut *db)
        .await?
        .ok_or_else(|| rule("MatchingAborted"))?;
    let id = run.get::<i64, _>("id");
    if run.get::<i64,_>("kind")!=kind {return Err(rule("MatchingAborted"));}
    let mut data: Value = parse(&run.get::<String, _>("data"))?;
    if data["OnlineMatchUid"].is_null() && run.get::<i64,_>("started") < now()-settings(s,"ArenaMatchExpirySeconds",900) {
        return Err(rule("MatchingAborted"));
    }
    if !trusted && (data["ServiceOwned"]==true || s.tables.services.rules["RequireBattleService"]==true){return Err(rule("MatchingAborted"));}
    if n(&data, "Season") != season(s).0 {
        return Err(rule("MatchingAborted"));
    }
    let win = int(r, "Win")?;
    if win > 1 {
        return Err(rule("InvalidValue"));
    }
    let _ = int(r, "PlayTime")?;
    let alive = ids(r, "AliveHeroIndices", 4)?;
    let heroes: Vec<i64> =
        serde_json::from_value(data["Heroes"].clone()).map_err(|_| rule("InvalidHero"))?;
    if alive.iter().any(|h| !heroes.contains(h)) {
        return Err(rule("InvalidHero"));
    }
    let previous = battle_info_kind(db, s, a,kind).await?;
    // Online results identify the shared match and record a forfeit. Only
    // the battle service settles online runs (they are ServiceOwned), so the
    // final End is written once here rather than patched afterwards.
    let online = data["OnlineMatchUid"].as_i64();
    let give_up = trusted && online.is_some() && r.text("GiveUp") == "1";
    // The shared MatchInfo repeats both players' registrations (~1 MB for
    // Honor) and is only served while the run waits or fights. Completed runs
    // keep their own Register, Heroes and End.
    if online.is_some() {
        if let Some(info) = data.pointer_mut("/Wait/MatchInfo").and_then(Value::as_object_mut) {
            info.insert("AccountInfos".into(), json!([]));
        }
    }
    if n(&data,"DuelType")==1 {
        out["MatchResult"]=json!({"MatchUid":n(&data,"OnlineMatchUid"),"ArenaType":if kind==1{"BanPick"}else{"Normal"},"Win":win==1,"GiveUp":give_up,"Canceled":false,"GainedMatchScore":0,"NewMatchScore":previous["MatchScore"],"NewTierIndex":previous["TierIndex"],"NewTierRank":previous["Rank"],"NewTotalRank":previous["Rank"],"PrevTierIndex":previous["TierIndex"],"PrevTierRank":previous["Rank"],"PrevTotalRank":previous["Rank"],"CurrencyResults":[],"ItemResults":[],"HeroExpResults":[],"SwordResult":match_tickets(db,s,a,kind,0).await?});
        out[if kind==1{"BattleBanPickInfo"}else{"BattleInfo"}]=previous;
        data["End"]=out.clone();data["CompletedAt"]=json!(now());
        sqlx::query("UPDATE arena_runs SET status='complete',data=? WHERE id=?").bind(data.to_string()).bind(id).execute(db).await?;
        return Ok(out);
    }
    let delta = if win == 1 {
        settings(s, "ArenaWinScore", 20)
    } else {
        -settings(s, "ArenaLossScore", 10)
    };
    let score = (n(&previous, "MatchScore") + delta).clamp(0, i32::MAX as i64);
    let delta = score - n(&previous, "MatchScore");
    let details = json!({"SuccessiveWin":if win==1{n(&previous,"SuccessiveWin")+1}else{0},"SuccessiveLose":if win==0{n(&previous,"SuccessiveLose")+1}else{0},"LastMatchTime":time(now())});
    sqlx::query("UPDATE arena_scores SET score=?,wins=wins+?,losses=losses+?,data=? WHERE account=? AND kind=? AND season=?").bind(score).bind(win).bind(1-win).bind(details.to_string()).bind(a).bind(kind).bind(season(s).0).execute(&mut *db).await?;
    let currency = hero::currency(
        db,
        a,
        if kind==1 { "GloryPoint" } else { "PvpCoin" },
        if kind==1 {
            settings(s,"HonorAttendanceMedals",10) + if win==1 {settings(s,"HonorWinBonusMedals",40)} else {0}
        } else { settings(
            s,
            if win == 1 {
                "ArenaWinCoins"
            } else {
                "ArenaLossCoins"
            },
            10,
        ) },
    )
    .await?;
    let current = battle_info_kind(db, s, a,kind).await?;
    if kind==0 {crate::api::progression::play_record::best(db,a,"arena_tier",n(&current,"TierIndex")).await?;}
    if kind==1 {honor_rewards::record(db,a,&current,now()).await?;}
    out["MatchResult"] = json!({"MatchUid":online.unwrap_or(id),"ArenaType":if kind==1{"BanPick"}else{"Normal"},"Win":win==1,"GiveUp":give_up,"Canceled":false,"GainedMatchScore":delta,"NewMatchScore":score,"NewTierIndex":current["TierIndex"],"NewTierRank":current["Rank"],"NewTotalRank":current["Rank"],"PrevTierIndex":previous["TierIndex"],"PrevTierRank":previous["Rank"],"PrevTotalRank":previous["Rank"],"ItemResults":[],"HeroExpResults":[],"SwordResult":match_tickets(db,s,a,kind,0).await?,"CurrencyResults":[currency]});
    out[if kind==1{"BattleBanPickInfo"}else{"BattleInfo"}] = current;
    if data["OnlineMatchUid"].is_null() {
        crate::api::services::records::arena_history(db,s,a,kind,
            &[data["Register"]["AccountInfo"].clone(),data["Wait"]["MatchedNpcInfo"].clone()],
            &[a],win==1,int(r,"PlayTime")?.saturating_mul(1000),r.text("BattleLogs")).await?;
    }
    data["End"] = out.clone();
    data["CompletedAt"] = json!(now());
    sqlx::query("UPDATE arena_runs SET status='complete',data=? WHERE id=?")
        .bind(data.to_string())
        .bind(id)
        .execute(&mut *db)
        .await?;
    if kind==0 {put(
        db,
        a,
        "arena_daily",
        now()/86400,
        // Preserve the existing one-based daily reward snapshot format.
        &json!({"Day":day(),"TierIndex":out["BattleInfo"]["TierIndex"],"Rank":n(&out["BattleInfo"], "Rank") + 1}),
    )
    .await?;}
    Ok(out)
}

#[cfg(test)]
mod ranking_tests {
    use super::*;
    use crate::api::community::tests::{login, setup};

    #[tokio::test]
    async fn single_rows_pages_and_counts_match_the_full_leaderboard() {
        let s = setup().await;
        let season = season(&s).0;
        let mut accounts = vec![];
        for i in 0..7 {
            let u = login(&s, &format!("ranked-{i}")).await;
            accounts.push(n(&u["UserInfo"], "AccountId"));
        }
        // Score ties broken by wins, then by account (login enrolls the last
        // account at the initial score).
        let standings = [(1200, 3), (1100, 5), (1100, 5), (1100, 2), (1000, 9), (1000, 9)];
        for kind in [0, 1] {
            for (a, (score, wins)) in accounts.iter().zip(standings) {
                sqlx::query("INSERT OR REPLACE INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,?,?,?,?,1)")
                    .bind(a).bind(kind).bind(season).bind(score).bind(wins).execute(&s.db).await.unwrap();
            }
        }
        // Formations: a one-Hero squad, and a squad whose Hero 2 is missing from
        // the registration snapshot (falls back to the owned Hero).
        let snapshot = json!({"1":{"Star":5,"Level":100},"3":{"Star":3,"Level":80}});
        let owned = 50_i64;
        sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level) VALUES(?,?,?,4,77)")
            .bind(accounts[2]).bind(owned).bind(owned).execute(&s.db).await.unwrap();
        for (a, heroes) in [(accounts[0], json!([3])), (accounts[2], json!([1,owned,1,3]))] {
            let data = json!({"Season":season,"DuelType":0,"Heroes":heroes,"Register":{"AccountInfo":{"HeroInfos":snapshot}}});
            sqlx::query("INSERT INTO arena_runs(account,kind,status,started,data) VALUES(?,1,'complete',?,?)")
                .bind(a).bind(now()).bind(data.to_string()).execute(&s.db).await.unwrap();
        }
        let mut db = s.db.acquire().await.unwrap();
        for kind in [0, 1] {
            let full = ranking_kind(&mut db, &s, season, kind).await.unwrap();
            assert!(full.len() >= 6);
            assert_eq!(ranked_count(&mut db, season, kind).await.unwrap(), full.len() as i64);
            for a in &accounts {
                let expected = full.iter().find(|v| n(v, "AccountId") == *a).cloned();
                assert_eq!(ranked(&mut db, &s, season, kind, *a).await.unwrap(), expected, "account {a}");
            }
            for (from, to) in [(0, 99), (1, 3), (5, 5), (6, 9)] {
                let page = ranking_page(&mut db, &s, season, kind, from, to).await.unwrap();
                let expected: Vec<Value> = full.iter().filter(|v| (from..=to).contains(&n(v, "Rank"))).cloned().collect();
                assert_eq!(page, expected, "page {from}..={to}");
            }
        }
        let honor = ranked(&mut db, &s, season, 1, accounts[0]).await.unwrap().unwrap();
        assert_eq!(honor["HeroInfos"], json!([{"Index":3,"Star":3,"Level":80}]));
        let fallback = ranked(&mut db, &s, season, 1, accounts[2]).await.unwrap().unwrap();
        let ids: Vec<Value> = fallback["HeroInfos"].as_array().unwrap().iter().map(|h| h["Index"].clone()).collect();
        assert_eq!(ids, vec![json!(1), json!(owned), json!(3)]);
        assert_eq!(fallback["HeroInfos"][0]["Star"], 5);
        let (star, level): (i64, i64) = sqlx::query_as("SELECT star,level FROM heroes WHERE account_id=? AND hero_index=?")
            .bind(accounts[2]).bind(owned).fetch_one(&mut *db).await.unwrap();
        assert_eq!((fallback["HeroInfos"][1]["Star"].as_i64(), fallback["HeroInfos"][1]["Level"].as_i64()), (Some(star), Some(level)));
    }
}
