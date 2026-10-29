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
    pub fn load_arena(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data = Self::load_with_rules(path, "ArenaGuildRules.json")?;
        let recovered: Value = serde_json::from_reader(std::fs::File::open(path.with_file_name("HonorRewards.json"))?)?;
        for name in ["HonorTierReward", "HonorRewardBundle", "HonorSeasonBooster"] {
            let rows = recovered["Tables"][name].as_array().ok_or_else(|| format!("Missing Honor reward table: {name}"))?;
            data.tables.insert(name.into(), rows.clone());
        }
        Ok(data)
    }
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data = Self::load_with_rules(path, "BattleRules.json")?;
        // Native MazeClearRewardTable: cumulative floors across all seven
        // Royal Labyrinth towers, rather than independent tower requirements.
        let maze = serde_json::from_reader(std::fs::File::open(path.with_file_name("MazeClearRewardTable.json"))?)?;
        data.tables.insert("MazeClearReward".into(), maze);
        let missions: Value = serde_json::from_reader(std::fs::File::open(path.with_file_name("MissionCompletionSupport.json"))?)?;
        for name in ["MonthlyHero", "SubStoryUnlock", "MonsterArchive"] {
            data.tables.insert(name.into(), missions["Tables"][name].as_array()
                .ok_or("Missing mission completion support table")?.clone());
        }
        let challenge: Value = serde_json::from_reader(std::fs::File::open(path.with_file_name("ChallengeRaidSupport.json"))?)?;
        data.tables.insert("ChallengeAffix".into(), challenge["Tables"]["ChallengeAffix"].as_array()
            .ok_or("Missing recovered Challenge Raid affixes")?.clone());
        let recovered: Value = serde_json::from_reader(std::fs::File::open(
            path.with_file_name("DivinePunishmentSupport.json"),
        )?)?;
        for name in ["Infinity", "InfinityReward", "InfinityRewardGroup", "ShardItem", "KarmaReward"] {
            let rows = recovered["Tables"][name].as_array()
                .ok_or_else(|| format!("Missing Divine Punishment table: {name}"))?;
            data.tables.insert(name.into(), rows.clone());
        }
        let world: Value = serde_json::from_reader(std::fs::File::open(path.with_file_name("WorldBossSupport.json"))?)?;
        for name in ["WorldBossGlobalReward", "WorldBossServerReward", "WorldBossDailyReward", "WorldBossDailyAchievement"] {
            let rows = world["Tables"][name].as_array().ok_or_else(|| format!("Missing recovered World Boss table: {name}"))?;
            data.tables.insert(name.into(), rows.clone());
        }
        // SPRK party policy: two players can fill the eight-hero Challenge
        // Raid roster. Match the client's RaidCoopLimits native-table bake.
        for raid in data.tables.get_mut("Raid").into_iter().flatten() {
            let index = raid["Index"].as_i64().unwrap_or(0);
            if (70001..=70016).contains(&index) && raid["Type"] == 7 {
                if raid["DeckCount"] != 8 || raid["PlayerCount"] != 3 {
                    return Err("Unexpected party Challenge Raid roster limits".into());
                }
                raid["DeckCountPerPlayer"] = serde_json::json!(4);
            }
        }
        Ok(data)
    }
    pub fn load_with_rules(path: &Path, rules: &str) -> Result<Self, Box<dyn std::error::Error>> {
        let mut data: Self = serde_json::from_reader(std::fs::File::open(path)?)?;
        data.rules = serde_json::from_reader(std::fs::File::open(path.with_file_name(rules))?)?;
        // The installed client patch distinguishes locked content from invalid rooms.
        for path in ["raid/join_raid_room","party_dungeon/join_party_dungeon_room","party_dungeon/wait_party_dungeon_room"] {
            if let Some(results)=data.contracts.get_mut(path).and_then(|c|c["Results"].as_array_mut()) {
                if !results.iter().any(|v|v=="DungeonLocked") {results.push(Value::String("DungeonLocked".into()));}
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
