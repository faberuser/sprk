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
    let mut tx = db.begin().await?;
    let response = issue_in(&mut tx, login_id).await?;
    tx.commit().await?;
    Ok(response)
}
async fn issue_in(db: &mut sqlx::SqliteConnection, login_id: &str) -> Result<TokenResponse> {
    let access = secret(); let refresh = secret();
    sqlx::query("DELETE FROM auth_tickets WHERE expires_at <= ?").bind(now()).execute(&mut *db).await?;
    for (token, kind, ttl) in [(&access, "access", ACCESS_SECONDS), (&refresh, "refresh", REFRESH_SECONDS)] {
        sqlx::query("INSERT INTO auth_tickets VALUES (?, ?, ?, ?)").bind(digest(token)).bind(login_id).bind(kind).bind(now()+ttl).execute(&mut *db).await?;
    }
    Ok(TokenResponse { status: "success".into(), message: String::new(), data: TokenData {
        id: login_id.into(), access_token: access, refresh_token: refresh, expires_in: ACCESS_SECONDS.to_string(),
    }})
}

#[derive(Deserialize)]
pub struct ChangePassword {
    session_key: String,
    current_password: String,
    new_password: String,
}

#[derive(Deserialize)]
pub struct DeleteAccount { session_key: String, current_password: String, confirmation: String }

