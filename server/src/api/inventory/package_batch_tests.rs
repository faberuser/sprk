use super::*;

async fn open(state: &AppState, u: &user::LoginResponse, package: i32, batch: i32) -> Value {
    item::use_package_item(
        State(state.clone()),
        form(u, &format!("ItemIndex={package}&ItemCount={batch}")),
    )
    .await
    .unwrap()
    .0
}

async fn reset_rewards(state: &AppState, u: &user::LoginResponse, package: i32, batch: i32) {
    crate::db::query(
        "UPDATE user_info SET gold=100,gem=200,pay_gem=300,mileage=0 WHERE account_id=$1",
    )
    .bind(u.user_info.account_id)
    .execute(&state.db)
    .await
    .unwrap();
    crate::db::query("DELETE FROM battle_currencies WHERE account=$1 AND kind='LuaPoint'")
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    for id in [40005, 40006, 40007, 40008, 40010] {
        put(state, u, id, 0).await;
    }
    crate::db::query("UPDATE items SET locked=1 WHERE account_id=$1 AND item_index=40005")
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    put(state, u, package, batch).await;
}

#[tokio::test]
async fn package_batch_results_match_individual_opens_including_ruby_partitions_and_locks() {
    let (state, u) = setup().await;
    for package in [94067, 93008, 93999] {
        reset_rewards(&state, &u, package, 4).await;
        let mut expected = Vec::new();
        for _ in 0..4 {
            let result = open(&state, &u, package, 1).await;
            assert_eq!(result["Result"], "Success");
            expected.push(result["RewardResults"][0].clone());
        }
        reset_rewards(&state, &u, package, 4).await;
        let result = open(&state, &u, package, 4).await;
        assert_eq!(result["Result"], "Success");
        assert_eq!(
            result["RewardResults"],
            json!(expected),
            "package {package}"
        );
        assert_eq!(count(&state, &u, package).await, 0);
        if package == 93008 {
            let balances: (i64, i64) =
                crate::db::query_as("SELECT gem,pay_gem FROM user_info WHERE account_id=$1")
                    .bind(u.user_info.account_id)
                    .fetch_one(&state.db)
                    .await
                    .unwrap();
            assert_eq!(balances, (8200, 300));
            assert_eq!(count(&state, &u, 40005).await, 2000);
            for reward in result["RewardResults"].as_array().unwrap() {
                assert_eq!(
                    reward["CurrencyResults"]
                        .as_array()
                        .unwrap()
                        .iter()
                        .filter(|c| c["CurrencyType"] == "Gem")
                        .count(),
                    2
                );
                assert_eq!(
                    reward["ItemResults"]
                        .as_array()
                        .unwrap()
                        .iter()
                        .find(|r| r["ItemIndex"] == 40005)
                        .unwrap()["Locked"],
                    1
                );
            }
        }
    }
}

#[tokio::test]
async fn package_batch_row_writes_are_bounded_by_reward_keys_not_open_count() {
    let (state, u) = setup().await;
    for package in [94067, 93008] {
        reset_rewards(&state, &u, package, 1).await;
        assert_eq!(open(&state, &u, package, 1).await["Result"], "Success");
        let mut measured = Vec::new();
        for batch in [2, 1000] {
            put(&state, &u, package, batch).await;
            let before = crate::database::row_changes(&state.db).await;
            let response = open(&state, &u, package, batch).await;
            assert_eq!(response["Result"], "Success");
            assert_eq!(
                response["RewardResults"].as_array().unwrap().len(),
                batch as usize
            );
            let writes = crate::database::row_changes(&state.db).await - before;
            assert!(writes < 100, "package {package}: {writes} row writes");
            measured.push(writes);
        }
        assert_eq!(measured[0], measured[1], "package {package}");
    }
}

#[tokio::test]
async fn package_batch_late_item_cap_failure_rolls_back_all_grants_and_consumption() {
    let (mut state, u) = setup().await;
    reset_rewards(&state, &u, 93008, 4).await;
    let tables = Arc::make_mut(&mut state.tables);
    Arc::make_mut(&mut tables.inventory)
        .items
        .get_mut(&40005)
        .unwrap()["ItemMaxCap"] = json!(1500);
    let result = open(&state, &u, 93008, 4).await;
    assert_eq!(result["Result"], "InvalidItemCount");
    assert_eq!(count(&state, &u, 93008).await, 4);
    for id in [40005, 40006, 40007, 40008, 40010] {
        assert_eq!(count(&state, &u, id).await, 0);
    }
    let balances: (i64, i64, i64) =
        crate::db::query_as("SELECT gold,gem,pay_gem FROM user_info WHERE account_id=$1")
            .bind(u.user_info.account_id)
            .fetch_one(&state.db)
            .await
            .unwrap();
    assert_eq!(balances, (100, 200, 300));
    assert!(!crate::db::query_scalar::<bool>(
        "SELECT EXISTS(SELECT 1 FROM battle_currencies WHERE account=$1 AND kind='LuaPoint')"
    )
    .bind(u.user_info.account_id)
    .fetch_one(&state.db)
    .await
    .unwrap());
}

