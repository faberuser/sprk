use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    state::AppState,
};

// ============================================================================
// TUTORIAL SYSTEM DOCUMENTATION
// ============================================================================
//
// The tutorial system in King's Raid is complex and involves multiple steps.
// Each tutorial is identified by an index (e.g., 10000, 10010, etc.)
//
// TUTORIAL FLOW (from TutorialTable.json):
// ----------------------------------------
// Tutorial 10000 (Seq 221): RewardDungeonIndex [1,1] -> Unlocks dungeon 1-1
// Tutorial 17010 (Seq 54):  RewardDungeonIndex [1,1] -> Also unlocks 1-1 (alt path)
// Tutorial 10010 (Seq 89):  RewardDungeonIndex [1,2] -> Unlocks 1-2 (implies 1-1 complete)
// Tutorial 10110 (Seq 86):  RewardDungeonIndex [1,3] -> Unlocks 1-3 (implies 1-2 complete)
// Tutorial 10202 (Seq 9):   End of basic tutorial
// Tutorial 10220 (Seq 28):  RewardDungeonIndex [1,5] -> Unlocks 1-5
// Tutorial 10230 (Seq 27):  RewardDungeonIndex [1,6] -> Unlocks 1-6
// Tutorial 10300 (Seq 52):  RewardDungeonIndex [1,7] -> Unlocks 1-7
// Tutorial 10310 (Seq 36):  RewardDungeonIndex [1,8] -> Unlocks 1-8
// Tutorial 10400 (Seq 90):  RewardDungeonIndex [1,9] -> Unlocks 1-9
// Tutorial 10600 (Seq 43):  RewardDungeonIndex [1,11] -> Unlocks 1-11
//
// API ENDPOINTS:
// --------------
// POST /tutorial/begin_tutorial   - Called when a tutorial sequence starts
// POST /tutorial/complete_tutorial - Called when a tutorial sequence completes
//
// RESPONSE FORMAT:
// ----------------
// CompleteTutorial.Response includes:
//   - Info: TutorialInfo with the completed tutorial index
//   - DungeonInfos: Array of ChapterDungeonInfo for unlocked/completed dungeons
//   - CurrencyResults: Any gold/gem rewards
//   - ItemResults: Any item rewards
//
// CLIENT PROCESSING:
// ------------------
// In TutorialManager.coResponseCompleteTutorial(), the client:
//   1. Adds the tutorial to the completed list
//   2. Applies DungeonInfos via CampaignManager.Apply()
//   3. Applies currency/item rewards
//
// DUNGEON COMPLETION CHECK (client-side):
// ----------------------------------------
// DungeonCompleteChecker.CheckDefault() checks:
//   - ChapterDungeonInfo exists for the dungeon
//   - CompletedTime is not empty
//   - MaxStar / 10 >= difficulty (MaxStar encodes difficulty*10 + stars)
//
// CURRENT STATUS:
// ---------------
// The tutorial is currently SKIPPED (see user.rs tutorial_skip = true).
// Dungeons are pre-populated in user.rs login handler instead.
// To re-enable tutorial, set tutorial_skip = !is_new_user in user.rs.
//
// ============================================================================

// ============================================================================
// Common Types matching client's NShared types
// ============================================================================

/// Tutorial info matching client's NShared.TutorialInfo
#[derive(Debug, Serialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialInfo {
    pub tutorial_index: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub completed_time: Option<String>,
}

/// Currency result info matching client's NShared.CurrencyResultInfo3
#[derive(Debug, Serialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo {
    pub currency_type: String,
    pub add_value: i64,
    pub new_value: i64,
}

/// Chapter dungeon info for tutorials that unlock dungeons
#[derive(Debug, Serialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct ChapterDungeonInfo {
    pub chapter_index: i32,
    pub dungeon_index: i32,
    pub max_star: i16,
    pub first_rewarded_diff: i16,
    pub scenario_complete: i16,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub visited_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub completed_time: Option<String>,
    pub daily_completed_count: i32,
    pub reset_count: i32,
}

// ============================================================================
// Begin Tutorial API - Called when a tutorial starts
// ============================================================================

/// Begin tutorial request matching client's NShared.BeginTutorial.Request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct BeginTutorialRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub tutorial_index: Option<i32>,
}

