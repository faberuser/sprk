//! Directional invitations, mutual friendships and once-per-day point exchange.
use crate::api::system::request::Request;
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};

pub const FRIEND_LIMIT: i64 = 50;
pub const POINTS_PER_ACTION: i64 = 10;
pub const DAILY_POINT_LIMIT: i64 = 1000;
const DAILY_REMOVE_LIMIT: i64 = 5;

fn response(result: &str) -> Value {
    json!({"BaseResult":"Success", "Result":result})
}

pub async fn profile(state: &AppState, account: i64) -> Result<Value> {
    let row = sqlx::query("SELECT a.account_id, a.nick, a.last_login, u.team_level, u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id = ? AND a.is_banned = 0")
        .bind(account).fetch_one(&state.db).await?;
    Ok(profile_row(state, &row))
}
fn profile_row(state: &AppState, row: &sqlx::sqlite::SqliteRow) -> Value {
    let account: i64 = row.get("account_id");
    let last: Option<String> = row.get("last_login");
    let online = state.chat.online(account)
        || state.sessions.iter().any(|s| {
            s.account_id == account
                && s.last_activity > chrono::Utc::now() - chrono::Duration::minutes(2)
        });
    json!({"AccountId":account, "Nick":row.get::<String,_>("nick"), "TeamLevel":row.get::<i32,_>("team_level"), "AvatarHeroIndex":row.get::<i32,_>("avatar_hero_index"),
        "VIPLevel":0, "IsOnline":online, "LastLoginTime":last, "LastOnlineTime":last, "LastLogoutTime": if online { None } else { last.clone() }, "TierIndex":0, "MatchScore":0, "LastChapterIndex":1, "LastDungeonIndex":1})
}

pub async fn login_data(
    state: &AppState,
    account: i64,
) -> Result<(Vec<Value>, Vec<Value>, Vec<Value>, Vec<Value>)> {
    let rows = sqlx::query("SELECT f.account_id AS sender, f.friend_account_id AS receiver, f.status, f.created_at, a.account_id, a.nick, a.last_login, u.team_level, u.avatar_hero_index FROM friends f JOIN accounts a ON a.account_id = CASE WHEN f.account_id = ? THEN f.friend_account_id ELSE f.account_id END JOIN user_info u ON u.account_id = a.account_id WHERE (f.account_id = ? OR f.friend_account_id = ?) AND a.is_banned = 0 ORDER BY f.id")
        .bind(account).bind(account).bind(account).fetch_all(&state.db).await?;
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
            let mut info = profile_row(state, &row);
            info["InvitedTime"] = json!(time);
            incoming.push(info);
        }
    }
    let own = point_infos(&mut *state.db.acquire().await?, account).await?;
    let rows = sqlx::query("SELECT p.account_id AS friend_id, p.last_send_time, p.last_recv_time FROM friend_points p WHERE p.friend_id = ? AND EXISTS (SELECT 1 FROM friends f WHERE f.status = 'accepted' AND ((f.account_id = p.account_id AND f.friend_account_id = p.friend_id) OR (f.account_id = p.friend_id AND f.friend_account_id = p.account_id)))")
        .bind(account).fetch_all(&state.db).await?;
    let sent = rows.iter().map(point_row).collect();
    Ok((outgoing, incoming, own, sent))
}
fn point_row(row: &sqlx::sqlite::SqliteRow) -> Value {
    json!({"FriendId":row.get::<i64,_>("friend_id"), "LastSendTime":row.get::<Option<String>,_>("last_send_time"), "LastRecvTime":row.get::<Option<String>,_>("last_recv_time")})
}
async fn point_infos(db: &mut SqliteConnection, account: i64) -> Result<Vec<Value>> {
    let rows = sqlx::query("SELECT p.* FROM friend_points p WHERE p.account_id = ? AND EXISTS(SELECT 1 FROM friends f WHERE f.status='accepted' AND ((f.account_id=p.account_id AND f.friend_account_id=p.friend_id) OR (f.account_id=p.friend_id AND f.friend_account_id=p.account_id))) ORDER BY p.friend_id").bind(account).fetch_all(db).await?;
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
    if keyword.is_empty() || keyword.chars().count() > 50 {
        return Ok(Json(response("FriendNotFound")));
    }
    let pattern = format!(
        "%{}%",
        keyword
            .replace('!', "!!")
            .replace('%', "!%")
            .replace('_', "!_")
    );
    let rows = sqlx::query("SELECT a.account_id,a.nick,a.last_login,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.is_banned = 0 AND a.account_id != ? AND a.nick LIKE ? ESCAPE '!' AND NOT EXISTS(SELECT 1 FROM friends f WHERE f.status = 'accepted' AND ((f.account_id = ? AND f.friend_account_id = a.account_id) OR (f.friend_account_id = ? AND f.account_id = a.account_id))) ORDER BY a.account_id LIMIT 20")
        .bind(account).bind(pattern).bind(account).bind(account).fetch_all(&state.db).await?;
    let infos: Vec<_> = rows.iter().map(|row| profile_row(&state, row)).collect();
    Ok(Json(
        json!({"BaseResult":"Success", "Result":if infos.is_empty() {"FriendNotFound"} else {"Success"}, "FriendInfos":infos}),
    ))
}

