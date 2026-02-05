use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    models::{
        user::UserInfo,
        hero::HeroInfo,
        item::ItemInfo,
        hero_inn::PlayerHeroFriendlyInfo,
    },
    state::AppState,
};

/// First lobby request (called after initial login for new users)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct FirstLobbyRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
    pub nick: Option<String>,
}

/// First lobby response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct FirstLobbyResponse {
    pub base_result: String,
    pub result: String,
}

/// Handle first lobby request (set initial nickname)
pub async fn first_lobby(
    State(state): State<AppState>,
    Form(req): Form<FirstLobbyRequest>,
) -> Result<Json<FirstLobbyResponse>> {
    // Client sends SessionKey, not SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    if let Some(nick) = req.nick {
        // Update nickname
        sqlx::query("UPDATE accounts SET nick = ? WHERE account_id = ?")
            .bind(&nick)
            .bind(session.account_id)
            .execute(&state.db)
            .await?;
    }

    Ok(Json(FirstLobbyResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
    }))
}

/// Chapter dungeon info for lobby responses
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

/// Enter lobby request (called when entering the main game lobby)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct EnterLobbyRequest {
    pub session_id: Option<String>,
    #[serde(rename = "SessionKey")]
    pub session_key: Option<String>,
}

/// Enter lobby response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct EnterLobbyResponse {
    pub base_result: String,
    pub result: String,
    pub user_info: Option<UserInfo>,
    pub heroes: Vec<HeroInfo>,
    pub items: Vec<ItemInfo>,
    pub dungeon_infos: Vec<ChapterDungeonInfo>,
    pub server_time: String,
    #[serde(rename = "ServerUTCTime")]
    pub server_utc_time: String,
    pub server_local_time: String,
    #[serde(rename = "ServerUTCOffsetHour")]
    pub server_utc_offset_hour: i32,
    pub new_mail_count: i32,
    pub friend_request_count: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendly_info: Option<PlayerHeroFriendlyInfo>,
}

