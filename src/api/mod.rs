pub mod middleware;
pub mod user;
pub mod ping;
pub mod query;
pub mod lobby;
pub mod hero;
pub mod hero_inn;
pub mod equip;
pub mod extensions;
pub mod shop;
pub mod item;
pub mod mail;
pub mod friend;
pub mod chat;
pub mod campaign;
pub mod battle;
pub mod guild;
pub mod stamina;
pub mod tutorial;
pub mod cheat;
pub mod fallback;
pub mod auth;
pub mod cdn;


pub(crate) mod social_request;

#[cfg(test)]
mod social_tests;

pub mod craft;

#[cfg(test)]
mod inventory_tests;

#[cfg(test)]
mod hero_shop_tests;

pub mod hero_presets;
pub mod progression;
