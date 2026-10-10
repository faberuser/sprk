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
    // Later-client fixtures are retained only to test rejection of limit breaks.
    #[cfg(test)]
    pub limit_exp_items: Vec<Value>,
    #[cfg(test)]
    pub limit_breaks: Vec<Value>,
    pub challenges: Vec<Value>,
    pub awake: Vec<Value>,
    pub constants: HashMap<String, String>,
    pub results: HashMap<String, Vec<String>>,
}
impl HeroShopTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        data.apply_archived_cosmetic_flags();
        Ok(data)
    }
    fn apply_archived_cosmetic_flags(&mut self) {
        for costume in self.costumes.values_mut() {
            costume["Buyable"] = Value::Bool(costume["IsOpen"] == true && costume["IsBuy"] == true);
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
    fn archived_costume_flags_do_not_invent_prices_or_open_closed_sales() {
        let mut table = HeroShopTable { costumes: [
            (1, json!({"IsDefault":true,"IsOpen":true,"IsBuy":false,"ReqBuyGem":0})),
            (2, json!({"IsDefault":false,"IsOpen":false,"IsBuy":true,"ReqBuyGem":0})),
            (3, json!({"IsDefault":false,"IsOpen":true,"IsBuy":true,"ReqBuyGem":6000})),
            (4, json!({"IsDefault":false,"IsOpen":true,"IsBuy":true,"ReqBuyGem":0,"ReqBuyMileage":2500})),
        ].into(), ..Default::default() };
        table.apply_archived_cosmetic_flags();
        assert_eq!(table.costumes[&1]["Buyable"], false);
        assert_eq!(table.costumes[&1]["ReqBuyGem"], 0);
        assert_eq!(table.costumes[&2]["ReqBuyGem"], 0);
        assert_eq!(table.costumes[&2]["Buyable"], false);
        assert_eq!(table.costumes[&3]["Buyable"], true);
        assert_eq!(table.costumes[&3]["ReqBuyGem"], 6000);
        assert_eq!(table.costumes[&4]["ReqBuyGem"], 0);
        assert_eq!(table.costumes[&4]["ReqBuyMileage"], 2500);
    }
}
