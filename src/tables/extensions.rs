use serde::Deserialize;
use serde_json::Value;
use std::{collections::HashMap, path::Path};

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(transparent)]
pub struct ExtensionTable(pub HashMap<String, Vec<Value>>);
impl ExtensionTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        let local = path.with_file_name("ExtensionRules.json");
        if local.exists() {
            let rules: HashMap<String, Vec<Value>> =
                serde_json::from_reader(std::fs::File::open(local)?)?;
            for (key, rows) in rules {
                data.0.insert(key, rows);
            }
        }
        data.enable_cosmetic_sales();
        Ok(data)
    }
    fn enable_cosmetic_sales(&mut self) {
        for name in ["HairCostume", "WeaponCostume", "AccessoryCostume"] {
            for row in self.0.entry(name.into()).or_default() {
                row["IsOpen"] = Value::Bool(true);
                if name == "AccessoryCostume" {
                    row["IsBuy"] = Value::Bool(true);
                    if row["ReqBuyGem"].as_i64().unwrap_or(0) <= 0 {
                        row["ReqBuyGem"] = Value::from(500);
                    }
                } else if row["IsDefault"] != true
                    && row["NeedCostume"].as_array().is_none_or(|v| v.is_empty())
                    && row["ReqBuyGem"].as_i64().unwrap_or(0) <= 0 {
                    // Parts bundled with body costumes remain included in that purchase.
                    row["ReqBuyGem"] = Value::from(3000);
                }
            }
        }
    }
    pub fn rows(&self, name: &str) -> &[Value] {
        self.0.get(name).map(Vec::as_slice).unwrap_or(&[])
    }
    pub fn find(&self, name: &str, fields: &[(&str, i64)]) -> Option<&Value> {
        self.rows(name)
            .iter()
            .find(|r| fields.iter().all(|(k, v)| r[*k].as_i64() == Some(*v)))
    }
}

#[cfg(test)]
mod cosmetic_sale_tests {
    use super::*;
    use serde_json::json;

    #[test]
    fn accessory_sale_prices_and_bundled_parts() {
        let mut table = ExtensionTable([
            ("AccessoryCostume".into(), vec![json!({"ReqBuyGem":0}), json!({"ReqBuyGem":750})]),
            ("HairCostume".into(), vec![json!({"IsDefault":false,"ReqBuyGem":0,"NeedCostume":[114]}),
                json!({"IsDefault":false,"ReqBuyGem":0,"NeedCostume":[]})]),
            ("WeaponCostume".into(), vec![json!({"IsDefault":true,"ReqBuyGem":0})]),
        ].into());
        table.enable_cosmetic_sales();
        assert_eq!(table.rows("AccessoryCostume")[0]["ReqBuyGem"], 500);
        assert_eq!(table.rows("AccessoryCostume")[1]["ReqBuyGem"], 750);
        assert!(table.rows("AccessoryCostume").iter().all(|r| r["IsBuy"] == true && r["IsOpen"] == true));
        assert_eq!(table.rows("HairCostume")[0]["ReqBuyGem"], 0);
        assert_eq!(table.rows("HairCostume")[1]["ReqBuyGem"], 3000);
        assert_eq!(table.rows("WeaponCostume")[0]["ReqBuyGem"], 0);
    }
}
