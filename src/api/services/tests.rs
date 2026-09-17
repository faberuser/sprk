use super::*;
use crate::{api::account::user, database, tables::GameTables};
use std::{
    path::Path,
    sync::{Arc, OnceLock},
};
async fn setup() -> (AppState, Value) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let mut s = AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    s.battle_service_key = Arc::new(Some("test-battle-service-secret-32-characters".into()));
    let u = json!(
        user::login(State(s.clone()), Bytes::from("LoginId=services-test"))
            .await
            .unwrap()
            .0
    );
    (s, u)
}
fn form(u: &Value, args: &str) -> Bytes {
    Bytes::from(format!(
        "SessionKey={}&{args}",
        u["UserInfo"]["SessionKey"].as_str().unwrap()
    ))
}
fn aid(u: &Value) -> i64 {
    n(&u["UserInfo"], "AccountId")
}
async fn call(s: &AppState, u: &Value, path: &str, args: &str) -> Value {
    execute_request(s, path, &HeaderMap::new(), form(u, args))
        .await
        .unwrap()
}
fn ok(v: &Value) {
    assert_eq!(v["Result"], "Success", "{v}");
}
async fn battle(s: &AppState, u: &Value, path: &str, args: &str) -> Value {
    crate::api::battle::execute_request(s, path, form(u, args))
        .await
        .unwrap()
}
fn headers(run: &str) -> HeaderMap {
    let mut h = HeaderMap::new();
    h.insert(
        "x-battle-service-key",
        "test-battle-service-secret-32-characters".parse().unwrap(),
    );
    h.insert("x-battle-run-id", run.parse().unwrap());
    h
}
const ENTRY: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
#[tokio::test]
async fn stamina_prices_regeneration_limits_and_rollback() {
    let (s, u) = setup().await;
    sqlx::query(
        "UPDATE user_info SET stamina=0,stamina_recharge_time=strftime('%s','now')-65,gem=5000",
    )
    .execute(&s.db)
    .await
    .unwrap();
    let v = call(&s, &u, "user/get_stamina", "StaminaType=Chicken").await;
    ok(&v);
    assert_eq!(v["StaminaResult"]["NewValue"], 2);
    assert!(n(&v["StaminaResult"], "NextRechargeRemainTime") <= 25);
    let v = call(
        &s,
        &u,
        "user/buy_stamina",
        "StaminaType=1&Amount=2000000000",
    )
    .await;
    ok(&v);
    assert_eq!(v["StaminaResult"]["AddValue"], 150);
    assert_eq!(v["CurrencyResult"]["NewValue"], 4950);
    let v = call(
        &s,
        &u,
        "user/recharge_stamina",
        "StaminaType=UndergroundPrisonKey",
    )
    .await;
    ok(&v);
    assert_eq!(v["StaminaResult"]["NewValue"], 8);
    assert_eq!(v["CurrencyResult"]["AddValue"], -100);
    let v = call(&s, &u, "user/recharge_stamina", "StaminaType=5").await;
    ok(&v);
    assert_eq!(v["CurrencyResult"]["AddValue"], -200);
    assert_eq!(v["StaminaResult"]["RechargeCount"], 2);
    let v = call(
        &s,
        &u,
        "user/get_stamina_infos",
        "StaminaTypes=Chicken&StaminaTypes=UndergroundPrisonKey",
    )
    .await;
    ok(&v);
    assert_eq!(v["StaminaResults"][1]["NewValue"], 11);
    sqlx::query("UPDATE user_info SET gem=0")
        .execute(&s.db)
        .await
        .unwrap();
    assert_ne!(
        call(&s, &u, "user/recharge_stamina", "StaminaType=5").await["Result"],
        "Success"
    );
    let v = call(&s, &u, "user/get_stamina", "StaminaType=5").await;
    assert_eq!(v["StaminaResult"]["RechargeCount"], 2);
    assert_eq!(v["StaminaResult"]["NewValue"], 11);
    assert!(execute_request(
        &s,
        "user/get_stamina",
        &HeaderMap::new(),
        Bytes::from("StaminaType=1")
    )
    .await
    .is_err());
    assert!(
        crate::api::account::stamina::use_stamina(State(s.clone()), form(&u, "Amount=-5"))
            .await
            .is_err()
    );
    assert!(crate::api::account::stamina::restore_stamina(
        State(s.clone()),
        form(&u, "Amount=500")
    )
    .await
    .is_err());
}
#[tokio::test]
async fn replay_roundtrip_privacy_deduplication_and_retention() {
    let (mut s, u) = setup().await;
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).services).rules["ReplayLimitPerAccount"] =
        json!(2);
    let b = json!(
        user::login(State(s.clone()), Bytes::from("LoginId=other"))
            .await
            .unwrap()
            .0
    );
    let args = "Type=Arena&Info=%7B%22Test%22%3A1%7D&BattleLogs=opaque-lz4";
    let first = call(&s, &u, "replay/save_replay", args).await;
    ok(&first);
    assert_eq!(first, call(&s, &u, "replay/save_replay", args).await);
    let id = first["ReplayUid"].as_i64().unwrap();
    let v = call(&s, &u, "replay/get_replay", &format!("ReplayUid={id}")).await;
    ok(&v);
    assert_eq!(v["Replay"]["Data"], "opaque-lz4");
    assert_ne!(
        call(&s, &b, "replay/get_replay", &format!("ReplayUid={id}")).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "replay/save_replay",
            &format!("{args}&AccountIds=[{},{}]", aid(&u), aid(&b))
        )
        .await["Result"],
        "Success"
    );
    for i in 2..4 {
        ok(&call(
            &s,
            &u,
            "replay/save_replay",
            &format!("Type=0&Info={{}}&BattleLogs=log{i}"),
        )
        .await);
    }
    let v = call(&s, &u, "replay/get_replay_list", "Type=0&Count=10").await;
    assert_eq!(v["Replays"].as_array().unwrap().len(), 2);
    assert_ne!(
        call(&s, &u, "replay/get_replay", &format!("ReplayUid={id}")).await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn authoritative_campaign_requires_credentials_run_binding_and_applies_once() {
    let (s, u) = setup().await;
    let a = aid(&u);
    let begin = battle(&s, &u, "campaign/begin_campaign", ENTRY).await;
    ok(&begin);
    let id = begin["RunId"].as_str().unwrap();
    let request = Bytes::from(format!(
        "AccountId={a}&ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1"
    ));
    assert!(execute_request(
        &s,
        "internal/b2g_battle_start",
        &HeaderMap::new(),
        request.clone()
    )
    .await
    .is_err());
    assert_ne!(
        execute_request(
            &s,
            "internal/b2g_battle_start",
            &headers("old-run"),
            request.clone()
        )
        .await
        .unwrap()["Result"],
        "Success"
    );
    ok(
        &execute_request(&s, "internal/b2g_battle_start", &headers(id), request)
            .await
            .unwrap(),
    );
    let finish =
        "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&AliveHeroIndices=[1]";
    assert_ne!(
        battle(&s, &u, "campaign/end_campaign", finish).await["Result"],
        "Success"
    );
    let req = Bytes::from(format!(
        "AccountId={a}&Win=true&PlayTime=30&AliveHeroIndices=[1]&TotalDamage=100"
    ));
    let out = execute_request(
        &s,
        "internal/b2g_set_campaign_result",
        &headers(id),
        req.clone(),
    )
    .await
    .unwrap();
    ok(&out);
    let gold: i64 = sqlx::query_scalar("SELECT gold FROM user_info WHERE account_id=?")
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap();
    ok(
        &execute_request(&s, "internal/b2g_set_campaign_result", &headers(id), req)
            .await
            .unwrap(),
    );
    assert_eq!(
        gold,
        sqlx::query_scalar::<_, i64>("SELECT gold FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap()
    );
    let changed = Bytes::from(format!(
        "AccountId={a}&Win=true&PlayTime=30&AliveHeroIndices=[1]&TotalDamage=200"
    ));
    assert_ne!(
        execute_request(
            &s,
            "internal/b2g_set_campaign_result",
            &headers(id),
            changed
        )
        .await
        .unwrap()["Result"],
        "Success"
    );
    ok(&battle(&s, &u, "campaign/end_campaign", finish).await);
    let recovery = call(&s, &u, "battle/recover", "").await;
    assert_eq!(recovery["Battle"]["Completed"], true);
    assert!(recovery["Battle"]["Result"].is_object());
    let decks = call(
        &s,
        &u,
        "recommend_deck/get_recommend_deck_list",
        "ChapterIndex=1&DungeonIndex=1&Difficulty=1",
    )
    .await;
    ok(&decks);
    assert_eq!(decks["RecommendDecks"].as_array().unwrap().len(), 2);
    assert_eq!(decks["RecommendDecks"][0]["PlayTime"], 30);
    assert_ne!(
        execute_request(
            &s,
            "internal/b2m_get_simulation",
            &headers(id),
            Bytes::new()
        )
        .await
        .unwrap()["Result"],
        "Success"
    );
}
#[tokio::test]
async fn honor_ranking_uses_scores_and_deterministic_ties() {
    let (s, u) = setup().await;
    let a = aid(&u);
    sqlx::query(
        "INSERT INTO arena_scores(account,kind,season,score,wins,losses) VALUES(?,0,7,1234,4,2)",
    )
    .bind(a)
    .execute(&s.db)
    .await
    .unwrap();
    let v = call(
        &s,
        &u,
        "records_of_honor/get_records_of_honor_ranking_list",
        "ContentType=Match&Season=7",
    )
    .await;
    ok(&v);
    assert_eq!(v["ServerInfo"][0]["Score"], 1234);
    assert_eq!(v["MyInfo"][0]["Rank"], 1);
    assert_eq!(v["ServerInfo"][0]["Win"], 4);
    let v = call(
        &s,
        &u,
        "records_of_honor/get_contents_notice_popup_ranker_list",
        "ContentsType=Match&SeasonIndex=7",
    )
    .await;
    ok(&v);
    assert_eq!(v["ServerRankInfos"][0]["Score"], 1234);
    let v = call(
        &s,
        &u,
        "records_of_honor/get_records_of_honor_ranking_list",
        "ContentType=Match&Season=6",
    )
    .await;
    ok(&v);
    assert!(v["ServerInfo"].as_array().unwrap().is_empty());
}

#[tokio::test]
async fn arena_service_claim_blocks_client_results_and_callback_retries_do_not_pay_twice() {
    let (s, u) = setup().await;
    let a = aid(&u);
    sqlx::query("UPDATE user_info SET team_level=60 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let args = form(&u, "ArenaType=0&HeroIndices=[1]&LeaderHeroIndex=1");
    let v = crate::api::community::execute_request(&s, "match/register_match", args)
        .await
        .unwrap();
    ok(&v);
    let v = crate::api::community::execute_request(
        &s,
        "match/wait_match",
        form(&u, "ArenaType=0&PlayOfflineMatch=true"),
    )
    .await
    .unwrap();
    assert_eq!(v["Result"], "WaitMore");
    let row = sqlx::query("SELECT id,data FROM arena_runs WHERE account=? AND status='battle'")
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap();
    let id = row.get::<i64, _>("id");
    let data: Value = serde_json::from_str(row.get("data")).unwrap();
    let season = n(&data, "Season");
    ok(&execute_request(
        &s,
        "internal/b2m_get_match_info",
        &headers(""),
        Bytes::from(format!("MatchUid={id}")),
    )
    .await
    .unwrap());
    let fields=format!("AccountId={a}&MatchUid={id}&SeasonIndex={season}&ArenaType=Normal&Win=1&PlayTime=10&AliveHeroIndices=[1]");
    let client = crate::api::community::execute_request(
        &s,
        "match/set_offline_match_result",
        form(&u, &fields),
    )
    .await
    .unwrap();
    assert_ne!(client["Result"], "Success");
    let v = execute_request(
        &s,
        "internal/b2g_set_match_result",
        &headers(""),
        Bytes::from(fields.clone()),
    )
    .await
    .unwrap();
    ok(&v);
    let coins: i64 = sqlx::query_scalar("SELECT pvp_coin FROM user_info WHERE account_id=?")
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(
        v,
        execute_request(
            &s,
            "internal/b2g_set_match_result",
            &headers(""),
            Bytes::from(fields)
        )
        .await
        .unwrap()
    );
    assert_eq!(
        coins,
        sqlx::query_scalar::<_, i64>("SELECT pvp_coin FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap()
    );
}
