use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{path::Path, sync::OnceLock};
async fn setup() -> (AppState, Value) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let s = AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    let u = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    (s, u)
}
async fn call(s: &AppState, u: &Value, path: &str, args: &str) -> Value {
    execute_request(
        s,
        path,
        Bytes::from(format!(
            "SessionKey={}&{args}",
            u["UserInfo"]["SessionKey"].as_str().unwrap()
        )),
    )
    .await
    .unwrap()
}
fn account(u: &Value) -> i64 {
    n(&u["UserInfo"], "AccountId")
}
const ENTRY: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
const END: &str =
    "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&Star=3&AliveHeroIndices=[1]";

#[tokio::test]
async fn unity_repeated_form_fields_preserve_the_campaign_party() {
    let (s, u) = setup().await;
    for index in 2..=4 {
        hero::recruit_at(&mut *s.db.acquire().await.unwrap(), &s, account(&u), index, 1, 1, 0)
            .await.unwrap();
    }
    let entry = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=1&HeroIndices=2&HeroIndices=3&HeroIndices=4&LeaderHeroIndex=1&ScenarioDungeon=False&Repeat=False";
    let duplicate = entry.replace("HeroIndices=4", "HeroIndices=1");
    let stamina = balance(&s, "stamina").await;
    assert_ne!(call(&s, &u, "campaign/begin_campaign", &duplicate).await["Result"], "Success");
    assert_eq!(balance(&s, "stamina").await, stamina);
    let begin = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    let saved: String = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?")
        .bind(account(&u)).fetch_one(&s.db).await.unwrap();
    assert_eq!(read_json::<Value>(&saved).unwrap()["Heroes"], json!([1,2,3,4]));
    let end = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=True&Star=13&AliveHeroIndices=1&AliveHeroIndices=2&AliveHeroIndices=3&AliveHeroIndices=4";
    let result = call(&s, &u, "campaign/end_campaign", end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["HeroExpResults"].as_array().unwrap().len(), 4);
    let progress = campaign::progress(&mut *s.db.acquire().await.unwrap(), &s, account(&u), 1, 1).await.unwrap();
    assert_eq!(progress["MaxStar"], 13);
    let single = call(&s, &u, "campaign/begin_campaign", &ENTRY.replace("[1]", "1")).await;
    assert_eq!(single["Result"], "Success", "{single}");
}

#[tokio::test]
async fn native_string_hero_ids_can_enter_and_complete_campaign() {
    let (s, u) = setup().await;
    let entry = ENTRY.replace("HeroIndices=[1]", "HeroIndices=[\"1\"]&GroupHeroIndices=[]&LeaderHeroIndex=1");
    let begin = call(&s, &u, "campaign/begin_campaign", &entry).await;
    assert_eq!(begin["Result"], "Success", "{begin}");
    // Numeric and native string representations identify the same retry.
    assert_eq!(call(&s, &u, "campaign/begin_campaign", ENTRY).await, begin);
    let end = END.replace("AliveHeroIndices=[1]", "AliveHeroIndices=[\"1\"]");
    let result = call(&s, &u, "campaign/end_campaign", &end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["HeroExpResults"][0]["HeroIndex"], 1);
}

#[test]
fn campaign_star_encoding_matches_difficulty() {
    for diff in 0..=3 {
        for stars in 1..=3 {
            let request = Request::parse(format!("DungeonDifficulty={diff}&Star={}", diff * 10 + stars).as_bytes()).unwrap();
            assert_eq!(campaign::result_star(&request, true).unwrap(), stars);
        }
    }
    for raw in [-1, 0, 4, 10, 14, 23, 33, 43, i64::MAX] {
        let request = Request::parse(format!("DungeonDifficulty=1&Star={raw}").as_bytes()).unwrap();
        assert!(campaign::result_star(&request, true).is_err(), "{raw}");
    }
    let loss = Request::parse(b"DungeonDifficulty=1&Star=0").unwrap();
    assert_eq!(campaign::result_star(&loss, false).unwrap(), 0);
}

