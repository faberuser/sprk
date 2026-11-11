use super::tests::{account, call, setup};
use super::*;
async fn fixture(deferred: bool) -> (AppState, Value) {
    let (s, u) = setup().await;
    let a = account(&u);
    crate::db::query("UPDATE user_info SET stamina=1000 WHERE account_id=$1")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let mut db = s.db.acquire().await.unwrap();
    put(
        &mut db,
        a,
        "dungeon",
        campaign::key(1, 1),
        &json!({"FirstRewardedDiff":2,"MaxStar":13}),
    )
    .await
    .unwrap();
    if deferred {
        put(
            &mut db,
            a,
            "entry_policy",
            0,
            &json!({"RefundPveDefeats":true}),
        )
        .await
        .unwrap();
    }
    drop(db);
    (s, u)
}

#[tokio::test]
async fn offline_dispatch_overdue_estimate_refreshes_without_fabricating_results() {
    let (s, u) = fixture(true).await;
    let job = start(&s, &u, 3).await;
    let slot = n(&job, "SlotIndex");
    let a = account(&u);
    let mut saved = job.clone();
    saved["SimulationVersion"] = json!(2);
    saved["StartTimestamp"] = json!(now() - 600);
    saved["FinishTimestamp"] = json!(now() - 1);
    saved["SimulationResults"] = json!([{"Outcome":1,"TimeMs":10000},{"Outcome":0,"TimeMs":10000}]);
    put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &saved).await.unwrap();
    let list = call(&s, &u, "dispatch/get_dispatch_list", "").await;
    let info = &list["DispatchBattleInfos"][0];
    assert!(n(info,"FinishTimestamp") > now());
    assert_eq!(info["WinCount"], 1);
    assert_eq!(info["LoseCount"], 1);
    assert_eq!(info["State"], "Battle");
    assert_ne!(call(&s, &u, "dispatch/request_complete_dispatch", &format!("SlotIndex={slot}")).await["Result"], "Success");
    let done = call(&s, &u, "dispatch/cancel_dispatch", &format!("SlotIndex={slot}")).await;
    assert_eq!(done["Result"], "Success", "{done}");
    assert_eq!(done["DispatchBattleInfo"]["ServerResult"], "1,0");
    assert_eq!(done["DispatchBattleInfo"]["WinCount"], 1);
    assert_eq!(done["DispatchBattleInfo"]["LoseCount"], 1);
}

