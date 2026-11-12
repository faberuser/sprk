//! Directional invitations, mutual friendships and once-per-day point exchange.
use crate::api::system::request::Request;
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{QueryBuilder, PgConnection};
use crate::db::RowExt;

pub const FRIEND_LIMIT: i64 = 50;
pub const POINTS_PER_ACTION: i64 = 10;
pub const DAILY_POINT_LIMIT: i64 = 1000;
const DAILY_REMOVE_LIMIT: i64 = 5;

fn response(result: &str) -> Value {
    json!({"BaseResult":"Success", "Result":result})
}

pub async fn profile(state: &AppState, account: i64) -> Result<Value> {
    let row = crate::db::query("SELECT a.account_id, a.nick, a.last_login, a.last_logout, u.team_level, u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id = $1 AND a.is_banned = 0")
        .bind(account).fetch_one(&state.db).await?;
    profile_row(&mut *state.db.acquire().await?, state, &row).await
}
async fn profile_row(db: &mut PgConnection, state: &AppState, row: &sqlx::postgres::PgRow) -> Result<Value> {
    let account: i64 = row.get("account_id");
    let last: Option<String> = row.get("last_login");
    let logout: Option<String> = row.get("last_logout");
    let last_online = last.iter().chain(logout.iter()).max().cloned();
    let online = online(state, account);
    // Both persisted progress formats are in use. An unlock or a numerically
    // higher raid/event chapter is not main-story completion.
    let clears: Vec<(i64,i64)> = crate::db::query_as("SELECT chapter_id,dungeon_id FROM campaign_progress WHERE account_id=$1 AND clear_count>0 UNION SELECT COALESCE(sprk_json_int(data,'$.ChapterIndex'),idx >> 32),COALESCE(sprk_json_int(data,'$.DungeonIndex'),idx & 4294967295) FROM battle_state WHERE account=$2 AND kind='dungeon' AND (sprk_json_int(data,'$.FirstRewardedDiff')>0 OR sprk_json_int(data,'$.MaxStar')>0 OR sprk_json_int(data,'$.ScenarioComplete')=1)")
        .bind(account).bind(account).fetch_all(db).await?;
    let main = main_story_stages(state);
    let (chapter,dungeon) = clears.into_iter().filter(|stage| main.contains(stage)).max().unwrap_or((1,1));
    Ok(json!({"AccountId":account, "Nick":row.get::<String,_>("nick"), "TeamLevel":row.get::<i32,_>("team_level"), "AvatarHeroIndex":row.get::<i32,_>("avatar_hero_index"),
        "VIPLevel":0, "IsOnline":online, "LastLoginTime":last, "LastOnlineTime":last_online, "LastLogoutTime":logout, "TierIndex":0, "MatchScore":0, "LastChapterIndex":chapter, "LastDungeonIndex":dungeon}))
}

/// Main-story stages (first `CampaignChapter` row has MainField, first
/// `CampaignDungeon` row has BattleType 1, 2 or 10), built once per loaded table.
/// Login builds a profile per friend, and scanning ~2800 dungeon rows per
/// cleared stage made friend data most of the login time.
fn main_story_stages(state: &AppState) -> std::sync::Arc<std::collections::HashSet<(i64, i64)>> {
    type Cache = Option<(std::sync::Arc<crate::tables::BattleTable>, std::sync::Arc<std::collections::HashSet<(i64, i64)>>)>;
    static CACHE: std::sync::Mutex<Cache> = std::sync::Mutex::new(None);
    let mut cache = CACHE.lock().unwrap_or_else(|e| e.into_inner());
    if let Some((table, stages)) = cache.as_ref() {
        if std::sync::Arc::ptr_eq(table, &state.tables.battle) { return stages.clone(); }
    }
    let mut main_chapter = std::collections::HashMap::new();
    for row in state.tables.battle.rows("CampaignChapter") {
        if let Some(index) = row["Index"].as_i64() { main_chapter.entry(index).or_insert(row["MainField"] == true); }
    }
    let mut battle_type = std::collections::HashMap::new();
    for row in state.tables.battle.rows("CampaignDungeon") {
        if let (Some(c), Some(d)) = (row["ChapterIndex"].as_i64(), row["DungeonIndex"].as_i64()) {
            battle_type.entry((c, d)).or_insert(super::n(row, "BattleType"));
        }
    }
    let stages = std::sync::Arc::new(battle_type.into_iter()
        .filter(|((c, _), kind)| main_chapter.get(c) == Some(&true) && matches!(kind, 1 | 2 | 10))
        .map(|(stage, _)| stage).collect());
    *cache = Some((state.tables.battle.clone(), std::sync::Arc::clone(&stages)));
    stages
}

