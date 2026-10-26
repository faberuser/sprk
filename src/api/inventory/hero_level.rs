//! Unique equipment grows with its equipped hero, rather than forge EXP.
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

pub(crate) async fn synchronize(
    db: &mut SqliteConnection,
    state: &AppState,
    account: i64,
) -> Result<Vec<Value>> {
    let rows = sqlx::query(
        "SELECT e.slot_index,e.item_index,e.level,MAX(h.level) AS hero_level
         FROM equip_items e JOIN heroes h ON h.account_id=e.account_id AND e.slot_index IN
         (h.equip_item_slot_index_1,h.equip_item_slot_index_2,h.equip_item_slot_index_3,
          h.equip_item_slot_index_4,h.equip_item_slot_index_5,h.equip_item_slot_index_6,
          h.equip_item_slot_index_7,h.equip_item_slot_index_8,h.equip_item_slot_index_9,h.equip_item_slot_index_10)
         WHERE e.account_id=? GROUP BY e.slot_index HAVING e.level<>MAX(h.level)"
    ).bind(account).fetch_all(&mut *db).await?;
    let mut updates = Vec::new();
    for row in rows {
        let item: i32 = row.get("item_index");
        let Some(data) = state
            .tables
            .extensions
            .find("EquipItem", &[("ItemIndex", item as i64)])
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
        let level: i32 = row.get("hero_level");
        if level < 1 {
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
    // Unequipping retains the last attained level. Equipping to a different hero
    // recalculates it, including a lower-level recipient of a class weapon.
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
