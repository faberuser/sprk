use super::*;
use std::sync::Arc;

#[tokio::test]
async fn world_boss_history_accepts_max_scores_on_multiple_days() {
    let (s,u)=setup().await;
    let a=u.user_info.account_id;
    for day in ["2026-10-27","2026-10-28"] {
        sqlx::query("INSERT INTO battle_scores(family,boss,season,account,day,score) VALUES('world_boss',3,43,?,?,?)")
            .bind(a).bind(day).bind(i64::MAX).execute(&s.db).await.unwrap();
    }
    let history=mission_history::History::load(&mut s.db.acquire().await.unwrap(),a).await.unwrap();
    assert_eq!(history.world_rank,Some(1));
}

async fn setup() -> (AppState, crate::api::account::user::LoginResponse) {
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    crate::database::create_tables(&db).await.unwrap();
    let s = AppState::new(db, crate::tables::test_tables());
    let u = crate::api::account::user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=mission-history"),
    )
    .await
    .unwrap()
    .0;
    (s, u)
}
fn quest<'a>(s: &'a AppState, id: i64) -> &'a Value {
    s.tables
        .progression
        .sub_quests
        .iter()
        .find(|r| n(r, "QuestIndex") == id)
        .unwrap()
}
fn achievement<'a>(s: &'a AppState, id: i64) -> &'a Value {
    s.tables
        .progression
        .achievements
        .iter()
        .find(|r| n(r, "Index") == id && n(r, "Step") == 1)
        .unwrap()
}
async fn call(
    s: &AppState,
    u: &crate::api::account::user::LoginResponse,
    action: &str,
    fields: &str,
) -> Value {
    handle(
        s.clone(),
        Bytes::from(format!("SessionKey={}&{fields}", u.user_info.session_key)),
        action,
    )
    .await
    .unwrap()
    .0
}

#[tokio::test]
async fn mission_history_covers_retained_goals_without_awarding_empty_accounts() {
    let (s, u) = setup().await;
    let v = View::load(&mut s.db.acquire().await.unwrap(), u.user_info.account_id)
        .await
        .unwrap();
    let kinds = [
        "CraftEquip",
        "EndArena",
        "EndEclipse",
        "EndOrdealArena",
        "EnterArena",
        "EnterRaid",
        "ExecEquipGacha",
        "GuildArenaAttackSuccess",
        "GuildLoginDaily",
        "IdentifiedEquip",
        "MonthlyHeroClear",
        "SetGuildArenaDeck",
        "SetGuildArenaDeckSkill",
        "SubStoryCostumeUnlock",
        "TransExtendCount",
        "UsePetIncubator",
    ];
    let mut count = 0;
    for row in s
        .tables
        .progression
        .sub_quests
        .iter()
        .filter(|r| kinds.contains(&r["Kind"].as_str().unwrap_or("")))
    {
        assert_eq!(
            v.history_quest(&s, row),
            Some(0),
            "unhandled or unearned quest: {row}"
        );
        count += 1;
    }
    assert_eq!(count, 103);
    let restored = [
        "BattleRewardEquip",
        "ClearChallengeRaidWeekly",
        "ClearHardRaid",
        "ClearRaidDaily",
        "ClearRaidWeekly",
        "ClearTechnoMagicKingdomRaid",
        "ClearTower",
        "ClearTreasureHouse",
        "EnterDungeon",
        "ExecEquipGacha",
        "GlobalMatchBanPickGrade",
        "GlobalMatchBanPickTierRank",
        "JoinGuildSuppress",
        "JoinMatch",
        "JoinMatchBanPick",
        "JoinMatchBanPickWeekly",
        "KillMonster",
        "LinkAccount",
        "MatchGrade",
        "MatchTierRank",
        "MatchVictory",
        "TotalArtifact",
        "TotalBossMonster",
        "TotalEquipment",
        "TotalItem",
        "TotalMonster",
        "TotalNormalItem",
        "TotalNormalMonster",
        "TotalSoulWeapon",
        "TotalSoulWeaponReinforce",
        "TotalSoulWeaponUpgrade",
        "TotalUniqueTreasure",
        "TotalUniqueWeapon",
        "WinMatchBanPick",
        "WorldBossMaxRank",
    ];
    for row in s
        .tables
        .progression
        .achievements
        .iter()
        .filter(|r| restored.contains(&r["Kind"].as_str().unwrap_or("")))
    {
        assert!(
            v.history_achievement(&s, row, &period(row)).is_some(),
            "unhandled achievement: {row}"
        );
    }
}

