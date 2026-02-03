use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use crate::{
    error::{Result, ServerError},
    models::hero_inn::{CurrencyResultInfo3, FriendshipPointResultInfo, ItemResultInfo},
    state::AppState,
};

/// ShopItemInfo matching client's NShared.ShopItemInfo
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ShopItemInfo {
    pub shop_index: i32,
    pub item_index: i32,
    pub purchase_count: i32,
    pub max_purchase_count: i32,
}

/// Get shop list request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct GetShopListRequest {
    #[serde(alias = "SessionKey")]
    pub session_id: Option<String>,
    pub shop_index: Option<String>,
}

/// Get shop list response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetShopListResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub shop_items: Option<Vec<ShopItemInfo>>,
}

/// Handle get shop list request
pub async fn get_shop_list(
    State(state): State<AppState>,
    Form(req): Form<GetShopListRequest>,
) -> Result<Json<GetShopListResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let _session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Return empty shop list for now - would need to implement full shop system
    Ok(Json(GetShopListResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        shop_items: Some(vec![]),
    }))
}

/// Buy shop item request - matching client's NShared.BuyShopItem.Request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct BuyShopItemRequest {
    #[serde(alias = "SessionKey")]
    pub session_id: Option<String>,
    pub shop_index: Option<String>,
    pub list_no: Option<String>,
    pub shop_item_index: Option<String>,
    pub shop_item_purchase_count: Option<String>,
}

/// Buy shop item response - matching client's NShared.BuyShopItem.Response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyShopItemResponse {
    pub base_result: String,
    pub result: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub currency_results: Option<Vec<CurrencyResultInfo3>>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub friendship_point_result: Option<FriendshipPointResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub item_result: Option<ItemResultInfo>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub shop_item: Option<ShopItemInfo>,
}

/// Hero Inn shop item definition (ShopIndex=10)
/// Items purchasable with Friendship Points - these are gift items for Hero Inn
#[derive(Debug, Clone)]
#[allow(dead_code)]
struct HeroInnShopItem {
    index: i32,          // Shop item index (1-6)
    item_index: i32,     // ItemTable Index (41001-41006)
    item_code: i32,      // ItemTable Code (kept for reference)
    count: i32,
    friendship_point_cost: i64,
}

/// Get Hero Inn shop items with their prices
/// Based on ItemTable entries with Type=11 (gift items) and BuyFriendshipPoint > 0
fn get_hero_inn_shop_items() -> Vec<HeroInnShopItem> {
    // Hero Inn shop items (ShopIndex=10)
    // These are gift items used for Hero Inn interactions
    // Data from ItemTable: Index, Code, BuyFriendshipPoint
    vec![
        // Index 41001: Aromatic Toasted Nuts (Code 1436) - 30 FP
        HeroInnShopItem { index: 1, item_index: 41001, item_code: 1436, count: 1, friendship_point_cost: 30 },
        // Index 41002: Silver Catfish Filet (Code 1434) - 60 FP
        HeroInnShopItem { index: 2, item_index: 41002, item_code: 1434, count: 1, friendship_point_cost: 60 },
        // Index 41003: Special Meat Stew (Code 1433) - 90 FP
        HeroInnShopItem { index: 3, item_index: 41003, item_code: 1433, count: 1, friendship_point_cost: 90 },
        // Index 41004: Hot Spring Egg (Code 1432) - 120 FP
        HeroInnShopItem { index: 4, item_index: 41004, item_code: 1432, count: 1, friendship_point_cost: 120 },
        // Index 41005: Silver Hot Spring Egg (Code 1431) - 150 FP
        HeroInnShopItem { index: 5, item_index: 41005, item_code: 1431, count: 1, friendship_point_cost: 150 },
        // Index 41006: Gold Hot Spring Egg (Code 1429) - 180 FP
        HeroInnShopItem { index: 6, item_index: 41006, item_code: 1429, count: 1, friendship_point_cost: 180 },
    ]
}

/// Handle buy shop item request
pub async fn buy_shop_item(
    State(state): State<AppState>,
    Form(req): Form<BuyShopItemRequest>,
) -> Result<Json<BuyShopItemResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;
    
    let account_id = session.account_id;
    let shop_index: i32 = req.shop_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);
    let shop_item_index: i32 = req.shop_item_index.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(0);
    let purchase_count: i32 = req.shop_item_purchase_count.as_ref()
        .and_then(|s| s.parse().ok())
        .unwrap_or(1);

    // Hero Inn Shop (ShopIndex = 10)
    if shop_index == 10 {
        return handle_hero_inn_shop_purchase(
            &state, 
            account_id, 
            shop_item_index, 
            purchase_count
        ).await;
    }

    // For other shop types, return success with empty result
    Ok(Json(BuyShopItemResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        currency_results: None,
        friendship_point_result: None,
        item_result: None,
        shop_item: None,
    }))
}

