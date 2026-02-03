use axum::{
    extract::{State, Query},
    Json,
};
use serde::{Deserialize, Serialize};
use uuid::Uuid;
use crate::{
    error::Result,
    state::AppState,
};

/// Guest login request body
#[derive(Debug, Deserialize)]
#[allow(dead_code)]
pub struct GuestLoginBody {
    pub product: Option<String>,
    pub id: Option<String>,
}

/// Guest login query params
#[derive(Debug, Deserialize)]
pub struct GuestLoginQuery {
    pub device: Option<String>,
}

/// Token data in response (matches GetTokenData in client)
#[derive(Debug, Serialize)]
pub struct TokenData {
    pub id: String,
    pub access_token: String,
    pub refresh_token: String,
    pub expires_in: String, // Client expects string
}

/// Guest login response wrapper (matches WebRequestTokenData in client)
#[derive(Debug, Serialize)]
pub struct GuestLoginResponse {
    pub status: String,
    pub message: String,
    pub data: TokenData,
}

/// Handle guest login
/// POST /api/auth/login/guest?device=xxx
pub async fn guest_login(
    State(state): State<AppState>,
    Query(query): Query<GuestLoginQuery>,
    Json(body): Json<GuestLoginBody>,
) -> Result<Json<GuestLoginResponse>> {
    let device_id = query.device.or(body.id).unwrap_or_else(|| "unknown".to_string());
    
    tracing::info!("Guest login attempt: device_id={}", device_id);
    
    // Generate tokens
    let access_token = Uuid::new_v4().to_string();
    let refresh_token = Uuid::new_v4().to_string();
    
    // Store the token -> device_id mapping for later verification
    state.store_auth_token(&access_token, &device_id);
    
    Ok(Json(GuestLoginResponse {
        status: "success".to_string(),
        message: "".to_string(),
        data: TokenData {
            id: device_id.clone(),
            access_token,
            refresh_token,
            expires_in: "86400".to_string(), // 24 hours
        },
    }))
}

/// Token verify request body
#[derive(Debug, Deserialize)]
#[allow(dead_code)]
pub struct VerifyTokenBody {
    pub product: Option<String>,
}

/// Session data in verify response
#[derive(Debug, Serialize)]
pub struct SessionData {
    pub id: String,
    pub session: String,
}

/// Token verify response wrapper (matches WebRequestSessionData in client)
#[derive(Debug, Serialize)]
pub struct VerifyTokenResponse {
    pub status: String,
    pub message: String,
    pub data: SessionData,
}

/// Handle token verification
/// POST /api/auth/token/verify
/// Authorization: Bearer <access_token>
pub async fn verify_token(
    State(state): State<AppState>,
    headers: axum::http::HeaderMap,
    Json(_body): Json<VerifyTokenBody>,
) -> Result<Json<VerifyTokenResponse>> {
    // Extract bearer token from Authorization header
    let auth_header = headers
        .get("Authorization")
        .and_then(|v| v.to_str().ok())
        .unwrap_or("");
    
    let access_token = auth_header
        .strip_prefix("Bearer ")
        .unwrap_or(auth_header);
    
    tracing::info!("Token verify: token={}", access_token);
    
    // Look up the device_id from the token
    let device_id = state.get_auth_token(access_token)
        .unwrap_or_else(|| "guest".to_string());
    
    // Generate a session ID for this login
    let session_id = Uuid::new_v4().to_string();
    
    // Store session info
    state.create_session_simple(&session_id, &device_id);
    
    Ok(Json(VerifyTokenResponse {
        status: "success".to_string(),
        message: "".to_string(),
        data: SessionData {
            id: device_id,
            session: session_id,
        },
    }))
}

/// Refresh token request body  
#[derive(Debug, Deserialize)]
#[allow(dead_code)]
pub struct RefreshTokenBody {
    pub product: Option<String>,
}

/// Refresh token query params
#[derive(Debug, Deserialize)]
pub struct RefreshTokenQuery {
    pub device: Option<String>,
}

/// Handle token refresh
/// POST /api/auth/refresh-token?device=xxx
pub async fn refresh_token(
    State(state): State<AppState>,
    Query(query): Query<RefreshTokenQuery>,
    _headers: axum::http::HeaderMap,
    Json(_body): Json<RefreshTokenBody>,
) -> Result<Json<GuestLoginResponse>> {
    let device_id = query.device.unwrap_or_else(|| "unknown".to_string());
    
    tracing::info!("Token refresh: device_id={}", device_id);
    
    // Generate new tokens
    let access_token = Uuid::new_v4().to_string();
    let refresh_token = Uuid::new_v4().to_string();
    
    // Store the new token
    state.store_auth_token(&access_token, &device_id);
    
    Ok(Json(GuestLoginResponse {
        status: "success".to_string(),
        message: "".to_string(),
        data: TokenData {
            id: device_id.clone(),
            access_token,
            refresh_token,
            expires_in: "86400".to_string(),
        },
    }))
}
