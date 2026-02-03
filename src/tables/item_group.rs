//! Item group table data
//!
//! Item groups are used for loot tables, gacha, etc.
//! Groups can contain items directly, or references to other groups (nested).
//!
//! The JIT format stores string codes as integer indices into a StringPool.
//! We load the resolved version where all indices have been converted to strings.

use serde::Deserialize;
use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

/// Sub-item data within an item group (from resolved JSON)
#[allow(dead_code)]
#[derive(Debug, Clone, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ItemGroupSubData {
    /// Item code string (e.g., "ARTIFACT_UNIQUE_1000" or another group code)
    pub item_code: String,
    /// Drop rate for this item within the group
    #[serde(default)]
    pub rate: i32,
    /// Minimum count when dropping
    #[serde(default)]
    pub item_min: i32,
    /// Maximum count when dropping
    #[serde(default)]
    pub item_max: i32,
    /// Star level (unused in some contexts)
    #[allow(dead_code)]
    #[serde(default)]
    pub item_star: i32,
    /// Minimum star when dropping
    #[serde(default)]
    pub item_star_min: i32,
    /// Maximum star when dropping  
    #[serde(default)]
    pub item_star_max: i32,
    /// Free flag
    #[allow(dead_code)]
    #[serde(default)]
    pub free: i32,
}

/// Item group data from resolved ItemGroupTable
#[allow(dead_code)]
#[derive(Debug, Clone, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ItemGroupData {
    /// Group code string
    #[allow(dead_code)]
    pub item_group_code: String,
    /// Description (resolved from StringPool)
    #[allow(dead_code)]
    #[serde(default)]
    pub item_group_desc: String,
    #[allow(dead_code)]
    #[serde(default)]
    pub show_detail: bool,
    /// Display name key (resolved from StringPool)
    #[allow(dead_code)]
    #[serde(default)]
    pub item_group_name_key: String,
    /// Total rate for probability calculations
    #[serde(default)]
    pub total_rate: i32,
    /// List of items in this group
    #[serde(default)]
    pub sub_data_list: Vec<ItemGroupSubData>,
}

impl ItemGroupData {
    /// Roll a random item from this group based on rates
    /// Returns (item_code, count, star_min, star_max) or None if no drop
    pub fn roll_item(&self) -> Option<(String, i32, i32, i32)> {
        if self.sub_data_list.is_empty() {
            return None;
        }
        
        // Calculate total rate
        let calculated_total: i32 = self.sub_data_list.iter().map(|s| s.rate).sum();
        let total = self.total_rate.max(calculated_total);
        
        if total <= 0 {
            return None;
        }
        
        let roll = rand::random::<i32>().abs() % total;
        let mut cumulative = 0;
        
        for item in &self.sub_data_list {
            cumulative += item.rate;
            if roll < cumulative {
                let count = if item.item_min >= item.item_max {
                    item.item_min.max(1)
                } else {
                    item.item_min + (rand::random::<i32>().abs() % (item.item_max - item.item_min + 1))
                };
                
                return Some((item.item_code.clone(), count, item.item_star_min, item.item_star_max));
            }
        }
        
        // Fallback to first item
        let item = &self.sub_data_list[0];
        Some((item.item_code.clone(), item.item_min.max(1), item.item_star_min, item.item_star_max))
    }
    
    /// Roll item from group, filtering by allowed grades (from bracket notation like [1:2:4])
    /// grade_filter contains the allowed grade indices (1-based position in list)
    pub fn roll_item_with_filter(&self, grade_filter: &[i32]) -> Option<(String, i32, i32, i32)> {
        if self.sub_data_list.is_empty() {
            return None;
        }
        
        // Filter items by grade (position in list, 1-based)
        let filtered: Vec<_> = self.sub_data_list.iter()
            .enumerate()
            .filter(|(idx, _)| grade_filter.is_empty() || grade_filter.contains(&((*idx as i32) + 1)))
            .map(|(_, item)| item)
            .collect();
        
        if filtered.is_empty() {
            return None;
        }
        
        // Roll based on filtered rates
        let total: i32 = filtered.iter().map(|i| i.rate).sum();
        if total <= 0 {
            return None;
        }
        
        let roll = rand::random::<i32>().abs() % total;
        let mut cumulative = 0;
        
        for item in &filtered {
            cumulative += item.rate;
            if roll < cumulative {
                let count = if item.item_min >= item.item_max {
                    item.item_min.max(1)
                } else {
                    item.item_min + (rand::random::<i32>().abs() % (item.item_max - item.item_min + 1))
                };
                
                return Some((item.item_code.clone(), count, item.item_star_min, item.item_star_max));
            }
        }
        
        let item = filtered[0];
        Some((item.item_code.clone(), item.item_min.max(1), item.item_star_min, item.item_star_max))
    }
}

/// Item group table - indexed by group code string
#[derive(Debug, Clone, Default)]
pub struct ItemGroupTable {
    pub entries: HashMap<String, ItemGroupData>,
}

impl ItemGroupTable {
    /// Load item group table from resolved JSON file
    /// The JSON should be a map from group code string to ItemGroupData
    pub fn load(path: &Path) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let entries: HashMap<String, ItemGroupData> = serde_json::from_reader(reader)?;
        
        Ok(Self { entries })
    }
    
    /// Get item group by string code
    pub fn get(&self, code: &str) -> Option<&ItemGroupData> {
        self.entries.get(code)
    }
    
    /// Check if a code is an item group
    pub fn contains(&self, code: &str) -> bool {
        self.entries.contains_key(code)
    }
}

/// ItemGroup StringPool - maps integer indices to string group codes
/// This is needed because campaign tables reference groups by integer index
#[derive(Debug, Clone, Default)]
pub struct ItemGroupStringPool {
    strings: Vec<String>,
}

impl ItemGroupStringPool {
    /// Load from JSON array of strings
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

/// Parse an item code string like "1100[1:2:4]" or "100[2]" or "251"
/// Returns (base_code, grade_filter)
pub fn parse_item_code(code: &str) -> (String, Vec<i32>) {
    if let Some(bracket_pos) = code.find('[') {
        let base = code[..bracket_pos].to_string();
        let bracket_content = &code[bracket_pos + 1..code.len() - 1]; // Remove [ and ]
        
        let grades: Vec<i32> = bracket_content
            .split(':')
            .filter_map(|s| s.parse().ok())
            .collect();
        
        (base, grades)
    } else {
        (code.to_string(), vec![])
    }
}
