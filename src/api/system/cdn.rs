use axum::{
    response::IntoResponse,
    http::StatusCode,
};

/// CDN endpoint to serve LastBuildVersion.txt
/// The patch downloader uses this to check if updates are needed
/// 
/// The client parses this as:
/// 1. Split by '.' and take first part as int
/// 2. If result is 0, try to parse as JSON dict mapping version->build
/// 3. If JSON fails, error out
/// 
/// We return "1" which will be parsed as version 1, meaning patches exist
/// but since our patch.json returns empty files, nothing will be downloaded.
pub async fn get_last_build_version() -> impl IntoResponse {
    tracing::info!("CDN: LastBuildVersion.txt requested - returning version 1");
    "1"
}

/// CDN endpoint for patch.json files
/// If client requests patch info, return empty/minimal patch data
pub async fn get_patch_json() -> impl IntoResponse {
    tracing::info!("CDN: patch.json requested - returning empty patch");
    // Return minimal patch info with empty Archives array
    // This tells the client there are no files to download
    (
        StatusCode::OK,
        [(axum::http::header::CONTENT_TYPE, "application/json")],
        r#"{"Archives":[]}"#
    )
}

