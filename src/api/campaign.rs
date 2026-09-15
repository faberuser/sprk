use axum::{
    extract::{State, Form},
    body::Bytes,
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, ChapterDungeonInfo, equip::EquipItemInfo},
    state::AppState,
    tables::item::ItemTable,
};

/// Campaign chapter info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ChapterInfo {
    pub chapter_id: i32,
    pub is_unlocked: bool,
    pub clear_count: i32,
    pub dungeons: Vec<ChapterDungeonInfo>,
}

/// Get campaign info request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetCampaignInfoRequest {
    pub session_id: Option<String>,
}

/// Get campaign info response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetCampaignInfoResponse {
    pub base_result: i32,
    pub chapters: Vec<ChapterInfo>,
}

/// Handle get campaign info request
pub async fn get_campaign_info(
    State(state): State<AppState>,
    Form(req): Form<GetCampaignInfoRequest>,
) -> Result<Json<GetCampaignInfoResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Get campaign progress
    let progress_rows = sqlx::query(
        "SELECT chapter_id, dungeon_id, clear_count, best_star, is_unlocked FROM campaign_progress WHERE account_id = ? ORDER BY chapter_id, dungeon_id"
    )
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    // Group by chapter
    let mut chapters: Vec<ChapterInfo> = vec![];
    let mut current_chapter: Option<ChapterInfo> = None;

    for row in progress_rows {
        let chapter_id: i32 = row.get("chapter_id");
        let dungeon_id: i32 = row.get("dungeon_id");
        let clear_count: i32 = row.get("clear_count");
        let best_star: i32 = row.get("best_star");
        let is_unlocked: bool = row.get::<i32, _>("is_unlocked") != 0;

        let dungeon = ChapterDungeonInfo {
            dungeon_id,
            chapter_id,
            clear_count,
            best_star,
            first_clear_time: None,
        };

        match current_chapter.as_mut() {
            Some(ch) if ch.chapter_id == chapter_id => {
                ch.dungeons.push(dungeon);
                ch.clear_count += clear_count;
            }
            _ => {
                if let Some(ch) = current_chapter.take() {
                    chapters.push(ch);
                }
                current_chapter = Some(ChapterInfo {
                    chapter_id,
                    is_unlocked,
                    clear_count,
                    dungeons: vec![dungeon],
                });
            }
        }
    }

    if let Some(ch) = current_chapter {
        chapters.push(ch);
    }

    // If no progress, create default first chapter
    if chapters.is_empty() {
        chapters.push(ChapterInfo {
            chapter_id: 1,
            is_unlocked: true,
            clear_count: 0,
            dungeons: vec![ChapterDungeonInfo {
                dungeon_id: 1,
                chapter_id: 1,
                clear_count: 0,
                best_star: 0,
                first_clear_time: None,
            }],
        });
    }

    Ok(Json(GetCampaignInfoResponse {
        base_result: BaseResultType::Success as i32,
        chapters,
    }))
}

/// Battle result data
#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct BattleResultData {
    pub is_win: bool,
    pub star_rating: i32,
    pub damage_dealt: i64,
    pub gold_earned: i64,
    pub exp_earned: i64,
}

/// Submit campaign battle request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SubmitCampaignBattleRequest {
    pub session_id: Option<String>,
    pub chapter_id: Option<i32>,
    pub dungeon_id: Option<i32>,
    pub is_win: Option<bool>,
    pub star_rating: Option<i32>,
}

/// Submit campaign battle response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SubmitCampaignBattleResponse {
    pub base_result: i32,
    pub result: i32,
    pub gold_reward: i64,
    pub exp_reward: i64,
    pub unlocked_next: bool,
}

/// Handle submit campaign battle request
pub async fn submit_campaign_battle(
    State(state): State<AppState>,
    Form(req): Form<SubmitCampaignBattleRequest>,
) -> Result<Json<SubmitCampaignBattleResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let chapter_id = req.chapter_id.ok_or_else(|| ServerError::InvalidRequest("Missing chapter_id".to_string()))?;
    let dungeon_id = req.dungeon_id.ok_or_else(|| ServerError::InvalidRequest("Missing dungeon_id".to_string()))?;
    let is_win = req.is_win.unwrap_or(true);
    let star_rating = req.star_rating.unwrap_or(3);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    if !is_win {
        // Battle lost, no rewards
        return Ok(Json(SubmitCampaignBattleResponse {
            base_result: BaseResultType::Success as i32,
            result: 0,
            gold_reward: 0,
            exp_reward: 0,
            unlocked_next: false,
        }));
    }

    // Calculate rewards based on chapter/dungeon
    let gold_reward = (chapter_id as i64 * 100) + (dungeon_id as i64 * 50);
    let exp_reward = (chapter_id as i64 * 50) + (dungeon_id as i64 * 25);

    // Update or insert campaign progress
    let existing = sqlx::query(
        "SELECT clear_count, best_star FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
    )
    .bind(session.account_id)
    .bind(chapter_id)
    .bind(dungeon_id)
    .fetch_optional(&state.db)
    .await?;

    if let Some(row) = existing {
        let best_star: i32 = row.get("best_star");
        let new_best_star = best_star.max(star_rating);
        
        sqlx::query(
            "UPDATE campaign_progress SET clear_count = clear_count + 1, best_star = ? WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
        )
        .bind(new_best_star)
        .bind(session.account_id)
        .bind(chapter_id)
        .bind(dungeon_id)
        .execute(&state.db)
        .await?;
    } else {
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) VALUES (?, ?, ?, 1, ?, 1)"
        )
        .bind(session.account_id)
        .bind(chapter_id)
        .bind(dungeon_id)
        .bind(star_rating)
        .execute(&state.db)
        .await?;
    }

    // Check if we should unlock next dungeon/chapter
    let unlocked_next = star_rating >= 1; // At least 1 star to progress
    
    if unlocked_next {
        // Unlock next dungeon (simplified logic)
        let next_dungeon_id = dungeon_id + 1;
        let next_chapter_id = if next_dungeon_id > 10 { chapter_id + 1 } else { chapter_id };
        let actual_next_dungeon = if next_dungeon_id > 10 { 1 } else { next_dungeon_id };

        // Insert next dungeon as unlocked if not exists
        sqlx::query(
            "INSERT OR IGNORE INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) VALUES (?, ?, ?, 0, 0, 1)"
        )
        .bind(session.account_id)
        .bind(next_chapter_id)
        .bind(actual_next_dungeon)
        .execute(&state.db)
        .await?;
    }

    // Add rewards to user
    sqlx::query("UPDATE user_info SET gold = gold + ?, exp = exp + ? WHERE account_id = ?")
        .bind(gold_reward)
        .bind(exp_reward)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    Ok(Json(SubmitCampaignBattleResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        gold_reward,
        exp_reward,
        unlocked_next,
    }))
}

