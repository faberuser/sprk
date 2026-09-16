use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use chrono::{Utc, Duration};
use rand::Rng;

use crate::{
    error::{Result, ServerError},
    models::{
        hero::HeroInfo,
        hero_inn::{
            PlayerHeroFriendlyInfo,
            FriendlyActionType, CurrencyResultInfo3, FriendshipPointResultInfo,
            get_available_inn_hero_indices, MAX_FRIENDSHIP_POINTS,
            GREETING_POINTS, CONVERSATION_POINTS, GIFT_POINTS,
            RouletteInfo, RouletteRewardInfo, StaminaResultInfo,
            get_default_roulette_rewards,
        },
    },
    state::AppState,
};

/// Select random heroes from a list
fn select_random_heroes(available: &[i32], count: usize) -> Vec<i32> {
    use rand::seq::SliceRandom;
    let mut rng = rand::thread_rng();
    available.choose_multiple(&mut rng, count.min(available.len())).cloned().collect()
}

// ============================================================================
// Request New Friendly Hero - Called when opening Hero Inn
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestNewFriendlyHeroRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
}

/// Result types for RequestNewFriendlyHero (client expects string names)
#[derive(Debug, Clone, Copy)]
#[allow(dead_code)]
pub enum RequestNewFriendlyHeroResult {
    Fail,
    CoolTimeMismatch,
    CannotRecruitHero,
    InFriendlyState,
    Success,
}

impl RequestNewFriendlyHeroResult {
    fn as_str(&self) -> &'static str {
        match self {
            RequestNewFriendlyHeroResult::Fail => "Fail",
            RequestNewFriendlyHeroResult::CoolTimeMismatch => "CoolTimeMismatch",
            RequestNewFriendlyHeroResult::CannotRecruitHero => "CannotRecruitHero",
            RequestNewFriendlyHeroResult::InFriendlyState => "InFriendlyState",
            RequestNewFriendlyHeroResult::Success => "Success",
        }
    }
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestNewFriendlyHeroResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendly_info: Option<PlayerHeroFriendlyInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub max_closeness_hero_indice: Option<Vec<i32>>,
}

/// Handle request for new friendly heroes in the inn
pub async fn request_new_friendly_hero(
    State(state): State<AppState>,
    Form(req): Form<RequestNewFriendlyHeroRequest>,
) -> Result<Json<RequestNewFriendlyHeroResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let current_time = state.server_time_str();

    // Get player's owned heroes to exclude from inn
    let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&state.db)
        .await?
        .iter()
        .map(|row| row.get("hero_index"))
        .collect();

    // Get available heroes (not owned)
    let available_heroes: Vec<i32> = get_available_inn_hero_indices()
        .into_iter()
        .filter(|h| !owned_heroes.contains(h))
        .collect();

    // Check if there's an existing friendly info for this account
    let existing = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;

    let friendly_info = if let Some(row) = existing {
        // Return existing info
        PlayerHeroFriendlyInfo {
            hero_index: row.get("hero_index"),
            selected_hero_index: row.get("selected_hero_index"),
            friendly_point: row.get("friendly_point"),
            last_greeting_time: row.get("last_greeting_time"),
            last_conversation_time: row.get("last_conversation_time"),
            last_gift_time: row.get("last_gift_time"),
            selected_time: row.get("selected_time"),
            selected_hero_indice: row.get("selected_hero_indices"),
            last_roulette_time: row.get("last_roulette_time"),
        }
    } else {
        // Create new info with random heroes (6 heroes displayed in the inn)
        let selected_heroes = select_random_heroes(&available_heroes, 6);

        let selected_hero_indices_str = selected_heroes
            .iter()
            .map(|h| h.to_string())
            .collect::<Vec<_>>()
            .join(",");

        let first_hero = selected_heroes.first().copied().unwrap_or(0);

        // Insert new record
        sqlx::query(
            "INSERT INTO hero_friendly_info (account_id, hero_index, selected_hero_index, friendly_point, selected_time, selected_hero_indices) VALUES (?, ?, ?, 0, ?, ?)"
        )
        .bind(account_id)
        .bind(first_hero)
        .bind(first_hero)
        .bind(&current_time)
        .bind(&selected_hero_indices_str)
        .execute(&state.db)
        .await?;

        let friendly_info = PlayerHeroFriendlyInfo {
            hero_index: first_hero,
            selected_hero_index: first_hero,
            friendly_point: 0,
            last_greeting_time: None,
            last_conversation_time: None,
            last_gift_time: None,
            selected_time: Some(current_time.clone()),
            selected_hero_indice: Some(selected_hero_indices_str),
            last_roulette_time: None,
        };
        
        friendly_info
    };

    // Check for heroes that reached max closeness (for owned heroes)
    let max_closeness_heroes: Vec<i32> = sqlx::query(
        "SELECT hero_index FROM heroes WHERE account_id = ? AND closeness >= 1000"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await?
    .iter()
    .map(|row| row.get("hero_index"))
    .collect();

    let response = RequestNewFriendlyHeroResponse {
        base_result: "Success".to_string(),
        result: RequestNewFriendlyHeroResult::Success.as_str().to_string(),
        friendly_info: Some(friendly_info),
        max_closeness_hero_indice: if max_closeness_heroes.is_empty() { 
            None 
        } else { 
            Some(max_closeness_heroes) 
        },
    };
    
    Ok(Json(response))
}

