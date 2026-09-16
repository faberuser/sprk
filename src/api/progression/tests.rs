use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{
    path::Path,
    sync::{Arc, OnceLock},
};
async fn setup() -> (AppState, user::LoginResponse) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let s = AppState::new(
        pool,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    let u = relogin(&s).await;
    (s, u)
}
async fn relogin(s: &AppState) -> user::LoginResponse {
    user::login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=progression-test"),
    )
    .await
    .unwrap()
    .0
}
fn form(u: &user::LoginResponse, data: &str) -> Bytes {
    Bytes::from(format!("SessionKey={}&{data}", u.user_info.session_key))
}
async fn call(s: &AppState, u: &user::LoginResponse, action: &str, data: &str) -> Value {
    handle(s.clone(), form(u, data), action).await.unwrap().0
}
async fn count(s: &AppState, u: &user::LoginResponse, id: i64) -> i64 {
    sqlx::query_scalar(
        "SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=?),0)",
    )
    .bind(u.user_info.account_id)
    .bind(id)
    .fetch_one(&s.db)
    .await
    .unwrap()
}
async fn currency(s: &AppState, u: &user::LoginResponse) -> (i64, i64) {
    sqlx::query_as("SELECT gold,gem FROM user_info WHERE account_id=?")
        .bind(u.user_info.account_id)
        .fetch_one(&s.db)
        .await
        .unwrap()
}

