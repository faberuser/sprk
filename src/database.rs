use sqlx::{sqlite::{SqliteConnectOptions, SqliteJournalMode, SqlitePoolOptions, SqliteSynchronous}, Pool, Sqlite};
use std::{path::Path, sync::{Arc, Mutex}, time::{Duration, Instant}};

pub type DbPool = Pool<Sqlite>;

tokio::task_local! {
    /// HTTP path of the request running in this task, for attributing slow holds.
    pub(crate) static REQUEST_PATH: Arc<str>;
}

/// Holding the single connection this long delays every queued request.
const SLOW_HOLD: Duration = Duration::from_millis(250);

/// Tracks who holds the pool's only connection and since when. With one
/// connection, checkout and release strictly alternate, so one slot is exact.
#[derive(Clone, Default)]
struct HoldTracker(Arc<Mutex<Option<(Instant, Arc<str>)>>>);

impl HoldTracker {
    fn acquired(&self) {
        let holder = REQUEST_PATH.try_with(Arc::clone).unwrap_or_else(|_| Arc::from("background"));
        *self.0.lock().unwrap_or_else(|e| e.into_inner()) = Some((Instant::now(), holder));
    }
    fn released(&self) -> Option<(Arc<str>, Duration)> {
        let (since, holder) = self.0.lock().unwrap_or_else(|e| e.into_inner()).take()?;
        Some((holder, since.elapsed()))
    }
}

#[cfg(test)]
pub(crate) async fn remove_test_database(path: &Path) {
    // SQLite's worker threads/Windows scanners can retain a handle briefly after
    // Pool::close completes. Retry only sharing violations; keep other failures.
    for attempt in 0..20 {
        match std::fs::remove_file(path) {
            Ok(()) => return,
            Err(error) if cfg!(windows) && matches!(error.raw_os_error(), Some(32 | 33)) && attempt < 19 => {
                tokio::time::sleep(std::time::Duration::from_millis(25)).await;
            }
            Err(error) => panic!("Could not remove test database {}: {error}", path.display()),
        }
    }
}

/// Open the production pool as a single-writer queue.
///
/// SQLite permits one writer at a time. With several pooled connections, SQLx's
/// deferred `BEGIN` lets a transaction read first and fail its later write with
/// `SQLITE_BUSY`/`SQLITE_BUSY_SNAPSHOT` (no busy-timeout retry applies), and
/// concurrent writers poll the file lock until `busy_timeout` expires. One
/// connection makes requests wait in the pool's async acquire queue instead, so
/// in-process writers can never conflict. Code already runs on single-connection
/// pools in tests, so nested pool use while holding a transaction is not relied on.
pub(crate) async fn open_pool(path: &Path) -> anyhow::Result<DbPool> {
    open_tracked_pool(path, HoldTracker::default()).await
}

async fn open_tracked_pool(path: &Path, tracker: HoldTracker) -> anyhow::Result<DbPool> {
    let on_acquire = tracker.clone();
    Ok(SqlitePoolOptions::new()
        .max_connections(1)
        .min_connections(1)
        // Time spent queued behind other requests' transactions.
        .acquire_timeout(Duration::from_secs(30))
        .before_acquire(move |_, _| {
            on_acquire.acquired();
            Box::pin(async { Ok(true) })
        })
        .after_release(move |_, _| {
            if let Some((holder, held)) = tracker.released() {
                if held >= SLOW_HOLD {
                    tracing::warn!(holder = %holder, held_ms = held.as_millis() as u64,
                        "Database connection held long; queued requests waited behind it");
                }
            }
            Box::pin(async { Ok(true) })
        })
        .connect_with(
            SqliteConnectOptions::new()
                .filename(path)
                .create_if_missing(true)
                .journal_mode(SqliteJournalMode::Wal)
                // Safe with WAL: no corruption on crash; only an OS/power loss
                // can drop the most recent commits. Avoids an fsync per commit.
                .synchronous(SqliteSynchronous::Normal)
                // Only external processes (admin tools, backups) can hold the
                // file lock now; wait for them rather than failing requests.
                .busy_timeout(Duration::from_secs(30)),
        )
        .await?)
}