// ============================================================================
// Do Hero Friendly - Perform actions (greet, chat, gift) with inn heroes
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct DoHeroFriendlyRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub hero_index: Option<String>,  // Client sends as string
    pub action: Option<String>,       // Client sends enum name as string (e.g., "Greeting", "Conversation", "Gift")
}

/// Parse action string to FriendlyActionType
fn parse_action_string(action: &str) -> FriendlyActionType {
    match action {
        "Greeting" => FriendlyActionType::Greeting,
        "Conversation" => FriendlyActionType::Conversation,
        "Gift" => FriendlyActionType::Gift,
        "Closeness_Greeting" => FriendlyActionType::ClosenessGreeting,
        "Closeness_Conversation" => FriendlyActionType::ClosenessConversation,
        "Closeness_Gift" => FriendlyActionType::ClosenessGift,
        "Connect_Talk_1" => FriendlyActionType::ConnectTalk1,
        "Connect_Talk_2" => FriendlyActionType::ConnectTalk2,
        "Connect_Talk_3" => FriendlyActionType::ConnectTalk3,
        _ => {
            // Try parsing as integer for backwards compatibility
            if let Ok(num) = action.parse::<i32>() {
                FriendlyActionType::from(num)
            } else {
                FriendlyActionType::None
            }
        }
    }
}

/// Result types for DoHeroFriendly (client expects string names)
#[derive(Debug, Clone, Copy)]
#[allow(dead_code)]
pub enum DoHeroFriendlyResult {
    Fail,
    HeroIndexMismatch,
    AlreadyRecruited,
    HeroNotExist,
    CannotRecruit,
    HeroNotExistInFriendlyTable,
    HeroFriendlyPointMax,
    ActionNotReady,
    ActionNotExist,
    CostMismatch,
    Success,
}

impl DoHeroFriendlyResult {
    fn as_str(&self) -> &'static str {
        match self {
            DoHeroFriendlyResult::Fail => "Fail",
            DoHeroFriendlyResult::HeroIndexMismatch => "HeroIndexMismatch",
            DoHeroFriendlyResult::AlreadyRecruited => "AlreadyRecruited",
            DoHeroFriendlyResult::HeroNotExist => "HeroNotExist",
            DoHeroFriendlyResult::CannotRecruit => "CannotRecruit",
            DoHeroFriendlyResult::HeroNotExistInFriendlyTable => "HeroNotExistInFriendlyTable",
            DoHeroFriendlyResult::HeroFriendlyPointMax => "HeroFriendlyPointMax",
            DoHeroFriendlyResult::ActionNotReady => "ActionNotReady",
            DoHeroFriendlyResult::ActionNotExist => "ActionNotExist",
            DoHeroFriendlyResult::CostMismatch => "CostMismatch",
            DoHeroFriendlyResult::Success => "Success",
        }
    }
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct DoHeroFriendlyResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendly_info: Option<PlayerHeroFriendlyInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hero_info: Option<HeroInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_result: Option<CurrencyResultInfo3>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendship_point_result: Option<FriendshipPointResultInfo>,
}

