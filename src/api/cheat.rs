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

/// Cheat codes for development/testing
/// These endpoints allow GM/admin commands for testing purposes

/// GM add currency request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddCurrencyRequest {
    pub session_id: Option<String>,
    pub gold: Option<i64>,
    pub gem: Option<i32>,
    pub stamina: Option<i32>,
}

/// GM add currency response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddCurrencyResponse {
    pub base_result: i32,
    pub new_gold: i64,
    pub new_gem: i32,
    pub new_stamina: i32,
}

/// Handle GM add currency request
pub async fn gm_add_currency(
    State(state): State<AppState>,
    Form(req): Form<GmAddCurrencyRequest>,
) -> Result<Json<GmAddCurrencyResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let gold = req.gold.unwrap_or(0);
    let gem = req.gem.unwrap_or(0);
    let stamina = req.stamina.unwrap_or(0);

    // Add currencies
    sqlx::query(
        "UPDATE user_info SET gold = gold + ?, gem = gem + ?, stamina = stamina + ? WHERE account_id = ?"
    )
    .bind(gold)
    .bind(gem)
    .bind(stamina)
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    // Get new values
    let user = sqlx::query("SELECT gold, gem, stamina FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_one(&state.db)
        .await?;

    Ok(Json(GmAddCurrencyResponse {
        base_result: BaseResultType::Success as i32,
        new_gold: user.get("gold"),
        new_gem: user.get("gem"),
        new_stamina: user.get("stamina"),
    }))
}

/// GM add hero request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddHeroRequest {
    pub session_id: Option<String>,
    pub hero_id: Option<i64>,
    pub level: Option<i32>,
    pub star: Option<i32>,
}

/// GM add hero response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddHeroResponse {
    pub base_result: i32,
    pub result: i32,
    pub unique_hero_id: i64,
}

/// Handle GM add hero request
pub async fn gm_add_hero(
    State(state): State<AppState>,
    Form(req): Form<GmAddHeroRequest>,
) -> Result<Json<GmAddHeroResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_id = req.hero_id.ok_or_else(|| ServerError::InvalidRequest("Missing hero_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let level = req.level.unwrap_or(1);
    let star = req.star.unwrap_or(1);

    // Add hero
    let result = sqlx::query(
        "INSERT INTO heroes (account_id, hero_id, level, exp, star, skill_level, transcend_level, uw_level) VALUES (?, ?, ?, 0, ?, 1, 0, 0)"
    )
    .bind(session.account_id)
    .bind(hero_id)
    .bind(level)
    .bind(star)
    .execute(&state.db)
    .await?;

    let unique_hero_id = result.last_insert_rowid();

    Ok(Json(GmAddHeroResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        unique_hero_id,
    }))
}

/// GM set level request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmSetLevelRequest {
    pub session_id: Option<String>,
    pub level: Option<i32>,
}

/// GM set level response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmSetLevelResponse {
    pub base_result: i32,
    pub new_level: i32,
}

/// Handle GM set level request
pub async fn gm_set_level(
    State(state): State<AppState>,
    Form(req): Form<GmSetLevelRequest>,
) -> Result<Json<GmSetLevelResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let level = req.level.ok_or_else(|| ServerError::InvalidRequest("Missing level".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Set player level
    sqlx::query("UPDATE user_info SET level = ? WHERE account_id = ?")
        .bind(level)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(GmSetLevelResponse {
        base_result: BaseResultType::Success as i32,
        new_level: level,
    }))
}

/// GM unlock all request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmUnlockAllRequest {
    pub session_id: Option<String>,
}

/// GM unlock all response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmUnlockAllResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle GM unlock all request - unlocks all chapters, skips tutorial
pub async fn gm_unlock_all(
    State(state): State<AppState>,
    Form(req): Form<GmUnlockAllRequest>,
) -> Result<Json<GmUnlockAllResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let completed_time = state.server_time_str();

    // Skip tutorial - mark tutorial index 1 as completed
    sqlx::query(
        "INSERT OR REPLACE INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, 1, 1, ?)"
    )
    .bind(session.account_id)
    .bind(&completed_time)
    .execute(&state.db)
    .await?;

    // Unlock all chapters (1-10, each with 10 dungeons)
    for chapter_id in 1..=10 {
        for dungeon_id in 1..=10 {
            sqlx::query(
                "INSERT OR IGNORE INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) VALUES (?, ?, ?, 0, 0, 1)"
            )
            .bind(session.account_id)
            .bind(chapter_id)
            .bind(dungeon_id)
            .execute(&state.db)
            .await?;
        }
    }

    Ok(Json(GmUnlockAllResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// GM reset account request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmResetAccountRequest {
    pub session_id: Option<String>,
    pub keep_heroes: Option<bool>,
}

/// GM reset account response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmResetAccountResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle GM reset account request
pub async fn gm_reset_account(
    State(state): State<AppState>,
    Form(req): Form<GmResetAccountRequest>,
) -> Result<Json<GmResetAccountResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let keep_heroes = req.keep_heroes.unwrap_or(false);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Reset user info to defaults
    sqlx::query(
        "UPDATE user_info SET level = 1, exp = 0, gold = 10000, gem = 100, stamina = 100 WHERE account_id = ?"
    )
    .bind(session.account_id)
    .execute(&state.db)
    .await?;

    // Clear progress
    sqlx::query("DELETE FROM campaign_progress WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM tutorial_progress WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM attendance WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    sqlx::query("DELETE FROM achievements WHERE account_id = ?")
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    if !keep_heroes {
        sqlx::query("DELETE FROM heroes WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;

        sqlx::query("DELETE FROM equip_items WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;

        sqlx::query("DELETE FROM items WHERE account_id = ?")
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(GmResetAccountResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// GM add all heroes request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllHeroesRequest {
    pub session_id: Option<String>,
    pub level: Option<i32>,
    pub star: Option<i32>,
}

/// GM add all heroes response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GmAddAllHeroesResponse {
    pub base_result: i32,
    pub heroes_added: i32,
}

/// Handle GM add all heroes request
pub async fn gm_add_all_heroes(
    State(state): State<AppState>,
    Form(req): Form<GmAddAllHeroesRequest>,
) -> Result<Json<GmAddAllHeroesResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let level = req.level.unwrap_or(90);
    let star = req.star.unwrap_or(5);

    // Add sample heroes (IDs 100001-100050)
    let mut heroes_added = 0;
    for hero_id in 100001..=100050 {
        let existing = sqlx::query("SELECT 1 FROM heroes WHERE account_id = ? AND hero_id = ?")
            .bind(session.account_id)
            .bind(hero_id)
            .fetch_optional(&state.db)
            .await?;

        if existing.is_none() {
            sqlx::query(
                "INSERT INTO heroes (account_id, hero_id, level, exp, star, skill_level, transcend_level, uw_level) VALUES (?, ?, ?, 0, ?, 1, 0, 0)"
            )
            .bind(session.account_id)
            .bind(hero_id)
            .bind(level)
            .bind(star)
            .execute(&state.db)
            .await?;
            heroes_added += 1;
        }
    }

    Ok(Json(GmAddAllHeroesResponse {
        base_result: BaseResultType::Success as i32,
        heroes_added,
    }))
}