#[tokio::test]
async fn mission_history_raids_filter_type_period_and_reward_replays() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let dragon = s
        .tables
        .battle
        .rows("Raid")
        .iter()
        .find(|r| n(r, "Type") == 1)
        .unwrap();
    let hard = s
        .tables
        .battle
        .rows("Raid")
        .iter()
        .find(|r| n(r, "Type") == 3)
        .unwrap();
    let tech = s
        .tables
        .battle
        .rows("CampaignDungeon")
        .iter()
        .find(|r| n(r, "BattleType") == 40)
        .unwrap();
    let mut db = s.db.acquire().await.unwrap();
    for row in [dragon, hard, tech] {
        record(
            &mut db,
            a,
            "ClearDungeon",
            n(row, "ChapterIndex"),
            n(row, "DungeonIndex"),
            3,
        )
        .await
        .unwrap();
        record(
            &mut db,
            a,
            "EnterDungeon",
            n(row, "ChapterIndex"),
            n(row, "DungeonIndex"),
            3,
        )
        .await
        .unwrap();
    }
    record(&mut db, a, "ClearDungeon", 1, 1, 1000)
        .await
        .unwrap();
    let v = View::load(&mut db, a).await.unwrap();
    assert_eq!(v.achievement(&s, achievement(&s, 1109)).0, 6);
    assert_eq!(v.achievement(&s, achievement(&s, 1716)), (3, 3));
    assert_eq!(v.achievement(&s, achievement(&s, 1719)).0, 3);
    assert_eq!(v.quest(&s, quest(&s, 11241)), 3);
    drop(db);
    let fields = "AchievementIndices=[1109]&Steps=[0]";
    assert_eq!(
        call(&s, &u, "reward_achievement", fields).await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "reward_achievement", fields).await["Result"],
        "Success"
    );
    let mut db = s.db.acquire().await.unwrap();
    sqlx::query("UPDATE progression_metrics SET day='2020-01-01' WHERE account_id=?")
        .bind(a)
        .execute(&mut *db)
        .await
        .unwrap();
    let v = View::load(&mut db, a).await.unwrap();
    for id in [1109, 1711, 1716, 1719] {
        assert_eq!(
            v.achievement(&s, achievement(&s, id)).0,
            0,
            "old period: {id}"
        );
    }
    assert_eq!(
        v.quest(&s, quest(&s, 11241)),
        3,
        "lifetime mission retains old valid entries"
    );
}