#[test]
fn native_hero_ids_still_reject_invalid_and_duplicate_values() {
    for value in [r#"["1",1]"#, r#"["0"]"#, r#"["-1"]"#, r#"["2147483648"]"#, r#"["bad"]"#, "[true]", "[1.5]", "[null]"] {
        let request = Request::parse(format!("HeroIndices={value}").as_bytes()).unwrap();
        assert!(ids(&request, "HeroIndices", 32).is_err(), "{value}");
    }
}

#[tokio::test]
async fn world_boss_hp_scores_and_tickets_change_once_per_entered_battle() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE heroes SET level=60 WHERE account_id=? AND hero_index=1")
        .bind(account(&u))
        .execute(&s.db)
        .await
        .unwrap();
    let request =
        "ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&WorldBossIndex=1";
    let first = call(&s, &u, "campaign/begin_campaign", request).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["StaminaResult"]["Type"], "WorldBossTicket");
    assert_eq!(first["StaminaResult"]["AddValue"], -1);
    let again = call(&s, &u, "campaign/begin_campaign", request).await;
    assert_eq!(first, again);
    let finish =
        "ChapterIndex=7001&DungeonIndex=1&DungeonDifficulty=0&Completed=false&TotalDamage=1000";
    let end = call(&s, &u, "campaign/end_campaign", finish).await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_eq!(
        n(&first["WorldBossInfo"], "MonsterHp0") - n(&end["WorldBossInfo"], "MonsterHp0"),
        1000
    );
    assert_ne!(
        call(&s, &u, "campaign/end_campaign", finish).await["Result"],
        "Success"
    );
    let score: i64 = sqlx::query_scalar(
        "SELECT SUM(score) FROM battle_scores WHERE account=? AND family='world_boss'",
    )
    .bind(account(&u))
    .fetch_one(&s.db)
    .await
    .unwrap();
    assert_eq!(score, 1000);
    assert_eq!(balance(&s, "world_boss_ticket").await, 1);
}
async fn balance(s: &AppState, col: &str) -> i64 {
    sqlx::query_scalar(&format!("SELECT {col} FROM user_info LIMIT 1"))
        .fetch_one(&s.db)
        .await
        .unwrap()
}

#[tokio::test]
async fn shakmeh_gauge_caps_passives_persist_and_final_entry_charges_once() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query(
        "INSERT INTO battle_currencies(account,kind,value) VALUES(?,'ShakmehMiddleBossPoint',595)",
    )
    .bind(a)
    .execute(&s.db)
    .await
    .unwrap();
    let start = "ChapterIndex=50000&DungeonIndex=501&DungeonDifficulty=0&HeroIndices=[1]";
    let end="ChapterIndex=50000&DungeonIndex=501&DungeonDifficulty=0&Completed=true&AliveHeroIndices=[1]";
    let entered = call(&s, &u, "campaign/begin_campaign", start).await;
    assert_eq!(entered["Result"], "Success", "{entered}");
    let completed = call(&s, &u, "campaign/end_campaign", end).await;
    assert_eq!(completed["Result"], "Success", "{completed}");
    let points = completed["CurrencyResults"]
        .as_array()
        .unwrap()
        .iter()
        .find(|v| v["CurrencyType"] == "ShakmehMiddleBossPoint")
        .unwrap();
    assert_eq!(points["NewValue"], 600);
    assert_eq!(points["AddValue"], 5);
    let passive = call(&s, &u, "shakmeh_dungeon/get_passive_info", "").await;
    assert_eq!(passive["PassiveInfos"].as_array().unwrap().len(), 1);
    assert_ne!(
        call(&s, &u, "campaign/begin_campaign", start).await["Result"],
        "Success"
    );
    let login = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    assert_eq!(
        login["MiscInfo"]["ShakemehPassiveInfos"],
        passive["PassiveInfos"]
    );
    let final_entry = "ChapterIndex=50000&DungeonIndex=601&DungeonDifficulty=0&HeroIndices=[1]";
    let first = call(&s, &login, "campaign/begin_campaign", final_entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(
        call(&s, &login, "campaign/begin_campaign", final_entry).await,
        first
    );
    let gauge: i64 = sqlx::query_scalar(
        "SELECT value FROM battle_currencies WHERE account=? AND kind='ShakmehMiddleBossPoint'",
    )
    .bind(a)
    .fetch_one(&s.db)
    .await
    .unwrap();
    assert_eq!(gauge, 0);
    assert_eq!(
        call(&s, &login, "shakmeh_dungeon/get_passive_info", "").await["PassiveInfos"],
        passive["PassiveInfos"]
    );
    let result = call(
        &s,
        &login,
        "campaign/end_campaign",
        "ChapterIndex=50000&DungeonIndex=601&DungeonDifficulty=0&Completed=false",
    )
    .await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(
        call(&s, &login, "shakmeh_dungeon/get_passive_info", "").await["PassiveInfos"],
        json!([])
    );
}

