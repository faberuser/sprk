//! Exercise native forms and TCP packets against isolated databases, never sprk.db.
use super::{chat, friend, mail, user};
use crate::{database, state::AppState, tables::GameTables};
use axum::{body::Bytes, extract::State};
use base64::{engine::general_purpose::STANDARD, Engine};
use serde_json::{json, Value};
use sqlx::Row;
use std::{path::Path, sync::OnceLock, time::Duration};
use tokio::{
    io::{AsyncBufReadExt, AsyncWriteExt, BufReader},
    net::{TcpListener, TcpStream},
};

async fn login(state: &AppState, name: &str) -> user::LoginResponse {
    user::login(State(state.clone()), Bytes::from(format!("LoginId={name}")))
        .await
        .unwrap()
        .0
}
async fn setup() -> (AppState, user::LoginResponse, user::LoginResponse) {
    static TABLES: OnceLock<GameTables> = OnceLock::new();
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let tables = TABLES
        .get_or_init(|| {
            GameTables::load(&Path::new(env!("CARGO_MANIFEST_DIR")).join("tables")).unwrap()
        })
        .clone();
    let state = AppState::new(pool, tables);
    let a = login(&state, "social-a").await;
    let b = login(&state, "social-b").await;
    sqlx::query("DELETE FROM mails")
        .execute(&state.db)
        .await
        .unwrap();
    (state, a, b)
}
fn form(user: &user::LoginResponse, fields: &str) -> Bytes {
    Bytes::from(format!(
        "SessionKey={}&{fields}",
        user.user_info.session_key
    ))
}
async fn befriend(state: &AppState, a: &user::LoginResponse, b: &user::LoginResponse) {
    let id = b.user_info.account_id;
    let sent = friend::request_friend(
        State(state.clone()),
        form(a, &format!("FriendId={id}&InviteType=Request")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(sent["Result"], "Success");
    let accepted = friend::request_friend(
        State(state.clone()),
        form(
            b,
            &format!("FriendId={}&InviteType=1", a.user_info.account_id),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(accepted["Result"], "Success");
}
async fn add_mail(
    state: &AppState,
    a: &user::LoginResponse,
    gold: i64,
    gem: i64,
    items: Option<Vec<mail::MailItemInfo>>,
) -> i64 {
    mail::send_system_mail(
        state,
        a.user_info.account_id,
        "Test",
        "Attachments",
        gold,
        gem,
        items,
    )
    .await
    .unwrap()
}

#[tokio::test]
async fn invitations_search_acceptance_and_removal_survive_login() {
    let (state, a, b) = setup().await;
    sqlx::query("UPDATE accounts SET nick='Friend With Space' WHERE account_id=?")
        .bind(b.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    let search = friend::search_friend(State(state.clone()), form(&a, "Keyword=Friend+With+Space"))
        .await
        .unwrap()
        .0;
    assert_eq!(search["Result"], "Success");
    assert_eq!(
        search["FriendInfos"][0]["AccountId"],
        b.user_info.account_id
    );
    let body = form(&a, &format!("FriendId={}", b.user_info.account_id));
    let _ = friend::request_friend(State(state.clone()), body.clone())
        .await
        .unwrap();
    let _ = friend::request_friend(State(state.clone()), body)
        .await
        .unwrap();
    let (out, inc, _, _) = friend::login_data(&state, a.user_info.account_id)
        .await
        .unwrap();
    assert_eq!(out.len(), 1);
    assert!(inc.is_empty());
    let (out, inc, _, _) = friend::login_data(&state, b.user_info.account_id)
        .await
        .unwrap();
    assert!(out.is_empty());
    assert_eq!(inc.len(), 1);
    befriend(&state, &a, &b).await;
    let reloaded = serde_json::to_value(login(&state, "social-a").await).unwrap();
    assert_eq!(
        reloaded["FriendInfos"][0]["FriendId"],
        b.user_info.account_id
    );
    assert_eq!(
        reloaded["FriendInvitorInfos"][0]["AccountId"],
        b.user_info.account_id
    );
    let removed = friend::reject_friend(
        State(state.clone()),
        form(&b, &format!("FriendId={}", a.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(removed["DailyRemoveFriends"], 1);
    let (out, inc, _, _) = friend::login_data(&state, a.user_info.account_id)
        .await
        .unwrap();
    assert!(out.is_empty() && inc.is_empty());
}

#[tokio::test]
async fn invalid_friend_actions_cannot_change_other_accounts() {
    let (state, a, b) = setup().await;
    let self_req = friend::request_friend(
        State(state.clone()),
        form(&a, &format!("FriendId={}", a.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(self_req["Result"], "RequestMyself");
    let accept = friend::request_friend(
        State(state.clone()),
        form(
            &a,
            &format!("FriendId={}&InviteType=Accept", b.user_info.account_id),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(accept["Result"], "FriendNotFound");
    befriend(&state, &a, &b).await;
    let c = login(&state, "social-c").await;
    let remove = friend::reject_friend(
        State(state.clone()),
        form(&c, &format!("FriendId={}", a.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(remove["Result"], "FriendNotFound");
    assert_eq!(
        friend::login_data(&state, a.user_info.account_id)
            .await
            .unwrap()
            .0
            .len(),
        1
    );
    assert!(friend::get_friend_list(
        State(state.clone()),
        Bytes::from_static(b"SessionKey=invalid")
    )
    .await
    .is_err());
}

#[tokio::test]
async fn friendship_points_are_reciprocal_daily_and_retry_safe() {
    let (state, a, b) = setup().await;
    befriend(&state, &a, &b).await;
    let to_b = format!(
        "FriendIds=[{},{}]",
        b.user_info.account_id, b.user_info.account_id
    );
    let early = friend::recv_friendship_point(State(state.clone()), form(&a, &to_b))
        .await
        .unwrap()
        .0;
    assert_eq!(early["FriendshipPointResult"]["AddValue"], 0);
    let (one, two) = tokio::join!(
        friend::send_friendship_point(State(state.clone()), form(&a, &to_b)),
        friend::send_friendship_point(State(state.clone()), form(&a, &to_b))
    );
    let added = one.unwrap().0["FriendshipPointResult"]["AddValue"]
        .as_i64()
        .unwrap()
        + two.unwrap().0["FriendshipPointResult"]["AddValue"]
            .as_i64()
            .unwrap();
    assert_eq!(added, friend::POINTS_PER_ACTION);
    let combined = friend::send_recv_friendship_point(
        State(state.clone()),
        form(
            &b,
            &format!(
                "SendFriendIds=[{}]&RecvFriendIds=[{}]",
                a.user_info.account_id, a.user_info.account_id
            ),
        ),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(
        combined["FriendshipPointResult"]["AddValue"],
        2 * friend::POINTS_PER_ACTION
    );
    let recv = friend::recv_friendship_point(State(state.clone()), form(&a, &to_b))
        .await
        .unwrap()
        .0;
    assert_eq!(
        recv["FriendshipPointResult"]["NewDailyAccValue"],
        2 * friend::POINTS_PER_ACTION
    );
    let _ = friend::reject_friend(
        State(state.clone()),
        form(&a, &format!("FriendId={}", b.user_info.account_id)),
    )
    .await
    .unwrap();
    befriend(&state, &a, &b).await;
    let retry = friend::send_friendship_point(State(state.clone()), form(&a, &to_b))
        .await
        .unwrap()
        .0;
    assert_eq!(retry["FriendshipPointResult"]["AddValue"], 0);
    let again = login(&state, "social-a").await;
    assert_eq!(
        again.misc_info.unwrap().daily_acc_friendship_point,
        2 * friend::POINTS_PER_ACTION
    );
    sqlx::query("UPDATE friend_points SET last_send_time='2000-01-01 00:00:00',last_recv_time='2000-01-01 00:00:00'").execute(&state.db).await.unwrap();
    sqlx::query("UPDATE friend_daily SET day='2000-01-01',points=1000")
        .execute(&state.db)
        .await
        .unwrap();
    let next = friend::send_friendship_point(State(state.clone()), form(&a, &to_b))
        .await
        .unwrap()
        .0;
    assert_eq!(
        next["FriendshipPointResult"]["NewDailyAccValue"],
        friend::POINTS_PER_ACTION
    );
}

#[tokio::test]
async fn point_cap_rolls_back_the_daily_send_ledger() {
    let (state, a, b) = setup().await;
    befriend(&state, &a, &b).await;
    sqlx::query("INSERT INTO friend_daily(account_id,day,points) VALUES (?,?,?)")
        .bind(a.user_info.account_id)
        .bind(state.server_date())
        .bind(friend::DAILY_POINT_LIMIT)
        .execute(&state.db)
        .await
        .unwrap();
    let response = friend::send_friendship_point(
        State(state.clone()),
        form(&a, &format!("FriendIds=[{}]", b.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(response["Result"], "MaxFriendshipPoint");
    let count: i64 = sqlx::query_scalar("SELECT COUNT(*) FROM friend_points")
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(count, 0);
}

#[tokio::test]
async fn mail_lists_obey_cursor_page_expiry_and_owner() {
    let (state, a, b) = setup().await;
    let first = add_mail(&state, &a, 1, 0, None).await;
    let second = add_mail(&state, &a, 2, 0, None).await;
    let expired = add_mail(&state, &a, 3, 0, None).await;
    sqlx::query("UPDATE mails SET expires_at='2000-01-01 00:00:00' WHERE mail_id=?")
        .bind(expired)
        .execute(&state.db)
        .await
        .unwrap();
    let list = mail::get_mail_list(State(state.clone()), form(&a, "MaxCount=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(list["Result"], "Success");
    assert_eq!(list["TotalCount"], 2);
    assert_eq!(list["MailInfos"][0]["MailIndex"], second);
    let next = mail::get_mail_list(
        State(state.clone()),
        form(&a, &format!("LastMailIndex={second}&MaxCount=1")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(next["MailInfos"][0]["MailIndex"], first);
    let page = mail::get_mail_list_by_page(State(state.clone()), form(&a, "PageNo=1&MaxCount=1"))
        .await
        .unwrap()
        .0;
    assert_eq!(page["MailInfos"], next["MailInfos"]);
    let check = mail::check_new_mail(
        State(state.clone()),
        form(&a, &format!("TopmostMailIndex={first}")),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(check["NewMailArrived"], true);
    assert_eq!(check["TotalMailCount"], 2);
    for (who, id) in [(&a, expired), (&b, first)] {
        assert_eq!(
            mail::receive_mail(State(state.clone()), form(who, &format!("MailIndex={id}")))
                .await
                .unwrap()
                .0["Result"],
            "Fail"
        );
    }
}

#[tokio::test]
async fn mail_claims_all_attachment_types_once() {
    let (state, a, _) = setup().await;
    let item = state.tables.get_item_index("HERO_FREY").unwrap();
    let stack = state
        .tables
        .items
        .reward_item(2001)
        .filter(|m| m.kind == "Item")
        .map(|_| 2001)
        .unwrap_or_else(|| {
            (1..500000)
                .find(|i| {
                    state
                        .tables
                        .items
                        .reward_item(*i)
                        .is_some_and(|m| m.kind == "Item")
                })
                .unwrap()
        });
    let equip = (1..500000)
        .find(|i| {
            state
                .tables
                .items
                .reward_item(*i)
                .is_some_and(|m| m.kind == "Equip")
        })
        .unwrap();
    let id = add_mail(
        &state,
        &a,
        123,
        7,
        Some(vec![
            mail::MailItemInfo {
                item_index: item,
                item_count: 1,
            },
            mail::MailItemInfo {
                item_index: stack,
                item_count: 3,
            },
            mail::MailItemInfo {
                item_index: equip,
                item_count: 2,
            },
        ]),
    )
    .await;
    sqlx::query("UPDATE mails SET reward_stamina=12 WHERE mail_id=?")
        .bind(id)
        .execute(&state.db)
        .await
        .unwrap();
    let response = mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
        .await
        .unwrap()
        .0;
    assert_eq!(response["Result"], "Success");
    let reward = &response["MailReceiveResult"];
    assert_eq!(reward["CurrencyResults"][1]["Field1"], "NewSysGem");
    assert_eq!(reward["HeroAddResult"]["HeroInfo"]["HeroIndex"], 2);
    assert_eq!(reward["ItemResults"][0]["AddCount"], 3);
    assert_eq!(reward["EquipItemResults"].as_array().unwrap().len(), 2);
    assert_eq!(reward["StaminaResults"][0]["AddValue"], 12);
    assert_eq!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
            .await
            .unwrap()
            .0["Result"],
        "Fail"
    );
    let user = login(&state, "social-a").await;
    assert_eq!(user.user_info.gold, a.user_info.gold + 123);
    assert_eq!(user.user_info.gem, a.user_info.gem + 7);
    assert_eq!(user.equip_items.len(), 2);
    assert_eq!(user.heroes.len(), 2);
}

#[tokio::test]
async fn malformed_and_locked_mail_cannot_consume_or_partially_grant() {
    let (state, a, _) = setup().await;
    let id = add_mail(
        &state,
        &a,
        123,
        7,
        Some(vec![mail::MailItemInfo {
            item_index: i32::MAX,
            item_count: 1,
        }]),
    )
    .await;
    assert_eq!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
            .await
            .unwrap()
            .0["Result"],
        "Fail"
    );
    let row = sqlx::query("SELECT gold,gem FROM user_info WHERE account_id=?")
        .bind(a.user_info.account_id)
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(row.get::<i64, _>("gold"), a.user_info.gold);
    assert_eq!(row.get::<i32, _>("gem"), a.user_info.gem);
    sqlx::query(
        "UPDATE mails SET reward_items=NULL,opens_at='2999-01-01 00:00:00' WHERE mail_id=?",
    )
    .bind(id)
    .execute(&state.db)
    .await
    .unwrap();
    assert_eq!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
            .await
            .unwrap()
            .0["Result"],
        "Fail"
    );
    let pending: i64 = sqlx::query_scalar("SELECT is_received FROM mails WHERE mail_id=?")
        .bind(id)
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(pending, 0);
}

#[tokio::test]
async fn concurrent_mail_claims_and_bulk_limits_prevent_double_rewards() {
    let (state, a, _) = setup().await;
    let id = add_mail(&state, &a, 100, 0, None).await;
    let (one, two) = tokio::join!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}"))),
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
    );
    let results = [one.unwrap().0, two.unwrap().0];
    assert_eq!(
        results.iter().filter(|v| v["Result"] == "Success").count(),
        1
    );
    for _ in 0..3 {
        add_mail(&state, &a, 10, 0, None).await;
    }
    let bulk = mail::receive_all_mail(State(state.clone()), form(&a, "MaxCount=2"))
        .await
        .unwrap()
        .0;
    assert_eq!(bulk["ReceivedMailCount"], 2);
    assert_eq!(bulk["MailReceiveResults"].as_array().unwrap().len(), 2);
    let gold: i64 = sqlx::query_scalar("SELECT gold FROM user_info WHERE account_id=?")
        .bind(a.user_info.account_id)
        .fetch_one(&state.db)
        .await
        .unwrap();
    assert_eq!(gold, a.user_info.gold + 120);
}

#[tokio::test]
async fn global_mail_is_delivered_and_claimed_once_per_account() {
    let (state, a, b) = setup().await;
    sqlx::query("INSERT INTO global_mails(title,reward_gold) VALUES ('Announcement',50)")
        .execute(&state.db)
        .await
        .unwrap();
    let list = mail::get_global_mail_list(State(state.clone()), form(&a, ""))
        .await
        .unwrap()
        .0;
    let id = list["MailInfos"][0]["MailIndex"].as_i64().unwrap();
    assert_eq!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
            .await
            .unwrap()
            .0["Result"],
        "Success"
    );
    let again = mail::get_global_mail_list(State(state.clone()), form(&a, ""))
        .await
        .unwrap()
        .0;
    assert!(again["MailInfos"].as_array().unwrap().is_empty());
    let other = mail::get_global_mail_list(State(state.clone()), form(&b, ""))
        .await
        .unwrap()
        .0;
    assert_eq!(other["MailInfos"].as_array().unwrap().len(), 1);
    assert_ne!(other["MailInfos"][0]["MailIndex"], id);
    let normal = mail::get_mail_list(State(state.clone()), form(&b, ""))
        .await
        .unwrap()
        .0;
    assert!(
        normal["MailInfos"].as_array().unwrap().is_empty(),
        "Client merges the two inbox lists"
    );
}

#[tokio::test]
async fn duplicate_heroes_grant_the_client_tables_compensation() {
    let (state, a, _) = setup().await;
    let index = (1..500000)
        .find(|i| {
            state
                .tables
                .items
                .reward_item(*i)
                .is_some_and(|m| m.kind == "Hero" && m.duplicate_reward_index > 0)
        })
        .unwrap();
    let hero = state.tables.items.reward_item(index).unwrap().hero_index;
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,star,level) VALUES (?,99,?,5,1)")
        .bind(a.user_info.account_id)
        .bind(hero)
        .execute(&state.db)
        .await
        .unwrap();
    let id = add_mail(
        &state,
        &a,
        0,
        0,
        Some(vec![mail::MailItemInfo {
            item_index: index,
            item_count: 1,
        }]),
    )
    .await;
    let claimed = mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
        .await
        .unwrap()
        .0;
    assert_eq!(claimed["Result"], "Success");
    assert!(claimed["MailReceiveResult"]["HeroAddResult"].is_null());
    assert!(
        claimed["MailReceiveResult"]["ItemResults"][0]["AddCount"]
            .as_i64()
            .unwrap()
            > 0
    );
    let count: i64 =
        sqlx::query_scalar("SELECT COUNT(*) FROM heroes WHERE account_id=? AND hero_index=?")
            .bind(a.user_info.account_id)
            .bind(hero)
            .fetch_one(&state.db)
            .await
            .unwrap();
    assert_eq!(count, 1);
}

#[tokio::test]
async fn claims_serialize_across_database_connections_and_survive_restart() {
    let (template, _, _) = setup().await;
    let path = std::env::temp_dir().join(format!("sprk-social-{}.sqlite", uuid::Uuid::new_v4()));
    let options = sqlx::sqlite::SqliteConnectOptions::new()
        .filename(&path)
        .create_if_missing(true)
        .busy_timeout(Duration::from_secs(10));
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(5)
        .min_connections(5)
        .connect_with(options)
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let state = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let a = login(&state, "file-account").await;
    let id = add_mail(&state, &a, 90, 0, None).await;
    let (one, two) = tokio::join!(
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}"))),
        mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}")))
    );
    let results = [one.unwrap().0, two.unwrap().0];
    assert_eq!(
        results.iter().filter(|v| v["Result"] == "Success").count(),
        1
    );
    pool.close().await;
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(2)
        .connect_with(sqlx::sqlite::SqliteConnectOptions::new().filename(&path))
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let restarted = AppState::new(pool.clone(), template.tables.as_ref().clone());
    let again = login(&restarted, "file-account").await;
    assert_eq!(again.user_info.gold, a.user_info.gold + 90);
    assert_eq!(
        mail::receive_mail(State(restarted), form(&again, &format!("MailIndex={id}")))
            .await
            .unwrap()
            .0["Result"],
        "Fail"
    );
    pool.close().await;
    std::fs::remove_file(&path).unwrap();
}

struct Socket(BufReader<TcpStream>);
impl Socket {
    async fn send(&mut self, name: &str, body: Value) {
        self.0
            .get_mut()
            .write_all(&chat::encode_packet(name, &body))
            .await
            .unwrap();
    }
    async fn read(&mut self) -> (String, Value) {
        let mut line = String::new();
        tokio::time::timeout(Duration::from_secs(3), self.0.read_line(&mut line))
            .await
            .unwrap()
            .unwrap();
        let (name, encoded) = line.trim_end().split_once(' ').unwrap();
        (
            name.into(),
            serde_json::from_slice(&STANDARD.decode(encoded).unwrap()).unwrap(),
        )
    }
    async fn connect(address: std::net::SocketAddr, a: &user::LoginResponse, channel: i32) -> Self {
        let mut socket = Self(BufReader::new(TcpStream::connect(address).await.unwrap()));
        // Native LoginReq deliberately has no SessionKey field.
        socket.send("LoginReq",json!({"RequestId":11,"AccountId":a.user_info.account_id,"ChannelNo":channel,"FriendIds":[]})).await;
        let (name, value) = socket.read().await;
        assert_eq!(name, "LoginRes");
        assert_eq!(value["RequestId"], 11);
        assert_eq!(value["Result"], "Success");
        socket
    }
}

#[tokio::test]
async fn native_socket_routes_channels_world_whispers_and_friend_notifications() {
    let (state, a, b) = setup().await;
    let c = login(&state, "social-c").await;
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    let server = tokio::spawn(chat::serve(listener, state.clone()));
    let mut sa = Socket::connect(addr, &a, 1).await;
    let mut sb = Socket::connect(addr, &b, 1).await;
    let mut sc = Socket::connect(addr, &c, 2).await;
    sa.send("MessageReq",json!({"RequestId":12,"GroupType":"Channel","Message":format!("ChannelChat {}",json!({"Chat":"hello channel","SenderName":"FORGED","AdminLevel":99}))})).await;
    assert_eq!(sa.read().await.0, "MessageRes");
    assert_eq!(sa.read().await.0, "MessageNot");
    let (_, message) = sb.read().await;
    assert_eq!(message["Type"], "ChannelChat");
    let content: Value = serde_json::from_str(message["Content"].as_str().unwrap()).unwrap();
    assert_eq!(content["SenderName"], a.user_info.nick);
    assert_eq!(content["AdminLevel"], 0);
    assert!(
        tokio::time::timeout(Duration::from_millis(100), sc.0.fill_buf())
            .await
            .is_err()
    );
    sa.send(
        "MessageReq",
        json!({"RequestId":13,"Message":"WorldChat {\"Chat\":\"hello world\"}"}),
    )
    .await;
    assert_eq!(sa.read().await.0, "MessageRes");
    sa.read().await;
    assert_eq!(sb.read().await.1["Type"], "WorldChat");
    assert_eq!(sc.read().await.1["Type"], "WorldChat");
    sa.send("MessageReq",json!({"RequestId":14,"ReceiverIds":[b.user_info.account_id],"Message":"WhisperChat {\"Chat\":\"private\"}"})).await;
    sa.read().await;
    sa.read().await;
    assert_eq!(sb.read().await.1["Type"], "WhisperChat");
    let history = chat::get_recent_chats(State(state.clone()), form(&c, "ChannelId=2"))
        .await
        .unwrap()
        .0;
    assert!(history["WhisperChats"].as_array().unwrap().is_empty());
    assert_eq!(history["WorldChats"].as_array().unwrap().len(), 1);
    sa.send(
        "MessageReq",
        json!({"RequestId":15,"Message":"WorldChat {\"Chat\":\"rate limited\"}"}),
    )
    .await;
    assert_eq!(sa.read().await.1["Result"], "Fail");
    let _ = friend::request_friend(
        State(state.clone()),
        form(&a, &format!("FriendId={}", b.user_info.account_id)),
    )
    .await
    .unwrap();
    assert_eq!(sb.read().await.1["Type"], "RequestFriend");
    sa.send(
        "MessageReq",
        json!({"RequestId":16,"ReceiverIds":[b.user_info.account_id],"Message":"RemoveFriend {}"}),
    )
    .await;
    assert_eq!(sa.read().await.1["Result"], "Fail");
    let mut reconnected = Socket::connect(addr, &b, 2).await;
    // World history follows LoginRes; whisper history is embedded in LoginRes.
    assert_eq!(reconnected.read().await.1["Type"], "WorldChat");
    sb.send("ChangeChannelReq", json!({"RequestId":17,"ChannelNo":2}))
        .await;
    assert_eq!(sb.read().await.1["Changed"], true);
    server.abort();
}

#[tokio::test]
async fn socket_rejects_unknown_accounts_and_handles_fragmented_coalesced_packets() {
    let (state, a, _) = setup().await;
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    let server = tokio::spawn(chat::serve(listener, state));
    let mut unknown = Socket(BufReader::new(TcpStream::connect(addr).await.unwrap()));
    unknown
        .send("LoginReq", json!({"RequestId":7,"AccountId":999999}))
        .await;
    assert_eq!(unknown.read().await.1["Result"], "Fail");
    let mut socket = Socket(BufReader::new(TcpStream::connect(addr).await.unwrap()));
    let login = chat::encode_packet(
        "LoginReq",
        &json!({"RequestId":8,"AccountId":a.user_info.account_id}),
    );
    socket.0.get_mut().write_all(&login[..5]).await.unwrap();
    let mut tail = login[5..].to_vec();
    tail.extend(chat::encode_packet("PingReq", &json!({"RequestId":9})));
    socket.0.get_mut().write_all(&tail).await.unwrap();
    assert_eq!(socket.read().await.1["RequestId"], 8);
    let (name, ping) = socket.read().await;
    assert_eq!(name, "PingRes");
    assert_eq!(ping["RequestId"], 9);
    server.abort();
}
