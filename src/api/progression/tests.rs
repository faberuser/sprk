use super::*;
use crate::api::account::user;
use crate::database;
use std::sync::Arc;

#[tokio::test]
async fn notification_middleware_skips_reads_and_cosmetics_and_negotiates_unchanged_state() {
    use tower::ServiceExt;
    let (s,u)=setup().await;
    let mut app=axum::Router::new();
    for path in ["/friend/list","/hero/bookmark_hero","/hero/future_mutation"] {
        app=app.route(path,axum::routing::post(||async{axum::Json(json!({"BaseResult":"Success","Result":"Success"}))}));
    }
    let app=app.layer(axum::middleware::from_fn_with_state(s.clone(),notifications::notify)).with_state(s.clone());
    async fn send(app:axum::Router,u:&user::LoginResponse,path:&str,token:&str)->Value {
        let response=app.oneshot(axum::http::Request::post(path).header("x-sprk-progression",token)
            .body(axum::body::Body::from(form(u,""))).unwrap()).await.unwrap();
        serde_json::from_slice(&axum::body::to_bytes(response.into_body(),usize::MAX).await.unwrap()).unwrap()
    }
    for path in ["/friend/list","/hero/bookmark_hero"] {
        let reply=send(app.clone(),&u,path,"").await;
        assert!(reply.get("AchievementInfos").is_none());assert!(reply.get("SprkProgressionToken").is_none());
    }
    let first=send(app.clone(),&u,"/hero/future_mutation","").await;
    assert!(first["AchievementInfos"].is_array());
    let token=first["SprkProgressionToken"].as_str().unwrap();
    let next=send(app.clone(),&u,"/hero/future_mutation",token).await;
    assert!(next.get("AchievementInfos").is_none());assert_eq!(next["SprkProgressionToken"],token);
    let resent=send(app,&u,"/hero/future_mutation","").await;
    assert_eq!(resent["AchievementInfos"],first["AchievementInfos"]);
}

fn play_value(infos: &[Value], id: i64) -> &str {
    infos.iter().find(|r|r["RecordIndex"]==id).unwrap()["RecordValue"].as_str().unwrap()
}

#[tokio::test]
async fn play_records_recover_legacy_inventory_and_classify_battle_clears() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    let mut rewards=Rewards::default();
    let mut tx=s.db.begin().await.unwrap();
    item::give(&mut tx,&s,a,ItemGrant{index:1001,count:1,star:3,custom:0},&mut rewards).await.unwrap();
    // Simulate equipment saved before lifetime acquisition tracking existed.
    sqlx::query("DELETE FROM play_record_values WHERE account_id=?").bind(a).execute(&mut *tx).await.unwrap();
    sqlx::query("DELETE FROM progression_metrics WHERE account_id=? AND kind='AddEquip'").bind(a).execute(&mut *tx).await.unwrap();
    let red=s.tables.battle.rows("Raid").iter().find(|r|n(r,"Index")==1).unwrap();
    record(&mut tx,a,"ClearDungeon",n(red,"ChapterIndex"),n(red,"DungeonIndex"),3).await.unwrap();
    let floor=s.tables.battle.rows("TowerFloor").iter().find(|r|n(r,"TowerIndex")==21 && n(r,"Floor")==10).unwrap();
    record(&mut tx,a,"ClearDungeon",n(floor,"ChapterIndex"),n(floor,"DungeonIndex"),2).await.unwrap();
    crate::api::battle::put(&mut tx,a,"eclipse_record",0,&json!({"BestWave":42})).await.unwrap();
    tx.commit().await.unwrap();
    play_record::migrate(&s.db).await.unwrap();
    play_record::migrate(&s.db).await.unwrap();
    let login=relogin(&s).await;
    for (id,value) in [(201,"1"),(203,"3"),(606,"3"),(619,"2"),(620,"10"),(626,"42")] {assert_eq!(play_value(&login.play_record_infos,id),value,"record {id}");}
}

#[tokio::test]
async fn play_records_track_inventory_history_transactionally_and_isolate_accounts() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let created:String=sqlx::query_scalar("SELECT created_at FROM accounts WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(play_value(&u.play_record_infos,1),created);
    assert_eq!(play_value(&u.play_record_infos,2),"1");
    assert_eq!(play_value(&u.play_record_infos,3),"1");
    assert_eq!(play_value(&u.play_record_infos,101),u.heroes.len().to_string());
    let initial:i64=play_value(&u.play_record_infos,201).parse().unwrap();
    let mut rewards=Rewards::default();
    let mut tx=s.db.begin().await.unwrap();
    item::give(&mut tx,&s,a,ItemGrant{index:1001,count:1,star:5,custom:0},&mut rewards).await.unwrap();
    let slot=rewards.equipment[0].slot_index;
    crate::api::extensions::put(&mut tx,a,"soul",slot as i64,&json!({"EquipItemSlotIndex":slot,"ItemIndex":1001,"Grade":2,"ReinforceLevel":20})).await.unwrap();
    play_record::liberated(&mut tx,a).await.unwrap();
    tx.commit().await.unwrap();
    let with_item=relogin(&s).await;
    assert_eq!(play_value(&with_item.play_record_infos,201),(initial+1).to_string());
    assert_eq!(play_value(&with_item.play_record_infos,203),"5");
    assert_eq!(play_value(&with_item.play_record_infos,301),"1");
    assert_eq!(play_value(&with_item.play_record_infos,302),"2");
    assert_eq!(play_value(&with_item.play_record_infos,303),"20");
    sqlx::query("DELETE FROM equip_items WHERE slot_index=?").bind(slot).execute(&s.db).await.unwrap();
    sqlx::query("DELETE FROM extension_state WHERE account_id=? AND kind='soul'").bind(a).execute(&s.db).await.unwrap();
    let mut tx=s.db.begin().await.unwrap();
    item::give(&mut tx,&s,a,ItemGrant{index:1001,count:1,star:0,custom:0},&mut Rewards::default()).await.unwrap();
    tx.rollback().await.unwrap();
    play_record::migrate(&s.db).await.unwrap();
    play_record::migrate(&s.db).await.unwrap();
    let after=relogin(&s).await;
    for info in &with_item.play_record_infos {let id=n(info,"RecordIndex");assert_eq!(play_value(&after.play_record_infos,id),info["RecordValue"].as_str().unwrap());}
    let other=user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=record-other")).await.unwrap().0;
    assert_eq!(play_value(&other.play_record_infos,203),"0");
    assert_eq!(play_value(&other.play_record_infos,301),"0");
}

#[tokio::test]
async fn play_records_login_streaks_use_calendar_days_and_login_is_idempotent() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    sqlx::query("DELETE FROM progression_metrics WHERE account_id=? AND kind='LoginDaily'").bind(a).execute(&s.db).await.unwrap();
    for offset in [0,1,2,5,6,7,8] {
        let date=(chrono::Utc::now().date_naive()-chrono::Duration::days(offset)).to_string();
        sqlx::query("INSERT INTO progression_metrics(account_id,day,kind,value) VALUES(?,?,'LoginDaily',1)").bind(a).bind(date).execute(&s.db).await.unwrap();
    }
    let login=relogin(&s).await;
    assert_eq!(play_value(&login.play_record_infos,3),"3");
    assert_eq!(play_value(&login.play_record_infos,4),"4");
    assert_eq!(play_value(&relogin(&s).await.play_record_infos,2),"1");
    sqlx::query("DELETE FROM progression_metrics WHERE account_id=? AND kind='LoginDaily' AND day=date('now','-1 day')").bind(a).execute(&s.db).await.unwrap();
    let login=relogin(&s).await;
    assert_eq!(play_value(&login.play_record_infos,3),"1");
    assert_eq!(play_value(&login.play_record_infos,4),"4");
}

#[tokio::test]
async fn play_records_arena_bests_and_totals_survive_losses_and_season_removal() {
    let (s,u)=setup().await;let a=u.user_info.account_id;
    sqlx::query("INSERT INTO arena_scores(account,kind,season,score) VALUES(?,0,987654,1000)").bind(a).execute(&s.db).await.unwrap();
    for (score,wins,losses,streak) in [(1020,1,0,1),(1040,2,0,2),(1030,2,1,0)] {
        sqlx::query("UPDATE arena_scores SET score=?,wins=?,losses=?,data=? WHERE account=? AND kind=0 AND season=987654").bind(score).bind(wins).bind(losses).bind(json!({"SuccessiveWin":streak}).to_string()).bind(a).execute(&s.db).await.unwrap();
    }
    sqlx::query("DELETE FROM arena_scores WHERE account=? AND season=987654").bind(a).execute(&s.db).await.unwrap();
    play_record::migrate(&s.db).await.unwrap();
    let login=relogin(&s).await;
    for (id,value) in [(401,"1040"),(403,"2"),(404,"2"),(405,"1")] {assert_eq!(play_value(&login.play_record_infos,id),value);}
}