#[tokio::test]
async fn attendance_claim_once_per_day_and_persist() {
    let (s, u) = setup().await;
    let r = &s.tables.progression.calendars[0]["Reward"][0]["Reward"][0];
    let id = n(r, "Value");
    let before = count(&s, &u, id).await;
    let a = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(count(&s, &u, id).await, before + n(r, "Count"));
    assert_eq!(a["AttendanceInfos"][0]["LastRewardedDay"], 1);
    assert_ne!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Success"
    );
    let login = relogin(&s).await;
    assert_eq!(login.attendance_infos[0]["LastRewardedDay"], 1);
    assert_eq!(login.misc_info.unwrap().login_daily_count, 1);
    sqlx::query("UPDATE attendance_calendar_state SET last_day='2000-01-01' WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let a = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(a["AttendanceInfos"][0]["LastRewardedDay"], 2);
}
#[tokio::test]
async fn attendance_conditional_bulk_failure_rolls_back_first_delivery() {
    let (mut s, u) = setup().await;
    let tables = Arc::make_mut(&mut s.tables);
    let data = Arc::make_mut(&mut tables.progression);
    let mut c = data.calendars[0].clone();
    c["Index"] = json!(2);
    c["Conditions"] = json!(["TeamLevel", "999"]);
    data.calendars.push(c);
    let id = n(
        &s.tables.progression.calendars[0]["Reward"][0]["Reward"][0],
        "Value",
    );
    let before = count(&s, &u, id).await;
    assert_ne!(
        call(
            &s,
            &u,
            "get_all_conditional_attendance_reward",
            "AttendanceIndex=[1,2]"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, id).await, before);
    assert_eq!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn accumulated_login_uses_days_not_request_count() {
    let (s, u) = setup().await;
    assert_eq!(
        call(&s, &u, "get_logindaily_reward", "LoginDailyIndex=11").await["Result"],
        "IsNotCondition"
    );
    for _ in 0..2 {
        assert_eq!(relogin(&s).await.misc_info.unwrap().login_daily_count, 1);
    }
    sqlx::query("UPDATE progression_login SET days=6,last_day='2000-01-01' WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(relogin(&s).await.misc_info.unwrap().login_daily_count, 7);
    assert_eq!(
        call(&s, &u, "get_logindaily_reward", "LoginDailyIndex=11").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "get_logindaily_reward", "LoginDailyIndex=11").await["Result"],
        "AlreadyReceived"
    );
    assert_eq!(
        relogin(&s).await.login_daily_infos[0]["LoginDailyIndex"],
        11
    );
}
#[tokio::test]
async fn daily_achievement_native_rewards_and_reset() {
    let (s, u) = setup().await;
    let before = currency(&s, &u).await;
    let a = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[1101]&Steps=[1]",
    )
    .await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(currency(&s, &u).await.1, before.1 + 20);
    assert!(a["itemResults"].is_array());
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1101]&Steps=[1]"
        )
        .await["Result"],
        "InvalidStep"
    );
    sqlx::query("UPDATE progression_claims SET period='2000-01-01' WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1101]&Steps=[1]"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        relogin(&s)
            .await
            .achievement_infos
            .iter()
            .find(|r| r["AchievementIndex"] == 1101)
            .unwrap()["LastStep"],
        1
    );
}
#[tokio::test]
async fn batch_achievement_error_and_forged_progress_never_grant() {
    let (s, u) = setup().await;
    let before = currency(&s, &u).await;
    assert_ne!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1101,99999999]&Steps=[1,1]"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(currency(&s, &u).await, before);
    let check = call(
        &s,
        &u,
        "check_achievements",
        "AchievementId=1705&SetValue=999999999&Increment=99999999",
    )
    .await;
    assert_eq!(check["Result"], "Success");
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1705]&Steps=[1]"
        )
        .await["Result"],
        "AchievementNotCleared"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1101]&Steps=[1]"
        )
        .await["Result"],
        "Success"
    );
    assert!(handle(
        s.clone(),
        Bytes::from_static(b"SessionKey=invalid&AttendanceIndex=1"),
        "get_attendance_reward"
    )
    .await
    .is_err());
}
#[tokio::test]
async fn currency_metrics_follow_transaction_rollback() {
    let (s, u) = setup().await;
    let mut tx = s.db.begin().await.unwrap();
    item::money(&mut tx, u.user_info.account_id, "Gold", -1000000)
        .await
        .unwrap();
    tx.rollback().await.unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1705]&Steps=[1]"
        )
        .await["Result"],
        "AchievementNotCleared"
    );
    let mut tx = s.db.begin().await.unwrap();
    item::money(&mut tx, u.user_info.account_id, "Gold", -1000000)
        .await
        .unwrap();
    tx.commit().await.unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1705]&Steps=[1]"
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn subquest_requires_saved_dungeon_and_claims_once() {
    let (s, u) = setup().await;
    assert_ne!(
        call(&s, &u, "complete_sub_quest", "SubQuestIndices=[10010]").await["Result"],
        "Success"
    );
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,1,1,13) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=1,best_star=13").bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let a = call(&s, &u, "complete_sub_quest", "SubQuestIndices=[10010]").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(a["SucceededSubQuestInfos"][0]["LastStep"], 1);
    assert_eq!(a["RewardResults"].as_array().unwrap().len(), 1);
    assert_ne!(
        call(&s, &u, "complete_sub_quest", "SubQuestIndices=[10010]").await["Result"],
        "Success"
    );
    assert_eq!(
        relogin(&s)
            .await
            .sub_quest_infos
            .iter()
            .find(|r| r["SubQuestIndex"] == 10010)
            .unwrap()["LastStep"],
        1
    );
}
#[tokio::test]
async fn chapter_reward_uses_decimal_star_encoding_and_bitmask() {
    let (s, u) = setup().await;
    assert_eq!(
        call(&s, &u, "reward_clear_chapter", "ChapterIndex=1&Step=1").await["Result"],
        "NotEnoughStar"
    );
    for id in 1..=8 {
        sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,?,1,13) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=1,best_star=13").bind(u.user_info.account_id).bind(id).execute(&s.db).await.unwrap();
    }
    let a = call(&s, &u, "reward_clear_chapter", "ChapterIndex=1&Step=1").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(a["RewardInfo"]["LastRewardDiff"], 2);
    assert_eq!(
        call(&s, &u, "reward_clear_chapter", "ChapterIndex=1&Step=1").await["Result"],
        "WrongStep"
    );
}
#[tokio::test]
async fn paid_missions_require_entitlement_and_gameplay() {
    let (s, u) = setup().await;
    assert_eq!(
        call(&s, &u, "get_clear_mission_reward", "MissionIndex=1001").await["Result"],
        "CheckConditionNotPassed"
    );
    sqlx::query("INSERT INTO progression_entitlements(account_id,product_index) VALUES(?,410015)")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        call(&s, &u, "get_clear_mission_reward", "MissionIndex=1001").await["Result"],
        "ClearMissionNotCleared"
    );
    sqlx::query("UPDATE user_info SET team_level=3 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        call(&s, &u, "get_clear_mission_reward", "MissionIndex=1001").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "get_newbie_mission_reward", "MissionIndex=5001").await["Result"],
        "CheckConditionNotPassed"
    );
    for id in 410009..=410011 {
        sqlx::query("INSERT INTO progression_entitlements(account_id,product_index) VALUES(?,?)")
            .bind(u.user_info.account_id)
            .bind(id)
            .execute(&s.db)
            .await
            .unwrap();
    }
    assert_eq!(
        call(&s, &u, "get_newbie_mission_reward", "MissionIndex=5001").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "get_newbie_mission_reward", "MissionIndex=5001").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "get_newbie_mission_reward", "MissionIndex=5002").await["Result"],
        "Success"
    );
    assert_eq!(relogin(&s).await.player_product_purchase_infos.len(), 4);
}
#[tokio::test]
async fn world_map_claim_needs_owned_completed_event_and_rolls_back_batch() {
    let (s, u) = setup().await;
    let code = s
        .tables
        .progression
        .achievements
        .iter()
        .find(|r| {
            r["RewardKind"] == "Item"
                && s.tables
                    .get_item_index(r["RewardCode"].as_str().unwrap())
                    .is_some()
        })
        .unwrap()["RewardCode"]
        .as_str()
        .unwrap();
    let id = s.tables.get_item_index(code).unwrap() as i64;
    let items_before = count(&s, &u, id).await;
    let data = json!({"EventIndex":99,"ChapterIndex":1,"DungeonIndex":1,"Completed":true,"NoExpire":true,"RewardGold":456,"RewardItems":json!([{"Code":code,"Count":1,"Star":0}]).to_string()});
    sqlx::query("INSERT INTO progression_world_events(account_id,chapter,dungeon,event,data) VALUES(?,1,1,99,?)").bind(u.user_info.account_id).bind(data.to_string()).execute(&s.db).await.unwrap();
    let before = currency(&s, &u).await;
    assert_ne!(
        call(
            &s,
            &u,
            "receive_completed_reward",
            "ChapterIndex=1&DungeonIndices=[1,2]&EventIndices=[99,100]"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(currency(&s, &u).await, before);
    let a = call(
        &s,
        &u,
        "receive_completed_reward",
        "ChapterIndex=1&DungeonIndices=[1]&EventIndices=[99]",
    )
    .await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(currency(&s, &u).await.0, before.0 + 456);
    assert_eq!(count(&s, &u, id).await, items_before + 1);
    assert_ne!(
        call(
            &s,
            &u,
            "receive_completed_reward",
            "ChapterIndex=1&DungeonIndices=[1]&EventIndices=[99]"
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn missing_main_quests_and_unsupported_missions_do_not_autocomplete() {
    let (s, u) = setup().await;
    assert_eq!(
        call(&s, &u, "progress_client_main_quest", "").await["Result"],
        "QuestDataNotFound"
    );
    assert_ne!(
        call(&s, &u, "complete_sub_quest", "SubQuestIndices=[1001010]").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn hero_achievement_requires_actual_star_and_booster_reward_activates() {
    let (mut s, u) = setup().await;
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1002]&Steps=[1]"
        )
        .await["Result"],
        "AchievementNotCleared"
    );
    sqlx::query("UPDATE heroes SET star=2 WHERE account_id=? AND hero_index=1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let infos = call(&s, &u, "get_achievements", "").await;
    assert_eq!(
        infos["AchievementInfos"]
            .as_array()
            .unwrap()
            .iter()
            .find(|r| r["AchievementIndex"] == 1002)
            .unwrap()["Achievement"],
        2
    );
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1002]&Steps=[1]"
        )
        .await["Result"],
        "Success"
    );
    let r = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[55]&Steps=[1]",
    )
    .await;
    assert_eq!(r["Result"], "RewardTimeEnded");
    // Exercise booster delivery with an explicitly enabled local definition.
    let tables = Arc::make_mut(&mut s.tables);
    let progression = Arc::make_mut(&mut tables.progression);
    let row = progression
        .achievements
        .iter_mut()
        .find(|r| r["Index"] == 55)
        .unwrap();
    row["Type"] = json!(1);
    row["OpenCondition"] = json!([]);
    let r = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[55]&Steps=[1]",
    )
    .await;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ItemTimeDurationInfo"]["ItemIndex"], 110018);
}
#[tokio::test]
async fn mission_bonus_only_counts_its_own_category() {
    let (s, u) = setup().await;
    let mut db = s.db.acquire().await.unwrap();
    for id in 900000..900040 {
        claim(&mut db, u.user_info.account_id, "subquest", id, 1, "all")
            .await
            .unwrap();
    }
    let v = View::load(&mut db, u.user_info.account_id).await.unwrap();
    let row = s
        .tables
        .progression
        .sub_quests
        .iter()
        .find(|r| r["QuestIndex"] == 20001)
        .unwrap();
    assert_eq!(v.quest(&s, row), 0);
}
#[tokio::test]
async fn separate_connections_claim_attendance_once_and_migrate_idempotently() {
    let (template, _) = setup().await;
    let path =
        std::env::temp_dir().join(format!("sprk-progression-{}.sqlite", uuid::Uuid::new_v4()));
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .min_connections(3)
        .max_connections(3)
        .connect_with(
            sqlx::sqlite::SqliteConnectOptions::new()
                .filename(&path)
                .create_if_missing(true)
                .busy_timeout(std::time::Duration::from_secs(20)),
        )
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let s = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let u = relogin(&s).await;
    let (a, b) = tokio::join!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1"),
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1")
    );
    assert_eq!(
        [a, b].iter().filter(|r| r["Result"] == "Success").count(),
        1
    );
    database::create_tables(&pool).await.unwrap();
    assert_eq!(relogin(&s).await.attendance_infos[0]["LastRewardedDay"], 1);
    pool.close().await;
    std::fs::remove_file(path).unwrap();
}

