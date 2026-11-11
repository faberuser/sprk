//! Account-wide hero equipment/skill presets. Apply validates ownership before any write.
use crate::api::{
    heroes as hero,
    inventory::item::{self, n, rule},
    system::request::Request,
};
use crate::{
    error::{Result, ServerError},
    models::equip::EquipItemInfo,
    state::AppState,
};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};
use std::collections::BTreeSet;

/// Called inside the same transaction that permanently removes equipment.
pub(crate) async fn forget_equipment(db: &mut SqliteConnection, account: i64, slot: i64) -> Result<()> {
    forget_equipments(db, account, &BTreeSet::from([slot])).await
}
/// `forget_equipment` for many pieces at once: storage rows and presets are
/// read, rewritten and saved once per batch instead of once per piece.
pub(crate) async fn forget_equipments(db: &mut SqliteConnection, account: i64, slots: &BTreeSet<i64>) -> Result<()> {
    if slots.is_empty() { return Ok(()); }
    let rows = sqlx::query("SELECT idx,data FROM extension_state WHERE account_id=? AND kind='equip_storage'")
        .bind(account).fetch_all(&mut *db).await?;
    for row in rows {
        let mut value: Value = serde_json::from_str(&row.get::<String,_>("data"))
            .map_err(|e|ServerError::Internal(e.to_string()))?;
        let mut changed=false;
        for part in 1..=10 {
            let key=format!("EquipItemSlotIndex{part}");
            if slots.contains(&n(&value,&key)) { value[key]=json!(0); changed=true; }
        }
        if changed {
            sqlx::query("UPDATE extension_state SET data=? WHERE account_id=? AND kind='equip_storage' AND idx=?")
                .bind(value.to_string()).bind(account).bind(row.get::<i64,_>("idx")).execute(&mut *db).await?;
        }
    }
    let rows=sqlx::query("SELECT storage_key,data FROM hero_presets WHERE account_id=?")
        .bind(account).fetch_all(&mut *db).await?;
    for row in rows {
        let mut presets:Vec<Value>=serde_json::from_str(&row.get::<String,_>("data"))
            .map_err(|e|ServerError::Internal(e.to_string()))?;
        let mut changed=false;
        for preset in &mut presets {
            let parts:Vec<usize>=preset["EquipItemSlotIndex"].as_array().into_iter().flatten().enumerate()
                .filter(|(_,value)|value.as_i64().is_some_and(|slot|slots.contains(&slot))).map(|(part,_)|part).collect();
            for part in parts {
                preset["EquipItemSlotIndex"][part]=json!(0);
                if let Some(uid)=preset["EquipItemUid"].as_array_mut().and_then(|a|a.get_mut(part)) { *uid=json!(""); }
                if let Some(page)=preset["EquipItemRunePage"].as_array_mut().and_then(|a|a.get_mut(part)) { *page=json!(0); }
                changed=true;
            }
        }
        if changed {
            sqlx::query("UPDATE hero_presets SET data=? WHERE account_id=? AND storage_key=?")
                .bind(json!(presets).to_string()).bind(account).bind(row.get::<String,_>("storage_key")).execute(&mut *db).await?;
        }
    }
    sqlx::query("DELETE FROM equipment_pending WHERE account_id=? AND slot_index IN (SELECT value FROM json_each(?))")
        .bind(account).bind(json!(slots).to_string()).execute(&mut *db).await?;
    Ok(())
}