#[tokio::test]
async fn play_records_campaign_response_updates_native_cache_and_rejects_duplicate_clear() {
    use tower::ServiceExt;
    let (s,u)=setup().await;
    let app=axum::Router::new()
        .route("/campaign/begin",axum::routing::post(crate::api::battle::campaign_handlers::begin_campaign))
        .route("/campaign/end",axum::routing::post(crate::api::battle::campaign_handlers::end_campaign))
        .layer(axum::middleware::from_fn_with_state(s.clone(),notifications::notify)).with_state(s.clone());
    let entry="ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    for (path,args) in [("begin",entry.to_string()),("end",format!("{entry}&Completed=true&PureBattleTime=30"))] {
        let response=app.clone().oneshot(axum::http::Request::builder().method("POST").uri(format!("/campaign/{path}"))
            .header("Content-Type","application/x-www-form-urlencoded").body(axum::body::Body::from(form(&u,&args))).unwrap()).await.unwrap();
        let out:Value=serde_json::from_slice(&axum::body::to_bytes(response.into_body(),usize::MAX).await.unwrap()).unwrap();
        assert_eq!(out["Result"],"Success","{out}");
        if path=="end" {assert_eq!(play_value(out["ReservedPlayRecordInfos"].as_array().unwrap(),6),"1");}
    }
    let login=relogin(&s).await;
    let ids:BTreeSet<i64>=login.play_record_infos.iter().map(|r|n(r,"RecordIndex")).collect();
    assert_eq!(ids.len(),42,"Every native row has a wire value after a story clear");
    let invalid=crate::api::battle::campaign_handlers::end_campaign(State(s.clone()),form(&login,&format!("{entry}&Completed=true"))).await.unwrap().0;
    assert_ne!(invalid["Result"],"Success");
}
pub(super) async fn setup() -> (AppState, user::LoginResponse) {
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let s = AppState::new(pool, crate::tables::test_tables());
    let u = relogin(&s).await;
    (s, u)
}

#[tokio::test]
async fn present_level_missions_recover_claimed_rewards_and_reject_unclaimed_progress() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let other = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=present-other"))
        .await.unwrap().0.user_info.account_id;
    let quest = s.tables.progression.sub_quests.iter()
        .find(|q| n(q, "QuestIndex") == 1002320).unwrap();
    assert_eq!(quest["StateValue"], json!(["6", "835"]));
    let mut db = s.db.acquire().await.unwrap();
    // Neither a different account's reward nor a different NPC's reward counts.
    for (account, npc) in [(other, 835), (a, 827)] {
        crate::api::extensions::put(&mut db, account, "npc", npc,
            &json!({"HeroIndex":npc,"Step":8,"FriendlyPoint":100000,"EndRewardCount":1})).await.unwrap();
    }
    assert_eq!(View::load(&mut db, a).await.unwrap().quest(&s, quest), 0);
    // Enough gifts for level six still require the actual reward claim.
    crate::api::extensions::put(&mut db, a, "npc", 835,
        &json!({"HeroIndex":835,"Step":6,"FriendlyPoint":100000,"EndRewardCount":0})).await.unwrap();
    assert_eq!(View::load(&mut db, a).await.unwrap().quest(&s, quest), 0);
    // Existing saves after that claim point at the next reward.
    crate::api::extensions::put(&mut db, a, "npc", 835,
        &json!({"HeroIndex":835,"Step":7,"FriendlyPoint":100000,"EndRewardCount":0})).await.unwrap();
    assert_eq!(View::load(&mut db, a).await.unwrap().quest(&s, quest), 1);
    // The final reward never advances Step; its claim counter proves completion.
    let final_quest = json!({"Kind":"HeroDungeonPresentLevel","StateValue":["8","835"]});
    for (count, expected) in [(0, 0), (1, 1)] {
        crate::api::extensions::put(&mut db, a, "npc", 835,
            &json!({"HeroIndex":835,"Step":8,"FriendlyPoint":0,"EndRewardCount":count})).await.unwrap();
        assert_eq!(View::load(&mut db, a).await.unwrap().quest(&s, &final_quest), expected);
    }
    drop(db);
    let login = relogin(&s).await;
    assert!(login.sub_quest_infos.iter().any(|q| n(q,"SubQuestIndex")==1002320 && n(q,"Progress")==1 && n(q,"LastStep")==0));
    let reward = call(&s, &login, "complete_sub_quest", "SubQuestIndices=[1002320]").await;
    assert_eq!(reward["Result"], "Success", "{reward}");
    let after = currency(&s, &login).await;
    // A second claim cannot grant the reward again.
    assert_ne!(call(&s, &login, "complete_sub_quest", "SubQuestIndices=[1002320]").await["Result"], "Success");
    assert_eq!(currency(&s, &login).await, after);
    assert!(relogin(&s).await.sub_quest_infos.iter().any(|q| n(q,"SubQuestIndex")==1002320 && n(q,"LastStep")==1));
}
async fn relogin(s: &AppState) -> user::LoginResponse {
    user::test_login(
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
async fn sanctum_missions_recover_existing_souls_and_enforce_level_and_grade_filters() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let other = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=sanctum-other"))
        .await.unwrap().0.user_info.account_id;
    let mut rewards = Rewards::default();
    let slot;
    {
        let mut db = s.db.acquire().await.unwrap();
        item::give(&mut db, &s, a, ItemGrant { index: 1001, count: 1, star: 0, custom: 0 }, &mut rewards).await.unwrap();
        slot = rewards.equipment[0].slot_index as i64;
        // Existing persisted state needs no new liberation or enhancement event.
        crate::api::extensions::put(&mut db, a, "soul", slot,
            &json!({"EquipItemSlotIndex":slot,"ItemIndex":1001,"ReinforceLevel":20,"Grade":2})).await.unwrap();
        crate::api::extensions::put(&mut db, a, "soul", 999999,
            &json!({"ReinforceLevel":20,"Grade":2})).await.unwrap();
        let view = View::load(&mut db, a).await.unwrap();
        for (values, expected) in [(json!(null),1), (json!(["20","2"]),1), (json!(["21"]),0), (json!(["0","3"]),0)] {
            assert_eq!(view.quest(&s, &json!({"Kind":"SoulWeaponCount","StateValue":values})), expected);
        }
        assert_eq!(View::load(&mut db, other).await.unwrap()
            .quest(&s, &json!({"Kind":"SoulWeaponCount"})), 0);
        sqlx::query("DELETE FROM extension_state WHERE account_id=? AND kind='soul' AND idx=999999")
            .bind(a).execute(&mut *db).await.unwrap();
    }
    let login = relogin(&s).await;
    for quest in [10892, 10952] {
        assert!(login.sub_quest_infos.iter().any(|q| q["SubQuestIndex"] == quest && q["Progress"] == 1));
        let args = format!("SubQuestIndices=[{quest}]");
        assert_eq!(call(&s, &login, "complete_sub_quest", &args).await["Result"], "Success");
        assert_ne!(call(&s, &login, "complete_sub_quest", &args).await["Result"], "Success");
    }
    sqlx::query("DELETE FROM equip_items WHERE account_id=? AND slot_index=?")
        .bind(a).bind(slot).execute(&s.db).await.unwrap();
    let mut db = s.db.acquire().await.unwrap();
    assert_eq!(View::load(&mut db,a).await.unwrap().quest(&s,&json!({"Kind":"SoulWeaponCount"})),0);
}

#[tokio::test]
async fn world_boss_normal_achievement_claim_uses_completed_battle_access_proof() {
    let (mut s,u)=setup().await;
    let a=u.user_info.account_id;
    let claim="AchievementIndices=[20]&Steps=[0]";
    // A menu predicate alone must never grant participation or a reward.
    assert_ne!(call(&s,&u,"reward_achievement",claim).await["Result"],"Success");
    sqlx::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES(?,date('now'),'CompleteWorldBoss',4,0,1)")
        .bind(a).execute(&s.db).await.unwrap();
    let infos=call(&s,&u,"get_achievements","").await;
    assert_eq!(infos["AchievementInfos"].as_array().unwrap().iter()
        .find(|r|r["AchievementIndex"]==20).unwrap()["Achievement"],1);
    // Only the exact World Boss menu condition can use this proof.
    let row=Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression)
        .achievements.iter_mut().find(|r|r["Index"]==20 && r["Step"]==1).unwrap();
    let original=row["OpenCondition"].clone();
    row["OpenCondition"]=json!(["TeamLevel","999"]);
    assert_eq!(call(&s,&u,"reward_achievement",claim).await["Result"],"RewardTimeEnded");
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression)
        .achievements.iter_mut().find(|r|r["Index"]==20 && r["Step"]==1).unwrap()["OpenCondition"]=original;
    // A retired event in Claim All still rolls the whole transaction back.
    assert_eq!(call(&s,&u,"reward_achievement","AchievementIndices=[20,5000010]&Steps=[0,0]").await["Result"],"RewardTimeEnded");
    let points=|s:AppState| async move {
        sqlx::query_scalar::<_,i64>("SELECT COALESCE((SELECT value FROM battle_currencies WHERE account=? AND kind='WorldBossPoint'),0)")
            .bind(a).fetch_one(&s.db).await.unwrap()
    };
    assert_eq!(points(s.clone()).await,0);
    let result=call(&s,&u,"reward_achievement",claim).await;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(points(s.clone()).await,1000);
    assert_eq!(call(&s,&u,"reward_achievement",claim).await["Result"],"InvalidStep");
    assert_eq!(call(&s,&u,"reward_achievement","AchievementIndices=[20]&Steps=[1]").await["Result"],"AchievementNotCleared");
    assert_eq!(points(s.clone()).await,1000);
    let relogged=relogin(&s).await;
    let result=call(&s,&relogged,"get_achievements","").await;
    assert_eq!(result["AchievementInfos"].as_array().unwrap().iter()
        .find(|r|r["AchievementIndex"]==20).unwrap()["LastStep"],1);
}

