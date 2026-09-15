use serde::Deserialize;
use serde_json::Value;
use std::{collections::HashMap, path::Path};

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct HeroShopTable {
    pub items: HashMap<i32, Value>,
    pub heroes: HashMap<i32, Value>,
    pub costumes: HashMap<i32, Value>,
    pub preset_slots: HashMap<String, Value>,
    pub growth_items: Vec<Value>,
    pub multi_hero_items: Vec<Value>,
    pub costume_selectors: Vec<Value>,
    pub costume_groups: HashMap<String, String>,
    pub prices: Vec<Value>,
    pub stars: Vec<Value>,
    pub equip_star_prices: Vec<Value>,
    pub hero_bonuses: Vec<Value>,
    pub skill_prices: Vec<Value>,
    pub shops: HashMap<i32, Value>,
    pub shop_items: Vec<Value>,
    pub books: Vec<Value>,
    pub limit_exp_items: Vec<Value>,
    pub challenges: Vec<Value>,
    pub awake: Vec<Value>,
    pub limit_breaks: Vec<Value>,
    pub constants: HashMap<String, String>,
    pub results: HashMap<String, Vec<String>>,
}
impl HeroShopTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        Ok(serde_json::from_reader(std::fs::File::open(path)?)?)
    }
    pub fn constant(&self, key: &str, default: i64) -> i64 {
        self.constants
            .get(key)
            .and_then(|v| v.parse().ok())
            .unwrap_or(default)
    }
    pub fn result<'a>(&self, action: &str, code: &'a str) -> &'a str {
        if self
            .results
            .get(action)
            .is_some_and(|v| v.iter().any(|s| s == code))
        {
            code
        } else {
            "Fail"
        }
    }
}
