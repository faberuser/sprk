use super::*;
use crate::models::item::ItemGrant;
use crate::{api::account::user, database};
use std::sync::Arc;
async fn setup() -> (AppState, Value) {
    let db = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&db).await.unwrap();
    let s = AppState::new(db, crate::tables::test_tables());
    let u = json!(
        user::test_login(State(s.clone()), Bytes::from("LoginId=live-test"))
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
fn daily_limit(s: &mut AppState, indices: &[i64]) {
    let rules = &mut Arc::make_mut(&mut Arc::make_mut(&mut s.tables).live).rules;
    for product in rules["Products"].as_array_mut().unwrap() {
        if indices.contains(&n(product, "Index")) {
            product["PurchasableCount"] = json!(1);
            product["ResetSeconds"] = json!(86400);
            product["DailyReset"] = json!(true);
            product["ResetType"] = json!(1);
        }
    }
}

#[tokio::test]
async fn recovered_shop_is_delivered_to_native_lobby_with_readable_prices_and_items() {
    let (s, u) = setup().await;
    let lobby = json!(crate::api::account::lobby::enter_lobby(State(s.clone()),
        axum::extract::Form(crate::api::account::lobby::EnterLobbyRequest {
            session_id: None, session_key: Some(u["UserInfo"]["SessionKey"].as_str().unwrap().into()),
        })).await.unwrap().0);
    let products = lobby["PayShopProductInfos"].as_array().unwrap();
    assert!(products.len() > 1300);
    let mut indices = std::collections::HashSet::new();
    for product in products {
        assert!(indices.insert(n(product, "Index")));
        assert!(product["ProductType"].is_string());
        assert!(product["PriceType"].is_string());
        if [3094170, 3094594, 3094595].contains(&n(product, "Index")) {
            assert_eq!(product["Price"], 0);
            assert_eq!(product["Flag"], "Free");
        } else {
            assert!(n(product, "Price") > 0);
        }
        assert_eq!(product["Begin"], 0);
        assert_eq!(product["End"], 0);
        assert_eq!(product["BeginTime"], "");
        assert_eq!(product["EndTime"], "");
        assert_eq!(product["Durational"], 0);
        let free = n(product, "Price") == 0;
        assert_eq!(product["PurchasableCount"], if free { 1 } else { 0 });
        assert_eq!(product["ResetType"], if free { "Daily" } else { "None" });
        assert_eq!(product["DailyReset"], free);
        for field in ["GoogleId", "AppleId", "OneStoreId", "UDPStoreId"] {
            assert!(product[field].is_null() || product[field] == "");
        }
        for grant in product["ItemInfos"].as_array().into_iter().flatten() {
            assert!(s.tables.items.reward_item(n(grant, "ItemIndex") as i32).is_some());
            assert!(n(grant, "ItemCount") > 0);
        }
    }
    for category in ["Beginner", "BirthDay", "Booster", "Convenience", "Daily", "Equipment", "Event", "Gold", "Growth", "HotNew", "Lua", "Mileage", "Package", "Return", "Ruby", "Stamina", "Technomagic"] {
        assert!(products.iter().any(|p| p["Category"] == category), "Missing category {category}");
    }
    let token = products.iter().find(|p| p["Category"] == "Lua").unwrap();
    assert_eq!(token["PriceType"], "LuaPoint");
    let package = products.iter().find(|p| p["Index"] == 3093009).unwrap();
    assert_eq!(package["ProductName"], "Mini Unique Weapon Random Package");
    assert_eq!(package["UseItemCode"][0], "PACKAGE_PAY_WEAPON_UNIQUE_1");
    assert_eq!(package["ItemInfos"][0]["ItemIndex"], 93009);
    assert_eq!(package["ResetType"], "None");
}

#[tokio::test]
async fn unlimited_shop_repeated_purchases_and_cached_selection_use_current_stock_rules() {
    let (s, u) = setup().await;
    let a = account(&u);
    assert!(s.tables.live.rules["Products"].as_array().unwrap().iter()
        .filter(|p| n(p, "Price") > 0)
        .all(|p| n(p, "PurchasableCount") == 0 && n(p, "ResetSeconds") == 0));
    put(&mut s.db.acquire().await.unwrap(), a, "product", 3093009,
        &json!({"ProductIndex":3093009,"Period":0,"PurchasedCount":19})).await.unwrap();
    let before = balance(&s, &u, "gem").await;
    for expected in 20..=22 {
        let result = call(&s, &u, "shop/buy_payshop_product", "Index=3093009").await;
        ok(&result);
        assert_eq!(result["PlayerProductPurchaseInfo"]["PurchasedCount"], expected);
    }
    assert_eq!(balance(&s, &u, "gem").await, before - 15000);
    let count:i32=sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=93009")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(count, 3);
    let snapshot=shop_snapshot(&s,a).await.unwrap();
    let product=snapshot["PayShopProductInfos"].as_array().unwrap().iter().find(|p|p["Index"]==3093009).unwrap();
    assert_eq!(product["PurchasableCount"], 0);
    let selection=call(&s,&u,"shop/get_select_shop_info","CategoryGroup=Fragment").await;
    let mut cached=selection["SelectShopInfo"].clone();
    for product in cached["PayShopProductInfos"].as_array_mut().unwrap() {
        product["PurchasableCount"]=json!(1);product["ResetSeconds"]=json!(86400);
        product["ResetType"]=json!(1);product["DailyReset"]=json!(true);
    }
    put(&mut s.db.acquire().await.unwrap(),a,"select_shop",n(&cached,"Index"),&cached).await.unwrap();
    let refreshed=call(&s,&u,"shop/get_select_shop_info","CategoryGroup=Fragment").await;
    assert_eq!(refreshed["SelectShopInfo"]["ItemInfos"],cached["ItemInfos"]);
    for product in refreshed["SelectShopInfo"]["PayShopProductInfos"].as_array().unwrap() {
        assert_eq!(product["PurchasableCount"],0);assert_eq!(product["ResetType"],"None");
        assert_eq!(product["DailyReset"],false);
    }
}

#[tokio::test]
async fn shop_purchases_preserve_current_mission_unlocks() {
    let (s, u) = setup().await;
    let a = account(&u);
    for progressed in [false, true] {
        if progressed {
            sqlx::query("INSERT INTO progression_claims(account_id,family,idx,step,period) VALUES(?,'subquest',10110,1,'all')")
                .bind(a).execute(&s.db).await.unwrap();
        }
        let expected = crate::api::progression::login(&s, a).await.unwrap()["OpendMissionCategories"].clone();
        let purchase = call(&s, &u, "shop/buy_payshop_product", "Index=3093009").await;
        ok(&purchase);
        let categories = purchase["OpendMissionCategories"].as_array().unwrap();
        // These sections have no prerequisite and must stay usable after buying.
        for main in [2, 3, 6, 7, 8, 9, 10, 11, 12] {
            assert!(categories.iter().any(|c| n(c, "MainCategoryIndex") == main), "Missing section {main}");
        }
        assert_eq!(purchase["OpendMissionCategories"], expected);
        assert_eq!(categories.contains(&json!({"MainCategoryIndex":4,"SubCategoryIndex":1})), progressed);
        assert!(!categories.iter().any(|c| n(c, "MainCategoryIndex") >= 98));
    }
    // Failed purchases must not send a replacement list either.
    sqlx::query("UPDATE user_info SET gem=0 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let rejected = call(&s, &u, "shop/buy_payshop_product", "Index=3093009").await;
    assert_ne!(rejected["Result"], "Success");
    assert!(rejected["OpendMissionCategories"].is_null());
}

#[tokio::test]
async fn free_shop_packages_are_once_daily_and_restore_after_reset() {
    let (s, u) = setup().await;
    let a = account(&u);
    sqlx::query("UPDATE user_info SET gold=0,gem=0,mileage=0 WHERE account_id=?")
        .bind(a).execute(&s.db).await.unwrap();
    let catalog = shop_snapshot(&s, a).await.unwrap();
    let products = catalog["PayShopProductInfos"].as_array().unwrap();
    let free: Vec<_> = products.iter().filter(|p| n(p, "Price") == 0).collect();
    assert_eq!(free.len(), 3);
    for (id, item) in [(3094170, 94170), (3094594, 94594), (3094595, 94595)] {
        let product = free.iter().find(|p| n(p, "Index") == id).unwrap();
        assert_eq!(product["Flag"], "Free");
        assert_eq!(product["PurchasableCount"], 1);
        assert_eq!(product["ResetType"], "Daily");
        assert_eq!(product["DailyReset"], true);
        assert_eq!(product["ResetSeconds"], 86400);
        for expected in 1..=2 {
            if expected == 2 {
                // Reset only this package's ledger to simulate the next UTC day.
                sqlx::query("UPDATE extension_state SET data=json_set(data,'$.Period',-1) WHERE account_id=? AND kind='product' AND idx=?")
                    .bind(a).bind(id).execute(&s.db).await.unwrap();
                let refreshed = shop_snapshot(&s, a).await.unwrap();
                assert_eq!(refreshed["PlayerProductPurchaseInfos"].as_array().unwrap().iter()
                    .find(|p| n(p, "ProductIndex") == id).unwrap()["PurchasedCount"], 0);
            }
            let result = call(&s, &u, "shop/buy_payshop_product", &format!("Index={id}")).await;
            ok(&result);
            assert!(result["DecCurrencyResult"].is_null());
            assert_eq!(result["CurrencyResults"], json!([]));
            assert_eq!(result["PlayerProductPurchaseInfo"]["PurchasedCount"], 1);
            // Concurrent duplicate requests must both fail without granting extras.
            let args = format!("Index={id}&Price=0&ItemCount=999");
            let (duplicate, concurrent) = tokio::join!(
                call(&s, &u, "shop/buy_payshop_product", &args),
                call(&s, &u, "shop/buy_payshop_product", &args));
            assert_ne!(duplicate["Result"], "Success");
            assert_ne!(concurrent["Result"], "Success");
            let persisted = shop_snapshot(&s, a).await.unwrap();
            assert_eq!(persisted["PlayerProductPurchaseInfos"].as_array().unwrap().iter()
                .find(|p| n(p, "ProductIndex") == id).unwrap()["PurchasedCount"], 1);
            assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count FROM items WHERE account_id=? AND item_index=?")
                .bind(a).bind(item).fetch_one(&s.db).await.unwrap(), expected);
        }
    }
    for currency in ["gold", "gem", "mileage"] {
        assert_eq!(balance(&s, &u, currency).await, 0);
    }
    // A client cannot turn an ordinary paid package into a free purchase.
    let paid = call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Price=0").await;
    assert_ne!(paid["Result"], "Success");
}

#[tokio::test]
async fn special_shop_batches_charge_deliver_and_enforce_stock_atomically() {
    let (mut s, u) = setup().await;
    let a = account(&u);
    daily_limit(&mut s, &[3093009]);
    let rules = &mut Arc::make_mut(&mut Arc::make_mut(&mut s.tables).live).rules;
    rules["Products"].as_array_mut().unwrap().iter_mut()
        .find(|p| n(p, "Index") == 3093009).unwrap()["PurchasableCount"] = json!(5);
    let before = balance(&s, &u, "gem").await;
    for quantity in ["0", "-1", "1000", "abc", "9223372036854775807"] {
        let result = call(&s, &u, "shop/buy_payshop_product", &format!("Index=3093009&Quantity={quantity}")).await;
        assert_ne!(result["Result"], "Success");
    }
    assert_eq!(balance(&s, &u, "gem").await, before);
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "product", 3093009).await.unwrap().is_null());
    let batch = call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Quantity=3&Price=0&ItemCount=999").await;
    ok(&batch);
    assert_eq!(batch["PlayerProductPurchaseInfo"]["PurchasedCount"], 3);
    assert_eq!(balance(&s, &u, "gem").await, before - 15000);
    assert_eq!(batch["CurrencyResults"][0]["AddValue"], -15000);
    let rejected = call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Quantity=3").await;
    assert_ne!(rejected["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, before - 15000);
    ok(&call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Quantity=2").await);
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count FROM items WHERE account_id=? AND item_index=93009")
        .bind(a).fetch_one(&s.db).await.unwrap(), 5);
    assert_eq!(balance(&s, &u, "gem").await, before - 25000);
    assert_ne!(call(&s, &u, "shop/buy_payshop_product", "Index=3094170&Quantity=2").await["Result"], "Success");
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "product", 3094170).await.unwrap().is_null());
    // Affordable units are insufficient for this batch: no partial purchase.
    sqlx::query("UPDATE user_info SET gem=6000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    let other = 3093003;
    assert_ne!(call(&s, &u, "shop/buy_payshop_product", &format!("Index={other}&Quantity=999")).await["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, 6000);
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "product", other).await.unwrap().is_null());
    // A later invalid grant rolls back the debit and all earlier grants.
    let rules = &mut Arc::make_mut(&mut Arc::make_mut(&mut s.tables).live).rules;
    let product = rules["Products"].as_array_mut().unwrap().iter_mut().find(|p| n(p, "Index") == 3093009).unwrap();
    product["PurchasableCount"] = json!(0);
    product["ItemInfos"].as_array_mut().unwrap().push(json!({"ItemIndex":-1,"ItemCount":1}));
    sqlx::query("UPDATE user_info SET gem=100000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Quantity=2").await["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, 100000);
    assert_eq!(sqlx::query_scalar::<_, i64>("SELECT count FROM items WHERE account_id=? AND item_index=93009")
        .bind(a).fetch_one(&s.db).await.unwrap(), 5);
    assert_eq!(get(&mut s.db.acquire().await.unwrap(), a, "product", 3093009).await.unwrap()["PurchasedCount"], 5);
}

#[tokio::test]
async fn recovered_shop_purchases_are_atomic_limited_and_restore_after_reset() {
    let (mut s, u) = setup().await;
    daily_limit(&mut s, &[3093009]);
    let a = account(&u);
    let before = balance(&s, &u, "gem").await;
    let v = call(&s, &u, "shop/buy_payshop_product", "Index=3093009&Price=1&ItemCount=999").await;
    ok(&v);
    assert_eq!(balance(&s, &u, "gem").await, before - 5000);
    assert_eq!(v["CurrencyResults"][0], v["DecCurrencyResult"]);
    assert_eq!(v["CurrencyResults"][0]["CurrencyType"], "Gem");
    assert_eq!(v["CurrencyResults"][0]["AddValue"], -5000);
    assert_eq!(v["CurrencyResults"][0]["Value1"], before - 5000);
    assert!(v["firstPurchase"].is_null());
    let count: i32 = sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=93009")
        .bind(a).fetch_one(&s.db).await.unwrap();
    assert_eq!(count, 1);
    assert_ne!(call(&s, &u, "shop/buy_payshop_product", "Index=3093009").await["Result"], "Success");
    assert_eq!(balance(&s, &u, "gem").await, before - 5000);
    let snapshot = shop_snapshot(&s, a).await.unwrap();
    assert_eq!(snapshot["PlayerProductPurchaseInfos"].as_array().unwrap().iter()
        .find(|v| v["ProductIndex"] == 3093009).unwrap()["PurchasedCount"], 1);
    sqlx::query("UPDATE extension_state SET data=json_set(data,'$.Period',-1) WHERE account_id=? AND kind='product' AND idx=3093009")
        .bind(a).execute(&s.db).await.unwrap();
    let snapshot = shop_snapshot(&s, a).await.unwrap();
    assert_eq!(snapshot["PlayerProductPurchaseInfos"].as_array().unwrap().iter()
        .find(|v| v["ProductIndex"] == 3093009).unwrap()["PurchasedCount"], 0);
    ok(&call(&s, &u, "shop/buy_payshop_product", "Index=3093009").await);
    sqlx::query("UPDATE user_info SET gem=0 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
    assert_ne!(call(&s, &u, "shop/buy_payshop_product", "Index=3093003").await["Result"], "Success");
    assert!(get(&mut s.db.acquire().await.unwrap(), a, "product", 3093003).await.unwrap().is_null());
    let before_gold = balance(&s, &u, "gold").await;
    ok(&call(&s, &u, "shop/buy_payshop_product", "Index=50001002").await);
    assert_eq!(balance(&s, &u, "gold").await, before_gold - 10000000);
    assert_eq!(balance(&s, &u, "gem").await, 1000);
    ok(&call(&s, &u, "shop/buy_payshop_product", "Index=50001001").await);
    assert_eq!(balance(&s, &u, "gold").await, before_gold - 9000000);
    assert_eq!(balance(&s, &u, "gem").await, 0);
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
    let (mut s, u) = setup().await;
    daily_limit(&mut s, &[920001]);
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
    let (mut s, u) = setup().await;
    let indices=s.tables.live.rules["Products"].as_array().unwrap().iter()
        .filter(|p| p["SelectionCategory"] == "Fragment").map(|p|n(p,"Index")).collect::<Vec<_>>();
    daily_limit(&mut s, &indices);
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
    assert_eq!(v["PetMiscInfos"].as_array().unwrap().iter()
        .find(|v| v["MiscKey"] == "PetGachaCount").unwrap()["MiscValue"], "1");
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
    pets::add(&mut s.db.acquire().await.unwrap(), &s, account(&u), 7100000)
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
        &mut s.db.acquire().await.unwrap(),
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
        &mut s.db.acquire().await.unwrap(),
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
async fn main_pet_avatar_uses_native_enum_names_and_survives_relogin() {
    let (s, u) = setup().await;
    let a = account(&u);
    for id in [7400000, 7100000] {
        pets::add(&mut s.db.acquire().await.unwrap(), &s, a, id).await.unwrap();
    }
    // Include existing numeric records: the fix must repair their wire format
    // without resetting saved avatars or requiring a database migration.
    put(&mut s.db.acquire().await.unwrap(), a, "pet_misc", 1,
        &json!({"MiscKey":1,"MiscValue":"7400000","UpdatedTime":time(now())})).await.unwrap();
    fn avatar(v: &Value) -> &Value {
        v["PetMiscInfos"].as_array().unwrap().iter()
            .find(|v| v["MiscKey"] == "AvatarPetIndex").unwrap()
    }
    assert_eq!(avatar(&snapshot(&s, a).await.unwrap())["MiscValue"], "7400000");
    for id in [7100000, 7400000, 0] {
        let response = call(&s, &u, "pet/change_pet_avatar",
            &format!("AvatarPetIndex={id}&PetIndex={id}")).await;
        ok(&response);
        assert_eq!(avatar(&response)["MiscValue"], id.to_string());
        assert!(response["PetMiscInfos"].as_array().unwrap().iter().all(|v| v["MiscKey"].is_string()));
        let stored = get(&mut s.db.acquire().await.unwrap(), a, "pet_misc", 1).await.unwrap();
        assert_eq!(stored["MiscKey"], 1);
        assert_eq!(stored["MiscValue"], id.to_string());
        let login = json!(user::test_login(State(s.clone()), Bytes::from("LoginId=live-test")).await.unwrap().0);
        assert_eq!(avatar(&login)["MiscValue"], id.to_string());
    }
    assert_ne!(call(&s, &u, "pet/change_pet_avatar", "AvatarPetIndex=7100007&PetIndex=7100007").await["Result"], "Success");
    assert_eq!(avatar(&snapshot(&s, a).await.unwrap())["MiscValue"], "0");
}

#[tokio::test]
async fn native_pet_placement_preserves_slots_and_rejects_invalid_layouts() {
    let (s, u) = setup().await;
    let a = account(&u);
    for id in [7400000, 7100000] {
        pets::add(&mut s.db.acquire().await.unwrap(), &s, a, id)
            .await
            .unwrap();
    }
    // Native WWWForm uses repeated fields, including one field for a single pet.
    for (args, expected) in [
        ("PetIndices=7400000", "7400000"),
        ("PetIndices=7400000&PetIndices=0&PetIndices=7100000&PetIndices=0", "7400000,0,7100000,0"),
        ("PetIndices=[7100000,7400000]", "7100000,7400000"),
    ] {
        let v = call(&s, &u, "pet/change_pet_layout", args).await;
        ok(&v);
        assert_eq!(
            v["PetMiscInfos"].as_array().unwrap().iter()
                .find(|v| v["MiscKey"] == "PetLayout").unwrap()["MiscValue"],
            expected
        );
        assert_eq!(get(&mut s.db.acquire().await.unwrap(), a, "pet_misc", 2).await.unwrap()["MiscValue"], expected);
    }
    for args in [
        "PetIndices=7400000&PetIndices=7400000",
        "PetIndices=7400000&PetIndices=-1",
        "PetIndices=7400000&PetIndices=not-a-number",
        "PetIndices=7400000&PetIndices=7100007", // Not owned.
        "PetIndices=[7400000,0,0,0,0,0,0,0,0]", // Too many slots.
    ] {
        assert_ne!(call(&s, &u, "pet/change_pet_layout", args).await["Result"], "Success");
        assert_eq!(get(&mut s.db.acquire().await.unwrap(), a, "pet_misc", 2).await.unwrap()["MiscValue"], "7100000,7400000");
    }
    let snap = snapshot(&s, a).await.unwrap();
    assert_eq!(snap["PetMiscInfos"].as_array().unwrap().iter()
        .find(|v| v["MiscKey"] == "PetLayout").unwrap()["MiscValue"], "7100000,7400000");
}
#[tokio::test]
async fn pet_care_duplicate_souls_and_layout_survive_login() {
    let (s, u) = setup().await;
    let a = account(&u);
    let mut rewards = Rewards::default();
    item::give(
        &mut s.db.acquire().await.unwrap(),
        &s,
        a,
        ItemGrant {
            index: 7400000,
            count: 2,
            star: 0,
            custom: 0,
        },
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
            .find(|v| v["MiscKey"] == "PetLayout")
            .unwrap()["MiscValue"],
        "7400000,0,0"
    );
}
#[tokio::test]
async fn pickup_summons_accept_native_repeated_hero_fields_and_charge_once() {
    let (s,u)=setup().await;
    for id in [18,5] {
        let before=balance(&s,&u,"gem").await;let mileage=balance(&s,&u,"mileage").await;
        let out=call(&s,&u,"equip_gacha/exec_equip_gacha",&format!("GachaIndex={id}&HeroIndices=1&HeroIndices=2&HeroIndices=3&HeroIndices=4&HighGachaCategory=None&Free=False&Discount=False&DiscountedCost=2500")).await;
        ok(&out);assert_eq!(out["GachaItemResults"].as_array().unwrap().len(),11,"Native x10 includes its bonus result");
        assert_eq!(balance(&s,&u,"gem").await,before-2500);
        assert_eq!(balance(&s,&u,"mileage").await,mileage+250);
        for roll in out["GachaItemResults"].as_array().unwrap() {
            assert!(roll["ItemResult"].is_object() || roll["EquipItemResult"].is_object());
        }
        if id==18 {assert_eq!(out["StarterPickupEquipGachaInfo"]["GachaCount"],1);}
    }
    let before=balance(&s,&u,"gem").await;
    let out=call(&s,&u,"equip_gacha/exec_equip_gacha","GachaIndex=5&HeroIndices=[1,2,3,4]").await;ok(&out);
    assert_eq!(balance(&s,&u,"gem").await,before-2500,"JSON API arrays remain supported");
    let out=call(&s,&u,"equip_gacha/exec_equip_gacha","GachaIndex=18&HeroIndices=72&HeroIndices=85&HeroIndices=101&HeroIndices=102").await;
    ok(&out);assert_eq!(out["GachaItemResults"].as_array().unwrap().len(),11,"All native starter hero choices have usable pools");
}

#[tokio::test]
async fn pickup_summons_reject_invalid_selections_without_spending_or_awarding() {
    let (s,u)=setup().await;let a=account(&u);
    let before:(i64,i64,i64)=sqlx::query_as("SELECT gold,gem,mileage FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    let items:Vec<(i64,i64)>=sqlx::query_as("SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index").bind(a).fetch_all(&s.db).await.unwrap();
    let gear:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
    for id in [18,5] {
        for heroes in ["HeroIndices=1&HeroIndices=2&HeroIndices=3","HeroIndices=1&HeroIndices=1&HeroIndices=3&HeroIndices=4","HeroIndices=1&HeroIndices=2&HeroIndices=3&HeroIndices=999999","HeroIndices=1&HeroIndices=2&HeroIndices=3&HeroIndices=-4"] {
            assert_ne!(call(&s,&u,"equip_gacha/exec_equip_gacha",&format!("GachaIndex={id}&{heroes}")).await["Result"],"Success");
            assert_eq!(sqlx::query_as::<_,(i64,i64,i64)>("SELECT gold,gem,mileage FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before);
            assert_eq!(sqlx::query_as::<_,(i64,i64)>("SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index").bind(a).fetch_all(&s.db).await.unwrap(),items);
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),gear);
            assert!(get(&mut s.db.acquire().await.unwrap(),a,"summon",id).await.unwrap().is_null());
        }
    }
}

#[tokio::test]
async fn starter_pickup_uses_native_twenty_purchase_reward_track() {
    let (s,u)=setup().await;let a=account(&u);
    put(&mut s.db.acquire().await.unwrap(),a,"summon",18,&json!({"GachaIndex":18,"GachaCount":110,"StarterDraws":10,"Claimed":0,"Day":day(),"IsTakeLastReward":false})).await.unwrap();
    assert_ne!(call(&s,&u,"equip_gacha/get_starter_pickup_equip_gacha_last_reward","GachaIndex=18").await["Result"],"Success","Ten purchases cannot claim the 20-purchase reward");
    let out=call(&s,&u,"equip_gacha/exec_equip_gacha","GachaIndex=18&HeroIndices=1&HeroIndices=2&HeroIndices=3&HeroIndices=4").await;
    ok(&out);assert_eq!(out["StarterPickupEquipGachaInfo"]["GachaCount"],11,"The starter summon remains usable past ten");
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
        &mut s.db.acquire().await.unwrap(),
        account(&u),
        "summon",
        18,
        &json!({"GachaIndex":18,"GachaCount":209,"StarterDraws":19,"Claimed":0,"Day":day(),"IsTakeLastReward":false}),
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
    assert_eq!(starter["StarterPickupEquipGachaInfo"]["GachaCount"], 20);
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
    // The archived single special summon is ticket-only; funding a ruby
    // balance must not bypass that restriction.
    assert_eq!(call(&s,&u,"equip_gacha/exec_equip_gacha","GachaIndex=4&HighGachaCategory=All").await["Result"],"ItemNotOwned");
    let ticket=s.tables.live.rows("GachaSelectItem").iter().find(|r|
        r["GachaIndices"].as_array().is_some_and(|indices|indices.contains(&json!(4))) && n(r,"GachaCategoryType")==0).unwrap();
    let ticket=n(ticket,"ItemIndex") as i32;
    seed(&s,&u,ticket,1).await;
    ok(&call(
        &s,
        &u,
        "equip_gacha/exec_equip_gacha",
        &format!("GachaIndex=4&HighGachaCategory=All&ItemIndex={ticket}"),
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
    let (mut s, u) = setup().await;
    daily_limit(&mut s, &[920001]);
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

#[tokio::test]
async fn native_summon_gauges_count_purchases_grant_milestones_and_preserve_overflow() {
    let (s,u)=setup().await;
    let a=account(&u);
    // Three old 10+1 purchases are three gauge points, not 33.
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "summon",
        25,
        &json!({"GachaCount":33,"Claimed":0,"Day":day()}),
    )
    .await
    .unwrap();
    let v = call(&s, &u, "equip_gacha/get_equip_gacha", "").await;
    assert_eq!(
        v["PetGachaCountInfos"]
            .as_array()
            .unwrap()
            .iter()
            .find(|x| n(x, "GachaIndex") == 25)
            .unwrap()["GachaCount"],
        3
    );
    let v = call(&s, &u, "equip_gacha/exec_equip_gacha", "GachaIndex=25").await;
    ok(&v);
    assert_eq!(v["PetGachaCountInfo"]["GachaCount"], 4);
    assert_eq!(v["GachaItemResults"].as_array().unwrap().len(), 11);
    // Reach the original ten-purchase milestone. Reward comes from RewardIndices.
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "summon",
        25,
        &json!({"GachaCount":99,"RewardGauge":9,"Day":day()}),
    )
    .await
    .unwrap();
    let v = call(&s, &u, "equip_gacha/exec_equip_gacha", "GachaIndex=25").await;
    ok(&v);
    assert_eq!(v["PetGachaCountInfo"]["GachaCount"], 0);
    assert!(!v["PetRewardInfo"]["ItemResults"]
        .as_array()
        .unwrap()
        .is_empty());
    assert_ne!(
        call(&s, &u, "pet/pet_gacha_roof_reward", "GachaIndex=25").await["Result"],
        "Success"
    );
    // Existing overflow can be recovered without a purchase, exactly once.
    put(
        &mut s.db.acquire().await.unwrap(),
        a,
        "summon",
        25,
        &json!({"RewardGauge":23,"Day":day()}),
    )
    .await
    .unwrap();
    let v = call(&s, &u, "pet/pet_gacha_roof_reward", "GachaIndex=25").await;
    ok(&v);
    assert_eq!(v["PetGachaCountInfo"]["GachaCount"], 3);
    assert_ne!(
        call(&s, &u, "pet/pet_gacha_roof_reward", "GachaIndex=25").await["Result"],
        "Success"
    );
    // All-In-One rewards at 10, keeps its gauge, then rewards/reset at 20.
    for (before, after) in [(9, 10), (19, 0)] {
        put(
            &mut s.db.acquire().await.unwrap(),
            a,
            "summon",
            11,
            &json!({"RewardGauge":before,"RewardGaugePaid":if before==19 {1}else{0},"Day":day()}),
        )
        .await
        .unwrap();
        let v = call(&s, &u, "equip_gacha/exec_equip_gacha", "GachaIndex=11").await;
        ok(&v);
        assert!(!v["GachaCeilingRewardInfo"]["ItemResults"]
            .as_array()
            .unwrap()
            .is_empty());
        assert_eq!(
            v["GachaCeilingCountInfos"]
                .as_array()
                .unwrap()
                .iter()
                .find(|x| n(x, "GachaIndex") == 11)
                .unwrap()["GachaCount"],
            after
        );
        assert_ne!(
            call(
                &s,
                &u,
                "equip_gacha/equip_gacha_ceiling_reward",
                "GachaIndex=11"
            )
            .await["Result"],
            "Success"
        );
    }
    // Ticket draws grant the items but no ruby-purchase gauge point.
    let ticket=s.tables.live.rows("GachaSelectItem").iter().find(|r|r["GachaIndices"].as_array().is_some_and(|v|v.contains(&json!(25))) && n(r,"GachaCategoryType")==0).unwrap();
    let ticket=n(ticket,"ItemIndex") as i32;seed(&s,&u,ticket,1).await;
    let v=call(&s,&u,"equip_gacha/exec_equip_gacha",&format!("GachaIndex=25&ItemIndex={ticket}")).await;ok(&v);
    assert_eq!(v["PetGachaCountInfo"]["GachaCount"],3);
}
