//! Native forms and real client table rules against isolated databases.
use crate::api::{
    account::user,
    battle::campaign_handlers as campaign,
    heroes as hero,
    inventory::{item, shop},
};
use crate::models::item::ItemGrant;
use crate::state::AppState;
use axum::{body::Bytes, extract::State};
use serde_json::Value;

async fn inn_refresh(s: &AppState, u: &user::LoginResponse) -> hero::inn::RequestNewFriendlyHeroResponse {
    hero::inn::request_new_friendly_hero(State(s.clone()), axum::extract::Form(hero::inn::RequestNewFriendlyHeroRequest {
        session_id: None, session_key: Some(u.user_info.session_key.clone()),
    })).await.unwrap().0
}

async fn inn_action(s: &AppState, u: &user::LoginResponse, hero_index: i32, action: &str) -> hero::inn::DoHeroFriendlyResponse {
    hero::inn::do_hero_friendly(State(s.clone()), axum::extract::Form(hero::inn::DoHeroFriendlyRequest {
        session_id: None, session_key: Some(u.user_info.session_key.clone()),
        hero_index: Some(hero_index.to_string()), action: Some(action.into()),
    })).await.unwrap().0
}

#[tokio::test]
async fn inn_end_relationship_preserves_visitors_and_waits_for_explicit_selection() {
    let (s, u) = setup().await; let account = u.user_info.account_id;
    let first = inn_refresh(&s, &u).await.friendly_info.unwrap();
    let visitors: Vec<i32> = first.selected_hero_indice.as_ref().unwrap().split(',').map(|v| v.parse().unwrap()).collect();
    assert_eq!(visitors[0], first.hero_index);
    assert_eq!(inn_action(&s, &u, first.hero_index, "Greeting").await.result, "Success");
    crate::db::query("UPDATE hero_friendly_info SET last_roulette_time='2026-11-04 00:00:01' WHERE account_id=$1")
        .bind(account).execute(&s.db).await.unwrap();
    let change = |id| axum::extract::Form(hero::inn::ChangeRecruitHeroRequest {
        session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some(id),
    });
    for _ in 0..2 {
        let ended = hero::inn::change_recruit_hero(State(s.clone()), change(0)).await.unwrap().0;
        assert_eq!(ended.result, "Success");
        let info = ended.friendly_info.unwrap();
        assert_eq!((info.hero_index, info.selected_hero_index, info.friendly_point), (0, 0, 0));
        assert_eq!(info.selected_hero_indice, first.selected_hero_indice);
        assert_eq!(info.selected_time, first.selected_time);
        assert_eq!(info.last_roulette_time.as_deref(), Some("2026-11-04 00:00:01"));
        assert!(info.last_greeting_time.is_none() && info.last_conversation_time.is_none() && info.last_gift_time.is_none());
        let reopened = inn_refresh(&s, &u).await.friendly_info.unwrap();
        assert_eq!(serde_json::to_value(reopened).unwrap(), serde_json::to_value(info).unwrap());
        // The old hero can neither gain recruitment points nor be recruited
        // again until the player deliberately starts a relationship.
        assert_eq!(inn_action(&s, &u, first.hero_index, "Greeting").await.result, "HeroIndexMismatch");
    }
    let next = visitors[1];
    let started = hero::inn::change_recruit_hero(State(s.clone()), change(next)).await.unwrap().0;
    assert_eq!(started.result, "Success");
    let info = started.friendly_info.unwrap();
    assert_eq!((info.hero_index, info.selected_hero_index, info.friendly_point), (next, next, 0));
    assert_eq!(info.selected_hero_indice, first.selected_hero_indice);
    assert_eq!(inn_action(&s, &u, next, "Greeting").await.result, "Success");
    // A daily visitor rotation must not silently undo an ended relationship.
    let _ = hero::inn::change_recruit_hero(State(s.clone()), change(0)).await.unwrap();
    crate::db::query("UPDATE hero_friendly_info SET selected_time='2000-01-01 00:00:00' WHERE account_id=$1")
        .bind(account).execute(&s.db).await.unwrap();
    let rotated = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!((rotated.hero_index, rotated.selected_hero_index, rotated.friendly_point), (0, 0, 0));
    assert_eq!(rotated.selected_hero_indice.as_ref().unwrap().split(',').count(), 6);
    assert_eq!(&rotated.selected_time.as_ref().unwrap()[..10], s.server_date());
    let reopened = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!(serde_json::to_value(reopened).unwrap(), serde_json::to_value(rotated).unwrap());
}

#[tokio::test]
async fn inn_owned_cooldowns_are_per_hero_action_persist_and_reset_daily() {
    let (s, u) = setup().await; let account = u.user_info.account_id;
    inn_refresh(&s, &u).await;
    crate::db::query("UPDATE user_info SET friendship_point=1000 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    for id in [1, 2] {
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,$2,$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
            .bind(account).bind(100000 + id).bind(id).bind(account).bind(id).execute(&s.db).await.unwrap();
    }
    for (action, wire) in [("Closeness_Greeting", "LastGreetingTime"), ("Closeness_Conversation", "LastConversationTime"), ("Closeness_Gift", "LastGiftTime")] {
        let first = inn_action(&s, &u, 1, action).await;
        assert_eq!(first.result, "Success");
        let hero = serde_json::to_value(first.hero_info.unwrap()).unwrap();
        assert_eq!(&hero[wire].as_str().unwrap()[..10], s.server_date());
        let before: (i64, i64, i32) = crate::db::query_as("SELECT u.gold,u.friendship_point,h.closeness FROM user_info u JOIN heroes h ON h.account_id=u.account_id WHERE u.account_id=$1 AND h.hero_index=1")
            .bind(account).fetch_one(&s.db).await.unwrap();
        let repeat = inn_action(&s, &u, 1, action).await;
        assert_eq!(repeat.result, "ActionNotReady");
        assert!(repeat.hero_info.is_none() && repeat.currency_result.is_none() && repeat.friendship_point_result.is_none());
        let after: (i64, i64, i32) = crate::db::query_as("SELECT u.gold,u.friendship_point,h.closeness FROM user_info u JOIN heroes h ON h.account_id=u.account_id WHERE u.account_id=$1 AND h.hero_index=1")
            .bind(account).fetch_one(&s.db).await.unwrap();
        assert_eq!(before, after);
        // A different hero has an independent cooldown for this same action.
        assert_eq!(inn_action(&s, &u, 2, action).await.result, "Success");
        let snapshots = hero::snapshot(&mut s.db.acquire().await.unwrap(), account).await.unwrap();
        let restored = serde_json::to_value(snapshots.iter().find(|h| h.hero_index==1).unwrap()).unwrap();
        assert_eq!(restored[wire], hero[wire]);
        let mut details = hero::details(&mut s.db.acquire().await.unwrap(), account, 1).await.unwrap();
        details[wire] = serde_json::json!("2000-01-01 00:00:00");
        hero::save_details(&mut s.db.acquire().await.unwrap(), account, 1, &details).await.unwrap();
        assert_eq!(inn_action(&s, &u, 1, action).await.result, "Success");
    }
    // Using recruitment action names cannot bypass an owned hero's cooldowns.
    assert_eq!(inn_action(&s, &u, 1, "Greeting").await.result, "ActionNotExist");
}

#[tokio::test]
async fn inn_recruitment_actions_have_independent_daily_cooldowns() {
    let (s, u) = setup().await; let account = u.user_info.account_id;
    let recruit = inn_refresh(&s, &u).await.friendly_info.unwrap().hero_index;
    crate::db::query("UPDATE user_info SET friendship_point=1000 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    for (action, column) in [("Greeting", "last_greeting_time"), ("Conversation", "last_conversation_time"), ("Gift", "last_gift_time")] {
        assert_eq!(inn_action(&s, &u, recruit, action).await.result, "Success");
        let before: (i64, i64, i32) = crate::db::query_as("SELECT u.gold,u.friendship_point,i.friendly_point FROM user_info u JOIN hero_friendly_info i ON i.account_id=u.account_id WHERE u.account_id=$1")
            .bind(account).fetch_one(&s.db).await.unwrap();
        assert_eq!(inn_action(&s, &u, recruit, action).await.result, "ActionNotReady");
        let after: (i64, i64, i32) = crate::db::query_as("SELECT u.gold,u.friendship_point,i.friendly_point FROM user_info u JOIN hero_friendly_info i ON i.account_id=u.account_id WHERE u.account_id=$1")
            .bind(account).fetch_one(&s.db).await.unwrap();
        assert_eq!(before, after);
        crate::db::query(sqlx::AssertSqlSafe(format!("UPDATE hero_friendly_info SET {column}='2000-01-01 00:00:00' WHERE account_id=$1")))
            .bind(account).execute(&s.db).await.unwrap();
        assert_eq!(inn_action(&s, &u, recruit, action).await.result, "Success");
    }
}

#[tokio::test]
async fn inn_concurrent_gifts_charge_once_and_failed_updates_roll_back() {
    let (s, u) = setup().await; let account = u.user_info.account_id;
    inn_refresh(&s, &u).await;
    crate::db::query("UPDATE user_info SET gold=100000 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    // Simulate a failure after charging currency, before persisting the cooldown.
    crate::database::fail_trigger(&s.db, "fail_inn_details", "BEFORE INSERT ON hero_details", None, "fixture detail failure").await;
    let request = || axum::extract::Form(hero::inn::DoHeroFriendlyRequest {
        session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some("1".into()), action: Some("Closeness_Gift".into()),
    });
    assert!(hero::inn::do_hero_friendly(State(s.clone()), request()).await.is_err());
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gold FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap(), 100000);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT closeness FROM heroes WHERE account_id=$1 AND hero_index=1").bind(account).fetch_one(&s.db).await.unwrap(), 0);
    crate::database::drop_fail_trigger(&s.db, "fail_inn_details", "hero_details").await;
    let (left, right) = tokio::join!(inn_action(&s, &u, 1, "Closeness_Gift"), inn_action(&s, &u, 1, "Closeness_Gift"));
    assert_eq!([&left.result, &right.result].iter().filter(|r| r.as_str()=="Success").count(), 1);
    assert_eq!([&left.result, &right.result].iter().filter(|r| r.as_str()=="ActionNotReady").count(), 1);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT gold FROM user_info WHERE account_id=$1").bind(account).fetch_one(&s.db).await.unwrap(), 75000);
    assert_eq!(crate::db::query_scalar::<i32>("SELECT closeness FROM heroes WHERE account_id=$1 AND hero_index=1").bind(account).fetch_one(&s.db).await.unwrap(), 100);
}

#[tokio::test]
async fn inn_all_owned_max_closeness_keeps_visits_greetings_and_roulette() {
    for fresh in [false, true] {
        let (s, u) = setup().await;
        let account = u.user_info.account_id;
        if !fresh { inn_refresh(&s, &u).await; }
        // Include the whole supported roster, not only the smaller recruit pool.
        for id in s.tables.hero_shop.inn_recruitment_points.keys() {
            crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,$2,$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
                .bind(account).bind(100000 + id).bind(id).bind(account).bind(id).execute(&s.db).await.unwrap();
        }
        crate::db::query("UPDATE heroes SET closeness=1000 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
        let response = inn_refresh(&s, &u).await;
        assert_eq!(response.result, "Success");
        let info = response.friendly_info.unwrap();
        assert_eq!((info.hero_index, info.selected_hero_index, info.friendly_point), (0, 0, 0));
        let visitors: Vec<i32> = info.selected_hero_indice.as_ref().unwrap().split(',').map(|id| id.parse().unwrap()).collect();
        assert_eq!(visitors.len(), 6);
        assert_eq!(visitors.iter().collect::<std::collections::HashSet<_>>().len(), 6);
        for id in &visitors {
            assert!(s.tables.hero_shop.inn_recruitment_points.contains_key(id));
            let greet = hero::inn::do_hero_friendly(State(s.clone()), axum::extract::Form(hero::inn::DoHeroFriendlyRequest {
                session_id: None, session_key: Some(u.user_info.session_key.clone()),
                hero_index: Some(id.to_string()), action: Some("Closeness_Greeting".into()),
            })).await.unwrap().0;
            assert_eq!(greet.result, "Success");
            let wire = serde_json::to_value(greet.hero_info.unwrap()).unwrap();
            assert_eq!(wire["ClosenessPoint"], 1000);
            assert!(wire.get("Closeness").is_none());
        }
        // Reopening must not reshuffle visitors, reset the day, or mark an owned
        // hero as an active recruit (which causes native request/scene loops).
        let again = inn_refresh(&s, &u).await.friendly_info.unwrap();
        assert_eq!(again.selected_hero_indice, info.selected_hero_indice);
        assert_eq!(again.selected_time, info.selected_time);
        assert_eq!(again.hero_index, 0);
        let roulette = hero::inn::request_hero_inn_roulette(State(s.clone()), axum::extract::Form(hero::inn::RequestHeroInnRouletteRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), roulette_index: Some("1".into()),
        })).await.unwrap().0.roulette_info.unwrap();
        assert_eq!((roulette.max_count, roulette.remain_count), (7, 7));
        for remaining in (0..7).rev() {
            let spin = hero::inn::give_reward_hero_inn_roulette(State(s.clone()), axum::extract::Form(hero::inn::GiveRewardHeroInnRouletteRequest {
                session_id: None, session_key: Some(u.user_info.session_key.clone()), roulette_index: Some("1".into()),
            })).await.unwrap().0;
            assert_eq!(spin.result, "Success");
            assert_eq!(spin.roulette_info.unwrap().remain_count, remaining);
        }
        let reset = hero::inn::change_recruit_hero(State(s.clone()), axum::extract::Form(hero::inn::ChangeRecruitHeroRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some(0),
        })).await.unwrap().0;
        assert_eq!(reset.result, "Success");
        assert_eq!(reset.friendly_info.unwrap().hero_index, 0);
        assert_eq!(inn_refresh(&s, &u).await.result, "Success");
        let exhausted = hero::inn::give_reward_hero_inn_roulette(State(s.clone()), axum::extract::Form(hero::inn::GiveRewardHeroInnRouletteRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), roulette_index: Some("1".into()),
        })).await.unwrap().0;
        assert_eq!(exhausted.result, "NoMoreRoulette");
        let recruit = hero::inn::recruit_hero(State(s.clone()), axum::extract::Form(hero::inn::RecruitHeroRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some(visitors[0]),
        })).await.unwrap().0;
        assert_eq!(recruit["Result"], "AlreadyRecruited");
    }
}