/// Start campaign battle request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct StartCampaignBattleRequest {
    pub session_id: Option<String>,
    pub chapter_id: Option<i32>,
    pub dungeon_id: Option<i32>,
    pub team_slot: Option<i32>,
}

/// Start campaign battle response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct StartCampaignBattleResponse {
    pub base_result: i32,
    pub result: i32,
    pub battle_id: String,
    pub stamina_cost: i32,
}

/// Handle start campaign battle request
pub async fn start_campaign_battle(
    State(state): State<AppState>,
    Form(req): Form<StartCampaignBattleRequest>,
) -> Result<Json<StartCampaignBattleResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let chapter_id = req.chapter_id.unwrap_or(1);
    let dungeon_id = req.dungeon_id.unwrap_or(1);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Generate battle ID
    let battle_id = format!("battle_{}_{}_{}_{}", session.account_id, chapter_id, dungeon_id, state.server_time());

    // Stamina cost (for reference, but actual deduction happens in begin_campaign)
    let stamina_cost = 6;

    // Note: Stamina deduction is now handled by begin_campaign endpoint
    // which the client calls before this endpoint

    Ok(Json(StartCampaignBattleResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        battle_id,
        stamina_cost,
    }))
}

// =====================================================
// BEGIN CAMPAIGN - Called when entering a battle (deducts stamina)
// =====================================================

/// Stamina result info matching client's NShared.StaminaResultInfo
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct StaminaResultInfo {
    #[serde(rename = "Type")]
    pub stamina_type: String,  // "Chicken" for regular stamina
    pub add_value: i32,        // Negative for consumption
    pub new_value: i32,        // New stamina value after deduction
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_recharge_time: Option<String>,
    pub next_recharge_remain_time: i32,
    pub full_recharge_remain_time: i32,
    pub recharge_count: i32,
    pub is_hide: bool,
}

/// Begin campaign request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct BeginCampaignRequest {
    pub session_id: Option<String>,
    #[serde(default)]
    pub session_key: Option<String>,
    pub chapter_index: Option<i32>,
    pub dungeon_index: Option<i32>,
    pub dungeon_difficulty: Option<i32>,
    pub scenario_dungeon: Option<bool>,
    pub use_stamina: Option<bool>,
    // Other fields we don't use but client may send
    pub hero_indices: Option<String>,
    pub leader_hero_index: Option<i32>,
}

/// Begin campaign response matching client's NShared.BeginCampaign.Response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BeginCampaignResponse {
    pub result: String,          // "Success"
    pub base_result: String,     // "Success"
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result: Option<StaminaResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub drop_gold: Option<i32>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub recharge_time: Option<String>,
}

/// Handle begin campaign request - deducts stamina and prepares battle
pub async fn begin_campaign(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<BeginCampaignResponse>> {
    // Parse form body manually - handle repeated fields like HeroIndices
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("Begin campaign request body: {}", body_str);
    
    // Parse URL-encoded form manually to handle repeated fields
    let params: std::collections::HashMap<String, String> = body_str
        .split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            let key = parts.next()?.to_string();
            let value = parts.next().unwrap_or("").to_string();
            Some((key, value))
        })
        .collect();
    
    let session_id = params.get("SessionId")
        .or_else(|| params.get("SessionKey"))
        .cloned()
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    tracing::info!("Session ID extracted: {} (first 20 chars)", &session_id[..session_id.len().min(20)]);
    
    let chapter_index = params.get("ChapterIndex")
        .and_then(|s| s.parse::<i32>().ok())
        .unwrap_or(1);
    let dungeon_index = params.get("DungeonIndex")
        .and_then(|s| s.parse::<i32>().ok())
        .unwrap_or(1);
    let difficulty = params.get("DungeonDifficulty")
        .and_then(|s| s.parse::<i32>().ok())
        .unwrap_or(1);
    
    tracing::info!("Looking up session for key: {}", &session_id);
    let session = state.get_session(&session_id)
        .ok_or_else(|| {
            tracing::error!("Session not found for key: {}", session_id);
            ServerError::SessionExpired
        })?;

    if super::hero::trial(&state, session.account_id, chapter_index, dungeon_index, None).await?.is_some() {
        return Ok(Json(BeginCampaignResponse { base_result: "Success".into(), result: "Success".into(), stamina_result: None, drop_gold: Some(0), recharge_time: None }));
    }

    // Get stamina cost from dungeon table
    let stamina_cost = state.tables.get_campaign_dungeon(chapter_index, dungeon_index)
        .map(|d| d.get_req_stamina(difficulty))
        .unwrap_or(6);  // Default to 6 if dungeon not found
    
    tracing::info!("Begin campaign: chapter={}, dungeon={}, difficulty={}, stamina_cost={}", 
        chapter_index, dungeon_index, difficulty, stamina_cost);

    // Get current stamina
    let stamina_row = sqlx::query("SELECT stamina FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let current_stamina: i32 = stamina_row.map(|r| r.get("stamina")).unwrap_or(0);
    
    if current_stamina < stamina_cost {
        return Err(ServerError::InvalidRequest("Not enough stamina".to_string()));
    }
    
    let new_stamina = current_stamina - stamina_cost;
    
    // Deduct stamina
    sqlx::query("UPDATE user_info SET stamina = ? WHERE account_id = ?")
        .bind(new_stamina)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;
    
    tracing::info!("Stamina deducted: {} -> {} (cost: {})", current_stamina, new_stamina, stamina_cost);

    // Build stamina result for client to update UI
    let stamina_result = StaminaResultInfo {
        stamina_type: "Chicken".to_string(),  // Regular stamina
        add_value: -stamina_cost,             // Negative for consumption
        new_value: new_stamina,
        stamina_recharge_time: None,
        next_recharge_remain_time: 0,
        full_recharge_remain_time: 0,
        recharge_count: 0,
        is_hide: false,
    };

    Ok(Json(BeginCampaignResponse {
        result: "Success".to_string(),
        base_result: "Success".to_string(),
        stamina_result: Some(stamina_result),
        drop_gold: None,
        recharge_time: None,
    }))
}

