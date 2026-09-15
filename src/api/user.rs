use axum::{
    extract::State,
    body::Bytes,
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    crypto::{generate_session_key, generate_aes_key},
    error::Result,
    models::{
        user::{UserInfo, PlayerMiscInfo, PlayerBattleInfo},
        hero::HeroInfo,
        item::ItemInfo,
        equip::EquipItemInfo,
        BaseResultType,
    },
    state::AppState,
};

/// Login request matching client's NShared.Login.Request
/// Note: Form data always comes as strings, so we accept all as String and parse later
/// Many fields are reserved for future use (client sends them but we don't process yet)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct LoginRequest {
    pub login_method: Option<String>,
    pub login_id: Option<String>,
    pub legacy_login_id: Option<String>,
    pub selected_account_id: Option<String>,
    pub encrypt_salt: Option<String>,
    pub device_id: Option<String>,
    pub version: Option<String>,
    pub battle_version: Option<String>,
    #[serde(rename = "UTCOffsetHour")]
    pub utc_offset_hour: Option<String>,
    pub forced_host: Option<String>,
    pub device_os: Option<String>,
    pub device_platform: Option<String>,
    pub device_model: Option<String>,
    pub device_memory_size: Option<String>,
    pub device_token: Option<String>,
    pub language: Option<String>,
    pub country_code: Option<String>,
    // Session id from base Request class
    pub session_id: Option<String>,
    // SessionKey sent separately
    pub session_key: Option<String>,
}

/// Stamina result info matching client's NShared.StaminaResultInfo
#[derive(Debug, Serialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct StaminaResultInfo {
    #[serde(rename = "Type")]
    pub stamina_type: String,  // Enum string like "Chicken", "Sword", etc.
    pub add_value: i32,
    pub new_value: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_recharge_time: Option<String>,
    pub next_recharge_remain_time: i32,
    pub full_recharge_remain_time: i32,
    pub recharge_count: i32,
    pub is_hide: bool,
}

/// Tutorial info matching client's NShared.TutorialInfo
#[derive(Debug, Serialize, Clone)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialInfo {
    pub tutorial_index: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub completed_time: Option<String>,
}

/// Player hero friendly info matching client's NShared.PlayerHeroFriendlyInfo
#[derive(Debug, Serialize, Clone, Default)]
#[serde(rename_all = "PascalCase")]
pub struct PlayerHeroFriendlyInfo {
    pub hero_index: i32,
    pub selected_hero_index: i32,
    pub friendly_point: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_greeting_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_conversation_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_gift_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub selected_time: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub selected_hero_indice: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_roulette_time: Option<String>,
}

/// Chapter dungeon info matching client's NShared.ChapterDungeonInfo
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

