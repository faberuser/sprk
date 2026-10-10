use super::tests::{account, call, setup, unlock_godking};
use super::*;
async fn enable(s: &AppState, a: i64) {
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "entry_policy",
        0,
        &json!({"RefundPveDefeats":true}),
    )
    .await
    .unwrap();
}
async fn balance(s: &AppState, a: i64, column: &str) -> i64 {
    sqlx::query_scalar(&format!(
        "SELECT {column} FROM user_info WHERE account_id=?"
    ))
    .bind(a)
    .fetch_one(&s.db)
    .await
    .unwrap()
}
#[tokio::test]
async fn pve_only_victory_deducts_once_and_awards_stamina_exp() {
    let (mut s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).rules
        ["RaiderExpPerStamina"] = json!(1);
    sqlx::query("UPDATE user_info SET stamina=100,team_level=1,team_exp=0 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let entry = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    let first = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    let cost = campaign::stamina_cost(
        campaign::dungeon(&s, &Request::parse(entry.as_bytes()).unwrap()).unwrap(),
        1,
    );
    assert_eq!(first["StaminaResult"]["AddValue"], 0);
    assert!(cost > 0);
    assert_eq!(balance(&s, a, "team_exp").await, 0);
    assert_eq!(call(&s, &u, "campaign/begin_campaign", entry).await, first);
    let wrong = call(
        &s,
        &u,
        "campaign/end_campaign",
        "ChapterIndex=1&DungeonIndex=2&DungeonDifficulty=1&Completed=false",
    )
    .await;
    assert_ne!(wrong["Result"], "Success");
    assert_eq!(balance(&s, a, "stamina").await, 100);
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=false&Star=0"),
    )
    .await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    assert_eq!(loss["StaminaResult"]["AddValue"], 0);
    assert_eq!(balance(&s, a, "stamina").await, 100);
    assert_eq!(balance(&s, a, "team_exp").await, 0);
    assert_ne!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{entry}&Completed=false")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, a, "stamina").await, 100);
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let won = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]"),
    )
    .await;
    assert_eq!(won["Result"], "Success", "{won}");
    assert_eq!(balance(&s, a, "stamina").await, 100 - cost);
    assert_eq!(won["ExpResult2"]["AddValue"], cost);
}
#[tokio::test]
async fn tower_defeat_keeps_ticket_and_legacy_refund_can_exceed_cap() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    let entry="ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=21&TowerFloor=1";
    let begin = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=false"),
    )
    .await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    assert_eq!(loss["StaminaResult"]["Type"], "ChallengeTowerKey");
    assert_eq!(
        n(&loss["StaminaResult"], "NewValue"),
        n(&begin["StaminaResult"], "NewValue")
    );
    let mut tx = s.db.begin().await.unwrap();
    put(&mut tx, a, "key", 12, &json!({"Count":2,"Day":day()}))
        .await
        .unwrap();
    let mut receipt = json!({"VictoryEntryCosts":[]});
    entry_costs::charge(&mut tx, &s, a, &mut receipt, 12, 1)
        .await
        .unwrap();
    put(&mut tx, a, "key", 12, &json!({"Count":2,"Day":day()}))
        .await
        .unwrap();
    let mut out = json!({});
    entry_costs::settle(&mut tx, &s, a, &mut receipt, false, &mut out)
        .await
        .unwrap();
    assert_eq!(out["StaminaResult"]["NewValue"], 3);
    entry_costs::settle(&mut tx, &s, a, &mut receipt, false, &mut out)
        .await
        .unwrap();
    assert_eq!(
        crate::api::account::stamina::snapshot(&mut tx, &s, a, 12)
            .await
            .unwrap()["NewValue"],
        3
    );
    tx.commit().await.unwrap();
}
#[tokio::test]
async fn technomagic_lobby_withdrawal_releases_hold_and_retries_keep_balance() {
    for (index, dungeon) in [(501, 4001), (502, 4101)] {
        let (s, u) = setup().await;
        let a = account(&u);
        enable(&s, a).await;
        sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?")
            .bind(a)
            .execute(&s.db)
            .await
            .unwrap();
        for (c, d) in [(97, 5), (10, 9)] {
            put(
                &mut s.db.acquire().await.unwrap(),
                a,
                "dungeon",
                campaign::key(c, d),
                &json!({"FirstRewardedDiff":2,"MaxStar":13}),
            )
            .await
            .unwrap();
        }
        let entry=format!("ChapterIndex=10&DungeonIndex={dungeon}&DungeonDifficulty=0&RaidIndex={index}&RaidLevel=1&HeroIndices=[1]");
        let first = call(&s, &u, "campaign/begin_campaign", &entry).await;
        assert_eq!(first["Result"], "Success", "{first}");
        assert_eq!(call(&s, &u, "campaign/begin_campaign", &entry).await, first);
        let session = u["UserInfo"]["SessionKey"].as_str().unwrap();
        abandon_local_run_on_lobby(&s, a, session)
            .await
            .unwrap();
        abandon_local_run_on_lobby(&s, a, session)
            .await
            .unwrap();
        assert_eq!(
            get(&mut s.db.acquire().await.unwrap(), a, "key", 29)
                .await
                .unwrap()["Count"],
            5
        );
        assert_eq!(
            call(&s, &u, "campaign/begin_campaign", &entry).await["StaminaResult"]["NewValue"],
            5
        );
        let loss = call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{entry}&Completed=false"),
        )
        .await;
        assert_eq!(loss["StaminaResult"]["NewValue"], 5, "{loss}");
    }
}
#[tokio::test]
async fn godking_opening_cost_is_kept_on_defeat_with_pve_refunds_enabled() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    unlock_godking(&s, &u).await;
    let open = call(
        &s,
        &u,
        "godking_trial/open_godking_trial_dungeon",
        "ChapterIndex=100000",
    )
    .await;
    assert_eq!(open["StaminaResult"]["NewValue"], 1);
    let entry = "ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]";
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=false"),
    )
    .await;
    assert!(loss["StaminaResult"].is_null(), "{loss}");
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "godking", 100000)
            .await
            .unwrap()["IsOpen"],
        1
    );
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "key", 21)
            .await
            .unwrap()["Count"],
        1
    );
    let retry = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(retry["Result"], "Success", "{retry}");
    assert!(retry["StaminaResult"].is_null(), "{retry}");
    let loss_again = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=false"),
    )
    .await;
    assert_eq!(loss_again["Result"], "Success");
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "key", 21)
            .await
            .unwrap()["Count"],
        1
    );
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let won = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]"),
    )
    .await;
    assert_eq!(won["Result"], "Success", "{won}");
    assert_eq!(won["GodkingTrialDungeonInfo"]["IsOpen"], 0);
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "key", 21)
            .await
            .unwrap()["Count"],
        1
    );
}
#[tokio::test]
async fn score_modes_never_receive_refundable_cost_receipts() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    for kind in [4, 15, 16, 19, 20, 25, 26, 30, 32, 36, 43, 44, 48] {
        let Some(d) = s
            .tables
            .battle
            .rows("CampaignDungeon")
            .iter()
            .find(|v| n(v, "BattleType") == kind)
        else {
            continue;
        };
        let r = Request::parse(
            format!(
                "ChapterIndex={}&DungeonIndex={}&DungeonDifficulty=0",
                n(d, "ChapterIndex"),
                n(d, "DungeonIndex")
            )
            .as_bytes(),
        )
        .unwrap();
        let mut receipt = json!({});
        entry_costs::prepare(
            &mut s.db.acquire().await.unwrap(),
            &s,
            a,
            &r,
            &mut receipt,
            &mut json!({}),
        )
        .await
        .unwrap();
        assert!(receipt["VictoryEntryCosts"].is_null(), "mode {kind}");
    }
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    sqlx::query("UPDATE user_info SET world_boss_ticket=2").execute(&s.db).await.unwrap();
    let request =
        "ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&WorldBossIndex=1";
    let begin = call(&s, &u, "campaign/begin_campaign", request).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    let end = call(
        &s,
        &u,
        "campaign/end_campaign",
        "ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&Completed=false&TotalDamage=1000",
    )
    .await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_eq!(balance(&s, a, "world_boss_ticket").await, 1);
}

