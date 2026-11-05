//! Reads routed to the read-only pool must never write. Production readers are
//! opened read-only, so any misrouted write fails here with SQLITE_READONLY.
use crate::{api, database, state::AppState};
use axum::{body::Bytes, extract::State};
use serde_json::{json, Value};
use tower::ServiceExt;

#[tokio::test]
async fn read_pool_serves_routed_reads_and_rejects_writes() {
    let path = std::env::temp_dir().join(format!("sprk-readers-{}.sqlite", uuid::Uuid::new_v4()));
    let db = database::open_pool(&path).await.unwrap();
    database::create_tables(&db).await.unwrap();
    let reads = database::open_read_pool(&path).await.unwrap();
    let s = AppState::new(db.clone(), crate::tables::test_tables()).with_reads(reads.clone());

    let error = sqlx::query("INSERT INTO global_mails(title) VALUES('x')").execute(&reads).await.unwrap_err();
    assert!(error.to_string().contains("readonly"), "{error}");

    let login = api::account::user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=reader-pool")).await.unwrap().0;
    let key = login.user_info.session_key.clone();
    let account = login.user_info.account_id;
    let form = |extra: &str| Bytes::from(format!("SessionKey={key}&{extra}"));

    // Mail reads see the writer's committed deliveries; pending global mail
    // still takes the writer path and is delivered exactly once.
    api::community::mail::send_system_mail(&s, account, "Reader", "Body", 1, 0, None).await.unwrap();
    sqlx::query("INSERT INTO global_mails(title,reward_gold) VALUES('Reader announcement',1)").execute(&s.db).await.unwrap();
    let globals = api::community::mail::get_global_mail_list(State(s.clone()), form("")).await.unwrap().0;
    assert_eq!(globals["MailInfos"].as_array().unwrap().len(), 1, "{globals}");
    let again = api::community::mail::get_global_mail_list(State(s.clone()), form("")).await.unwrap().0;
    assert_eq!(again["MailInfos"].as_array().unwrap().len(), 1, "{again}");
    let check = api::community::mail::check_new_mail(State(s.clone()), form("TopmostMailIndex=0")).await.unwrap().0;
    assert_eq!(check["NewMailArrived"], true, "{check}");
    let list = api::community::mail::get_mail_list(State(s.clone()), form("MaxCount=10")).await.unwrap().0;
    // Personal mail: the welcome mail plus the one just sent (global excluded).
    assert_eq!(list["MailInfos"].as_array().unwrap().len(), 2, "{list}");
    assert!(list["MailInfos"].as_array().unwrap().iter().any(|m| m["Title"] == "Reader"), "{list}");

    for value in [
        api::community::friend::get_friend_list(State(s.clone()), form("")).await.unwrap().0,
        api::community::friend::search_friend(State(s.clone()), form("Keyword=")).await.unwrap().0,
        api::community::chat::get_recent_chats(State(s.clone()), form("ChannelId=1")).await.unwrap().0,
    ] {
        assert_eq!(value["BaseResult"], "Success", "{value}");
    }

    let mut headers = axum::http::HeaderMap::new();
    headers.insert("x-sprk-compact-lobby", "1".parse().unwrap());
    let request = || api::account::lobby::EnterLobbyRequest { session_id: None, session_key: Some(key.clone()) };
    let lobby = api::account::lobby::enter_lobby_http(State(s.clone()), headers.clone(), axum::extract::Form(request())).await.unwrap().0;
    assert!(lobby["PayShopProductInfos"].as_array().is_some_and(|p| !p.is_empty()), "catalog missing");
    let unread: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND is_received=0")
        .bind(account).fetch_one(&s.db).await.unwrap();
    assert_eq!(lobby["NewMailCount"], unread);

    // Mutation notifiers: the first response repairs under the writer, the
    // second takes the reader-side unchanged-revision paths.
    let app = axum::Router::new()
        .route("/test/mutate", axum::routing::post(|| async { axum::Json(json!({"BaseResult":"Success","Result":"Success"})) }))
        .layer(axum::middleware::from_fn_with_state(s.clone(), api::progression::notifications::notify))
        .layer(axum::middleware::from_fn_with_state(s.clone(), api::inventory::hero_level::notify))
        .with_state(s.clone());
    for _ in 0..2 {
        let response = app.clone().oneshot(axum::http::Request::post("/test/mutate")
            .header("Content-Type", "application/x-www-form-urlencoded")
            .body(axum::body::Body::from(form(""))).unwrap()).await.unwrap();
        assert!(response.status().is_success());
        let body: Value = serde_json::from_slice(&axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap()).unwrap();
        assert_eq!(body["Result"], "Success", "{body}");
    }

    reads.close().await;
    db.close().await;
    database::remove_test_database(&path).await;
}