/// Begin tutorial response matching client's NShared.BeginTutorial.Response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BeginTutorialResponse {
    pub base_result: String,
    pub result: String,
}

/// Handle begin tutorial request
/// This is called when the client starts a tutorial sequence
pub async fn begin_tutorial(
    State(state): State<AppState>,
    Form(req): Form<BeginTutorialRequest>,
) -> Result<Json<BeginTutorialResponse>> {
    // Client may send SessionKey instead of SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    let tutorial_index = req.tutorial_index.unwrap_or(0);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    tracing::info!("Begin tutorial {} for account {}", tutorial_index, session.account_id);

    // Just acknowledge the tutorial start - we track completion, not start
    Ok(Json(BeginTutorialResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
    }))
}

// ============================================================================
// Complete Tutorial API - Called when a tutorial completes
// ============================================================================

/// Complete tutorial request matching client's NShared.CompleteTutorial.Request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct CompleteTutorialRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub tutorial_index: Option<i32>,
}

/// Complete tutorial response matching client's NShared.CompleteTutorial.Response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct CompleteTutorialResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub info: Option<TutorialInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_results: Option<Vec<CurrencyResultInfo>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub item_results: Option<Vec<serde_json::Value>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub equip_item_results: Option<Vec<serde_json::Value>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hero_infos: Option<Vec<serde_json::Value>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub team_exp_result_infos: Option<Vec<serde_json::Value>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result_infos: Option<Vec<serde_json::Value>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub dungeon_infos: Option<Vec<ChapterDungeonInfo>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub free_equip_gacha_info: Option<serde_json::Value>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub equip_gacha_info: Option<serde_json::Value>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub opend_mission_categories: Option<Vec<serde_json::Value>>,
}

