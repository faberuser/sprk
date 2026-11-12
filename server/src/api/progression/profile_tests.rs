//! Opt-in profiling against a real database copy:
//! SPRK_PROFILE_DATABASE_URL=<copy url> SPRK_PROFILE_ACCOUNT=<id> cargo test --release profile_snapshot -- --ignored --nocapture
use super::*;
use std::time::Instant;

#[tokio::test]
#[ignore]
async fn profile_snapshot() {
    let Ok(path) = std::env::var("SPRK_PROFILE_DATABASE_URL") else { return; };
    let account: i64 = std::env::var("SPRK_PROFILE_ACCOUNT").ok().and_then(|v| v.parse().ok()).unwrap_or(1);
    let db = crate::postgres::connect_read(&path).await.unwrap();
    let tables = crate::tables::GameTables::load(std::path::Path::new("tables")).unwrap();
    let state = AppState::new(db, tables);
    let mut conn = state.db.acquire().await.unwrap();
    for round in 0..3 {
        let t = Instant::now();
        let view = state::View::load(&mut conn, account).await.unwrap();
        let load = t.elapsed();
        let t = Instant::now(); let a = view.achievement_infos(&state); let ach = t.elapsed();
        let t = Instant::now(); let s = view.subquest_infos(&state); let sub = t.elapsed();
        let t = Instant::now();
        let c: Vec<Value> = state.tables.progression.clear_missions.iter().map(|r| json!({"P":view.quest(&state,r),"L":view.last("clear",n(r,"Index"),"all")})).collect();
        let clear = t.elapsed();
        let t = Instant::now(); let o = opened_mission_categories(&state, &view); let cats = t.elapsed();
        let t = Instant::now(); let p = play_record::snapshot(&mut conn, &state, account, &view).await.unwrap(); let play = t.elapsed();
        let t = Instant::now(); let at = attendance::snapshot(&mut conn, &state, account, &view).await.unwrap(); let att = t.elapsed();
        let t = Instant::now(); let full = snapshot(&mut conn, &state, account).await.unwrap(); let total = t.elapsed();
        println!("round {round}: load={load:?} achievements={ach:?}({}) subquests={sub:?}({}) clear={clear:?}({}) categories={cats:?} play_records={play:?}({}) attendance={att:?} | full snapshot={total:?} bytes={}",
            a.len(), s.len(), c.len(), p.len(), serde_json::to_vec(&full).unwrap().len());
        let t = Instant::now();
        let mut lasts = 0i64;
        for row in &state.tables.progression.sub_quests { lasts += view.last("subquest", n(row,"QuestIndex"), "all"); }
        let last_time = t.elapsed();
        let t = Instant::now();
        let mut quests = 0i64; let mut kinds = std::collections::BTreeMap::<String,(u32,std::time::Duration)>::new();
        for row in &state.tables.progression.sub_quests {
            let k = Instant::now(); quests += view.quest(&state,row);
            let e = kinds.entry(row["Kind"].as_str().unwrap_or("?").to_owned()).or_default(); e.0 += 1; e.1 += k.elapsed();
        }
        let quest_time = t.elapsed();
        let mut top: Vec<_> = kinds.into_iter().collect(); top.sort_by_key(|(_, (_, d))| std::cmp::Reverse(*d));
        println!("  subquest last()={last_time:?} quest()={quest_time:?} ({lasts},{quests}) top kinds: {:?}", &top[..top.len().min(6)]);
        let t = Instant::now(); let h = mission_history::History::load(&mut conn, account).await.unwrap(); println!("  history load={:?}", t.elapsed()); drop(h);
        let _ = (o, at);
    }
}

fn field_sizes(label: &str, value: &Value) {
    let mut sizes: Vec<(usize, String)> = value.as_object().map(|o| o.iter().map(|(k, v)| (serde_json::to_vec(v).unwrap().len(), k.clone())).collect()).unwrap_or_default();
    sizes.sort_by(|a, b| b.0.cmp(&a.0));
    println!("{label}: total={} top={:?}", serde_json::to_vec(value).unwrap().len(), &sizes[..sizes.len().min(12)]);
}