#[tokio::test]
async fn gameplay_response_updates_native_progress_and_stamina_achievement() {
    use tower::Service;
    let (s, u) = setup().await;
    let mut app = axum::Router::new()
        .route(
            "/campaign/begin",
            axum::routing::post(crate::api::battle::campaign_handlers::begin_campaign),
        )
        .layer(axum::middleware::from_fn_with_state(
            s.clone(),
            notifications::notify,
        ))
        .with_state(s.clone());
    let body = form(&u, "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]");
    let response = app
        .call(
            axum::http::Request::builder()
                .method("POST")
                .uri("/campaign/begin")
                .header("Content-Type", "application/x-www-form-urlencoded")
                .body(axum::body::Body::from(body))
                .unwrap(),
        )
        .await
        .unwrap();
    let response: Value = serde_json::from_slice(
        &axum::body::to_bytes(response.into_body(), usize::MAX)
            .await
            .unwrap(),
    )
    .unwrap();
    assert_eq!(response["Result"], "Success");
    assert!(response["ReservedClearMissionInfos"].as_array().is_some());
    let infos = response["AchievementInfos"].as_array().unwrap();
    let progress = n(
        infos.iter().find(|r| r["AchievementIndex"] == 301).unwrap(),
        "Achievement",
    );
    assert!(progress > 0);
    let mut db = s.db.acquire().await.unwrap();
    let view = View::load(&mut db, u.user_info.account_id).await.unwrap();
    assert_eq!(view.metric("EnterDungeon", "all", Some(1), Some(1)), 1);
    assert_eq!(view.metric("ConsumeStamina", "all", None, None), progress);
}

