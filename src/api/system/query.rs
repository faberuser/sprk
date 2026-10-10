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

/// Query session request
#[derive(Debug, Deserialize)]
#[serde(rename_all = "PascalCase")]
pub struct QuerySessionRequest {
    pub session_id: Option<String>,
}

/// Query session response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct QuerySessionResponse {
    pub base_result: i32,
    pub is_valid: bool,
}

/// Handle query session request
pub async fn query_session(
    State(state): State<AppState>,
    Form(req): Form<QuerySessionRequest>,
) -> Result<Json<QuerySessionResponse>> {
    let is_valid = if let Some(session_id) = &req.session_id {
        state.get_session(session_id).is_some()
    } else {
        false
    };

    Ok(Json(QuerySessionResponse {
        base_result: BaseResultType::Success as i32,
        is_valid,
    }))
}

/// App version info for platform compatibility
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct AppVersionInfo {
    pub platform: String,
    pub min_version: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub hotfix_version: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub patch_version_url: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub cdn_proxy_url: Option<String>,
}

/// Server info returned in host query response
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct AppServerInfo {
    pub name: String,
    pub host: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub opened: Option<bool>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub https: Option<bool>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub login_server: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub cookie_keyword: Option<String>,
}

/// VersionedHost - contains server list and app version info
#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct VersionedHost {
    #[serde(skip_serializing_if = "Option::is_none")]
    pub r#type: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub description: Option<String>,
    pub app_version: Vec<AppVersionInfo>,
    pub server: Vec<AppServerInfo>,
}

/// Handle initial host query from client
/// The client fetches this URL to get the list of available game servers
/// Path: /Masang_Tokyo/Live/1/host_1.0_304dbdffe094.json (or similar)
/// 
/// IMPORTANT: Response must be an ARRAY of VersionedHost objects
pub async fn get_host_info() -> Json<Vec<VersionedHost>> {
    // Get server host from environment or use default
    let host = std::env::var("SERVER_HOST").unwrap_or_else(|_| "127.0.0.1:8080".to_string());
    let use_https = std::env::var("SERVER_HTTPS").map(|v| v == "true").unwrap_or(false);
    let scheme = if use_https { "https" } else { "http" };
    let server_name = std::env::var("SERVER_NAME").unwrap_or_else(|_| "Local".to_string());
    // LoginServer must end with / as client appends paths directly
    let login_server = std::env::var("LOGIN_SERVER")
        .unwrap_or_else(|_| format!("{scheme}://{host}/"));

    Json(vec![
        VersionedHost {
            r#type: None,
            description: Some("Private Server".to_string()),
            app_version: vec![
                // Windows Standalone
                AppVersionInfo {
                    platform: "StandaloneWindows".to_string(),
                    min_version: "00.00.001".to_string(),  // Accept any version >= this
                    hotfix_version: None,
                    patch_version_url: None,
                    cdn_proxy_url: Some(format!("{scheme}://{host}/cdn/")),
                },
                // Also support Android just in case
                AppVersionInfo {
                    platform: "Android".to_string(),
                    min_version: "00.00.001".to_string(),
                    hotfix_version: None,
                    patch_version_url: None,
                    cdn_proxy_url: Some(format!("{scheme}://{host}/cdn/")),
                },
                // iOS
                AppVersionInfo {
                    platform: "iOS".to_string(),
                    min_version: "00.00.001".to_string(),
                    hotfix_version: None,
                    patch_version_url: None,
                    cdn_proxy_url: Some(format!("{scheme}://{host}/cdn/")),
                },
            ],
            server: vec![
                AppServerInfo {
                    name: server_name,
                    host,
                    opened: Some(true),
                    https: Some(use_https),
                    login_server: Some(login_server),
                    cookie_keyword: None,
                },
            ],
        },
    ])
}

