//! Exercise native forms and TCP packets against isolated databases, never sprk.db.
use crate::api::{
    account::user,
    community::{chat, friend, mail},
};
use crate::{database, state::AppState};
use axum::{body::Bytes, extract::State};
use base64::{engine::general_purpose::STANDARD, Engine};
use serde_json::{json, Value};
use sqlx::Row;
use std::time::Duration;
use tokio::{
    io::{AsyncBufReadExt, AsyncWriteExt, BufReader},
    net::{TcpListener, TcpStream},
};

#[tokio::test]
async fn arcdim_guest_second_team_ready_reaches_owner_over_native_socket() {
    for raid in [401,402] {
        let (state,host,guest)=setup().await;
        let a=host.user_info.account_id;let b=guest.user_info.account_id;
        for account in [a,b] {
            crate::api::battle::put(&mut state.db.acquire().await.unwrap(),account,"dungeon",(10_i64<<32)|9,
                &json!({"ChapterIndex":10,"DungeonIndex":9,"FirstRewardedDiff":15})).await.unwrap();
        }
        sqlx::query("UPDATE heroes SET level=100").execute(&state.db).await.unwrap();
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
        let server=tokio::spawn(chat::serve(listener,state.clone()));
        let mut owner=Socket::connect(addr,&host,1).await;let mut follower=Socket::connect(addr,&guest,1).await;
        // LoginRes precedes reconnect replay; finish that handshake before
        // creating the room so the fixture cannot race a second room snapshot.
        for socket in [&mut owner,&mut follower] {
            socket.send("PingReq",json!({"RequestId":100})).await;
            assert_eq!(socket.read().await.0,"PingRes");
        }
        let created=crate::api::battle::execute_request(&state,"raid/create_raid_room",form(&host,&format!("RaidIndex={raid}&RaidLevel=1&Opened=1"))).await.unwrap();
        assert_eq!(created["Result"],"Success");let room=created["RoomNo"].as_i64().unwrap();
        let joined=crate::api::battle::execute_request(&state,"raid/join_raid_room",form(&guest,&format!("RoomNo={room}"))).await.unwrap();
        assert_eq!(joined["Result"],"Success");owner.read().await;follower.read().await;
        follower.send("MessageReq",json!({"RequestId":1,"ReceiverIds":[a],"Message":"PartyDeckHeroInfo {\"HeroIndex\":1,\"DeckIndex\":1,\"HeroInfo\":{\"HeroIndex\":1}}"})).await;
        assert_eq!(follower.read().await.1["Result"],"Success");
        assert_eq!(owner.read().await.1["Type"],"PartyRoomReady");
        assert_eq!(owner.read().await.1["Type"],"PartyDeckHeroInfo");
        for ready in [true,false,true] {
            follower.send("MessageReq",json!({"RequestId":2,"ReceiverIds":[a],"Message":format!("PartyRoomReady {}",json!({"IsReady":ready}))})).await;
            assert_eq!(follower.read().await.1["Result"],"Success");
            let notice=owner.read().await.1;assert_eq!(notice["Type"],"PartyRoomReady");assert_eq!(notice["SenderId"],b);
            let content:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();assert_eq!(content["IsReady"],ready);
            let raw:String=sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind='party_member' AND idx=?")
                .bind(b).bind(room).fetch_one(&state.db).await.unwrap();
            let stored:Value=serde_json::from_str(&raw).unwrap();assert_eq!(stored["IsBattleReady"],ready);
            assert!(stored["DeckHeros"].as_object().unwrap().is_empty());
            assert_eq!(stored["SubDeckHeros"].as_object().unwrap().len(),1);
        }
        server.abort();
    }
}

#[tokio::test]
async fn conquest_solo_chat_reconnect_then_leave_allows_party_invitation_for_every_boss() {
    for level in 101..=103 {
        let(state,host,guest)=setup().await;let a=host.user_info.account_id;let b=guest.user_info.account_id;
        let created=crate::api::battle::execute_request(&state,"raid/create_raid_room",form(&guest,&format!("RaidIndex=90001&RaidLevel={level}&Opened=0"))).await.unwrap();
        assert_eq!(created["Result"],"Success");let solo=created["RoomNo"].as_i64().unwrap();
        // Connecting with an existing solo room exercises actual chat login:
        // an invalid solo snapshot used to terminate the socket after LoginRes.
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let address=listener.local_addr().unwrap();
        let server=tokio::spawn(chat::serve(listener,state.clone()));
        let mut owner=Socket::connect(address,&host,1).await;
        owner.send("PingReq",json!({"RequestId":100})).await;
        assert_eq!(owner.read().await.0,"PingRes");
        let mut follower=Socket::connect(address,&guest,1).await;
        let notice=follower.read().await.1;assert_eq!(notice["Type"],"JoinPartyNotice");
        let replay:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        let expected=state.tables.arena_guild.rows("GuildSuppressSession").iter().find(|v|v["RaidIndex"]==90001).unwrap()["SuppressDungeonIndex"][(level-101) as usize].clone();
        assert_eq!(replay["PartyInfo"]["GuildSuppressDungeonIndex"],expected);
        assert_eq!(replay["PartyInfo"]["RaidLevel"],level);
        follower.send("PingReq",json!({"RequestId":100})).await;
        assert_eq!(follower.read().await.0,"PingRes");
        let guild=sqlx::query("INSERT INTO guilds(name,master_account_id) VALUES('SoloReturnInvite',?)")
            .bind(a).execute(&state.db).await.unwrap().last_insert_rowid();
        for account in [a,b] {
            sqlx::query("INSERT INTO guild_members(guild_id,account_id) VALUES(?,?)")
                .bind(guild).bind(account).execute(&state.db).await.unwrap();
        }
        // The explicit result-to-lobby client hook sends this ordinary leave.
        let left=crate::api::battle::execute_request(&state,"raid/leave_raid_room",form(&guest,&format!("RoomNo={solo}&AccountId={b}"))).await.unwrap();
        assert_eq!(left["Result"],"Success");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_room_members WHERE account=?").bind(b).fetch_one(&state.db).await.unwrap(),0);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_rooms WHERE id=?").bind(solo).fetch_one(&state.db).await.unwrap(),0);
        let created=crate::api::battle::execute_request(&state,"raid/create_raid_room",form(&host,"RaidIndex=90001&RaidLevel=1&Opened=0")).await.unwrap();
        assert_eq!(created["Result"],"Success");
        owner.send("MessageReq",json!({"RequestId":1,"ReceiverIds":[b],"Message":"InvitePartyReq {}"})).await;
        assert_eq!(owner.read().await.1["Result"],"Success");
        assert_eq!(follower.read().await.1["Type"],"InvitePartyReq");
        server.abort();
    }
}

