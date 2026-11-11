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

/// Record the player a request acts for, so `database::begin` can take that
/// player's account lock in each of its transactions. The single SQLite writer
/// ran a player's transactions one at a time; this keeps that per player while
/// different players run in parallel. Requests without a valid session (and
/// GETs) run without an acting account.
pub async fn acting_account(
    axum::extract::State(state): axum::extract::State<crate::state::AppState>,
    request: Request,
    next: Next,
) -> Response {
    if request.method() != axum::http::Method::POST {
        return next.run(request).await;
    }
    let (parts, body) = request.into_parts();
    // Same limit as the handlers' own body extractors.
    let Ok(bytes) = axum::body::to_bytes(body, 2 * 1024 * 1024).await else {
        return Response::builder().status(StatusCode::PAYLOAD_TOO_LARGE).body(axum::body::Body::empty()).unwrap();
    };
    let account = super::request::Request::parse(&bytes).ok()
        .and_then(|r| r.account(&state).ok())
        .filter(|a| *a > 0);
    let request = Request::from_parts(parts, axum::body::Body::from(bytes));
    match account {
        Some(account) => crate::database::ACTING_ACCOUNT.scope(account, next.run(request)).await,
        None => next.run(request).await,
    }
}
