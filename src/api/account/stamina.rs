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

/// Get stamina info request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetStaminaInfoRequest {
    pub session_id: Option<String>,
}

/// Get stamina info response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetStaminaInfoResponse {
    pub base_result: i32,
    pub current_stamina: i32,
    pub max_stamina: i32,
    pub stamina_regen_time: i64,
    pub next_regen_at: i64,
}

/// Handle get stamina info request
pub async fn get_stamina_info(
    State(state): State<AppState>,
    Form(req): Form<GetStaminaInfoRequest>,
) -> Result<Json<GetStaminaInfoResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get user stamina info
    let user = sqlx::query(
        "SELECT stamina, max_stamina, last_stamina_update FROM user_info WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_one(&state.db)
    .await?;

    let mut current_stamina: i32 = user.get("stamina");
    let max_stamina: i32 = user.get("max_stamina");
    let last_update: i64 = user.get("last_stamina_update");

    // Calculate regenerated stamina (1 stamina per 5 minutes = 300 seconds)
    let stamina_regen_time: i64 = 300;
    let now = state.server_time();
    let elapsed = now - last_update;
    let regenerated = (elapsed / stamina_regen_time) as i32;

    if regenerated > 0 && current_stamina < max_stamina {
        current_stamina = (current_stamina + regenerated).min(max_stamina);
        
        // Update stamina in database
        sqlx::query("UPDATE user_info SET stamina = ?, last_stamina_update = ? WHERE account_id = ?")
            .bind(current_stamina)
            .bind(now)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    // Calculate next regen time
    let next_regen_at = if current_stamina >= max_stamina {
        0
    } else {
        let time_since_last_regen = elapsed % stamina_regen_time;
        now + (stamina_regen_time - time_since_last_regen)
    };

    Ok(Json(GetStaminaInfoResponse {
        base_result: BaseResultType::Success as i32,
        current_stamina,
        max_stamina,
        stamina_regen_time,
        next_regen_at,
    }))
}

/// Buy stamina request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyStaminaRequest {
    pub session_id: Option<String>,
    pub amount: Option<i32>,
}

/// Buy stamina response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyStaminaResponse {
    pub base_result: i32,
    pub result: i32,
    pub new_stamina: i32,
    pub gem_cost: i32,
    pub daily_purchases_remaining: i32,
}

/// Handle buy stamina request
pub async fn buy_stamina(
    State(state): State<AppState>,
    Form(req): Form<BuyStaminaRequest>,
) -> Result<Json<BuyStaminaResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let amount = req.amount.unwrap_or(100); // Default stamina purchase amount
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Cost per purchase (could scale with daily purchases)
    let gem_cost = 50;

    // Get user info
    let user = sqlx::query(
        "SELECT stamina, max_stamina, gem FROM user_info WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_one(&state.db)
    .await?;

    let current_gem: i32 = user.get("gem");
    
    if current_gem < gem_cost {
        return Ok(Json(BuyStaminaResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Not enough gems
            new_stamina: user.get("stamina"),
            gem_cost,
            daily_purchases_remaining: 0,
        }));
    }

    // Buy stamina
    let new_stamina: i32 = user.get::<i32, _>("stamina") + amount;
    
    sqlx::query("UPDATE user_info SET stamina = ?, gem = gem - ? WHERE account_id = ?")
        .bind(new_stamina)
        .bind(gem_cost)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(BuyStaminaResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        new_stamina,
        gem_cost,
        daily_purchases_remaining: 10, // Simplified, should track daily purchases
    }))
}

/// Use stamina request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct UseStaminaRequest {
    pub session_id: Option<String>,
    pub amount: Option<i32>,
    #[allow(dead_code)]
    pub content_type: Option<String>,  // Reserved for future use
}

/// Use stamina response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct UseStaminaResponse {
    pub base_result: i32,
    pub result: i32,
    pub new_stamina: i32,
}

/// Handle use stamina request
pub async fn use_stamina(
    State(state): State<AppState>,
    Form(req): Form<UseStaminaRequest>,
) -> Result<Json<UseStaminaResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let amount = req.amount.ok_or_else(|| ServerError::InvalidRequest("Missing amount".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get current stamina
    let user = sqlx::query("SELECT stamina FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;

    let current_stamina: i32 = user.get("stamina");
    
    if current_stamina < amount {
        return Ok(Json(UseStaminaResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Not enough stamina
            new_stamina: current_stamina,
        }));
    }

    // Deduct stamina
    let new_stamina = current_stamina - amount;
    
    sqlx::query("UPDATE user_info SET stamina = ? WHERE account_id = ?")
        .bind(new_stamina)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(UseStaminaResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        new_stamina,
    }))
}

/// Restore stamina (for overflow from mail/rewards)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RestoreStaminaRequest {
    pub session_id: Option<String>,
    pub amount: Option<i32>,
}

/// Restore stamina response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RestoreStaminaResponse {
    pub base_result: i32,
    pub new_stamina: i32,
}