#[tokio::test]
async fn inn_last_recruit_keeps_progress_and_fills_empty_seats_with_owned_visitors() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    for id in crate::models::hero_inn::get_available_inn_hero_indices().into_iter().filter(|id| *id != 5) {
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,$2,$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
            .bind(account).bind(100000 + id).bind(id).bind(account).bind(id).execute(&s.db).await.unwrap();
    }
    let first = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!(first.hero_index, 5);
    crate::db::query("UPDATE hero_friendly_info SET friendly_point=350,selected_time='2000-01-01 00:00:00' WHERE account_id=$1")
        .bind(account).execute(&s.db).await.unwrap();
    let refreshed = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!((refreshed.hero_index, refreshed.friendly_point), (5, 350));
    let visitors: Vec<i32> = refreshed.selected_hero_indice.unwrap().split(',').map(|id| id.parse().unwrap()).collect();
    assert_eq!(visitors.len(), 6);
    assert_eq!(visitors[0], 5);
    for id in &visitors[1..] {
        assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM heroes WHERE account_id=$1 AND hero_index=$2")
            .bind(account).bind(id).fetch_one(&s.db).await.unwrap(), 1);
    }
}

#[tokio::test]
async fn restored_special_costume_sales_match_catalog_and_charge_once() {
    let (s,u)=setup().await;let account=u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gem=100000,pay_gem=0 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    for id in [112,214,1514,415,7211,7212,3514,6313] {
        let row=&s.tables.hero_shop.costumes[&id];let h=item::n(row,"HeroIndex");
        assert_eq!(row["Buyable"],true);assert_eq!(row["ReqBuyGem"],3000);
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,COALESCE((SELECT MAX(hero_id)+1 FROM heroes WHERE account_id=$2),1),$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
            .bind(account).bind(account).bind(h).bind(account).bind(h).execute(&s.db).await.unwrap();
        let before=balance(&s,&u,"gem").await;
        let args=|price|format!("HeroIndex={h}&CostumeIndex={id}&BuyGem={price}&BuyGold={price}&BuyMileage={price}");
        let bad=hero::buy_costume(State(s.clone()),form(&u,&args(2999))).await.unwrap().0;
        assert_ne!(bad["Result"],"Success");assert_eq!(balance(&s,&u,"gem").await,before);
        let result=hero::buy_costume(State(s.clone()),form(&u,&args(3000))).await.unwrap().0;
        assert_eq!(result["Result"],"Success","{id}: {result}");assert_eq!(balance(&s,&u,"gem").await,before-3000);
        assert_eq!(hero::buy_costume(State(s.clone()),form(&u,&args(3000))).await.unwrap().0["Result"],"AlreadyHaveCostume");
        assert_eq!(balance(&s,&u,"gem").await,before-3000);
    }
    // No other open, priced regular outfit may retain the same stale sale flag.
    for row in s.tables.hero_shop.costumes.values().filter(|r|r["IsOpen"]==true && item::n(r,"ReqBuyGem")>0 && r["IsDefault"]!=true) {
        if !matches!(row["BodyPrefix"].as_str(),Some("Trans"|"Trans_Hair01"|"Trial")) {
            assert_eq!(row["Buyable"],true,"Unpurchasable regular costume: {row}");
        }
    }
    for id in [180,190] {assert_ne!(s.tables.hero_shop.costumes[&id]["Buyable"],true);}
}

#[tokio::test]
async fn inn_friendship_uses_each_hero_target_and_unblocks_saved_1000_points() {
    for (index, required) in [(27, 1200), (6, 1200), (5, 1000)] {
        let (s, u) = setup().await;
        let account = u.user_info.account_id;
        inn_refresh(&s, &u).await;
        let selected = hero::inn::change_recruit_hero(State(s.clone()), axum::extract::Form(hero::inn::ChangeRecruitHeroRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some(index),
        })).await.unwrap().0;
        assert_eq!(selected.result, "Success");
        let recruit = || axum::extract::Form(hero::inn::RecruitHeroRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()), hero_index: Some(index),
        });
        let greet = || axum::extract::Form(hero::inn::DoHeroFriendlyRequest {
            session_id: None, session_key: Some(u.user_info.session_key.clone()),
            hero_index: Some(index.to_string()), action: Some("Greeting".to_string()),
        });
        // Reproduce a saved relationship stuck at 1000 for a 1200-point hero.
        crate::db::query("UPDATE hero_friendly_info SET friendly_point=$1 WHERE account_id=$2")
            .bind(required - 200).bind(account).execute(&s.db).await.unwrap();
        let early = hero::inn::recruit_hero(State(s.clone()), recruit()).await.unwrap().0;
        assert_eq!(early["Result"], "FriendlyPointMismatch");
        let action = hero::inn::do_hero_friendly(State(s.clone()), greet()).await.unwrap().0;
        assert_eq!(action.result, "Success");
        assert_eq!(action.friendly_info.unwrap().friendly_point, required - 150);
        assert_eq!(inn_refresh(&s, &u).await.friendly_info.unwrap().friendly_point, required - 150);
        crate::db::query("UPDATE hero_friendly_info SET friendly_point=$1 WHERE account_id=$2")
            .bind(required - 1).bind(account).execute(&s.db).await.unwrap();
        assert_eq!(hero::inn::recruit_hero(State(s.clone()), recruit()).await.unwrap().0["Result"], "FriendlyPointMismatch");
        assert_eq!(hero::inn::do_hero_friendly(State(s.clone()), greet()).await.unwrap().0.result, "ActionNotReady");
        crate::db::query("UPDATE hero_friendly_info SET last_greeting_time='2000-01-01 00:00:00' WHERE account_id=$1")
            .bind(account).execute(&s.db).await.unwrap();
        // A new day's action crossing the target must clamp at that hero's target.
        let action = hero::inn::do_hero_friendly(State(s.clone()), greet()).await.unwrap().0;
        assert_eq!(action.friendly_info.unwrap().friendly_point, required);
        let recruited = hero::inn::recruit_hero(State(s.clone()), recruit()).await.unwrap().0;
        assert_eq!(recruited["Result"], "Success");
        assert_eq!(recruited["HeroResult"]["HeroInfo"]["HeroIndex"], index);
        assert_eq!(crate::db::query_scalar::<i32>("SELECT friendly_point FROM hero_friendly_info WHERE account_id=$1")
            .bind(account).fetch_one(&s.db).await.unwrap(), 0);
        assert_eq!(hero::inn::recruit_hero(State(s.clone()), recruit()).await.unwrap().0["Result"], "AlreadyRecruited");
    }
}

#[tokio::test]
async fn inn_daily_refresh_acknowledges_reset_and_preserves_recruit_progress() {
    let (s, u) = setup().await;
    let first = inn_refresh(&s, &u).await.friendly_info.unwrap();
    crate::db::query("UPDATE hero_friendly_info SET selected_time='2000-01-01 00:00:00',friendly_point=350,last_greeting_time='2000-01-01 00:00:01' WHERE account_id=$1")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let response = inn_refresh(&s, &u).await;
    assert_eq!(response.result, "Success");
    let refreshed = response.friendly_info.unwrap();
    assert_eq!(refreshed.hero_index, first.hero_index);
    assert_eq!(refreshed.selected_hero_index, first.selected_hero_index);
    assert_eq!(refreshed.friendly_point, 350);
    assert_eq!(refreshed.last_greeting_time.as_deref(), Some("2000-01-01 00:00:01"));
    assert_eq!(&refreshed.selected_time.as_ref().unwrap()[..10], s.server_date());
    let visitors: Vec<i32> = refreshed.selected_hero_indice.as_ref().unwrap().split(',').map(|id| id.parse().unwrap()).collect();
    assert_eq!(visitors.len(), 6);
    assert!(visitors.contains(&first.hero_index));
    let again = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!(again.selected_time, refreshed.selected_time);
    assert_eq!(again.selected_hero_indice, refreshed.selected_hero_indice);
    assert_eq!(again.friendly_point, 350);
    let saved: Option<String> = crate::db::query_scalar("SELECT selected_time FROM hero_friendly_info WHERE account_id=$1")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap();
    assert_eq!(saved, refreshed.selected_time);
}

#[tokio::test]
async fn inn_excludes_maya_repairs_saved_visitors_and_rejects_recruitment() {
    let (s,u)=setup().await;
    assert_eq!(s.tables.hero_shop.heroes[&14]["CodeName"],"Hero.Maya");
    assert!(!crate::models::hero_inn::get_available_inn_hero_indices().contains(&14));
    let first=inn_refresh(&s,&u).await.friendly_info.unwrap();
    assert!(!first.selected_hero_indice.unwrap().split(',').any(|id|id=="14"));
    // A previously saved Maya visit must be removed even before its daily reset.
    crate::db::query("UPDATE hero_friendly_info SET hero_index=14,selected_hero_index=14,friendly_point=1000,selected_hero_indices='14',last_greeting_time='2000-01-01 00:00:01' WHERE account_id=$1")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let refreshed=inn_refresh(&s,&u).await.friendly_info.unwrap();
    assert_eq!((refreshed.hero_index,refreshed.selected_hero_index,refreshed.friendly_point),(0,0,0));
    assert!(refreshed.last_greeting_time.is_none());
    let visitors=refreshed.selected_hero_indice.unwrap();
    assert_eq!(visitors.split(',').count(),6);
    assert!(!visitors.split(',').any(|id|id=="14"));
    let selected=hero::inn::change_recruit_hero(State(s.clone()),axum::extract::Form(hero::inn::ChangeRecruitHeroRequest {
        session_id:None,session_key:Some(u.user_info.session_key.clone()),hero_index:Some(14),
    })).await.unwrap().0;
    assert_eq!(selected.result,"CannotRecruit");
    // A stale client cannot complete a previously eligible Maya recruitment.
    crate::db::query("UPDATE hero_friendly_info SET hero_index=14,friendly_point=1000 WHERE account_id=$1")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let recruited=hero::inn::recruit_hero(State(s.clone()),axum::extract::Form(hero::inn::RecruitHeroRequest {
        session_id:None,session_key:Some(u.user_info.session_key.clone()),hero_index:Some(14),
    })).await.unwrap().0;
    assert_eq!(recruited["Result"],"HeroIndexMismatch");
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM heroes WHERE account_id=$1 AND hero_index=14")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap(),0);
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES($1,90014,14,80,5)")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    inn_refresh(&s,&u).await;
    assert_eq!(crate::db::query_scalar::<i64>("SELECT level FROM heroes WHERE account_id=$1 AND hero_index=14")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap(),80);
}

#[tokio::test]
async fn inn_refresh_repairs_owned_recruit_and_invalid_visitors() {
    let (s, u) = setup().await;
    inn_refresh(&s, &u).await;
    let owned: i32 = crate::db::query_scalar("SELECT hero_index FROM heroes WHERE account_id=$1 LIMIT 1")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap();
    crate::db::query("UPDATE hero_friendly_info SET hero_index=$1,selected_hero_index=$2,friendly_point=350,selected_time=NULL,selected_hero_indices='999999' WHERE account_id=$3")
        .bind(owned).bind(owned).bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let refreshed = inn_refresh(&s, &u).await.friendly_info.unwrap();
    assert_eq!(refreshed.hero_index, 0);
    assert_eq!(refreshed.selected_hero_index, 0);
    assert_eq!(refreshed.friendly_point, 0);
    let visitors: Vec<i32> = refreshed.selected_hero_indice.unwrap().split(',').map(|id| id.parse().unwrap()).collect();
    assert_eq!(visitors.len(), 6);
    for id in visitors {
        assert!(crate::models::hero_inn::get_available_inn_hero_indices().contains(&id));
        let have: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM heroes WHERE account_id=$1 AND hero_index=$2")
            .bind(u.user_info.account_id).bind(id).fetch_one(&s.db).await.unwrap();
        assert_eq!(have, 0);
    }
}

async fn setup() -> (AppState, user::LoginResponse) {
    let db = crate::database::test_pool().await;
    let state = AppState::new(db, crate::tables::test_tables());
    let u = login(&state).await;
    crate::db::query("UPDATE user_info SET gold=100000000,gem=100000").execute(&state.db).await.unwrap();
    (state, u)
}