/// Handle do hero friendly action
pub async fn do_hero_friendly(
    State(state): State<AppState>,
    Form(req): Form<DoHeroFriendlyRequest>,
) -> Result<Json<DoHeroFriendlyResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let hero_index: i32 = req.hero_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);
    let action = req.action.as_ref()
        .map(|s| parse_action_string(s))
        .unwrap_or(FriendlyActionType::None);

    if hero_index == 0 {
        return Ok(Json(DoHeroFriendlyResponse {
            base_result: "Success".to_string(),
            result: DoHeroFriendlyResult::HeroIndexMismatch.as_str().to_string(),
            friendly_info: None,
            hero_info: None,
            currency_result: None,
            friendship_point_result: None,
        }));
    }

    // Check if hero is already owned
    let owned_hero = sqlx::query("SELECT * FROM heroes WHERE account_id = ? AND hero_index = ?")
        .bind(account_id)
        .bind(hero_index)
        .fetch_optional(&state.db)
        .await?;

    let is_owned = owned_hero.is_some();

    // Get current friendly info
    let friendly_row = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;

    let friendly_row = match friendly_row {
        Some(row) => row,
        None => {
            return Ok(Json(DoHeroFriendlyResponse {
                base_result: "Success".to_string(),
                result: DoHeroFriendlyResult::HeroNotExistInFriendlyTable.as_str().to_string(),
                friendly_info: None,
                hero_info: None,
                currency_result: None,
                friendship_point_result: None,
            }));
        }
    };

    let current_friendly_point: i32 = friendly_row.get("friendly_point");
    let current_time = state.server_time_str();

    // Calculate points based on action
    let points_gained = match action {
        FriendlyActionType::Greeting => GREETING_POINTS,
        FriendlyActionType::Conversation => CONVERSATION_POINTS,
        FriendlyActionType::Gift => GIFT_POINTS,
        FriendlyActionType::ClosenessGreeting => 25,
        FriendlyActionType::ClosenessConversation => 50,
        FriendlyActionType::ClosenessGift => 100,
        _ => 0,
    };

    // Cost handling based on action type
    // ActionCostType: 0 = Free, 1 = Gold, 7 = Friendship Points
    let mut friendship_point_result = None;
    let mut currency_result = None;
    
    // Conversation costs Friendship Points (ActionCostType=7, ActionCostValue=160)
    if action == FriendlyActionType::Conversation {
        let user_row = sqlx::query("SELECT friendship_point FROM user_info WHERE account_id = ?")
            .bind(account_id)
            .fetch_one(&state.db)
            .await?;
        
        let current_friendship: i64 = user_row.get("friendship_point");
        let conversation_cost: i64 = 160; // From table: ActionCostValue2
        
        if current_friendship < conversation_cost {
            return Ok(Json(DoHeroFriendlyResponse {
                base_result: "Success".to_string(),
                result: DoHeroFriendlyResult::CostMismatch.as_str().to_string(),
                friendly_info: None,
                hero_info: None,
                currency_result: None,
                friendship_point_result: None,
            }));
        }
        
        // Deduct friendship points
        let new_friendship = current_friendship - conversation_cost;
        sqlx::query("UPDATE user_info SET friendship_point = ? WHERE account_id = ?")
            .bind(new_friendship)
            .bind(account_id)
            .execute(&state.db)
            .await?;
        
        friendship_point_result = Some(FriendshipPointResultInfo {
            add_value: -conversation_cost,
            add_daily_acc_value: 0,
            new_value: new_friendship,
            new_daily_acc_value: 0,
        });
    }
    
    // Gift costs Gold (ActionCostType=1, ActionCostValue=40000)
    if action == FriendlyActionType::Gift {
        let user_row = sqlx::query("SELECT gold FROM user_info WHERE account_id = ?")
            .bind(account_id)
            .fetch_one(&state.db)
            .await?;
        
        let current_gold: i64 = user_row.get("gold");
        let gift_gold_cost: i64 = 40000; // From table: ActionCostValue3
        
        if current_gold < gift_gold_cost {
            return Ok(Json(DoHeroFriendlyResponse {
                base_result: "Success".to_string(),
                result: DoHeroFriendlyResult::CostMismatch.as_str().to_string(),
                friendly_info: None,
                hero_info: None,
                currency_result: None,
                friendship_point_result: None,
            }));
        }
        
        // Deduct gold
        let new_gold = current_gold - gift_gold_cost;
        sqlx::query("UPDATE user_info SET gold = ? WHERE account_id = ?")
            .bind(new_gold)
            .bind(account_id)
            .execute(&state.db)
            .await?;
        
        currency_result = Some(CurrencyResultInfo3 {
            currency_type: "Gold".to_string(),
            add_value: -gift_gold_cost,
            new_value: new_gold,
            ..Default::default()
        });
    }
    
    // Closeness Conversation costs Friendship Points (ActionCostType=7, ActionCostValue=50)
    if action == FriendlyActionType::ClosenessConversation {
        let user_row = sqlx::query("SELECT friendship_point FROM user_info WHERE account_id = ?")
            .bind(account_id)
            .fetch_one(&state.db)
            .await?;
        
        let current_friendship: i64 = user_row.get("friendship_point");
        let conversation_cost: i64 = 50; // From table: ActionCostValue5
        
        if current_friendship < conversation_cost {
            return Ok(Json(DoHeroFriendlyResponse {
                base_result: "Success".to_string(),
                result: DoHeroFriendlyResult::CostMismatch.as_str().to_string(),
                friendly_info: None,
                hero_info: None,
                currency_result: None,
                friendship_point_result: None,
            }));
        }
        
        let new_friendship = current_friendship - conversation_cost;
        sqlx::query("UPDATE user_info SET friendship_point = ? WHERE account_id = ?")
            .bind(new_friendship)
            .bind(account_id)
            .execute(&state.db)
            .await?;
        
        friendship_point_result = Some(FriendshipPointResultInfo {
            add_value: -conversation_cost,
            add_daily_acc_value: 0,
            new_value: new_friendship,
            new_daily_acc_value: 0,
        });
    }
    
    // Closeness Gift costs Gold (ActionCostType=1, ActionCostValue=25000)
    if action == FriendlyActionType::ClosenessGift {
        let user_row = sqlx::query("SELECT gold FROM user_info WHERE account_id = ?")
            .bind(account_id)
            .fetch_one(&state.db)
            .await?;
        
        let current_gold: i64 = user_row.get("gold");
        let gift_gold_cost: i64 = 25000; // From table: ActionCostValue6
        
        if current_gold < gift_gold_cost {
            return Ok(Json(DoHeroFriendlyResponse {
                base_result: "Success".to_string(),
                result: DoHeroFriendlyResult::CostMismatch.as_str().to_string(),
                friendly_info: None,
                hero_info: None,
                currency_result: None,
                friendship_point_result: None,
            }));
        }
        
        let new_gold = current_gold - gift_gold_cost;
        sqlx::query("UPDATE user_info SET gold = ? WHERE account_id = ?")
            .bind(new_gold)
            .bind(account_id)
            .execute(&state.db)
            .await?;
        
        currency_result = Some(CurrencyResultInfo3 {
            currency_type: "Gold".to_string(),
            add_value: -gift_gold_cost,
            new_value: new_gold,
            ..Default::default()
        });
    }

    // Update friendly points
    let new_friendly_point = if is_owned {
        // For owned heroes, we update closeness instead
        current_friendly_point
    } else {
        (current_friendly_point + points_gained).min(MAX_FRIENDSHIP_POINTS)
    };

    // Update last action time based on action type
    let time_column = match action {
        FriendlyActionType::Greeting | FriendlyActionType::ClosenessGreeting => "last_greeting_time",
        FriendlyActionType::Conversation | FriendlyActionType::ClosenessConversation => "last_conversation_time",
        FriendlyActionType::Gift | FriendlyActionType::ClosenessGift => "last_gift_time",
        _ => "last_greeting_time",
    };

    // Update friendly info
    let query = format!(
        "UPDATE hero_friendly_info SET friendly_point = ?, {} = ? WHERE account_id = ?",
        time_column
    );
    sqlx::query(&query)
        .bind(new_friendly_point)
        .bind(&current_time)
        .bind(account_id)
        .execute(&state.db)
        .await?;

    // If hero is owned, update closeness on the hero
    let hero_info = if is_owned {
        let hero_row = owned_hero.unwrap();
        let current_closeness: i32 = hero_row.get("closeness");
        let new_closeness = (current_closeness + points_gained).min(1000);
        
        sqlx::query("UPDATE heroes SET closeness = ? WHERE account_id = ? AND hero_index = ?")
            .bind(new_closeness)
            .bind(account_id)
            .bind(hero_index)
            .execute(&state.db)
            .await?;
        
        Some(HeroInfo {
            hero_id: hero_row.get("hero_id"),
            hero_index,
            star: hero_row.get("star"),
            level: hero_row.get("level"),
            exp: hero_row.get("exp"),
            transcend: hero_row.get("transcend"),
            awakened: hero_row.get("awakened"),
            skill_level_1: hero_row.get("skill_level_1"),
            skill_level_2: hero_row.get("skill_level_2"),
            skill_level_3: hero_row.get("skill_level_3"),
            skill_level_4: hero_row.get("skill_level_4"),
            unique_weapon_id: hero_row.get("unique_weapon_id"),
            is_bookmarked: hero_row.get::<i32, _>("is_bookmarked") != 0,
            closeness: new_closeness,
            ..Default::default()
        })
    } else {
        None
    };

    // Get updated friendly info from hero_friendly_info table
    let updated_row = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;

    let friendly_info = if let Some(row) = updated_row {
        PlayerHeroFriendlyInfo {
            hero_index: row.get("hero_index"),
            selected_hero_index: row.get("selected_hero_index"),
            friendly_point: row.get("friendly_point"),
            last_greeting_time: row.get("last_greeting_time"),
            last_conversation_time: row.get("last_conversation_time"),
            last_gift_time: row.get("last_gift_time"),
            selected_time: row.get("selected_time"),
            selected_hero_indice: row.get("selected_hero_indices"),
            last_roulette_time: row.get("last_roulette_time"),
        }
    } else {
        PlayerHeroFriendlyInfo {
            hero_index,
            selected_hero_index: hero_index,
            friendly_point: new_friendly_point,
            last_greeting_time: None,
            last_conversation_time: None,
            last_gift_time: None,
            selected_time: None,
            selected_hero_indice: None,
            last_roulette_time: None,
        }
    };

    Ok(Json(DoHeroFriendlyResponse {
        base_result: "Success".to_string(),
        result: DoHeroFriendlyResult::Success.as_str().to_string(),
        friendly_info: Some(friendly_info),
        hero_info,
        currency_result,
        friendship_point_result,
    }))
}

