//! Native client TCP chat: `<packet-name> <base64 JSON>\r\n`.
use super::social_request::Request;
use crate::{
    error::{Result, ServerError},
    state::AppState,
};
use axum::{body::Bytes, extract::State, Json};
use base64::{engine::general_purpose::STANDARD, Engine};
use dashmap::DashMap;
use serde_json::{json, Value};
use sqlx::Row;
use std::sync::{
    atomic::{AtomicI32, Ordering},
    Arc,
};
use tokio::{
    io::{AsyncReadExt, AsyncWriteExt},
    net::{TcpListener, TcpStream},
    sync::mpsc,
};

const MAX_FRAME: usize = 64 * 1024;
const MAX_TEXT: usize = 500;

pub struct Peer {
    account: i64,
    channel: AtomicI32,
    sender: mpsc::Sender<Value>,
}

pub struct ChatHub {
    peers: DashMap<String, Arc<Peer>>,
    pub address: String,
    pub port: u16,
}
impl Default for ChatHub {
    fn default() -> Self {
        Self {
            peers: DashMap::new(),
            address: std::env::var("CHAT_ADDRESS").unwrap_or_else(|_| "127.0.0.1".into()),
            port: std::env::var("CHAT_PORT")
                .ok()
                .and_then(|v| v.parse().ok())
                .unwrap_or(9001),
        }
    }
}
impl ChatHub {
    pub fn online(&self, account: i64) -> bool {
        self.peers.iter().any(|p| p.account == account)
    }
    pub fn notify(&self, receiver: i64, sender: i64, protocol: &str, content: Value) {
        let message = notification(sender, "None", &[receiver], protocol, content);
        for peer in &self.peers {
            if peer.account == receiver {
                let _ = peer.sender.try_send(message.clone());
            }
        }
    }
}

fn notification(
    sender: i64,
    group: &str,
    receivers: &[i64],
    protocol: &str,
    content: Value,
) -> Value {
    json!({"SenderId": sender, "GroupType": group, "ReceiverIds": receivers, "Type": protocol, "Content": content.to_string()})
}

pub fn encode_packet(name: &str, body: &Value) -> Vec<u8> {
    format!(
        "{name} {}\r\n",
        STANDARD.encode(body.to_string().as_bytes())
    )
    .into_bytes()
}
fn decode_packet(frame: &[u8]) -> Result<(String, Value)> {
    let text = std::str::from_utf8(frame)
        .map_err(|_| ServerError::InvalidRequest("Invalid socket text".into()))?;
    let (name, encoded) = text
        .trim_end_matches(['\r', '\n'])
        .split_once(' ')
        .ok_or_else(|| ServerError::InvalidRequest("Invalid socket frame".into()))?;
    let bytes = STANDARD
        .decode(encoded)
        .map_err(|_| ServerError::InvalidRequest("Invalid socket encoding".into()))?;
    let value =
        serde_json::from_slice(&bytes).map_err(|e| ServerError::InvalidRequest(e.to_string()))?;
    Ok((name.into(), value))
}

pub async fn serve(listener: TcpListener, state: AppState) -> std::io::Result<()> {
    loop {
        let (socket, _) = listener.accept().await?;
        let state = state.clone();
        tokio::spawn(async move {
            if let Err(error) = connection(socket, state).await {
                tracing::debug!(%error, "Chat connection ended");
            }
        });
    }
}

