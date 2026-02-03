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

/// Achievement info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct AchievementInfo {
    pub achievement_id: i64,
    pub category: i32,
    pub current_value: i64,
    pub target_value: i64,
    pub is_completed: bool,
    pub is_rewarded: bool,
}

/// Get achievement list request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetAchievementListRequest {
    pub session_id: Option<String>,
    pub category: Option<i32>,
}

/// Get achievement list response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetAchievementListResponse {
    pub base_result: i32,
    pub achievements: Vec<AchievementInfo>,
    pub total_completed: i32,
}

/// Handle get achievement list request
pub async fn get_achievement_list(
    State(state): State<AppState>,
    Form(req): Form<GetAchievementListRequest>,
) -> Result<Json<GetAchievementListResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get achievements from database
    let achievements = if let Some(category) = req.category {
        sqlx::query(
            "SELECT achievement_id, category, current_value, target_value, is_completed, is_rewarded FROM achievements WHERE account_id = ? AND category = ?"
        )
        .bind(session.account_id)
        .bind(category)
        .fetch_all(&state.db)
        .await?
    } else {
        sqlx::query(
            "SELECT achievement_id, category, current_value, target_value, is_completed, is_rewarded FROM achievements WHERE account_id = ?"
        )
        .bind(session.account_id)
        .fetch_all(&state.db)
        .await?
    };

    let achievements: Vec<AchievementInfo> = achievements.iter().map(|row| AchievementInfo {
        achievement_id: row.get("achievement_id"),
        category: row.get("category"),
        current_value: row.get("current_value"),
        target_value: row.get("target_value"),
        is_completed: row.get::<i32, _>("is_completed") != 0,
        is_rewarded: row.get::<i32, _>("is_rewarded") != 0,
    }).collect();

    let total_completed = achievements.iter().filter(|a| a.is_completed).count() as i32;

    // If no achievements exist, create default ones
    if achievements.is_empty() {
        // Create default achievements for new player
        let default_achievements = create_default_achievements(session.account_id, &state).await?;
        
        return Ok(Json(GetAchievementListResponse {
            base_result: BaseResultType::Success as i32,
            achievements: default_achievements,
            total_completed: 0,
        }));
    }

    Ok(Json(GetAchievementListResponse {
        base_result: BaseResultType::Success as i32,
        achievements,
        total_completed,
    }))
}

/// Create default achievements for a new player
async fn create_default_achievements(account_id: i64, state: &AppState) -> Result<Vec<AchievementInfo>> {
    let default_achievements = vec![
        (1001, 0, 10, "Complete 10 dungeons"),
        (1002, 0, 50, "Complete 50 dungeons"),
        (1003, 0, 100, "Complete 100 dungeons"),
        (2001, 1, 5, "Collect 5 heroes"),
        (2002, 1, 10, "Collect 10 heroes"),
        (2003, 1, 20, "Collect 20 heroes"),
        (3001, 2, 10, "Reach level 10"),
        (3002, 2, 30, "Reach level 30"),
        (3003, 2, 50, "Reach level 50"),
    ];

    let mut achievements = vec![];

    for (id, category, target, _name) in default_achievements {
        sqlx::query(
            "INSERT OR IGNORE INTO achievements (account_id, achievement_id, category, current_value, target_value, is_completed, is_rewarded) VALUES (?, ?, ?, 0, ?, 0, 0)"
        )
        .bind(account_id)
        .bind(id)
        .bind(category)
        .bind(target)
        .execute(&state.db)
        .await?;

        achievements.push(AchievementInfo {
            achievement_id: id,
            category,
            current_value: 0,
            target_value: target,
            is_completed: false,
            is_rewarded: false,
        });
    }

    Ok(achievements)
}

/// Receive achievement reward request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAchievementRewardRequest {
    pub session_id: Option<String>,
    pub achievement_id: Option<i64>,
}

/// Receive achievement reward response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct ReceiveAchievementRewardResponse {
    pub base_result: i32,
    pub result: i32,
    pub reward_gold: i64,
    pub reward_gem: i32,
    pub reward_items: Vec<RewardItemInfo>,
}