#[tokio::test]
async fn mission_history_arena_ignores_queue_cancel_and_other_accounts() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    let at = chrono::Utc::now().timestamp();
    for (account, kind, status, cancel, win, started) in [
        (a, 0, "waiting", false, true, at),
        (a, 0, "complete", true, true, at),
        (a + 999, 0, "complete", false, true, at),
        (a, 0, "complete", false, false, at),
        (a, 0, "complete", false, true, at),
        (a, 1, "complete", false, true, at),
        (a, 1, "complete", false, true, 1),
    ] {
        let key = if kind == 0 {
            "BattleInfo"
        } else {
            "BattleBanPickInfo"
        };
        let data = json!({"CompletedAt":started,"End":{"MatchResult":{"Canceled":cancel,"Win":win,"NewTierIndex":35,"NewTierRank":4},key:{"SuccessiveWin":5}}});
        sqlx::query("INSERT INTO arena_runs(account,kind,started,status,data) VALUES(?,?,?,?,?)")
            .bind(account)
            .bind(kind)
            .bind(started)
            .bind(status)
            .bind(data.to_string())
            .execute(&mut *db)
            .await
            .unwrap();
    }
    let v = View::load(&mut db, a).await.unwrap();
    assert_eq!(v.achievement(&s, achievement(&s, 1103)).0, 2);
    assert_eq!(v.achievement(&s, achievement(&s, 1703)).0, 1);
    assert_eq!(v.achievement(&s, achievement(&s, 1712)).0, 1);
    assert_eq!(v.quest(&s, quest(&s, 10562)), 2);
    assert_eq!(v.quest(&s, quest(&s, 1001790)), 1, "five wins in a row");
    assert_eq!(
        v.quest(&s, quest(&s, 1001800)),
        0,
        "does not imply ten wins in a row"
    );
    assert_eq!(v.quest(&s, quest(&s, 1001710)), 1, "gold tier");
    assert_eq!(v.quest(&s, quest(&s, 1001720)), 0, "not platinum");
}

#[tokio::test]
async fn mission_history_monthly_story_receipts_persist_and_claim_once() {
    let (mut s, u) = setup().await;
    let a = u.user_info.account_id;
    for id in 1..=10 {
        let r = call(
            &s,
            &u,
            "complete_monthly_hero",
            &format!("MonthlyHeroIndex={id}"),
        )
        .await;
        assert_eq!(r["Result"], "Success", "Monthly Hero {id}");
        assert_eq!(r["Info"]["MonthlyHeroIndex"], id);
    }
    assert_ne!(
        call(&s, &u, "complete_monthly_hero", "MonthlyHeroIndex=1").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "complete_monthly_hero", "MonthlyHeroIndex=999999").await["Result"],
        "Success"
    );
    let v = View::load(&mut s.db.acquire().await.unwrap(), a)
        .await
        .unwrap();
    assert_eq!(v.quest(&s, quest(&s, 1002000)), 10);
    let table = Arc::make_mut(&mut Arc::make_mut(&mut s.tables).battle);
    table
        .tables
        .get_mut("MonthlyHero")
        .unwrap()
        .iter_mut()
        .find(|r| n(r, "Index") == 11)
        .unwrap()["OpenDate"] = json!("2099-01-01 00:00:00");
    assert_ne!(
        call(&s, &u, "complete_monthly_hero", "MonthlyHeroIndex=11").await["Result"],
        "Success"
    );
    let login = crate::api::account::user::test_login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=mission-history"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(login.monthly_hero_infos.len(), 10);
}

#[tokio::test]
async fn mission_history_summons_collections_and_transcendence_are_specific() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    for (id, count) in [(2, 50), (3, 2), (11, 10)] {
        record(&mut db, a, "ExecEquipGacha", id, 0, count)
            .await
            .unwrap();
    }
    let mut v = View::load(&mut db, a).await.unwrap();
    assert_eq!(v.quest(&s, quest(&s, 10391)), 2);
    assert_eq!(v.quest(&s, quest(&s, 10392)), 10);
    let h = v
        .heroes
        .iter_mut()
        .find(|h| n(h, "HeroIndex") == 1)
        .unwrap();
    h["ApplySkillPage"] = json!(2);
    h["TranscendSkillPage1"] = json!("[10,11,12]");
    h["TranscendSkillPage2"] = json!("[10]");
    assert_eq!(
        v.quest(&s, quest(&s, 5002370)),
        0,
        "inactive page is not applied"
    );
    v.heroes
        .iter_mut()
        .find(|h| n(h, "HeroIndex") == 1)
        .unwrap()["TranscendSkillPage2"] = json!("[10,20,30]");
    assert_eq!(v.quest(&s, quest(&s, 5002370)), 1);
    for (kind, part, detail) in [
        ("TotalUniqueWeapon", 1, 90001),
        ("TotalUniqueTreasure", 7, 90004),
        ("TotalArtifact", 5, 0),
    ] {
        let id = s
            .tables
            .extensions
            .rows("EquipItem")
            .iter()
            .find(|r| n(r, "PartType") == part && (detail == 0 || n(r, "DetailIndex") == detail))
            .map(|r| n(r, "ItemIndex"))
            .unwrap();
        v.history.facts.push(("equip_count".into(), id, 20));
        let row = json!({"Kind":kind,"ReqValue":"1"});
        assert_eq!(
            v.history_achievement(&s, &row, "all"),
            Some((1, 1)),
            "distinct types, not copies"
        );
    }
}