#[tokio::test]
async fn attendance_cycle_wraps_and_nonrepeating_calendar_finishes() {
    let (mut s, u) = setup().await;
    sqlx::query("INSERT INTO attendance_calendar_state(account_id,idx,claims,last_day) VALUES(?,1,7,date('now'))")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    assert_eq!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Fail"
    );
    sqlx::query(
        "UPDATE attendance_calendar_state SET last_day=date('now','-1 day') WHERE account_id=?",
    )
    .bind(u.user_info.account_id)
    .execute(&s.db)
    .await
    .unwrap();
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression).calendars[0]["Repeatable"] =
        json!(0);
    assert_eq!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Fail"
    );
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression).calendars[0]["Repeatable"] =
        json!(1);
    let r = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["AttendanceInfos"][0]["LastRewardedDay"], 1);
}

#[tokio::test]
async fn item_removal_does_not_count_as_use_and_stamina_reward_matches_native_type() {
    let (mut s, u) = setup().await;
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,2002,10) ON CONFLICT(account_id,item_index) DO UPDATE SET count=10")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE items SET count=0 WHERE account_id=? AND item_index=2002")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    {
        let mut db = s.db.acquire().await.unwrap();
        assert_eq!(
            View::load(&mut db, u.user_info.account_id)
                .await
                .unwrap()
                .metric("UseItem", "all", None, None),
            0
        );
    }
    let table = Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression);
    let row = table
        .achievements
        .iter_mut()
        .find(|r| r["Index"] == 1101)
        .unwrap();
    row["RewardKind"] = json!("Stamina");
    row["RewardValue"] = json!(10);
    let r = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[1101]&Steps=[1]",
    )
    .await;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["staminaResults"][0]["Type"], "Chicken");
    assert_eq!(r["staminaResults"][0]["AddValue"], 10);
}
