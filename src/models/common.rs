use serde::{Deserialize, Serialize};

/// Base result types matching the client's BaseResultType enum
#[derive(Debug, Clone, Copy, Serialize, Deserialize, Default)]
#[repr(i32)]
pub enum BaseResultType {
    Fail = 0,
    #[default]
    Success = 1,
    Timeout = 2,
    SessionError = 3,
    SequenceError = 4,
    AccountError = 5,
    Unhandled = 6,
    Exception = 7,
    HttpError = 8,
    InternalError = 9,
    InternalServerError = 10,
    ParsingError = 11,
    EmptyHost = 12,
    Disabled = 13,
    Offline = 14,
    InvalidAppError = 15,
    NoGuild = 16,
}

impl From<BaseResultType> for i32 {
    fn from(val: BaseResultType) -> Self {
        val as i32
    }
}

/// Currency change result
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct CurrencyResultInfo {
    pub currency_type: i32,
    pub before_value: i64,
    pub after_value: i64,
    pub change_value: i64,
}

/// Reward item info for mail attachments etc.
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct RewardItemInfo {
    pub reward_type: i32,
    pub item_index: i32,
    pub count: i32,
}
