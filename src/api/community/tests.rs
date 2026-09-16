use super::*;
use crate::api::account::user;
use crate::database;
use crate::tables::GameTables;
use std::{path::Path, sync::OnceLock};
async fn setup() -> AppState {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    )
}
async fn login(s: &AppState, id: &str) -> Value {
    let u = json!(
        user::login(State(s.clone()), Bytes::from(format!("LoginId={id}")))
            .await
            .unwrap()
            .0
    );
    sqlx::query("UPDATE user_info SET team_level=60,gold=100000000,gem=100000 WHERE account_id=?")
        .bind(n(&u["UserInfo"], "AccountId"))
        .execute(&s.db)
        .await
        .unwrap();
    u
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
async fn create(s: &AppState, u: &Value, name: &str, way: i64) -> i64 {
    let v = call(
        s,
        u,
        "guild/create_guild",
        &format!("name={name}&logo=1&logoBackground=1&joinWay={way}&reqTeamLevel=1"),
    )
    .await;
    assert_eq!(v["Result"], "Success", "{v}");
    n(&v, "GuildId")
}
#[tokio::test]
async fn guild_applications_roles_and_master_transfer_validate_ownership() {
    let s = setup().await;
    let u = login(&s, "master").await;
    let v = login(&s, "member").await;
    let w = login(&s, "outsider").await;
    let g = create(&s, &u, "GuildOne", 2).await;
    let a = n(&v["UserInfo"], "AccountId");
    assert_eq!(
        call(&s, &v, "guild/request_join_guild", &format!("GuildId={g}")).await["JoinWay"],
        2
    );
    assert_ne!(
        call(
            &s,
            &w,
            "guild/accept_join_request",
            &format!("AccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/accept_join_request",
            &format!("AccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    let members = call(
        &s,
        &u,
        "guild/get_all_guild_member_info",
        &format!("GuildId={g}"),
    )
    .await;
    assert_eq!(members["MemberInfos"][0]["Rank"], 1);
    assert_eq!(members["MemberInfos"][1]["Rank"], 3);
    assert_ne!(
        call(&s, &v, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/delegate_master",
            &format!("DelegateAccountId={a}")
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/kick_guildmember", &format!("AccountId={a}")).await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/withdraw_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &v, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn guild_contribution_limits_and_attendance_are_persistent() {
    let s = setup().await;
    let u = login(&s, "donor").await;
    create(&s, &u, "Donators", 1).await;
    for _ in 0..5 {
        let v = call(&s, &u, "guild/contribute_guild", "").await;
        assert_eq!(v["Result"], "Success", "{v}");
    }
    assert_ne!(
        call(&s, &u, "guild/contribute_guild", "").await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/set_guild_attendance", "").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/set_guild_attendance", "").await["Result"],
        "Success"
    );
    let first = call(&s, &u, "guild/send_guild_attendance_reward", "").await;
    assert_eq!(first["Result"], "Success", "{first}");
    call(&s, &u, "guild/send_guild_attendance_reward", "").await;
    let count: i64 =
        sqlx::query_scalar("SELECT count(*) FROM mails WHERE title='Guild attendance reward'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let relog = login(&s, "donor").await;
    assert_eq!(relog["UserInfo"]["GuildPoint"], 600);
    assert_eq!(
        call(&s, &relog, "guild/get_guild_attendance", "").await["GuildAttendanceInfo"]["Daily"],
        1
    );
    assert_eq!(
        call(&s, &relog, "guild/destroy_guild", "").await["Result"],
        "Success"
    );
    create(&s, &relog, "NextGuild", 1).await;
    assert_ne!(
        call(&s, &relog, "guild/contribute_guild", "").await["Result"],
        "Success"
    );
    let body = Bytes::from(format!(
        "SessionKey={}&ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]",
        relog["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    let blocked = super::super::battle::execute_request(&s, "campaign/begin_campaign", body)
        .await
        .unwrap();
    assert_ne!(blocked["Result"], "Success");
}
#[tokio::test]
async fn arena_offline_flow_rejects_unentered_and_replayed_results() {
    let s = setup().await;
    let u = login(&s, "arena-one").await;
    let _v = login(&s, "arena-two").await;
    let end = "Win=1&PlayTime=30&AliveHeroIndices=[1]";
    assert_ne!(
        call(&s, &u, "match/set_offline_match_result", end).await["Result"],
        "Success"
    );
    let entry = "ArenaType=Normal&HeroIndices=[1]&LeaderHeroIndex=1";
    let first = call(&s, &u, "match/register_match", entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(call(&s, &u, "match/register_match", entry).await, first);
    let wait = call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await;
    assert_eq!(wait["Result"], "WaitMore", "{wait}");
    assert_eq!(wait["SwordResult"]["AddValue"], -1);
    assert_eq!(
        call(&s, &u, "match/wait_match", "PlayOfflineMatch=true").await,
        wait
    );
    assert_ne!(
        call(
            &s,
            &u,
            "match/set_offline_match_result",
            "Win=1&AliveHeroIndices=[2]"
        )
        .await["Result"],
        "Success"
    );
    let result = call(&s, &u, "match/set_offline_match_result", end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_eq!(result["MatchResult"]["GainedMatchScore"], 20);
    assert_ne!(
        call(&s, &u, "match/set_offline_match_result", end).await["Result"],
        "Success"
    );
    let u = login(&s, "arena-one").await;
    assert_eq!(u["BattleInfo"]["MatchScore"], 1020);
}
#[tokio::test]
async fn guild_raid_damage_advances_shared_boss_and_charges_once() {
    let s = setup().await;
    let u = login(&s, "raider").await;
    create(&s, &u, "Raiders", 1).await;
    let invoke = |path: &str, args: &str| {
        let s = s.clone();
        let body = Bytes::from(format!(
            "SessionKey={}&{args}",
            u["UserInfo"]["SessionKey"].as_str().unwrap()
        ));
        let path = path.to_owned();
        async move {
            super::super::battle::execute_request(&s, &path, body)
                .await
                .unwrap()
        }
    };
    let entry = "ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]";
    let first = invoke("campaign/begin_campaign", entry).await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["StaminaResult"]["AddValue"], -1);
    assert_eq!(invoke("campaign/begin_campaign", entry).await, first);
    let end="ChapterIndex=6002&DungeonIndex=1&DungeonDifficulty=0&Completed=false&TotalDamage=1000000000";
    let result = invoke("campaign/end_campaign", end).await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_ne!(
        invoke("campaign/end_campaign", end).await["Result"],
        "Success"
    );
    let list = call(&s, &u, "guild_raid/get_guild_raid_list", "").await;
    let current = list["GuildRaidInfos"]
        .as_array()
        .unwrap()
        .iter()
        .find(|v| n(v, "ChapterIndex") == 6002)
        .unwrap();
    assert_eq!(current["DungeonIndex"], 2);
    let count: i64 =
        sqlx::query_scalar("SELECT count(*) FROM mails WHERE title='Guild raid boss defeated'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let a = n(&u["UserInfo"], "AccountId");
    let saved: String=sqlx::query_scalar("SELECT reward_currencies FROM mails WHERE account_id=? AND title='Guild raid boss defeated'").bind(a).fetch_one(&s.db).await.unwrap();
    assert!(saved.contains("GuildPoint"));
}
#[tokio::test]
async fn guild_levels_buildings_and_skills_charge_target_level_once() {
    let s = setup().await;
    let u = login(&s, "builder").await;
    let g = create(&s, &u, "Builders", 1).await;
    {
        let mut db = s.db.acquire().await.unwrap();
        let mut v = guild::state(&mut db, &s, g).await.unwrap();
        v["ActivityPoint"] = json!(10000000);
        v["Wood"] = json!(1000000);
        v["Stone"] = json!(1000000);
        v["Metal"] = json!(1000000);
        guild::save(&mut db, g, &v).await.unwrap();
    }
    let level = call(&s, &u, "guild/level_up_guild", "Level=2").await;
    assert_eq!(level["Result"], "Success", "{level}");
    assert_ne!(
        call(&s, &u, "guild/level_up_guild", "Level=2").await["Result"],
        "Success"
    );
    let shop = call(
        &s,
        &u,
        "guild/level_up_guild_building",
        "BuildingIndex=3&BuildingLevel=1",
    )
    .await;
    assert_eq!(shop["Result"], "Success", "{shop}");
    assert_ne!(
        call(
            &s,
            &u,
            "guild/level_up_guild_building",
            "BuildingIndex=3&BuildingLevel=1"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(
            &s,
            &u,
            "guild/level_up_guild_building",
            "BuildingIndex=2&BuildingLevel=1"
        )
        .await["Result"],
        "Success"
    );
    let skill = call(
        &s,
        &u,
        "guild/level_up_guild_skill",
        "SkillIndex=1&SkillLevel=1",
    )
    .await;
    assert_eq!(skill["Result"], "Success", "{skill}");
    assert_ne!(
        call(
            &s,
            &u,
            "guild/level_up_guild_skill",
            "SkillIndex=1&SkillLevel=1"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(
        call(&s, &u, "guild/init_guild_skill", "").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "guild/init_guild_skill", "").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn guild_arena_decks_matches_and_replay_protection() {
    let mut s = setup().await;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules
        ["GuildApplyDays"] = json!(0);
    let u = login(&s, "attacker").await;
    let v = login(&s, "defender").await;
    let g = create(&s, &u, "Attackers", 1).await;
    let enemy = create(&s, &v, "Defenders", 1).await;
    let season = arena::season(&s).0;
    for player in [&u, &v] {
        let d = call(
            &s,
            player,
            "guild_arena/set_guild_arena_deck",
            &format!("SeasonIndex={season}&HeroIndices1=[1]"),
        )
        .await;
        assert_eq!(d["Result"], "Success", "{d}");
        assert_eq!(
            call(
                &s,
                player,
                "guild_arena/apply_guild_arena",
                &format!("SessionIndex={season}")
            )
            .await["Result"],
            "Success"
        );
    }
    let entry=format!("SessionIndex={season}&EnemyGuildId={enemy}&EnemyAccountId={}&EnemyDeckIndex=1&HeroIndices=[1]",n(&v["UserInfo"],"AccountId"));
    let start = call(&s, &u, "guild_arena/match_guild_arena", &entry).await;
    assert_eq!(start["Result"], "Success", "{start}");
    assert_eq!(
        call(&s, &u, "guild_arena/match_guild_arena", &entry).await,
        start
    );
    let result = call(
        &s,
        &u,
        "guild_arena/set_guild_arena_match_result",
        "Win=1&PlayTime=10&AliveHeroIndices=[1]",
    )
    .await;
    assert_eq!(result["Result"], "Success", "{result}");
    assert_ne!(
        call(&s, &u, "guild_arena/set_guild_arena_match_result", "Win=1").await["Result"],
        "Success"
    );
    let rank = call(
        &s,
        &u,
        "guild_arena/get_guild_arena_ranker",
        &format!("SeasonIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(rank["MyRankerInfo"]["GuildId"], g);
    assert_eq!(rank["MyRankerInfo"]["SessionScore"], 1);
    let records = call(
        &s,
        &u,
        "guild_arena/get_guild_arena_member_record",
        &format!("SessionIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(records["MemberRecords"].as_array().unwrap().len(), 1);
    assert_eq!(
        records["MemberRecords"][0]["LeftTeam"]["AccountId"],
        u["UserInfo"]["AccountId"]
    );
    let old_start = arena::season(&s).1;
    std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).arena_guild).rules
        ["SeasonEpoch"] = json!(time(old_start - season * 7 * 86400));
    let _ = login(&s, "attacker").await;
    let _ = login(&s, "attacker").await;
    let count: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE title='Guild arena season reward'")
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
    let history = call(
        &s,
        &login(&s, "attacker").await,
        "guild_arena/get_guild_arena_ranker_record",
        &format!("SeasonIndex={season}&PageNo=0"),
    )
    .await;
    assert_eq!(history["RankerRecords"][0]["Win"], 1);
}

#[tokio::test]
async fn arena_pages_and_reward_rollover_are_persistent() {
    let s = setup().await;
    let u = login(&s, "rollover").await;
    let a = n(&u["UserInfo"], "AccountId");
    let first = call(&s, &u, "match/get_match_ranker", "StartRank=0&EndRank=0").await;
    assert_eq!(first["MatchRankers"].as_array().unwrap().len(), 1);
    assert_eq!(first["MatchRankers"][0]["UserInfo"]["AccountId"], a);
    assert!(
        call(&s, &u, "match/get_match_ranker", "StartRank=1&EndRank=1").await["MatchRankers"]
            .as_array()
            .unwrap()
            .is_empty()
    );
    {
        let mut db = s.db.acquire().await.unwrap();
        put(
            &mut db,
            a,
            "arena_daily",
            now() / 86400 - 1,
            &json!({"Day":time(now()-86400)[..10],"Rank":1,"TierIndex":70}),
        )
        .await
        .unwrap();
        sqlx::query(
            "INSERT INTO arena_scores(account,kind,season,score,wins) VALUES(?,0,?,1100,1)",
        )
        .bind(a)
        .bind(arena::season(&s).0 - 1)
        .execute(&mut *db)
        .await
        .unwrap();
    }
    login(&s, "rollover").await;
    login(&s, "rollover").await;
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE sender='Arena'")
        .fetch_one(&s.db)
        .await
        .unwrap();
    assert_eq!(count, 2);
}

#[tokio::test]
async fn unavailable_suppression_does_not_advertise_or_register_a_battle() {
    let s = setup().await;
    let u = login(&s, "suppression").await;
    create(&s, &u, "Conquest", 1).await;
    let info = call(&s, &u, "guild_suppress/get_guild_suppress_session_info", "").await;
    assert_eq!(info["GuildSuppressSessionInfo"]["State"], "NotHeld");
    assert_eq!(info["GuildSuppressApplied"], false);
    assert_ne!(
        call(&s, &u, "guild_suppress/apply_guild_suppress", "").await["Result"],
        "Success"
    );
}
