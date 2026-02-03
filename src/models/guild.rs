use serde::{Deserialize, Serialize};

/// Guild info matching client's GuildInfo
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct GuildInfo {
    pub guild_id: i64,
    pub name: String,
    pub notice: String,
    pub level: i32,
    pub exp: i32,
    pub master_account_id: i64,
    pub master_nick: String,
    pub member_count: i32,
    pub max_members: i32,
}
