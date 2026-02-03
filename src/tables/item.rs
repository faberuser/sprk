use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

/// Item table for resolving item codes to item indices
/// 
/// The game uses item codes (strings like "ARTIFACT_UNIQUE_1000") in tables,
/// which need to be resolved to integer ItemIndex values.
#[derive(Debug)]
pub struct ItemTable {
    /// Mapping from item code (string) to item index (i32)
    code_to_index: HashMap<String, i32>,
}

impl Default for ItemTable {
    fn default() -> Self {
        Self {
            code_to_index: HashMap::new(),
        }
    }
}

impl ItemTable {
    /// Load from ItemCodeToIndex.json which is a direct string→i32 map
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, Box<dyn std::error::Error>> {
        let file = File::open(path)?;
        let reader = BufReader::new(file);
        let code_to_index: HashMap<String, i32> = serde_json::from_reader(reader)?;
        
        tracing::info!("Loaded {} item code→index mappings", code_to_index.len());
        
        // Debug: Log some specific codes we're interested in
        for code in &["ARTIFACT_UNIQUE_1000", "HERO_KASEL", "HERO_FREY"] {
            if let Some(idx) = code_to_index.get(*code) {
                tracing::debug!("Item code '{}' → index {}", code, idx);
            }
        }
        
        Ok(ItemTable { code_to_index })
    }
    
    /// Resolve an item code to its actual item index
    /// Returns None if the code is not found
    pub fn get_index(&self, code: &str) -> Option<i32> {
        self.code_to_index.get(code).copied()
    }
    
    /// Get the number of loaded items
    pub fn len(&self) -> usize {
        self.code_to_index.len()
    }
    
    /// Check if an item index represents equipment (stored in equip_items table)
    /// Equipment items have unique instances with stars, levels, options, etc.
    /// Based on item index ranges:
    /// - Weapons: 50000-59999
    /// - Armor: 60000-79999
    /// - Accessories: 80000-89999
    /// - Runes: 100000-129999
    /// - Orbs: 140000-149999
    pub fn is_equipment(item_index: i32) -> bool {
        matches!(item_index,
            50000..=59999 |  // Weapons
            60000..=79999 |  // Armor
            80000..=89999 |  // Accessories
            100000..=129999 | // Runes
            140000..=149999   // Orbs
        )
    }
}
