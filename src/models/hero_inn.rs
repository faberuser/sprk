use serde::{Deserialize, Serialize};
use crate::models::hero::HeroInfo;

/// Friendly action types matching the client's FriendlyActionType enum
#[derive(Debug, Clone, Copy, Serialize, Deserialize, Default, PartialEq)]
#[repr(i32)]
pub enum FriendlyActionType {
    #[default]
    None = 0,
    Greeting = 1,
    Conversation = 2,
    Gift = 3,
    ClosenessGreeting = 4,
    ClosenessConversation = 5,
    ClosenessGift = 6,
    ConnectTalk1 = 7,
    ConnectTalk2 = 8,
    ConnectTalk3 = 9,
    GreetingMax = 10,
    GiftItem = 11,
    Iii = 12,
    RewardNPCFriendly = 13,
}

impl From<i32> for FriendlyActionType {
    fn from(val: i32) -> Self {
        match val {
            0 => FriendlyActionType::None,
            1 => FriendlyActionType::Greeting,
            2 => FriendlyActionType::Conversation,
            3 => FriendlyActionType::Gift,
            4 => FriendlyActionType::ClosenessGreeting,
            5 => FriendlyActionType::ClosenessConversation,
            6 => FriendlyActionType::ClosenessGift,
            7 => FriendlyActionType::ConnectTalk1,
            8 => FriendlyActionType::ConnectTalk2,
            9 => FriendlyActionType::ConnectTalk3,
            10 => FriendlyActionType::GreetingMax,
            11 => FriendlyActionType::GiftItem,
            12 => FriendlyActionType::Iii,
            13 => FriendlyActionType::RewardNPCFriendly,
            _ => FriendlyActionType::None,
        }
    }
}

/// Player hero friendly info - the main Hero Inn state
/// Matches the client's PlayerHeroFriendlyInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
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
    // Always serialize this field - the client needs it for the 3 hero portraits
    #[serde(default)]
    pub selected_hero_indice: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub last_roulette_time: Option<String>,
}

/// Hero add result info - returned when recruiting a hero
/// Matches the client's HeroAddResultInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct HeroAddResultInfo {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hero_info: Option<HeroInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub team_exp_result: Option<ExpResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result: Option<StaminaResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hero_friendly_info: Option<PlayerHeroFriendlyInfo>,
}

/// Experience result info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ExpResultInfo {
    pub before_level: i32,
    pub after_level: i32,
    pub before_exp: i64,
    pub after_exp: i64,
    pub exp_change: i64,
}

/// Stamina result info
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct StaminaResultInfo {
    pub stamina_type: i32,
    pub before_value: i32,
    pub after_value: i32,
    pub change_value: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub recharge_start_time: Option<String>,
}