/// Chat-connected, or a game session active within the last two minutes.
pub(crate) fn online(state: &AppState, account: i64) -> bool {
    state.chat.online(account)
        || state.sessions.iter().any(|s| {
            s.account_id == account
                && s.last_activity > chrono::Utc::now() - chrono::Duration::minutes(2)
        })
}

/// Native Hero Info popup: return public snapshots, never private login data.
pub async fn inspect_user(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    req.account(&state)?;
    let target = req.number("TargetAccountId", 0)?;
    // Read-only snapshot of another player (~100 Hero snapshots): serve it
    // from a reader instead of queueing every request behind it on the writer.
    let mut db = state.reads.begin().await?;
    let row = crate::db::query("SELECT a.account_id,a.nick,a.last_login, a.last_logout,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id=$1 AND a.is_banned=0")
        .bind(target).fetch_optional(&mut *db).await?;
    let Some(row) = row else { return Ok(Json(response("NoTargetAccountId"))); };
    let mut user = profile_row(&mut db, &state, &row).await?;
    let arena = super::arena::inspection_info(&mut db, &state, target).await?;
    for key in ["TierIndex","MatchScore","SeasonWin","SeasonLose","SuccessiveWin"] {
        user[key] = arena[key].clone();
    }
    let ids: Vec<i64> = crate::db::query_scalar("SELECT hero_index FROM heroes WHERE account_id=$1 ORDER BY hero_index")
        .bind(target).fetch_all(&mut *db).await?;
    if ids.is_empty() { return Ok(Json(response("NoCache"))); }
    let mut heroes = serde_json::Map::new();
    for id in ids {
        heroes.insert(id.to_string(), super::cached_hero(&mut db, target, id).await?);
    }
    // The native Arena inspector selects DeckInfos[BattleType.Arena], not
    // ranking-row HeroInfos. Keep the last real Victory team across rollovers,
    // while the profile statistics above always describe the current season.
    let saved: Option<(i64,String)> = crate::db::query_as("SELECT started,data FROM arena_runs WHERE account=$1 AND kind=0 AND status='complete' AND squad=1 ORDER BY id DESC LIMIT 1")
        .bind(target).fetch_optional(&mut *db).await?;
    let mut decks = json!({});
    if let Some((started,saved)) = saved {
        let saved: Value = super::parse(&saved)?;
        let ids: Vec<i64> = saved["Heroes"].as_array().into_iter().flatten().filter_map(Value::as_i64)
            .filter(|id|heroes.contains_key(&id.to_string())).take(4).collect();
        if !ids.is_empty() {
            let mut deck = json!({"BattleType":"Arena","ChapterIndex":1000,"DungeonIndex":1,"PlayedTime":super::time(started)});
            for slot in 0..4 { deck[format!("HeroIndex{}",slot+1)] = json!(ids.get(slot).copied().unwrap_or(0)); }
            decks["Arena"] = deck;
        }
    }
    // Friends/Guild use the native Campaign deck, independently of Victory.
    // Store it durably because battle_runs is replaced by raids and other modes.
    let mut campaign = crate::api::battle::get(&mut db,target,"inspection_deck",1).await?;
    if campaign.is_null() {
        if let Some((started,entry)) = crate::db::query_as::<(i64,String)>("SELECT started,entry FROM battle_runs WHERE account=$1")
            .bind(target).fetch_optional(&mut *db).await? {
            campaign = crate::api::battle::campaign_inspection_deck(&state,&super::parse(&entry)?,started).unwrap_or(Value::Null);
        }
    }
    if campaign.is_object() {
        let valid: Vec<i64> = (1..=4).map(|slot|super::n(&campaign,&format!("HeroIndex{slot}")))
            .filter(|id|heroes.contains_key(&id.to_string())).collect();
        if !valid.is_empty() {
            for slot in 0..4 { campaign[format!("HeroIndex{}",slot+1)] = json!(valid.get(slot).copied().unwrap_or(0)); }
            decks["Campaign"] = campaign;
        }
    }
    let guild: Option<i64> = crate::db::query_scalar("SELECT guild_id FROM guild_members WHERE account_id=$1")
        .bind(target).fetch_optional(&mut *db).await?;
    let guild_info = if let Some(g) = guild { super::guild::view(&mut db, &state, g).await? } else { Value::Null };
    db.commit().await?;
    Ok(Json(json!({"BaseResult":"Success","Result":"Success",
        "CachedAccountInfo":{"UserInfo":user,"HeroInfos":heroes,"AiHeroInfos":{},"GroupHeroInfos":{},"DeckInfos":decks},
        "GuildInfo":guild_info,"MatchTierRank":arena["Rank"],"MatchTotalRank":arena["Rank"]})))
}

