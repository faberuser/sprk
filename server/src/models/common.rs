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