/// Currency result info (version 3 with additional fields)
/// Client expects: CurrencyType (enum string), AddValue, NewValue
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo3 {
    pub currency_type: String,  // "Gold", "Ruby", etc.
    pub add_value: i64,         // Change amount (negative for deduction)
    pub new_value: i64,         // New total value
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

/// Friendship point result info
/// Client expects: AddValue, AddDailyAccValue, NewValue, NewDailyAccValue
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct FriendshipPointResultInfo {
    pub add_value: i64,           // Change amount (negative for deduction)
    pub add_daily_acc_value: i32, // Usually 0
    pub new_value: i64,           // New total value
    pub new_daily_acc_value: i32, // Usually 0
}

/// Item result info - matches client's NShared.ItemResultInfo
/// Client expects: ItemIndex, AddCount, NewCount
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct ItemResultInfo {
    pub item_index: i32,
    pub add_count: i32,
    pub new_count: i32,
    #[serde(default)]
    pub add_booster_count: i32,
    #[serde(default, rename = "AddNPCBoosterCount")]
    pub add_npc_booster_count: i32,
    #[serde(default)]
    pub add_bonus_assigned_item_percent: i32,
    #[serde(default)]
    pub locked: u8,
    #[serde(default)]
    pub is_first_clear_reward: bool,
}

/// Default available hero indices for the Hero Inn
/// These are heroes that can appear in the inn for recruitment
pub fn get_available_inn_hero_indices() -> Vec<i32> {
    // Common heroes available in the inn (excluding starter heroes 1-4)
    // This includes various 2-star and 3-star heroes
    vec![
        5,   // Clause - Knight
        6,   // Gau - Warrior
        7,   // Naila - Assassin
        8,   // Lakrak - Mechanic
        9,   // Miruru - Mechanic
        10,  // Dimael - Archer
        11,  // Selene - Archer
        12,  // Rephy - Priest
        13,  // Baudouin - Priest
        14,  // Leo - Priest
        15,  // Maria - Wizard
        16,  // Lorraine - Wizard
        17,  // Pavel - Wizard
        18,  // Morrah - Knight
        19,  // Jane - Knight
        20,  // Epis - Assassin
        21,  // Fluss - Assassin
        22,  // Rodina - Mechanic
        23,  // Luna - Archer
        24,  // Arch - Archer
        25,  // Laias - Priest
        26,  // Kaulah - Priest
        27,  // Nyx - Wizard
        28,  // Aisha - Wizard
        29,  // Phillop - Knight
        30,  // Ricardo - Knight
        31,  // Reina - Assassin
        32,  // Tanya - Assassin
        33,  // Annette - Mechanic
        34,  // Mitra - Mechanic
        35,  // Yanne - Archer
        36,  // Priscilla - Warrior
        37,  // Theo - Warrior
        38,  // Artemia - Wizard
        39,  // Sonia - Knight
        40,  // Demia - Knight
        // Add more hero indices as needed
    ]
}

/// Max friendship points needed to recruit a hero
pub const MAX_FRIENDSHIP_POINTS: i32 = 1000;

/// Points gained per action
pub const GREETING_POINTS: i32 = 50;
pub const CONVERSATION_POINTS: i32 = 100;
pub const GIFT_POINTS: i32 = 200;

/// Closeness action points (when hero is already owned)
#[allow(dead_code)]
pub const CLOSENESS_GREETING_POINTS: i32 = 25;
#[allow(dead_code)]
pub const CLOSENESS_CONVERSATION_POINTS: i32 = 50;
#[allow(dead_code)]
pub const CLOSENESS_GIFT_POINTS: i32 = 100;

// ============================================================================
// Hero Inn Roulette Structures
// ============================================================================

/// Roulette info - current state of the roulette
/// Matches the client's RouletteInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct RouletteInfo {
    pub roulette_info_index: i32,
    pub remain_count: i32,
    pub max_count: i32,
}

/// Roulette reward info - defines a reward section on the wheel
/// Matches the client's RouletteRewardInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct RouletteRewardInfo {
    pub index: i32,
    pub section: i32,  // 0-7 for 8 sections on the wheel
    pub type1: String, // RewardType as string
    pub value1: i32,   // CurrencyType or ItemCode
    pub count1: i32,   // Amount
    #[serde(skip_serializing_if = "Option::is_none")]
    pub type2: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value2: Option<i32>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub count2: Option<i32>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub type3: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub value3: Option<i32>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub count3: Option<i32>,
}

/// Default roulette rewards for the Hero Inn
/// Creates 8 sections with various rewards (sections are 1-based: 1-8)
/// RewardType enum values: None=0, Item=1, Gold=2, Gem=3, FriendshipPoint=14
pub fn get_default_roulette_rewards(roulette_index: i32) -> Vec<RouletteRewardInfo> {
    vec![
        RouletteRewardInfo {
            index: roulette_index * 100 + 1,
            section: 1,  // 1-based
            type1: "Gold".to_string(),  // RewardType.Gold
            value1: 50000,              // Amount
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 2,
            section: 2,
            type1: "FriendshipPoint".to_string(),  // RewardType.FriendshipPoint
            value1: 100,                           // Amount
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 3,
            section: 3,
            type1: "Gold".to_string(),
            value1: 100000,
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 4,
            section: 4,
            type1: "FriendshipPoint".to_string(),
            value1: 150,
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 5,
            section: 5,
            type1: "Gem".to_string(),  // RewardType.Gem = Ruby
            value1: 50,
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 6,
            section: 6,
            type1: "FriendshipPoint".to_string(),
            value1: 200,
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 7,
            section: 7,
            type1: "Gold".to_string(),
            value1: 200000,
            count1: 1,
            ..Default::default()
        },
        RouletteRewardInfo {
            index: roulette_index * 100 + 8,
            section: 8,
            type1: "Gem".to_string(),
            value1: 100,
            count1: 1,
            ..Default::default()
        },
    ]
}