use serde::{Deserialize, Serialize};

/// An item grant before it is applied to inventory (not a wire response).
#[derive(Debug, Clone, Copy)]
pub(crate) struct ItemGrant {
    pub index: i32,
    pub count: i32,
    pub star: i32,
    pub custom: i32,
}

/// Item info matching client's ItemInfo
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ItemInfo {
    pub item_index: i32,
    pub count: i32,
    pub locked: u8,
    pub created_time: Option<String>,
    pub uid: String,
}
