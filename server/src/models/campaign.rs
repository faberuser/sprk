use serde::{Deserialize, Serialize};

/// Chapter dungeon info for campaign progress
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ChapterDungeonInfo {
    pub dungeon_id: i32,
    pub chapter_id: i32,
    pub clear_count: i32,
    pub best_star: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub first_clear_time: Option<String>,
}