#[tokio::test]
async fn tower_party_rules_and_recovery_reject_forged_creatures() {
    let (s, u) = setup().await;
    let invalid=call(&s,&u,"campaign/begin_campaign","ChapterIndex=9001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=1001&TowerFloor=1").await;
    assert_ne!(invalid["Result"], "Success");
    let entry="ChapterIndex=3001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=1&TowerFloor=1";
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    let creature = |id, hp, max| json!({"Index":id,"Key":format!("{id}_0_0"),"TeamId":0,"Hp":hp,"MaxHp":max,"Mp":0,"MaxMp":1000});
    let args = |v: Value| {
        format!(
            "ChapterIndex=3001&DungeonIndex=1&DungeonDifficulty=0&Completed=false&{}",
            serde_urlencoded::to_string([("CreatureInfoString", v.to_string())]).unwrap()
        )
    };
    for invalid in [
        json!([creature(2, 1, 100)]),
        json!([creature(1, 101, 100)]),
        json!([creature(1, 1, 100), creature(1, 1, 100)]),
    ] {
        assert_ne!(
            call(&s, &u, "campaign/end_campaign", &args(invalid)).await["Result"],
            "Success"
        );
    }
    assert_eq!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &args(json!([creature(1, 40, 100)]))
        )
        .await["Result"],
        "Success"
    );
    let next = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(next["TowerIntialUserHeroes"][0]["Hp"], 40);
    let gold = balance(&s, "gold").await;
    assert_ne!(
        call(&s, &u, "campaign/reset_tower", "TowerIndex=1").await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "gold").await, gold);
    assert_eq!(
        call(
            &s,
            &u,
            "campaign/end_campaign",
            &args(json!([creature(1, 0, 100)]))
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "campaign/begin_campaign", entry).await["Result"],
        "Success"
    );
    sqlx::query("UPDATE user_info SET gold=1000000 WHERE account_id=?")
        .bind(account(&u))
        .execute(&s.db)
        .await
        .unwrap();
    let reset = call(&s, &u, "campaign/reset_tower", "TowerIndex=1").await;
    assert_eq!(reset["Result"], "Success", "{reset}");
    assert_eq!(balance(&s, "gold").await, 900000);
    assert_ne!(
        call(&s, &u, "campaign/reset_tower", "TowerIndex=1").await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "gold").await, 900000);
    let start = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(start["Result"], "Success", "{start}");
    assert_eq!(start["TowerIntialUserHeroes"], json!([]));
    assert_eq!(start["TowerIntialEnemyHeroes"], json!([]));
}

#[tokio::test]
async fn tower_npc_snapshots_follow_enabled_flag_and_survive_relogin() {
    let (mut s, u) = setup().await;
    assert_eq!(
        call(&s, &u, "campaign/get_tower_npc_info", "TowerIndex=11").await["TowerNpcInfos"],
        json!([])
    );
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
        .tables
        .get_mut("Tower")
        .unwrap()
        .iter_mut()
        .find(|v| n(v, "TowerIndex") == 11)
        .unwrap()["UseNpc"] = json!(true);
    let first = call(&s, &u, "campaign/get_tower_npc_info", "TowerIndex=11").await;
    assert_eq!(first["Result"], "Success", "{first}");
    let floors = s
        .tables
        .battle
        .rows("TowerFloor")
        .iter()
        .filter(|f| n(f, "TowerIndex") == 11)
        .map(|f| n(f, "Floor"))
        .max()
        .unwrap() as usize;
    assert_eq!(first["TowerNpcInfos"].as_array().unwrap().len(), floors);
    assert!(
        first["TowerNpcInfos"][0]["HeroInfos"]
            .as_object()
            .unwrap()
            .len()
            > 0
    );
    sqlx::query("UPDATE heroes SET level=40 WHERE account_id=?")
        .bind(account(&u))
        .execute(&s.db)
        .await
        .unwrap();
    let login = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    assert_eq!(
        call(&s, &login, "campaign/get_tower_npc_info", "TowerIndex=11").await,
        first
    );
}

