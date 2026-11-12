//! Native equipment and hero extensions; all costs and mutations share one transaction.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
    tutorial::Rewards,
};
use crate::{
    error::{Result, ServerError},
    models::equip::EquipItemInfo,
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};
use sqlx::{PgConnection};
use crate::db::RowExt;
use std::collections::BTreeSet;
pub(crate) mod buffs;
pub(crate) mod consumables;
pub(crate) mod cosmetics;
mod equipment;
mod npc;
pub(crate) async fn npc_friendly_snapshot(db: &mut PgConnection,state: &AppState,account:i64)->Result<Vec<Value>> {
    npc::snapshot(db,state,account).await
}
pub(crate) mod punishment;
mod runes;
pub(crate) mod schema;
mod soul;
mod storage;
#[cfg(test)]
mod tests;
pub(crate) mod valance;

pub(crate) fn refresh_soul_weapon_stats(state: &AppState, infos: &mut [Value]) -> Result<()> {
    for info in infos { soul::stats(state,info)?; }
    Ok(())
}

pub fn routes() -> axum::Router<AppState> {
    let mut router = axum::Router::new();
    for (family,actions) in [
        ("equip", "awaken_equip upgrade_equip upgrade_equip_tier upgrade_equip_option max_upgrade_equip_option renew_equip_option renew_confirm_equip_option renew_equip_skill renew_confirm_equip_skill enchant_equip confirm_equip_enchant_option restore_artifact break_equip soul_weapon_liberation soul_weapon_upgrade soul_weapon_ether_injection soul_weapon_reinforce soul_weapon_renew_option soul_weapon_confirm_renew_option soul_weapon_transition restore_soul_stone confirm_soul_stone get_soul_stone_mileage_reward"),
        ("hero", "equip_rune unequip_rune extend_rune_page apply_hero_rune_page"),
        ("hero", "buy_customizing_costumes edit_accessory_costume_position reset_all_customizing_costumes"),
        ("npc", "give_gift_item recv_reward_friendly_point"),
        ("valance", "valance_craft valance_identified valance_awaken valance_enchant confirm_valance_enchant valance_tier_upgrade"),
        ("equip_storage_slot", "buy_equip_storage_slot add_equip_storage_slot set_equip_storage_slot remove_hero_equip_storage_slot change_equip_storage_slot_name reset_equip_storage_slot"),
        ("class_buff", "get_class_buff_info init_class_buff level_up_class_buff"),
        ("user", "reinforce_team_level_buff"),
        ("punishment_rune", "craft_punishment_rune"),
        ("equip", "extend_punishment_rune_storage_slot soul_break soul_weapon_limit_break"),
    ] {for action in actions.split_whitespace(){router=router.route(&format!("/{family}/{action}"),axum::routing::post(handle));}}
    router
}

