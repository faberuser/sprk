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
    #[serde(default)]
    pub booster_definitions: HashMap<String, Value>,
    pub crafts: HashMap<i32, Value>,
    pub break_rewards: HashMap<i32, i32>,
    pub extensions: Vec<Value>,
    pub craft_instant_prices: Vec<Value>,
    pub team_levels: Vec<Value>,
    pub constants: HashMap<String, i64>,
}
impl InventoryTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut table: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        let crafting = path.with_file_name("RestoredCrafting.json");
        if crafting.exists() {
            let rows: Vec<Value> = serde_json::from_reader(std::fs::File::open(crafting)?)?;
            for row in rows {
                let id = row["CraftIndex"].as_i64().and_then(|id| i32::try_from(id).ok()).ok_or("Invalid restored craft ID")?;
                let item = row["ItemIndex"].as_i64().and_then(|id| i32::try_from(id).ok()).ok_or("Invalid restored craft output")?;
                if !table.items.contains_key(&item) { return Err("Missing restored craft output item".into()); }
                for material in row["Materials"].as_array().ok_or("Invalid restored craft materials")? {
                    let item = material["ItemIndex"].as_i64().and_then(|id| i32::try_from(id).ok()).ok_or("Invalid craft material")?;
                    if !table.items.contains_key(&item) { return Err("Missing restored craft material item".into()); }
                }
                table.crafts.insert(id, row);
            }
        }
        let overrides = path.with_file_name("RestoredSelectors.json");
        if overrides.exists() {
            let rows: Vec<Value> = serde_json::from_reader(std::fs::File::open(overrides)?)?;
            for row in rows {
                let map = match row["Family"].as_str() {
                    Some("EquipmentSelectors") => &mut table.equipment_selectors,
                    Some("Selectors") => &mut table.selectors,
                    Some("HeroSelectors") => &mut table.hero_selectors,
                    _ => return Err("Unknown restored selector family".into()),
                };
                let id = row["ItemIndex"].as_i64().ok_or("Invalid selector ID")? as i32;
                let entry = map.get_mut(&id).ok_or("Restored selector is missing")?;
                let field = row["Field"].as_str().ok_or("Missing selector field")?;
                entry[field] = row["Choices"].clone();
            }
        }
        Ok(table)
    }
    pub fn constant(&self, name: &str, fallback: i64) -> i64 {
        self.constants.get(name).copied().unwrap_or(fallback)
    }
}