// =====================================================
// END CAMPAIGN - Called when battle completes
// =====================================================

/// Hero EXP result info matching client's NShared.HeroExpResultInfo
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct HeroExpResultInfo {
    pub hero_index: i32,
    pub add_value: i32,
    pub add_booster_value: i32,
    pub add_team_level_value: i32,
    pub new_value: i32,
    pub old_level: i16,
    pub new_level: i16,
}

/// Currency result info matching client's NShared.CurrencyResultInfo3
/// CurrencyType enum: None, Gold, Gem, Stamina, PvpCoin, TeamExp, etc.
/// For Gem type, client uses GetFieldValue("NewSysGem") and GetFieldValue("NewPayGem")
/// instead of NewValue directly!
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo3 {
    pub currency_type: String,  // Enum string: "Gold", "Gem", etc.
    pub add_value: i64,
    pub new_value: i64,
    // Extended fields for gem type (and potentially others)
    #[serde(skip_serializing_if = "Option::is_none")]
    pub field1: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value1: Option<i64>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub field2: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value2: Option<i64>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub field3: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value3: Option<i64>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub field4: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value4: Option<i64>,
}

impl CurrencyResultInfo3 {
    /// Create a simple currency result (for Gold, etc.)
    pub fn new(currency_type: &str, add_value: i64, new_value: i64) -> Self {
        CurrencyResultInfo3 {
            currency_type: currency_type.to_string(),
            add_value,
            new_value,
            field1: None,
            value1: None,
            field2: None,
            value2: None,
            field3: None,
            value3: None,
            field4: None,
            value4: None,
        }
    }
    
    /// Create a Gem currency result with NewSysGem and NewPayGem fields
    /// Client reads gems via: SetGem((int)info.GetFieldValue("NewSysGem"))
    pub fn gem(add_value: i64, new_sys_gem: i64, new_pay_gem: i64) -> Self {
        CurrencyResultInfo3 {
            currency_type: "Gem".to_string(),
            add_value,
            new_value: new_sys_gem + new_pay_gem, // Total for reference
            field1: Some("NewSysGem".to_string()),
            value1: Some(new_sys_gem),
            field2: Some("NewPayGem".to_string()),
            value2: Some(new_pay_gem),
            field3: None,
            value3: None,
            field4: None,
            value4: None,
        }
    }
}

/// EXP result info matching client's NShared.ExpResultInfo
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ExpResultInfo {
    pub add_value: i64,
    pub new_value: i64,
    pub old_level: i32,
    pub new_level: i32,
}

/// Campaign chapter dungeon info for results
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CampaignResultInfo {
    pub chapter_index: i32,
    pub dungeon_index: i32,
    pub max_star: i16,
    #[serde(rename = "FirstRewardedDiff")]
    pub first_rewarded_diff: i16,
    pub scenario_complete: i16,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub visited_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub completed_time: Option<String>,
    pub daily_completed_count: i32,
    pub reset_count: i32,
}

/// Item result info matching client's NShared.ItemResultInfo
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ItemResultInfo {
    pub item_index: i32,
    pub add_count: i32,
    pub new_count: i32,
    pub add_booster_count: i32,
    #[serde(rename = "AddNPCBoosterCount")]
    pub add_npc_booster_count: i32,
    pub add_bonus_assigned_item_percent: i32,
    pub locked: u8,
    pub is_first_clear_reward: bool,
}

/// End campaign request matching client's NShared.EndCampaign.Request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct EndCampaignRequest {
    pub session_id: Option<String>,
    pub chapter_index: Option<i32>,
    pub dungeon_index: Option<i32>,
    pub dungeon_difficulty: Option<i32>,
    pub scenario_dungeon: Option<bool>,
    pub completed: Option<bool>,
    pub star: Option<i32>,
    pub monster_indices: Option<Vec<i32>>,
    pub kill_counts: Option<Vec<i32>>,
    pub alive_hero_indices: Option<Vec<i32>>,
    pub battle_log_string: Option<String>,
    pub creature_info_string: Option<String>,
    pub pure_battle_time: Option<i32>,
}

