use super::*;
use crate::api::{account::user, extensions, inventory::item, tutorial::Rewards};
use crate::models::item::ItemGrant;
use axum::{body::Bytes, extract::{OriginalUri, State}};

async fn setup() -> (AppState, user::LoginResponse) {
    let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1)
        .connect("sqlite::memory:").await.unwrap();
    crate::database::create_tables(&db).await.unwrap();
    let s = AppState::new(db, crate::tables::test_tables());
    let u = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=episode-test"))
        .await.unwrap().0;
    sqlx::query("UPDATE user_info SET gold=100000000,gem=100000,stamina=1000000")
        .execute(&s.db).await.unwrap();
    (s,u)
}

async fn call(s: &AppState, u: &user::LoginResponse, action: &str, args: &str) -> Value {
    extensions::handle(State(s.clone()), OriginalUri(format!("/npc/{action}").parse().unwrap()),
        Bytes::from(format!("SessionKey={}&{args}",u.user_info.session_key)))
        .await.unwrap().0
}

async fn give(s: &AppState, a: i64, index: i32, count: i32) {
    let mut tx=s.db.begin().await.unwrap();
    let mut rewards=Rewards::default();
    item::give(&mut tx,s,a,ItemGrant {index,count,star:0,custom:0},&mut rewards).await.unwrap();
    tx.commit().await.unwrap();
}

#[tokio::test]
async fn episodes_native_gifts_claim_original_rewards_and_persist_on_login() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let initial_heroes:i64=sqlx::query_scalar("SELECT COUNT(*) FROM heroes WHERE account_id=?")
        .bind(a).fetch_one(&s.db).await.unwrap();
    for (npc,gift) in [(827,47021),(831,47022),(835,47023),(837,47024),(838,47031),(839,47032),(80,47033)] {
        let rows:Vec<_>=s.tables.extensions.rows("NPCFriendlyPoint").iter()
            .filter(|r|n(r,"HeroIndex")==npc).collect();
        let max=rows.iter().map(|r|n(r,"Step")).max().unwrap();
        let points:i64=rows.iter().filter(|r|n(r,"Step")<max).map(|r|n(r,"ReqFriendlyPoint")).sum();
        give(&s,a,gift,(points+100) as i32).await;
        // Native WWWForm sends scalar/repeated array keys, not only JSON arrays.
        let r=call(&s,&u,"give_gift_item",&format!("HeroIndex={npc}&ItemIndices={gift}&ItemCount={points}")).await;
        assert_eq!(r["Result"],"Success","{r}");
        assert_eq!(r["FriendlyInfo"]["FriendlyPoint"],points);
        assert_ne!(call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step=7")).await["Result"],"Success");
        for step in 1..max {
            let r=call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step={step}")).await;
            assert_eq!(r["Result"],"Success","NPC {npc}, step {step}: {r}");
            assert_eq!(r["FriendlyInfo"]["Step"],step+1);
            if (npc,step)==(835,6) || (npc,step)==(835,7) || (npc,step)==(838,7) {
                let hero=if npc==838 {83} else if step==6 {85} else {84};
                assert_eq!(r["HeroInfos"][0]["HeroIndex"],hero);
                assert_eq!(r["HeroInfos"][0]["Level"],50);
                assert_eq!(r["HeroInfos"][0]["Star"],5);
                assert_eq!(r["HeroInfos"][0]["Transcended"],5);
            } else {assert_eq!(r["HeroInfos"],json!([]));}
            assert_ne!(call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step={step}")).await["Result"],"Success");
        }
        assert_ne!(call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step={max}")).await["Result"],"Success");
        assert_eq!(call(&s,&u,"give_gift_item",&format!("HeroIndex={npc}&ItemIndices={gift}&ItemCount=50")).await["Result"],"Success");
        let r=call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step={max}")).await;
        assert_eq!(r["Result"],"Success","{r}");
        assert_eq!(r["FriendlyInfo"]["EndRewardCount"],1);
        let stock:i64=sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=?")
            .bind(a).bind(gift).fetch_one(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"give_gift_item",&format!("HeroIndex={npc}&ItemIndices={gift}&ItemCount=50")).await["Result"],"Success");
        assert_ne!(call(&s,&u,"recv_reward_friendly_point",&format!("HeroIndex={npc}&Step={max}")).await["Result"],"Success");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=?")
            .bind(a).bind(gift).fetch_one(&s.db).await.unwrap(),stock);
    }
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM heroes WHERE account_id=?")
        .bind(a).fetch_one(&s.db).await.unwrap(),initial_heroes+3);
    let login=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=episode-test")).await.unwrap().0;
    for e in EPISODES {
        let p=login.npc_friendly_infos.iter().find(|v|n(v,"HeroIndex")==e.npc).unwrap();
        assert_eq!(p["Step"],8);assert_eq!(p["EndRewardCount"],1);
    }
    assert_eq!(login.drop_bonus_events.len(),19);
    assert_eq!(login.drop_bonus_events.iter().filter(|e|e["BonusType"]=="EventDungeon").count(),7);
    assert_eq!(login.drop_bonus_events.iter().filter(|e|e["BonusType"]=="EventShop").count(),6);
    for e in login.drop_bonus_events.iter().filter(|e|e["BonusType"]=="EventDungeon" || e["BonusType"]=="EventShop") {
        assert_eq!(e["DungeonType"],json!(["EventDungeon"]));
        assert_eq!(e["MoveTaskType"],"None");
        assert_eq!(e["BuffPosition"],"None");
    }
}