pub async fn login_data(
    state: &AppState,
    account: i64,
) -> Result<(Vec<Value>, Vec<Value>, Vec<Value>, Vec<Value>)> {
    let rows = crate::db::query("SELECT f.account_id AS sender, f.friend_account_id AS receiver, f.status, f.created_at, a.account_id, a.nick, a.last_login, a.last_logout, u.team_level, u.avatar_hero_index FROM friends f JOIN accounts a ON a.account_id = CASE WHEN f.account_id = $1 THEN f.friend_account_id ELSE f.account_id END JOIN user_info u ON u.account_id = a.account_id WHERE (f.account_id = $2 OR f.friend_account_id = $3) AND a.is_banned = 0 ORDER BY f.id")
        .bind(account).bind(account).bind(account).fetch_all(&state.reads).await?;
    let mut outgoing = Vec::new();
    let mut incoming = Vec::new();
    for row in rows {
        let other: i64 = row.get("account_id");
        let accepted = row.get::<String, _>("status") == "accepted";
        let time: String = row.get("created_at");
        if accepted || row.get::<i64, _>("sender") == account {
            outgoing.push(json!({"FriendId":other,"InvitedTime":time}));
        }
        if accepted || row.get::<i64, _>("receiver") == account {
            let mut info = profile_row(&mut *state.reads.acquire().await?, state, &row).await?;
            info["InvitedTime"] = json!(time);
            incoming.push(info);
        }
    }
    let own = point_infos(&mut *state.reads.acquire().await?, account).await?;
    let rows = crate::db::query("SELECT p.account_id AS friend_id, p.last_send_time, p.last_recv_time FROM friend_points p WHERE p.friend_id = $1 AND EXISTS (SELECT 1 FROM friends f WHERE f.status = 'accepted' AND ((f.account_id = p.account_id AND f.friend_account_id = p.friend_id) OR (f.account_id = p.friend_id AND f.friend_account_id = p.account_id)))")
        .bind(account).fetch_all(&state.reads).await?;
    let sent = rows.iter().map(point_row).collect();
    Ok((outgoing, incoming, own, sent))
}
fn point_row(row: &sqlx::postgres::PgRow) -> Value {
    json!({"FriendId":row.get::<i64,_>("friend_id"), "LastSendTime":row.get::<Option<String>,_>("last_send_time"), "LastRecvTime":row.get::<Option<String>,_>("last_recv_time")})
}
async fn point_infos(db: &mut PgConnection, account: i64) -> Result<Vec<Value>> {
    let rows = crate::db::query("SELECT p.* FROM friend_points p WHERE p.account_id = $1 AND EXISTS(SELECT 1 FROM friends f WHERE f.status='accepted' AND ((f.account_id=p.account_id AND f.friend_account_id=p.friend_id) OR (f.account_id=p.friend_id AND f.friend_account_id=p.account_id))) ORDER BY p.friend_id").bind(account).fetch_all(db).await?;
    Ok(rows.iter().map(point_row).collect())
}

