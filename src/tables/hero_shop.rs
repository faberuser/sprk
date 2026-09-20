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
        let mut data: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        data.enable_cosmetic_sales();
        Ok(data)
    }
    fn enable_cosmetic_sales(&mut self) {
        for costume in self.costumes.values_mut() {
            if costume["IsDefault"] == true { continue; }
            costume["Buyable"] = Value::Bool(true);
            if ["ReqBuyGem", "ReqBuyGold", "ReqBuyMileage"].iter()
                .all(|key| costume[*key].as_i64().unwrap_or(0) <= 0) {
                costume["ReqBuyGem"] = Value::from(3000);
            }
        }
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

#[cfg(test)]
mod cosmetic_sale_tests {
    use super::*;
    use serde_json::json;

    #[test]
    fn costume_sale_prices_preserve_defaults_and_existing_currencies() {
        let mut table = HeroShopTable::default();
        table.costumes = [
            (1, json!({"IsDefault":true,"ReqBuyGem":0,"Buyable":false})),
            (2, json!({"IsDefault":false,"CostumeType":2,"ReqBuyGem":0})),
            (3, json!({"IsDefault":false,"ReqBuyGem":6000})),
            (4, json!({"IsDefault":false,"ReqBuyGem":0,"ReqBuyMileage":2500})),
        ].into();
        table.enable_cosmetic_sales();
        assert_eq!(table.costumes[&1]["Buyable"], false);
        assert_eq!(table.costumes[&1]["ReqBuyGem"], 0);
        assert_eq!(table.costumes[&2]["ReqBuyGem"], 3000);
        assert_eq!(table.costumes[&2]["Buyable"], true);
        assert_eq!(table.costumes[&3]["ReqBuyGem"], 6000);
        assert_eq!(table.costumes[&4]["ReqBuyGem"], 0);
        assert_eq!(table.costumes[&4]["ReqBuyMileage"], 2500);
    }
}