/// End campaign response matching client's NShared.EndCampaign.Response
#[derive(Debug, Serialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct EndCampaignResponse {
    pub base_result: String,  // String "Success" for C# enum parsing
    pub result: String,  // String "Success" for C# enum parsing
    #[serde(skip_serializing_if = "Option::is_none")]
    pub forward_host: Option<String>,
    pub currency_results: Vec<CurrencyResultInfo3>,
    pub campaign_results: Vec<CampaignResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub exp_result: Option<ExpResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result: Option<serde_json::Value>,
    pub item_results: Vec<ItemResultInfo>,
    pub equip_item_infos: Vec<EquipItemInfo>,
    pub hero_exp_results: Vec<HeroExpResultInfo>,
    pub hero_infos: Vec<serde_json::Value>,
    pub tower_infos: Vec<serde_json::Value>,
}

/// Parse end campaign request from form body
fn parse_end_campaign_request(body: &str) -> EndCampaignRequest {
    use std::collections::HashMap;
    
    let params: HashMap<String, String> = body
        .split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            match (parts.next(), parts.next()) {
                (Some(key), Some(value)) => {
                    let decoded_value = urlencoding::decode(value)
                        .map(|s| s.into_owned())
                        .unwrap_or_else(|_| value.to_string());
                    Some((key.to_string(), decoded_value))
                }
                _ => None,
            }
        })
        .collect();
    
    // Client sends SessionKey, not SessionId
    EndCampaignRequest {
        session_id: params.get("SessionKey").or(params.get("SessionId")).cloned(),
        chapter_index: params.get("ChapterIndex").and_then(|s| s.parse().ok()),
        dungeon_index: params.get("DungeonIndex").and_then(|s| s.parse().ok()),
        dungeon_difficulty: params.get("DungeonDifficulty").and_then(|s| s.parse().ok()),
        scenario_dungeon: params.get("ScenarioDungeon").map(|s| s == "true" || s == "True"),
        completed: params.get("Completed").map(|s| s == "true" || s == "True"),
        star: params.get("Star").and_then(|s| s.parse().ok()),
        monster_indices: None, // Complex array, skip for now
        kill_counts: None,
        alive_hero_indices: None, // Complex array, skip for now
        battle_log_string: params.get("BattleLogString").cloned(),
        creature_info_string: params.get("CreatureInfoString").cloned(),
        pure_battle_time: params.get("PureBattleTime").and_then(|s| s.parse().ok()),
    }
}