#[tokio::test]
async fn bulk_hero_snapshot_preserves_details_defaults_and_account_isolation() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let mut db=s.db.acquire().await.unwrap();
    hero::save_details(&mut db,a,1,&serde_json::json!({"Level":77,"TranscendSkillPage1":"[1,2]","ApplySkillPage":2})).await.unwrap();
    for id in 10001..10100 {
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) VALUES($1,$2,$3)")
            .bind(a).bind(id).bind(id).execute(&mut *db).await.unwrap();
    }
    let started=std::time::Instant::now();
    let snapshots=hero::snapshot(&mut db,a).await.unwrap();
    let bulk=started.elapsed();let started=std::time::Instant::now();
    assert_eq!(snapshots.len(),100);
    for h in &snapshots {
        let expected:crate::models::hero::HeroInfo=serde_json::from_value(hero::info(&mut db,a,h.hero_index).await.unwrap()).unwrap();
        assert_eq!(serde_json::to_value(h).unwrap(),serde_json::to_value(expected).unwrap());
    }
    println!("100-hero fixture: one joined query {:.2}ms; 200 reference detail/hero queries plus equality checks {:.2}ms",bulk.as_secs_f64()*1000.0,started.elapsed().as_secs_f64()*1000.0);
    assert!(hero::snapshot(&mut db,a+1000).await.unwrap().is_empty());
    crate::db::query("UPDATE hero_details SET data='invalid' WHERE account_id=$1 AND hero_index=1").bind(a).execute(&mut *db).await.unwrap();
    assert!(hero::snapshot(&mut db,a).await.is_err());
}
async fn login(s: &AppState) -> user::LoginResponse {
    user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=hero-shop-test"),
    )
    .await
    .unwrap()
    .0
}
fn form(u: &user::LoginResponse, s: &str) -> Bytes {
    Bytes::from(format!("SessionKey={}&{s}", u.user_info.session_key))
}

#[tokio::test]
async fn lobby_selection_retains_normal_hair_and_weapon_after_relogin() {
    let (s, mut u) = setup().await;
    let account = u.user_info.account_id;
    let costume = item::n(&costume(&s), "CostumeIndex");
    let normal_avatar = 10 + item::n(&get(&s, &u, 1).await, "Star");
    crate::db::query("INSERT INTO costumes(account_id,costume_index) VALUES ($1,$2)")
        .bind(account).bind(costume).execute(&s.db).await.unwrap();
    // Include explicit normal hair/weapon (zero), separate costume cosmetics,
    // and a normal-body portrait while the in-game body wears a costume.
    for (body, hair, weapon, hidden) in [
        (costume, 0, costume, 0), (costume, costume, 0, 1),
        (costume, 0, 0, 0), (0, costume, costume, 1),
    ] {
        let equipped = serde_json::json!({"CostumeIndex":costume,
            "HairCostumeIndex":hair,"WeaponCostumeIndex":weapon,"HideUniqueWeapon":hidden});
        hero::save_details(&mut s.db.acquire().await.unwrap(), account, 1, &equipped).await.unwrap();
        let avatar_index = if body == 0 { normal_avatar } else { 10000 + body };
        let selected = hero::change_avatar_hero(State(s.clone()),
            form(&u, &format!("HeroIndex=1&AvatarHeroIndex={avatar_index}")))
            .await.unwrap().0;
        assert_eq!(selected["Result"], "Success", "{selected}");
        let expected = selected["PlayerAvatarHeroInfo"].clone();
        assert_eq!(expected["CostumeIndex"], body);
        assert_eq!(expected["HairCostumeIndex"], hair);
        assert_eq!(expected["WeaponCostumeIndex"], weapon);
        let changed = serde_json::json!({"CostumeIndex":0,
            "HairCostumeIndex":if hair == 0 { costume } else { 0 },
            "WeaponCostumeIndex":if weapon == 0 { costume } else { 0 },
            "HideUniqueWeapon":1-hidden});
        hero::save_details(&mut s.db.acquire().await.unwrap(), account, 1, &changed).await.unwrap();
        for _ in 0..2 {
            u = login(&s).await;
            assert_eq!(u.player_avatar_hero_info, expected);
            let live = get(&s, &u, 1).await;
            for key in ["CostumeIndex", "HairCostumeIndex", "WeaponCostumeIndex", "HideUniqueWeapon"] {
                assert_eq!(live[key], changed[key], "equipped {key}");
            }
        }
    }
}


#[tokio::test]
async fn lobby_snapshot_retains_all_accessory_slots_and_seven_transform_values() {
    use crate::api::extensions;
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    let mut equipped = serde_json::json!({});
    let mut positions = Vec::new();
    for slot in 1..=4 {
        let data = s.tables.extensions.rows("AccessoryCostume").iter()
            .find(|v| item::n(v, "PartType") == slot).unwrap();
        let id = item::n(data, "Index");
        let position = serde_json::json!({"PositionX":"0.1","PositionY":"0.2","PositionZ":"0.3",
            "RotationX":"10","RotationY":"20","RotationZ":"30","Scale":"1.2"});
        equipped[format!("AccessoryCostumeIndex{slot}")] = serde_json::json!(id);
        let ownership = serde_json::json!({"HeroIndex":1,"AccessoryCostumeIndex":id,"PositionInfo":position.to_string()});
        extensions::put(&mut s.db.acquire().await.unwrap(), account, "accessory", (1_i64 << 32) | id, &ownership)
            .await.unwrap();
        positions.push((id, position));
    }
    hero::save_details(&mut s.db.acquire().await.unwrap(), account, 1, &equipped).await.unwrap();
    let normal_avatar = 10 + item::n(&get(&s, &u, 1).await, "Star");
    let selected = hero::change_avatar_hero(State(s.clone()),
        form(&u, &format!("HeroIndex=1&AvatarHeroIndex={normal_avatar}")))
        .await.unwrap().0;
    assert_eq!(selected["Result"], "Success");
    let snapshot = selected["PlayerAvatarHeroInfo"].clone();
    for (slot, (id, position)) in (1..=4).zip(&positions) {
        assert_eq!(snapshot[format!("AccessoryCostumeIndex{slot}")], *id);
        assert_eq!(serde_json::from_str::<Value>(snapshot[format!("PositionInfo{slot}")].as_str().unwrap()).unwrap(), *position);
        // Edit all seven values in the live inventory, independently per slot.
        let changed = serde_json::json!({"PositionX":"1","PositionY":"2","PositionZ":"3",
            "RotationX":"40","RotationY":"50","RotationZ":"60","Scale":"0.8"});
        let ownership = serde_json::json!({"HeroIndex":1,"AccessoryCostumeIndex":id,"PositionInfo":changed.to_string()});
        extensions::put(&mut s.db.acquire().await.unwrap(), account, "accessory", (1_i64 << 32) | *id, &ownership)
            .await.unwrap();
    }
    // Unequip every live slot; both same-session reads and repeated logins retain
    // the chosen accessories and the original transforms in every slot.
    hero::save_details(&mut s.db.acquire().await.unwrap(), account, 1, &serde_json::json!({})).await.unwrap();
    assert_eq!(hero::avatar_info(&mut s.db.acquire().await.unwrap(), &s, account).await.unwrap(), snapshot);
    for _ in 0..2 {
        let again = login(&s).await;
        assert_eq!(again.player_avatar_hero_info, snapshot);
    }
}


