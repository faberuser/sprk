//! Shared request parsing, HTTP transport, server discovery, and development tools.
pub mod cdn;
pub mod cheat;
pub mod fallback;
pub mod middleware;
pub mod ping;
pub mod query;
pub(crate) mod request;
pub(crate) mod performance;
pub(crate) mod revisions;

pub mod storage;
