//! Tutorial table data
//!
//! Loads TutorialTable.json and provides access to tutorial dungeon rewards.

use serde::Deserialize;
use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

/// Raw tutorial entry from TutorialTable.json
#[derive(Debug, Clone, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct TutorialEntry {
    pub index: i32,
    /// [chapter_index, dungeon_index] - the dungeon to unlock when this tutorial completes
    #[serde(default)]
    pub reward_dungeon_index: Option<Vec<i32>>,
}

/// Processed tutorial dungeon reward info
#[derive(Debug, Clone)]
pub struct TutorialDungeonReward {
    pub chapter_index: i32,
    pub dungeon_index: i32,
}

/// Tutorial table - maps tutorial index to dungeon rewards
#[derive(Debug, Clone, Default)]
pub struct TutorialTable {
    /// Map of tutorial_index -> dungeon that gets UNLOCKED
    pub dungeon_rewards: HashMap<i32, TutorialDungeonReward>,
}

impl TutorialTable {
    /// Load tutorial table from JSON file
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let entries: Vec<TutorialEntry> = serde_json::from_reader(reader)?;
        
        let mut dungeon_rewards: HashMap<i32, TutorialDungeonReward> = HashMap::new();
        
        for entry in entries {
            if let Some(ref reward) = entry.reward_dungeon_index {
                if reward.len() >= 2 {
                    let chapter = reward[0];
                    let dungeon = reward[1];
                    
                    // Only add if not already present (first occurrence wins)
                    if !dungeon_rewards.contains_key(&entry.index) {
                        dungeon_rewards.insert(entry.index, TutorialDungeonReward {
                            chapter_index: chapter,
                            dungeon_index: dungeon,
                        });
                    }
                }
            }
        }
        
        Ok(Self {
            dungeon_rewards,
        })
    }
    
    /// Get the dungeon that a tutorial unlocks
    pub fn get_reward_dungeon(&self, tutorial_index: i32) -> Option<&TutorialDungeonReward> {
        self.dungeon_rewards.get(&tutorial_index)
    }
}
