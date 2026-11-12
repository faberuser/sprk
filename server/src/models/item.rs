use serde::{Deserialize, Serialize};

/// Unsupported hero Limit Break rewards, scrolls, tickets and currencies.
/// Keep metadata/owned inventory intact, but never issue new copies.
pub(crate) fn disabled_reward(index: i32) -> bool {
    matches!(index,
        30005 | 30010 | 30015 | 30020 | 30025 | 30030 | 30035 | 91006 |
        9000101..=9000202 | 9001001..=9001002 | 9002001..=9002016
    )
}

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
