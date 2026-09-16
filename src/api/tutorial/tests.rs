use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use axum::body::Bytes;
use std::{
    path::Path,
    sync::{Arc, OnceLock},
};

fn tables() -> GameTables {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    TABLES
        .get_or_init(|| {
            GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
        })
        .clone()
}

async fn setup() -> (AppState, user::LoginResponse) {
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let state = AppState::new(pool, tables());
    let response = login(&state).await;
    (state, response)
}

async fn login(state: &AppState) -> user::LoginResponse {
    user::login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=tutorial-test"),
    )
    .await
    .unwrap()
    .0
}

fn request(key: &str, index: i32) -> TutorialRequest {
    TutorialRequest {
        session_id: None,
        session_key: Some(key.into()),
        tutorial_index: Some(index),
    }
}

async fn complete(state: &AppState, key: &str, index: i32) -> Value {
    complete_tutorial(State(state.clone()), Form(request(key, index)))
        .await
        .unwrap()
        .0
}

#[tokio::test]
async fn fresh_account_recruits_party_and_resumes_after_login() {
    let (state, first) = setup().await;
    assert!(!first.is_tutorial_skip);
    assert_eq!(first.heroes.len(), 1);
    assert_eq!((first.heroes[0].hero_index, first.heroes[0].star), (1, 1));
    let key = &first.user_info.session_key;
    let frey = complete(&state, key, 1000).await;
    assert_eq!(frey["HeroInfos"][0]["HeroIndex"], 2);
    assert_eq!(frey["TeamExpResultInfos"][0]["NewLevel"], 2);
    let battle = complete(&state, key, 10000).await;
    assert_eq!(battle["DungeonInfos"][0]["DungeonIndex"], 1);
    assert_eq!(battle["DungeonInfos"][0]["MaxStar"], 13);
    assert_eq!(battle["HeroInfos"][0]["Level"], 2);
    assert_eq!(battle["HeroInfos"][0]["Exp"], 0);
    let recruits = complete(&state, key, 10001).await;
    assert_eq!(
        recruits["HeroInfos"]
            .as_array()
            .unwrap()
            .iter()
            .map(|h| h["HeroIndex"].as_i64().unwrap())
            .collect::<Vec<_>>(),
        vec![3, 4]
    );
    assert_eq!(recruits["HeroInfos"][0]["Level"], 3);
    assert_eq!(recruits["EquipItemResults"].as_array().unwrap().len(), 2);
    let gem = recruits["CurrencyResults"]
        .as_array()
        .unwrap()
        .iter()
        .find(|r| r["CurrencyType"] == "Gem")
        .unwrap();
    assert_eq!(gem["AddValue"], 37);
    assert_eq!(gem["Field1"], "NewSysGem");
    let again = login(&state).await;
    assert!(!again.is_new_user);
    assert!(!again.is_tutorial_skip);
    assert_eq!(again.heroes.len(), 4);
    assert_eq!(again.tutorials.len(), 3);
    assert_eq!(again.equip_items.len(), 2);
    assert_eq!(again.user_info.gem, first.user_info.gem + 37);
}

#[tokio::test]
async fn begin_never_clears_or_rewards_a_battle() {
    let (state, first) = setup().await;
    let _ = begin_tutorial(
        State(state.clone()),
        Form(request(&first.user_info.session_key, 10010)),
    )
    .await
    .unwrap();
    let clears: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM campaign_progress")
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(clears, 0);
    let again = login(&state).await;
    assert!(again.tutorials.is_empty());
    assert_eq!(again.user_info.gold, first.user_info.gold);
    assert_eq!(again.heroes.len(), 1);
}

#[tokio::test]
async fn concurrent_completion_grants_once_and_replays_response() {
    let (state, first) = setup().await;
    let key = &first.user_info.session_key;
    let (left, right) = tokio::join!(complete(&state, key, 10001), complete(&state, key, 10001));
    assert_eq!(left, right);
    let current = login(&state).await;
    assert_eq!(current.user_info.gold, first.user_info.gold + 1200);
    assert_eq!(current.heroes.len(), 3);
    assert_eq!(current.equip_items.len(), 2);
    let replay = complete(&state, key, 10001).await;
    assert_eq!(left, replay);
}

