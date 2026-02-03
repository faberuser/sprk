use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, hero::HeroInfo, CurrencyResultInfo},
    state::AppState,
};

/// Buy hero request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyHeroRequest {
    pub session_id: Option<String>,
    pub hero_index: Option<i32>,
}

/// Buy hero response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyHeroResponse {
    pub base_result: i32,
    pub result: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hero_info: Option<HeroInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_result: Option<CurrencyResultInfo>,
}

/// Handle buy hero request
pub async fn buy_hero(
    State(state): State<AppState>,
    Form(req): Form<BuyHeroRequest>,
) -> Result<Json<BuyHeroResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_index = req.hero_index.ok_or_else(|| ServerError::InvalidRequest("Missing hero_index".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;

    // Check if hero already owned
    let existing = sqlx::query("SELECT hero_id FROM heroes WHERE account_id = ? AND hero_index = ?")
        .bind(account_id)
        .bind(hero_index)
        .fetch_optional(&state.db)
        .await?;

    if existing.is_some() {
        return Ok(Json(BuyHeroResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Already owned
            hero_info: None,
            currency_result: None,
        }));
    }

    // Get current gems
    let user_info = sqlx::query("SELECT gem FROM user_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_one(&state.db)
        .await?;
    
    let current_gem: i32 = user_info.get("gem");
    let hero_cost = 6000; // Standard hero cost

    if current_gem < hero_cost {
        return Ok(Json(BuyHeroResponse {
            base_result: BaseResultType::Success as i32,
            result: 2, // Not enough currency
            hero_info: None,
            currency_result: None,
        }));
    }

    // Deduct gems
    let new_gem = current_gem - hero_cost;
    sqlx::query("UPDATE user_info SET gem = ? WHERE account_id = ?")
        .bind(new_gem)
        .bind(account_id)
        .execute(&state.db)
        .await?;

    // Create hero
    let result = sqlx::query(
        "INSERT INTO heroes (account_id, hero_index, star, level) VALUES (?, ?, 2, 1)"
    )
    .bind(account_id)
    .bind(hero_index)
    .execute(&state.db)
    .await?;

    let hero_id = result.last_insert_rowid();

    let hero_info = HeroInfo {
        hero_id,
        hero_index,
        star: 2,
        level: 1,
        ..Default::default()
    };

    Ok(Json(BuyHeroResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        hero_info: Some(hero_info),
        currency_result: Some(CurrencyResultInfo {
            currency_type: 2, // Gem
            before_value: current_gem as i64,
            after_value: new_gem as i64,
            change_value: -(hero_cost as i64),
        }),
    }))
}

/// Bookmark hero request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct BookmarkHeroRequest {
    pub session_id: Option<String>,
    pub hero_id: Option<i64>,
    pub is_bookmarked: Option<bool>,
}

/// Bookmark hero response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BookmarkHeroResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle bookmark hero request
pub async fn bookmark_hero(
    State(state): State<AppState>,
    Form(req): Form<BookmarkHeroRequest>,
) -> Result<Json<BookmarkHeroResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_id = req.hero_id.ok_or_else(|| ServerError::InvalidRequest("Missing hero_id".to_string()))?;
    let is_bookmarked = req.is_bookmarked.unwrap_or(true);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    sqlx::query("UPDATE heroes SET is_bookmarked = ? WHERE hero_id = ? AND account_id = ?")
        .bind(if is_bookmarked { 1 } else { 0 })
        .bind(hero_id)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(BookmarkHeroResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

/// Change avatar hero request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ChangeAvatarHeroRequest {
    pub session_id: Option<String>,
    pub hero_index: Option<i32>,
}

/// Change avatar hero response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ChangeAvatarHeroResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle change avatar hero request
pub async fn change_avatar_hero(
    State(state): State<AppState>,
    Form(req): Form<ChangeAvatarHeroRequest>,
) -> Result<Json<ChangeAvatarHeroResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_index = req.hero_index.ok_or_else(|| ServerError::InvalidRequest("Missing hero_index".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    sqlx::query("UPDATE user_info SET avatar_hero_index = ? WHERE account_id = ?")
        .bind(hero_index)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(ChangeAvatarHeroResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}
