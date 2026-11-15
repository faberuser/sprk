//! Opt-in end-to-end request timing against a restored copy (it writes):
//!
//! SPRK_PROFILE_DATABASE_URL=<copy url> SPRK_PROFILE_ACCOUNT=<id> SPRK_PROFILE_REQUESTS=<file>
//! cargo test --release profile_requests -- --ignored --nocapture
//!
//! Run from the repository root (it loads `tables/`). Each line of the file is
//! `<path> <form fields>` (the SessionKey is added); `#` starts a comment. Every
//! request goes through the full game router and middleware, and acknowledges
//! the progression, section and catalog tokens as the client does. With a
//! superuser URL it also reports the statements each request ran.
//! `SPRK_PROFILE_LOG=sqlx::query=debug` adds a timeline of every statement.
use crate::state::AppState;
use axum::body::Body;
use serde_json::Value;
use std::time::Instant;
use tower::ServiceExt;

#[tokio::test]
#[ignore]
async fn profile_requests() {
    let Ok(url) = std::env::var("SPRK_PROFILE_DATABASE_URL") else { return; };
    // SPRK_PROFILE_LOG=sqlx::query=debug prints each statement with its time:
    // gaps between statements are application work.
    if let Ok(filter) = std::env::var("SPRK_PROFILE_LOG") {
        let _ = tracing_subscriber::fmt().with_env_filter(filter).with_timer(tracing_subscriber::fmt::time::uptime()).try_init();
    }
    let account: i64 = std::env::var("SPRK_PROFILE_ACCOUNT").ok().and_then(|v| v.parse().ok()).unwrap_or(1);
    let repeat: usize = std::env::var("SPRK_PROFILE_REPEAT").ok().and_then(|v| v.parse().ok()).unwrap_or(5);
    let lines = std::fs::read_to_string(std::env::var("SPRK_PROFILE_REQUESTS").expect("SPRK_PROFILE_REQUESTS")).unwrap();
    let db = crate::postgres::connect(&url).await.unwrap();
    let reads = crate::postgres::connect_read(&url).await.unwrap();
    let tables = crate::tables::GameTables::load(std::path::Path::new("tables")).unwrap();
    let state = AppState::new(db, tables).with_reads(reads);
    let login_id: String = crate::db::query_scalar("SELECT login_id FROM accounts WHERE account_id=$1")
        .bind(account).fetch_one(&state.db).await.unwrap();
    let login = crate::api::account::user::test_login(axum::extract::State(state.clone()), axum::body::Bytes::from(format!("LoginId={login_id}")))
        .await.unwrap().0;
    let session = login.user_info.session_key.clone();
    crate::api::progression::notifications::remember_login_sections(&state, &session, &serde_json::to_value(&login).unwrap()["AchievementInfos"]);
    let app = crate::game_routes(state.clone());
    let stats = crate::db::query_scalar::<i64>("SELECT COALESCE(SUM(calls),0)::bigint FROM pg_stat_statements").fetch_one(&state.db).await.is_ok();
    if stats {
        // Leave only the measured requests in pg_stat_statements.
        crate::db::query("SELECT pg_stat_statements_reset()").execute(&state.db).await.unwrap();
    }
    // Tokens the client echoes back (header name -> value).
    let mut acks = std::collections::BTreeMap::<&str, String>::new();
    let requests: Vec<(&str, &str)> = lines.lines().map(str::trim).filter(|l| !l.is_empty() && !l.starts_with('#'))
        .map(|line| line.split_once(char::is_whitespace).unwrap_or((line, ""))).collect();
    // The whole list runs once per round, so dependent requests stay in order.
    let mut runs: Vec<(Vec<f64>, String, usize, i64)> = vec![(Vec::new(), String::new(), 0, 0); requests.len()];
    for _ in 0..repeat {
        for (i, (path, fields)) in requests.iter().enumerate() {
            let mut request = axum::http::Request::post(*path)
                .header("content-type", "application/x-www-form-urlencoded")
                .header("x-sprk-sections", "1").header("x-sprk-achievement-delta", "1").header("x-sprk-compact-lobby", "1");
            for (name, value) in &acks { request = request.header(*name, value); }
            let body = if fields.trim().is_empty() { format!("SessionKey={session}") } else { format!("SessionKey={session}&{}", fields.trim()) };
            let before = if stats { crate::db::query_scalar::<i64>("SELECT COALESCE(SUM(calls),0)::bigint FROM pg_stat_statements").fetch_one(&state.db).await.unwrap() } else { 0 };
            let start = Instant::now();
            let response = app.clone().oneshot(request.body(Body::from(body)).unwrap()).await.unwrap();
            let status = response.status();
            let raw = axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap();
            let run = &mut runs[i];
            run.0.push(start.elapsed().as_secs_f64() * 1000.0);
            if stats {
                // Minus this query and the one before the request.
                run.3 = crate::db::query_scalar::<i64>("SELECT COALESCE(SUM(calls),0)::bigint FROM pg_stat_statements").fetch_one(&state.db).await.unwrap() - before - 1;
            }
            run.2 = raw.len();
            let value: Value = serde_json::from_slice(&raw).unwrap_or(Value::Null);
            run.1 = format!("{status} {}", value["Result"].as_str().or(value["ErrorCode"].as_str()).unwrap_or("?"));
            for (field, header) in [("SprkProgressionToken", "x-sprk-progression"), ("SprkAchievementToken", "x-sprk-achievements"),
                ("SprkMainQuestToken", "x-sprk-main-quest"), ("SprkClearMissionToken", "x-sprk-clear-missions"),
                ("SprkPlayRecordToken", "x-sprk-play-records"), ("SprkShopCatalogToken", "x-sprk-shop-catalog")] {
                if let Some(token) = value[field].as_str() { acks.insert(header, token.to_owned()); }
            }
        }
    }
    for ((path, _), (times, result, bytes, statements)) in requests.iter().zip(runs) {
        let mut sorted = times.clone();
        sorted.sort_by(f64::total_cmp);
        println!("{path:<44} median={:>7.2}ms min={:>7.2}ms {result} bytes={bytes} statements={statements}",
            sorted[sorted.len() / 2], sorted[0]);
    }
}