#[tokio::test]
async fn shakmeh_losses_keep_devourer_keys_and_otherworldly_gauge() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    for (c, d) in [(97, 2), (50000, 601)] {
        put(
            &mut s.db.acquire().await.unwrap(),
            a,
            "dungeon",
            campaign::key(c, d),
            &json!({"FirstRewardedDiff":1,"ScenarioComplete":1}),
        )
        .await
        .unwrap();
    }
    let devourer = "ChapterIndex=50000&DungeonIndex=501&DungeonDifficulty=0&HeroIndices=[1]";
    let begun = call(&s, &u, "campaign/begin_campaign", devourer).await;
    assert_eq!(begun["Result"], "Success", "{begun}");
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{devourer}&Completed=false"),
    )
    .await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    assert_eq!(loss["StaminaResult"]["NewValue"], 3);
    sqlx::query("INSERT INTO battle_currencies(account,kind,value) VALUES(?,'ShakmehMiddleBossPoint',600) ON CONFLICT(account,kind) DO UPDATE SET value=600").bind(a).execute(&s.db).await.unwrap();
    let final_boss = "ChapterIndex=50000&DungeonIndex=602&DungeonDifficulty=0&HeroIndices=[1]";
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", final_boss).await["Result"],
        "Success"
    );
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{final_boss}&Completed=false"),
    )
    .await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    assert_eq!(
        loss["ReservedCurrencyResults"]
            .as_array()
            .unwrap()
            .last()
            .unwrap()["NewValue"],
        600
    );
    assert_eq!(
        sqlx::query_scalar::<_, i64>(
            "SELECT value FROM battle_currencies WHERE account=? AND kind='ShakmehMiddleBossPoint'"
        )
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap(),
        600
    );
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", final_boss).await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{final_boss}&Completed=true&Star=3&AliveHeroIndices=[1]")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        sqlx::query_scalar::<_, i64>(
            "SELECT value FROM battle_currencies WHERE account=? AND kind='ShakmehMiddleBossPoint'"
        )
        .bind(a)
        .fetch_one(&s.db)
        .await
        .unwrap(),
        0
    );
}
#[tokio::test]
async fn apocalypsion_opening_stamina_is_deducted_on_first_victory() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "dungeon",
        campaign::key(11, 10),
        &json!({"FirstRewardedDiff":1}),
    )
    .await
    .unwrap();
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    sqlx::query("UPDATE user_info SET stamina=50000 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let open = "GroupIndex=101001&Level=1&DungeonType=2";
    assert_eq!(
        call(&s, &u, "punishment_raid/open_punishment_raid", open).await["Result"],
        "Success"
    );
    let entry="ChapterIndex=70000&DungeonIndex=1&DungeonDifficulty=0&GroupIndex=101001&DungeonType=2&Level=1&HeroIndices=[1]";
    assert_eq!(
        call(&s, &u, "contents/begin_content", entry).await["Result"],
        "Success"
    );
    let loss = call(
        &s,
        &u,
        "contents/end_content",
        &format!("{entry}&Completed=false"),
    )
    .await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    assert_eq!(loss["StaminaResult"]["AddValue"], 0);
    assert_eq!(balance(&s, a, "stamina").await, 50000);
    let retry = call(&s, &u, "contents/begin_content", entry).await;
    assert_eq!(balance(&s, a, "stamina").await, 50000, "{retry}");
    let won = call(
        &s,
        &u,
        "contents/end_content",
        &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]"),
    )
    .await;
    assert_eq!(won["Result"], "Success", "{won}");
    assert_eq!(balance(&s, a, "stamina").await, 44000);
    let second = entry.replace("DungeonIndex=1", "DungeonIndex=2");
    assert_eq!(
        call(&s, &u, "contents/begin_content", &second).await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "contents/end_content",
            &format!("{second}&Completed=false")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, a, "stamina").await, 44000);
}

