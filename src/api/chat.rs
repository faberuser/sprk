use axum::{
    extract::State,
    body::Bytes,
    Json,
};
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use crate::{
    error::{Result, ServerError},
    models::BaseResultType,
    state::AppState,
};

// ============================================================
// Chat Data Structures (matching client's NShared.NMessage)
// ============================================================

/// BaseChat - matches client's NShared.NMessage.NProtocol.BaseChat
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct ChatMessage {
    pub sender_account_id: i64,
    pub sender_avatar_index: i32,
    pub sender_team_level: i32,
    pub sender_name: String,
    pub receiver_id: i64,
    pub chat: String,
    pub send_time: String,
    pub emoticon_index: i32,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub linked_item: Option<Vec<serde_json::Value>>,
}

/// WorldChat entry
#[derive(Debug, Clone, Serialize, Deserialize, Default)]
#[serde(rename_all = "PascalCase")]
pub struct WorldChatEntry {
    pub channel: i32,
    #[serde(flatten)]
    pub message: ChatMessage,
}

// ============================================================
// Helper: Parse form body to HashMap
// ============================================================

fn parse_form_body(body: &str) -> HashMap<String, String> {
    body.split('&')
        .filter_map(|pair| {
            let mut parts = pair.splitn(2, '=');
            match (parts.next(), parts.next()) {
                (Some(key), Some(value)) => {
                    let decoded = urlencoding::decode(value)
                        .map(|s| s.into_owned())
                        .unwrap_or_else(|_| value.to_string());
                    Some((key.to_string(), decoded))
                }
                _ => None,
            }
        })
        .collect()
}

fn get_session_key(params: &HashMap<String, String>) -> Option<String> {
    params.get("SessionKey")
        .or(params.get("SessionId"))
        .cloned()
}

// ============================================================
// Get Chat Info - returns chat server connection info
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetChatInfoResponse {
    pub base_result: i32,
    pub result: i32,
    pub chat_server_address: String,
    pub chat_server_port: i32,
    pub channel_id: i32,
}

pub async fn get_chat_info(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<GetChatInfoResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // Return placeholder chat server info
    // In a real implementation, you'd have a separate WebSocket server for chat
    Ok(Json(GetChatInfoResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        chat_server_address: "localhost".to_string(),
        chat_server_port: 9001,
        channel_id: 1,
    }))
}

// ============================================================
// Send World Chat
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SendWorldChatResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn send_world_chat(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<SendWorldChatResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("World chat request: {}", body_str);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // In a real implementation, you'd broadcast this to all connected clients
    // For now, just acknowledge the message
    Ok(Json(SendWorldChatResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Send Whisper Chat
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SendWhisperResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn send_whisper(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<SendWhisperResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("Whisper chat request: {}", body_str);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // In a real implementation, you'd route this to the specific user
    Ok(Json(SendWhisperResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Send Guild Chat
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct SendGuildChatResponse {
    pub base_result: i32,
    pub result: i32,
}

pub async fn send_guild_chat(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<SendGuildChatResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    tracing::info!("Guild chat request: {}", body_str);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // In a real implementation, you'd broadcast to guild members
    Ok(Json(SendGuildChatResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
    }))
}

// ============================================================
// Get Recent Chats - returns recent chat history
// ============================================================

#[derive(Debug, Serialize)]
#[serde(rename_all = "PascalCase")]
pub struct GetRecentChatsResponse {
    pub base_result: i32,
    pub result: i32,
    pub world_chats: Vec<ChatMessage>,
    pub guild_chats: Vec<ChatMessage>,
    pub whisper_chats: Vec<ChatMessage>,
}

pub async fn get_recent_chats(
    State(state): State<AppState>,
    body: Bytes,
) -> Result<Json<GetRecentChatsResponse>> {
    let body_str = String::from_utf8_lossy(&body);
    let params = parse_form_body(&body_str);
    
    let session_key = get_session_key(&params)
        .ok_or(ServerError::SessionExpired)?;
    let _session = state.get_session(&session_key)
        .ok_or(ServerError::SessionExpired)?;

    // Return empty chat history for now
    // In a full implementation, you'd fetch recent messages from a chat store
    Ok(Json(GetRecentChatsResponse {
        base_result: BaseResultType::Success as i32,
        result: 0,
        world_chats: vec![],
        guild_chats: vec![],
        whisper_chats: vec![],
    }))
}