/// Handle Hero Inn shop purchase (ShopIndex = 10)
async fn handle_hero_inn_shop_purchase(
    state: &AppState,
    account_id: i64,
    shop_item_index: i32,
    purchase_count: i32,
) -> Result<Json<BuyShopItemResponse>> {
    // Find the shop item
    let shop_items = get_hero_inn_shop_items();
    let shop_item = shop_items.iter()
        .find(|item| item.index == shop_item_index);
    
    let shop_item = match shop_item {
        Some(item) => item,
        None => {
            return Ok(Json(BuyShopItemResponse {
                base_result: "Success".to_string(),
                result: "ShopItemNotFound".to_string(),
                currency_results: None,
                friendship_point_result: None,
                item_result: None,
                shop_item: None,
            }));
        }
    };

    let total_cost = shop_item.friendship_point_cost * purchase_count as i64;

    // Get current friendship points
    let current_fp: i64 = sqlx::query_scalar::<_, i64>(
        "SELECT COALESCE(friendship_point, 0) FROM user_info WHERE account_id = ?"
    )
    .bind(account_id)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    // Check if player has enough friendship points
    if current_fp < total_cost {
        return Ok(Json(BuyShopItemResponse {
            base_result: "Success".to_string(),
            result: "NotEnoughCurrency".to_string(),
            currency_results: None,
            friendship_point_result: None,
            item_result: None,
            shop_item: None,
        }));
    }

    // Deduct friendship points
    let new_fp = current_fp - total_cost;
    sqlx::query("UPDATE user_info SET friendship_point = ? WHERE account_id = ?")
        .bind(new_fp)
        .bind(account_id)
        .execute(&state.db)
        .await?;

    // Add items to player's items table (used by get_user_data)
    let item_count = shop_item.count * purchase_count;
    let current_item_count: i32 = sqlx::query_scalar::<_, i32>(
        "SELECT COALESCE(count, 0) FROM items WHERE account_id = ? AND item_index = ?"
    )
    .bind(account_id)
    .bind(shop_item.item_index)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    let new_item_count = current_item_count + item_count;
    
    // Upsert items table
    sqlx::query(
        "INSERT INTO items (account_id, item_index, count) VALUES (?, ?, ?)
         ON CONFLICT(account_id, item_index) DO UPDATE SET count = ?"
    )
    .bind(account_id)
    .bind(shop_item.item_index)
    .bind(new_item_count)
    .bind(new_item_count)
    .execute(&state.db)
    .await?;

    // Get current purchase count for this shop item
    let current_purchase_count: i32 = sqlx::query_scalar::<_, i32>(
        "SELECT COALESCE(purchase_count, 0) FROM shop_purchases 
         WHERE account_id = ? AND shop_index = ? AND item_index = ?"
    )
    .bind(account_id)
    .bind(10)  // Hero Inn shop
    .bind(shop_item_index)
    .fetch_optional(&state.db)
    .await?
    .unwrap_or(0);

    let new_purchase_count = current_purchase_count + purchase_count;

    // Track purchase count
    sqlx::query(
        "INSERT INTO shop_purchases (account_id, shop_index, item_index, purchase_count)
         VALUES (?, ?, ?, ?)
         ON CONFLICT(account_id, shop_index, item_index) DO UPDATE SET purchase_count = ?"
    )
    .bind(account_id)
    .bind(10)  // Hero Inn shop
    .bind(shop_item_index)
    .bind(new_purchase_count)
    .bind(new_purchase_count)
    .execute(&state.db)
    .await?;

    // Build response
    let friendship_point_result = FriendshipPointResultInfo {
        add_value: -total_cost,
        add_daily_acc_value: 0,
        new_value: new_fp,
        new_daily_acc_value: 0,
    };

    let item_result = ItemResultInfo {
        item_index: shop_item.item_index,
        add_count: item_count,
        new_count: new_item_count,
        add_booster_count: 0,
        add_npc_booster_count: 0,
        add_bonus_assigned_item_percent: 0,
        locked: 0,
        is_first_clear_reward: false,
    };

    let shop_item_info = ShopItemInfo {
        shop_index: 10,
        item_index: shop_item_index,
        purchase_count: new_purchase_count,
        max_purchase_count: 0, // 0 means unlimited
    };

    Ok(Json(BuyShopItemResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        currency_results: None,
        friendship_point_result: Some(friendship_point_result),
        item_result: Some(item_result),
        shop_item: Some(shop_item_info),
    }))
}