/// Handle complete tutorial request
/// This is called when the client finishes a tutorial sequence
pub async fn complete_tutorial(
    State(state): State<AppState>,
    Form(req): Form<CompleteTutorialRequest>,
) -> Result<Json<CompleteTutorialResponse>> {
    // Client may send SessionKey instead of SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    let tutorial_index = req.tutorial_index.unwrap_or(0);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    tracing::info!("Complete tutorial {} for account {}", tutorial_index, session.account_id);

    let completed_time = state.server_time_str();
    let account_id = session.account_id;

    // Check if already completed
    let existing = sqlx::query(
        "SELECT is_completed FROM tutorial_progress WHERE account_id = ? AND tutorial_index = ?"
    )
    .bind(account_id)
    .bind(tutorial_index)
    .fetch_optional(&state.db)
    .await?;

    if let Some(row) = existing {
        if row.get::<i32, _>("is_completed") != 0 {
            // Already completed, just return success
            return Ok(Json(CompleteTutorialResponse {
                base_result: "Success".to_string(),
                result: "Success".to_string(),
                info: Some(TutorialInfo {
                    tutorial_index,
                    completed_time: Some(completed_time),
                }),
                currency_results: None,
                item_results: None,
                equip_item_results: None,
                hero_infos: None,
                team_exp_result_infos: None,
                stamina_result_infos: None,
                dungeon_infos: None,
                free_equip_gacha_info: None,
                equip_gacha_info: None,
                opend_mission_categories: None,
            }));
        }

        // Update as completed
        sqlx::query("UPDATE tutorial_progress SET is_completed = 1, completed_time = ? WHERE account_id = ? AND tutorial_index = ?")
            .bind(&completed_time)
            .bind(account_id)
            .bind(tutorial_index)
            .execute(&state.db)
            .await?;
    } else {
        // Insert new tutorial as completed
        sqlx::query(
            "INSERT INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, ?, 1, ?)"
        )
        .bind(account_id)
        .bind(tutorial_index)
        .bind(&completed_time)
        .execute(&state.db)
        .await?;
    }

    // Handle dungeon unlocking based on tutorial index
    // Tutorial indices and their meaning (from TutorialTable.json RewardDungeonIndex):
    // 10000 = Tutorial start -> unlock 1-1 (not completed, just available to play)
    // 10010 = 1-1 battle complete -> mark 1-1 completed, unlock 1-2
    // 10110 = 1-2 battle complete -> mark 1-2 completed, unlock 1-3
    // 10202 = 1-3 complete / tutorial end -> mark 1-3 completed, unlock 1-4
    let mut dungeon_infos: Vec<ChapterDungeonInfo> = Vec::new();
    
    // Helper to create a dungeon info
    // FirstRewardedDiff is a bitmask: bit 0 = Easy, bit 1 = Normal, bit 2 = Hard##
    // So Easy first clear = 1, Normal first clear = 2, Hard first clear = 4
    // For a completed dungeon on Easy (which is what tutorial uses), FirstRewardedDiff = 1
    let make_dungeon_info = |chapter: i32, dungeon: i32, max_star: i16, completed: bool| {
        ChapterDungeonInfo {
            chapter_index: chapter,
            dungeon_index: dungeon,
            max_star,
            // If completed, set Easy bit (1). If not completed, 0 (no first rewards yet)
            first_rewarded_diff: if completed { 1 } else { 0 },
            scenario_complete: if completed { 1 } else { 0 },
            visited_time: if completed { Some(state.server_time_str()) } else { None },
            completed_time: if completed { Some(state.server_time_str()) } else { None },
            daily_completed_count: if completed { 1 } else { 0 },
            reset_count: 0,
        }
    };
    
    match tutorial_index {
        10000 | 17010 => {
            // Tutorial start - unlock 1-1 (available to play, but NOT completed yet)
            tracing::info!("Tutorial {}: Unlocking 1-1 (not completed)", tutorial_index);
            
            // Insert 1-1 as unlocked but not completed
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) 
                 VALUES (?, 1, 1, 0, 0, 1)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET is_unlocked = 1"
            )
            .bind(account_id)
            .execute(&state.db)
            .await?;
            
            // Return dungeon info for 1-1 (unlocked, not completed)
            dungeon_infos.push(make_dungeon_info(1, 1, 0, false));
        }
        10010 => {
            // 1-1 complete - mark as completed with 3 stars and unlock 1-2
            tracing::info!("Tutorial 10010: Marking 1-1 as complete, unlocking 1-2");
            
            // Update/insert 1-1 as completed
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
                 VALUES (?, 1, 1, 1, 3, 1, ?)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
            )
            .bind(account_id)
            .bind(&completed_time)
            .bind(&completed_time)
            .execute(&state.db)
            .await?;
            
            // Unlock 1-2
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
                 VALUES (?, 1, 2, 1)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET is_unlocked = 1"
            )
            .bind(account_id)
            .execute(&state.db)
            .await?;
            
            // Return dungeon infos for 1-1 (completed) and 1-2 (unlocked)
            dungeon_infos.push(make_dungeon_info(1, 1, 3, true));
            dungeon_infos.push(make_dungeon_info(1, 2, 0, false));
        }
        10110 => {
            // 1-2 complete - mark as completed with 3 stars and unlock 1-3
            tracing::info!("Tutorial 10110: Marking 1-2 as complete, unlocking 1-3");
            
            // Update/insert 1-2 as completed
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
                 VALUES (?, 1, 2, 1, 3, 1, ?)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
            )
            .bind(account_id)
            .bind(&completed_time)
            .bind(&completed_time)
            .execute(&state.db)
            .await?;
            
            // Unlock 1-3
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
                 VALUES (?, 1, 3, 1)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET is_unlocked = 1"
            )
            .bind(account_id)
            .execute(&state.db)
            .await?;
            
            // Return dungeon infos for 1-2 (completed) and 1-3 (unlocked)
            dungeon_infos.push(make_dungeon_info(1, 2, 3, true));
            dungeon_infos.push(make_dungeon_info(1, 3, 0, false));
        }
        10202 => {
            // 1-3 complete - mark as completed with 3 stars and unlock 1-4
            tracing::info!("Tutorial 10202: Marking 1-3 as complete, unlocking 1-4");
            
            // Update/insert 1-3 as completed
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
                 VALUES (?, 1, 3, 1, 3, 1, ?)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
            )
            .bind(account_id)
            .bind(&completed_time)
            .bind(&completed_time)
            .execute(&state.db)
            .await?;
            
            // Unlock 1-4
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
                 VALUES (?, 1, 4, 1)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET is_unlocked = 1"
            )
            .bind(account_id)
            .execute(&state.db)
            .await?;
            
            // Return dungeon infos for 1-3 (completed) and 1-4 (unlocked)
            dungeon_infos.push(make_dungeon_info(1, 3, 3, true));
            dungeon_infos.push(make_dungeon_info(1, 4, 0, false));
        }
        _ => {
            // Other tutorials don't unlock dungeons
        }
    }

    // Return tutorial completion info with dungeon infos if any
    let response = CompleteTutorialResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        info: Some(TutorialInfo {
            tutorial_index,
            completed_time: Some(completed_time),
        }),
        currency_results: None,
        item_results: None,
        equip_item_results: None,
        hero_infos: None,
        team_exp_result_infos: None,
        stamina_result_infos: None,
        dungeon_infos: if dungeon_infos.is_empty() { None } else { Some(dungeon_infos.clone()) },
        free_equip_gacha_info: None,
        equip_gacha_info: None,
        opend_mission_categories: None,
    };
    
    // Log the dungeon infos we're returning
    if !dungeon_infos.is_empty() {
        tracing::info!("Returning dungeon_infos: {:?}", dungeon_infos);
        // Log the full JSON response to verify field names
        tracing::info!("Full JSON response: {}", serde_json::to_string(&response).unwrap_or_default());
    }
    
    Ok(Json(response))
}