#[tokio::test]
async fn mission_history_restores_guild_story_ordeal_and_monster_records() {
    let (s, u) = setup().await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    for guild in [1, 2] {
        sqlx::query("INSERT INTO community_claims(account,kind,target,period) VALUES(?,'guild_attend_total',?,'2026-10-01')")
            .bind(a).bind(guild).execute(&mut *db).await.unwrap();
    }
    sqlx::query(
        "INSERT INTO guild_arena_decks(guild_id,account,season,deck,data) VALUES(1,?,1,1,?)",
    )
    .bind(a)
    .bind(json!({"SkillSlotIndices":[0,1,2]}).to_string())
    .execute(&mut *db)
    .await
    .unwrap();
    sqlx::query("INSERT INTO guild_arena_records(guild_id,enemy_guild,account,enemy,season,win,data) VALUES(1,2,?,999,1,1,'{}')")
        .bind(a).execute(&mut *db).await.unwrap();
    record(&mut db, a, "WinOrdealArena", 0, 0, 5).await.unwrap();
    record(&mut db, a, "ClearOrdealArenaTier", 2, 0, 1)
        .await
        .unwrap();
    crate::api::extensions::put(&mut db, a, "pet_misc", 6, &json!({"MiscValue":2}))
        .await
        .unwrap();
    crate::api::extensions::put(&mut db, a, "npc", 835, &json!({"HeroIndex":835,"Step":7}))
        .await
        .unwrap();
    crate::api::battle::put(&mut db, a, "eclipse_record", 0, &json!({"BestWave":10}))
        .await
        .unwrap();
    let story = &s.tables.battle.rows("SubStoryUnlock")[0];
    sqlx::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star) VALUES(?,?,?,1,3)")
        .bind(a).bind(n(story,"ChapterIndex")).bind(n(story,"DungeonIndex")).execute(&mut *db).await.unwrap();
    worker_defeats(
        &mut db,
        &s,
        a,
        &json!({"DefeatedCreatures":{"1001":3,"4401":1,"1":99}}),
    )
    .await
    .unwrap();
    let v = View::load(&mut db, a).await.unwrap();
    for (id, progress) in [
        (5002470, 1),
        (1002110, 1),
        (1002120, 1),
        (1002140, 1),
        (5002400, 2),
        (1002620, 5),
        (1002570, 1),
        (1002580, 0),
        (1001010, 1),
        (1001020, 0),
        (5000360, 1),
        (1001950, 1),
    ] {
        assert_eq!(v.quest(&s, quest(&s, id)), progress, "quest {id}");
    }
    for (kind, expected) in [
        ("KillMonster", 4),
        ("TotalMonster", 2),
        ("TotalNormalMonster", 1),
        ("TotalBossMonster", 1),
    ] {
        assert_eq!(
            v.history_achievement(&s, &json!({"Kind":kind,"ReqValue":"10"}), "all"),
            Some((expected, 10))
        );
    }
    assert!(!v.history_proves_access(
        &json!({"Kind":"JoinMatchBanPickWeekly","OpenCondition":["UnknownPaidService"]})
    ));
}