// ============================================================================
// Change Recruit Hero - Change which hero to recruit in the inn
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ChangeRecruitHeroRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub hero_index: Option<i32>,
}

/// Result types for ChangeRecruitHero (client expects string names)
#[derive(Debug, Clone, Copy)]
#[allow(dead_code)]
pub enum ChangeRecruitHeroResult {
    Fail,
    AlreadyHave,
    HeroNotExist,
    CannotRecruit,
    Success,
}

impl ChangeRecruitHeroResult {
    fn as_str(&self) -> &'static str {
        match self {
            ChangeRecruitHeroResult::Fail => "Fail",
            ChangeRecruitHeroResult::AlreadyHave => "AlreadyHave",
            ChangeRecruitHeroResult::HeroNotExist => "HeroNotExist",
            ChangeRecruitHeroResult::CannotRecruit => "CannotRecruit",
            ChangeRecruitHeroResult::Success => "Success",
        }
    }
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ChangeRecruitHeroResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendly_info: Option<PlayerHeroFriendlyInfo>,
}

/// Handle change recruit hero
pub async fn change_recruit_hero(
    State(state): State<AppState>,
    Form(req): Form<ChangeRecruitHeroRequest>,
) -> Result<Json<ChangeRecruitHeroResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let hero_index = req.hero_index.unwrap_or(0);

    // If hero_index is 0, reset and get new heroes
    if hero_index == 0 {
        // Get player's owned heroes
        let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
            .bind(account_id)
            .fetch_all(&state.db)
            .await?
            .iter()
            .map(|row| row.get("hero_index"))
            .collect();

        // Get available heroes
        let available_heroes: Vec<i32> = get_available_inn_hero_indices()
            .into_iter()
            .filter(|h| !owned_heroes.contains(h))
            .collect();

        // Select random heroes (6 heroes displayed in the inn)
        let selected_heroes = select_random_heroes(&available_heroes, 6);

        let selected_hero_indices_str = selected_heroes
            .iter()
            .map(|h| h.to_string())
            .collect::<Vec<_>>()
            .join(",");

        let first_hero = selected_heroes.first().copied().unwrap_or(0);
        let current_time = state.server_time_str();

        // Update the friendly info
        sqlx::query(
            "UPDATE hero_friendly_info SET hero_index = ?, selected_hero_index = ?, friendly_point = 0, selected_time = ?, selected_hero_indices = ?, last_greeting_time = NULL, last_conversation_time = NULL, last_gift_time = NULL WHERE account_id = ?"
        )
        .bind(first_hero)
        .bind(first_hero)
        .bind(&current_time)
        .bind(&selected_hero_indices_str)
        .bind(account_id)
        .execute(&state.db)
        .await?;

        let friendly_info = PlayerHeroFriendlyInfo {
            hero_index: first_hero,
            selected_hero_index: first_hero,
            friendly_point: 0,
            last_greeting_time: None,
            last_conversation_time: None,
            last_gift_time: None,
            selected_time: Some(current_time),
            selected_hero_indice: Some(selected_hero_indices_str),
            last_roulette_time: None,
        };

        return Ok(Json(ChangeRecruitHeroResponse {
            base_result: "Success".to_string(),
            result: ChangeRecruitHeroResult::Success.as_str().to_string(),
            friendly_info: Some(friendly_info),
        }));
    }

    // Check if player already owns this hero
    let owned = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ? AND hero_index = ?")
        .bind(account_id)
        .bind(hero_index)
        .fetch_optional(&state.db)
        .await?;

    if owned.is_some() {
        return Ok(Json(ChangeRecruitHeroResponse {
            base_result: "Success".to_string(),
            result: ChangeRecruitHeroResult::AlreadyHave.as_str().to_string(),
            friendly_info: None,
        }));
    }

    let current_time = state.server_time_str();

    // Get current selected_hero_indices from database
    let current_row = sqlx::query("SELECT selected_hero_indices FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let selected_hero_indice: Option<String> = current_row.as_ref().and_then(|row| row.get("selected_hero_indices"));

    // Update selected hero
    sqlx::query(
        "UPDATE hero_friendly_info SET hero_index = ?, selected_hero_index = ?, friendly_point = 0, selected_time = ?, last_greeting_time = NULL, last_conversation_time = NULL, last_gift_time = NULL WHERE account_id = ?"
    )
    .bind(hero_index)
    .bind(hero_index)
    .bind(&current_time)
    .bind(account_id)
    .execute(&state.db)
    .await?;

    let friendly_info = PlayerHeroFriendlyInfo {
        hero_index,
        selected_hero_index: hero_index,
        friendly_point: 0,
        last_greeting_time: None,
        last_conversation_time: None,
        last_gift_time: None,
        selected_time: Some(current_time),
        selected_hero_indice,
        last_roulette_time: None,
    };

    Ok(Json(ChangeRecruitHeroResponse {
        base_result: "Success".to_string(),
        result: ChangeRecruitHeroResult::Success.as_str().to_string(),
        friendly_info: Some(friendly_info),
    }))
}

