use super::*;
use super::tests::{call, create, login, setup};

async fn store_card(s: &AppState, a: i64, g: i64, value: &Value) {
    sqlx::query("UPDATE community_state SET data=? WHERE owner=? AND kind='attendance' AND idx=?")
        .bind(value.to_string()).bind(a).bind(g).execute(&s.db).await.unwrap();
}

#[tokio::test]
async fn guild_attendance_first_card_is_claimable_and_counts_once_after_relogin() {
    let s = setup().await;
    let u = login(&s, "attendance-first").await;
    create(&s, &u, "AttendFirst", 1).await;
    let before = call(&s, &u, "guild/get_guild_attendance", "").await;
    let member = &before["GuildMemberAttendanceInfo"];
    assert!(chrono::NaiveDateTime::parse_from_str(member["UpdatedTime"].as_str().unwrap(), "%Y-%m-%d %H:%M:%S").is_ok());
    assert_eq!(member["AttendanceCycleStart"], day());
    assert_eq!(member["SuccessiveCount"], 0);
    let first = call(&s, &u, "guild/set_guild_attendance", "").await;
    assert_eq!(first["Result"], "Success", "{first}");
    assert_eq!(first["GuildMemberAttendanceInfo"]["Day1"], 1);
    for d in 2..=7 { assert_eq!(first["GuildMemberAttendanceInfo"][format!("Day{d}")], 0); }
    assert_eq!(first["GuildMemberAttendanceInfo"]["SuccessiveCount"], 1);
    assert_eq!(first["GuildMemberAttendanceInfo"]["Successive"], 0);
    assert_eq!(first["GuildAttendanceInfo"]["Daily"], 1);
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count(*) FROM mails WHERE title='Guild attendance reward'").fetch_one(&s.db).await.unwrap(), 1);
    assert_ne!(call(&s, &u, "guild/set_guild_attendance", "").await["Result"], "Success");
    let relog = login(&s, "attendance-first").await;
    let saved = call(&s, &relog, "guild/get_guild_attendance", "").await;
    assert_eq!(saved["GuildMemberAttendanceInfo"]["SuccessiveCount"], 1);
    assert_eq!(saved["GuildMemberAttendanceInfo"]["Day1"], 1);
    assert_eq!(relog["UserInfo"]["GuildPoint"], 100);
    call(&s, &relog, "guild/send_guild_attendance_reward", "").await;
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count(*) FROM mails WHERE title='Guild attendance reward'").fetch_one(&s.db).await.unwrap(), 1);
}