#[tokio::test]
async fn attendance_claim_once_per_day_and_persist() {
    let (s, u) = setup().await;
    assert_eq!(u.attendance_datas[0]["AttendanceRewardableDays"], 1);
    assert_eq!(u.attendance_infos[0]["CompletedDay"], 0);
    let pending = call(&s, &u, "attendance_info", "").await;
    assert_eq!(pending["AttendanceDatas"][0]["AttendanceRewardableDays"], 1);
    assert_eq!(pending["AttendanceInfos"][0]["CompletedDay"], 0);
    let r = &s.tables.progression.calendars[0]["Reward"][0]["Reward"][0];
    let id = n(r, "Value");
    let before = count(&s, &u, id).await;
    let a = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(count(&s, &u, id).await, before + n(r, "Count"));
    assert_eq!(a["AttendanceInfos"][0]["LastRewardedDay"], 1);
    assert_eq!(a["AttendanceInfos"][0]["CompletedDay"], 1);
    assert_eq!(a["AttendanceDatas"][0]["AttendanceRewardableDays"], 0);
    assert_ne!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Success"
    );
    let login = relogin(&s).await;
    assert_eq!(login.attendance_infos[0]["LastRewardedDay"], 1);
    assert_eq!(login.attendance_infos[0]["CompletedDay"], 1);
    assert_eq!(login.attendance_datas[0]["AttendanceRewardableDays"], 0);
    assert_eq!(login.misc_info.unwrap().login_daily_count, 1);
    sqlx::query("UPDATE attendance_calendar_state SET last_day='2000-01-01' WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let login = relogin(&s).await;
    assert_eq!(login.attendance_infos[0]["CompletedDay"], 1);
    assert_eq!(login.attendance_datas[0]["AttendanceRewardableDays"], 2);
    let pending = call(&s, &u, "attendance_info", "").await;
    assert_eq!(pending["AttendanceInfos"][0]["CompletedDay"], 1);
    assert_eq!(pending["AttendanceDatas"][0]["AttendanceRewardableDays"], 2);
    let a = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(a["Result"], "Success");
    assert_eq!(a["AttendanceInfos"][0]["LastRewardedDay"], 2);
    assert_eq!(a["AttendanceInfos"][0]["CompletedDay"], 2);
    assert_eq!(a["AttendanceDatas"][0]["AttendanceRewardableDays"], 0);
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
async fn lobby_preserves_accumulated_login_days_and_claims() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE progression_login SET days=14,last_day=date('now') WHERE account_id=?")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let login = relogin(&s).await;
    assert_eq!(serde_json::to_value(&login).unwrap()["MiscInfo"]["LoginDailyCount"], 14);
    for index in [11, 12] {
        assert_eq!(call(&s, &login, "get_logindaily_reward", &format!("LoginDailyIndex={index}")).await["Result"], "Success");
    }
    for _ in 0..2 {
        let lobby = crate::api::account::lobby::enter_lobby(State(s.clone()), axum::extract::Form(crate::api::account::lobby::EnterLobbyRequest {
            session_id: None, session_key: Some(login.user_info.session_key.clone()),
        })).await.unwrap().0;
        assert_eq!(serde_json::to_value(&lobby).unwrap()["LoginDailyCount"], 14);
    }
    let login = relogin(&s).await;
    assert_eq!(login.login_daily_infos.len(), 2);
    assert_eq!(login.misc_info.unwrap().login_daily_count, 14);
    assert_eq!(call(&s, &u, "get_logindaily_reward", "LoginDailyIndex=12").await["Result"], "AlreadyReceived");
}

#[tokio::test]
async fn attendance_continues_past_seven_days_without_resetting_claims() {
    let (s, u) = setup().await;
    assert_eq!(u.attendance_datas[0]["PageSize"], 28);
    assert_eq!(u.attendance_datas[0]["RewardEndDay"], 28);
    assert_eq!(u.attendance_datas[0]["Reward"].as_array().unwrap().len(), 28);
    sqlx::query("INSERT INTO attendance_calendar_state(account_id,idx,claims,last_day) VALUES(?,1,7,date('now','-1 day'))")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let response = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(response["Result"], "Success");
    assert_eq!(response["AttendanceInfos"][0]["LastRewardedDay"], 8);
    assert_eq!(relogin(&s).await.attendance_infos[0]["LastRewardedDay"], 8);
}
#[tokio::test]
async fn automatic_dungeon_milestones_unlock_categories_without_claiming_rewards() {
    let (s, u) = setup().await;
    let opened = |wire: &Value, main: i64, sub: i64| {
        wire["OpendMissionCategories"].as_array().unwrap()
            .contains(&json!({"MainCategoryIndex":main,"SubCategoryIndex":sub}))
    };
    let before = login(&s, u.user_info.account_id).await.unwrap();
    assert!(!opened(&before, 5, 1));
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star,is_unlocked) VALUES(?,6,1,0,0,1)")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    assert!(!opened(&login(&s, u.user_info.account_id).await.unwrap(), 5, 1));
    sqlx::query("UPDATE campaign_progress SET clear_count=1,best_star=3 WHERE account_id=? AND chapter_id=6 AND dungeon_id=1")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let balance = currency(&s, &u).await;
    let refreshed = login(&s, u.user_info.account_id).await.unwrap();
    assert!(opened(&refreshed, 5, 1));
    assert!(opened(&refreshed, 5, 3));
    assert!(!opened(&refreshed, 5, 2)); // 8-26 still required.
    assert!(!opened(&refreshed, 5, 4)); // 10-9 still required.
    assert_eq!(currency(&s, &u).await, balance);
    let claims: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM progression_claims WHERE account_id=? AND family='subquest' AND idx=10740")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap();
    assert_eq!(claims, 0);
}

#[tokio::test]
async fn native_claim_all_and_lobby_categories() {
    let (s, u) = setup().await;
    for (kind, value) in [("ClearDungeon", 5), ("BuyShopItem", 1)] {
        record(
            &mut s.db.acquire().await.unwrap(),
            u.user_info.account_id,
            kind,
            0,
            0,
            value,
        )
        .await
        .unwrap();
    }
    let batch = "AchievementIndices=1101&Steps=0&AchievementIndices=1102&Steps=0&AchievementIndices=1104&Steps=0&AchievementIndices=1111&Steps=0&AchievementIndices=1112&Steps=0";
    let a = call(&s, &u, "reward_achievement", batch).await;
    assert_eq!(a["Result"], "Success");
    for id in [1101, 1102, 1104, 1111, 1112] {
        assert_eq!(a["AchievementInfos"].as_array().unwrap().iter().find(|r| r["AchievementIndex"] == id).unwrap()["LastStep"], 1);
    }
    let balance = currency(&s, &u).await;
    assert_eq!(call(&s, &u, "reward_achievement", batch).await["Result"], "InvalidStep");
    assert_eq!(currency(&s, &u).await, balance);
    let lobby = crate::api::account::lobby::enter_lobby(State(s.clone()), axum::extract::Form(crate::api::account::lobby::EnterLobbyRequest {
        session_id: None, session_key: Some(u.user_info.session_key.clone()),
    })).await.unwrap().0;
    let wire = serde_json::to_value(lobby).unwrap();
    let categories = wire["OpendMissionCategories"].as_array().unwrap();
    assert!(categories.contains(&json!({"MainCategoryIndex":2,"SubCategoryIndex":1})));
    assert!(categories.contains(&json!({"MainCategoryIndex":2,"SubCategoryIndex":2})));
    assert!(!categories.contains(&json!({"MainCategoryIndex":4,"SubCategoryIndex":1})));
    sqlx::query("INSERT INTO progression_claims(account_id,family,idx,step,period) VALUES(?,'subquest',10110,1,'all')")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    let refreshed = login(&s, u.user_info.account_id).await.unwrap();
    let categories = refreshed["OpendMissionCategories"].as_array().unwrap();
    assert!(categories.contains(&json!({"MainCategoryIndex":4,"SubCategoryIndex":1})));
    assert!(!categories.contains(&json!({"MainCategoryIndex":1,"SubCategoryIndex":2})));
}

