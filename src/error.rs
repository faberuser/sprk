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
        let (status, base_result, message) = match self {
            ServerError::Database(e) => (
                StatusCode::INTERNAL_SERVER_ERROR,
                10, // InternalServerError
                format!("Database error: {}", e),
            ),
            ServerError::Authentication(msg) => (
                StatusCode::UNAUTHORIZED,
                5, // AccountError
                msg,
            ),
            ServerError::SessionExpired => (
                StatusCode::UNAUTHORIZED,
                3, // SessionError
                "Session expired".to_string(),
            ),
            ServerError::InvalidRequest(msg) => (
                StatusCode::BAD_REQUEST,
                0, // Fail
                msg,
            ),
            ServerError::NotFound(msg) => (
                StatusCode::NOT_FOUND,
                0, // Fail
                msg,
            ),
            ServerError::Encryption(msg) => (
                StatusCode::BAD_REQUEST,
                0, // Fail
                format!("Encryption error: {}", msg),
            ),
            ServerError::Internal(msg) => (
                StatusCode::INTERNAL_SERVER_ERROR,
                10, // InternalServerError
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