#[tokio::test]
async fn episodes_wrong_gifts_and_malformed_native_arrays_are_atomic() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    give(&s,a,47023,2000).await;give(&s,a,47031,2000).await;
    for args in ["HeroIndex=835&ItemIndices=47031&ItemCount=1000",
        "HeroIndex=835&ItemIndices=47023&ItemIndices=47031&ItemCount=10&ItemCount=10",
        "HeroIndex=835&ItemIndices=47023&ItemCount=-1",
        "HeroIndex=835&ItemIndices=47023&ItemCount=3000",
        "HeroIndex=835&ItemIndices=47023&ItemCount=1&ItemCount=2"] {
        assert_ne!(call(&s,&u,"give_gift_item",args).await["Result"],"Success");
        for item in [47023,47031] {
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=?")
                .bind(a).bind(item).fetch_one(&s.db).await.unwrap(),2000);
        }
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM extension_state WHERE account_id=? AND kind='npc'")
            .bind(a).fetch_one(&s.db).await.unwrap(),0);
    }
}

#[tokio::test]
async fn episodes_battle_gates_use_real_clears_and_can_be_disabled() {
    let (mut s,u)=setup().await;let a=u.user_info.account_id;
    for e in EPISODES {
        let mut db=s.db.acquire().await.unwrap();
        require_battle(&mut db,&s,a,e.chapter,1,0).await.unwrap();
        assert!(require_battle(&mut db,&s,a,e.chapter,2,0).await.is_err());
        for (c,d) in [e.gate,(e.chapter,1)] {
            sqlx::query("INSERT OR REPLACE INTO battle_state(account,kind,idx,data) VALUES(?,'dungeon',?,?)")
                .bind(a).bind((c<<32)|d).bind(json!({"MaxStar":3,"CompletedTime":"2026-01-01 00:00:00","FirstRewardedDiff":1}).to_string())
                .execute(&mut *db).await.unwrap();
        }
        require_battle(&mut db,&s,a,e.chapter,2,0).await.unwrap();
        assert!(require_battle(&mut db,&s,a,e.chapter,3,0).await.is_err());
        assert!(require_battle(&mut db,&s,a,e.chapter,1,1).await.is_err());
        sqlx::query("UPDATE battle_state SET data=json_set(data,'$.MaxStar',0,'$.CompletedTime',NULL) WHERE account=? AND kind='dungeon' AND idx=?")
            .bind(a).bind((e.gate.0<<32)|e.gate.1).execute(&mut *db).await.unwrap();
        assert!(require_battle(&mut db,&s,a,e.chapter,2,0).await.is_err());
    }
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).rules["EnablePieceOfMemoryEpisodes"]=json!(false);
    assert!(events(&s).is_empty());assert!(require_npc(&s,835).is_err());
    assert!(require_battle(&mut s.db.acquire().await.unwrap(),&s,a,30036,1,0).await.is_err());
}

