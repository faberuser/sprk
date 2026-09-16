//! API services grouped by feature. HTTP route registration lives in `main.rs`
//! and the feature modules that provide their own `routes()` function.
pub mod account;
pub mod battle;
pub mod community;
pub mod extensions;
pub mod heroes;
pub mod inventory;
pub mod progression;
pub mod system;
pub mod tutorial;