#[tokio::test]
async fn conquest_invite_acceptance_preserves_native_party_screen_context_for_every_boss() {
    for level in 1..=3 {
        let(state,host,guest)=setup().await;let a=host.user_info.account_id;let b=guest.user_info.account_id;
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let address=listener.local_addr().unwrap();
        let server=tokio::spawn(chat::serve(listener,state.clone()));
        let mut owner=Socket::connect(address,&host,1).await;let mut follower=Socket::connect(address,&guest,1).await;
        for socket in [&mut owner,&mut follower] {
            socket.send("PingReq",json!({"RequestId":100})).await;assert_eq!(socket.read().await.0,"PingRes");
        }
        let guild=sqlx::query("INSERT INTO guilds(name,master_account_id) VALUES('ConquestInvite',?)")
            .bind(a).execute(&state.db).await.unwrap().last_insert_rowid();
        for account in [a,b] {
            sqlx::query("INSERT INTO guild_members(guild_id,account_id) VALUES(?,?)").bind(guild).bind(account).execute(&state.db).await.unwrap();
        }
        let created=crate::api::battle::execute_request(&state,"raid/create_raid_room",form(&host,&format!("RaidIndex=90001&RaidLevel={level}&Opened=0"))).await.unwrap();
        assert_eq!(created["Result"],"Success","{created}");let room=created["RoomNo"].as_i64().unwrap();
        owner.send("MessageReq",json!({"RequestId":1,"GroupType":if level==2 {"Guild"}else{"None"},"ReceiverIds":[b],"Message":"InvitePartyReq {}"})).await;
        assert_eq!(owner.read().await.1["Result"],"Success");assert_eq!(follower.read().await.1["Type"],"InvitePartyReq");
        let key=639_000_000_000_000_001_i64+level;
        follower.send("MessageReq",json!({"RequestId":2,"ReceiverIds":[a],"Message":format!("JoinPartyReq {}",json!({"RoomNo":room.to_string(),"Accepted":"True","SendKey":key.to_string()}))})).await;
        assert_eq!(follower.read().await.1["Result"],"Success");
        let notice=owner.read().await.1;assert_eq!(notice["Type"],"JoinPartyReq");
        let pending:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        assert_eq!(pending["Invitee"]["AccountId"],b);assert_eq!(pending["SendKey"],key);
        // The host's ordinary native reply has an optimistic roster. Neither
        // its dungeon identifier nor its serialized hero stats are trusted.
        let party=json!({"GuildSuppressDungeonIndex":999999,"PartyMembers":[{"AccountId":a,"DeckHeros":{"1":{"HeroIndex":1,"Level":999}},"SubDeckHeros":{}}]});
        owner.send("MessageReq",json!({"RequestId":3,"ReceiverIds":[b],"Message":format!("JoinPartyRes {}",json!({"Result":"Success","SendKey":key.to_string(),"PartyInfo":party}))})).await;
        assert_eq!(owner.read().await.1["Result"],"Success");
        let notice=follower.read().await.1;assert_eq!(notice["Type"],"JoinPartyRes");
        let accepted:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        assert_eq!(accepted["Result"],"Success");assert_eq!(accepted["SendKey"],key);
        let index=state.tables.arena_guild.rows("GuildSuppressSession").iter().find(|v|v["RaidIndex"]==90001).unwrap()["SuppressDungeonIndex"][(level-1) as usize].as_i64().unwrap();
        assert_eq!(accepted["PartyInfo"]["GuildSuppressDungeonIndex"],index,"The native guest UI needs a valid conquest boss definition");
        assert!(state.tables.arena_guild.find("GuildSuppressDungeon",&[("Index",index)]).is_some());
        assert_eq!(accepted["PartyInfo"]["RaidIndex"],90001);assert_eq!(accepted["PartyInfo"]["RaidLevel"],level);
        assert_eq!(accepted["PartyInfo"]["PartyMembers"].as_array().unwrap().len(),2);
        assert_eq!(accepted["PartyInfo"]["PartyMembers"][0]["DeckHeros"]["1"]["Level"],1);
        if let Ok(directory)=std::env::var("SPRK_CONQUEST_INVITE_CAPTURE_DIR") {
            std::fs::create_dir_all(&directory).unwrap();
            std::fs::write(std::path::Path::new(&directory).join(format!("party-{level}.json")),accepted["PartyInfo"].to_string()).unwrap();
        }
        // The native guest confirms the socket approval over HTTP. This must
        // remain idempotent and publish the same complete roster to both sides.
        let joined=crate::api::battle::execute_request(&state,"raid/join_raid_room",form(&guest,&format!("RoomNo={room}"))).await.unwrap();
        assert_eq!(joined["Result"],"Success");
        for socket in [&mut owner,&mut follower] {
            let notice=socket.read().await.1;assert_eq!(notice["Type"],"JoinPartyNotice");
            let roster:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
            assert_eq!(roster["PartyInfo"]["GuildSuppressDungeonIndex"],index);
            assert_eq!(roster["PartyInfo"]["PartyMembers"].as_array().unwrap().len(),2);
        }
        crate::api::battle::party_messages::reconnect(&state,b).await.unwrap();
        let notice=follower.read().await.1;assert_eq!(notice["Type"],"JoinPartyNotice");
        let replay:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        assert_eq!(replay["PartyInfo"]["GuildSuppressDungeonIndex"],index);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_room_members WHERE room=?").bind(room).fetch_one(&state.db).await.unwrap(),2);
        server.abort();
    }
}

#[tokio::test]
async fn mail_notifications_follow_commits_coalesce_bursts_and_isolate_recipients() {
    let (state,a,b)=setup().await;
    mail::deliver_notifications(&state).await.unwrap();
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();
    let address=listener.local_addr().unwrap();let server=tokio::spawn(chat::serve(listener,state.clone()));
    let mut sa=Socket::connect(address,&a,1).await;let mut sb=Socket::connect(address,&b,1).await;
    let mut tx=state.db.begin().await.unwrap();
    sqlx::query("INSERT INTO mails(account_id,sender,title) VALUES(?,'Guild','Rolled back reward')")
        .bind(a.user_info.account_id).execute(&mut *tx).await.unwrap();
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mail_notifications").fetch_one(&mut *tx).await.unwrap(),1);
    tx.rollback().await.unwrap();
    assert_eq!(mail::deliver_notifications(&state).await.unwrap(),0);
    let mut tx=state.db.begin().await.unwrap();
    let mut latest=0;
    for title in ["Dispatch equipment rewards","Guild attendance reward","Arena season reward","Battle equipment rewards"] {
        latest=sqlx::query("INSERT INTO mails(account_id,sender,title) VALUES(?,'System',?)")
            .bind(a.user_info.account_id).bind(title).execute(&mut *tx).await.unwrap().last_insert_rowid();
    }
    let other=sqlx::query("INSERT INTO mails(account_id,sender,title) VALUES(?,'System','Other recipient')")
        .bind(b.user_info.account_id).execute(&mut *tx).await.unwrap().last_insert_rowid();
    let global=sqlx::query("INSERT INTO mails(account_id,sender,title,global_id) VALUES(?,'System','Global',123)")
        .bind(a.user_info.account_id).execute(&mut *tx).await.unwrap().last_insert_rowid();
    // Expired, deleted or already claimed mail must not generate stale hints.
    sqlx::query("INSERT INTO mails(account_id,sender,title,expires_at) VALUES(?,'System','Expired','2000-01-01 00:00:00')")
        .bind(a.user_info.account_id).execute(&mut *tx).await.unwrap();
    sqlx::query("INSERT INTO mails(account_id,sender,title,is_received) VALUES(?,'System','Claimed',1)")
        .bind(a.user_info.account_id).execute(&mut *tx).await.unwrap();
    tx.commit().await.unwrap();
    let delivery=mail::start_notifications(state.clone());
    for (recipient,id,global) in [(0,latest,false),(0,global,true),(1,other,false)] {
        let socket=if recipient==0 {&mut sa}else{&mut sb};
        let (name,message)=socket.read().await;
        assert_eq!(name,"MessageNot");assert_eq!(message["Type"],"NewMailArrived");
        let body:Value=serde_json::from_str(message["Content"].as_str().unwrap()).unwrap();
        assert_eq!(body,json!({"MailIndex":id,"IsGlobal":global}));
    }
    // Let the dispatcher acknowledge the batch before simulating shutdown.
    // Killing it between socket send and queue deletion can replay safe hints.
    for _ in 0..50 {
        if sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mail_notifications").fetch_one(&state.db).await.unwrap()==0 {break;}
        tokio::time::sleep(Duration::from_millis(10)).await;
    }
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mail_notifications").fetch_one(&state.db).await.unwrap(),0);
    delivery.abort();let _=delivery.await;
    // A replay can only produce a duplicate hint, never a second reward.
    mail::deliver_notifications(&state).await.unwrap();
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mail_notifications").fetch_one(&state.db).await.unwrap(),0);
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE title='Guild attendance reward'").fetch_one(&state.db).await.unwrap(),1);
    for socket in [&mut sa,&mut sb] {
        socket.send("PingReq",json!({"RequestId":45})).await;
        assert_eq!(socket.read().await.0,"PingRes","No notification from a rolled-back reward or another account may leak");
    }
    // Mail queued while no dispatcher runs survives until delivery resumes.
    let id=add_mail(&state,&b,1,0,None).await;
    assert_eq!(mail::deliver_notifications(&state).await.unwrap(),1);
    let message=sb.read().await.1;let content:Value=serde_json::from_str(message["Content"].as_str().unwrap()).unwrap();
    assert_eq!(content["MailIndex"],id);
    server.abort();
}

