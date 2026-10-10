//! Free player nickname changes. Login credentials and AccountId remain stable.
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use crate::{api::system::request::Request, error::Result, state::AppState};

fn validation(state: &AppState, nick: &str) -> &'static str {
    let length = nick.chars().count() as i64;
    if length == 0 { return "EmptyNick"; }
    if length < state.tables.hero_shop.constant("MinNickLength", 2) { return "TooShortNick"; }
    if length > state.tables.hero_shop.constant("MaxNickLength", 12) { return "TooLongNick"; }
    if nick.trim() != nick || nick.chars().any(|c| c.is_control() || matches!(c, '<' | '>' | '[' | ']')) {
        return "InvalidNick";
    }
    "Success"
}

pub async fn query(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let nick = req.text("Nick");
    let mut result = validation(&state, nick);
    let account = req.account(&state).unwrap_or(0);
    if result == "Success" {
        let taken: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM accounts WHERE nick=? COLLATE NOCASE AND account_id<>?)")
            .bind(nick).bind(account).fetch_one(&state.db).await?;
        if taken { result = "AlreadyExist"; }
    }
    Ok(Json(json!({"BaseResult":"Success","Result":result,"IsAvailable":result=="Success"})))
}

pub async fn change(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let nick = req.text("Nick");
    let mut result = validation(&state, nick);
    if result == "Success" {
        // A single write checks availability and changes the name atomically.
        // No inventory or currency mutations, even on repeated changes.
        let changed = sqlx::query("UPDATE accounts SET nick=? WHERE account_id=? AND NOT EXISTS(SELECT 1 FROM accounts WHERE nick=? COLLATE NOCASE AND account_id<>?)")
            .bind(nick).bind(account).bind(nick).bind(account).execute(&state.db).await?;
        if changed.rows_affected() == 0 { result = "AlreadyExist"; }
    }
    Ok(Json(json!({"BaseResult":"Success","Result":result})))
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn free_nickname_changes_validate_persist_and_never_charge() {
        let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        crate::database::create_tables(&db).await.unwrap();
        let tables = crate::tables::test_tables();
        let state = AppState::new(db, tables);
        for id in [1i64,2] {
            sqlx::query("INSERT INTO accounts(account_id,login_id,nick) VALUES(?,?,?)").bind(id).bind(format!("login{id}")).bind(format!("Raider{id}")).execute(&state.db).await.unwrap();
            sqlx::query("INSERT INTO user_info(account_id,gold,gem) VALUES(?,0,0)").bind(id).execute(&state.db).await.unwrap();
            state.create_session(format!("session{id}"),id,String::new());
        }
        for nick in ["NewName","OtherName","OtherName"] {
            let body=Bytes::from(format!("SessionKey=session1&Nick={nick}"));
            assert_eq!(query(State(state.clone()),body.clone()).await.unwrap().0["Result"],"Success");
            assert_eq!(change(State(state.clone()),body).await.unwrap().0["Result"],"Success");
        }
        for (nick,result) in [("", "EmptyNick"),("a","TooShortNick"),("[bad]","InvalidNick"),("raider2","AlreadyExist")] {
            let body=Bytes::from(format!("SessionKey=session1&Nick={nick}"));
            assert_eq!(query(State(state.clone()),body.clone()).await.unwrap().0["Result"],result);
            assert_eq!(change(State(state.clone()),body).await.unwrap().0["Result"],result);
        }
        assert!(change(State(state.clone()),Bytes::from("Nick=NoSession")).await.is_err());
        let row:(String,String,i64,i64)=sqlx::query_as("SELECT nick,login_id,gold,gem FROM accounts JOIN user_info USING(account_id) WHERE account_id=1").fetch_one(&state.db).await.unwrap();
        assert_eq!(row,("OtherName".into(),"login1".into(),0,0));
        let count:i64=sqlx::query_scalar("SELECT COUNT(*) FROM items WHERE account_id=1").fetch_one(&state.db).await.unwrap();
        assert_eq!(count,0);
        // Two players competing for the same name cannot both succeed.
        let (a,b)=tokio::join!(
            change(State(state.clone()),Bytes::from("SessionKey=session1&Nick=SharedName")),
            change(State(state.clone()),Bytes::from("SessionKey=session2&Nick=SharedName"))
        );
        let results=[a.unwrap().0,b.unwrap().0];
        assert_eq!(results.iter().filter(|r|r["Result"]=="Success").count(),1);
        assert_eq!(results.iter().filter(|r|r["Result"]=="AlreadyExist").count(),1);
    }
}
