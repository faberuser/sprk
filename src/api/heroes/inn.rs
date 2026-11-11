use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use chrono::{Utc, Duration};
use rand::RngExt;

use crate::{
    error::{Result, ServerError},
    models::{
        hero::HeroInfo,
        hero_inn::{
            PlayerHeroFriendlyInfo,
            FriendlyActionType, CurrencyResultInfo3, FriendshipPointResultInfo,
            get_available_inn_hero_indices, INN_RECRUITMENT_EXCLUDED_HEROES,
            GREETING_POINTS, CONVERSATION_POINTS, GIFT_POINTS,
            RouletteInfo, RouletteRewardInfo, StaminaResultInfo,
            get_default_roulette_rewards,
        },
    },
    state::AppState,
};

/// Select random heroes from a list
fn select_random_heroes(available: &[i32], count: usize) -> Vec<i32> {
    use rand::seq::IndexedRandom;
    let mut rng = rand::rng();
    available.sample(&mut rng, count.min(available.len())).cloned().collect()
}

// Recruitment and visiting are separate: owned heroes can fill the remaining
// seats, including heroes at maximum closeness. Keep an active recruit first.
fn select_inn_visitors(available: &[i32], owned_visitors: &[i32], active: i32) -> Vec<i32> {
    let mut selected = if available.contains(&active) { vec![active] } else { Vec::new() };
    let candidates: Vec<i32> = available.iter().copied().filter(|id| *id != active).collect();
    selected.extend(select_random_heroes(&candidates, 6 - selected.len()));
    selected.extend(select_random_heroes(owned_visitors, 6 - selected.len()));
    selected
}

