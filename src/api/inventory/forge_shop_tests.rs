use super::*;
use crate::api::inventory::shop;
use chrono::{Datelike, TimeZone, Utc};

fn forge_catalog(state: &AppState) -> Vec<Value> {
    let mut rows: Vec<_> = state
        .tables
        .hero_shop
        .shop_items
        .iter()
        .filter(|r| r["ShopIndex"] == 11)
        .cloned()
        .collect();
    rows.sort_by_key(|r| item::n(r, "Index"));
    assert_eq!(rows.len(), 416);
    rows
}

async fn seed_stock(state: &AppState, account: i64, shop: i32, expires: i64, rows: &[Value]) {
    crate::db::query("INSERT INTO shop_stock(account_id,shop_index,revision,restock_time,restock_count,stock) VALUES ($1,$2,7,$3,0,$4)")
        .bind(account).bind(shop).bind(expires).bind(json!(rows).to_string())
        .execute(&state.db).await.unwrap();
}

async fn saved_stock(state: &AppState, account: i64, shop: i32) -> (i64, i64, i64, String) {
    crate::db::query_as("SELECT revision,restock_time,restock_count,stock FROM shop_stock WHERE account_id=$1 AND shop_index=$2")
        .bind(account).bind(shop).fetch_one(&state.db).await.unwrap()
}

async fn raid_points(state: &AppState, account: i64) -> i64 {
    crate::db::query_scalar("SELECT raid_point FROM user_info WHERE account_id=$1")
        .bind(account)
        .fetch_one(&state.db)
        .await
        .unwrap()
}

async fn buy(state: &AppState, u: &user::LoginResponse, index: i64, quantity: i64) -> Value {
    shop::buy_shop_item(
        State(state.clone()),
        form(
            u,
            &format!("ShopIndex=11&ShopItemIndex={index}&ShopItemPurchaseCount={quantity}"),
        ),
    )
    .await
    .unwrap()
    .0
}

#[tokio::test]
async fn forge_ticket_repairs_incomplete_and_mismatched_stock_without_resetting_receipts() {
    for incomplete in [true, false] {
        let (state, u) = setup().await;
        let account = u.user_info.account_id;
        let catalog = forge_catalog(&state);
        let ticket = catalog.iter().find(|r| r["Index"] == 407).unwrap();
        assert_eq!(ticket["ItemIndex"], 2815);
        assert_eq!(ticket["ShowIndex"], 409);
        assert_eq!(ticket["PurchasableCount"], 4);
        assert_eq!(state.tables.hero_shop.items[&2815]["BuyRaidPoint"], 460800);
        let mut stale = if incomplete {
            vec![catalog[0].clone()]
        } else {
            catalog.clone()
        };
        // A cached index may also survive while identifying a different offer.
        let wrong = if incomplete {
            &mut stale[0]
        } else {
            stale.iter_mut().find(|r| r["Index"] == 407).unwrap()
        };
        wrong["ItemIndex"] = json!(2200);
        wrong["ItemCode"] = json!("FREE_AUGMENTATION");
        wrong["Count"] = json!(2);
        wrong["TargetPrice"] = json!(1);
        seed_stock(&state, account, 11, 0, &stale).await;
        crate::db::query("UPDATE user_info SET raid_point=10000000 WHERE account_id=$1")
            .bind(account)
            .execute(&state.db)
            .await
            .unwrap();
        let week = Utc
            .timestamp_opt(state.server_time(), 0)
            .unwrap()
            .iso_week();
        let period = format!("week:{}:{}", week.year(), week.week());
        for (period, purchased) in [(&period[..], 2), ("week:2000:1", 4)] {
            crate::db::query("INSERT INTO shop_purchase_ledger(account_id,shop_index,item_index,period,purchased,purchased_time) VALUES ($1,11,407,$2,$3,123)")
                .bind(account).bind(period).bind(purchased).execute(&state.db).await.unwrap();
        }

        // Purchase directly, without a GetShopList call to repair the cache first.
        let before = count(&state, &u, 2815).await;
        let out = buy(&state, &u, 407, 1).await;
        assert_eq!(out["Result"], "Success", "incomplete={incomplete}: {out}");
        assert_eq!(out["CurrencyResults"][0]["AddValue"], -460800);
        assert_eq!(out["ItemResult"]["ItemIndex"], 2815);
        assert_eq!(out["ItemResult"]["AddCount"], 1);
        assert_eq!(count(&state, &u, 2815).await, before + 1);
        assert_eq!(out["ShopItem"]["Purchased"], 3);
        assert_eq!(out["ShopItem"]["Sold"], 0);
        let repaired = saved_stock(&state, account, 11).await;
        assert_eq!((repaired.0, repaired.1), (8, 0));
        assert_eq!(
            serde_json::from_str::<Vec<Value>>(&repaired.3).unwrap(),
            catalog
        );

        // Another repair must not create a fresh weekly allowance.
        crate::db::query("UPDATE shop_stock SET stock='[]' WHERE account_id=$1 AND shop_index=11")
            .bind(account)
            .execute(&state.db)
            .await
            .unwrap();
        let listed = shop::get_shop_list(State(state.clone()), form(&u, "ShopIndex=11"))
            .await
            .unwrap()
            .0;
        assert_eq!(listed["ShopItems"].as_array().unwrap().len(), 416);
        let visible = listed["ShopItems"]
            .as_array()
            .unwrap()
            .iter()
            .find(|r| r["ListNo"] == 407)
            .unwrap();
        assert_eq!(visible["Purchased"], 3);
        let repaired = saved_stock(&state, account, 11).await;
        assert_eq!(repaired.0, 9);
        let counters =
            shop::get_all_shop_item_purchase_count(State(state.clone()), form(&u, "ShopIndex=11"))
                .await
                .unwrap()
                .0;
        assert_eq!(
            counters["ShopItems"]
                .as_array()
                .unwrap()
                .iter()
                .find(|r| r["ListNo"] == 407)
                .unwrap()["Purchased"],
            3
        );
        assert_eq!(
            saved_stock(&state, account, 11).await,
            repaired,
            "unchanged catalogs must not rebuild"
        );
        let out = buy(&state, &u, 407, 1).await;
        assert_eq!(out["Result"], "Success");
        assert_eq!(out["ShopItem"]["Purchased"], 4);
        assert_eq!(out["ShopItem"]["Sold"], 1);
        let balance = raid_points(&state, account).await;
        let out = buy(&state, &u, 407, 1).await;
        assert_eq!(out["Result"], "SoldOut", "{out}");
        assert_eq!(raid_points(&state, account).await, balance);
        assert_eq!(count(&state, &u, 2815).await, before + 2);
        let receipts: Vec<(String, i64)> = crate::db::query_as("SELECT period,purchased FROM shop_purchase_ledger WHERE account_id=$1 AND shop_index=11 AND item_index=407 ORDER BY period")
            .bind(account).fetch_all(&state.db).await.unwrap();
        assert_eq!(receipts, vec![("week:2000:1".into(), 4), (period, 4)]);
    }
}