#[tokio::test]
async fn daily_achievement_native_rewards_and_reset() {
    let (s, u) = setup().await;
    let before = currency(&s, &u).await;
    let a = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[1101]&Steps=[0]",
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
            "AchievementIndices=[1101]&Steps=[0]"
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
            "AchievementIndices=[1101]&Steps=[0]"
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
            "AchievementIndices=[1101,99999999]&Steps=[0,0]"
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
            "AchievementIndices=[1705]&Steps=[0]"
        )
        .await["Result"],
        "AchievementNotCleared"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1101]&Steps=[0]"
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
    sqlx::query("UPDATE user_info SET gold=2000000").execute(&s.db).await.unwrap();
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
            "AchievementIndices=[1705]&Steps=[0]"
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
            "AchievementIndices=[1705]&Steps=[0]"
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn guideline_claim_all_preserves_native_batch_and_rolls_back_invalid_batch() {
    let (s, u) = setup().await;
    for dungeon in 1..=3 {
        sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,?,1,13) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=1,best_star=13")
            .bind(u.user_info.account_id).bind(dungeon).execute(&s.db).await.unwrap();
    }
    let before = currency(&s, &u).await;
    let invalid = call(&s, &u, "complete_sub_quest", "SubQuestIndices=10010&SubQuestIndices=99999999").await;
    assert_ne!(invalid["Result"], "Success");
    assert_eq!(currency(&s, &u).await, before);
    let batch = "SubQuestIndices=10010&SubQuestIndices=10020&SubQuestIndices=10030";
    let result = call(&s, &u, "complete_sub_quest", batch).await;
    assert_eq!(result["Result"], "Success");
    let completed: Vec<i64> = result["SucceededSubQuestInfos"].as_array().unwrap().iter()
        .map(|r| r["SubQuestIndex"].as_i64().unwrap()).collect();
    assert_eq!(completed, vec![10010, 10020, 10030]);
    assert_eq!(result["RewardResults"].as_array().unwrap().len(), 3);
    let chapter_progress = result["ReservedSubQuestInfos"].as_array().unwrap().iter()
        .find(|info| info["SubQuestIndex"] == 20001).unwrap();
    assert_eq!(chapter_progress["Progress"], 3);
    let after = currency(&s, &u).await;
    assert_ne!(call(&s, &u, "complete_sub_quest", batch).await["Result"], "Success");
    assert_eq!(currency(&s, &u).await, after);
    let restored = relogin(&s).await;
    assert_eq!(restored.sub_quest_infos.iter().find(|info| info["SubQuestIndex"] == 20001).unwrap()["Progress"], 3);
    for id in completed {
        assert_eq!(restored.sub_quest_infos.iter().find(|r| r["SubQuestIndex"] == id).unwrap()["LastStep"], 1);
    }
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
    // Assert the actual wire key: the native login parser uses lower camel case.
    let restored = serde_json::to_value(relogin(&s).await).unwrap();
    assert_eq!(restored["chapterRewardInfos"], json!([{"ChapterIndex":1,"LastRewardDiff":2}]));
    assert!(restored.get("ChapterRewardInfos").is_none());
    assert_eq!(
        call(&s, &u, "reward_clear_chapter", "ChapterIndex=1&Step=1").await["Result"],
        "WrongStep"
    );
    for id in 9..=20 {
        sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,?,1,13)")
            .bind(u.user_info.account_id).bind(id).execute(&s.db).await.unwrap();
    }
    for step in [2, 3] {
        assert_eq!(call(&s, &u, "reward_clear_chapter", &format!("ChapterIndex=1&Step={step}")).await["Result"], "Success");
    }
    let balances = currency(&s, &u).await;
    for _ in 0..2 {
        let restored = serde_json::to_value(relogin(&s).await).unwrap();
        assert_eq!(restored["chapterRewardInfos"], json!([{"ChapterIndex":1,"LastRewardDiff":14}]));
    }
    assert_eq!(currency(&s, &u).await, balances);
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
async fn hero_achievement_requires_actual_star_and_retired_event_stays_closed() {
    let (s, u) = setup().await;
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[1002]&Steps=[0]"
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
            "AchievementIndices=[1002]&Steps=[0]"
        )
        .await["Result"],
        "Success"
    );
    let r = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[5000010]&Steps=[0]",
    )
    .await;
    assert_eq!(r["Result"], "RewardTimeEnded");
}

#[tokio::test]
async fn daily_hot_time_claims_activate_boosters_once_per_step_and_reset_daily() {
    let (s, u) = setup().await;
    let account = u.user_info.account_id;
    for (id, item_id) in [(54, 110017), (55, 110018)] {
        let duration = n(&s.tables.inventory.boosters[&item_id], "Duration");
        assert!(duration > 0);
        let mut first_start = None;
        for step in 0..3 {
            let data = format!("AchievementIndices=[{id}]&Steps=[{step}]");
            let r = call(&s, &u, "reward_achievement", &data).await;
            assert_eq!(r["Result"], "Success", "{id}, step {step}: {r}");
            let booster = &r["ItemTimeDurationInfo"];
            assert_eq!(booster["ItemIndex"], item_id);
            let parse = |key: &str| {
                chrono::NaiveDateTime::parse_from_str(
                    booster[key].as_str().unwrap(),
                    "%Y-%m-%d %H:%M:%S",
                )
                .unwrap()
            };
            let start = *first_start.get_or_insert_with(|| parse("StartTime"));
            assert_eq!(parse("StartTime"), start);
            assert_eq!(
                (parse("EndTime") - start).num_seconds(),
                duration * (step + 1)
            );
            let infos = &r["AchievementInfos"];
            assert_eq!(
                infos
                    .as_array()
                    .unwrap()
                    .iter()
                    .find(|v| v["AchievementIndex"] == id)
                    .unwrap()["LastStep"],
                step + 1
            );
            let saved: (String, String) = sqlx::query_as(
                "SELECT start_time,end_time FROM item_boosters WHERE account_id=? AND item_index=?",
            )
            .bind(account)
            .bind(item_id)
            .fetch_one(&s.db)
            .await
            .unwrap();
            assert_eq!(
                call(&s, &u, "reward_achievement", &data).await["Result"],
                "InvalidStep"
            );
            let after: (String, String) = sqlx::query_as(
                "SELECT start_time,end_time FROM item_boosters WHERE account_id=? AND item_index=?",
            )
            .bind(account)
            .bind(item_id)
            .fetch_one(&s.db)
            .await
            .unwrap();
            assert_eq!(after, saved);
        }
        assert_ne!(
            call(
                &s,
                &u,
                "reward_achievement",
                &format!("AchievementIndices=[{id}]&Steps=[3]")
            )
            .await["Result"],
            "Success"
        );
    }
    let login = relogin(&s).await;
    let buffs = crate::api::inventory::item::booster_login(&s, account)
        .await
        .unwrap();
    assert!(buffs.iter().any(|b| b["ItemIndex"] == 110018));
    assert!(buffs.iter().any(|b| b["ItemIndex"] == 110017));
    let infos = call(&s, &login, "get_achievements", "").await;
    for id in [54, 55] {
        assert_eq!(
            infos["AchievementInfos"]
                .as_array()
                .unwrap()
                .iter()
                .find(|v| v["AchievementIndex"] == id)
                .unwrap()["LastStep"],
            3
        );
    }
    // Move only this fixture's claims to yesterday to exercise the next day's allowance.
    sqlx::query("UPDATE progression_claims SET period=date('now','-1 day') WHERE account_id=? AND family='achievement' AND idx IN (54,55)")
        .bind(account).execute(&s.db).await.unwrap();
    for id in [54, 55] {
        assert_eq!(
            call(
                &s,
                &login,
                "reward_achievement",
                &format!("AchievementIndices=[{id}]&Steps=[0]")
            )
            .await["Result"],
            "Success"
        );
    }
}