#[tokio::test]
async fn lobby_snapshot_retains_rank_and_weapon_values_after_gear_changes() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    let slot = crate::db::query_scalar::<i64>("INSERT INTO equip_items(account_id,item_index,star) VALUES ($1,1001,3) RETURNING slot_index")
        .bind(account).fetch_one(&s.db).await.unwrap();
    crate::db::query("UPDATE heroes SET star=3,equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1")
        .bind(slot).bind(account).execute(&s.db).await.unwrap();
    crate::api::extensions::put(&mut s.db.acquire().await.unwrap(), account, "soul", slot,
        &serde_json::json!({"ItemIndex":1001,"EquipItemSlotIndex":slot,"Grade":1,"ReinforceLevel":0})).await.unwrap();
    let selected = hero::change_avatar_hero(State(s.clone()), form(&u, "HeroIndex=1&AvatarHeroIndex=13"))
        .await.unwrap().0;
    assert_eq!(selected["Result"], "Success");
    let snapshot = selected["PlayerAvatarHeroInfo"].clone();
    let render = &snapshot["SprkLobbyRenderInfo"];
    assert_eq!(render["Star"], 3);
    assert_eq!(render["Equipment"][0]["ItemIndex"], 1001);
    assert_eq!(render["Equipment"][0]["Star"], 3);
    assert_eq!(render["Equipment"][0]["SoulWeaponInfo"]["Grade"], 1);
    crate::db::query("UPDATE heroes SET star=5,equip_item_slot_index_1=0 WHERE account_id=$1 AND hero_index=1")
        .bind(account).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE equip_items SET star=5 WHERE account_id=$1 AND slot_index=$2")
        .bind(account).bind(slot).execute(&s.db).await.unwrap();
    crate::api::extensions::put(&mut s.db.acquire().await.unwrap(), account, "soul", slot,
        &serde_json::json!({"ItemIndex":1001,"EquipItemSlotIndex":slot,"Grade":2,"ReinforceLevel":0})).await.unwrap();
    let again = login(&s).await;
    assert_eq!(again.player_avatar_hero_info, snapshot);
    let reselected = hero::change_avatar_hero(State(s.clone()), form(&again, "HeroIndex=1&AvatarHeroIndex=13"))
        .await.unwrap().0;
    assert_eq!(reselected["Result"], "Success");
    assert_eq!(reselected["PlayerAvatarHeroInfo"]["SprkLobbyRenderInfo"], serde_json::json!({"Star":5,"Equipment":[]}));
}
async fn count(s: &AppState, u: &user::LoginResponse, id: i32) -> i64 {
    crate::db::query_scalar(
        "SELECT COALESCE((SELECT count FROM items WHERE account_id=$1 AND item_index=$2),0)",
    )
    .bind(u.user_info.account_id)
    .bind(id)
    .fetch_one(&s.db)
    .await
    .unwrap()
}
async fn put(s: &AppState, u: &user::LoginResponse, id: i32, count: i64) {
    crate::db::query("INSERT INTO items(account_id,item_index,count) VALUES ($1,$2,$3) ON CONFLICT(account_id,item_index) DO UPDATE SET count=excluded.count").bind(u.user_info.account_id).bind(id).bind(count).execute(&s.db).await.unwrap();
}
async fn balance(s: &AppState, u: &user::LoginResponse, key: &str) -> i64 {
    crate::db::query_scalar(sqlx::AssertSqlSafe(format!("SELECT {key} FROM user_info WHERE account_id=$1")))
        .bind(u.user_info.account_id)
        .fetch_one(&s.db)
        .await
        .unwrap()
}
async fn get(s: &AppState, u: &user::LoginResponse, id: i32) -> Value {
    hero::info(
        &mut s.db.acquire().await.unwrap(),
        u.user_info.account_id,
        id,
    )
    .await
    .unwrap()
}
fn buy_data(s: &AppState) -> (i32, i32, i64) {
    s.tables
        .hero_shop
        .heroes
        .iter()
        .filter(|(id, h)| **id != 1 && h["Buyable"] == true)
        .find_map(|(&id, h)| {
            s.tables.hero_shop.items.iter().find_map(|(&item_id, meta)| {
                let m = s.tables.items.reward_item(item_id)?;
                if m.kind == "Hero"
                    && m.hero_index == id
                    && m.star as i64 == item::n(h, "StartHeroStar")
                    && m.transcend == 0
                    && m.level as i64 == item::n(h, "StartHeroLevel").max(1)
                    && meta["Type"] == 15
                {
                    let p = s
                        .tables
                        .hero_shop
                        .prices
                        .iter()
                        .find(|v| {
                            v["Star"] == h["StartHeroStar"] && v["OpenStatus"] == h["OpenStatus"]
                        })
                        .unwrap();
                    Some((id, item_id, item::n(p, "BuyGem")))
                } else {
                    None
                }
            })
        })
        .expect("no purchasable hero")
}
#[tokio::test]
async fn ruby_hero_purchase_uses_native_contract_prices_and_rejects_duplicates() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    crate::db::query("UPDATE user_info SET gem=100,pay_gem=10000 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}");
    let r = hero::buy_hero(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroResult"]["HeroInfo"]["HeroIndex"], id);
    assert!(r["HeroResult"]["HeroInfo"].get("Transcended").is_some());
    assert_eq!(balance(&s, &u, "gem").await, 0);
    assert_eq!(balance(&s, &u, "pay_gem").await, 10100 - cost);
    assert!(balance(&s, &u, "mileage").await > 0);
    let r = hero::buy_hero(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "HeroAlreadyExist");
    assert_eq!(balance(&s, &u, "pay_gem").await, 10100 - cost);
    assert!(login(&s).await.heroes.iter().any(|h| h.hero_index == id));
}
#[tokio::test]
async fn forged_hero_prices_mismatched_items_and_insufficient_funds_are_atomic() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    let before = balance(&s, &u, "gem").await;
    for req in [
        format!("HeroIndex={id}&ItemIndex={item}&BuyGem=1"),
        format!("HeroIndex={id}&ItemIndex=1&BuyGem={cost}"),
        format!("HeroIndex=99999&ItemIndex={item}&BuyGem={cost}"),
    ] {
        assert_ne!(
            hero::buy_hero(State(s.clone()), form(&u, &req))
                .await
                .unwrap()
                .0["Result"],
            "Success"
        );
    }
    assert_eq!(balance(&s, &u, "gem").await, before);
    crate::db::query("UPDATE user_info SET gem=0,pay_gem=0 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::buy_hero(
            State(s.clone()),
            form(
                &u,
                &format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}")
            )
        )
        .await
        .unwrap()
        .0["Result"],
        "NotEnoughGem"
    );
    assert_eq!(login(&s).await.heroes.len(), 1);
}
#[tokio::test]
async fn competing_purchases_charge_and_recruit_once() {
    let (template, _) = setup().await;
    let pool = crate::database::test_pool().await;
    let s = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let u = login(&s).await;
    let (id, item, cost) = buy_data(&s);
    crate::db::query("UPDATE user_info SET gem=$1 WHERE account_id=$2")
        .bind(cost).bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let before = balance(&s, &u, "gem").await;
    let req = format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}");
    let (a, b) = tokio::join!(
        hero::buy_hero(State(s.clone()), form(&u, &req)),
        hero::buy_hero(State(s.clone()), form(&u, &req))
    );
    assert_eq!(
        [a.unwrap().0, b.unwrap().0]
            .iter()
            .filter(|v| v["Result"] == "Success")
            .count(),
        1
    );
    assert_eq!(balance(&s, &u, "gem").await, before - cost);
    assert_eq!(
        login(&s)
            .await
            .heroes
            .iter()
            .filter(|h| h.hero_index == id)
            .count(),
        1
    );
}
fn costume(s: &AppState) -> Value {
    s.tables
        .hero_shop
        .costumes
        .values()
        .find(|v| {
            v["HeroIndex"] == 1
                && v["Buyable"] == true
                && item::n(v, "ReqBuyGem") > 0
                && item::n(v, "ReqBuyGold") == 0
                && item::n(v, "ReqBuyMileage") == 0
        })
        .unwrap()
        .clone()
}
#[tokio::test]
async fn costume_purchase_equip_presets_and_avatar_survive_relogin() {
    let (s, u) = setup().await;
    let c = costume(&s);
    let id = item::n(&c, "CostumeIndex");
    let cost = item::n(&c, "ReqBuyGem");
    assert_ne!(
        hero::set_costume(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&CostumeIndex={id}"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    // Native client repeats the selected price in every currency field.
    let req = format!("HeroIndex=1&CostumeIndex={id}&BuyGem={cost}&BuyGold={cost}&BuyMileage={cost}");
    let before = balance(&s, &u, "gem").await;
    let gold_before = balance(&s, &u, "gold").await;
    let mileage_before = balance(&s, &u, "mileage").await;
    let r = hero::buy_costume(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroCostumeResult"]["CostumeIndex"], id);
    assert_eq!(balance(&s, &u, "gem").await, before - cost);
    assert_eq!(balance(&s, &u, "gold").await, gold_before);
    assert_eq!(balance(&s, &u, "mileage").await, mileage_before + item::n(&c, "Mileage"));
    assert_eq!(
        hero::buy_costume(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "AlreadyHaveCostume"
    );
    assert_eq!(
        hero::save_costume_storage_slot(
            State(s.clone()),
            form(&u, "HeroIndex=1&CostumeStorageSlotIndex=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let _ = hero::unset_costume(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap();
    assert_eq!(get(&s, &u, 1).await["CostumeIndex"], 0);
    let _ = hero::set_costume_storage_slot(
        State(s.clone()),
        form(&u, "HeroIndex=1&CostumeStorageSlotIndex=1"),
    )
    .await
    .unwrap();
    assert_eq!(
        hero::change_avatar_hero(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&AvatarHeroIndex={}", 10000 + id))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let again = login(&s).await;
    assert!(again.costume_infos.iter().any(|v| v["CostumeIndex"] == id));
    assert_eq!(again.heroes[0].details["CostumeIndex"], id);
    assert_eq!(again.user_info.avatar_hero_index as i64, 10000 + id);
    assert_eq!(again.costume_storage_slot_infos.len(), 1);
}
#[tokio::test]
async fn costume_wrong_hero_free_default_and_price_forgery_do_not_charge() {
    let (s, u) = setup().await;
    let c = costume(&s);
    let id = item::n(&c, "CostumeIndex");
    let before = balance(&s, &u, "gem").await;
    for req in [
        format!("HeroIndex=1&CostumeIndex={id}&BuyGem=1"),
        format!("HeroIndex=1&CostumeIndex={id}&BuyGem=1&BuyGold=3000&BuyMileage=3000"),
        format!("HeroIndex=2&CostumeIndex={id}&BuyGem=3000"),
        "HeroIndex=1&CostumeIndex=101&BuyGem=0".into(),
    ] {
        assert_ne!(
            hero::buy_costume(State(s.clone()), form(&u, &req))
                .await
                .unwrap()
                .0["Result"],
            "Success"
        );
    }
    assert_eq!(balance(&s, &u, "gem").await, before);
}
#[tokio::test]
async fn bookmarks_replace_selection_and_reject_unowned_heroes() {
    let (s, u) = setup().await;
    assert_eq!(
        hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[1]"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert!(login(&s).await.heroes[0].is_bookmarked);
    assert_ne!(
        hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[1,9999]"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert!(get(&s, &u, 1).await["IsBookmarked"].as_bool().unwrap());
    let _ = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[]"))
        .await
        .unwrap();
    assert!(!get(&s, &u, 1).await["IsBookmarked"].as_bool().unwrap());
}
#[tokio::test]
async fn bookmarks_accept_native_repeated_fields_and_omitted_empty_list() {
    let (s, u) = setup().await;
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES ($1,2,2,2)")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let set = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=2&HeroIndex=1"))
        .await.unwrap().0;
    assert_eq!(set["Result"], "Success", "{set}");
    {
        let mut db = s.db.acquire().await.unwrap();
        assert_eq!(hero::bookmarks(&mut db, u.user_info.account_id).await.unwrap()["BookMarkHeroIndices"], "2,1");
    }
    for invalid in ["HeroIndex=1&HeroIndex=9999", "HeroIndex=2&HeroIndex=2", "HeroIndex=invalid", "HeroIndex=null"] {
        let rejected = hero::bookmark_hero(State(s.clone()), form(&u, invalid)).await.unwrap().0;
        assert_ne!(rejected["Result"], "Success", "{invalid}: {rejected}");
        let mut db = s.db.acquire().await.unwrap();
        assert_eq!(hero::bookmarks(&mut db, u.user_info.account_id).await.unwrap()["BookMarkHeroIndices"], "2,1");
    }
    let one = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=1")).await.unwrap().0;
    assert_eq!(one["Result"], "Success", "{one}");
    assert!(login(&s).await.heroes.iter().find(|h| h.hero_index==1).unwrap().is_bookmarked);
    // Empty native int[] is encoded without any HeroIndex fields.
    let clear = hero::bookmark_hero(State(s.clone()), form(&u, "")).await.unwrap().0;
    assert_eq!(clear["Result"], "Success", "{clear}");
    assert!(login(&s).await.heroes.iter().all(|h| !h.is_bookmarked));
}
#[tokio::test]
async fn skill_upgrade_uses_current_level_price_and_extend_uses_class_book() {
    let (s, u) = setup().await;
    crate::db::query("UPDATE heroes SET level=10 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let c = &s.tables.hero_shop.heroes[&1];
    let skill = item::n(c, "SkillIndex1");
    let price: i64 = s
        .tables
        .hero_shop
        .skill_prices
        .iter()
        .filter(|v| v["SlotIndex"] == 1 && item::n(v, "Level") < 3)
        .map(|v| item::n(v, "ReqGold"))
        .sum();
    let before = balance(&s, &u, "gold").await;
    let req = format!("HeroIndex=1&SkillIndices=[{skill}]&TargetLevels=[3]");
    assert_eq!(
        hero::upgrade_hero_skill(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gold").await, before - price);
    assert_eq!(get(&s, &u, 1).await["SkillLevel1"], 3);
    assert_ne!(
        hero::upgrade_hero_skill(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let book = s
        .tables
        .hero_shop
        .books
        .iter()
        .find(|v| v["TagType"] == c["TagType"] && v["Grade"] == 1)
        .unwrap();
    let book = item::n(book, "ItemIndex") as i32;
    put(&s, &u, book, 20).await;
    assert_eq!(
        hero::extend_hero_skill(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&SkillIndex={skill}&SkillExtend=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, book).await, 0);
    assert_eq!(login(&s).await.heroes[0].details["SkillExtend1"], 1);
}
#[tokio::test]
async fn extend_all_skills_spends_each_slot_book_independently_and_atomically() {
    let (s, u) = setup().await;
    let c = &s.tables.hero_shop.heroes[&1];
    let books: Vec<i32> = (1..=4)
        .map(|slot| {
            let b = s.tables.hero_shop.books.iter()
                .find(|v| v["TagType"] == c["TagType"] && item::n(v, "Grade") == slot)
                .unwrap();
            item::n(b, "ItemIndex") as i32
        })
        .collect();
    assert_eq!(books.iter().collect::<std::collections::BTreeSet<_>>().len(), 4, "each slot has its own book");
    // Tiers cost 20/60/120: 200 -> IV, 79 -> II, 0 -> unchanged, 100 -> III.
    for (book, owned) in books.iter().zip([200, 79, 0, 100]) {
        if owned > 0 { put(&s, &u, *book, owned).await; }
    }
    let r = hero::extend_hero_skill(State(s.clone()), form(&u, "HeroIndex=1&SkillIndex=0&SkillExtend=-1&ExtendAll=1"))
        .await.unwrap().0;
    assert_eq!(r["Result"], "Success", "{r}");
    let extends: Vec<(i64, i64)> = r["SkillExtends"].as_array().unwrap().iter()
        .map(|v| (item::n(v, "SkillSlotIndex"), item::n(v, "SkillExtend"))).collect();
    assert_eq!(extends, vec![(1, 3), (2, 1), (4, 2)]);
    assert_eq!(r["ItemResults"].as_array().unwrap().len(), 3);
    assert!(r["ItemResult"].is_object());
    for (book, left) in books.iter().zip([0, 59, 0, 20]) {
        assert_eq!(count(&s, &u, *book).await, left);
    }
    let details = &login(&s).await.heroes[0].details;
    assert_eq!((details["SkillExtend1"].clone(), details["SkillExtend2"].clone(), details["SkillExtend4"].clone()), (serde_json::json!(3), serde_json::json!(1), serde_json::json!(2)));
    // Nothing affordable remains: rejected without spending.
    let again = hero::extend_hero_skill(State(s.clone()), form(&u, "HeroIndex=1&SkillIndex=0&SkillExtend=-1&ExtendAll=1"))
        .await.unwrap().0;
    assert_eq!(again["Result"], "NotEnoughItem", "{again}");
    for (book, left) in books.iter().zip([0, 59, 0, 20]) {
        assert_eq!(count(&s, &u, *book).await, left);
    }
}

#[tokio::test]
async fn awakening_requires_challenge_battle_purification_then_consumes_essence_once() {
    let (s, u) = setup().await;
    let c = &s.tables.hero_shop.heroes[&1];
    let row = s
        .tables
        .hero_shop
        .challenges
        .iter()
        .find(|v| v["TagType"] == c["TagType"])
        .unwrap();
    for i in 1..=3 {
        put(&s, &u, item::n(row, &format!("ItemIndex{i}")) as i32, 5).await;
    }
    let r = hero::get_awake_material_challenge(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ItemResults"].as_array().unwrap().len(), 3);
    assert_ne!(
        hero::upgrade_hero_star(State(s.clone()), form(&u, "HeroIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let awake = s
        .tables
        .hero_shop
        .awake
        .iter()
        .find(|v| v["HeroIndex"] == 1)
        .unwrap();
    let chapter = item::n(awake, "AwakeChapter") as i32;
    let dungeon = item::n(awake, "AwakeDungeon1") as i32;
    assert!(
        hero::trial(&s, u.user_info.account_id, chapter, dungeon, Some(true))
            .await
            .is_err()
    );
    let _ = campaign::begin_campaign(
        State(s.clone()),
        form(
            &u,
            &format!("ChapterIndex={chapter}&DungeonIndex={dungeon}"),
        ),
    )
    .await
    .unwrap();
    let r = campaign::end_campaign(
        State(s.clone()),
        form(
            &u,
            &format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&Completed=true"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["ItemResults"].as_array().unwrap().len(), 1);
    assert!(r["HeroInfos"].as_array().is_none_or(|heroes| heroes.is_empty()));
    let essence = r["ItemResults"][0]["ItemIndex"].as_i64().unwrap() as i32;
    assert!(
        hero::trial(&s, u.user_info.account_id, chapter, dungeon, Some(true))
            .await
            .is_err()
    );
    let next = s.tables.hero_shop.stars.iter()
        .find(|v| v["Star"] == 2 && v["Transcended"] == 0).unwrap();
    let amount = item::n(next, "PurifyAmount1");
    let expected_cost = ((1000 + amount - 1) / amount) * item::n(next, "Gold");
    let gold_before = balance(&s, &u, "gold").await;
    let r = hero::max_purify_hero(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["PurifyResult"]["NewValue"], 1000);
    assert_eq!(balance(&s, &u, "gold").await, gold_before - expected_cost);
    assert_eq!(count(&s, &u, essence).await, 1);
    let r = hero::upgrade_hero_star(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["NewHeroStar"], 2);
    assert_eq!(count(&s, &u, essence).await, 0);
    assert_eq!(login(&s).await.heroes[0].star, 2);
}
#[tokio::test]
async fn trial_reentry_preserves_purification_and_rejects_unstarted_or_wrong_stage_rewards() {
    let (s,u)=setup().await;let account=u.user_info.account_id;
    {let mut db=s.db.acquire().await.unwrap();hero::recruit_at(&mut db,&s,account,82,5,90,0).await.unwrap();}
    let awake=s.tables.hero_shop.awake.iter().find(|r|r["HeroIndex"]==82).unwrap();
    let chapter=item::n(awake,"AwakeChapter");let dungeon=item::n(awake,"TranscendDungeon0");
    let essence=item::n(awake,"TranscendMaterialItemIndex0") as i32;
    // Reproduce the saved, cleared trial with purification already underway.
    {
        let mut db=s.db.acquire().await.unwrap();let mut saved=hero::details(&mut db,account,82).await.unwrap();
        saved["EnterDungeon"]=serde_json::json!(2);saved["Purified"]=serde_json::json!(350);saved["TrialStarted"]=serde_json::json!(false);
        hero::save_details(&mut db,account,82,&saved).await.unwrap();
    }
    put(&s,&u,essence,1).await;
    let begin=format!("ChapterIndex={chapter}&DungeonIndex={dungeon}");let end=format!("{begin}&Completed=true");
    let unstarted=campaign::end_campaign(State(s.clone()),form(&u,&end)).await.unwrap().0;
    assert_ne!(unstarted["Result"],"Success");
    for _ in 0..2 {
        let before=count(&s,&u,essence).await;
        let started=campaign::begin_campaign(State(s.clone()),form(&u,&begin)).await.unwrap().0;
        assert_eq!(started["Result"],"Success","{started}");
        let won=campaign::end_campaign(State(s.clone()),form(&u,&end)).await.unwrap().0;
        assert_eq!(won["Result"],"Success");assert_eq!(count(&s,&u,essence).await,before+1);
        assert!(won["HeroInfos"].as_array().is_none_or(|rows|rows.is_empty()));
        let duplicate=campaign::end_campaign(State(s.clone()),form(&u,&end)).await.unwrap().0;
        assert_ne!(duplicate["Result"],"Success");assert_eq!(count(&s,&u,essence).await,before+1);
        let info=hero::info(&mut s.db.acquire().await.unwrap(),account,82).await.unwrap();
        assert_eq!(info["EnterDungeon"],2);assert_eq!(info["Purified"],350);
    }
    let wrong=campaign::begin_campaign(State(s.clone()),form(&u,&format!("ChapterIndex={chapter}&DungeonIndex={}",dungeon+1))).await.unwrap().0;
    assert_ne!(wrong["Result"],"Success");
    let before=count(&s,&u,essence).await;
    let started=campaign::begin_campaign(State(s.clone()),form(&u,&begin)).await.unwrap().0;
    assert_eq!(started["Result"],"Success");
    let lost=campaign::end_campaign(State(s.clone()),form(&u,&format!("{begin}&Completed=false"))).await.unwrap().0;
    assert_eq!(lost["Result"],"Success");assert_eq!(count(&s,&u,essence).await,before);
    crate::db::query("UPDATE hero_details SET data=sprk_json_set(data,'$.EnterDungeon',0) WHERE account_id=$1 AND hero_index=82")
        .bind(account).execute(&s.db).await.unwrap();
    let locked=campaign::begin_campaign(State(s.clone()),form(&u,&begin)).await.unwrap().0;
    assert_ne!(locked["Result"],"Success");assert_eq!(count(&s,&u,essence).await,before);
}

#[tokio::test]
async fn isolet_light_trial_rewards_material_without_acquiring_hero() {
    let (s, u) = setup().await;
    {
        let mut db = s.db.acquire().await.unwrap();
        hero::recruit_at(&mut db, &s, u.user_info.account_id, 82, 5, 90, 0).await.unwrap();
    }
    let row = s.tables.hero_shop.challenges.iter()
        .find(|r| r["TagType"] == s.tables.hero_shop.heroes[&82]["TagType"]).unwrap();
    for i in 1..=3 {
        put(&s, &u, item::n(row, &format!("TranscendItemIndex{i}")) as i32, 10000).await;
    }
    let next = s.tables.hero_shop.stars.iter().find(|r| r["Star"] == 5 && r["Transcended"] == 1).unwrap();
    let extra_material = item::n(next, "HeroTranscendItemIndex") as i32;
    if extra_material > 0 { put(&s, &u, extra_material, 10000).await; }
    let challenge = hero::get_awake_material_challenge(State(s.clone()), form(&u, "HeroIndex=82")).await.unwrap().0;
    assert_eq!(challenge["Result"], "Success");
    let awake = s.tables.hero_shop.awake.iter().find(|r| r["HeroIndex"] == 82).unwrap();
    let chapter = item::n(awake, "AwakeChapter");
    let dungeon = item::n(awake, "TranscendDungeon0");
    let essence = item::n(awake, "TranscendMaterialItemIndex0") as i32;
    assert_eq!((chapter, dungeon), (10082, 10));
    let before = count(&s, &u, essence).await;
    let begin = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}");
    let _ = campaign::begin_campaign(State(s.clone()), form(&u, &begin)).await.unwrap();
    let lost = campaign::end_campaign(State(s.clone()), form(&u, &format!("{begin}&Completed=false"))).await.unwrap().0;
    assert!(lost["HeroInfos"].as_array().is_none_or(|heroes| heroes.is_empty()));
    assert_eq!(count(&s, &u, essence).await, before);
    let _ = campaign::begin_campaign(State(s.clone()), form(&u, &begin)).await.unwrap();
    let won = campaign::end_campaign(State(s.clone()), form(&u, &format!("{begin}&Completed=true"))).await.unwrap().0;
    assert!(won["HeroInfos"].as_array().is_none_or(|heroes| heroes.is_empty()));
    assert_eq!(won["ItemResults"].as_array().unwrap().len(), 1);
    assert_eq!(won["ItemResults"][0]["ItemIndex"], essence);
    assert_eq!(count(&s, &u, essence).await, before + 1);
    assert!(hero::trial(&s, u.user_info.account_id, chapter as i32, dungeon as i32, Some(true)).await.is_err());
    let login = login(&s).await;
    let isolet = login.heroes.iter().find(|h| h.hero_index == 82).unwrap();
    assert_eq!((isolet.star, isolet.transcend), (5, 0));
    assert_eq!(isolet.details["EnterDungeon"], 2);
}

#[tokio::test]
async fn transcend_pages_parse_native_pairs_enforce_points_and_persist() {
    let (s, u) = setup().await;
    crate::db::query("UPDATE heroes SET star=5,transcend=1 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let r = hero::learn_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=1&TranscendSkills=[10,1]"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["Hero"]["TranscendSkillPage1"], "[10]");
    let r = hero::learn_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=1&TranscendSkills=[11,1]"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "NotEnoughSkillPoint");
    let cost = s.tables.hero_shop.constant("AddTranscendSkillPageGem2", 0);
    assert_eq!(
        hero::buy_hero_transcend_skill_page(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&PageIndex=2&BuyGem={cost}"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let _ = hero::apply_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=2"),
    )
    .await
    .unwrap();
    let h = login(&s).await.heroes.remove(0);
    assert_eq!(h.transcend, 1);
    assert_eq!(h.details["ApplySkillPage"], 2);
    assert_eq!(
        hero::reset_hero_transcend_skill_page(
            State(s.clone()),
            form(&u, "HeroIndex=1&PageIndex=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(get(&s, &u, 1).await["TranscendSkillPage1"], "[]");
}
#[tokio::test]
async fn void_rune_shops_allow_repeat_purchases_beyond_old_caps_at_original_prices() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let offers: Vec<_> = s.tables.hero_shop.shop_items.iter()
        .filter(|r| (45123..=45127).contains(&item::n(r, "ItemIndex"))).collect();
    assert!(!offers.is_empty());
    let mut runes = std::collections::BTreeSet::new();
    for row in &offers {
        assert_eq!(item::n(row, "PurchasableCount"), 0, "{row}");
        runes.insert(item::n(row, "ItemIndex"));
    }
    assert_eq!(runes, (45123..=45127).collect());
    // Exercise the actual Arena, World Boss, Honor and Challenge shop paths.
    // Guild-level rows are covered above without bypassing guild membership.
    for (shop_id, currency) in [(3, "PvpCoin"), (12, "WorldBossPoint"), (15, "GloryPoint"), (38, "ChallengeRaidPoint")] {
        let row = offers.iter().find(|r| item::n(r, "ShopIndex") == shop_id).unwrap();
        let index = item::n(row, "Index");
        let rune = item::n(row, "ItemIndex") as i32;
        let price = item::n(row, "TargetPrice");
        assert!(price > 0);
        let current = hero::currency(&mut s.db.acquire().await.unwrap(), a, currency, 0).await.unwrap();
        hero::currency(&mut s.db.acquire().await.unwrap(), a, currency, 1000000 - item::n(&current, "NewValue")).await.unwrap();
        let before = count(&s, &u, rune).await;
        for amount in [3, 4] {
            let req = format!("ShopIndex={shop_id}&ShopItemIndex={index}&ShopItemPurchaseCount={amount}");
            let result = shop::buy_shop_item(State(s.clone()), form(&u, &req)).await.unwrap().0;
            assert_eq!(result["Result"], "Success", "{result}");
            assert_eq!(result["ShopItem"]["Sold"], 0);
        }
        assert_eq!(count(&s, &u, rune).await, before + 7);
        let remaining = hero::currency(&mut s.db.acquire().await.unwrap(), a, currency, 0).await.unwrap();
        assert_eq!(item::n(&remaining, "NewValue"), 1000000 - price * 7);
        let listed = shop::get_shop_list(State(s.clone()), form(&u, &format!("ShopIndex={shop_id}"))).await.unwrap().0;
        let offer = listed["ShopItems"].as_array().unwrap().iter().find(|r| item::n(r, "ListNo") == index).unwrap();
        assert_eq!(offer["Sold"], 0);
        assert_eq!(offer["PurchasableCount"], 0);
        assert_eq!(offer["Purchased"], 7);
        hero::currency(&mut s.db.acquire().await.unwrap(), a, currency, -item::n(&remaining, "NewValue")).await.unwrap();
        let rejected = shop::buy_shop_item(State(s.clone()), form(&u,
            &format!("ShopIndex={shop_id}&ShopItemIndex={index}&ShopItemPurchaseCount=1"))).await.unwrap().0;
        assert_ne!(rejected["Result"], "Success");
        assert_eq!(count(&s, &u, rune).await, before + 7);
        let empty = hero::currency(&mut s.db.acquire().await.unwrap(), a, currency, 0).await.unwrap();
        assert_eq!(item::n(&empty, "NewValue"), 0);
    }
}

#[tokio::test]
async fn inn_shop_lists_actual_item_codes_and_atomic_friendship_purchases() {
    let (s, u) = setup().await;
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    let list = r["ShopItems"].as_array().unwrap();
    assert!(!list.is_empty());
    let row = &list[0];
    assert!(row["ItemCode"].is_string());
    let index = item::n(row, "ListNo");
    crate::db::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = format!("ShopIndex=10&ShopItemIndex={index}&ShopItemPurchaseCount=2");
    let r = shop::buy_shop_item(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ShopItem"]["Purchased"], 2);
    let before = balance(&s, &u, "friendship_point").await;
    assert_ne!(
        shop::buy_shop_item(
            State(s.clone()),
            form(
                &u,
                &format!("ShopIndex=10&ShopItemIndex={index}&ShopItemPurchaseCount=-1")
            )
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    let r = shop::get_all_shop_item_purchase_count(State(s.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap()
        .0;
    assert_eq!(
        r["ShopItems"]
            .as_array()
            .unwrap()
            .iter()
            .find(|r| item::n(r, "ListNo") == index)
            .unwrap()["Purchased"],
        2
    );
}
#[tokio::test]
async fn may_rotating_void_rune_uses_displayed_gold_price_for_cached_stock() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    // Reproduce the saved May stock: the fallback catalog row came from a
    // Guild Shop, while native ShopListItemInfo displays BuyGold (1,000).
    let mut row = s.tables.hero_shop.shop_items.iter()
        .find(|r| r["ItemCode"] == "VOID_RUNE_5" && r["ShopIndex"] == 0)
        .unwrap().clone();
    row["Index"] = serde_json::json!(3);
    row["PurchasableCount"] = serde_json::json!(1);
    assert_eq!(row["BuyCostTypeForItem"], 4);
    assert_eq!(row["TargetPrice"], 120000);
    crate::db::query("INSERT INTO shop_stock(account_id,shop_index,revision,restock_time,restock_count,stock) VALUES ($1,1,1,$2,0,$3)")
        .bind(account).bind(s.server_time()+3600)
        .bind(serde_json::json!([row]).to_string()).execute(&s.db).await.unwrap();
    let request = "ShopIndex=1&ListNo=3&ShopItemPurchaseCount=1";
    let before = count(&s, &u, 45127).await;
    crate::db::query("UPDATE user_info SET gold=999 WHERE account_id=$1")
        .bind(account).execute(&s.db).await.unwrap();
    assert_eq!(shop::buy_shop_item(State(s.clone()), form(&u, request)).await.unwrap().0["Result"], "NotEnoughGold");
    assert_eq!(balance(&s, &u, "gold").await, 999);
    assert_eq!(count(&s, &u, 45127).await, before);
    crate::db::query("UPDATE user_info SET gold=100000 WHERE account_id=$1")
        .bind(account).execute(&s.db).await.unwrap();
    let gold = balance(&s, &u, "gold").await;
    let response = shop::buy_shop_item(State(s.clone()), form(&u, request)).await.unwrap().0;
    assert_eq!(response["Result"], "Success", "{response}");
    assert_eq!(balance(&s, &u, "gold").await, gold-1000);
    assert_eq!(count(&s, &u, 45127).await, before+1);
    assert_eq!(response["ItemResult"]["ItemIndex"], 45127);
    assert_eq!(response["ShopItem"]["Sold"], 1);
    assert_eq!(shop::buy_shop_item(State(s.clone()), form(&u, request)).await.unwrap().0["Result"], "SoldOut");
    assert_eq!(balance(&s, &u, "gold").await, gold-1000);
    assert_eq!(count(&s, &u, 45127).await, before+1);
}

#[tokio::test]
async fn stock_restock_and_capacity_failure_preserve_currency() {
    let (s, u) = setup().await;
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ShopItems"].as_array().unwrap().len(), 6);
    let before = balance(&s, &u, "gem").await;
    let r = shop::request_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, before - 25);
    assert_eq!(r["RestockTimeInfo"]["ShopItemListIndex"], 2);
    crate::db::query("UPDATE shop_stock SET restock_time=1 WHERE account_id=$1 AND shop_index=1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let gold = balance(&s, &u, "gold").await;
    assert_eq!(
        shop::buy_shop_item(
            State(s.clone()),
            form(&u, "ShopIndex=1&ListNo=1&ShopItemPurchaseCount=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "InvalidShopItemListIndex"
    );
    assert_eq!(balance(&s, &u, "gold").await, gold);
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["RestockTimeInfo"]["ShopItemListIndex"], 3);
    assert_ne!(
        shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=99999"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
}
#[tokio::test]
async fn npc_shop_discount_preserves_daily_friendship_earnings() {
    let (s, u) = setup().await;
    crate::db::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    crate::db::query("INSERT INTO friend_daily(account_id,day,points) VALUES ($1,$2,340)")
        .bind(u.user_info.account_id)
        .bind(s.server_date())
        .execute(&s.db)
        .await
        .unwrap();
    let req = "ShopIndex=10&ShopItemIndex=1&ShopItemPurchaseCount=1";
    let r = shop::buy_shop_item(State(s.clone()), form(&u, req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    let full = -item::n(&r["FriendshipPointResult"], "AddValue");
    assert_eq!(r["FriendshipPointResult"]["NewDailyAccValue"], 340);
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES ($1,2,94,2)")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let r = shop::buy_shop_item(State(s.clone()), form(&u, req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(
        -item::n(&r["FriendshipPointResult"], "AddValue"),
        (full * 60 + 99) / 100
    );
    assert_eq!(r["FriendshipPointResult"]["NewDailyAccValue"], 340);
}
#[tokio::test]
async fn legacy_hero_cap_rejects_limit_break_without_spending_materials() {
    let (s, u) = setup().await;
    crate::db::query("UPDATE heroes SET star=5,transcend=5,level=100 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let row = s
        .tables
        .hero_shop
        .limit_breaks
        .iter()
        .find(|v| v["HeroIndex"] == 1 && v["LimitBreakLevel"] == 1)
        .unwrap();
    for i in 1..=3 {
        put(
            &s,
            &u,
            item::n(row, &format!("MaterialItemIndex{i}")) as i32,
            item::n(row, &format!("MaterialItemCount{i}")),
        )
        .await;
    }
    let before = get(&s, &u, 1).await;
    let gold = balance(&s, &u, "gold").await;
    assert_ne!(
        hero::hero_limit_break_level_up(State(s.clone()), form(&u, "HeroIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let exp = &s.tables.hero_shop.limit_exp_items[0];
    let id = item::n(exp, "ItemIndex");
    let qty =
        (item::n(row, "ReqLocalExp") + item::n(exp, "ExpAmount") - 1) / item::n(exp, "ExpAmount");
    put(&s, &u, id as i32, qty).await;
    let r = hero::hero_limit_break_exp_up(
        State(s.clone()),
        form(
            &u,
            &format!("HeroIndex=1&ExpItemIndices=[{id}]&ExpItemCounts=[{qty}]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(r["Result"], "Success");
    assert_eq!(count(&s, &u, id as i32).await, qty);
    for i in 1..=3 {
        let id=item::n(row, &format!("MaterialItemIndex{i}")) as i32;
        let qty=item::n(row, &format!("MaterialItemCount{i}"));
        if qty>0 { assert_eq!(count(&s,&u,id).await,qty); }
    }
    assert_eq!(get(&s, &u, 1).await, before);
    assert_eq!(balance(&s, &u, "gold").await, gold);
    assert_eq!(login(&s).await.heroes[0].level, 100);
    // Even a stale record from the later client cannot gain Limit Break EXP.
    let mut extra = crate::api::heroes::details(
        &mut s.db.acquire().await.unwrap(),
        u.user_info.account_id,
        1,
    )
    .await
    .unwrap();
    extra["LimitBreakLevel"] = serde_json::json!(1);
    crate::api::heroes::save_details(
        &mut s.db.acquire().await.unwrap(),
        u.user_info.account_id,
        1,
        &extra,
    )
    .await
    .unwrap();
    assert_ne!(
        hero::hero_limit_break_exp_up(
            State(s.clone()),
            form(
                &u,
                &format!("HeroIndex=1&ExpItemIndices=[{id}]&ExpItemCounts=[{qty}]")
            )
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(get(&s, &u, 1).await["Level"], 100);
    assert_eq!(count(&s, &u, id as i32).await, qty);
}

#[tokio::test]
async fn selectors_and_growth_items_validate_allowed_targets() {
    let (s, u) = setup().await;
    let selector = s
        .tables
        .hero_shop
        .costume_selectors
        .iter()
        .find(|v| {
            item::n(v, "CostumeSelectItemType") == 0
                && v["CostumeIndices"]
                    .as_array()
                    .is_some_and(|a| a.contains(&serde_json::json!(106)))
        })
        .unwrap();
    let id = item::n(selector, "ItemIndex") as i32;
    put(&s, &u, id, 1).await;
    assert_ne!(
        hero::use_costume_select_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&CostumeIndex=999999"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, id).await, 1);
    assert_eq!(
        hero::use_costume_select_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&CostumeIndex=106"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let bonus = crate::api::battle::bonuses::calculate(
        &mut s.db.acquire().await.unwrap(),
        &s,
        u.user_info.account_id,
        1,
    )
    .await
    .unwrap();
    assert!(bonus.0 > 0 || bonus.1 > 0);
    let growth = s
        .tables
        .hero_shop
        .growth_items
        .iter()
        .find(|v| {
            v["HeroIndices"]
                .as_array()
                .unwrap()
                .contains(&serde_json::json!(1))
        })
        .unwrap();
    let id = item::n(growth, "ItemIndex") as i32;
    put(&s, &u, id, 2).await;
    assert_eq!(
        hero::use_hero_growth_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&HeroIndex=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let h = get(&s, &u, 1).await;
    assert_eq!(h["Star"], 5);
    assert_eq!(h["Transcended"], 5);
    assert_eq!(h["Level"], 100);
    assert_ne!(
        hero::use_hero_growth_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&HeroIndex=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, id).await, 1);
    // Existing campaign code must not lower a hero grown to level 100.
    let _ = campaign::end_campaign(
        State(s.clone()),
        form(&u, "ChapterIndex=1&DungeonIndex=1&Completed=true"),
    )
    .await
    .unwrap();
    assert_eq!(get(&s, &u, 1).await["Level"], 100);
}

#[tokio::test]
async fn category_costume_tickets_accept_native_group_ids_and_preserve_failed_claims() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let mut tested=0;
    for selector in &s.tables.hero_shop.costume_selectors {
        let Some(groups)=selector["CostumeCategories"].as_array().filter(|v|!v.is_empty()) else {continue;};
        if item::n(selector,"CostumeSelectItemType")!=0 {continue;}
        let id=item::n(selector,"ItemIndex") as i32;
        let selected=*s.tables.hero_shop.costumes.iter().find(|(_,c)|groups.contains(&c["Group"])
            && !c["BonusCostumeIndices"].as_array().is_some_and(|b|!b.is_empty() && c["MainIndex"]!=c["CostumeIndex"]))
            .expect("Shipped ticket must offer costumes").0;
        let outside=*s.tables.hero_shop.costumes.iter().find(|(_,c)| !groups.contains(&c["Group"])
            && !selector["CostumeIndices"].as_array().is_some_and(|v|v.contains(&c["CostumeIndex"]))).unwrap().0;
        crate::db::query("DELETE FROM costumes WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
        put(&s,&u,id,2).await;
        let failed=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={id}&CostumeIndex={outside}"))).await.unwrap().0;
        assert_ne!(failed["Result"],"Success","{id}: {failed}");
        assert_eq!(count(&s,&u,id).await,2);
        let out=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={id}&CostumeIndex={selected}"))).await.unwrap().0;
        assert_eq!(out["Result"],"Success","ticket {id}, costume {selected}: {out}");
        assert_eq!(out["CostumeResults"][0]["CostumeIndex"],selected);
        assert_eq!(count(&s,&u,id).await,1);
        let duplicate=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={id}&CostumeIndex={selected}"))).await.unwrap().0;
        assert_ne!(duplicate["Result"],"Success");
        assert_eq!(count(&s,&u,id).await,1);
        tested+=1;
    }
    assert_eq!(tested,38);
}

#[tokio::test]
async fn category_costume_ticket_other_reward_requires_every_eligible_costume() {
    let (s,u)=setup().await;
    let selector=s.tables.hero_shop.costume_selectors.iter().find(|r|r["ItemIndex"]==2137).unwrap();
    let groups=selector["CostumeCategories"].as_array().unwrap();
    put(&s,&u,2137,2).await;
    let request="ItemIndex=2137&GetOtherReward=true";
    let failed=hero::use_costume_select_item(State(s.clone()),form(&u,request)).await.unwrap().0;
    assert_ne!(failed["Result"],"Success");
    assert_eq!(count(&s,&u,2137).await,2);
    for (id,costume) in &s.tables.hero_shop.costumes {
        if groups.contains(&costume["Group"]) {
            crate::db::query("INSERT INTO costumes(account_id,costume_index) VALUES($1,$2) ON CONFLICT DO NOTHING")
                .bind(u.user_info.account_id).bind(id).execute(&s.db).await.unwrap();
        }
    }
    let out=hero::use_costume_select_item(State(s.clone()),form(&u,request)).await.unwrap().0;
    assert_eq!(out["Result"],"Success","{out}");
    assert_eq!(count(&s,&u,2137).await,1);
    assert!(out["CurrencyResults"].as_array().is_some_and(|v|!v.is_empty())
        || out["ItemResults"].as_array().is_some_and(|v|!v.is_empty()));
}
#[tokio::test]
async fn multiple_hero_selector_grants_two_allowed_heroes_and_rolls_back_partial_failure() {
    let (s, u) = setup().await;
    let row = s
        .tables
        .hero_shop
        .multi_hero_items
        .iter()
        .find(|v| v["HeroStar"] == 5)
        .unwrap();
    let id = item::n(row, "ItemIndex") as i32;
    let left = row["HeroIndices1"][0].as_i64().unwrap();
    let right = row["HeroIndices2"][0].as_i64().unwrap();
    put(&s, &u, id, 1).await;
    let r = hero::use_multiple_hero_select_item(
        State(s.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&LeftHeroIndices=[{left}]&RightHeroIndices=[99999]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(r["Result"], "Success");
    assert_eq!(login(&s).await.heroes.len(), 1);
    assert_eq!(count(&s, &u, id).await, 1);
    let r = hero::use_multiple_hero_select_item(
        State(s.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&LeftHeroIndices=[{left}]&RightHeroIndices=[{right}]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroResults"].as_array().unwrap().len(), 2);
    assert_eq!(login(&s).await.heroes.len(), 3);
}
#[tokio::test]
async fn hero_presets_save_restore_and_clearing_preserves_paid_slot() {
    let (s, u) = setup().await;
    let free = s
        .tables
        .hero_shop
        .preset_slots
        .iter()
        .find(|(_, v)| v["IsPaid"] == false)
        .unwrap()
        .0
        .clone();
    let paid = s
        .tables
        .hero_shop
        .preset_slots
        .iter()
        .find(|(_, v)| v["IsPaid"] == true)
        .unwrap()
        .0
        .clone();
    let c = &s.tables.hero_shop.heroes[&1];
    let mut db = s.db.begin().await.unwrap();
    let r = hero::recruit_at(&mut db, &s, u.user_info.account_id, 2, 1, 1, 0)
        .await
        .unwrap();
    assert_eq!(r["HeroInfo"]["HeroIndex"], 2);
    db.commit().await.unwrap();
    let equipped: Option<i32> =
        crate::db::query_scalar("SELECT slot_index FROM equip_items WHERE account_id=$1 LIMIT 1")
            .bind(u.user_info.account_id)
            .fetch_optional(&s.db)
            .await
            .unwrap();
    let slot = if let Some(i) = equipped {
        i
    } else {
        let id = *s
            .tables
            .hero_shop
            .items
            .keys()
            .find(|i| {
                s.tables
                    .items
                    .reward_item(**i)
                    .is_some_and(|v| v.kind == "Equip")
            })
            .unwrap();
        let mut tx = s.db.begin().await.unwrap();
        let mut r = crate::api::tutorial::Rewards::default();
        item::give(
            &mut tx,
            &s,
            u.user_info.account_id,
            ItemGrant {
                index: id,
                count: 1,
                star: 0,
                custom: 0,
            },
            &mut r,
        )
        .await
        .unwrap();
        tx.commit().await.unwrap();
        r.equipment[0].slot_index
    };
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1")
        .bind(slot)
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::add_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=0 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::set_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(get(&s, &u, 1).await["EquipItemSlotIndex1"], slot);
    crate::db::query("UPDATE equip_items SET inventory_type=1 WHERE account_id=$1 AND slot_index=$2")
        .bind(u.user_info.account_id)
        .bind(slot)
        .execute(&s.db)
        .await
        .unwrap();
    assert_ne!(
        hero::set_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_ne!(
        hero::add_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let before = balance(&s, &u, "gem").await;
    let price = item::n(&s.tables.hero_shop.preset_slots[&paid], "BuyGem");
    assert_eq!(
        hero::buy_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let _ =
        hero::remove_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap();
    assert_ne!(
        hero::buy_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gem").await, before - price);
    let _ = c;
    let _ = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[2,1]"))
        .await
        .unwrap();
    let r = crate::api::account::lobby::first_lobby(
        State(s.clone()),
        axum::extract::Form(crate::api::account::lobby::FirstLobbyRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            nick: None,
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r.player_book_mark_hero_info["BookMarkHeroIndices"], "2,1");
    assert_eq!(r.hero_preset_storages.len(), 2);
}

#[tokio::test]
async fn deleted_preset_gear_keeps_current_equipment_and_other_slots() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let key=s.tables.hero_shop.preset_slots.iter().find(|(_,v)|v["IsPaid"]==false).unwrap().0.clone();
    let mut tx=s.db.begin().await.unwrap();let mut rewards=crate::api::tutorial::Rewards::default();
    item::give(&mut tx,&s,a,ItemGrant{index:1001,count:2,star:0,custom:0},&mut rewards).await.unwrap();
    tx.commit().await.unwrap();let old=rewards.equipment[0].slot_index;let current=rewards.equipment[1].slot_index;
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1").bind(old).bind(a).execute(&s.db).await.unwrap();
    assert_eq!(hero::add_hero_storage_slot(State(s.clone()),form(&u,&format!("StorageKey={key}"))).await.unwrap().0["Result"],"Success");
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1").bind(current).bind(a).execute(&s.db).await.unwrap();
    crate::db::query("DELETE FROM equip_items WHERE slot_index=$1 AND account_id=$2").bind(old).bind(a).execute(&s.db).await.unwrap();
    let out=hero::set_hero_storage_slot(State(s.clone()),form(&u,&format!("StorageKey={key}"))).await.unwrap().0;
    assert_eq!(out["Result"],"Success","{out}");assert_eq!(get(&s,&u,1).await["EquipItemSlotIndex1"],current);
    let stored:String=crate::db::query_scalar("SELECT data FROM hero_presets WHERE account_id=$1 AND storage_key=$2")
        .bind(a).bind(&key).fetch_one(&s.db).await.unwrap();
    let stored:Value=serde_json::from_str(&stored).unwrap();
    let saved=stored.as_array().unwrap().iter().find(|p|p["HeroIndex"]==1).unwrap();
    assert_eq!(saved["EquipItemSlotIndex"][0],current);
    assert_eq!(saved["EquipItemUid"][0],current.to_string());
}

#[tokio::test]
async fn every_advancement_costume_can_be_purchased_at_its_listed_price() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gem=1000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let costumes:Vec<_>=s.tables.hero_shop.costumes.values().filter(|c|c["BodyPrefix"].as_str().is_some_and(|v|v.contains("Advance")) && c["IsDefault"]==false).cloned().collect();
    assert_eq!(costumes.len(),21);
    for c in costumes {
        let hero_index=item::n(&c,"HeroIndex");let id=item::n(&c,"CostumeIndex");let price=item::n(&c,"ReqBuyGem");
        assert!(price>0);assert_eq!(c["Buyable"],true);
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES($1,$2,$3,100,5) ON CONFLICT DO NOTHING").bind(a).bind(hero_index).bind(hero_index).execute(&s.db).await.unwrap();
        // Couple variants can grant each other as a bonus; test every direct sale independently.
        crate::db::query("DELETE FROM costumes WHERE account_id=$1 AND costume_index=$2").bind(a).bind(id).execute(&s.db).await.unwrap();
        let out=hero::buy_costume(State(s.clone()),form(&u,&format!("HeroIndex={hero_index}&CostumeIndex={id}&BuyGem={price}&BuyGold=0&BuyMileage=0"))).await.unwrap().0;
        assert_eq!(out["Result"],"Success","costume {id}: {out}");
    }
}
#[tokio::test]
async fn shop_purchase_limits_reset_and_failed_delivery_does_not_charge() {
    let (mut s, u) = setup().await;
    let table = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).hero_shop);
    let r = table
        .shop_items
        .iter_mut()
        .find(|v| v["ShopIndex"] == 10 && v["Index"] == 1)
        .unwrap();
    r["PurchasableCount"] = serde_json::json!(1);
    r["DailyReset"] = serde_json::json!(true);
    crate::db::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = "ShopIndex=10&ShopItemIndex=1&ShopItemPurchaseCount=1";
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let before = balance(&s, &u, "friendship_point").await;
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "SoldOut"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    crate::db::query("UPDATE shop_purchase_ledger SET period='day:2000-01-01' WHERE account_id=$1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let bad = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory);
    bad.items.get_mut(&41002).unwrap()["ItemMaxCap"] = serde_json::json!(1);
    put(&s, &u, 41002, 1).await;
    let before = balance(&s, &u, "friendship_point").await;
    assert_ne!(
        shop::buy_shop_item(
            State(s.clone()),
            form(&u, "ShopIndex=10&ShopItemIndex=2&ShopItemPurchaseCount=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    assert_eq!(count(&s, &u, 41002).await, 1);
}
#[tokio::test]
async fn inn_recruitment_uses_table_star_account_local_ids_and_cannot_duplicate_shop_heroes() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    let _ = hero::buy_hero(
        State(s.clone()),
        form(
            &u,
            &format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}"),
        ),
    )
    .await
    .unwrap();
    crate::db::query("INSERT INTO hero_friendly_info(account_id,hero_index,friendly_point) VALUES ($1,$2,1000) ON CONFLICT(account_id) DO UPDATE SET hero_index=excluded.hero_index,friendly_point=excluded.friendly_point").bind(u.user_info.account_id).bind(id).execute(&s.db).await.unwrap();
    let r = crate::api::heroes::inn::recruit_hero(
        State(s.clone()),
        axum::extract::Form(crate::api::heroes::inn::RecruitHeroRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            hero_index: Some(id),
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "AlreadyRecruited");
    let id = if id == 2 { 3 } else { 2 };
    crate::db::query(
        "UPDATE hero_friendly_info SET hero_index=$1,friendly_point=100000 WHERE account_id=$2",
    )
    .bind(id)
    .bind(u.user_info.account_id)
    .execute(&s.db)
    .await
    .unwrap();
    let r = crate::api::heroes::inn::recruit_hero(
        State(s.clone()),
        axum::extract::Form(crate::api::heroes::inn::RecruitHeroRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            hero_index: Some(id),
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(
        r["HeroResult"]["HeroInfo"]["Star"],
        s.tables.hero_shop.heroes[&id]["StartHeroStar"]
    );
    assert!(r["HeroResult"]["TeamExpResult"].is_object());
    assert_eq!(login(&s).await.heroes.len(), 3);
}


#[tokio::test]
async fn fallen_heroes_accept_native_items_but_recruit_at_base_progression() {
    let (s,u)=setup().await;
    crate::db::query("UPDATE user_info SET gem=26000,pay_gem=0,mileage=0 WHERE account_id=$1")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let wrong=hero::buy_hero(State(s.clone()),form(&u,"HeroIndex=84&ItemIndex=1008510&BuyGem=6000")).await.unwrap().0;
    assert_eq!(wrong["Result"],"InvalidItemIndex");
    for (id,item) in [(83,1008310),(84,1008410),(85,1008510)] {
        let bad=hero::buy_hero(State(s.clone()),form(&u,&format!("HeroIndex={id}&ItemIndex={item}&BuyGem=1"))).await.unwrap().0;
        assert_eq!(bad["Result"],"InvalidPrice");
        let req=format!("HeroIndex={id}&ItemIndex={item}&BuyGem=6000");
        let r=hero::buy_hero(State(s.clone()),form(&u,&req)).await.unwrap().0;
        assert_eq!(r["Result"],"Success","{r}");
        let h=&r["HeroResult"]["HeroInfo"];
        assert_eq!(h["HeroIndex"],id);
        assert_eq!(h["Level"],1);
        assert_eq!(h["Star"],5);
        assert_eq!(h["Transcended"],0);
        assert_eq!(hero::buy_hero(State(s.clone()),form(&u,&req)).await.unwrap().0["Result"],"HeroAlreadyExist");
    }
    assert_eq!(balance(&s,&u,"gem").await,8000);
    // Rebel Clause's OpenStatus=1 price grants 700 mileage; the others grant 600.
    assert_eq!(balance(&s,&u,"mileage").await,1900);
}

#[tokio::test]
async fn native_perk_forms_preserve_pairs_points_validation_reset_and_login() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    crate::db::query(
        "UPDATE heroes SET star=5,transcend=5,level=100 WHERE account_id=$1 AND hero_index=1",
    )
    .bind(a)
    .execute(&s.db)
    .await
    .unwrap();
    crate::db::query("UPDATE user_info SET gold=1000000 WHERE account_id=$1")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let mut extra = hero::details(&mut s.db.acquire().await.unwrap(), a, 1)
        .await
        .unwrap();
    extra["TranscendPoint"] = serde_json::json!(10);
    hero::save_details(&mut s.db.acquire().await.unwrap(), a, 1, &extra)
        .await
        .unwrap();
    let result=hero::learn_hero_transcend_skill_page(State(s.clone()),form(&u,
        "HeroIndex=1&PageIndex=1&TranscendSkills=10&TranscendSkills=1&TranscendSkills=20&TranscendSkills=1")).await.unwrap().0;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(result["Hero"]["TranscendSkillPage1"],"[10,20]");
    let result=hero::learn_hero_transcend_skill_page(State(s.clone()),form(&u,
        "HeroIndex=1&PageIndex=1&TranscendSkills=11&TranscendSkills=1")).await.unwrap().0;
    assert_eq!(result["Result"],"Success","{result}");
    let saved=get(&s,&u,1).await["TranscendSkillPage1"].clone();
    for fields in ["TranscendSkills=12", "TranscendSkills=12&TranscendSkills=0",
        "TranscendSkills=12&TranscendSkills=1&TranscendSkills=12&TranscendSkills=1",
        "TranscendSkills=30&TranscendSkills=1&TranscendSkills=31&TranscendSkills=1"] {
        let result=hero::learn_hero_transcend_skill_page(State(s.clone()),form(&u,&format!("HeroIndex=1&PageIndex=1&{fields}"))).await.unwrap().0;
        assert_ne!(result["Result"],"Success","{result}");
        assert_eq!(get(&s,&u,1).await["TranscendSkillPage1"],saved);
    }
    let fields=[12,13,14,21,22].iter().map(|code|format!("TranscendSkills={code}&TranscendSkills=1")).collect::<Vec<_>>().join("&");
    let result=hero::learn_hero_transcend_skill_page(State(s.clone()),form(&u,&format!("HeroIndex=1&PageIndex=1&{fields}"))).await.unwrap().0;
    assert_eq!(result["Result"],"NotEnoughSkillPoint","{result}");
    assert_eq!(get(&s,&u,1).await["TranscendSkillPage1"],saved);
    let result=hero::reset_hero_transcend_skill_page(State(s.clone()),form(&u,"HeroIndex=1&PageIndex=1")).await.unwrap().0;
    assert_eq!(result["Result"],"Success");
    assert_eq!(balance(&s,&u,"gold").await,500000);
    let result=hero::learn_hero_transcend_skill_page(State(s.clone()),form(&u,
        "HeroIndex=1&PageIndex=1&TranscendSkills=12&TranscendSkills=1")).await.unwrap().0;
    assert_eq!(result["Result"],"Success");
    let logged=login(&s).await;
    assert_eq!(logged.heroes.iter().find(|h|h.hero_index==1).unwrap().details["TranscendSkillPage1"],"[12]");
    assert_eq!(logged.heroes.iter().find(|h|h.hero_index==1).unwrap().details["TranscendPoint"],10);
}


#[tokio::test]
async fn xerah_native_shop_items_recruit_at_base_progression_and_preserve_guards() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    for item in [776, 1007710] {
        crate::db::query("DELETE FROM heroes WHERE account_id=$1 AND hero_index=77")
            .bind(account).execute(&s.db).await.unwrap();
        crate::db::query("UPDATE user_info SET gem=10000,pay_gem=0,mileage=0 WHERE account_id=$1")
            .bind(account).execute(&s.db).await.unwrap();
        for (fields, expected) in [
            ("HeroIndex=77&ItemIndex=816&BuyGem=6000".to_string(), "InvalidItemIndex"),
            (format!("HeroIndex=77&ItemIndex={item}&BuyGem=1"), "InvalidPrice"),
        ] {
            let rejected = hero::buy_hero(State(s.clone()), form(&u, &fields)).await.unwrap().0;
            assert_eq!(rejected["Result"], expected, "{rejected}");
            assert_eq!(balance(&s, &u, "gem").await, 10000);
            assert_eq!(balance(&s, &u, "mileage").await, 0);
        }
        let request = format!("HeroIndex=77&ItemIndex={item}&BuyGem=6000");
        let result = hero::buy_hero(State(s.clone()), form(&u, &request)).await.unwrap().0;
        assert_eq!(result["Result"], "Success", "{result}");
        let h = &result["HeroResult"]["HeroInfo"];
        assert_eq!(h["HeroIndex"], 77);
        assert_eq!(h["Level"], 1);
        assert_eq!(h["Star"], 5);
        assert_eq!(h["Transcended"], 0);
        assert!(result["HeroResult"]["TeamExpResult"].is_object());
        assert_eq!(balance(&s, &u, "gem").await, 4000);
        assert_eq!(balance(&s, &u, "mileage").await, 600);
        let duplicate = hero::buy_hero(State(s.clone()), form(&u, &request)).await.unwrap().0;
        assert_eq!(duplicate["Result"], "HeroAlreadyExist");
        assert_eq!(balance(&s, &u, "gem").await, 4000);
        assert_eq!(balance(&s, &u, "mileage").await, 600);
        assert!(login(&s).await.heroes.iter().any(|h| h.hero_index == 77 && h.level == 1 && h.star == 5 && h.transcend == 0));
    }
}

#[tokio::test]
async fn all_roster_costume_tickets_cover_102_heroes_and_preserve_failed_claims() {
    use std::collections::BTreeSet;
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let expected:BTreeSet<i64>=(1..=102).filter(|i|*i!=14).chain([111]).collect();
    for ticket in [600035,600100] {
        let row=s.tables.hero_shop.costume_selectors.iter().find(|r|r["ItemIndex"]==ticket).unwrap();
        let choices=row["CostumeIndices"].as_array().unwrap();
        assert_eq!(choices.len(),102);
        let actual:BTreeSet<_>=choices.iter().map(|v|s.tables.hero_shop.costumes[&(v.as_i64().unwrap() as i32)]["HeroIndex"].as_i64().unwrap()).collect();
        assert_eq!(actual,expected);
    }
    // A valid but unowned hero, an outside costume and the all-owned shortcut
    // must never consume a ticket or grant a costume.
    crate::db::query("DELETE FROM heroes WHERE account_id=$1 AND hero_index=111").bind(a).execute(&s.db).await.unwrap();
    put(&s,&u,600100,103).await;
    for query in ["CostumeIndex=11106","CostumeIndex=105","GetOtherReward=true"] {
        let out=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex=600100&{query}"))).await.unwrap().0;
        assert_ne!(out["Result"],"Success","{out}");
        assert_eq!(count(&s,&u,600100).await,103);
    }
    for hero_index in &expected {
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level,transcend) SELECT $1,COALESCE(MAX(hero_id),0)+1,$2,5,1,0 FROM heroes WHERE account_id=$3 HAVING NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5) ON CONFLICT DO NOTHING")
            .bind(a).bind(hero_index).bind(a).bind(a).bind(hero_index).execute(&s.db).await.unwrap();
    }
    for ticket in [600035,600100] {
        crate::db::query("DELETE FROM costumes WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
        put(&s,&u,ticket,103).await;
        let row=s.tables.hero_shop.costume_selectors.iter().find(|r|r["ItemIndex"]==ticket).unwrap();
        for choice in row["CostumeIndices"].as_array().unwrap() {
            let out=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={ticket}&CostumeIndex={choice}"))).await.unwrap().0;
            assert_eq!(out["Result"],"Success","{ticket} {choice}: {out}");
        }
        assert_eq!(count(&s,&u,ticket).await,1);
        let first=row["CostumeIndices"][0].as_i64().unwrap();
        let duplicate=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={ticket}&CostumeIndex={first}"))).await.unwrap().0;
        assert_ne!(duplicate["Result"],"Success");assert_eq!(count(&s,&u,ticket).await,1);
        let before=balance(&s,&u,"gem").await;
        let out=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={ticket}&GetOtherReward=true"))).await.unwrap().0;
        assert_eq!(out["Result"],"Success","{out}");assert_eq!(count(&s,&u,ticket).await,0);
        assert_eq!(balance(&s,&u,"gem").await,before+2100);
    }
}

#[tokio::test]
async fn honor_costumes_sell_for_legend_rubies_without_replacing_honor_shop() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let costumes:Vec<_>=s.tables.hero_shop.costumes.values()
        .filter(|c|c["BodyPrefix"].as_str().is_some_and(|p|p.contains("GloryLeague"))).cloned().collect();
    assert_eq!(costumes.len(),80);
    let legend=s.tables.hero_shop.costumes.values().find(|c|c["CostumeType"]==2).unwrap();
    assert_eq!(item::n(legend,"ReqBuyGem"),10000);
    let buy=|hero:i64,id:i64,price:i64|format!("HeroIndex={hero}&CostumeIndex={id}&BuyGem={price}&BuyGold={price}&BuyMileage={price}");
    let before=balance(&s,&u,"gem").await;
    for req in [buy(84,8409,10000),buy(1,113,9999)] {
        let out=hero::buy_costume(State(s.clone()),form(&u,&req)).await.unwrap().0;
        assert_ne!(out["Result"],"Success","{out}");
        assert_eq!(balance(&s,&u,"gem").await,before);
    }
    crate::db::query("UPDATE user_info SET gem=9999,pay_gem=0 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let out=hero::buy_costume(State(s.clone()),form(&u,&buy(1,113,10000))).await.unwrap().0;
    assert_ne!(out["Result"],"Success");assert_eq!(balance(&s,&u,"gem").await,9999);
    crate::db::query("UPDATE user_info SET gem=1000000 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let gold=balance(&s,&u,"gold").await;
    let mut spent=0;
    for c in costumes {
        let id=item::n(&c,"CostumeIndex");let h=item::n(&c,"HeroIndex");
        assert_eq!(c["Group"],"LeagueOfHonor");assert_eq!(c["Buyable"],true);
        assert_eq!(item::n(&c,"ReqBuyGem"),10000);assert_eq!(item::n(&c,"ReqBuyGold"),0);assert_eq!(item::n(&c,"ReqBuyMileage"),0);
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,COALESCE((SELECT MAX(hero_id)+1 FROM heroes WHERE account_id=$2),1),$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
            .bind(a).bind(a).bind(h).bind(a).bind(h).execute(&s.db).await.unwrap();
        let out=hero::buy_costume(State(s.clone()),form(&u,&buy(h,id,10000))).await.unwrap().0;
        assert_eq!(out["Result"],"Success","{id}: {out}");spent+=10000;
        assert_eq!(balance(&s,&u,"gem").await,1000000-spent);
        assert_eq!(balance(&s,&u,"gold").await,gold);
        let duplicate=hero::buy_costume(State(s.clone()),form(&u,&buy(h,id,10000))).await.unwrap().0;
        assert_eq!(duplicate["Result"],"AlreadyHaveCostume");assert_eq!(balance(&s,&u,"gem").await,1000000-spent);
    }
    let archived:Value=serde_json::from_str(include_str!("../../../../tables/ArenaShopCatalog.json")).unwrap();
    for row in archived["ShopItems"].as_array().unwrap().iter().filter(|r|r["ShopIndex"]==15) {
        assert!(s.tables.hero_shop.shop_items.iter().any(|r|r["ShopIndex"]==15 && r["Index"]==row["Index"] && r["ItemIndex"]==row["ItemIndex"] && r["BuyCostTypeForItem"]==row["BuyCostTypeForItem"]));
    }
}

#[tokio::test]
async fn ashen_costumes_sell_for_3000_rubies_and_retain_dedicated_tickets() {
    let (s,u)=setup().await;
    let account=u.user_info.account_id;
    crate::db::query("UPDATE user_info SET gem=10000,pay_gem=0 WHERE account_id=$1").bind(account).execute(&s.db).await.unwrap();
    for (hero_index,costume,ticket) in [(84,8408,600066),(85,8508,600067),(83,8307,600068)] {
        let row=&s.tables.hero_shop.costumes[&costume];
        assert_eq!(row["Group"],"Ash1");assert_eq!(row["Buyable"],true);
        assert_eq!(row["ReqBuyGem"],3000);assert_eq!(row["ReqBuyGold"],0);assert_eq!(row["ReqBuyMileage"],0);
        let selector=s.tables.hero_shop.costume_selectors.iter().find(|r|r["ItemIndex"]==ticket).unwrap();
        assert_eq!(selector["CostumeIndices"],serde_json::json!([costume]));
        crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index) SELECT $1,COALESCE((SELECT MAX(hero_id)+1 FROM heroes WHERE account_id=$2),1),$3 WHERE NOT EXISTS(SELECT 1 FROM heroes WHERE account_id=$4 AND hero_index=$5)")
            .bind(account).bind(account).bind(hero_index).bind(account).bind(hero_index).execute(&s.db).await.unwrap();
        let before=balance(&s,&u,"gem").await;
        let request=|price|format!("HeroIndex={hero_index}&CostumeIndex={costume}&BuyGem={price}&BuyGold=0&BuyMileage=0");
        let rejected=hero::buy_costume(State(s.clone()),form(&u,&request(2999))).await.unwrap().0;
        assert_ne!(rejected["Result"],"Success");assert_eq!(balance(&s,&u,"gem").await,before);
        let result=hero::buy_costume(State(s.clone()),form(&u,&request(3000))).await.unwrap().0;
        assert_eq!(result["Result"],"Success","{result}");assert_eq!(balance(&s,&u,"gem").await,before-3000);
        let duplicate=hero::buy_costume(State(s.clone()),form(&u,&request(3000))).await.unwrap().0;
        assert_eq!(duplicate["Result"],"AlreadyHaveCostume");assert_eq!(balance(&s,&u,"gem").await,before-3000);
        // Exercise the original ticket as well as direct purchase.
        crate::db::query("DELETE FROM costumes WHERE account_id=$1 AND costume_index=$2").bind(account).bind(costume).execute(&s.db).await.unwrap();
        put(&s,&u,ticket,1).await;
        let claim=hero::use_costume_select_item(State(s.clone()),form(&u,&format!("ItemIndex={ticket}&CostumeIndex={costume}"))).await.unwrap().0;
        assert_eq!(claim["Result"],"Success","{claim}");assert_eq!(count(&s,&u,ticket).await,0);
        assert_eq!(balance(&s,&u,"gem").await,before-3000);
    }
}

#[tokio::test]
async fn applying_a_preset_moves_gear_back_and_reports_every_removed_piece() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let key=s.tables.hero_shop.preset_slots.iter().find(|(_,v)|v["IsPaid"]==false).unwrap().0.clone();
    let mut tx=s.db.begin().await.unwrap();let mut rewards=crate::api::tutorial::Rewards::default();
    item::give(&mut tx,&s,a,ItemGrant{index:1001,count:2,star:0,custom:0},&mut rewards).await.unwrap();
    tx.commit().await.unwrap();let x=rewards.equipment[0].slot_index;let y=rewards.equipment[1].slot_index;
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1").bind(x).bind(a).execute(&s.db).await.unwrap();
    assert_eq!(hero::add_hero_storage_slot(State(s.clone()),form(&u,&format!("StorageKey={key}"))).await.unwrap().0["Result"],"Success");
    // After saving, the piece moves to Hero 2 (not part of the preset) and
    // Hero 1 wears another one.
    crate::db::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES($1,2,2,1,1) ON CONFLICT DO NOTHING").bind(a).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE heroes SET equip_item_slot_index_1=$1 WHERE account_id=$2 AND hero_index=1").bind(y).bind(a).execute(&s.db).await.unwrap();
    crate::db::query("UPDATE heroes SET equip_item_slot_index_2=$1 WHERE account_id=$2 AND hero_index=2").bind(x).bind(a).execute(&s.db).await.unwrap();
    let out=hero::set_hero_storage_slot(State(s.clone()),form(&u,&format!("StorageKey={key}"))).await.unwrap().0;
    assert_eq!(out["Result"],"Success","{out}");
    assert_eq!(get(&s,&u,1).await["EquipItemSlotIndex1"],x);
    assert_eq!(get(&s,&u,2).await["EquipItemSlotIndex2"],0);
    let removed=out["UnEquippedSlotIndices"].as_array().unwrap();
    assert!(removed.contains(&serde_json::json!(x)) && removed.contains(&serde_json::json!(y)),"{out}");
    let wearing:i64=crate::db::query_scalar("SELECT count(*) FROM heroes WHERE account_id=$1 AND $2 IN (equip_item_slot_index_1,equip_item_slot_index_2,equip_item_slot_index_3,equip_item_slot_index_4,equip_item_slot_index_5,equip_item_slot_index_6,equip_item_slot_index_7,equip_item_slot_index_8,equip_item_slot_index_9,equip_item_slot_index_10)")
        .bind(a).bind(x).fetch_one(&s.db).await.unwrap();
    assert_eq!(wearing,1,"a piece is worn by exactly one Hero");
    let heroes=out["Heroes"].as_array().unwrap();
    assert_eq!(heroes.iter().find(|h|h["HeroIndex"]==1).unwrap()["EquipItemSlotIndex1"],x);
}
