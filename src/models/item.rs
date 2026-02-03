use serde::{Deserialize, Serialize};

/// Item info matching client's ItemInfo
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ItemInfo {
    pub item_index: i32,
    pub count: i32,
}