/// Login response matching client's NShared.Login.Response
#[derive(Debug, Serialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct LoginResponse {
    pub player_avatar_hero_info: serde_json::Value,
    pub base_result: String,  // Must be string like "Success" for C# enum parsing
    #[serde(skip_serializing_if = "Option::is_none")]
    pub internal_error_message: Option<String>,
    pub result: String,  // Must be string like "Success" for C# enum parsing
    pub server_private_ip: String,
    pub server_port: i32,
    pub admin_level: i32,
    pub sticky_host: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub alternative_host: Option<String>,
    pub is_new_user: bool,
    pub server_local_time: String,
    #[serde(rename = "ServerUTCTime")]
    pub server_utc_time: String,
    #[serde(rename = "ServerUTCOffsetHour")]
    pub server_utc_offset_hour: i32,
    pub login_id: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub face_book_id: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub google_plus_id: Option<String>,
    pub event_push: bool,
    pub guild_raid_push: bool,
    pub guild_suppress_push: bool,
    pub night_push: bool,
    pub user_info: UserInfo,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub battle_info: Option<PlayerBattleInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub misc_info: Option<PlayerMiscInfo>,
    pub guild_id: i64,
    pub heroes: Vec<HeroInfo>,
    pub items: Vec<ItemInfo>,
    // Additional required fields
    pub stamina_results: Vec<StaminaResultInfo>,
    pub tutorials: Vec<TutorialInfo>,
    // Always include these arrays (even when empty) - client expects keys to exist
    pub equip_items: Vec<EquipItemInfo>,
    pub shop_list_items: Vec<serde_json::Value>,
    pub shop_restock_times: Vec<serde_json::Value>,
    pub chapter_dungeons: Vec<ChapterDungeonInfo>,
    pub towers: Vec<serde_json::Value>,
    pub deck_infos: Vec<serde_json::Value>,
    pub achievement_infos: Vec<serde_json::Value>,
    pub is_tutorial_skip: bool,
    pub server_name: String,
    // Critical field - account creation time
    pub created_time: String,
    // Additional arrays that the client expects (always include even when empty)
    pub message_server_info: serde_json::Value,
    pub friend_infos: Vec<serde_json::Value>,
    pub friend_invitor_infos: Vec<serde_json::Value>,
    pub attendance_infos: Vec<serde_json::Value>,
    pub hideout_dungeons: Vec<serde_json::Value>,
    pub conquest_dungeons: Vec<serde_json::Value>,
    pub contents_statuses: Vec<serde_json::Value>,
    pub contents_values: Vec<serde_json::Value>,
    // More arrays used in AfterTableLoad_OnLoginResponse
    pub hero_rune_page_infos: Vec<serde_json::Value>,
    pub item_time_durations: Vec<serde_json::Value>,
    pub purchase_time_durations: Vec<serde_json::Value>,
    pub world_map_event_time_infos: Vec<serde_json::Value>,
    pub world_map_event_infos: Vec<serde_json::Value>,
    pub wanted_quest_infos: Vec<serde_json::Value>,
    pub raid_infos: Vec<serde_json::Value>,
    pub craft_slot_infos: Vec<serde_json::Value>,
    pub costume_infos: Vec<serde_json::Value>,
    pub weapon_costume_infos: Vec<serde_json::Value>,
    pub hair_costume_infos: Vec<serde_json::Value>,
    pub costume_storage_slot_infos: Vec<serde_json::Value>,
    pub player_accessory_costume_infos: Vec<serde_json::Value>,
    pub purchase_marketing_infos: Vec<serde_json::Value>,
    pub player_item_use_infos: Vec<serde_json::Value>,
    pub godking_trial_dungeons: Vec<serde_json::Value>,
    pub under_prison_infos: Vec<serde_json::Value>,
    pub play_record_infos: Vec<serde_json::Value>,
    pub login_daily_infos: Vec<serde_json::Value>,
    pub player_archive_infos: Vec<serde_json::Value>,
    pub player_currency_infos: Vec<serde_json::Value>,
    pub newbie_mission_infos: Vec<serde_json::Value>,
    pub free_equip_gacha_infos: Vec<serde_json::Value>,
    pub equip_gacha_infos: Vec<serde_json::Value>,
    pub chapter_reward_infos: Vec<serde_json::Value>,
    pub class_buff_point_infos: Vec<serde_json::Value>,
    pub class_buff_infos: Vec<serde_json::Value>,
    pub dispatch_battle_infos: Vec<serde_json::Value>,
    pub monthly_hero_infos: Vec<serde_json::Value>,
    pub sub_quest_infos: Vec<serde_json::Value>,
    pub eclipse_dungeon_infos: Vec<serde_json::Value>,
    pub clear_mission_infos: Vec<serde_json::Value>,
    pub player_any_miscs: Vec<serde_json::Value>,
    pub soul_weapon_infos: Vec<serde_json::Value>,
    pub player_product_purchase_infos: Vec<serde_json::Value>,
    pub drop_bonus_events: Vec<serde_json::Value>,
    pub pay_shop_item_event_infos: Vec<serde_json::Value>,
    pub send_recv_friendship_point_infos: Vec<serde_json::Value>,
    pub sent_friendship_point_infos: Vec<serde_json::Value>,
    pub npc_friendly_infos: Vec<serde_json::Value>,
    pub chat_ban_infos: Vec<serde_json::Value>,
    // Hero friendly info - CRITICAL: client crashes if null
    pub hero_friendly_info: PlayerHeroFriendlyInfo,
}

