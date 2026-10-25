//! Optional authenticated rollout receipts. No paths, credentials or hardware IDs.
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use crate::{error::{Result,ServerError},state::AppState};
use super::request::Request;

pub(crate) async fn migrate(db: &sqlx::SqlitePool) -> Result<()> {
    sqlx::query("CREATE TABLE IF NOT EXISTS client_storage_reports(account_id INTEGER NOT NULL,installation TEXT NOT NULL,schema_version INTEGER NOT NULL,source_present INTEGER NOT NULL,files_copied INTEGER NOT NULL,preferences_copied INTEGER NOT NULL,first_seen TEXT NOT NULL DEFAULT (datetime('now')),last_seen TEXT NOT NULL DEFAULT (datetime('now')),PRIMARY KEY(account_id,installation),FOREIGN KEY(account_id) REFERENCES accounts(account_id) ON DELETE CASCADE)").execute(db).await?;
    Ok(())
}

pub async fn report(State(s): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let r=Request::parse(&body)?;let account=r.account(&s)?;
    let installation=r.text("Installation");
    let schema=r.number("Schema",0)?;let files=r.number("Files",-1)?;let prefs=r.number("Preferences",-1)?;let source=r.number("Source",-1)?;
    if installation.len()!=32 || !installation.bytes().all(|c|c.is_ascii_hexdigit()) || schema!=1
        || !(0..=10000).contains(&files) || !(0..=10000).contains(&prefs) || !matches!(source,0|1) {
        return Err(ServerError::InvalidRequest("Invalid storage migration receipt".into()));
    }
    sqlx::query("INSERT INTO client_storage_reports(account_id,installation,schema_version,source_present,files_copied,preferences_copied) VALUES(?,?,?,?,?,?) ON CONFLICT(account_id,installation) DO UPDATE SET last_seen=datetime('now')")
        .bind(account).bind(installation.to_ascii_lowercase()).bind(schema).bind(source).bind(files).bind(prefs).execute(&s.db).await?;
    Ok(Json(json!({"BaseResult":"Success","Result":"Success"})))
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup,login};
    use sqlx::Row;
    #[tokio::test]
    async fn storage_migration_reports_require_sessions_validate_and_upsert_per_installation() {
        let s=setup().await;let a=login(&s,"storage-receipt-a").await;let b=login(&s,"storage-receipt-b").await;
        let fields="Schema=1&Installation=1234567890abcdef1234567890abcdef&Files=2&Preferences=12&Source=1";
        assert!(report(State(s.clone()),Bytes::from(fields)).await.is_err());
        let form=|user:&Value,extra:&str|Bytes::from(format!("SessionKey={}&{extra}",user["UserInfo"]["SessionKey"].as_str().unwrap()));
        for invalid in [fields.replace("Schema=1","Schema=0"),fields.replace("Files=2","Files=-1"),fields.replace("Installation=1234567890abcdef1234567890abcdef","Installation=bad/path"),fields.replace("Source=1","Source=3")] {
            assert!(report(State(s.clone()),form(&a,&invalid)).await.is_err());
        }
        for user in [&a,&a,&b] {assert_eq!(report(State(s.clone()),form(user,fields)).await.unwrap().0["Result"],"Success");}
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM client_storage_reports").fetch_one(&s.db).await.unwrap(),2);
        let row=sqlx::query("SELECT files_copied,preferences_copied,first_seen,last_seen FROM client_storage_reports WHERE account_id=?").bind(a["UserInfo"]["AccountId"].as_i64().unwrap()).fetch_one(&s.db).await.unwrap();
        assert_eq!(row.get::<i64,_>("files_copied"),2);assert_eq!(row.get::<i64,_>("preferences_copied"),12);
        assert!(!row.get::<String,_>("first_seen").is_empty());
        migrate(&s.db).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM client_storage_reports").fetch_one(&s.db).await.unwrap(),2);
    }
    #[tokio::test]
    async fn storage_migration_reports_are_removed_with_a_deleted_account() {
        let db=sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        sqlx::query("PRAGMA foreign_keys=ON").execute(&db).await.unwrap();
        sqlx::query("CREATE TABLE accounts(account_id INTEGER PRIMARY KEY)").execute(&db).await.unwrap();
        migrate(&db).await.unwrap();
        sqlx::query("INSERT INTO accounts VALUES(1)").execute(&db).await.unwrap();
        sqlx::query("INSERT INTO client_storage_reports(account_id,installation,schema_version,source_present,files_copied,preferences_copied) VALUES(1,'1234567890abcdef1234567890abcdef',1,1,1,1)").execute(&db).await.unwrap();
        sqlx::query("DELETE FROM accounts WHERE account_id=1").execute(&db).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM client_storage_reports").fetch_one(&db).await.unwrap(),0);
    }

}
