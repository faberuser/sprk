//! Game data tables module
//! 
//! This module loads and provides access to game data from decoded JSON table files.

use std::fs::File;
use std::io::BufReader;
use std::path::Path;
use std::sync::Arc;

mod campaign_dungeon;
pub mod item;
mod item_group;
mod reward;
mod tutorial;

pub use campaign_dungeon::*;
pub use item::*;
pub use item_group::*;
pub use reward::*;
pub use tutorial::*;

/// String pool for resolving HashString indices to actual strings
#[derive(Debug, Clone, Default)]
pub struct StringPool {
    strings: Vec<String>,
}

impl StringPool {
    /// Load string pool from JSON file
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let strings: Vec<String> = serde_json::from_reader(reader)?;
        Ok(Self { strings })
    }
    
    /// Get string by index
    pub fn get(&self, index: usize) -> Option<&str> {
        self.strings.get(index).map(|s| s.as_str())
    }
    
    /// Get the number of strings in the pool
    pub fn len(&self) -> usize {
        self.strings.len()
    }
}

/// Reward table string pool - for resolving field_15 indices in RewardTable
/// This is DIFFERENT from ItemGroupStringPool - each JIT file has its own string pool!
pub type RewardStringPool = StringPool;

/// Game tables container - holds all loaded table data
#[derive(Debug, Clone)]
pub struct GameTables {
    pub campaign_dungeons: Arc<CampaignDungeonTable>,
    pub rewards: Arc<RewardTable>,
    pub item_groups: Arc<ItemGroupTable>,
    pub item_group_string_pool: Arc<ItemGroupStringPool>,
    pub reward_string_pool: Arc<RewardStringPool>,
    pub items: Arc<ItemTable>,
    pub tutorials: Arc<TutorialTable>,
}

impl GameTables {
    /// Load all game tables from the specified directory
    pub fn load(table_dir: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        tracing::info!("Loading game tables from: {:?}", table_dir);
        
        let campaign_dungeons = CampaignDungeonTable::load(
            &table_dir.join("CampaignDungeonTable.json")
        )?;
        tracing::info!("Loaded {} campaign dungeon entries", campaign_dungeons.entries.len());
        
        let rewards = RewardTable::load(
            &table_dir.join("RewardTable.json")
        )?;
        tracing::info!("Loaded {} reward entries", rewards.entries.len());
        
        // Load the resolved item group table (keyed by string codes)
        let item_groups = ItemGroupTable::load(
            &table_dir.join("ItemGroupTableResolved.json")
        )?;
        tracing::info!("Loaded {} item group entries", item_groups.entries.len());
        
        // Load the item group string pool (for resolving integer indices to string codes in ItemGroupTable)
        let item_group_string_pool = ItemGroupStringPool::load(
            &table_dir.join("ItemGroupStringPool.json")
        )?;
        tracing::info!("Loaded ItemGroup string pool with {} entries", item_group_string_pool.len());
        
        // Load the reward string pool (for resolving field_15 indices in RewardTable)
        let reward_string_pool = RewardStringPool::load(
            &table_dir.join("RewardStringPool.json")
        )?;
        tracing::info!("Loaded Reward string pool with {} entries", reward_string_pool.len());
        
        // Load item code→index mapping
        let items = ItemTable::load(
            &table_dir.join("ItemCodeToIndex.json")
        )?;
        tracing::info!("Loaded {} item code→index mappings", items.len());
        
        // Load tutorial table
        let tutorials = TutorialTable::load(
            &table_dir.join("TutorialTable.json")
        )?;
        tracing::info!("Loaded {} tutorial dungeon rewards", tutorials.dungeon_rewards.len());
        
        Ok(Self {
            campaign_dungeons: Arc::new(campaign_dungeons),
            rewards: Arc::new(rewards),
            item_groups: Arc::new(item_groups),
            item_group_string_pool: Arc::new(item_group_string_pool),
            reward_string_pool: Arc::new(reward_string_pool),
            items: Arc::new(items),
            tutorials: Arc::new(tutorials),
        })
    }
    