#[tokio::test]
async fn package_batch_currency_overflow_rolls_back_deferred_awards() {
    let (state, u) = setup().await;
    for (package, gold, gem, paid, expected) in [
        (94067, i64::MAX - 200_000_000, 200, 300, "Fail"),
        (93008, 100, i64::MAX - 4300, 300, "Fail"),
    ] {
        reset_rewards(&state, &u, package, 3).await;
        crate::db::query("UPDATE user_info SET gold=$1,gem=$2,pay_gem=$3 WHERE account_id=$4")
            .bind(gold)
            .bind(gem)
            .bind(paid)
            .bind(u.user_info.account_id)
            .execute(&state.db)
            .await
            .unwrap();
        let result = open(&state, &u, package, 3).await;
        assert_eq!(result["Result"], expected);
        assert_eq!(count(&state, &u, package).await, 3);
        let balances: (i64, i64, i64) =
            crate::db::query_as("SELECT gold,gem,pay_gem FROM user_info WHERE account_id=$1")
                .bind(u.user_info.account_id)
                .fetch_one(&state.db)
                .await
                .unwrap();
        assert_eq!(balances, (gold, gem, paid));
        for id in [40005, 40006, 40007, 40008, 40010] {
            assert_eq!(count(&state, &u, id).await, 0);
        }
        // The same balance succeeds exactly at the bigint boundary.
        let result = open(&state, &u, package, 2).await;
        assert_eq!(result["Result"], "Success");
        let final_currency = result["RewardResults"][1]["CurrencyResults"]
            .as_array()
            .unwrap()
            .iter()
            .rev()
            .find(|c| c["CurrencyType"] == if package == 94067 { "Gold" } else { "Gem" })
            .unwrap();
        assert_eq!(final_currency["NewValue"], i64::MAX);
    }
}

fn fixture_drop(state: &AppState, kind: &str, index: i32, amount: i32) -> Value {
    let pool_index = (0..)
        .map_while(|i| state.tables.reward_string_pool.get(i).map(|code| (i, code)))
        .find(|(_, code)| {
            if index != 0 {
                state.tables.get_item_index(code) == Some(index)
            } else {
                *code == kind
            }
        })
        .unwrap()
        .0;
    json!([0, [pool_index, 0], 1000, amount, null, amount, 0, 0, 0])
}

fn fixture_package(state: &mut AppState, package: i32, drops: Vec<Value>) {
    let tables = Arc::make_mut(&mut state.tables);
    let package = Arc::make_mut(&mut tables.inventory)
        .packages
        .get_mut(&package)
        .unwrap();
    package["MaxOpenCount"] = json!(1000);
    package["RewardRate1"] = json!(1000);
    let index = item::n(package, "RewardIndex1") as i32;
    let reward = Arc::make_mut(&mut tables.rewards)
        .entries
        .iter_mut()
        .find(|r| r.index == index)
        .unwrap();
    reward.gold_rate = 0;
    reward.gem_rate = 0;
    reward.lua_point_rate = 0;
    reward.shop_event_point_rate = 0;
    reward.limited_shop_event_point_rate = 0;
    reward.sub_data_list = drops;
}

async fn metric(state: &AppState, u: &user::LoginResponse, kind: &str, arg: i32) -> i64 {
    crate::db::query_scalar("SELECT COALESCE(SUM(value),0)::bigint FROM progression_metrics WHERE account_id=$1 AND kind=$2 AND arg=$3")
        .bind(u.user_info.account_id).bind(kind).bind(arg).fetch_one(&state.db).await.unwrap()
}

