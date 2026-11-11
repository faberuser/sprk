use super::*;
use crate::api::inventory::shop;

fn scroll_family() -> impl Iterator<Item = i32> {
    (9000101..=9000202)
        .chain(9001001..=9001002)
        .chain(9002001..=9002016)
}

#[tokio::test]
async fn retired_limit_break_family_cannot_be_granted_or_rolled() {
    let (state, u) = setup().await;
    let mut tx = state.db.begin().await.unwrap();
    let mut rewards = crate::api::tutorial::Rewards::default();
    for index in scroll_family() {
        assert!(
            state.tables.inventory.items.contains_key(&index),
            "metadata must remain: {index}"
        );
        assert!(
            crate::models::item::disabled_reward(index),
            "unretired item: {index}"
        );
        let grant = ItemGrant {
            index,
            count: 10,
            star: 0,
            custom: 0,
        };
        item::give(&mut tx, &state, u.user_info.account_id, grant, &mut rewards)
            .await
            .unwrap();
        crate::api::tutorial::grant_item(
            &mut tx,
            &state,
            u.user_info.account_id,
            grant,
            &mut rewards,
        )
        .await
        .unwrap();
    }
    assert!(rewards.items.is_empty());
    tx.commit().await.unwrap();
    for index in scroll_family() {
        assert_eq!(count(&state, &u, index).await, 0);
    }
    for group in state.tables.item_groups.entries.values() {
        assert!(
            group.sub_data_list.iter().all(|leaf| !state
                .tables
                .items
                .get_index(&leaf.item_code)
                .is_some_and(crate::models::item::disabled_reward)),
            "retired output in {}",
            group.item_group_code
        );
    }
    let groups = &state.tables.item_groups.entries;
    for code in [
        "RANDOM_LIMITSCROLL_ALL_GROUP",
        "GACHA_ALL_IN_ONE_ALL_220920_LIMITBREAK_SCROLL_GROUP",
        "GACHA_ALL_IN_ONE_ALL_LIMITSCROLL_ALL_SELECT_GROUP",
        "GACHA_ALL_IN_ONE_ALL_220920_LIMITPOINT_GROUP",
    ] {
        assert!(groups[code].sub_data_list.is_empty(), "{code}");
        assert_eq!(groups[code].total_rate, 0, "{code}");
        assert!(
            groups.values().all(|group| group
                .sub_data_list
                .iter()
                .all(|leaf| leaf.item_code != code)),
            "empty retired group is still referenced: {code}"
        );
    }
    let mixed = &groups["GACHA_ALL_IN_ONE_ALL_220920_GROUP"];
    assert!(!mixed.sub_data_list.is_empty());
    assert_eq!(
        mixed.total_rate,
        mixed
            .sub_data_list
            .iter()
            .map(|leaf| leaf.rate)
            .sum::<i32>()
    );
    assert!(mixed
        .sub_data_list
        .iter()
        .all(|leaf| !leaf.item_code.contains("LIMIT")));
    put(&state, &u, 9002007, 2).await;
    let result = item::use_package_item(
        State(state.clone()),
        form(&u, "ItemIndex=9002007&ItemCount=1"),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(result["Result"], "Success");
    assert_eq!(
        count(&state, &u, 9002007).await,
        2,
        "owned copies must not be deleted"
    );
    for index in [
        9000100, 9000203, 9001000, 9001003, 9002000, 9002017, 45044, 45045, 45046,
    ] {
        assert!(
            !crate::models::item::disabled_reward(index),
            "unrelated item: {index}"
        );
    }
}

#[tokio::test]
async fn retired_limit_break_offers_are_hidden_from_cached_may_stock_without_resetting_it() {
    let (state, u) = setup().await;
    let a = u.user_info.account_id;
    let valid = state
        .tables
        .hero_shop
        .shop_items
        .iter()
        .find(|r| r["ShopIndex"] == 0 && r["ItemIndex"] == 990001)
        .unwrap()
        .clone();
    let mut rows = vec![valid];
    for id in [9002007, 9002005] {
        rows.push(
            state
                .tables
                .hero_shop
                .shop_items
                .iter()
                .find(|r| r["ShopIndex"] == 0 && r["ItemIndex"] == id)
                .unwrap()
                .clone(),
        );
    }
    for (i, row) in rows.iter_mut().enumerate() {
        row["Index"] = json!(i + 1);
        row["PurchasableCount"] = json!(1);
    }
    let expires = state.server_time() + 3600;
    sqlx::query("INSERT INTO shop_stock(account_id,shop_index,revision,restock_time,restock_count,stock) VALUES (?,1,7,?,3,?)")
        .bind(a).bind(expires).bind(json!(rows).to_string()).execute(&state.db).await.unwrap();
    sqlx::query("INSERT INTO shop_purchase_ledger(account_id,shop_index,item_index,period,purchased,purchased_time) VALUES (?,1,1,'stock:7',1,0)")
        .bind(a).execute(&state.db).await.unwrap();
    let balances = || async {
        sqlx::query_as::<_, (i64, i64)>("SELECT gold,gem FROM user_info WHERE account_id=?")
            .bind(a)
            .fetch_one(&state.db)
            .await
            .unwrap()
    };
    let before = balances().await;
    for counters in [false, true] {
        let result = if counters {
            shop::get_all_shop_item_purchase_count(State(state.clone()), form(&u, "ShopIndex=1"))
                .await
                .unwrap()
                .0
        } else {
            shop::get_shop_list(State(state.clone()), form(&u, "ShopIndex=1"))
                .await
                .unwrap()
                .0
        };
        assert_eq!(result["Result"], "Success", "{result}");
        let visible = result["ShopItems"].as_array().unwrap();
        assert_eq!(visible.len(), 1, "{result}");
        assert_eq!(
            visible[0]["ListNo"], 1,
            "surviving slots must not be renumbered"
        );
        assert_eq!(visible[0]["Purchased"], 1);
        assert_eq!(visible[0]["Sold"], 1);
    }
    for list in [2, 3] {
        let result = shop::buy_shop_item(
            State(state.clone()),
            form(
                &u,
                &format!("ShopIndex=1&ListNo={list}&ShopItemPurchaseCount=1"),
            ),
        )
        .await
        .unwrap()
        .0;
        assert_ne!(result["Result"], "Success", "stale selection: {result}");
    }
    assert_eq!(balances().await, before);
    assert_eq!(count(&state, &u, 9002007).await, 0);
    assert_eq!(count(&state, &u, 9002005).await, 0);
    let stock: (i64, i64, i64) = sqlx::query_as("SELECT revision,restock_time,restock_count FROM shop_stock WHERE account_id=? AND shop_index=1")
        .bind(a).fetch_one(&state.db).await.unwrap();
    assert_eq!(stock, (7, expires, 3));
}

#[tokio::test]
async fn retired_limit_break_offers_never_enter_fresh_may_stock() {
    let (mut state, u) = setup().await;
    // Constrain the pool so the test cannot miss a retired offer through chance.
    let tables = Arc::make_mut(&mut state.tables);
    let pool = &mut Arc::make_mut(&mut tables.hero_shop).shop_items;
    pool.retain(|r| {
        r["ShopIndex"] == 0
            && [990001, 9002007, 9002005].contains(&(item::n(r, "ItemIndex") as i32))
    });
    pool.sort_by_key(|r| item::n(r, "ItemIndex"));
    pool.dedup_by_key(|r| item::n(r, "ItemIndex"));
    let result = shop::get_shop_list(State(state.clone()), form(&u, "ShopIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(result["Result"], "Success", "{result}");
    let visible = result["ShopItems"].as_array().unwrap();
    assert_eq!(visible.len(), 1, "{result}");
    assert_eq!(
        visible[0]["ItemCode"],
        state
            .tables
            .hero_shop
            .shop_items
            .iter()
            .find(|r| r["ItemIndex"] == 990001)
            .unwrap()["ItemCode"]
    );
}
