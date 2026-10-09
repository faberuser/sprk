//! Password accounts and expiring, server-validated authentication tickets.
use axum::{extract::State, http::HeaderMap, Json};
use argon2::{Argon2, PasswordHash, PasswordHasher, PasswordVerifier, password_hash::{rand_core::OsRng, SaltString}};
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use sqlx::Row;
use crate::{database::DbPool, error::{Result, ServerError}, state::AppState};
use once_cell::sync::Lazy;
use tokio::sync::Semaphore;

// Bound expensive password work independently of the HTTP worker threads.
static PASSWORD_WORK: Lazy<Semaphore> = Lazy::new(|| Semaphore::new(4));
const ACCESS_SECONDS: i64 = 3600;
const REFRESH_SECONDS: i64 = 30 * 86400;

pub(crate) async fn create_tables(db: &DbPool) -> std::result::Result<(), sqlx::Error> {
    for query in [
        "CREATE TABLE IF NOT EXISTS credentials (username TEXT PRIMARY KEY, login_id TEXT NOT NULL UNIQUE, password_hash TEXT NOT NULL, created_at INTEGER NOT NULL)",
        "CREATE TABLE IF NOT EXISTS auth_tickets (token_hash TEXT PRIMARY KEY, login_id TEXT NOT NULL, kind TEXT NOT NULL, expires_at INTEGER NOT NULL)",
        "CREATE TABLE IF NOT EXISTS account_claims (code_hash TEXT PRIMARY KEY, login_id TEXT NOT NULL UNIQUE, expires_at INTEGER NOT NULL)",
        "CREATE TABLE IF NOT EXISTS auth_attempts (username TEXT PRIMARY KEY, attempts INTEGER NOT NULL, expires_at INTEGER NOT NULL)",
        "CREATE INDEX IF NOT EXISTS auth_ticket_expiry ON auth_tickets(expires_at)",
    ] { sqlx::query(query).execute(db).await?; }
    Ok(())
}
fn now() -> i64 { chrono::Utc::now().timestamp() }
pub(crate) fn digest(value: &str) -> String { hex::encode(Sha256::digest(value.as_bytes())) }
fn secret() -> String { format!("{}{}", uuid::Uuid::new_v4().simple(), uuid::Uuid::new_v4().simple()) }
fn denied() -> ServerError { ServerError::Authentication("Invalid or expired credentials. Please log in again.".into()) }

#[derive(Deserialize)]
pub struct Credentials {
    username: String,
    password: String,
    #[serde(default)]
    claim_code: String,
}
fn validate(body: &Credentials) -> Result<String> {
    let username = body.username.trim().to_ascii_lowercase();
    if !(3..=32).contains(&username.len()) || !username.bytes().all(|c| c.is_ascii_alphanumeric() || c == b'_' || c == b'-') {
        return Err(ServerError::InvalidRequest("Username must be 3-32 letters, numbers, underscores or hyphens.".into()));
    }
    if !(8..=128).contains(&body.password.len()) {
        return Err(ServerError::InvalidRequest("Password must be 8-128 bytes.".into()));
    }
    if body.claim_code.len() > 128 { return Err(ServerError::InvalidRequest("Invalid claim code.".into())); }
    Ok(username)
}
async fn throttle(db: &DbPool, username: &str) -> Result<()> {
    sqlx::query("DELETE FROM auth_attempts WHERE expires_at <= ?").bind(now()).execute(db).await?;
    // Include a global budget so random usernames cannot grow this table without bound.
    for key in ["*", username] {
        let count: i64 = sqlx::query_scalar("INSERT INTO auth_attempts(username, attempts, expires_at) VALUES (?, 1, ?) ON CONFLICT(username) DO UPDATE SET attempts=attempts+1 RETURNING attempts")
            .bind(key).bind(now()+60).fetch_one(db).await?;
        if count > if key == "*" { 120 } else { 10 } {
            return Err(ServerError::Authentication("Too many attempts. Wait a minute and try again.".into()));
        }
    }
    Ok(())
}

