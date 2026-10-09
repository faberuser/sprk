use super::*;
use crate::api::heroes;
use std::collections::BTreeSet;

pub(super) async fn execute(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
    trusted: bool,
) -> Result<Value> {
    match path {
        "replay/save_replay" | "internal/b2m_save_replay" => save(db, s, a, r, trusted).await,
        "replay/get_replay" => {
            let row=sqlx::query("SELECT r.* FROM service_replays r JOIN service_replay_accounts p ON p.replay=r.id WHERE r.id=? AND p.account=?")
                .bind(r.number("ReplayUid",0)?).bind(a).fetch_optional(db).await?.ok_or_else(||rule("Fail"))?;
            Ok(json!({"Replay":replay(&row,true)}))
        }
        "replay/get_replay_list" => {
            let kind = r.number("Type", 0)?;
            if !(0..=2).contains(&kind) {
                return Err(rule("Fail"));
            }
            let count = r.number("Count", 20)?;
            if !(1..=100).contains(&count) {
                return Err(rule("Fail"));
            }
            let rows=sqlx::query("SELECT r.id,r.info FROM service_replays r JOIN service_replay_accounts p ON p.replay=r.id WHERE p.account=? AND r.kind=? ORDER BY r.id DESC LIMIT ?").bind(a).bind(kind).bind(count).fetch_all(db).await?;
            Ok(json!({"Replays":rows.iter().map(|r|replay(r,false)).collect::<Vec<_>>()}))
        }
        "recommend_deck/get_recommend_deck_list" => {
            let c = r.number("ChapterIndex", 0)?;
            let d = r.number("DungeonIndex", 0)?;
            let diff = r.number("Difficulty", 0)?;
            if s.tables
                .battle
                .find(
                    "CampaignDungeon",
                    &[("ChapterIndex", c), ("DungeonIndex", d)],
                )
                .is_none()
                || !(0..=3).contains(&diff)
            {
                return Err(rule("ResourceNotFoundError"));
            }
            let mut out = vec![];
            for kind in 0..=1 {
                let rows=sqlx::query("SELECT data FROM service_decks WHERE chapter=? AND dungeon=? AND difficulty=? AND kind=? ORDER BY metric,account LIMIT 10").bind(c).bind(d).bind(diff).bind(kind).fetch_all(&mut *db).await?;
                for (i, row) in rows.iter().enumerate() {
                    let mut v: Value =
                        serde_json::from_str(row.get("data")).map_err(|_| rule("Fail"))?;
                    v["Type"] = json!(if kind == 0 { "Level" } else { "Time" });
                    v["Rank"] = json!(i + 1);
                    out.push(v);
                }
            }
            Ok(json!({"RecommendDecks":out}))
        }
        _ => honor(db, s, a, r, path).await,
    }
}
fn replay(r: &sqlx::sqlite::SqliteRow, data: bool) -> Value {
    json!({"Uid":r.get::<i64,_>("id"),"Info":r.get::<String,_>("info"),"Data":if data{Some(r.get::<String,_>("data"))}else{None}})
}
async fn save(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    trusted: bool,
) -> Result<Value> {
    let kind = r.number("Type", 0)?;
    if !(0..=2).contains(&kind) {
        return Err(rule("TypeError"));
    }
    let info = r.text("Info");
    let logs = r.text("BattleLogs");
    if info.len() > setting(s, "ReplayInfoMaxBytes", 131072).clamp(1, 1048576) as usize
        || logs.is_empty()
        || logs.len() > setting(s, "ReplayMaxBytes", 1048576).clamp(1, 1048576) as usize
    {
        return Err(rule("InfoError"));
    }
    if !info.is_empty() && !serde_json::from_str::<Value>(info).is_ok_and(|v| v.is_object()) {
        return Err(rule("InfoError"));
    }
    let mut accounts = r.ids("AccountIds")?;
    if trusted {
        if accounts.is_empty() || accounts.len() > 16 {
            return Err(rule("AccountIdsError"));
        }
    } else {
        if accounts.is_empty() {
            accounts.push(a);
        }
        if accounts != [a] {
            return Err(rule("AccountIdsError"));
        }
    }
    for id in &accounts {
        let exists: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM accounts WHERE account_id=? AND is_banned=0)",
        )
        .bind(id)
        .fetch_one(&mut *db)
        .await?;
        if !exists {
            return Err(rule("AccountIdsError"));
        }
    }
    let owner = if trusted { accounts[0] } else { a };
    let old:Option<i64>=sqlx::query_scalar("SELECT id FROM service_replays WHERE owner=? AND kind=? AND info=? AND data=? ORDER BY id DESC LIMIT 1").bind(owner).bind(kind).bind(info).bind(logs).fetch_optional(&mut *db).await?;
    let id = match old {
        Some(id) => id,
        None => sqlx::query(
            "INSERT INTO service_replays(owner,kind,info,data,created) VALUES(?,?,?,?,?)",
        )
        .bind(owner)
        .bind(kind)
        .bind(info)
        .bind(logs)
        .bind(now())
        .execute(&mut *db)
        .await?
        .last_insert_rowid(),
    };
    for id2 in accounts {
        sqlx::query("INSERT OR IGNORE INTO service_replay_accounts(replay,account) VALUES(?,?)")
            .bind(id)
            .bind(id2)
            .execute(&mut *db)
            .await?;
    }
    sqlx::query("DELETE FROM service_replays WHERE id IN (SELECT id FROM service_replays WHERE owner=? ORDER BY id DESC LIMIT -1 OFFSET ?)").bind(owner).bind(setting(s,"ReplayLimitPerAccount",100).clamp(1,1000)).execute(db).await?;
    Ok(json!({"ReplayUid":id}))
}
/// Snapshot at entry so later level-ups, selling, and equipment changes do not rewrite a recommendation.
pub(crate) async fn snapshot(db: &mut SqliteConnection, a: i64, party: &[i64]) -> Result<Value> {
    let mut hs = vec![];
    let mut equips = vec![];
    let mut seen = BTreeSet::new();
    for id in party {
        let mut h = heroes::info(db, a, *id as i32).await?;
        h["PunishmentRuneOptionInfos"] = json!([]);
        for part in 1..=10 {
            let slot = n(&h, &format!("EquipItemSlotIndex{part}"));
            if slot > 0 && seen.insert(slot) {
                equips.push(json!(crate::api::extensions::equip(db, a, slot).await?));
            }
        }
        hs.push(h);
    }
    Ok(json!({"AccountId":a.to_string(),"Heroes":hs,"EquipItems":equips}))
}
pub(crate) async fn record_clear(
    db: &mut SqliteConnection,
    a: i64,
    entry: &Value,
    seconds: i64,
) -> Result<()> {
    let mut v = entry["DeckSnapshot"].clone();
    if !v.is_object() || seconds <= 0 || seconds > 86400 {
        return Ok(());
    }
    v["PlayTime"] = json!(seconds);
    let levels = v["Heroes"]
        .as_array()
        .map(|v| v.iter().map(|h| n(h, "Level")).sum::<i64>())
        .unwrap_or(0);
    for (kind, metric) in [(0, levels), (1, seconds)] {
        sqlx::query("INSERT INTO service_decks(account,chapter,dungeon,difficulty,kind,metric,data) VALUES(?,?,?,?,?,?,?) ON CONFLICT(account,chapter,dungeon,difficulty,kind) DO UPDATE SET metric=excluded.metric,data=excluded.data WHERE excluded.metric<service_decks.metric")
            .bind(a).bind(n(entry,"ChapterIndex")).bind(n(entry,"DungeonIndex")).bind(n(entry,"DungeonDifficulty")).bind(kind).bind(metric).bind(v.to_string()).execute(&mut *db).await?;
    }
    Ok(())
}
async fn honor(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
    path: &str,
) -> Result<Value> {
    let popup = path.ends_with("get_contents_notice_popup_ranker_list");
    let kind = if popup {
        r.number("ContentsType", 0)? + 1
    } else {
        r.number("ContentType", 0)?
    };
    let season = r.number(if popup { "SeasonIndex" } else { "Season" }, 0)?;
    if !(1..=4).contains(&kind) || season < 0 {
        return Err(rule("Fail"));
    }
    // Rank source is persisted server score state; no ranking payload is accepted from clients.
    let source=match kind {
        1=>"SELECT account AS id,season,SUM(score) AS score,0 AS wins,0 AS losses,MAX(boss) AS idx FROM battle_scores WHERE family='world_boss' GROUP BY account,season",
        2=>"SELECT guild_id AS id,season,SUM(score) AS score,0 AS wins,0 AS losses,0 AS idx FROM guild_battle_scores WHERE kind='suppress' GROUP BY guild_id,season",
        3=>"SELECT account AS id,season,score,wins,losses,0 AS idx FROM arena_scores WHERE kind=0",
        _=>"SELECT account AS id,season,score,wins,losses,0 AS idx FROM arena_scores WHERE kind=1"
    };
    let seasons: Vec<i64> = sqlx::query_scalar(&format!(
        "SELECT DISTINCT season FROM ({source}) ORDER BY season DESC"
    ))
    .fetch_all(&mut *db)
    .await?;
    let selected = if season == 0 {
        seasons.first().copied().unwrap_or(0)
    } else {
        season
    };
    let rows=sqlx::query(&format!("SELECT *,ROW_NUMBER() OVER(ORDER BY score DESC,id) AS ranking FROM ({source}) WHERE season=? ORDER BY score DESC,id")).bind(selected).fetch_all(&mut *db).await?;
    let group = s.tables.services.rules["ServerGroup"]
        .as_str()
        .unwrap_or("Local");
    let me = if kind == 2 {
        sqlx::query_scalar::<_, i64>("SELECT guild_id FROM guild_members WHERE account_id=?")
            .bind(a)
            .fetch_optional(&mut *db)
            .await?
            .unwrap_or(0)
    } else {
        a
    };
    let mut server = vec![];
    let mut my = vec![];
    let mut total = 0i64;
    for row in rows {
        let id = row.get::<i64, _>("id");
        let rank = row.get::<i64, _>("ranking");
        let score = row.get::<i64, _>("score");
        total = total.saturating_add(score);
        if id == me {
            for typ in ["Server", "World"] {
                my.push(
                    json!({"ContentType":kind,"RankingType":typ,"Season":selected,"Rank":rank}),
                );
            }
        }
        if rank > 100 {
            continue;
        }
        let (name, level, picture) = if kind == 2 {
            let user = sqlx::query("SELECT name,level FROM guilds WHERE guild_id=?")
                .bind(id)
                .fetch_optional(&mut *db)
                .await?;
            match user {
                Some(v) => (v.get::<String, _>("name"), v.get::<i64, _>("level"), 0),
                None => continue,
            }
        } else {
            let u=sqlx::query("SELECT a.nick,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id=?").bind(id).fetch_optional(&mut *db).await?;
            match u {
                Some(u) => (
                    u.get::<String, _>("nick"),
                    u.get::<i64, _>("team_level"),
                    u.get::<i64, _>("avatar_hero_index"),
                ),
                None => continue,
            }
        };
        let mut deck = vec![];
        if matches!(kind, 3 | 4) {
            let saved:Option<String>=sqlx::query_scalar("SELECT data FROM arena_runs WHERE account=? AND kind=? AND status='complete' AND json_extract(data,'$.Season')=? ORDER BY id DESC LIMIT 1").bind(id).bind(if kind==3{0}else{1}).bind(selected).fetch_optional(&mut *db).await?;
            if let Some(saved) = saved {
                let saved: Value = serde_json::from_str(&saved).map_err(|_| rule("Fail"))?;
                for h in saved["Register"]["AccountInfo"]["HeroInfos"]
                    .as_object()
                    .into_iter()
                    .flat_map(|v| v.values())
                {
                    deck.push(json!({"Index":h["HeroIndex"],"Level":h["Level"],"Star":h["Star"]}));
                }
            }
        }
        server.push(json!({"ContentType":kind,"RankingType":"Server","Season":selected,"Rank":rank,"ServerGroup":group,"Id":id,"Name":name,"Level":level,"PictureIndex":picture,"CountryCode":"","Score":score,"MasterAvatarIndex":picture.to_string(),"Win":row.get::<i64,_>("wins"),"Lose":row.get::<i64,_>("losses"),"SuccessiveWin":0,"Deck":deck,"Index":row.get::<i64,_>("idx")}));
    }
    if popup {
        return Ok(
            json!({"ChapterIndex":0,"DungeonIndex":0,"RankerInfos":server.iter().map(|v|json!({"Rank":v["Rank"],"Nick":v["Name"],"Score":v["Score"],"AvatarHeroIndex":v["PictureIndex"],"CountryCode":"","ServerGroup":group})).collect::<Vec<_>>(),"ServerRankInfos":if server.is_empty(){json!([])}else{json!([{"Rank":1,"ServerGroup":group,"Score":total}])}}),
        );
    }
    let world: Vec<_> = server
        .iter()
        .map(|v| {
            let mut v = v.clone();
            v["RankingType"] = json!("World");
            v
        })
        .collect();
    Ok(
        json!({"SeasonInfo":[{"ContentType":kind,"Season":seasons}],"ServerInfo":server,"WorldInfo":world,"MyInfo":my}),
    )
}
