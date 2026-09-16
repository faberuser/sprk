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
                let presets: Vec<Value> = serde_json::from_str(&row.get::<String, _>("data"))
                    .map_err(|_| rule("InvalidHeroStorageKey"))?;
                if presets.is_empty() {
                    return Err(rule("InvalidHeroStorageKey"));
                }
                let mut used = BTreeSet::new();
                let mut equips = vec![];
                for p in &presets {
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
                    let slots = p["EquipItemSlotIndex"]
                        .as_array()
                        .ok_or_else(|| rule("InvalidHeroStorageKey"))?;
                    if slots.len() != 10 {
                        return Err(rule("EquipSlotDataNotFound"));
                    }
                    for s in slots {
                        let slot = s.as_i64().ok_or_else(|| rule("EquipSlotDataNotFound"))?;
                        if slot <= 0 {
                            continue;
                        }
                        if !used.insert(slot) {
                            return Err(rule("EquipSlotDataNotFound"));
                        }
                        let r=sqlx::query("SELECT * FROM equip_items WHERE account_id=? AND slot_index=? AND inventory_type=0").bind(account).bind(slot).fetch_optional(&mut *db).await?.ok_or_else(||rule("EquipSlotDataNotFound"))?;
                        equips.push(EquipItemInfo::from_row(&r));
                    }
                }
                let current = hero::snapshot(db, account).await?;
                let mut removed = BTreeSet::new();
                for h in &current {
                    let v = serde_json::to_value(h)
                        .map_err(|e| ServerError::Internal(e.to_string()))?;
                    let saved = presets
                        .iter()
                        .any(|p| n(p, "HeroIndex") == h.hero_index as i64);
                    for i in 1..=10 {
                        let slot = n(&v, &format!("EquipItemSlotIndex{i}"));
                        if saved || used.contains(&slot) {
                            if slot > 0 {
                                removed.insert(slot);
                            }
                            sqlx::query(&format!("UPDATE heroes SET equip_item_slot_index_{i}=0 WHERE account_id=? AND hero_index=?")).bind(account).bind(h.hero_index).execute(&mut *db).await?;
                        }
                    }
                }
                for p in &presets {
                    let id = n(p, "HeroIndex") as i32;
                    for (i, slot) in p["EquipItemSlotIndex"]
                        .as_array()
                        .unwrap()
                        .iter()
                        .enumerate()
                    {
                        sqlx::query(&format!("UPDATE heroes SET equip_item_slot_index_{}=? WHERE account_id=? AND hero_index=?",i+1)).bind(slot.as_i64().unwrap()).bind(account).bind(id).execute(&mut *db).await?;
                    }
                    let mut d = hero::details(db, account, id).await?;
                    d["ApplySkillPage"] = p["ApplySkillPage"].clone();
                    d["ApplyRunePage"] = p["ApplyRunePage"].clone();
                    hero::save_details(db, account, id, &d).await?;
                }
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