#[tokio::test]
async fn package_batch_special_currency_barriers_keep_running_balances_and_clamping() {
    let (mut state, u) = setup().await;
    reset_rewards(&state, &u, 94067, 4).await;
    let drops = vec![
        fixture_drop(&state, "Gold", 0, 1),
        fixture_drop(&state, "ShakmehMiddleBossPoint", 0, 1),
        fixture_drop(&state, "", 40005, 1),
        fixture_drop(&state, "Gold", 0, 2),
        fixture_drop(&state, "", 40005, 1),
    ];
    fixture_package(&mut state, 94067, drops);
    let max = item::n(
        state
            .tables
            .battle
            .find("CurrencyType", &[("CurrencyType", 48)])
            .unwrap(),
        "MaxValue",
    );
    crate::db::query("INSERT INTO battle_currencies(account,kind,value) VALUES($1,'ShakmehMiddleBossPoint',$2) ON CONFLICT(account,kind) DO UPDATE SET value=excluded.value")
        .bind(u.user_info.account_id).bind(max - 2).execute(&state.db).await.unwrap();
    let gold_metric = metric(&state, &u, "GetGold", 0).await;
    let item_metric = metric(&state, &u, "AddItem", 40005).await;
    let result = open(&state, &u, 94067, 4).await;
    assert_eq!(result["Result"], "Success");
    for (i, reward) in result["RewardResults"]
        .as_array()
        .unwrap()
        .iter()
        .enumerate()
    {
        let currencies = reward["CurrencyResults"].as_array().unwrap();
        assert_eq!(currencies[0]["NewValue"], 101 + i as i64 * 3);
        assert_eq!(currencies[1]["AddValue"], if i < 2 { 1 } else { 0 });
        assert_eq!(currencies[1]["NewValue"], (max - 1 + i as i64).min(max));
        assert_eq!(currencies[2]["NewValue"], 103 + i as i64 * 3);
        assert_eq!(reward["ItemResults"][0]["NewCount"], i as i64 * 2 + 1);
        assert_eq!(reward["ItemResults"][1]["NewCount"], i as i64 * 2 + 2);
    }
    assert_eq!(count(&state, &u, 40005).await, 8);
    assert_eq!(metric(&state, &u, "GetGold", 0).await - gold_metric, 12);
    assert_eq!(metric(&state, &u, "AddItem", 40005).await - item_metric, 8);
}

#[tokio::test]
async fn package_batch_self_rewards_preserve_consumption_and_item_gain_metrics() {
    let (mut state, u) = setup().await;
    let package = (0..)
        .map_while(|i| state.tables.reward_string_pool.get(i))
        .filter_map(|code| state.tables.get_item_index(code))
        .find(|id| {
            state.tables.inventory.packages.contains_key(id)
                && !crate::models::item::disabled_reward(*id)
                && state
                    .tables
                    .items
                    .reward_item(*id)
                    .is_some_and(|m| m.kind == "Item")
        })
        .unwrap();
    let drops = vec![fixture_drop(&state, "", package, 1)];
    fixture_package(&mut state, package, drops);
    put(&state, &u, package, 3).await;
    let gained = metric(&state, &u, "AddItem", package).await;
    let result = open(&state, &u, package, 3).await;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["ItemResult"]["NewCount"], 0);
    for i in 0..3 {
        assert_eq!(
            result["RewardResults"][i]["ItemResults"][0]["NewCount"],
            i + 1
        );
    }
    assert_eq!(count(&state, &u, package).await, 3);
    assert_eq!(metric(&state, &u, "AddItem", package).await - gained, 3);
}

#[tokio::test]
#[ignore = "manual package-opening latency and row-write benchmark"]
async fn package_batch_benchmark() {
    let (state, u) = setup().await;
    for package in [94067, 93008] {
        for run in 0..4 {
            let batch = if run == 0 { 1 } else { 1000 };
            put(&state, &u, package, batch).await;
            let before = crate::database::row_changes(&state.db).await;
            let start = std::time::Instant::now();
            let response = item::use_package_item(
                State(state.clone()),
                form(&u, &format!("ItemIndex={package}&ItemCount={batch}")),
            )
            .await
            .unwrap()
            .0;
            let elapsed = start.elapsed();
            assert_eq!(response["Result"], "Success", "{response}");
            assert_eq!(
                response["RewardResults"].as_array().unwrap().len(),
                batch as usize
            );
            let writes = crate::database::row_changes(&state.db).await - before;
            eprintln!("package={package} batch={batch} run={run} elapsed_ms={:.3} row_writes={writes} response_bytes={}",
                elapsed.as_secs_f64() * 1000.0, serde_json::to_vec(&response).unwrap().len());
        }
    }
}