#[tokio::test]
async fn expired_local_run_releases_hold_before_a_replacement_reserves_cost() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    let entry = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    let before = balance(&s, a, "stamina").await;
    let first = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(first["Result"], "Success");
    sqlx::query("UPDATE battle_runs SET started=? WHERE account=?")
        .bind(now() - settings(&s, "BattleExpirySeconds", 14400) - 1)
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let replacement = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(replacement["Result"], "Success", "{replacement}");
    assert_ne!(replacement["RunId"], first["RunId"]);
    assert_eq!(
        replacement["StaminaResult"]["NewValue"],
        first["StaminaResult"]["NewValue"]
    );
    let loss = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=false"),
    )
    .await;
    assert_eq!(loss["Result"], "Success");
    assert_eq!(balance(&s, a, "stamina").await, before);
}

#[tokio::test]
async fn legacy_refunded_godking_gate_is_paid_once_and_receipts_cannot_refund_it() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    unlock_godking(&s, &u).await;
    put(&mut s.db.acquire().await.unwrap(),a,"godking",100000,
        &json!({"ChapterIndex":100000,"Day":"2020-01-01","IsOpen":1,"IsOpened":true,"RunVersion":2,
            "VictoryRefund":true,"EntryPaid":false,"CostSettled":false,"EntryCostType":21,"EntryCost":1})).await.unwrap();
    let entry = "ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]";
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let mut tx = s.db.begin().await.unwrap();
    let gate = get(&mut tx, a, "godking", 100000).await.unwrap();
    assert!(gate["VictoryRefund"].is_null());
    assert_eq!(gate["IsOpen"], 1);
    assert_eq!(get(&mut tx, a, "key", 21).await.unwrap()["Count"], 1);
    let mut legacy = json!({"ChapterIndex":100000,"VictoryEntryCosts":[{"Family":"Stamina","Kind":21,"Amount":1}],"EntryCostGate":{"State":"godking","Index":100000}});
    entry_costs::settle(&mut tx, &s, a, &mut legacy, false, &mut json!({}))
        .await
        .unwrap();
    assert_eq!(get(&mut tx, a, "key", 21).await.unwrap()["Count"], 1);
    assert_eq!(get(&mut tx, a, "godking", 100000).await.unwrap(), gate);
    tx.commit().await.unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{entry}&Completed=false")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        get(&mut s.db.acquire().await.unwrap(), a, "key", 21)
            .await
            .unwrap()["Count"],
        1
    );
}