    /// Create empty tables (for testing or when tables aren't available)
    pub fn empty() -> Self {
        Self {
            campaign_dungeons: Arc::new(CampaignDungeonTable::default()),
            rewards: Arc::new(RewardTable::default()),
            item_groups: Arc::new(ItemGroupTable::default()),
            item_group_string_pool: Arc::new(ItemGroupStringPool::default()),
            reward_string_pool: Arc::new(RewardStringPool::default()),
            items: Arc::new(ItemTable::default()),
            tutorials: Arc::new(TutorialTable::default()),
        }
    }
    
    /// Get campaign dungeon data by chapter and dungeon index
    pub fn get_campaign_dungeon(&self, chapter: i32, dungeon: i32) -> Option<&CampaignDungeonData> {
        self.campaign_dungeons.get(chapter, dungeon)
    }
    
    /// Get reward data by index
    pub fn get_reward(&self, index: i32) -> Option<&RewardData> {
        self.rewards.get(index)
    }
    
    /// Get item group by string code
    pub fn get_item_group(&self, code: &str) -> Option<&ItemGroupData> {
        self.item_groups.get(code)
    }
    
    /// Resolve an integer group code to its string code via ItemGroup StringPool
    pub fn resolve_group_code(&self, index: i32) -> Option<&str> {
        self.item_group_string_pool.get(index as usize)
    }
    
    /// Get item index by item code
    /// The game uses string codes in reward tables which need to be resolved to integer indices
    pub fn get_item_index(&self, code: &str) -> Option<i32> {
        self.items.get_index(code)
    }
    
    /// Roll from an item group given an integer code (from ItemGroupTable)
    /// First resolves the integer to a string code via the ItemGroup StringPool,
    /// then recursively rolls from item groups until we get an actual item.
    /// 
    /// Returns (item_index, count, star_min, star_max) or None
    pub fn roll_item_from_group(&self, group_code_index: i32, grade_filter: &[i32]) -> Option<(i32, i32, i32, i32)> {
        // First, resolve the integer code to a string code
        let group_code = self.item_group_string_pool.get(group_code_index as usize)?;
        
        tracing::debug!("Rolling from group: {} → '{}'", group_code_index, group_code);
        
        self.roll_item_recursive(group_code, grade_filter, 0)
    }
    
    /// Roll from an item group given a string code directly
    pub fn roll_item_from_group_code(&self, group_code: &str, grade_filter: &[i32]) -> Option<(i32, i32, i32, i32)> {
        self.roll_item_recursive(group_code, grade_filter, 0)
    }
    
    fn roll_item_recursive(&self, code: &str, grade_filter: &[i32], depth: i32) -> Option<(i32, i32, i32, i32)> {
        // Prevent infinite loops
        if depth > 10 {
            tracing::warn!("Item group recursion too deep at code '{}'", code);
            return None;
        }
        
        // Check if this code is an item group
        if let Some(group) = self.item_groups.get(code) {
            // Roll from the group
            let result = if depth == 0 && !grade_filter.is_empty() {
                // Only apply grade filter on the first level
                group.roll_item_with_filter(grade_filter)
            } else {
                group.roll_item()
            };
            
            if let Some((rolled_code, count, star_min, star_max)) = result {
                // Check if the rolled code is also an item group (nested)
                if self.item_groups.contains(&rolled_code) {
                    // Recurse into nested group
                    if let Some((final_idx, nested_count, nested_star_min, nested_star_max)) = 
                        self.roll_item_recursive(&rolled_code, &[], depth + 1) 
                    {
                        // Multiply counts, take star values from nested
                        return Some((final_idx, count * nested_count, nested_star_min, nested_star_max));
                    }
                } else {
                    // rolled_code is not a group - it's the final item code
                    // Look up the actual item index from the ItemTable
                    if let Some(item_index) = self.items.get_index(&rolled_code) {
                        tracing::debug!("Resolved item: group '{}' → code '{}' → index {}", 
                            code, rolled_code, item_index);
                        return Some((item_index, count, star_min, star_max));
                    } else {
                        // Code not found in ItemTable
                        tracing::warn!("Item code '{}' not found in ItemTable", rolled_code);
                        return None;
                    }
                }
            }
        }
        
        None
    }
}