/// Handle end campaign request - called when battle completes
pub async fn end_campaign(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<EndCampaignResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("End campaign request body: {}", body_str);
    
    let req = parse_end_campaign_request(&body_str);
    
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let chapter_index = req.chapter_index.unwrap_or(1);
    let dungeon_index = req.dungeon_index.unwrap_or(1);
    let difficulty = req.dungeon_difficulty.unwrap_or(0);  // 0=Easy, 1=Normal, 2=Hard, 3=Hell
    let completed = req.completed.unwrap_or(true);
    let star = req.star.unwrap_or(3);
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    tracing::info!("End campaign: chapter={}, dungeon={}, difficulty={}, completed={}, star={}", 
        chapter_index, dungeon_index, difficulty, completed, star);

    if let Some(trial) = super::hero::trial(&state, session.account_id, chapter_index, dungeon_index, Some(completed)).await? {
        let items = serde_json::from_value::<Vec<serde_json::Value>>(trial["ItemResults"].clone()).unwrap_or_default();
        return Ok(Json(EndCampaignResponse {
            base_result: "Success".into(), result: "Success".into(),
            hero_infos: trial["HeroInfos"].as_array().cloned().unwrap_or_default(),
            item_results: items.iter().map(|v| ItemResultInfo {item_index: super::item::n(v,"ItemIndex") as i32, add_count: super::item::n(v,"AddCount") as i32, new_count: super::item::n(v,"NewCount") as i32, ..Default::default()}).collect(),
            ..Default::default()
        }));
    }

    if !completed {
        // Battle lost, no rewards
        return Ok(Json(EndCampaignResponse {
            base_result: "Success".to_string(),
            result: "Success".to_string(),
            currency_results: vec![],
            campaign_results: vec![],
            hero_exp_results: vec![],
            item_results: vec![],
            equip_item_infos: vec![],
            hero_infos: vec![],
            tower_infos: vec![],
            ..Default::default()
        }));
    }

    // Calculate rewards based on campaign dungeon table data
    // Get dungeon data from tables
    let dungeon_data = state.tables.get_campaign_dungeon(chapter_index, dungeon_index);
    
    if let Some(ref d) = dungeon_data {
        tracing::info!("Using table data for chapter {} dungeon {}: drop_reward={}, exp={}, first_gem={}", 
            chapter_index, dungeon_index, 
            d.get_drop_reward_index(difficulty),
            d.get_creature_exp(difficulty),
            d.get_first_clear_gem(difficulty));
    } else {
        tracing::warn!("No table data for chapter {} dungeon {}, using fallback", chapter_index, dungeon_index);
    }
    
    // Get reward index for this difficulty
    let drop_reward_index = dungeon_data
        .as_ref()
        .map(|d| d.get_drop_reward_index(difficulty))
        .unwrap_or(1001 + (chapter_index - 1) * 12 + (dungeon_index - 1));
    
    tracing::info!("Drop reward index: {}", drop_reward_index);
    
    // Get reward data from tables
    let reward_data = state.tables.get_reward(drop_reward_index);
    
    if let Some(ref r) = reward_data {
        tracing::info!("Reward data found: gold_rate={}, gold_min={}, gold_max={}", 
            r.gold_rate, r.gold_min, r.gold_max);
    } else {
        tracing::warn!("No reward data for index {}", drop_reward_index);
    }
    
    // Gold calculation from reward table
    let gold_reward = reward_data
        .as_ref()
        .map(|r| r.roll_gold())
        .unwrap_or_else(|| {
            // Fallback calculation if tables not loaded
            tracing::warn!("Reward index {} not found in tables, using fallback", drop_reward_index);
            let (gold_min, gold_max) = match difficulty {
                0 => (800 + (chapter_index - 1) * 200, 1200 + (chapter_index - 1) * 300),
                1 => (1200 + (chapter_index - 1) * 300, 1800 + (chapter_index - 1) * 450),
                2 => (2000 + (chapter_index - 1) * 500, 3000 + (chapter_index - 1) * 750),
                _ => (4000 + (chapter_index - 1) * 1000, 6000 + (chapter_index - 1) * 1500),
            };
            gold_min as i64 + (rand::random::<i64>().abs() % ((gold_max - gold_min + 1) as i64))
        });
    
    // EXP calculation from dungeon table
    let base_exp = dungeon_data
        .as_ref()
        .map(|d| d.get_creature_exp(difficulty))
        .unwrap_or_else(|| {
            // Fallback calculation
            match difficulty {
                0 => 192 + (chapter_index - 1) * 50 + (dungeon_index - 1) * 10,
                1 => 250 + (chapter_index - 1) * 80 + (dungeon_index - 1) * 15,
                2 => 300 + (chapter_index - 1) * 100 + (dungeon_index - 1) * 20,
                _ => 500 + (chapter_index - 1) * 200 + (dungeon_index - 1) * 50,
            }
        });
    let (gold_boost, exp_boost) = super::item::campaign_boost(&state,session.account_id).await?;
    let gold_reward = gold_reward + gold_reward * gold_boost as i64 / 100;
    let exp_bonus = base_exp * exp_boost / 100;
    let exp_per_hero = base_exp + exp_bonus;
    
    tracing::info!("Calculated rewards: gold={}, exp_per_hero={}", gold_reward, exp_per_hero);

    // Get current user gold
    let user_row = sqlx::query("SELECT gold FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let current_gold: i64 = user_row.map(|r| r.get("gold")).unwrap_or(0);
    let new_gold = current_gold + gold_reward;

    // Update user gold
    sqlx::query("UPDATE user_info SET gold = ? WHERE account_id = ?")
        .bind(new_gold)
        .bind(session.account_id)
        .execute(&state.db)
        .await?;

    // Update or insert campaign progress
    let now = chrono::Utc::now().format("%Y-%m-%d %H:%M:%S").to_string();
    let existing = sqlx::query(
        "SELECT clear_count, best_star FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
    )
    .bind(session.account_id)
    .bind(chapter_index)
    .bind(dungeon_index)
    .fetch_optional(&state.db)
    .await?;

    // Check if this is a first clear - either no record exists, or clear_count is 0
    let (is_first_clear, new_star) = if let Some(row) = existing {
        let clear_count: i32 = row.get("clear_count");
        let best_star: i32 = row.get("best_star");
        let new_best_star = best_star.max(star);
        let first_clear = clear_count == 0;  // First clear if we haven't cleared before
        
        sqlx::query(
            "UPDATE campaign_progress SET clear_count = clear_count + 1, best_star = ?, completed_time = ? WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
        )
        .bind(new_best_star)
        .bind(&now)
        .bind(session.account_id)
        .bind(chapter_index)
        .bind(dungeon_index)
        .execute(&state.db)
        .await?;
        
        (first_clear, new_best_star)
    } else {
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) VALUES (?, ?, ?, 1, ?, 1, ?)"
        )
        .bind(session.account_id)
        .bind(chapter_index)
        .bind(dungeon_index)
        .bind(star)
        .bind(&now)
        .execute(&state.db)
        .await?;
        
        (true, star)
    };

    // Unlock next dungeon
    let next_dungeon_index = dungeon_index + 1;
    let (next_chapter, next_dungeon) = if next_dungeon_index > 12 { 
        (chapter_index + 1, 1) 
    } else { 
        (chapter_index, next_dungeon_index) 
    };

    sqlx::query(
        "INSERT OR IGNORE INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) VALUES (?, ?, ?, 0, 0, 1)"
    )
    .bind(session.account_id)
    .bind(next_chapter)
    .bind(next_dungeon)
    .execute(&state.db)
    .await?;

    // Get heroes that participated and update their EXP
    let mut hero_exp_results = Vec::new();
    
    // Get all heroes for this account
    let hero_rows = sqlx::query(
        "SELECT hero_index, exp, level, star, transcend FROM heroes WHERE account_id = ?"
    )
    .bind(session.account_id)
    .fetch_all(&state.db)
    .await?;

    for hero_row in &hero_rows {
        let hero_index: i32 = hero_row.get("hero_index");
        
        // Give EXP to all heroes (simplified - no alive hero check for now)
        {
            let current_exp: i32 = hero_row.get("exp");
            let current_level: i32 = hero_row.get("level");
            let cap=state.tables.tutorials.support.hero_stars.iter()
                .find(|v|v.star==hero_row.get::<i32,_>("star")&&v.transcended==hero_row.get::<i32,_>("transcend"))
                .map(|v|v.max_hero_level).unwrap_or(current_level);
            let (new_level,new_exp)=if current_level>=cap {(current_level,current_exp as i64)}else{
                crate::tables::add_exp(&state.tables.tutorials.support.hero_levels,current_level,current_exp as i64,exp_per_hero as i64,cap)
            };
            
            // Update hero exp in database
            sqlx::query("UPDATE heroes SET exp = ?, level = ? WHERE account_id = ? AND hero_index = ?")
                .bind(new_exp)
                .bind(new_level)
                .bind(session.account_id)
                .bind(hero_index)
                .execute(&state.db)
                .await?;

            hero_exp_results.push(HeroExpResultInfo {
                hero_index,
                add_value: base_exp,
                add_booster_value: exp_bonus,
                add_team_level_value: 0,
                new_value: new_exp as i32,
                old_level: current_level as i16,
                new_level: new_level as i16,
            });
        }
    }

    tracing::info!("is_first_clear={}, hero_exp_results.len()={}", is_first_clear, hero_exp_results.len());

    // First clear rewards: Gems (rubies) from dungeon table
    let gem_reward = if is_first_clear {
        let gem = dungeon_data
            .as_ref()
            .map(|d| d.get_first_clear_gem(difficulty))
            .unwrap_or_else(|| {
                // Fallback calculation
                match difficulty {
                    0 => 5 + (chapter_index - 1) * 2,
                    1 => 10 + (chapter_index - 1) * 3,
                    2 => 10 + (chapter_index - 1) * 4,
                    _ => 15 + (chapter_index - 1) * 5,
                }
            });
        tracing::info!("First clear! Gem reward from table: {}", gem);
        gem
    } else {
        tracing::info!("Not first clear, no gem reward");
        0
    };

    // Get current gems first (we need this for the response either way)
    let gem_row = sqlx::query("SELECT gem FROM user_info WHERE account_id = ?")
        .bind(session.account_id)
        .fetch_optional(&state.db)
        .await?;
    
    let current_gems: i64 = gem_row.map(|r| r.get("gem")).unwrap_or(0);
    let new_gems = current_gems + gem_reward as i64;

    // Update gems if first clear
    if gem_reward > 0 {
        sqlx::query("UPDATE user_info SET gem = ? WHERE account_id = ?")
            .bind(new_gems)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
        
        tracing::info!("First clear gem reward: {} gems (total: {})", gem_reward, new_gems);
    }

    // Always include both Gold and Gem in currency_results to prevent client UI reset
    // CurrencyType enum: "Gold", "Gem", etc.
    // IMPORTANT: For Gem type, client uses GetFieldValue("NewSysGem") and GetFieldValue("NewPayGem")
    // instead of NewValue directly!
    let currency_results = vec![
        CurrencyResultInfo3::new("Gold", gold_reward, new_gold),
        // All gems are "system gems" (free), pay_gem stays at 0
        CurrencyResultInfo3::gem(gem_reward as i64, new_gems, 0),
    ];

    // Item rewards - Early chapters (1-3) don't really give items, just gold/exp/rubies
    // Item drops from RewardTable SubDataList
    let mut item_results: Vec<ItemResultInfo> = Vec::new();
    let mut equip_item_infos: Vec<EquipItemInfo> = Vec::new();
    
    if let Some(ref reward) = reward_data {
        // Use Reward StringPool to resolve item codes (they're indices into RewardTable's own string pool)
        // NOTE: RewardTable has its OWN StringPool - different from ItemGroupStringPool!
        let item_drops = reward.roll_items(&state.tables.reward_string_pool);
        use crate::tables::parse_item_code;
        
        for drop in &item_drops {
            tracing::info!("Item drop rolled: code={}, count={}, star_min={}, star_max={}", 
                drop.item_code, drop.count, drop.star_min, drop.star_max);
            
            // Parse item code - can be "251" (direct code), "1100[1:2:4]" (group with filter), etc.
            let (base_code, grade_filter) = parse_item_code(&drop.item_code);
            
            // Try to resolve to an item index
            // The base_code is an integer that's a StringPool index in the ItemGroup's string pool
            let resolved_item: Option<(i32, i32)> = if let Ok(pool_index) = base_code.parse::<i32>() {
                // First, check if this pool_index resolves to an item group code
                if let Some(group_code) = state.tables.resolve_group_code(pool_index) {
                    tracing::debug!("Resolved pool index {} → group code '{}'", pool_index, group_code);
                    
                    if state.tables.get_item_group(group_code).is_some() {
                        // It's an item group - use recursive resolution
                        state.tables.roll_item_from_group(pool_index, &grade_filter)
                            .map(|(idx, cnt, _, _)| (idx, cnt * drop.count))
                    } else {
                        // The resolved code is not a group - might be a direct item code
                        if let Some(item_index) = state.tables.get_item_index(group_code) {
                            tracing::info!("Resolved item code '{}' → index {}", group_code, item_index);
                            Some((item_index, drop.count))
                        } else {
                            tracing::warn!("Item code '{}' not found in ItemTable", group_code);
                            None
                        }
                    }
                } else {
                    // Pool index out of bounds - try as direct item code lookup
                    if let Some(item_index) = state.tables.get_item_index(&base_code) {
                        tracing::info!("Resolved direct item code '{}' → index {}", base_code, item_index);
                        Some((item_index, drop.count))
                    } else {
                        tracing::warn!("Item code '{}' not found in ItemTable and not a valid pool index", base_code);
                        None
                    }
                }
            } else {
                // Non-numeric code - it's already a string item/group code (resolved from StringPool in roll_items)
                // First check if it's an item group
                if state.tables.get_item_group(&base_code).is_some() {
                    tracing::debug!("String code '{}' is an item group, rolling recursively", base_code);
                    state.tables.roll_item_from_group_code(&base_code, &grade_filter)
                        .map(|(idx, cnt, _, _)| (idx, cnt * drop.count))
                } else if let Some(item_index) = state.tables.get_item_index(&base_code) {
                    tracing::info!("Resolved string item code '{}' → index {}", base_code, item_index);
                    Some((item_index, drop.count))
                } else {
                    tracing::warn!("Could not resolve item code: {}", drop.item_code);
                    None
                }
            };
            
            if let Some((item_index, count)) = resolved_item {
                tracing::info!("Final resolved item: index={}, count={}", item_index, count);
                
                // Check if this is equipment (unique items with stars/levels)
                if ItemTable::is_equipment(item_index) {
                    // Equipment items - each is a unique instance
                    // Insert into equip_items table
                    for _ in 0..count {
                        let star = drop.star_min; // Use the star from the drop
                        let created_time = state.server_time_str();
                        
                        let result = sqlx::query(
                            "INSERT INTO equip_items (account_id, item_index, star, created_time) VALUES (?, ?, ?, ?)"
                        )
                        .bind(session.account_id)
                        .bind(item_index)
                        .bind(star)
                        .bind(&created_time)
                        .execute(&state.db)
                        .await?;
                        
                        let slot_index = result.last_insert_rowid() as i32;
                        
                        let equip_info = EquipItemInfo::new(slot_index, item_index, star, created_time);
                        equip_item_infos.push(equip_info);
                        
                        tracing::info!("Equipment added: slot={}, index={}, star={}", 
                            slot_index, item_index, star);
                    }
                } else {
                    // Stackable items - update count or insert
                    let existing = sqlx::query(
                        "SELECT count FROM items WHERE account_id = ? AND item_index = ?"
                    )
                    .bind(session.account_id)
                    .bind(item_index)
                    .fetch_optional(&state.db)
                    .await?;
                    
                    let (_old_count, new_count) = if let Some(row) = existing {
                        let old: i32 = row.get("count");
                        let new = old + count;
                        
                        sqlx::query(
                            "UPDATE items SET count = ? WHERE account_id = ? AND item_index = ?"
                        )
                        .bind(new)
                        .bind(session.account_id)
                        .bind(item_index)
                        .execute(&state.db)
                        .await?;
                        
                        (old, new)
                    } else {
                        sqlx::query(
                            "INSERT INTO items (account_id, item_index, count) VALUES (?, ?, ?)"
                        )
                        .bind(session.account_id)
                        .bind(item_index)
                        .bind(count)
                        .execute(&state.db)
                        .await?;
                        
                        (0, count)
                    };
                    
                    item_results.push(ItemResultInfo {
                        item_index,
                        add_count: count,
                        new_count,
                        add_booster_count: 0,
                        add_npc_booster_count: 0,
                        add_bonus_assigned_item_percent: 0,
                        locked: 0,
                        is_first_clear_reward: false,
                    });
                    
                    tracing::info!("Item added: index={}, add={}, new_total={}", 
                        item_index, count, new_count);
                }
            }
        }
    }

    // Build campaign result
    // MaxStar encoding: difficulty * 10 + star
    // e.g., Easy(0) with 3 stars = 0*10+3 = 3
    //       Normal(1) with 3 stars = 1*10+3 = 13
    //       Hard(2) with 3 stars = 2*10+3 = 23
    // The client checks: (MaxStar / 10) >= difficulty to verify completion
    let encoded_max_star = (difficulty * 10 + new_star) as i16;
    
    // FirstRewardedDiff is a BITMASK: bit 0 = Easy (1), bit 1 = Normal (2), bit 2 = Hard (4), bit 3 = Hell (8)
    // For first clear, we set the bit for the difficulty that was cleared
    let first_rewarded_diff_bitmask = if is_first_clear { 
        1 << difficulty  // Easy=1, Normal=2, Hard=4, Hell=8
    } else { 
        0 
    };
    
    let campaign_result = CampaignResultInfo {
        chapter_index,
        dungeon_index,
        max_star: encoded_max_star,
        first_rewarded_diff: first_rewarded_diff_bitmask as i16,
        scenario_complete: 1,
        visited_time: Some(now.clone()),
        completed_time: Some(now.clone()),
        daily_completed_count: 1,
        reset_count: 0,
    };
    
    // Also include the NEXT dungeon as unlocked (but not completed)
    // This tells the client that the next dungeon is now accessible
    let next_dungeon_result = CampaignResultInfo {
        chapter_index: next_chapter,
        dungeon_index: next_dungeon,
        max_star: 0,
        first_rewarded_diff: 0,
        scenario_complete: 0,
        visited_time: None,
        completed_time: None,  // Not completed yet, just unlocked
        daily_completed_count: 0,
        reset_count: 0,
    };

    tracing::info!("Campaign completed: gold_reward={}, gem_reward={}, hero_exp_count={}, item_count={}, max_star={}", 
        gold_reward, gem_reward, hero_exp_results.len(), item_results.len(), encoded_max_star);
    
    // Log item results for debugging
    for item in &item_results {
        tracing::info!("ItemResult being sent: index={}, add={}, new={}", 
            item.item_index, item.add_count, item.new_count);
    }
    
    // Log equipment results for debugging
    for equip in &equip_item_infos {
        tracing::info!("EquipItemInfo being sent: slot={}, index={}, star={}", 
            equip.slot_index, equip.item_index, equip.star);
    }

    let response = EndCampaignResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        forward_host: None,
        currency_results,
        // Include both the completed dungeon AND the next unlocked dungeon
        campaign_results: vec![campaign_result, next_dungeon_result],
        exp_result: None,
        stamina_result: None,
        item_results,
        equip_item_infos,
        hero_exp_results,
        hero_infos: vec![],
        tower_infos: vec![],
    };
    
    Ok(Json(response))
}

