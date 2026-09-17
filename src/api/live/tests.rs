use super::*;
use crate::{api::account::user, database, tables::GameTables};
use std::{
    path::Path,
    sync::{Arc, OnceLock},
};
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
        user::login(State(s.clone()), Bytes::from("LoginId=live-test"))
            .await
            .unwrap()
            .0
    );
    sqlx::query("UPDATE user_info SET gold=100000000,gem=100000,friendship_point=10000")
        .execute(&s.db)
        .await
        .unwrap();
    (s, u)
}
fn account(u: &Value) -> i64 {
    n(&u["UserInfo"], "AccountId")
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
fn ok(v: &Value) {
    assert_eq!(v["Result"], "Success", "{v}");
}
async fn seed(s: &AppState, u: &Value, id: i32, count: i32) {
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES(?,?,?) ON CONFLICT(account_id,item_index) DO UPDATE SET count=count+excluded.count").bind(account(u)).bind(id).bind(count).execute(&s.db).await.unwrap();
}
async fn balance(s: &AppState, u: &Value, col: &str) -> i64 {
    sqlx::query_scalar(&format!("SELECT {col} FROM user_info WHERE account_id=?"))
        .bind(account(u))
        .fetch_one(&s.db)
        .await
        .unwrap()
}
#[tokio::test]
async fn noncash_offers_enforce_prices_limits_and_restore() {
    let (s, u) = setup().await;
    let before = balance(&s, &u, "gem").await;
    let v = call(&s, &u, "shop/buy_payshop_product", "Index=920001").await;
    ok(&v);
    assert_eq!(balance(&s, &u, "gem").await, before - 80);
    assert_ne!(
        call(&s, &u, "shop/buy_payshop_product", "Index=920001").await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "shop/buy_payshop_product", "Index=1").await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gem").await, before - 80);
    let snap = snapshot(&s, account(&u)).await.unwrap();
    assert_eq!(
        snap["PlayerProductPurchaseInfos"]
            .as_array()
            .unwrap()
            .iter()
            .find(|v| v["ProductIndex"] == 920001)
            .unwrap()["PurchasedCount"],
        1
    );
}
#[tokio::test]
async fn selection_stock_is_stable_and_restock_cannot_reset_purchase_limit() {
    let (s, u) = setup().await;
    let x = call(
        &s,
        &u,
        "shop/get_select_shop_info",
        "CategoryGroup=Fragment",
    )
    .await;
    ok(&x);
    let y = call(
        &s,
        &u,
        "shop/get_select_shop_info",
        "CategoryGroup=Fragment",
    )
    .await;
    assert_eq!(x, y);
    let id = n(&x["SelectShopInfo"]["PayShopProductInfos"][0], "Index");
    ok(&call(&s, &u, "shop/buy_payshop_product", &format!("Index={id}")).await);
    ok(&call(
        &s,
        &u,
        "shop/restock_select_shop_info",
        "CategoryGroup=Fragment",
    )
    .await);
    assert_ne!(
        call(&s, &u, "shop/buy_payshop_product", &format!("Index={id}")).await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn free_ticket_and_currency_summons_are_atomic() {
    let (s, u) = setup().await;
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=3&Free=true",
    )
    .await);
    assert_ne!(
        call(
            &s,
            &u,
            "equip_gacha/exec_equip_gacha",
            "GachaIndex=3&Free=true"
        )
        .await["Result"],
        "Success"
    );
    seed(&s, &u, 200002, 1).await;
    assert_ne!(
        call(
            &s,
            &u,
            "equip_gacha/exec_equip_gacha",
            "GachaIndex=4&ItemIndex=200002"
        )
        .await["Result"],
        "Success"
    );
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=2&ItemIndex=200002",
    )
    .await);
    let gold = balance(&s, &u, "gold").await;
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=2&Discount=true&DiscountedCost=0",
    )
    .await);
    assert_eq!(balance(&s, &u, "gold").await, gold - 100000);
}
#[tokio::test]
async fn ceiling_is_claimed_once_and_pet_summons_create_owned_pets() {
    let (mut s, u) = setup().await;
    Arc::make_mut(&mut Arc::make_mut(&mut s.tables).live).rules["Summons"]["28"]["CeilingCount"] =
        json!(1);
    let v = call(&s, &u, "pet/exec_pet_gacha", "GachaIndex=28").await;
    ok(&v);
    assert_eq!(v["PetGachaItemResults"].as_array().unwrap().len(), 1);
    assert_eq!(
        snapshot(&s, account(&u)).await.unwrap()["PetInfos"]
            .as_array()
            .unwrap()
            .len(),
        1
    );
    ok(&call(&s, &u, "pet/pet_gacha_roof_reward", "GachaIndex=28").await);
    assert_ne!(
        call(&s, &u, "pet/pet_gacha_roof_reward", "GachaIndex=28").await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn incubation_checks_time_identity_and_replay() {
    let (s, u) = setup().await;
    seed(&s, &u, 7500001, 1).await;
    ok(&call(
        &s,
        &u,
        "pet/set_egg_in_pet_incubator",
        "SlotIndex=101&IncubatorIndex=11001&ItemIndex=7500001",
    )
    .await);
    assert_ne!(
        call(
            &s,
            &u,
            "pet/get_egg_rewards",
            "SlotIndex=101&IncubatorIndex=11001&ItemIndex=7500001"
        )
        .await["Result"],
        "Success"
    );
    sqlx::query("UPDATE extension_state SET data=json_set(data,'$.CompletedTime','2000-01-01 00:00:00') WHERE kind='pet_slot'").execute(&s.db).await.unwrap();
    let v = call(
        &s,
        &u,
        "pet/get_egg_rewards",
        "SlotIndex=101&IncubatorIndex=11001&ItemIndex=7500001",
    )
    .await;
    ok(&v);
    assert_eq!(v["PetAddResultInfos"].as_array().unwrap().len(), 1);
    assert_ne!(
        call(
            &s,
            &u,
            "pet/get_egg_rewards",
            "SlotIndex=101&IncubatorIndex=11001&ItemIndex=7500001"
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn pets_enforce_ownership_food_exploration_and_tier_materials() {
    let (s, u) = setup().await;
    assert_ne!(
        call(&s, &u, "pet/feed_the_pet", "PetIndex=7100000&FeedCount=1").await["Result"],
        "Success"
    );
    pets::add(
        &mut *s.db.acquire().await.unwrap(),
        &s,
        account(&u),
        7100000,
    )
    .await
    .unwrap();
    ok(&call(&s, &u, "pet/feed_the_pet", "PetIndex=7100000&FeedCount=4").await);
    ok(&call(
        &s,
        &u,
        "pet/pet_start_explore",
        "ExploreIndex=10101&DeckIndex=1&PetIndices=[7100000]",
    )
    .await);
    assert_ne!(
        call(
            &s,
            &u,
            "pet/pet_end_explore",
            "ExploreIndex=10101&DeckIndex=1"
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(
            &s,
            &u,
            "pet/change_pet_avatar",
            "AvatarPetIndex=7100000&PetIndex=7100000"
        )
        .await["Result"],
        "Success"
    );
    ok(&call(
        &s,
        &u,
        "pet/pet_cancel_explore",
        "ExploreIndex=10101&DeckIndex=1",
    )
    .await);
    seed(&s, &u, 20111, 10).await;
    ok(&call(
        &s,
        &u,
        "pet/pet_upgrade_tier",
        "PetSouls=[20111,20111,20111,20111,20111,20111,20111,20111,20111,20111]",
    )
    .await);
}
#[tokio::test]
async fn event_contributions_rewards_and_craft_rollback() {
    let (s, u) = setup().await;
    ok(&call(&s, &u, "shop/buy_payshop_product", "Index=920002").await);
    for _ in 0..2 {
        ok(&call(&s, &u, "event_step/put_event_step", "").await);
    }
    let v = call(
        &s,
        &u,
        "event_step/get_event_step_reward",
        "EventStepType=Daily",
    )
    .await;
    ok(&v);
    assert_eq!(v["RewardStep"], 1);
    assert_ne!(
        call(
            &s,
            &u,
            "event_step/get_event_step_reward",
            "EventStepType=Daily"
        )
        .await["Result"],
        "Success"
    );
    assert_ne!(
        call(&s, &u, "event_step/put_event_step", "").await["Result"],
        "Success"
    );
    let recipe = row(&s, "EventCraft", &[("Index", 1)]).unwrap();
    let material = s
        .tables
        .get_item_index(recipe["MaterialItemCode1"].as_str().unwrap())
        .unwrap();
    seed(&s, &u, material, 10).await;
    assert_ne!(
        call(&s, &u, "item/event_craft_item", "CraftIndex=1&CraftCount=1").await["Result"],
        "Success"
    );
    let left: i64 =
        sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=?")
            .bind(account(&u))
            .bind(material)
            .fetch_one(&s.db)
            .await
            .unwrap();
    assert_eq!(left, 10);
    let material2 = s
        .tables
        .get_item_index(recipe["MaterialItemCode2"].as_str().unwrap())
        .unwrap();
    seed(&s, &u, material2, 1).await;
    ok(&call(&s, &u, "item/event_craft_item", "CraftIndex=1&CraftCount=1").await);
    let before = balance(&s, &u, "friendship_point").await;
    ok(&call(
        &s,
        &u,
        "item/reward_event_roulette",
        "RouletteIndex=1&RouletteCount=2",
    )
    .await);
    assert_eq!(balance(&s, &u, "friendship_point").await, before - 400);
}
#[tokio::test]
async fn forge_validates_level_materials_and_exchange_replay() {
    let (s, u) = setup().await;
    let v = call(&s, &u, "shop/buy_payshop_product", "Index=920003").await;
    ok(&v);
    let eq = &v["EquipItemInfos"][0];
    let slot = n(eq, "SlotIndex");
    let level = n(eq, "Level") + 1;
    let request = Bytes::from(format!(
        "SessionKey={}&EquipItemSlotIndex={slot}&UpgradeLevel={level}",
        u["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    let normal = crate::api::extensions::handle(
        State(s.clone()),
        OriginalUri("/equip/upgrade_equip".parse().unwrap()),
        request,
    )
    .await
    .unwrap()
    .0;
    assert_ne!(normal["Result"], "Success");
    let request = Bytes::from(format!(
        "SessionKey={}&HeroIndex=1&HeroPartIndex=[5]&EquipItemSlotIndex=[{slot}]",
        u["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    assert!(
        crate::api::inventory::equip::set_equip(State(s.clone()), request)
            .await
            .is_err()
    );
    let v = call(
        &s,
        &u,
        "event_equip/event_forge",
        &format!("EquipItemSlotIndex={slot}&UpgradeLevel={level}&IsFixed=true"),
    )
    .await;
    ok(&v);
    assert_eq!(v["Success"], true);
    assert_ne!(
        call(
            &s,
            &u,
            "event_equip/event_forge",
            &format!("EquipItemSlotIndex={slot}&UpgradeLevel={level}&IsFixed=true")
        )
        .await["Result"],
        "Success"
    );
    ok(&call(
        &s,
        &u,
        "event_equip/event_exchange_equip",
        &format!("EquipItemSlotIndices=[{slot}]"),
    )
    .await);
    assert_ne!(
        call(
            &s,
            &u,
            "event_equip/event_exchange_equip",
            &format!("EquipItemSlotIndices=[{slot}]")
        )
        .await["Result"],
        "Success"
    );
}
#[tokio::test]
async fn purchase_dungeon_uses_table_price_and_persists_booster() {
    let (s, u) = setup().await;
    assert!(validate_purchase_dungeon(
        &mut *s.db.acquire().await.unwrap(),
        &s,
        account(&u),
        40001,
        1
    )
    .await
    .is_err());
    assert_ne!(
        call(
            &s,
            &u,
            "shop/buy_purchase_dungeon",
            "ChapterIndex=40001&DungeonIndex=1&Price=0"
        )
        .await["Result"],
        "Success"
    );
    let v = call(
        &s,
        &u,
        "shop/buy_purchase_dungeon",
        "ChapterIndex=40001&DungeonIndex=1&Price=200",
    )
    .await;
    ok(&v);
    assert!(v["ItemTimeDurations"][0]["EndTime"].as_str().is_some());
    validate_purchase_dungeon(
        &mut *s.db.acquire().await.unwrap(),
        &s,
        account(&u),
        40001,
        1,
    )
    .await
    .unwrap();
    let body = Bytes::from(format!(
        "SessionKey={}&ChapterIndex=40001&DungeonIndex=1&DungeonDifficulty=0&HeroIndices=[1]",
        u["UserInfo"]["SessionKey"].as_str().unwrap()
    ));
    ok(
        &crate::api::battle::execute_request(&s, "campaign/begin_campaign", body)
            .await
            .unwrap(),
    );
}

#[tokio::test]
async fn pet_care_duplicate_souls_and_layout_survive_login() {
    let (s, u) = setup().await;
    let a = account(&u);
    let mut rewards = Rewards::default();
    item::give(
        &mut *s.db.acquire().await.unwrap(),
        &s,
        a,
        7400000,
        2,
        0,
        0,
        &mut rewards,
    )
    .await
    .unwrap();
    assert_eq!(rewards.pets.len(), 1);
    assert_eq!(rewards.items.len(), 1);
    ok(&call(&s, &u, "pet/pet_awaken", "PetIndex=7400000").await);
    assert_ne!(
        call(&s, &u, "pet/pet_awaken", "PetIndex=7400000").await["Result"],
        "Success"
    );
    ok(&call(&s, &u, "pet/change_pet_layout", "PetIndices=[7400000,0,0]").await);
    ok(&call(
        &s,
        &u,
        "pet/pet_interactive",
        "PetIndex=7400000&ActionType=Play&ActionCount=4",
    )
    .await);
    ok(&call(&s, &u, "pet/pet_happy_gift", "PetIndex=7400000").await);
    assert_ne!(
        call(&s, &u, "pet/pet_happy_gift", "PetIndex=7400000").await["Result"],
        "Success"
    );
    let snap = snapshot(&s, a).await.unwrap();
    assert_eq!(snap["PetInfos"][0]["Star"], 1);
    assert_eq!(
        snap["PetMiscInfos"]
            .as_array()
            .unwrap()
            .iter()
            .find(|v| v["MiscKey"] == 2)
            .unwrap()["MiscValue"],
        "7400000,0,0"
    );
}
#[tokio::test]
async fn pickup_step_up_and_native_pet_summon_contracts() {
    let (s, u) = setup().await;
    let snap = call(&s, &u, "equip_gacha/get_equip_gacha", "").await;
    assert!(snap["EquipGachaInfos"]
        .as_array()
        .unwrap()
        .iter()
        .all(|v| v["OnSale"] == true && v["IsUTCBeginTime"] == 1));
    put(
        &mut *s.db.acquire().await.unwrap(),
        account(&u),
        "summon",
        18,
        &json!({"GachaIndex":18,"GachaCount":99,"StarterDraws":9,"Claimed":0,"Day":day()}),
    )
    .await
    .unwrap();
    let starter = call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=18&HeroIndices=[1,2,3,4]",
    )
    .await;
    ok(&starter);
    assert_eq!(starter["StarterPickupEquipGachaInfo"]["GachaCount"], 10);
    assert_ne!(
        call(
            &s,
            &u,
            "equip_gacha/exec_equip_gacha",
            "GachaIndex=18&HeroIndices=[1,2,3,4]"
        )
        .await["Result"],
        "Success"
    );
    ok(&call(
        &s,
        &u,
        "equip_gacha/get_starter_pickup_equip_gacha_last_reward",
        "GachaIndex=18",
    )
    .await);
    assert_ne!(
        call(
            &s,
            &u,
            "equip_gacha/get_starter_pickup_equip_gacha_last_reward",
            "GachaIndex=18"
        )
        .await["Result"],
        "Success"
    );
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=4&HighGachaCategory=All",
    )
    .await);
    let before = balance(&s, &u, "gem").await;
    assert_ne!(
        call(
            &s,
            &u,
            "equip_gacha/exec_equip_gacha",
            "GachaIndex=5&HeroIndices=[1,1,2,3]"
        )
        .await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gem").await, before);
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=5&HeroIndices=[1,2,3,4]",
    )
    .await);
    ok(&call(&s, &u, "equip_gacha/exec_equip_gacha", "GachaIndex=13").await);
    let before = balance(&s, &u, "gem").await;
    let v = call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        "GachaIndex=13&DiscountedCost=0",
    )
    .await;
    ok(&v);
    assert_eq!(balance(&s, &u, "gem").await, before - 2500);
    assert_eq!(v["StepUpEquipGachaInfo"]["CompletedStep"], 2);
    let v = call(&s, &u, "equip_gacha/exec_equip_gacha", "GachaIndex=28").await;
    ok(&v);
    assert!(v["GachaItemResults"][0]["PetItemResult"]["PetIndex"]
        .as_i64()
        .is_some());
}
#[tokio::test]
async fn concurrent_purchase_limit_and_capacity_failure_do_not_spend() {
    let (s, u) = setup().await;
    let (v, w) = tokio::join!(
        call(&s, &u, "shop/buy_payshop_product", "Index=920001"),
        call(&s, &u, "shop/buy_payshop_product", "Index=920001")
    );
    assert_eq!(
        [v, w].iter().filter(|v| v["Result"] == "Success").count(),
        1
    );
    let before = balance(&s, &u, "gold").await;
    sqlx::query("WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<300) INSERT INTO equip_items(account_id,item_index,created_time) SELECT ?,1001,datetime('now') FROM n").bind(account(&u)).execute(&s.db).await.unwrap();
    assert_ne!(
        call(&s, &u, "shop/buy_payshop_product", "Index=920003").await["Result"],
        "Success"
    );
    assert_eq!(balance(&s, &u, "gold").await, before);
}