#[tokio::test]
async fn daily_hot_time_preserves_conditions_and_failed_bulk_claims_roll_back() {
    let (mut s, u) = setup().await;
    let r = call(
        &s,
        &u,
        "reward_achievement",
        "AchievementIndices=[54,5000010]&Steps=[0,0]",
    )
    .await;
    assert_eq!(r["Result"], "RewardTimeEnded");
    let account = u.user_info.account_id;
    let buffs = crate::api::inventory::item::booster_login(&s, account)
        .await
        .unwrap();
    assert!(!buffs.iter().any(|b| b["ItemIndex"] == 110017));
    let tables = Arc::make_mut(&mut s.tables);
    let progression = Arc::make_mut(&mut tables.progression);
    progression
        .achievements
        .iter_mut()
        .find(|r| r["Index"] == 54 && r["Step"] == 1)
        .unwrap()["OpenCondition"] = json!(["TeamLevel", "999"]);
    assert_eq!(
        call(
            &s,
            &u,
            "reward_achievement",
            "AchievementIndices=[54]&Steps=[0]"
        )
        .await["Result"],
        "RewardTimeEnded"
    );
    let claims: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM progression_claims WHERE account_id=? AND family='achievement' AND idx=54")
        .bind(account).fetch_one(&s.db).await.unwrap();
    assert_eq!(claims, 0);
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
async fn mission_bars_count_claimed_quests_in_their_own_category() {
    let (s, u) = setup().await;
    // Chapter 2 is the 21-quest Guideline bar. Also cover step-based missions.
    for (main, sub, reward, kind) in [(1, 2, 20002, "MissionClear"), (200, 1, 20037, "StepMissionClear")] {
        let header = s.tables.progression.sub_quests.iter()
            .find(|q| n(q, "QuestIndex") == reward && n(q, "Step") == 1).unwrap();
        assert_eq!(header["Kind"], kind);
        let mut members: Vec<_> = s.tables.progression.mission_visuals.iter()
            .filter(|v| n(v, "MainCategoryIndex") == main && n(v, "SubCategoryIndex") == sub && v["QuestType"] == "SubQuest")
            .collect();
        members.sort_by_key(|v| (n(v, "MainStep"), n(v, "SubStep")));
        let mut db = s.db.acquire().await.unwrap();
        let before = View::load(&mut db, u.user_info.account_id).await.unwrap();
        assert_eq!(before.quest(&s, header), 0);
        for (i, member) in members.iter().enumerate() {
            let id = n(member, "QuestIndex");
            let last = s.tables.progression.sub_quests.iter()
                .filter(|q| n(q, "QuestIndex") == id).map(|q| n(q, "Step")).max().unwrap();
            claim(&mut db, u.user_info.account_id, "subquest", id, last, "all").await.unwrap();
            let view = View::load(&mut db, u.user_info.account_id).await.unwrap();
            let expected = if kind == "MissionClear" { (i + 1) as i64 } else {
                let steps: BTreeSet<_> = members.iter().map(|v| n(v, "MainStep")).collect();
                steps.iter().filter(|step| members.iter().filter(|v| n(v, "MainStep") == **step)
                    .all(|v| members[..=i].iter().any(|done| n(done, "QuestIndex") == n(v, "QuestIndex"))))
                    .count() as i64
            };
            assert_eq!(view.quest(&s, header), expected, "{kind}, quest {id}");
            let chapter_one = s.tables.progression.sub_quests.iter().find(|q| n(q, "QuestIndex") == 20001).unwrap();
            assert_eq!(view.quest(&s, chapter_one), 0);
        }
        if reward == 20002 {
            assert_eq!(members.len(), 21);
            assert_eq!(View::load(&mut db, u.user_info.account_id).await.unwrap().quest(&s, header), 21);
        }
    }
    let restored = relogin(&s).await;
    assert_eq!(restored.sub_quest_infos.iter().find(|q| q["SubQuestIndex"] == 20002).unwrap()["Progress"], 21);
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
                .journal_mode(sqlx::sqlite::SqliteJournalMode::Wal)
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
    database::remove_test_database(&path).await;
}

#[tokio::test]
async fn mailbox_reads_skip_progression_writes_but_claims_update_missions() {
    use tower::Service;
    use crate::api::community::mail;
    async fn send(app: &mut axum::Router, u: &user::LoginResponse, path: &str, data: &str) -> Value {
        let response=app.call(axum::http::Request::builder().method("POST").uri(path)
            .header("Content-Type","application/x-www-form-urlencoded")
            .body(axum::body::Body::from(form(u,data))).unwrap()).await.unwrap();
        serde_json::from_slice(&axum::body::to_bytes(response.into_body(),usize::MAX).await.unwrap()).unwrap()
    }
    let (s,u)=setup().await;
    let mut app=axum::Router::new()
        .route("/mail/check_new_mail",axum::routing::post(mail::check_new_mail))
        .route("/mail/get_mail_list",axum::routing::post(mail::get_mail_list))
        .route("/mail/get_mail_list_by_page",axum::routing::post(mail::get_mail_list_by_page))
        .route("/mail/get_global_mail_list",axum::routing::post(mail::get_global_mail_list))
        .route("/mail/list",axum::routing::post(mail::get_mail_list))
        .route("/mail/receive_mail",axum::routing::post(mail::receive_mail))
        .layer(axum::middleware::from_fn_with_state(s.clone(),notifications::notify))
        .with_state(s.clone());
    let account=u.user_info.account_id;
    let id=mail::send_system_mail(&s,account,"Polling test","Reward",1,0,None).await.unwrap();
    sqlx::query("INSERT INTO global_mails(title,reward_gold) VALUES('Polling announcement',1)").execute(&s.db).await.unwrap();
    // Global mail must still materialize exactly once before reads become idle.
    let globals=send(&mut app,&u,"/mail/get_global_mail_list","").await;
    assert_eq!(globals["Result"],"Success");assert_eq!(globals["MailInfos"].as_array().unwrap().len(),1);
    for path in ["/mail/check_new_mail","/mail/get_mail_list","/mail/get_mail_list_by_page","/mail/get_global_mail_list","/mail/list"] {
        let before:i64=sqlx::query_scalar("SELECT total_changes()").fetch_one(&s.db).await.unwrap();
        let result=send(&mut app,&u,path,"MaxCount=10&PageNo=0").await;
        assert_eq!(result["Result"],"Success","{path}: {result}");
        assert!(result.get("AchievementInfos").is_none(),"Mailbox reads must not create progression snapshots");
        assert!(result.get("ReservedSubQuestInfos").is_none());
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT total_changes()").fetch_one(&s.db).await.unwrap(),before,
            "Idle mailbox read wrote to the database: {path}");
    }
    let mut db=s.db.acquire().await.unwrap();
    assert_eq!(View::load(&mut db,account).await.unwrap().metric("ReceiveMail","all",None,None),0);
    drop(db);
    let claimed=send(&mut app,&u,"/mail/receive_mail",&format!("MailIndex={id}")).await;
    assert_eq!(claimed["Result"],"Success");
    assert!(claimed["AchievementInfos"].as_array().is_some(),"Claims must retain progression notifications");
    let mut db=s.db.acquire().await.unwrap();
    assert_eq!(View::load(&mut db,account).await.unwrap().metric("ReceiveMail","all",None,None),1);
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
async fn world_boss_guideline_restores_existing_participation_and_claims_once() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let quest=s.tables.progression.sub_quests.iter().find(|q|n(q,"QuestIndex")==10731).unwrap();
    let mut db=s.db.acquire().await.unwrap();
    record(&mut db,a,"EnterDungeon",1,1,10).await.unwrap();
    assert_eq!(View::load(&mut db,a).await.unwrap().quest(&s,quest),0);
    // This is the persisted metric written by validated World Boss entry before
    // the Guideline evaluator supported EnterWorldBoss. No migration or new run
    // should be needed, even if participation was in a previous season.
    record(&mut db,a,"EnterDungeon",7003,1,1).await.unwrap();
    sqlx::query("UPDATE progression_metrics SET day='2020-01-01' WHERE account_id=? AND arg=7003")
        .bind(a).execute(&mut *db).await.unwrap();
    assert_eq!(View::load(&mut db,a).await.unwrap().quest(&s,quest),1);
    drop(db);
    let login=relogin(&s).await;
    assert!(login.sub_quest_infos.iter().any(|v|n(v,"SubQuestIndex")==10731 && n(v,"Progress")==1 && n(v,"LastStep")==0));
    let before=currency(&s,&login).await;
    async fn points(s:&AppState,a:i64)->i64 {
        sqlx::query_scalar("SELECT value FROM battle_currencies WHERE account=? AND kind='WorldBossPoint'")
            .bind(a).fetch_optional(&s.db).await.unwrap().unwrap_or(0)
    }
    let before_points=points(&s,a).await;
    let claim=call(&s,&login,"complete_sub_quest","SubQuestIndices=[10731]").await;
    assert_eq!(claim["Result"],"Success","{claim}");
    assert_eq!(points(&s,a).await,before_points+2000);
    assert_eq!(currency(&s,&login).await,before);
    assert_ne!(call(&s,&login,"complete_sub_quest","SubQuestIndices=[10731]").await["Result"],"Success");
    assert_eq!(points(&s,a).await,before_points+2000);
}

