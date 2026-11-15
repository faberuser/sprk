use super::*;
use super::tests::{call, create, login, setup};

async fn store_card(s: &AppState, a: i64, g: i64, value: &Value) {
    crate::db::query("UPDATE community_state SET data=$1 WHERE owner=$2 AND kind='attendance' AND idx=$3")
        .bind(value.to_string()).bind(a).bind(g).execute(&s.db).await.unwrap();
}

#[tokio::test]
async fn guild_member_activity_tracks_checkins_and_long_lived_sessions() {
    let s=setup().await;let u=login(&s,"guild-activity").await;
    let a=n(&u["UserInfo"],"AccountId");let g=create(&s,&u,"Activity",1).await;
    let session=u["UserInfo"]["SessionKey"].as_str().unwrap();
    // Simulate a client that logged in days ago and stayed connected.
    crate::db::query("UPDATE accounts SET last_login='2020-01-01 00:00:00',last_logout='2020-01-02 00:00:00' WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    if let Some(mut entry)=s.sessions.get_mut(session) {entry.last_activity=chrono::Utc::now()-chrono::Duration::days(4);}
    let checked=call(&s,&u,"guild/set_guild_attendance","").await;
    assert_eq!(checked["Result"],"Success","{checked}");
    // Even without an in-memory session, persisted check-in must repair the
    // native roster timestamp, including after a server restart.
    let removed=s.sessions.remove(session).unwrap();
    let mut db=s.db.acquire().await.unwrap();
    let member=guild::member(&mut db,&s,a).await.unwrap();
    assert_eq!(member["LastLoginTime"],checked["GuildMemberAttendanceInfo"]["LastAttendanceTime"]);
    let actual:Option<String>=crate::db::query_scalar("SELECT last_login FROM accounts WHERE account_id=$1").bind(a).fetch_one(&mut *db).await.unwrap();
    assert_eq!(actual.as_deref(),Some("2020-01-01 00:00:00"),"Do not overwrite the account's real login timestamp");
    // Another member without attendance still exposes their latest real session
    // activity. Reading the roster must not mark a stale member active.
    crate::db::query("DELETE FROM community_state WHERE owner=$1 AND kind='attendance' AND idx=$2").bind(a).bind(g).execute(&mut *db).await.unwrap();
    assert_eq!(guild::member(&mut db,&s,a).await.unwrap()["LastLoginTime"],"2020-01-02 00:00:00");
    s.sessions.insert(removed.0,removed.1);
    let activity=chrono::Utc::now();s.sessions.get_mut(session).unwrap().last_activity=activity;
    assert_eq!(guild::member(&mut db,&s,a).await.unwrap()["LastLoginTime"],activity.format("%Y-%m-%d %H:%M:%S").to_string());
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
    assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM mails WHERE title='Guild attendance reward'").fetch_one(&s.db).await.unwrap(), 1);
    assert_eq!(crate::db::query_scalar::<i64>("SELECT COUNT(*) FROM mail_notifications q JOIN mails m ON m.mail_id=q.mail_id WHERE m.title='Guild attendance reward'").fetch_one(&s.db).await.unwrap(),1);
    assert_ne!(call(&s, &u, "guild/set_guild_attendance", "").await["Result"], "Success");
    let relog = login(&s, "attendance-first").await;
    let saved = call(&s, &relog, "guild/get_guild_attendance", "").await;
    assert_eq!(saved["GuildMemberAttendanceInfo"]["SuccessiveCount"], 1);
    assert_eq!(saved["GuildMemberAttendanceInfo"]["Day1"], 1);
    assert_eq!(relog["UserInfo"]["GuildPoint"], 100);
    call(&s, &relog, "guild/send_guild_attendance_reward", "").await;
    assert_eq!(crate::db::query_scalar::<i64>("SELECT count(*) FROM mails WHERE title='Guild attendance reward'").fetch_one(&s.db).await.unwrap(), 1);
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
    crate::db::query("UPDATE community_claims SET period=$1 WHERE account=$2 AND period=$3 AND kind IN ('guild_attend','guild_attend_total','guild_attendance_reward')")
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
    crate::db::query("INSERT INTO community_state(owner,kind,idx,data) VALUES($1,'attendance',$2,$3)")
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

#[tokio::test]
async fn spent_stamina_earns_guild_and_weekly_member_activity() {
    let s=setup().await;let u=login(&s,"guild-stamina").await;
    let a=n(&u["UserInfo"],"AccountId");let g=create(&s,&u,"Stamina",1).await;
    crate::db::query("UPDATE user_info SET stamina=100 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
    let mut db=s.db.acquire().await.unwrap();
    let before=n(&guild::state(&mut db,&s,g).await.unwrap(),"ActivityPoint");
    let mine=n(&guild::member(&mut db,&s,a).await.unwrap(),"AccActivityPoint");
    let rate=settings(&s,"GuildActivityPerStamina",1);
    // The shared stamina charge used by campaign, raid and dispatch settlement.
    let charged=crate::api::battle::charge_key(&mut db,&s,a,1,10).await.unwrap();
    assert_eq!(charged["AddValue"],-10,"{charged}");
    use chrono::Datelike;
    let week=chrono::NaiveDate::parse_from_str(&guild::activity_week(&s),"%Y-%m-%d").unwrap();
    assert_eq!(week.weekday(),chrono::Weekday::Sat,"GuildMemberAccActivityPointResetDOW=6");
    let award=&charged["EclipsePointAward"];
    assert_eq!(award["GuildActivityPointResult"]["NewValue"],before+10*rate,"{award}");
    assert_eq!(award["GuildMemberAccActivityPointResult"]["AddValue"],10*rate);
    assert_eq!(award["GuildMemberAccActivityPointResult"]["NewValue"],mine+10*rate);
    assert_eq!(n(&guild::state(&mut db,&s,g).await.unwrap(),"ActivityPoint"),before+10*rate);
    let exp:i64=crate::db::query_scalar("SELECT exp FROM guilds WHERE guild_id=$1").bind(g).fetch_one(&mut *db).await.unwrap();
    assert_eq!(exp,before+10*rate);
    assert_eq!(n(&guild::member(&mut db,&s,a).await.unwrap(),"AccActivityPoint"),mine+10*rate);
    // A new week clears the personal score but the guild keeps its total.
    let mut m=get(&mut db,a,"guild_member",g).await.unwrap();
    m["ActivityWeek"]=json!("2000-01-01");put(&mut db,a,"guild_member",g,&m).await.unwrap();
    assert_eq!(n(&guild::member(&mut db,&s,a).await.unwrap(),"AccActivityPoint"),0);
    assert_eq!(n(&guild::state(&mut db,&s,g).await.unwrap(),"ActivityPoint"),before+10*rate);
    // Guild Coins: the creator is Guild Master, earning and capping 20% more.
    let coins=&award["GuildPointResult"];
    assert_eq!(coins["AddValue"],12,"{award}");assert_eq!(coins["AddDailyAccValue"],12);assert_eq!(coins["NewDailyAccValue"],12);
    assert_eq!(daily_guild_point(&mut db,a).await.unwrap(),12);
    let today=chrono::Utc::now().format("%Y-%m-%d").to_string();
    put(&mut db,a,"guild_point_daily",0,&json!({"Day":today,"Acc":1195})).await.unwrap();
    let capped=crate::api::battle::charge_key(&mut db,&s,a,1,10).await.unwrap();
    assert_eq!(capped["EclipsePointAward"]["GuildPointResult"]["AddValue"],5,"master cap is 1200");
    let full=crate::api::battle::charge_key(&mut db,&s,a,1,10).await.unwrap();
    assert!(full["EclipsePointAward"]["GuildPointResult"].is_null(),"{full}");
    assert!(!full["EclipsePointAward"]["GuildActivityPointResult"].is_null(),"activity has no daily cap");
    // A regular member earns 1:1 up to 1000.
    crate::db::query("UPDATE guild_members SET role=0 WHERE account_id=$1").bind(a).execute(&mut *db).await.unwrap();
    put(&mut db,a,"guild_point_daily",0,&json!({"Day":"2000-01-01","Acc":1000})).await.unwrap();
    let member=crate::api::battle::charge_key(&mut db,&s,a,1,10).await.unwrap();
    assert_eq!(member["EclipsePointAward"]["GuildPointResult"]["AddValue"],10,"yesterday's total resets");
    put(&mut db,a,"guild_point_daily",0,&json!({"Day":today,"Acc":995})).await.unwrap();
    let member=crate::api::battle::charge_key(&mut db,&s,a,1,10).await.unwrap();
    assert_eq!(member["EclipsePointAward"]["GuildPointResult"]["AddValue"],5,"member cap is 1000");
    // Free charges and players outside a guild earn nothing.
    assert!(crate::api::battle::charge_key(&mut db,&s,a,1,0).await.unwrap()["EclipsePointAward"]["GuildActivityPointResult"].is_null());
}