#[tokio::test]
async fn quit_and_relogin_release_entry_without_spending_then_win_charges_once() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    let entry = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
    let before = balance(&s, a, "stamina").await;
    let begin = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(begin["Result"], "Success");
    assert_eq!(balance(&s, a, "stamina").await, before);
    assert!(
        entry_costs::held(&mut s.db.acquire().await.unwrap(), a, "Stamina", 1)
            .await
            .unwrap()
            > 0
    );
    // Recreate application state against the persisted database, then log in.
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
    assert_eq!(balance(&restarted, a, "stamina").await, before);
    assert_eq!(
        entry_costs::held(&mut s.db.acquire().await.unwrap(), a, "Stamina", 1)
            .await
            .unwrap(),
        0
    );
    assert_ne!(
        call(
            &restarted,
            &login,
            "campaign/end_campaign",
            &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]")
        )
        .await["Result"],
        "Success"
    );
    let next = call(&restarted, &login, "campaign/begin_campaign", entry).await;
    assert_eq!(next["Result"], "Success", "{next}");
    let win = call(
        &restarted,
        &login,
        "campaign/end_campaign",
        &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]"),
    )
    .await;
    assert_eq!(win["Result"], "Success", "{win}");
    let after = balance(&s, a, "stamina").await;
    assert!(after < before);
    assert_ne!(
        call(
            &restarted,
            &login,
            "campaign/end_campaign",
            &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, a, "stamina").await, after);
}
#[tokio::test]
async fn held_entries_protect_concurrent_spending_and_survive_state_restart() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    sqlx::query("UPDATE user_info SET stamina=10,gold=100,gem=100,pay_gem=50 WHERE account_id=?")
        .bind(a)
        .execute(&s.db)
        .await
        .unwrap();
    let mut db = s.db.acquire().await.unwrap();
    let mut receipt = entry_costs::deferred_receipt();
    entry_costs::charge(&mut db, &s, a, &mut receipt, 1, 7)
        .await
        .unwrap();
    entry_costs::charge_currency(&mut db, &s, a, &mut receipt, "Currency", 3, 80)
        .await
        .unwrap();
    entry_costs::charge_currency(&mut db, &s, a, &mut receipt, "Currency", 4, 120)
        .await
        .unwrap();
    assert!(dungeons::charge_reserved(&mut db, &s, a, 1, 4)
        .await
        .is_err());
    assert!(hero::currency(&mut db, a, "Gold", -21).await.is_err());
    assert!(hero::currency(&mut db, a, "Gem", -31).await.is_err());
    assert_eq!(
        entry_costs::held(&mut db, a, "Stamina", 1).await.unwrap(),
        7
    );
    drop(db);
    let restarted = AppState::new(s.db.clone(), s.tables.as_ref().clone());
    let mut db = restarted.db.acquire().await.unwrap();
    let mut out = json!({});
    entry_costs::settle(&mut db, &restarted, a, &mut receipt, true, &mut out)
        .await
        .unwrap();
    assert_eq!(out["StaminaResult"]["NewValue"], 3);
    assert_eq!(
        hero::currency(&mut db, a, "Gold", 0).await.unwrap()["NewValue"],
        20
    );
    assert_eq!(
        hero::currency(&mut db, a, "Gem", 0).await.unwrap()["NewValue"],
        30
    );
    entry_costs::settle(&mut db, &restarted, a, &mut receipt, true, &mut out)
        .await
        .unwrap();
    assert_eq!(
        hero::currency(&mut db, a, "Gold", 0).await.unwrap()["NewValue"],
        20
    );
    assert_eq!(
        entry_costs::held(&mut db, a, "Stamina", 1).await.unwrap(),
        0
    );
}
#[tokio::test]
async fn dispatch_keeps_balance_and_only_completed_wins_deduct_on_collection() {
    let (s, u) = setup().await;
    let a = account(&u);
    enable(&s, a).await;
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "dungeon",
        campaign::key(1, 1),
        &json!({"FirstRewardedDiff":2,"MaxStar":13}),
    )
    .await
    .unwrap();
    let args =
        "ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=2&DeckIndex=1";
    let before = balance(&s, a, "stamina").await;
    let start = call(&s, &u, "dispatch/start_dispatch", args).await;
    assert_eq!(start["Result"], "Success", "{start}");
    assert_eq!(balance(&s, a, "stamina").await, before);
    let slot = n(&start["DispatchBattleInfo"], "SlotIndex");
    let slot_args = format!("SlotIndex={slot}");
    let canceled = call(&s, &u, "dispatch/cancel_dispatch", &slot_args).await;
    assert_eq!(canceled["Result"], "Success", "{canceled}");
    assert_eq!(balance(&s, a, "stamina").await, before);
    assert_eq!(
        entry_costs::held(&mut s.db.acquire().await.unwrap(), a, "Stamina", 1)
            .await
            .unwrap(),
        0
    );
    let start = call(&s, &u, "dispatch/start_dispatch", args).await;
    let cost = n(&start["DispatchBattleInfo"], "Cost") / 2;
    super::tests::prepared_dispatch(&s,&u,slot,&[1,1]).await;
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut run = get(&mut db, a, "dispatch", slot).await.unwrap();
        run["StartTimestamp"] = json!(now() - settings(&s, "DispatchSecondsPerBattle", 60).max(1));
        put(&mut db, a, "dispatch", slot, &run).await.unwrap();
    }
    let canceled = call(&s, &u, "dispatch/cancel_dispatch", &slot_args).await;
    assert_eq!(canceled["Result"], "Success", "{canceled}");
    assert_eq!(canceled["DispatchBattleInfo"]["WinCount"], 1);
    assert_eq!(balance(&s, a, "stamina").await, before - cost);
    assert_ne!(
        call(&s, &u, "dispatch/cancel_dispatch", &slot_args).await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, a, "stamina").await, before - cost);
}

#[tokio::test]
async fn tower_currency_fee_stays_available_on_defeat_and_deducts_on_victory() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE user_info SET team_level=100").execute(&s.db).await.unwrap();
    let a = account(&u);
    enable(&s, a).await;
    let entry="ChapterIndex=3001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=1&TowerFloor=1";
    let before = balance(&s, a, "gold").await;
    let begin = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    assert_eq!(balance(&s, a, "gold").await, before);
    assert_eq!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &format!("{entry}&Completed=false")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, a, "gold").await, before);
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let won = call(
        &s,
        &u,
        "campaign/end_campaign",
        &format!("{entry}&Completed=true&Star=3&AliveHeroIndices=[1]"),
    )
    .await;
    assert_eq!(won["Result"], "Success", "{won}");
    assert!(won["CurrencyResults2"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v["CurrencyType"] == "Gold" && v["AddValue"] == -5000));
    assert_eq!(
        entry_costs::held(&mut s.db.acquire().await.unwrap(), a, "Currency", 3)
            .await
            .unwrap(),
        0
    );
}