// ============================================================================
// Recruit Hero - Actually recruit the hero when max friendship reached
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RecruitHeroRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub hero_index: Option<i32>,
}

/// Handle recruit hero
pub async fn recruit_hero(
    State(state): State<AppState>,
    Form(req): Form<RecruitHeroRequest>,
) -> Result<Json<serde_json::Value>> {
    use serde_json::json;
    let key=req.session_key.or(req.session_id).ok_or(ServerError::SessionExpired)?;
    let account=state.get_session(&key).ok_or(ServerError::SessionExpired)?.account_id;
    let index=req.hero_index.unwrap_or(0);
    let fail=|code:&str|Json(json!({"BaseResult":"Success","Result":code}));
    let Some(c)=state.tables.hero_shop.heroes.get(&index) else {return Ok(fail("HeroIndexMismatch"))};
    let mut tx=state.db.begin().await?;
    crate::api::inventory::item::init(&mut tx,&state,account).await?;
    let owned:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)").bind(account).bind(index).fetch_one(&mut *tx).await?;
    if owned {return Ok(fail("AlreadyRecruited"));}
    let row=sqlx::query("SELECT hero_index,friendly_point FROM hero_friendly_info WHERE account_id=?").bind(account).fetch_optional(&mut *tx).await?;
    let Some(row)=row else {return Ok(fail("HeroNotExist"))};
    if row.get::<i32,_>("hero_index")!=index {return Ok(fail("HeroIndexMismatch"));}
    if row.get::<i32,_>("friendly_point")<MAX_FRIENDSHIP_POINTS {return Ok(fail("FriendlyPointMismatch"));}
    let mut reward=crate::api::heroes::recruit_at(&mut tx,&state,account,index,crate::api::inventory::item::n(c,"StartHeroStar"),crate::api::inventory::item::n(c,"StartHeroLevel"),0).await?;
    sqlx::query("UPDATE hero_friendly_info SET hero_index=0,selected_hero_index=0,friendly_point=0,selected_time=datetime('now') WHERE account_id=?").bind(account).execute(&mut *tx).await?;
    reward["HeroFriendlyInfo"]=json!({"HeroIndex":0,"SelectedHeroIndex":0,"FriendlyPoint":0,"SelectedTime":state.server_time_str(),"SelectedHeroIndice":""});
    tx.commit().await?;
    Ok(Json(json!({"BaseResult":"Success","Result":"Success","HeroResult":reward})))
}

