use serde::{Deserialize, Serialize};

/// User info matching the client's UserInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct UserInfo {
    pub session_key: String,
    pub account_id: i64,
    pub nick: String,
    pub gold: i64,
    pub gem: i32,
    pub pay_gem: i32,
    pub pvp_coin: i32,
    pub stamina: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_recharge_time: Option<String>,
    pub team_level: i32,
    pub team_exp: i32,
    pub sword: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub sword_recharge_time: Option<String>,
    pub sword_recharge_count: i32,
    pub sword2: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub sword2_recharge_time: Option<String>,
    pub avatar_hero_index: i32,
    pub royal_point: i32,
    pub underground_prison_key: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub underground_prison_key_recharge_time: Option<String>,
    pub underground_prison_key_recharge_count: i32,
    pub underground_labyrinth_key: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub underground_labyrinth_key_recharge_time: Option<String>,
    pub mileage: i32,
    pub friendship_point: i32,
    pub hideout_key: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hideout_key_recharge_time: Option<String>,
    pub hideout_key_recharge_count: i32,
    pub challenge_tower_key: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub challenge_tower_key_recharge_time: Option<String>,
    pub challenge_tower_key_recharge_count: i32,
    pub last_chapter_index: i32,
    pub last_dungeon_index: i32,
    pub raid_point: i64,
    pub event_dungeon_point: i64,
    pub guild_raid_ticket: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub guild_raid_ticket_recharge_time: Option<String>,
    pub world_boss_ticket: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub world_boss_ticket_recharge_time: Option<String>,
    pub glitter_point: i32,
    pub guild_point: i32,
    pub lua_point: i64,
    pub karma_point: i32,
    pub ordeal_point: i32,
}

impl UserInfo {
    /// Create a new user with default values
    pub fn new_user(account_id: i64, nick: &str, session_key: &str) -> Self {
        Self {
            session_key: session_key.to_string(),
            account_id,
            nick: nick.to_string(),
            gold: 999_999_999,
            gem: 999_999,
            pay_gem: 0,
            pvp_coin: 0,
            stamina: 999_999,
            stamina_recharge_time: None,
            team_level: 1,
            team_exp: 0,
            sword: 5,
            sword_recharge_time: None,
            sword_recharge_count: 0,
            sword2: 5,
            sword2_recharge_time: None,
            avatar_hero_index: 1,
            royal_point: 0,
            underground_prison_key: 5,
            underground_prison_key_recharge_time: None,
            underground_prison_key_recharge_count: 0,
            underground_labyrinth_key: 5,
            underground_labyrinth_key_recharge_time: None,
            mileage: 0,
            friendship_point: 0,
            hideout_key: 5,
            hideout_key_recharge_time: None,
            hideout_key_recharge_count: 0,
            challenge_tower_key: 5,
            challenge_tower_key_recharge_time: None,
            challenge_tower_key_recharge_count: 0,
            last_chapter_index: 1,
            last_dungeon_index: 1,
            raid_point: 0,
            event_dungeon_point: 0,
            guild_raid_ticket: 3,
            guild_raid_ticket_recharge_time: None,
            world_boss_ticket: 3,
            world_boss_ticket_recharge_time: None,
            glitter_point: 0,
            guild_point: 0,
            lua_point: 0,
            karma_point: 0,
            ordeal_point: 0,
        }
    }
}

/// Player misc info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct PlayerMiscInfo {
    pub login_daily_count: i64,
    pub inventory_extend: i64,
    pub chest_extend: i64,
    pub daily_acc_friendship_point: i64,
    pub daily_acc_friendship_point_reset_time: String,
    pub nick_change_count: i32,
    pub inventory_expand_count: i32,
    pub last_login_time: String,
    pub total_play_time: i64,
    pub current_chapter_index: i32,
    pub current_dungeon_index: i32,
}

/// Player battle info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct PlayerBattleInfo {
    pub tier: i32,
    pub score: i32,
    pub rank: i32,
    pub win_count: i32,
    pub lose_count: i32,
}