async fn friend_count(db: &mut SqliteConnection, account: i64) -> Result<i64> {
    Ok(sqlx::query_scalar("SELECT COUNT(*) FROM friends WHERE status = 'accepted' AND (account_id = ? OR friend_account_id = ?)").bind(account).bind(account).fetch_one(db).await?)
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
    let mut tx = state.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login = last_login WHERE account_id = ?")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let exists: bool = sqlx::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM accounts WHERE account_id = ? AND is_banned = 0)",
    )
    .bind(target)
    .fetch_one(&mut *tx)
    .await?;
    if !exists {
        return Ok(Json(response("FriendNotFound")));
    }
    let existing = sqlx::query("SELECT id, account_id, status, created_at FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)")
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
            sqlx::query("UPDATE friends SET status = 'accepted' WHERE id = ?")
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
        let waiting: i64 = sqlx::query_scalar(
            "SELECT COUNT(*) FROM friends WHERE friend_account_id = ? AND status = 'pending'",
        )
        .bind(target)
        .fetch_one(&mut *tx)
        .await?;
        if waiting >= FRIEND_LIMIT {
            return Ok(Json(response("TargetFriendWaitFull")));
        }
        sqlx::query("INSERT INTO friends(account_id,friend_account_id,status,created_at) VALUES (?,?,'pending',?)")
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

async fn reset_daily(db: &mut SqliteConnection, account: i64, day: &str) -> Result<()> {
    sqlx::query("INSERT INTO friend_daily(account_id,day) VALUES (?,?) ON CONFLICT(account_id) DO UPDATE SET day = excluded.day, points = CASE WHEN friend_daily.day = excluded.day THEN friend_daily.points ELSE 0 END, removed = CASE WHEN friend_daily.day = excluded.day THEN friend_daily.removed ELSE 0 END")
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
    let mut tx = state.db.begin().await?;
    reset_daily(&mut tx, account, &state.server_date()).await?;
    let removed: i64 = sqlx::query_scalar("SELECT removed FROM friend_daily WHERE account_id = ?")
        .bind(account)
        .fetch_one(&mut *tx)
        .await?;
    let status: Option<String> = sqlx::query_scalar("SELECT status FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)")
        .bind(account).bind(target).bind(target).bind(account).fetch_optional(&mut *tx).await?;
    let Some(status) = status else {
        return Ok(Json(response("FriendNotFound")));
    };
    let accepted = status == "accepted";
    if accepted && removed >= DAILY_REMOVE_LIMIT {
        return Ok(Json(response("MaxDailyRemoveFriends")));
    }
    sqlx::query("DELETE FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)")
        .bind(account).bind(target).bind(target).bind(account).execute(&mut *tx).await?;
    // Keep today's point ledger across removal/re-addition to prevent repeated grants.
    if accepted {
        sqlx::query("UPDATE friend_daily SET removed = removed + 1 WHERE account_id = ?")
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
    let req = Request::parse(&body)?;
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
    let mut tx = state.db.begin().await?;
    reset_daily(&mut tx, account, &state.server_date()).await?;
    let daily: i64 = sqlx::query_scalar("SELECT points FROM friend_daily WHERE account_id=?")
        .bind(account)
        .fetch_one(&mut *tx)
        .await?;
    let mut actions = 0_i64;
    let mut sent = Vec::new();
    let now = state.server_time_str();
    let day = state.server_date();
    for (is_send, ids) in [(true, &send), (false, &recv)] {
        for target in ids {
            let accepted: bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM friends WHERE status='accepted' AND ((account_id=? AND friend_account_id=?) OR (account_id=? AND friend_account_id=?)))")
                .bind(account).bind(target).bind(target).bind(account).fetch_one(&mut *tx).await?;
            if !accepted {
                return Ok(Json(response("Fail")));
            }
            sqlx::query("INSERT OR IGNORE INTO friend_points(account_id,friend_id) VALUES (?,?)")
                .bind(account)
                .bind(target)
                .execute(&mut *tx)
                .await?;
            let changed = if is_send {
                sqlx::query("UPDATE friend_points SET last_send_time=? WHERE account_id=? AND friend_id=? AND (last_send_time IS NULL OR substr(last_send_time,1,10) != ?)")
                    .bind(&now).bind(account).bind(target).bind(&day).execute(&mut *tx).await?.rows_affected()>0
            } else {
                sqlx::query("UPDATE friend_points SET last_recv_time=? WHERE account_id=? AND friend_id=? AND substr(last_send_time,1,10)=? AND (last_recv_time IS NULL OR substr(last_recv_time,1,10) != ?) AND EXISTS(SELECT 1 FROM friend_points p WHERE p.account_id=? AND p.friend_id=? AND substr(p.last_send_time,1,10)=?)")
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
    let new: i64=sqlx::query_scalar("UPDATE user_info SET friendship_point=friendship_point+? WHERE account_id=? RETURNING friendship_point")
        .bind(add).bind(account).fetch_one(&mut *tx).await?;
    sqlx::query("UPDATE friend_daily SET points=points+? WHERE account_id=?")
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
