use thiserror::Error;
use axum::{
    http::StatusCode,
    response::{IntoResponse, Response},
    Json,
};
use serde_json::json;

#[derive(Error, Debug)]
#[allow(dead_code)]
pub enum ServerError {
    #[error("Database error: {0}")]
    Database(#[from] sqlx::Error),
    
    #[error("Authentication error: {0}")]
    Authentication(String),
    
    #[error("Session expired")]
    SessionExpired,
    
    #[error("Invalid request: {0}")]
    InvalidRequest(String),
    
    #[error("Not found: {0}")]
    NotFound(String),
    
    #[error("Encryption error: {0}")]
    Encryption(String),
    
    #[error("Internal error: {0}")]
    Internal(String),
}

impl IntoResponse for ServerError {
    fn into_response(self) -> Response {
        // BaseResult is sent by name: the game parses enums by name only (a number
        // becomes Fail).
        let (status, base_result, message) = match self {
            ServerError::Database(e) => {
                tracing::error!(error = %e, "Database request failed");
                (
                StatusCode::INTERNAL_SERVER_ERROR,
                "InternalServerError",
                format!("Database error: {}", e),
                )
            },
            ServerError::Authentication(msg) => (
                StatusCode::UNAUTHORIZED,
                "AccountError",
                msg,
            ),
            // 200, not 401: the game only reads BaseResult from a successful HTTP
            // response. SessionError then shows "session expired" and returns to
            // the login screen (LoginManager.OnSessionExpired); a 401 became a
            // retried HttpError and a generic popup that left the player in game.
            ServerError::SessionExpired => (
                StatusCode::OK,
                "SessionError",
                "Session expired".to_string(),
            ),
            ServerError::InvalidRequest(msg) => (
                StatusCode::BAD_REQUEST,
                "Fail",
                msg,
            ),
            ServerError::NotFound(msg) => (
                StatusCode::NOT_FOUND,
                "Fail",
                msg,
            ),
            ServerError::Encryption(msg) => (
                StatusCode::BAD_REQUEST,
                "Fail",
                format!("Encryption error: {}", msg),
            ),
            ServerError::Internal(msg) => (
                StatusCode::INTERNAL_SERVER_ERROR,
                "InternalServerError",
                msg,
            ),
        };

        let body = Json(json!({
            "BaseResult": base_result,
            "InternalErrorMessage": message
        }));

        (status, body).into_response()
    }
}

pub type Result<T> = std::result::Result<T, ServerError>;

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn expired_sessions_reach_the_game_as_a_session_error() {
        let response = ServerError::SessionExpired.into_response();
        assert_eq!(response.status(), StatusCode::OK);
        let body = axum::body::to_bytes(response.into_body(), 1024).await.unwrap();
        let body: serde_json::Value = serde_json::from_slice(&body).unwrap();
        assert_eq!(body["BaseResult"], "SessionError");
    }
}
