use super::tests::{account, call, setup};
use super::*;

const CLAIM: &str = "maze_tower/receive_maze_tower_reward";
const INFO: &str = "maze_tower/get_maze_tower_info";

async fn floors(s: &AppState, a: i64, total: i64) {
    let mut db = s.db.acquire().await.unwrap();
    for (index, id) in (1001..=1007).enumerate() {
        let mut info = dungeons::tower(&mut db, s, a, id).await.unwrap();
        info["CompletedFloor"] = json!((total - index as i64 * 5).clamp(0, 5));
        put(&mut db, a, "tower", id, &info).await.unwrap();
    }
}

async fn fragments(s: &AppState, a: i64) -> i64 {
    sqlx::query_scalar("SELECT COALESCE(SUM(count),0) FROM items WHERE account_id=? AND item_index=45039")
        .bind(a).fetch_one(&s.db).await.unwrap()
}

#[tokio::test]
async fn royal_labyrinth_22_cleared_floors_claim_three_native_fragment_milestones() {
    let (s, u) = setup().await;
    let a = account(&u);
    floors(&s, a, 22).await;
    let before = fragments(&s, a).await;
    assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], 0);
    // Even with enough total clears, claims must advance in native UI order.
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=3").await["Result"], "NotCleared");
    let mut awarded = 0;
    for (step, count) in [(1, 500), (2, 525), (3, 550)] {
        let args = format!("MazeTowerRewardIndex={step}");
        let reward = call(&s, &u, CLAIM, &args).await;
        assert_eq!(reward["Result"], "Success", "{reward}");
        assert!(reward["ItemResults"].as_array().unwrap().iter()
            .any(|r| n(r, "ItemIndex") == 45039 && n(r, "AddCount") == count));
        awarded += count;
        assert_eq!(fragments(&s, a).await, before + awarded);
        assert_eq!(call(&s, &u, CLAIM, &args).await["Result"], "AlreadyRewarded");
        assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], step);
    }
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=4").await["Result"], "NotCleared");
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=999").await["Result"], "RewardDataNotFound");
    let login = json!(crate::api::account::user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
        .await.unwrap().0);
    assert_eq!(call(&s, &login, INFO, "").await["MazeTowerRewardStep"], 3);
    assert_eq!(call(&s, &login, CLAIM, "MazeTowerRewardIndex=3").await["Result"], "AlreadyRewarded");
    assert_eq!(fragments(&s, a).await, before + 1575);
}

#[tokio::test]
async fn royal_labyrinth_counts_only_current_cycle_class_tower_clears() {
    let (s, u) = setup().await;
    let a = account(&u);
    floors(&s, a, 6).await;
    let mut db = s.db.acquire().await.unwrap();
    let mut challenge = dungeons::tower(&mut db, &s, a, 21).await.unwrap();
    challenge["CompletedFloor"] = json!(80);
    put(&mut db, a, "tower", 21, &challenge).await.unwrap();
    drop(db);
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1").await["Result"], "NotCleared");
    floors(&s, a, 7).await;
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1").await["Result"], "Success");
}

#[tokio::test]
async fn royal_labyrinth_resets_claims_with_towers_and_can_redeem_all_five_steps() {
    let (s, u) = setup().await;
    let a = account(&u);
    let mut db = s.db.acquire().await.unwrap();
    put(&mut db, a, "maze_step", 0, &json!({"Step":5,"Period":"1900-01-16"})).await.unwrap();
    for id in 1001..=1007 {
        put(&mut db, a, "tower", id, &json!({"TowerIndex":id,"CompletedFloor":5,"Period":"1900-01-16"})).await.unwrap();
    }
    sqlx::query("INSERT INTO battle_reward_claims(account,kind,idx,period) VALUES(?,'maze',1,'1900-01-16')")
        .bind(a).execute(&mut *db).await.unwrap();
    drop(db);
    assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], 0);
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1").await["Result"], "NotCleared");
    floors(&s, a, 35).await;
    let before = fragments(&s, a).await;
    for step in 1..=5 {
        assert_eq!(call(&s, &u, CLAIM, &format!("MazeTowerRewardIndex={step}")).await["Result"], "Success");
    }
    assert_eq!(fragments(&s, a).await, before + 2750);
    assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], 5);
}

#[tokio::test]
async fn royal_labyrinth_migrates_legacy_daily_claim_without_regranting_it() {
    let (s, u) = setup().await;
    let a = account(&u);
    floors(&s, a, 14).await;
    let mut db = s.db.acquire().await.unwrap();
    let period = dungeons::tower(&mut db, &s, a, 1001).await.unwrap()["Period"].as_str().unwrap().to_owned();
    // The old implementation saved no cycle alongside its step and used a
    // calendar date for its claim receipt. Preserve an earlier day this cycle.
    put(&mut db, a, "maze_step", 0, &json!({"Step":1})).await.unwrap();
    sqlx::query("INSERT INTO battle_reward_claims(account,kind,idx,period) VALUES(?,'maze',1,?)")
        .bind(a).bind(period).execute(&mut *db).await.unwrap();
    drop(db);
    let before = fragments(&s, a).await;
    assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], 1);
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1").await["Result"], "AlreadyRewarded");
    assert_eq!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=2").await["Result"], "Success");
    assert_eq!(fragments(&s, a).await, before + 525);
}

#[tokio::test]
async fn royal_labyrinth_failed_grant_rolls_back_and_concurrent_claims_pay_once() {
    let (mut s, u) = setup().await;
    let a = account(&u);
    floors(&s, a, 7).await;
    let before = fragments(&s, a).await;
    let table = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle);
    table.tables.get_mut("MazeClearReward").unwrap()[0]["RewardIndex"] = json!(i32::MAX);
    assert_ne!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1").await["Result"], "Success");
    assert_eq!(fragments(&s, a).await, before);
    assert_eq!(call(&s, &u, INFO, "").await["MazeTowerRewardStep"], 0);
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_reward_claims WHERE account=? AND kind='maze'")
        .bind(a).fetch_one(&s.db).await.unwrap(), 0);
    let table = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle);
    table.tables.get_mut("MazeClearReward").unwrap()[0]["RewardIndex"] = json!(1300301);
    let (first, second) = tokio::join!(call(&s, &u, CLAIM, "MazeTowerRewardIndex=1"), call(&s, &u, CLAIM, "MazeTowerRewardIndex=1"));
    assert_eq!([&first, &second].iter().filter(|r| r["Result"] == "Success").count(), 1);
    assert_eq!([&first, &second].iter().filter(|r| r["Result"] == "AlreadyRewarded").count(), 1);
    assert_eq!(fragments(&s, a).await, before + 500);
}
