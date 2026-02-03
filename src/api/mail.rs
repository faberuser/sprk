use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, RewardItemInfo},
    state::AppState,
};

/// Mail info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MailInfo {
    pub mail_id: i64,
    pub sender: String,
    pub title: String,
    pub content: String,
    pub reward_gold: i64,
    pub reward_gem: i32,
    pub reward_items: Vec<RewardItemInfo>,
    pub is_read: bool,
    pub is_received: bool,
    pub created_at: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub expires_at: Option<String>,
}

/// Get mail list request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetMailListRequest {
    pub session_id: Option<String>,
}

/// Get mail list response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetMailListResponse {
    pub base_result: i32,
    pub mails: Vec<MailInfo>,
}

/// Handle get mail list request
pub async fn get_mail_list(
    State(state): State<AppState>,
    Form(req): Form<GetMailListRequest>,
) -> Result<Json<GetMailListResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let mail_rows = sqlx::query(
        "SELECT * FROM mails WHERE account_id = ? ORDER BY created_at DESC LIMIT 100"
    )
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    let mails: Vec<MailInfo> = mail_rows.iter().map(|row| MailInfo {
        mail_id: row.get("mail_id"),
        sender: row.get("sender"),
        title: row.get("title"),
        content: row.get::<Option<String>, _>("content").unwrap_or_default(),
        reward_gold: row.get::<Option<i64>, _>("reward_gold").unwrap_or(0),
        reward_gem: row.get::<Option<i32>, _>("reward_gem").unwrap_or(0),
        reward_items: vec![], // Would parse from JSON column
        is_read: row.get::<i32, _>("is_read") != 0,
        is_received: row.get::<i32, _>("is_received") != 0,
        created_at: row.get("created_at"),
        expires_at: row.get("expires_at"),
    }).collect();

    Ok(Json(GetMailListResponse {
        base_result: BaseResultType::Success as i32,
        mails,
    }))
}

/// Receive mail request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveMailRequest {
    pub session_id: Option<String>,
    pub mail_id: Option<i64>,
}

/// Receive mail response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveMailResponse {
    pub base_result: i32,
    pub result: i32,
    pub reward_gold: i64,
    pub reward_gem: i32,
}

/// Handle receive mail request
pub async fn receive_mail(
    State(state): State<AppState>,
    Form(req): Form<ReceiveMailRequest>,
) -> Result<Json<ReceiveMailResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let mail_id = req.mail_id.ok_or_else(|| ServerError::InvalidRequest("Missing mail_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get mail info
    let mail = sqlx::query(
        "SELECT reward_gold, reward_gem FROM mails WHERE mail_id = ? AND account_id = ? AND is_received = 0"
    )
    .bind(mail_id)
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    let mail = mail.ok_or_else(|| ServerError::NotFound("Mail not found or already received".to_string()))?;
    
    let reward_gold: i64 = mail.get::<Option<i64>, _>("reward_gold").unwrap_or(0);
    let reward_gem: i32 = mail.get::<Option<i32>, _>("reward_gem").unwrap_or(0);

    // Mark mail as received
    sqlx::query("UPDATE mails SET is_received = 1, is_read = 1 WHERE mail_id = ?")
        .bind(mail_id)
        .execute(&state.db)
        .await?;

    // Add rewards to user
    if reward_gold > 0 || reward_gem > 0 {
        sqlx::query("UPDATE user_info SET gold = gold + ?, gem = gem + ? WHERE account_id = ?")
            .bind(reward_gold)
            .bind(reward_gem)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(ReceiveMailResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        reward_gold,
        reward_gem,
    }))
}

/// Receive all mail request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAllMailRequest {
    pub session_id: Option<String>,
}

/// Receive all mail response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAllMailResponse {
    pub base_result: i32,
    pub result: i32,
    pub total_reward_gold: i64,
    pub total_reward_gem: i32,
    pub received_count: i32,
}

/// Handle receive all mail request
pub async fn receive_all_mail(
    State(state): State<AppState>,
    Form(req): Form<ReceiveAllMailRequest>,
) -> Result<Json<ReceiveAllMailResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get all unreceived mail rewards
    let mail_rows = sqlx::query(
        "SELECT SUM(COALESCE(reward_gold, 0)) as total_gold, SUM(COALESCE(reward_gem, 0)) as total_gem, COUNT(*) as count FROM mails WHERE account_id = ? AND is_received = 0"
    )
    .bind(session.account_id)
    .fetch_one(&state.db)
    .await?;

    let total_gold: i64 = mail_rows.get::<Option<i64>, _>("total_gold").unwrap_or(0);
    let total_gem: i32 = mail_rows.get::<Option<i32>, _>("total_gem").unwrap_or(0);
    let count: i32 = mail_rows.get("count");

    // Mark all mail as received
    sqlx::query("UPDATE mails SET is_received = 1, is_read = 1 WHERE account_id = ? AND is_received = 0")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    // Add rewards to user
    if total_gold > 0 || total_gem > 0 {
        sqlx::query("UPDATE user_info SET gold = gold + ?, gem = gem + ? WHERE account_id = ?")
            .bind(total_gold)
            .bind(total_gem)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(ReceiveAllMailResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        total_reward_gold: total_gold,
        total_reward_gem: total_gem,
        received_count: count,
    }))
}