async fn connection(mut socket: TcpStream, state: AppState) -> anyhow::Result<()> {
    socket.set_nodelay(true)?;
    let id = uuid::Uuid::new_v4().to_string();
    let (sender, mut receiver) = mpsc::channel::<Value>(128);
    let mut incoming = Vec::new();
    let mut chunk = [0_u8; 4096];
    let mut account_id = 0;
    let mut session_key = String::new();
    let result: anyhow::Result<()> = async {
        loop {
            tokio::select! {
                received = tokio::time::timeout(std::time::Duration::from_secs(90), socket.read(&mut chunk)) => {
                    let count = received??;
                    if count == 0 { break; }
                    incoming.extend_from_slice(&chunk[..count]);
                    if incoming.len() > MAX_FRAME { anyhow::bail!("Chat frame too large"); }
                    while let Some(end) = incoming.iter().position(|v| *v == b'\n') {
                        let frame: Vec<_> = incoming.drain(..=end).collect();
                        let (name, request) = decode_packet(&frame)?;
                        let request_id = request["RequestId"].as_i64().unwrap_or(0);
                        let response_name = name.strip_suffix("Req").map(|v| format!("{v}Res")).unwrap_or_else(|| "MessageRes".into());
                        let mut response = json!({"RequestId": request_id, "Result": "Success"});
                        if name == "LoginReq" && account_id == 0 {
                            let requested = request["AccountId"].as_i64().unwrap_or(0);
                            // The shipped LoginReq carries AccountId only. Token-aware clients may
                            // additionally provide SessionKey. Never accept accounts without a game login.
                            let key = request["SessionKey"].as_str().map(str::to_owned).or_else(|| state.sessions.iter()
                                .filter(|s| s.account_id == requested && s.last_activity > chrono::Utc::now() - chrono::Duration::minutes(10))
                                .max_by_key(|s| s.login_time).map(|s| s.session_key.clone()));
                            let valid = key.as_ref().and_then(|key| state.get_session(key)).is_some_and(|s| s.account_id == requested && requested > 0);
                            let allowed: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM accounts WHERE account_id = ? AND is_banned = 0)").bind(requested).fetch_one(&state.db).await?;
                            if !valid || !allowed {
                                response["Result"] = json!("Fail");
                                socket.write_all(&encode_packet("LoginRes", &response)).await?;
                                break;
                            }
                            account_id = requested;
                            session_key = key.unwrap();
                            let channel = request["ChannelNo"].as_i64().unwrap_or(1).clamp(1, 9999) as i32;
                            state.chat.peers.insert(id.clone(), Arc::new(Peer { account: account_id, channel: AtomicI32::new(channel), sender: sender.clone() }));
                            response["ChannelNo"] = json!(channel);
                            let history = history(&state, account_id, channel, 30).await?;
                            response["WhisperChats"] = history["WhisperChats"].clone();
                            response["GuildChats"] = history["GuildChats"].clone();
                            socket.write_all(&encode_packet("LoginRes", &response)).await?;
                            for item in history["WorldChats"].as_array().unwrap() { socket.write_all(&encode_packet("MessageNot", item)).await?; }
                            continue;
                        }
                        if account_id == 0 || state.get_session(&session_key).is_none() {
                            response["Result"] = json!("NotLogined");
                        } else {
                            state.touch_session(&session_key);
                            match name.as_str() {
                                "PingReq" | "ChangeFriendReq" | "ChangeGuildReq" => {},
                                "ChangeChannelReq" => {
                                    let channel = request["ChannelNo"].as_i64().unwrap_or(0);
                                    response["Changed"] = json!((1..=9999).contains(&channel));
                                    if (1..=9999).contains(&channel) { if let Some(peer) = state.chat.peers.get(&id) { peer.channel.store(channel as i32, Ordering::Relaxed); } }
                                }
                                "MessageReq" => {
                                    let channel = state.chat.peers.get(&id).map(|p| p.channel.load(Ordering::Relaxed)).unwrap_or(1);
                                    let text = request["Message"].as_str().unwrap_or("");
                                    let (protocol, body) = text.split_once(' ').unwrap_or((text, "{}"));
                                    let content: Value = serde_json::from_str(body).unwrap_or(Value::Null);
                                    let target = request["ReceiverIds"].as_array().and_then(|v| v.first()).and_then(Value::as_i64).unwrap_or(0);
                                    if matches!(protocol, "Login" | "Logout") {
                                        // Presence targets come from the persisted friend graph.
                                        presence(&state, account_id, protocol).await?;
                                    } else if send_chat(&state, account_id, channel, target, protocol, content).await.is_err() {
                                        response["Result"] = json!("Fail");
                                    }
                                }
                                "ClearMessageReq" => {
                                    sqlx::query("DELETE FROM chat_messages WHERE protocol = 'WhisperChat' AND receiver_id = ? AND sender_id = ?")
                                        .bind(account_id).bind(request["RemoveAccountId"].as_i64().unwrap_or(0)).execute(&state.db).await?;
                                }
                                _ => { response["Result"] = json!("Fail"); }
                            }
                        }
                        socket.write_all(&encode_packet(&response_name, &response)).await?;
                    }
                }
                Some(message) = receiver.recv() => { socket.write_all(&encode_packet("MessageNot", &message)).await?; }
            }
        }
        Ok(())
    }.await;
    state.chat.peers.remove(&id);
    if account_id > 0 && !state.chat.online(account_id) {
        let _ = presence(&state, account_id, "Logout").await;
    }
    result
}

