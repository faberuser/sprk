use axum::{
    extract::{State, Form},
    body::Bytes,
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use std::collections::HashMap;
use crate::{
    error::{Result, ServerError},
    models::BaseResultType,
    state::AppState,
};

// ============================================================
// Helper Functions
// ============================================================

fn parse_form_body(body: &str) -> HashMap<String, String> {
    body.split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            match (parts.next(), parts.next()) {
                (Some(key), Some(value)) => {
                    let decoded = urlencoding::decode(value)
                        .map(|s| s.into_owned())
                        .unwrap_or_else(|_| value.to_string());
                    Some((key.to_string(), decoded))
                }
                _ => None,
            }
        })
        .collect()
}

fn get_session_key(params: &HashMap<String, String>) -> Option<String> {
    params.get("SessionKey")
        .or(params.get("SessionId"))
        .cloned()
}

/// Friend info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct FriendInfo {
    pub account_id: i64,
    pub nickname: String,
    pub level: i32,
    pub avatar_hero_id: i64,
    pub last_login: String,
    pub is_online: bool,
}

/// Get friend list request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetFriendListRequest {
    pub session_id: Option<String>,
}

/// Get friend list response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetFriendListResponse {
    pub base_result: i32,
    pub friends: Vec<FriendInfo>,
    pub pending_requests: Vec<FriendInfo>,
}

/// Handle get friend list request
pub async fn get_friend_list(
    State(state): State<AppState>,
    Form(req): Form<GetFriendListRequest>,
) -> Result<Json<GetFriendListResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get accepted friends
    let friend_rows = sqlx::query(
        r#"
        SELECT a.account_id, a.nick as nickname, u.team_level as level, u.avatar_hero_index as avatar_hero_id
        FROM friends f
        JOIN accounts a ON (f.friend_account_id = a.account_id OR f.account_id = a.account_id)
        JOIN user_info u ON a.account_id = u.account_id
        WHERE (f.account_id = ? OR f.friend_account_id = ?)
            AND f.status = 'accepted'
            AND a.account_id != ?
        "#
    )
    .bind(session.account_id)
    .bind(session.account_id)
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    let friends: Vec<FriendInfo> = friend_rows.iter().map(|row| FriendInfo {
        account_id: row.get("account_id"),
        nickname: row.get("nickname"),
        level: row.get("level"),
        avatar_hero_id: row.get("avatar_hero_id"),
        last_login: String::new(),
        is_online: state.sessions.iter().any(|s| s.value().account_id == row.get::<i64, _>("account_id")),
    }).collect();

    // Get pending friend requests
    let pending_rows = sqlx::query(
        r#"
        SELECT a.account_id, a.nick as nickname, u.team_level as level, u.avatar_hero_index as avatar_hero_id
        FROM friends f
        JOIN accounts a ON f.account_id = a.account_id
        JOIN user_info u ON f.account_id = u.account_id
        WHERE f.friend_account_id = ? AND f.status = 'pending'
        "#
    )
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    let pending_requests: Vec<FriendInfo> = pending_rows.iter().map(|row| FriendInfo {
        account_id: row.get("account_id"),
        nickname: row.get("nickname"),
        level: row.get("level"),
        avatar_hero_id: row.get("avatar_hero_id"),
        last_login: String::new(),
        is_online: false,
    }).collect();

    Ok(Json(GetFriendListResponse {
        base_result: BaseResultType::Success as i32,
        friends,
        pending_requests,
    }))
}

/// Add friend request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct AddFriendRequest {
    pub session_id: Option<String>,
    pub target_nickname: Option<String>,
    pub target_account_id: Option<i64>,
}

