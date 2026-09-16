use serde::Deserialize;
use serde_json::Value;
use std::{collections::HashMap, path::Path};

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct BattleTable {
    pub tables: HashMap<String, Vec<Value>>,
    pub contracts: HashMap<String, Value>,
    #[serde(default)]
    pub enums: HashMap<String, HashMap<String, i64>>,
    #[serde(default)]
    pub rules: Value,
}
impl BattleTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        Self::load_with_rules(path, "BattleRules.json")
    }
    pub fn load_with_rules(path: &Path, rules: &str) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        data.rules = serde_json::from_reader(std::fs::File::open(
            path.with_file_name(rules),
        )?)?;
        Ok(data)
    }
    pub fn rows(&self, name: &str) -> &[Value] {
        self.tables.get(name).map(Vec::as_slice).unwrap_or(&[])
    }
    pub fn find(&self, name: &str, fields: &[(&str, i64)]) -> Option<&Value> {
        self.rows(name)
            .iter()
            .find(|r| fields.iter().all(|(k, v)| r[*k].as_i64() == Some(*v)))
    }
}
