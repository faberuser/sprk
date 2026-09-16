use super::*;
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
async fn boss(db: &mut SqliteConnection, s: &AppState, family: &str, id: i64) -> Result<Value> {
    let def = source(s, family, id)?;
    if family == "event_world_boss" && def["IsOpen"] != true {
        return Err(rule("WorldBossNotActive"));
    }
    let (season, start, end) = season(s);
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
) -> Result<()> {
    sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score,battle_time) VALUES(?,?,?,?,?,?,?) ON CONFLICT(family,boss,season,account,day) DO UPDATE SET score=score+excluded.score,battle_time=battle_time+excluded.battle_time").bind(family).bind(id).bind(season).bind(a).bind(day()).bind(damage).bind(battle_time).execute(db).await?;
    Ok(())
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
        if n(&entry, "BossSeason") != season(s).0 {
            return Err(rule("WorldBossSeasonChanged"));
        }
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
        v["MonsterHp0"] = json!(n(&v, "MonsterHp0") - dealt);
        v["TotalDamage"] = json!(n(&v, "TotalDamage")
            .checked_add(dealt)
            .ok_or_else(|| rule("WorldBossHpError"))?);
        let killed = n(&v, "MonsterHp0") == 0;
        if killed {
            v["Status"] = json!("Ended");
            v["BattleCompletedTime"] = json!(time(now()));
        }
        put(db, 0, family, campaign::key(id, season(s).0), &v).await?;
        score(db, family, id, season(s).0, a, dealt, elapsed).await?;
        out[field] = v;
        out[if family == "world_boss" {
            "WorldBossKilledByMe"
        } else {
            "EventWorldBossKilledByMe"
        }] = json!(killed);
        let mut reward = Rewards::default();
        let def = source(s, family, id)?;
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
    let rows=sqlx::query("SELECT b.account,SUM(b.score) score,SUM(b.battle_time) battle_time,a.nick,u.avatar_hero_index FROM battle_scores b JOIN accounts a ON a.account_id=b.account JOIN user_info u ON u.account_id=b.account WHERE b.family=? AND b.boss=? AND b.season=? GROUP BY b.account ORDER BY score DESC,b.account LIMIT 10000").bind(family).bind(id).bind(season).fetch_all(db).await?;
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
        let ids = s
            .tables
            .battle
            .rows(name)
            .iter()
            .filter(|v| family == "world_boss" || v["IsOpen"] == true && n(v, "BossLevel") == 1)
            .map(|v| n(v, "Index"))
            .collect::<BTreeSet<_>>();
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
    let current = season(s).0;
    let season = r.number("Season", current)?;
    if season < 1 || season > current {
        return Err(rule("NotFoundSeasonData"));
    }
    if action.ends_with("_hp") {
        out["Hp"] = boss(db, s, family, id).await?["MonsterHp0"].clone();
        return Ok(out);
    }
    if action.contains("daily_score") || action.contains("score_info") {
        let score:i64=sqlx::query_scalar("SELECT COALESCE(SUM(score),0) FROM battle_scores WHERE family=? AND boss=? AND season=? AND account=? AND (?=0 OR day=?)").bind(family).bind(id).bind(season).bind(a).bind(if action.contains("daily"){1}else{0}).bind(day()).fetch_one(db).await?;
        out[if action.contains("daily") {
            "WorldBossDailyScore"
        } else {
            "ScoreInfo"
        }] = json!({"AccountId":a,"WorldBossIndex":id,"Season":season,"Score":score,"IsGetDailyParticipantReward":0,"GetDailyAchievementStep":0,"UpdateTime":time(now())});
        return Ok(out);
    }
    if action.contains("rank") {
        let kind = if family == "raid" {
            "challenge"
        } else {
            family
        };
        let rankings = rankers(db, kind, id, season).await?;
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
        }
        return Ok(out);
    }
    Err(rule("ContentsDisabled"))
}

// Settle closed daily/season records lazily into the existing persistent mailbox.
// The ledger and mail are written in the same transaction, including after reconnect.
pub(super) async fn settle(db: &mut SqliteConnection, s: &AppState, a: i64) -> Result<()> {
    let records=sqlx::query("SELECT family,boss,season,day,score FROM battle_scores b WHERE account=? AND ((family='world_boss' AND day<? AND NOT EXISTS(SELECT 1 FROM battle_reward_claims c WHERE c.account=b.account AND c.kind='world_boss_daily' AND c.idx=b.boss AND c.period=CAST(b.season AS TEXT)||':'||b.day)) OR (family IN ('world_boss','challenge') AND season<? AND NOT EXISTS(SELECT 1 FROM battle_reward_claims c WHERE c.account=b.account AND c.kind=b.family||'_season' AND c.idx=b.boss AND c.period=CAST(b.season AS TEXT)))) ORDER BY season,day LIMIT 500").bind(a).bind(day()).bind(season(s).0).fetch_all(&mut *db).await?;
    for record in records {
        let family: String = record.get("family");
        let id: i64 = record.get("boss");
        let old: i64 = record.get("season");
        let date: String = record.get("day");
        let score: i64 = record.get("score");
        if family == "world_boss" && date < day() {
            if let Some(reward) = s
                .tables
                .battle
                .rows("WorldBossScoreReward")
                .iter()
                .find(|v| {
                    n(v, "WorldBossIndex") == id
                        && score >= n(v, "MinScore")
                        && score <= n(v, "MaxScore")
                })
            {
                deliver(
                    db,
                    s,
                    a,
                    "world_boss_daily",
                    id,
                    &format!("{old}:{date}"),
                    n(reward, "RewardIndex"),
                    "World boss daily reward",
                )
                .await?;
            }
        }
        if old >= season(s).0 || family == "event_world_boss" {
            continue;
        }
        let ranks = rankers(db, &family, id, old).await?;
        let Some(me) = ranks.iter().find(|v| n(v, "AccountId") == a) else {
            continue;
        };
        let rank = n(me, "Rank");
        let data = if family == "challenge" {
            "ChallengeRaidReward"
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
                &format!("{family}_season"),
                id,
                &old.to_string(),
                n(reward, "RewardIndex"),
                "Battle season ranking reward",
            )
            .await?;
        }
    }
    Ok(())
}
async fn deliver(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    kind: &str,
    id: i64,
    period: &str,
    reward: i64,
    title: &str,
) -> Result<()> {
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
    sqlx::query("INSERT INTO mails(account_id,sender,title,content,reward_gold,reward_gem,reward_items,reward_currencies,expires_at) VALUES(?,'System',?,?,?,?,?,?,?)").bind(a).bind(title).bind(format!("Reward for boss {id}, period {period}.")).bind(data.roll_gold()).bind(data.roll_gem()).bind(json!(items).to_string()).bind(json!(currencies).to_string()).bind(time(now()+7*86400)).execute(db).await?;
    Ok(())
}
