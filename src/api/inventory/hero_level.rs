//! Hero-specific unique equipment grows with its hero, including spare copies.
//! Shared unique equipment grows with its equipped hero, rather than forge EXP.
//! Also deliver automatic Team Level buff unlocks through the native response.
//! The client consumes ReservedUpdatedEquipItemInfos and ReserveTeamLevelBuffInfos.
use super::item::n;
use crate::{error::Result, state::AppState};
use axum::{
    body::{to_bytes, Body},
    extract::{Request, State},
    middleware::Next,
    response::Response,
};
use serde_json::{json, Value};
use sqlx::{Row, SqliteConnection};
use std::collections::HashMap;

pub(crate) async fn synchronize(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<Vec<Value>> {
    let heroes = sqlx::query("SELECT * FROM heroes WHERE account_id=?")
        .bind(account).fetch_all(&mut *db).await?;
    let mut hero_levels = HashMap::<i64, i32>::new();
    let mut equipped_levels = HashMap::<i32, i32>::new();
    for hero in heroes {
        let level: i32 = hero.get("level");
        hero_levels.entry(hero.get("hero_index"))
            .and_modify(|old| *old = (*old).max(level)).or_insert(level);
        for part in 1..=10 {
            let slot: i32 = hero.get(format!("equip_item_slot_index_{part}").as_str());
            if slot > 0 {
                equipped_levels.entry(slot)
                    .and_modify(|old| *old = (*old).max(level)).or_insert(level);
            }
        }
    }
    let rows = sqlx::query(
        "SELECT slot_index,item_index,level FROM equip_items WHERE account_id=? ORDER BY slot_index"
    ).bind(account).fetch_all(&mut *db).await?;
    let equipment: HashMap<_, _> = state.tables.extensions.rows("EquipItem").iter()
        .map(|data| (n(data, "ItemIndex"), data)).collect();
    let mut updates = Vec::new();
    for row in rows {
        let item: i32 = row.get("item_index");
        let Some(data) = equipment.get(&(item as i64))
        else {
            continue;
        };
        let Some(detail) = state.tables.extensions.find(
            "EquipItemDetail",
            &[("DetailIndex", n(data, "DetailIndex"))],
        ) else {
            continue;
        };
        // Table-driven: includes hero/class unique weapons and both treasure types,
        // while ordinary equipment keeps its separately earned enhancement level.
        if detail["MaxLevelByHero"] != true {
            continue;
        }
        let slot: i32 = row.get("slot_index");
        // UWs and UTs bound to exactly one hero must agree across all copies,
        // including inventory/storage copies that have never been equipped.
        // Class weapons and other shared gear have no single owning hero.
        let level = match data["CreatureIndex"].as_array() {
            Some(owners) if owners.len() == 1 => owners[0].as_i64()
                .and_then(|owner| hero_levels.get(&owner)).copied(),
            _ => equipped_levels.get(&slot).copied(),
        };
        let Some(level) = level else { continue; };
        if level < 1 || level == row.get::<i32, _>("level") {
            continue;
        }
        sqlx::query("UPDATE equip_items SET level=? WHERE account_id=? AND slot_index=?")
            .bind(level)
            .bind(account)
            .bind(slot)
            .execute(&mut *db)
            .await?;
        updates.push(json!({"SlotIndex":slot,"ItemIndex":item,"Level":level}));
    }
    // Unequipped shared gear retains its last level. Transferring it recalculates
    // that level, including when its new recipient has a lower hero level.
    Ok(updates)
}

fn update_snapshots(value: &mut Value, updates: &[Value]) {
    match value {
        Value::Array(items) => {
            for item in items {
                update_snapshots(item, updates);
            }
        }
        Value::Object(object) => {
            if object.contains_key("Level") {
                if let Some(update) = updates.iter().find(|u| {
                    object.get("SlotIndex") == u.get("SlotIndex")
                        && object.get("ItemIndex") == u.get("ItemIndex")
                }) {
                    object.insert("Level".into(), update["Level"].clone());
                }
            }
            for child in object.values_mut() {
                update_snapshots(child, updates);
            }
        }
        _ => {}
    }
}

/// Cover equipment, presets, potions, battle EXP and recruitment items through
/// one native response path, so new hero-level sources cannot leave stale gear.
/// Team EXP from any successful mutation also unlocks its newly eligible buffs.
pub(crate) async fn notify(
    State(state): State<AppState>,
    request: Request,
    next: Next,
) -> Response {
    if request.method() != axum::http::Method::POST
        || crate::api::system::performance::observation(request.uri().path())
        || crate::api::system::performance::cosmetic(request.uri().path()) {
        return next.run(request).await;
    }
    let (parts, body) = request.into_parts();
    let Ok(bytes) = to_bytes(body, 2 * 1024 * 1024).await else {
        return Response::builder().status(413).body(Body::empty()).unwrap();
    };
    let account = crate::api::system::request::Request::parse(&bytes)
        .ok()
        .and_then(|r| r.account(&state).ok());
    let response = next
        .run(Request::from_parts(parts, Body::from(bytes)))
        .await;
    let Some(account) = account else {
        return response;
    };
    if !response.status().is_success() {
        return response;
    }
    let (mut parts, body) = response.into_parts();
    let Ok(bytes) = to_bytes(body, 16 * 1024 * 1024).await else {
        return Response::builder().status(500).body(Body::empty()).unwrap();
    };
    let Ok(mut value) = serde_json::from_slice::<Value>(&bytes) else {
        return Response::from_parts(parts, Body::from(bytes));
    };
    if value["BaseResult"] != "Success" || value["Result"] != "Success" {
        return Response::from_parts(parts, Body::from(bytes));
    }
    let result = async {
        let mut tx = state.db.begin().await?;
        let updates = synchronize(&mut tx, &state, account).await?;
        let team_buffs = crate::api::extensions::buffs::unlock_team_buffs(&mut tx, &state, account).await?;
        tx.commit().await?;
        Ok::<_, crate::error::ServerError>((updates, team_buffs))
    }
    .await;
    match result {
        Ok((updates, team_buffs)) if !updates.is_empty() || !team_buffs.is_empty() => {
            // Full equipment snapshots (e.g. applying presets) must agree with
            // reserved updates regardless of the client's callback ordering.
            update_snapshots(&mut value, &updates);
            let reserved = value
                .as_object_mut()
                .unwrap()
                .entry("ReservedUpdatedEquipItemInfos")
                .or_insert_with(|| json!([]));
            if !reserved.is_array() {
                *reserved = json!([]);
            }
            let array = reserved.as_array_mut().unwrap();
            array.retain(|old| !updates.iter().any(|u| u["SlotIndex"] == old["SlotIndex"]));
            array.extend(updates);
            if !team_buffs.is_empty() {
                let reserved = value.as_object_mut().unwrap().entry("ReserveTeamLevelBuffInfos")
                    .or_insert_with(|| json!([]));
                if !reserved.is_array() { *reserved = json!([]); }
                let array = reserved.as_array_mut().unwrap();
                // Keep an endpoint's newer enhancement value if supplied.
                for info in team_buffs {
                    if !array.iter().any(|old| old["OpenBuffLevel"] == info["OpenBuffLevel"]) { array.push(info); }
                }
            }
            parts.headers.remove(axum::http::header::CONTENT_LENGTH);
            Response::from_parts(parts, Body::from(serde_json::to_vec(&value).unwrap()))
        }
        Err(error) => {
            tracing::warn!(%error,"Could not synchronize hero-level equipment and Team Level buffs");
            Response::from_parts(parts, Body::from(bytes))
        }
        _ => Response::from_parts(parts, Body::from(bytes)),
    }
}