pub async fn get_friend_list(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let account = Request::parse(&body)?.account(&state)?;
    let (outgoing, incoming, own, sent) = login_data(&state, account).await?;
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","FriendInfos":outgoing,"FriendInvitorInfos":incoming,"SendRecvFriendshipPointInfos":own,"SentFriendshipPointInfos":sent}),
    ))
}

pub async fn search_friend(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let keyword = req.text("Keyword").trim();
    if keyword.chars().count() > 50 {
        return Ok(Json(
            json!({"BaseResult":"Success", "Result":"FriendNotFound", "FriendInfos":[]}),
        ));
    }
    // The native client requests recommendations with an omitted/empty Keyword
    // when opening Accept/Request, and uses this same endpoint for Find.
    let mut query = QueryBuilder::<sqlx::Postgres>::new(
        "SELECT a.account_id,a.nick,a.last_login, a.last_logout,u.team_level,u.avatar_hero_index \
         FROM accounts a JOIN user_info u USING(account_id) \
         LEFT JOIN credentials c ON c.login_id = a.login_id \
         WHERE a.is_banned = 0 AND u.team_level > 0 AND COALESCE(a.nick, '') != '' \
         AND a.account_id != ",
    );
    query.push_bind(account);
    // Pending invitations in either direction are already shown/handled by the
    // client and must not consume recommendation slots.
    query.push(" AND NOT EXISTS(SELECT 1 FROM friends f WHERE (f.account_id = ");
    query.push_bind(account);
    query.push(" AND f.friend_account_id = a.account_id) OR (f.friend_account_id = ");
    query.push_bind(account);
    query.push(" AND f.account_id = a.account_id))");
    if keyword.is_empty() {
        query.push(" ORDER BY RANDOM()");
    } else {
        let pattern = format!(
            "%{}%",
            keyword
                .replace('!', "!!")
                .replace('%', "!%")
                .replace('_', "!_")
        );
        let player_id = keyword.parse::<i64>().ok().filter(|id| *id > 0);
        query.push(" AND (a.nick ILIKE ");
        query.push_bind(pattern.clone());
        query.push(" ESCAPE '!' OR c.username ILIKE ");
        query.push_bind(pattern);
        query.push(" ESCAPE '!' OR a.account_id = ");
        query.push_bind(player_id);
        // Prioritize exact IDs/names ahead of partial name matches.
        query.push(") ORDER BY CASE WHEN a.account_id = ");
        query.push_bind(player_id);
        query.push(" THEN 0 WHEN lower(a.nick) = lower(");
        query.push_bind(keyword);
        query.push(") OR lower(c.username) = lower(");
        query.push_bind(keyword);
        query.push(") THEN 1 ELSE 2 END, a.account_id");
    }
    query.push(" LIMIT 20");
    let rows = query.build().fetch_all(&state.reads).await?;
    let mut infos = Vec::with_capacity(rows.len());
    for row in &rows { infos.push(profile_row(&mut *state.reads.acquire().await?, &state, row).await?); }
    Ok(Json(
        json!({"BaseResult":"Success", "Result":if !keyword.is_empty() && infos.is_empty() {"FriendNotFound"} else {"Success"}, "FriendInfos":infos}),
    ))
}

async fn friend_count(db: &mut PgConnection, account: i64) -> Result<i64> {
    Ok(crate::db::query_scalar("SELECT COUNT(*) FROM friends WHERE status = 'accepted' AND (account_id = $1 OR friend_account_id = $2)").bind(account).bind(account).fetch_one(db).await?)
}

