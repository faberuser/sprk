//! Campaign dungeon table data

use serde::Deserialize;
use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

/// Campaign dungeon data entry from CampaignDungeonTable.json
#[allow(dead_code)]
#[derive(Debug, Clone, Deserialize, Default)]
pub struct CampaignDungeonData {
    #[serde(rename = "ChapterIndex")]
    pub chapter_index: i32,
    #[serde(rename = "DungeonIndex")]
    pub dungeon_index: i32,
    
    // Easy difficulty rewards
    #[serde(default, rename = "DropRewardIndex_Easy")]
    pub drop_reward_index_easy: i32,
    #[serde(default, rename = "CreatureExp_Easy")]
    pub creature_exp_easy: i32,
    #[allow(dead_code)]
    #[serde(default, rename = "FirstClearRewardIndex_Easy")]
    pub first_clear_reward_index_easy: i32,
    #[serde(default, rename = "FirstClearGem_Easy")]
    pub first_clear_gem_easy: i32,
    #[serde(default, rename = "ReqStamina_Easy")]
    pub req_stamina_easy: i32,
    
    // Normal difficulty rewards
    #[serde(default, rename = "DropRewardIndex_Normal")]
    pub drop_reward_index_normal: i32,
    #[serde(default, rename = "CreatureExp_Normal")]
    pub creature_exp_normal: i32,
    #[allow(dead_code)]
    #[serde(default, rename = "FirstClearRewardIndex_Normal")]
    pub first_clear_reward_index_normal: i32,
    #[serde(default, rename = "FirstClearGem_Normal")]
    pub first_clear_gem_normal: i32,
    #[serde(default, rename = "ReqStamina_Normal")]
    pub req_stamina_normal: i32,
    
    // Hard difficulty rewards
    #[serde(default, rename = "DropRewardIndex_Hard")]
    pub drop_reward_index_hard: i32,
    #[serde(default, rename = "CreatureExp_Hard")]
    pub creature_exp_hard: i32,
    #[allow(dead_code)]
    #[serde(default, rename = "FirstClearRewardIndex_Hard")]
    pub first_clear_reward_index_hard: i32,
    #[serde(default, rename = "FirstClearGem_Hard")]
    pub first_clear_gem_hard: i32,
    #[serde(default, rename = "ReqStamina_Hard")]
    pub req_stamina_hard: i32,
    
    // Hell difficulty rewards
    #[serde(default, rename = "DropRewardIndex_Hell")]
    pub drop_reward_index_hell: i32,
    #[serde(default, rename = "CreatureExp_Hell")]
    pub creature_exp_hell: i32,
    #[allow(dead_code)]
    #[serde(default, rename = "FirstClearRewardIndex_Hell")]
    pub first_clear_reward_index_hell: i32,
    #[serde(default, rename = "FirstClearGem_Hell")]
    pub first_clear_gem_hell: i32,
    #[serde(default, rename = "ReqStamina_Hell")]
    pub req_stamina_hell: i32,
    
    // Other fields
    #[allow(dead_code)]
    #[serde(default, rename = "FirstRewardIndex")]
    pub first_reward_index: i32,
    #[serde(default, rename = "ReqStaminaType")]
    pub req_stamina_type: i32,
    #[serde(default, rename = "ReqStamina")]
    pub req_stamina: i32,
}

/// Campaign dungeon table - indexed by (chapter, dungeon)
#[derive(Debug, Clone, Default)]
pub struct CampaignDungeonTable {
    pub entries: Vec<CampaignDungeonData>,
    /// Map from (chapter_index, dungeon_index) to entry index
    index_map: HashMap<(i32, i32), usize>,
}

impl CampaignDungeonTable {
    /// Load campaign dungeon table from JSON file
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let entries: Vec<CampaignDungeonData> = serde_json::from_reader(reader)?;
        
        let mut index_map = HashMap::new();
        for (idx, entry) in entries.iter().enumerate() {
            index_map.insert((entry.chapter_index, entry.dungeon_index), idx);
        }
        
        Ok(Self { entries, index_map })
    }
    
    /// Get dungeon data by chapter and dungeon index
    pub fn get(&self, chapter: i32, dungeon: i32) -> Option<&CampaignDungeonData> {
        self.index_map
            .get(&(chapter, dungeon))
            .and_then(|&idx| self.entries.get(idx))
    }
}