pub async fn handle(
    State(state): State<AppState>,
    axum::extract::OriginalUri(uri): axum::extract::OriginalUri,
    body: Bytes,
) -> Result<Json<Value>> {
    let req = if uri.path() == "/punishment_rune/craft_punishment_rune" {
        Request::parse_with_integer_arrays(&body, &["IngredientRuneIndices"])?
    } else if uri.path() == "/npc/give_gift_item" {
        Request::parse_with_integer_arrays(&body, &["ItemIndices", "ItemCount"])?
    } else if uri.path().starts_with("/valance/") {
        Request::parse_with_integer_arrays(&body, &["EquipItemSlotIndices", "SelectEquipOptionIndices"])?
    } else if uri.path() == "/equip/break_equip" {
        Request::parse_with_integer_arrays(&body, &["EquipItemSlotIndices"])?
    } else if uri.path() == "/hero/buy_customizing_costumes" {
        Request::parse_with_integer_arrays(&body, &["UnsetAccessoryCostumeIndices"])?
    } else if uri.path() == "/equip_storage_slot/add_equip_storage_slot" {
        Request::parse_with_integer_arrays(&body, &["HeroPartIndex", "EquipItemSlotIndex"])?
    } else if matches!(uri.path(), "/equip/awaken_equip" | "/equip/upgrade_equip") {
        Request::parse_with_integer_arrays(&body, &["MaterialSlotIndices", "MaterialItemInfors"])?
    } else if uri.path() == "/equip/soul_weapon_ether_injection" {
        Request::parse_with_integer_arrays(&body, &["InjectionItemInfos"])?
    } else { Request::parse(&body)? };
    let account = req.account(&state)?;
    let action = uri.path().rsplit('/').next().unwrap_or("");
    let mut tx = crate::database::begin(&state.db).await?;
    item::init(&mut tx, &state, account).await?;
    let result = if matches!(
        action,
        "craft_punishment_rune" | "extend_punishment_rune_storage_slot"
    ) {
        punishment::execute(&mut tx, &state, account, &req, action).await
    } else if uri.path().starts_with("/item/") {
        consumables::execute(&mut tx, &state, account, &req, action).await
    } else if action.contains("class_buff") || action == "reinforce_team_level_buff" {
        buffs::execute(&mut tx, &state, account, &req, action).await
    } else if action.contains("equip_storage_slot") {
        storage::execute(&mut tx, &state, account, &req, action).await
    } else if matches!(
        action,
        "buy_customizing_costumes"
            | "edit_accessory_costume_position"
            | "reset_all_customizing_costumes"
    ) {
        cosmetics::execute(&mut tx, &state, account, &req, action).await
    } else if action.contains("valance") {
        valance::execute(&mut tx, &state, account, &req, action).await
    } else if matches!(
        action,
        "equip_rune" | "unequip_rune" | "extend_rune_page" | "apply_hero_rune_page"
    ) {
        runes::execute(&mut tx, &state, account, &req, action).await
    } else if matches!(action, "give_gift_item" | "recv_reward_friendly_point") {
        npc::execute(&mut tx, &state, account, &req, action).await
    } else if action.contains("soul_") {
        soul::execute(&mut tx, &state, account, &req, action).await
    } else {
        equipment::execute(&mut tx, &state, account, &req, action).await
    };
    match result {
        Ok(out) => {
            tx.commit().await?;
            Ok(Json(out))
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            if action == "soul_weapon_ability_ticket" {
                tracing::warn!(account, ticket = req.text("ItemIndex"), slot = req.text("TargetEquipItemSlotIndex"), reason = %code, "Soul Weapon ticket rejected");
            }
            if uri.path().starts_with("/equip_storage_slot/") {
                tracing::warn!(action, account, reason = %code, "Gear Custom request rejected");
            }
            if action == "break_equip" {
                let selected = serde_json::from_str::<Vec<Value>>(req.text("EquipItemSlotIndices"))
                    .map(|v| v.len()).unwrap_or(0);
                tracing::warn!(account, selected, reason = %code, "Equipment grind rejected");
            }
            if action == "upgrade_equip_tier" {
                tracing::warn!(account, slot = req.text("EquipItemSlotIndex"), reason = %code, "Equipment tier upgrade/restoration rejected");
            }
            let code = if matches!(action, "upgrade_equip_option" | "max_upgrade_equip_option") {
                tracing::warn!(action, account, slot = req.text("EquipItemSlotIndex"), reason = %code, "Equipment option enhancement rejected");
                match code.as_str() {
                    "EquipItemNotFound" => "EquipNotOwned",
                    "ItemNotOwned" | "NotEnoughItem" => "NotEnoughMaterialItem",
                    other => other,
                }
            } else if action == "upgrade_equip_tier" {
                // These native result names have localized messages. Falling
                // back to Fail makes the client misleadingly report a network issue.
                match code.as_str() {
                    "EquipItemNotFound" => "EquipNotOwned",
                    "NotEnoughStar" => "NotEquipMaxStar",
                    "ItemNotOwned" => "NotEnoughMaterialItem",
                    "UnconfirmedOption" => "ImpossibleEquipItem",
                    other => other,
                }
            } else if action == "break_equip" {
                match code.as_str() {
                    "EquipItemNotFound" => "EquipNotOwned",
                    "LockedEquip" => "LockedEquipItemExists",
                    "EquippedItem" => "Equipped",
                    "InvalidMaterial" => "SoulWeaponCanNotBreak",
                    other => other,
                }
            } else { &code };
            Ok(Json(
                json!({"BaseResult":"Success","Result":state.tables.hero_shop.result(action,code)}),
            ))
        }
        Err(e) => Err(e),
    }
}
fn row<'a>(state: &'a AppState, table: &str, fields: &[(&str, i64)]) -> Result<&'a Value> {
    state
        .tables
        .extensions
        .find(table, fields)
        .ok_or_else(|| rule("ItemDataNotFound"))
}
fn meta(state: &AppState, item: i32) -> Result<&Value> {
    equip_item(state, item as i64).ok_or_else(|| rule("ItemDataNotFound"))
}
/// The first `EquipItem` row with this ItemIndex, like `find`, but indexed once
/// per loaded table instead of scanning thousands of rows on every lookup.
pub(crate) fn equip_item(state: &AppState, item: i64) -> Option<&Value> {
    type Index = std::sync::Arc<std::collections::HashMap<i64, usize>>;
    static CACHE: std::sync::Mutex<Option<(std::sync::Arc<crate::tables::ExtensionTable>, Index)>> = std::sync::Mutex::new(None);
    let index = {
        let mut cache = CACHE.lock().unwrap_or_else(|e| e.into_inner());
        match cache.as_ref() {
            Some((table, index)) if std::sync::Arc::ptr_eq(table, &state.tables.extensions) => index.clone(),
            _ => {
                let mut index = std::collections::HashMap::new();
                for (i, row) in state.tables.extensions.rows("EquipItem").iter().enumerate() {
                    if let Some(id) = row["ItemIndex"].as_i64() { index.entry(id).or_insert(i); }
                }
                let index: Index = std::sync::Arc::new(index);
                *cache = Some((state.tables.extensions.clone(), index.clone()));
                index
            }
        }
    };
    index.get(&item).and_then(|i| state.tables.extensions.rows("EquipItem").get(*i))
}
pub(crate) async fn equip(db: &mut PgConnection, account: i64, slot: i64) -> Result<EquipItemInfo> {
    let r = crate::db::query(
        "SELECT * FROM equip_items WHERE account_id=$1 AND slot_index=$2 AND inventory_type=0",
    )
    .bind(account)
    .bind(slot)
    .fetch_optional(db)
    .await?
    .ok_or_else(|| rule("EquipItemNotFound"))?;
    Ok(EquipItemInfo::from_row(&r))
}
pub(crate) async fn save_equip(
    db: &mut PgConnection,
    account: i64,
    eq: &EquipItemInfo,
) -> Result<()> {
    let value = json!(eq);
    let mut query = sqlx::QueryBuilder::new("UPDATE equip_items SET ");
    {
        let mut set = query.separated(",");
        for (key, v) in value.as_object().unwrap() {
            if key != "SlotIndex" && v.is_number() {
                set.push(format!("{}=", EquipItemInfo::column(key)))
                    .push_bind_unseparated(v.as_i64().unwrap());
            }
        }
    }
    query
        .push(" WHERE account_id=")
        .push_bind(account)
        .push(" AND slot_index=")
        .push_bind(eq.slot_index);
    query.build().execute(&mut *db).await?;
    crate::db::query(
        "UPDATE equip_items SET punishment_rune_option=$1 WHERE account_id=$2 AND slot_index=$3",
    )
    .bind(eq.punishment_rune_option.as_ref().map(Value::to_string))
    .bind(account)
    .bind(eq.slot_index)
    .execute(db)
    .await?;
    Ok(())
}
pub(crate) async fn material(db: &mut PgConnection, account: i64, slot: i64) -> Result<EquipItemInfo> {
    material_checked(db, account, slot, false).await
}
async fn material_checked(db: &mut PgConnection, account: i64, slot: i64, disposing: bool) -> Result<EquipItemInfo> {
    let eq = equip(db, account, slot).await?;
    if eq.locked != 0 {
        return Err(rule("LockedEquip"));
    }
    // One prepared statement for every slot (formatting the slot into the SQL
    // prepared a new statement per piece in batch breaks).
    let worn: bool = crate::db::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=$1 AND $2 IN (equip_item_slot_index_1,equip_item_slot_index_2,equip_item_slot_index_3,equip_item_slot_index_4,equip_item_slot_index_5,equip_item_slot_index_6,equip_item_slot_index_7,equip_item_slot_index_8,equip_item_slot_index_9,equip_item_slot_index_10))",
    )
    .bind(account)
    .bind(slot)
    .fetch_one(&mut *db)
    .await?;
    let pending: bool = crate::db::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=$1 AND slot_index=$2)",
    )
    .bind(account)
    .bind(slot)
    .fetch_one(&mut *db)
    .await?;
    if worn || (pending && !disposing) {
        return Err(rule("EquippedItem"));
    }
    if !get(&mut *db, account, "soul", slot).await?.is_null() {
        return Err(rule("InvalidMaterial"));
    }
    Ok(eq)
}
/// `remove_equip` for a batch: presets and storage are rewritten once.
async fn remove_equips(db: &mut PgConnection, account: i64, slots: &BTreeSet<i64>) -> Result<()> {
    crate::api::heroes::presets::forget_equipments(db, account, slots).await?;
    crate::db::query("DELETE FROM equip_items WHERE account_id=$1 AND slot_index IN (SELECT sprk_json_ints($2))")
        .bind(account)
        .bind(json!(slots).to_string())
        .execute(&mut *db)
        .await?;
    Ok(())
}
async fn remove_equip(db: &mut PgConnection, account: i64, slot: i32) -> Result<()> {
    crate::api::heroes::presets::forget_equipment(db, account, slot as i64).await?;
    crate::db::query("DELETE FROM equip_items WHERE account_id=$1 AND slot_index=$2")
        .bind(account)
        .bind(slot)
        .execute(&mut *db)
        .await?;
    Ok(())
}
fn ids(req: &Request, key: &str) -> Result<Vec<i64>> {
    ids_with_limit(req, key, 100)
}
fn ids_with_limit(req: &Request, key: &str, limit: usize) -> Result<Vec<i64>> {
    if req.text(key).is_empty() {
        return Ok(vec![]);
    }
    let list: Vec<i64> =
        serde_json::from_str(req.text(key)).map_err(|_| rule("InvalidItemCount"))?;
    if list.len() > limit
        || list.iter().any(|v| *v <= 0)
        || list.iter().collect::<BTreeSet<_>>().len() != list.len()
    {
        return Err(rule("InvalidItemCount"));
    }
    Ok(list)
}
fn currency_type(v: i64) -> Result<&'static str> {
    match v {
        1 => Ok("Gold"),
        2 => Ok("Gem"),
        3 => Ok("Stamina"),
        4 => Ok("PvpCoin"),
        6 => Ok("Mileage"),
        7 => Ok("FriendshipPoint"),
        8 => Ok("GuildPoint"),
        14 => Ok("RaidPoint"),
        _ => Err(rule("InvalidCurrencyType")),
    }
}
fn roll(chance: i64, scale: i64) -> bool {
    use rand::RngExt;
    rand::rng().random_range(0..scale) < chance.clamp(0, scale)
}

