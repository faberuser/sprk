//! Transactional invalidation also covers background jobs and external DB writers.
//! Progression has cross-account dependencies (friends, rankings, guild history),
//! so its revision is deliberately global. Equipment revisions are per account.
use crate::{database::DbPool, error::Result};
use sqlx::{Row, SqliteConnection};

pub(crate) async fn migrate(pool: &DbPool) -> anyhow::Result<()> {
    sqlx::query("CREATE TABLE IF NOT EXISTS response_revisions(scope TEXT NOT NULL,account INTEGER NOT NULL,revision INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(scope,account))").execute(pool).await?;
    sqlx::query("INSERT OR IGNORE INTO response_revisions(scope,account) VALUES('progression',0)").execute(pool).await?;
    // Keep this list alongside the queries in progression::{state,mission_history,
    // play_record,attendance}. Unknown HTTP mutations still run notifications.
    for table in ["accounts", "credentials", "heroes", "hero_details", "equip_items",
        "extension_state", "campaign_progress", "progression_metrics", "progression_claims",
        "progression_login", "user_info", "costumes", "friends", "guild_members",
        "tutorial_progress", "items", "progression_entitlements", "progression_main_quest",
        "progression_world_events", "attendance_calendar_state", "play_record_values",
        "arena_runs", "battle_state", "guild_battle_scores", "community_claims",
        "guild_arena_records", "community_state", "guild_arena_decks", "battle_scores"] {
        let columns: Option<&[&str]> = match table {
            "accounts" => Some(&["account_id", "login_id", "created_at"]),
            "user_info" => Some(&["account_id", "team_level"]),
            "items" => Some(&["account_id", "item_index", "count"]),
            _ => None,
        };
        install(pool, table, "progression", "0", columns).await?;
    }
    for (table, columns) in [
        ("heroes", Some(vec!["account_id", "hero_index", "level", "equip_item_slot_index_1", "equip_item_slot_index_2", "equip_item_slot_index_3", "equip_item_slot_index_4", "equip_item_slot_index_5", "equip_item_slot_index_6", "equip_item_slot_index_7", "equip_item_slot_index_8", "equip_item_slot_index_9", "equip_item_slot_index_10"])),
        ("equip_items", Some(vec!["account_id", "slot_index", "item_index", "level"])),
        ("user_info", Some(vec!["account_id", "team_level"])),
        ("extension_state", None),
    ] {
        install(pool, table, "equipment", "account_id", columns.as_deref()).await?;
    }
    Ok(())
}

async fn install(pool: &DbPool, table: &str, scope: &str, account: &str, columns: Option<&[&str]>) -> anyhow::Result<()> {
    let all = sqlx::query(&format!("PRAGMA table_info(\"{table}\")")).fetch_all(pool).await?;
    anyhow::ensure!(!all.is_empty(), "Missing revision dependency: {table}");
    let names: Vec<String> = all.iter().map(|r| r.get("name")).collect();
    let columns: Vec<&str> = columns.map(Vec::from).unwrap_or_else(|| names.iter().map(String::as_str).collect());
    let changed = columns.iter().map(|c| format!("OLD.\"{c}\" IS NOT NEW.\"{c}\"")).collect::<Vec<_>>().join(" OR ");
    for (event, row) in [("INSERT", "NEW"), ("UPDATE", "NEW"), ("DELETE", "OLD")] {
        let owner = if account == "0" { "0".to_owned() } else { format!("{row}.\"{account}\"") };
        let bump = |owner: &str| format!("INSERT INTO response_revisions(scope,account,revision) VALUES('{scope}',{owner},1) ON CONFLICT(scope,account) DO UPDATE SET revision=revision+1;");
        let old_owner = if event == "UPDATE" && account != "0" {
            format!("INSERT INTO response_revisions(scope,account,revision) SELECT '{scope}',OLD.\"{account}\",1 WHERE OLD.\"{account}\" IS NOT NEW.\"{account}\" ON CONFLICT(scope,account) DO UPDATE SET revision=revision+1;")
        } else { String::new() };
        let condition = if event == "UPDATE" { format!(" WHEN {changed}") } else { String::new() };
        sqlx::query(&format!("CREATE TRIGGER IF NOT EXISTS response_revision_{scope}_{table}_{event} AFTER {event} ON \"{table}\"{condition} BEGIN {}{old_owner} END", bump(&owner))).execute(pool).await?;
    }
    Ok(())
}

pub(crate) async fn get(db: &mut SqliteConnection, scope: &str, account: i64) -> Result<i64> {
    Ok(sqlx::query_scalar("SELECT revision FROM response_revisions WHERE scope=? AND account=?")
        .bind(scope).bind(account).fetch_optional(db).await?.unwrap_or(0))
}