#[tokio::test]
async fn fresh_accounts_have_avatars_without_startup_repairs() {
    let (s, a, b) = setup().await;
    assert!(a.user_info.avatar_hero_index > 0);
    assert!(b.user_info.avatar_hero_index > 0);
    database::create_tables(&s.db).await.unwrap();
    let restored = login(&s, "social-a").await;
    assert_eq!(restored.user_info.avatar_hero_index, a.user_info.avatar_hero_index);
}

async fn login(state: &AppState, name: &str) -> user::LoginResponse {
    user::test_login(State(state.clone()), Bytes::from(format!("LoginId={name}")))
        .await
        .unwrap()
        .0
}
async fn setup() -> (AppState, user::LoginResponse, user::LoginResponse) {
    let pool = sqlx::sqlite::SqlitePoolOptions::new()
        .max_connections(1)
        .connect("sqlite::memory:")
        .await
        .unwrap();
    database::create_tables(&pool).await.unwrap();
    let tables = crate::tables::test_tables();
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
async fn friend_recommendations_exclude_existing_relationships_and_allow_empty_lists() {
    let (state, a, b) = setup().await;
    let incoming = login(&state, "incoming").await;
    let accepted = login(&state, "accepted").await;
    let banned = login(&state, "banned").await;
    befriend(&state, &a, &accepted).await;
    let request = friend::request_friend(
        State(state.clone()),
        form(&incoming, &format!("FriendId={}", a.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(request["Result"], "Success");
    sqlx::query("UPDATE accounts SET is_banned=1 WHERE account_id=?")
        .bind(banned.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();

    // Opening Accept/Request omits Keyword; blank and whitespace are equivalent.
    for fields in ["", "Keyword=", "Keyword=+++%09"] {
        let result = friend::search_friend(State(state.clone()), form(&a, fields))
            .await
            .unwrap()
            .0;
        assert_eq!(result["Result"], "Success");
        assert_eq!(result["FriendInfos"].as_array().unwrap().len(), 1);
        assert_eq!(result["FriendInfos"][0]["AccountId"], b.user_info.account_id);
    }

    let request = friend::request_friend(
        State(state.clone()),
        form(&a, &format!("FriendId={}", b.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(request["Result"], "Success");
    let empty = friend::search_friend(State(state.clone()), form(&a, ""))
        .await
        .unwrap()
        .0;
    assert_eq!(empty["Result"], "Success");
    assert_eq!(empty["FriendInfos"], json!([]));
    let pending_search = friend::search_friend(
        State(state.clone()),
        form(&a, &format!("Keyword={}", b.user_info.account_id)),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(pending_search["Result"], "FriendNotFound");
    assert_eq!(pending_search["FriendInfos"], json!([]));
}

#[tokio::test]
async fn friend_recommendations_sample_the_player_pool_with_a_limit() {
    let (state, a, b) = setup().await;
    let mut eligible = std::collections::BTreeSet::from([b.user_info.account_id]);
    for index in 0..30 {
        let id: i64 = sqlx::query_scalar(
            "INSERT INTO accounts(login_id,nick,last_login) VALUES(?,?,datetime('now')) RETURNING account_id",
        )
        .bind(format!("candidate-{index}"))
        .bind(format!("Candidate {index}"))
        .fetch_one(&state.db)
        .await
        .unwrap();
        sqlx::query("INSERT INTO user_info(account_id) VALUES(?)")
            .bind(id)
            .execute(&state.db)
            .await
            .unwrap();
        eligible.insert(id);
    }
    let mut samples = std::collections::BTreeSet::new();
    for _ in 0..4 {
        let result = friend::search_friend(State(state.clone()), form(&a, ""))
            .await
            .unwrap()
            .0;
        assert_eq!(result["Result"], "Success");
        let ids: std::collections::BTreeSet<_> = result["FriendInfos"]
            .as_array()
            .unwrap()
            .iter()
            .map(|info| info["AccountId"].as_i64().unwrap())
            .collect();
        assert_eq!(ids.len(), 20);
        assert!(ids.is_subset(&eligible));
        samples.insert(ids);
    }
    // Four identical subsets out of 31 choose 20 would be vanishingly unlikely.
    assert!(
        samples.len() > 1,
        "recommendations must sample beyond the first 20 IDs"
    );
}

#[tokio::test]
async fn friend_search_matches_username_nickname_and_exact_player_id() {
    let (state, a, b) = setup().await;
    sqlx::query("UPDATE accounts SET nick='Friend With Space' WHERE account_id=?")
        .bind(b.user_info.account_id)
        .execute(&state.db)
        .await
        .unwrap();
    // Launcher usernames are stored separately from the internal login ID.
    sqlx::query("INSERT INTO credentials(username,login_id,password_hash,created_at) VALUES('launcher_name','social-b','unused-test-hash',0)")
        .execute(&state.db)
        .await
        .unwrap();
    for keyword in [
        "friend with space".to_string(),
        "WITH SPACE".to_string(),
        "LAUNCHER_NAME".to_string(),
        "launcher_".to_string(),
        b.user_info.account_id.to_string(),
        format!("  {}  ", b.user_info.account_id),
    ] {
        let result = friend::search_friend(
            State(state.clone()),
            form(&a, &format!("Keyword={}", urlencoding::encode(&keyword))),
        )
        .await
        .unwrap()
        .0;
        assert_eq!(result["Result"], "Success", "keyword: {keyword}");
        assert_eq!(result["FriendInfos"].as_array().unwrap().len(), 1);
        assert_eq!(result["FriendInfos"][0]["AccountId"], b.user_info.account_id);
        assert!(result["FriendInfos"][0].get("username").is_none());
        assert!(result["FriendInfos"][0].get("login_id").is_none());
    }
    for keyword in ["social-b", "%", "_", "!", "no-match", "9223372036854775808"] {
        let result = friend::search_friend(
            State(state.clone()),
            form(&a, &format!("Keyword={}", urlencoding::encode(keyword))),
        )
        .await
        .unwrap()
        .0;
        // '_' is literal and does match the launcher username.
        assert_eq!(
            result["Result"],
            if keyword == "_" { "Success" } else { "FriendNotFound" }
        );
    }
    let oversized = friend::search_friend(
        State(state.clone()),
        form(&a, &format!("Keyword={}", "a".repeat(51))),
    )
    .await
    .unwrap()
    .0;
    assert_eq!(oversized["Result"], "FriendNotFound");
    assert_eq!(oversized["FriendInfos"], json!([]));
    assert!(friend::search_friend(State(state.clone()), Bytes::from_static(b"Keyword="))
        .await
        .is_err());
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
async fn friendship_exchange_all_preserves_native_repeated_fields() {
    let (state, a, b) = setup().await;
    let c = login(&state, "points-third").await;
    befriend(&state, &a, &b).await;
    befriend(&state, &a, &c).await;
    let aid = a.user_info.account_id;
    let bid = b.user_info.account_id;
    let cid = c.user_info.account_id;
    // Both friends have sent points; exchange-all must send and receive with
    // each friend in one request, including mixed native/JSON array formats.
    for other in [&b, &c] {
        let sent = friend::send_friendship_point(State(state.clone()), form(other, &format!("FriendIds={aid}"))).await.unwrap().0;
        assert_eq!(sent["Result"], "Success");
    }
    let fields = format!("SendFriendIds={bid}&SendFriendIds={cid}&SendFriendIds={bid}&RecvFriendIds={bid}&RecvFriendIds={cid}");
    let result = friend::send_recv_friendship_point(State(state.clone()), form(&a, &fields)).await.unwrap().0;
    assert_eq!(result["Result"], "Success");
    assert_eq!(result["FriendshipPointResult"]["AddValue"], 4 * friend::POINTS_PER_ACTION);
    assert_eq!(result["FriendshipPointInfos"].as_array().unwrap().len(), 2);
    let retry = friend::send_recv_friendship_point(State(state.clone()), form(&a, &fields)).await.unwrap().0;
    assert_eq!(retry["FriendshipPointResult"]["AddValue"], 0);
    sqlx::query("UPDATE friend_points SET last_send_time=NULL,last_recv_time=NULL WHERE account_id=?")
        .bind(aid).execute(&state.db).await.unwrap();
    let sent = friend::send_friendship_point(State(state.clone()), form(&a, &format!("FriendIds={bid}&FriendIds={cid}"))).await.unwrap().0;
    assert_eq!(sent["FriendshipPointResult"]["AddValue"], 2 * friend::POINTS_PER_ACTION);
    let recv = friend::recv_friendship_point(State(state.clone()), form(&a, &format!("FriendIds={bid}&FriendIds={cid}"))).await.unwrap().0;
    assert_eq!(recv["FriendshipPointResult"]["AddValue"], 2 * friend::POINTS_PER_ACTION);
}

#[tokio::test]
async fn friend_list_preserves_persisted_disconnect_time() {
    let (state, a, b) = setup().await;
    befriend(&state, &a, &b).await;
    state.sessions.clear();
    sqlx::query("UPDATE accounts SET last_login='2026-10-01 01:00:00',last_logout='2026-10-01 05:00:00' WHERE account_id=?")
        .bind(b.user_info.account_id).execute(&state.db).await.unwrap();
    for _ in 0..2 {
        let (_, friends, _, _) = friend::login_data(&state, a.user_info.account_id).await.unwrap();
        assert_eq!(friends[0]["IsOnline"], false);
        assert_eq!(friends[0]["LastOnlineTime"], "2026-10-01 05:00:00");
        assert_eq!(friends[0]["LastLogoutTime"], "2026-10-01 05:00:00");
    }
    sqlx::query("UPDATE accounts SET last_login='2026-10-02 01:00:00' WHERE account_id=?")
        .bind(b.user_info.account_id).execute(&state.db).await.unwrap();
    let info = friend::profile(&state, b.user_info.account_id).await.unwrap();
    assert_eq!(info["LastOnlineTime"], "2026-10-02 01:00:00");
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
    assert_eq!(row.get::<i64, _>("gem"), a.user_info.gem);
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

#[tokio::test]
async fn hero_info_returns_target_public_heroes_and_rejects_missing_accounts() {
    let (s, a, b) = setup().await;
    sqlx::query("UPDATE heroes SET level=37 WHERE account_id=?").bind(b.user_info.account_id).execute(&s.db).await.unwrap();
    let out = friend::inspect_user(State(s.clone()), form(&a, &format!("TargetAccountId={}", b.user_info.account_id))).await.unwrap().0;
    assert_eq!(out["Result"], "Success");
    let account = &out["CachedAccountInfo"];
    assert_eq!(account["UserInfo"]["AccountId"], b.user_info.account_id);
    assert_eq!(account["UserInfo"]["Nick"], b.user_info.nick);
    assert_eq!(account["HeroInfos"]["1"]["Level"], 37);
    assert!(account["HeroInfos"]["1"]["PunishmentRuneOptionInfos"].is_array());
    assert!(account["AiHeroInfos"].is_object() && account["DeckInfos"].is_object());
    assert!(account["UserInfo"].get("SessionKey").is_none());
    assert!(account["UserInfo"].get("Gold").is_none());
    assert!(out["GuildInfo"].is_null());
    assert_eq!(friend::inspect_user(State(s.clone()), form(&a, "TargetAccountId=99999")).await.unwrap().0["Result"], "NoTargetAccountId");
    sqlx::query("UPDATE accounts SET is_banned=1 WHERE account_id=?").bind(b.user_info.account_id).execute(&s.db).await.unwrap();
    assert_eq!(friend::inspect_user(State(s.clone()), form(&a, &format!("TargetAccountId={}",b.user_info.account_id))).await.unwrap().0["Result"], "NoTargetAccountId");
    assert!(friend::inspect_user(State(s), Bytes::from("TargetAccountId=1")).await.is_err());
}

#[tokio::test]
async fn raid_invites_presence_groups_private_join_and_sender_validation() {
    let (state, a, b) = setup().await;
    let c = login(&state, "invite-outsider").await;
    befriend(&state, &a, &b).await;
    let guild = sqlx::query("INSERT INTO guilds(name,master_account_id) VALUES('InviteTest',?)")
        .bind(a.user_info.account_id).execute(&state.db).await.unwrap().last_insert_rowid();
    for id in [a.user_info.account_id, b.user_info.account_id] {
        sqlx::query("INSERT INTO guild_members(guild_id,account_id) VALUES(?,?)")
            .bind(guild).bind(id).execute(&state.db).await.unwrap();
    }
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    let server = tokio::spawn(chat::serve(listener, state.clone()));
    let mut sa = Socket::connect(addr, &a, 1).await;
    assert_eq!(sa.read().await.1["Type"], "Logout");
    assert_eq!(sa.read().await.1["Type"], "GuildLogout");
    let mut sb = Socket::connect(addr, &b, 1).await;
    assert_eq!(sa.read().await.1["Type"], "Login");
    assert_eq!(sa.read().await.1["Type"], "GuildLogin");
    assert_eq!(sb.read().await.1["Type"], "Login");
    assert_eq!(sb.read().await.1["Type"], "GuildLogin");
    let mut sc = Socket::connect(addr, &c, 1).await;
    let list = super::tests::call(&state, &json!(a), "guild/get_all_guild_member_info", &format!("GuildId={guild}")).await;
    assert!(list.to_string().contains("\"IsOnline\":true"), "{list}");
    let created = crate::api::battle::execute_request(&state, "party_dungeon/create_party_dungeon_room", form(&a, "DungeonType=1&ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=2&Opened=0")).await.unwrap();
    let room = created["RoomNo"].as_i64().unwrap();
    // Direct and Invite All must ignore forged identity/room/dungeon fields.
    for group in [json!("None"), json!("Friend"), json!(4), json!("Guild"), json!(3)] {
        sa.send("MessageReq", json!({"RequestId":30,"GroupType":group,"ReceiverIds":[b.user_info.account_id,c.user_info.account_id],"Message":"InvitePartyReq {\"RoomNo\":99999,\"InvitorName\":\"FORGED\",\"RaidIndex\":99999}"})).await;
        assert_eq!(sa.read().await.1["Result"], "Success");
        let notice = sb.read().await.1;
        assert_eq!(notice["Type"], "InvitePartyReq");
        assert_eq!(notice["SenderId"], a.user_info.account_id);
        let payload: Value = serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        assert_eq!(payload["RoomNo"], room);
        assert_eq!(payload["InvitorName"], a.user_info.nick);
        assert_eq!(payload["chapterIndex"], 1);
        assert_eq!(payload["difficulty"], "2");
        assert_eq!(payload["dungeonType"], "1");
        if group == "None" { assert_eq!(sc.read().await.1["Type"], "InvitePartyReq"); }
        else { assert!(tokio::time::timeout(Duration::from_millis(50), sc.0.fill_buf()).await.is_err()); }
    }
    sc.send("MessageReq",json!({"RequestId":31,"ReceiverIds":[b.user_info.account_id],"Message":format!("InvitePartyReq {{\"RoomNo\":{room}}}")})).await;
    assert_eq!(sc.read().await.1["Result"], "Fail");
    assert!(tokio::time::timeout(Duration::from_millis(50), sb.0.fill_buf()).await.is_err());
    // The shipped JM serializer writes scalar fields as strings, including bools.
    sb.send("MessageReq",json!({"RequestId":"32","ReceiverIds":[a.user_info.account_id.to_string()],"Message":format!("JoinPartyReq {}",json!({"RoomNo":room.to_string(),"Accepted":"True","SendKey":"42"}))})).await;
    assert_eq!(sb.read().await.1["Result"], "Success");
    assert_eq!(sa.read().await.1["Type"], "JoinPartyReq");
    sa.send("MessageReq",json!({"RequestId":33,"ReceiverIds":[b.user_info.account_id],"Message":"JoinPartyRes {\"Result\":\"Success\",\"SendKey\":42}"})).await;
    assert_eq!(sa.read().await.1["Result"], "Success");
    assert_eq!(sb.read().await.1["Type"], "JoinPartyRes");
    let count: i64 = sqlx::query_scalar("SELECT count(*) FROM battle_room_members WHERE room=?").bind(room).fetch_one(&state.db).await.unwrap();
    assert_eq!(count, 2);
    // The last socket disconnect, even without an explicit Logout, updates peers.
    drop(sb);
    let mut disconnected = std::collections::BTreeSet::new();
    for _ in 0..8 {
        let notice = sa.read().await.1;
        if notice["Type"] == "Logout" || notice["Type"] == "GuildLogout" {
            assert_eq!(notice["SenderId"], b.user_info.account_id);
            disconnected.insert(notice["Type"].as_str().unwrap().to_owned());
        }
        if disconnected.len() == 2 { break; }
    }
    assert_eq!(disconnected.len(), 2);
    let saved: Option<String> = sqlx::query_scalar("SELECT last_logout FROM accounts WHERE account_id=?")
        .bind(b.user_info.account_id).fetch_one(&state.db).await.unwrap();
    let saved = chrono::NaiveDateTime::parse_from_str(&saved.expect("disconnect timestamp persisted"), "%Y-%m-%d %H:%M:%S").unwrap();
    assert!((chrono::Utc::now().naive_utc() - saved).num_seconds().abs() < 5);
    server.abort();
}
#[tokio::test]
async fn native_party_socket_join_deck_ready_and_isolation() {
    let (state,a,b)=setup().await;
    let c=login(&state,"party-outsider").await;
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
    let server=tokio::spawn(chat::serve(listener,state.clone()));
    let mut sa=Socket::connect(addr,&a,1).await;let mut sb=Socket::connect(addr,&b,1).await;let mut sc=Socket::connect(addr,&c,1).await;
    let created=crate::api::battle::execute_request(&state,"party_dungeon/create_party_dungeon_room",form(&a,"DungeonType=1&ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=2&Opened=1")).await.unwrap();
    assert_eq!(created["Result"],"Success","{created}");let room=created["RoomNo"].as_i64().unwrap();
    sb.send("MessageReq",json!({"RequestId":20,"ReceiverIds":[a.user_info.account_id],"Message":format!("JoinPartyReq {}",json!({"RoomNo":room,"SendKey":42,"Accepted":true,"Invitee":{"AccountId":999,"Nick":"forged"}}))})).await;
    assert_eq!(sb.read().await.1["Result"],"Success");
    let (_,notice)=sa.read().await;assert_eq!(notice["Type"],"JoinPartyReq");
    let invite:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
    assert_eq!(invite["Invitee"]["AccountId"],b.user_info.account_id);
    assert_eq!(invite["Invitee"]["Nick"],b.user_info.nick);
    sqlx::query("INSERT INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,2,2,1,1)").bind(a.user_info.account_id).execute(&state.db).await.unwrap();
    sa.send("MessageReq",json!({"RequestId":21,"ReceiverIds":[b.user_info.account_id],"Message":format!("JoinPartyRes {}",json!({"Result":"Success","SendKey":42,"PartyInfo":{"PartyMembers":[{"AccountId":a.user_info.account_id,"DeckHeros":{"2":{"HeroIndex":2,"Level":999}},"SubDeckHeros":{}}]}}))})).await;
    assert_eq!(sa.read().await.1["Result"],"Success");
    let (_,notice)=sb.read().await;assert_eq!(notice["Type"],"JoinPartyRes");
    let joined:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
    assert_eq!(joined["PartyInfo"]["PartyMembers"].as_array().unwrap().len(),2);
    assert_eq!(joined["PartyInfo"]["Difficulty"], "2");
    assert_eq!(joined["PartyInfo"]["PartyRoom"]["DungeonDifficulty"], "2");
    assert_eq!(joined["PartyInfo"]["PartyRoom"]["DungeonType"], "1");
    assert_eq!(joined["PartyInfo"]["PartyMembers"][0]["DeckHeros"]["2"]["Level"],1);
    sb.send("MessageReq",json!({"RequestId":22,"ReceiverIds":[c.user_info.account_id],"Message":"PartyDeckHeroInfo {\"HeroIndex\":1,\"DeckIndex\":0,\"HeroInfo\":{\"HeroIndex\":1,\"Level\":999}}"})).await;
    assert_eq!(sb.read().await.1["Result"],"Success");
    assert_eq!(sa.read().await.1["Type"],"PartyRoomReady");
    let (_,notice)=sa.read().await;assert_eq!(notice["Type"],"PartyDeckHeroInfo");
    let deck:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();assert_eq!(deck["HeroInfo"]["Level"],1);
    sb.send("MessageReq",json!({"RequestId":23,"Message":"PartyRoomReady {\"IsReady\":\"True\"}"})).await;
    assert_eq!(sb.read().await.1["Result"],"Success");assert_eq!(sa.read().await.1["Type"],"PartyRoomReady");
    sc.send("MessageReq",json!({"RequestId":24,"ReceiverIds":[a.user_info.account_id],"Message":"PartyRoomReady {\"IsReady\":true}"})).await;
    assert_eq!(sc.read().await.1["Result"],"Fail");
    assert!(tokio::time::timeout(Duration::from_millis(100),sc.0.fill_buf()).await.is_err());
    let saved:String=sqlx::query_scalar("SELECT data FROM battle_state WHERE account=? AND kind='party_member' AND idx=?").bind(b.user_info.account_id).bind(room).fetch_one(&state.db).await.unwrap();
    assert_eq!(serde_json::from_str::<Value>(&saved).unwrap()["IsBattleReady"],true);
    sb.send("MessageReq",json!({"RequestId":26,"Message":"ChangePartyDungeonRoomInfo {\"IsResetSquard\":\"False\"}"})).await;
    assert_eq!(sb.read().await.1["Result"],"Fail");
    sa.send("MessageReq",json!({"RequestId":27,"Message":"ChangePartyDungeonRoomInfo {\"ChapterIndex\":999,\"Difficulty\":\"Hell\",\"IsResetSquard\":\"False\"}"})).await;
    assert_eq!(sa.read().await.1["Result"],"Success");
    let (_,notice)=sb.read().await;
    assert_eq!(notice["Type"],"ChangePartyDungeonRoomInfo");
    let room_update:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
    assert_eq!(room_update["ChapterIndex"],1);assert_eq!(room_update["Difficulty"],"2");
    assert_eq!(room_update["DungeonType"],"1");assert_eq!(room_update["IsResetSquard"],false);
    let delegated=crate::api::battle::execute_request(&state,"party_dungeon/delegate_party_dungeon_room_master",form(&a,&format!("RoomNo={room}&PrevMasterId={}&CurMasterId={}",a.user_info.account_id,b.user_info.account_id))).await.unwrap();
    assert_eq!(delegated["Result"],"Success","{delegated}");
    assert_eq!(sa.read().await.1["Type"],"ChangePartyRoomMaster");sa.read().await;
    assert_eq!(sb.read().await.1["Type"],"ChangePartyRoomMaster");sb.read().await;
    let left=crate::api::battle::execute_request(&state,"party_dungeon/leave_party_dungeon_room",form(&a,&format!("RoomNo={room}"))).await.unwrap();
    assert_eq!(left["Result"],"Success");
    assert_eq!(sb.read().await.1["Type"],"KickedRaidMemberRes");sb.read().await;sb.read().await;
    sa.send("MessageReq",json!({"RequestId":25,"Message":"PartyRoomReady {\"IsReady\":true}"})).await;
    assert_eq!(sa.read().await.1["Result"],"Fail");
    let rejoined=crate::api::battle::execute_request(&state,"party_dungeon/join_party_dungeon_room",form(&a,&format!("RoomNo={room}"))).await.unwrap();
    assert_eq!(rejoined["Result"],"Success");
    assert_eq!(sa.read().await.1["Type"],"JoinPartyNotice");
    assert_eq!(sb.read().await.1["Type"],"JoinPartyNotice");
    let kicked=crate::api::battle::execute_request(&state,"party_dungeon/leave_party_dungeon_room",form(&b,&format!("RoomNo={room}&AccountId={}",a.user_info.account_id))).await.unwrap();
    assert_eq!(kicked["Result"],"Success");
    let (_,notice)=sa.read().await;
    assert_eq!(notice["Type"],"KickedRaidMemberRes");
    let kicked:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
    assert_eq!(kicked["KickedAccountIds"],json!([a.user_info.account_id]));
    assert_eq!(sb.read().await.1["Type"],"KickedRaidMemberRes");sb.read().await;sb.read().await;
    server.abort();
}
#[tokio::test]
async fn locked_raid_invites_reject_before_join_and_recheck_changed_settings_on_approval() {
    for family in ["raid","party_dungeon"] {
    let (state,host,guest)=setup().await;
    let a=host.user_info.account_id;let b=guest.user_info.account_id;
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
    let server=tokio::spawn(chat::serve(listener,state.clone()));
    let mut sa=Socket::connect(addr,&host,1).await;let mut sb=Socket::connect(addr,&guest,1).await;
    crate::api::battle::put(&mut state.db.acquire().await.unwrap(),a,"raid",1,&json!({"RaidIndex":1,"RaidLevel":9})).await.unwrap();
    let created=crate::api::battle::execute_request(&state,if family=="raid" {"raid/create_raid_room"}else{"party_dungeon/create_party_dungeon_room"},form(&host,if family=="raid" {"RaidIndex=1&RaidLevel=8&Opened=0"}else{"DungeonType=1&ChapterIndex=1&DungeonIndex=3&DungeonDifficulty=2&Opened=0"})).await.unwrap();
    assert_eq!(created["Result"],"Success","{created}");let room=created["RoomNo"].as_i64().unwrap();
    let before:(i64,i64,i64)=sqlx::query_as("SELECT stamina,gold,gem FROM user_info WHERE account_id=?").bind(b).fetch_one(&state.db).await.unwrap();
    sa.send("MessageReq",json!({"RequestId":30,"ReceiverIds":[b],"Message":format!("InvitePartyReq {}",json!({"RoomNo":room}))})).await;
    assert_eq!(sa.read().await.1["Result"],"Success");assert_eq!(sb.read().await.1["Type"],"InvitePartyReq");
    // Even a valid private invitation cannot bypass the saved unlock.
    let direct=crate::api::battle::execute_request(&state,if family=="raid" {"raid/join_raid_room"}else{"party_dungeon/join_party_dungeon_room"},form(&guest,&format!("RoomNo={room}"))).await.unwrap();assert_eq!(direct["Result"],"DungeonLocked");
    for (key,unlocked) in [(100,false),(101,true),(102,true)] {
        if unlocked && family=="party_dungeon" {
            let dungeon=if key==102 {3}else{2};
            crate::api::battle::put(&mut state.db.acquire().await.unwrap(),b,"dungeon",(1i64<<32)|dungeon,&json!({"ChapterIndex":1,"DungeonIndex":dungeon,"FirstRewardedDiff":2})).await.unwrap();
        }
        if unlocked {crate::api::battle::put(&mut state.db.acquire().await.unwrap(),b,"raid",1,&json!({"RaidIndex":1,"RaidLevel":if key==102 {9}else{8}})).await.unwrap();}
        sb.send("MessageReq",json!({"RequestId":key,"ReceiverIds":[a],"Message":format!("JoinPartyReq {}",json!({"RoomNo":room,"Accepted":true,"SendKey":key,"Invitee":{"AccountId":a,"RaidLevel":99}}))})).await;
        assert_eq!(sb.read().await.1["Result"],"Success");
        if unlocked {
            let notice=sa.read().await.1;assert_eq!(notice["Type"],"JoinPartyReq");
            let pending:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();assert_eq!(pending["Invitee"]["AccountId"],b);
            if key==101 {
                let changed=crate::api::battle::execute_request(&state,if family=="raid" {"raid/change_raid_room_info"}else{"party_dungeon/change_party_dungeon_room_info"},form(&host,&if family=="raid" {format!("RoomNo={room}&RaidLevel=9")}else{format!("RoomNo={room}&DungeonIndex=4")})).await.unwrap();assert_eq!(changed["Result"],"Success");
                assert_eq!(sa.read().await.1["Type"],if family=="raid" {"ChangePartyRoomInfo"}else{"ChangePartyDungeonRoomInfo"});
            }
            // Approval of a now-locked stage must precede all deck validation/mutation.
            let party=if key==101 {json!({"PartyMembers":[{"AccountId":a,"DeckHeros":{"999":{"HeroIndex":999}},"SubDeckHeros":{}}]})}else{Value::Null};
            sa.send("MessageReq",json!({"RequestId":key+10,"ReceiverIds":[b],"Message":format!("JoinPartyRes {}",json!({"Result":"Success","SendKey":key,"PartyInfo":party}))})).await;
            assert_eq!(sa.read().await.1["Result"],"Success");
        }
        let notice=sb.read().await.1;assert_eq!(notice["Type"],"JoinPartyRes");
        let response:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
        assert_eq!(response["SendKey"],key);
        assert_eq!(response["Result"],if key==102 {"Success"}else{"DungeonLocked"});
        if key!=102 {assert!(response["PartyInfo"].is_null());}
        let count:i64=sqlx::query_scalar("SELECT count(*) FROM battle_room_members WHERE room=?").bind(room).fetch_one(&state.db).await.unwrap();assert_eq!(count,if key==102 {2}else{1});
        let pending=crate::api::battle::get(&mut state.db.acquire().await.unwrap(),b,"party_pending",room).await.unwrap();assert!(pending.is_null());
        if key==101 {
            let notice=sa.read().await.1;assert_eq!(notice["Type"],"KickedRaidMemberRes");
            let kicked:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();assert_eq!(kicked["KickedAccountIds"],json!([b]));
        }
        if key!=102 {
            assert!(crate::api::battle::get(&mut state.db.acquire().await.unwrap(),b,"party_member",room).await.unwrap().is_null());
            assert!(crate::api::battle::get(&mut state.db.acquire().await.unwrap(),a,"party_member",room).await.unwrap().is_null());
            assert!(tokio::time::timeout(Duration::from_millis(50),sa.0.fill_buf()).await.is_err());
        } else {
            assert_eq!(response["PartyInfo"]["PartyMembers"].as_array().unwrap().len(),2);
            if family=="raid" {assert_eq!(response["PartyInfo"]["RaidLevel"],9);}else{assert_eq!(response["PartyInfo"]["DungeonIndex"],4);}
            let joined=crate::api::battle::execute_request(&state,if family=="raid" {"raid/join_raid_room"}else{"party_dungeon/join_party_dungeon_room"},form(&guest,&format!("RoomNo={room}"))).await.unwrap();assert_eq!(joined["Result"],"Success");
        }
    }
    let after:(i64,i64,i64)=sqlx::query_as("SELECT stamina,gold,gem FROM user_info WHERE account_id=?").bind(b).fetch_one(&state.db).await.unwrap();assert_eq!(before,after);
    server.abort();
    }
}

#[tokio::test]
async fn native_party_battle_callouts_echo_all_four_buttons_and_stay_inside_the_party() {
    for family in ["raid","party_dungeon"] {
        let (state,host,guest)=setup().await;let outsider=login(&state,"callout-outsider").await;
        let a=host.user_info.account_id;let b=guest.user_info.account_id;let c=outsider.user_info.account_id;
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
        let server=tokio::spawn(chat::serve(listener,state.clone()));
        let mut sa=Socket::connect(addr,&host,1).await;let mut sb=Socket::connect(addr,&guest,2).await;let mut sc=Socket::connect(addr,&outsider,1).await;
        sqlx::query("INSERT INTO battle_rooms(id,family,master,data,updated) VALUES(1,?,?,?,0)")
            .bind(family).bind(a).bind(json!({"RoomNo":1,"Status":2,"CurPartyMember":2}).to_string()).execute(&state.db).await.unwrap();
        for member in [a,b] {sqlx::query("INSERT INTO battle_room_members(room,account,joined,updated) VALUES(1,?,0,0)").bind(member).execute(&state.db).await.unwrap();}
        for (wire,expected) in [(json!("Attack"),"Attack"),(json!("Heal"),"Heal"),(json!("Cc"),"Cc"),(json!("Defense"),"Defense"),
            (json!(1),"Attack"),(json!("2"),"Heal"),(json!(3),"Cc"),(json!("4"),"Defense")] {
            sb.send("MessageReq",json!({"RequestId":20,"GroupType":"World","ReceiverIds":[c],"Message":format!("PartyMemberEmoti {}",json!({"Emoti":wire,"ChatType":"World","SenderName":"FORGED","SenderAvatarIndex":999,"SenderTeamLevel":999,"ReceiverId":c,"ReceiverName":"FORGED","LinkedItem":[{"EquipItemInfo":{"SlotIndex":999}}]}))})).await;
            assert_eq!(sb.read().await.1["Result"],"Success");
            let (name,echo)=sb.read().await;assert_eq!(name,"MessageNot");assert_eq!(echo["Type"],"PartyMemberEmoti");
            assert_eq!(echo["SenderId"],b);assert_eq!(echo["ReceiverIds"],json!([a,b]));assert_eq!(sa.read().await.1,echo);
            let body:Value=serde_json::from_str(echo["Content"].as_str().unwrap()).unwrap();
            assert_eq!(body,json!({"ChatType":"Party","Emoti":expected,"SenderName":guest.user_info.nick,"SenderAvatarIndex":guest.user_info.avatar_hero_index,"SenderTeamLevel":guest.user_info.team_level,"ReceiverId":0,"ReceiverName":""}));
            sc.send("PingReq",json!({"RequestId":21})).await;assert_eq!(sc.read().await.0,"PingRes","Outsiders must not receive callouts");
            // Simulate the native button cooldown without slowing the socket test.
            sqlx::query("UPDATE chat_messages SET created_at=datetime('now','-5 seconds') WHERE sender_id=?").bind(b).execute(&state.db).await.unwrap();
        }
        for invalid in [json!("None"),json!(0),json!(5),json!(-1),json!(1.5),json!("heal"),Value::Null] {
            sb.send("MessageReq",json!({"RequestId":22,"Message":format!("PartyMemberEmoti {}",json!({"Emoti":invalid}))})).await;
            assert_eq!(sb.read().await.1["Result"],"Fail");
        }
        sqlx::query("DELETE FROM battle_room_members WHERE account=?").bind(b).execute(&state.db).await.unwrap();
        sb.send("MessageReq",json!({"RequestId":23,"ReceiverIds":[a],"Message":"PartyMemberEmoti {\"Emoti\":\"Heal\"}"})).await;
        assert_eq!(sb.read().await.1["Result"],"Fail","Departed players cannot signal the old party");
        for request in 24..=27 {
            sa.send("MessageReq",json!({"RequestId":request,"ReceiverIds":[c],"Message":"PartyMemberEmoti {\"Emoti\":\"Defense\"}"})).await;
            assert_eq!(sa.read().await.1["Result"],if request==27 {"Fail"}else{"Success"});
            if request!=27 {assert_eq!(sa.read().await.1["ReceiverIds"],json!([a]),"A one-member party still receives local feedback");}
        }
        for socket in [&mut sb,&mut sc] {socket.send("PingReq",json!({"RequestId":28})).await;assert_eq!(socket.read().await.0,"PingRes");}
        let history=chat::get_recent_chats(State(state.clone()),form(&guest,"ChannelId=1")).await.unwrap().0;
        assert_eq!(history["WorldChats"],json!([]));assert_eq!(history["WhisperChats"],json!([]));assert_eq!(history["GuildChats"],json!([]));
        server.abort();
    }
}

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
        // Patched LoginReq proves identity with the game session.
        socket.send("LoginReq",json!({"RequestId":11,"SessionKey":a.user_info.session_key,"AccountId":a.user_info.account_id,"ChannelNo":channel,"FriendIds":[]})).await;
        let (name, value) = socket.read().await;
        assert_eq!(name, "LoginRes");
        assert_eq!(value["RequestId"], 11);
        assert_eq!(value["Result"], "Success");
        assert_eq!(value["ChannelNo"], channel.clamp(1, 99));
        socket
    }
}

#[tokio::test]
async fn native_socket_channel_limit_rejects_invalid_changes_and_keeps_membership() {
    let (state, a, b) = setup().await;
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    let server = tokio::spawn(chat::serve(listener, state.clone()));
    // Existing clients may reconnect with a channel from the previous range.
    let mut socket = Socket::connect(addr, &a, 1001).await;
    let mut other = Socket::connect(addr, &b, 99).await;
    for channel in [json!(1), json!("99")] {
        socket.send("ChangeChannelReq", json!({"RequestId":20,"ChannelNo":channel})).await;
        let (name, response) = socket.read().await;
        assert_eq!(name, "ChangeChannelRes");
        assert_eq!(response["Changed"], true);
    }
    for channel in [json!(0), json!(-1), json!(100), json!(9999), json!("100")] {
        socket.send("ChangeChannelReq", json!({"RequestId":21,"ChannelNo":channel})).await;
        assert_eq!(socket.read().await.1["Changed"], false);
    }
    other.send("MessageReq", json!({"RequestId":22,"GroupType":"Channel","Message":"ChannelChat {\"Chat\":\"still channel 99\"}"})).await;
    assert_eq!(other.read().await.0, "MessageRes");
    let own = other.read().await;
    assert_eq!(own.0, "MessageNot");
    assert_eq!(socket.read().await, own);
    let history = chat::get_recent_chats(State(state.clone()), form(&a, "ChannelId=100")).await.unwrap().0;
    assert_eq!(history["WorldChats"].as_array().unwrap().len(), 1);
    let _lower = Socket::connect(addr, &a, 0).await;
    server.abort();
}

#[tokio::test]
async fn native_socket_party_chat_delivers_to_both_members() {
    let (state,a,b)=setup().await;
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
    let server=tokio::spawn(chat::serve(listener,state.clone()));
    let mut sa=Socket::connect(addr,&a,1).await;let mut sb=Socket::connect(addr,&b,2).await;
    for client in [&mut sa,&mut sb] {client.send("PingReq",json!({"RequestId":1})).await;assert_eq!(client.read().await.0,"PingRes");}
    sqlx::query("INSERT INTO battle_rooms(id,family,master,data,updated) VALUES(1,'raid',?,'{}',0)").bind(a.user_info.account_id).execute(&state.db).await.unwrap();
    for account in [a.user_info.account_id,b.user_info.account_id] {sqlx::query("INSERT INTO battle_room_members(room,account,joined,updated) VALUES(1,?,0,0)").bind(account).execute(&state.db).await.unwrap();}
    sa.send("MessageReq",json!({"RequestId":2,"GroupType":"None","ReceiverIds":[b.user_info.account_id],"Message":"PartyChat {\"Chat\":\"native party message\"}"})).await;
    assert_eq!(sa.read().await.1["Result"],"Success");let own=sa.read().await;
    assert_eq!(own.0,"MessageNot");assert_eq!(own.1["Type"],"PartyChat");assert_eq!(sb.read().await,own);
    server.abort();
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
    assert!(a.user_info.avatar_hero_index > 0);
    assert_eq!(content["SenderAvatarIndex"], a.user_info.avatar_hero_index);
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
        &json!({"RequestId":8,"SessionKey":a.user_info.session_key,"AccountId":a.user_info.account_id}),
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

#[tokio::test]
async fn socket_requires_session_proof_and_accepts_native_string_ids() {
    let (state,a,b)=setup().await;
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
    let server=tokio::spawn(chat::serve(listener,state));
    for key in [None,Some(b.user_info.session_key.as_str())] {
        let mut socket=Socket(BufReader::new(TcpStream::connect(addr).await.unwrap()));
        socket.send("LoginReq",json!({"AccountId":a.user_info.account_id.to_string(),"RequestId":"2","SessionKey":key})).await;
        let response=socket.read().await.1;assert_eq!(response["Result"],"Fail");assert_eq!(response["RequestId"],2);
    }
    let mut socket=Socket(BufReader::new(TcpStream::connect(addr).await.unwrap()));
    socket.send("LoginReq",json!({"AccountId":a.user_info.account_id.to_string(),"RequestId":"3","SessionKey":a.user_info.session_key,"ChannelNo":"1"})).await;
    assert_eq!(socket.read().await.1["Result"],"Success");server.abort();
}
#[tokio::test]
async fn chat_equipment_links_use_owned_snapshot() {
    let (state,a,b)=setup().await;
    sqlx::query("INSERT INTO equip_items(account_id,slot_index,item_index) VALUES(?,50,1010101)").bind(a.user_info.account_id).execute(&state.db).await.unwrap();
    let link=urlencoding::encode(r#"[{"MaxEquipLevel":999,"EquipItemInfo":{"SlotIndex":"50","ItemIndex":99999,"EnchantLevel":999,"SoulWeaponInfo":{"Grade":2,"ReinforceLevel":20}}}]"#).into_owned();
    let v=chat::send_world_chat(State(state.clone()),form(&a,&format!("Chat=item&LinkedItem={link}"))).await.unwrap().0;
    let content:Value=serde_json::from_str(v["Message"]["Content"].as_str().unwrap()).unwrap();
    assert_eq!(content["LinkedItem"][0]["EquipItemInfo"]["ItemIndex"],1010101);
    assert_eq!(content["LinkedItem"][0]["MaxEquipLevel"],0);
    assert!(content["LinkedItem"][0]["EquipItemInfo"]["SoulWeaponInfo"].is_null());
    assert!(chat::send_world_chat(State(state.clone()),form(&b,&format!("Chat=item&LinkedItem={link}"))).await.is_err());
}

#[tokio::test]
async fn chat_equipment_links_include_sender_soul_weapon_snapshot() {
    let (state,a,b)=setup().await;
    sqlx::query("INSERT INTO equip_items(account_id,slot_index,item_index,star) VALUES(?,50,1001,5)")
        .bind(a.user_info.account_id).execute(&state.db).await.unwrap();
    let mut soul=json!({"EquipItemSlotIndex":50,"ItemIndex":1001,"Grade":2,"ReinforceLevel":20,
        "OptionRatio1":500,"OptionRatio2":500,"OptionBonusRatio1":100,"OptionBonusRatio2":200});
    {
        let mut db=state.db.acquire().await.unwrap();
        crate::api::extensions::put(&mut db,a.user_info.account_id,"soul",50,&soul).await.unwrap();
        // Even conflicting recipient-side extension state cannot replace the sender's soul.
        crate::api::extensions::put(&mut db,b.user_info.account_id,"soul",50,
            &json!({"EquipItemSlotIndex":50,"ItemIndex":1001,"Grade":0,"ReinforceLevel":0})).await.unwrap();
    }
    crate::api::extensions::refresh_soul_weapon_stats(&state,std::slice::from_mut(&mut soul)).unwrap();
    let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let addr=listener.local_addr().unwrap();
    let server=tokio::spawn(chat::serve(listener,state.clone()));
    let mut sender=Socket::connect(addr,&a,1).await;
    let mut receiver=Socket::connect(addr,&b,1).await;
    sender.send("MessageReq",json!({"RequestId":42,"GroupType":"Channel","Message":format!("ChannelChat {}",
        json!({"Chat":"shared UW","LinkedItem":[{"EquipItemInfo":{"SlotIndex":50,"SoulWeaponInfo":{"Grade":0}}}]}))})).await;
    assert_eq!(sender.read().await.1["Result"],"Success");
    let notice=receiver.read().await.1;
    let content:Value=serde_json::from_str(notice["Content"].as_str().unwrap()).unwrap();
    assert_eq!(content["LinkedItem"][0]["EquipItemInfo"]["SoulWeaponInfo"],soul);
    assert_eq!(content["LinkedItem"][0]["EquipItemInfo"]["Star"],5);
    let stored:String=sqlx::query_scalar("SELECT content FROM chat_messages WHERE sender_id=? ORDER BY message_id DESC LIMIT 1")
        .bind(a.user_info.account_id).fetch_one(&state.db).await.unwrap();
    assert_eq!(serde_json::from_str::<Value>(&stored).unwrap()["LinkedItem"],content["LinkedItem"]);
    server.abort();
}

#[tokio::test]
async fn retired_hero_rewards_are_hidden_and_skipped_in_old_mail() {
    let (state, a, _) = setup().await;
    let id = add_mail(&state, &a, 123, 7, None).await;
    // Simulate mail queued by an older server, bypassing send-time filtering.
    sqlx::query("UPDATE mails SET reward_items=? WHERE mail_id=?")
        .bind(json!([{"ItemIndex":9001001,"ItemCount":180},
            {"ItemIndex":30005,"ItemCount":5},{"ItemIndex":91006,"ItemCount":1},
            {"ItemIndex":45045,"ItemCount":1}]).to_string())
        .bind(id).execute(&state.db).await.unwrap();
    let list = mail::get_mail_list(State(state.clone()), form(&a, "MaxCount=50")).await.unwrap().0;
    let info = list["MailInfos"].as_array().unwrap().iter().find(|v|v["MailIndex"]==id).unwrap();
    assert_eq!(info["ItemInfos"],json!([{"ItemIndex":45045,"ItemCount":1}]));
    let claimed = mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}"))).await.unwrap().0;
    assert_eq!(claimed["Result"],"Success");
    assert_eq!(claimed["MailReceiveResult"]["ItemResults"].as_array().unwrap().len(),1);
    let blocked:i64=sqlx::query_scalar("SELECT COUNT(*) FROM items WHERE account_id=? AND item_index IN (9001001,30005,91006)")
        .bind(a.user_info.account_id).fetch_one(&state.db).await.unwrap();
    assert_eq!(blocked,0);
    assert_eq!(mail::receive_mail(State(state.clone()), form(&a, &format!("MailIndex={id}"))).await.unwrap().0["Result"],"Fail");
}