async fn presence(state: &AppState, account: i64, protocol: &str) -> Result<()> {
    let ids: Vec<i64> = sqlx::query_scalar("SELECT CASE WHEN account_id = ? THEN friend_account_id ELSE account_id END FROM friends WHERE status = 'accepted' AND (account_id = ? OR friend_account_id = ?)")
        .bind(account).bind(account).bind(account).fetch_all(&state.db).await?;
    for id in ids {
        state.chat.notify(id, account, protocol, json!({}));
    }
    Ok(())
}

async fn guild(state: &AppState, account: i64) -> Result<i64> {
    Ok(
        sqlx::query_scalar("SELECT guild_id FROM guild_members WHERE account_id = ?")
            .bind(account)
            .fetch_optional(&state.db)
            .await?
            .unwrap_or(0),
    )
}

async fn send_chat(
    state: &AppState,
    account: i64,
    channel: i32,
    target: i64,
    protocol: &str,
    mut content: Value,
) -> Result<Value> {
    let group = match protocol {
        "WorldChat" => "World",
        "ChannelChat" => "Channel",
        "GuildChat" => "Guild",
        "WhisperChat" => "None",
        _ => return Err(ServerError::InvalidRequest("Unsupported chat type".into())),
    };
    let text = content["Chat"].as_str().unwrap_or("").trim();
    let emoticon = content["EmoticonIndex"].as_i64().unwrap_or(0);
    if !content.is_object()
        || text.chars().count() > MAX_TEXT
        || (text.is_empty() && emoticon <= 0)
        || text.chars().any(|c| c.is_control())
    {
        return Err(ServerError::InvalidRequest("Invalid chat message".into()));
    }
    let clean = text.to_string();
    let user = sqlx::query("SELECT a.nick, u.avatar_hero_index, u.team_level FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id = ? AND a.is_banned = 0")
        .bind(account).fetch_one(&state.db).await?;
    let guild_id = if group == "Guild" {
        guild(state, account).await?
    } else {
        0
    };
    if group == "Guild" && guild_id == 0 {
        return Err(ServerError::InvalidRequest("No guild".into()));
    }
    let target_name: Option<String> = if group == "None" {
        sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id = ? AND is_banned = 0")
            .bind(target)
            .fetch_optional(&state.db)
            .await?
    } else {
        None
    };
    if group == "None" && (target_name.is_none() || target == account) {
        return Err(ServerError::InvalidRequest("Invalid recipient".into()));
    }
    // Identity, timing and privileges are always server-owned.
    content["Chat"] = json!(clean);
    content["SenderName"] = json!(user.get::<String, _>("nick"));
    content["SenderAvatarIndex"] = json!(user.get::<i32, _>("avatar_hero_index"));
    content["SenderTeamLevel"] = json!(user.get::<i32, _>("team_level"));
    content["AdminLevel"] = json!(0);
    content["ChatIconList"] = json!([]);
    content["SendTime"] = json!(state.server_time_str());
    content["ReceiverId"] = json!(if group == "None" { target } else { 0 });
    content["ReceiverName"] = json!(target_name.unwrap_or_default());
    // Unverified item links are omitted until owned equipment links are supported.
    if content
        .get("LinkedItem")
        .is_some_and(|v| !v.is_null() && v.as_array().is_none_or(|a| !a.is_empty()))
    {
        content["LinkedItem"] = json!([]);
    }
    let mut tx = state.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login = last_login WHERE account_id = ?")
        .bind(account)
        .execute(&mut *tx)
        .await?;
    let recent: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM chat_messages WHERE sender_id = ? AND created_at > datetime('now', '-2 seconds')")
        .bind(account).fetch_one(&mut *tx).await?;
    if recent >= 3 {
        return Err(ServerError::InvalidRequest("Chat rate limit".into()));
    }
    sqlx::query("INSERT INTO chat_messages (sender_id, group_type, channel, guild_id, receiver_id, protocol, content) VALUES (?, ?, ?, ?, ?, ?, ?)")
        .bind(account).bind(group).bind(channel).bind(guild_id).bind(if group == "None" { target } else { 0 }).bind(protocol).bind(content.to_string()).execute(&mut *tx).await?;
    sqlx::query("DELETE FROM chat_messages WHERE message_id IN (SELECT message_id FROM chat_messages ORDER BY message_id DESC LIMIT -1 OFFSET 10000)").execute(&mut *tx).await?;
    tx.commit().await?;
    let targets = [target];
    let message = notification(
        account,
        group,
        if group == "None" { &targets } else { &[] },
        protocol,
        content,
    );
    let peers: Vec<_> = state.chat.peers.iter().map(|p| p.value().clone()).collect();
    for peer in peers {
        let deliver = match group {
            "World" => true,
            "Channel" => peer.channel.load(Ordering::Relaxed) == channel,
            "Guild" => guild(state, peer.account).await? == guild_id,
            _ => peer.account == target || peer.account == account,
        };
        if deliver {
            let _ = peer.sender.try_send(message.clone());
        }
    }
    Ok(message)
}

