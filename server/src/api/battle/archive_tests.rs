//! Monster Index records: defeated archive monsters are stored, sent after the
//! battle and at login, and stages cleared before recording are backfilled.
use super::tests::{account, call, setup};
use super::*;
use crate::api::account::user;

const ENTRY: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[1]";
const STAGE: &str = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1";

async fn battle(s: &AppState, u: &Value, end: &str) -> Value {
    assert_eq!(call(s, u, "campaign/begin_campaign", ENTRY).await["Result"], "Success");
    let out = call(s, u, "campaign/end_campaign", &format!("{STAGE}&{end}")).await;
    assert_eq!(out["Result"], "Success", "{out}");
    out
}

async fn login_archive(s: &AppState) -> Value {
    let login = user::test_login(State(s.clone()), Bytes::from_static(b"LoginId=battle-test")).await.unwrap().0;
    serde_json::to_value(&login).unwrap()["PlayerArchiveInfos"].clone()
}

fn monster_info(kind: i64, value: &str) -> Value {
    json!({"ArchiveMainIndex": 10, "ArchiveSubIndex": kind, "ArchiveValue": value})
}

fn without_time(infos: &Value) -> Value {
    json!(infos.as_array().unwrap().iter().map(|v| {
        let mut v = v.clone();
        assert!(v["UpdatedTime"].is_string());
        v.as_object_mut().unwrap().remove("UpdatedTime");
        v
    }).collect::<Vec<_>>())
}

#[tokio::test]
async fn defeated_archive_monsters_are_recorded_returned_and_listed_at_login() {
    let (s, u) = setup().await;
    let a = account(&u);
    // Stage 1-1 (Normal) holds archive monster 21001 (ArchiveType Normal = 7).
    assert_eq!(s.tables.monster_archive.monsters(1, 1, 1, false), &[(7, 21001)]);

    // A lost battle that killed nothing records nothing.
    let lost = battle(&s, &u, "Completed=false&Star=0").await;
    assert!(lost.get("ReservedPlayerArchiveInfos").is_none(), "{lost}");
    // A lost battle records what it killed, but only monsters of that stage:
    // 4401 (the Red Dragon) is not in 1-1, so a forged kill adds nothing.
    let killed = battle(&s, &u, r#"Completed=false&Star=0&MonsterIndices=["4401","21001"]&KillCounts=["1","2"]"#).await;
    assert_eq!(without_time(&killed["ReservedPlayerArchiveInfos"]), json!([monster_info(7, "21001")]));
    // Clearing again adds nothing new, so the response carries no entries.
    let won = battle(&s, &u, "Completed=true&Star=3&AliveHeroIndices=[1]").await;
    assert!(won.get("ReservedPlayerArchiveInfos").is_none(), "{won}");

    let stored: Vec<(i64, i64)> = crate::db::query_as("SELECT archive_type::bigint,monster FROM monster_archive WHERE account_id=$1")
        .bind(a).fetch_all(&s.db).await.unwrap();
    assert_eq!(stored, vec![(7, 21001)]);
    assert_eq!(without_time(&login_archive(&s).await), json!([monster_info(7, "21001")]));
}

#[tokio::test]
async fn clearing_a_stage_records_its_monsters_without_kill_reports() {
    let (s, u) = setup().await;
    let won = battle(&s, &u, "Completed=true&Star=3&AliveHeroIndices=[1]").await;
    assert_eq!(without_time(&won["ReservedPlayerArchiveInfos"]), json!([monster_info(7, "21001")]));
}

#[tokio::test]
async fn login_backfills_monsters_of_stages_cleared_before_recording() {
    let (s, u) = setup().await;
    let a = account(&u);
    assert_eq!(s.tables.monster_archive.monsters(1, 3, 1, false), &[(7, 3001), (7, 3002)]);
    crate::db::query("INSERT INTO campaign_progress(account_id,chapter_id,dungeon_id,clear_count,best_star,is_unlocked) VALUES($1,1,3,1,3,1)")
        .bind(a).execute(&s.db).await.unwrap();
    crate::db::query("INSERT INTO monster_archive(account_id,archive_type,monster,recorded_time) VALUES($1,1,4401,'2026-01-01 00:00:00')")
        .bind(a).execute(&s.db).await.unwrap();
    assert_eq!(without_time(&login_archive(&s).await), json!([monster_info(1, "4401"), monster_info(7, "3001,3002")]));
}

#[tokio::test]
async fn shared_hp_bosses_count_once_fought_and_others_once_killed() {
    let (s, _) = setup().await;
    let defeated = |fields: &[(&str, &str)]| {
        super::archive::defeated(&s, &Request(fields.iter().map(|(k, v)| (k.to_string(), v.to_string())).collect())).unwrap()
    };
    // World Boss 7001-1 is never killed in one run, so fighting it records it.
    assert_eq!(defeated(&[("ChapterIndex", "7001"), ("DungeonIndex", "1"), ("DungeonDifficulty", "0"), ("Completed", "false")]), vec![(4, 5501)]);
    // The Dragon raid 901-7 records nothing when lost without kills.
    assert!(defeated(&[("ChapterIndex", "901"), ("DungeonIndex", "7"), ("DungeonDifficulty", "0"), ("Completed", "false")]).is_empty());
    assert_eq!(defeated(&[("ChapterIndex", "901"), ("DungeonIndex", "7"), ("DungeonDifficulty", "0"), ("Completed", "false"),
        ("MonsterIndices", "[4401]"), ("KillCounts", "[1]")]), vec![(1, 4401)]);
    // A difficulty without waves falls back to the stage's lowest one.
    assert_eq!(defeated(&[("ChapterIndex", "901"), ("DungeonIndex", "7"), ("DungeonDifficulty", "1"), ("Completed", "True")]), vec![(1, 4401)]);
}
