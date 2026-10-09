use axum::{
    extract::Form,
    Json,
};
use serde::{Deserialize, Serialize};
use crate::error::Result;

/// Fallback request for unimplemented endpoints
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct FallbackRequest {
    #[serde(flatten)]
    pub data: std::collections::HashMap<String, serde_json::Value>,
}

/// Fallback response - returns success with empty data
/// Note: BaseResult and Result must be STRINGS for the C# client to parse them
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct FallbackResponse {
    pub base_result: String,
    pub result: String,
    pub message: String,
}

/// Handle unimplemented endpoints gracefully
/// This returns a success response so the client doesn't crash
pub async fn handle_fallback(
    Form(req): Form<FallbackRequest>,
) -> Result<Json<FallbackResponse>> {
    tracing::warn!("Unimplemented endpoint called with data: {:?}", req.data);
    
    Ok(Json(FallbackResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        message: "Endpoint not implemented".to_string(),
    }))
}

