use super::*;
#[path = "arena_online.rs"]
pub(crate) mod online;
#[path = "arena_honor_rewards.rs"]
pub(crate) mod honor_rewards;
pub(crate) fn season(s: &AppState) -> (i64, i64, i64) {
    let epoch = s.tables.arena_guild.rules["SeasonEpoch"]
        .as_str()
        .and_then(|v| chrono::NaiveDateTime::parse_from_str(v, "%Y-%m-%d %H:%M:%S").ok())
        .map(|v| v.and_utc().timestamp())
        .unwrap_or(1767571200);
    let length = settings(s, "SeasonDays", 7).clamp(1, 365) * 86400;
    let index = (now() - epoch).max(0) / length + 1;
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
                .filter(|v| *v<=i32::MAX as i64).ok_or_else(||rule("MaxStamina"))?;
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
        .filter(|v| *v >= 0 && *v <= i32::MAX as i64)
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
    for id in heroes {
        if sqlx::query_scalar::<_, i64>(
            "SELECT count(*) FROM heroes WHERE account_id=? AND hero_index=?",
        )
        .bind(a)
        .bind(id)
        .fetch_one(&mut *db)
        .await?
            != 1
        {
            return Err(rule("InvalidHero"));
        }
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
    let u = user(db, a).await?;
    let mut map = serde_json::Map::new();
    for id in heroes {
        map.insert(id.to_string(), cached_hero(db, a, *id).await?);
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
pub(super) async fn ranking(
    db: &mut SqliteConnection,
    s: &AppState,
    season: i64,
) -> Result<Vec<Value>> {
    ranking_kind(db, s, season, 0).await
}
async fn ranking_kind(db: &mut SqliteConnection, s: &AppState, season: i64, kind: i64) -> Result<Vec<Value>> {
    let rows=sqlx::query("SELECT account,score,wins,losses FROM arena_scores WHERE kind=? AND season=? ORDER BY score DESC,wins DESC,account LIMIT 10000").bind(kind).bind(season).fetch_all(&mut *db).await?;
    let mut out = vec![];
    for (i, r) in rows.iter().enumerate() {
        let mut u = user(db, r.get("account")).await?;
        let score = r.get::<i64, _>("score");
        // Native Arena ranks are zero-based; table tiers and rewards are one-based.
        let rank = i as i64;
        merge(
            &mut u,
            json!({"Rank":rank,"TotalRank":rank,"TierRank":rank,"TierIndex":tier(s,kind,rank+1,score),"MatchScore":score,"SeasonWin":r.get::<i64,_>("wins"),"SeasonLose":r.get::<i64,_>("losses"),"ServerGroup":"local","CountryCode":"US"}),
        );
        u["UserInfo"] = u.clone();
        u["HeroInfos"] = json!([]);
        out.push(u);
    }
    Ok(out)
}
pub(super) async fn battle_info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    battle_info_kind(db, s, a, 0).await
}
pub(crate) async fn battle_info_kind(db: &mut SqliteConnection, s: &AppState, a: i64, kind: i64) -> Result<Value> {
    ensure_kind(db, s, a, kind).await?;
    let rank = ranking_kind(db, s, season(s).0, kind)
        .await?
        .into_iter()
        .find(|v| n(v, "AccountId") == a)
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
        sqlx::query_scalar::<_,i64>("SELECT kind FROM arena_runs WHERE account=? AND ((?=0 AND status IN ('waiting','battle')) OR (?!=0 AND (id=? OR json_extract(data,'$.OnlineMatchUid')=?))) ORDER BY id DESC LIMIT 1")
            .bind(a).bind(uid).bind(uid).bind(uid).bind(uid).fetch_optional(&mut *db).await?.unwrap_or(int(r,"ArenaType")?)
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
            let ranks = ranking_kind(db, s, season(s).0, kind).await?;
            let rank = ranks.iter().find(|v| n(v, "AccountId") == target);
            out[if kind == 1 { "BattleBanPickInfo" } else { "BattleInfo" }] = if target == a {
                battle_info_kind(db, s, a, kind).await?
            } else {
                rank.cloned().ok_or_else(|| rule("MatchResultNotFound"))?
            };
            let position = rank.map(|v| n(v, "Rank")).unwrap_or(0);
            out["RankResult"] =
                json!({"TierRank":position,"TotalRank":position,"TotalRankerCount":ranks.len()});
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
            let ranks = ranking_kind(db, s, old, kind)
                .await?
                .into_iter()
                .filter(|v| n(v, "Rank") >= from && n(v, "Rank") <= to)
                .collect::<Vec<_>>();
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
            if int(r, "DuelType")? != 0 {
                return Err(rule("ContentsDisabled"));
            }
            if n(&user(db, a).await?, "TeamLevel") < constant(s, "ArenaOpenTeamLevel", 10) {
                return Err(rule("ContentsDisabled"));
            }
            let heroes = ids(r, "HeroIndices", if kind==1{200}else{4})?;
            owned(db, a, &heroes).await?;
            if kind==1 {
                if heroes.len() < constant(s,"MatchingBanPickHeroCountMin",12) as usize {return Err(rule("NoHeroes"));}
                for id in &heroes {
                    let h=cached_hero(db,a,*id).await?;
                    if n(&h,"Level")<constant(s,"MatchingBanPickHeroLevelMin",1) || n(&h,"Star")<constant(s,"MatchingBanPickHeroStarMin",1) {return Err(rule("InvalidHero"));}
                }
                if n(&match_tickets(db,s,a,kind,0).await?,"NewValue")<1 {return Err(rule("NotEnoughSword"));}
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
                if data["Heroes"] == json!(heroes) && n(&data, "Leader") == leader {
                    return Ok(data["Register"].clone());
                }
                return Err(rule("WaitMore"));
            }
            super::ensure_available(db, a, &heroes).await?;
            let mut mine = account(db, a, &heroes, false).await?;
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
            let data = json!({"Kind":kind,"Heroes":heroes,"Leader":leader,"Season":season(s).0,"Register":out,"LastPoll":now(),"Expires":now()+settings(s,"ArenaMatchExpirySeconds",900)});
            sqlx::query("INSERT INTO arena_runs(account,kind,started,status,data) VALUES(?,?,?,'waiting',?)").bind(a).bind(kind).bind(now()).bind(data.to_string()).execute(db).await?;
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
            .await?
            .ok_or_else(|| rule("WaiterNotFound"))?;
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
            data["LastPoll"] = json!(now());
            sqlx::query("UPDATE arena_runs SET data=? WHERE id=?")
                .bind(data.to_string()).bind(id).execute(&mut *db).await?;
            if let Some(matched) = online::pair(db, s, a, id, &data).await? {
                return Ok(matched);
            }
            if kind==1 || !flag(r, "PlayOfflineMatch")? {
                out["Result"] = json!("WaitMore");
                return Ok(out);
            }
            let selected:Option<i64>=sqlx::query_scalar("SELECT account_id FROM heroes WHERE account_id!=? GROUP BY account_id ORDER BY RANDOM() LIMIT 1").bind(a).fetch_optional(&mut *db).await?;
            let opponent = selected.unwrap_or(a);
            let heroes:Vec<i64>=sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? ORDER BY level DESC,hero_index LIMIT 4").bind(opponent).fetch_all(&mut *db).await?;
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
                "SELECT data FROM arena_runs WHERE (id=? OR json_extract(data,'$.OnlineMatchUid')=?) AND account=? AND status='complete' ORDER BY id DESC LIMIT 1",
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
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    honor_rewards::settle(db, s, a).await?;
    let days=sqlx::query("SELECT idx,data FROM community_state c WHERE owner=? AND kind='arena_daily' AND idx<? AND NOT EXISTS(SELECT 1 FROM community_claims x WHERE x.account=c.owner AND x.kind='arena_daily_reward' AND x.period=json_extract(c.data,'$.Day')) ORDER BY idx LIMIT 500").bind(a).bind(now()/86400).fetch_all(&mut *db).await?;
    for day in days {
        let v: Value = parse(&day.get::<String, _>("data"))?;
        rank_mail(
            db,
            s,
            a,
            n(&v, "Rank"),
            "arena_daily_reward",
            v["Day"].as_str().unwrap_or(""),
        )
        .await?;
    }
    let seasons:Vec<i64>=sqlx::query_scalar("SELECT season FROM arena_scores q WHERE account=? AND kind=0 AND season<? AND wins+losses>0 AND NOT EXISTS(SELECT 1 FROM community_claims c WHERE c.account=q.account AND c.kind='arena_season_reward' AND c.period=CAST(q.season AS TEXT)) ORDER BY season LIMIT 100").bind(a).bind(season(s).0).fetch_all(&mut *db).await?;
    for old in seasons {
        let rank = ranking(db, s, old)
            .await?
            .into_iter()
            .find(|v| n(v, "AccountId") == a)
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
    out["MatchResult"] = json!({"MatchUid":id,"ArenaType":if kind==1{"BanPick"}else{"Normal"},"Win":win==1,"GiveUp":false,"Canceled":false,"GainedMatchScore":delta,"NewMatchScore":score,"NewTierIndex":current["TierIndex"],"NewTierRank":current["Rank"],"NewTotalRank":current["Rank"],"PrevTierIndex":previous["TierIndex"],"PrevTierRank":previous["Rank"],"PrevTotalRank":previous["Rank"],"ItemResults":[],"HeroExpResults":[],"SwordResult":match_tickets(db,s,a,kind,0).await?,"CurrencyResults":[currency]});
    out[if kind==1{"BattleBanPickInfo"}else{"BattleInfo"}] = current;
    data["End"] = out.clone();
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