/// Parse form-urlencoded body into LoginRequest manually
fn parse_login_request(body: &str) -> LoginRequest {
    use std::collections::HashMap;
    
    // Parse the form data
    let params: HashMap<String, String> = body
        .split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            match (parts.next(), parts.next()) {
                (Some(key), Some(value)) => {
                    // URL decode the value
                    let decoded_value = urlencoding::decode(value)
                        .map(|s| s.into_owned())
                        .unwrap_or_else(|_| value.to_string());
                    Some((key.to_string(), decoded_value))
                }
                _ => None,
            }
        })
        .collect();
    
    LoginRequest {
        login_method: params.get("LoginMethod").cloned(),
        login_id: params.get("LoginId").cloned(),
        legacy_login_id: params.get("LegacyLoginId").cloned(),
        selected_account_id: params.get("SelectedAccountId").cloned(),
        encrypt_salt: params.get("EncryptSalt").cloned(),
        device_id: params.get("DeviceId").cloned(),
        version: params.get("Version").cloned(),
        battle_version: params.get("BattleVersion").cloned(),
        utc_offset_hour: params.get("UTCOffsetHour").cloned(),
        forced_host: params.get("ForcedHost").cloned(),
        device_os: params.get("DeviceOs").cloned(),
        device_platform: params.get("DevicePlatform").cloned(),
        device_model: params.get("DeviceModel").cloned(),
        device_memory_size: params.get("DeviceMemorySize").cloned(),
        device_token: params.get("DeviceToken").cloned(),
        language: params.get("Language").cloned(),
        country_code: params.get("CountryCode").cloned(),
        session_id: params.get("SessionId").cloned(),
        session_key: params.get("SessionKey").cloned(),
    }
}

