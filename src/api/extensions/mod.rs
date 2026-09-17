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
use sqlx::{Row, SqliteConnection};
use std::collections::BTreeSet;
pub(crate) mod buffs;
pub(crate) mod consumables;
mod cosmetics;
mod equipment;
mod npc;
pub(crate) mod punishment;
mod runes;
pub(crate) mod schema;
mod soul;
mod storage;
#[cfg(test)]
mod tests;
pub(crate) mod valance;

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
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let action = uri.path().rsplit('/').next().unwrap_or("");
    let mut tx = state.db.begin().await?;
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
            Ok(Json(
                json!({"BaseResult":"Success","Result":state.tables.hero_shop.result(action,&code)}),
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
    row(state, "EquipItem", &[("ItemIndex", item as i64)])
}
pub(crate) async fn equip(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<EquipItemInfo> {
    let r = sqlx::query(
        "SELECT * FROM equip_items WHERE account_id=? AND slot_index=? AND inventory_type=0",
    )
    .bind(account)
    .bind(slot)
    .fetch_optional(db)
    .await?
    .ok_or_else(|| rule("EquipItemNotFound"))?;
    Ok(EquipItemInfo::from_row(&r))
}
pub(crate) async fn save_equip(
    db: &mut SqliteConnection,
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
    sqlx::query(
        "UPDATE equip_items SET punishment_rune_option=? WHERE account_id=? AND slot_index=?",
    )
    .bind(eq.punishment_rune_option.as_ref().map(Value::to_string))
    .bind(account)
    .bind(eq.slot_index)
    .execute(db)
    .await?;
    Ok(())
}
pub(crate) async fn material(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<EquipItemInfo> {
    let eq = equip(db, account, slot).await?;
    if eq.locked != 0 {
        return Err(rule("LockedEquip"));
    }
    let clauses = (1..=10)
        .map(|i| format!("equip_item_slot_index_{i}={slot}"))
        .collect::<Vec<_>>()
        .join(" OR ");
    let worn: bool = sqlx::query_scalar(&format!(
        "SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND ({clauses}))"
    ))
    .bind(account)
    .fetch_one(&mut *db)
    .await?;
    let pending: bool = sqlx::query_scalar(
        "SELECT EXISTS(SELECT 1 FROM equipment_pending WHERE account_id=? AND slot_index=?)",
    )
    .bind(account)
    .bind(slot)
    .fetch_one(&mut *db)
    .await?;
    if worn || pending {
        return Err(rule("EquippedItem"));
    }
    let preset:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM extension_state s,json_each(s.data) j WHERE s.account_id=? AND s.kind='equip_storage' AND j.key LIKE 'EquipItemSlotIndex%' AND j.value=?)").bind(account).bind(slot).fetch_one(&mut *db).await?;
    if preset {
        return Err(rule("EquippedItem"));
    }
    if !get(&mut *db, account, "soul", slot).await?.is_null() {
        return Err(rule("InvalidMaterial"));
    }
    Ok(eq)
}
async fn remove_equip(db: &mut SqliteConnection, account: i64, slot: i32) -> Result<()> {
    sqlx::query("DELETE FROM equip_items WHERE account_id=? AND slot_index=?")
        .bind(account)
        .bind(slot)
        .execute(&mut *db)
        .await?;
    Ok(())
}
fn ids(req: &Request, key: &str) -> Result<Vec<i64>> {
    if req.text(key).is_empty() {
        return Ok(vec![]);
    }
    let list: Vec<i64> =
        serde_json::from_str(req.text(key)).map_err(|_| rule("InvalidItemCount"))?;
    if list.len() > 100
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
    use rand::Rng;
    rand::thread_rng().gen_range(0..scale) < chance.clamp(0, scale)
}

pub(crate) async fn get(db: &mut SqliteConnection, account: i64, kind: &str, id: i64) -> Result<Value> {
    let text: Option<String> = sqlx::query_scalar(
        "SELECT data FROM extension_state WHERE account_id=? AND kind=? AND idx=?",
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
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
    id: i64,
    value: &Value,
) -> Result<()> {
    sqlx::query("INSERT INTO extension_state(account_id,kind,idx,data) VALUES(?,?,?,?) ON CONFLICT(account_id,kind,idx) DO UPDATE SET data=excluded.data").bind(account).bind(kind).bind(id).bind(value.to_string()).execute(db).await?;
    Ok(())
}
pub(crate) async fn list(
    db: &mut SqliteConnection,
    account: i64,
    kind: &str,
) -> Result<Vec<Value>> {
    let rows: Vec<String> = sqlx::query_scalar(
        "SELECT data FROM extension_state WHERE account_id=? AND kind=? ORDER BY idx",
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
    db: &mut SqliteConnection,
    account: i64,
) -> Result<serde_json::Map<String, Value>> {
    let mut out = json!({"RenewOptionEquipItemSlotIndex":0,"RenewOptionResultIndex":0,"RenewOptionResultStep":0,"RenewOptionSlotIndex":0,"RenewEnchantOptionEquipItemSlotIndex":0,"RenewEnchantOptionResultIndex":0,"RenewEnchantOptionResultStep":0,"SoulStoneMileage":0,"RestoreSoulStoneIndices":"[]"});
    let pending =
        sqlx::query("SELECT slot_index,kind,data FROM equipment_pending WHERE account_id=?")
            .bind(account)
            .fetch_all(&mut *db)
            .await?;
    for r in pending {
        let slot: i64 = r.get("slot_index");
        let kind: String = r.get("kind");
        let p: Value = serde_json::from_str(r.get("data")).map_err(|_| rule("InvalidOption"))?;
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
        out["RestoreSoulStoneIndices"] = json!(restore["Choices"].to_string());
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
    let mut tx = state.db.begin().await?;
    storage::initialize(&mut tx, state, account).await?;
    let rows = list(&mut tx, account, "equip_storage").await?;
    tx.commit().await?;
    Ok(rows)
}
pub(crate) async fn first_lobby(state: &AppState, account: i64) -> Result<Value> {
    let mut tx = state.db.begin().await?;
    storage::initialize(&mut tx, state, account).await?;
    let mut presets = vec![];
    for v in list(&mut tx, account, "equip_storage").await? {
        presets.push(storage::preset(&mut tx, account, &v).await?);
    }
    let mut identified = vec![];
    let mut techno = vec![];
    for r in sqlx::query("SELECT * FROM equip_items WHERE account_id=?")
        .bind(account)
        .fetch_all(&mut *tx)
        .await?
    {
        let eq = EquipItemInfo::from_row(&r);
        if state
            .tables
            .extensions
            .find("EquipItem", &[("ItemIndex", eq.item_index as i64)])
            .is_some_and(|v| n(v, "EquipType") == 1)
        {
            techno.push(eq.item_index);
            let mut v = json!(eq);
            v["EquipItemSlotIndex"] = json!(eq.slot_index);
            identified.push(v);
        }
    }
    let pets = list(&mut tx, account, "pet").await?;
    let team = list(&mut tx, account, "team_buff").await?;
    tx.commit().await?;
    Ok(
        json!({"EquipPresetInfos":presets,"EquipItemIdentifiedInfos":identified,"TechnoMagicEquipItems":techno,"PetInfos":pets,"TeamLevelBuffInfos":team}),
    )
}
