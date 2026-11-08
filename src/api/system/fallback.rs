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
    method: axum::http::Method,
    axum::extract::OriginalUri(uri): axum::extract::OriginalUri,
    Form(req): Form<FallbackRequest>,
) -> Result<Json<FallbackResponse>> {
    // Name the endpoint; log field names only. Values include live session
    // keys (and the common T1/T2/TEMP transport fields), which must not reach logs.
    let mut fields: Vec<&str> = req.data.keys().map(String::as_str)
        .filter(|k| !matches!(*k, "SessionKey" | "SessionId" | "T1" | "T2" | "TEMP")).collect();
    fields.sort_unstable();
    tracing::warn!(%method, path = %uri.path(), ?fields, "Unimplemented endpoint called");
    
    Ok(Json(FallbackResponse {
        base_result: "Success".to_string(),
        result: "Success".to_string(),
        message: "Endpoint not implemented".to_string(),
    }))
}