/// Handle login request
/// Note: Client sends form-urlencoded data, not JSON
pub async fn login(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<LoginResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("Login request body: {}", body_str);
    
    let req = parse_login_request(&body_str);
    
    // For guest login, use DeviceId as the unique identifier to allow multiple guest accounts
    let raw_login_id = req.login_id.clone().unwrap_or_default();
    let device_id = req.device_id.clone().unwrap_or_default();
    
    let login_id = if raw_login_id.to_lowercase() == "guest" && !device_id.is_empty() {
        // Use device_id for guest accounts to allow multiple devices to have separate accounts
        format!("guest_{}", device_id)
    } else if raw_login_id.is_empty() {
        // Fallback to device_id or generate new UUID
        if !device_id.is_empty() {
            device_id.clone()
        } else {
            uuid::Uuid::new_v4().to_string()
        }
    } else {
        raw_login_id
    };
    
    let login_method: i32 = req.login_method.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);

    tracing::info!("Login request from: {} (device: {}, method: {})", login_id, device_id, login_method);

    // Check if account exists
    let existing_account = sqlx::query(
        "SELECT account_id, nick, session_key FROM accounts WHERE login_id = ?"
    )
    .bind(&login_id)
    .fetch_optional(&state.db)
    .await?;

    let (account_id, nick, is_new_user) = match existing_account {
        Some(row) => {
            let account_id: i64 = row.get("account_id");
            let nick: String = row.get("nick");
            (account_id, nick, false)
        }
        None => {
            let mut tx = state.db.begin().await?;
            // Create new account
            let nick = format!("Raider{}", rand::random::<u32>() % 100000);
            
            let result = sqlx::query(
                "INSERT INTO accounts (login_id, login_method, device_id, nick) VALUES (?, ?, ?, ?)"
            )
            .bind(&login_id)
            .bind(login_method)
            .bind(&device_id)
            .bind(&nick)
            .execute(&mut *tx)
            .await?;
            
            let account_id = result.last_insert_rowid();
            
            // Create user_info entry with starting friendship points for Hero's Inn
            sqlx::query(
                "INSERT INTO user_info (account_id, friendship_point) VALUES (?, 6000)"
            )
            .bind(account_id)
            .execute(&mut *tx)
            .await?;
            
            sqlx::query("INSERT INTO tutorial_settings (account_id, is_skipped) VALUES (?, 0)")
                .bind(account_id).execute(&mut *tx).await?;
            // Kasel is present in the opening scene. The tutorial recruits the rest.
            let kasel = state.tables.tutorials.support.items.get(&1)
                .ok_or_else(|| crate::error::ServerError::Internal("Missing starter hero data".into()))?;
            sqlx::query("INSERT INTO heroes (account_id, hero_id, hero_index, star, level) VALUES (?, 1, ?, ?, ?)")
                .bind(account_id).bind(kasel.hero_index).bind(kasel.star).bind(kasel.level)
                .execute(&mut *tx).await?;

            // New users start with empty tutorial progress
            // Tutorials will be triggered by EventTriggers in the client
            // The client will call begin_tutorial and complete_tutorial endpoints
            
            // Send welcome mail
            sqlx::query(
                "INSERT INTO mails (account_id, sender, title, content, reward_gold, reward_gem) VALUES (?, 'System', 'Welcome to King''s Raid!', 'Thank you for playing on this private server. Enjoy your adventure!', 1000000, 5000)"
            )
            .bind(account_id)
            .execute(&mut *tx)
            .await?;
            
            tx.commit().await?;
            (account_id, nick, true)
        }
    };

    // Generate session and AES keys
    let session_key = generate_session_key();
    let aes_key = generate_aes_key();

    // Update session in database
    sqlx::query(
        "UPDATE accounts SET session_key = ?, aes_key = ?, last_login = datetime('now') WHERE account_id = ?"
    )
    .bind(&session_key)
    .bind(&aes_key)
    .bind(account_id)
    .execute(&state.db)
    .await?;

    // Store session in memory
    state.create_session(session_key.clone(), account_id, aes_key.clone());

    // Fetch user info
    let user_info_row = sqlx::query(
        "SELECT * FROM user_info WHERE account_id = ?"
    )
    .bind(account_id)
    .fetch_optional(&state.db)
    .await?;

    let user_info = match user_info_row {
        Some(row) => UserInfo {
            session_key: session_key.clone(),
            account_id,
            nick: nick.clone(),
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
            event_dungeon_point: row.get("event_dungeon_point"),
            mileage: row.get("mileage"),
            friendship_point: row.get("friendship_point"),
            guild_raid_ticket: row.get("guild_raid_ticket"),
            world_boss_ticket: row.get("world_boss_ticket"),
            ..Default::default()
        },
        None => UserInfo::new_user(account_id, &nick, &session_key),
    };

    let heroes = super::hero::snapshot(&mut *state.db.acquire().await?, account_id).await?;

    // Fetch items
    let item_rows = sqlx::query(
        "SELECT * FROM items WHERE account_id = ? AND count > 0"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await?;

    let items: Vec<ItemInfo> = item_rows.iter().map(|row| ItemInfo {
        item_index: row.get("item_index"),
        count: row.get("count"),
        locked: row.get::<i32,_>("locked") as u8,
        created_time: row.get("created_time"),
        uid: row.get::<i32,_>("item_index").to_string(),
    }).collect();

    // Fetch equipment items
    let equip_rows = sqlx::query(
        "SELECT * FROM equip_items WHERE account_id = ?"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await?;

    let equip_items: Vec<EquipItemInfo> = equip_rows.iter().map(EquipItemInfo::from_row).collect();

    // Check guild membership
    let guild_row = sqlx::query(
        "SELECT guild_id FROM guild_members WHERE account_id = ?"
    )
    .bind(account_id)
    .fetch_optional(&state.db)
    .await?;
    
    let guild_id = guild_row.map(|r| r.get("guild_id")).unwrap_or(0i64);

    // Generate stamina results for common stamina types - use actual DB values
    let stamina_results = vec![
        StaminaResultInfo {
            stamina_type: "Chicken".to_string(),  // Main stamina
            add_value: 0,
            new_value: user_info.stamina,  // Use actual stamina from database
            stamina_recharge_time: None,
            next_recharge_remain_time: 0,
            full_recharge_remain_time: 0,
            recharge_count: 0,
            is_hide: false,
        },
        StaminaResultInfo {
            stamina_type: "Sword".to_string(),  // Arena entries
            add_value: 0,
            new_value: user_info.sword,  // Use actual sword from database
            stamina_recharge_time: None,
            next_recharge_remain_time: 0,
            full_recharge_remain_time: 0,
            recharge_count: 0,
            is_hide: false,
        },
        StaminaResultInfo {
            stamina_type: "GuildRaidTicket".to_string(),
            add_value: 0,
            new_value: user_info.guild_raid_ticket,  // Use actual DB value
            stamina_recharge_time: None,
            next_recharge_remain_time: 0,
            full_recharge_remain_time: 0,
            recharge_count: 0,
            is_hide: false,
        },
        StaminaResultInfo {
            stamina_type: "WorldBossTicket".to_string(),
            add_value: 0,
            new_value: user_info.world_boss_ticket,  // Use actual DB value
            stamina_recharge_time: None,
            next_recharge_remain_time: 0,
            full_recharge_remain_time: 0,
            recharge_count: 0,
            is_hide: false,
        },
    ];

    // Fetch tutorial progress - only return completed tutorials
    // The client expects an array of TutorialInfo with TutorialIndex and CompletedTime
    let tutorial_rows = sqlx::query(
        "SELECT tutorial_index, completed_time FROM tutorial_progress WHERE account_id = ? AND is_completed = 1"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await
    .unwrap_or_default();

    // Convert to TutorialInfo - only completed tutorials are included
    let tutorials: Vec<TutorialInfo> = tutorial_rows.iter().map(|row| TutorialInfo {
        tutorial_index: row.get("tutorial_index"),
        completed_time: row.get::<Option<String>, _>("completed_time"),
    }).collect();

    // Keep incomplete tutorials enabled across reconnects. Completion is tracked per index.
    let tutorial_skip: bool = sqlx::query_scalar("SELECT is_skipped FROM tutorial_settings WHERE account_id = ?")
        .bind(account_id).fetch_optional(&state.db).await?.unwrap_or(false);

    // Fetch campaign progress and build chapter_dungeons
    let campaign_rows = sqlx::query(
        "SELECT chapter_id, dungeon_id, clear_count, best_star, completed_time FROM campaign_progress WHERE account_id = ? ORDER BY chapter_id, dungeon_id"
    )
    .bind(account_id)
    .fetch_all(&state.db)
    .await
    .unwrap_or_default();

    // Encode difficulty and stars consistently with tutorial completion.
    let mut chapter_dungeons: Vec<ChapterDungeonInfo> = campaign_rows.iter().map(|row| {
        let chapter_id: i32 = row.get("chapter_id");
        let clear_count: i32 = row.get("clear_count");
        let best_star: i32 = row.get("best_star");
        let completed_time: Option<String> = row.get("completed_time");
        let is_completed = clear_count > 0 || best_star > 0 || completed_time.is_some();
        
        let difficulty = state.tables.tutorials.dungeon_difficulty(chapter_id, row.get("dungeon_id"));
        let max_star = if is_completed { (difficulty * 10 + best_star) as i16 } else { 0 };
        let first_rewarded_diff = if is_completed { (1 << difficulty) as i16 } else { 0 };
        
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

    // ==========================================================================
    // DUNGEON PRE-POPULATION (when tutorial is skipped)
    // ==========================================================================
    //
    // When tutorial_skip is true and user has no dungeon progress, we need to
    // pre-populate dungeon 1-1 so the user can start playing.
    //
    // IMPORTANT: Chapter 1 has MinDifficulty = Normal (1), not Easy (0)!
    // This affects the MaxStar encoding:
    //   - Normal difficulty (1) with 3 stars = 1*10 + 3 = 13
    //   - The client checks: (MaxStar / 10) >= MinDifficulty
    //
    // FirstRewardedDiff bitmask for Normal cleared = 2 (bit 1)
    //
    // CompletedTime must be non-empty for the client to consider it "completed"
    // (see DungeonCompleteChecker.CheckDefault in client code)
    //
    if chapter_dungeons.is_empty() && tutorial_skip {
        // Only unlock 1-1, NOT completed - player starts fresh and must play 1-1 first
        tracing::info!("Tutorial skipped - unlocking dungeon 1-1 (not completed)");
        
        sqlx::query(
            "INSERT INTO campaign_progress (account_id, chapter_id, dungeon_id, is_unlocked, clear_count, best_star, completed_time) 
             VALUES (?, 1, 1, 1, 0, 0, NULL)
             ON CONFLICT(account_id, chapter_id, dungeon_id) DO NOTHING"
        )
        .bind(account_id)
        .execute(&state.db)
        .await
        .ok();
        
        chapter_dungeons.push(ChapterDungeonInfo {
            chapter_index: 1,
            dungeon_index: 1,
            max_star: 0,              // Not completed yet
            first_rewarded_diff: 0,   // No rewards claimed
            scenario_complete: 0,     // Scenario not done
            visited_time: None,
            completed_time: None,     // Must be None/empty for "not completed"
            daily_completed_count: 0,
            reset_count: 0,
        });
    }

    // Set current position to dungeon 1-1 (the starting point)
    // The client will update this position as the player navigates
    let (current_chapter_index, current_dungeon_index) = (1, 1);

    let craft_slot_infos = super::craft::login_data(&state,account_id).await?;
    let inventory_settings = sqlx::query("SELECT * FROM inventory_settings WHERE account_id=?").bind(account_id).fetch_one(&state.db).await?;
    let (friend_infos, friend_invitor_infos, point_infos, sent_points) = super::friend::login_data(&state, account_id).await?;
    let costume_infos = super::hero::costumes(&mut *state.db.acquire().await?, account_id).await?;
    let costume_storage_slot_infos = super::hero::presets(&mut *state.db.acquire().await?, account_id).await?;

    let player_avatar_hero_info = super::hero::avatar_info(&mut *state.db.acquire().await?,&state,account_id).await?;
    let response = LoginResponse {
        player_avatar_hero_info,
        base_result: "Success".to_string(),
        internal_error_message: None,
        result: "Success".to_string(),
        server_private_ip: "127.0.0.1".to_string(),
        server_port: 8080,
        admin_level: 0,
        sticky_host: "http://127.0.0.1:8080/".to_string(),
        alternative_host: None,
        is_new_user,
        server_local_time: state.server_time_str(),
        server_utc_time: state.server_utc_time_str(),
        server_utc_offset_hour: 0,
        login_id: login_id.clone(),
        face_book_id: None,
        google_plus_id: None,
        event_push: true,
        guild_raid_push: true,
        guild_suppress_push: true,
        night_push: false,
        user_info,
        battle_info: Some(PlayerBattleInfo::default()),
        misc_info: Some(PlayerMiscInfo {
            inventory_extend: inventory_settings.get("inventory_extend"),
            chest_extend: inventory_settings.get("chest_extend"),
            daily_acc_friendship_point: sqlx::query_scalar("SELECT points FROM friend_daily WHERE account_id=? AND day=?").bind(account_id).bind(state.server_date()).fetch_optional(&state.db).await?.unwrap_or(0),
            daily_acc_friendship_point_reset_time: (chrono::Utc::now().date_naive()+chrono::Duration::days(1)).format("%Y-%m-%d 00:00:00").to_string(),
            nick_change_count: 0,
            inventory_expand_count: 0,
            last_login_time: state.server_time_str(),
            total_play_time: 0,
            current_chapter_index,
            current_dungeon_index,
        }),
        guild_id,
        heroes,
        items,
        stamina_results,
        tutorials,
        equip_items,
        shop_list_items: vec![],
        shop_restock_times: vec![],
        chapter_dungeons,
        towers: vec![],
        deck_infos: vec![],
        achievement_infos: vec![],
        is_tutorial_skip: tutorial_skip,
        server_name: "Private Server".to_string(),
        created_time: state.server_time_str(),
        message_server_info: serde_json::json!({"Address":state.chat.address, "Port":state.chat.port}),
        send_recv_friendship_point_infos: point_infos,
        sent_friendship_point_infos: sent_points,
        friend_infos,
        friend_invitor_infos,
        attendance_infos: vec![],
        hideout_dungeons: vec![],
        conquest_dungeons: vec![],
        contents_statuses: vec![],
        contents_values: vec![],
        // All the additional empty arrays
        hero_rune_page_infos: vec![],
        item_time_durations: super::item::booster_login(&state,account_id).await?,
        purchase_time_durations: vec![],
        world_map_event_time_infos: vec![],
        world_map_event_infos: vec![],
        wanted_quest_infos: vec![],
        raid_infos: vec![],
        craft_slot_infos,
        costume_infos,
        weapon_costume_infos: vec![],
        hair_costume_infos: vec![],
        costume_storage_slot_infos,
        player_accessory_costume_infos: vec![],
        purchase_marketing_infos: vec![],
        player_item_use_infos: vec![],
        godking_trial_dungeons: vec![],
        under_prison_infos: vec![],
        play_record_infos: vec![],
        login_daily_infos: vec![],
        player_archive_infos: vec![],
        player_currency_infos: vec![],
        newbie_mission_infos: vec![],
        free_equip_gacha_infos: vec![],
        equip_gacha_infos: vec![],
        chapter_reward_infos: vec![],
        class_buff_point_infos: vec![],
        class_buff_infos: vec![],
        dispatch_battle_infos: vec![],
        monthly_hero_infos: vec![],
        sub_quest_infos: vec![],
        eclipse_dungeon_infos: vec![],
        clear_mission_infos: vec![],
        player_any_miscs: vec![],
        soul_weapon_infos: vec![],
        player_product_purchase_infos: vec![],
        drop_bonus_events: vec![],
        pay_shop_item_event_infos: vec![],
        npc_friendly_infos: vec![],
        chat_ban_infos: vec![],
        // Hero friendly info - CRITICAL: cannot be null or client crashes
        hero_friendly_info: PlayerHeroFriendlyInfo {
            hero_index: 0,
            selected_hero_index: 0,
            friendly_point: 0,
            last_greeting_time: None,
            last_conversation_time: None,
            last_gift_time: None,
            selected_time: None,
            selected_hero_indice: Some("".to_string()), // Empty string to avoid null parse issues
            last_roulette_time: None,
        },
    };

    tracing::info!("Login successful for account_id: {}", account_id);
    Ok(Json(response))
}

/// Logout request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct LogoutRequest {
    pub session_id: Option<String>,
}

/// Logout response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct LogoutResponse {
    pub base_result: i32,
}

/// Handle logout request
pub async fn logout(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<LogoutResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    // Parse session_id from form data
    let session_id = body_str
        .split('&')
        .find_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            if parts.next() == Some("SessionId") {
                parts.next().map(|s| s.to_string())
            } else {
                None
            }
        });
    
    if let Some(session_id) = session_id {
        state.remove_session(&session_id);
        
        // Clear session in database
        sqlx::query(
            "UPDATE accounts SET session_key = NULL WHERE session_key = ?"
        )
        .bind(&session_id)
        .execute(&state.db)
        .await?;
    }

    Ok(Json(LogoutResponse {
        base_result: BaseResultType::Success as i32,
    }))
}

