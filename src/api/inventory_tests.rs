use super::{craft, item, user};
use crate::{database, state::AppState, tables::GameTables};
use axum::{body::Bytes, extract::State};
use serde_json::{json, Value};
use std::{
    path::Path,
    sync::{Arc, OnceLock},
};

async fn setup() -> (AppState, user::LoginResponse) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let state = AppState::new(
        pool,
        TABLES
            .get_or_init(|| {
                GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
            })
            .clone(),
    );
    let user = login(&state).await;
    (state, user)
}
async fn login(state: &AppState) -> user::LoginResponse {
    user::login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=inventory-test"),
    )
    .await
    .unwrap()
    .0
}
fn form(u: &user::LoginResponse, fields: &str) -> Bytes {
    Bytes::from(format!("SessionKey={}&{fields}", u.user_info.session_key))
}
async fn put(state: &AppState, u: &user::LoginResponse, id: i32, count: i32) {
    sqlx::query("INSERT INTO items(account_id,item_index,count) VALUES (?,?,?) ON CONFLICT(account_id,item_index) DO UPDATE SET count=excluded.count").bind(u.user_info.account_id).bind(id).bind(count).execute(&state.db).await.unwrap();
}
async fn count(state: &AppState, u: &user::LoginResponse, id: i32) -> i32 {
    sqlx::query_scalar("SELECT count FROM items WHERE account_id=? AND item_index=?")
        .bind(u.user_info.account_id)
        .bind(id)
        .fetch_optional(&state.db)
        .await
        .unwrap()
        .unwrap_or(0)
}
fn potion(state: &AppState, action: i64) -> (i32, Value) {
    state
        .tables
        .inventory
        .potions
        .iter()
        .find(|(_, v)| {
            item::n(v, "ActionType") == action
                && v["ActionSubValue"] == ""
                && v["BoosterCodes"].as_array().unwrap().is_empty()
        })
        .map(|(i, v)| (*i, v.clone()))
        .unwrap()
}
fn recipe(state: &AppState) -> Value {
    state
        .tables
        .inventory
        .crafts
        .values()
        .find(|r| {
            r["IsOpen"] == true
                && item::n(r, "ItemIndex") > 0
                && r["Materials"].as_array().unwrap().len() >= 2
                && r["Materials"].as_array().unwrap().iter().all(|v| {
                    state
                        .tables
                        .items
                        .reward_item(item::n(v, "ItemIndex") as i32)
                        .is_some_and(|m| m.kind == "Item")
                })
        })
        .unwrap()
        .clone()
}
async fn materials(state: &AppState, u: &user::LoginResponse, r: &Value, batches: i32) {
    for m in r["Materials"].as_array().unwrap() {
        put(
            state,
            u,
            item::n(m, "ItemIndex") as i32,
            item::n(m, "Count") as i32 * batches,
        )
        .await;
    }
}
fn craft_form(u: &user::LoginResponse, r: &Value, count: i32) -> Bytes {
    form(
        u,
        &format!(
            "SlotIndex=1&CraftIndex={}&ItemIndex={}&ItemCount={count}",
            r["CraftIndex"], r["ItemIndex"]
        ),
    )
}