/// Add friend response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct AddFriendResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle add friend request
pub async fn add_friend(
    State(state): State<AppState>,
    Form(req): Form<AddFriendRequest>,
) -> Result<Json<AddFriendResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Find target user
    let target_account_id = if let Some(id) = req.target_account_id {
        id
    } else if let Some(nickname) = req.target_nickname {
        let row = sqlx::query("SELECT account_id FROM accounts WHERE nick = ?")
            .bind(nickname)
            .fetch_optional(&state.db)
            .await?
            .ok_or_else(|| ServerError::NotFound("User not found".to_string()))?;
        row.get("account_id")
    } else {
        return Err(ServerError::InvalidRequest("Missing target".to_string()));
    };

    // Check if already friends or pending
    let existing = sqlx::query(
        "SELECT 1 FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)"
    )
    .bind(session.account_id)
    .bind(target_account_id)
    .bind(target_account_id)
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    if existing.is_some() {
        return Ok(Json(AddFriendResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Already friends or pending
        }));
    }

    // Create friend request
    sqlx::query(
        "INSERT INTO friends (account_id, friend_account_id, status) VALUES (?, ?, 'pending')"
    )
    .bind(session.account_id)
    .bind(target_account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(AddFriendResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// Accept friend request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct AcceptFriendRequest {
    pub session_id: Option<String>,
    pub friend_account_id: Option<i64>,
}

/// Accept friend response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct AcceptFriendResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle accept friend request
pub async fn accept_friend(
    State(state): State<AppState>,
    Form(req): Form<AcceptFriendRequest>,
) -> Result<Json<AcceptFriendResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let friend_account_id = req.friend_account_id.ok_or_else(|| ServerError::InvalidRequest("Missing friend_account_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Update friend request status
    let result = sqlx::query(
        "UPDATE friends SET status = 'accepted' WHERE account_id = ? AND friend_account_id = ? AND status = 'pending'"
    )
    .bind(friend_account_id)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    if result.rows_affected() == 0 {
        return Err(ServerError::NotFound("Friend request not found".to_string()));
    }

    Ok(Json(AcceptFriendResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// Remove friend request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RemoveFriendRequest {
    pub session_id: Option<String>,
    pub friend_account_id: Option<i64>,
}

/// Remove friend response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RemoveFriendResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle remove friend request
pub async fn remove_friend(
    State(state): State<AppState>,
    Form(req): Form<RemoveFriendRequest>,
) -> Result<Json<RemoveFriendResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let friend_account_id = req.friend_account_id.ok_or_else(|| ServerError::InvalidRequest("Missing friend_account_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Delete friend relationship
    sqlx::query(
        "DELETE FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)"
    )
    .bind(session.account_id)
    .bind(friend_account_id)
    .bind(friend_account_id)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(RemoveFriendResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Search Friend (friend/search_friend)
// ============================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SearchFriendRequest {
    pub session_id: Option<String>,
    pub keyword: String,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SearchFriendResponse {
    pub base_result: i32,
    pub user_list: Vec<FriendInfo>,
}

pub async fn search_friend(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<SearchFriendResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    tracing::debug!("search_friend params: {:?}", params);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    
    tracing::debug!("Got session_key: {}", session_key);
    
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;
    
    let keyword = params.get("Keyword").map(|s| s.as_str()).unwrap_or("");
    
    tracing::debug!("Searching for keyword: '{}'", keyword);

    // Find users matching the nickname
    let user_rows = sqlx::query(
        r#"
        SELECT a.account_id, a.nick as nickname, u.team_level as level, u.avatar_hero_index as avatar_hero_id
        FROM accounts a
        JOIN user_info u ON a.account_id = u.account_id
        WHERE a.nick LIKE ?
        LIMIT 20
        "#
    )
    .bind(format!("%{}%", keyword))
    .fetch_all(&state.db)
    .await?;

    let user_list: Vec<FriendInfo> = user_rows.iter().map(|row| FriendInfo {
        account_id: row.get("account_id"),
        nickname: row.get("nickname"),
        level: row.get("level"),
        avatar_hero_id: row.get("avatar_hero_id"),
        last_login: String::new(),
        is_online: false,
    }).collect();

    Ok(Json(SearchFriendResponse {
        base_result: BaseResultType::Success as i32,
        user_list,
    }))
}

// ============================================================
// Request Friend (friend/request_friend)
// ============================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestFriendRequest {
    pub session_id: Option<String>,
    pub friend_id: i64,
    pub invite_type: Option<i32>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestFriendResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn request_friend(
    State(state): State<AppState>,
    Form(req): Form<RequestFriendRequest>,
) -> Result<Json<RequestFriendResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Check if already friends or pending
    let existing = sqlx::query(
        "SELECT 1 FROM friends WHERE (account_id = ? AND friend_account_id = ?) OR (account_id = ? AND friend_account_id = ?)"
    )
    .bind(session.account_id)
    .bind(req.friend_id)
    .bind(req.friend_id)
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    if existing.is_some() {
        return Ok(Json(RequestFriendResponse {
            base_result: BaseResultType::Success as i32,
            result: -1, // Already friends or pending
        }));
    }

    // Create friend request
    sqlx::query(
        "INSERT INTO friends (account_id, friend_account_id, status) VALUES (?, ?, 'pending')"
    )
    .bind(session.account_id)
    .bind(req.friend_id)
    .execute(&state.db)
    .await?;

    Ok(Json(RequestFriendResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Reject Friend (friend/reject_friend)
// ============================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RejectFriendRequest {
    pub session_id: Option<String>,
    pub friend_id: i64,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RejectFriendResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn reject_friend(
    State(state): State<AppState>,
    Form(req): Form<RejectFriendRequest>,
) -> Result<Json<RejectFriendResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Delete the pending friend request
    sqlx::query(
        "DELETE FROM friends WHERE account_id = ? AND friend_account_id = ? AND status = 'pending'"
    )
    .bind(req.friend_id)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    Ok(Json(RejectFriendResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Send/Receive Friendship Point
// ============================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct FriendshipPointRequest {
    pub session_id: Option<String>,
    pub friend_ids: Vec<i64>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct FriendshipPointResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn send_friendship_point(
    State(state): State<AppState>,
    Form(req): Form<FriendshipPointRequest>,
) -> Result<Json<FriendshipPointResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let _session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // TODO: Implement friendship point sending logic
    // For now, just return success
    Ok(Json(FriendshipPointResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

pub async fn recv_friendship_point(
    State(state): State<AppState>,
    Form(req): Form<FriendshipPointRequest>,
) -> Result<Json<FriendshipPointResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let _session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // TODO: Implement friendship point receiving logic
    // For now, just return success
    Ok(Json(FriendshipPointResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}