#[derive(Serialize)]
pub struct TokenData { pub id: String, pub access_token: String, pub refresh_token: String, pub expires_in: String }
#[derive(Serialize)]
pub struct TokenResponse { pub status: String, pub message: String, pub data: TokenData }
async fn issue(db: &DbPool, login_id: &str) -> Result<TokenResponse> {
    let access = secret(); let refresh = secret();
    let mut tx = db.begin().await?;
    sqlx::query("DELETE FROM auth_tickets WHERE expires_at <= ?").bind(now()).execute(&mut *tx).await?;
    for (token, kind, ttl) in [(&access, "access", ACCESS_SECONDS), (&refresh, "refresh", REFRESH_SECONDS)] {
        sqlx::query("INSERT INTO auth_tickets VALUES (?, ?, ?, ?)").bind(digest(token)).bind(login_id).bind(kind).bind(now()+ttl).execute(&mut *tx).await?;
    }
    tx.commit().await?;
    Ok(TokenResponse { status: "success".into(), message: String::new(), data: TokenData {
        id: login_id.into(), access_token: access, refresh_token: refresh, expires_in: ACCESS_SECONDS.to_string(),
    }})
}

pub async fn register(State(state): State<AppState>, Json(body): Json<Credentials>) -> Result<Json<TokenResponse>> {
    let username = validate(&body)?;
    throttle(&state.db, &username).await?;
    let permit = PASSWORD_WORK.try_acquire().map_err(|_| ServerError::Authentication("Server busy. Please try again.".into()))?;
    let hash = tokio::task::spawn_blocking(move || Argon2::default().hash_password(body.password.as_bytes(), &SaltString::generate(&mut OsRng)).map(|h| h.to_string()))
        .await.map_err(|_| ServerError::Internal("Password service unavailable".into()))?
        .map_err(|_| ServerError::Internal("Password service unavailable".into()))?;
    drop(permit);
    let mut tx = state.db.begin().await?;
    let login_id = if body.claim_code.trim().is_empty() { format!("account_{}", uuid::Uuid::new_v4().simple()) } else {
        sqlx::query_scalar::<_, String>("DELETE FROM account_claims WHERE code_hash=? AND expires_at>? RETURNING login_id")
            .bind(digest(body.claim_code.trim())).bind(now()).fetch_optional(&mut *tx).await?
            .ok_or_else(|| ServerError::InvalidRequest("Invalid or expired save claim code.".into()))?
    };
    let inserted = sqlx::query("INSERT INTO credentials VALUES (?, ?, ?, ?) ON CONFLICT DO NOTHING")
        .bind(&username).bind(&login_id).bind(hash).bind(now()).execute(&mut *tx).await?;
    if inserted.rows_affected() != 1 {
        return Err(ServerError::InvalidRequest("Username or save already registered.".into()));
    }
    // Existing save rows remain unchanged. New game data is created by the normal game login.
    tx.commit().await?;
    Ok(Json(issue(&state.db, &login_id).await?))
}

pub async fn password_login(State(state): State<AppState>, Json(body): Json<Credentials>) -> Result<Json<TokenResponse>> {
    let username = validate(&body)?;
    throttle(&state.db, &username).await?;
    let row = sqlx::query("SELECT login_id, password_hash FROM credentials WHERE username=?").bind(username).fetch_optional(&state.db).await?;
    let permit = PASSWORD_WORK.try_acquire().map_err(|_| ServerError::Authentication("Server busy. Please try again.".into()))?;
    let (login_id, stored) = row.map(|r| (r.get::<String,_>("login_id"), r.get::<String,_>("password_hash"))).unwrap_or_default();
    let valid = tokio::task::spawn_blocking(move || {
        if stored.is_empty() {
            // Do comparable work for unknown users.
            let _ = Argon2::default().hash_password(body.password.as_bytes(), &SaltString::generate(&mut OsRng));
            false
        } else {
            PasswordHash::new(&stored).map(|h| Argon2::default().verify_password(body.password.as_bytes(), &h).is_ok()).unwrap_or(false)
        }
    }).await.map_err(|_| ServerError::Internal("Password service unavailable".into()))?;
    drop(permit);
    if !valid { return Err(denied()); }
    Ok(Json(issue(&state.db, &login_id).await?))
}