/// Handle receive achievement reward request
pub async fn receive_achievement_reward(
    State(state): State<AppState>,
    Form(req): Form<ReceiveAchievementRewardRequest>,
) -> Result<Json<ReceiveAchievementRewardResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let achievement_id = req.achievement_id.ok_or_else(|| ServerError::InvalidRequest("Missing achievement_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get achievement
    let achievement = sqlx::query(
        "SELECT is_completed, is_rewarded, target_value FROM achievements WHERE account_id = ? AND achievement_id = ?"
    )
    .bind(session.account_id)
    .bind(achievement_id)
    .fetch_optional(&state.db)
    .await?;

    let achievement = achievement.ok_or_else(|| ServerError::NotFound("Achievement not found".to_string()))?;
    
    let is_completed: bool = achievement.get::<i32, _>("is_completed") != 0;
    let is_rewarded: bool = achievement.get::<i32, _>("is_rewarded") != 0;

    if !is_completed {
        return Ok(Json(ReceiveAchievementRewardResponse {
            base_result: BaseResultType::Success as i32,
            result: 1, // Not completed
            reward_gold: 0,
            reward_gem: 0,
            reward_items: vec![],
        }));
    }

    if is_rewarded {
        return Ok(Json(ReceiveAchievementRewardResponse {
            base_result: BaseResultType::Success as i32,
            result: 2, // Already rewarded
            reward_gold: 0,
            reward_gem: 0,
            reward_items: vec![],
        }));
    }

    // Calculate reward based on achievement
    let target_value: i64 = achievement.get("target_value");
    let reward_gold = target_value * 100;
    let reward_gem = (target_value / 10) as i32;

    // Mark as rewarded
    sqlx::query("UPDATE achievements SET is_rewarded = 1 WHERE account_id = ? AND achievement_id = ?")
        .bind(session.account_id)
        .bind(achievement_id)
        .execute(&state.db)
        .await?;

    // Add rewards
    sqlx::query("UPDATE user_info SET gold = gold + ?, gem = gem + ? WHERE account_id = ?")
        .bind(reward_gold)
        .bind(reward_gem)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(ReceiveAchievementRewardResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        reward_gold,
        reward_gem,
        reward_items: vec![],
    }))
}

/// Update achievement progress request (internal, called by other handlers)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct UpdateAchievementProgressRequest {
    pub session_id: Option<String>,
    pub achievement_id: Option<i64>,
    pub increment: Option<i64>,
    pub set_value: Option<i64>,
}

/// Update achievement progress response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct UpdateAchievementProgressResponse {
    pub base_result: i32,
    pub achievement: AchievementInfo,
    pub newly_completed: bool,
}

/// Handle update achievement progress request
pub async fn update_achievement_progress(
    State(state): State<AppState>,
    Form(req): Form<UpdateAchievementProgressRequest>,
) -> Result<Json<UpdateAchievementProgressResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let achievement_id = req.achievement_id.ok_or_else(|| ServerError::InvalidRequest("Missing achievement_id".to_string()))?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get current achievement
    let achievement = sqlx::query(
        "SELECT current_value, target_value, is_completed FROM achievements WHERE account_id = ? AND achievement_id = ?"
    )
    .bind(session.account_id)
    .bind(achievement_id)
    .fetch_optional(&state.db)
    .await?;

    let achievement = achievement.ok_or_else(|| ServerError::NotFound("Achievement not found".to_string()))?;
    
    let current_value: i64 = achievement.get("current_value");
    let target_value: i64 = achievement.get("target_value");
    let was_completed: bool = achievement.get::<i32, _>("is_completed") != 0;

    // Calculate new value
    let new_value = if let Some(set) = req.set_value {
        set
    } else if let Some(inc) = req.increment {
        current_value + inc
    } else {
        current_value + 1
    };

    let is_completed = new_value >= target_value;
    let newly_completed = is_completed && !was_completed;

    // Update achievement
    sqlx::query("UPDATE achievements SET current_value = ?, is_completed = ? WHERE account_id = ? AND achievement_id = ?")
        .bind(new_value)
        .bind(if is_completed { 1 } else { 0 })
        .bind(session.account_id)
        .bind(achievement_id)
        .execute(&state.db)
        .await?;

    Ok(Json(UpdateAchievementProgressResponse {
        base_result: BaseResultType::Success as i32,
        achievement: AchievementInfo {
            achievement_id,
            category: 0,
            current_value: new_value,
            target_value,
            is_completed,
            is_rewarded: false,
        },
        newly_completed,
    }))
}
