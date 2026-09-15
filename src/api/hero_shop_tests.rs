//! Native forms and real client table rules against isolated databases.
use super::{campaign, hero, item, shop, user};
use crate::{database, state::AppState, tables::GameTables};
use axum::{body::Bytes, extract::State};
use serde_json::Value;
use std::{path::Path, sync::OnceLock};
async fn setup() -> (AppState, user::LoginResponse) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let state = AppState::new(
        db,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    let u = login(&state).await;
    (state, u)
}
async fn login(s: &AppState) -> user::LoginResponse {
    user::login(
        State(s.clone()),
        Bytes::from_static(b"LoginId=hero-shop-test"),
    )
    .await
    .unwrap()
    .0
}
fn form(u: &user::LoginResponse, s: &str) -> Bytes {
    Bytes::from(format!("SessionKey={}&{s}", u.user_info.session_key))
}
async fn count(s: &AppState, u: &user::LoginResponse, id: i32) -> i64 {
    sqlx::query_scalar(
        "SELECT COALESCE((SELECT count FROM items WHERE account_id=? AND item_index=?),0)",
    )
    .bind(u.user_info.account_id)
    .bind(id)
    .fetch_one(&s.db)
    .await
    .unwrap()
}
async fn put(s: &AppState, u: &user::LoginResponse, id: i32, count: i64) {
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES (?,?,?) ON CONFLICT(account_id,item_index) DO UPDATE SET count=excluded.count").bind(u.user_info.account_id).bind(id).bind(count).execute(&s.db).await.unwrap();
}
async fn balance(s: &AppState, u: &user::LoginResponse, key: &str) -> i64 {
    sqlx::query_scalar(&format!("SELECT {key} FROM user_info WHERE account_id=?"))
        .bind(u.user_info.account_id)
        .fetch_one(&s.db)
        .await
        .unwrap()
}
async fn get(s: &AppState, u: &user::LoginResponse, id: i32) -> Value {
    hero::info(
        &mut *s.db.acquire().await.unwrap(),
        u.user_info.account_id,
        id,
    )
    .await
    .unwrap()
}
fn buy_data(s: &AppState) -> (i32, i32, i64) {
    for (id, h) in &s.tables.hero_shop.heroes {
        if *id == 1 || h["Buyable"] != true {
            continue;
        }
        for (item_id, meta) in &s.tables.hero_shop.items {
            if let Some(m) = s.tables.items.reward_item(*item_id) {
                if m.kind == "Hero"
                    && m.hero_index == *id
                    && m.star as i64 == item::n(h, "StartHeroStar")
                    && m.transcend == 0
                    && m.level as i64 == item::n(h, "StartHeroLevel").max(1)
                    && meta["Type"] == 15
                {
                    let p = s
                        .tables
                        .hero_shop
                        .prices
                        .iter()
                        .find(|v| {
                            v["Star"] == h["StartHeroStar"] && v["OpenStatus"] == h["OpenStatus"]
                        })
                        .unwrap();
                    return (*id, *item_id, item::n(p, "BuyGem"));
                }
            }
        }
    }
    panic!("no purchasable hero")
}
#[tokio::test]
async fn ruby_hero_purchase_uses_native_contract_prices_and_rejects_duplicates() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    sqlx::query("UPDATE user_info SET gem=100,pay_gem=10000 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}");
    let r = hero::buy_hero(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroResult"]["HeroInfo"]["HeroIndex"], id);
    assert!(r["HeroResult"]["HeroInfo"].get("Transcended").is_some());
    assert_eq!(balance(&s, &u, "gem").await, 0);
    assert_eq!(balance(&s, &u, "pay_gem").await, 10100 - cost);
    assert!(balance(&s, &u, "mileage").await > 0);
    let r = hero::buy_hero(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "HeroAlreadyExist");
    assert_eq!(balance(&s, &u, "pay_gem").await, 10100 - cost);
    assert!(login(&s).await.heroes.iter().any(|h| h.hero_index == id));
}
#[tokio::test]
async fn forged_hero_prices_mismatched_items_and_insufficient_funds_are_atomic() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    let before = balance(&s, &u, "gem").await;
    for req in [
        format!("HeroIndex={id}&ItemIndex={item}&BuyGem=1"),
        format!("HeroIndex={id}&ItemIndex=1&BuyGem={cost}"),
        format!("HeroIndex=99999&ItemIndex={item}&BuyGem={cost}"),
    ] {
        assert_ne!(
            hero::buy_hero(State(s.clone()), form(&u, &req))
                .await
                .unwrap()
                .0["Result"],
            "Success"
        );
    }
    assert_eq!(balance(&s, &u, "gem").await, before);
    sqlx::query("UPDATE user_info SET gem=0,pay_gem=0 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::buy_hero(
            State(s.clone()),
            form(
                &u,
                &format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}")
            )
        )
        .await
        .unwrap()
        .0["Result"],
        "NotEnoughGem"
    );
    assert_eq!(login(&s).await.heroes.len(), 1);
}
#[tokio::test]
async fn competing_purchases_charge_and_recruit_once() {
    let (template, _) = setup().await;
    let path = std::env::temp_dir().join(format!("sprk-hero-shop-{}.sqlite", uuid::Uuid::new_v4()));
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .min_connections(3)
        .max_connections(3)
        .connect_with(
            sqlx::sqlite::SqliteConnectOptions::new()
                .filename(&path)
                .create_if_missing(true)
                .busy_timeout(std::time::Duration::from_secs(10)),
        )
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let s = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let u = login(&s).await;
    let (id, item, cost) = buy_data(&s);
    let before = balance(&s, &u, "gem").await;
    let req = format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}");
    let (a, b) = tokio::join!(
        hero::buy_hero(State(s.clone()), form(&u, &req)),
        hero::buy_hero(State(s.clone()), form(&u, &req))
    );
    assert_eq!(
        [a.unwrap().0, b.unwrap().0]
            .iter()
            .filter(|v| v["Result"] == "Success")
            .count(),
        1
    );
    assert_eq!(balance(&s, &u, "gem").await, before - cost);
    database::create_tables(&pool).await.unwrap();
    assert_eq!(
        login(&s)
            .await
            .heroes
            .iter()
            .filter(|h| h.hero_index == id)
            .count(),
        1
    );
    pool.close().await;
    std::fs::remove_file(path).unwrap();
}
fn costume(s: &AppState) -> Value {
    s.tables
        .hero_shop
        .costumes
        .values()
        .find(|v| {
            v["HeroIndex"] == 1
                && v["Buyable"] == true
                && item::n(v, "ReqBuyGem") > 0
                && item::n(v, "ReqBuyGold") == 0
                && item::n(v, "ReqBuyMileage") == 0
        })
        .unwrap()
        .clone()
}
#[tokio::test]
async fn costume_purchase_equip_presets_and_avatar_survive_relogin() {
    let (s, u) = setup().await;
    let c = costume(&s);
    let id = item::n(&c, "CostumeIndex");
    let cost = item::n(&c, "ReqBuyGem");
    assert_ne!(
        hero::set_costume(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&CostumeIndex={id}"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let req = format!("HeroIndex=1&CostumeIndex={id}&BuyGem={cost}&BuyGold=0&BuyMileage=0");
    let before = balance(&s, &u, "gem").await;
    let r = hero::buy_costume(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroCostumeResult"]["CostumeIndex"], id);
    assert_eq!(balance(&s, &u, "gem").await, before - cost);
    assert_eq!(
        hero::buy_costume(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "AlreadyHaveCostume"
    );
    assert_eq!(
        hero::save_costume_storage_slot(
            State(s.clone()),
            form(&u, "HeroIndex=1&CostumeStorageSlotIndex=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let _ = hero::unset_costume(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap();
    assert_eq!(get(&s, &u, 1).await["CostumeIndex"], 0);
    let _ = hero::set_costume_storage_slot(
        State(s.clone()),
        form(&u, "HeroIndex=1&CostumeStorageSlotIndex=1"),
    )
    .await
    .unwrap();
    assert_eq!(
        hero::change_avatar_hero(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&AvatarHeroIndex={}", 10000 + id))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let again = login(&s).await;
    assert!(again.costume_infos.iter().any(|v| v["CostumeIndex"] == id));
    assert_eq!(again.heroes[0].details["CostumeIndex"], id);
    assert_eq!(again.user_info.avatar_hero_index as i64, 10000 + id);
    assert_eq!(again.costume_storage_slot_infos.len(), 1);
}
#[tokio::test]
async fn costume_wrong_hero_free_default_and_price_forgery_do_not_charge() {
    let (s, u) = setup().await;
    let c = costume(&s);
    let id = item::n(&c, "CostumeIndex");
    let before = balance(&s, &u, "gem").await;
    for req in [
        format!("HeroIndex=1&CostumeIndex={id}&BuyGem=1"),
        format!("HeroIndex=2&CostumeIndex={id}&BuyGem=3000"),
        "HeroIndex=1&CostumeIndex=101&BuyGem=0".into(),
    ] {
        assert_ne!(
            hero::buy_costume(State(s.clone()), form(&u, &req))
                .await
                .unwrap()
                .0["Result"],
            "Success"
        );
    }
    assert_eq!(balance(&s, &u, "gem").await, before);
}
#[tokio::test]
async fn bookmarks_replace_selection_and_reject_unowned_heroes() {
    let (s, u) = setup().await;
    assert_eq!(
        hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[1]"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert!(login(&s).await.heroes[0].is_bookmarked);
    assert_ne!(
        hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[1,9999]"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert!(get(&s, &u, 1).await["IsBookmarked"].as_bool().unwrap());
    let _ = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[]"))
        .await
        .unwrap();
    assert!(!get(&s, &u, 1).await["IsBookmarked"].as_bool().unwrap());
}
#[tokio::test]
async fn skill_upgrade_uses_current_level_price_and_extend_uses_class_book() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE heroes SET level=10 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let c = &s.tables.hero_shop.heroes[&1];
    let skill = item::n(c, "SkillIndex1");
    let price: i64 = s
        .tables
        .hero_shop
        .skill_prices
        .iter()
        .filter(|v| v["SlotIndex"] == 1 && item::n(v, "Level") < 3)
        .map(|v| item::n(v, "ReqGold"))
        .sum();
    let before = balance(&s, &u, "gold").await;
    let req = format!("HeroIndex=1&SkillIndices=[{skill}]&TargetLevels=[3]");
    assert_eq!(
        hero::upgrade_hero_skill(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gold").await, before - price);
    assert_eq!(get(&s, &u, 1).await["SkillLevel1"], 3);
    assert_ne!(
        hero::upgrade_hero_skill(State(s.clone()), form(&u, &req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let book = s
        .tables
        .hero_shop
        .books
        .iter()
        .find(|v| v["TagType"] == c["TagType"] && v["Grade"] == 1)
        .unwrap();
    let book = item::n(book, "ItemIndex") as i32;
    put(&s, &u, book, 20).await;
    assert_eq!(
        hero::extend_hero_skill(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&SkillIndex={skill}&SkillExtend=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, book).await, 0);
    assert_eq!(login(&s).await.heroes[0].details["SkillExtend1"], 1);
}
#[tokio::test]
async fn awakening_requires_challenge_battle_purification_then_consumes_essence_once() {
    let (s, u) = setup().await;
    let c = &s.tables.hero_shop.heroes[&1];
    let row = s
        .tables
        .hero_shop
        .challenges
        .iter()
        .find(|v| v["TagType"] == c["TagType"])
        .unwrap();
    for i in 1..=3 {
        put(&s, &u, item::n(row, &format!("ItemIndex{i}")) as i32, 5).await;
    }
    let r = hero::get_awake_material_challenge(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ItemResults"].as_array().unwrap().len(), 3);
    assert_ne!(
        hero::upgrade_hero_star(State(s.clone()), form(&u, "HeroIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let awake = s
        .tables
        .hero_shop
        .awake
        .iter()
        .find(|v| v["HeroIndex"] == 1)
        .unwrap();
    let chapter = item::n(awake, "AwakeChapter") as i32;
    let dungeon = item::n(awake, "AwakeDungeon1") as i32;
    assert!(
        hero::trial(&s, u.user_info.account_id, chapter, dungeon, Some(true))
            .await
            .is_err()
    );
    let _ = campaign::begin_campaign(
        State(s.clone()),
        form(
            &u,
            &format!("ChapterIndex={chapter}&DungeonIndex={dungeon}"),
        ),
    )
    .await
    .unwrap();
    let r = campaign::end_campaign(
        State(s.clone()),
        form(
            &u,
            &format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&Completed=true"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r.item_results.len(), 1);
    let essence = r.item_results[0].item_index;
    assert!(
        hero::trial(&s, u.user_info.account_id, chapter, dungeon, Some(true))
            .await
            .is_err()
    );
    let r = hero::max_purify_hero(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["PurifyResult"]["NewValue"], 1000);
    assert_eq!(count(&s, &u, essence).await, 1);
    let r = hero::upgrade_hero_star(State(s.clone()), form(&u, "HeroIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["NewHeroStar"], 2);
    assert_eq!(count(&s, &u, essence).await, 0);
    assert_eq!(login(&s).await.heroes[0].star, 2);
}
#[tokio::test]
async fn transcend_pages_parse_native_pairs_enforce_points_and_persist() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE heroes SET star=5,transcend=1 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let r = hero::learn_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=1&TranscendSkills=[10,1]"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["Hero"]["TranscendSkillPage1"], "[10]");
    let r = hero::learn_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=1&TranscendSkills=[11,1]"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "NotEnoughSkillPoint");
    let cost = s.tables.hero_shop.constant("AddTranscendSkillPageGem2", 0);
    assert_eq!(
        hero::buy_hero_transcend_skill_page(
            State(s.clone()),
            form(&u, &format!("HeroIndex=1&PageIndex=2&BuyGem={cost}"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let _ = hero::apply_hero_transcend_skill_page(
        State(s.clone()),
        form(&u, "HeroIndex=1&PageIndex=2"),
    )
    .await
    .unwrap();
    let h = login(&s).await.heroes.remove(0);
    assert_eq!(h.transcend, 1);
    assert_eq!(h.details["ApplySkillPage"], 2);
    assert_eq!(
        hero::reset_hero_transcend_skill_page(
            State(s.clone()),
            form(&u, "HeroIndex=1&PageIndex=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(get(&s, &u, 1).await["TranscendSkillPage1"], "[]");
}
#[tokio::test]
async fn inn_shop_lists_actual_item_codes_and_atomic_friendship_purchases() {
    let (s, u) = setup().await;
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    let list = r["ShopItems"].as_array().unwrap();
    assert!(!list.is_empty());
    let row = &list[0];
    assert!(row["ItemCode"].is_string());
    let index = item::n(row, "ListNo");
    sqlx::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = format!("ShopIndex=10&ShopItemIndex={index}&ShopItemPurchaseCount=2");
    let r = shop::buy_shop_item(State(s.clone()), form(&u, &req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ShopItem"]["Purchased"], 2);
    let before = balance(&s, &u, "friendship_point").await;
    assert_ne!(
        shop::buy_shop_item(
            State(s.clone()),
            form(
                &u,
                &format!("ShopIndex=10&ShopItemIndex={index}&ShopItemPurchaseCount=-1")
            )
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    let r = shop::get_all_shop_item_purchase_count(State(s.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap()
        .0;
    assert_eq!(
        r["ShopItems"]
            .as_array()
            .unwrap()
            .iter()
            .find(|r| item::n(r, "ListNo") == index)
            .unwrap()["Purchased"],
        2
    );
}
#[tokio::test]
async fn stock_restock_and_capacity_failure_preserve_currency() {
    let (s, u) = setup().await;
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["ShopItems"].as_array().unwrap().len(), 6);
    let before = balance(&s, &u, "gem").await;
    let r = shop::request_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, before - 25);
    assert_eq!(r["RestockTimeInfo"]["ShopItemListIndex"], 2);
    sqlx::query("UPDATE shop_stock SET restock_time=1 WHERE account_id=? AND shop_index=1")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let gold = balance(&s, &u, "gold").await;
    assert_eq!(
        shop::buy_shop_item(
            State(s.clone()),
            form(&u, "ShopIndex=1&ListNo=1&ShopItemPurchaseCount=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "InvalidShopItemListIndex"
    );
    assert_eq!(balance(&s, &u, "gold").await, gold);
    let r = shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(r["RestockTimeInfo"]["ShopItemListIndex"], 3);
    assert_ne!(
        shop::get_shop_list(State(s.clone()), form(&u, "ShopIndex=99999"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
}
#[tokio::test]
async fn npc_shop_discount_preserves_daily_friendship_earnings() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    sqlx::query("INSERT INTO friend_daily(account_id,day,points) VALUES (?,?,340)")
        .bind(u.user_info.account_id)
        .bind(s.server_date())
        .execute(&s.db)
        .await
        .unwrap();
    let req = "ShopIndex=10&ShopItemIndex=1&ShopItemPurchaseCount=1";
    let r = shop::buy_shop_item(State(s.clone()), form(&u, req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    let full = -item::n(&r["FriendshipPointResult"], "AddValue");
    assert_eq!(r["FriendshipPointResult"]["NewDailyAccValue"], 340);
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star) VALUES (?,2,94,2)")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let r = shop::buy_shop_item(State(s.clone()), form(&u, req))
        .await
        .unwrap()
        .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(
        -item::n(&r["FriendshipPointResult"], "AddValue"),
        (full * 60 + 99) / 100
    );
    assert_eq!(r["FriendshipPointResult"]["NewDailyAccValue"], 340);
}
#[tokio::test]
async fn limits_spend_materials_then_limit_exp_levels_once() {
    let (s, u) = setup().await;
    sqlx::query("UPDATE heroes SET star=5,transcend=5,level=100 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let row = s
        .tables
        .hero_shop
        .limit_breaks
        .iter()
        .find(|v| v["HeroIndex"] == 1 && v["LimitBreakLevel"] == 1)
        .unwrap();
    for i in 1..=3 {
        put(
            &s,
            &u,
            item::n(row, &format!("MaterialItemIndex{i}")) as i32,
            item::n(row, &format!("MaterialItemCount{i}")),
        )
        .await;
    }
    assert_eq!(
        hero::hero_limit_break_level_up(State(s.clone()), form(&u, "HeroIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let exp = &s.tables.hero_shop.limit_exp_items[0];
    let id = item::n(exp, "ItemIndex");
    let qty =
        (item::n(row, "ReqLocalExp") + item::n(exp, "ExpAmount") - 1) / item::n(exp, "ExpAmount");
    put(&s, &u, id as i32, qty).await;
    let r = hero::hero_limit_break_exp_up(
        State(s.clone()),
        form(
            &u,
            &format!("HeroIndex=1&ExpItemIndices=[{id}]&ExpItemCounts=[{qty}]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["NewHeroLevel"], 101);
    assert_eq!(count(&s, &u, id as i32).await, 0);
    assert_eq!(login(&s).await.heroes[0].details["LimitBreakLevel"], 1);
}

#[tokio::test]
async fn selectors_growth_items_and_costume_bonuses_validate_allowed_targets() {
    let (s, u) = setup().await;
    let selector = s
        .tables
        .hero_shop
        .costume_selectors
        .iter()
        .find(|v| {
            item::n(v, "CostumeSelectItemType") == 0
                && v["CostumeIndices"]
                    .as_array()
                    .is_some_and(|a| a.contains(&serde_json::json!(106)))
        })
        .unwrap();
    let id = item::n(selector, "ItemIndex") as i32;
    put(&s, &u, id, 1).await;
    assert_ne!(
        hero::use_costume_select_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&CostumeIndex=999999"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, id).await, 1);
    assert_eq!(
        hero::use_costume_select_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&CostumeIndex=106"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let bonus = hero::costume_boost(&s, u.user_info.account_id)
        .await
        .unwrap();
    assert!(bonus.0 > 0 || bonus.1 > 0);
    assert_eq!(
        item::campaign_boost(&s, u.user_info.account_id)
            .await
            .unwrap(),
        bonus
    );
    let growth = s
        .tables
        .hero_shop
        .growth_items
        .iter()
        .find(|v| {
            v["HeroIndices"]
                .as_array()
                .unwrap()
                .contains(&serde_json::json!(1))
        })
        .unwrap();
    let id = item::n(growth, "ItemIndex") as i32;
    put(&s, &u, id, 2).await;
    assert_eq!(
        hero::use_hero_growth_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&HeroIndex=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let h = get(&s, &u, 1).await;
    assert_eq!(h["Star"], 5);
    assert_eq!(h["Transcended"], 5);
    assert_eq!(h["Level"], 100);
    assert_ne!(
        hero::use_hero_growth_item(
            State(s.clone()),
            form(&u, &format!("ItemIndex={id}&HeroIndex=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&s, &u, id).await, 1);
    // Existing campaign code must not lower a hero grown to level 100.
    let _ = campaign::end_campaign(
        State(s.clone()),
        form(&u, "ChapterIndex=1&DungeonIndex=1&Completed=true"),
    )
    .await
    .unwrap();
    assert_eq!(get(&s, &u, 1).await["Level"], 100);
}
#[tokio::test]
async fn multiple_hero_selector_grants_two_allowed_heroes_and_rolls_back_partial_failure() {
    let (s, u) = setup().await;
    let row = s
        .tables
        .hero_shop
        .multi_hero_items
        .iter()
        .find(|v| v["HeroStar"] == 5)
        .unwrap();
    let id = item::n(row, "ItemIndex") as i32;
    let left = row["HeroIndices1"][0].as_i64().unwrap();
    let right = row["HeroIndices2"][0].as_i64().unwrap();
    put(&s, &u, id, 1).await;
    let r = hero::use_multiple_hero_select_item(
        State(s.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&LeftHeroIndices=[{left}]&RightHeroIndices=[99999]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(r["Result"], "Success");
    assert_eq!(login(&s).await.heroes.len(), 1);
    assert_eq!(count(&s, &u, id).await, 1);
    let r = hero::use_multiple_hero_select_item(
        State(s.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&LeftHeroIndices=[{left}]&RightHeroIndices=[{right}]"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(r["HeroResults"].as_array().unwrap().len(), 2);
    assert_eq!(login(&s).await.heroes.len(), 3);
}
#[tokio::test]
async fn hero_presets_save_restore_and_clearing_preserves_paid_slot() {
    let (s, u) = setup().await;
    let free = s
        .tables
        .hero_shop
        .preset_slots
        .iter()
        .find(|(_, v)| v["IsPaid"] == false)
        .unwrap()
        .0
        .clone();
    let paid = s
        .tables
        .hero_shop
        .preset_slots
        .iter()
        .find(|(_, v)| v["IsPaid"] == true)
        .unwrap()
        .0
        .clone();
    let c = &s.tables.hero_shop.heroes[&1];
    let mut db = s.db.begin().await.unwrap();
    let r = hero::recruit_at(&mut db, &s, u.user_info.account_id, 2, 1, 1, 0)
        .await
        .unwrap();
    assert_eq!(r["HeroInfo"]["HeroIndex"], 2);
    db.commit().await.unwrap();
    let equipped: Option<i32> =
        sqlx::query_scalar("SELECT slot_index FROM equip_items WHERE account_id=? LIMIT 1")
            .bind(u.user_info.account_id)
            .fetch_optional(&s.db)
            .await
            .unwrap();
    let slot = if let Some(i) = equipped {
        i
    } else {
        let id = *s
            .tables
            .hero_shop
            .items
            .keys()
            .find(|i| {
                s.tables
                    .items
                    .reward_item(**i)
                    .is_some_and(|v| v.kind == "Equip")
            })
            .unwrap();
        let mut tx = s.db.begin().await.unwrap();
        let mut r = super::tutorial::Rewards::default();
        item::give(&mut tx, &s, u.user_info.account_id, id, 1, 0, 0, &mut r)
            .await
            .unwrap();
        tx.commit().await.unwrap();
        r.equipment[0].slot_index
    };
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=? WHERE account_id=? AND hero_index=1")
        .bind(slot)
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::add_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=0 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        hero::set_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(get(&s, &u, 1).await["EquipItemSlotIndex1"], slot);
    sqlx::query("UPDATE equip_items SET inventory_type=1 WHERE account_id=? AND slot_index=?")
        .bind(u.user_info.account_id)
        .bind(slot)
        .execute(&s.db)
        .await
        .unwrap();
    assert_ne!(
        hero::set_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={free}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_ne!(
        hero::add_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let before = balance(&s, &u, "gem").await;
    let price = item::n(&s.tables.hero_shop.preset_slots[&paid], "BuyGem");
    assert_eq!(
        hero::buy_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let _ =
        hero::remove_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap();
    assert_ne!(
        hero::buy_hero_storage_slot(State(s.clone()), form(&u, &format!("StorageKey={paid}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gem").await, before - price);
    let _ = c;
    let _ = hero::bookmark_hero(State(s.clone()), form(&u, "HeroIndex=[2,1]"))
        .await
        .unwrap();
    let r = super::lobby::first_lobby(
        State(s.clone()),
        axum::extract::Form(super::lobby::FirstLobbyRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            nick: None,
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r.player_book_mark_hero_info["BookMarkHeroIndices"], "2,1");
    assert_eq!(r.hero_preset_storages.len(), 2);
}
#[tokio::test]
async fn shop_purchase_limits_reset_and_failed_delivery_does_not_charge() {
    let (mut s, u) = setup().await;
    let table = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).hero_shop);
    let r = table
        .shop_items
        .iter_mut()
        .find(|v| v["ShopIndex"] == 10 && v["Index"] == 1)
        .unwrap();
    r["PurchasableCount"] = serde_json::json!(1);
    r["DailyReset"] = serde_json::json!(true);
    sqlx::query("UPDATE user_info SET friendship_point=10000 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    let req = "ShopIndex=10&ShopItemIndex=1&ShopItemPurchaseCount=1";
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let before = balance(&s, &u, "friendship_point").await;
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "SoldOut"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    sqlx::query("UPDATE shop_purchase_ledger SET period='day:2000-01-01' WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&s.db)
        .await
        .unwrap();
    assert_eq!(
        shop::buy_shop_item(State(s.clone()), form(&u, req))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let bad = std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut s.tables).inventory);
    bad.items.get_mut(&41002).unwrap()["ItemMaxCap"] = serde_json::json!(1);
    put(&s, &u, 41002, 1).await;
    let before = balance(&s, &u, "friendship_point").await;
    assert_ne!(
        shop::buy_shop_item(
            State(s.clone()),
            form(&u, "ShopIndex=10&ShopItemIndex=2&ShopItemPurchaseCount=1")
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "friendship_point").await, before);
    assert_eq!(count(&s, &u, 41002).await, 1);
}
#[tokio::test]
async fn inn_recruitment_uses_table_star_account_local_ids_and_cannot_duplicate_shop_heroes() {
    let (s, u) = setup().await;
    let (id, item, cost) = buy_data(&s);
    let _ = hero::buy_hero(
        State(s.clone()),
        form(
            &u,
            &format!("HeroIndex={id}&ItemIndex={item}&BuyGem={cost}"),
        ),
    )
    .await
    .unwrap();
    sqlx::query("INSERT INTO hero_friendly_info(account_id,hero_index,friendly_point) VALUES (?,?,1000) ON CONFLICT(account_id) DO UPDATE SET hero_index=excluded.hero_index,friendly_point=excluded.friendly_point").bind(u.user_info.account_id).bind(id).execute(&s.db).await.unwrap();
    let r = super::hero_inn::recruit_hero(
        State(s.clone()),
        axum::extract::Form(super::hero_inn::RecruitHeroRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            hero_index: Some(id),
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "AlreadyRecruited");
    let id = if id == 2 { 3 } else { 2 };
    sqlx::query(
        "UPDATE hero_friendly_info SET hero_index=?,friendly_point=100000 WHERE account_id=?",
    )
    .bind(id)
    .bind(u.user_info.account_id)
    .execute(&s.db)
    .await
    .unwrap();
    let r = super::hero_inn::recruit_hero(
        State(s.clone()),
        axum::extract::Form(super::hero_inn::RecruitHeroRequest {
            session_id: None,
            session_key: Some(u.user_info.session_key.clone()),
            hero_index: Some(id),
        }),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(r["Result"], "Success");
    assert_eq!(
        r["HeroResult"]["HeroInfo"]["Star"],
        s.tables.hero_shop.heroes[&id]["StartHeroStar"]
    );
    assert!(r["HeroResult"]["TeamExpResult"].is_object());
    assert_eq!(login(&s).await.heroes.len(), 3);
}
