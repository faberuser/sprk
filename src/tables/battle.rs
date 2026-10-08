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
        data.rules = serde_json::from_reader(std::fs::File::open(path.with_file_name(rules))?)?;
        if rules == "ArenaGuildRules.json" {
            let conquest = path.with_file_name("RestoredGuildConquest.json");
            if conquest.exists() {
                let restored: HashMap<String, Vec<Value>> = serde_json::from_reader(std::fs::File::open(conquest)?)?;
                data.tables.extend(restored);
            }
            let guild = path.with_file_name("RestoredGuildContent.json");
            if guild.exists() {
                let restored: Value = serde_json::from_reader(std::fs::File::open(guild)?)?;
                data.tables.insert("GuildRaidBoss".into(), restored["GuildRaidBoss"].as_array().unwrap().clone());
            }
        }
        if rules == "BattleRules.json" {
            let overlay = path.with_file_name("ReconstructedRaids.json");
            if overlay.exists() {
                let rows: Vec<Value> = serde_json::from_reader(std::fs::File::open(overlay)?)?;
                let raids = data.tables.entry("Raid".into()).or_default();
                for row in rows {
                    // These entries translate archived content into Windows
                    // navigation IDs. Raw archive rows may use the same IDs for
                    // another chapter; the translation must take precedence.
                    raids.retain(|v| !(v["Index"] == row["Index"] && v["Level"] == row["Level"]));
                    raids.push(row);
                }
            }
        }
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
