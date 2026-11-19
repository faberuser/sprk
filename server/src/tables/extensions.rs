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
        let mut rows=json!({"AccessoryCostume":[{"Index":1,"ReqBuyGem":0,"IsBuy":false,"IsOpen":false}],
            "HairCostume":[{"Index":2,"ReqBuyGem":0,"NeedCostume":[],"IsOpen":false}]});
        for part in 1..=6 {
            for price in [10000, 20000] {
                rows["AccessoryCostume"].as_array_mut().unwrap().push(json!({
                    "Index":part * 100000 + price,"PartType":part,"ReqBuyGem":price,
                    "IsBuy":false,"PreviewableWhenOwned":true,"IsOpen":false
                }));
            }
        }
        let file=directory.join("ExtensionSupport.json");std::fs::write(&file,rows.to_string()).unwrap();
        let table=ExtensionTable::load(&file).unwrap();
        assert_eq!(table.rows("AccessoryCostume"),rows["AccessoryCostume"].as_array().unwrap().as_slice());
        assert_eq!(table.rows("HairCostume")[0],rows["HairCostume"][0]);
        std::fs::remove_file(file).unwrap();std::fs::remove_dir(directory).unwrap();
    }

    // Hairstyles the client adds (so a costume's hair can be worn with other
    // costumes) must exist on the server with the same unlock rules, or
    // equipping them fails with a missing-row error.
    #[test]
    fn client_hairstyle_patches_are_mirrored_by_the_server_table() {
        let root=std::path::Path::new(env!("CARGO_MANIFEST_DIR"));
        let table=ExtensionTable::load(&root.join("tables/ExtensionSupport.json")).unwrap();
        let server=table.rows("HairCostume");
        let mut checked=0;
        for entry in std::fs::read_dir(root.join("../client/tables/costumes")).unwrap() {
            let patch:serde_json::Value=serde_json::from_str(&std::fs::read_to_string(entry.unwrap().path()).unwrap()).unwrap();
            if patch["table"]!="HairCostumeTable" { continue; }
            for row in patch["upsert"].as_array().unwrap() {
                let found=server.iter().find(|r| r["Index"]==row["Index"])
                    .unwrap_or_else(|| panic!("server HairCostume row {} is missing",row["Index"]));
                assert_eq!(found["HeroIndex"],row["HeroIndex"],"{}",row["Index"]);
                assert_eq!(found["NeedCostume"],row["NeedCostume"],"{}",row["Index"]);
                assert_eq!(found["Star"].as_i64().unwrap_or(0),row["Star"].as_i64().unwrap_or(0),"{}",row["Index"]);
                assert_eq!(found["IsDefault"]==true,row["IsDefault"]==true,"{}",row["Index"]);
                assert_eq!(found["ReqBuyGem"].as_i64().unwrap_or(0),row["ReqBuyGem"].as_i64().unwrap_or(0),"{}",row["Index"]);
                checked+=1;
            }
        }
        assert!(checked>500,"expected the shared costume hairstyles, checked {checked}");
    }
}