/// Get certificate request (empty, just needs session)
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct GetCertificateRequest {
    pub session_id: Option<String>,
}

/// Get certificate response - provides RSA certificate for encryption
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetCertificateResponse {
    pub base_result: String,  // Must be string "Success" or "Fail" for C# enum parsing
    pub result: String,       // Must be string "Success" or "Fail" for C# enum parsing
    pub certificate: String,
    pub salt: i64,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub public_ip: Option<String>,
}

/// Handle get_certificate request
/// This is called after Masang auth to get encryption keys for login
pub async fn get_certificate(
    State(_state): State<AppState>,
    _body: Bytes,
) -> Result<Json<GetCertificateResponse>> {
    tracing::info!("Get certificate request");
    
    // The client uses X509Certificate2(Encoding.UTF8.GetBytes(certificateString))
    // This expects DER-encoded certificate bytes, NOT PEM format.
    // We need to provide the raw Base64-encoded DER certificate (without PEM headers).
    // The certificate was generated with: openssl req -x509 -newkey rsa:4096 -keyout certs/server.key -out certs/server.crt -days 365 -nodes
    // Then extracted the Base64 content (without BEGIN/END markers) for use here.
    
    // This is the DER certificate encoded as Base64 (single line, no headers)
    let certificate = "MIIFNzCCAx+gAwIBAgIUb1Ws4oXLoq7bIQadRDQz093oQN4wDQYJKoZIhvcNAQELBQAwKzEpMCcGA1UEAwwga3ItYXBuZTEtcGF0Y2hzcmMubWFzYW5nc29mdC5jb20wHhcNMjYwMjAyMTIzNjI1WhcNMjcwMjAyMTIzNjI1WjArMSkwJwYDVQQDDCBrci1hcG5lMS1wYXRjaHNyYy5tYXNhbmdzb2Z0LmNvbTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAOqzuzNBbCbSbELs9BcrRUyJs9lyn1//IS2Sa4v2hX4UvySn8fDrT3h5JSpAV3I8Yrxo6D3J+SmxxB0JipWvZsylPyzv2vCPgqPCIDSpXQYtkw8W0ixZbS8VOmYPOYt6Z6Z5aYnTb52dhD8zIUuUxEfH1omo5jZDVEJM+sVSXUY05OEsbQsqq4AthZcYNTj0e8AFu0oqSauySCHncV2Hk+ZhGX4epDpWa3ZtpYuxokJQc/cUYt/MoYzZiwUgeafJEQyX1SD32H0hrjVmVm0lMlGZuz2TMEl66+Q24TboDzrvpOq6oB7EmUNGGCWR9aEkMFfW0tkxllgnVQOE7ZuRag6aNaRLjWKjRnjbykE5z2s6iXnPDW4ND0CxdUDQu4YCwa6eIj3k030CN95fDTiYslOJ4VHQdlCOeFWeJf50/VOlBD6F5d1ImpU/jDzTJ97OKU0QEBkCcWfQb/6Ga0PBk1Y3zQEyp3KqWNVsGL5TUQA9j7tgY9+su3ayjwBkO0/KTl7sYoPlrpC8fhzvHp0Dt0Tjlk3Ys/9fIqx5eZAdkTsKlZXL1GTZ7/wWWAdomxgiL+iS2pHlXsFZbWvZNKO5IAjh1dYa3SXmbJCoEpjbNSmyWGMnHkFcarQOqva2KY5wp52n/lIQzaTTQ+nGPi31XLB8mlXcEvwF6JpCOGifZ0cJAgMBAAGjUzBRMB0GA1UdDgQWBBTxoXkZpHqiSmxLMb4pzx94SHoYxzAfBgNVHSMEGDAWgBTxoXkZpHqiSmxLMb4pzx94SHoYxzAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4ICAQB10YKLVAhfY8c8MDE6qj/PUn8lwvJ45XYBUJL2Z/WdW/BM2nXlsFv0cHO3R1OobsO+oeZqf5GgTMlmot4RCXbDEkZL3VADM2Tx4Q34NSmSlIjvJOfIOic6VXweX1DaxKKtcmf7BXj9A8HesK+qbd21yVxENX/pD9N5vyYGa/zNgqw9dgTwoekwxflXk/euZKP9/w644PAEBosHYQyF71QvPv5rzEIv1AQ765xrbI2m1lsod89dzMhznaV+DHOTCf+5EuvvcmeSMk1BHW3cVf7r2jIUq9tf/FXMEpRaBZAYPVNP2bvIopr5rKOoIBMHUrfTs83bdsEPbPk5RVPQUu+dju1MHBYlelt/Rj1WUC58vY/NfzkQFVkmPjGb9wYEZfzCnhEJyVkdybTF+O7ORvhbQ/QRJq0nEvzGTLm6MXIFQTy1JDdd1QOpEfxHZ3A09jfvE/KVSO2JADHP2pujLEJsVNfHw8ZFzmQZUAjAz9QilUDeXKWZ3IrGZgLIjvydCGWW7KB6908vebKatXNAcSvl4tcbMvlLXC0tqAuhI6pvY0j3wga4oSbqLr0LEYox96JDvPJ5YZT3RurF9tl5twjKD86oZduBRG1AMEJS1gIIu2+LOJj0x5Qv7WSNuZwNQ60R8uCdhLbgInSNMnOsHbkDhv6sObjqNMr6JpySwkM6rw==".to_string();
    
    // Generate a random salt
    let salt: i64 = rand::random::<i64>().abs();
    
    Ok(Json(GetCertificateResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        certificate,
        salt,
        public_ip: None, // Optional, can redirect to different server
    }))
}
