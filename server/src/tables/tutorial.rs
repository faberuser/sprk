//! Tutorial rewards are attached to sequence rows, keyed by tutorial index.
use super::StringPool;
use crate::models::equip::EquipItemInfo;
use serde::Deserialize;
use std::{collections::HashMap, fs::File, io::BufReader, path::Path};

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
struct TutorialEntry {
    index: i32,
    sequence: i32,
    #[serde(default)]
    reward_gold: i64,
    #[serde(default)]
    reward_gem: i64,
    #[serde(default)]
    reward_index: i32,
    reward_action: Option<Vec<serde_json::Value>>,
    reward_dungeon_index: Option<Vec<i32>>,
}

#[derive(Debug, Clone, Default)]
pub struct TutorialDefinition {
    pub gold: i64,
    pub gem: i64,
    pub rewards: Vec<i32>,
    pub actions: Vec<Vec<String>>,
    /// Campaign nodes cleared by scripted battles, not the next node to unlock.
    pub dungeons: Vec<(i32, i32)>,
}

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialItem {
    #[serde(default)]
    pub duplicate_reward_index: i32,
    pub kind: String,
    #[serde(default)]
    pub hero_index: i32,
    #[serde(default)]
    pub star: i32,
    #[serde(default)]
    pub level: i32,
    #[serde(default)]
    pub transcend: i32,
}

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialLevel {
    pub level: i32,
    pub local_exp: i64,
}

#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialStar {
    pub star: i32,
    pub transcended: i32,
    pub get_hero_team_exp: i64,
    pub max_hero_level: i32,
}

#[derive(Debug, Clone, Default, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialSupport {
    pub dungeon_difficulties: HashMap<String, i32>,
    pub items: HashMap<i32, TutorialItem>,
    pub hero_levels: Vec<TutorialLevel>,
    pub hero_stars: Vec<TutorialStar>,
    pub team_levels: Vec<TutorialLevel>,
    pub custom_equipment: HashMap<i32, EquipItemInfo>,
}

#[derive(Debug, Clone, Default)]
pub struct TutorialTable {
    pub definitions: HashMap<i32, TutorialDefinition>,
    pub support: TutorialSupport,
}

impl TutorialTable {
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let mut rows: Vec<TutorialEntry> =
            serde_json::from_reader(BufReader::new(File::open(path)?))?;
        let dir = path.parent().unwrap_or(Path::new("."));
        let pool = StringPool::load(&dir.join("TutorialStringPool.json"))?;
        let support = serde_json::from_reader(BufReader::new(File::open(
            dir.join("TutorialSupport.json"),
        )?))?;
        let mut table = Self {
            support,
            ..Self::default()
        };
        rows.sort_by_key(|row| (row.index, row.sequence));
        for row in rows {
            let definition = table.definitions.entry(row.index).or_default();
            definition.gold += row.reward_gold;
            definition.gem += row.reward_gem;
            if row.reward_index != 0 {
                definition.rewards.push(row.reward_index);
            }
            if let Some(indices) = row.reward_dungeon_index {
                if indices.len() != 2 {
                    return Err(format!("Invalid dungeon reward for tutorial {}", row.index).into());
                }
                if !definition.dungeons.contains(&(indices[0], indices[1])) {
                    definition.dungeons.push((indices[0], indices[1]));
                }
            }
            if let Some(action) = row.reward_action {
                let mut decoded = Vec::new();
                for value in action {
                    let text = value
                        .as_str()
                        .or_else(|| value.as_u64().and_then(|i| pool.get(i as usize)))
                        .ok_or_else(|| {
                            format!("Invalid action reference in tutorial {}", row.index)
                        })?;
                    decoded.push(text.trim().to_string());
                }
                if !decoded.is_empty() {
                    definition.actions.push(decoded);
                }
            }
        }
        Ok(table)
    }

    pub fn get(&self, index: i32) -> Option<&TutorialDefinition> {
        self.definitions.get(&index)
    }

    pub fn dungeon_difficulty(&self, chapter: i32, dungeon: i32) -> i32 {
        self.support
            .dungeon_difficulties
            .get(&format!("{chapter}:{dungeon}"))
            .copied()
            .unwrap_or(if chapter <= 10 { 1 } else { 0 })
    }
}

/// EXP in the client is local to the current level.
pub fn add_exp(
    levels: &[TutorialLevel],
    mut level: i32,
    exp: i64,
    add: i64,
    cap: i32,
) -> (i32, i64) {
    let mut remaining = exp + add;
    while level < cap {
        let Some(data) = levels.iter().find(|data| data.level == level) else {
            break;
        };
        if data.local_exp <= 0 || remaining < data.local_exp {
            break;
        }
        remaining -= data.local_exp;
        level += 1;
    }
    (level, if level >= cap { 0 } else { remaining })
}
