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
        Ok(data)
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
    fn archived_cosmetic_flags_and_prices_are_preserved_on_load() {
        let directory=std::env::temp_dir().join(format!("sprk-archive-cosmetics-{}",uuid::Uuid::new_v4()));
        std::fs::create_dir(&directory).unwrap();
        let rows=json!({"AccessoryCostume":[{"Index":1,"ReqBuyGem":0,"IsBuy":false,"IsOpen":false}],
            "HairCostume":[{"Index":2,"ReqBuyGem":0,"NeedCostume":[],"IsOpen":false}]});
        let file=directory.join("ExtensionSupport.json");std::fs::write(&file,rows.to_string()).unwrap();
        let table=ExtensionTable::load(&file).unwrap();
        assert_eq!(table.rows("AccessoryCostume")[0],rows["AccessoryCostume"][0]);
        assert_eq!(table.rows("HairCostume")[0],rows["HairCostume"][0]);
        std::fs::remove_file(file).unwrap();std::fs::remove_dir(directory).unwrap();
    }
}
