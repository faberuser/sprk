use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{path::Path, sync::OnceLock};
pub(super) async fn setup() -> (AppState, Value) {
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
pub(super) async fn call(s: &AppState, u: &Value, path: &str, args: &str) -> Value {
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
pub(super) fn account(u: &Value) -> i64 {
    n(&u["UserInfo"], "AccountId")
}
const ENTRY: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
const END: &str =
    "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&Star=3&AliveHeroIndices=[1]";

#[tokio::test]
async fn ice_crystal_cave_opens_after_7_6_and_completes_both_battles() {
    let (s, u) = setup().await;
    for chapter in [117, 127] {
        let data = row(&s, "CampaignChapter", &[("Index", chapter)]).unwrap();
        assert_eq!(data["IsOpen"], true);
        assert_eq!(n(data, "ReqChapterIndex"), 7);
        assert_eq!(n(data, "ReqDungeonIndex"), 6);
    }
    let entry = "ChapterIndex=117&DungeonIndex=3&DungeonDifficulty=0&ScenarioDungeon=true&HeroIndices=1";
    assert_eq!(call(&s, &u, "campaign/begin_campaign", entry).await["Result"], "NotCompletedReqDungeon");
    put(&mut *s.db.acquire().await.unwrap(), account(&u), "dungeon", campaign::key(7, 6),
        &json!({"ChapterIndex":7,"DungeonIndex":6,"FirstRewardedDiff":2,"MaxStar":13,"ScenarioComplete":1,"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
    for dungeon in [3, 4] {
        let args = entry.replace("DungeonIndex=3", &format!("DungeonIndex={dungeon}"))
            .replace("ScenarioDungeon=true", if dungeon == 3 { "ScenarioDungeon=true" } else { "ScenarioDungeon=false" });
        let begin = call(&s, &u, "campaign/begin_campaign", &args).await;
        assert_eq!(begin["Result"], "Success", "{begin}");
        let end = call(&s, &u, "campaign/end_campaign", &format!("{args}&Completed=true&Star=3&AliveHeroIndices=1")).await;
        assert_eq!(end["Result"], "Success", "{end}");
    }
}

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
    assert_eq!(result["ExpResultsByGetHero"], result["ExpResultInfos"]);
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
async fn world_boss_advertises_native_rotation_with_a_previous_boss() {
    let (s, u) = setup().await;
    let response = call(&s, &u, "world_boss/get_world_boss_info", "").await;
    assert_eq!(response["Result"], "Success");
    let bosses = response["WorldBossInfos"].as_array().unwrap();
    assert_eq!(bosses.len(), 1);
    let id = n(&bosses[0], "Index");
    let cycle = [3, 5, 4];
    assert_eq!(id, cycle[((seasons::season(&s).0 - 1) % 3) as usize]);
    let def = row(&s, "WorldBoss", &[("Index", id)]).unwrap();
    assert_eq!(def["IsGlobal"], true);
    assert!(s.tables.battle.rows("WorldBoss").iter().any(|previous|
        n(previous, "NextIndex") == id && previous["IsGlobal"] == def["IsGlobal"]));
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
    let before = get(&mut *s.db.acquire().await.unwrap(), account(&u), "key", 26).await.unwrap();
    let blocked = call(&s, &u, "campaign/begin_campaign", "ChapterIndex=50000&DungeonIndex=501&DungeonDifficulty=0&HeroIndices=[1]").await;
    assert_ne!(blocked["Result"], "Success");
    assert_eq!(get(&mut *s.db.acquire().await.unwrap(), account(&u), "key", 26).await.unwrap(), before);
    put(&mut *s.db.acquire().await.unwrap(), account(&u), "dungeon", campaign::key(97,2),
        &json!({"ChapterIndex":97,"DungeonIndex":2,"FirstRewardedDiff":1,"MaxStar":3,"ScenarioComplete":1,"CompletedTime":time(now()),"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
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
async fn missing_punishment_raid_definitions_do_not_spend_stamina() {
    let (mut s, u) = setup().await;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).tables.get_mut("Raid").unwrap().retain(|r| n(r,"Index")!=1005);
    let before = call(&s, &u, "punishment_raid/get_punishment_raid_info", "GroupIndex=101001").await;
    let result = call(
        &s,
        &u,
        "punishment_raid/open_punishment_raid",
        "GroupIndex=101001&Level=1&DungeonType=2",
    )
    .await;
    assert_ne!(result["Result"], "Success");
    let after = call(&s, &u, "punishment_raid/get_punishment_raid_info", "GroupIndex=101001").await;
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
    assert_eq!(
        call(&s, &u, "dispatch/get_dispatch_list", "").await["DispatchBattleInfos"],
        json!([])
    );
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
    assert_eq!(r["DeckResults"][0]["HeroIndices"], "1");
}

#[tokio::test]
async fn reconstructed_punishment_raids_enforce_progression_and_grant_rewards_once() {
    let (s,u)=setup().await;
    let locked = call(&s,&u,"punishment_raid/open_punishment_raid","GroupIndex=101001&Level=1&DungeonType=2").await;
    assert_ne!(locked["Result"], "Success");
    put(&mut *s.db.acquire().await.unwrap(),account(&u),"dungeon",campaign::key(11,10),
        &json!({"ChapterIndex":11,"DungeonIndex":10,"FirstRewardedDiff":1})).await.unwrap();
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=50000 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    let open="GroupIndex=101001&Level=1&DungeonType=2";
    let opened=call(&s,&u,"punishment_raid/open_punishment_raid",open).await;
    assert_eq!(opened["Result"],"Success","{opened}");
    assert_eq!(opened["StaminaResult"]["AddValue"],-6000);
    assert_ne!(call(&s,&u,"punishment_raid/open_punishment_raid",open).await["Result"],"Success");
    for (raid,dungeon) in [(1001,1),(1002,2),(1005,5)] {
        let affixes=if raid==1005 {"&AffixIndices=5003&AffixIndices=5004"} else {""};
        let enter=format!("ChapterIndex=70000&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex=101001&DungeonType=2&Level=1&HeroIndices=[1]{affixes}");
        let end=format!("ChapterIndex=70000&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex=101001&DungeonType=2&Completed=true&AliveHeroIndices=[1]&Star=3");
        if raid==1005 {
            assert_ne!(call(&s,&u,"contents/begin_content",&enter.replace(affixes,"")).await["Result"],"Success");
            assert_ne!(call(&s,&u,"contents/begin_content","ChapterIndex=70000&DungeonIndex=3&DungeonDifficulty=0&GroupIndex=101001&DungeonType=2&Level=1&HeroIndices=[1]").await["Result"],"Success");
        }
        let begun=call(&s,&u,"contents/begin_content",&enter).await;
        assert_eq!(begun["Result"],"Success","{begun}");
        assert_eq!(call(&s,&u,"contents/begin_content",&enter).await,begun);
        if raid==1005 {
            assert_ne!(call(&s,&u,"contents/begin_content",&enter.replace(affixes,"")).await["Result"],"Success");
        }
        assert_ne!(call(&s,&u,"punishment_raid/reset_punishment_raid","GroupIndex=101001&DungeonType=2").await["Result"],"Success");
        let result=call(&s,&u,"contents/end_content",&end).await;
        assert_eq!(result["Result"],"Success","{result}");
        assert!(result["ItemResults"].as_array().is_some_and(|v|!v.is_empty()) || result["CurrencyResults"].as_array().is_some_and(|v|!v.is_empty()) || result["EquipItemInfos"].as_array().is_some_and(|v|!v.is_empty()),"{result}");
        assert_ne!(call(&s,&u,"contents/end_content",&end).await["Result"],"Success");
        assert_ne!(call(&s,&u,"contents/begin_content",&enter).await["Result"],"Success");
    }
    let info=call(&s,&u,"punishment_raid/get_punishment_raid_info","GroupIndex=101001").await;
    assert_eq!(info["OpenPunishmentRaidInfo"]["ClearCount"],1);
    let reopened=call(&s,&u,"punishment_raid/open_punishment_raid",open).await;
    assert_eq!(reopened["Result"],"Success","{reopened}");
    assert_eq!(reopened["StaminaResult"]["AddValue"],-9000);
    assert_eq!(reopened["PunishmentRaidInfos"],json!([]));
    assert!(info["PunishmentRaidInfos"].as_array().unwrap().iter().any(|v|v["ClearCheckIndex"]==1105));
}

#[tokio::test]
async fn punishment_second_group_native_failure_retry_and_group_isolation() {
    let (s,u)=setup().await;
    put(&mut *s.db.acquire().await.unwrap(),account(&u),"dungeon",campaign::key(11,10),
        &json!({"FirstRewardedDiff":1})).await.unwrap();
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=50000 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    for group in [101001,101002] {
        assert_eq!(call(&s,&u,"punishment_raid/open_punishment_raid",&format!("GroupIndex={group}&Level=1&DungeonType=2")).await["Result"],"Success");
    }
    for dungeon in 1..=3 {
        let affixes=if dungeon==3 {"&AffixIndices=6001&AffixIndices=6002"} else {""};
        let entry=format!("ChapterIndex=70001&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex=101002&DungeonType=Boss&Level=1&HeroIndices=1{affixes}");
        let end=format!("ChapterIndex=70001&DungeonIndex={dungeon}&DungeonDifficulty=0&GroupIndex=101002&DungeonType=Boss&Completed=true&Star=3&AliveHeroIndices=1");
        assert_ne!(call(&s,&u,"contents/begin_content",&entry.replace("101002","101001")).await["Result"],"Success");
        assert_eq!(call(&s,&u,"contents/begin_content",&entry).await["Result"],"Success");
        if dungeon==1 {
            let loss=call(&s,&u,"contents/end_content",&end.replace("Completed=true&Star=3","Completed=false&Star=0")).await;
            assert_eq!(loss["Result"],"Success");
            assert_eq!(loss["PunishmentRaidInfos"],json!([]));
            assert_eq!(loss["OpenPunishmentRaidInfos"][0]["IsOpen"],1);
            assert_eq!(call(&s,&u,"contents/begin_content",&entry).await["Result"],"Success");
        }
        let result=call(&s,&u,"contents/end_content",&end).await;
        assert_eq!(result["Result"],"Success","{result}");
        assert_eq!(result["PunishmentRaidInfos"].as_array().unwrap().len(),dungeon as usize);
        assert_eq!(result["OpenPunishmentRaidInfos"][0]["IsOpen"],if dungeon==3 {0}else{1});
    }
    let first=call(&s,&u,"punishment_raid/get_punishment_raid_info","GroupIndex=101001&DungeonType=2").await;
    assert_eq!(first["OpenPunishmentRaidInfo"]["IsOpen"],1);
    assert_eq!(first["PunishmentRaidInfos"].as_array().unwrap().len(),3);
    let login=json!(user::login(State(s.clone()),Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0);
    assert_eq!(login["PunishmentRaidInfos"],first["PunishmentRaidInfos"]);
    assert_eq!(login["OpenPunishmentRaidInfos"].as_array().unwrap().len(),2);
}

#[tokio::test]
async fn karma_native_groups_settle_default_shards_once_and_restore_login() {
    let (s,u)=setup().await;
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=50000 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    for (group,chapter,flask,shard) in [(101001,70101,40236,40241),(101002,70201,40246,40251)] {
        let open=format!("GroupIndex={group}&DungeonType=1");
        let entry=format!("GroupIndex={group}&DungeonType=Karma&ChapterIndex={chapter}&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=1&FlaskItemIndex={flask}");
        assert_ne!(call(&s,&u,"contents/begin_content",&entry).await["Result"],"Success");
        let opened=call(&s,&u,"punishment_raid/open_punishment_raid",&open).await;
        assert_eq!(opened["Result"],"Success","{opened}");
        assert_eq!(opened["StaminaResult"]["AddValue"],-3000);
        let begun=call(&s,&u,"contents/begin_content",&entry).await;
        assert_eq!(begun["Result"],"Success","{begun}");
        assert_eq!(call(&s,&u,"contents/begin_content",&entry).await,begun);
        assert_ne!(call(&s,&u,"punishment_raid/reset_punishment_raid",&open).await["Result"],"Success");
        sqlx::query("UPDATE battle_runs SET started=started-60 WHERE account=?").bind(account(&u)).execute(&s.db).await.unwrap();
        let end=format!("GroupIndex={group}&DungeonType=Karma&ChapterIndex={chapter}&DungeonIndex=1&DungeonDifficulty=0&Completed=false&ClearWave=5&EndWave=6&EndKillCount=0&TotalKillCount=31");
        assert_ne!(call(&s,&u,"contents/end_content",&end.replace("TotalKillCount=31","TotalKillCount=30000")).await["Result"],"Success");
        assert_ne!(call(&s,&u,"contents/end_content",&end.replace("EndWave=6","EndWave=999")).await["Result"],"Success");
        assert_ne!(call(&s,&u,"contents/end_content",&end.replace("ClearWave=5","ClearWave=1000")).await["Result"],"Success");
        let result=call(&s,&u,"contents/end_content",&end).await;
        assert_eq!(result["Result"],"Success","{result}");
        assert!(result["ItemResults"].as_array().unwrap().iter().any(|v|n(v,"ItemIndex")==shard && n(v,"AddCount")>=1),"{result}");
        assert_eq!(result["OpenPunishmentRaidInfos"][0]["IsOpen"],0);
        assert_ne!(call(&s,&u,"contents/end_content",&end).await["Result"],"Success");
    }
    let login=json!(user::login(State(s.clone()),Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0);
    assert_eq!(login["OpenPunishmentRaidInfos"].as_array().unwrap().len(),2);
}

#[tokio::test]
async fn karma_owned_flasks_convert_at_end_and_reject_changed_retry() {
    let (s,u)=setup().await;
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=50000 WHERE account_id=?").bind(account(&u)).execute(&s.db).await.unwrap();
    {
        let mut db=s.db.acquire().await.unwrap();
        let mut grant=Rewards::default();
        tutorial::grant_item(&mut db,&s,account(&u),40237,1,0,0,&mut grant).await.unwrap();
    }
    assert_eq!(call(&s,&u,"punishment_raid/open_punishment_raid","GroupIndex=101001&DungeonType=1").await["Result"],"Success");
    let entry="GroupIndex=101001&DungeonType=Karma&ChapterIndex=70101&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=1&FlaskItemIndex=40237";
    assert_ne!(call(&s,&u,"contents/begin_content",&entry.replace("40237","40247")).await["Result"],"Success");
    let begun=call(&s,&u,"contents/begin_content",entry).await;
    assert_eq!(begun["Result"],"Success","{begun}");
    assert_ne!(call(&s,&u,"contents/begin_content",&entry.replace("40237","40236")).await["Result"],"Success");
    sqlx::query("UPDATE battle_runs SET started=started-60 WHERE account=?").bind(account(&u)).execute(&s.db).await.unwrap();
    let end="GroupIndex=101001&DungeonType=Karma&ChapterIndex=70101&DungeonIndex=1&DungeonDifficulty=0&Completed=false&ClearWave=12&EndWave=13&EndKillCount=0&TotalKillCount=66";
    sqlx::query("UPDATE items SET locked=1 WHERE account_id=? AND item_index=40237").bind(account(&u)).execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"contents/end_content",end).await["Result"],"Success");
    let pending:i64=sqlx::query_scalar("SELECT completed FROM battle_runs WHERE account=?").bind(account(&u)).fetch_one(&s.db).await.unwrap();
    assert_eq!(pending,0);
    sqlx::query("UPDATE items SET locked=0 WHERE account_id=? AND item_index=40237").bind(account(&u)).execute(&s.db).await.unwrap();
    let result=call(&s,&u,"contents/end_content",end).await;
    assert_eq!(result["Result"],"Success","{result}");
    for (index,count) in [(40237,-1),(40242,1)] {
        assert!(result["ItemResults"].as_array().unwrap().iter().any(|v|n(v,"ItemIndex")==index && n(v,"AddCount")==count),"{result}");
    }
    assert!(result["ItemResults"].as_array().unwrap().iter().any(|v|n(v,"ItemIndex")==40241 && n(v,"AddCount")>0));
}

#[tokio::test]
async fn eclipse_native_decks_round_trip_and_invalid_replacements_are_atomic() {
    let (s, u) = setup().await;
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,2,2,1,1)")
        .bind(account(&u)).execute(&s.db).await.unwrap();
    let owned: Vec<i64> = sqlx::query_scalar("SELECT hero_index FROM heroes WHERE account_id=? ORDER BY hero_index LIMIT 2")
        .bind(account(&u)).fetch_all(&s.db).await.unwrap();
    assert_eq!(owned.len(), 2);
    // This is the shape produced by JM_NShared_EclipseDeckResult, including
    // string DeckIndex and comma-separated HeroIndices (not a JSON array).
    let native_ids = format!("{},{}", owned[1], owned[0]);
    let decks = json!([{"DeckIndex":"1","HeroIndices":native_ids,"ClearMaxWaveIndex":"999","CachedHeroInfos":[{"HeroIndex":1,"Level":999}]}]);
    let args = serde_urlencoded::to_string([("HeroInfos", decks.to_string())]).unwrap();
    let saved = call(&s, &u, "eclipse/set_eclipse_deck", &args).await;
    assert_eq!(saved["Result"], "Success", "{saved}");
    assert_eq!(saved["DeckResults"][0]["HeroIndices"], native_ids);
    assert_eq!(saved["DeckResults"][0]["CachedHeroInfos"][0]["HeroIndex"], owned[1]);
    assert_eq!(saved["DeckResults"][0]["CachedHeroInfos"][0]["Level"], 1);
    assert_eq!(saved["DeckResults"][0]["ClearMaxWaveIndex"], 0);
    assert_eq!(call(&s, &u, "eclipse/get_eclipse_deck", "").await["DeckResults"], saved["DeckResults"]);
    assert_eq!(call(&s, &u, "eclipse/get_eclipse_info", "").await["EclipseInfo"]["DeckResults"], saved["DeckResults"]);

    let invalid = vec![
        json!([{"DeckIndex":"1","HeroIndices":"1,1"}]),
        json!([{"DeckIndex":"1","HeroIndices":"1,bad"}]),
        json!([{"DeckIndex":"1","HeroIndices":"1,"}]),
        json!([{"DeckIndex":"1","HeroIndices":"0"}]),
        json!([{"DeckIndex":"1","HeroIndices":"2147483648"}]),
        json!([{"DeckIndex":"1","HeroIndices":"999999"}]),
        json!([{"DeckIndex":"1","HeroIndices":null}]),
        json!([{"DeckIndex":"2","HeroIndices":"1"}]),
        json!([{"DeckIndex":"1","HeroIndices":"1"},{"DeckIndex":"2","HeroIndices":"1"}]),
        json!([{"DeckIndex":"1","HeroIndices":"1"},{"DeckIndex":"1","HeroIndices":"2"}]),
    ];
    for invalid in invalid {
        let args = serde_urlencoded::to_string([("HeroInfos", invalid.to_string())]).unwrap();
        let rejected = call(&s, &u, "eclipse/set_eclipse_deck", &args).await;
        assert_ne!(rejected["Result"], "Success", "{invalid}");
        assert_eq!(call(&s, &u, "eclipse/get_eclipse_deck", "").await["DeckResults"], saved["DeckResults"], "{invalid}");
    }

    // Compatibility with decks persisted by older server versions.
    let mut legacy = saved["DeckResults"][0].clone();
    legacy["HeroIndices"] = json!(json!([owned[1], owned[0]]).to_string());
    put(&mut *s.db.acquire().await.unwrap(), account(&u), "eclipse_deck", 1, &legacy).await.unwrap();
    assert_eq!(call(&s, &u, "eclipse/get_eclipse_deck", "").await["DeckResults"], saved["DeckResults"]);
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
    let unopened = call(&s, &u, "ordeal_arena/ordeal_info", "").await;
    assert_eq!(unopened["OrdealNodeInfos"], json!([]));
    let selected = call(&s, &u, "ordeal_arena/select_tier", "Tier=0").await;
    assert_eq!(selected["Result"], "Success", "{selected}");
    assert!(!selected["OrdealNodeInfos"].as_array().unwrap().is_empty());
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
    assert_ne!(n(&npc["AccountInfo"]["UserInfo"], "AccountId"), account(&u));
    // A previously saved mirror deck must be repaired without resetting it.
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut saved = get(&mut db, account(&u), "ordeal", 0).await.unwrap();
        for node in saved["OrdealNodeInfos"].as_array_mut().unwrap() {
            if node["NodeType"] == "Battle" {
                node["AccountInfo"]["UserInfo"]["AccountId"] = json!(account(&u));
            }
        }
        put(&mut db, account(&u), "ordeal", 0, &saved).await.unwrap();
    }
    let repaired = call(&s, &u, "ordeal_arena/ordeal_info", "").await;
    assert_eq!(repaired["ClearNodeIndices"], event["ClearNodeIndices"]);
    for node in repaired["OrdealNodeInfos"].as_array().unwrap() {
        if node["NodeType"] == "Battle" {
            assert_eq!(node["AccountInfo"]["UserInfo"]["AccountId"], -account(&u));
        }
    }
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
    assert_eq!(end["CurrencyResult"]["CurrencyType"], "OrdealArenaPoint");
    assert_eq!(end["CurrencyResult"]["AddValue"], 100);
    assert_eq!(end["CurrencyResult"]["NewValue"], 100);
    assert_ne!(
        call(&s, &u, "ordeal_arena/end_ordeal_arena", &finish).await["Result"],
        "Success"
    );
    let buff = end["SelectableBuffIndices"][0].as_i64().unwrap();
    let balance = hero::currency(&mut *s.db.acquire().await.unwrap(), account(&u), "OrdealArenaPoint", 0).await.unwrap();
    assert_eq!(balance["NewValue"], 100, "duplicate settlement must not grant currency");
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
        call(&s, &u, "dispatch/get_dispatch_list", "").await["DispatchBattleInfos"],
        json!([])
    );
    let login = json!(
        user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test"))
            .await.unwrap().0
    );
    assert_eq!(login["DispatchBattleInfos"], json!([]));
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
pub(super) async fn unlock_godking(s: &AppState, u: &Value) {
    put(&mut *s.db.acquire().await.unwrap(), account(u), "dungeon", campaign::key(65,11),
        &json!({"ChapterIndex":65,"DungeonIndex":11,"FirstRewardedDiff":1,"MaxStar":3,"ScenarioComplete":1,"CompletedTime":time(now()),"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
}

#[tokio::test]
async fn godking_and_eclipse_reject_locked_accounts_without_charging() {
    let (s,u) = setup().await;
    let result = call(&s,&u,"godking_trial/open_godking_trial_dungeon","ChapterIndex=100000").await;
    assert_ne!(result["Result"],"Success", "{result}");
    let result = call(&s,&u,"campaign/begin_campaign","ChapterIndex=8201&DungeonIndex=1&DungeonDifficulty=0&EnterTicketCount=1").await;
    assert_ne!(result["Result"],"Success", "{result}");
    assert!(list(&mut *s.db.acquire().await.unwrap(),account(&u),"godking").await.unwrap().is_empty());
}

#[tokio::test]
async fn godking_open_and_keys_survive_relogin_without_double_charge() {
    let (s, u) = setup().await;
    unlock_godking(&s,&u).await;
    for (chapter, allowed) in [(100000, vec![3,1]), (100001, vec![6,2,7]), (100002, vec![5,4])] {
        let group = row(&s,"GodkingTrialGroup",&[("ChapterIndex",chapter)]).unwrap();
        for hero in s.tables.battle.rows("BattleHero") {
            let req = Request::parse(format!("HeroIndices=[{}]",n(hero,"Index")).as_bytes()).unwrap();
            let expected = allowed.contains(&n(hero,"TagType")) && !(chapter == 100002 && hero["CodeName"] == "Hero.Kain");
            assert_eq!(restrictions::party(&s,&req,n(group,"BanRuleIndex")).is_ok(),expected,"chapter {chapter}, hero {}",n(hero,"Index"));
        }
    }
    let first = call(
        &s,
        &u,
        "godking_trial/open_godking_trial_dungeon",
        "ChapterIndex=100000",
    )
    .await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["StaminaResult"]["NewValue"], 1);
    hero::recruit_at(&mut *s.db.acquire().await.unwrap(), &s, account(&u), 2, 1, 1, 0).await.unwrap();
    let invalid = call(&s,&u,"campaign/begin_campaign","ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1,2]").await;
    assert_ne!(invalid["Result"],"Success");
    assert_eq!(get(&mut *s.db.acquire().await.unwrap(),account(&u),"godking",100000).await.unwrap()["IsOpen"],1);
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
    assert_ne!(call(&s,&u,"godking_trial/open_godking_trial_dungeon","ChapterIndex=100001").await["Result"],"Success");
    let entry = "ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]";
    let begin = call(&s,&u,"campaign/begin_campaign",entry).await;
    assert_eq!(begin["Result"],"Success", "{begin}");
    let loss = call(&s,&u,"campaign/end_campaign","ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&Completed=false&Star=0").await;
    assert_eq!(loss["Result"],"Success", "{loss}");
    assert_eq!(get(&mut *s.db.acquire().await.unwrap(),account(&u),"godking",100000).await.unwrap()["IsOpen"],1);
    assert_eq!(call(&s,&u,"campaign/begin_campaign",entry).await["Result"],"Success");
    let end = "ChapterIndex=100000&DungeonIndex=1&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=[1]";
    let win = call(&s,&u,"campaign/end_campaign",end).await;
    assert_eq!(win["Result"],"Success", "{win}");
    assert_eq!(win["GodkingTrialDungeonInfo"]["IsOpen"],0);
    assert_eq!(win["CampaignResults"][0]["FirstRewardedDiff"],1);
    assert_ne!(call(&s,&u,"campaign/end_campaign",end).await["Result"], "Success");
    let again = call(&s,&u,"godking_trial/open_godking_trial_dungeon","ChapterIndex=100000").await;
    assert_eq!(again["Result"],"Success", "{again}");
    assert_eq!(again["StaminaResult"]["NewValue"],0);
    assert_eq!(call(&s,&u,"campaign/begin_campaign",entry).await["Result"],"Success");
    let again = call(&s,&u,"campaign/end_campaign",end).await;
    assert_eq!(again["Result"],"Success", "{again}");
    assert_eq!(again["GodkingTrialDungeonInfo"]["IsOpen"],0);
    let login = json!(user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0);
    assert_eq!(login["GodkingTrialDungeons"][0]["IsOpen"],0);

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

#[tokio::test]
async fn campaign_rewards_expose_raider_exp_in_native_response_fields() {
    let (s, u) = setup().await;
    let a = account(&u);
    let before: (i32, i64) = sqlx::query_as("SELECT team_level,team_exp FROM user_info WHERE account_id=?")
        .bind(a).fetch_one(&s.db).await.unwrap();
    let mut grant = Rewards::default();
    grant.team_exp_to_add = 1000;
    let response = rewards(&mut *s.db.acquire().await.unwrap(), &s, a, grant).await.unwrap();
    let changes = response["ExpResultsByGetHero"].as_array().unwrap();
    assert_eq!(changes.len(), 1);
    assert_eq!(changes[0]["AddValue"], 1000);
    assert_eq!(changes[0]["OldLevel"], before.0);
    let saved: (i32, i64) = sqlx::query_as("SELECT team_level,team_exp FROM user_info WHERE account_id=?")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(changes[0]["NewLevel"], saved.0);
    assert_eq!(changes[0]["NewValue"], saved.1);
    assert_eq!(response["HeroExpResults"], json!([]));
    let empty = rewards(&mut *s.db.acquire().await.unwrap(), &s, a, Rewards::default()).await.unwrap();
    assert_eq!(empty["ExpResultsByGetHero"], json!([]));
}

#[tokio::test]
async fn stamina_exp_replaces_hero_exp_and_is_transactional() {
    let (mut s, u) = setup().await;
    let a=account(&u);
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).rules["RaiderExpPerStamina"]=json!(10);
    let before=balance(&s,"team_exp").await;
    let mut tx=s.db.begin().await.unwrap();
    let spent=dungeons::charge(&mut tx,&s,a,1,3).await.unwrap();
    assert_eq!(spent["RaiderExpResult"]["AddValue"],30);
    assert!(dungeons::charge(&mut tx,&s,a,1,0).await.unwrap()["RaiderExpResult"].is_null());
    assert!(dungeons::charge(&mut tx,&s,a,5,1).await.unwrap()["RaiderExpResult"].is_null());
    tx.commit().await.unwrap();
    assert_eq!(balance(&s,"team_exp").await,before+30);
    let mut tx=s.db.begin().await.unwrap();
    dungeons::charge(&mut tx,&s,a,1,4).await.unwrap();
    tx.rollback().await.unwrap();
    assert_eq!(balance(&s,"team_exp").await,before+30);
    let mut tx=s.db.begin().await.unwrap();
    assert!(dungeons::charge(&mut tx,&s,a,1,i32::MAX as i64).await.is_err());
    tx.rollback().await.unwrap();
    let entry=call(&s,&u,"campaign/begin_campaign",ENTRY).await;
    assert_eq!(entry["Result"],"Success");
    let exp=balance(&s,"team_exp").await;
    assert_eq!(call(&s,&u,"campaign/begin_campaign",ENTRY).await,entry);
    assert_eq!(balance(&s,"team_exp").await,exp);
    let end=call(&s,&u,"campaign/end_campaign",END).await;
    assert_eq!(end["Result"],"Success");
    assert_eq!(balance(&s,"team_exp").await,exp);
    assert!(end["ExpResult"].is_null());
    assert_ne!(call(&s,&u,"campaign/end_campaign",END).await["Result"],"Success");
    let login=user::login(State(s.clone()),Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0;
    assert_eq!(login.user_info.team_exp as i64,exp);
}

#[tokio::test]
async fn dragon_dispatch_infers_raid_and_settles_once() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("UPDATE heroes SET level=70 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=10000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    for (chapter, dungeon, raid, cost) in [(905, 7, 101, 48), (931, 1, 111, 60)] {
        let args = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&Difficulty=0&HeroIndices=1&RepeatCount=2&DeckIndex=1");
        let before = balance(&s, "stamina").await;
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        assert_eq!(balance(&s,"stamina").await,before);
        let entry = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={dungeon}&HeroIndices=1");
        assert_eq!(call(&s,&u,"campaign/begin_campaign",&entry).await["Result"],"Success");
        let end = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=1");
        assert_eq!(call(&s,&u,"campaign/end_campaign",&end).await["Result"],"Success");
        sqlx::query("UPDATE heroes SET level=1 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        sqlx::query("UPDATE heroes SET level=70 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        let before = balance(&s,"stamina").await;
        let exp_before = balance(&s,"team_exp").await;
        let start = call(&s,&u,"dispatch/start_dispatch",&format!("{args}&RaidIndex=999&RaidLevel=999")).await;
        assert_eq!(start["Result"],"Success","{start}");
        assert_eq!(start["StaminaResult"]["AddValue"],-2*cost);
        assert_eq!(start["DispatchBattleInfo"]["Request"]["RaidIndex"],raid.to_string());
        let slot = n(&start["DispatchBattleInfo"],"SlotIndex");
        let slot_args = format!("SlotIndex={slot}");
        assert_ne!(call(&s,&u,"campaign/begin_campaign",&entry).await["Result"],"Success");
        let canceled=call(&s,&u,"dispatch/cancel_dispatch",&slot_args).await;
        assert_eq!(canceled["Result"],"Success");
        assert_eq!(balance(&s,"team_exp").await,exp_before);
        assert_eq!(balance(&s,"stamina").await,before);
        // One completed dispatch run earns EXP; the unplayed run is refunded.
        assert_eq!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        {
            let mut db=s.db.acquire().await.unwrap();
            let mut run=get(&mut db,a,"dispatch",slot).await.unwrap();
            run["StartTimestamp"]=json!(now()-settings(&s,"DispatchSecondsPerBattle",60));
            put(&mut db,a,"dispatch",slot,&run).await.unwrap();
        }
        let partial=call(&s,&u,"dispatch/cancel_dispatch",&slot_args).await;
        assert_eq!(partial["Result"],"Success");
        assert_eq!(partial["TeamExpResult"]["AddValue"],cost*200);
        assert_eq!(partial["StaminaResult"]["AddValue"],cost);
        assert_ne!(call(&s,&u,"dispatch/cancel_dispatch",&slot_args).await["Result"],"Success");
        assert_eq!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
        {
            let mut db=s.db.acquire().await.unwrap();
            let mut run=get(&mut db,a,"dispatch",slot).await.unwrap();
            run["FinishTimestamp"]=json!(now()-1);
            put(&mut db,a,"dispatch",slot,&run).await.unwrap();
        }
        let finish=call(&s,&u,"dispatch/request_complete_dispatch",&slot_args).await;
        assert_eq!(finish["Result"],"Success","{finish}");
        assert_eq!(finish["DispatchBattleInfo"]["WinCount"],2);
        assert_eq!(finish["TeamExpResult"]["AddValue"],2*cost*200);
        if chapter == 905 {
            let raw: String = sqlx::query_scalar("SELECT reward_equipment FROM mails WHERE account_id=? AND title='Dispatch equipment rewards' ORDER BY mail_id DESC LIMIT 1")
                .bind(a).fetch_one(&s.db).await.unwrap();
            let gear: Vec<crate::models::equip::EquipItemInfo> = read_json(&raw).unwrap();
            assert!(!gear.is_empty());
            assert!(gear.iter().all(|v| v.slot_index == 0 && v.uid.is_empty()));
        } else {
            assert!(!finish["ItemResults"].as_array().unwrap().is_empty());
        }
        let mail_count: i64 = sqlx::query_scalar("SELECT count(*) FROM mails WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
        assert_ne!(call(&s,&u,"dispatch/request_complete_dispatch",&slot_args).await["Result"],"Success");
        assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count(*) FROM mails WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),mail_count);
        assert_eq!(call(&s,&u,"dispatch/get_dispatch_list","").await["DispatchBattleInfos"],json!([]));
        assert!(dispatch::ensure_available(&mut *s.db.acquire().await.unwrap(),a,&[1],None).await.is_ok());
    }
    assert_ne!(call(&s,&u,"dispatch/start_dispatch","ChapterIndex=901&DungeonIndex=7&Difficulty=0&HeroIndices=1&RepeatCount=2&DeckIndex=1").await["Result"],"Success");
}

#[tokio::test]
async fn dragon_solo_native_raid_type_keeps_party_and_level_guards() {
    let (s, u) = setup().await;
    let a = account(&u);
    let args = "ChapterIndex=905&DungeonIndex=7&DungeonDifficulty=0&RaidIndex=101&RaidLevel=7&HeroIndices=1";
    let low = call(&s, &u, "campaign/begin_campaign", args).await;
    assert_ne!(low["Result"], "Success");
    sqlx::query("UPDATE heroes SET level=60 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let party = call(&s, &u, "campaign/begin_campaign", "ChapterIndex=901&DungeonIndex=7&DungeonDifficulty=0&RaidIndex=1&RaidLevel=7&HeroIndices=1").await;
    assert_ne!(party["Result"], "Success");
    let entry = call(&s, &u, "campaign/begin_campaign", args).await;
    assert_eq!(entry["Result"], "Success", "{entry}");
    assert_eq!(entry["StaminaResult"]["AddValue"], -48);
    assert_eq!(call(&s, &u, "campaign/begin_campaign", args).await, entry);
    let loss = call(&s, &u, "campaign/end_campaign", "ChapterIndex=905&DungeonIndex=7&DungeonDifficulty=0&Completed=false").await;
    assert_eq!(loss["Result"], "Success", "{loss}");
}

#[tokio::test]
async fn hard_dragon_solo_requires_unlock_and_keeps_multiplayer_guard() {
    let (s, u) = setup().await;
    let a = account(&u);
    let args = "ChapterIndex=931&DungeonIndex=1&DungeonDifficulty=0&RaidIndex=111&RaidLevel=1&HeroIndices=1";
    sqlx::query("UPDATE heroes SET level=70 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s, &u, "campaign/begin_campaign", args).await["Result"], "Success");
    put(&mut *s.db.acquire().await.unwrap(), a, "raid", 1,
        &json!({"RaidIndex":1,"RaidLevel":8})).await.unwrap();
    sqlx::query("UPDATE heroes SET level=69 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s, &u, "campaign/begin_campaign", args).await["Result"], "Success");
    sqlx::query("UPDATE heroes SET level=70 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    for bad in [args.replace("RaidIndex=111", "RaidIndex=112"), args.replace("ChapterIndex=931", "ChapterIndex=911").replace("RaidIndex=111", "RaidIndex=11"), args.replace("DungeonIndex=1", "DungeonIndex=2").replace("RaidLevel=1", "RaidLevel=2")] {
        assert_ne!(call(&s, &u, "campaign/begin_campaign", &bad).await["Result"], "Success");
    }
    assert_eq!(balance(&s, "stamina").await, 1000);
    let entry = call(&s, &u, "campaign/begin_campaign", args).await;
    assert_eq!(entry["Result"], "Success", "{entry}");
    assert_eq!(entry["StaminaResult"]["AddValue"], -60);
    assert_eq!(call(&s, &u, "campaign/begin_campaign", args).await, entry);
    let loss = call(&s, &u, "campaign/end_campaign", "ChapterIndex=931&DungeonIndex=1&DungeonDifficulty=0&Completed=false").await;
    assert_eq!(loss["Result"], "Success", "{loss}");
}

#[tokio::test]
async fn dragon_solo_wins_unlock_shared_stages_without_downgrading() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("UPDATE heroes SET level=70 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    for (chapter, dungeon, raid, shared, expected) in [(905,7,101,1,8),(931,1,111,11,2),(931,2,111,11,3),(931,1,111,11,3)] {
        let args = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={dungeon}&HeroIndices=1");
        let entry = call(&s, &u, "campaign/begin_campaign", &args).await;
        assert_eq!(entry["Result"], "Success", "{entry}");
        let end = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=1");
        let win = call(&s, &u, "campaign/end_campaign", &end).await;
        assert_eq!(win["Result"], "Success", "{win}");
        assert_eq!(win["CompletedRaidInfo"]["RaidIndex"], shared);
        assert_eq!(win["CompletedRaidInfo"]["RaidLevel"], expected);
        let gold = balance(&s, "gold").await;
        assert_ne!(call(&s, &u, "campaign/end_campaign", &end).await["Result"], "Success");
        assert_eq!(balance(&s, "gold").await, gold);
        let saved = get(&mut *s.db.acquire().await.unwrap(), a, "raid", shared).await.unwrap();
        assert_eq!(saved["RaidLevel"], expected);
    }
}

#[tokio::test]
async fn field_raid_unlock_cost_and_progression_are_scoped_to_solo() {
    let (mut s, u) = setup().await;
    // Exercise the legacy closed-map exception even when story chapters are enabled.
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle)
        .tables.get_mut("CampaignChapter").unwrap().iter_mut()
        .find(|r| n(r,"Index")==8).unwrap()["IsOpen"] = json!(false);
    let a = account(&u);
    let args = "ChapterIndex=8&DungeonIndex=101&DungeonDifficulty=0&RaidIndex=121&RaidLevel=1&HeroIndices=1";
    sqlx::query("UPDATE heroes SET level=80 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"campaign/begin_campaign",args).await["Result"], "Success");
    put(&mut *s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(7,12),
        &json!({"ChapterIndex":7,"DungeonIndex":12,"FirstRewardedDiff":2,"MaxStar":13,"ScenarioComplete":1,"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
    assert_ne!(call(&s,&u,"campaign/begin_campaign",args).await["Result"], "Success");
    put(&mut *s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(8,26),
        &json!({"ChapterIndex":8,"DungeonIndex":26,"FirstRewardedDiff":2,"MaxStar":13,"ScenarioComplete":1,"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
    for bad in ["ChapterIndex=8&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=1".to_string(),
        args.replace("DungeonIndex=101", "DungeonIndex=100").replace("RaidIndex=121", "RaidIndex=21"),
        args.replace("DungeonIndex=101", "DungeonIndex=111").replace("RaidLevel=1", "RaidLevel=2"),
        args.replace("RaidIndex=121", "RaidIndex=122")] {
        assert_ne!(call(&s,&u,"campaign/begin_campaign",&bad).await["Result"], "Success");
    }
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).rules["EnableLegacyFieldRaids"] = json!(false);
    assert_ne!(call(&s,&u,"campaign/begin_campaign",args).await["Result"], "Success");
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).battle).rules["EnableLegacyFieldRaids"] = json!(true);
    sqlx::query("UPDATE heroes SET level=79 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s,&u,"campaign/begin_campaign",args).await["Result"], "Success");
    sqlx::query("UPDATE heroes SET level=80 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_eq!(balance(&s,"stamina").await,1000);
    let entry = call(&s,&u,"campaign/begin_campaign",args).await;
    assert_eq!(entry["Result"], "Success", "{entry}");
    assert_eq!(entry["StaminaResult"]["AddValue"], -54);
    assert_eq!(call(&s,&u,"campaign/begin_campaign",args).await,entry);
    let equipment_before: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    let win = call(&s,&u,"campaign/end_campaign","ChapterIndex=8&DungeonIndex=101&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=1").await;
    assert_eq!(win["Result"], "Success", "{win}");
    assert_eq!(win["CompletedRaidInfo"]["RaidIndex"],21);
    assert_eq!(win["CompletedRaidInfo"]["RaidLevel"],2);
    let equipment_after: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    assert!((4..=5).contains(&(equipment_after-equipment_before)), "Field raid must award its individual loot bundle");
    let duplicate = call(&s,&u,"campaign/end_campaign","ChapterIndex=8&DungeonIndex=101&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=1").await;
    assert_ne!(duplicate["Result"], "Success");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(), equipment_after);
    let next = args.replace("DungeonIndex=101", "DungeonIndex=111").replace("RaidLevel=1", "RaidLevel=2");
    let entry = call(&s,&u,"campaign/begin_campaign",&next).await;
    assert_eq!(entry["Result"], "Success", "{entry}");
    assert_eq!(entry["StaminaResult"]["AddValue"], -70);
    let loss = call(&s,&u,"campaign/end_campaign","ChapterIndex=8&DungeonIndex=111&DungeonDifficulty=0&Completed=false").await;
    assert_eq!(loss["Result"], "Success", "{loss}");
    let login = user::login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0;
    let login = serde_json::to_value(login).unwrap();
    assert!(login["ChapterDungeons"].as_array().unwrap().iter()
        .any(|v| v["ChapterIndex"] == 8 && v["DungeonIndex"] == 26 && v["MaxStar"] == 13));
}


#[tokio::test]
async fn all_field_raid_stages_settle_once_and_cap_shared_progress() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    sqlx::query("UPDATE user_info SET stamina=10000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    for (chapter, dungeon) in [(7,12),(8,26),(9,23)] {
        put(&mut *s.db.acquire().await.unwrap(), a, "dungeon", campaign::key(chapter,dungeon),
            &json!({"ChapterIndex":chapter,"DungeonIndex":dungeon,"FirstRewardedDiff":2,"MaxStar":13,"ScenarioComplete":1,"DailyCompletedCount":0,"ResetCount":0})).await.unwrap();
    }
    for (chapter, first, raid, shared) in [(8,101,121,21),(9,103,122,22),(9,104,123,23),(9,105,124,24)] {
        for stage in [1,2,1] {
            let dungeon = first + (stage-1)*10;
            let args = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={stage}&HeroIndices=1");
            let stamina = balance(&s,"stamina").await;
            let entry = call(&s,&u,"campaign/begin_campaign",&args).await;
            assert_eq!(entry["Result"],"Success","raid {raid}/{stage}: {entry}");
            assert_eq!(balance(&s,"stamina").await,stamina-if stage==1 {54} else {70});
            assert_eq!(call(&s,&u,"campaign/begin_campaign",&args).await,entry);
            let count_before: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
            let points_before = balance(&s,"raid_point").await;
            let end = format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&DungeonDifficulty=0&Completed=true&Star=3&AliveHeroIndices=1");
            let win = call(&s,&u,"campaign/end_campaign",&end).await;
            assert_eq!(win["Result"],"Success","raid {raid}/{stage}: {win}");
            assert_eq!(win["CampaignResults"],json!([]),"Field Raids must not trigger story-map unlock effects");
            assert_eq!(win["CompletedRaidInfo"]["RaidIndex"],shared);
            assert_eq!(win["CompletedRaidInfo"]["RaidLevel"],2);
            let count_after: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
            assert!(count_after-count_before>=4,"missing Field Raid equipment: {raid}/{stage}");
            let points_after = balance(&s,"raid_point").await;
            assert!(points_after>points_before,"missing Field Raid points: {raid}/{stage}");
            assert_ne!(call(&s,&u,"campaign/end_campaign",&end).await["Result"],"Success");
            assert_eq!(balance(&s,"raid_point").await,points_after);
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),count_after);
        }
    }
}


#[tokio::test]
async fn world_boss_unranked_entry_and_zero_based_ranking_match_client() {
    let (s,u) = setup().await;
    let a = account(&u);
    let args = format!("WorldBossIndex=3&Season={}",seasons::season(&s).0);
    let empty = call(&s,&u,"world_boss/get_world_boss_rank_info",&args).await;
    assert_eq!(empty["Result"],"Success");
    assert_eq!(empty["RankInfo"]["AccountId"],a);
    assert_eq!(empty["RankInfo"]["Rank"],-1);
    assert_eq!(empty["RankInfo"]["Score"],0);
    assert_eq!(empty["TotalRankerCount"],0);
    sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',3,?,?,'2026-10-01',1000)")
        .bind(seasons::season(&s).0).bind(a).execute(&s.db).await.unwrap();
    let ranked = call(&s,&u,"world_boss/get_world_boss_rank_info",&args).await;
    assert_eq!(ranked["RankInfo"]["Rank"],0);
    assert_eq!(ranked["RankInfo"]["Score"],1000);
    assert_eq!(ranked["TotalRankerCount"],1);
    let list = call(&s,&u,"world_boss/get_world_boss_ranker_list",&format!("{args}&PageNo=0")).await;
    assert_eq!(list["RankerInfos"][0]["Rank"],0);
}


#[tokio::test]
async fn world_boss_native_creature_damage_settles_once() {
    let (s,u)=setup().await;
    sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
    let begin="ChapterIndex=7003&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]&WorldBossIndex=4";
    assert_eq!(call(&s,&u,"campaign/begin_campaign",begin).await["Result"],"Success");
    for bad in [
        json!([{"Index":"1","Key":"1_0_0","TeamId":"0","GivedDamage":"12345"}]),
        json!([{"Index":"900","Key":"900_0_1","TeamId":"1","GivedDamage":"-1"}]),
        json!([{"Index":"900","Key":"901_0_1","TeamId":"1","GivedDamage":"12345"}]),
        json!([{"Index":"900","Key":"900_0_1","TeamId":"1","GivedDamage":"1"},{"Index":"900","Key":"900_0_1","TeamId":"1","GivedDamage":"1"}]),
    ] {
        let encoded=serde_urlencoded::to_string([("CreatureInfoString",bad.to_string())]).unwrap();
        let invalid=format!("ChapterIndex=7003&DungeonIndex=1&DungeonDifficulty=0&Completed=false&{encoded}");
        assert_ne!(call(&s,&u,"campaign/end_campaign",&invalid).await["Result"],"Success");
    }
    let record=json!([{"Index":"900","Key":"900_0_1","TeamId":"1","GivedDamage":"12345"}]);
    let escaped = urlencoding::encode(&record.to_string()).into_owned();
    let payload=serde_urlencoded::to_string([("CreatureInfoString",escaped)]).unwrap();
    let end=format!("ChapterIndex=7003&DungeonIndex=1&DungeonDifficulty=0&Completed=false&TotalDamage=0&{payload}");
    let result=call(&s,&u,"campaign/end_campaign",&end).await;
    assert_eq!(result["Result"],"Success","{result}");
    assert_eq!(result["WorldBossInfo"]["TotalDamage"],12345);
    assert_eq!(result["WorldBossRankInfo"]["Score"],12345);
    assert_eq!(result["WorldBossRankInfo"]["Rank"],0);
    assert_ne!(call(&s,&u,"campaign/end_campaign",&end).await["Result"],"Success");
}

#[tokio::test]
async fn temple_story_uses_exact_loaned_party_without_ownership_or_hero_exp() {
    let (s,u) = setup().await;
    let entry = "ChapterIndex=65&DungeonIndex=1&DungeonDifficulty=0&ScenarioDungeon=true&HeroIndices=[46,58]&LeaderHeroIndex=46";
    assert_ne!(call(&s,&u,"campaign/begin_campaign",entry).await["Result"],"Success");
    put(&mut *s.db.acquire().await.unwrap(),account(&u),"dungeon",campaign::key(6,21),
        &json!({"FirstRewardedDiff":2})).await.unwrap();
    let forged = entry.replace("[46,58]","[46,1]");
    assert_ne!(call(&s,&u,"campaign/begin_campaign",&forged).await["Result"],"Success");
    let begin = call(&s,&u,"campaign/begin_campaign",entry).await;
    assert_eq!(begin["Result"],"Success","{begin}");
    let end = call(&s,&u,"campaign/end_campaign","ChapterIndex=65&DungeonIndex=1&DungeonDifficulty=0&ScenarioDungeon=true&Completed=true&Star=3&AliveHeroIndices=[46,58]").await;
    assert_eq!(end["Result"],"Success","{end}");
    assert_eq!(end["HeroExpResults"],json!([]));
    assert_eq!(end["CampaignResults"][0]["ScenarioComplete"],1);
    assert!(hero::info(&mut *s.db.acquire().await.unwrap(),account(&u),46).await.is_err());
    let normal = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[46,58]";
    assert_ne!(call(&s,&u,"campaign/begin_campaign",normal).await["Result"],"Success");
}

#[tokio::test]
async fn late_raid_unlocks_require_the_specific_story_clear() {
    let (s,u) = setup().await;
    let mut db = s.db.acquire().await.unwrap();
    let a = account(&u);
    for kind in [40,41,42,45,46,47] {
        assert!(dungeons::require_late_raid_unlock(&mut db,&s,a,kind).await.is_err());
    }
    for (chapter,dungeon,unlocked,still_locked) in [
        (10,9,vec![40,45,46],vec![41,42,47]),
        (10,30,vec![41,42],vec![47]),
        (11,10,vec![47],vec![]),
    ] {
        put(&mut db,a,"dungeon",campaign::key(chapter,dungeon),
            &json!({"ChapterIndex":chapter,"DungeonIndex":dungeon,"FirstRewardedDiff":1})).await.unwrap();
        for kind in unlocked { assert!(dungeons::require_late_raid_unlock(&mut db,&s,a,kind).await.is_ok()); }
        for kind in still_locked { assert!(dungeons::require_late_raid_unlock(&mut db,&s,a,kind).await.is_err()); }
    }
}

#[tokio::test]
async fn new_login_can_replace_abandoned_local_battle_without_rewarding_it() {
    let (s,u)=setup().await;
    hero::recruit_at(&mut *s.db.acquire().await.unwrap(),&s,account(&u),2,1,1,0).await.unwrap();
    let first=call(&s,&u,"campaign/begin_campaign",ENTRY).await;
    assert_eq!(first["Result"],"Success");
    let changed=ENTRY.replace("[1]","[2]");
    assert_ne!(call(&s,&u,"campaign/begin_campaign",&changed).await["Result"],"Success");
    let new=json!(user::login(State(s.clone()),Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0);
    let retry=call(&s,&new,"campaign/begin_campaign",ENTRY).await;
    assert_eq!(retry["RunId"],first["RunId"]);
    let replacement=call(&s,&new,"campaign/begin_campaign",&changed).await;
    assert_eq!(replacement["Result"],"Success","{replacement}");
    assert_ne!(replacement["RunId"],first["RunId"]);
    assert_ne!(call(&s,&new,"campaign/end_campaign",END).await["Result"],"Success");
    let p=campaign::progress(&mut *s.db.acquire().await.unwrap(),&s,account(&u),1,1).await.unwrap();
    assert_eq!(n(&p,"FirstRewardedDiff"),0);
}
