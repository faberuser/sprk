use crate::{
    error::{Result, ServerError},
    models::{BaseResultType, ChapterDungeonInfo},
    state::AppState,
};
use axum::{
    body::Bytes,
    extract::{Form, State},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;

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

    let session = state
        .get_session(&session_id)
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

/// Handle submit campaign battle request
pub async fn submit_campaign_battle(
    State(state): State<AppState>,
    Form(req): Form<SubmitCampaignBattleRequest>,
) -> Result<Json<serde_json::Value>> {
    let body = serde_urlencoded::to_string([
        ("SessionId", req.session_id.unwrap_or_default()),
        ("ChapterIndex", req.chapter_id.unwrap_or(0).to_string()),
        ("DungeonIndex", req.dungeon_id.unwrap_or(0).to_string()),
        ("DungeonDifficulty", "1".into()),
        ("Completed", req.is_win.unwrap_or(false).to_string()),
        ("Star", req.star_rating.unwrap_or(0).to_string()),
    ])
    .map_err(|_| ServerError::InvalidRequest("Invalid campaign request".into()))?;
    super::battle::execute_request(&state, "campaign/end_campaign", Bytes::from(body))
        .await
        .map(Json)
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

    let session = state
        .get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Generate battle ID
    let battle_id = format!(
        "battle_{}_{}_{}_{}",
        session.account_id,
        chapter_id,
        dungeon_id,
        state.server_time()
    );

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

/// Begin campaign request

/// Begin campaign response matching client's NShared.BeginCampaign.Response

/// Handle begin campaign request - deducts stamina and prepares battle
pub async fn begin_campaign(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<serde_json::Value>> {
    super::battle::execute_request(&state, "campaign/begin_campaign", body)
        .await
        .map(Json)
}

// =====================================================
// END CAMPAIGN - Called when battle completes
// =====================================================

/// Hero EXP result info matching client's NShared.HeroExpResultInfo

/// Currency result info matching client's NShared.CurrencyResultInfo3
/// CurrencyType enum: None, Gold, Gem, Stamina, PvpCoin, TeamExp, etc.
/// For Gem type, client uses GetFieldValue("NewSysGem") and GetFieldValue("NewPayGem")
/// instead of NewValue directly!
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo3 {
    pub currency_type: String, // Enum string: "Gold", "Gem", etc.
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

/// Handle end campaign request - called when battle completes
pub async fn end_campaign(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<serde_json::Value>> {
    super::battle::execute_request(&state, "campaign/end_campaign", body)
        .await
        .map(Json)
}

/// Handle visit dungeon request - allows player to move to a dungeon
pub async fn visit_dungeon(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<serde_json::Value>> {
    super::battle::execute_request(&state, "campaign/visit_dungeon", body)
        .await
        .map(Json)
}

/// Record a scenario replay after its dungeon has been cleared.
pub async fn complete_scenario_dungeon(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<serde_json::Value>> {
    super::battle::execute_request(&state, "campaign/complete_scenario_dungeon", body)
        .await
        .map(Json)
}