pub async fn guest_login() -> Result<Json<serde_json::Value>> {
    Err(ServerError::Authentication("Guest login has been replaced. Please register or log in with your username and password.".into()))
}
fn bearer(headers: &HeaderMap) -> Result<&str> {
    headers.get("Authorization").and_then(|v| v.to_str().ok()).and_then(|v| v.strip_prefix("Bearer ")).filter(|v| !v.is_empty()).ok_or_else(denied)
}
#[derive(Serialize)]
pub struct SessionData { id: String, session: String }
#[derive(Serialize)]
pub struct VerifyResponse { status: String, message: String, data: SessionData }
pub async fn verify_token(State(state): State<AppState>, headers: HeaderMap) -> Result<Json<VerifyResponse>> {
    let id: String = sqlx::query_scalar("SELECT login_id FROM auth_tickets WHERE token_hash=? AND kind='access' AND expires_at>?")
        .bind(digest(bearer(&headers)?)).bind(now()).fetch_optional(&state.db).await?.ok_or_else(denied)?;
    let session = secret();
    sqlx::query("INSERT INTO auth_tickets VALUES (?, ?, 'game', ?)").bind(digest(&session)).bind(&id).bind(now()+300).execute(&state.db).await?;
    Ok(Json(VerifyResponse { status: "success".into(), message: String::new(), data: SessionData { id, session } }))
}
pub async fn refresh_token(State(state): State<AppState>, headers: HeaderMap) -> Result<Json<TokenResponse>> {
    let id: String = sqlx::query_scalar("DELETE FROM auth_tickets WHERE token_hash=? AND kind='refresh' AND expires_at>? RETURNING login_id")
        .bind(digest(bearer(&headers)?)).bind(now()).fetch_optional(&state.db).await?.ok_or_else(denied)?;
    Ok(Json(issue(&state.db, &id).await?))
}
/// The game login must prove ownership; a LoginId or device ID alone grants nothing.
pub(crate) async fn consume_game_ticket(db: &DbPool, ticket: &str, login_id: &str) -> Result<()> {
    let result = sqlx::query("DELETE FROM auth_tickets WHERE token_hash=? AND login_id=? AND kind='game' AND expires_at>?")
        .bind(digest(ticket)).bind(login_id).bind(now()).execute(db).await?;
    if result.rows_affected() != 1 { return Err(denied()); }
    let banned: Option<i64> = sqlx::query_scalar("SELECT is_banned FROM accounts WHERE login_id=?").bind(login_id).fetch_optional(db).await?;
    if banned == Some(1) { return Err(denied()); }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use axum::{body::Bytes, http::HeaderValue};
    async fn state() -> AppState {
        let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        create_tables(&db).await.unwrap();
        sqlx::query("CREATE TABLE accounts(account_id INTEGER PRIMARY KEY, login_id TEXT UNIQUE, nick TEXT, is_banned INTEGER DEFAULT 0)").execute(&db).await.unwrap();
        AppState::new(db, crate::tables::GameTables::empty())
    }
    fn credentials(name: &str, password: &str, claim: &str) -> Credentials {
        Credentials { username: name.into(), password: password.into(), claim_code: claim.into() }
    }
    fn headers(token: &str) -> HeaderMap {
        let mut h=HeaderMap::new();h.insert("Authorization",HeaderValue::from_str(&format!("Bearer {}",token)).unwrap());h
    }
    #[tokio::test]
    async fn registration_login_tickets_and_refresh() {
        let s=state().await;
        let registered=register(State(s.clone()),Json(credentials("Raider_One","correct password",""))).await.unwrap().0;
        let stored:String=sqlx::query_scalar("SELECT password_hash FROM credentials").fetch_one(&s.db).await.unwrap();
        assert!(stored.starts_with("$argon2id$"));assert!(!stored.contains("correct password"));
        assert!(password_login(State(s.clone()),Json(credentials("raider_one","wrong password",""))).await.is_err());
        let logged=password_login(State(s.clone()),Json(credentials("RAIDER_ONE","correct password",""))).await.unwrap().0;
        assert_eq!(registered.data.id,logged.data.id);
        assert!(verify_token(State(s.clone()),headers("bogus")).await.is_err());
        let verified=verify_token(State(s.clone()),headers(&logged.data.access_token)).await.unwrap().0;
        assert!(consume_game_ticket(&s.db,&verified.data.session,"someone_else").await.is_err());
        consume_game_ticket(&s.db,&verified.data.session,&logged.data.id).await.unwrap();
        assert!(consume_game_ticket(&s.db,&verified.data.session,&logged.data.id).await.is_err());
        assert!(refresh_token(State(s.clone()),headers("bogus")).await.is_err());
        let _ = refresh_token(State(s.clone()),headers(&logged.data.refresh_token)).await.unwrap();
        assert!(refresh_token(State(s.clone()),headers(&logged.data.refresh_token)).await.is_err());
        sqlx::query("UPDATE auth_tickets SET expires_at=0").execute(&s.db).await.unwrap();
        assert!(verify_token(State(s.clone()),headers(&registered.data.access_token)).await.is_err());
        assert!(guest_login().await.is_err());
        assert!(super::super::user::login(State(s),Bytes::from("LoginId=someone_else&DeviceId=trusted-device")).await.is_err());
    }
    #[tokio::test]
    async fn claim_preserves_save_and_is_atomic() {
        let s=state().await;
        sqlx::query("INSERT INTO accounts VALUES(42,'legacy-device','Veteran',0)").execute(&s.db).await.unwrap();
        sqlx::query("INSERT INTO account_claims VALUES(?,'legacy-device',?)").bind(digest("secret-claim")).bind(now()+60).execute(&s.db).await.unwrap();
        let _ = register(State(s.clone()),Json(credentials("taken","password one",""))).await.unwrap();
        assert!(register(State(s.clone()),Json(credentials("taken","password two","secret-claim"))).await.is_err());
        let result=register(State(s.clone()),Json(credentials("veteran","password two","secret-claim"))).await.unwrap().0;
        assert_eq!(result.data.id,"legacy-device");
        let id:i64=sqlx::query_scalar("SELECT account_id FROM accounts WHERE login_id=?").bind(&result.data.id).fetch_one(&s.db).await.unwrap();
        assert_eq!(id,42);
        assert!(register(State(s.clone()),Json(credentials("thief","password three","secret-claim"))).await.is_err());
        let valid=verify_token(State(s.clone()),headers(&result.data.access_token)).await.unwrap().0;
        sqlx::query("UPDATE accounts SET is_banned=1").execute(&s.db).await.unwrap();
        assert!(consume_game_ticket(&s.db,&valid.data.session,&result.data.id).await.is_err());
    }
    #[tokio::test]
    async fn validation_and_attempt_limits() {
        let s=state().await;
        assert!(validate(&credentials("ab","password","" )).is_err());
        assert!(validate(&credentials("valid","short","" )).is_err());
        assert!(validate(&credentials("invalid@name","password","" )).is_err());
        for _ in 0..10 {throttle(&s.db,"limited").await.unwrap();}
        assert!(throttle(&s.db,"limited").await.is_err());
        sqlx::query("UPDATE auth_attempts SET expires_at=0").execute(&s.db).await.unwrap();
        throttle(&s.db,"limited").await.unwrap();
    }
}
