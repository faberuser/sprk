use axum::{
    extract::State,
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
// Mail Data Structures (matching client's NShared.MailInfo)
// ============================================================

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MailItemInfo {
    pub item_index: i32,
    pub item_count: i32,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MailCurrencyInfo {
    pub currency_type: i32,
    pub amount: i32,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MailInfo {
    pub mail_index: i64,
    pub open_index: i32,
    pub sender_id: i64,
    pub sender_name: String,
    pub receiver_id: i64,
    pub title: String,
    pub content: String,
    pub send_gold: i32,
    pub send_gem: i32,
    pub pvp_coin: i32,
    pub mileage: i32,
    pub lua_point: i32,
    pub guild_point: i32,
    pub world_boss_point: i32,
    pub event_dungeon_point: i32,
    pub event_dungeon_point2: i32,
    pub glory_point: i32,
    pub event_gift_point: i32,
    pub send_chicken: i32,
    pub send_sword: i32,
    pub send_sword2: i32,
    pub send_underground_prison_key: i32,
    pub send_underground_labyrinth_key: i32,
    pub send_hideout_key: i32,
    pub send_challenge_tower_key: i32,
    pub send_maze_key: i32,
    pub read_time: String,
    pub expire_time: String,
    pub open_time: String,
    pub open_remain_time: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub item_infos: Option<Vec<MailItemInfo>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_infos: Option<Vec<MailCurrencyInfo>>,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo3 {
    pub currency_type: i32,
    pub add_value: i64,
    pub new_value: i64,
}

#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct MailReceiveResult {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_results: Option<Vec<CurrencyResultInfo3>>,
}

// ============================================================
// Helper: Parse form body to HashMap
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

// ============================================================
// Check New Mail Response
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct CheckNewMailResponse {
    pub base_result: i32,
    pub result: i32,
    pub new_mail_arrived: bool,
    pub total_mail_count: i32,
    pub last_checked_mail_index: i64,
}

pub async fn check_new_mail(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<CheckNewMailResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    let topmost_index: i64 = params.get("TopmostMailIndex")
        .and_then(|v| v.parse().ok())
        .unwrap_or(0);

    let stats = sqlx::query(
        "SELECT COUNT(*) as total, MAX(mail_id) as max_index FROM mails WHERE account_id = ? AND is_received = 0"
    )
    .bind(session.account_id)
    .fetch_one(&state.db)
    .await?;

    let total_count: i32 = stats.get::<i64, _>("total") as i32;
    let max_index: i64 = stats.get::<Option<i64>, _>("max_index").unwrap_or(0);

    Ok(Json(CheckNewMailResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        new_mail_arrived: max_index > topmost_index,
        total_mail_count: total_count,
        last_checked_mail_index: max_index,
    }))
}

// ============================================================
// Get Mail List Response
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetMailListResponse {
    pub base_result: i32,
    pub result: i32,
    pub mail_infos: Vec<MailInfo>,
    pub total_count: i32,
}

pub async fn get_mail_list(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<GetMailListResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    let max_count: i32 = params.get("MaxCount")
        .and_then(|v| v.parse().ok())
        .unwrap_or(50)
        .min(100);

    let mail_rows = sqlx::query(
        r#"SELECT mail_id, sender, title, content, 
           reward_gold, reward_gem, is_read, expires_at, reward_items
           FROM mails 
           WHERE account_id = ? AND is_received = 0
           ORDER BY mail_id DESC 
           LIMIT ?"#
    )
    .bind(session.account_id)
    .bind(max_count)
    .fetch_all(&state.db)
    .await?;

    let count_row = sqlx::query(
        "SELECT COUNT(*) as count FROM mails WHERE account_id = ? AND is_received = 0"
    )
    .bind(session.account_id)
    .fetch_one(&state.db)
    .await?;
    let total_count: i32 = count_row.get::<i64, _>("count") as i32;

    let mail_infos: Vec<MailInfo> = mail_rows.iter().map(|row| {
        let items_json: Option<String> = row.get("reward_items");
        let item_infos = items_json.and_then(|j| serde_json::from_str(&j).ok());

        MailInfo {
            mail_index: row.get("mail_id"),
            sender_id: 0,
            sender_name: row.get::<Option<String>, _>("sender").unwrap_or_else(|| "System".to_string()),
            receiver_id: session.account_id,
            title: row.get::<Option<String>, _>("title").unwrap_or_default(),
            content: row.get::<Option<String>, _>("content").unwrap_or_default(),
            send_gold: row.get::<Option<i32>, _>("reward_gold").unwrap_or(0),
            send_gem: row.get::<Option<i32>, _>("reward_gem").unwrap_or(0),
            read_time: String::new(),
            expire_time: row.get::<Option<String>, _>("expires_at").unwrap_or_default(),
            item_infos,
            ..Default::default()
        }
    }).collect();

    Ok(Json(GetMailListResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        mail_infos,
        total_count,
    }))
}

// ============================================================
// Get Global Mail List (server-wide announcements)
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetGlobalMailListResponse {
    pub base_result: i32,
    pub result: i32,
    pub mail_infos: Vec<MailInfo>,
}

pub async fn get_global_mail_list(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<GetGlobalMailListResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // Return empty list for now (global mails are server-wide announcements)
    Ok(Json(GetGlobalMailListResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        mail_infos: vec![],
    }))
}

// ============================================================
// Receive Mail
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveMailResponse {
    pub base_result: i32,
    pub result: i32,
    pub mail_receive_result: MailReceiveResult,
}

