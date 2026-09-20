use axum::{
    extract::State,
    body::Bytes,
    Json,
};
use serde::{Deserialize, Serialize};
use sqlx::Row;
use crate::{
    error::{Result, ServerError},
    state::AppState,
};

/// Set equip request - equip items to a hero
/// Client sends: HeroIndex, HeroPartIndex[], EquipItemSlotIndex[]
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct SetEquipRequest {
    pub session_id: Option<String>,
    pub hero_index: Option<i32>,
    pub hero_part_index: Option<Vec<i32>>,
    pub equip_item_slot_index: Option<Vec<i32>>,
}

/// Set equip response
#[derive(Debug, Serialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct SetEquipResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub internal_error_message: Option<String>,
    /// Equipment slots that were equipped
    pub equipped_slot_index: Vec<i32>,
    /// Equipment slots that were unequipped (from previous hero)
    pub un_equipped_slot_index: Vec<i32>,
}

/// Parse set equip request from form body
fn parse_set_equip_request(body: &str) -> SetEquipRequest {
    // Try JSON first
    if body.starts_with('{') {
        if let Ok(req) = serde_json::from_str::<SetEquipRequest>(body) {
            return req;
        }
    }
    
    // WWWForm repeats each array field; preserve the item/part pairing and order.
    let params = match crate::api::system::request::Request::parse_with_arrays(
        body.as_bytes(), &["HeroPartIndex", "EquipItemSlotIndex"],
    ) {
        Ok(req) => req.0,
        Err(_) => return SetEquipRequest { session_id: None, hero_index: None,
            hero_part_index: None, equip_item_slot_index: None },
    };
    let array = |key: &str| -> Option<Vec<i32>> {
        let values: Vec<serde_json::Value> = serde_json::from_str(params.get(key)?).ok()?;
        values.iter().map(|v| v.as_i64().and_then(|n| i32::try_from(n).ok())
            .or_else(|| v.as_str()?.parse().ok())).collect()
    };
    SetEquipRequest {
        session_id: params.get("SessionKey").or(params.get("SessionId")).cloned(),
        hero_index: params.get("HeroIndex").and_then(|v| v.parse().ok()),
        hero_part_index: array("HeroPartIndex"),
        equip_item_slot_index: array("EquipItemSlotIndex"),
    }
}

/// Handle set equip request
pub async fn set_equip(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<SetEquipResponse>> {
    let body_str = String::from_utf8_lossy(&body);

    
    let req = parse_set_equip_request(&body_str);
    
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let hero_index = req.hero_index.ok_or_else(|| ServerError::InvalidRequest("Missing hero_index".to_string()))?;
    let hero_part_indices = req.hero_part_index.unwrap_or_default();
    let equip_slot_indices = req.equip_item_slot_index.unwrap_or_default();
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    tracing::info!("SetEquip: hero_index={}, parts={:?}, slots={:?}", 
        hero_index, hero_part_indices, equip_slot_indices);

    if hero_part_indices.is_empty() || hero_part_indices.len()!=equip_slot_indices.len() || hero_part_indices.len()>10
        || hero_part_indices.iter().any(|p|!(0..10).contains(p))
        || hero_part_indices.iter().collect::<std::collections::HashSet<_>>().len()!=hero_part_indices.len()
        || equip_slot_indices.iter().collect::<std::collections::HashSet<_>>().len()!=equip_slot_indices.len() {
        return Err(ServerError::InvalidRequest("Invalid equipment slots".into()));
    }
    let mut tx=state.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(session.account_id).execute(&mut *tx).await?;
    let owned:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM heroes WHERE account_id=? AND hero_index=?)").bind(session.account_id).bind(hero_index).fetch_one(&mut *tx).await?;
    if !owned {return Err(ServerError::InvalidRequest("HeroNotOwned".into()));}
    for slot in &equip_slot_indices {
        let owned:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM equip_items WHERE account_id=? AND slot_index=? AND inventory_type=0)").bind(session.account_id).bind(slot).fetch_one(&mut *tx).await?;
        if !owned {return Err(ServerError::InvalidRequest("EquipNotOwned".into()));}
        let item_index:i32=sqlx::query_scalar("SELECT item_index FROM equip_items WHERE account_id=? AND slot_index=?").bind(session.account_id).bind(slot).fetch_one(&mut *tx).await?;
        if state.tables.inventory.items.get(&item_index).is_some_and(|v|crate::api::inventory::item::n(v,"Type")==52) {
            return Err(ServerError::InvalidRequest("ImpossibleEquipItem".into()));
        }
    }
    let mut equipped_slots: Vec<i32> = Vec::new();
    let mut unequipped_slots: Vec<i32> = Vec::new();

    // Process each equipment slot
    for (i, &part_index) in hero_part_indices.iter().enumerate() {
        if i >= equip_slot_indices.len() {
            break;
        }
        let equip_slot_index = equip_slot_indices[i];
        
        // part_index is 0-9 for equipment slots 1-10
        // equip_slot_index is the SlotIndex of the equipment item from inventory
        
        // First, check if the equipment is currently on another hero and unequip it
        let current_owner = sqlx::query(
            r#"SELECT hero_index FROM heroes 
               WHERE account_id = ? AND (
                   equip_item_slot_index_1 = ? OR equip_item_slot_index_2 = ? OR
                   equip_item_slot_index_3 = ? OR equip_item_slot_index_4 = ? OR
                   equip_item_slot_index_5 = ? OR equip_item_slot_index_6 = ? OR
                   equip_item_slot_index_7 = ? OR equip_item_slot_index_8 = ? OR
                   equip_item_slot_index_9 = ? OR equip_item_slot_index_10 = ?
               )"#
        )
        .bind(session.account_id)
        .bind(equip_slot_index).bind(equip_slot_index)
        .bind(equip_slot_index).bind(equip_slot_index)
        .bind(equip_slot_index).bind(equip_slot_index)
        .bind(equip_slot_index).bind(equip_slot_index)
        .bind(equip_slot_index).bind(equip_slot_index)
        .fetch_optional(&mut *tx)
        .await?;

        if let Some(row) = current_owner {
            let other_hero_index: i32 = row.get("hero_index");
            if other_hero_index != hero_index {
                // Unequip from the other hero by setting all matching slots to 0
                for slot in 1..=10 {
                    let column = format!("equip_item_slot_index_{}", slot);
                    sqlx::query(&format!(
                        "UPDATE heroes SET {} = 0 WHERE account_id = ? AND hero_index = ? AND {} = ?",
                        column, column
                    ))
                    .bind(session.account_id)
                    .bind(other_hero_index)
                    .bind(equip_slot_index)
                    .execute(&mut *tx)
                    .await?;
                }
                unequipped_slots.push(equip_slot_index);
                tracing::info!("Unequipped slot {} from hero {}", equip_slot_index, other_hero_index);
            }
        }

        // Check if something is currently in the target slot and unequip it
        let slot_column = format!("equip_item_slot_index_{}", part_index + 1);
        let current_in_slot = sqlx::query(&format!(
            "SELECT {} as current_equip FROM heroes WHERE account_id = ? AND hero_index = ?",
            slot_column
        ))
        .bind(session.account_id)
        .bind(hero_index)
        .fetch_optional(&mut *tx)
        .await?;

        if let Some(row) = current_in_slot {
            let current_equip: i32 = row.get("current_equip");
            if current_equip != 0 && current_equip != equip_slot_index {
                unequipped_slots.push(current_equip);
                tracing::info!("Unequipped slot {} from target slot {}", current_equip, part_index);
            }
        }

        // Now equip the item to the hero
        let query = format!(
            "UPDATE heroes SET {} = ? WHERE account_id = ? AND hero_index = ?",
            slot_column
        );
        sqlx::query(&query)
            .bind(equip_slot_index)
            .bind(session.account_id)
            .bind(hero_index)
            .execute(&mut *tx)
            .await?;

        equipped_slots.push(equip_slot_index);
        tracing::info!("Equipped slot {} to hero {} part {}", equip_slot_index, hero_index, part_index);
    }

    tx.commit().await?;
    Ok(Json(SetEquipResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        internal_error_message: None,
        equipped_slot_index: equipped_slots,
        un_equipped_slot_index: unequipped_slots,
    }))
}

