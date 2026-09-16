use axum::{
    extract::Request,
    middleware::Next,
    response::Response,
    http::StatusCode,
};

/// Middleware to decrypt incoming requests if they're encrypted
/// Currently passes through - encryption is handled per-endpoint
pub async fn decrypt_request(
    request: Request,
    next: Next,
) -> Result<Response, StatusCode> {
    // For now, pass through - encryption will be handled per-endpoint
    // as we need the session key from the request itself
    Ok(next.run(request).await)
}
