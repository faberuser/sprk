use sqlx::Row;
use serde::{Deserialize, Serialize};

/// Equipment item info matching client's EquipItemInfo
/// Based on JM_NShared_EquipItemInfo.cs parsing
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct EquipItemInfo {
    #[serde(default, skip_serializing_if="Option::is_none")]
    pub punishment_rune_option: Option<serde_json::Value>,
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
    /// Build the same equipment snapshot for login and tutorial retries.
    pub fn from_row(row: &sqlx::sqlite::SqliteRow) -> Self {
        let slot_index: i32 = row.get("slot_index");
        let mut info = EquipItemInfo {
            slot_index,
            item_index: row.get("item_index"),
            star: row.get("star"),
            level: row.get("level"),
            exp: row.get("exp"),
            option_index_1: row.get("option_index_1"),
            option_step_1: row.get("option_step_1"),
            option_renew_count_1: row.get("option_renew_count_1"),
            is_renewed_option_1: row.get("is_renewed_option_1"),
            option_index_2: row.get("option_index_2"),
            option_step_2: row.get("option_step_2"),
            option_renew_count_2: row.get("option_renew_count_2"),
            is_renewed_option_2: row.get("is_renewed_option_2"),
            option_index_3: row.get("option_index_3"),
            option_step_3: row.get("option_step_3"),
            option_renew_count_3: row.get("option_renew_count_3"),
            is_renewed_option_3: row.get("is_renewed_option_3"),
            option_index_4: row.get("option_index_4"),
            option_step_4: row.get("option_step_4"),
            option_renew_count_4: row.get("option_renew_count_4"),
            is_renewed_option_4: row.get("is_renewed_option_4"),
            rune_slot_count: row.get("rune_slot_count"),
            rune_item_index_1: row.get("rune_item_index_1"),
            rune_item_index_2: row.get("rune_item_index_2"),
            rune_item_index_3: row.get("rune_item_index_3"),
            created_time: row.get("created_time"),
            upgrade_star_fail_bonus: row.get("upgrade_star_fail_bonus"),
            locked: row.get("locked"),
            inventory_type: row.get("inventory_type"),
            identified: row.get("identified"),
            uid: format!("{}", slot_index),
            ..Default::default()
        };
        // Read every persisted extension field, including enchantments and extra options.
        let mut value = serde_json::to_value(&info).expect("equipment serializes");
        if let Ok(Some(raw))=row.try_get::<Option<String>,_>("punishment_rune_option") {
            value["PunishmentRuneOption"]=serde_json::from_str(&raw).expect("stored rune option JSON");
        }
        for (key, field) in value.as_object_mut().unwrap() {
            if field.is_number() {
                if let Ok(number) = row.try_get::<i64,_>(Self::column(key).as_str()) {
                    *field = serde_json::json!(number);
                }
            }
        }
        info = serde_json::from_value(value).expect("equipment columns match native types");
        info
    }

    pub(crate) fn column(wire: &str) -> String {
        let mut out = String::new();
        for (i, c) in wire.chars().enumerate() {
            if i > 0 && (c.is_ascii_uppercase() || c.is_ascii_digit()) { out.push('_'); }
            out.push(c.to_ascii_lowercase());
        }
        out
    }

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
