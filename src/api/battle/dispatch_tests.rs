use super::tests::{account, call, setup};
use super::*;
async fn fixture(deferred: bool) -> (AppState, Value) {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?")
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
async fn technomagic_dispatch_preserves_entry_gates_and_charges_and_rewards_only_wins() {
    let (s,u)=fixture(true).await;
    let a=account(&u);
    for (index,dungeon) in [(301,2001),(302,2101),(303,2201)] {
        let args=format!("ChapterIndex=10&DungeonIndex={dungeon}&Difficulty=0&HeroIndices=[1]&RepeatCount=10&DeckIndex=1");
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        for (chapter,required) in [(97,5),(10,9),(10,30)] {
            let diff=s.tables.tutorials.dungeon_difficulty(chapter,required);
            put(&mut *s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(chapter as i64,required as i64),&json!({"FirstRewardedDiff":1<<diff})).await.unwrap();
        }
        // Having the story prerequisite is not enough: this raid stage must be cleared.
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        put(&mut *s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(10,dungeon),&json!({"FirstRewardedDiff":1})).await.unwrap();
        sqlx::query("UPDATE heroes SET level=59 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        assert_eq!(stamina(&s,&u).await,1000);
        sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        sqlx::query("UPDATE user_info SET stamina=999 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        let started=call(&s,&u,"dispatch/start_dispatch",&format!("{args}&RaidIndex=999&RaidLevel=999")).await;
        assert_eq!(started["Result"],"Success","{started}");
        let job=&started["DispatchBattleInfo"];
        assert_eq!(job["Request"]["RaidIndex"],index.to_string());
        assert_eq!(job["Request"]["RaidLevel"],"1");
        assert_eq!(job["Cost"],1000);
        assert_eq!(stamina(&s,&u).await,1000);
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        assert_eq!(call(&s,&u,"dispatch/complete_calculate_dispatch_result",&report(job,&[1,0,1,0,0,1,0,0,0,0])).await["Result"],"Success");
        finish_now(&s,&u,n(job,"SlotIndex")).await;
        let mail_before:i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
        let collect=format!("SlotIndex={}",n(job,"SlotIndex"));
        let done=call(&s,&u,"dispatch/request_complete_dispatch",&collect).await;
        assert_eq!(done["Result"],"Success","{done}");
        assert_eq!(done["DispatchBattleInfo"]["WinCount"],3);
        assert_eq!(done["DispatchBattleInfo"]["LoseCount"],7);
        assert_eq!(stamina(&s,&u).await,700);
        assert_eq!(done["TeamExpResult"]["AddValue"],60000);
        assert_eq!(done["EquipItemResults"],json!([]));
        let mail_after:i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
        assert!(mail_after>mail_before || done["ItemResults"].as_array().is_some_and(|items|!items.is_empty()),"raid rewards were not delivered: {done}");
        assert_ne!(call(&s,&u,"dispatch/request_complete_dispatch",&collect).await["Result"],"Success");
        assert_eq!(stamina(&s,&u).await,700);
        assert!(call(&s,&u,"dispatch/get_dispatch_list","").await["DispatchBattleInfos"].as_array().unwrap().is_empty());
        sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    }
}
async fn stamina(s: &AppState, u: &Value) -> i64 {
    sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?")
        .bind(account(u))
        .fetch_one(&s.db)
        .await
        .unwrap()
}
async fn start(s: &AppState, u: &Value, count: i64) -> Value {
    let v=call(s,u,"dispatch/start_dispatch",&format!("ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount={count}&DeckIndex=1")).await;
    assert_eq!(v["Result"], "Success", "{v}");
    v["DispatchBattleInfo"].clone()
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
            &mut *s.db.acquire().await.unwrap(),
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
            campaign::progress(&mut *s.db.acquire().await.unwrap(), &s, account(&u), 1, 1)
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
    assert_eq!(done["TeamExpResult"]["AddValue"], 2 * cost * 200);
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