/// Initialize the SQLite database with the required tables
pub async fn init_database() -> anyhow::Result<DbPool> {
    let pool = open_pool(Path::new("sprk.db")).await?;

    // Run migrations
    create_tables(&pool).await?;
    
    tracing::info!("Database initialized successfully");
    
    Ok(pool)
}

pub(crate) async fn create_tables(pool: &DbPool) -> anyhow::Result<()> {
    crate::api::account::auth::create_tables(pool).await?;
    // Users/Accounts table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS accounts (
            account_id INTEGER PRIMARY KEY AUTOINCREMENT,
            login_id TEXT UNIQUE NOT NULL,
            login_method INTEGER NOT NULL DEFAULT 0,
            device_id TEXT,
            nick TEXT,
            created_at TEXT NOT NULL DEFAULT (datetime('now')),
            last_login TEXT,
            session_key TEXT,
            aes_key TEXT,
            country_code TEXT DEFAULT 'US',
            is_banned INTEGER NOT NULL DEFAULT 0
        )
    "#).execute(pool).await?;

    // User info table (currencies, levels, etc.)
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS user_info (
            account_id INTEGER PRIMARY KEY,
            gold INTEGER NOT NULL DEFAULT 0,
            gem INTEGER NOT NULL DEFAULT 0,
            pay_gem INTEGER NOT NULL DEFAULT 0,
            pvp_coin INTEGER NOT NULL DEFAULT 0,
            stamina INTEGER NOT NULL DEFAULT 200,
            stamina_recharge_time INTEGER NOT NULL DEFAULT 0,
            team_level INTEGER NOT NULL DEFAULT 1,
            team_exp INTEGER NOT NULL DEFAULT 0,
            sword INTEGER NOT NULL DEFAULT 5,
            sword_recharge_time INTEGER NOT NULL DEFAULT 0,
            sword2 INTEGER NOT NULL DEFAULT 0,
            avatar_hero_index INTEGER NOT NULL DEFAULT 0,
            royal_point INTEGER NOT NULL DEFAULT 0,
            raid_point INTEGER NOT NULL DEFAULT 0,
            mileage INTEGER NOT NULL DEFAULT 0,
            friendship_point INTEGER NOT NULL DEFAULT 0,
            guild_raid_ticket INTEGER NOT NULL DEFAULT 0,
            world_boss_ticket INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Heroes table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS heroes (
            unique_hero_id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            hero_id INTEGER NOT NULL,
            hero_index INTEGER NOT NULL,
            star INTEGER NOT NULL DEFAULT 2,
            level INTEGER NOT NULL DEFAULT 1,
            exp INTEGER NOT NULL DEFAULT 0,
            transcend INTEGER NOT NULL DEFAULT 0,
            awakened INTEGER NOT NULL DEFAULT 0,
            skill_level_1 INTEGER NOT NULL DEFAULT 1,
            skill_level_2 INTEGER NOT NULL DEFAULT 1,
            skill_level_3 INTEGER NOT NULL DEFAULT 1,
            skill_level_4 INTEGER NOT NULL DEFAULT 1,
            unique_weapon_id INTEGER NOT NULL DEFAULT 0,
            closeness INTEGER NOT NULL DEFAULT 0,
            is_bookmarked INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_1 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_2 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_3 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_4 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_5 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_6 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_7 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_8 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_9 INTEGER NOT NULL DEFAULT 0,
            equip_item_slot_index_10 INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, hero_id)
        )
    "#).execute(pool).await?;

    // Equipment items table - matches EquipItemInfo structure
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS equip_items (
            slot_index INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            item_index INTEGER NOT NULL,
            star INTEGER NOT NULL DEFAULT 0,
            level INTEGER NOT NULL DEFAULT 0,
            exp INTEGER NOT NULL DEFAULT 0,
            option_index_1 INTEGER NOT NULL DEFAULT 0,
            option_step_1 INTEGER NOT NULL DEFAULT 0,
            option_index_2 INTEGER NOT NULL DEFAULT 0,
            option_step_2 INTEGER NOT NULL DEFAULT 0,
            option_index_3 INTEGER NOT NULL DEFAULT 0,
            option_step_3 INTEGER NOT NULL DEFAULT 0,
            option_index_4 INTEGER NOT NULL DEFAULT 0,
            option_step_4 INTEGER NOT NULL DEFAULT 0,
            rune_slot_count INTEGER NOT NULL DEFAULT 0,
            rune_item_index_1 INTEGER NOT NULL DEFAULT 0,
            rune_item_index_2 INTEGER NOT NULL DEFAULT 0,
            rune_item_index_3 INTEGER NOT NULL DEFAULT 0,
            created_time TEXT NOT NULL DEFAULT (datetime('now')),
            upgrade_star_fail_bonus INTEGER NOT NULL DEFAULT 0,
            locked INTEGER NOT NULL DEFAULT 0,
            inventory_type INTEGER NOT NULL DEFAULT 0,
            identified INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Regular items table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS items (
            item_id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            item_index INTEGER NOT NULL,
            count INTEGER NOT NULL DEFAULT 1,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, item_index)
        )
    "#).execute(pool).await?;

    // Guild table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS guilds (
            guild_id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT UNIQUE NOT NULL,
            notice TEXT,
            level INTEGER NOT NULL DEFAULT 1,
            exp INTEGER NOT NULL DEFAULT 0,
            master_account_id INTEGER NOT NULL,
            member_count INTEGER NOT NULL DEFAULT 1,
            max_members INTEGER NOT NULL DEFAULT 30,
            created_at TEXT NOT NULL DEFAULT (datetime('now')),
            FOREIGN KEY (master_account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Guild members table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS guild_members (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL UNIQUE,
            guild_id INTEGER NOT NULL,
            role INTEGER NOT NULL DEFAULT 0,
            contribution INTEGER NOT NULL DEFAULT 0,
            joined_at TEXT NOT NULL DEFAULT (datetime('now')),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            FOREIGN KEY (guild_id) REFERENCES guilds(guild_id)
        )
    "#).execute(pool).await?;

    // Mail table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS mails (
            mail_id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            sender TEXT NOT NULL DEFAULT 'System',
            title TEXT NOT NULL,
            content TEXT,
            reward_gold INTEGER DEFAULT 0,
            reward_gem INTEGER DEFAULT 0,
            reward_items TEXT,
            is_read INTEGER NOT NULL DEFAULT 0,
            is_received INTEGER NOT NULL DEFAULT 0,
            created_at TEXT NOT NULL DEFAULT (datetime('now')),
            expires_at TEXT,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Friend table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS friends (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            friend_account_id INTEGER NOT NULL,
            status TEXT NOT NULL DEFAULT 'pending',
            created_at TEXT NOT NULL DEFAULT (datetime('now')),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            FOREIGN KEY (friend_account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, friend_account_id)
        )
    "#).execute(pool).await?;

    // Campaign progress table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS campaign_progress (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            chapter_id INTEGER NOT NULL,
            dungeon_id INTEGER NOT NULL,
            clear_count INTEGER NOT NULL DEFAULT 0,
            best_star INTEGER NOT NULL DEFAULT 0,
            is_unlocked INTEGER NOT NULL DEFAULT 0,
            completed_time TEXT,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, chapter_id, dungeon_id)
        )
    "#).execute(pool).await?;

    // Tutorial progress - stores completed tutorial indices
    // tutorial_index matches the Index field in TutorialTable.json
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS tutorial_progress (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            tutorial_index INTEGER NOT NULL,
            is_completed INTEGER NOT NULL DEFAULT 0,
            completed_time TEXT,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, tutorial_index)
        )
    "#).execute(pool).await?;

    // Attendance/Daily login
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS attendance (
            account_id INTEGER PRIMARY KEY,
            current_day INTEGER NOT NULL DEFAULT 0,
            total_days INTEGER NOT NULL DEFAULT 0,
            last_attendance TEXT NOT NULL DEFAULT '',
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Achievements table
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS achievements (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            achievement_id INTEGER NOT NULL,
            category INTEGER NOT NULL DEFAULT 0,
            current_value INTEGER NOT NULL DEFAULT 0,
            target_value INTEGER NOT NULL DEFAULT 0,
            is_completed INTEGER NOT NULL DEFAULT 0,
            is_rewarded INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, achievement_id)
        )
    "#).execute(pool).await?;

    // Sessions table for managing active sessions
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS sessions (
            session_id TEXT PRIMARY KEY,
            account_id INTEGER NOT NULL,
            aes_key TEXT NOT NULL,
            created_at TEXT NOT NULL DEFAULT (datetime('now')),
            last_activity TEXT NOT NULL DEFAULT (datetime('now')),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Hero Inn - Hero Friendly state table for tracking hero recruitment progress
    // This tracks the player's progress with heroes in the Hero's Inn
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS hero_friendly_state (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            account_id INTEGER NOT NULL,
            hero_index INTEGER NOT NULL,
            friendly_point INTEGER NOT NULL DEFAULT 0,
            step INTEGER NOT NULL DEFAULT 0,
            action_reset_count INTEGER NOT NULL DEFAULT 0,
            last_greeting_time TEXT,
            last_conversation_time TEXT,
            last_gift_time TEXT,
            is_lock INTEGER NOT NULL DEFAULT 0,
            selected_time TEXT,
            visit_period INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id),
            UNIQUE(account_id, hero_index)
        )
    "#).execute(pool).await?;

    // Hero Inn - Player's current hero inn session
    // Tracks which heroes are currently available in the inn
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS hero_friendly_info (
            account_id INTEGER PRIMARY KEY,
            hero_index INTEGER NOT NULL DEFAULT 0,
            selected_hero_index INTEGER NOT NULL DEFAULT 0,
            friendly_point INTEGER NOT NULL DEFAULT 0,
            last_greeting_time TEXT,
            last_conversation_time TEXT,
            last_gift_time TEXT,
            selected_time TEXT,
            selected_hero_indices TEXT,
            last_roulette_time TEXT,
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Hero Inn Roulette - Track daily spin counts
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS hero_inn_roulette_spins (
            account_id INTEGER NOT NULL,
            spin_date TEXT NOT NULL,
            spin_count INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (account_id, spin_date),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Inventory - Track consumable items
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS inventory (
            account_id INTEGER NOT NULL,
            item_code INTEGER NOT NULL,
            count INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (account_id, item_code),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Shop Purchases - Track shop item purchase counts
    sqlx::query(r#"
        CREATE TABLE IF NOT EXISTS shop_purchases (
            account_id INTEGER NOT NULL,
            shop_index INTEGER NOT NULL,
            item_index INTEGER NOT NULL,
            purchase_count INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY (account_id, shop_index, item_index),
            FOREIGN KEY (account_id) REFERENCES accounts(account_id)
        )
    "#).execute(pool).await?;

    // Existing accounts used the old forced-skip flow. Preserve that preference;
    // login explicitly opts newly created accounts into the tutorial.
    sqlx::query("CREATE TABLE IF NOT EXISTS tutorial_settings (account_id INTEGER PRIMARY KEY, is_skipped INTEGER NOT NULL DEFAULT 0, FOREIGN KEY (account_id) REFERENCES accounts(account_id))")
        .execute(pool).await?;
    sqlx::query("INSERT OR IGNORE INTO tutorial_settings (account_id, is_skipped) SELECT account_id, 1 FROM accounts")
        .execute(pool).await?;

    // Additive migrations also work with databases created by earlier builds.
    for (table, column, definition) in [
        ("accounts", "last_logout", "TEXT"),
        ("tutorial_progress", "completion_response", "TEXT"),
        ("user_info", "event_dungeon_point", "INTEGER NOT NULL DEFAULT 0"),
    ] {
        use sqlx::Row;
        let columns = sqlx::query(&format!("PRAGMA table_info({table})")).fetch_all(pool).await?;
        if !columns.iter().any(|row| row.get::<String, _>("name") == column) {
            sqlx::query(&format!("ALTER TABLE {table} ADD COLUMN {column} {definition}"))
                .execute(pool).await?;
        }
    }

    let columns = sqlx::query("PRAGMA table_info(equip_items)").fetch_all(pool).await?;
    for slot in 1..=4 {
        use sqlx::Row;
        for prefix in ["option_renew_count", "is_renewed_option"] {
            let column = format!("{prefix}_{slot}");
            if !columns.iter().any(|row| row.get::<String, _>("name") == column) {
                sqlx::query(&format!("ALTER TABLE equip_items ADD COLUMN {column} INTEGER NOT NULL DEFAULT 0"))
                    .execute(pool).await?;
            }
        }
    }

    for statement in [
        "CREATE TABLE IF NOT EXISTS friend_points (account_id INTEGER NOT NULL, friend_id INTEGER NOT NULL, last_send_time TEXT, last_recv_time TEXT, PRIMARY KEY(account_id, friend_id))",
        "CREATE TABLE IF NOT EXISTS friend_daily (account_id INTEGER PRIMARY KEY, day TEXT NOT NULL, points INTEGER NOT NULL DEFAULT 0, removed INTEGER NOT NULL DEFAULT 0)",
        "CREATE TABLE IF NOT EXISTS global_mails (global_id INTEGER PRIMARY KEY AUTOINCREMENT, sender TEXT NOT NULL DEFAULT 'System', title TEXT NOT NULL, content TEXT NOT NULL DEFAULT '', reward_gold INTEGER NOT NULL DEFAULT 0, reward_gem INTEGER NOT NULL DEFAULT 0, reward_items TEXT, reward_stamina INTEGER NOT NULL DEFAULT 0, reward_equipment TEXT, created_at TEXT NOT NULL DEFAULT (datetime('now')), expires_at TEXT, opens_at TEXT)",
        "CREATE TABLE IF NOT EXISTS global_mail_deliveries (account_id INTEGER NOT NULL, global_id INTEGER NOT NULL, mail_id INTEGER NOT NULL, PRIMARY KEY(account_id, global_id))",
        "CREATE TABLE IF NOT EXISTS chat_messages (message_id INTEGER PRIMARY KEY AUTOINCREMENT, sender_id INTEGER NOT NULL, group_type TEXT NOT NULL, channel INTEGER NOT NULL DEFAULT 1, guild_id INTEGER NOT NULL DEFAULT 0, receiver_id INTEGER NOT NULL DEFAULT 0, protocol TEXT NOT NULL, content TEXT NOT NULL, created_at TEXT NOT NULL DEFAULT (datetime('now')))",
        "CREATE INDEX IF NOT EXISTS chat_channel_history ON chat_messages(group_type, channel, message_id)",
        "CREATE INDEX IF NOT EXISTS chat_whisper_history ON chat_messages(receiver_id, sender_id, message_id)",
        "CREATE INDEX IF NOT EXISTS mail_inbox ON mails(account_id, is_received, mail_id)",
    ] { sqlx::query(statement).execute(pool).await?; }
    for (column, definition) in [
        ("read_time", "TEXT"), ("received_time", "TEXT"), ("opens_at", "TEXT"),
        ("reward_stamina", "INTEGER NOT NULL DEFAULT 0"), ("reward_equipment", "TEXT"),
        ("global_id", "INTEGER"), ("receipt", "TEXT"), ("reward_currencies", "TEXT"),
    ] {
        use sqlx::Row;
        let columns = sqlx::query("PRAGMA table_info(mails)").fetch_all(pool).await?;
        if !columns.iter().any(|row| row.get::<String, _>("name") == column) {
            sqlx::query(&format!("ALTER TABLE mails ADD COLUMN {column} {definition}")).execute(pool).await?;
        }
    }

    // Every mail writer participates automatically. Trigger inserts commit or
    // roll back with the reward transaction; sockets only read committed rows.
    for statement in [
        "CREATE TABLE IF NOT EXISTS mail_notifications(mail_id INTEGER PRIMARY KEY,account_id INTEGER NOT NULL,is_global INTEGER NOT NULL)",
        "CREATE TRIGGER IF NOT EXISTS mail_arrival_notification AFTER INSERT ON mails BEGIN INSERT INTO mail_notifications(mail_id,account_id,is_global) VALUES(NEW.mail_id,NEW.account_id,NEW.global_id IS NOT NULL); END",
    ] { sqlx::query(statement).execute(pool).await?; }

    for statement in [
        "CREATE TABLE IF NOT EXISTS inventory_settings(account_id INTEGER PRIMARY KEY, inventory_extend INTEGER NOT NULL DEFAULT 0, chest_extend INTEGER NOT NULL DEFAULT 0)",
        "CREATE TABLE IF NOT EXISTS inventory_ticket_redemptions(account_id INTEGER NOT NULL,token TEXT NOT NULL,request TEXT NOT NULL,response TEXT NOT NULL,PRIMARY KEY(account_id,token))",
        "CREATE TABLE IF NOT EXISTS craft_slots(account_id INTEGER NOT NULL, slot_index INTEGER NOT NULL, craft_index INTEGER NOT NULL DEFAULT 0, item_index INTEGER NOT NULL DEFAULT 0, item_count INTEGER NOT NULL DEFAULT 0, complete_time INTEGER NOT NULL DEFAULT 0, paid_gold INTEGER NOT NULL DEFAULT 0, materials TEXT NOT NULL DEFAULT '[]', PRIMARY KEY(account_id,slot_index))",
        "CREATE TABLE IF NOT EXISTS item_boosters(account_id INTEGER NOT NULL, item_index INTEGER NOT NULL, start_time TEXT NOT NULL, end_time TEXT NOT NULL, PRIMARY KEY(account_id,item_index))",
    ] { sqlx::query(statement).execute(pool).await?; }
    for (column,definition) in [("locked","INTEGER NOT NULL DEFAULT 0"),("created_time","TEXT")] {
        use sqlx::Row;
        let columns=sqlx::query("PRAGMA table_info(items)").fetch_all(pool).await?;
        if !columns.iter().any(|r|r.get::<String,_>("name")==column) {
            sqlx::query(&format!("ALTER TABLE items ADD COLUMN {column} {definition}")).execute(pool).await?;
        }
    }

    for statement in [
        "CREATE TABLE IF NOT EXISTS hero_presets(account_id INTEGER NOT NULL,storage_key TEXT NOT NULL,name TEXT NOT NULL DEFAULT '',data TEXT NOT NULL DEFAULT '[]',PRIMARY KEY(account_id,storage_key))",
        "CREATE TABLE IF NOT EXISTS hero_details(account_id INTEGER NOT NULL,hero_index INTEGER NOT NULL,data TEXT NOT NULL DEFAULT '{}',PRIMARY KEY(account_id,hero_index))",
        "CREATE TABLE IF NOT EXISTS costumes(account_id INTEGER NOT NULL,costume_index INTEGER NOT NULL,created_time TEXT NOT NULL DEFAULT (datetime('now')),PRIMARY KEY(account_id,costume_index))",
        "CREATE TABLE IF NOT EXISTS costume_presets(account_id INTEGER NOT NULL,hero_index INTEGER NOT NULL,slot_index INTEGER NOT NULL,data TEXT NOT NULL,PRIMARY KEY(account_id,hero_index,slot_index))",
        "CREATE TABLE IF NOT EXISTS lobby_appearance(account_id INTEGER PRIMARY KEY,avatar_hero_index INTEGER NOT NULL,costume_index INTEGER NOT NULL,hair_costume_index INTEGER NOT NULL,weapon_costume_index INTEGER NOT NULL,hide_unique_weapon INTEGER NOT NULL,snapshot TEXT)",
        "CREATE TABLE IF NOT EXISTS shop_stock(account_id INTEGER NOT NULL,shop_index INTEGER NOT NULL,revision INTEGER NOT NULL DEFAULT 1,restock_time INTEGER NOT NULL DEFAULT 0,restock_count INTEGER NOT NULL DEFAULT 0,stock TEXT NOT NULL DEFAULT '[]',PRIMARY KEY(account_id,shop_index))",
        "CREATE TABLE IF NOT EXISTS shop_purchase_ledger(account_id INTEGER NOT NULL,shop_index INTEGER NOT NULL,item_index INTEGER NOT NULL,period TEXT NOT NULL,purchased INTEGER NOT NULL DEFAULT 0,purchased_time INTEGER NOT NULL DEFAULT 0,PRIMARY KEY(account_id,shop_index,item_index,period))",
    ] { sqlx::query(statement).execute(pool).await?; }

    // Upgrade the earlier cosmetic-only lobby snapshot without losing it.
    let columns = sqlx::query("PRAGMA table_info(lobby_appearance)").fetch_all(pool).await?;
    if !columns.iter().any(|row| {
        use sqlx::Row;
        row.get::<String, _>("name") == "snapshot"
    }) {
        sqlx::query("ALTER TABLE lobby_appearance ADD COLUMN snapshot TEXT").execute(pool).await?;
    }

    crate::api::progression::schema::migrate(pool).await?;
    crate::api::extensions::schema::migrate(pool).await?;
    crate::api::battle::migrate(pool).await?;
    crate::api::services::migrate(pool).await?;
    crate::api::community::migrate(pool).await?;
    crate::api::system::storage::migrate(pool).await?;
    crate::api::progression::play_record::migrate(pool).await?;
    for query in [
        "CREATE INDEX IF NOT EXISTS heroes_account_index ON heroes(account_id,hero_index)",
        "CREATE INDEX IF NOT EXISTS equip_account_inventory ON equip_items(account_id,inventory_type,slot_index)",
        "CREATE INDEX IF NOT EXISTS friends_incoming ON friends(friend_account_id,status)",
    ] { sqlx::query(query).execute(pool).await?; }
    crate::api::system::revisions::migrate(pool).await?;
    crate::currency::migrate(pool).await?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn hold_tracker_attributes_connection_time_to_the_request_path() {
        let path = std::env::temp_dir().join(format!("sprk-db-hold-{}.sqlite", uuid::Uuid::new_v4()));
        let tracker = HoldTracker::default();
        let pool = open_tracked_pool(&path, tracker.clone()).await.unwrap();
        // Simulate a slow handler holding the only connection.
        REQUEST_PATH.scope(Arc::from("/test/slow"), async {
            let conn = pool.acquire().await.unwrap();
            let (_, holder) = tracker.0.lock().unwrap().clone().expect("checkout recorded");
            assert_eq!(&*holder, "/test/slow");
            tokio::time::sleep(Duration::from_millis(30)).await;
            drop(conn);
        }).await;
        // Release runs on a pool task; it consumes the record. Re-arm a probe:
        // a fresh checkout from a background task must be labelled as such.
        let conn = pool.acquire().await.unwrap();
        let (_, holder) = tracker.0.lock().unwrap().clone().expect("checkout recorded");
        assert_eq!(&*holder, "background");
        drop(conn);
        pool.close().await;
        // Measured hold duration, on a tracker independent of the pool's
        // asynchronous release task.
        let direct = HoldTracker::default();
        REQUEST_PATH.scope(Arc::from("/test/slow"), async { direct.acquired() }).await;
        tokio::time::sleep(Duration::from_millis(30)).await;
        let (holder, held) = direct.released().unwrap();
        assert_eq!((&*holder, held >= Duration::from_millis(30)), ("/test/slow", true));
        assert!(direct.released().is_none(), "a release without checkout must not report");
        remove_test_database(&path).await;
    }

    // Reproduces the production "database is locked" pattern: deferred
    // transactions that read, yield to other requests, then write.
    #[tokio::test]
    async fn production_pool_queues_concurrent_read_then_write_transactions() {
        let path = std::env::temp_dir().join(format!("sprk-db-queue-{}.sqlite", uuid::Uuid::new_v4()));
        let pool = open_pool(&path).await.unwrap();
        let mode: String = sqlx::query_scalar("PRAGMA journal_mode").fetch_one(&pool).await.unwrap();
        let sync: i64 = sqlx::query_scalar("PRAGMA synchronous").fetch_one(&pool).await.unwrap();
        assert_eq!((mode.as_str(), sync), ("wal", 1));
        sqlx::query("CREATE TABLE counter(id INTEGER PRIMARY KEY, value INTEGER NOT NULL)").execute(&pool).await.unwrap();
        sqlx::query("INSERT INTO counter VALUES (1, 0)").execute(&pool).await.unwrap();
        let tasks: Vec<_> = (0..40).map(|_| {
            let pool = pool.clone();
            tokio::spawn(async move {
                let mut tx = pool.begin().await?;
                let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&mut *tx).await?;
                tokio::time::sleep(Duration::from_millis(2)).await;
                sqlx::query("UPDATE counter SET value=? WHERE id=1").bind(value + 1).execute(&mut *tx).await?;
                tx.commit().await
            })
        }).collect();
        for task in tasks { task.await.unwrap().expect("transaction must queue, not fail with SQLITE_BUSY"); }
        let value: i64 = sqlx::query_scalar("SELECT value FROM counter WHERE id=1").fetch_one(&pool).await.unwrap();
        assert_eq!(value, 40, "queued transactions must not lose updates");
        pool.close().await;
        remove_test_database(&path).await;
    }
}
