use super::*;
// Local seasons contain one guild-war session. Reward eligibility is frozen at pairing.
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let rows=sqlx::query("SELECT owner,idx,data FROM community_state c WHERE kind='guild_arena' AND idx<? AND COALESCE(json_extract(data,'$.Peer'),0)>0 AND EXISTS(SELECT 1 FROM json_each(c.data,'$.Members') WHERE value=?) AND NOT EXISTS(SELECT 1 FROM community_claims x WHERE x.account=? AND x.kind='guild_arena_season_reward' AND x.period=CAST(c.idx AS TEXT)) ORDER BY idx LIMIT 100").bind(arena::season(s).0).bind(a).bind(a).fetch_all(&mut *db).await?;
    for entry in rows {
        let period = entry.get::<i64, _>("idx");
        let g = entry.get::<i64, _>("owner");
        let ranks = rankers(db, s, "arena", period).await?;
        let Some(mine) = ranks.iter().find(|v| n(v, "GuildId") == g) else {
            continue;
        };
        let Some(def) = s
            .tables
            .arena_guild
            .rows("GuildArenaSeasonReward")
            .iter()
            .find(|v| {
                n(v, "TierIndex") == n(mine, "TierIndex")
                    || (n(mine, "TierIndex") >= 80
                        && n(v, "MinRank") > 0
                        && n(mine, "Rank") >= n(v, "MinRank")
                        && n(mine, "Rank") <= n(v, "MaxRank"))
            })
        else {
            continue;
        };
        // One reward per player and season, even if their guild changes.
        let inserted=sqlx::query("INSERT OR IGNORE INTO community_claims(account,kind,target,period) VALUES(?,'guild_arena_season_reward',0,?)").bind(a).bind(period.to_string()).execute(&mut *db).await?.rows_affected();
        if inserted == 0 {
            continue;
        }
        let items = def["RewardItemIndices"]
            .as_array()
            .into_iter()
            .flatten()
            .zip(def["RewardItemCounts"].as_array().into_iter().flatten())
            .map(|(id, count)| json!({"ItemIndex":id,"ItemCount":count}))
            .collect::<Vec<_>>();
        let currencies =
            json!([{"CurrencyType":"GuildArenaPoint","Amount":n(def,"SeasonPointReward")}]);
        sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_gem,reward_items,reward_currencies,expires_at) VALUES(?,'Guild','Guild arena season reward',?,?,?,?,?)").bind(a).bind(format!("Local season {period}, tier {}",n(mine,"TierIndex"))).bind(n(def,"SeasonRubyReward")).bind(json!(items).to_string()).bind(currencies.to_string()).bind(time(now()+7*86400)).execute(&mut *db).await?;
    }
    Ok(())
}
fn session(s: &AppState) -> Value {
    let (index, start, end) = arena::season(s);
    let apply = (start + settings(s, "GuildApplyDays", 2).max(0) * 86400).min(end);
    json!({"SessionIndex":index,"SeasonIndex":index,"SessionNumber":1,"BeginTime":time(start),"EndTime":time(end),"ApplyEndTime":time(apply),"BattleBeginTime":time(apply),"BattleEndTime":time(end),"State":if now()<apply{"Apply"}else{"Battle"},"IsFirstSession":1,"IsLastSession":1,"ApplyEndedTime":if now()>=apply{json!(time(apply))}else{Value::Null},"SessionRewardedTime":null,"CompletedRewardedTime":null})
}
fn valid_session(s: &AppState, r: &Request, key: &str) -> Result<()> {
    let value = r.number(key, arena::season(s).0)?;
    if value != 0 && value != arena::season(s).0 {
        return Err(rule("InvalidGuildArenaSessionIndex"));
    }
    Ok(())
}
async fn guild_info(db: &mut SqliteConnection, s: &AppState, g: i64) -> Result<Value> {
    let mut v = guild::state(db, s, g).await?;
    v["GuildId"] = json!(g);
    v["ServerGroup"] = json!("local");
    v["MemberCount"] = v["CurMember"].clone();
    Ok(v)
}
async fn member_info(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<Value> {
    let mut v = guild::member(db, s, a).await?;
    v["ServerGroup"] = json!("local");
    v["CountryCode"] = json!("US");
    Ok(v)
}
async fn registration(
    db: &mut SqliteConnection,
    s: &AppState,
    g: i64,
    kind: &str,
) -> Result<Value> {
    let index = arena::season(s).0;
    let mut v = get(db, g, kind, index).await?;
    if v.is_null() {
        v = json!({"Applied":false,"Peer":0,"AppliedTime":null});
    }
    let info = guild::state(db, s, g).await?;
    let auto = if kind == "guild_arena" {
        "AutoApply"
    } else {
        "AutoApplySuppress"
    };
    if n(&info, auto) > 0 {
        v["Applied"] = json!(true);
        if v["AppliedTime"].is_null() {
            v["AppliedTime"] = json!(time(now()));
        }
    }
    put(db, g, kind, index, &v).await?;
    Ok(v)
}
async fn pair(db: &mut SqliteConnection, s: &AppState, g: i64) -> Result<i64> {
    if session(s)["State"] != "Battle" {
        return Err(rule("GuildArenaNotBattleState"));
    }
    let index = arena::season(s).0;
    let mut v = registration(db, s, g, "guild_arena").await?;
    if v["Applied"] != true {
        return Err(rule("MatchedGuildArenaEnemyNotFound"));
    }
    if n(&v, "Peer") > 0 {
        return Ok(n(&v, "Peer"));
    }
    let own_decks: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM guild_arena_decks WHERE guild_id=? AND season=?")
            .bind(g)
            .bind(index)
            .fetch_one(&mut *db)
            .await?;
    if own_decks == 0 {
        return Err(rule("GuildArenaDeckHeroNotFound"));
    }
    let candidates:Vec<i64>=sqlx::query_scalar("SELECT c.owner FROM community_state c JOIN guilds g ON g.guild_id=c.owner WHERE c.kind='guild_arena' AND c.idx=? AND c.owner!=? AND json_extract(c.data,'$.Applied')=1 AND COALESCE(json_extract(c.data,'$.Peer'),0)=0 AND EXISTS(SELECT 1 FROM guild_arena_decks d WHERE d.guild_id=c.owner AND d.season=c.idx) ORDER BY c.owner").bind(index).bind(g).fetch_all(&mut *db).await?;
    let peer = candidates
        .into_iter()
        .next()
        .ok_or_else(|| rule("MatchedGuildArenaEnemyNotFound"))?;
    let mut p = registration(db, s, peer, "guild_arena").await?;
    p["Peer"] = json!(g);
    v["Peer"] = json!(peer);
    for (id, info) in [(g, &mut v), (peer, &mut p)] {
        let members: Vec<i64> = sqlx::query_scalar(
            "SELECT account_id FROM guild_members WHERE guild_id=? ORDER BY account_id",
        )
        .bind(id)
        .fetch_all(&mut *db)
        .await?;
        info["Members"] = json!(members);
    }
    put(db, g, "guild_arena", index, &v).await?;
    put(db, peer, "guild_arena", index, &p).await?;
    Ok(peer)
}
async fn decks(db: &mut SqliteConnection, a: i64, season: i64) -> Result<Vec<Value>> {
    let rows: Vec<String> = sqlx::query_scalar(
        "SELECT data FROM guild_arena_decks WHERE account=? AND season=? ORDER BY deck",
    )
    .bind(a)
    .bind(season)
    .fetch_all(db)
    .await?;
    rows.iter().map(|s| parse(s)).collect()
}
async fn member_decks(db: &mut SqliteConnection, s: &AppState, g: i64) -> Result<Vec<Value>> {
    let ids: Vec<i64> = sqlx::query_scalar(
        "SELECT account_id FROM guild_members WHERE guild_id=? ORDER BY account_id",
    )
    .bind(g)
    .fetch_all(&mut *db)
    .await?;
    let mut out = vec![];
    for id in ids {
        let d = decks(db, id, arena::season(s).0).await?;
        let mut m = member_info(db, s, id).await?;
        m["DeckCount"] = json!(d.len());
        m["DefeatDeck"] = json!(d
            .iter()
            .filter(|v| n(v, "IsLose") == 1)
            .map(|v| n(v, "DeckIndex"))
            .collect::<Vec<_>>());
        out.push(m);
    }
    Ok(out)
}
async fn scores(db: &mut SqliteConnection, s: &AppState, g: i64, kind: &str) -> Result<Vec<Value>> {
    let rows=sqlx::query("SELECT account,SUM(score) AS score,COUNT(*) AS battles FROM guild_battle_scores WHERE guild_id=? AND kind=? AND season=? GROUP BY account ORDER BY score DESC,account").bind(g).bind(kind).bind(arena::season(s).0).fetch_all(&mut *db).await?;
    let mut out = vec![];
    for (i, r) in rows.iter().enumerate() {
        let a = r.get::<i64, _>("account");
        let mut u = user(db, a).await?;
        let value = r.get::<i64, _>("score");
        merge(
            &mut u,
            json!({"GuildId":g,"ServerGroup":"local","Score":value,"SessionScore":value,"TotalBattleCount":r.get::<i64,_>("battles"),"ScoreRank":i+1,"WinCount":value,"MatchWinCount":value,"DefenceWinCount":0,"LoseCount":0,"SessionNumber":1}),
        );
        if kind == "arena" {
            let totals=sqlx::query("SELECT COUNT(*) AS battles,COALESCE(SUM(win),0) AS wins FROM guild_arena_records WHERE guild_id=? AND account=? AND season=?").bind(g).bind(a).bind(arena::season(s).0).fetch_one(&mut *db).await?;
            let battles = totals.get::<i64, _>("battles");
            let wins = totals.get::<i64, _>("wins");
            u["TotalBattleCount"] = json!(battles);
            u["WinCount"] = json!(wins);
            u["MatchWinCount"] = json!(wins);
            u["LoseCount"] = json!(battles - wins);
        }
        out.push(u);
    }
    Ok(out)
}
async fn rankers(
    db: &mut SqliteConnection,
    s: &AppState,
    kind: &str,
    period: i64,
) -> Result<Vec<Value>> {
    if kind == "suppress" {
        return Ok(vec![]);
    }
    let rows=sqlx::query("SELECT g.guild_id,COALESCE(SUM(b.score),0) AS score FROM guilds g LEFT JOIN guild_battle_scores b ON b.guild_id=g.guild_id AND b.kind=? AND b.season=? GROUP BY g.guild_id ORDER BY score DESC,g.guild_id LIMIT 10000").bind(kind).bind(period).fetch_all(&mut *db).await?;
    let mut out = vec![];
    for (i, r) in rows.iter().enumerate() {
        let g = r.get::<i64, _>("guild_id");
        let mut v = guild_info(db, s, g).await?;
        let score = r.get::<i64, _>("score");
        let tier = s
            .tables
            .arena_guild
            .rows("GuildArenaTier")
            .iter()
            .find(|v| {
                1000 + score >= n(v, "MinScore")
                    && 1000 + score <= n(v, "MaxScore")
                    && (n(v, "MaxRank") == 0 || i as i64 + 1 <= n(v, "MaxRank"))
            })
            .map(|v| n(v, "Index"))
            .unwrap_or(10);
        merge(
            &mut v,
            json!({"Rank":i+1,"Ranking":i+1,"Score":score,"SeasonScore":1000+score,"SessionScore":score,"SeasonWin":0,"SeasonLose":0,"SeasonDraw":0,"TierIndex":tier,"GuildMembers":[],"SeasonIndex":period,"ViewSeasonIndex":period}),
        );
        out.push(v);
    }
    if kind == "arena" && period < arena::season(s).0 {
        let scores = out
            .iter()
            .map(|v| (n(v, "GuildId"), n(v, "SessionScore")))
            .collect::<std::collections::BTreeMap<_, _>>();
        for v in &mut out {
            let registration = get(db, n(v, "GuildId"), "guild_arena", period).await?;
            if let Some(peer) = scores.get(&n(&registration, "Peer")) {
                let score = n(v, "SessionScore");
                v["SeasonWin"] = json!(if score > *peer { 1 } else { 0 });
                v["SeasonLose"] = json!(if score < *peer { 1 } else { 0 });
                v["SeasonDraw"] = json!(if score == *peer { 1 } else { 0 });
            }
        }
    }
    Ok(out)
}
pub(super) async fn raid_list(
    db: &mut SqliteConnection,
    s: &AppState,
    g: i64,
) -> Result<Vec<Value>> {
    let period = arena::season(s).0;
    let mut out = vec![];
    let chapters = s
        .tables
        .arena_guild
        .rows("GuildRaidDungeon")
        .iter()
        .map(|r| n(r, "ChapterIndex"))
        .collect::<BTreeSet<_>>();
    for c in chapters {
        let mut v = get(db, g, "guild_raid", c).await?;
        if n(&v, "Season") != period {
            let first = s
                .tables
                .arena_guild
                .rows("GuildRaidDungeon")
                .iter()
                .filter(|v| n(v, "ChapterIndex") == c)
                .min_by_key(|v| n(v, "Step"))
                .unwrap();
            v = json!({"GuildId":g,"Season":period,"ChapterIndex":c,"DungeonIndex":n(first,"DungeonIndex"),"Step":n(first,"Step"),"OpenedTime":time(arena::season(s).1),"CompletedTime":null,"ClearMemberId":0,"MonsterHp0":settings(s,"GuildRaidMaxHp",1000000000),"IsOngoing":1,"FinishMode":0,"CurrentChapterIndex":c,"CurrentDungeonIndex":n(first,"DungeonIndex"),"CurrentDungeonStep":n(first,"Step"),"BonusDungeonOpenStep":0,"BonusChapterIndex":0,"BonusDungeonIndex":0,"IsBonusDungeon":0,"IsDummyDungeon":0,"CheckBonusDungeon":false,"CheckDummyDungeon":false,"CheckFinishMode":false});
            put(db, g, "guild_raid", c, &v).await?;
        }
        out.push(v);
    }
    Ok(out)
}
pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    let action = path.rsplit('/').next().unwrap_or("");
    let mut out = item::success();
    let index = arena::season(s).0;
    if path.starts_with("guild_ranking_board/") {
        if int(r, "BoardType")? != 1
            && action != "get_guild_ranking_board_server_total_ranking_list"
        {
            return Err(rule("UnknownType"));
        }
        let ranks = rankers(db, s, "suppress", index).await?;
        let (g, _) = guild::membership(db, a).await?;
        out["ViewSeasonIndex"] = json!(index);
        out["TotalRankerCount"] = json!(ranks.len());
        if action.contains("server_total") {
            let score: i64 = ranks.iter().map(|v| n(v, "Score")).sum();
            let v = json!({"ServerGroup":"local","Rank":1,"Score":score,"SeasonIndex":index});
            if action.contains("list") {
                out["RankingInfos"] = json!([v]);
            } else {
                out["RankInfo"] = v;
            }
        } else if action.contains("ranker_list") {
            let page = int(r, "PageNo")?;
            out["RankerInfos"] = json!(ranks
                .into_iter()
                .skip(page as usize * 100)
                .take(100)
                .collect::<Vec<_>>());
        } else {
            out["RankInfo"] = ranks
                .into_iter()
                .find(|v| n(v, "GuildId") == g)
                .unwrap_or(Value::Null);
        }
        return Ok(out);
    }
    // Portal polls the raid list, then member scores, even without a guild.
    // An empty list is a valid read result; membership remains mandatory for actions.
    if matches!(
        path,
        "guild_raid/get_guild_raid_list" | "guild_raid/get_guild_raid_member_score_list"
    ) {
        let guild_id: Option<i64> =
            sqlx::query_scalar("SELECT guild_id FROM guild_members WHERE account_id=?")
                .bind(a)
                .fetch_optional(&mut *db)
                .await?;
        if action == "get_guild_raid_list" {
            out["GuildRaidInfos"] = match guild_id {
                Some(g) => json!(raid_list(db, s, g).await?),
                None => json!([]),
            };
        } else {
            out["GuildRaidMemberTotalScores"] = match guild_id {
                Some(g) => json!(scores(db, s, g, "raid").await?),
                None => json!([]),
            };
        }
        return Ok(out);
    }
    let (g, role) = guild::membership(db, a).await?;
    if path.starts_with("guild_raid/") {
        match action {
            "get_guild_raid_guild_ranking" => {
                let chapter = int(r, "ChapterIndex")?;
                let difficulty = int(r, "Difficulty")?;
                let rows=sqlx::query("SELECT c.owner,c.data FROM community_state c JOIN guilds g ON g.guild_id=c.owner WHERE c.kind='guild_raid' AND c.idx=? AND json_extract(c.data,'$.Season')=? AND json_extract(c.data,'$.IsOngoing')=0 ORDER BY json_extract(c.data,'$.ClearTime'),c.owner LIMIT 100").bind(chapter).bind(index).fetch_all(&mut *db).await?;
                let mut ranks = vec![];
                for row in rows {
                    let guild = row.get::<i64, _>("owner");
                    let raid: Value = parse(&row.get::<String, _>("data"))?;
                    let info = guild::state(db, s, guild).await?;
                    ranks.push(json!({"ChapterIndex":chapter,"Difficulty":difficulty,"GuildId":guild,"GuildName":info["Name"],"GuildLogo":info["Logo"],"ClearTime":n(&raid,"ClearTime")}));
                }
                out["RankingInfos"] = json!(ranks);
            }
            "ping_guild_raid" => {
                let entry: Option<String> = sqlx::query_scalar(
                    "SELECT entry FROM battle_runs WHERE account=? AND completed=0",
                )
                .bind(a)
                .fetch_optional(db)
                .await?;
                let entry: Value = parse(&entry.ok_or_else(|| rule("CampaignInfoNotFound"))?)?;
                if n(&entry, "GuildId") != g {
                    return Err(rule("NotGuildRaid"));
                }
            }
            "get_guild_raid_all_booty_items" => {
                let rows:Vec<String>=sqlx::query_scalar("SELECT data FROM community_state WHERE owner=? AND kind='guild_booty' AND json_extract(data,'$.Expires')>?").bind(g).bind(now()).fetch_all(db).await?;
                let items = rows
                    .iter()
                    .map(|v| parse::<Value>(v))
                    .collect::<Result<Vec<_>>>()?;
                out["ItemInfos"] = json!(items
                    .iter()
                    .filter(|v| v["Equipment"] != true && n(v, "ItemCount") > 0)
                    .collect::<Vec<_>>());
                out["EquipItemInfos"] = json!(items
                    .iter()
                    .filter(|v| v["Equipment"] == true && n(v, "ItemCount") > 0)
                    .collect::<Vec<_>>());
            }
            "buy_guild_raid_booty_item" => {
                let id = r.number("Id", 0)?;
                let mut booty = get(db, g, "guild_booty", id).await?;
                if booty.is_null() || n(&booty, "Expires") <= now() {
                    return Err(rule("ItemDestroyed"));
                }
                let count = int(r, "Count")?;
                if count <= 0
                    || count > n(&booty, "ItemCount")
                    || int(r, "ItemIndex")? != n(&booty, "ItemIndex")
                    || int(r, "ShopIndex")? != n(&booty, "ShopIndex")
                {
                    return Err(rule("WrongItemCount"));
                }
                out["GuildPointResult"] = hero::currency(
                    db,
                    a,
                    "GuildPoint",
                    -n(&booty, "Price")
                        .checked_mul(count)
                        .ok_or_else(|| rule("WrongItemCount"))?,
                )
                .await?;
                let mut rw = Rewards::default();
                item::give(
                    db,
                    s,
                    a,
                    n(&booty, "ItemIndex") as i32,
                    count as i32,
                    n(&booty, "Star") as i32,
                    0,
                    &mut rw,
                )
                .await?;
                let result = rewards(db, s, a, rw).await?;
                out["ItemResult"] = result["ItemResults"][0].clone();
                out["EquipItemInfo"] = result["EquipItemResults"][0].clone();
                booty["ItemCount"] = json!(n(&booty, "ItemCount") - count);
                booty["Count"] = booty["ItemCount"].clone();
                put(db, g, "guild_booty", id, &booty).await?;
            }
            _ => return Err(rule("ContentsDisabled")),
        }
        return Ok(out);
    }
    let suppress = path.starts_with("guild_suppress/");
    if suppress {
        // RaidIndex 90001 is missing and party combat is unavailable.
        match action {
            "get_guild_suppress_session_info" | "get_guild_suppress_status_board" => {
                out["GuildSuppressSessionInfo"] = json!({"SessionIndex":index,"State":"NotHeld"});
                out["GuildSuppressSeasonInfo"] = json!({"SeasonIndex":index});
                out["GuildSuppressApplied"] = json!(false);
                return Ok(out);
            }
            "get_guild_suppress_member_score_list" | "get_guild_suppress_group_ranker_list" => {
                return Ok(out)
            }
            _ => return Err(rule("ContentsDisabled")),
        }
    }
    let kind = if suppress {
        "guild_suppress"
    } else {
        "guild_arena"
    };
    let mut applied = registration(db, s, g, kind).await?;
    if action.starts_with("auto_apply_") {
        guild::admin(role)?;
        let mut info = guild::state(db, s, g).await?;
        let key = if suppress {
            "AutoApplySuppress"
        } else {
            "AutoApply"
        };
        let enabled = flag(r, "IsApply")?;
        info[key] = json!(if enabled { 1 } else { 0 });
        guild::save(db, g, &info).await?;
        out[key] = info[key].clone();
        return Ok(out);
    }
    if action.starts_with("apply_") {
        guild::admin(role)?;
        valid_session(s, r, "SessionIndex")?;
        if applied["Applied"] == true {
            return Err(rule("AlreadyAppliedGuildArena"));
        }
        if session(s)["State"] != "Apply"
            && s.tables.arena_guild.rules["GuildArenaAllowLateRegistration"] != true
        {
            return Err(rule("NotGuildArenaAppliable"));
        }
        let info = guild::state(db, s, g).await?;
        if n(&info, "CurMember") < settings(s, "GuildArenaMinMembers", 1) {
            return Err(rule("NotGuildArenaAppliable"));
        }
        applied["Applied"] = json!(true);
        applied["AppliedTime"] = json!(time(now()));
        put(db, g, kind, index, &applied).await?;
        out["GuildArenaApplied"] = json!(true);
        return Ok(out);
    }
    match action {
        "get_guild_arena_session_info" => {
            out["SeasonIndex"] = json!(index);
            out["SessionInfo"] = session(s);
            out["GuildArenaApplied"] = applied["Applied"].clone();
            out["AutoApply"] = guild::state(db, s, g).await?["AutoApply"].clone();
            out["SeasonType"] = json!("Regular");
            out["IsMatched"] = json!(n(&applied, "Peer") > 0);
            out["Rankers"] = json!(rankers(db, s, "arena", index).await?);
            out["StaminaResult"] = arena::tickets(db, s, a, "GuildArenaKey", 0).await?;
        }
        "get_guild_arena_deck" => {
            valid_session(s, r, "SeasonIndex")?;
            let target = r.number("MemberId", a)?;
            let enemy = r.number("EnemyAccountId", 0)?;
            let target = if enemy > 0 {
                enemy
            } else if target == 0 {
                a
            } else {
                target
            };
            let (tg, _) = guild::membership(db, target).await?;
            if tg != g && n(&applied, "Peer") != tg {
                return Err(rule("NoGuild"));
            }
            out["DeckInfos"] = json!(decks(db, target, index).await?);
        }
        "set_guild_arena_deck" => {
            valid_session(s, r, "SeasonIndex")?;
            if n(&applied, "Peer") > 0 {
                return Err(rule("GuildArenaNotApplySetDeck"));
            }
            let mut used = BTreeSet::new();
            let mut values = vec![];
            for i in 1..=5 {
                let ids = ids(r, &format!("HeroIndices{i}"), 4)?;
                if ids.is_empty() {
                    continue;
                }
                if i > 3 {
                    return Err(rule("GuildArenaDeckHeroNotFound"));
                }
                arena::owned(db, a, &ids).await?;
                for id in &ids {
                    if !used.insert(*id) {
                        return Err(rule("GuildArenaHeroNotFound"));
                    }
                }
                let mut heroes = vec![];
                for id in &ids {
                    heroes.push(cached_hero(db, a, *id).await?);
                }
                values.push(json!({"AccountId":a,"DeckIndex":i,"CachedHeroInfos":heroes,"HeroIndices":ids,"SkillSlotIndices":[],"IsBattle":0,"IsLose":0,"IsShow":1,"IsBlind":0,"IsNPC":0,"IsForceAttack":0}));
            }
            sqlx::query("DELETE FROM guild_arena_decks WHERE account=? AND season=?")
                .bind(a)
                .bind(index)
                .execute(&mut *db)
                .await?;
            for v in &values {
                sqlx::query("INSERT INTO guild_arena_decks(guild_id,account,season,deck,data) VALUES(?,?,?,?,?)").bind(g).bind(a).bind(index).bind(n(v,"DeckIndex")).bind(v.to_string()).execute(&mut *db).await?;
            }
            out["DeckInfos"] = json!(values);
        }
        "set_guild_arena_deck_skill" => {
            valid_session(s, r, "SeasonIndex")?;
            if n(&applied, "Peer") > 0 {
                return Err(rule("GuildArenaNotApplySetDeckSkill"));
            }
            for mut v in decks(db, a, index).await? {
                let key = format!("SkillSlotIndices{}", n(&v, "DeckIndex"));
                let slots: Vec<i64> = serde_json::from_str(if r.text(&key).is_empty() {
                    "[]"
                } else {
                    r.text(&key)
                })
                .map_err(|_| rule("InvalidValue"))?;
                if slots.len() > 5 || slots.iter().any(|v| !(0..=4).contains(v)) {
                    return Err(rule("InvalidValue"));
                }
                v["SkillSlotIndices"] = json!(slots);
                sqlx::query(
                    "UPDATE guild_arena_decks SET data=? WHERE account=? AND season=? AND deck=?",
                )
                .bind(v.to_string())
                .bind(a)
                .bind(index)
                .bind(n(&v, "DeckIndex"))
                .execute(&mut *db)
                .await?;
            }
        }
        "get_guild_arena_member_deck" => {
            valid_session(s, r, "SessionIndex")?;
            out["MemberDeckInfos"] = json!(member_decks(db, s, g).await?);
        }
        "get_guild_arena_group" => {
            valid_session(s, r, "SessionIndex")?;
            let peer = pair(db, s, g).await?;
            out["LeftMemberDeckInfos"] = json!(member_decks(db, s, g).await?);
            out["RightMemberDeckInfos"] = json!(member_decks(db, s, peer).await?);
            out["Rankers"] = json!(rankers(db, s, "arena", index)
                .await?
                .into_iter()
                .filter(|v| [g, peer].contains(&n(v, "GuildId")))
                .collect::<Vec<_>>());
            let used = get(db, a, "guild_arena_used", index).await?;
            out["BattledHeroIndices"] = if used.is_array() { used } else { json!([]) };
        }
        "get_guild_arena_member_score" => {
            valid_session(s, r, "SessionIndex")?;
            let list = scores(db, s, g, "arena").await?;
            out["MyMemberScore"] = list
                .iter()
                .find(|v| n(v, "AccountId") == a)
                .cloned()
                .unwrap_or(user(db, a).await?);
            out["MemberScores"] = json!(list);
        }
        "get_guild_arena_ranker" => {
            let period = r.number("SeasonIndex", index)?;
            let period = if period == 0 { index } else { period };
            let ranks = rankers(db, s, "arena", period).await?;
            out["MyRankerInfo"] = ranks
                .iter()
                .find(|v| n(v, "GuildId") == g)
                .cloned()
                .unwrap_or(Value::Null);
            out["Rankers"] = json!(ranks
                .into_iter()
                .skip(int(r, "PageNo")? as usize * 10)
                .take(10)
                .collect::<Vec<_>>());
        }
        "get_guild_arena_member_record" => {
            let period = r.number("SessionIndex", index)?;
            let period = if period == 0 { index } else { period };
            let page = int(r, "PageNo")?.min(10000);
            let rows:Vec<String>=sqlx::query_scalar("SELECT data FROM guild_arena_records WHERE (guild_id=? OR enemy_guild=?) AND season=? ORDER BY id DESC LIMIT 10 OFFSET ?").bind(g).bind(g).bind(period).bind(page*10).fetch_all(db).await?;
            out["MemberRecords"] = json!(rows
                .iter()
                .map(|v| parse::<Value>(v))
                .collect::<Result<Vec<_>>>()?);
        }
        "get_guild_arena_ranker_record" => {
            let period = r.number("SeasonIndex", index)?;
            let period = if period == 0 { index } else { period };
            if period < index && int(r, "PageNo")? == 0 {
                let registration = get(db, g, "guild_arena", period).await?;
                let peer = n(&registration, "Peer");
                if peer > 0 {
                    let ranks = rankers(db, s, "arena", period).await?;
                    let left = ranks.iter().find(|v| n(v, "GuildId") == g);
                    let right = ranks.iter().find(|v| n(v, "GuildId") == peer);
                    if let (Some(left), Some(right)) = (left, right) {
                        let win = match n(left, "SessionScore").cmp(&n(right, "SessionScore")) {
                            std::cmp::Ordering::Greater => 1,
                            std::cmp::Ordering::Less => 0,
                            _ => 2,
                        };
                        out["RankerRecords"] = json!([{"SeasonIndex":period,"SessionIndex":period,"SessionNumber":1,"LeftRanker":left,"RightRanker":right,"Win":win}]);
                    }
                }
            }
        }
        "match_guild_arena" | "set_guild_arena_match_result" => {
            return guild_match(db, s, a, g, r, action).await
        }
        _ => return Err(rule("ContentsDisabled")),
    }
    Ok(out)
}
async fn guild_match(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    g: i64,
    r: &Request,
    action: &str,
) -> Result<Value> {
    guild::contents_available(db, a).await?;
    let index = arena::season(s).0;
    let mut active = get(db, a, "guild_arena_active", 0).await?;
    let mut out = item::success();
    if action == "match_guild_arena" {
        valid_session(s, r, "SessionIndex")?;
        if session(s)["State"] != "Battle" {
            return Err(rule("GuildArenaNotBattleState"));
        }
        let peer = pair(db, s, g).await?;
        let enemy = r.number("EnemyAccountId", 0)?;
        if r.number("EnemyGuildId", 0)? != peer || guild::membership(db, enemy).await?.0 != peer {
            return Err(rule("GuildArenaEnemyNotCorrect"));
        }
        let heroes = ids(r, "HeroIndices", 4)?;
        arena::owned(db, a, &heroes).await?;
        let deck = int(r, "EnemyDeckIndex")?;
        if active["Active"] == true && n(&active, "Expires") > now() {
            if active["Heroes"] == json!(heroes)
                && n(&active, "Enemy") == enemy
                && n(&active, "Deck") == deck
            {
                return Ok(active["Response"].clone());
            }
            return Err(rule("CannotPlayInBattle"));
        }
        super::ensure_available(db, a, &heroes).await?;
        let registration = get(db, g, "guild_arena", index).await?;
        if !registration["Members"]
            .as_array()
            .is_some_and(|v| v.contains(&json!(a)))
        {
            return Err(rule("GuildArenaHeroNotFound"));
        }
        let used = get(db, a, "guild_arena_used", index).await?;
        if used
            .as_array()
            .is_some_and(|v| heroes.iter().any(|id| v.contains(&json!(id))))
        {
            return Err(rule("AlreadyUsedBattleHero"));
        }
        let mut target = decks(db, enemy, index)
            .await?
            .into_iter()
            .find(|v| n(v, "DeckIndex") == deck)
            .ok_or_else(|| rule("GuildArenaEnemyDeckNotFound"))?;
        if n(&target, "IsLose") == 1 {
            return Err(rule("GuildArenaEnemyDeckAlreadyCompleted"));
        }
        if n(&target, "LockedUntil") > now() {
            return Err(rule("CannotPlayInBattle"));
        }
        out["HeroIndices"] = json!(heroes);
        out["AccountInfo"] = arena::account(db, a, &heroes, false).await?;
        out["AccountInfo"]["ArenaType"] = json!("GuildArena");
        let enemy_ids: Vec<i64> = serde_json::from_value(target["HeroIndices"].clone())
            .map_err(|_| rule("GuildArenaEnemyDeckNotFound"))?;
        let mut npc = arena::account(db, enemy, &enemy_ids, true).await?;
        let mut map = serde_json::Map::new();
        for hero in target["CachedHeroInfos"].as_array().into_iter().flatten() {
            map.insert(n(hero, "HeroIndex").to_string(), hero.clone());
        }
        npc["HeroInfos"] = json!(map);
        npc["ArenaType"] = json!("GuildArena");
        out["MatchedNpcInfo"] = npc;
        out["ChapterIndex"] = json!(1000);
        out["DungeonIndex"] = json!(3);
        out["StaminaResult"] = arena::tickets(db, s, a, "GuildArenaKey", -1).await?;
        out["BattledHeroIndices"] = if used.is_array() { used } else { json!([]) };
        out["MyScoreInfo"] = user(db, a).await?;
        out["EnemyScoreInfo"] = user(db, enemy).await?;
        let expires = now() + settings(s, "ArenaMatchExpirySeconds", 900);
        target["LockedUntil"] = json!(expires);
        target["LockedBy"] = json!(a);
        target["IsBattle"] = json!(1);
        sqlx::query("UPDATE guild_arena_decks SET data=? WHERE account=? AND season=? AND deck=?")
            .bind(target.to_string())
            .bind(enemy)
            .bind(index)
            .bind(deck)
            .execute(&mut *db)
            .await?;
        active = json!({"Active":true,"GuildId":g,"Season":index,"EnemyGuild":peer,"Enemy":enemy,"Deck":deck,"Heroes":heroes,"Response":out,"Expires":expires});
        put(db, a, "guild_arena_active", 0, &active).await?;
    } else {
        if active["Active"] != true
            || n(&active, "Season") != index
            || n(&active, "GuildId") != g
            || n(&active, "Expires") < now()
        {
            return Err(rule("GuildArenaScoreInfoNotFound"));
        }
        let win = int(r, "Win")?;
        if win > 1 {
            return Err(rule("InvalidValue"));
        }
        let _ = int(r, "PlayTime")?;
        let alive = ids(r, "AliveHeroIndices", 4)?;
        let heroes = active["Heroes"]
            .as_array()
            .ok_or_else(|| rule("GuildArenaScoreInfoNotFound"))?;
        if alive.iter().any(|id| !heroes.contains(&json!(id))) {
            return Err(rule("GuildArenaHeroNotFound"));
        }
        let enemy = n(&active, "Enemy");
        let deck = n(&active, "Deck");
        let mut target = decks(db, enemy, index)
            .await?
            .into_iter()
            .find(|v| n(v, "DeckIndex") == deck)
            .ok_or_else(|| rule("GuildArenaEnemyDeckNotFound"))?;
        if n(&target, "LockedBy") != a {
            return Err(rule("GuildArenaScoreInfoNotFound"));
        }
        target["IsLose"] = json!(win);
        target["IsBattle"] = json!(0);
        target["LockedUntil"] = json!(0);
        target["LockedBy"] = json!(0);
        sqlx::query("UPDATE guild_arena_decks SET data=? WHERE account=? AND season=? AND deck=?")
            .bind(target.to_string())
            .bind(enemy)
            .bind(index)
            .bind(deck)
            .execute(&mut *db)
            .await?;
        let mut used = get(db, a, "guild_arena_used", index)
            .await?
            .as_array()
            .cloned()
            .unwrap_or_default();
        used.extend(heroes.clone());
        put(db, a, "guild_arena_used", index, &json!(used)).await?;
        sqlx::query("INSERT INTO guild_battle_scores(guild_id,account,kind,season,stage,score) VALUES(?,?,'arena',?,?,?) ON CONFLICT(guild_id,account,kind,season,stage) DO UPDATE SET score=score+excluded.score").bind(g).bind(a).bind(index).bind(enemy*10+deck).bind(win).execute(&mut *db).await?;
        let record = json!({"Season":index,"LeftTeam":member_info(db,s,a).await?,"RightTeam":member_info(db,s,enemy).await?,"DeckIndex":deck,"Win":win,"UpdatedTime":time(now())});
        sqlx::query("INSERT INTO guild_arena_records(guild_id,enemy_guild,account,enemy,season,win,data) VALUES(?,?,?,?,?,?,?)").bind(g).bind(n(&active,"EnemyGuild")).bind(a).bind(enemy).bind(index).bind(win).bind(record.to_string()).execute(&mut *db).await?;
        let score = scores(db, s, g, "arena")
            .await?
            .into_iter()
            .find(|v| n(v, "AccountId") == a)
            .map(|v| n(&v, "SessionScore"))
            .unwrap_or(0);
        out["Win"] = json!(win);
        out["AddSessionScore"] = json!(win);
        out["MySessionScore"] = json!(score);
        out["StaminaResult"] = arena::tickets(db, s, a, "GuildArenaKey", 0).await?;
        if win == 1 {
            let mut rw = Rewards::default();
            reward(
                db,
                s,
                a,
                constant(s, "GuildArenaAttackSuccessRewardIndex", 900),
                &mut rw,
            )
            .await?;
            out["RewardItems"] = rewards(db, s, a, rw).await?["ItemResults"].clone();
        }
        active["Active"] = json!(false);
        put(db, a, "guild_arena_active", 0, &active).await?;
    }
    Ok(out)
}
pub(crate) async fn raid_validate(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<()> {
    guild::contents_available(db, a).await?;
    let (g, _) = guild::membership(db, a).await?;
    let c = int(r, "ChapterIndex")?;
    let d = int(r, "DungeonIndex")?;
    let state = raid_list(db, s, g)
        .await?
        .into_iter()
        .find(|v| n(v, "ChapterIndex") == c)
        .ok_or_else(|| rule("DungeonNotFound"))?;
    if n(&state, "IsOngoing") != 1 || n(&state, "DungeonIndex") != d || n(&state, "MonsterHp0") <= 0
    {
        return Err(rule("NotGuildRaid"));
    }
    Ok(())
}
pub(crate) async fn raid_enter(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &mut Value,
    out: &mut Value,
) -> Result<()> {
    let (g, _) = guild::membership(db, a).await?;
    let c = int(r, "ChapterIndex")?;
    out["GuildRaidDungeonInfo"] = raid_list(db, s, g)
        .await?
        .into_iter()
        .find(|v| n(v, "ChapterIndex") == c)
        .ok_or_else(|| rule("DungeonNotFound"))?;
    out["StaminaResult"] = super::guild_ticket(db, s, a, -1).await?;
    let mut raid = get(db, g, "guild_raid", c).await?;
    if n(&raid, "StartedAt") == 0 {
        raid["StartedAt"] = json!(now());
        put(db, g, "guild_raid", c, &raid).await?;
    }
    entry["GuildId"] = json!(g);
    entry["GuildSeason"] = json!(arena::season(s).0);
    entry["GuildBattleStart"] = json!(now());
    Ok(())
}
pub(crate) async fn raid_finish(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    entry: &Value,
    out: &mut Value,
) -> Result<()> {
    guild::contents_available(db, a).await?;
    let (g, _) = guild::membership(db, a).await?;
    let season = arena::season(s).0;
    if g != n(entry, "GuildId") || season != n(entry, "GuildSeason") {
        return Err(rule("NotGuildRaid"));
    }
    let c = n(entry, "ChapterIndex");
    let d = n(entry, "DungeonIndex");
    let mut raid = get(db, g, "guild_raid", c).await?;
    if n(&raid, "DungeonIndex") != d || n(&raid, "IsOngoing") != 1 {
        return Err(rule("AlreadyCompleted"));
    }
    let damage = r.number("TotalDamage", 0)?;
    if damage < 0
        || damage
            > settings(s, "GuildMaxDamagePerSecond", 1000000000)
                .saturating_mul((now() - n(entry, "GuildBattleStart")).max(1))
    {
        return Err(rule("InvalidValue"));
    }
    let damage = damage.min(n(&raid, "MonsterHp0"));
    raid["MonsterHp0"] = json!(n(&raid, "MonsterHp0") - damage);
    sqlx::query("INSERT INTO guild_battle_scores(guild_id,account,kind,season,stage,score) VALUES(?,?,'raid',?,?,?) ON CONFLICT(guild_id,account,kind,season,stage) DO UPDATE SET score=score+excluded.score").bind(g).bind(a).bind(season).bind(c*1000+d).bind(damage).execute(&mut *db).await?;
    if n(&raid, "MonsterHp0") == 0 {
        let def = row(
            s,
            "GuildRaidDungeon",
            &[("ChapterIndex", c), ("DungeonIndex", d)],
        )?;
        let mut info = guild::state(db, s, g).await?;
        info["ActivityPoint"] = json!(n(&info, "ActivityPoint") + n(def, "KillActivityPoint"));
        guild::save(db, g, &info).await?;
        let members:Vec<i64>=sqlx::query_scalar("SELECT b.account FROM guild_battle_scores b JOIN guild_members m ON m.account_id=b.account AND m.guild_id=b.guild_id WHERE b.guild_id=? AND b.kind='raid' AND b.season=? AND b.stage=? AND b.score>0").bind(g).bind(season).bind(c*1000+d).fetch_all(&mut *db).await?;
        for member in members {
            claim(
                db,
                member,
                "guild_raid_kill",
                g * 100000000 + c * 1000 + d,
                &season.to_string(),
            )
            .await?;
            progression::mail_reward(
                db,
                s,
                member,
                n(def, "KillRewardIndex"),
                "Guild raid boss defeated",
            )
            .await?;
        }
        let booty = n(def, "BuyableRewardIndex");
        if booty > 0 {
            let reward = s
                .tables
                .get_reward(booty as i32)
                .ok_or_else(|| rule("ItemDataNotFound"))?;
            for (slot, drop) in reward
                .roll_items(&s.tables.reward_string_pool)
                .into_iter()
                .enumerate()
            {
                let (code, filter) = crate::tables::parse_item_code(&drop.item_code);
                let item = if let Some(id) = s.tables.get_item_index(&code) {
                    Some((id, drop.count, drop.star_min))
                } else {
                    s.tables
                        .roll_item_from_group_code(&code, &filter)
                        .map(|(id, count, star, _)| (id, count * drop.count, star))
                };
                let Some((id, count, star)) = item else {
                    return Err(rule("ItemDataNotFound"));
                };
                if drop.custom_option_index != 0 {
                    return Err(rule("ItemDataNotFound"));
                }
                let price = s
                    .tables
                    .hero_shop
                    .items
                    .get(&id)
                    .map(|v| n(v, "BuyGuildPoint"))
                    .unwrap_or(0);
                if price <= 0 {
                    continue;
                }
                let id_key = season * 1000000000 + c * 10000 + d * 100 + slot as i64;
                let equipment = s
                    .tables
                    .items
                    .reward_item(id)
                    .is_some_and(|v| v.kind == "Equip");
                let v = json!({"Id":id_key,"ShopIndex":4,"ItemIndex":id,"ItemCount":count,"Count":count,"Star":star,"Equipment":equipment,"Price":price,"CreatedTime":time(now()),"DestroyTime":time(now()+constant(s,"GuildRaidBootyItemDestroyMin",10080)*60),"Expires":now()+constant(s,"GuildRaidBootyItemDestroyMin",10080)*60});
                put(db, g, "guild_booty", id_key, &v).await?;
            }
        }
        if let Some(next) = s
            .tables
            .arena_guild
            .rows("GuildRaidDungeon")
            .iter()
            .filter(|v| n(v, "ChapterIndex") == c && n(v, "Step") > n(def, "Step"))
            .min_by_key(|v| n(v, "Step"))
        {
            raid["DungeonIndex"] = json!(n(next, "DungeonIndex"));
            raid["Step"] = json!(n(next, "Step"));
            raid["CurrentDungeonIndex"] = raid["DungeonIndex"].clone();
            raid["CurrentDungeonStep"] = raid["Step"].clone();
            raid["MonsterHp0"] = json!(settings(s, "GuildRaidMaxHp", 1000000000));
        } else {
            raid["IsOngoing"] = json!(0);
            raid["CompletedTime"] = json!(time(now()));
            raid["ClearMemberId"] = json!(a);
            raid["ClearTime"] = json!((now() - n(&raid, "StartedAt")).max(1));
        }
    }
    put(db, g, "guild_raid", c, &raid).await?;
    out["GuildRaidInfos"] = json!(raid_list(db, s, g).await?);
    out["GuildRaidMemberTotalScoreInfo"] = scores(db, s, g, "raid")
        .await?
        .into_iter()
        .find(|v| n(v, "AccountId") == a)
        .unwrap_or(Value::Null);
    Ok(())
}
