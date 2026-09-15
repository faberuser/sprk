use serde::Deserialize;
use serde_json::Value;
use std::{collections::HashMap, path::Path};

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct InventoryTable {
    pub rune_breaks: HashMap<i32, Vec<Value>>,
    pub equipment: HashMap<i32, Value>,
    pub option_groups: HashMap<i32, Vec<i32>>,
    pub options: HashMap<i32, Value>,
    pub custom_equipment: HashMap<i32, crate::models::equip::EquipItemInfo>,
    pub items: HashMap<i32, Value>,
    pub potions: HashMap<i32, Value>,
    pub packages: HashMap<i32, Value>,
    pub selectors: HashMap<i32, Value>,
    pub equipment_selectors: HashMap<i32, Value>,
    pub hero_selectors: HashMap<i32, Value>,
    pub boosters: HashMap<i32, Value>,
    pub crafts: HashMap<i32, Value>,
    pub break_rewards: HashMap<i32, i32>,
    pub extensions: Vec<Value>,
    pub craft_instant_prices: Vec<Value>,
    pub team_levels: Vec<Value>,
    pub constants: HashMap<String, i64>,
}
impl InventoryTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        Ok(serde_json::from_reader(std::fs::File::open(path)?)?)
    }
    pub fn constant(&self, name: &str, fallback: i64) -> i64 {
        self.constants.get(name).copied().unwrap_or(fallback)
    }
}
