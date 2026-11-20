use crate::database::DbPool;
use crate::tables::GameTables;
use dashmap::DashMap;
use std::sync::Arc;

/// Session information stored in memory for quick access
#[derive(Clone, Debug)]
pub struct SessionInfo {
    pub account_id: i64,
    /// Last quest records delivered to this client; reserved quest fields are events.
    pub(crate) delivered_sub_quests: Arc<std::sync::Mutex<std::collections::BTreeMap<i64, serde_json::Value>>>,
    pub(crate) progression_cache: Arc<tokio::sync::Mutex<Option<crate::api::progression::cache::Snapshot>>>,
    pub(crate) equipment_revision: Arc<std::sync::Mutex<Option<(i64, usize)>>>,
    /// Section tokens for snapshots the login reply already delivered. Used
    /// only until the client acknowledges or receives that section again.
    pub(crate) login_sections: Arc<std::sync::Mutex<std::collections::BTreeMap<String, String>>>,
    /// Achievement lists recently delivered to this client, for sending only
    /// the entries that changed since the one it holds.
    pub(crate) delivered_achievements: Arc<std::sync::Mutex<crate::api::progression::notifications::DeliveredAchievements>>,
    #[allow(dead_code)]
    pub session_key: String,
    /// The DeviceId the game sent at login (empty for sessions made without one).
    pub(crate) device_id: String,
    #[allow(dead_code)]
    pub aes_key: String,
    #[allow(dead_code)]
    pub login_time: chrono::DateTime<chrono::Utc>,
    pub last_activity: chrono::DateTime<chrono::Utc>,
}

/// Application state shared across all handlers
#[derive(Clone)]
pub struct AppState {
    /// The single writer. Anything that may write, or must read inside a
    /// write transaction, uses this pool.
    pub db: DbPool,
    /// Read-only connections for standalone reads that never write. Tests and
    /// tools default this to `db`; production opens a separate reader pool.
    pub(crate) reads: DbPool,
    pub(crate) battle_service_key: Arc<Option<String>>,
    pub chat: Arc<crate::api::community::chat::ChatHub>,
    pub conquest: Arc<crate::api::battle::cooperative::BattleHub>,
    pub sessions: Arc<DashMap<String, SessionInfo>>,
    pub(crate) hero_equipment_lookup: Arc<std::sync::Mutex<crate::api::inventory::hero_level::Lookup>>,
    /// Completed arena runs' formations (community::arena::ranking_deck).
    pub(crate) registered_decks: Arc<std::sync::Mutex<std::collections::HashMap<i64, crate::api::community::RegisteredDeck>>>,
    /// Players' last main-story stages for friend profiles (community::friend::last_stages).
    pub(crate) last_stages: Arc<std::sync::Mutex<crate::api::community::friend::LastStages>>,
    #[allow(dead_code)]
    pub server_start_time: chrono::DateTime<chrono::Utc>,
    pub tables: Arc<GameTables>,
}

impl AppState {
    pub fn new(db: DbPool, tables: GameTables) -> Self {
        Self {
            reads: db.clone(),
            db,
            battle_service_key: Arc::new(std::env::var("BATTLE_SERVICE_KEY").ok().or_else(||
                std::fs::read_to_string(std::env::var("BATTLE_SERVICE_KEY_FILE").unwrap_or_else(|_|"conquest-service.key".into())).ok().map(|v|v.trim().to_owned())
            ).filter(|s|s.len()>=32)),
            chat: Arc::new(crate::api::community::chat::ChatHub::default()),
            conquest: Arc::new(crate::api::battle::cooperative::BattleHub::default()),
            sessions: Arc::new(DashMap::new()),
            hero_equipment_lookup: Arc::new(std::sync::Mutex::new(Default::default())),
            registered_decks: Arc::new(std::sync::Mutex::new(Default::default())),
            last_stages: Arc::new(std::sync::Mutex::new(Default::default())),
            server_start_time: chrono::Utc::now(),
            tables: Arc::new(tables),
        }
    }

    /// Serve standalone reads from `reads` instead of queuing on the writer.
    pub(crate) fn with_reads(mut self, reads: DbPool) -> Self {
        self.reads = reads;
        self
    }

    /// Get session by session key
    pub fn get_session(&self, session_key: &str) -> Option<SessionInfo> {
        self.sessions.get(session_key).map(|s| s.clone())
    }

    /// Create a new session
    pub fn create_session(&self, session_key: String, account_id: i64, aes_key: String) {
        self.create_device_session(session_key, account_id, aes_key, String::new());
    }

    /// A game login: one device per account. Sessions of the account on other
    /// devices end; their next request gets SessionExpired and their chat and
    /// battle sockets close. A new login on the same device keeps its earlier
    /// session working until it is no longer used.
    pub(crate) fn create_device_session(&self, session_key: String, account_id: i64, aes_key: String, device_id: String) {
        self.sessions.retain(|_, s| s.account_id != account_id || s.device_id == device_id);
        let now = chrono::Utc::now();
        let session = SessionInfo {
            account_id,
            delivered_sub_quests: Arc::new(std::sync::Mutex::new(std::collections::BTreeMap::new())),
            progression_cache: Arc::new(tokio::sync::Mutex::new(None)),
            equipment_revision: Arc::new(std::sync::Mutex::new(None)),
            login_sections: Arc::new(std::sync::Mutex::new(std::collections::BTreeMap::new())),
            delivered_achievements: Arc::new(std::sync::Mutex::new(Default::default())),
            session_key: session_key.clone(),
            device_id,
            aes_key,
            login_time: now,
            last_activity: now,
        };
        self.sessions.insert(session_key, session);
    }

    /// Update session activity
    pub fn touch_session(&self, session_key: &str) {
        if let Some(mut session) = self.sessions.get_mut(session_key) {
            session.last_activity = chrono::Utc::now();
        }
    }

    /// Remove session
    pub fn remove_session(&self, session_key: &str) {
        self.sessions.remove(session_key);
    }

    /// Get current server time as string
    pub fn server_time_str(&self) -> String {
        chrono::Utc::now().format("%Y-%m-%d %H:%M:%S").to_string()
    }

    /// Get current server time as Unix timestamp (seconds)
    pub fn server_time(&self) -> i64 {
        chrono::Utc::now().timestamp()
    }

    /// Get current server date as string (YYYY-MM-DD)
    pub fn server_date(&self) -> String {
        chrono::Utc::now().format("%Y-%m-%d").to_string()
    }

    /// Get current server UTC time as string
    pub fn server_utc_time_str(&self) -> String {
        chrono::Utc::now().format("%Y-%m-%d %H:%M:%S").to_string()
    }

}