#[tokio::test]
async fn failure_rolls_back_completion_and_partial_rewards() {
    let (mut state, first) = setup().await;
    let table = Arc::make_mut(&mut Arc::make_mut(&mut state.tables).tutorials);
    table.support.items.get_mut(&4).unwrap().star = 99; // Fail after inserting heroes, before granting team EXP.
    assert!(complete_tutorial(
        State(state.clone()),
        Form(request(&first.user_info.session_key, 10001))
    )
    .await
    .is_err());
    let current = login(&state).await;
    assert_eq!(current.heroes.len(), 1);
    assert!(current.tutorials.is_empty());
    assert_eq!(current.user_info.team_exp, first.user_info.team_exp);
    assert_eq!(current.user_info.team_level, first.user_info.team_level);
    state.tables = Arc::new(tables());
    assert_eq!(
        complete(&state, &first.user_info.session_key, 10001).await["HeroInfos"]
            .as_array()
            .unwrap()
            .len(),
        2
    );
}

#[tokio::test]
async fn unknown_missing_and_unauthenticated_requests_do_not_write() {
    let (state, first) = setup().await;
    for req in [
        request("bad-key", 1000),
        request(&first.user_info.session_key, -1),
        TutorialRequest {
            tutorial_index: None,
            ..request(&first.user_info.session_key, 1)
        },
    ] {
        assert!(complete_tutorial(State(state.clone()), Form(req))
            .await
            .is_err());
    }
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM tutorial_progress")
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(count, 0);
    let req: TutorialRequest = serde_urlencoded::from_str(&format!(
        "SessionId={}&TutorialIndex=1000",
        first.user_info.session_key
    ))
    .unwrap();
    assert!(begin_tutorial(State(state), Form(req)).await.is_ok());
}

#[tokio::test]
async fn every_shipped_tutorial_reward_is_supported() {
    let (state, first) = setup().await;
    let mut indices: Vec<_> = state.tables.tutorials.definitions.keys().copied().collect();
    indices.sort();
    for index in indices {
        let response = complete_tutorial(
            State(state.clone()),
            Form(request(&first.user_info.session_key, index)),
        )
        .await;
        assert!(response.is_ok(), "tutorial {index}: {response:?}");
    }
    let current = login(&state).await;
    assert!(current
        .heroes
        .iter()
        .any(|h| h.hero_index == 15 && h.level == 20));
    assert!(current.heroes.iter().any(|h| h.hero_index == 111));
    assert!(current
        .heroes
        .iter()
        .any(|h| h.hero_index == 88 && h.level == 50));
    assert!(current.user_info.event_dungeon_point > 0);
    assert_eq!(current.user_info.raid_point, 400);
    assert!(current
        .equip_items
        .iter()
        .any(|e| e.item_index == 81104 && e.option_index_4 == 805));
    assert!(current
        .equip_items
        .iter()
        .any(|e| e.item_index == 51401 && e.option_step_1 == 10));
}