pub(crate) async fn get(db: &mut PgConnection, account: i64, kind: &str, id: i64) -> Result<Value> {
    let text: Option<String> = crate::db::query_scalar(
        "SELECT data FROM extension_state WHERE account_id=$1 AND kind=$2 AND idx=$3",
    )
    .bind(account)
    .bind(kind)
    .bind(id)
    .fetch_optional(db)
    .await?;
    text.map(|s| serde_json::from_str(&s).map_err(|e| ServerError::Internal(e.to_string())))
        .transpose()
        .map(|v| v.unwrap_or(Value::Null))
}
pub(crate) async fn put(
    db: &mut PgConnection,
    account: i64,
    kind: &str,
    id: i64,
    value: &Value,
) -> Result<()> {
    crate::db::query("INSERT INTO extension_state(account_id,kind,idx,data) VALUES($1,$2,$3,$4) ON CONFLICT(account_id,kind,idx) DO UPDATE SET data=excluded.data").bind(account).bind(kind).bind(id).bind(value.to_string()).execute(db).await?;
    Ok(())
}
pub(crate) async fn list(
    db: &mut PgConnection,
    account: i64,
    kind: &str,
) -> Result<Vec<Value>> {
    let rows: Vec<String> = crate::db::query_scalar(
        "SELECT data FROM extension_state WHERE account_id=$1 AND kind=$2 ORDER BY idx",
    )
    .bind(account)
    .bind(kind)
    .fetch_all(db)
    .await?;
    rows.iter()
        .map(|s| serde_json::from_str(s).map_err(|e| ServerError::Internal(e.to_string())))
        .collect()
}

