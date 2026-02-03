use serde::{Deserialize, Serialize};

/// Equipment item info matching client's EquipItemInfo
/// Based on JM_NShared_EquipItemInfo.cs parsing
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct EquipItemInfo {
    /// Slot index - unique identifier for the equipment
    pub slot_index: i32,
    /// Item index from ItemTable
    pub item_index: i32,
    /// Star level (0-5)
    pub star: i32,
    /// Enhancement level
    pub level: i32,
    /// Experience points
    #[serde(default)]
    pub exp: i32,
    
    // Option 1
    #[serde(default)]
    pub option_index_1: i32,
    #[serde(default)]
    pub option_step_1: i32,
    #[serde(default)]
    pub option_renew_count_1: i32,
    #[serde(default)]
    pub is_renewed_option_1: i32,
    
    // Option 2
    #[serde(default)]
    pub option_index_2: i32,
    #[serde(default)]
    pub option_step_2: i32,
    #[serde(default)]
    pub option_renew_count_2: i32,
    #[serde(default)]
    pub is_renewed_option_2: i32,
    
    // Option 3
    #[serde(default)]
    pub option_index_3: i32,
    #[serde(default)]
    pub option_step_3: i32,
    #[serde(default)]
    pub option_renew_count_3: i32,
    #[serde(default)]
    pub is_renewed_option_3: i32,
    
    // Option 4
    #[serde(default)]
    pub option_index_4: i32,
    #[serde(default)]
    pub option_step_4: i32,
    #[serde(default)]
    pub option_renew_count_4: i32,
    #[serde(default)]
    pub is_renewed_option_4: i32,
    
    // Option 5 (for unique gear)
    #[serde(default)]
    pub option_index_5: i32,
    #[serde(default)]
    pub option_step_5: i32,
    #[serde(default)]
    pub option_renew_count_5: i32,
    #[serde(default)]
    pub is_renewed_option_5: i32,
    
    /// Number of rune slots unlocked
    #[serde(default)]
    pub rune_slot_count: i32,
    /// Rune item indices
    #[serde(default)]
    pub rune_item_index_1: i32,
    #[serde(default)]
    pub rune_item_index_2: i32,
    #[serde(default)]
    pub rune_item_index_3: i32,
    
    /// Creation time
    #[serde(default)]
    pub created_time: String,
    /// Bonus from failed star upgrades
    #[serde(default)]
    pub upgrade_star_fail_bonus: i32,
    /// Unique ID string
    #[serde(default, rename = "Uid")]
    pub uid: String,
    /// Locked status (0 = unlocked, 1 = locked)
    #[serde(default)]
    pub locked: u8,
    
    // Enchant options
    #[serde(default)]
    pub enchant_option_index_1: i32,
    #[serde(default)]
    pub enchant_option_step_1: i32,
    #[serde(default)]
    pub enchant_option_index_2: i32,
    #[serde(default)]
    pub enchant_option_step_2: i32,
    #[serde(default)]
    pub enchant_option_index_3: i32,
    #[serde(default)]
    pub enchant_option_step_3: i32,
    #[serde(default)]
    pub renew_enchant_option_slot_index: u8,
    
    /// Inventory type (0 = normal, 1 = storage)
    #[serde(default)]
    pub inventory_type: i32,
    /// Applied rune page
    #[serde(default)]
    pub apply_rune_page: i32,
    /// Whether the item has been identified
    #[serde(default)]
    pub identified: i32,
    
    // Extra options (for special gear)
    #[serde(default)]
    pub extra_option_index_1: i32,
    #[serde(default)]
    pub extra_option_step_1: i32,
    #[serde(default)]
    pub extra_option_renew_count_1: i32,
    #[serde(default)]
    pub is_renewed_extra_option_1: i32,
    #[serde(default)]
    pub extra_option_index_2: i32,
    #[serde(default)]
    pub extra_option_step_2: i32,
    #[serde(default)]
    pub extra_option_renew_count_2: i32,
    #[serde(default)]
    pub is_renewed_extra_option_2: i32,
}

impl EquipItemInfo {
    /// Create a new equipment item with default values
    pub fn new(slot_index: i32, item_index: i32, star: i32, created_time: String) -> Self {
        Self {
            slot_index,
            item_index,
            star,
            level: 0,
            exp: 0,
            created_time,
            uid: format!("{}", slot_index),
            identified: 1, // Default to identified
            ..Default::default()
        }
    }
}
