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
// The tutorial system in sprk is complex and involves multiple steps.
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
    let account_id = session.account_id;
    let completed_time = state.server_time_str();

    tracing::info!("Begin tutorial {} for account {}", tutorial_index, account_id);

    // Some tutorials indicate that a battle just finished
    // We handle dungeon completion here because complete_tutorial may not be called
    // for tutorials that get stuck or have complex sequences
    //
    // Pattern observed:
    //   - 10110 begins after battle 1-2 ends → should mark 1-2 complete, unlock 1-3
    //   - 10220 begins after battle 1-4 ends → should mark 1-4 complete, unlock 1-5
    //   - etc.
    //
    // We use the tutorial table to determine what dungeon is being unlocked,
    // then mark the previous dungeon as complete.
    
    if let Some(reward) = state.tables.tutorials.get_reward_dungeon(tutorial_index) {
        let unlock_chapter = reward.chapter_index;
        let unlock_dungeon = reward.dungeon_index;
        
        // Calculate the previous dungeon that should be completed
        if unlock_dungeon > 1 {
            let prev_chapter = unlock_chapter;
            let prev_dungeon = unlock_dungeon - 1;
            
            tracing::info!(
                "Begin tutorial {}: Marking dungeon {}-{} as complete (unlocking {}-{})",
                tutorial_index, prev_chapter, prev_dungeon, unlock_chapter, unlock_dungeon
            );
            
            // Mark previous dungeon as completed
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
                 VALUES (?, ?, ?, 1, 3, 1, ?)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET clear_count = MAX(clear_count, 1), best_star = MAX(best_star, 3), completed_time = COALESCE(completed_time, ?)"
            )
            .bind(account_id)
            .bind(prev_chapter)
            .bind(prev_dungeon)
            .bind(&completed_time)
            .bind(&completed_time)
            .execute(&state.db)
            .await
            .ok(); // Ignore errors, this is opportunistic
            
            // Unlock the new dungeon
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
                 VALUES (?, ?, ?, 1)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET is_unlocked = 1"
            )
            .bind(account_id)
            .bind(unlock_chapter)
            .bind(unlock_dungeon)
            .execute(&state.db)
            .await
            .ok();
        }
    }

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

    // Handle dungeon unlocking based on tutorial index using the table data
    // TutorialTable.json has RewardDungeonIndex which tells us what dungeon to unlock
    let mut dungeon_infos: Vec<ChapterDungeonInfo> = Vec::new();
    
    // Helper to create a dungeon info
    // 
    // MaxStar encoding: difficulty * 10 + stars
    //   - Chapter 1 has MinDifficulty = Normal (1), not Easy (0)!
    //   - So for Normal 3-star: MaxStar = 1*10 + 3 = 13
    //   - The client checks: (MaxStar / 10) >= MinDifficulty to verify completion
    //
    // FirstRewardedDiff is a bitmask for which difficulties have been first-cleared:
    //   - Bit 0 (value 1) = Easy cleared
    //   - Bit 1 (value 2) = Normal cleared  
    //   - Bit 2 (value 4) = Hard cleared
    //   - Bit 3 (value 8) = Hell cleared
    //   - For chapter 1 (min difficulty = Normal), use value 2
    //
    let make_dungeon_info = |chapter: i32, dungeon: i32, max_star: i16, completed: bool| {
        ChapterDungeonInfo {
            chapter_index: chapter,
            dungeon_index: dungeon,
            max_star,
            // If completed on Normal (chapter 1 min difficulty), set Normal bit (2)
            first_rewarded_diff: if completed { 2 } else { 0 },
            scenario_complete: if completed { 1 } else { 0 },
            visited_time: if completed { Some(state.server_time_str()) } else { None },
            completed_time: if completed { Some(state.server_time_str()) } else { None },
            daily_completed_count: if completed { 1 } else { 0 },
            reset_count: 0,
        }
    };
    
    // Detect post-battle tutorials that need special handling
    // Pattern: 10X10 where X is the dungeon number minus 1
    //   10010 = after battle 1-2
    //   10110 = after battle 1-3
    //   10210 = after battle 1-4
    //   10310 = after battle 1-5, etc.
    // These fire AFTER completing a battle and should mark that dungeon as complete
    let is_post_battle_tutorial = tutorial_index >= 10010 && 
                                   tutorial_index < 20000 && 
                                   (tutorial_index % 100) == 10;
    
    if is_post_battle_tutorial {
        // Extract dungeon number from tutorial index
        // 10010 -> dungeon 2, 10110 -> dungeon 3, etc.
        let dungeon_offset = (tutorial_index - 10010) / 100;
        let completed_dungeon = (dungeon_offset + 2) as i32;
        let next_dungeon = completed_dungeon + 1;
        
        // Mark the just-completed dungeon as complete
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
             VALUES (?, 1, ?, 1, 3, 1, ?)
             ON CONFLICT(account_id, chapter_id, dungeon_id) 
             DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
        )
        .bind(account_id)
        .bind(completed_dungeon)
        .bind(&completed_time)
        .bind(&completed_time)
        .execute(&state.db)
        .await?;
        
        // Unlock the next dungeon
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
             VALUES (?, 1, ?, 1)
             ON CONFLICT(account_id, chapter_id, dungeon_id) 
             DO UPDATE SET is_unlocked = 1"
        )
        .bind(account_id)
        .bind(next_dungeon)
        .execute(&state.db)
        .await?;
        
        // Return dungeon infos - next dungeon unlocked FIRST, then completed dungeon LAST
        // MaxStar = 13 = Normal (1) * 10 + 3 stars (chapter 1 min difficulty is Normal)
        dungeon_infos.push(make_dungeon_info(1, next_dungeon, 0, false));
        dungeon_infos.push(make_dungeon_info(1, completed_dungeon, 13, true));
    }
    // Look up tutorial in the table to see if it unlocks a dungeon
    else if let Some(reward) = state.tables.tutorials.get_reward_dungeon(tutorial_index) {
        let unlock_chapter = reward.chapter_index;
        let unlock_dungeon = reward.dungeon_index;
        
        // Calculate the previous dungeon that should be completed
        let prev_dungeon = if unlock_dungeon > 1 {
            Some((unlock_chapter, unlock_dungeon - 1))
        } else if unlock_chapter > 1 {
            // Previous chapter's last dungeon (assuming 12 dungeons per chapter)
            Some((unlock_chapter - 1, 12))
        } else {
            // Dungeon 1-1 has no previous, it's just being unlocked
            None
        };
        
        // Mark previous dungeon as completed (if any)
        if let Some((prev_chapter, prev_dung)) = prev_dungeon {
            sqlx::query(
                "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
                 VALUES (?, ?, ?, 1, 3, 1, ?)
                 ON CONFLICT(account_id, chapter_id, dungeon_id) 
                 DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
            )
            .bind(account_id)
            .bind(prev_chapter)
            .bind(prev_dung)
            .bind(&completed_time)
            .bind(&completed_time)
            .execute(&state.db)
            .await?;
            
            // Add completed dungeon info (will be added LAST for positioning)
            // MaxStar = 13 = Normal (1) * 10 + 3 stars
            dungeon_infos.push(make_dungeon_info(prev_chapter, prev_dung, 13, true));
        }
        
        // Unlock the new dungeon
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked) 
             VALUES (?, ?, ?, 1)
             ON CONFLICT(account_id, chapter_id, dungeon_id) 
             DO UPDATE SET is_unlocked = 1"
        )
        .bind(account_id)
        .bind(unlock_chapter)
        .bind(unlock_dungeon)
        .execute(&state.db)
        .await?;
        
        // Add unlocked dungeon info FIRST
        // Reorder: unlocked dungeon first, then completed dungeon last
        // This is because client positions character at the LAST dungeon
        if !dungeon_infos.is_empty() {
            let completed_dungeon = dungeon_infos.remove(0);
            dungeon_infos.insert(0, make_dungeon_info(unlock_chapter, unlock_dungeon, 0, false));
            dungeon_infos.push(completed_dungeon);
        } else {
            // No previous dungeon (e.g., tutorial 10000 just unlocks 1-1)
            dungeon_infos.push(make_dungeon_info(unlock_chapter, unlock_dungeon, 0, false));
        }
    } else {
        // Tutorial not in table, check if it's a known post-battle tutorial
        // Tutorial 10002 fires after battle 1-1 ends (observed behavior)
        // It's not in the table but we need to handle it
        if tutorial_index == 10002 {
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
            
            // Return dungeon infos - 1-2 unlocked FIRST, then 1-1 completed LAST
            // MaxStar = 13 = Normal (1) * 10 + 3 stars (chapter 1 min difficulty is Normal)
            dungeon_infos.push(make_dungeon_info(1, 2, 0, false));
            dungeon_infos.push(make_dungeon_info(1, 1, 13, true));
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