pub(crate) async fn misc(
    db: &mut PgConnection,
    account: i64,
) -> Result<serde_json::Map<String, Value>> {
    let mut out = json!({"RenewOptionEquipItemSlotIndex":0,"RenewOptionResultIndex":0,"RenewOptionResultStep":0,"RenewOptionSlotIndex":0,"RenewEnchantOptionEquipItemSlotIndex":0,"RenewEnchantOptionResultIndex":0,"RenewEnchantOptionResultStep":0,"SoulStoneMileage":0,"RestoreSoulStoneIndices":""});
    let pending =
        crate::db::query("SELECT slot_index,kind,data FROM equipment_pending WHERE account_id=$1")
            .bind(account)
            .fetch_all(&mut *db)
            .await?;
    for r in pending {
        let slot: i64 = r.get("slot_index");
        let kind: String = r.get("kind");
        let p: Value = serde_json::from_str(&r.get::<String, _>("data")).map_err(|_| rule("InvalidOption"))?;
        match kind.as_str() {
            "option" => {
                let v = json!({"RenewOptionEquipItemSlotIndex":slot,"RenewOptionResultIndex":p["Index"],"RenewOptionResultStep":p["Step"],"RenewOptionSlotIndex":p["Slot"]});
                for (k, v) in v.as_object().unwrap() {
                    out[k] = v.clone();
                }
                out["ValanceRenewOptionInfo"] = v;
            }
            "enchant" => {
                out["RenewEnchantOptionEquipItemSlotIndex"] = json!(slot);
                out["RenewEnchantOptionResultIndex"] = p["Index"].clone();
                out["RenewEnchantOptionResultStep"] = p["Step"].clone();
            }
            "skill" => {
                out["ValanceRenewSkillInfo"] = json!({"RenewSkillEquipItemSlotIndex":slot,"RenewSkillResultIndex":p["Index"],"RenewSkillResultStep":p["Step"],"RenewSkillSlotIndex":p["Slot"]});
            }
            "valance_enchant" => out["ValanceEnchantOptionInfo"] = p,
            _ => {}
        }
    }
    let restore = get(db, account, "soul_restore", 0).await?;
    if !restore.is_null() {
        out["SoulStoneMileage"] = restore["Mileage"].clone();
        // The native client treats every nonempty string (including "[]") as
        // a pending Soul Stone selection. Match SetRestoreSoulStoneIndices:
        // empty text for no choices, comma-separated indices otherwise.
        out["RestoreSoulStoneIndices"] = json!(restore["Choices"].as_array()
            .into_iter().flatten().filter_map(Value::as_i64)
            .map(|id| id.to_string()).collect::<Vec<_>>().join(","));
    }
    out["PunishmentRuneStorageSlotExtend"] = json!(n(
        &get(db, account, "punishment_storage", 0).await?,
        "Extend"
    ));
    out["EquipStorageSlotExtend"] =
        json!((list(db, account, "equip_storage").await?.len() as i64 - 4).max(0));
    Ok(out.as_object().unwrap().clone())
}
pub(crate) async fn storage_login(state: &AppState, account: i64) -> Result<Vec<Value>> {
    let mut tx = crate::database::begin(&state.db).await?;
    storage::initialize(&mut tx, state, account).await?;
    let mut rows = list(&mut tx, account, "equip_storage").await?;
    for value in &mut rows {
        storage::reconcile(&mut tx, account, value).await?;
    }
    tx.commit().await?;
    Ok(rows)
}
pub(crate) async fn first_lobby(state: &AppState, account: i64) -> Result<Value> {
    let mut tx = crate::database::begin(&state.db).await?;
    storage::initialize(&mut tx, state, account).await?;
    let mut presets = vec![];
    for v in list(&mut tx, account, "equip_storage").await? {
        presets.push(storage::preset(&mut tx, account, &v).await?);
    }
    let mut identified = vec![];
    let mut techno = vec![];
    for r in crate::db::query("SELECT * FROM equip_items WHERE account_id=$1")
        .bind(account)
        .fetch_all(&mut *tx)
        .await?
    {
        let eq = EquipItemInfo::from_row(&r);
        if equip_item(state, eq.item_index as i64)
            .is_some_and(|v| n(v, "EquipType") == 1)
        {
            techno.push(eq.item_index);
            let mut v = json!(eq);
            v["EquipItemSlotIndex"] = json!(eq.slot_index);
            identified.push(v);
        }
    }
    let pets = list(&mut tx, account, "pet").await?;
    let team = buffs::team_snapshot(&mut tx, state, account).await?;
    tx.commit().await?;
    Ok(
        json!({"EquipPresetInfos":presets,"EquipItemIdentifiedInfos":identified,"TechnoMagicEquipItems":techno,"PetInfos":pets,"TeamLevelBuffInfos":team}),
    )
}

#[cfg(test)]
mod equip_item_index_tests {
    use super::*;

    #[tokio::test]
    async fn equip_item_index_matches_first_row_lookup() {
        let db = crate::database::test_pool().await;
        let state = AppState::new(db, crate::tables::test_tables());
        let rows = state.tables.extensions.rows("EquipItem");
        assert!(!rows.is_empty());
        for id in rows.iter().filter_map(|r| r["ItemIndex"].as_i64()).chain([0, -1, 999_999_999]) {
            let expected = state.tables.extensions.find("EquipItem", &[("ItemIndex", id)]);
            assert_eq!(equip_item(&state, id).map(|v| v as *const Value), expected.map(|v| v as *const Value), "item {id}");
        }
        // A replaced table is re-indexed rather than served from the old one.
        let mut changed = state.clone();
        let first = rows[0]["ItemIndex"].as_i64().unwrap();
        std::sync::Arc::make_mut(&mut std::sync::Arc::make_mut(&mut changed.tables).extensions).0
            .get_mut("EquipItem").unwrap().retain(|r| r["ItemIndex"].as_i64() != Some(first));
        assert!(equip_item(&changed, first).is_none());
        assert!(equip_item(&state, first).is_some());
    }
}