pub async fn request_friend(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    invite(state, body, false).await
}
pub async fn accept_friend(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    invite(state, body, true).await
}
async fn invite(state: AppState, body: Bytes, force_accept: bool) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let target = req.number("FriendId", req.number("FriendAccountId", 0)?)?;
    let accept = force_accept || matches!(req.text("InviteType"), "Accept" | "1");
    if !matches!(
        req.text("InviteType"),
        "" | "Request" | "Accept" | "0" | "1"
    ) {
        return Err(ServerError::InvalidRequest("Invalid InviteType".into()));
    }
    if target == account {
        return Ok(Json(response("RequestMyself")));
    }
    let mut tx = crate::database::begin(&state.db).await?;
    crate::postgres::lock_account(&mut *tx, account).await?;
    let exists: bool = crate::db::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM accounts WHERE account_id = $1 AND is_banned = 0)",
    )
    .bind(target)
    .fetch_one(&mut *tx)
    .await?;
    if !exists {
        return Ok(Json(response("FriendNotFound")));
    }
    let existing = crate::db::query("SELECT id, account_id, status, created_at FROM friends WHERE (account_id = $1 AND friend_account_id = $2) OR (account_id = $3 AND friend_account_id = $4)")
        .bind(account).bind(target).bind(target).bind(account).fetch_optional(&mut *tx).await?;
    let now = state.server_time_str();
    let mut changed = false;
    let mut invited = now.clone();
    if let Some(row) = existing {
        invited = row.get("created_at");
        if row.get::<String, _>("status") != "accepted"
            && row.get::<i64, _>("account_id") != account
        {
            // A reverse invitation is acceptance, matching the client's two-list model.
            if friend_count(&mut tx, account).await? >= FRIEND_LIMIT {
                return Ok(Json(response("MyFriendFull")));
            }
            if friend_count(&mut tx, target).await? >= FRIEND_LIMIT {
                return Ok(Json(response("TargetFriendFull")));
            }
            crate::db::query("UPDATE friends SET status = 'accepted' WHERE id = $1")
                .bind(row.get::<i64, _>("id"))
                .execute(&mut *tx)
                .await?;
            changed = true;
        } else if accept && row.get::<String, _>("status") != "accepted" {
            return Ok(Json(response("FriendNotFound")));
        }
    } else {
        if accept {
            return Ok(Json(response("FriendNotFound")));
        }
        if friend_count(&mut tx, account).await? >= FRIEND_LIMIT {
            return Ok(Json(response("MyFriendFull")));
        }
        if friend_count(&mut tx, target).await? >= FRIEND_LIMIT {
            return Ok(Json(response("TargetFriendFull")));
        }
        let waiting: i64 = crate::db::query_scalar(
            "SELECT COUNT(*) FROM friends WHERE friend_account_id = $1 AND status = 'pending'",
        )
        .bind(target)
        .fetch_one(&mut *tx)
        .await?;
        if waiting >= FRIEND_LIMIT {
            return Ok(Json(response("TargetFriendWaitFull")));
        }
        crate::db::query("INSERT INTO friends(account_id,friend_account_id,status,created_at) VALUES ($1,$2,'pending',$3)")
            .bind(account).bind(target).bind(&now).execute(&mut *tx).await?;
        changed = true;
    }
    tx.commit().await?;
    if changed {
        let mut info = profile(&state, account).await?;
        info["InvitedTime"] = json!(invited);
        state.chat.notify(
            target,
            account,
            "RequestFriend",
            json!({"InvitorInfo":info}),
        );
    }
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","FriendInfo":{"FriendId":target,"InvitedTime":invited}}),
    ))
}