#[tokio::test]
async fn forge_all_416_offers_charge_catalog_prices_grant_counts_and_enforce_limits() {
    let (state, u) = setup().await;
    let account = u.user_info.account_id;
    let catalog = forge_catalog(&state);
    let shop = &state.tables.hero_shop.shops[&11];
    assert_eq!(shop["BuyCostType"], 8);
    assert_eq!(shop["HeroIndex"], 0);
    assert_eq!(
        state
            .tables
            .hero_shop
            .equip_star_prices
            .iter()
            .find(|r| r["Star"] == 0)
            .unwrap()["BuyPriceFactor"],
        1000
    );
    seed_stock(&state, account, 11, 0, &[]).await;
    crate::db::query("UPDATE user_info SET raid_point=1000000000000 WHERE account_id=$1")
        .bind(account)
        .execute(&state.db)
        .await
        .unwrap();
    let mut total_cost = 0;
    for row in &catalog {
        let index = item::n(row, "Index");
        let id = item::n(row, "ItemIndex") as i32;
        let max = item::n(row, "PurchasableCount");
        let quantity = if max > 0 { max } else { 2 };
        assert_eq!(row["BuyCostTypeForItem"], 0);
        assert_eq!(row["Star"], 0);
        let price = if item::n(row, "TargetPrice") > 0 {
            item::n(row, "TargetPrice")
        } else {
            item::n(&state.tables.hero_shop.items[&id], "BuyRaidPoint") * item::n(row, "Count")
        };
        assert!(price > 0, "row={row}");
        let amount = item::n(row, "Count") * quantity;
        let equip = state.tables.items.reward_item(id).unwrap().kind == "Equip";
        let before = if equip {
            crate::db::query_scalar::<i64>(
                "SELECT COUNT(*) FROM equip_items WHERE account_id=$1 AND item_index=$2",
            )
            .bind(account)
            .bind(id)
            .fetch_one(&state.db)
            .await
            .unwrap()
        } else {
            count(&state, &u, id).await as i64
        };
        let balance = raid_points(&state, account).await;
        let out = buy(&state, &u, index, quantity).await;
        assert_eq!(out["Result"], "Success", "row={row}, out={out}");
        assert_eq!(out["CurrencyResults"][0]["CurrencyType"], "RaidPoint");
        assert_eq!(
            out["CurrencyResults"][0]["AddValue"],
            -price * quantity,
            "row={row}"
        );
        assert_eq!(
            raid_points(&state, account).await,
            balance - price * quantity
        );
        total_cost += price * quantity;
        assert_eq!(out["ShopItem"]["Purchased"], quantity);
        assert_eq!(out["ShopItem"]["Sold"], if max > 0 { 1 } else { 0 });
        if equip {
            let granted = out["EquipItems"].as_array().unwrap();
            assert_eq!(granted.len() as i64, amount);
            assert!(granted
                .iter()
                .all(|r| r["ItemIndex"] == id && r["Star"] == row["Star"]));
            assert_eq!(
                crate::db::query_scalar::<i64>(
                    "SELECT COUNT(*) FROM equip_items WHERE account_id=$1 AND item_index=$2"
                )
                .bind(account)
                .bind(id)
                .fetch_one(&state.db)
                .await
                .unwrap(),
                before + amount
            );
            // Keep this exhaustive catalog audit independent of equipment bag capacity.
            crate::db::query("DELETE FROM equip_items WHERE account_id=$1 AND item_index=$2")
                .bind(account)
                .bind(id)
                .execute(&state.db)
                .await
                .unwrap();
        } else {
            assert_eq!(out["ItemResult"]["ItemIndex"], id);
            assert_eq!(out["ItemResult"]["AddCount"], amount);
            assert_eq!(count(&state, &u, id).await as i64, before + amount);
        }
        if max > 0 {
            let balance = raid_points(&state, account).await;
            assert_eq!(
                buy(&state, &u, index, 1).await["Result"],
                "SoldOut",
                "row={row}"
            );
            assert_eq!(raid_points(&state, account).await, balance);
            assert_eq!(count(&state, &u, id).await as i64, before + amount);
        }
    }
    assert_eq!(
        raid_points(&state, account).await,
        1000000000000 - total_cost
    );
    let counters =
        shop::get_all_shop_item_purchase_count(State(state.clone()), form(&u, "ShopIndex=11"))
            .await
            .unwrap()
            .0;
    let counters = counters["ShopItems"].as_array().unwrap();
    assert_eq!(counters.len(), 416);
    for row in &catalog {
        let saved = counters
            .iter()
            .find(|r| r["ListNo"] == row["Index"])
            .unwrap();
        let max = item::n(row, "PurchasableCount");
        assert_eq!(saved["Purchased"], if max > 0 { max } else { 2 });
        assert_eq!(saved["Sold"], if max > 0 { 1 } else { 0 });
    }
    assert_eq!(saved_stock(&state, account, 11).await.0, 8);
}

