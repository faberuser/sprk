//! Player WebSockets preserve the existing newline-framed authenticated protocol.
//! The native battle worker is deliberately excluded from the public endpoint.
use crate::state::AppState;
use axum::{
    extract::{
        ws::{Message, WebSocket, WebSocketUpgrade},
        State,
    },
    response::Response,
};
use futures_util::{SinkExt, StreamExt};
use std::time::Duration;
use tokio::io::{AsyncReadExt, AsyncWriteExt};

pub(crate) fn public_url(service: &str) -> String {
    let origin = std::env::var("SPRK_WEBSOCKET_ORIGIN").unwrap_or_else(|_| {
        let secure = std::env::var("SERVER_HTTPS").is_ok_and(|v| v == "true");
        let host = std::env::var("SERVER_HOST").unwrap_or_else(|_| "127.0.0.1:8080".into());
        format!("{}://{host}", if secure { "wss" } else { "ws" })
    });
    format!("{}/ws/{service}", origin.trim_end_matches('/'))
}

pub(crate) fn validate_config() -> anyhow::Result<()> {
    for name in ["chat", "battle"] {
        let value = public_url(name);
        let uri: axum::http::Uri = value.parse()?;
        anyhow::ensure!(
            matches!(uri.scheme_str(), Some("ws" | "wss")) && uri.host().is_some(),
            "SPRK_WEBSOCKET_ORIGIN must be ws://host[:port] or wss://host[:port]"
        );
        anyhow::ensure!(
            uri.path() == format!("/ws/{name}") && uri.query().is_none(),
            "SPRK_WEBSOCKET_ORIGIN cannot contain a path or query"
        );
    }
    Ok(())
}

pub(crate) async fn chat(State(state): State<AppState>, ws: WebSocketUpgrade) -> Response {
    ws.max_message_size(64 * 1024)
        .max_frame_size(64 * 1024)
        .on_upgrade(move |socket| serve(socket, state, false))
}

pub(crate) async fn battle(State(state): State<AppState>, ws: WebSocketUpgrade) -> Response {
    ws.max_message_size(8 * 1024 * 1024)
        .max_frame_size(8 * 1024 * 1024)
        .on_upgrade(move |socket| serve(socket, state, true))
}

async fn serve(socket: WebSocket, state: AppState, battle: bool) {
    // Bounded byte-stream adapter reuses handlers and their disconnect cleanup.
    let (protocol, transport) = tokio::io::duplex(64 * 1024);
    let handler = async move {
        if battle {
            if let Err(error) =
                crate::api::battle::cooperative::connection_stream(protocol, state, false).await
            {
                tracing::debug!(%error, "Battle WebSocket ended");
            }
        } else if let Err(error) =
            crate::api::community::chat::connection_stream(protocol, state).await
        {
            tracing::debug!(%error, "Chat WebSocket ended");
        }
    };
    let bridge = async move {
        let (mut sink, mut source) = socket.split();
        let (mut reader, mut writer) = tokio::io::split(transport);
        let (pong_tx, mut pong_rx) = tokio::sync::mpsc::channel(4);
        let receive = async move {
            loop {
                let message = tokio::time::timeout(Duration::from_secs(75), source.next()).await;
                match message {
                    Ok(Some(Ok(Message::Binary(bytes)))) => {
                        tokio::time::timeout(Duration::from_secs(30), writer.write_all(&bytes))
                            .await??;
                    }
                    Ok(Some(Ok(Message::Text(text)))) => {
                        tokio::time::timeout(
                            Duration::from_secs(30),
                            writer.write_all(text.as_bytes()),
                        )
                        .await??;
                    }
                    Ok(Some(Ok(Message::Ping(bytes)))) => {
                        if pong_tx.try_send(bytes).is_err() {
                            break;
                        }
                    }
                    Ok(Some(Ok(Message::Pong(_)))) => {}
                    _ => break,
                }
            }
            Ok::<_, anyhow::Error>(())
        };
        let send = async move {
            let mut heartbeat = tokio::time::interval(Duration::from_secs(25));
            let mut buffer = vec![0; 32 * 1024];
            loop {
                let message = tokio::select! {
                    result = reader.read(&mut buffer) => {
                        let count = result?;
                        if count == 0 { break; }
                        Message::Binary(buffer[..count].to_vec())
                    }
                    _ = heartbeat.tick() => Message::Ping(Vec::new()),
                    pong = pong_rx.recv() => {
                        match pong { Some(bytes) => Message::Pong(bytes), None => break }
                    }
                };
                tokio::time::timeout(Duration::from_secs(30), sink.send(message)).await??;
            }
            Ok::<_, anyhow::Error>(())
        };
        tokio::select! { _ = receive => {}, _ = send => {} }
        // Dropping both halves wakes the protocol handler, allowing its normal
        // account/party cleanup to complete rather than aborting its task.
    };
    tokio::join!(handler, bridge);
}