#[tokio::test]
async fn guild_attendance_personal_card_rolls_over_without_losing_streak_or_reward_step() {
    let s = setup().await;
    let u = login(&s, "attendance-rollover").await;
    let a = n(&u["UserInfo"], "AccountId");
    let g = create(&s, &u, "AttendRoll", 1).await;
    let mut member = call(&s, &u, "guild/get_guild_attendance", "").await["GuildMemberAttendanceInfo"].clone();
    let today = chrono::Utc::now().date_naive();
    let yesterday = (today - chrono::Duration::days(1)).to_string();
    member["AttendanceCycleStart"] = json!((today - chrono::Duration::days(6)).to_string());
    member["LastDay"] = json!(yesterday);
    member["LastAttendanceTime"] = json!(format!("{yesterday} 12:00:00"));
    member["UpdatedTime"] = member["LastAttendanceTime"].clone();
    member["LastWeekFirstAttendanceTime"] = json!(format!("{} 12:00:00", member["AttendanceCycleStart"].as_str().unwrap()));
    member["SuccessiveCount"] = json!(6);
    member["MaxSuccessiveCount"] = json!(6);
    member["Weekly"] = json!(6);
    for d in 1..=6 { member[format!("Day{d}")] = json!(1); }
    store_card(&s, a, g, &member).await;
    let seventh = call(&s, &u, "guild/set_guild_attendance", "").await;
    assert_eq!(seventh["Result"], "Success", "{seventh}");
    assert_eq!(seventh["GuildMemberAttendanceInfo"]["Day7"], 1);
    assert_eq!(seventh["GuildMemberAttendanceInfo"]["Weekly"], 7);
    assert_eq!(seventh["GuildMemberAttendanceInfo"]["SuccessiveCount"], 7);
    let rewards = call(&s, &u, "guild/send_guild_attendance_reward", "").await;
    assert_eq!(rewards["GuildMemberAttendanceInfo"]["Successive"], 3);
    // Move the completed card to yesterday, then claim the next cycle's first day.
    member = rewards["GuildMemberAttendanceInfo"].clone();
    member["AttendanceCycleStart"] = json!((today - chrono::Duration::days(7)).to_string());
    member["LastDay"] = json!(yesterday);
    member["LastAttendanceTime"] = json!(format!("{yesterday} 12:00:00"));
    member["UpdatedTime"] = member["LastAttendanceTime"].clone();
    store_card(&s, a, g, &member).await;
    sqlx::query("UPDATE community_claims SET period=? WHERE account=? AND period=? AND kind IN ('guild_attend','guild_attend_total','guild_attendance_reward')")
        .bind(&yesterday).bind(a).bind(day()).execute(&s.db).await.unwrap();
    let reset = call(&s, &u, "guild/get_guild_attendance", "").await;
    for d in 1..=7 { assert_eq!(reset["GuildMemberAttendanceInfo"][format!("Day{d}")], 0); }
    assert_eq!(reset["GuildMemberAttendanceInfo"]["SuccessiveCount"], 7);
    let eighth = call(&s, &u, "guild/set_guild_attendance", "").await;
    assert_eq!(eighth["Result"], "Success", "{eighth}");
    assert_eq!(eighth["GuildMemberAttendanceInfo"]["Day1"], 1);
    assert_eq!(eighth["GuildMemberAttendanceInfo"]["Weekly"], 1);
    assert_eq!(eighth["GuildMemberAttendanceInfo"]["SuccessiveCount"], 8);
    assert_eq!(eighth["GuildMemberAttendanceInfo"]["Successive"], 3);
}

#[tokio::test]
async fn guild_attendance_missed_day_resets_streak_and_legacy_empty_card_starts_on_day_one() {
    let s = setup().await;
    let u = login(&s, "attendance-legacy").await;
    let a = n(&u["UserInfo"], "AccountId");
    let g = create(&s, &u, "AttendOld", 1).await;
    sqlx::query("INSERT INTO community_state(owner,kind,idx,data) VALUES(?,'attendance',?,?)")
        .bind(a).bind(g).bind(json!({"GuildId":g,"AccountId":a,"Weekly":0,"Successive":0,"SuccessiveCount":0,"LastAttendanceTime":null,"Week":"2000-01-03"}).to_string())
        .execute(&s.db).await.unwrap();
    let mut member = call(&s, &u, "guild/get_guild_attendance", "").await["GuildMemberAttendanceInfo"].clone();
    assert_eq!(member["AttendanceCycleStart"], day());
    assert!(member["UpdatedTime"].is_string());
    let today = chrono::Utc::now().date_naive();
    member["AttendanceCycleStart"] = json!((today - chrono::Duration::days(2)).to_string());
    member["LastDay"] = member["AttendanceCycleStart"].clone();
    member["UpdatedTime"] = json!(format!("{} 12:00:00", member["LastDay"].as_str().unwrap()));
    member["LastAttendanceTime"] = member["UpdatedTime"].clone();
    member["LastWeekFirstAttendanceTime"] = member["UpdatedTime"].clone();
    member["Day1"] = json!(1);
    member["Weekly"] = json!(1);
    member["SuccessiveCount"] = json!(10);
    member["MaxSuccessiveCount"] = json!(10);
    store_card(&s, a, g, &member).await;
    let gap = call(&s, &u, "guild/get_guild_attendance", "").await;
    assert_eq!(gap["GuildMemberAttendanceInfo"]["SuccessiveCount"], 0);
    let claim = call(&s, &u, "guild/set_guild_attendance", "").await;
    assert_eq!(claim["Result"], "Success", "{claim}");
    assert_eq!(claim["GuildMemberAttendanceInfo"]["Day3"], 1);
    assert_eq!(claim["GuildMemberAttendanceInfo"]["Day2"], 0);
    assert_eq!(claim["GuildMemberAttendanceInfo"]["SuccessiveCount"], 1);
    assert_eq!(claim["GuildMemberAttendanceInfo"]["MaxSuccessiveCount"], 10);
}