/// Handle enter lobby request
pub async fn enter_lobby(
    State(state): State<AppState>,
    Form(req): Form<EnterLobbyRequest>,
) -> Result<Json<EnterLobbyResponse>> {
    // Client sends SessionKey, not SessionId
    let session_id = req.session_key.or(req.session_id)
        .ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;

    // Fetch account info
    let account_row = sqlx::query("SELECT nick FROM accounts WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?
        .ok_or_else(|| ServerError::NotFound("Account not found".to_string()))?;
    
    let nick: String = account_row.get("nick");

    // Fetch user info
    let user_info_row = sqlx::query("SELECT * FROM user_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?;

    let user_info = match user_info_row {
        Some(row) => Some(UserInfo {
            session_key: session_id.clone(),
            account_id,
            nick,
            gold: row.get("gold"),
            gem: row.get("gem"),
            pay_gem: row.get("pay_gem"),
            pvp_coin: row.get("pvp_coin"),
            stamina: row.get("stamina"),
            stamina_recharge_time: None, // Handled separately if needed
            team_level: row.get("team_level"),
            team_exp: row.get("team_exp"),
            sword: row.get("sword"),
            sword_recharge_time: None, // Handled separately if needed
            sword2: row.get("sword2"),
            avatar_hero_index: row.get("avatar_hero_index"),
            royal_point: row.get("royal_point"),
            raid_point: row.get("raid_point"),
            mileage: row.get("mileage"),
            friendship_point: row.get("friendship_point"),
            guild_raid_ticket: row.get("guild_raid_ticket"),
            world_boss_ticket: row.get("world_boss_ticket"),
            ..Default::default()
        }),
        None => None,
    };

    // Fetch heroes
    let hero_rows = sqlx::query("SELECT * FROM heroes WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&state.db)
        .await?;

    let heroes: Vec<HeroInfo> = hero_rows.iter().map(|row| HeroInfo {
        hero_id: row.get("hero_id"),
        hero_index: row.get("hero_index"),
        star: row.get("star"),
        level: row.get("level"),
        exp: row.get("exp"),
        transcend: row.get("transcend"),
        awakened: row.get("awakened"),
        skill_level_1: row.get("skill_level_1"),
        skill_level_2: row.get("skill_level_2"),
        skill_level_3: row.get("skill_level_3"),
        skill_level_4: row.get("skill_level_4"),
        unique_weapon_id: row.get("unique_weapon_id"),
        is_bookmarked: row.get::<i32, _>("is_bookmarked") != 0,
        closeness: row.get("closeness"),
        transcend_skill_point: 0,
        equip_item_slot_index_1: row.get("equip_item_slot_index_1"),
        equip_item_slot_index_2: row.get("equip_item_slot_index_2"),
        equip_item_slot_index_3: row.get("equip_item_slot_index_3"),
        equip_item_slot_index_4: row.get("equip_item_slot_index_4"),
        equip_item_slot_index_5: row.get("equip_item_slot_index_5"),
        equip_item_slot_index_6: row.get("equip_item_slot_index_6"),
        equip_item_slot_index_7: row.get("equip_item_slot_index_7"),
        equip_item_slot_index_8: row.get("equip_item_slot_index_8"),
        equip_item_slot_index_9: row.get("equip_item_slot_index_9"),
        equip_item_slot_index_10: row.get("equip_item_slot_index_10"),
    }).collect();

    // Fetch items
    let item_rows = sqlx::query("SELECT * FROM items WHERE account_id = ?")
        .bind(account_id)
        .fetch_all(&state.db)
        .await?;

    let items: Vec<ItemInfo> = item_rows.iter().map(|row| ItemInfo {
        item_index: row.get("item_index"),
        count: row.get("count"),
    }).collect();

    // Count unread mail
    let mail_count: i32 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM mails WHERE account_id = ? AND is_received = 0"
    )
    .bind(account_id)
    .fetch_one(&state.db)
    .await?;

    // Count friend requests
    let friend_request_count: i32 = sqlx::query_scalar(
        "SELECT COUNT(*) FROM friends WHERE friend_account_id = ? AND status = 0"
    )
    .bind(account_id)
    .fetch_one(&state.db)
    .await?;

    // Fetch campaign progress for dungeon infos
    let campaign_rows = sqlx::query(
        "SELECT chapter_id, dungeon_id, clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? ORDER BY chapter_id, dungeon_id"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await
    .unwrap_or_default();

    // Build dungeon infos from database
    // 
    // MaxStar encoding: difficulty * 10 + stars
    //   - Chapter 1 has MinDifficulty = Normal (1), not Easy (0)!
    //   - So the DB stores best_star = 3, but we send MaxStar = 13 (Normal 3-star)
    //   - The client checks: (MaxStar / 10) >= MinDifficulty to verify completion
    //
    let dungeon_infos: Vec<ChapterDungeonInfo> = campaign_rows.iter().map(|row| {
        let chapter_id: i32 = row.get("chapter_id");
        let clear_count: i32 = row.get("clear_count");
        let best_star: i32 = row.get("best_star");
        let completed_time: Option<String> = row.get("completed_time");
        let is_completed = clear_count > 0 || best_star > 0 || completed_time.is_some();
        
        // For chapter 1-10, MinDifficulty is Normal (1), so encode as Normal + stars
        let max_star = if is_completed && chapter_id <= 10 {
            (10 + best_star) as i16
        } else {
            best_star as i16
        };
        
        // FirstRewardedDiff: For Normal cleared (chapter 1-10), use bit 1 (value 2)
        let first_rewarded_diff = if is_completed { 2 } else { 0 };
        
        ChapterDungeonInfo {
            chapter_index: chapter_id,
            dungeon_index: row.get("dungeon_id"),
            max_star,
            first_rewarded_diff,
            scenario_complete: if is_completed { 1 } else { 0 },
            visited_time: Some(state.server_time_str()),
            completed_time,
            daily_completed_count: clear_count,
            reset_count: 0,
        }
    }).collect();

    // Note: We don't pre-populate dungeon 1-1 here for new users.
    // The tutorial flow (tutorial 10000) will unlock 1-1 when appropriate.
    // For returning users who skip tutorial, user.rs handles pre-populating dungeons.

    // Fetch Hero Inn friendly info if exists
    let friendly_info = sqlx::query("SELECT * FROM hero_friendly_info WHERE account_id = ?")
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?
        .map(|row| PlayerHeroFriendlyInfo {
            hero_index: row.get("hero_index"),
            selected_hero_index: row.get("selected_hero_index"),
            friendly_point: row.get("friendly_point"),
            last_greeting_time: row.get("last_greeting_time"),
            last_conversation_time: row.get("last_conversation_time"),
            last_gift_time: row.get("last_gift_time"),
            selected_time: row.get("selected_time"),
            selected_hero_indice: row.get("selected_hero_indices"),
            last_roulette_time: row.get("last_roulette_time"),
        });

    Ok(Json(EnterLobbyResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        user_info,
        heroes,
        items,
        dungeon_infos,
        server_time: state.server_time_str(),
        server_utc_time: state.server_utc_time_str(),
        server_local_time: state.server_time_str(),
        server_utc_offset_hour: 0,
        new_mail_count: mail_count,
        friend_request_count,
        friendly_info,
    }))
}
