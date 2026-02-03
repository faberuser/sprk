//! Reward table data

use serde::Deserialize;
use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

use super::StringPool;

/// Item drop info - resolved from SubDataList
#[derive(Debug, Clone)]
pub struct ItemDrop {
    pub item_code: String,  // Item code string resolved from RewardStringPool
    pub count: i32,         // Number of items
    pub star_min: i32,      // Min star level (for equipment)
    pub star_max: i32,      // Max star level (for equipment)
}

/// Reward data entry from RewardTable.json
#[allow(dead_code)]
#[derive(Debug, Clone, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct RewardData {
    pub index: i32,
    #[serde(default)]
    pub gold_rate: i32,      // Rate for gold drop (1000 = 100%)
    #[serde(default)]
    pub gold_min: i32,       // Minimum gold amount
    #[serde(default)]
    pub gold_max: i32,       // Maximum gold amount
    #[allow(dead_code)]
    #[serde(default)]
    pub gem_rate: i32,       // Rate for gem drop (1000 = 100%)
    #[allow(dead_code)]
    #[serde(default)]
    pub gem_min: i32,        // Minimum gem amount
    #[allow(dead_code)]
    #[serde(default)]
    pub gem_max: i32,        // Maximum gem amount
    #[allow(dead_code)]
    #[serde(default)]
    pub lua_point_rate: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub lua_point_min: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub lua_point_max: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub shop_event_point_rate: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub shop_event_point_min: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub shop_event_point_max: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub limited_shop_event_point_rate: i32,
    #[allow(dead_code)]
    #[serde(default)]
    pub limited_shop_event_point_value: i32,
    
    /// Item drop list - field_15 in the JSON (SubDataList in C#)
    /// Each entry is: [No, [StringPoolIdx, StringLen], Rate, CountMin, ???, CountMax, StarMin, StarMax, CustomOptionIndex]
    #[serde(default, rename = "field_15")]
    pub sub_data_list: Vec<serde_json::Value>,
}

impl RewardData {
    /// Calculate random gold reward
    pub fn roll_gold(&self) -> i64 {
        if self.gold_rate <= 0 || self.gold_max <= 0 {
            return 0;
        }
        
        // Check if gold should drop (rate is per 1000)
        if self.gold_rate < 1000 {
            let roll: i32 = rand::random::<i32>().abs() % 1000;
            if roll >= self.gold_rate {
                return 0;
            }
        }
        
        // Roll random amount between min and max
        if self.gold_min >= self.gold_max {
            return self.gold_min as i64;
        }
        
        let range = (self.gold_max - self.gold_min + 1) as i64;
        self.gold_min as i64 + (rand::random::<i64>().abs() % range)
    }
    
    /// Calculate random gem reward
    #[allow(dead_code)]
    pub fn roll_gem(&self) -> i64 {
        if self.gem_rate <= 0 || self.gem_max <= 0 {
            return 0;
        }
        
        // Check if gem should drop (rate is per 1000)
        if self.gem_rate < 1000 {
            let roll: i32 = rand::random::<i32>().abs() % 1000;
            if roll >= self.gem_rate {
                return 0;
            }
        }
        
        // Roll random amount between min and max
        if self.gem_min >= self.gem_max {
            return self.gem_min as i64;
        }
        
        let range = (self.gem_max - self.gem_min + 1) as i64;
        self.gem_min as i64 + (rand::random::<i64>().abs() % range)
    }
    
    /// Parse and roll item drops from sub_data_list
    /// Requires the RewardStringPool to resolve item codes (they are indices into Reward's string pool)
    /// NOTE: RewardTable has its OWN StringPool - do NOT use ItemGroupStringPool here!
    /// Returns Vec<ItemDrop>
    pub fn roll_items(&self, string_pool: &StringPool) -> Vec<ItemDrop> {
        let mut drops = Vec::new();
        
        for item in &self.sub_data_list {
            if let Some(arr) = item.as_array() {
                if arr.len() >= 6 {
                    // Parse: [No, [StringPoolIdx, StringLen], Rate, CountMin, ???, CountMax, StarMin, StarMax, CustomOptionIndex]
                    let code = arr.get(1);
                    let rate = arr.get(2).and_then(|v| v.as_i64()).unwrap_or(0) as i32;
                    let count_min = arr.get(3).and_then(|v| v.as_i64()).unwrap_or(1) as i32;
                    // Index 4 is skipped (null in data)
                    let count_max = arr.get(5).and_then(|v| v.as_i64()).unwrap_or(1) as i32;
                    let star_min = arr.get(6).and_then(|v| v.as_i64()).unwrap_or(0) as i32;
                    let star_max = arr.get(7).and_then(|v| v.as_i64()).unwrap_or(0) as i32;
                    
                    // Parse item code [StringPoolIdx, StringLen] - HashString format
                    let item_code = if let Some(code_arr) = code.and_then(|c| c.as_array()) {
                        let string_pool_idx = code_arr.get(0).and_then(|v| v.as_i64()).unwrap_or(0) as usize;
                        // Resolve from string pool
                        string_pool.get(string_pool_idx).map(|s| s.to_string())
                    } else {
                        None
                    };
                    
                    let item_code = match item_code {
                        Some(code) if !code.is_empty() => code,
                        _ => continue,
                    };
                    
                    // Roll for drop
                    if rate < 1000 {
                        let roll: i32 = rand::random::<i32>().abs() % 1000;
                        if roll >= rate {
                            continue;
                        }
                    }
                    
                    // Roll count
                    let count = if count_min >= count_max {
                        count_min
                    } else {
                        count_min + (rand::random::<i32>().abs() % (count_max - count_min + 1))
                    };
                    
                    if count > 0 {
                        drops.push(ItemDrop {
                            item_code,
                            count,
                            star_min,
                            star_max,
                        });
                    }
                }
            }
        }
        
        drops
    }
}

/// Reward table - indexed by reward index
#[derive(Debug, Clone, Default)]
pub struct RewardTable {
    pub entries: Vec<RewardData>,
    /// Map from index to entry position
    index_map: HashMap<i32, usize>,
}

impl RewardTable {
    /// Load reward table from JSON file
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let entries: Vec<RewardData> = serde_json::from_reader(reader)?;
        
        let mut index_map = HashMap::new();
        for (idx, entry) in entries.iter().enumerate() {
            index_map.insert(entry.index, idx);
        }
        
        Ok(Self { entries, index_map })
    }
    
    /// Get reward data by index
    pub fn get(&self, index: i32) -> Option<&RewardData> {
        self.index_map
            .get(&index)
            .and_then(|&idx| self.entries.get(idx))
    }
}
