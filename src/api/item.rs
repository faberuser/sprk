use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use crate::{
    error::{Result, ServerError},
    models::hero_inn::{ItemResultInfo},
    state::AppState,
};

/// Stamina result info matching client's NShared.StaminaResultInfo
#[derive(Debug, Clone, Serialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct StaminaResultInfo {
    #[serde(rename = "Type")]
    pub stamina_type: String,  // Enum string: "Chicken" for main stamina
    pub add_value: i32,
    pub new_value: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_recharge_time: Option<String>,
    pub next_recharge_remain_time: i32,
    pub full_recharge_remain_time: i32,
    pub recharge_count: i32,
    pub is_hide: bool,
}

/// Use potion item request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct UsePotionItemRequest {
    #[serde(alias = "SessionKey")]
    pub session_id: Option<String>,
    pub item_index: Option<String>,
    pub item_count: Option<String>,
    pub hero_index: Option<String>,
    pub chapter_index: Option<String>,
}

/// Use potion item response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct UsePotionItemResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stamina_result: Option<StaminaResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub item_result: Option<ItemResultInfo>,
}

/// Get stamina recovery amount for an item
/// Returns (stamina_amount, stamina_type) if the item is a stamina recovery item
/// 
/// Supports:
/// - Hero Inn gift items (41001-41006)
/// - Regular stamina potions (120001, 120004)
/// - And other stamina recovery items
fn get_stamina_item_info(item_index: i32) -> Option<(i32, &'static str)> {
    match item_index {
        // Hero Inn gift items - recover "Chicken" stamina
        41001 => Some((30, "Chicken")),   // Aromatic Toasted Nuts
        41002 => Some((60, "Chicken")),   // Silver Catfish Filet
        41003 => Some((90, "Chicken")),   // Special Meat Stew
        41004 => Some((120, "Chicken")),  // Hot Spring Egg
        41005 => Some((150, "Chicken")),  // Silver Hot Spring Egg
        41006 => Some((180, "Chicken")),  // Gold Hot Spring Egg
        
        // Regular stamina potions - recover "Chicken" stamina
        120001 => Some((150, "Chicken")), // STAMINA_POTION_S - Small Stamina Potion
        120004 => Some((0, "Chicken")),   // STAMINA_POTION_FULL - Full Stamina (special: fills to max)
        
        // Delicacy/Food items that might give stamina
        // Add more as needed...
        
        _ => None,
    }
}

/// Handle use potion item request
pub async fn use_potion_item(
    State(state): State<AppState>,
    Form(req): Form<UsePotionItemRequest>,
) -> Result<Json<UsePotionItemResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let item_index: i32 = req.item_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);
    let item_count: i32 = req.item_count.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(1);

    // Check if player has the item
    let current_count: i32 = sqlx::query_scalar::<_, i32>(
        "SELECT COALESCE(count, 0) FROM items WHERE account_id = ? AND item_index = ?"
    )
    .bind(account_id)
    .bind(item_index)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    if current_count < item_count {
        return Ok(Json(UsePotionItemResponse {
            base_result: "Success".to_string(),
            result: "NotEnoughItem".to_string(),
            stamina_result: None,
            item_result: None,
        }));
    }

    // Check if this is a stamina recovery item
    let stamina_info = get_stamina_item_info(item_index);
    
    let mut stamina_result: Option<StaminaResultInfo> = None;

    if let Some((stamina_value, stamina_type)) = stamina_info {
        // Get current stamina
        let current_stamina: i32 = sqlx::query_scalar::<_, i32>(
            "SELECT COALESCE(stamina, 0) FROM user_info WHERE account_id = ?"
        )
        .bind(account_id)
        .fetch_optional(&state.db)
        .await?
        .unwrap_or(0);

        // Calculate new stamina
        // stamina_value of 0 means "fill to max" (like STAMINA_POTION_FULL)
        let max_stamina = 999999; // High cap to allow overflow stamina
        let total_stamina = if stamina_value == 0 {
            // Full stamina potion - fills to a reasonable max (e.g., 999)
            let target_max = 999;
            std::cmp::max(0, target_max - current_stamina)
        } else {
            stamina_value * item_count
        };
        
        let new_stamina = std::cmp::min(current_stamina + total_stamina, max_stamina);

        // Update stamina
        sqlx::query("UPDATE user_info SET stamina = ? WHERE account_id = ?")
            .bind(new_stamina)
            .bind(account_id)
            .execute(&state.db)
            .await?;

        stamina_result = Some(StaminaResultInfo {
            stamina_type: stamina_type.to_string(),
            add_value: new_stamina - current_stamina,
            new_value: new_stamina,
            stamina_recharge_time: None,
            next_recharge_remain_time: 0,
            full_recharge_remain_time: 0,
            recharge_count: 0,
            is_hide: false,
        });
    }

    // Consume the item
    let new_count = current_count - item_count;
    if new_count <= 0 {
        // Remove item from inventory
        sqlx::query("DELETE FROM items WHERE account_id = ? AND item_index = ?")
            .bind(account_id)
            .bind(item_index)
            .execute(&state.db)
            .await?;
    } else {
        // Update count
        sqlx::query("UPDATE items SET count = ? WHERE account_id = ? AND item_index = ?")
            .bind(new_count)
            .bind(account_id)
            .bind(item_index)
            .execute(&state.db)
            .await?;
    }

    let item_result = ItemResultInfo {
        item_index,
        add_count: -item_count,
        new_count,
        add_booster_count: 0,
        add_npc_booster_count: 0,
        add_bonus_assigned_item_percent: 0,
        locked: 0,
        is_first_clear_reward: false,
    };

    Ok(Json(UsePotionItemResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        stamina_result,
        item_result: Some(item_result),
    }))
}