/// Visit dungeon request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct VisitDungeonRequest {
    pub session_key: Option<String>,
    pub chapter_index: Option<i32>,
    pub dungeon_index: Option<i32>,
}

/// Visit dungeon response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct VisitDungeonResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub dungeon_info: Option<VisitDungeonInfo>,
    pub currency_results: Vec<serde_json::Value>,
    pub item_results: Vec<serde_json::Value>,
    pub equip_item_results: Vec<serde_json::Value>,
    pub hero_infos: Vec<serde_json::Value>,
    pub exp_result_infos: Vec<serde_json::Value>,
    pub stamina_result_infos: Vec<serde_json::Value>,
}

/// Dungeon info for visit response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct VisitDungeonInfo {
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

/// Handle visit dungeon request - allows player to move to a dungeon
pub async fn visit_dungeon(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<VisitDungeonResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("visit_dungeon request: {}", body_str);

    // Parse form data
    let mut session_key = None;
    let mut chapter_index = 1;
    let mut dungeon_index = 1;
    
    for pair in body_str.split('&') {
        let mut parts = pair.splitn(2, '=');
        let key = parts.next().unwrap_or("");
        let value = parts.next().unwrap_or("");
        
        match key {
            "SessionKey" => session_key = Some(value.to_string()),
            "ChapterIndex" => chapter_index = value.parse().unwrap_or(1),
            "DungeonIndex" => dungeon_index = value.parse().unwrap_or(1),
            _ => {}
        }
    }

    let session_key = session_key.ok_or_else(|| ServerError::SessionExpired)?;
    let session = state.get_session(&session_key).ok_or(ServerError::SessionExpired)?;

    let now = state.server_time_str();

    // Check if dungeon exists in progress, if not create it as visited
    let existing = sqlx::query(
        "SELECT chapter_id, dungeon_id, clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
    )
    .bind(session.account_id)
    .bind(chapter_index)
    .bind(dungeon_index)
    .fetch_optional(&state.db)
    .await?;

    let dungeon_info = if let Some(row) = existing {
        // Dungeon already in progress
        VisitDungeonInfo {
            chapter_index,
            dungeon_index,
            max_star: row.get::<i32, _>("best_star") as i16,
            first_rewarded_diff: if row.get::<i32, _>("clear_count") > 0 { 1 } else { 0 },
            scenario_complete: 1,
            visited_time: Some(now),
            completed_time: row.get::<Option<String>, _>("completed_time"),
            daily_completed_count: row.get("clear_count"),
            reset_count: 0,
        }
    } else {
        // Create new dungeon visit entry
        sqlx::query(
            "INSERT OR IGNORE INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked) VALUES (?, ?, ?, 0, 0, 1)"
        )
        .bind(session.account_id)
        .bind(chapter_index)
        .bind(dungeon_index)
        .execute(&state.db)
        .await?;

        VisitDungeonInfo {
            chapter_index,
            dungeon_index,
            max_star: 0,
            first_rewarded_diff: 0,
            scenario_complete: 0,
            visited_time: Some(now),
            completed_time: None,
            daily_completed_count: 0,
            reset_count: 0,
        }
    };

    tracing::info!("Visit dungeon: chapter={}, dungeon={}", chapter_index, dungeon_index);

    Ok(Json(VisitDungeonResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        dungeon_info: Some(dungeon_info),
        currency_results: vec![],
        item_results: vec![],
        equip_item_results: vec![],
        hero_infos: vec![],
        exp_result_infos: vec![],
        stamina_result_infos: vec![],
    }))
}