// ============================================================================
// Hero Inn Reset Time - Get the reset time for daily hero inn actions
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct HeroInnResetTimeRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct HeroInnResetTimeResponse {
    pub base_result: String,
    pub result: String,
    pub last_reset_time: String,
    pub next_reset_time: String,
}

/// Handle hero inn reset time request
pub async fn hero_inn_reset_time(
    State(state): State<AppState>,
    Form(req): Form<HeroInnResetTimeRequest>,
) -> Result<Json<HeroInnResetTimeResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let _session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Calculate reset times (daily reset at midnight UTC)
    let now = Utc::now();
    let last_reset = now.date_naive().and_hms_opt(0, 0, 0).unwrap();
    let next_reset = (now + Duration::days(1)).date_naive().and_hms_opt(0, 0, 0).unwrap();

    let last_reset_str = last_reset.format("%Y-%m-%d %H:%M:%S").to_string();
    let next_reset_str = next_reset.format("%Y-%m-%d %H:%M:%S").to_string();

    Ok(Json(HeroInnResetTimeResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        last_reset_time: last_reset_str,
        next_reset_time: next_reset_str,
    }))
}

// ============================================================================
// Give Reward Max Closeness Hero - Claim rewards for max closeness heroes
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct GiveRewardMaxClosenessHeroRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub hero_indices: Option<Vec<i32>>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GiveRewardMaxClosenessHeroResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_results: Option<Vec<CurrencyResultInfo3>>,
}