pub(crate) async fn list(db: &mut SqliteConnection, account: i64) -> Result<Vec<Value>> {
    let rows = sqlx::query("SELECT * FROM hero_presets WHERE account_id=? ORDER BY storage_key")
        .bind(account)
        .fetch_all(db)
        .await?;
    rows.iter().map(|r|Ok(json!({"StorageKey":r.get::<String,_>("storage_key"),"Name":r.get::<String,_>("name"),"HeroPresetInfos":serde_json::from_str::<Value>(&r.get::<String,_>("data")).map_err(|e|ServerError::Internal(e.to_string()))?}))).collect()
}
pub(crate) async fn execute(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
    req: &Request,
    action: &str,
) -> Result<Value> {
    let key = req.text("StorageKey");
    let definition = state
        .tables
        .hero_shop
        .preset_slots
        .get(key)
        .ok_or_else(|| rule("InvalidHeroStorageKey"))?;
    let mut out = item::success();
    let existing =
        sqlx::query("SELECT name,data FROM hero_presets WHERE account_id=? AND storage_key=?")
            .bind(account)
            .bind(key)
            .fetch_optional(&mut *db)
            .await?;
    if action == "buy_hero_storage_slot" {
        if existing.is_some() {
            return Err(rule("AlreadyHasHeroStorageKey"));
        }
        let cost = n(definition, "BuyGem");
        if cost <= 0 {
            return Err(rule("InvalidHeroStorageKey"));
        }
        out["CurrencyResults"] = json!([hero::currency(db, account, "Gem", -cost).await?]);
        sqlx::query("INSERT INTO hero_presets(account_id,storage_key) VALUES (?,?)")
            .bind(account)
            .bind(key)
            .execute(&mut *db)
            .await?;
    } else {
        if existing.is_none() && definition["IsPaid"] == true {
            return Err(rule("InvalidHeroStorageKey"));
        }
        match action {
            "add_hero_storage_slot" => {
                let heroes = hero::snapshot(db, account).await?;
                let mut presets = vec![];
                for (position, h) in heroes.iter().enumerate() {
                    let value = serde_json::to_value(h)
                        .map_err(|e| ServerError::Internal(e.to_string()))?;
                    let slots: Vec<i64> = (1..=10)
                        .map(|i| n(&value, &format!("EquipItemSlotIndex{i}")))
                        .collect();
                    let uids: Vec<String> = slots
                        .iter()
                        .map(|s| if *s > 0 { s.to_string() } else { "".into() })
                        .collect();
                    presets.push(json!({"HeroIndex":h.hero_index,"ApplySkillPage":n(&value,"ApplySkillPage").max(1),"ApplyRunePage":n(&value,"ApplyRunePage"),"Position":position,"EquipItemSlotIndex":slots,"EquipItemUid":uids,"EquipItemRunePage":vec![0;10]}));
                }
                let name = if req.text("StorageName").is_empty() {
                    existing
                        .as_ref()
                        .map(|r| r.get::<String, _>("name"))
                        .unwrap_or_default()
                } else {
                    req.text("StorageName").to_string()
                };
                if name.chars().count() > 80 {
                    return Err(rule("InvalidHeroStorageKey"));
                }
                sqlx::query("INSERT INTO hero_presets(account_id,storage_key,name,data) VALUES (?,?,?,?) ON CONFLICT(account_id,storage_key) DO UPDATE SET name=excluded.name,data=excluded.data").bind(account).bind(key).bind(name).bind(json!(presets).to_string()).execute(&mut *db).await?;
            }
            "change_hero_storage_slot_name" => {
                let name = req.text("Name");
                if existing.is_none() || name.chars().count() > 80 {
                    return Err(rule("InvalidHeroStorageKey"));
                }
                sqlx::query("UPDATE hero_presets SET name=? WHERE account_id=? AND storage_key=?")
                    .bind(name)
                    .bind(account)
                    .bind(key)
                    .execute(&mut *db)
                    .await?;
                out["StorageKey"] = json!(key);
                out["NewName"] = json!(name);
                return Ok(out);
            }
            "remove_hero_storage_slot" => {
                if existing.is_none() {
                    return Err(rule("InvalidHeroStorageKey"));
                }
                // Clearing a slot preserves its purchase entitlement.
                sqlx::query(
                    "UPDATE hero_presets SET data='[]' WHERE account_id=? AND storage_key=?",
                )
                .bind(account)
                .bind(key)
                .execute(&mut *db)
                .await?;
                out["StorageKey"] = json!(key);
                return Ok(out);
            }
            "set_hero_storage_slot" => {
                let row = existing.ok_or_else(|| rule("InvalidHeroStorageKey"))?;
                let mut presets: Vec<Value> = serde_json::from_str(&row.get::<String, _>("data"))
                    .map_err(|_| rule("InvalidHeroStorageKey"))?;
                if presets.is_empty() {
                    return Err(rule("InvalidHeroStorageKey"));
                }
                let mut used = BTreeSet::new();
                let mut equips = vec![];
                // Whole-roster presets reference ~500 pieces: load the stored
                // gear they name in one statement instead of two per slot.
                let named: Vec<i64> = presets.iter()
                    .flat_map(|p| p["EquipItemSlotIndex"].as_array().into_iter().flatten().filter_map(Value::as_i64))
                    .filter(|slot| *slot > 0).collect();
                let mut gear: std::collections::HashMap<i64, sqlx::sqlite::SqliteRow> =
                    sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND inventory_type=0 AND slot_index IN (SELECT value FROM json_each(?))")
                        .bind(account).bind(json!(named).to_string()).fetch_all(&mut *db).await?
                        .into_iter().map(|r| (r.get::<i64, _>("slot_index"), r)).collect();
                for p in &mut presets {
                    let h = hero::info(db, account, n(p, "HeroIndex") as i32).await?;
                    let page = n(p, "ApplySkillPage");
                    if page > 1
                        && h[format!("TranscendSkillPage{page}")]
                            .as_str()
                            .unwrap_or("")
                            .is_empty()
                    {
                        return Err(rule("InvalidHeroStorageKey"));
                    }
                    let uids = p["EquipItemUid"].as_array().cloned().unwrap_or_default();
                    let slots = p["EquipItemSlotIndex"]
                        .as_array_mut()
                        .ok_or_else(|| rule("InvalidHeroStorageKey"))?;
                    if slots.len() != 10 {
                        return Err(rule("EquipSlotDataNotFound"));
                    }
                    for (part, s) in slots.iter_mut().enumerate() {
                        let mut slot = s.as_i64().ok_or_else(|| rule("EquipSlotDataNotFound"))?;
                        if slot <= 0 {
                            continue;
                        }
                        let exists = gear.contains_key(&slot);
                        let uid_matches = uids.get(part).and_then(Value::as_str)
                            .is_none_or(|uid| uid.is_empty() || uid == slot.to_string());
                        if !exists || !uid_matches {
                            // Sold, ground, upgraded or stored gear leaves this slot as equipped.
                            slot = n(&h, &format!("EquipItemSlotIndex{}", part + 1));
                            *s = json!(slot);
                            if slot <= 0 { continue; }
                        }
                        if !used.insert(slot) {
                            return Err(rule("EquipSlotDataNotFound"));
                        }
                        if !gear.contains_key(&slot) {
                            // A fallback to the Hero's currently worn piece.
                            let r=sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND slot_index=? AND inventory_type=0").bind(account).bind(slot).fetch_optional(&mut *db).await?.ok_or_else(||rule("EquipSlotDataNotFound"))?;
                            gear.insert(slot, r);
                        }
                        equips.push(EquipItemInfo::from_row(&gear[&slot]));
                    }
                }
                let current = hero::snapshot(db, account).await?;
                let mut removed = BTreeSet::new();
                // Each Hero's final ten slots are decided in memory and written
                // with one statement, only for Heroes whose gear changes: a
                // whole-roster preset issued ~2000 single-column updates.
                // Preset Heroes take their saved gear; others lose any piece a
                // preset now uses. Every piece taken off is reported.
                let mut wanted: std::collections::HashMap<i64, Vec<i64>> = std::collections::HashMap::new();
                for p in &presets {
                    let slots = p["EquipItemSlotIndex"].as_array().unwrap().iter().map(|slot| slot.as_i64().unwrap()).collect();
                    wanted.insert(n(p, "HeroIndex"), slots);
                }
                for h in &current {
                    let v = serde_json::to_value(h)
                        .map_err(|e| ServerError::Internal(e.to_string()))?;
                    let before: Vec<i64> = (1..=10).map(|i| n(&v, &format!("EquipItemSlotIndex{i}"))).collect();
                    let saved = wanted.get(&(h.hero_index as i64));
                    let mut after = before.clone();
                    for (part, slot) in before.iter().enumerate() {
                        if saved.is_some() || used.contains(slot) {
                            if *slot > 0 {
                                removed.insert(*slot);
                            }
                            after[part] = 0;
                        }
                    }
                    if let Some(slots) = saved {
                        after = slots.clone();
                    }
                    if after != before {
                        let mut update = sqlx::query("UPDATE heroes SET equip_item_slot_index_1=?,equip_item_slot_index_2=?,equip_item_slot_index_3=?,equip_item_slot_index_4=?,equip_item_slot_index_5=?,equip_item_slot_index_6=?,equip_item_slot_index_7=?,equip_item_slot_index_8=?,equip_item_slot_index_9=?,equip_item_slot_index_10=? WHERE account_id=? AND hero_index=?");
                        for slot in &after { update = update.bind(*slot); }
                        update.bind(account).bind(h.hero_index).execute(&mut *db).await?;
                    }
                }
                for p in &presets {
                    let id = n(p, "HeroIndex") as i32;
                    let mut d = hero::details(db, account, id).await?;
                    if d["ApplySkillPage"] != p["ApplySkillPage"] || d["ApplyRunePage"] != p["ApplyRunePage"] {
                        d["ApplySkillPage"] = p["ApplySkillPage"].clone();
                        d["ApplyRunePage"] = p["ApplyRunePage"].clone();
                        hero::save_details(db, account, id, &d).await?;
                    }
                }
                for p in &mut presets {
                    p["EquipItemUid"] = json!(p["EquipItemSlotIndex"].as_array().unwrap().iter()
                        .map(|slot| slot.as_i64().filter(|slot| *slot > 0).map(|slot| slot.to_string()).unwrap_or_default())
                        .collect::<Vec<_>>());
                }
                sqlx::query("UPDATE hero_presets SET data=? WHERE account_id=? AND storage_key=?")
                    .bind(json!(presets).to_string()).bind(account).bind(key).execute(&mut *db).await?;
                out["Heroes"] = json!(hero::snapshot(db, account).await?);
                out["EquipItems"] = json!(equips);
                out["UnEquippedSlotIndices"] = json!(removed);
                out["EquipStorageSlotIndex"] = json!([]);
                return Ok(out);
            }
            _ => return Err(rule("Fail")),
        }
    }
    out["HeroPresetStorage"] = list(db, account)
        .await?
        .into_iter()
        .find(|p| p["StorageKey"] == key)
        .ok_or_else(|| rule("InvalidHeroStorageKey"))?;
    Ok(out)
}