#[cfg(test)]
pub(crate) mod tests {
    use super::*;
    use crate::api::{
        community::chat::decode_packet,
        community::{
            chat::encode_packet,
            tests::{login, setup},
        },
    };
    use serde_json::{json, Value};
    use tokio::net::{TcpListener, TcpStream};
    use tokio_tungstenite::{
        connect_async, tungstenite::Message as Frame, MaybeTlsStream, WebSocketStream,
    };

    pub(crate) struct Server(pub String, tokio::task::JoinHandle<()>);
    impl Drop for Server {
        fn drop(&mut self) {
            self.1.abort();
        }
    }
    pub(crate) async fn server(state: AppState) -> Server {
        let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
        let address = format!("ws://{}", listener.local_addr().unwrap());
        let router = axum::Router::new()
            .route("/ws/chat", axum::routing::get(chat))
            .route("/ws/battle", axum::routing::get(battle))
            .with_state(state);
        Server(
            address,
            tokio::spawn(async move {
                axum::serve(listener, router).await.unwrap();
            }),
        )
    }
    pub(crate) struct Client {
        pub socket: WebSocketStream<MaybeTlsStream<TcpStream>>,
        buffer: Vec<u8>,
    }
    impl Client {
        pub(crate) async fn connect(server: &Server, path: &str) -> Self {
            let (socket, _) = connect_async(format!("{}/ws/{path}", server.0))
                .await
                .unwrap();
            Self {
                socket,
                buffer: vec![],
            }
        }
        pub(crate) async fn send(&mut self, name: &str, body: Value) {
            self.socket
                .send(Frame::Binary(encode_packet(name, &body)))
                .await
                .unwrap();
        }
        pub(crate) async fn recv(&mut self, expected: &str) -> Value {
            tokio::time::timeout(Duration::from_secs(5), async {
                loop {
                    if let Some(end) = self.buffer.iter().position(|b| *b == b'\n') {
                        let frame: Vec<_> = self.buffer.drain(..=end).collect();
                        let (name, body) = decode_packet(&frame).unwrap();
                        assert_eq!(name, expected, "{body}");
                        return body;
                    }
                    match self.socket.next().await.expect("socket ended").unwrap() {
                        Frame::Binary(bytes) => self.buffer.extend(bytes),
                        Frame::Text(text) => self.buffer.extend(text.as_bytes()),
                        Frame::Ping(bytes) => self.socket.send(Frame::Pong(bytes)).await.unwrap(),
                        Frame::Pong(_) => {}
                        other => panic!("unexpected frame {other:?}"),
                    }
                }
            })
            .await
            .expect("packet timeout")
        }
        pub(crate) async fn closed(&mut self) {
            tokio::time::timeout(Duration::from_secs(5), async {
                loop {
                    match self.socket.next().await {
                        None | Some(Err(_)) | Some(Ok(Frame::Close(_))) => break,
                        Some(Ok(Frame::Ping(bytes))) => {
                            let _ = self.socket.send(Frame::Pong(bytes)).await;
                        }
                        other => panic!("unexpected frame {other:?}"),
                    }
                }
            })
            .await
            .expect("close timeout");
        }
    }