/// Handle give reward max closeness hero
pub async fn give_reward_max_closeness_hero(
    State(state): State<AppState>,
    Form(req): Form<GiveRewardMaxClosenessHeroRequest>,
) -> Result<Json<GiveRewardMaxClosenessHeroResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let _account_id = session.account_id;

    // For now, just return success - rewards would be distributed based on hero indices
    // In a full implementation, this would give items/currency based on the heroes
    Ok(Json(GiveRewardMaxClosenessHeroResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        currency_results: None,
    }))
}

// ============================================================================
// Request Hero Inn Roulette - Get roulette info and rewards
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestHeroInnRouletteRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub roulette_index: Option<String>,  // Client sends as string
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct RequestHeroInnRouletteResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub roulette_info: Option<RouletteInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    #[serde(rename = "RouletteRewardInfo")]
    pub roulette_reward_info: Option<Vec<RouletteRewardInfo>>,
}

/// Handle request for hero inn roulette info
pub async fn request_hero_inn_roulette(
    State(state): State<AppState>,
    Form(req): Form<RequestHeroInnRouletteRequest>,
) -> Result<Json<RequestHeroInnRouletteResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let roulette_index: i32 = req.roulette_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(1);

    // Get today's date for spin tracking
    let today = Utc::now().format("%Y-%m-%d").to_string();

    // Calculate max spins: 1 base + number of owned heroes visiting the inn
    // Get the selected heroes in the inn
    let friendly_row = sqlx::query("SELECT selected_hero_indices FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let selected_hero_indices: Vec<i32> = friendly_row
        .and_then(|row| row.get::<Option<String>, _>("selected_hero_indices"))
        .map(|s| s.split(',').filter_map(|x| x.parse().ok()).collect())
        .unwrap_or_default();
    
    // Get owned heroes
    let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&state.db)
        .await?
        .iter()
        .map(|row| row.get("hero_index"))
        .collect();
    
    // Count how many heroes in the inn are owned
    let owned_heroes_in_inn = selected_hero_indices.iter()
        .filter(|h| owned_heroes.contains(h))
        .count() as i32;
    
    // Max spins = 1 base + owned heroes in inn
    let max_spins = 1 + owned_heroes_in_inn;

    // Check how many spins the player has done today
    let spins_today: i32 = sqlx::query_scalar::<_, i32>(
        "SELECT COALESCE(spin_count, 0) FROM hero_inn_roulette_spins WHERE account_id = ? AND spin_date = ?"
    )
    .bind(account_id)
    .bind(&today)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    let remain_count = (max_spins - spins_today).max(0);

    let roulette_info = RouletteInfo {
        roulette_info_index: roulette_index,
        remain_count,
        max_count: max_spins,
    };

    let roulette_rewards = get_default_roulette_rewards(roulette_index);

    let response = RequestHeroInnRouletteResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        roulette_info: Some(roulette_info),
        roulette_reward_info: Some(roulette_rewards),
    };

    Ok(Json(response))
}

// ============================================================================
// Give Reward Hero Inn Roulette - Spin the roulette and give rewards
// ============================================================================

#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GiveRewardHeroInnRouletteRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub roulette_index: Option<String>,  // Client sends as string
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GiveRewardHeroInnRouletteResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub roulette_info: Option<RouletteInfo>,
    pub section: i32,  // The winning section (0-7)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_results: Option<Vec<CurrencyResultInfo3>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result: Option<StaminaResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendship_point_result: Option<FriendshipPointResultInfo>,
}

