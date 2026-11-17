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
    let db = crate::database::test_pool().await;
    let tables = crate::tables::test_tables();
    let state = AppState::new(db, tables);
    for id in [1i64,2] {
        crate::db::query("INSERT INTO accounts(account_id,login_id,nick) VALUES($1,$2,$3)").bind(id).bind(format!("player{id}")).bind(format!("Raider{id}")).execute(&state.db).await.unwrap();
        crate::db::query("INSERT INTO user_info(account_id) VALUES($1)").bind(id).execute(&state.db).await.unwrap();
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
    let untouched:i64=crate::db::query_scalar("SELECT gold FROM user_info WHERE account_id=1").fetch_one(&state.db).await.unwrap();
    assert_eq!(untouched,0);
    assert_eq!(call(admin.clone(),"/cheat/level",Some(key),"AccountId=2&Level=12").await.0,StatusCode::OK);
    let level:i64=crate::db::query_scalar("SELECT team_level FROM user_info WHERE account_id=2").fetch_one(&state.db).await.unwrap();
    assert_eq!(level,12);
    for body in ["AccountId=0", "AccountId=-1", "", "AccountId=2&SessionId=player-session"] {
        assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),body).await.0,StatusCode::BAD_REQUEST);
    }
    assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),"AccountId=999").await.0,StatusCode::NOT_FOUND);
    assert_eq!(call(admin.clone(),"/cheat/currency",Some(key),"SessionId=player-session&Gold=5").await.0,StatusCode::OK);
    let (status,players)=call(admin.clone(),"/cheat/players",Some(key),"Query=raider2").await;
    assert_eq!(status,StatusCode::OK);assert_eq!(players["Players"].as_array().unwrap().len(),1);
    assert_eq!(players["Players"][0]["AccountId"],2);
    assert!(players["Players"][0].get("SessionId").is_none());
    assert_eq!(players["Players"][0]["Online"],false,"{players}");
    // Account 1 holds the active game session "player-session".
    let (_,all)=call(admin,"/cheat/players",Some(key),"").await;
    assert_eq!(all["Players"][0]["AccountId"],1);assert_eq!(all["Players"][0]["Online"],true,"{all}");
}

#[tokio::test]
async fn hero_grants_allocate_free_ids_after_story_recruits() {
    // Story recruits get sequential hero_ids, so one can hold the ID equal to
    // another hero's index; granting that hero used to hit the unique key.
    let state = AppState::new(crate::database::test_pool().await, crate::tables::test_tables());
    crate::db::query("INSERT INTO accounts(account_id,login_id,nick) VALUES(1,'player1','Raider1')").execute(&state.db).await.unwrap();
    crate::db::query("INSERT INTO user_info(account_id) VALUES(1)").execute(&state.db).await.unwrap();
    for (hero_id, hero_index) in [(1, 1), (2, 2), (3, 3), (4, 4), (5, 20)] {
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level) VALUES(1,$1,$2,1,1)")
            .bind(hero_id).bind(hero_index).execute(&state.db).await.unwrap();
    }
    let key = "test-only-administrator-key-123456789";
    let admin = admin_routes(state.clone(), key.into());
    let (status, single) = call(admin.clone(), "/cheat/hero", Some(key), "AccountId=1&HeroId=5&Level=10&Star=3").await;
    assert_eq!(status, StatusCode::OK, "{single}");
    let (status, again) = call(admin.clone(), "/cheat/hero", Some(key), "AccountId=1&HeroId=5").await;
    assert_eq!(status, StatusCode::OK, "{again}");
    assert_eq!(again["UniqueHeroId"], single["UniqueHeroId"]);
    let (status, all) = call(admin.clone(), "/cheat/allheroes", Some(key), "AccountId=1").await;
    assert_eq!(status, StatusCode::OK, "{all}");
    assert!(all["HeroesAdded"].as_i64().unwrap() > 0);
    let (status, repeat) = call(admin, "/cheat/allheroes", Some(key), "AccountId=1").await;
    assert_eq!(status, StatusCode::OK, "{repeat}");
    assert_eq!(repeat["HeroesAdded"], 0);
    let (owned, distinct_ids, distinct_indices, clause): (i64, i64, i64, i64) = crate::db::query_as(
        "SELECT COUNT(*), COUNT(DISTINCT hero_id), COUNT(DISTINCT hero_index), MAX(hero_id) FILTER (WHERE hero_index=20) FROM heroes WHERE account_id=1")
        .fetch_one(&state.db).await.unwrap();
    assert_eq!((owned, distinct_ids, distinct_indices), (owned, owned, owned));
    assert_eq!(clause, 5, "existing heroes keep their IDs");
}

#[tokio::test]
async fn all_heroes_grants_and_raises_transcendence_without_lowering() {
    let state = AppState::new(crate::database::test_pool().await, crate::tables::test_tables());
    crate::db::query("INSERT INTO accounts(account_id,login_id,nick) VALUES(1,'player1','Raider1')").execute(&state.db).await.unwrap();
    crate::db::query("INSERT INTO user_info(account_id) VALUES(1)").execute(&state.db).await.unwrap();
    // An owned 5-star hero (from an earlier grant) and a hero levelled past the request.
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level) VALUES(1,1,1,5,90),(1,2,2,5,100)").execute(&state.db).await.unwrap();
    let key = "test-only-administrator-key-123456789";
    let admin = admin_routes(state.clone(), key.into());
    for bad in ["Star=4&Transcend=1", "Star=5&Transcend=6", "Star=0"] {
        assert_ne!(call(admin.clone(), "/cheat/allheroes", Some(key), &format!("AccountId=1&{bad}")).await.0, StatusCode::OK, "{bad}");
    }
    let (status, out) = call(admin.clone(), "/cheat/allheroes", Some(key), "AccountId=1&Level=90&Star=5&Transcend=5").await;
    assert_eq!(status, StatusCode::OK, "{out}");
    assert_eq!(out["HeroesUpgraded"], 2, "both owned 5-star heroes are transcended");
    let below: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM heroes WHERE account_id=1 AND (star<>5 OR transcend<>5 OR level<90)")
        .fetch_one(&state.db).await.unwrap();
    assert_eq!(below, 0);
    let kept: (i32, i32) = crate::db::query_as("SELECT level, hero_id FROM heroes WHERE account_id=1 AND hero_index=2").fetch_one(&state.db).await.unwrap();
    assert_eq!(kept, (100, 2));
    let (_, again) = call(admin, "/cheat/allheroes", Some(key), "AccountId=1&Star=5&Transcend=5").await;
    assert_eq!((again["HeroesAdded"].as_i64(), again["HeroesUpgraded"].as_i64()), (Some(0), Some(0)));
}
