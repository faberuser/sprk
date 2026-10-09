use axum::{
    extract::{State, Form},
    Json,
};
use serde::{Deserialize, Serialize};
use crate::{
    error::Result,
    models::BaseResultType,
    state::AppState,
};

/// Ping request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct PingRequest {
    pub session_id: Option<String>,
}

/// Ping response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct PingResponse {
    pub base_result: i32,
    pub server_time: String,
    #[serde(rename = "ServerUTCTime")]
    pub server_utc_time: String,
}

/// Handle ping request
pub async fn ping(
    State(state): State<AppState>,
    Form(req): Form<PingRequest>,
) -> Result<Json<PingResponse>> {
    // Update session activity if session provided
    if let Some(session_id) = &req.session_id {
        state.touch_session(session_id);
    }

    Ok(Json(PingResponse {
        base_result: BaseResultType::Success as i32,
        server_time: state.server_time_str(),
        server_utc_time: state.server_utc_time_str(),
    }))
}

/// Ping idle request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct PingIdleRequest {
    pub session_id: Option<String>,
}

/// Ping idle response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct PingIdleResponse {
    pub base_result: i32,
}

/// Handle ping idle request (keep session alive)
pub async fn ping_idle(
    State(state): State<AppState>,
    Form(req): Form<PingIdleRequest>,
) -> Result<Json<PingIdleResponse>> {
    if let Some(session_id) = &req.session_id {
        state.touch_session(session_id);
    }

    Ok(Json(PingIdleResponse {
        base_result: BaseResultType::Success as i32,
    }))
}