#[tokio::test]
#[ignore]
async fn profile_login_payloads() {
    let Ok(path) = std::env::var("SPRK_PROFILE_DATABASE_URL") else { return; };
    let account: i64 = std::env::var("SPRK_PROFILE_ACCOUNT").ok().and_then(|v| v.parse().ok()).unwrap_or(1);
    let db = crate::postgres::connect_read(&path).await.unwrap();
    let tables = crate::tables::GameTables::load(std::path::Path::new("tables")).unwrap();
    let state = AppState::new(db, tables);
    let login_id: String = crate::db::query_scalar("SELECT login_id FROM accounts WHERE account_id=$1").bind(account).fetch_one(&state.db).await.unwrap();
    let t = Instant::now();
    let login = crate::api::account::user::test_login(axum::extract::State(state.clone()), axum::body::Bytes::from(format!("LoginId={login_id}"))).await.unwrap().0;
    println!("login handler {:?}", t.elapsed());
    let login_value = serde_json::to_value(&login).unwrap();
    field_sizes("login", &login_value);
    let key = login.user_info.session_key.clone();
    let mut headers = axum::http::HeaderMap::new();
    headers.insert("x-sprk-compact-lobby", "1".parse().unwrap());
    headers.insert("x-sprk-sections", "1".parse().unwrap());
    let req = || crate::api::account::lobby::EnterLobbyRequest { session_id: None, session_key: Some(key.clone()) };
    let t = Instant::now();
    let first = crate::api::account::lobby::enter_lobby_http(axum::extract::State(state.clone()), headers.clone(), axum::extract::Form(req())).await.unwrap().0;
    println!("first lobby {:?}", t.elapsed());
    field_sizes("lobby first", &first);
    if let Some(token) = first["SprkShopCatalogToken"].as_str() { headers.insert("x-sprk-shop-catalog", token.parse().unwrap()); }
    for (name, header) in [("SprkAchievementToken","x-sprk-achievements"),("SprkMainQuestToken","x-sprk-main-quest"),("SprkClearMissionToken","x-sprk-clear-missions"),("SprkPlayRecordToken","x-sprk-play-records")] {
        if let Some(token) = first[name].as_str() { headers.insert(header, token.parse().unwrap()); }
    }
    let t = Instant::now();
    let repeat = crate::api::account::lobby::enter_lobby_http(axum::extract::State(state.clone()), headers.clone(), axum::extract::Form(req())).await.unwrap().0;
    println!("repeat lobby {:?}", t.elapsed());
    field_sizes("lobby repeat", &repeat);
    let mut shared: Vec<(usize, &String)> = first.as_object().unwrap().iter()
        .filter(|(k, v)| login_value.get(k.as_str()) == Some(*v) && serde_json::to_vec(*v).unwrap().len() > 1000)
        .map(|(k, v)| (serde_json::to_vec(v).unwrap().len(), k)).collect();
    shared.sort_by(|a, b| b.0.cmp(&a.0));
    println!("identical in login and first lobby (>1KB): {shared:?}");
}

#[tokio::test]
#[ignore]
async fn profile_lobby_parts() {
    let Ok(path) = std::env::var("SPRK_PROFILE_DATABASE_URL") else { return; };
    let account: i64 = std::env::var("SPRK_PROFILE_ACCOUNT").ok().and_then(|v| v.parse().ok()).unwrap_or(1);
    let db = crate::postgres::connect_read(&path).await.unwrap();
    let tables = crate::tables::GameTables::load(std::path::Path::new("tables")).unwrap();
    let state = AppState::new(db, tables);
    for round in 0..3 {
        let t = Instant::now();
        let shop = crate::api::live::shop_snapshot(&state, account).await.unwrap();
        let build = t.elapsed();
        let t = Instant::now();
        let bytes = serde_json::to_vec(&shop).unwrap();
        let encode = t.elapsed();
        let t = Instant::now();
        let mut gz = flate2::write::GzEncoder::new(Vec::new(), flate2::Compression::default());
        std::io::Write::write_all(&mut gz, &bytes).unwrap();
        let packed = gz.finish().unwrap();
        let gzip = t.elapsed();
        println!("round {round}: catalog build={build:?} serialize={encode:?} ({} B) gzip={gzip:?} ({} B)", bytes.len(), packed.len());
    }
}