#[tokio::test]
async fn forge_fixed_catalog_invalidation_also_repairs_unlisted_fixed_shops() {
    let (state, u) = setup().await;
    let account = u.user_info.account_id;
    assert_eq!(state.tables.hero_shop.shops[&10]["MaxStock"], 0);
    seed_stock(&state, account, 10, 0, &[]).await;
    let out = shop::get_shop_list(State(state.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap()
        .0;
    assert_eq!(out["Result"], "Success");
    assert!(!out["ShopItems"].as_array().unwrap().is_empty());
    let saved = saved_stock(&state, account, 10).await;
    assert_eq!(saved.0, 8);
    let _ = shop::get_shop_list(State(state.clone()), form(&u, "ShopIndex=10"))
        .await
        .unwrap();
    assert_eq!(saved_stock(&state, account, 10).await, saved);
}

#[tokio::test]
async fn forge_fixed_catalog_invalidation_preserves_active_rotating_stock_and_receipts() {
    let (state, u) = setup().await;
    let account = u.user_info.account_id;
    let mut row = forge_catalog(&state)
        .into_iter()
        .find(|r| r["Index"] == 407)
        .unwrap();
    row["Index"] = json!(1);
    row["PurchasableCount"] = json!(1);
    // These randomized slots intentionally differ from every authoritative source row.
    for (id, expires) in [(1, state.server_time() + 3600), (2, 0)] {
        seed_stock(&state, account, id, expires, &[row.clone()]).await;
        crate::db::query("INSERT INTO shop_purchase_ledger(account_id,shop_index,item_index,period,purchased,purchased_time) VALUES ($1,$2,1,'stock:7',1,123)")
            .bind(account).bind(id).execute(&state.db).await.unwrap();
        let before = saved_stock(&state, account, id).await;
        for counters in [false, true] {
            let args = format!("ShopIndex={id}");
            let out = if counters {
                shop::get_all_shop_item_purchase_count(State(state.clone()), form(&u, &args))
                    .await
                    .unwrap()
                    .0
            } else {
                shop::get_shop_list(State(state.clone()), form(&u, &args))
                    .await
                    .unwrap()
                    .0
            };
            assert_eq!(out["Result"], "Success");
            assert_eq!(out["ShopItems"].as_array().unwrap().len(), 1);
            assert_eq!(out["ShopItems"][0]["Purchased"], 1);
            assert_eq!(out["ShopItems"][0]["Sold"], 1);
            assert_eq!(saved_stock(&state, account, id).await, before);
        }
        let out = shop::buy_shop_item(
            State(state.clone()),
            form(
                &u,
                &format!("ShopIndex={id}&ListNo=1&ShopItemPurchaseCount=1"),
            ),
        )
        .await
        .unwrap()
        .0;
        assert_eq!(out["Result"], "SoldOut");
        assert_eq!(saved_stock(&state, account, id).await, before);
    }
}