#[tokio::test]
async fn world_boss_guideline_notifies_on_valid_entry_without_a_kill_or_duplicate_credit() {
    use tower::Service;
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    sqlx::query("UPDATE heroes SET level=60 WHERE account_id=? AND hero_index=1").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET world_boss_ticket=2 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let mut app=axum::Router::new()
        .route("/campaign/begin",axum::routing::post(crate::api::battle::campaign_handlers::begin_campaign))
        .route("/campaign/end",axum::routing::post(crate::api::battle::campaign_handlers::end_campaign))
        .layer(axum::middleware::from_fn_with_state(s.clone(),notifications::notify)).with_state(s.clone());
    async fn send(app:&mut axum::Router,u:&user::LoginResponse,path:&str,args:&str)->Value {
        let r=app.call(axum::http::Request::builder().method("POST").uri(path)
            .header("Content-Type","application/x-www-form-urlencoded")
            .body(axum::body::Body::from(form(u,args))).unwrap()).await.unwrap();
        serde_json::from_slice(&axum::body::to_bytes(r.into_body(),usize::MAX).await.unwrap()).unwrap()
    }
    let entry="ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&WorldBossIndex=1";
    let bad=send(&mut app,&u,"/campaign/begin",&entry.replace("WorldBossIndex=1","WorldBossIndex=2")).await;
    assert_ne!(bad["Result"],"Success");
    assert_eq!(call(&s,&u,"complete_sub_quest","SubQuestIndices=[10731]").await["Result"],"Fail");
    let first=send(&mut app,&u,"/campaign/begin",entry).await;
    assert_eq!(first["Result"],"Success","{first}");
    assert!(first["ReservedSubQuestInfos"].as_array().unwrap().iter().any(|v|n(v,"SubQuestIndex")==10731 && n(v,"Progress")==1));
    let again=send(&mut app,&u,"/campaign/begin",entry).await;
    assert_eq!(again["Result"],"Success");
    assert!(!again["ReservedSubQuestInfos"].as_array().unwrap().iter().any(|v|n(v,"SubQuestIndex")==10731));
    let end=send(&mut app,&u,"/campaign/end","ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&Completed=false&TotalDamage=1000").await;
    assert_eq!(end["Result"],"Success","{end}");
    let mut db=s.db.acquire().await.unwrap();
    let view=View::load(&mut db,a).await.unwrap();
    for quest in s.tables.progression.sub_quests.iter().filter(|q|q["Kind"]=="EnterWorldBoss") {
        assert_eq!(view.quest(&s,quest),1);
    }
    assert_eq!(view.metric("EnterDungeon","all",Some(7001),Some(1)),1);
}

#[tokio::test]
async fn battle_missions_filter_modes_and_respect_daily_and_weekly_periods() {
    use chrono::{Duration,Utc};
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    let today=Utc::now().date_naive();
    let yesterday=(today-Duration::days(1)).to_string();
    let old=(today-Duration::days(8)).to_string();
    for (day,kind,chapter,dungeon,value) in [
        (today.to_string(),"ClearDungeon",20001,1,1), (yesterday.clone(),"ClearDungeon",20001,1,2), (old.clone(),"ClearDungeon",20001,1,7),
        (today.to_string(),"ClearDungeon",5001,1,2), (yesterday.clone(),"ClearDungeon",5001,2,3), (old.clone(),"ClearDungeon",5001,1,9),
        (today.to_string(),"ClearDungeon",1,1,1000),
        (today.to_string(),"CompleteWorldBoss",1,0,1), (yesterday.clone(),"CompleteWorldBoss",1,0,2), (old,"CompleteWorldBoss",1,0,7),
    ] {
        sqlx::query("INSERT INTO progression_metrics(account_id,day,kind,arg,arg2,value) VALUES(?,?,?,?,?,?)")
            .bind(a).bind(day).bind(kind).bind(chapter).bind(dungeon).bind(value).execute(&s.db).await.unwrap();
    }
    let mut db=s.db.acquire().await.unwrap();
    let view=View::load(&mut db,a).await.unwrap();
    for (id,daily,previous) in [(1105,1,0),(1107,2,0),(1108,1,0),(1706,1,2),(1707,2,3),(1721,2,3),(1710,1,2)] {
        let row=s.tables.progression.achievements.iter().find(|r|n(r,"Index")==id).unwrap();
        let expected=daily+if previous>0 && yesterday>=period(row) {previous} else {0};
        assert_eq!(view.achievement(&s,row).0,expected,"mission {id}");
    }
    for (kind,chapter,dungeon,value) in [("ClearDungeon",20001,1,10),("ClearDungeon",5001,1,10),("CompleteWorldBoss",1,0,3)] {
        record(&mut db,a,kind,chapter,dungeon,value).await.unwrap();
    }
    drop(db);
    let batch="AchievementIndices=1706&Steps=0&AchievementIndices=1707&Steps=0&AchievementIndices=1721&Steps=0&AchievementIndices=1710&Steps=0";
    let claimed=call(&s,&u,"reward_achievement",batch).await;
    assert_eq!(claimed["Result"],"Success","{claimed}");
    for id in [1706,1707,1721,1710] {
        assert!(claimed["AchievementInfos"].as_array().unwrap().iter().any(|r|n(r,"AchievementIndex")==id && n(r,"LastStep")==1));
    }
    assert_ne!(call(&s,&u,"reward_achievement",batch).await["Result"],"Success");
}

#[tokio::test]
async fn battle_missions_notify_clears_sweeps_and_finished_world_boss_without_duplicate_claims() {
    use tower::Service;
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    sqlx::query("UPDATE heroes SET level=60 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET world_boss_ticket=2,gold=10000000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let mut app=axum::Router::new()
        .route("/campaign/begin_campaign",axum::routing::post(crate::api::battle::handle))
        .route("/campaign/end_campaign",axum::routing::post(crate::api::battle::handle))
        .route("/sweep/sweep_dungeon",axum::routing::post(crate::api::battle::handle))
        .layer(axum::middleware::from_fn_with_state(s.clone(),notifications::notify)).with_state(s.clone());
    async fn send(app:&mut axum::Router,u:&user::LoginResponse,path:&str,args:&str)->Value {
        let r=app.call(axum::http::Request::builder().method("POST").uri(path)
            .header("Content-Type","application/x-www-form-urlencoded")
            .body(axum::body::Body::from(form(u,args))).unwrap()).await.unwrap();
        serde_json::from_slice(&axum::body::to_bytes(r.into_body(),usize::MAX).await.unwrap()).unwrap()
    }
    fn progress(r:&Value,id:i64)->i64 {
        n(r["AchievementInfos"].as_array().expect("Gameplay must return mission updates").iter()
            .find(|v|n(v,"AchievementIndex")==id).unwrap(),"Achievement")
    }
    let tower="ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=21&TowerFloor=1";
    assert_eq!(send(&mut app,&u,"/campaign/begin_campaign",tower).await["Result"],"Success");
    let defeat=send(&mut app,&u,"/campaign/end_campaign",&format!("{tower}&Completed=false")).await;
    assert_eq!(defeat["Result"],"Success","{defeat}");
    assert_eq!(progress(&defeat,1107),0);
    assert_eq!(send(&mut app,&u,"/campaign/begin_campaign",tower).await["Result"],"Success");
    let won=send(&mut app,&u,"/campaign/end_campaign",&format!("{tower}&Completed=true&AliveHeroIndices=[1]")).await;
    assert_eq!(won["Result"],"Success","{won}");
    assert_eq!(progress(&won,1107),1);
    assert_eq!(progress(&won,1707),1);
    assert_eq!(progress(&won,1105),0);
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,120125,10) ON CONFLICT(account_id,item_index) DO UPDATE SET count=10")
        .bind(a).execute(&s.db).await.unwrap();
    let sweep=send(&mut app,&u,"/sweep/sweep_dungeon","ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&SweepCount=2").await;
    assert_eq!(sweep["Result"],"Success","{sweep}");
    assert_eq!(progress(&sweep,1107),3);
    assert_eq!(progress(&sweep,1707),3);
    let stockade="ChapterIndex=20001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]";
    assert_eq!(send(&mut app,&u,"/campaign/begin_campaign",stockade).await["Result"],"Success");
    let won=send(&mut app,&u,"/campaign/end_campaign",&format!("{stockade}&Completed=true&AliveHeroIndices=[1]")).await;
    assert_eq!(won["Result"],"Success","{won}");
    assert_eq!(progress(&won,1105),1);
    assert_eq!(progress(&won,1706),1);
    let boss="ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&WorldBossIndex=1";
    let begin=send(&mut app,&u,"/campaign/begin_campaign",boss).await;
    assert_eq!(begin["Result"],"Success","{begin}");
    assert_eq!(progress(&begin,1108),0,"Entry alone must not count as a completed fight");
    let ending=format!("{boss}&Completed=false&TotalDamage=1000");
    let end=send(&mut app,&u,"/campaign/end_campaign",&ending).await;
    assert_eq!(end["Result"],"Success","{end}");
    assert_eq!(progress(&end,1108),1);
    assert_eq!(progress(&end,1710),1);
    assert_ne!(send(&mut app,&u,"/campaign/end_campaign",&ending).await["Result"],"Success");
    let batch="AchievementIndices=1105&Steps=0&AchievementIndices=1107&Steps=0&AchievementIndices=1108&Steps=0";
    let flask=s.tables.get_item_index("RUBYFLASK_EMPTY_13").unwrap() as i64;
    let material=s.tables.get_item_index("MATERIAL_56").unwrap() as i64;
    let before_flask=count(&s,&u,flask).await;
    let before_material=count(&s,&u,material).await;
    let claim=call(&s,&u,"reward_achievement",batch).await;
    assert_eq!(claim["Result"],"Success","{claim}");
    assert_eq!(count(&s,&u,flask).await,before_flask+3);
    assert_eq!(count(&s,&u,material).await,before_material+10);
    let points:i64=sqlx::query_scalar("SELECT value FROM battle_currencies WHERE account=? AND kind='WorldBossPoint'")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(points,1000);
    let after=currency(&s,&u).await;
    assert_ne!(call(&s,&u,"reward_achievement",batch).await["Result"],"Success");
    assert_eq!(currency(&s,&u).await,after);
    assert_eq!(count(&s,&u,flask).await,before_flask+3);
    assert_eq!(count(&s,&u,material).await,before_material+10);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT value FROM battle_currencies WHERE account=? AND kind='WorldBossPoint'")
        .bind(a).fetch_one(&s.db).await.unwrap(),1000);
}