async fn reset_daily(db: &mut PgConnection, account: i64, day: &str) -> Result<()> {
    crate::db::query("INSERT INTO friend_daily(account_id,day) VALUES ($1,$2) ON CONFLICT(account_id) DO UPDATE SET day = excluded.day, points = CASE WHEN friend_daily.day = excluded.day THEN friend_daily.points ELSE 0 END, removed = CASE WHEN friend_daily.day = excluded.day THEN friend_daily.removed ELSE 0 END")
        .bind(account).bind(day).execute(db).await?;
    Ok(())
}
pub async fn reject_friend(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    remove(state, body).await
}
pub async fn remove_friend(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    remove(state, body).await
}
async fn remove(state: AppState, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let target = req.number("FriendId", req.number("FriendAccountId", 0)?)?;
    let mut tx = crate::database::begin(&state.db).await?;
    reset_daily(&mut tx, account, &state.server_date()).await?;
    let removed: i64 = crate::db::query_scalar("SELECT removed FROM friend_daily WHERE account_id = $1")
        .bind(account)
        .fetch_one(&mut *tx)
        .await?;
    let status: Option<String> = crate::db::query_scalar("SELECT status FROM friends WHERE (account_id = $1 AND friend_account_id = $2) OR (account_id = $3 AND friend_account_id = $4)")
        .bind(account).bind(target).bind(target).bind(account).fetch_optional(&mut *tx).await?;
    let Some(status) = status else {
        return Ok(Json(response("FriendNotFound")));
    };
    let accepted = status == "accepted";
    if accepted && removed >= DAILY_REMOVE_LIMIT {
        return Ok(Json(response("MaxDailyRemoveFriends")));
    }
    crate::db::query("DELETE FROM friends WHERE (account_id = $1 AND friend_account_id = $2) OR (account_id = $3 AND friend_account_id = $4)")
        .bind(account).bind(target).bind(target).bind(account).execute(&mut *tx).await?;
    // Keep today's point ledger across removal/re-addition to prevent repeated grants.
    if accepted {
        crate::db::query("UPDATE friend_daily SET removed = removed + 1 WHERE account_id = $1")
            .bind(account)
            .execute(&mut *tx)
            .await?;
    }
    tx.commit().await?;
    state.chat.notify(
        target,
        account,
        if accepted {
            "RemoveFriend"
        } else {
            "RequestFriendCanceled"
        },
        json!({}),
    );
    let reset = (chrono::Utc::now().date_naive() + chrono::Duration::days(1))
        .format("%Y-%m-%d 00:00:00")
        .to_string();
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","DailyRemoveFriends":removed + if accepted {1} else {0},"DailyRemoveFriendsResetTime":reset}),
    ))
}

