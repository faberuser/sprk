use super::*;
pub(super) fn season(s: &AppState) -> (i64, i64, i64) {
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
    json!({"Index":index,"Name":"Local arena season","Begin":time(start),"End":time(end),"Description":"","SeasonIndex":index,"ShowSeasonIndex":index,"ArenaType":name,"MatchSeasonType":"Regular","IsTakeReward":false,"PlayTimeBegin":"00:00:00","PlayTimeEnd":"23:59:59","LimitedHeroStar":0,"LimitedHeroLevel":0,"LimitedHeroCount":4,"ActiveTier":true})
}
pub(crate) async fn tickets(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: &str,
    amount: i64,
) -> Result<Value> {
    let mut v = get(db, a, "tickets", if kind == "Sword" { 0 } else { 1 }).await?;
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
async fn ensure(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    sqlx::query("INSERT OR IGNORE INTO arena_scores(account,kind,season,score) VALUES(?,0,?,?)")
        .bind(a)
        .bind(season(s).0)
        .bind(constant(s, "InitialMatchScore", 1000))
        .execute(db)
        .await?;
    Ok(())
}
fn tier(s: &AppState, rank: i64, score: i64) -> i64 {
    s.tables
        .arena_guild
        .rows("MatchTier")
        .iter()
        .find(|v| {
            rank >= n(v, "MinRank")
                && (n(v, "MaxRank") == 0 || rank <= n(v, "MaxRank"))
                && score >= n(v, "MinScore")
                && score <= n(v, "MaxScore")
        })
        .map(|v| n(v, "Index"))
        .unwrap_or_else(|| {
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
    let rows=sqlx::query("SELECT account,score,wins,losses FROM arena_scores WHERE kind=0 AND season=? ORDER BY score DESC,wins DESC,account LIMIT 10000").bind(season).fetch_all(&mut *db).await?;
    let mut out = vec![];
    for (i, r) in rows.iter().enumerate() {
        let mut u = user(db, r.get("account")).await?;
        let score = r.get::<i64, _>("score");
        let rank = i as i64 + 1;
        merge(
            &mut u,
            json!({"Rank":rank,"TotalRank":rank,"TierRank":rank,"TierIndex":tier(s,rank,score),"MatchScore":score,"SeasonWin":r.get::<i64,_>("wins"),"SeasonLose":r.get::<i64,_>("losses"),"ServerGroup":"local","CountryCode":"US"}),
        );
        u["UserInfo"] = u.clone();
        u["HeroInfos"] = json!([]);
        out.push(u);
    }
    Ok(out)
}
pub(super) async fn battle_info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    ensure(db, s, a).await?;
    let rank = ranking(db, s, season(s).0)
        .await?
        .into_iter()
        .find(|v| n(v, "AccountId") == a)
        .ok_or_else(|| rule("MatchResultNotFound"))?;
    let data: String =
        sqlx::query_scalar("SELECT data FROM arena_scores WHERE account=? AND kind=0 AND season=?")
            .bind(a)
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
    execute_inner(db,s,a,r,"set_offline_match_result",true).await
}
async fn execute_inner(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
    trusted: bool,
) -> Result<Value> {
    let kind = int(r, "ArenaType")?;
    if action == "get_season_info" {
        if kind == 5 {
            return Ok(json!({"SeasonData":super::super::battle::match_calendar(s)?}));
        }
        return Ok(json!({"SeasonData":season_info(s,kind)}));
    }
    if kind != 0 {
        return Err(rule("ContentsDisabled"));
    }
    ensure(db, s, a).await?;
    sqlx::query("UPDATE arena_runs SET status='expired' WHERE account=? AND status IN ('waiting','battle') AND started<?").bind(a).bind(now()-settings(s,"ArenaMatchExpirySeconds",900)).execute(&mut *db).await?;
    let mut out = item::success();
    match action {
        "get_match_rank" => {
            let target = r.number("RankerAccountId", a)?;
            let target = if target == 0 { a } else { target };
            let ranks = ranking(db, s, season(s).0).await?;
            let rank = ranks.iter().find(|v| n(v, "AccountId") == target);
            out["BattleInfo"] = if target == a {
                battle_info(db, s, a).await?
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
            let ranks = ranking(db, s, old)
                .await?
                .into_iter()
                .filter(|v| n(v, "Rank") - 1 >= from && n(v, "Rank") - 1 <= to)
                .collect::<Vec<_>>();
            out[if action == "get_match_ranker" {
                "MatchRankers"
            } else {
                "Rankers"
            }] = json!(ranks);
        }
        "get_server_group_ranker" => {
            let score: i64 = sqlx::query_scalar(
                "SELECT COALESCE(SUM(score),0) FROM arena_scores WHERE kind=0 AND season=?",
            )
            .bind(season(s).0)
            .fetch_one(db)
            .await?;
            out["Rankers"] = json!([{"ServerGroup":"local","Rank":1,"Score":score}]);
        }
        "register_match" => {
            if int(r, "DuelType")? != 0 {
                return Err(rule("ContentsDisabled"));
            }
            if n(&user(db, a).await?, "TeamLevel") < constant(s, "ArenaOpenTeamLevel", 10) {
                return Err(rule("ContentsDisabled"));
            }
            let heroes = ids(r, "HeroIndices", 4)?;
            owned(db, a, &heroes).await?;
            let leader = int(r, "LeaderHeroIndex")?;
            if leader > 0 && !heroes.contains(&leader) {
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
            mine["BattleInfo"] = battle_info(db, s, a).await?;
            out["AccountInfo"] = mine;
            out["SwordResult"] = tickets(db, s, a, "Sword", 0).await?;
            out["DeckInfo"] =
                json!({"HeroIndices":heroes,"LeaderHeroIndex":if leader==0{heroes[0]}else{leader}});
            let data = json!({"Heroes":heroes,"Leader":leader,"Season":season(s).0,"Register":out,"Expires":now()+settings(s,"ArenaMatchExpirySeconds",900)});
            sqlx::query("INSERT INTO arena_runs(account,kind,started,status,data) VALUES(?,0,?,'waiting',?)").bind(a).bind(now()).bind(data.to_string()).execute(db).await?;
        }
        "cancel_match" => {
            sqlx::query("UPDATE arena_runs SET status='canceled' WHERE account=? AND status IN ('waiting','battle')").bind(a).execute(&mut *db).await?;
            out["SwordResult"] = tickets(db, s, a, "Sword", 0).await?;
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
            if flag(r, "ReserveCancel")? {
                sqlx::query("UPDATE arena_runs SET status='canceled' WHERE id=?")
                    .bind(id)
                    .execute(&mut *db)
                    .await?;
                out["Result"] = json!("Canceled");
                out["SwordResult"] = tickets(db, s, a, "Sword", 0).await?;
                return Ok(out);
            }
            if !flag(r, "PlayOfflineMatch")? {
                out["Result"] = json!("WaitMore");
                return Ok(out);
            }
            if run.get::<String, _>("status") == "battle" {
                return Ok(data["Wait"].clone());
            }
            let selected:Option<i64>=sqlx::query_scalar("SELECT account_id FROM heroes WHERE account_id!=? GROUP BY account_id ORDER BY RANDOM() LIMIT 1").bind(a).fetch_optional(&mut *db).await?;
            let opponent = selected.unwrap_or(a);
            let heroes:Vec<i64>=sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? ORDER BY level DESC,hero_index LIMIT 4").bind(opponent).fetch_all(&mut *db).await?;
            out["Result"] = json!("WaitMore");
            out["MatchedNpcInfo"] = account(db, opponent, &heroes, true).await?;
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
            let run = sqlx::query("SELECT * FROM arena_runs WHERE account=? AND status='battle'")
                .bind(a)
                .fetch_optional(&mut *db)
                .await?
                .ok_or_else(|| rule("MatchingAborted"))?;
            let id = run.get::<i64, _>("id");
            let mut data: Value = parse(&run.get::<String, _>("data"))?;
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
            let previous = battle_info(db, s, a).await?;
            let delta = if win == 1 {
                settings(s, "ArenaWinScore", 20)
            } else {
                -settings(s, "ArenaLossScore", 10)
            };
            let score = (n(&previous, "MatchScore") + delta).clamp(0, i32::MAX as i64);
            let delta = score - n(&previous, "MatchScore");
            let details = json!({"SuccessiveWin":if win==1{n(&previous,"SuccessiveWin")+1}else{0},"SuccessiveLose":if win==0{n(&previous,"SuccessiveLose")+1}else{0},"LastMatchTime":time(now())});
            sqlx::query("UPDATE arena_scores SET score=?,wins=wins+?,losses=losses+?,data=? WHERE account=? AND kind=0 AND season=?").bind(score).bind(win).bind(1-win).bind(details.to_string()).bind(a).bind(season(s).0).execute(&mut *db).await?;
            let currency = hero::currency(
                db,
                a,
                "PvpCoin",
                settings(
                    s,
                    if win == 1 {
                        "ArenaWinCoins"
                    } else {
                        "ArenaLossCoins"
                    },
                    10,
                ),
            )
            .await?;
            let current = battle_info(db, s, a).await?;
            out["MatchResult"] = json!({"MatchUid":id,"ArenaType":"Normal","Win":win==1,"GiveUp":false,"Canceled":false,"GainedMatchScore":delta,"NewMatchScore":score,"NewTierIndex":current["TierIndex"],"NewTierRank":current["Rank"],"NewTotalRank":current["Rank"],"PrevTierIndex":previous["TierIndex"],"PrevTierRank":previous["Rank"],"PrevTotalRank":previous["Rank"],"ItemResults":[],"HeroExpResults":[],"SwordResult":tickets(db,s,a,"Sword",0).await?,"CurrencyResults":[currency]});
            out["BattleInfo"] = current;
            data["End"] = out.clone();
            sqlx::query("UPDATE arena_runs SET status='complete',data=? WHERE id=?")
                .bind(data.to_string())
                .bind(id)
                .execute(&mut *db)
                .await?;
            put(
                db,
                a,
                "arena_daily",
                now()/86400,
                &json!({"Day":day(),"TierIndex":out["BattleInfo"]["TierIndex"],"Rank":out["BattleInfo"]["Rank"]}),
            )
            .await?;
        }
        "get_match_result" => {
            let id = r.number("MatchUid", 0)?;
            let data: Option<String> = sqlx::query_scalar(
                "SELECT data FROM arena_runs WHERE id=? AND account=? AND status='complete'",
            )
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
        items.push(json!({"ItemIndex":id,"ItemCount":count}));
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
            .map(|v| n(&v, "Rank"))
            .ok_or_else(|| rule("MatchResultNotFound"))?;
        rank_mail(db, s, a, rank, "arena_season_reward", &old.to_string()).await?;
    }
    Ok(())
}
