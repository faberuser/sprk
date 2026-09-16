use crate::database::DbPool;
use crate::tables::GameTables;
use dashmap::DashMap;
use std::sync::Arc;

/// Session information stored in memory for quick access
#[derive(Clone, Debug)]
pub struct SessionInfo {
    pub account_id: i64,
    #[allow(dead_code)]
    pub session_key: String,
    #[allow(dead_code)]
    pub aes_key: String,
    #[allow(dead_code)]
    pub login_time: chrono::DateTime<chrono::Utc>,
    pub last_activity: chrono::DateTime<chrono::Utc>,
}

/// Application state shared across all handlers
#[derive(Clone)]
pub struct AppState {
    pub db: DbPool,
    pub chat: Arc<crate::api::community::chat::ChatHub>,
    pub sessions: Arc<DashMap<String, SessionInfo>>,
    pub auth_tokens: Arc<DashMap<String, String>>, // access_token -> device_id
    #[allow(dead_code)]
    pub server_start_time: chrono::DateTime<chrono::Utc>,
    pub tables: Arc<GameTables>,
}

impl AppState {
    pub fn new(db: DbPool, tables: GameTables) -> Self {
        Self {
            db,
            chat: Arc::new(crate::api::community::chat::ChatHub::default()),
            sessions: Arc::new(DashMap::new()),
            auth_tokens: Arc::new(DashMap::new()),
            server_start_time: chrono::Utc::now(),
            tables: Arc::new(tables),
        }
    }

    /// Get session by session key
    pub fn get_session(&self, session_key: &str) -> Option<SessionInfo> {
        self.sessions.get(session_key).map(|s| s.clone())
    }

    /// Create a new session
    pub fn create_session(&self, session_key: String, account_id: i64, aes_key: String) {
        let now = chrono::Utc::now();
        let session = SessionInfo {
            account_id,
            session_key: session_key.clone(),
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

    /// Store an auth token mapping to device_id
    pub fn store_auth_token(&self, token: &str, device_id: &str) {
        self.auth_tokens.insert(token.to_string(), device_id.to_string());
    }

    /// Get device_id from auth token
    pub fn get_auth_token(&self, token: &str) -> Option<String> {
        self.auth_tokens.get(token).map(|v| v.clone())
    }

    /// Create a simple session (for auth flow)
    pub fn create_session_simple(&self, session_key: &str, device_id: &str) {
        let now = chrono::Utc::now();
        let session = SessionInfo {
            account_id: 0, // Will be set later when actual login happens
            session_key: session_key.to_string(),
            aes_key: device_id.to_string(), // Use device_id as temp identifier
            login_time: now,
            last_activity: now,
        };
        self.sessions.insert(session_key.to_string(), session);
    }
}