pub async fn delete_account(State(state): State<AppState>, Json(body): Json<DeleteAccount>) -> Result<Json<serde_json::Value>> {
    let session=state.get_session(&body.session_key).filter(|s|s.account_id>0).ok_or(ServerError::SessionExpired)?;
    if body.confirmation!="DELETE" || body.current_password.is_empty() || body.current_password.len()>128 {
        return Err(ServerError::InvalidRequest("Enter your current password and type DELETE to permanently delete this account.".into()));
    }
    let row=sqlx::query("SELECT c.username,c.login_id,c.password_hash FROM credentials c JOIN accounts a ON a.login_id=c.login_id WHERE a.account_id=? AND a.is_banned=0")
        .bind(session.account_id).fetch_optional(&state.db).await?.ok_or_else(denied)?;
    let username:String=row.get("username");let login:String=row.get("login_id");let stored:String=row.get("password_hash");
    throttle(&state.db,&username).await?;
    let permit=PASSWORD_WORK.try_acquire().map_err(|_|ServerError::Authentication("Server busy. Please try again.".into()))?;
    let hash=stored.clone();
    let valid=tokio::task::spawn_blocking(move || PasswordHash::new(&hash).map(|h|Argon2::default().verify_password(body.current_password.as_bytes(),&h).is_ok()).unwrap_or(false))
        .await.map_err(|_|ServerError::Internal("Password service unavailable".into()))?;
    drop(permit);
    if !valid {return Err(ServerError::Authentication("Current password is incorrect.".into()));}
    let mut tx=state.db.begin().await?;
    if state.get_session(&body.session_key).is_none(){return Err(ServerError::SessionExpired);}
    // Acquire the write lock and re-check credentials before any deletion.
    let changed=sqlx::query("UPDATE credentials SET password_hash=password_hash WHERE login_id=? AND password_hash=?")
        .bind(&login).bind(&stored).execute(&mut *tx).await?.rows_affected();
    if changed!=1{return Err(denied());}
    let busy:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM guild_members WHERE account_id=?) OR EXISTS(SELECT 1 FROM guilds WHERE master_account_id=?) OR EXISTS(SELECT 1 FROM battle_room_members WHERE account=?) OR EXISTS(SELECT 1 FROM battle_rooms WHERE master=?)")
        .bind(session.account_id).bind(session.account_id).bind(session.account_id).bind(session.account_id).fetch_one(&mut *tx).await?;
    if busy{return Err(ServerError::InvalidRequest("Leave your guild and party before deleting your account.".into()));}
    // The schema is server-owned. Remove per-player rows across all content tables,
    // including features added after this endpoint; never touch shared global rows.
    sqlx::query("PRAGMA defer_foreign_keys=ON").execute(&mut *tx).await?;
    let tables:Vec<String>=sqlx::query_scalar("SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'").fetch_all(&mut *tx).await?;
    for table in tables {
        if table=="accounts" {continue;}
        let quoted=format!("\"{}\"",table.replace('"',"\"\""));
        let columns=sqlx::query(&format!("PRAGMA table_info({quoted})")).fetch_all(&mut *tx).await?;
        let mut keys:Vec<&str>=vec![];
        for column in &columns {
            let name:&str=column.get("name");
            if matches!(name,"account_id"|"account"|"friend_account_id"|"friend_id") {keys.push(name);}
            if table=="chat_messages" && matches!(name,"sender_id"|"receiver_id") {keys.push(name);}
            if matches!(table.as_str(),"community_state"|"service_replays") && name=="owner" {keys.push(name);}
            if table=="community_claims" && name=="target" {keys.push(name);}
        }
        if !keys.is_empty() {
            let clause=keys.iter().map(|k|format!("\"{k}\"=?")).collect::<Vec<_>>().join(" OR ");
            let sql=format!("DELETE FROM {quoted} WHERE {clause}");let mut query=sqlx::query(&sql);
            for _ in &keys{query=query.bind(session.account_id);}
            query.execute(&mut *tx).await?;
        }
    }
    for table in ["auth_tickets","account_claims","credentials"] {
        sqlx::query(&format!("DELETE FROM {table} WHERE login_id=?")).bind(&login).execute(&mut *tx).await?;
    }
    sqlx::query("DELETE FROM auth_attempts WHERE username=?").bind(&username).execute(&mut *tx).await?;
    // Retain only an anonymous, banned ID tombstone to prevent ID reuse and stale
    // in-flight requests from being associated with a future player.
    sqlx::query("UPDATE accounts SET login_id=?,nick='Deleted player',device_id=NULL,session_key=NULL,aes_key=NULL,country_code=NULL,is_banned=1 WHERE account_id=?")
        .bind(format!("deleted:{}",uuid::Uuid::new_v4())).bind(session.account_id).execute(&mut *tx).await?;
    tx.commit().await?;
    state.sessions.retain(|_,s|s.account_id!=session.account_id);
    Ok(Json(serde_json::json!({"status":"success"})))
}
pub async fn change_password(State(state): State<AppState>, Json(body): Json<ChangePassword>) -> Result<Json<TokenResponse>> {
    let session = state.get_session(&body.session_key).filter(|s| s.account_id > 0).ok_or(ServerError::SessionExpired)?;
    if body.current_password.is_empty() || body.current_password.len() > 128 {
        return Err(ServerError::InvalidRequest("Enter your current password.".into()));
    }
    if !(8..=128).contains(&body.new_password.len()) {
        return Err(ServerError::InvalidRequest("New password must be 8-128 bytes.".into()));
    }
    if body.current_password == body.new_password {
        return Err(ServerError::InvalidRequest("Choose a different new password.".into()));
    }
    let row = sqlx::query("SELECT c.username,c.login_id,c.password_hash FROM credentials c JOIN accounts a ON a.login_id=c.login_id WHERE a.account_id=? AND a.is_banned=0")
        .bind(session.account_id).fetch_optional(&state.db).await?.ok_or_else(denied)?;
    let username: String = row.get("username");
    let login_id: String = row.get("login_id");
    let stored: String = row.get("password_hash");
    throttle(&state.db, &username).await?;
    let permit = PASSWORD_WORK.try_acquire().map_err(|_| ServerError::Authentication("Server busy. Please try again.".into()))?;
    let previous = stored.clone();
    let hash = tokio::task::spawn_blocking(move || {
        if !PasswordHash::new(&previous).map(|h| Argon2::default().verify_password(body.current_password.as_bytes(), &h).is_ok()).unwrap_or(false) {
            return Err(ServerError::Authentication("Current password is incorrect.".into()));
        }
        Argon2::default().hash_password(body.new_password.as_bytes(), &SaltString::generate(&mut OsRng))
            .map(|h| h.to_string()).map_err(|_| ServerError::Internal("Password service unavailable".into()))
    }).await.map_err(|_| ServerError::Internal("Password service unavailable".into()))??;
    drop(permit);
    let mut tx = state.db.begin().await?;
    if state.get_session(&body.session_key).is_none() { return Err(ServerError::SessionExpired); }
    // Compare the verified hash again so two simultaneous changes cannot overwrite each other.
    let changed = sqlx::query("UPDATE credentials SET password_hash=? WHERE login_id=? AND password_hash=?")
        .bind(hash).bind(&login_id).bind(stored).execute(&mut *tx).await?.rows_affected();
    if changed != 1 { return Err(ServerError::Authentication("Password changed elsewhere. Please try again.".into())); }
    sqlx::query("DELETE FROM auth_tickets WHERE login_id=?").bind(&login_id).execute(&mut *tx).await?;
    let response = issue_in(&mut tx, &login_id).await?;
    tx.commit().await?;
    state.sessions.retain(|key, s| s.account_id != session.account_id || key == &body.session_key);
    state.touch_session(&body.session_key);
    Ok(Json(response))
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
    #[tokio::test]
    async fn delete_account_requires_password_and_confirmation_and_purges_only_target() {
        let db=sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        crate::database::create_tables(&db).await.unwrap();
        let s=AppState::new(db,crate::tables::GameTables::empty());
        let tokens=register(State(s.clone()),Json(credentials("delete_test","correct password",""))).await.unwrap().0;
        for (id,login) in [(1,tokens.data.id.as_str()),(2,"other-player")] {
            sqlx::query("INSERT INTO accounts(account_id,login_id,nick) VALUES(?,?,?)").bind(id).bind(login).bind(format!("Player{id}")).execute(&s.db).await.unwrap();
            sqlx::query("INSERT INTO user_info(account_id,gold) VALUES(?,123)").bind(id).execute(&s.db).await.unwrap();
            s.create_session(format!("session{id}"),id,String::new());
        }
        sqlx::query("INSERT INTO friends(account_id,friend_account_id) VALUES(2,1)").execute(&s.db).await.unwrap();
        let request=|password:&str,confirmation:&str|Json(DeleteAccount{session_key:"session1".into(),current_password:password.into(),confirmation:confirmation.into()});
        assert!(delete_account(State(s.clone()),request("wrong password","DELETE")).await.is_err());
        assert!(delete_account(State(s.clone()),request("correct password","no")).await.is_err());
        assert!(s.get_session("session1").is_some());
        sqlx::query("INSERT INTO guilds(guild_id,name,master_account_id) VALUES(1,'Guild',1)").execute(&s.db).await.unwrap();
        assert!(delete_account(State(s.clone()),request("correct password","DELETE")).await.is_err());
        sqlx::query("DELETE FROM guilds WHERE guild_id=1").execute(&s.db).await.unwrap();
        assert_eq!(delete_account(State(s.clone()),request("correct password","DELETE")).await.unwrap().0["status"],"success");
        assert!(s.get_session("session1").is_none());assert!(s.get_session("session2").is_some());
        for table in ["credentials","auth_tickets","friends"] {
            let count:i64=sqlx::query_scalar(&format!("SELECT COUNT(*) FROM {table}")).fetch_one(&s.db).await.unwrap();assert_eq!(count,0, "{table}");
        }
        let remaining:Vec<(i64,i64)>=sqlx::query_as("SELECT account_id,gold FROM user_info").fetch_all(&s.db).await.unwrap();assert_eq!(remaining,vec![(2,123)]);
        let tombstone:(String,i64)=sqlx::query_as("SELECT nick,is_banned FROM accounts WHERE account_id=1").fetch_one(&s.db).await.unwrap();assert_eq!(tombstone,("Deleted player".into(),1));
        assert!(password_login(State(s.clone()),Json(credentials("delete_test","correct password",""))).await.is_err());
        assert!(refresh_token(State(s.clone()),headers(&tokens.data.refresh_token)).await.is_err());
        assert!(delete_account(State(s),request("correct password","DELETE")).await.is_err());
    }
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
    fn change(session: &str, current: &str, new: &str) -> Json<ChangePassword> {
        Json(ChangePassword { session_key: session.into(), current_password: current.into(), new_password: new.into() })
    }
    #[tokio::test]
    async fn password_change_verifies_current_password_rotates_tokens_and_preserves_account() {
        let s=state().await;
        let original=register(State(s.clone()),Json(credentials("changing","original password",""))).await.unwrap().0;
        sqlx::query("INSERT INTO accounts VALUES(42,?,'Veteran',0)").bind(&original.data.id).execute(&s.db).await.unwrap();
        let other=register(State(s.clone()),Json(credentials("other","another password",""))).await.unwrap().0;
        sqlx::query("INSERT INTO accounts VALUES(43,?,'Other',0)").bind(&other.data.id).execute(&s.db).await.unwrap();
        s.create_session("current-game".into(),42,"aes".into());
        s.create_session("other-device".into(),42,"aes".into());
        s.create_session("other-account".into(),43,"aes".into());
        let before:String=sqlx::query_scalar("SELECT password_hash FROM credentials WHERE username='changing'").fetch_one(&s.db).await.unwrap();
        assert!(change_password(State(s.clone()),change("forged","original password","replacement password")).await.is_err());
        assert!(change_password(State(s.clone()),change("current-game","incorrect password","replacement password")).await.is_err());
        assert!(change_password(State(s.clone()),change("current-game","original password","short")).await.is_err());
        assert!(change_password(State(s.clone()),change("current-game","original password","original password")).await.is_err());
        let unchanged:String=sqlx::query_scalar("SELECT password_hash FROM credentials WHERE username='changing'").fetch_one(&s.db).await.unwrap();
        assert_eq!(before,unchanged);
        assert!(refresh_token(State(s.clone()),headers(&original.data.refresh_token)).await.is_ok());
        let game_ticket=verify_token(State(s.clone()),headers(&original.data.access_token)).await.unwrap().0;
        let changed=change_password(State(s.clone()),change("current-game","original password","replacement password")).await.unwrap().0;
        assert_eq!(changed.data.id,original.data.id);
        let stored:String=sqlx::query_scalar("SELECT password_hash FROM credentials WHERE username='changing'").fetch_one(&s.db).await.unwrap();
        assert_ne!(before,stored);assert!(stored.starts_with("$argon2id$"));assert!(!stored.contains("replacement password"));
        assert!(s.get_session("current-game").is_some());assert!(s.get_session("other-device").is_none());assert!(s.get_session("other-account").is_some());
        assert!(verify_token(State(s.clone()),headers(&original.data.access_token)).await.is_err());
        assert!(consume_game_ticket(&s.db,&game_ticket.data.session,&original.data.id).await.is_err());
        assert!(verify_token(State(s.clone()),headers(&changed.data.access_token)).await.is_ok());
        assert!(refresh_token(State(s.clone()),headers(&changed.data.refresh_token)).await.is_ok());
        assert!(refresh_token(State(s.clone()),headers(&other.data.refresh_token)).await.is_ok());
        assert!(password_login(State(s.clone()),Json(credentials("changing","original password",""))).await.is_err());
        let login=password_login(State(s.clone()),Json(credentials("changing","replacement password",""))).await.unwrap().0;
        assert_eq!(login.data.id,original.data.id);
        let account:(i64,String)=sqlx::query_as("SELECT account_id,nick FROM accounts WHERE login_id=?").bind(login.data.id).fetch_one(&s.db).await.unwrap();
        assert_eq!(account,(42,"Veteran".into()));
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
