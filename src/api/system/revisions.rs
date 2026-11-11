//! Transactional invalidation also covers background jobs and external DB writers.
//! Revisions are per account: a change bumps the account(s) whose responses
//! depend on that row. Progression data read across accounts (world boss
//! ranking, credential links) bumps the global `('progression', 0)` revision,
//! which every account's progression cache also checks.
use crate::{database::DbPool, error::Result};
use sqlx::{Row, SqliteConnection};

/// An account whose cached response depends on a changed row. `account` and
/// `when` are SQL templates with `{row}` standing for `NEW` or `OLD`.
struct Owner {
    account: &'static str,
    when: &'static str,
}
const fn owner(account: &'static str) -> Owner { Owner { account, when: "1" } }
const GLOBAL: Owner = owner("0");

pub(crate) async fn migrate(pool: &DbPool) -> anyhow::Result<()> {
    let mut tx = pool.begin().await?;
    sqlx::query("CREATE TABLE IF NOT EXISTS response_revisions(scope TEXT NOT NULL,account INTEGER NOT NULL,revision INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(scope,account))").execute(&mut *tx).await?;
    sqlx::query("INSERT OR IGNORE INTO response_revisions(scope,account) VALUES('progression',0)").execute(&mut *tx).await?;
    // Keep this list alongside the queries in progression::{state,mission_history,
    // play_record,attendance}. Every table read for one account names that account
    // here; only data read across accounts may bump the global revision, because
    // a global bump invalidates every online player's cache.
    let account_id = [owner("{row}.\"account_id\"")];
    let account = [owner("{row}.\"account\"")];
    let progression: [(&str, &[Owner]); 29] = [
        ("accounts", &account_id), ("heroes", &account_id), ("hero_details", &account_id),
        ("equip_items", &account_id), ("extension_state", &account_id), ("campaign_progress", &account_id),
        ("progression_metrics", &account_id), ("progression_claims", &account_id), ("progression_login", &account_id),
        ("user_info", &account_id), ("costumes", &account_id), ("guild_members", &account_id),
        ("tutorial_progress", &account_id), ("items", &account_id), ("progression_entitlements", &account_id),
        ("progression_main_quest", &account_id), ("progression_world_events", &account_id),
        ("attendance_calendar_state", &account_id), ("play_record_values", &account_id),
        ("arena_runs", &account), ("battle_state", &account), ("guild_battle_scores", &account),
        ("community_claims", &account), ("guild_arena_records", &account), ("guild_arena_decks", &account),
        // Accepted friends are counted from either side of the row.
        ("friends", &[owner("{row}.\"account_id\""), owner("{row}.\"friend_account_id\"")]),
        // Conquest history names its player inside the JSON; other kinds are unused.
        ("community_state", &[Owner { account: "COALESCE(json_extract({row}.\"data\",'$.AccountId'),0)", when: "{row}.\"kind\"='guild_conquest_history'" }]),
        // World boss placement ranks every account's scores.
        ("battle_scores", &[owner("{row}.\"account\""), Owner { account: "0", when: "{row}.\"family\"='world_boss'" }]),
        // Linked status joins by login_id; links change rarely.
        ("credentials", &[GLOBAL]),
    ];
    for (table, owners) in progression {
        let columns: Option<&[&str]> = match table {
            "accounts" => Some(&["account_id", "login_id", "created_at"]),
            "user_info" => Some(&["account_id", "team_level"]),
            "items" => Some(&["account_id", "item_index", "count"]),
            _ => None,
        };
        install(&mut tx, table, "progression", owners, columns).await?;
    }
    for (table, columns) in [
        ("heroes", Some(vec!["account_id", "hero_index", "level", "equip_item_slot_index_1", "equip_item_slot_index_2", "equip_item_slot_index_3", "equip_item_slot_index_4", "equip_item_slot_index_5", "equip_item_slot_index_6", "equip_item_slot_index_7", "equip_item_slot_index_8", "equip_item_slot_index_9", "equip_item_slot_index_10"])),
        ("equip_items", Some(vec!["account_id", "slot_index", "item_index", "level"])),
        ("user_info", Some(vec!["account_id", "team_level"])),
        ("extension_state", None),
    ] {
        install(&mut tx, table, "equipment", &account_id, columns.as_deref()).await?;
    }
    tx.commit().await?;
    Ok(())
}

async fn install(db: &mut SqliteConnection, table: &str, scope: &str, owners: &[Owner], columns: Option<&[&str]>) -> anyhow::Result<()> {
    let all = sqlx::query(sqlx::AssertSqlSafe(format!("PRAGMA table_info(\"{table}\")"))).fetch_all(&mut *db).await?;
    anyhow::ensure!(!all.is_empty(), "Missing revision dependency: {table}");
    let names: Vec<String> = all.iter().map(|r| r.get("name")).collect();
    // Queue heartbeats (`arena_runs.last_poll`) change no response.
    let columns: Vec<&str> = columns.map(Vec::from).unwrap_or_else(|| names.iter().map(String::as_str)
        .filter(|c| !(table == "arena_runs" && *c == "last_poll")).collect());
    let changed = columns.iter().map(|c| format!("OLD.\"{c}\" IS NOT NEW.\"{c}\"")).collect::<Vec<_>>().join(" OR ");
    let bump = |account: &str, when: &str| format!("INSERT INTO response_revisions(scope,account,revision) SELECT '{scope}',{account},1 WHERE {when} ON CONFLICT(scope,account) DO UPDATE SET revision=revision+1;");
    for event in ["INSERT", "UPDATE", "DELETE"] {
        let mut body = String::new();
        for o in owners {
            let at = |row: &str| (o.account.replace("{row}", row), o.when.replace("{row}", row));
            let (new, new_when) = at("NEW");
            let (old, old_when) = at("OLD");
            match event {
                "INSERT" => body += &bump(&new, &new_when),
                "DELETE" => body += &bump(&old, &old_when),
                _ => {
                    body += &bump(&new, &new_when);
                    // A row moving between owners (or out of a filter) changes the old one too.
                    body += &bump(&old, &format!("({old_when}) AND NOT COALESCE(({new_when}) AND {old} IS {new},0)"));
                }
            }
        }
        let condition = if event == "UPDATE" { format!(" WHEN {changed}") } else { String::new() };
        // Recreate: IF NOT EXISTS would keep a previous definition's owners.
        let name = format!("response_revision_{scope}_{table}_{event}");
        sqlx::query(sqlx::AssertSqlSafe(format!("DROP TRIGGER IF EXISTS {name}"))).execute(&mut *db).await?;
        sqlx::query(sqlx::AssertSqlSafe(format!("CREATE TRIGGER {name} AFTER {event} ON \"{table}\"{condition} BEGIN {body} END"))).execute(&mut *db).await?;
    }
    Ok(())
}

pub(crate) async fn get(db: &mut SqliteConnection, scope: &str, account: i64) -> Result<i64> {
    Ok(sqlx::query_scalar("SELECT revision FROM response_revisions WHERE scope=? AND account=?")
        .bind(scope).bind(account).fetch_optional(db).await?.unwrap_or(0))
}

/// The account's own progression revision and the global one it also depends on.
pub(crate) async fn progression(db: &mut SqliteConnection, account: i64) -> Result<(i64, i64)> {
    Ok(sqlx::query_as("SELECT COALESCE((SELECT revision FROM response_revisions WHERE scope='progression' AND account=?),0),COALESCE((SELECT revision FROM response_revisions WHERE scope='progression' AND account=0),0)")
        .bind(account).fetch_one(db).await?)
}