#[tokio::test]
async fn missing_punishment_raid_definitions_do_not_spend_opening_keys() {
    let (s, u) = setup().await;
    let before = call(&s, &u, "punishment_raid/get_punishment_raid_info", "").await;
    let result = call(
        &s,
        &u,
        "punishment_raid/open_punishment_raid",
        "GroupIndex=101001&Level=1&DungeonType=0",
    )
    .await;
    assert_ne!(result["Result"], "Success");
    let after = call(&s, &u, "punishment_raid/get_punishment_raid_info", "").await;
    assert_eq!(
        before["StaminaResult"]["NewValue"],
        after["StaminaResult"]["NewValue"]
    );
}
#[tokio::test]
async fn campaign_requires_entry_and_rejects_forged_results_and_replays() {
    let (s, u) = setup().await;
    let gold = balance(&s, "gold").await;
    assert_ne!(
        call(&s, &u, "campaign/end_campaign", END).await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "gold").await, gold);
    let stamina = balance(&s, "stamina").await;
    let first = call(&s, &u, "campaign/begin_campaign", ENTRY).await;
    assert_eq!(first["Result"], "Success", "{first}");
    let repeat = call(&s, &u, "campaign/begin_campaign", ENTRY).await;
    assert_eq!(repeat, first);
    assert_eq!(balance(&s, "stamina").await, stamina - 6);
    for bad in [
        END.replace("DungeonIndex=1", "DungeonIndex=2"),
        END.replace("Star=3", "Star=4"),
        END.replace("[1]", "[2]"),
    ] {
        assert_ne!(
            call(&s, &u, "campaign/end_campaign", &bad).await["Result"],
            "Success"
        );
        assert_eq!(balance(&s, "gold").await, gold);
    }
    let end = call(&s, &u, "campaign/end_campaign", END).await;
    assert_eq!(end["Result"], "Success", "{end}");
    let gold = balance(&s, "gold").await;
    assert_ne!(
        call(&s, &u, "campaign/end_campaign", END).await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "gold").await, gold);
}
#[tokio::test]
async fn only_selected_heroes_receive_exp_and_entry_survives_login() {
    let (s, u) = setup().await;
    hero::recruit_at(
        &mut *s.db.acquire().await.unwrap(),
        &s,
        account(&u),
        2,
        1,
        1,
        0,
    )
    .await
    .unwrap();
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", ENTRY).await["Result"],
        "Success"
    );
    let new = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    let end = call(&s, &new, "campaign/end_campaign", END).await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_eq!(end["HeroExpResults"].as_array().unwrap().len(), 1);
    let untouched: i64 = sqlx::query_scalar("SELECT exp FROM heroes WHERE hero_index=2")
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(untouched, 0);
}
#[tokio::test]
async fn duplicate_party_and_foreign_heroes_do_not_charge() {
    let (s, u) = setup().await;
    let before = balance(&s, "stamina").await;
    for ids in ["[]", "[1,1]", "[99999]", "[-1]"] {
        assert_ne!(
            call(
                &s,
                &u,
                "campaign/begin_campaign",
                &ENTRY.replace("[1]", ids)
            )
            .await["Result"],
            "Success"
        );
    }
    assert_eq!(balance(&s, "stamina").await, before);
}
#[tokio::test]
async fn dispatch_enforces_time_party_reservation_and_single_collection() {
    let (s, u) = setup().await;
    call(&s, &u, "campaign/begin_campaign", ENTRY).await;
    call(&s, &u, "campaign/end_campaign", END).await;
    let r = call(
        &s,
        &u,
        "dispatch/start_dispatch",
        "ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=2&DeckIndex=1",
    )
    .await;
    assert_eq!(r["Result"], "Success", "{r}");
    let slot = n(&r["DispatchBattleInfo"], "SlotIndex");
    let args = format!("SlotIndex={slot}");
    assert_ne!(
        call(&s, &u, "dispatch/request_complete_dispatch", &args).await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "campaign/begin_campaign", ENTRY).await["Result"],
        "Success"
    );
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut v = get(&mut db, account(&u), "dispatch", slot).await.unwrap();
        v["FinishTimestamp"] = json!(now() - 1);
        put(&mut db, account(&u), "dispatch", slot, &v)
            .await
            .unwrap();
    }
    let r = call(&s, &u, "dispatch/request_complete_dispatch", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["DispatchBattleInfo"]["WinCount"], 2);
    let gold = balance(&s, "gold").await;
    assert_ne!(
        call(&s, &u, "dispatch/request_complete_dispatch", &args).await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "gold").await, gold);
}
#[tokio::test]
async fn party_rooms_enforce_membership_and_transfer_master() {
    let (s, u) = setup().await;
    let v = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=room-second"))
            .await
            .unwrap()
            .0
    );
    let w = json!(
        user::login(
            State(s.clone()),
            Bytes::from_static(b"LoginId=room-outsider")
        )
        .await
        .unwrap()
        .0
    );
    let created = call(
        &s,
        &u,
        "party_dungeon/create_party_dungeon_room",
        "DungeonType=1&ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=2&Opened=1",
    )
    .await;
    assert_eq!(created["Result"], "Success", "{created}");
    let id = n(&created, "RoomNo");
    let args = format!("RoomNo={id}");
    assert_eq!(
        call(&s, &v, "party_dungeon/join_party_dungeon_room", &args).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &w,
            "party_dungeon/leave_party_dungeon_room",
            &format!("{args}&AccountId={}", account(&u))
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &v,
            "party_dungeon/change_party_dungeon_room_info",
            &format!("{args}&Status=1")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "party_dungeon/leave_party_dungeon_room", &args).await["Result"],
        "Success"
    );
    let joined = call(&s, &v, "party_dungeon/join_party_dungeon_room", &args).await;
    assert_eq!(joined["JoinedRaidRoomInfo"]["MasterAccountId"], account(&v));
}
#[tokio::test]
async fn closed_multiplayer_requires_battle_service_without_spending() {
    let (s, u) = setup().await;
    let stamina = balance(&s, "stamina").await;
    let r = call(
        &s,
        &u,
        "campaign/begin_campaign",
        &format!("{ENTRY}&MultiplayMasterId={}", account(&u)),
    )
    .await;
    assert_ne!(r["Result"], "Success");
    assert_eq!(balance(&s, "stamina").await, stamina);
}
#[tokio::test]
async fn tower_cannot_skip_floors_or_double_claim() {
    let (s, u) = setup().await;
    let r=call(&s,&u,"campaign/begin_campaign","ChapterIndex=3001&DungeonIndex=2&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=1&TowerFloor=2").await;
    assert_ne!(r["Result"], "Success");
    let entry="ChapterIndex=3001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=1&TowerFloor=1";
    let r = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(r["Result"], "Success", "{r}");
    let r=call(&s,&u,"campaign/end_campaign","ChapterIndex=3001&DungeonIndex=1&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=[1]").await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["TowerInfos"][0]["CompletedFloor"], 1);
    assert_ne!(
        call(&s, &u, "campaign/receive_tower_reward", "TowerIndex=1").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn eclipse_decks_ignore_forged_hero_stats() {
    let (s, u) = setup().await;
    let heroes = json!([{"DeckIndex":1,"HeroIndices":"[1]","CachedHeroInfos":[{"HeroIndex":1,"Level":999}]}]);
    let encoded = serde_urlencoded::to_string([("HeroInfos", heroes.to_string())]).unwrap();
    let r = call(&s, &u, "eclipse/set_eclipse_deck", &encoded).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_eq!(r["DeckResults"][0]["CachedHeroInfos"][0]["Level"], 1);
    let r = call(&s, &u, "eclipse/get_eclipse_deck", "").await;
    assert_eq!(r["DeckResults"].as_array().unwrap().len(), 1);
}

#[tokio::test]
async fn scenario_cannot_unlock_unplayed_dungeon_and_clear_metric_counts_once() {
    let (s, u) = setup().await;
    assert_ne!(
        call(
            &s,
            &u,
            "campaign/complete_scenario_dungeon",
            "ChapterIndex=1&DungeonIndex=3"
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "campaign/begin_campaign",
            &ENTRY.replace("DungeonIndex=1", "DungeonIndex=3")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", ENTRY).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "campaign/end_campaign", END).await["Result"],
        "Success"
    );
    let before: i64 = sqlx::query_scalar(
        "SELECT clear_count FROM campaign_progress WHERE chapter_id=1 AND dungeon_id=1",
    )
    .fetch_one(&s.db)
    .await
    .unwrap();
    for _ in 0..2 {
        assert_eq!(
            call(
                &s,
                &u,
                "campaign/complete_scenario_dungeon",
                "ChapterIndex=1&DungeonIndex=1"
            )
            .await["Result"],
            "Success"
        );
    }
    let after: i64 = sqlx::query_scalar(
        "SELECT clear_count FROM campaign_progress WHERE chapter_id=1 AND dungeon_id=1",
    )
    .fetch_one(&s.db)
    .await
    .unwrap();
    assert_eq!(after, before);
}
#[tokio::test]
async fn world_boss_closed_day_and_season_send_mail_once() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',1,1,?,'2026-01-05',1000)").bind(a).execute(&s.db).await.unwrap();
    for _ in 0..2 {
        let result = call(&s, &u, "world_boss/get_world_boss_info", "").await;
        assert_eq!(result["Result"], "Success", "{result}");
    }
    let count:i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title IN ('World boss daily reward','Battle season ranking reward')").bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(count, 2);
    let rank = call(
        &s,
        &u,
        "world_boss/get_world_boss_rank_info",
        "WorldBossIndex=1&Season=1",
    )
    .await;
    assert_eq!(rank["RankInfo"]["Score"], 1000);
    let mail: i64 = sqlx::query_scalar(
        "SELECT mail_id FROM mails WHERE account_id=? AND title='Battle season ranking reward'",
    )
    .bind(a)
    .fetch_one(&s.db)
    .await
    .unwrap();
    let body = Bytes::from(format!(
        "SessionKey={}&MailIndex={mail}",
        u["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    for _ in 0..2 {
        let _ = crate::api::community::mail::receive_mail(State(s.clone()), body.clone())
            .await
            .unwrap();
    }
    let value: i64 = sqlx::query_scalar(
        "SELECT value FROM battle_currencies WHERE account=? AND kind='WorldBossPoint'",
    )
    .bind(a)
    .fetch_one(&s.db)
    .await
    .unwrap();
    assert_eq!(value, 2000);
    let login = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    assert!(login["PlayerCurrencyInfos"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v["CurrencyType"] == "WorldBossPoint" && v["Amount"] == 2000));
}

#[tokio::test]
async fn ordeal_opponents_events_and_battle_proof_follow_native_flow() {
    let (s, u) = setup().await;
    let selected = call(&s, &u, "ordeal_arena/select_tier", "Tier=0").await;
    assert_eq!(selected["Result"], "Success", "{selected}");
    let first = s
        .tables
        .battle
        .rows("OrdealArenaNode")
        .iter()
        .find(|v| n(v, "Floor") == 1)
        .unwrap();
    let event = call(
        &s,
        &u,
        "ordeal_arena/end_ordeal_arena",
        &format!(
            "NodeIndex={}&ChapterIndex={}&DungeonIndex={}&Completed=true",
            n(first, "Index"),
            n(first, "ChapterIndex"),
            n(first, "DungeonIndex")
        ),
    )
    .await;
    assert_eq!(event["Result"], "Success", "{event}");
    let first_buff = event["SelectableBuffIndices"][0].as_i64().unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "ordeal_arena/select_buff",
            &format!("BuffIndex={first_buff}")
        )
        .await["Result"],
        "Success"
    );
    let node = s
        .tables
        .battle
        .rows("OrdealArenaNode")
        .iter()
        .find(|v| n(v, "Floor") == 2 && n(v, "NodeType") == 2)
        .unwrap();
    let id = n(node, "Index");
    let c = n(node, "ChapterIndex");
    let d = n(node, "DungeonIndex");
    let npc = selected["OrdealNodeInfos"]
        .as_array()
        .unwrap()
        .iter()
        .find(|v| n(v, "Index") == id)
        .unwrap();
    assert!(!npc["AccountInfo"]["HeroInfos"]
        .as_object()
        .unwrap()
        .is_empty());
    let finish =
        format!("NodeIndex={id}&ChapterIndex={c}&DungeonIndex={d}&HeroIndices=[1]&Completed=true");
    assert_ne!(
        call(&s, &u, "ordeal_arena/end_ordeal_arena", &finish).await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "ordeal_arena/begin_ordeal_arena",
            &format!("NodeIndex={id}&HeroIndices=[1]")
        )
        .await["Result"],
        "Success"
    );
    let end = call(&s, &u, "ordeal_arena/end_ordeal_arena", &finish).await;
    assert_eq!(end["Result"], "Success", "{end}");
    assert_ne!(
        call(&s, &u, "ordeal_arena/end_ordeal_arena", &finish).await["Result"],
        "Success"
    );
    let buff = end["SelectableBuffIndices"][0].as_i64().unwrap();
    assert_eq!(
        call(
            &s,
            &u,
            "ordeal_arena/select_buff",
            &format!("BuffIndex={buff}")
        )
        .await["Result"],
        "Success"
    );
    let event = s
        .tables
        .battle
        .rows("OrdealArenaNode")
        .iter()
        .find(|v| n(v, "NodeType") == 1 && n(v, "Floor") > 2)
        .unwrap();
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut state = get(&mut db, account(&u), "ordeal", 0).await.unwrap();
        state["ClearNodeIndices"] = json!(s
            .tables
            .battle
            .rows("OrdealArenaNode")
            .iter()
            .filter(|v| n(v, "Floor") < n(event, "Floor"))
            .map(|v| n(v, "Index"))
            .collect::<Vec<_>>());
        put(&mut db, account(&u), "ordeal", 0, &state)
            .await
            .unwrap();
    }
    let event_args = format!(
        "NodeIndex={}&ChapterIndex={}&DungeonIndex={}&Completed=true",
        n(event, "Index"),
        n(event, "ChapterIndex"),
        n(event, "DungeonIndex")
    );
    let event = call(&s, &u, "ordeal_arena/end_ordeal_arena", &event_args).await;
    assert_eq!(event["Result"], "Success", "{event}");
    assert_ne!(
        call(&s, &u, "ordeal_arena/end_ordeal_arena", &event_args).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "match/get_season_info", "ArenaType=Normal").await["SeasonData"]
            ["MatchSeasonType"],
        "Regular"
    );
}

