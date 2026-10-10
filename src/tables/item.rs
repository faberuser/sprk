use std::collections::HashMap;
use std::fs::File;
use std::io::BufReader;
use std::path::Path;

/// Item table for resolving item codes to item indices
///
/// The game uses item codes (strings like "ARTIFACT_UNIQUE_1000") in tables,
/// which need to be resolved to integer ItemIndex values.
#[derive(Debug, Default)]
pub struct ItemTable {
    /// Mapping from item code (string) to item index (i32)
    code_to_index: HashMap<String, i32>,
    reward_items: HashMap<i32, super::tutorial::TutorialItem>,
}


impl ItemTable {
    /// Load from ItemCodeToIndex.json which is a direct string→i32 map
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, Box<dyn std::error::Error>> {
        let metadata_path = path.as_ref().with_file_name("ItemRewardMetadata.json");
        let reward_items = serde_json::from_reader(BufReader::new(File::open(metadata_path)?))?;
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

        Ok(ItemTable {
            code_to_index,
            reward_items,
        })
    }

    pub fn reward_item(&self, index: i32) -> Option<&super::tutorial::TutorialItem> {
        self.reward_items.get(&index)
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

}
