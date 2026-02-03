use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use crate::{
    error::{Result, ServerError},
    models::BaseResultType,
    state::AppState,
};

/// Shop item info
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
    pub session_id: Option<String>,
    pub shop_type: Option<i32>,
}

/// Get shop list response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetShopListResponse {
    pub base_result: i32,
    pub shop_items: Vec<ShopItemInfo>,
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
        base_result: BaseResultType::Success as i32,
        shop_items: vec![],
    }))
}

/// Buy shop item request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
#[allow(dead_code)]
pub struct BuyShopItemRequest {
    pub session_id: Option<String>,
    pub shop_index: Option<i32>,
    pub item_index: Option<i32>,
    pub count: Option<i32>,
}

/// Buy shop item response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct BuyShopItemResponse {
    pub base_result: i32,
    pub result: i32,
}

/// Handle buy shop item request
pub async fn buy_shop_item(
    State(state): State<AppState>,
    Form(req): Form<BuyShopItemRequest>,
) -> Result<Json<BuyShopItemResponse>> {
    let session_id = req.session_id.ok_or_else(|| ServerError::SessionExpired)?;
    
    let _session = state.get_session(&session_id)
        .ok_or(ServerError::SessionExpired)?;

    // Placeholder - would implement full shop purchase logic
    Ok(Json(BuyShopItemResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}