pub async fn send_friendship_point(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    points(state, body, 0).await
}
pub async fn recv_friendship_point(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    points(state, body, 1).await
}
pub async fn send_recv_friendship_point(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<Value>> {
    points(state, body, 2).await
}
async fn points(state: AppState, body: Bytes, mode: u8) -> Result<Json<Value>> {
    let req = Request::parse_with_integer_arrays(&body, &["FriendIds", "SendFriendIds", "RecvFriendIds"])?;
    let account = req.account(&state)?;
    let send = if mode == 1 {
        vec![]
    } else {
        req.ids(if mode == 2 {
            "SendFriendIds"
        } else {
            "FriendIds"
        })?
    };
    let recv = if mode == 0 {
        vec![]
    } else {
        req.ids(if mode == 2 {
            "RecvFriendIds"
        } else {
            "FriendIds"
        })?
    };
    let mut tx = crate::database::begin(&state.db).await?;
    reset_daily(&mut tx, account, &state.server_date()).await?;
    let daily: i64 = crate::db::query_scalar("SELECT points FROM friend_daily WHERE account_id=$1")
        .bind(account)
        .fetch_one(&mut *tx)
        .await?;
    let mut actions = 0_i64;
    let mut sent = Vec::new();
    let now = state.server_time_str();
    let day = state.server_date();
    for (is_send, ids) in [(true, &send), (false, &recv)] {
        for target in ids {
            let accepted: bool=crate::db::query_scalar("SELECT EXISTS(SELECT 1 FROM friends WHERE status='accepted' AND ((account_id=$1 AND friend_account_id=$2) OR (account_id=$3 AND friend_account_id=$4)))")
                .bind(account).bind(target).bind(target).bind(account).fetch_one(&mut *tx).await?;
            if !accepted {
                return Ok(Json(response("Fail")));
            }
            crate::db::query("INSERT INTO friend_points(account_id,friend_id) VALUES ($1,$2) ON CONFLICT DO NOTHING")
                .bind(account)
                .bind(target)
                .execute(&mut *tx)
                .await?;
            let changed = if is_send {
                crate::db::query("UPDATE friend_points SET last_send_time=$1 WHERE account_id=$2 AND friend_id=$3 AND (last_send_time IS NULL OR substr(last_send_time,1,10) != $4)")
                    .bind(&now).bind(account).bind(target).bind(&day).execute(&mut *tx).await?.rows_affected()>0
            } else {
                crate::db::query("UPDATE friend_points SET last_recv_time=$1 WHERE account_id=$2 AND friend_id=$3 AND substr(last_send_time,1,10)=$4 AND (last_recv_time IS NULL OR substr(last_recv_time,1,10) != $5) AND EXISTS(SELECT 1 FROM friend_points p WHERE p.account_id=$6 AND p.friend_id=$7 AND substr(p.last_send_time,1,10)=$8)")
                    .bind(&now).bind(account).bind(target).bind(&day).bind(&day).bind(target).bind(account).bind(&day).execute(&mut *tx).await?.rows_affected()>0
            };
            if changed {
                actions += 1;
                if is_send {
                    sent.push(*target);
                }
            }
        }
    }
    let add = actions * POINTS_PER_ACTION;
    if daily + add > DAILY_POINT_LIMIT {
        return Ok(Json(response("MaxFriendshipPoint")));
    }
    let new: i64=crate::db::query_scalar("UPDATE user_info SET friendship_point=friendship_point+$1 WHERE account_id=$2 RETURNING friendship_point")
        .bind(add).bind(account).fetch_one(&mut *tx).await?;
    crate::db::query("UPDATE friend_daily SET points=points+$1 WHERE account_id=$2")
        .bind(add)
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let infos = point_infos(&mut tx, account).await?;
    tx.commit().await?;
    for target in sent {
        state.chat.notify(
            target,
            account,
            "SentFriendshipPoint",
            json!({"SentFriendshipPointInfo":{"FriendId":account,"LastSendTime":now}}),
        );
    }
    Ok(Json(
        json!({"BaseResult":"Success","Result":"Success","FriendshipPointResult":{"AddValue":add,"AddDailyAccValue":add,"NewValue":new,"NewDailyAccValue":daily+add},"FriendshipPointInfos":infos}),
    ))
}

#[cfg(test)]
mod main_story_index_tests {
    use super::*;

    #[tokio::test]
    async fn main_story_index_matches_first_row_table_lookups() {
        let db = crate::database::test_pool().await;
        let state = AppState::new(db, crate::tables::test_tables());
        let stages = main_story_stages(&state);
        let battle = &state.tables.battle;
        let mut candidates: Vec<(i64, i64)> = battle.rows("CampaignDungeon").iter()
            .filter_map(|r| Some((r["ChapterIndex"].as_i64()?, r["DungeonIndex"].as_i64()?))).collect();
        candidates.extend([(0, 0), (1, 999_999), (999_999, 1)]);
        let mut main = 0;
        for (c, d) in candidates {
            let expected = battle.find("CampaignChapter", &[("Index", c)]).is_some_and(|v| v["MainField"] == true)
                && battle.find("CampaignDungeon", &[("ChapterIndex", c), ("DungeonIndex", d)])
                    .is_some_and(|v| matches!(super::super::n(v, "BattleType"), 1 | 2 | 10));
            assert_eq!(stages.contains(&(c, d)), expected, "stage {c}-{d}");
            main += expected as usize;
        }
        assert!(main > 0, "fixture has no main-story stages");
        // Reused while the same table set is loaded.
        assert!(std::sync::Arc::ptr_eq(&stages, &main_story_stages(&state)));
    }
}
