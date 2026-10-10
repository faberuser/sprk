use super::*;
use axum::{body::{Body, to_bytes}, http::{Request, StatusCode}, Router};
use tower::ServiceExt;

async fn call(router: Router, path: &str, key: Option<&str>, body: &str) -> (StatusCode, serde_json::Value) {
    let mut request = Request::post(path).header("Content-Type", "application/x-www-form-urlencoded");
    if let Some(key) = key { request = request.header("Authorization", format!("Bearer {key}")); }
    let response = router.oneshot(request.body(Body::from(body.to_owned())).unwrap()).await.unwrap();
    let status = response.status();
    let bytes = to_bytes(response.into_body(), 100000).await.unwrap();
    (status, serde_json::from_slice(&bytes).unwrap_or_default())
}

#[tokio::test]
async fn private_admin_requires_key_and_targets_offline_accounts() {
    let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
    crate::database::create_tables(&db).await.unwrap();
    let tables = crate::tables::test_tables();
    let state = AppState::new(db, tables);
    for id in [1i64,2] {
        sqlx::query("INSERT INTO accounts(account_id,login_id,nick) VALUES(?,?,?)").bind(id).bind(format!("player{id}")).bind(format!("Raider{id}")).execute(&state.db).await.unwrap();
        sqlx::query("INSERT INTO user_info(account_id) VALUES(?)").bind(id).execute(&state.db).await.unwrap();
    }
    state.create_session("player-session".into(),1,"".into());
    let key = "test-only-administrator-key-123456789";
    let admin = admin_routes(state.clone(),key.into());
    let public = public_routes().with_state(state.clone());
    for path in ["players","currency","hero","level","unlock","reset","allheroes","uwut"] {
        let path = format!("/cheat/{path}");
        for supplied in [None, Some("wrong"), Some("player-session")] {
            assert_eq!(call(admin.clone(),&path,supplied,"SessionId=player-session&AccountId=2&Gold=100").await.0,StatusCode::UNAUTHORIZED);
        }
        assert_eq!(call(public.clone(),&path,Some(key),"AccountId=2").await.0,StatusCode::NOT_FOUND);
    }
    assert_eq!(call(public,"/cheat/unknown",Some(key),"").await.0,StatusCode::NOT_FOUND);
    assert_eq!(call(admin_routes(state.clone(),String::new()),"/cheat/players",Some(key),"").await.0,StatusCode::UNAUTHORIZED);
    let (status,result)=call(admin.clone(),"/cheat/currency",Some(key),"AccountId=2&Gold=123&Gem=7&Stamina=8").await;
    assert_eq!(status,StatusCode::OK);
    assert_eq!(result["NewGold"],123);assert_eq!(result["NewGem"],7);assert_eq!(result["NewStamina"],208);
    let untouched:i64=sqlx::query_scalar("SELECT gold FROM user_info WHERE account_id=1").fetch_one(&state.db).await.unwrap();
    assert_eq!(untouched,0);
    assert_eq!(call(admin.clone(),"/cheat/level",Some(key),"AccountId=2&Level=12").await.0,StatusCode::OK);
    let level:i64=sqlx::query_scalar("SELECT team_level FROM user_info WHERE account_id=2").fetch_one(&state.db).await.unwrap();
    assert_eq!(level,12);
    for body in ["AccountId=0", "AccountId=-1", "", "AccountId=2&SessionId=player-session"] {
        assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),body).await.0,StatusCode::BAD_REQUEST);
    }
    assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),"AccountId=999").await.0,StatusCode::NOT_FOUND);
    assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),"SessionId=player-session&Gold=5").await.0,StatusCode::OK);
    let (status,players)=call(admin,"/cheat/players",Some(key),"Query=raider2").await;
    assert_eq!(status,StatusCode::OK);assert_eq!(players["Players"].as_array().unwrap().len(),1);
    assert_eq!(players["Players"][0]["AccountId"],2);
    assert!(players["Players"][0].get("SessionId").is_none());
}
