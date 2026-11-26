use super::*;
use crate::api::{account::user, inventory::shop};
use axum::{body::Bytes, extract::State};

fn at(text: &str) -> i64 {
    chrono::NaiveDateTime::parse_from_str(text, "%Y-%m-%d %H:%M:%S").unwrap().and_utc().timestamp()
}
fn day_of(text: &str) -> i64 {
    at(&format!("{text} 00:00:00"))
}

async fn setup(name: &str) -> (AppState, user::LoginResponse) {
    let db = crate::database::test_pool().await;
    let s = AppState::new(db, crate::tables::test_tables());
    let u = user::test_login(State(s.clone()), Bytes::from(format!("LoginId={name}"))).await.unwrap().0;
    (s, u)
}

fn keys(cal: &Calendar) -> Vec<String> {
    let mut keys: Vec<String> = cal.active().iter().map(|o| o.key().to_owned()).collect();
    keys.sort();
    keys.dedup();
    keys
}

#[test]
fn windows_are_inclusive_days_and_cross_year_and_month_boundaries() {
    let christmas = json!({"Annual":[["12-15","01-07"]]});
    let runs = windows(&christmas, day_of("2026-12-01"), day_of("2027-02-01"));
    assert_eq!(runs, vec![(day_of("2026-12-15"), day_of("2027-01-08"))]);
    // A run that began last year is still found from inside it.
    assert_eq!(windows(&christmas, day_of("2027-01-07"), day_of("2027-01-07") + 1),
        vec![(day_of("2026-12-15"), day_of("2027-01-08"))]);
    // February 29 is clamped outside leap years.
    let leap = json!({"Annual":[["02-01","02-29"]]});
    assert_eq!(windows(&leap, day_of("2027-01-01"), day_of("2027-12-31")), vec![(day_of("2027-02-01"), day_of("2027-03-01"))]);
    assert_eq!(windows(&leap, day_of("2028-01-01"), day_of("2028-12-31")), vec![(day_of("2028-02-01"), day_of("2028-03-01"))]);
    let forge = json!({"Monthly":{"Months":[2,5,8,11],"Days":[1,14]}});
    assert_eq!(windows(&forge, day_of("2026-11-20"), day_of("2027-06-01")), vec![
        (day_of("2027-02-01"), day_of("2027-02-15")), (day_of("2027-05-01"), day_of("2027-05-15"))]);
    let monthly = json!({"Monthly":{"Days":[1,7]}});
    assert_eq!(windows(&monthly, day_of("2026-11-26"), day_of("2026-12-31")).len(), 1);
    // 2026-11-26 is a Thursday: the next weekend is Saturday the 28th.
    let weekend = json!({"Weekly":{"From":"Sat","Days":2}});
    let runs = windows(&weekend, day_of("2026-11-26"), day_of("2026-12-10"));
    assert_eq!(runs, vec![(day_of("2026-11-28"), day_of("2026-11-30")), (day_of("2026-12-05"), day_of("2026-12-07"))]);
}