#[tokio::test]
async fn tower_sweep_consumes_tickets_and_grants_repeat_rewards() {
    let (s, u) = setup().await;
    let entry="ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&TowerIndex=21&TowerFloor=1";
    let start = call(&s, &u, "campaign/begin_campaign", entry).await;
    assert_eq!(start["Result"], "Success", "{start}");
    assert_eq!(call(&s,&u,"campaign/end_campaign","ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&Completed=true&AliveHeroIndices=[1]").await["Result"],"Success");
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,120125,10) ON CONFLICT(account_id,item_index) DO UPDATE SET count=10").bind(account(&u)).execute(&s.db).await.unwrap();
    let sweep = call(
        &s,
        &u,
        "sweep/sweep_dungeon",
        "ChapterIndex=5001&DungeonIndex=1&DungeonDifficulty=0&SweepCount=2",
    )
    .await;
    assert_eq!(sweep["Result"], "Success", "{sweep}");
    assert_eq!(sweep["TowerInfoResult"]["CompletedFloor"], 1);
    assert_eq!(sweep["StaminaResult"]["AddValue"], -2);
    let count: i64 =
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=120125")
            .bind(account(&u))
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 8);
    assert!(
        sweep["ItemResults"]
            .as_array()
            .unwrap()
            .iter()
            .any(|v| n(v, "ItemIndex") != 120125)
            || !sweep["CurrencyResults"].as_array().unwrap().is_empty()
    );
    let lobby = json!(
        crate::api::account::lobby::enter_lobby(
            State(s.clone()),
            axum::extract::Form(crate::api::account::lobby::EnterLobbyRequest {
                session_id: None,
                session_key: u["UserInfo"]["SessionKey"].as_str().map(String::from)
            })
        )
        .await
        .unwrap()
        .0
    );
    assert!(lobby["TopClearDungeonInfos"]
        .as_array()
        .unwrap()
        .iter()
        .any(|v| v["DungeonIndex"] == 1));
}