#[tokio::test]
async fn battle_missions_world_boss_history_migration_and_score_writes_are_idempotent() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    for trigger in ["progress_world_boss_complete_insert","progress_world_boss_complete_update"] {
        sqlx::query(&format!("DROP TRIGGER {trigger}")).execute(&s.db).await.unwrap();
    }
    for family in ["world_boss","event_world_boss","challenge"] {
        sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score,battle_time) VALUES(?,1,1,?,date('now'),1000,60)")
            .bind(family).bind(a).execute(&s.db).await.unwrap();
    }
    for _ in 0..2 {crate::api::battle::migrate(&s.db).await.unwrap();}
    async fn completions(s:&AppState,a:i64)->i64 {
        let mut db=s.db.acquire().await.unwrap();
        View::load(&mut db,a).await.unwrap().metric("CompleteWorldBoss","all",None,None)
    }
    assert_eq!(completions(&s,a).await,1);
    sqlx::query("UPDATE battle_scores SET score=score+1000 WHERE account=? AND family='world_boss'").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(completions(&s,a).await,2);
    let mut tx=s.db.begin().await.unwrap();
    sqlx::query("UPDATE battle_scores SET score=score+1000 WHERE account=? AND family='world_boss'").bind(a).execute(&mut *tx).await.unwrap();
    tx.rollback().await.unwrap();
    sqlx::query("UPDATE battle_scores SET score=score WHERE account=?").bind(a).execute(&s.db).await.unwrap();
    crate::api::battle::migrate(&s.db).await.unwrap();
    assert_eq!(completions(&s,a).await,2,"Retries, rollback and other boss modes must not create extra completions");
}

#[tokio::test]
async fn story_mission_notifications_do_not_replay_after_battles_or_lobby_refresh() {
    use tower::Service;
    async fn send(app: &mut axum::Router, u: &user::LoginResponse, path: &str, data: &str) -> Value {
        let response = app.call(axum::http::Request::builder().method("POST").uri(path)
            .header("Content-Type", "application/x-www-form-urlencoded")
            .body(axum::body::Body::from(form(u, data))).unwrap()).await.unwrap();
        serde_json::from_slice(&axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap()).unwrap()
    }
    async fn lobby(s: &AppState, u: &user::LoginResponse) -> Value {
        json!(crate::api::account::lobby::enter_lobby(State(s.clone()),
            axum::extract::Form(crate::api::account::lobby::EnterLobbyRequest {
                session_id: None, session_key: Some(u.user_info.session_key.clone()),
            })).await.unwrap().0)
    }
    let (s, u) = setup().await;
    let mut app = axum::Router::new()
        .route("/campaign/begin", axum::routing::post(crate::api::battle::campaign_handlers::begin_campaign))
        .route("/campaign/end", axum::routing::post(crate::api::battle::campaign_handlers::end_campaign))
        .layer(axum::middleware::from_fn_with_state(s.clone(), notifications::notify))
        .with_state(s.clone());
    assert_eq!(lobby(&s, &u).await["ReservedSubQuestInfos"], json!([]));
    let mut completed = BTreeSet::new();
    for dungeon in [1, 2, 2] {
        let entry = format!("ChapterIndex=1&DungeonIndex={dungeon}&DungeonDifficulty=1&HeroIndices=[1]");
        let begin = send(&mut app, &u, "/campaign/begin", &entry).await;
        assert_eq!(begin["Result"], "Success", "{begin}");
        for info in begin["ReservedSubQuestInfos"].as_array().unwrap() {
            assert!(!completed.contains(&n(info, "SubQuestIndex")), "Old completion sent again at battle start: {info}");
        }
        let end = send(&mut app, &u, "/campaign/end", &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]")).await;
        assert_eq!(end["Result"], "Success", "{end}");
        let quest = 10000 + dungeon * 10;
        let updates = end["ReservedSubQuestInfos"].as_array().unwrap();
        for info in updates {
            assert!(!completed.contains(&n(info, "SubQuestIndex")), "Old completion sent again after battle: {info}");
        }
        if completed.insert(quest) {
            assert!(updates.iter().any(|i| n(i, "SubQuestIndex") == quest && n(i, "Progress") == 1));
        }
        // Empty native update arrays preserve the battle's pending popup queue;
        // the lobby consumes that queue without adding the same quests again.
        for _ in 0..2 {
            assert_eq!(lobby(&s, &u).await["ReservedSubQuestInfos"], json!([]));
        }
        assert_eq!(call(&s, &u, "get_achievements", "").await["ReservedSubQuestInfos"], json!([]));
    }
    // Unclaimed rewards remain available; notification deduplication must not claim them.
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM progression_claims WHERE account_id=? AND family='subquest'")
        .bind(u.user_info.account_id).fetch_one(&s.db).await.unwrap(), 0);
    let login = relogin(&s).await;
    for id in completed {
        let info = login.sub_quest_infos.iter().find(|i| n(i, "SubQuestIndex") == id).unwrap();
        assert_eq!(info["Progress"], 1);
        assert_eq!(info["LastStep"], 0);
    }
    assert_eq!(lobby(&s, &login).await["ReservedSubQuestInfos"], json!([]));
    let claim = call(&s, &login, "complete_sub_quest", "SubQuestIndices=[10010]").await;
    assert_eq!(claim["Result"], "Success");
    assert_eq!(claim["SucceededSubQuestInfos"][0]["LastStep"], 1);
    assert_eq!(lobby(&s, &login).await["ReservedSubQuestInfos"], json!([]));
}

#[tokio::test]
async fn mission_notification_baselines_are_isolated_per_session() {
    let (s, u) = setup().await;
    let other = relogin(&s).await;
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,1,1,13) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=1,best_star=13")
        .bind(u.user_info.account_id).execute(&s.db).await.unwrap();
    for session in [&u, &other] {
        let update = call(&s, session, "get_achievements", "").await;
        assert!(update["ReservedSubQuestInfos"].as_array().unwrap().iter().any(|i| i["SubQuestIndex"] == 10010 && i["Progress"] == 1));
        assert_eq!(call(&s, session, "get_achievements", "").await["ReservedSubQuestInfos"], json!([]));
    }
}

#[tokio::test]
async fn attendance_cycle_wraps_and_nonrepeating_calendar_finishes() {
    let (mut s, u) = setup().await;
    let days = s.tables.progression.calendars[0]["Reward"].as_array().unwrap().len() as i64;
    sqlx::query("INSERT INTO attendance_calendar_state(account_id,idx,claims,last_day) VALUES(?,1,?,date('now'))")
        .bind(u.user_info.account_id).bind(days).execute(&s.db).await.unwrap();
    let completed = call(&s, &u, "attendance_info", "").await;
    assert_eq!(completed["AttendanceInfos"][0]["CompletedDay"], days);
    assert_eq!(completed["AttendanceDatas"][0]["AttendanceRewardableDays"], 0);
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
    let completed = call(&s, &u, "attendance_info", "").await;
    assert_eq!(completed["AttendanceInfos"][0]["CompletedDay"], days);
    assert_eq!(completed["AttendanceDatas"][0]["AttendanceRewardableDays"], 0);
    assert_eq!(
        call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await["Result"],
        "Fail"
    );
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).progression).calendars[0]["Repeatable"] =
        json!(1);
    let pending = call(&s, &u, "attendance_info", "").await;
    assert_eq!(pending["AttendanceInfos"][0]["CompletedDay"], 0);
    assert_eq!(pending["AttendanceDatas"][0]["AttendanceRewardableDays"], 1);
    let r = call(&s, &u, "get_attendance_reward", "AttendanceIndex=1").await;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["AttendanceInfos"][0]["LastRewardedDay"], 1);
    assert_eq!(r["AttendanceInfos"][0]["CompletedDay"], 1);
    assert_eq!(r["AttendanceDatas"][0]["AttendanceRewardableDays"], 0);
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
        "AchievementIndices=[1101]&Steps=[0]",
    )
    .await;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["staminaResults"][0]["Type"], "Chicken");
    assert_eq!(r["staminaResults"][0]["AddValue"], 10);
}