fn owned_inn_visitors(state: &AppState, owned: &[i32]) -> Vec<i32> {
    owned.iter().copied().filter(|id| state.tables.hero_shop.inn_recruitment_points.contains_key(id)).collect()
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
    let mut tx = state.db.begin().await?;

    // Ownership excludes recruitment, not social visits.
    let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&mut *tx)
        .await?
        .iter()
        .map(|row| row.get("hero_index"))
        .collect();

    // Get available heroes (not owned)
    let available_heroes: Vec<i32> = get_available_inn_hero_indices()
        .into_iter()
        .filter(|h| !owned_heroes.contains(h))
        .collect();
    let owned_visitors = owned_inn_visitors(&state, &owned_heroes);

    // Check if there's an existing friendly info for this account
    let existing = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&mut *tx)
        .await?;

    let mut friendly_info = if let Some(row) = existing {
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
        let selected_heroes = select_inn_visitors(&available_heroes, &owned_visitors, 0);

        let selected_hero_indices_str = selected_heroes
            .iter()
            .map(|h| h.to_string())
            .collect::<Vec<_>>()
            .join(",");

        // HeroIndex tracks recruitment. An owned visitor belongs only in the
        // portrait list, otherwise the native client continually requests resets.
        let first_hero = selected_heroes.iter().copied().find(|id| available_heroes.contains(id)).unwrap_or(0);

        // Insert new record
        sqlx::query(
            "INSERT INTO hero_friendly_info (account_id, hero_index, selected_hero_index, friendly_point, selected_time, selected_hero_indices) VALUES (?, ?, ?, 0, ?, ?)"
        )
        .bind(account_id)
        .bind(first_hero)
        .bind(first_hero)
        .bind(&current_time)
        .bind(&selected_hero_indices_str)
        .execute(&mut *tx)
        .await?;

        PlayerHeroFriendlyInfo {
            hero_index: first_hero,
            selected_hero_index: first_hero,
            friendly_point: 0,
            last_greeting_time: None,
            last_conversation_time: None,
            last_gift_time: None,
            selected_time: Some(current_time.clone()),
            selected_hero_indice: Some(selected_hero_indices_str),
            last_roulette_time: None,
        }
    };

    // The native Inn polls SelectedTime against the daily reset. Returning an
    // expired timestamp as Success makes it reopen the scene on every poll,
    // continually restarting the black transition before it can finish.
    let today = Utc::now().date_naive();
    let expired = friendly_info.selected_time.as_deref()
        .and_then(|time| chrono::NaiveDateTime::parse_from_str(time, "%Y-%m-%d %H:%M:%S").ok())
        .is_none_or(|time| time.date() < today);
    let visitors: Vec<i32> = friendly_info.selected_hero_indice.as_deref().unwrap_or("")
        .split(',').filter_map(|id| id.parse().ok()).collect();
    let invalid_visitors = visitors.len() != 6.min(available_heroes.len() + owned_visitors.len())
        || visitors.iter().collect::<std::collections::HashSet<_>>().len() != visitors.len()
        || visitors.iter().any(|id| !available_heroes.contains(id) && !owned_visitors.contains(id));
    let recruited = friendly_info.hero_index != 0 && owned_heroes.contains(&friendly_info.hero_index);
    let excluded_recruit = INN_RECRUITMENT_EXCLUDED_HEROES.contains(&friendly_info.hero_index)
        || INN_RECRUITMENT_EXCLUDED_HEROES.contains(&friendly_info.selected_hero_index);
    if expired || invalid_visitors || recruited || excluded_recruit {
        if recruited || excluded_recruit {
            friendly_info.hero_index = 0;
            friendly_info.selected_hero_index = 0;
            friendly_info.friendly_point = 0;
            friendly_info.last_greeting_time = None;
            friendly_info.last_conversation_time = None;
            friendly_info.last_gift_time = None;
        }
        // Keep the hero being befriended, their progress, and action cooldowns.
        // Refresh only the other visitors and acknowledge this reset.
        let selected = select_inn_visitors(&available_heroes, &owned_visitors, friendly_info.hero_index);
        friendly_info.selected_hero_indice = Some(selected.iter().map(i32::to_string).collect::<Vec<_>>().join(","));
        friendly_info.selected_time = Some(current_time.clone());
        sqlx::query("UPDATE hero_friendly_info SET hero_index=?,selected_hero_index=?,friendly_point=?,last_greeting_time=?,last_conversation_time=?,last_gift_time=?,selected_time=?,selected_hero_indices=? WHERE account_id=?")
            .bind(friendly_info.hero_index).bind(friendly_info.selected_hero_index)
            .bind(friendly_info.friendly_point).bind(&friendly_info.last_greeting_time)
            .bind(&friendly_info.last_conversation_time).bind(&friendly_info.last_gift_time)
            .bind(&friendly_info.selected_time).bind(&friendly_info.selected_hero_indice)
            .bind(account_id).execute(&mut *tx).await?;
    }
    tx.commit().await?;

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
        result: if available_heroes.is_empty() && owned_visitors.is_empty() {
            RequestNewFriendlyHeroResult::CannotRecruitHero
        } else {
            RequestNewFriendlyHeroResult::Success
        }.as_str().to_string(),
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
    let session_id = req.session_key.or(req.session_id).ok_or(ServerError::SessionExpired)?;
    let account_id = state.get_session(&session_id).ok_or(ServerError::SessionExpired)?.account_id;
    let hero_index = req.hero_index.as_deref().and_then(|s| s.parse::<i32>().ok()).unwrap_or(0);
    let action = req.action.as_deref().map(parse_action_string).unwrap_or(FriendlyActionType::None);
    let failure = |result: DoHeroFriendlyResult| Json(DoHeroFriendlyResponse {
        base_result: "Success".into(), result: result.as_str().into(), friendly_info: None,
        hero_info: None, currency_result: None, friendship_point_result: None,
    });
    if hero_index == 0 { return Ok(failure(DoHeroFriendlyResult::HeroIndexMismatch)); }
    let (column, wire_time, points, gold_cost, friendship_cost, closeness_action) = match action {
        FriendlyActionType::Greeting => ("last_greeting_time", "LastGreetingTime", GREETING_POINTS, 0, 0, false),
        FriendlyActionType::Conversation => ("last_conversation_time", "LastConversationTime", CONVERSATION_POINTS, 0, 160, false),
        FriendlyActionType::Gift => ("last_gift_time", "LastGiftTime", GIFT_POINTS, 40000, 0, false),
        FriendlyActionType::ClosenessGreeting => ("last_greeting_time", "LastGreetingTime", 25, 0, 0, true),
        FriendlyActionType::ClosenessConversation => ("last_conversation_time", "LastConversationTime", 50, 0, 50, true),
        FriendlyActionType::ClosenessGift => ("last_gift_time", "LastGiftTime", 100, 25000, 0, true),
        _ => return Ok(failure(DoHeroFriendlyResult::ActionNotExist)),
    };
    // Serialize the cooldown check, costs, timestamp and Affinity update. A
    // concurrent click must not pay or earn points a second time.
    let mut tx = state.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(account_id).execute(&mut *tx).await?;
    let owned = sqlx::query("SELECT closeness FROM heroes WHERE account_id=? AND hero_index=?")
        .bind(account_id).bind(hero_index).fetch_optional(&mut *tx).await?;
    let is_owned = owned.is_some();
    let target = state.tables.hero_shop.inn_recruitment_points.get(&hero_index).copied();
    if !is_owned && target.is_none() { return Ok(failure(DoHeroFriendlyResult::CannotRecruit)); }
    if is_owned != closeness_action { return Ok(failure(DoHeroFriendlyResult::ActionNotExist)); }
    let Some(friendly) = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id=?")
        .bind(account_id).fetch_optional(&mut *tx).await? else {
        return Ok(failure(DoHeroFriendlyResult::HeroNotExistInFriendlyTable));
    };
    if !is_owned && friendly.get::<i32, _>("hero_index") != hero_index {
        return Ok(failure(DoHeroFriendlyResult::HeroIndexMismatch));
    }
    let current_time = state.server_time_str();
    let today = chrono::NaiveDateTime::parse_from_str(&current_time, "%Y-%m-%d %H:%M:%S")
        .map_err(|e| ServerError::Internal(e.to_string()))?.date();
    let mut details = if is_owned { super::details(&mut tx, account_id, hero_index).await? } else { serde_json::json!({}) };
    let last_time: Option<String> = if is_owned {
        details.get(wire_time).and_then(|v| v.as_str()).map(str::to_owned)
    } else { friendly.get(column) };
    if last_time.as_deref().and_then(|t| chrono::NaiveDateTime::parse_from_str(t, "%Y-%m-%d %H:%M:%S").ok())
        .is_some_and(|t| t.date() >= today) {
        return Ok(failure(DoHeroFriendlyResult::ActionNotReady));
    }
    let user = sqlx::query("SELECT gold,friendship_point FROM user_info WHERE account_id=?")
        .bind(account_id).fetch_one(&mut *tx).await?;
    let gold: i64 = user.get("gold");
    let friendship: i64 = user.get("friendship_point");
    if gold < gold_cost || friendship < friendship_cost { return Ok(failure(DoHeroFriendlyResult::CostMismatch)); }
    let currency_result = if gold_cost > 0 {
        sqlx::query("UPDATE user_info SET gold=gold-? WHERE account_id=?")
            .bind(gold_cost).bind(account_id).execute(&mut *tx).await?;
        Some(CurrencyResultInfo3 { currency_type: "Gold".into(), add_value: -gold_cost,
            new_value: gold-gold_cost, ..Default::default() })
    } else { None };
    let friendship_point_result = if friendship_cost > 0 {
        sqlx::query("UPDATE user_info SET friendship_point=friendship_point-? WHERE account_id=?")
            .bind(friendship_cost).bind(account_id).execute(&mut *tx).await?;
        Some(FriendshipPointResultInfo { add_value: -friendship_cost, add_daily_acc_value: 0,
            new_value: friendship-friendship_cost, new_daily_acc_value: 0 })
    } else { None };
    let current_point: i32 = friendly.get("friendly_point");
    let new_point = if is_owned { current_point } else { (current_point + points).min(target.unwrap()) };
    // Retain the Inn's action timestamps for recruitment and progression hooks.
    // Owned-hero cooldown decisions use only that hero's persisted timestamps.
    sqlx::query(sqlx::AssertSqlSafe(format!("UPDATE hero_friendly_info SET friendly_point=?, {column}=? WHERE account_id=?")))
        .bind(new_point).bind(&current_time).bind(account_id).execute(&mut *tx).await?;
    let hero_info = if let Some(owned) = owned {
        sqlx::query("UPDATE heroes SET closeness=? WHERE account_id=? AND hero_index=?")
            .bind((owned.get::<i32, _>("closeness") + points).min(1000))
            .bind(account_id).bind(hero_index).execute(&mut *tx).await?;
        if !details.is_object() { return Err(ServerError::Internal("Invalid hero details".into())); }
        details[wire_time] = serde_json::json!(current_time);
        super::save_details(&mut tx, account_id, hero_index, &details).await?;
        // Return the native LastGreetingTime/LastConversationTime/LastGiftTime
        // fields immediately and retain them in future login/lobby snapshots.
        Some(serde_json::from_value(super::info(&mut tx, account_id, hero_index).await?)
            .map_err(|e| ServerError::Internal(e.to_string()))?)
    } else { None };
    let row = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id=?")
        .bind(account_id).fetch_one(&mut *tx).await?;
    let friendly_info = PlayerHeroFriendlyInfo {
        hero_index: row.get("hero_index"), selected_hero_index: row.get("selected_hero_index"),
        friendly_point: row.get("friendly_point"), last_greeting_time: row.get("last_greeting_time"),
        last_conversation_time: row.get("last_conversation_time"), last_gift_time: row.get("last_gift_time"),
        selected_time: row.get("selected_time"), selected_hero_indice: row.get("selected_hero_indices"),
        last_roulette_time: row.get("last_roulette_time"),
    };
    tx.commit().await?;
    Ok(Json(DoHeroFriendlyResponse { base_result: "Success".into(), result: "Success".into(),
        friendly_info: Some(friendly_info), hero_info, currency_result, friendship_point_result }))
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

    // Zero ends the current relationship. The native "change and start"
    // flow sends a separate nonzero request afterward when explicitly chosen.
    // Preserve the day's visitors and reset schedule; cancellation is not a
    // visitor refresh and must never select a replacement recruit.
    if hero_index == 0 {
        let row = sqlx::query("UPDATE hero_friendly_info SET hero_index=0,selected_hero_index=0,friendly_point=0,last_greeting_time=NULL,last_conversation_time=NULL,last_gift_time=NULL WHERE account_id=? RETURNING *")
            .bind(account_id).fetch_optional(&state.db).await?;
        let Some(row) = row else {
            return Ok(Json(ChangeRecruitHeroResponse {
                base_result: "Success".into(), result: ChangeRecruitHeroResult::HeroNotExist.as_str().into(), friendly_info: None,
            }));
        };
        let friendly_info = PlayerHeroFriendlyInfo {
            hero_index: row.get("hero_index"), selected_hero_index: row.get("selected_hero_index"),
            friendly_point: row.get("friendly_point"), last_greeting_time: row.get("last_greeting_time"),
            last_conversation_time: row.get("last_conversation_time"), last_gift_time: row.get("last_gift_time"),
            selected_time: row.get("selected_time"), selected_hero_indice: row.get("selected_hero_indices"),
            last_roulette_time: row.get("last_roulette_time"),
        };
        return Ok(Json(ChangeRecruitHeroResponse {
            base_result: "Success".into(), result: ChangeRecruitHeroResult::Success.as_str().into(), friendly_info: Some(friendly_info),
        }));
    }

    if INN_RECRUITMENT_EXCLUDED_HEROES.contains(&hero_index) {
        return Ok(Json(ChangeRecruitHeroResponse {
            base_result: "Success".to_string(),
            result: ChangeRecruitHeroResult::CannotRecruit.as_str().to_string(),
            friendly_info: None,
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
    if INN_RECRUITMENT_EXCLUDED_HEROES.contains(&index) {return Ok(fail("HeroIndexMismatch"));}
    let Some(c)=state.tables.hero_shop.heroes.get(&index) else {return Ok(fail("HeroIndexMismatch"))};
    let mut tx=state.db.begin().await?;
    crate::api::inventory::item::init(&mut tx,&state,account).await?;
    let owned:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)").bind(account).bind(index).fetch_one(&mut *tx).await?;
    if owned {return Ok(fail("AlreadyRecruited"));}
    let row=sqlx::query("SELECT hero_index,friendly_point FROM hero_friendly_info WHERE account_id=?").bind(account).fetch_optional(&mut *tx).await?;
    let Some(row)=row else {return Ok(fail("HeroNotExist"))};
    if row.get::<i32,_>("hero_index")!=index {return Ok(fail("HeroIndexMismatch"));}
    let Some(required_points) = state.tables.hero_shop.inn_recruitment_points.get(&index) else {
        return Ok(fail("HeroIndexMismatch"));
    };
    if row.get::<i32,_>("friendly_point") < *required_points {return Ok(fail("FriendlyPointMismatch"));}
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
    let section=rand::rng().random_range(1..=8);
    spin_roulette(state,req,section).await
}

async fn spin_roulette(state:AppState,req:GiveRewardHeroInnRouletteRequest,winning_section:i32)->Result<Json<GiveRewardHeroInnRouletteResponse>> {
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;

    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    let account_id = session.account_id;
    let roulette_index: i32 = req.roulette_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(1);

    let mut tx=state.db.begin().await?;
    // Serialize quota checks and reward credits in the same transaction.
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(account_id).execute(&mut *tx).await?;

    // Get today's date for spin tracking
    let today = Utc::now().format("%Y-%m-%d").to_string();

    // Calculate max spins: 1 base + number of owned heroes visiting the inn
    let friendly_row = sqlx::query("SELECT selected_hero_indices FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&mut *tx)
        .await?;

    let selected_hero_indices: Vec<i32> = friendly_row
        .and_then(|row| row.get::<Option<String>, _>("selected_hero_indices"))
        .map(|s| s.split(',').filter_map(|x| x.parse().ok()).collect())
        .unwrap_or_default();

    let owned_heroes: Vec<i32> = sqlx::query("SELECT hero_index FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&mut *tx)
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
    .fetch_optional(&mut *tx)
    .await?
    .unwrap_or(0);

    if spins_today >= max_spins {
        return Ok(Json(GiveRewardHeroInnRouletteResponse {
            base_result: "Success".to_string(),
            result: "NoMoreRoulette".to_string(),
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
    .execute(&mut *tx)
    .await?;

    let new_spins_today = spins_today + 1;
    let remain_count = (max_spins - new_spins_today).max(0);

    // Get the reward for this section (section is 1-based)
    let rewards = get_default_roulette_rewards(roulette_index);
    let winning_reward = rewards.iter().find(|r| r.section == winning_section).cloned();

    let mut currency_results: Vec<CurrencyResultInfo3> = Vec::new();
    let mut friendship_point_result: Option<FriendshipPointResultInfo> = None;

    if let Some(reward) = winning_reward {
        let amount=reward.value1 as i64;
        let result=super::currency(&mut tx,account_id,&reward.type1,amount).await?;
        let value=crate::api::inventory::item::n(&result,"NewValue");
        if reward.type1=="FriendshipPoint" {
            friendship_point_result=Some(FriendshipPointResultInfo {
                add_value:amount,new_value:value,
                add_daily_acc_value:crate::api::inventory::item::n(&result,"AddDailyAccValue").clamp(0,i32::MAX as i64) as i32,
                new_daily_acc_value:crate::api::inventory::item::n(&result,"NewDailyAccValue").clamp(0,i32::MAX as i64) as i32,
            });
        } else {
            currency_results.push(CurrencyResultInfo3 {currency_type:reward.type1,add_value:amount,new_value:value,..Default::default()});
        }
    }
    tx.commit().await?;

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

#[cfg(test)]
#[path = "inn_tests.rs"]
mod tests;