#[tokio::test]
async fn skip_persists_without_overwriting_owned_heroes() {
    let (state, first) = setup().await;
    sqlx::query("UPDATE heroes SET level = 25 WHERE account_id = ?")
        .bind(first.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    let req = request(&first.user_info.session_key, 0);
    let _ = skip_tutorial(State(state.clone()), Form(req))
        .await
        .unwrap();
    let current = login(&state).await;
    assert!(current.is_tutorial_skip);
    assert_eq!(current.heroes.len(), 4);
    assert_eq!(
        current
            .heroes
            .iter()
            .find(|h| h.hero_index == 1)
            .unwrap()
            .level,
        25
    );
    assert_eq!(current.chapter_dungeons[0].max_star, 0);
    let _ = skip_tutorial(
        State(state.clone()),
        Form(request(&first.user_info.session_key, 0)),
    )
    .await
    .unwrap();
    let repeated = login(&state).await;
    assert_eq!(repeated.user_info.team_level, current.user_info.team_level);
    assert_eq!(repeated.user_info.team_exp, current.user_info.team_exp);
}

#[tokio::test]
async fn migration_preserves_legacy_skip_and_new_account_opt_in() {
    let (state, first) = setup().await;
    sqlx::query("INSERT INTO accounts (login_id, nick) VALUES ('legacy', 'Legacy')")
        .execute(&state.db)
        .await
        .unwrap();
    database::create_tables(&state.db).await.unwrap();
    database::create_tables(&state.db).await.unwrap();
    let legacy: bool = sqlx::query_scalar("SELECT is_skipped FROM tutorial_settings WHERE account_id = (SELECT account_id FROM accounts WHERE login_id = 'legacy')").fetch_one(&state.db).await.unwrap();
    assert!(legacy);
    assert!(!login(&state).await.is_tutorial_skip);
    sqlx::query("INSERT INTO tutorial_progress (account_id, tutorial_index, is_completed, completed_time) VALUES (?, 10001, 1, '2025-01-01 00:00:00')")
        .bind(first.user_info.account_id).execute(&state.db).await.unwrap();
    let response = complete(&state, &first.user_info.session_key, 10001).await;
    assert_eq!(response["Info"]["CompletedTime"], "2025-01-01 00:00:00");
    assert_eq!(login(&state).await.heroes.len(), 1);
}

#[tokio::test]
async fn dungeon_completion_uses_table_and_preserves_existing_clear() {
    let (state, first) = setup().await;
    let key = &first.user_info.session_key;
    let response = complete(&state, key, 10310).await;
    assert_eq!(response["DungeonInfos"][0]["DungeonIndex"], 8);
    sqlx::query("UPDATE campaign_progress SET clear_count = 7, completed_time = '2025-01-01 00:00:00' WHERE dungeon_id = 8").execute(&state.db).await.unwrap();
    let count: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM campaign_progress WHERE dungeon_id = 5")
            .fetch_one(&state.db)
            .await
            .unwrap();
    assert_eq!(count, 0);
    complete(&state, key, 10310).await;
    let clear_count: i64 =
        sqlx::query_scalar("SELECT clear_count FROM campaign_progress WHERE dungeon_id = 8")
            .fetch_one(&state.db)
            .await
            .unwrap();
    assert_eq!(clear_count, 7);
}

#[tokio::test]
async fn old_receipts_return_current_inventory_and_hero_state() {
    let (state, first) = setup().await;
    let key = &first.user_info.session_key;
    let original = complete(&state, key, 10001).await;
    sqlx::query(
        "UPDATE user_info SET gold = 7, gem = 9, pay_gem = 11, team_level = 15, team_exp = 23",
    )
    .execute(&state.db)
    .await
    .unwrap();
    sqlx::query("UPDATE heroes SET level = 20, star = 3, skill_level_1 = 5 WHERE hero_index = 3")
        .execute(&state.db)
        .await
        .unwrap();
    sqlx::query("UPDATE equip_items SET level = 8, locked = 1")
        .execute(&state.db)
        .await
        .unwrap();
    let replay = complete(&state, key, 10001).await;
    assert_eq!(replay["Info"], original["Info"]);
    assert_eq!(replay["CurrencyResults"][0]["NewValue"], 7);
    assert_eq!(replay["CurrencyResults"][1]["Value1"], 9);
    assert_eq!(replay["CurrencyResults"][1]["Value2"], 11);
    assert_eq!(replay["HeroInfos"][0]["Level"], 20);
    assert_eq!(replay["HeroInfos"][0]["SkillLevel1"], 5);
    assert_eq!(replay["EquipItemResults"][0]["Level"], 8);
    assert_eq!(replay["EquipItemResults"][0]["Locked"], 1);
    assert_eq!(replay["TeamExpResultInfos"][0]["NewLevel"], 15);
    assert_eq!(replay["TeamExpResultInfos"][0]["NewValue"], 23);
    let books = complete(&state, key, 10030).await;
    assert_eq!(books["ItemResults"][0]["AddCount"], 20);
    sqlx::query("UPDATE items SET count = 2")
        .execute(&state.db)
        .await
        .unwrap();
    assert_eq!(
        complete(&state, key, 10030).await["ItemResults"][0]["NewCount"],
        2
    );
}

#[tokio::test]
async fn custom_equipment_survives_relogin_and_replay() {
    let (state, first) = setup().await;
    let original = complete(&state, &first.user_info.session_key, 9500).await;
    assert_eq!(original["EquipItemResults"][0]["OptionRenewCount4"], 1);
    assert_eq!(original["EquipItemResults"][0]["IsRenewedOption4"], 1);
    let current = login(&state).await;
    assert_eq!(current.equip_items[0].option_renew_count_4, 1);
    assert_eq!(current.equip_items[0].is_renewed_option_4, 1);
    assert_eq!(
        original,
        complete(&state, &first.user_info.session_key, 9500).await
    );
}

#[tokio::test]
async fn gm_reset_restarts_tutorial_with_kasel() {
    use crate::api::system::cheat::{
        gm_reset_account, gm_unlock_all, GmResetAccountRequest, GmUnlockAllRequest,
    };
    let (state, first) = setup().await;
    let key = &first.user_info.session_key;
    complete(&state, key, 1000).await;
    complete(&state, key, 10001).await;
    let _ = gm_unlock_all(
        State(state.clone()),
        Form(GmUnlockAllRequest {
            session_id: Some(key.clone()),
        }),
    )
    .await
    .unwrap();
    assert!(login(&state).await.is_tutorial_skip);
    let _ = gm_reset_account(
        State(state.clone()),
        Form(GmResetAccountRequest {
            session_id: Some(key.clone()),
            keep_heroes: Some(false),
        }),
    )
    .await
    .unwrap();
    let current = login(&state).await;
    assert!(!current.is_tutorial_skip);
    assert!(current.tutorials.is_empty());
    assert!(current.chapter_dungeons.is_empty());
    assert!(current.equip_items.is_empty());
    assert_eq!(current.heroes.len(), 1);
    assert_eq!(current.heroes[0].hero_index, 1);
    assert_eq!(current.user_info.team_level, 1);
    assert_eq!(
        complete(&state, key, 1000).await["HeroInfos"][0]["HeroIndex"],
        2
    );
}

#[tokio::test]
async fn failed_new_account_creation_is_atomic() {
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let state = AppState::new(pool, GameTables::empty());
    assert!(user::login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=missing-data")
    )
    .await
    .is_err());
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM accounts")
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(count, 0);
}

#[test]
fn hero_exp_uses_local_levels_and_star_cap() {
    let data = tables();
    let levels = &data.tutorials.support.hero_levels;
    assert_eq!(add_exp(levels, 1, 0, 15, 30), (2, 0));
    assert_eq!(add_exp(levels, 2, 5, 110, 30), (4, 0));
    assert_eq!(add_exp(levels, 29, 0, 1_000_000, 30), (30, 0));
}

#[tokio::test]
async fn side_tutorial_difficulty_matches_relogin() {
    let (state, first) = setup().await;
    for index in [70001, 200100, 200101] {
        let response = complete(&state, &first.user_info.session_key, index).await;
        let dungeon = &response["DungeonInfos"][0];
        let current = login(&state).await;
        let saved = current
            .chapter_dungeons
            .iter()
            .find(|node| {
                Some(node.chapter_index as i64) == dungeon["ChapterIndex"].as_i64()
                    && Some(node.dungeon_index as i64) == dungeon["DungeonIndex"].as_i64()
            })
            .unwrap();
        assert_eq!(json!(saved.max_star), dungeon["MaxStar"]);
        assert_eq!(
            json!(saved.first_rewarded_diff),
            dungeon["FirstRewardedDiff"]
        );
    }
}