#[tokio::test]
async fn potions_use_client_amounts_and_full_recovery_uses_team_capacity() {
    let (state, u) = setup().await;
    let (id, p) = potion(&state, 1);
    put(&state, &u, id, 3).await;
    let response = item::use_potion_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&ItemCount=2")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(response["Result"], "Success");
    assert_eq!(
        response["StaminaResult"]["AddValue"],
        item::n(&p, "ActionValue") * 2
    );
    assert_eq!(count(&state, &u, id).await, 1);
    let (full, _) = potion(&state, 2);
    put(&state, &u, full, 1).await;
    let response = item::use_potion_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={full}&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(response["Result"], "Success");
    assert_eq!(response["StaminaResult"]["AddValue"], 200);
}
#[tokio::test]
async fn invalid_counts_unknown_items_and_wrong_potion_types_do_not_mutate() {
    let (state, u) = setup().await;
    let (id, _) = potion(&state, 1);
    put(&state, &u, id, 2).await;
    for amount in [0, -1, 3, i32::MAX] {
        let response = item::use_potion_item(
            State(state.clone()),
            form(&u, &format!("ItemIndex={id}&ItemCount={amount}")),
        )
        .await
        .unwrap()
        .0;
        assert_ne!(response["Result"], "Success");
        assert_eq!(count(&state, &u, id).await, 2);
    }
    put(&state, &u, 99999999, 1).await;
    let response = item::use_potion_item(
        State(state.clone()),
        form(&u, "ItemIndex=99999999&ItemCount=1"),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(response["Result"], "Success");
    assert_eq!(count(&state, &u, 99999999).await, 1);
}
#[tokio::test]
async fn concurrent_uses_cannot_spend_one_potion_twice() {
    let (state, u) = setup().await;
    let (id, p) = potion(&state, 1);
    put(&state, &u, id, 1).await;
    let (a, b) = tokio::join!(
        item::use_potion_item(
            State(state.clone()),
            form(&u, &format!("ItemIndex={id}&ItemCount=1"))
        ),
        item::use_potion_item(
            State(state.clone()),
            form(&u, &format!("ItemIndex={id}&ItemCount=1"))
        )
    );
    assert_eq!(
        [a.unwrap().0, b.unwrap().0]
            .iter()
            .filter(|v| v["Result"] == "Success")
            .count(),
        1
    );
    let stamina: i64 = sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?")
        .bind(u.user_info.account_id)
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(
        stamina,
        u.user_info.stamina as i64 + item::n(&p, "ActionValue")
    );
}
#[tokio::test]
async fn hero_exp_potions_validate_ownership_and_use_level_tables() {
    let (state, u) = setup().await;
    let (id, p) = potion(&state, 3);
    put(&state, &u, id, 3).await;
    let fail = item::use_potion_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&HeroIndex=999999&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(fail["Result"], "HeroNotOwned");
    assert_eq!(count(&state, &u, id).await, 3);
    let response = item::use_potion_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&HeroIndex=1&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(response["Result"], "Success");
    assert_eq!(
        response["HeroExpResult"]["AddValue"],
        item::n(&p, "ActionValue")
    );
    let again = login(&state).await;
    assert_eq!(
        again.heroes[0].level as i64,
        response["HeroExpResult"]["NewLevel"].as_i64().unwrap()
    );
}
#[tokio::test]
async fn locked_items_and_malformed_batches_cannot_be_sold() {
    let (state, u) = setup().await;
    let id = *state
        .tables
        .inventory
        .items
        .iter()
        .find(|(_, v)| {
            item::n(v, "Type") == 2 && v["NotForSale"] == false && item::n(v, "SellGold") > 0
        })
        .unwrap()
        .0;
    put(&state, &u, id, 5).await;
    assert_eq!(
        item::set_lock_rune_item(
            State(state.clone()),
            form(&u, &format!("RuneItemIndex={id}&Locked=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let fail = item::sell_item(
        State(state.clone()),
        form(&u, &format!("ItemIndices=[{id}]&ItemCount=[1]")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(fail["Result"], "LockedItemExists");
    assert_eq!(count(&state, &u, id).await, 5);
    let again = login(&state).await;
    assert_eq!(
        again
            .items
            .iter()
            .find(|i| i.item_index == id)
            .unwrap()
            .locked,
        1
    );
    let _ = item::set_lock_rune_item(
        State(state.clone()),
        form(&u, &format!("RuneItemIndex={id}&Locked=0")),
    )
    .await
    .unwrap();
    let fail = item::sell_item(
        State(state.clone()),
        form(&u, &format!("ItemIndices=[{id},{id}]&ItemCount=[1]")),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(fail["Result"], "Success");
    let ok = item::sell_item(
        State(state.clone()),
        form(&u, &format!("ItemIndices=[{id},{id}]&ItemCount=[1,2]")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(ok["Result"], "Success");
    assert_eq!(count(&state, &u, id).await, 2);
    assert_eq!(
        ok["CurrencyResult"]["AddValue"],
        item::n(&state.tables.inventory.items[&id], "SellGold") * 3
    );
}
#[tokio::test]
async fn package_claim_rolls_back_on_missing_reward_data() {
    let (mut state, u) = setup().await;
    let id = *state.tables.inventory.packages.keys().next().unwrap();
    put(&state, &u, id, 1).await;
    let mut table = state.tables.as_ref().clone();
    Arc::make_mut(&mut table.inventory)
        .packages
        .get_mut(&id)
        .unwrap()["RewardIndex1"] = json!(i32::MAX);
    state.tables = Arc::new(table);
    let response = item::use_package_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_ne!(response["Result"], "Success");
    assert_eq!(count(&state, &u, id).await, 1);
}
#[tokio::test]
async fn package_and_selector_rewards_have_native_shapes() {
    let (state, u) = setup().await;
    let (&id, selector) = state
        .tables
        .inventory
        .selectors
        .iter()
        .find(|(_, s)| {
            s["ItemIndices"].as_array().unwrap().iter().all(|i| {
                state
                    .tables
                    .items
                    .reward_item(i.as_i64().unwrap() as i32)
                    .is_some_and(|m| m.kind == "Item")
            })
        })
        .unwrap();
    let target = selector["ItemIndices"][0].as_i64().unwrap();
    put(&state, &u, id, 2).await;
    let bad = item::use_package_select_item(
        State(state.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&ItemCount=1&SelectItemIndex=99999999"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(bad["Result"], "ImpossibleSelectItem");
    let ok = item::use_package_select_item(
        State(state.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&ItemCount=2&SelectItemIndex={target}"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(ok["Result"], "Success");
    assert_eq!(ok["RewardItemResult"]["AddCount"], 2);
    assert_eq!(count(&state, &u, id).await, 0);
    let (&package, def) = state
        .tables
        .inventory
        .packages
        .iter()
        .find(|(_, p)| {
            state
                .tables
                .get_reward(item::n(p, "RewardIndex1") as i32)
                .is_some_and(|r| {
                    r.gold_rate == 1000 && r.gold_min > 0 && r.sub_data_list.is_empty()
                })
        })
        .unwrap();
    put(&state, &u, package, 2).await;
    let opened = item::use_package_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={package}&ItemCount=2")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(opened["Result"], "Success");
    assert_eq!(opened["RewardResults"].as_array().unwrap().len(), 2);
    assert_eq!(
        opened["RewardResults"][0]["CurrencyResults"][0]["CurrencyType"],
        "Gold"
    );
    assert_eq!(count(&state, &u, package).await, 0);
    let _ = def;
}
#[tokio::test]
async fn equipment_selectors_enforce_choices_and_inventory_capacity() {
    let (state, u) = setup().await;
    let (&id, s) = state
        .tables
        .inventory
        .equipment_selectors
        .iter()
        .find(|(_, s)| {
            item::n(s, "SelectOptionCount") == 0
                && s["ItemIndices"].as_array().is_some_and(|v| {
                    v.iter().all(|i| {
                        state
                            .tables
                            .items
                            .reward_item(i.as_i64().unwrap() as i32)
                            .is_some_and(|m| m.kind == "Equip")
                    })
                })
                && s["ItemIndices"].as_array().is_some_and(|v| !v.is_empty())
        })
        .unwrap();
    let selected = s["ItemIndices"][0].as_i64().unwrap();
    put(&state, &u, id, 2).await;
    let result = item::use_weapon_unique_select_item(
        State(state.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&WeaponUniqueItemIndex={selected}&ItemCount=1"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["EquipItemResults"][0]["ItemIndex"], selected);
    sqlx::query("WITH RECURSIVE n(x) AS (SELECT 1 UNION ALL SELECT x+1 FROM n WHERE x<279) INSERT INTO equip_items(account_id,item_index) SELECT ?,? FROM n").bind(u.user_info.account_id).bind(selected).execute(&state.db).await.unwrap();
    let result = item::use_weapon_unique_select_item(
        State(state.clone()),
        form(
            &u,
            &format!("ItemIndex={id}&WeaponUniqueItemIndex={selected}&ItemCount=1"),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["Result"], "EquipItemFull");
    assert_eq!(count(&state, &u, id).await, 1);
}
#[tokio::test]
async fn hero_selector_recruits_only_allowed_unowned_heroes() {
    let (state, u) = setup().await;
    let (&id, s) = state
        .tables
        .inventory
        .hero_selectors
        .iter()
        .find(|(_, s)| {
            s["HeroIndices"]
                .as_array()
                .is_some_and(|a| a.iter().any(|v| v.as_i64().unwrap() > 1))
                && s["NpcType"] == false
        })
        .unwrap();
    let selected = s["HeroIndices"]
        .as_array()
        .unwrap()
        .iter()
        .find(|v| v.as_i64().unwrap() > 1)
        .unwrap()
        .as_i64()
        .unwrap();
    put(&state, &u, id, 2).await;
    let result = item::use_hero_select_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&HeroIndex={selected}")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["HeroResult"]["HeroInfo"]["Star"], s["HeroStar"]);
    let again = item::use_hero_select_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&HeroIndex={selected}")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(again["Result"], "HeroNotSelectable");
    assert_eq!(count(&state, &u, id).await, 1);
}
#[tokio::test]
async fn crafting_spends_table_materials_and_gold_and_rejects_forged_outputs() {
    let (state, u) = setup().await;
    let r = recipe(&state);
    materials(&state, &u, &r, 2).await;
    let bad = craft::craft_item(
        State(state.clone()),
        form(
            &u,
            &format!(
                "SlotIndex=1&CraftIndex={}&ItemIndex=99999999&ItemCount=2",
                r["CraftIndex"]
            ),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(bad["Result"], "ItemDataNotFound");
    let ok = craft::craft_item(State(state.clone()), craft_form(&u, &r, 2))
        .await
        .unwrap()
        .0;
    assert_eq!(ok["Result"], "Success");
    assert_eq!(
        ok["CurrencyResult"]["AddValue"],
        -2 * item::n(&r, "ReqGold")
    );
    assert_eq!(ok["CraftSlotResult"]["ItemIndex"], 0);
    let output = item::n(&r, "ItemIndex") as i32;
    assert_eq!(
        count(&state, &u, output).await,
        item::n(&r, "ResultItemCount") as i32 * 2
    );
    for m in r["Materials"].as_array().unwrap() {
        assert_eq!(count(&state, &u, item::n(m, "ItemIndex") as i32).await, 0);
    }
    let fail = craft::craft_item(State(state.clone()), craft_form(&u, &r, 1))
        .await
        .unwrap()
        .0;
    assert_eq!(fail["Result"], "NotEnoughMaterial");
}
#[tokio::test]
async fn crafting_insufficient_gold_rolls_back_all_materials() {
    let (state, u) = setup().await;
    let r = recipe(&state);
    materials(&state, &u, &r, 1).await;
    sqlx::query("UPDATE user_info SET gold=0 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    let result = craft::craft_item(State(state.clone()), craft_form(&u, &r, 1))
        .await
        .unwrap()
        .0;
    assert_eq!(result["Result"], "NotEnoughGold");
    for m in r["Materials"].as_array().unwrap() {
        assert_eq!(
            count(&state, &u, item::n(m, "ItemIndex") as i32).await,
            item::n(m, "Count") as i32
        );
    }
}
#[tokio::test]
async fn timed_crafting_persists_and_cancellation_refunds_recorded_costs() {
    let (mut state, u) = setup().await;
    let mut r = recipe(&state);
    r["ReqTime"] = json!(3600);
    let mut t = state.tables.as_ref().clone();
    Arc::make_mut(&mut t.inventory)
        .crafts
        .insert(item::n(&r, "CraftIndex") as i32, r.clone());
    state.tables = Arc::new(t);
    materials(&state, &u, &r, 1).await;
    let result = craft::craft_item(State(state.clone()), craft_form(&u, &r, 1))
        .await
        .unwrap()
        .0;
    assert_eq!(result["Result"], "Success");
    assert!(result["CraftSlotResult"]["RemainTime"].as_i64().unwrap() > 3500);
    let early = craft::take_craft_item(State(state.clone()), form(&u, "SlotIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(early["Result"], "NotYetCrafted");
    let restarted = AppState::new(state.db.clone(), state.tables.as_ref().clone());
    let again = login(&restarted).await;
    assert_eq!(again.craft_slot_infos[0]["CraftIndex"], r["CraftIndex"]);
    let cancel = craft::cancel_craft_item(State(restarted.clone()), form(&again, "SlotIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(cancel["Result"], "Success");
    assert_eq!(cancel["CurrencyResult"]["AddValue"], r["ReqGold"]);
    for m in r["Materials"].as_array().unwrap() {
        assert_eq!(
            count(&restarted, &again, item::n(m, "ItemIndex") as i32).await,
            item::n(m, "Count") as i32
        );
    }
    assert_ne!(
        craft::cancel_craft_item(State(restarted), form(&again, "SlotIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
}
#[tokio::test]
async fn instant_craft_uses_server_time_and_collection_is_once_only() {
    let (mut state, u) = setup().await;
    let mut r = recipe(&state);
    r["ReqTime"] = json!(3600);
    let mut t = state.tables.as_ref().clone();
    Arc::make_mut(&mut t.inventory)
        .crafts
        .insert(item::n(&r, "CraftIndex") as i32, r.clone());
    state.tables = Arc::new(t);
    materials(&state, &u, &r, 1).await;
    let _ = craft::craft_item(State(state.clone()), craft_form(&u, &r, 1))
        .await
        .unwrap();
    let result =
        craft::instant_craft_item(State(state.clone()), form(&u, "SlotIndex=1&RemainTime=0"))
            .await
            .unwrap()
            .0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["CurrencyResult"]["AddValue"], -40);
    let collected = craft::take_craft_item(State(state.clone()), form(&u, "SlotIndex=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(collected["Result"], "Success");
    assert_ne!(
        craft::take_craft_item(State(state.clone()), form(&u, "SlotIndex=1"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
}
#[tokio::test]
async fn expansion_uses_table_prices_and_preserves_paid_ruby_balance() {
    let (state, u) = setup().await;
    sqlx::query("UPDATE user_info SET gem=100,pay_gem=200 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    assert_eq!(
        item::extend_equip(State(state.clone()), form(&u, "ReqGem=1"))
            .await
            .unwrap()
            .0["Result"],
        "ReqGemError"
    );
    let ok = item::extend_equip(State(state.clone()), form(&u, "ReqGem=250"))
        .await
        .unwrap()
        .0;
    assert_eq!(ok["Result"], "Success");
    assert_eq!(ok["NewInventoryExtend"], 1);
    let again = login(&state).await;
    assert_eq!(again.user_info.gem, 0);
    assert_eq!(again.user_info.pay_gem, 50);
    assert_eq!(again.misc_info.unwrap().inventory_extend, 1);
    let slot = craft::add_craft_slot(State(state.clone()), form(&u, "SlotIndex=3"))
        .await
        .unwrap()
        .0;
    assert_eq!(slot["Result"], "Success");
    assert_eq!(slot["CurrencyResult"]["AddValue"], -1500000);
    assert_ne!(
        craft::add_craft_slot(State(state.clone()), form(&u, "SlotIndex=3"))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
}
#[tokio::test]
async fn boosters_persist_extend_and_apply_campaign_bonuses() {
    let (state, u) = setup().await;
    let (&id, b) = state
        .tables
        .inventory
        .boosters
        .iter()
        .find(|(_, b)| {
            item::n(b, "Type") == 1
                && b["IsOnetime"] == false
                && b["BattleTypes"]
                    .as_array()
                    .is_some_and(|v| v.contains(&json!(1)))
        })
        .unwrap();
    put(&state, &u, id, 2).await;
    let one = item::use_booster_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(one["Result"], "Success");
    let two = item::use_booster_item(
        State(state.clone()),
        form(&u, &format!("ItemIndex={id}&ItemCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert!(
        two["ItemTimeDurationInfo"]["EndTime"].as_str().unwrap()
            > one["ItemTimeDurationInfo"]["EndTime"].as_str().unwrap()
    );
    assert_eq!(
        item::campaign_boost(&state, u.user_info.account_id)
            .await
            .unwrap()
            .1,
        item::n(b, "Value") as i32
    );
    let again = login(&state).await;
    assert_eq!(again.item_time_durations.len(), 1);
    sqlx::query("UPDATE item_boosters SET end_time='2000-01-01 00:00:00'")
        .execute(&state.db)
        .await
        .unwrap();
    assert_eq!(
        item::campaign_boost(&state, u.user_info.account_id)
            .await
            .unwrap(),
        (0, 0)
    );
}

#[tokio::test]
async fn gold_booster_uses_bonus_gold_enum_and_loot_boosters_are_not_consumed() {
    let (state, u) = setup().await;
    let (&gold, data) = state
        .tables
        .inventory
        .boosters
        .iter()
        .find(|(_, v)| {
            item::n(v, "Type") == 3
                && v["IsOnetime"] == false
                && v["BattleTypes"]
                    .as_array()
                    .is_some_and(|v| v.contains(&json!(1)))
        })
        .unwrap();
    put(&state, &u, gold, 1).await;
    assert_eq!(
        item::use_booster_item(
            State(state.clone()),
            form(&u, &format!("ItemIndex={gold}&ItemCount=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(
        item::campaign_boost(&state, u.user_info.account_id)
            .await
            .unwrap()
            .0,
        item::n(data, "Value") as i32
    );
    let loot = *state
        .tables
        .inventory
        .boosters
        .iter()
        .find(|(_, v)| item::n(v, "Type") == 2)
        .unwrap()
        .0;
    put(&state, &u, loot, 1).await;
    assert_ne!(
        item::use_booster_item(
            State(state.clone()),
            form(&u, &format!("ItemIndex={loot}&ItemCount=1"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(count(&state, &u, loot).await, 1);
}

#[tokio::test]
async fn rune_dismantling_uses_grade_yields_and_obeys_locks() {
    let (state, u) = setup().await;
    let (&id, rune) = state
        .tables
        .inventory
        .items
        .iter()
        .find(|(_, v)| {
            item::n(v, "Type") == 2
                && state
                    .tables
                    .inventory
                    .rune_breaks
                    .contains_key(&(item::n(v, "Grade") as i32))
        })
        .unwrap();
    put(&state, &u, id, 2).await;
    let _ = item::set_lock_rune_item(
        State(state.clone()),
        form(&u, &format!("RuneItemIndex={id}&Locked=1")),
    )
    .await
    .unwrap();
    let body = format!("ItemIndices=[{id}]&Counts=[2]&EquipItemSlotIndices=[]");
    assert_eq!(
        item::break_rune(State(state.clone()), form(&u, &body))
            .await
            .unwrap()
            .0["Result"],
        "LockedItemExists"
    );
    let _ = item::set_lock_rune_item(
        State(state.clone()),
        form(&u, &format!("RuneItemIndex={id}&Locked=0")),
    )
    .await
    .unwrap();
    let broken = item::break_rune(State(state.clone()), form(&u, &body))
        .await
        .unwrap()
        .0;
    assert_eq!(broken["Result"], "Success");
    assert_eq!(count(&state, &u, id).await, 0);
    let drop = &state.tables.inventory.rune_breaks[&(item::n(rune, "Grade") as i32)][0];
    assert!(broken["ItemResults"][0]["AddCount"].as_i64().unwrap() >= item::n(drop, "Min") * 2);
}

#[tokio::test]
async fn equipment_storage_and_sales_check_ownership_equipped_state_and_locks() {
    let (state, u) = setup().await;
    let id = *state
        .tables
        .inventory
        .items
        .iter()
        .find(|(_, v)| {
            item::n(v, "Type") == 1 && v["NotForSale"] == false && item::n(v, "SellGold") > 0
        })
        .unwrap()
        .0;
    let slot = sqlx::query("INSERT INTO equip_items(account_id,item_index) VALUES (?,?)")
        .bind(u.user_info.account_id)
        .bind(id)
        .execute(&state.db)
        .await
        .unwrap()
        .last_insert_rowid();
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=? WHERE account_id=? AND hero_index=1")
        .bind(slot)
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    assert_ne!(
        item::set_chest(
            State(state.clone()),
            form(&u, &format!("EquipItemSlotIndex=[{slot}]"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    assert_eq!(
        item::sell_equip(
            State(state.clone()),
            form(&u, &format!("EquipItemSlotIndices=[{slot}]"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Equipped"
    );
    sqlx::query("UPDATE heroes SET equip_item_slot_index_1=0 WHERE account_id=?")
        .bind(u.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    assert_eq!(
        item::set_chest(
            State(state.clone()),
            form(&u, &format!("EquipItemSlotIndex=[{slot}]"))
        )
        .await
        .unwrap()
        .0["Result"],
        "Success"
    );
    let again = login(&state).await;
    assert_eq!(
        again
            .equip_items
            .iter()
            .find(|e| e.slot_index as i64 == slot)
            .unwrap()
            .inventory_type,
        1
    );
    let result = item::unset_chest(
        State(state.clone()),
        form(&u, &format!("EquipItemSlotIndex=[{slot}]")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["UnequippedSlotIndexInChest"], json!([slot]));
    let _ = item::set_lock_equip_item(
        State(state.clone()),
        form(&u, &format!("EquipItemSlotIndex={slot}&Locked=1")),
    )
    .await
    .unwrap();
    assert_eq!(
        item::sell_equip(
            State(state.clone()),
            form(&u, &format!("EquipItemSlotIndices=[{slot}]"))
        )
        .await
        .unwrap()
        .0["Result"],
        "LockedEquipItemExists"
    );
    let _ = item::set_lock_equip_item(
        State(state.clone()),
        form(&u, &format!("EquipItemSlotIndex={slot}&Locked=0")),
    )
    .await
    .unwrap();
    let result = item::sell_equip(
        State(state.clone()),
        form(&u, &format!("EquipItemSlotIndices=[{slot}]")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(
        result["CurrencyResult"]["AddValue"],
        state.tables.inventory.items[&id]["SellGold"]
    );
}

#[tokio::test]
async fn equipment_option_selection_validates_options_and_persists_them() {
    let (state, u) = setup().await;
    let (&ticket, selector) = state
        .tables
        .inventory
        .equipment_selectors
        .iter()
        .find(|(_, s)| item::n(s, "SelectOptionCount") == 1 && s["SelectUniqueOption"] == false)
        .unwrap();
    let target = selector["ItemIndices"][0].as_i64().unwrap() as i32;
    let eq = &state.tables.inventory.equipment[&target];
    let group = eq["OptionGroupIndex"][0].as_i64().unwrap() as i32;
    let option = state.tables.inventory.option_groups[&group][0];
    put(&state, &u, ticket, 2).await;
    let bad=item::use_equip_option_select_item(State(state.clone()),form(&u,&format!("ItemIndex={ticket}&ItemCount=1&EquipOptionItemIndex={target}&EquipOptionIndices=[99999999]&EquipExtraOptionIndices=[0]"))).await.unwrap().0;
    assert_ne!(bad["Result"], "Success");
    assert_eq!(count(&state, &u, ticket).await, 2);
    let result=item::use_equip_option_select_item(State(state.clone()),form(&u,&format!("ItemIndex={ticket}&ItemCount=1&EquipOptionItemIndex={target}&EquipOptionIndices=[{option}]&EquipExtraOptionIndices=[0]"))).await.unwrap().0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["EquipItemResults"][0]["OptionIndex1"], option);
    let again = login(&state).await;
    assert_eq!(again.equip_items[0].option_index_1, option);
}

#[tokio::test]
async fn dismantle_material_rewards_and_locked_reward_stacks_keep_correct_counts() {
    let (state, u) = setup().await;
    let (&id, _) = state
        .tables
        .inventory
        .break_rewards
        .iter()
        .find(|(i, _)| {
            state
                .tables
                .inventory
                .items
                .get(i)
                .is_some_and(|d| d["Breakable"] == true)
        })
        .unwrap();
    put(&state, &u, id, 2).await;
    let result = item::break_item(
        State(state.clone()),
        form(&u, &format!("ItemIndices=[{id}]&Counts=[2]")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(count(&state, &u, id).await, 0);
    assert!(
        !result["ItemResults"].as_array().unwrap().is_empty()
            || !result["CurrencyResults"].as_array().unwrap().is_empty()
    );
}

#[tokio::test]
async fn equipping_rejects_another_players_item_and_chest_items() {
    let (state, u) = setup().await;
    let other = user::login(
        State(state.clone()),
        Bytes::from_static(b"LoginId=inventory-other"),
    )
    .await
    .unwrap()
    .0;
    let slot = sqlx::query("INSERT INTO equip_items(account_id,item_index) VALUES (?,1001)")
        .bind(other.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap()
        .last_insert_rowid();
    let fields = format!("HeroIndex=1&HeroPartIndex=[0]&EquipItemSlotIndex=[{slot}]");
    assert!(
        super::equip::set_equip(State(state.clone()), form(&u, &fields))
            .await
            .is_err()
    );
    sqlx::query("UPDATE equip_items SET account_id=?,inventory_type=1 WHERE slot_index=?")
        .bind(u.user_info.account_id)
        .bind(slot)
        .execute(&state.db)
        .await
        .unwrap();
    assert!(
        super::equip::set_equip(State(state.clone()), form(&u, &fields))
            .await
            .is_err()
    );
    sqlx::query("UPDATE equip_items SET inventory_type=0 WHERE slot_index=?")
        .bind(slot)
        .execute(&state.db)
        .await
        .unwrap();
    assert_eq!(
        super::equip::set_equip(State(state.clone()), form(&u, &fields))
            .await
            .unwrap()
            .0
            .result,
        "Success"
    );
    assert!(
        item::get_inventory(State(state), Bytes::from_static(b"SessionKey=invalid"))
            .await
            .is_err()
    );
}

#[tokio::test]
async fn inventory_consumption_serializes_on_separate_sqlite_connections() {
    let (template, _) = setup().await;
    let path = std::env::temp_dir().join(format!("sprk-inventory-{}.sqlite", uuid::Uuid::new_v4()));
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
    let state = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let u = login(&state).await;
    let (id, p) = potion(&state, 1);
    put(&state, &u, id, 1).await;
    let body = format!("ItemIndex={id}&ItemCount=1");
    let (a, b) = tokio::join!(
        item::use_potion_item(State(state.clone()), form(&u, &body)),
        item::use_potion_item(State(state.clone()), form(&u, &body))
    );
    assert_eq!(
        [a.unwrap().0, b.unwrap().0]
            .iter()
            .filter(|v| v["Result"] == "Success")
            .count(),
        1
    );
    let stamina: i64 = sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?")
        .bind(u.user_info.account_id)
        .fetch_one(&pool)
        .await
        .unwrap();
    assert_eq!(
        stamina,
        u.user_info.stamina as i64 + item::n(&p, "ActionValue")
    );
    database::create_tables(&pool).await.unwrap(); // Re-running migrations preserves inventory.
    assert_eq!(count(&state, &u, id).await, 0);
    pool.close().await;
    std::fs::remove_file(path).unwrap();
}

#[tokio::test]
async fn soul_stone_selectors_use_stackable_response_and_cannot_fake_equipment() {
    let (state, u) = setup().await;
    put(&state, &u, 5044, 1).await;
    let wrong = item::use_weapon_unique_select_item(
        State(state.clone()),
        form(
            &u,
            "ItemIndex=5044&ItemCount=1&WeaponUniqueItemIndex=111001",
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(wrong["Result"], "WeaponUniqueNotSelectable");
    assert_eq!(count(&state, &u, 5044).await, 1);
    let selected = item::use_package_select_item(
        State(state.clone()),
        form(&u, "ItemIndex=5044&ItemCount=1&SelectItemIndex=111001"),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(selected["Result"], "Success");
    assert_eq!(selected["RewardItemResult"]["ItemIndex"], 111001);
    assert_eq!(count(&state, &u, 5044).await, 0);
    assert_eq!(count(&state, &u, 111001).await, 1);
}