async fn history(state: &AppState, account: i64, channel: i32, limit: i64) -> Result<Value> {
    let guild_id = guild(state, account).await?;
    let rows = sqlx::query("SELECT * FROM (SELECT * FROM chat_messages WHERE group_type = 'World' OR (group_type = 'Channel' AND channel = ?) OR (group_type = 'Guild' AND guild_id = ? AND guild_id > 0) OR (protocol = 'WhisperChat' AND (sender_id = ? OR receiver_id = ?)) ORDER BY message_id DESC LIMIT ?) ORDER BY message_id")
        .bind(channel).bind(guild_id).bind(account).bind(account).bind(limit.clamp(1,100)).fetch_all(&state.db).await?;
    let mut result = json!({"BaseResult": "Success", "Result": "Success", "WorldChats": [], "GuildChats": [], "WhisperChats": []});
    for row in rows {
        let group: String = row.get("group_type");
        let key = match group.as_str() {
            "Guild" => "GuildChats",
            "None" => "WhisperChats",
            _ => "WorldChats",
        };
        let target: i64 = row.get("receiver_id");
        let content: Value = serde_json::from_str(row.get::<String, _>("content").as_str())
            .map_err(|e| ServerError::Internal(e.to_string()))?;
        let targets = [target];
        let message = notification(
            row.get("sender_id"),
            &group,
            if target > 0 { &targets } else { &[] },
            &row.get::<String, _>("protocol"),
            content,
        );
        result[key].as_array_mut().unwrap().push(message);
    }
    Ok(result)
}

pub async fn get_chat_info(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    Request::parse(&body)?.account(&state)?;
    Ok(Json(
        json!({"BaseResult":"Success", "Result":"Success", "ChatServerAddress":state.chat.address, "ChatServerPort":state.chat.port, "ChannelId":1}),
    ))
}
async fn http_send(state: AppState, body: Bytes, protocol: &str) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    let content =
        json!({"Chat": req.text("Chat"), "EmoticonIndex": req.number("EmoticonIndex",0)?});
    let message = send_chat(
        &state,
        account,
        req.number("ChannelId", 1)?.clamp(1, 9999) as i32,
        req.number("ReceiverId", 0)?,
        protocol,
        content,
    )
    .await?;
    Ok(Json(
        json!({"BaseResult":"Success", "Result":"Success", "Message":message}),
    ))
}
pub async fn send_world_chat(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    http_send(state, body, "WorldChat").await
}
pub async fn send_whisper(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    http_send(state, body, "WhisperChat").await
}
pub async fn send_guild_chat(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    http_send(state, body, "GuildChat").await
}
pub async fn get_recent_chats(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    let req = Request::parse(&body)?;
    let account = req.account(&state)?;
    Ok(Json(
        history(
            &state,
            account,
            req.number("ChannelId", 1)?.clamp(1, 9999) as i32,
            req.number("MaxCount", 50)?,
        )
        .await?,
    ))
}
