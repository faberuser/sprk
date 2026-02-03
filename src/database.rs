use sqlx::{sqlite::SqlitePool, Pool, Sqlite};
use std::path::Path;

pub type DbPool = Pool<Sqlite>;

/// Initialize the SQLite database with the required tables
pub async fn init_database() -> anyhow::Result<DbPool> {
    let db_path = "kings_raid.db";
    
    // Create database file if it doesn't exist
    if !Path::new(db_path).exists() {
        std::fs::File::create(db_path)?;
    }
    
    let pool = SqlitePool::connect(&format!("sqlite:{}", db_path)).await?;
    
    // Run migrations
    create_tables(&pool).await?;
    
    tracing::info!("Database initialized successfully");
    
    Ok(pool)
}

async fn create_tables(pool: &DbPool) -> anyhow::Result<()> {
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
            gold INTEGER NOT NULL DEFAULT 999999999,
            gem INTEGER NOT NULL DEFAULT 999999,
            pay_gem INTEGER NOT NULL DEFAULT 0,
            pvp_coin INTEGER NOT NULL DEFAULT 0,
            stamina INTEGER NOT NULL DEFAULT 999999,
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

    Ok(())
}