#[tokio::test]
async fn proposed_calendar_opens_the_expected_content() {
    let (s, _) = setup("festival-calendar").await;
    let cal = Calendar::at(&s, at("2026-11-26 12:00:00"), Value::Null);
    assert_eq!(keys(&cal), ["thanksgiving"]);
    assert!(cal.manages_chapter(30044) && !cal.chapter_open(30044));
    assert!(cal.chapter_open(30047));
    // Unscheduled event chapters (Piece of Memory episodes, resource dungeons) are not managed.
    assert!(!cal.manages_chapter(30031) && !cal.manages_chapter(40001));
    assert!(cal.craft().is_none() && cal.roulettes().is_empty() && !cal.forge() && cal.world_tree().is_none());
    assert!(cal.shop(43).is_none());
    // Login announces the running Thanksgiving and the upcoming December festivals.
    let events = cal.events();
    let find = |bonus: &str, condition: &str| events.iter().find(|e| e["BonusType"] == bonus && e["Condition"] == condition).cloned();
    let node = find("EventNode", "99,55").unwrap();
    assert_eq!(node["EndTime"], "2026-12-01 00:00:00");
    assert_eq!(find("EventDungeon", "30047").unwrap()["DungeonType"], json!(["EventDungeon"]));
    assert_eq!(find("EventShop", "EVENT_21THANKSGIVING").unwrap()["BeginTime"], "2026-11-15 00:00:00");
    assert_eq!(find("EventNode", "99,36").unwrap()["BeginTime"], "2026-12-15 00:00:00");
    assert_eq!(find("GrowWorldTree", "").unwrap()["BeginTime"], "2026-12-01 00:00:00");
    let craft = find("EventCraft", r#"{"ShowGroups":["211221_1","211221_2"]}"#).unwrap();
    assert_eq!(craft["EndTime"], "2027-01-08 00:00:00");
    assert!(find("EventRoulette", r#"{"RouletteIndices":[1,2,3,4]}"#).is_some());
    let exp = events.iter().find(|e| e["BonusType"] == "BonusExp").unwrap();
    assert_eq!((exp["BonusPercentage"].clone(), exp["BeginTime"].clone()), (json!(50), json!("2026-11-28 00:00:00")));
    assert!(exp["DungeonType"].as_array().unwrap().contains(&json!("EventDungeon")));
    assert!(events.iter().all(|e| e["BonusType"] != "BonusExp" || e["BonusPercentage"] == 50));
    let ids: std::collections::BTreeSet<_> = events.iter().map(|e| n(e, "Id")).collect();
    assert_eq!(ids.len(), events.len(), "event ids must be unique");
    assert!(ids.iter().all(|id| (600_000_000..i32::MAX as i64).contains(id)));

    let christmas = Calendar::at(&s, at("2026-12-18 00:00:00"), Value::Null);
    assert_eq!(keys(&christmas), ["christmas", "christmas-boss", "ophelia-winter", "world-tree"]);
    assert_eq!(christmas.craft().unwrap().0, ["211221_1", "211221_2"]);
    assert_eq!(christmas.roulettes(), [1, 2, 3, 4]);
    assert_eq!(christmas.world_tree(), Some(day_of("2026-12-01") / DAY));
    assert_eq!(christmas.shop(46).unwrap().season(), day_of("2026-12-15") / DAY);
    // The event bosses appear as World Boss skins, signalled to the client;
    // no standalone event boss is scheduled.
    assert!(christmas.bosses().is_empty());
    assert!(keys(&christmas).contains(&"christmas-boss".to_owned()));
    assert!(christmas.events().iter().any(|e| e["Condition"] == "SprkWorldBossChristmas"));
    assert!(Calendar::at(&s, at("2027-09-20 00:00:00"), Value::Null).events().iter()
        .any(|e| e["Condition"] == "SprkWorldBossMoonRabbit"));
    // Lunar New Year announces a marker shop so the client can switch Orvel's map.
    assert!(Calendar::at(&s, at("2027-02-06 12:00:00"), Value::Null).events().iter()
        .any(|e| e["BonusType"] == "EventShop" && e["Condition"] == "SprkLunarNewYear"));
    assert!(Calendar::at(&s, at("2027-02-14 23:59:59"), Value::Null).forge());
    assert!(!Calendar::at(&s, at("2027-02-15 00:00:00"), Value::Null).forge());
    assert!(Calendar::at(&s, at("2027-03-07 10:00:00"), Value::Null).shop(13).is_some());
    assert!(Calendar::at(&s, at("2027-03-08 10:00:00"), Value::Null).shop(13).is_none());
    let weekend = Calendar::at(&s, at("2026-11-29 20:00:00"), Value::Null);
    assert_eq!((weekend.bonus("BonusExp", 1), weekend.bonus("BonusGold", 18), weekend.bonus("BonusItem", 18)), (50, 50, 20));
    assert_eq!(weekend.bonus("BonusExp", 4), 0, "Arena has no Hot Time");
    assert_eq!(cal.bonus("BonusExp", 1), 0, "Thursday has no Hot Time");

    let (version, entries) = cal.calendar();
    assert!(entries.iter().any(|e| e["LocalInfo"]["Title"] == "Thanksgiving Defense" && e["EndDay"] == 30));
    assert!(entries.iter().all(|e| e["LocalInfo"]["Title"] != "Weekend Hot Time"));
    assert_ne!(version, Calendar::at(&s, at("2027-04-26 12:00:00"), Value::Null).calendar().0);
}

#[tokio::test]
async fn overrides_open_and_close_festivals_until_they_lapse() {
    let (s, _) = setup("festival-override").await;
    let now = at("2026-11-26 12:00:00");
    let overrides = json!({"halloween":{"Mode":"Open","Since":now,"Until":now + 3600},
        "thanksgiving":{"Mode":"Closed","Since":now,"Until":now + 3600}});
    let cal = Calendar::at(&s, now, overrides.clone());
    assert_eq!(keys(&cal), ["halloween"]);
    assert!(cal.chapter_open(30044) && !cal.chapter_open(30047));
    assert_eq!(cal.shop(43).unwrap().season(), now / DAY);
    // Lapsed overrides no longer apply.
    assert_eq!(keys(&Calendar::at(&s, now + 3600, overrides)), ["thanksgiving"]);

    let mut db = s.db.acquire().await.unwrap();
    let status = set_override(&mut db, &s, "lakrak-forge", "Open", 2).await.unwrap();
    let forge = status.iter().find(|f| f["Key"] == "lakrak-forge").unwrap();
    assert_eq!((forge["Open"].clone(), forge["Override"]["Mode"].clone()), (json!(true), json!("Open")));
    assert!(Calendar::load(&mut db, &s).await.unwrap().forge());
    set_override(&mut db, &s, "lakrak-forge", "Auto", 0).await.unwrap();
    assert!(get(&mut db, 0, OVERRIDE_KIND, 0).await.unwrap().get("lakrak-forge").is_none());
    assert!(set_override(&mut db, &s, "no-such-festival", "Open", 1).await.is_err());
    assert!(set_override(&mut db, &s, "halloween", "Sometimes", 1).await.is_err());
}

async fn points(s: &AppState, a: i64) -> (i64, i64, i64) {
    let mut db = s.db.acquire().await.unwrap();
    let edp: i64 = crate::db::query_scalar("SELECT event_dungeon_point FROM user_info WHERE account_id=$1").bind(a).fetch_one(&mut *db).await.unwrap();
    let tree = crate::api::heroes::currency(&mut db, a, "GrowWorldTreePoint", 0).await.unwrap();
    let material = s.tables.get_item_index("EVENTCRAFT_MATERIAL_1").unwrap() as i64;
    let item: i64 = crate::db::query_scalar("SELECT COALESCE(SUM(count),0)::bigint FROM items WHERE account_id=$1 AND item_index=$2")
        .bind(a).bind(material).fetch_one(&mut *db).await.unwrap();
    (edp, n(&tree, "NewValue"), item)
}

async fn seed(s: &AppState, a: i64, db: &mut PgConnection) {
    crate::db::query("UPDATE user_info SET event_dungeon_point=500 WHERE account_id=$1").bind(a).execute(&mut *db).await.unwrap();
    let current = crate::api::heroes::currency(db, a, "GrowWorldTreePoint", 0).await.unwrap();
    crate::api::heroes::currency(db, a, "GrowWorldTreePoint", 300 - n(&current, "NewValue")).await.unwrap();
    let material = s.tables.get_item_index("EVENTCRAFT_MATERIAL_1").unwrap() as i64;
    crate::db::query("INSERT INTO items(account_id,item_index,count) VALUES($1,$2,7) ON CONFLICT(account_id,item_index) DO UPDATE SET count=7")
        .bind(a).bind(material).execute(&mut *db).await.unwrap();
}

#[tokio::test]
async fn festival_balances_expire_when_their_merged_run_ends() {
    let (s, u) = setup("festival-expiry").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    // Login already recorded today's run; this test walks its own dates.
    crate::db::query("DELETE FROM extension_state WHERE account_id=$1 AND kind=$2")
        .bind(a).bind(EXPIRY_KIND).execute(&mut *db).await.unwrap();
    seed(&s, a, &mut db).await;
    // First sight records the run without taking earlier balances.
    expire(&mut db, &s, a, &Calendar::at(&s, at("2026-11-07 12:00:00"), Value::Null)).await.unwrap();
    assert_eq!(points(&s, a).await, (500, 300, 7));
    // Halloween ended and Thanksgiving has not begun: points expire.
    expire(&mut db, &s, a, &Calendar::at(&s, at("2026-11-10 12:00:00"), Value::Null)).await.unwrap();
    assert_eq!(points(&s, a).await, (0, 300, 7));
    seed(&s, a, &mut db).await;
    // Thanksgiving, the World Tree and Christmas are one adjacent run for
    // EventDungeonPoint; the World Tree point lasts only through its tree.
    for t in ["2026-11-20 00:00:00", "2026-12-01 00:00:00", "2026-12-18 00:00:00"] {
        expire(&mut db, &s, a, &Calendar::at(&s, at(t), Value::Null)).await.unwrap();
        assert_eq!(points(&s, a).await, (500, 300, 7), "{t}");
    }
    expire(&mut db, &s, a, &Calendar::at(&s, at("2026-12-21 00:00:00"), Value::Null)).await.unwrap();
    assert_eq!(points(&s, a).await, (500, 0, 7));
    // Ophelia's materials expire with the winter workshop, the points with Christmas.
    expire(&mut db, &s, a, &Calendar::at(&s, at("2027-01-08 00:00:00"), Value::Null)).await.unwrap();
    assert_eq!(points(&s, a).await, (0, 0, 0));
    // Gap-time balances wait for the next run's end, not its start.
    seed(&s, a, &mut db).await;
    expire(&mut db, &s, a, &Calendar::at(&s, at("2027-01-20 00:00:00"), Value::Null)).await.unwrap();
    expire(&mut db, &s, a, &Calendar::at(&s, at("2027-02-02 00:00:00"), Value::Null)).await.unwrap();
    assert_eq!(points(&s, a).await, (500, 300, 7));
}

#[tokio::test]
async fn closed_festivals_reject_battles_shops_and_bosses() {
    let (s, u) = setup("festival-gates").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    for key in ["halloween", "christmas-boss", "moon-rabbit"] {
        set_override(&mut db, &s, key, "Closed", 2).await.unwrap();
    }
    assert!(require_chapter(&mut db, &s, a, 30044).await.is_err());
    require_chapter(&mut db, &s, a, 30031).await.unwrap();
    let form = |q: &str| Bytes::from(format!("SessionKey={}&{q}", u.user_info.session_key));
    let closed = shop::get_shop_list(State(s.clone()), form("ShopIndex=43")).await.unwrap().0;
    assert_ne!(closed["Result"], "Success");
    let bosses = crate::api::battle::execute_request(&s, "event_world_boss/get_event_world_boss_info",
        form("")).await.unwrap();
    assert_eq!(bosses["EventWorldBossInfos"], json!([]), "{bosses}");

    set_override(&mut db, &s, "halloween", "Open", 2).await.unwrap();
    require_chapter(&mut db, &s, a, 30044).await.unwrap();
    let listed = shop::get_shop_list(State(s.clone()), form("ShopIndex=43")).await.unwrap().0;
    assert_eq!(listed["Result"], "Success", "{listed}");
    let offer = listed["ShopItems"].as_array().unwrap().iter().find(|r| n(r, "PurchasableCount") > 0).unwrap().clone();
    crate::db::query("UPDATE user_info SET event_dungeon_point=100000 WHERE account_id=$1").bind(a).execute(&mut *db).await.unwrap();
    let buy = format!("ShopIndex=43&ShopItemIndex={}&ShopItemPurchaseCount=1", n(&offer, "ListNo"));
    let bought = shop::buy_shop_item(State(s.clone()), form(&buy)).await.unwrap().0;
    assert_eq!(bought["Result"], "Success", "{bought}");
    // Purchase limits belong to this run of the festival.
    let period: String = crate::db::query_scalar("SELECT period FROM shop_purchase_ledger WHERE account_id=$1 AND shop_index=43")
        .bind(a).fetch_one(&mut *db).await.unwrap();
    assert_eq!(period, format!("festival:{}", chrono::Utc::now().timestamp() / DAY));
}

#[tokio::test]
async fn archived_rate_points_are_granted_only_for_scheduled_chapters() {
    let (s, u) = setup("festival-points").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    let row = s.tables.battle.find("EventDungeon", &[("ChapterIndex", 30003), ("DungeonIndex", 1)]).unwrap();
    assert_eq!(n(row, "EventDungeonPointRate_Easy"), 1000);
    let mut rewards = crate::api::tutorial::Rewards::default();
    dungeon_points(&mut db, &s, a, 30003, 1, 0, 3, &mut rewards).await.unwrap();
    assert_eq!(n(&rewards.currencies[0], "AddValue"), 3 * n(row, "EventDungeonPointAmount_Easy"));
    let edp: i64 = crate::db::query_scalar("SELECT event_dungeon_point FROM user_info WHERE account_id=$1").bind(a).fetch_one(&mut *db).await.unwrap();
    assert_eq!(edp, 30);
    let mut none = crate::api::tutorial::Rewards::default();
    dungeon_points(&mut db, &s, a, 40001, 1, 0, 3, &mut none).await.unwrap();
    assert!(none.currencies.is_empty());
}

#[tokio::test]
async fn login_sends_open_and_upcoming_festival_events() {
    let (s, u) = setup("festival-login").await;
    let festival: Vec<_> = u.drop_bonus_events.iter().filter(|e| n(e, "Id") >= 600_000_000).cloned().collect();
    let mut db = s.db.acquire().await.unwrap();
    let expected = Calendar::load(&mut db, &s).await.unwrap().events();
    assert_eq!(festival.len(), expected.len());
    // A weekend always falls within the look-ahead.
    assert!(festival.iter().any(|e| e["BonusType"] == "BonusExp"));
}

#[tokio::test]
async fn thanksgiving_defense_counts_kills_and_pays_ko_rewards() {
    let (s, u) = setup("festival-ko").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    set_override(&mut db, &s, "thanksgiving", "Open", 2).await.unwrap();
    let form = |q: &str| Bytes::from(format!("SessionKey={}&{q}", u.user_info.session_key));
    let live = |path: &'static str, q: String| {
        let s = s.clone();
        let body = form(&q);
        async move { crate::api::live::execute_request(&s, path, body).await.unwrap() }
    };
    let info = || live("phase_step/get_phase_step_info", "ChapterIndex=30047&DungeonIndex=1".into());
    let claim = |t: &str| live("phase_step/recv_reward_phase_step", format!("ChapterIndex=30047&DungeonIndex=1&Type={t}"));
    let fresh = info().await;
    assert_eq!((fresh["Result"].as_str(), n(&fresh, "TotalCount")), (Some("Success"), 0), "{fresh}");

    // A real defense battle: its kills raise the count and pay Daily levels 1-3.
    let begin = crate::api::battle::execute_request(&s, "campaign/begin_campaign",
        form("ChapterIndex=30047&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]")).await.unwrap();
    assert_eq!(begin["Result"], "Success", "{begin}");
    let end = crate::api::battle::execute_request(&s, "campaign/end_campaign",
        form("ChapterIndex=30047&DungeonIndex=1&DungeonDifficulty=0&Completed=false&MonsterIndices=[\"1\",\"2\"]&KillCounts=[\"70\",\"50\"]")).await.unwrap();
    assert_eq!(end["Result"], "Success", "{end}");
    assert!(!end["ItemResults"].as_array().unwrap().is_empty(), "{end}");
    let after = info().await;
    assert_eq!((n(&after, "TotalCount"), n(&after, "DailyAchievementStep"), n(&after, "TotalAchievementStep")), (120, 3, 0));

    // Levels already paid today are not paid again; kills still add up.
    let mut out = json!({});
    phase_step::finish(&mut db, &s, a, 30047, 1, 60, &mut out).await.unwrap();
    assert_eq!(out, json!({}));
    assert_eq!(claim("General").await["Result"], "InvalidStep");
    phase_step::finish(&mut db, &s, a, 30047, 1, 200, &mut out).await.unwrap();
    assert!(out["ItemResults"].is_array());
    let state = info().await;
    assert_eq!((n(&state, "TotalCount"), n(&state, "DailyAchievementStep")), (380, 5));

    // Milestones are claimed once each.
    let claimed = claim("General").await;
    assert_eq!((claimed["Result"].as_str(), n(&claimed, "AchievementStep")), (Some("Success"), 1), "{claimed}");
    assert!(!claimed["ItemResults"].as_array().unwrap().is_empty() || !claimed["EquipItemResults"].as_array().unwrap().is_empty());
    assert_eq!(claim("General").await["Result"], "InvalidStep");
    assert_eq!(claim("Daily").await["Result"], "InvalidStep");

    // Outside the festival the count stays visible but milestones close.
    set_override(&mut db, &s, "thanksgiving", "Closed", 2).await.unwrap();
    assert_eq!(n(&info().await, "TotalCount"), 380);
    phase_step::finish(&mut db, &s, a, 30047, 1, 1000, &mut json!({})).await.unwrap();
    assert_eq!(claim("General").await["Result"], "EventDungeonClosed");
    // One battle cannot report more than the cap.
    let r = crate::api::system::request::Request::parse(b"KillCounts=[\"9000\",\"x\",\"-5\"]").unwrap();
    assert_eq!(phase_step::kills(&s, &r), 500);
}

#[tokio::test]
async fn lunar_new_year_mails_participation_gifts() {
    let (s, u) = setup("festival-gifts").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    let def = s.tables.live.rules["Festivals"]["Events"].as_array().unwrap().iter()
        .find(|d| d["Key"] == "lunar-new-year").unwrap();
    // 2027-02-06 widened by 7 days before and 14 after (inclusive).
    assert_eq!(windows(def, day_of("2027-01-01"), day_of("2027-03-01")),
        vec![(day_of("2027-01-30"), day_of("2027-02-21"))]);

    let mut day = day_of("2027-02-01");
    for _ in 0..6 {
        gift(&s, &mut db, a, day).await;
        gift(&s, &mut db, a, day + 3600).await; // the same day pays once
        day += DAY;
    }
    assert_eq!(titles(&mut db, a).await, vec!["Happy New Year Gift"; 6]);
    gift(&s, &mut db, a, day).await;
    let sent = titles(&mut db, a).await;
    assert_eq!(sent.len(), 8);
    assert_eq!(sent.last().unwrap(), "New Year's Gratitude Gift");
    // Outside the window nothing is sent; the next year's run starts over.
    gift(&s, &mut db, a, day_of("2027-03-01")).await;
    assert_eq!(titles(&mut db, a).await.len(), 8);
    gift(&s, &mut db, a, day_of("2028-01-20")).await;
    assert_eq!(titles(&mut db, a).await.len(), 9);
    // The mailed box carries its item and opens through the mailbox.
    let items: String = crate::db::query_scalar("SELECT reward_items FROM mails WHERE account_id=$1 AND title='New Year''s Gratitude Gift'")
        .bind(a).fetch_one(&mut *db).await.unwrap();
    let box2 = s.tables.get_item_index("PACKAGE_EVENT_NEWYEAR_BOX2").unwrap();
    assert_eq!(items, format!("[{{\"ItemIndex\":{box2},\"ItemCount\":1}}]"));
}

async fn gift(s: &AppState, db: &mut PgConnection, a: i64, now: i64) {
    let cal = Calendar::at(s, now, Value::Null);
    gifts(db, s, a, &cal).await.unwrap();
}

async fn titles(db: &mut PgConnection, a: i64) -> Vec<String> {
    crate::db::query_scalar("SELECT title FROM mails WHERE account_id=$1 AND title LIKE '%New Year%' ORDER BY mail_id")
        .bind(a).fetch_all(db).await.unwrap()
}

#[tokio::test]
async fn event_boss_score_info_reads_the_summed_score() {
    let (s, u) = setup("festival-boss-score").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    let season = crate::api::battle::execute_request(&s, "event_world_boss/get_event_world_boss_score_info",
        Bytes::from(format!("SessionKey={}&EventWorldBossIndex=8003", u.user_info.session_key))).await.unwrap();
    let season = n(&season["ScoreInfo"], "Season");
    for (day, score) in [("2026-11-25", 40), ("2026-11-26", 2)] {
        crate::db::query("INSERT INTO battle_scores(family,boss,season,account,day,score,battle_time) VALUES('event_world_boss',8003,$1,$2,$3,$4,10)")
            .bind(season).bind(a).bind(day).bind(score as i64).execute(&mut *db).await.unwrap();
    }
    // SUM(score) is NUMERIC in PostgreSQL; the response must still read it.
    let info = crate::api::battle::execute_request(&s, "event_world_boss/get_event_world_boss_score_info",
        Bytes::from(format!("SessionKey={}&EventWorldBossIndex=8003", u.user_info.session_key))).await.unwrap();
    assert_eq!(n(&info["ScoreInfo"], "Score"), 42, "{info}");
}

#[tokio::test]
async fn gm_world_boss_override_replaces_the_rotation_until_reset() {
    let (s, u) = setup("festival-world-boss").await;
    let mut db = s.db.acquire().await.unwrap();
    let info = |s: AppState| {
        let body = Bytes::from(format!("SessionKey={}", u.user_info.session_key));
        async move {
            let v = crate::api::battle::execute_request(&s, "world_boss/get_world_boss_info", body).await.unwrap();
            n(&v["WorldBossInfos"][0], "Index")
        }
    };
    let rotation = info(s.clone()).await;
    let forced = if rotation == 4 { 5 } else { 4 };
    let status = crate::api::battle::set_world_boss_override(&mut db, &s, Some(forced), 2).await.unwrap();
    assert_eq!((n(&status, "Active"), n(&status, "Rotation")), (forced, rotation), "{status}");
    assert_eq!(info(s.clone()).await, forced);
    // Only global World Bosses can be forced.
    assert!(crate::api::battle::set_world_boss_override(&mut db, &s, Some(1), 2).await.is_err());
    crate::api::battle::set_world_boss_override(&mut db, &s, Some(0), 2).await.unwrap();
    assert_eq!(info(s.clone()).await, rotation);
}

#[tokio::test]
async fn lakrak_forge_rewards_follow_the_open_forge() {
    let (s, _) = setup("festival-forge-rewards").await;
    let mut db = s.db.acquire().await.unwrap();
    set_override(&mut db, &s, "lakrak-forge", "Open", 2).await.unwrap();
    let cal = Calendar::load(&mut db, &s).await.unwrap();
    assert_eq!(cal.forge_daily_mission_coins(), 5);
    assert_eq!(cal.forge_remnant_share(false), (1, 3));
    assert_eq!(cal.forge_remnant_share(true), (1, 2));
    set_override(&mut db, &s, "lakrak-forge", "Closed", 2).await.unwrap();
    let cal = Calendar::load(&mut db, &s).await.unwrap();
    assert_eq!(cal.forge_daily_mission_coins(), 0);
    assert_eq!(cal.forge_remnant_share(false), (0, 1));
    // Its coins and materials are among the balances that expire with the run.
    let def = s.tables.live.rules["Festivals"]["Events"].as_array().unwrap().iter()
        .find(|d| d["Key"] == "lakrak-forge").unwrap();
    assert_eq!(def["ExpireCurrencies"], json!(["EventOrvelPoint"]));
}

#[tokio::test]
async fn event_tickets_are_kept_per_event_chapter() {
    let (s, u) = setup("festival-event-tickets").await;
    let a = u.user_info.account_id;
    let mut db = s.db.acquire().await.unwrap();
    let ticket = |v: &Value| n(v, "NewValue");
    crate::api::battle::set_event_ticket_chapter(&mut db, a, 30022).await.unwrap();
    assert_eq!(ticket(&crate::api::battle::charge_key(&mut db, &s, a, 12, 1).await.unwrap()), 1);
    // Another event starts with its own full balance and keeps it.
    crate::api::battle::set_event_ticket_chapter(&mut db, a, 30010).await.unwrap();
    assert_eq!(ticket(&crate::api::battle::charge_key(&mut db, &s, a, 12, 0).await.unwrap()), 2);
    assert_eq!(ticket(&crate::api::battle::charge_key(&mut db, &s, a, 12, 2).await.unwrap()), 0);
    assert!(crate::api::battle::charge_key(&mut db, &s, a, 12, 1).await.is_err());
    crate::api::battle::set_event_ticket_chapter(&mut db, a, 30022).await.unwrap();
    assert_eq!(ticket(&crate::api::battle::charge_key(&mut db, &s, a, 12, 0).await.unwrap()), 1);
    drop(db);
    // The menu request selects the chapter and returns its balance.
    let body = |chapter: i64| Bytes::from(format!("SessionKey={}&ChapterIndex={chapter}", u.user_info.session_key));
    let v = crate::api::battle::event_ticket(State(s.clone()), body(30010)).await.unwrap().0;
    assert_eq!((v["StaminaResult"]["Type"].clone(), ticket(&v["StaminaResult"])), (json!("EventDungeonTicket"), 0));
    let v = crate::api::battle::event_ticket(State(s.clone()), body(30022)).await.unwrap().0;
    assert_eq!(ticket(&v["StaminaResult"]), 1);
    // Only chapters that spend event tickets.
    assert!(crate::api::battle::event_ticket(State(s.clone()), body(99)).await.is_err());
    // Without a chapter: every event's balance, keeping the current event.
    let v = crate::api::battle::event_ticket(State(s.clone()), body(0)).await.unwrap().0;
    assert_eq!(v["Current"], 30022);
    let of = |c: i64| v["Tickets"].as_array().unwrap().iter()
        .find(|t| t["ChapterIndex"] == c).map(|t| ticket(&t["StaminaResult"]));
    assert_eq!((of(30022), of(30010), of(30048)), (Some(1), Some(0), Some(2)));
    let mut db = s.db.acquire().await.unwrap();
    assert_eq!(ticket(&crate::api::battle::charge_key(&mut db, &s, a, 12, 0).await.unwrap()), 1);
}