    #[tokio::test]
    async fn websocket_chat_auth_framing_delivery_and_reconnect() {
        let s = setup().await;
        let a = login(&s, "websocket-a").await;
        let b = login(&s, "websocket-b").await;
        let server = server(s.clone()).await;
        let auth = |u: &Value| json!({"RequestId":1,"AccountId":u["UserInfo"]["AccountId"],"SessionKey":u["UserInfo"]["SessionKey"],"ChannelNo":1});
        let mut bad = Client::connect(&server, "chat").await;
        let mut forged = auth(&a);
        forged["SessionKey"] = json!("invalid");
        bad.send("LoginReq", forged).await;
        assert_eq!(bad.recv("LoginRes").await["Result"], "Fail");
        bad.closed().await;
        let mut first = Client::connect(&server, "chat").await;
        // Protocol frames may span WebSocket messages or share a message.
        let mut bytes = encode_packet("LoginReq", &auth(&a));
        bytes.extend(encode_packet("PingReq", &json!({"RequestId":2})));
        first
            .socket
            .send(Frame::Binary(bytes[..13].to_vec()))
            .await
            .unwrap();
        first
            .socket
            .send(Frame::Text(
                String::from_utf8(bytes[13..].to_vec()).unwrap(),
            ))
            .await
            .unwrap();
        assert_eq!(first.recv("LoginRes").await["Result"], "Success");
        assert_eq!(first.recv("PingRes").await["RequestId"], 2);
        let mut second = Client::connect(&server, "chat").await;
        second.send("LoginReq", auth(&b)).await;
        assert_eq!(second.recv("LoginRes").await["Result"], "Success");
        first.send("MessageReq", json!({"RequestId":3,"GroupType":"Channel","Message":format!("ChannelChat {}", json!({"Chat":"over websocket","SenderName":"FORGED","AdminLevel":99}))})).await;
        assert_eq!(first.recv("MessageRes").await["Result"], "Success");
        let delivered = second.recv("MessageNot").await;
        assert_eq!(delivered["SenderId"], a["UserInfo"]["AccountId"]);
        let content: Value = serde_json::from_str(delivered["Content"].as_str().unwrap()).unwrap();
        assert_eq!(content["Chat"], "over websocket");
        assert_ne!(content["SenderName"], "FORGED");
        drop(first); // Abrupt network loss must run the normal disconnect cleanup.
        let account = a["UserInfo"]["AccountId"].as_i64().unwrap();
        tokio::time::timeout(Duration::from_secs(5), async {
            while s.chat.online(account) {
                tokio::time::sleep(Duration::from_millis(10)).await;
            }
        })
        .await
        .unwrap();
        let mut again = Client::connect(&server, "chat").await;
        again.send("LoginReq", auth(&a)).await;
        assert_eq!(again.recv("LoginRes").await["Result"], "Success");
    }

    #[tokio::test]
    async fn websocket_rejects_oversized_chat_and_public_worker_login() {
        let mut s = setup().await;
        s.battle_service_key = std::sync::Arc::new(Some("test-secret".into()));
        let server = server(s.clone()).await;
        let mut chat = Client::connect(&server, "chat").await;
        chat.socket
            .send(Frame::Binary(vec![b'x'; 64 * 1024 + 1]))
            .await
            .unwrap();
        chat.closed().await;
        let mut worker = Client::connect(&server, "battle").await;
        worker
            .send("WorkerLogin", json!({"Key":"test-secret"}))
            .await;
        worker.closed().await;
        let mut player = Client::connect(&server, "battle").await;
        player
            .send(
                "BattleLoginReq",
                json!({"AccountId":1,"SessionKey":"invalid"}),
            )
            .await;
        player.closed().await;
    }
}
