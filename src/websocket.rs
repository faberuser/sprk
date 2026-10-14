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
    ws.max_message_size(crate::api::community::chat::MAX_FRAME)
        .max_frame_size(crate::api::community::chat::MAX_FRAME)
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
    async fn websocket_party_chat_echoes_to_members_and_rejects_cross_party_targets() {
        let s=setup().await;
        let a=login(&s,"party-chat-host").await;let b=login(&s,"party-chat-guest").await;let c=login(&s,"party-chat-outsider").await;
        let aid=a["UserInfo"]["AccountId"].as_i64().unwrap();let bid=b["UserInfo"]["AccountId"].as_i64().unwrap();let cid=c["UserInfo"]["AccountId"].as_i64().unwrap();
        let server=server(s.clone()).await;
        let mut sa=Client::connect(&server,"chat").await;let mut sb=Client::connect(&server,"chat").await;let mut sc=Client::connect(&server,"chat").await;
        for (client,user,channel) in [(&mut sa,&a,1),(&mut sb,&b,2),(&mut sc,&c,1)] {
            client.send("LoginReq",json!({"RequestId":1,"AccountId":user["UserInfo"]["AccountId"],"SessionKey":user["UserInfo"]["SessionKey"],"ChannelNo":channel})).await;
            assert_eq!(client.recv("LoginRes").await["Result"],"Success");
            client.send("PingReq",json!({"RequestId":2})).await;assert_eq!(client.recv("PingRes").await["Result"],"Success");
        }
        for (room,master,family,members) in [(1,aid,"party_dungeon",vec![aid,bid]),(2,cid,"raid",vec![cid])] {
            sqlx::query("INSERT INTO battle_rooms(id,family,master,data,updated) VALUES(?,?,?,'{}',0)").bind(room).bind(family).bind(master).execute(&s.db).await.unwrap();
            for member in members {sqlx::query("INSERT INTO battle_room_members(room,account,joined,updated) VALUES(?,?,0,0)").bind(room).bind(member).execute(&s.db).await.unwrap();}
        }
        sa.send("MessageReq",json!({"RequestId":3,"GroupType":"None","ReceiverIds":[cid],"Message":format!("PartyChat {}",json!({"Chat":"hello party","SenderName":"FORGED","AdminLevel":99,"SenderAvatarIndex":0}))})).await;
        assert_eq!(sa.recv("MessageRes").await["Result"],"Success");
        let own=sa.recv("MessageNot").await;assert_eq!(sb.recv("MessageNot").await,own);
        assert_eq!(own["Type"],"PartyChat");assert_eq!(own["GroupType"],"None");assert_eq!(own["SenderId"],aid);
        assert_eq!(own["ReceiverIds"],json!([aid,bid]),"Client receiver IDs cannot leak party chat");
        let content:Value=serde_json::from_str(own["Content"].as_str().unwrap()).unwrap();
        assert_eq!(content["Chat"],"hello party");assert_eq!(content["SenderName"],a["UserInfo"]["Nick"]);assert_eq!(content["AdminLevel"],0);assert_ne!(content["SenderAvatarIndex"],0);
        sc.send("PingReq",json!({"RequestId":4})).await;assert_eq!(sc.recv("PingRes").await["Result"],"Success","An outsider on the same channel must not receive this chat");
        sb.send("MessageReq",json!({"RequestId":5,"Message":"PartyChat {\"Chat\":\"guest reply\"}","ReceiverIds":[]})).await;
        assert_eq!(sb.recv("MessageRes").await["Result"],"Success");let reply=sb.recv("MessageNot").await;assert_eq!(sa.recv("MessageNot").await,reply);assert_eq!(reply["SenderId"],bid);
        sc.send("MessageReq",json!({"RequestId":6,"GroupType":"World","ReceiverIds":[aid,bid],"Message":"PartyChat {\"Chat\":\"other party\"}"})).await;
        assert_eq!(sc.recv("MessageRes").await["Result"],"Success");assert_eq!(sc.recv("MessageNot").await["ReceiverIds"],json!([cid]));
        for client in [&mut sa,&mut sb] {client.send("PingReq",json!({"RequestId":7})).await;assert_eq!(client.recv("PingRes").await["Result"],"Success");}
        sqlx::query("DELETE FROM battle_room_members WHERE account=?").bind(bid).execute(&s.db).await.unwrap();
        sb.send("MessageReq",json!({"RequestId":8,"ReceiverIds":[aid],"Message":"PartyChat {\"Chat\":\"after leaving\"}"})).await;
        assert_eq!(sb.recv("MessageRes").await["Result"],"Fail","Former members cannot send to the old party");
        for text in ["x".repeat(501),String::new(),"bad\nmessage".into()] {
            sa.send("MessageReq",json!({"RequestId":9,"Message":format!("PartyChat {}",json!({"Chat":text}))})).await;
            assert_eq!(sa.recv("MessageRes").await["Result"],"Fail");
        }
        sa.send("MessageReq",json!({"RequestId":10,"Message":format!("PartyChat {}",json!({"Chat":"x".repeat(500)}))})).await;
        assert_eq!(sa.recv("MessageRes").await["Result"],"Success");assert_eq!(sa.recv("MessageNot").await["ReceiverIds"],json!([aid]));
        sb.send("PingReq",json!({"RequestId":11})).await;assert_eq!(sb.recv("PingRes").await["Result"],"Success","Former members do not receive later messages");
        let history=crate::api::community::chat::get_recent_chats(axum::extract::State(s.clone()),axum::body::Bytes::from(format!("SessionKey={}&ChannelId=1",c["UserInfo"]["SessionKey"].as_str().unwrap()))).await.unwrap().0;
        assert_eq!(history["WorldChats"],json!([]));assert_eq!(history["WhisperChats"],json!([]));assert_eq!(history["GuildChats"],json!([]));
    }

    #[tokio::test]
    async fn websocket_story_join_preserves_three_preselected_host_heroes() {
        party_join_with_large_roster(false).await;
    }

    #[tokio::test]
    async fn websocket_dragon_join_preserves_three_preselected_host_heroes() {
        party_join_with_large_roster(true).await;
    }

    async fn party_join_with_large_roster(dragon: bool) {
        let s=setup().await;
        let host=login(&s,"roster-host").await;
        let guest=login(&s,"roster-guest").await;
        let host_id=host["UserInfo"]["AccountId"].as_i64().unwrap();
        let guest_id=guest["UserInfo"]["AccountId"].as_i64().unwrap();
        for hero in 1..=3 {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                .bind(host_id).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
        sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(host_id).execute(&s.db).await.unwrap();
        let server=server(s.clone()).await;
        let mut host_socket=Client::connect(&server,"chat").await;
        let mut guest_socket=Client::connect(&server,"chat").await;
        for (client,user) in [(&mut host_socket,&host),(&mut guest_socket,&guest)] {
            client.send("LoginReq",json!({"RequestId":1,"AccountId":user["UserInfo"]["AccountId"],"SessionKey":user["UserInfo"]["SessionKey"]})).await;
            assert_eq!(client.recv("LoginRes").await["Result"],"Success");
            // LoginRes precedes reconnect replay. Finish that replay before
            // creating a room so it cannot inject an unrelated join notice.
            client.send("PingReq",json!({"RequestId":0})).await;
            assert_eq!(client.recv("PingRes").await["Result"],"Success");
        }
        let (family,create,fields)=if dragon {
            ("raid","raid/create_raid_room","RaidIndex=1&RaidLevel=7&Opened=1")
        } else {
            ("party_dungeon","party_dungeon/create_party_dungeon_room","DungeonType=1&ChapterIndex=9&DungeonIndex=8&DungeonDifficulty=2&Opened=1")
        };
        let created=crate::api::battle::execute_request(&s,create,axum::body::Bytes::from(format!("SessionKey={}&{fields}",host["UserInfo"]["SessionKey"].as_str().unwrap()))).await.unwrap();
        assert_eq!(created["Result"],"Success");
        let room=created["RoomNo"].as_i64().unwrap();
        guest_socket.send("MessageReq",json!({"RequestId":2,"ReceiverIds":[host_id],"Message":format!("JoinPartyReq {}",json!({"RoomNo":room,"Accepted":true,"SendKey":42}))})).await;
        assert_eq!(guest_socket.recv("MessageRes").await["Result"],"Success");
        assert_eq!(host_socket.recv("MessageNot").await["Type"],"JoinPartyReq");
        // Native CachedHeroInfo includes equipment and nested stats. Three of
        // these double-escaped, base64-framed records exceed the old chat limit.
        let mut deck=serde_json::Map::new();
        for hero in 1..=3 {
            deck.insert(hero.to_string(),json!({"HeroIndex":hero.to_string(),"Level":"999","EquipItemInfo1":{"SlotIndex":"999","NativeCachedFields":"x".repeat(24*1024)}}));
        }
        let body=json!({"RequestId":3,"ReceiverIds":[guest_id],"Message":format!("JoinPartyRes {}",json!({"Result":"Success","SendKey":42,"PartyInfo":{"PartyMembers":[{"AccountId":host_id,"DeckHeros":deck,"SubDeckHeros":{}}]}}))});
        let bytes=encode_packet("MessageReq",&body);
        assert!(bytes.len()>64*1024);
        if dragon {
            host_socket.socket.send(Frame::Binary(bytes)).await.unwrap();
        } else {
            // The shipped client relays TCP chunks as separate WS messages.
            for chunk in bytes.chunks(32768) {host_socket.socket.send(Frame::Binary(chunk.to_vec())).await.unwrap();}
        }
        assert_eq!(host_socket.recv("MessageRes").await["Result"],"Success");
        let notice=guest_socket.recv("MessageNot").await;
        assert_eq!(notice["Type"],"JoinPartyRes");
        let joined:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        let members=joined["PartyInfo"]["PartyMembers"].as_array().unwrap();
        assert_eq!(members.len(),2);
        let host=members.iter().find(|m|m["AccountId"]==host_id).unwrap();
        assert_eq!(host["DeckHeros"].as_object().unwrap().len(),3);
        for hero in 1..=3 {
            assert_eq!(host["DeckHeros"][hero.to_string()]["Level"],100,"Server must still rebuild owned stats");
            assert!(host["DeckHeros"][hero.to_string()]["EquipItemInfo1"].is_null());
        }
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_room_members WHERE room=?").bind(room).fetch_one(&s.db).await.unwrap(),2);
        let saved_family:String=sqlx::query_scalar("SELECT family FROM battle_rooms WHERE id=?").bind(room).fetch_one(&s.db).await.unwrap();
        assert_eq!(saved_family,family);
        for client in [&mut host_socket,&mut guest_socket] {
            client.send("PingReq",json!({"RequestId":4})).await;
            assert_eq!(client.recv("PingRes").await["Result"],"Success","Joining must leave both connections alive");
        }
        host_socket.send("MessageReq",json!({"RequestId":5,"GroupType":"Channel","Message":format!("ChannelChat {}",json!({"Chat":"x".repeat(501)}))})).await;
        assert_eq!(host_socket.recv("MessageRes").await["Result"],"Fail","Larger party frames must not increase the ordinary chat text limit");
        host_socket.send("PingReq",json!({"RequestId":6})).await;
        assert_eq!(host_socket.recv("PingRes").await["Result"],"Success");
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
        let user=login(&s,"bounded-party-chat").await;
        for fragmented in [false,true] {
            let mut client=Client::connect(&server,"chat").await;
            client.send("LoginReq",json!({"RequestId":1,"AccountId":user["UserInfo"]["AccountId"],"SessionKey":user["UserInfo"]["SessionKey"]})).await;
            assert_eq!(client.recv("LoginRes").await["Result"],"Success");
            let oversized=vec![b'x';crate::api::community::chat::MAX_FRAME+1];
            if fragmented {
                for bytes in oversized.chunks(32768) {
                    if client.socket.send(Frame::Binary(bytes.to_vec())).await.is_err() {break;}
                }
            } else {let _=client.socket.send(Frame::Binary(oversized)).await;}
            client.closed().await;
        }
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
