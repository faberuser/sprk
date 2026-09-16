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