#[tokio::test]
async fn cancelling_dispatch_refunds_unplayed_runs_and_restores_party() {
    let (s, u) = setup().await;
    call(&s, &u, "campaign/begin_campaign", ENTRY).await;
    call(&s, &u, "campaign/end_campaign", END).await;
    let before = balance(&s, "stamina").await;
    let start = call(
        &s,
        &u,
        "dispatch/start_dispatch",
        "ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=2&DeckIndex=1",
    )
    .await;
    assert_eq!(start["Result"], "Success", "{start}");
    let args = format!("SlotIndex={}", n(&start["DispatchBattleInfo"], "SlotIndex"));
    assert_eq!(
        call(&s, &u, "dispatch/cancel_dispatch", &args).await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, "stamina").await, before);
    assert_ne!(
        call(&s, &u, "dispatch/cancel_dispatch", &args).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "campaign/begin_campaign", ENTRY).await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "dispatch/start_dispatch",
            "ChapterIndex=1&DungeonIndex=1&Difficulty=1&HeroIndices=[1]&RepeatCount=1"
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn godking_open_and_keys_survive_relogin_without_double_charge() {
    let (s, u) = setup().await;
    let first = call(
        &s,
        &u,
        "godking_trial/open_godking_trial_dungeon",
        "ChapterIndex=100000",
    )
    .await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["StaminaResult"]["NewValue"], 1);
    assert_ne!(
        call(
            &s,
            &u,
            "godking_trial/open_godking_trial_dungeon",
            "ChapterIndex=100000"
        )
        .await["Result"],
        "Success"
    );
    let login = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await
            .unwrap()
            .0
    );
    assert_eq!(login["UserInfo"]["GodkingTrialKey"], 1);
    assert_eq!(login["GodkingTrialDungeons"][0]["IsOpen"], 1);
}
#[tokio::test]
async fn selected_rewards_require_completion_and_validate_choices() {
    let (s, u) = setup().await;
    let c = 20001;
    let d = 1;
    let def = row(
        &s,
        "SelectReward",
        &[("ChapterIndex", c), ("DungeonIndex", d)],
    )
    .unwrap();
    let code = def["Item1Code"].as_str().unwrap();
    let args = serde_urlencoded::to_string([
        ("ChapterIndex", c.to_string()),
        ("DungeonIndex", d.to_string()),
        ("ItemCodes", json!([code]).to_string()),
    ])
    .unwrap();
    assert_ne!(
        call(&s, &u, "campaign/get_selected_reward", &args).await["Result"],
        "Success"
    );
    {
        let mut db = s.db.acquire().await.unwrap();
        put(
            &mut db,
            account(&u),
            "selected_reward",
            campaign::key(c, d),
            &json!({"Count":1}),
        )
        .await
        .unwrap();
    }
    let bad = args.replace(&urlencoding::encode(code).to_string(), "UNKNOWN");
    assert_ne!(
        call(&s, &u, "campaign/get_selected_reward", &bad).await["Result"],
        "Success"
    );
    let r = call(&s, &u, "campaign/get_selected_reward", &args).await;
    assert_eq!(r["Result"], "Success", "{r}");
    assert_ne!(
        call(&s, &u, "campaign/get_selected_reward", &args).await["Result"],
        "Success"
    );
}