pub async fn receive_mail(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<ReceiveMailResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    let mail_index: i64 = params.get("MailIndex")
        .and_then(|v| v.parse().ok())
        .ok_or_else(|| ServerError::InvalidRequest("Missing MailIndex".to_string()))?;

    let mail = sqlx::query(
        "SELECT reward_gold, reward_gem FROM mails WHERE mail_id = ? AND account_id = ? AND is_received = 0"
    )
    .bind(mail_index)
    .bind(session.account_id)
    .fetch_optional(&state.db)
    .await?;

    let mail = mail.ok_or_else(|| ServerError::NotFound("Mail not found".to_string()))?;

    let send_gold: i64 = mail.get::<Option<i64>, _>("reward_gold").unwrap_or(0);
    let send_gem: i64 = mail.get::<Option<i64>, _>("reward_gem").unwrap_or(0);

    let user = sqlx::query("SELECT gold, gem FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;
    
    let current_gold: i64 = user.get("gold");
    let current_gem: i64 = user.get("gem");
    let new_gold = current_gold + send_gold;
    let new_gem = current_gem + send_gem;

    sqlx::query("UPDATE mails SET is_received = 1, is_read = 1 WHERE mail_id = ?")
        .bind(mail_index)
        .execute(&state.db)
        .await?;

    if send_gold > 0 || send_gem > 0 {
        sqlx::query("UPDATE user_info SET gold = ?, gem = ? WHERE account_id = ?")
            .bind(new_gold)
            .bind(new_gem)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    let mut currency_results = Vec::new();
    if send_gold > 0 {
        currency_results.push(CurrencyResultInfo3 {
            currency_type: 1, // Gold
            add_value: send_gold,
            new_value: new_gold,
        });
    }
    if send_gem > 0 {
        currency_results.push(CurrencyResultInfo3 {
            currency_type: 2, // Gem
            add_value: send_gem,
            new_value: new_gem,
        });
    }

    Ok(Json(ReceiveMailResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        mail_receive_result: MailReceiveResult {
            currency_results: if currency_results.is_empty() { None } else { Some(currency_results) },
        },
    }))
}

// ============================================================
// Receive All Mail
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAllMailResponse {
    pub base_result: i32,
    pub result: i32,
    pub mail_receive_results: Vec<MailReceiveResult>,
    pub received_mail_count: i32,
}

pub async fn receive_all_mail(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<ReceiveAllMailResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    let max_count: i32 = params.get("MaxCount")
        .and_then(|v| v.parse().ok())
        .unwrap_or(50)
        .min(100);

    let mails = sqlx::query(
        "SELECT mail_id, reward_gold, reward_gem FROM mails WHERE account_id = ? AND is_received = 0 LIMIT ?"
    )
    .bind(session.account_id)
    .bind(max_count)
    .fetch_all(&state.db)
    .await?;

    if mails.is_empty() {
        return Ok(Json(ReceiveAllMailResponse {
            base_result: BaseResultType::Success as i32,
            result: 0,
            mail_receive_results: vec![],
            received_mail_count: 0,
        }));
    }

    let mut total_gold: i64 = 0;
    let mut total_gem: i64 = 0;
    let mut mail_indices: Vec<i64> = Vec::new();

    for mail in &mails {
        total_gold += mail.get::<Option<i64>, _>("reward_gold").unwrap_or(0);
        total_gem += mail.get::<Option<i64>, _>("reward_gem").unwrap_or(0);
        mail_indices.push(mail.get("mail_id"));
    }

    let user = sqlx::query("SELECT gold, gem FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;
    
    let current_gold: i64 = user.get("gold");
    let current_gem: i64 = user.get("gem");
    let new_gold = current_gold + total_gold;
    let new_gem = current_gem + total_gem;

    for mail_index in &mail_indices {
        sqlx::query("UPDATE mails SET is_received = 1, is_read = 1 WHERE mail_id = ?")
            .bind(mail_index)
            .execute(&state.db)
            .await?;
    }

    if total_gold > 0 || total_gem > 0 {
        sqlx::query("UPDATE user_info SET gold = ?, gem = ? WHERE account_id = ?")
            .bind(new_gold)
            .bind(new_gem)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    let mut currency_results = Vec::new();
    if total_gold > 0 {
        currency_results.push(CurrencyResultInfo3 {
            currency_type: 1,
            add_value: total_gold,
            new_value: new_gold,
        });
    }
    if total_gem > 0 {
        currency_results.push(CurrencyResultInfo3 {
            currency_type: 2,
            add_value: total_gem,
            new_value: new_gem,
        });
    }

    Ok(Json(ReceiveAllMailResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        mail_receive_results: vec![MailReceiveResult {
            currency_results: if currency_results.is_empty() { None } else { Some(currency_results) },
        }],
        received_mail_count: mails.len() as i32,
    }))
}

// ============================================================
// Helper: Send System Mail (for internal use)
// ============================================================

#[allow(dead_code)]
pub async fn send_system_mail(
    db: &sqlx::SqlitePool,
    receiver_id: i64,
    title: &str,
    content: &str,
    gold: i64,
    gem: i64,
    items: Option<Vec<MailItemInfo>>,
) -> Result<i64> {
    let items_json = items.map(|i| serde_json::to_string(&i).unwrap_or_default());
    
    let result = sqlx::query(
        r#"INSERT INTO mails (account_id, sender, title, content, 
           reward_gold, reward_gem, reward_items, is_received)
           VALUES (?, 'System', ?, ?, ?, ?, ?, 0)"#
    )
    .bind(receiver_id)
    .bind(title)
    .bind(content)
    .bind(gold)
    .bind(gem)
    .bind(items_json)
    .execute(db)
    .await?;

    Ok(result.last_insert_rowid())
}
