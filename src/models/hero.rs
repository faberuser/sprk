use serde::{Deserialize, Serialize};

/// Hero info matching the client's HeroInfo class
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct HeroInfo {
    pub hero_id: i64,
    pub hero_index: i32,
    pub star: i32,
    pub level: i32,
    pub exp: i32,
    pub transcend: i32,
    pub awakened: i32,
    pub skill_level_1: i32,
    pub skill_level_2: i32,
    pub skill_level_3: i32,
    pub skill_level_4: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub unique_weapon_id: Option<i64>,
    pub is_bookmarked: bool,
    pub closeness: i32,
    pub transcend_skill_point: i32,
    
    // Equipment slots (1-10, 0 means empty)
    // Slot 1: Weapon, Slot 2: Armor, Slot 3: Secondary, Slot 4: Accessory, Slot 5: Orb
    // Slots 6-10: Extended slots
    #[serde(default)]
    pub equip_item_slot_index_1: i32,
    #[serde(default)]
    pub equip_item_slot_index_2: i32,
    #[serde(default)]
    pub equip_item_slot_index_3: i32,
    #[serde(default)]
    pub equip_item_slot_index_4: i32,
    #[serde(default)]
    pub equip_item_slot_index_5: i32,
    #[serde(default)]
    pub equip_item_slot_index_6: i32,
    #[serde(default)]
    pub equip_item_slot_index_7: i32,
    #[serde(default)]
    pub equip_item_slot_index_8: i32,
    #[serde(default)]
    pub equip_item_slot_index_9: i32,
    #[serde(default)]
    pub equip_item_slot_index_10: i32,
}

impl HeroInfo {
    /// Create a new hero with starting values
    pub fn new(hero_id: i64, hero_index: i32, star: i32) -> Self {
        Self {
            hero_id,
            hero_index,
            star,
            level: 1,
            exp: 0,
            transcend: 0,
            awakened: 0,
            skill_level_1: 1,
            skill_level_2: 1,
            skill_level_3: 1,
            skill_level_4: 1,
            unique_weapon_id: None,
            is_bookmarked: false,
            closeness: 0,
            transcend_skill_point: 0,
            equip_item_slot_index_1: 0,
            equip_item_slot_index_2: 0,
            equip_item_slot_index_3: 0,
            equip_item_slot_index_4: 0,
            equip_item_slot_index_5: 0,
            equip_item_slot_index_6: 0,
            equip_item_slot_index_7: 0,
            equip_item_slot_index_8: 0,
            equip_item_slot_index_9: 0,
            equip_item_slot_index_10: 0,
        }
    }
}

/// Default starting heroes (Kasel, Frey, Cleo, Roi)
pub fn get_starting_heroes(base_hero_id: i64) -> Vec<HeroInfo> {
    vec![
        HeroInfo::new(base_hero_id, 1, 2),      // Kasel - Knight
        HeroInfo::new(base_hero_id + 1, 2, 2),  // Frey - Priest  
        HeroInfo::new(base_hero_id + 2, 3, 2),  // Cleo - Wizard
        HeroInfo::new(base_hero_id + 3, 4, 2),  // Roi - Assassin
    ]
}