#[tokio::test]
async fn tutorial_progression_does_not_complete_scoped_stockade_guild_or_crafting_missions() {
    let (s,u)=setup().await;
    let account=u.user_info.account_id;
    let completed=crate::api::tutorial::complete_tutorial(State(s.clone()),axum::extract::Form(crate::api::tutorial::TutorialRequest {
        session_id:None,session_key:Some(u.user_info.session_key.clone()),tutorial_index:Some(14000),
    })).await.unwrap().0;
    assert_eq!(completed["Result"],"Success");
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,1,1,1,13) ON CONFLICT(account_id,chapter_id,dungeon_id) DO UPDATE SET clear_count=clear_count+1")
        .bind(account).execute(&s.db).await.unwrap();
    for kind in ["ConsumeStamina","StaminaUses","AddItem","BuyShopItem","CraftItem","UseItem","EnterDungeon"] {
        record(&mut s.db.acquire().await.unwrap(),account,kind,1,1,100).await.unwrap();
    }
    let view=View::load(&mut s.db.acquire().await.unwrap(),account).await.unwrap();
    for row in s.tables.progression.sub_quests.iter().filter(|r|r["Kind"]=="ClearDungeon" && r["StateValue"].is_null()) {
        assert_eq!(view.quest(&s,row),0,"Unrelated tutorial clear credited quest {} ({})",row["QuestIndex"],row["Goal"]);
    }
    let balance=currency(&s,&u).await;
    for id in [10161,1002190,1002200,1002240,1002260,1002090,1002100,1002130,1002150,1002210,1002070] {
        let result=call(&s,&u,"complete_sub_quest",&format!("SubQuestIndices=[{id}]")).await;
        assert_ne!(result["Result"],"Success","Unrelated tutorial progress made mission {id} claimable");
    }
    assert_eq!(currency(&s,&u).await,balance);
    let restored=relogin(&s).await;
    assert_eq!(restored.sub_quest_infos.iter().find(|r|r["SubQuestIndex"]==10161).unwrap()["Progress"],0);
    // Correctly scoped tutorial and campaign missions still progress normally.
    let tutorial=s.tables.progression.sub_quests.iter().find(|r|r["QuestIndex"]==10140).unwrap();
    assert_eq!(view.quest(&s,tutorial),1);
}

#[tokio::test]
async fn stockade_progression_requires_fragment_clears_and_corrects_existing_unclaimed_progress() {
    let (s,u)=setup().await;
    let account=u.user_info.account_id;
    let row=s.tables.progression.sub_quests.iter().find(|r|r["QuestIndex"]==10161).unwrap();
    for (chapter,expected) in [(1,0),(20004,0),(20002,0),(20001,1)] {
        record(&mut s.db.acquire().await.unwrap(),account,"ClearDungeon",chapter,1,1).await.unwrap();
        let view=View::load(&mut s.db.acquire().await.unwrap(),account).await.unwrap();
        assert_eq!(view.quest(&s,row),expected,"chapter {chapter}");
    }
    assert_eq!(call(&s,&u,"complete_sub_quest","SubQuestIndices=[10161]").await["Result"],"Success");
    assert_ne!(call(&s,&u,"complete_sub_quest","SubQuestIndices=[10161]").await["Result"],"Success");
}

#[tokio::test]
async fn guild_damage_progression_uses_saved_personal_raid_damage_and_64_bit_thresholds() {
    let (s,u)=setup().await;
    let account=u.user_info.account_id;
    let row=s.tables.progression.sub_quests.iter().find(|r|r["QuestIndex"]==1002190).unwrap();
    record(&mut s.db.acquire().await.unwrap(),account,"ClearDungeon",1,1,999999).await.unwrap();
    sqlx::query("INSERT INTO guild_battle_scores(guild_id,account,kind,season,stage,score) VALUES(1,?,'suppress',1,1,90000000000),(1,?,'raid',1,1,4999999999)")
        .bind(account).bind(account).execute(&s.db).await.unwrap();
    let view=View::load(&mut s.db.acquire().await.unwrap(),account).await.unwrap();
    assert_eq!(view.quest(&s,row),0);
    sqlx::query("UPDATE guild_battle_scores SET score=score+1 WHERE account=? AND kind='raid'").bind(account).execute(&s.db).await.unwrap();
    let view=View::load(&mut s.db.acquire().await.unwrap(),account).await.unwrap();
    assert_eq!(view.quest(&s,row),1);
    let next=s.tables.progression.sub_quests.iter().find(|r|r["QuestIndex"]==1002200).unwrap();
    assert_eq!(view.quest(&s,next),0);
    assert_eq!(call(&s,&u,"complete_sub_quest","SubQuestIndices=[1002190]").await["Result"],"Success");
    assert_ne!(call(&s,&u,"complete_sub_quest","SubQuestIndices=[1002190]").await["Result"],"Success");
}


/// Diagnostic harness: uses an explicit isolated database copy, never the live DB.
#[tokio::test]
#[ignore]
async fn progression_latency_profile() {
    let path=std::path::PathBuf::from(std::env::var("SPRK_PERF_DB").expect("Set SPRK_PERF_DB to an isolated target database copy"));
    let path=path.canonicalize().unwrap();
    assert!(path.starts_with(std::env::current_dir().unwrap().join("target").canonicalize().unwrap()));
    let db=sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect_with(sqlx::sqlite::SqliteConnectOptions::new().filename(&path).read_only(true)).await.unwrap();
    let s=AppState::new(db,crate::tables::test_tables());
    let account:i64=sqlx::query_scalar("SELECT account_id FROM heroes GROUP BY account_id ORDER BY COUNT(*) DESC LIMIT 1").fetch_one(&s.db).await.unwrap();
    let mut connection=s.db.acquire().await.unwrap();
    let now=std::time::Instant::now();let view=View::load(&mut connection,account).await.unwrap();println!("PROFILE view_load_ms={:.2} heroes={} metrics={} claims={}",now.elapsed().as_secs_f64()*1000.,view.heroes.len(),view.metrics.len(),view.claims.len());
    let now=std::time::Instant::now();let achievements=view.achievement_infos(&s);println!("PROFILE achievements_ms={:.2} count={}",now.elapsed().as_secs_f64()*1000.,achievements.len());
    let now=std::time::Instant::now();let quests=view.subquest_infos(&s);println!("PROFILE subquests_ms={:.2} count={}",now.elapsed().as_secs_f64()*1000.,quests.len());
    let now=std::time::Instant::now();let records=play_record::snapshot(&mut connection,&s,account,&view).await.unwrap();println!("PROFILE records_ms={:.2} count={}",now.elapsed().as_secs_f64()*1000.,records.len());
    let now=std::time::Instant::now();let _=opened_mission_categories(&s,&view);println!("PROFILE categories_ms={:.2}",now.elapsed().as_secs_f64()*1000.);
    let now=std::time::Instant::now();let result=snapshot(&mut connection,&s,account).await.unwrap();println!("PROFILE full_snapshot_ms={:.2}",now.elapsed().as_secs_f64()*1000.);
    // PlayRecord timestamps describe response creation, not saved progress.
    let mut comparable = result;
    for row in comparable["PlayRecordInfos"].as_array_mut().unwrap() {
        row.as_object_mut().unwrap().remove("UpdatedTime");
    }
    let output=std::path::PathBuf::from("target/api-latency/progression-golden.json");
    if output.exists() {
        let mut before:Value=serde_json::from_slice(&std::fs::read(output).unwrap()).unwrap();
        for row in before["PlayRecordInfos"].as_array_mut().unwrap() {
            row.as_object_mut().unwrap().remove("UpdatedTime");
        }
        for (key, value) in comparable.as_object().unwrap() {
            assert!(value == &before[key], "Changed snapshot field {key}");
        }
        assert_eq!(comparable.as_object().unwrap().len(),before.as_object().unwrap().len());
        println!("PROFILE progression_payload_matches_baseline=true");
    } else {
        std::fs::write(output,serde_json::to_vec_pretty(&comparable).unwrap()).unwrap();
    }
}