/// Handle give reward hero inn roulette (spin the wheel)
pub async fn give_reward_hero_inn_roulette(
    State(state): State<AppState>,
    Form(req): Form<GiveRewardHeroInnRouletteRequest>,
) -> Result<Json<GiveRewardHeroInnRouletteResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let roulette_index: i32 = req.roulette_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(1);

    // Get today's date for spin tracking
    let today = Utc::now().format("%Y-%m-%d").to_string();

    // Calculate max spins: 1 base + number of owned heroes visiting the inn
    let friendly_row = sqlx::query("SELECT selected_hero_indices FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let selected_hero_indices: Vec<i32> = friendly_row
        .and_then(|row| row.get::<Option<String>, _>("selected_hero_indices"))
        .map(|s| s.split(',').filter_map(|x| x.parse().ok()).collect())
        .unwrap_or_default();
    
    let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&state.db)
        .await?
        .iter()
        .map(|row| row.get("hero_index"))
        .collect();
    
    let owned_heroes_in_inn = selected_hero_indices.iter()
        .filter(|h| owned_heroes.contains(h))
        .count() as i32;
    
    let max_spins = 1 + owned_heroes_in_inn;

    // Check how many spins the player has done today
    let spins_today: i32 = sqlx::query_scalar::<_, i32>(
        "SELECT COALESCE(spin_count, 0) FROM hero_inn_roulette_spins WHERE account_id = ? AND spin_date = ?"
    )
    .bind(account_id)
    .bind(&today)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    if spins_today >= max_spins {
        return Ok(Json(GiveRewardHeroInnRouletteResponse {
            base_result: "Success".to_string(),
            result: "NoRemainCount".to_string(),
            roulette_info: Some(RouletteInfo {
                roulette_info_index: roulette_index,
                remain_count: 0,
                max_count: max_spins,
            }),
            section: -1,
            currency_results: None,
            stamina_result: None,
            friendship_point_result: None,
        }));
    }

    // Update spin count
    sqlx::query(
        "INSERT INTO hero_inn_roulette_spins (account_id, spin_date, spin_count) 
         VALUES (?, ?, 1) 
         ON CONFLICT(account_id, spin_date) DO UPDATE SET spin_count = spin_count + 1"
    )
    .bind(account_id)
    .bind(&today)
    .execute(&state.db)
    .await?;

    let new_spins_today = spins_today + 1;
    let remain_count = (max_spins - new_spins_today).max(0);

    // Randomly select a winning section (1-8, 1-based for client)
    // Note: Generate random before any await points to satisfy Send trait
    let winning_section: i32 = {
        let mut rng = rand::thread_rng();
        rng.gen_range(1..=8)  // 1-based: 1, 2, 3, 4, 5, 6, 7, 8
    };

    // Get the reward for this section (section is 1-based)
    let rewards = get_default_roulette_rewards(roulette_index);
    let winning_reward = rewards.iter().find(|r| r.section == winning_section).cloned();

    let mut currency_results: Vec<CurrencyResultInfo3> = Vec::new();
    let mut friendship_point_result: Option<FriendshipPointResultInfo> = None;

    if let Some(reward) = winning_reward {
        let amount = reward.value1 as i64;
        
        match reward.type1.as_str() {
            "Gold" => {
                // Get current gold and update
                let current_gold: i64 = sqlx::query_scalar::<_, i64>(
                    "SELECT COALESCE(gold, 0) FROM user_info WHERE account_id = ?"
                )
                .bind(account_id)
                .fetch_optional(&state.db)
                .await?
                .unwrap_or(0);

                let new_gold = current_gold + amount;
                sqlx::query("UPDATE user_info SET gold = ? WHERE account_id = ?")
                    .bind(new_gold)
                    .bind(account_id)
                    .execute(&state.db)
                    .await?;

                currency_results.push(CurrencyResultInfo3 {
                    currency_type: "Gold".to_string(),
                    add_value: amount,
                    new_value: new_gold,
                    ..Default::default()
                });
            }
            "Gem" => {
                // Get current ruby and update
                let current_ruby: i64 = sqlx::query_scalar::<_, i64>(
                    "SELECT COALESCE(ruby, 0) FROM user_info WHERE account_id = ?"
                )
                .bind(account_id)
                .fetch_optional(&state.db)
                .await?
                .unwrap_or(0);

                let new_ruby = current_ruby + amount;
                sqlx::query("UPDATE user_info SET ruby = ? WHERE account_id = ?")
                    .bind(new_ruby)
                    .bind(account_id)
                    .execute(&state.db)
                    .await?;

                currency_results.push(CurrencyResultInfo3 {
                    currency_type: "Gem".to_string(),
                    add_value: amount,
                    new_value: new_ruby,
                    ..Default::default()
                });
            }
            "FriendshipPoint" => {
                // Get current friendship points and update
                let current_fp: i64 = sqlx::query_scalar::<_, i64>(
                    "SELECT COALESCE(friendship_point, 0) FROM user_info WHERE account_id = ?"
                )
                .bind(account_id)
                .fetch_optional(&state.db)
                .await?
                .unwrap_or(0);

                let new_fp = current_fp + amount;
                sqlx::query("UPDATE user_info SET friendship_point = ? WHERE account_id = ?")
                    .bind(new_fp)
                    .bind(account_id)
                    .execute(&state.db)
                    .await?;

                friendship_point_result = Some(FriendshipPointResultInfo {
                    add_value: amount,
                    add_daily_acc_value: 0,
                    new_value: new_fp,
                    new_daily_acc_value: 0,
                });
            }
            _ => {
                tracing::warn!("Unknown reward type: {}", reward.type1);
            }
        }
    }

    let roulette_info = RouletteInfo {
        roulette_info_index: roulette_index,
        remain_count,
        max_count: max_spins,
    };

    // Section is already 1-based (1-8) for the client
    let response = GiveRewardHeroInnRouletteResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        roulette_info: Some(roulette_info),
        section: winning_section,  // Already 1-based
        currency_results: if currency_results.is_empty() { None } else { Some(currency_results) },
        stamina_result: None,
        friendship_point_result,
    };

    Ok(Json(response))
}