async fn battle(s: &AppState,u: &user::LoginResponse,action: &str,args: &str) -> Value {
    crate::api::battle::execute_request(s,&format!("campaign/{action}"),
        Bytes::from(format!("SessionKey={}&{args}",u.user_info.session_key))).await.unwrap()
}

async fn clear(s: &AppState,a: i64,c: i64,d: i64) {
    sqlx::query("INSERT OR REPLACE INTO battle_state(account,kind,idx,data) VALUES(?,'dungeon',?,?)")
        .bind(a).bind((c<<32)|d).bind(json!({"ChapterIndex":c,"DungeonIndex":d,"MaxStar":3,
            "FirstRewardedDiff":1,"CompletedTime":"2026-01-01 00:00:00"}).to_string())
        .execute(&s.db).await.unwrap();
}

#[tokio::test]
async fn episodes_battles_charge_stamina_award_tokens_once_and_allow_farming() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let entry="ChapterIndex=30036&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[84,85]";
    let end="ChapterIndex=30036&DungeonIndex=1&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=[84,85]";
    let before:i64=sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(battle(&s,&u,"begin_campaign",entry).await["Result"],"Success");
    sqlx::query("UPDATE battle_runs SET started=? WHERE account=?").bind(chrono::Utc::now().timestamp()-60).bind(a).execute(&s.db).await.unwrap();
    let r=battle(&s,&u,"end_campaign",end).await;
    assert_eq!(r["Result"],"Success","{r}");
    assert_eq!(r["ItemResults"][0]["ItemIndex"],47023);assert_eq!(r["ItemResults"][0]["AddCount"],50);
    assert_eq!(battle(&s,&u,"end_campaign",end).await,r);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=47023").bind(a).fetch_one(&s.db).await.unwrap(),50);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before-100);
    let locked=battle(&s,&u,"begin_campaign","ChapterIndex=30036&DungeonIndex=2&DungeonDifficulty=0&HeroIndices=[84,85]").await;
    assert_ne!(locked["Result"],"Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before-100);
    clear(&s,a,97,2).await;
    clear(&s,a,30036,11).await;
    for win in [false,true,true] {
        let entry="ChapterIndex=30036&DungeonIndex=12&DungeonDifficulty=0&HeroIndices=[1]";
        assert_eq!(battle(&s,&u,"begin_campaign",entry).await["Result"],"Success");
        sqlx::query("UPDATE battle_runs SET started=? WHERE account=?").bind(chrono::Utc::now().timestamp()-60).bind(a).execute(&s.db).await.unwrap();
        let r=battle(&s,&u,"end_campaign",&format!("ChapterIndex=30036&DungeonIndex=12&DungeonDifficulty=0&Completed={win}&Star={}&AliveHeroIndices=[1]",if win {3}else{0})).await;
        assert_eq!(r["Result"],"Success","{r}");
        if win {assert_eq!(r["ItemResults"][0]["AddCount"],1000);}
        else {assert!(r["ItemResults"].as_array().is_none_or(|v|v.is_empty()));}
    }
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT count FROM items WHERE account_id=? AND item_index=47023").bind(a).fetch_one(&s.db).await.unwrap(),2050);
}

#[tokio::test]
async fn episodes_concurrent_milestone_claims_grant_one_reward() {
    let (s,u)=setup().await;let a=u.user_info.account_id;give(&s,a,47023,1000).await;
    assert_eq!(call(&s,&u,"give_gift_item","HeroIndex=835&ItemIndices=47023&ItemCount=1000").await["Result"],"Success");
    let (first,second)=tokio::join!(
        call(&s,&u,"recv_reward_friendly_point","HeroIndex=835&Step=1"),
        call(&s,&u,"recv_reward_friendly_point","HeroIndex=835&Step=1"));
    assert_eq!([first,second].iter().filter(|r|r["Result"]=="Success").count(),1);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT json_extract(data,'$.Step') FROM extension_state WHERE account_id=? AND kind='npc' AND idx=835")
        .bind(a).fetch_one(&s.db).await.unwrap(),2);
}
