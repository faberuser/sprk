use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::BaseResultType,
    state::AppState,
};

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
        SELECT u.account_id, u.nickname, u.level, u.avatar_hero_id
        FROM friends f
        JOIN user_info u ON (f.friend_account_id = u.account_id OR f.account_id = u.account_id)
        WHERE (f.account_id = ? OR f.friend_account_id = ?)
            AND f.status = 'accepted'
            AND u.account_id != ?
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
        SELECT u.account_id, u.nickname, u.level, u.avatar_hero_id
        FROM friends f
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
        let row = sqlx::query("SELECT account_id FROM user_info WHERE nickname = ?")
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