#[tokio::test]
async fn dispatch_polling_writes_only_when_progress_changes() {
    let (s,u)=fixture(true).await;let a=account(&u);
    let before:i64=crate::database::row_changes(&s.db).await;
    assert!(call(&s,&u,"dispatch/get_dispatch_list","").await["DispatchBattleInfos"].as_array().unwrap().is_empty());
    assert_eq!(crate::database::row_changes(&s.db).await,before);
    let job=start(&s,&u,2).await;let slot=n(&job,"SlotIndex");
    let mut saved=job.clone();saved["SimulationResults"]=json!([{"Outcome":1,"TimeMs":60000}]);
    saved["State"]=json!("Battle");saved["StartTimestamp"]=json!(now()+60);saved["FinishTimestamp"]=json!(now()+120);
    put(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot,&saved).await.unwrap();
    call(&s,&u,"dispatch/get_dispatch_list","").await; // normalize derived state once
    let before:i64=crate::database::row_changes(&s.db).await;
    for _ in 0..3 {call(&s,&u,"dispatch/get_dispatch_list","").await;}
    assert_eq!(crate::database::row_changes(&s.db).await,before);
    let mut saved=get(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot).await.unwrap();
    saved["StartTimestamp"]=json!(now()-300);saved["FinishTimestamp"]=json!(now()-1);
    put(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot,&saved).await.unwrap();
    let response=call(&s,&u,"dispatch/get_dispatch_list","").await;
    assert_eq!(response["DispatchBattleInfos"][0]["WinCount"],1);
}
#[tokio::test]
async fn technomagic_dispatch_preserves_entry_gates_and_charges_and_rewards_only_wins() {
    let (s,u)=fixture(true).await;
    let a=account(&u);
    for (index,dungeon) in [(301,2001),(302,2101),(303,2201)] {
        let args=format!("ChapterIndex=10&DungeonIndex={dungeon}&Difficulty=0&HeroIndices=[1]&RepeatCount=10&DeckIndex=1");
        assert_ne!(
            call(&s, &u, "dispatch/start_dispatch", &args).await["Result"],
            "Success"
        );
        for (chapter, required) in [(97, 5), (10, 9), (10, 30)] {
            let diff = s.tables.tutorials.dungeon_difficulty(chapter, required);
            put(
                &mut s.db.acquire().await.unwrap(),
                a,
                "dungeon",
                campaign::key(chapter as i64, required as i64),
                &json!({"FirstRewardedDiff":1<<diff}),
            )
            .await
            .unwrap();
        }
        // Having the story prerequisite is not enough: this raid stage must be cleared.
        assert_ne!(
            call(&s, &u, "dispatch/start_dispatch", &args).await["Result"],
            "Success"
        );
        put(
            &mut s.db.acquire().await.unwrap(),
            a,
            "dungeon",
            campaign::key(10, dungeon),
            &json!({"FirstRewardedDiff":1}),
        )
        .await
        .unwrap();
        crate::db::query("UPDATE heroes SET level=59 WHERE account_id=$1")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        assert_ne!(
            call(&s, &u, "dispatch/start_dispatch", &args).await["Result"],
            "Success"
        );
        assert_eq!(stamina(&s, &u).await, 1000);
        crate::db::query("UPDATE heroes SET level=100 WHERE account_id=$1")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        crate::db::query("UPDATE user_info SET stamina=999 WHERE account_id=$1")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        assert_ne!(
            call(&s, &u, "dispatch/start_dispatch", &args).await["Result"],
            "Success"
        );
        crate::db::query("UPDATE user_info SET stamina=1000 WHERE account_id=$1")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        let started = call(
            &s,
            &u,
            "dispatch/start_dispatch",
            &format!("{args}&RaidIndex=999&RaidLevel=999"),
        )
        .await;
        assert_eq!(started["Result"], "Success", "{started}");
        let job = &started["DispatchBattleInfo"];
        // Legacy upload/settlement coverage; server-owned jobs reject this endpoint.
        let mut legacy=job.clone();legacy["SimulationVersion"]=json!(1);
        put(&mut s.db.acquire().await.unwrap(),a,"dispatch",n(job,"SlotIndex"),&legacy).await.unwrap();
        assert_eq!(job["Request"]["RaidIndex"], index.to_string());
        assert_eq!(job["Request"]["RaidLevel"], "1");
        assert_eq!(job["Cost"], 1000);
        assert_eq!(stamina(&s, &u).await, 1000);
        assert_ne!(
            call(&s, &u, "dispatch/start_dispatch", &args).await["Result"],
            "Success"
        );
        assert_eq!(
            call(
                &s,
                &u,
                "dispatch/complete_calculate_dispatch_result",
                &report(job, &[1, 0, 1, 0, 0, 1, 0, 0, 0, 0])
            )
            .await["Result"],
            "Success"
        );
        finish_now(&s, &u, n(job, "SlotIndex")).await;
        let mail_before: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=$1")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
        let collect = format!("SlotIndex={}", n(job, "SlotIndex"));
        let done = call(&s, &u, "dispatch/request_complete_dispatch", &collect).await;
        assert_eq!(done["Result"], "Success", "{done}");
        assert_eq!(done["DispatchBattleInfo"]["WinCount"], 3);
        assert_eq!(done["DispatchBattleInfo"]["LoseCount"], 7);
        assert_eq!(stamina(&s, &u).await, 700);
        assert_eq!(done["TeamExpResult"]["AddValue"], 3000);
        assert_eq!(done["EquipItemInfos"], done["EquipItemResults"]);
        let mail_after: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=$1")
            .bind(a)
            .fetch_one(&s.db)
            .await
            .unwrap();
        assert!(
            mail_after > mail_before
                || done["EquipItemResults"].as_array().is_some_and(|items| !items.is_empty())
                || done["ItemResults"]
                    .as_array()
                    .is_some_and(|items| !items.is_empty()),
            "raid rewards were not delivered: {done}"
        );
        assert_ne!(
            call(&s, &u, "dispatch/request_complete_dispatch", &collect).await["Result"],
            "Success"
        );
        assert_eq!(stamina(&s, &u).await, 700);
        assert!(
            call(&s, &u, "dispatch/get_dispatch_list", "").await["DispatchBattleInfos"]
                .as_array()
                .unwrap()
                .is_empty()
        );
        crate::db::query("UPDATE user_info SET stamina=1000 WHERE account_id=$1")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
    }
}
async fn stamina(s: &AppState, u: &Value) -> i64 {
    crate::db::query_scalar("SELECT stamina FROM user_info WHERE account_id=$1")
        .bind(account(u))
        .fetch_one(&s.db)
        .await
        .unwrap()
}
async fn start(s: &AppState, u: &Value, count: i64) -> Value {
    let v=call(s,u,"dispatch/start_dispatch",&format!("ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount={count}&DeckIndex=1")).await;
    assert_eq!(v["Result"], "Success", "{v}");
    // These protocol tests deliberately exercise legacy client-owned preparation.
    let mut job=v["DispatchBattleInfo"].clone();
    job["SimulationVersion"]=json!(1);
    put(&mut s.db.acquire().await.unwrap(),account(u),"dispatch",n(&job,"SlotIndex"),&job).await.unwrap();
    job
}
fn report(job: &Value, outcomes: &[i64]) -> String {
    format!(
        "SlotIndex={}&SimulationJobId={}&Results={}&TimesMs={}",
        n(job, "SlotIndex"),
        job["SimulationJobId"].as_str().unwrap(),
        json!(outcomes),
        json!(vec![60000; outcomes.len()])
    )
}
async fn finish_now(s: &AppState, u: &Value, slot: i64) {
    let mut db = s.db.acquire().await.unwrap();
    let a = account(u);
    let mut job = get(&mut db, a, "dispatch", slot).await.unwrap();
    job["FinishTimestamp"] = json!(now() - 1);
    put(&mut db, a, "dispatch", slot, &job).await.unwrap();
}
#[tokio::test]
async fn dispatch_equipment_fills_inventory_and_mails_only_overflow_once() {
    for cancel in [false, true] {
        for spaces in [0, 1, 6] {
            let (mut s, u) = fixture(true).await;
            let a = account(&u);
            let dungeon = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
                .tables.get_mut("CampaignDungeon").unwrap().iter_mut()
                .find(|d| n(d,"ChapterIndex")==1 && n(d,"DungeonIndex")==1).unwrap();
            let drop_index = n(dungeon,"DropRewardIndex_Normal") as i32;
            dungeon["FirstClearRewardIndex_Normal"] = json!(0);
            dungeon["FirstRewardIndex"] = json!(0);
            assert_eq!(s.tables.reward_string_pool.get(66),Some("ARMOR_HEAVY_8_6_4"));
            let reward = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).rewards)
                .entries.iter_mut().find(|r| r.index==drop_index).unwrap();
            *reward = crate::tables::RewardData {
                index: drop_index,
                sub_data_list: vec![json!([2,[66,0],1000,3,null,3,0,0,0])],
                ..Default::default()
            };
            let job = start(&s,&u,if cancel {3} else {2}).await;
            let mut saved = job.clone();
            saved["SimulationResults"] = json!([{"Outcome":1,"TimeMs":1000},{"Outcome":1,"TimeMs":1000}]);
            saved["StartTimestamp"] = json!(now()-60);
            saved["FinishTimestamp"] = json!(now()-1);
            let slot = n(&job,"SlotIndex");
            let mut db = s.db.acquire().await.unwrap();
            put(&mut db,a,"dispatch",slot,&saved).await.unwrap();
            let free = item::free_slots(&mut db,&s,a,0).await.unwrap();
            for _ in 0..(free-spaces) {
                crate::db::query("INSERT INTO equip_items(account_id,item_index) VALUES($1,1001)")
                    .bind(a).execute(&mut *db).await.unwrap();
            }
            let before: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1")
                .bind(a).fetch_one(&mut *db).await.unwrap();
            drop(db);
            let path = if cancel {"dispatch/cancel_dispatch"} else {"dispatch/request_complete_dispatch"};
            let args = format!("SlotIndex={slot}");
            let done = call(&s,&u,path,&args).await;
            assert_eq!(done["Result"],"Success","spaces={spaces}, cancel={cancel}: {done}");
            let equipment = done["EquipItemResults"].as_array().unwrap();
            assert_eq!(equipment.len(),spaces as usize,"{done}");
            assert_eq!(done["EquipItemInfos"],done["EquipItemResults"]);
            let after: i64 = crate::db::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=$1")
                .bind(a).fetch_one(&s.db).await.unwrap();
            assert_eq!(after-before,spaces);
            for equip in equipment {
                let saved = crate::api::extensions::equip(&mut s.db.acquire().await.unwrap(),a,n(equip,"SlotIndex"))
                    .await.unwrap();
                assert_eq!(&json!(saved),equip,"Inventory must preserve the exact rolled equipment");
            }
            let mails: Vec<String> = crate::db::query_scalar("SELECT reward_equipment FROM mails WHERE account_id=$1 AND reward_equipment IS NOT NULL ORDER BY mail_id")
                .bind(a).fetch_all(&s.db).await.unwrap();
            let mailed: Vec<Value> = mails.iter().flat_map(|raw| serde_json::from_str::<Vec<Value>>(raw).unwrap()).collect();
            assert_eq!(mailed.len(),(6-spaces) as usize);
            for equip in &mailed {
                assert_eq!(equip["SlotIndex"],0);
                assert_eq!(equip["Uid"],"");
                assert_eq!(equip["ItemIndex"],s.tables.get_item_index("ARMOR_HEAVY_8_6_4").unwrap());
            }
            assert_ne!(call(&s,&u,path,&args).await["Result"],"Success");
            assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=$1")
                .bind(a).fetch_one(&s.db).await.unwrap(),after);
            assert_eq!(crate::db::query_scalar::<serde_json::Value>("SELECT COALESCE(json_agg(reward_equipment ORDER BY mail_id),'[]')::text FROM mails WHERE account_id=$1 AND reward_equipment IS NOT NULL")
                .bind(a).fetch_one(&s.db).await.unwrap(),json!(mails));
        }
    }
}
#[tokio::test]
async fn dispatch_mixed_results_charge_only_wins_and_release_unplayed_costs() {
    let (s, u) = fixture(true).await;
    let job = start(&s, &u, 4).await;
    let slot = n(&job, "SlotIndex");
    let cost = n(&job, "Cost") / 4;
    assert_eq!(job["State"], "Simulation");
    assert_eq!(stamina(&s, &u).await, 1000);
    let args = report(&job, &[1, 0, 1, 0]);
    assert_eq!(
        call(&s, &u, "dispatch/complete_calculate_dispatch_result", &args).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "dispatch/complete_calculate_dispatch_result", &args).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/complete_calculate_dispatch_result",
            &report(&job, &[1, 1, 1, 1])
        )
        .await["Result"],
        "Success"
    );
    {
        let mut db = s.db.acquire().await.unwrap();
        let a = account(&u);
        let mut j = get(&mut db, a, "dispatch", slot).await.unwrap();
        j["StartTimestamp"] = json!(now() - 120);
        put(&mut db, a, "dispatch", slot, &j).await.unwrap();
    }
    let list = call(&s, &u, "dispatch/get_dispatch_list", "").await;
    let current = &list["DispatchBattleInfos"][0];
    assert_eq!(current["WinCount"], 1);
    assert_eq!(current["LoseCount"], 1);
    assert_eq!(current["ServerResult"], "1,0");
    let done = call(
        &s,
        &u,
        "dispatch/cancel_dispatch",
        &format!("SlotIndex={slot}"),
    )
    .await;
    assert_eq!(done["Result"], "Success", "{done}");
    assert_eq!(done["DispatchBattleInfo"]["WinCount"], 1);
    assert_eq!(done["DispatchBattleInfo"]["LoseCount"], 1);
    assert_eq!(stamina(&s, &u).await, 1000 - cost);
    assert_eq!(done["StaminaResult"]["AddValue"], -cost);
    assert_eq!(done["EclipseStaminaResult"], done["StaminaResult"]);
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/cancel_dispatch",
            &format!("SlotIndex={slot}")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(stamina(&s, &u).await, 1000 - cost);
    assert_eq!(
        entry_costs::held(
            &mut s.db.acquire().await.unwrap(),
            account(&u),
            "Stamina",
            1
        )
        .await
        .unwrap(),
        0
    );
}
#[tokio::test]
async fn dispatch_all_losses_grant_no_rewards_and_keep_full_stamina_in_both_payment_modes() {
    for deferred in [true, false] {
        let (s, u) = fixture(deferred).await;
        let job = start(&s, &u, 3).await;
        let slot = n(&job, "SlotIndex");
        assert_eq!(
            call(
                &s,
                &u,
                "dispatch/complete_calculate_dispatch_result",
                &report(&job, &[0, 0, 0])
            )
            .await["Result"],
            "Success"
        );
        finish_now(&s, &u, slot).await;
        let done = call(
            &s,
            &u,
            "dispatch/request_complete_dispatch",
            &format!("SlotIndex={slot}"),
        )
        .await;
        assert_eq!(done["Result"], "Success", "{done}");
        assert_eq!(stamina(&s, &u).await, 1000);
        assert_eq!(done["DispatchBattleInfo"]["WinCount"], 0);
        assert_eq!(done["DispatchBattleInfo"]["LoseCount"], 3);
        assert_eq!(done["DispatchBattleInfo"]["ServerResult"], "0,0,0");
        assert!(done["ItemResults"].as_array().is_none_or(Vec::is_empty));
        assert!(done["TeamExpResult"].is_null());
        let progress =
            campaign::progress(&mut s.db.acquire().await.unwrap(), &s, account(&u), 1, 1)
                .await
                .unwrap();
        assert_eq!(n(&progress, "ClearCount"), 0);
    }
}
#[tokio::test]
async fn dispatch_preparation_rejects_invalid_results_and_stale_job_and_cannot_collect_early() {
    let (s, u) = fixture(true).await;
    let job = start(&s, &u, 2).await;
    let slot = n(&job, "SlotIndex");
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/request_complete_dispatch",
            &format!("SlotIndex={slot}")
        )
        .await["Result"],
        "Success"
    );
    for outcomes in [&[1][..], &[2, 0][..], &[1, 1, 1][..]] {
        assert_ne!(
            call(
                &s,
                &u,
                "dispatch/complete_calculate_dispatch_result",
                &report(&job, outcomes)
            )
            .await["Result"],
            "Success"
        );
    }
    let wrong =
        report(&job, &[1, 0]).replace(job["SimulationJobId"].as_str().unwrap(), "wrong-job");
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/complete_calculate_dispatch_result",
            &wrong
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "dispatch/cancel_dispatch",
            &format!("SlotIndex={slot}")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(stamina(&s, &u).await, 1000);
    let next = start(&s, &u, 2).await;
    assert_ne!(job["SimulationJobId"], next["SimulationJobId"]);
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/complete_calculate_dispatch_result",
            &report(&job, &[1, 1])
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn dispatch_persisted_mixed_results_complete_offline_and_rewards_follow_win_count() {
    let (s, u) = fixture(true).await;
    let job = start(&s, &u, 4).await;
    let slot = n(&job, "SlotIndex");
    let cost = n(&job, "Cost") / 4;
    assert_eq!(
        call(
            &s,
            &u,
            "dispatch/complete_calculate_dispatch_result",
            &report(&job, &[1, 0, 0, 1])
        )
        .await["Result"],
        "Success"
    );
    let restarted = AppState::new(s.db.clone(), s.tables.as_ref().clone());
    let login = json!(
        crate::api::account::user::test_login(
            State(restarted.clone()),
            Bytes::from_static(b"LoginId=battle-test")
        )
        .await
        .unwrap()
        .0
    );
    finish_now(&restarted, &login, slot).await;
    let done = call(
        &restarted,
        &login,
        "dispatch/request_complete_dispatch",
        &format!("SlotIndex={slot}"),
    )
    .await;
    assert_eq!(done["Result"], "Success", "{done}");
    assert_eq!(done["DispatchBattleInfo"]["WinCount"], 2);
    assert_eq!(done["DispatchBattleInfo"]["LoseCount"], 2);
    assert_eq!(done["DispatchBattleInfo"]["ServerResult"], "1,0,0,1");
    assert_eq!(stamina(&s, &u).await, 1000 - 2 * cost);
    assert_eq!(done["TeamExpResult"]["AddValue"], 2 * cost * 10);
    assert_ne!(
        call(
            &restarted,
            &login,
            "dispatch/request_complete_dispatch",
            &format!("SlotIndex={slot}")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(stamina(&s, &u).await, 1000 - 2 * cost);
}


#[tokio::test]
async fn progressive_dispatch_exposes_first_result_before_all_repeats_and_keeps_entry_clock() {
    let (s, u) = fixture(true).await;
    crate::db::query("UPDATE user_info SET stamina=100000 WHERE account_id=$1")
        .bind(account(&u)).execute(&s.db).await.unwrap();
    let mut job = start(&s, &u, 200).await;
    let a = account(&u);
    let slot = n(&job, "SlotIndex");
    let started = now() - 120;
    job["StartTimestamp"] = json!(started);
    put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &job).await.unwrap();
    let args = format!("{}&Progressive=true", report(&job, &[1]));
    let first = call(&s, &u, "dispatch/complete_calculate_dispatch_result", &args).await;
    assert_eq!(first["Result"], "Success", "{first}");
    let info = &first["DispatchInfo"];
    assert_eq!(info["StartTimestamp"], started);
    assert_eq!(info["WinCount"], 1);
    assert_eq!(info["ServerResult"], "1");
    assert_eq!(info["State"], "Battle");
    assert_eq!(info["SimulationResults"].as_array().unwrap().len(), 1);
    finish_now(&s, &u, slot).await;
    let collect = format!("SlotIndex={slot}");
    assert_ne!(call(&s, &u, "dispatch/request_complete_dispatch", &collect).await["Result"], "Success");
    let cancelled = call(&s, &u, "dispatch/cancel_dispatch", &collect).await;
    assert_eq!(cancelled["Result"], "Success", "{cancelled}");
    assert_eq!(cancelled["DispatchBattleInfo"]["WinCount"], 1);
    assert_eq!(cancelled["DispatchBattleInfo"]["LoseCount"], 0);
    assert_eq!(stamina(&s, &u).await, 100000 - n(&job, "Cost") / 200);
    assert_ne!(call(&s, &u, "dispatch/complete_calculate_dispatch_result", &args).await["Result"], "Success");
}

#[tokio::test]
async fn progressive_dispatch_prefixes_resume_retry_and_cannot_rewrite_saved_results() {
    let (s, u) = fixture(true).await;
    let job = start(&s, &u, 4).await;
    let a = account(&u);
    let slot = n(&job, "SlotIndex");
    let upload = |outcomes: &[i64]| format!("{}&Progressive=true", report(&job, outcomes));
    for outcomes in [&[1][..], &[1, 0][..], &[1, 0][..], &[1][..]] {
        let response = call(&s, &u, "dispatch/complete_calculate_dispatch_result", &upload(outcomes)).await;
        assert_eq!(response["Result"], "Success", "{response}");
    }
    let snapshot = call(&s, &u, "dispatch/get_dispatch_list", "").await;
    let saved = &snapshot["DispatchBattleInfos"][0];
    assert_eq!(saved["SimulationResults"].as_array().unwrap().len(), 2);
    assert_eq!(saved["SimulationResults"][1]["Outcome"], 0);
    for bad in [upload(&[0, 0, 1]), upload(&[]), upload(&[1, 0, 1, 1, 1]),
        format!("SlotIndex={slot}&SimulationJobId={}&Progressive=true&Results=[1,0]&TimesMs=[60001,60000]",job["SimulationJobId"].as_str().unwrap())] {
        assert_ne!(call(&s, &u, "dispatch/complete_calculate_dispatch_result", &bad).await["Result"], "Success");
    }
    let resumed = call(&s, &u, "dispatch/complete_calculate_dispatch_result", &upload(&[1, 0, 1, 0])).await;
    assert_eq!(resumed["Result"], "Success");
    let start = n(&resumed["DispatchInfo"], "StartTimestamp");
    assert_eq!(start, n(&job, "StartTimestamp"));
    let mut saved = get(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot).await.unwrap();
    saved["StartTimestamp"] = json!(now() - 300);
    saved["FinishTimestamp"] = json!(now() - 1);
    put(&mut s.db.acquire().await.unwrap(), a, "dispatch", slot, &saved).await.unwrap();
    let result = call(&s, &u, "dispatch/request_complete_dispatch", &format!("SlotIndex={slot}")).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["DispatchBattleInfo"]["WinCount"], 2);
    assert_eq!(result["DispatchBattleInfo"]["LoseCount"], 2);
    assert_eq!(stamina(&s, &u).await, 1000 - n(&job, "Cost") / 2);
    assert_ne!(call(&s, &u, "dispatch/request_complete_dispatch", &format!("SlotIndex={slot}")).await["Result"], "Success");
}
#[tokio::test]
async fn legend_costumes_discount_dispatch_stamina_like_the_native_window() {
    let (s, u) = fixture(false).await;
    let a = account(&u);
    let d = s.tables.battle.find("CampaignDungeon", &[("ChapterIndex", 1), ("DungeonIndex", 1)]).unwrap();
    let per_run = campaign::stamina_cost(d, 1);
    // Five Legend costumes save 2% each; an ordinary costume and a Legend
    // costume without the booster add nothing.
    for costume in [1212, 4515, 10010, 2519, 1112, 2520, 6808] {
        crate::db::query("INSERT INTO costumes(account_id,costume_index) VALUES($1,$2)").bind(a).bind(costume).execute(&s.db).await.unwrap();
    }
    assert_eq!(bonuses::dispatch_stamina_discount(&mut s.db.acquire().await.unwrap(), &s, a).await.unwrap(), 10);
    let job = start(&s, &u, 10).await;
    let charged = per_run * 90 / 100 * 10;
    assert!(charged < per_run * 10, "the stage must be expensive enough to show the discount");
    assert_eq!(n(&job, "StaminaDiscountRate"), 10);
    assert_eq!(n(&job, "Cost"), charged);
    assert_eq!(stamina(&s, &u).await, 1000 - charged);
}