/// Handle restore stamina request
pub async fn restore_stamina(
    State(state): State<AppState>,
    Form(req): Form<RestoreStaminaRequest>,
) -> Result<Json<RestoreStaminaResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let amount = req.amount.ok_or_else(|| ServerError::InvalidRequest("Missing amount".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Add stamina (can exceed max from rewards)
    sqlx::query("UPDATE user_info SET stamina = stamina + ? WHERE account_id = ?")
        .bind(amount)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    // Get new stamina
    let user = sqlx::query("SELECT stamina FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;

    Ok(Json(RestoreStaminaResponse {
        base_result: BaseResultType::Success as i32,
        new_stamina: user.get("stamina"),
    }))
}

/// Stamina result info for a specific type
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct StaminaResultInfo {
    #[serde(rename = "Type")]
    pub stamina_type: String,
    pub add_value: i32,
    pub new_value: i32,
    pub stamina_recharge_time: Option<String>,
    pub next_recharge_remain_time: i32,
    pub full_recharge_remain_time: i32,
    pub recharge_count: i32,
    pub is_hide: bool,
}

/// Get stamina infos response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetStaminaInfosResponse {
    pub base_result: String,
    pub result: String,
    pub stamina_results: Vec<StaminaResultInfo>,
}

/// Handle get stamina infos request
/// This returns stamina info for multiple stamina types (keys, tickets, etc.)
/// IMPORTANT: This now queries the database for actual user stamina values,
/// especially for "Chicken" (main stamina) which was previously returning 10
pub async fn get_stamina_infos(
    State(state): State<AppState>,
    body: axum::body::Bytes,
) -> Result<Json<GetStaminaInfosResponse>> {
    // Parse form-urlencoded data manually
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("get_stamina_infos called with body: {}", body_str);
    
    // Parse the form data manually - collect ALL StaminaTypes values and SessionKey
    let mut stamina_types: Vec<String> = Vec::new();
    let mut session_key: Option<String> = None;
    
    for pair in body_str.split('&') {
        let mut kv = pair.splitn(2, '=');
        if let (Some(key), Some(value)) = (kv.next(), kv.next()) {
            let decoded_value = urlencoding::decode(value).unwrap_or_default();
            match key {
                "StaminaTypes" => stamina_types.push(decoded_value.to_string()),
                "SessionKey" | "SessionId" => session_key = Some(decoded_value.to_string()),
                _ => {}
            }
        }
    }
    
    tracing::info!("Parsed {} stamina types: {:?}", stamina_types.len(), stamina_types);
    
    // Get user's actual stamina values from database if session is valid
    let user_stamina = if let Some(ref key) = session_key {
        if let Some(session) = state.get_session(key) {
            sqlx::query(
                "SELECT stamina, sword, sword2, guild_raid_ticket, world_boss_ticket FROM user_info WHERE account_id = ?"
            )
            .bind(session.account_id)
            .fetch_optional(&state.db)
            .await
            .ok()
            .flatten()
        } else {
            None
        }
    } else {
        None
    };
    
    // Create stamina results for each requested type
    let mut stamina_results = Vec::new();
    
    for stamina_type in &stamina_types {
        let stamina_type = stamina_type.trim();
        if stamina_type.is_empty() {
            continue;
        }
        
        // Get actual value from database for known types, otherwise use defaults
        let actual_value = if let Some(ref row) = user_stamina {
            match stamina_type {
                "Chicken" => row.get::<i32, _>("stamina"),
                "Sword" => row.get::<i32, _>("sword"),
                "Sword2" => row.get::<i32, _>("sword2"),
                "GuildRaidTicket" | "GuildRaidKey" => row.get::<i32, _>("guild_raid_ticket"),
                "WorldBossTicket" | "WorldBossKey" => row.get::<i32, _>("world_boss_ticket"),
                _ => get_max_for_stamina_type(stamina_type), // Use max for keys/tickets
            }
        } else {
            get_max_for_stamina_type(stamina_type)
        };
        
        tracing::debug!("Stamina type '{}' returning value: {}", stamina_type, actual_value);
        
        stamina_results.push(StaminaResultInfo {
            stamina_type: stamina_type.to_string(),
            add_value: 0,
            new_value: actual_value,
            stamina_recharge_time: None,
            next_recharge_remain_time: 3600,  // 1 hour until next recharge
            full_recharge_remain_time: 3600,  // 1 hour until full recharge
            recharge_count: 0,
            is_hide: false,
        });
    }
    
    Ok(Json(GetStaminaInfosResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        stamina_results,
    }))
}

/// Get max value for a specific stamina type (for keys/tickets that aren't stored in user_info)
fn get_max_for_stamina_type(stamina_type: &str) -> i32 {
    match stamina_type {
        "Chicken" => 100, // Default max stamina if not found in DB
        "Sword" => 5,
        "Sword2" => 5,
        "PunishmentRaidKey" => 5,
        "TrialOfGodKingKey" => 5,
        "TrialOfFlowKey" => 5,
        "DailyDungeonKey" => 5,
        "WorldBossKey" | "WorldBossTicket" => 3,
        "GuildRaidKey" | "GuildRaidTicket" => 3,
        "ArenaKey" => 5,
        "StockadeKey" => 5,
        "LabyrinthKey" | "UndergroundLabyrinthKey" => 1,
        "HideoutKey" => 5,
        "ChallengeTowerKey" => 5,
        "UndergroundPrisonKey" => 5,
        _ => 5, // Default for unknown types
    }
}