// ============================================================================
// Legacy/Helper Tutorial APIs
// ============================================================================

/// Get tutorial progress request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetTutorialProgressRequest {
    pub session_id: Option<String>,
}

/// Get tutorial progress response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetTutorialProgressResponse {
    pub base_result: String,
    pub result: String,
    pub tutorials: Vec<TutorialInfo>,
}

/// Handle get tutorial progress request
pub async fn get_tutorial_progress(
    State(state): State<AppState>,
    Form(req): Form<GetTutorialProgressRequest>,
) -> Result<Json<GetTutorialProgressResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get completed tutorials
    let progress = sqlx::query(
        "SELECT tutorial_index, completed_time FROM tutorial_progress WHERE account_id = ? AND is_completed = 1 ORDER BY tutorial_index"
    )
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    let tutorials: Vec<TutorialInfo> = progress.iter()
        .map(|row| TutorialInfo {
            tutorial_index: row.get("tutorial_index"),
            completed_time: row.get::<Option<String>, _>("completed_time"),
        })
        .collect();

    Ok(Json(GetTutorialProgressResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        tutorials,
    }))
}

/// Skip tutorial request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SkipTutorialRequest {
    pub session_id: Option<String>,
}

/// Skip tutorial response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SkipTutorialResponse {
    pub base_result: String,
    pub result: String,
}

/// Handle skip tutorial request - marks first tutorial as complete to skip intro
pub async fn skip_tutorial(
    State(state): State<AppState>,
    Form(req): Form<SkipTutorialRequest>,
) -> Result<Json<SkipTutorialResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let completed_time = state.server_time_str();

    // Mark tutorial index 1 as completed (the intro tutorial)
    // This will cause IsTutorialSkip to be true on next login
    sqlx::query(
        "INSERT OR REPLACE INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, 1, 1, ?)"
    )
    .bind(session.account_id)
    .bind(&completed_time)
    .execute(&state.db)
    .await?;

    Ok(Json(SkipTutorialResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
    }))
}

/// Get tutorial reward request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct GetTutorialRewardRequest {
    pub session_id: Option<String>,
    pub tutorial_index: Option<i32>,
}

/// Get tutorial reward response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetTutorialRewardResponse {
    pub base_result: String,
    pub result: String,
    pub tutorial_index: i32,
    pub reward_gold: i64,
    pub reward_gem: i32,
}

/// Handle get tutorial reward info request
pub async fn get_tutorial_reward(
    State(_state): State<AppState>,
    Form(req): Form<GetTutorialRewardRequest>,
) -> Result<Json<GetTutorialRewardResponse>> {
    let tutorial_index = req.tutorial_index.unwrap_or(1);

    // Define rewards for specific tutorial indices
    // Most tutorials don't give rewards - rewards come from dungeon completion, etc.
    let (reward_gold, reward_gem) = match tutorial_index {
        1 => (1000, 0),  // First tutorial
        _ => (0, 0),     // Most tutorials don't give direct rewards
    };

    Ok(Json(GetTutorialRewardResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        tutorial_index,
        reward_gold,
        reward_gem,
    }))
}