/// Complete scenario dungeon request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct CompleteScenarioDungeonRequest {
    pub session_key: Option<String>,
    pub session_id: Option<String>,
    pub chapter_index: i32,
    pub dungeon_index: i32,
}

/// Complete scenario dungeon response
/// Note: This endpoint only returns dungeon info, no rewards.
/// It's used for scenario replays where rewards were already given.
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct CompleteScenarioDungeonResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub dungeon_info: Option<CampaignResultInfo>,
}

/// Handle complete scenario dungeon request
/// This is called after winning a battle to grant rewards
pub async fn complete_scenario_dungeon(
    State(state): State<AppState>,
    Form(req): Form<CompleteScenarioDungeonRequest>,
) -> Result<Json<CompleteScenarioDungeonResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let chapter_index = req.chapter_index;
    let dungeon_index = req.dungeon_index;
    
    tracing::info!(
        "Complete scenario dungeon {}-{} for account {}",
        chapter_index, dungeon_index, account_id
    );
    
    // Mark dungeon as completed with 3 stars (Normal difficulty)
    let completed_time = state.server_time_str();
    sqlx::query(
        "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, clear_count, best_star, is_unlocked, completed_time) 
         VALUES (?, ?, ?, 1, 3, 1, ?)
         ON CONFLICT(account_id, chapter_id, dungeon_id) 
         DO UPDATE SET clear_count = clear_count + 1, best_star = MAX(best_star, 3), completed_time = ?"
    )
    .bind(account_id)
    .bind(chapter_index)
    .bind(dungeon_index)
    .bind(&completed_time)
    .bind(&completed_time)
    .execute(&state.db)
    .await?;
    
    // Get updated dungeon info
    let row = sqlx::query(
        "SELECT * FROM campaign_progress WHERE account_id = ? AND chapter_id = ? AND dungeon_id = ?"
    )
    .bind(account_id)
    .bind(chapter_index)
    .bind(dungeon_index)
    .fetch_one(&state.db)
    .await?;
    
    let clear_count: i32 = row.get("clear_count");
    let best_star: i32 = row.get("best_star");
    
    // MaxStar encoding: For chapter 1 (min difficulty Normal), encode as 1*10 + stars
    let max_star = if chapter_index <= 10 {
        (10 + best_star) as i16
    } else {
        best_star as i16
    };
    
    let dungeon_info = CampaignResultInfo {
        chapter_index,
        dungeon_index,
        max_star,
        first_rewarded_diff: 2, // Normal difficulty bit
        scenario_complete: 1,
        visited_time: Some(state.server_time_str()),
        completed_time: Some(completed_time),
        daily_completed_count: clear_count,
        reset_count: 0,
    };
    
    // CompleteScenarioDungeon doesn't return rewards (unlike EndCampaign)
    // It's used for scenario replays where rewards were already given
    let response = CompleteScenarioDungeonResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        dungeon_info: Some(dungeon_info),
    };
    
    Ok(Json(response))
}