/// Unset equip request - unequip items from hero
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct UnsetEquipRequest {
    pub session_id: Option<String>,
    pub equip_item_slot_index: Option<Vec<i32>>,
}

/// Unset equip response
#[derive(Debug, Serialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct UnsetEquipResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub internal_error_message: Option<String>,
    /// Equipment slots that were unequipped
    pub unequipped_slot_index: Vec<i32>,
}

/// Parse unset equip request from form body
fn parse_unset_equip_request(body: &str) -> UnsetEquipRequest {
    // Try JSON first
    if body.starts_with('{') {
        if let Ok(req) = serde_json::from_str::<UnsetEquipRequest>(body) {
            return req;
        }
    }
    
    // Fall back to form parsing
    use std::collections::HashMap;
    let params: HashMap<String, String> = body
        .split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            match (parts.next(), parts.next()) {
                (Some(key), Some(value)) => {
                    let decoded = urlencoding::decode(value)
                        .map(|s| s.into_owned())
                        .unwrap_or_else(|_| value.to_string());
                    Some((key.to_string(), decoded))
                }
                _ => None,
            }
        })
        .collect();

    // Client sends SessionKey, not SessionId
    UnsetEquipRequest {
        session_id: params.get("SessionKey").or(params.get("SessionId")).cloned(),
        equip_item_slot_index: params.get("EquipItemSlotIndex").and_then(|v| {
            // Try as JSON array first, then as single value
            serde_json::from_str(v).ok()
                .or_else(|| v.parse::<i32>().ok().map(|i| vec![i]))
        }),
    }
}

/// Handle unset equip request
pub async fn unset_equip(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<UnsetEquipResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("UnsetEquip request: {}", body_str);
    
    let req = parse_unset_equip_request(&body_str);
    
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    let equip_slot_indices = req.equip_item_slot_index.unwrap_or_default();
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    tracing::info!("UnsetEquip: slots={:?}", equip_slot_indices);

    let mut unequipped_slots: Vec<i32> = Vec::new();

    // For each equipment slot to unequip
    for equip_slot_index in equip_slot_indices {
        // Find which hero has this equipment and unequip it
        for slot in 1..=10 {
            let column = format!("equip_item_slot_index_{}", slot);
            let result = sqlx::query(&format!(
                "UPDATE heroes SET {} = 0 WHERE account_id = ? AND {} = ?",
                column, column
            ))
            .bind(session.account_id)
            .bind(equip_slot_index)
            .execute(&state.db)
            .await?;

            if result.rows_affected() > 0 {
                unequipped_slots.push(equip_slot_index);
                tracing::info!("Unequipped slot {} from slot position {}", equip_slot_index, slot);
                break;
            }
        }
    }

    Ok(Json(UnsetEquipResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        internal_error_message: None,
        unequipped_slot_index: unequipped_slots,
    }))
}
