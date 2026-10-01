//! Native PartyManager messages carried by the authenticated message socket.
use super::*;

pub(crate) fn supported(name: &str) -> bool {
    matches!(
        name,
        "JoinPartyReq"
            | "JoinPartyRes"
            | "JoinPartyNotice"
            | "PartyDeckHeroInfo"
            | "PartyRoomReady"
            | "PartyRoomRepeat"
            | "ChangePartyRoomMaster"
            | "ChangePartyRoomInfo"
            | "LeaveParty"
    )
}
fn number(v: &Value, key: &str) -> i64 {
    v[key]
        .as_i64()
        .or_else(|| v[key].as_str().and_then(|v| v.parse().ok()))
        .unwrap_or(0)
}
async fn membership(db: &mut SqliteConnection, a: i64) -> Result<(i64, String, Value)> {
    let row:Option<(i64,String,String)>=sqlx::query_as("SELECT r.id,r.family,r.data FROM battle_rooms r JOIN battle_room_members m ON m.room=r.id WHERE m.account=?")
        .bind(a).fetch_optional(db).await?;
    let (id, family, data) = row.ok_or_else(|| rule("RoomNotExist"))?;
    Ok((id, family, read_json(&data)?))
}
async fn ids_in(db: &mut SqliteConnection, id: i64) -> Result<Vec<i64>> {
    Ok(sqlx::query_scalar(
        "SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account",
    )
    .bind(id)
    .fetch_all(db)
    .await?)
}
async fn member(db: &mut SqliteConnection, a: i64, room: &Value, index: usize) -> Result<Value> {
    let user=sqlx::query("SELECT a.nick,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id=?").bind(a).fetch_one(&mut *db).await?;
    let mut v = get(db, a, "party_member", n(room, "RoomNo")).await?;
    if v.is_null() {
        v = json!({"DeckHeros":{},"SubDeckHeros":{},"IsBattleReady":false});
    }
    v["AccountId"] = json!(a);
    v["Nick"] = json!(user.get::<String, _>("nick"));
    v["TeamLevel"] = json!(user.get::<i64, _>("team_level"));
    v["AvatarHeroIndex"] = json!(user.get::<i64, _>("avatar_hero_index"));
    v["IsMaster"] = json!(n(room, "MasterAccountId") == a);
    v["MemberIndex"] = json!(index);
    v["ChapterIndex"] = room["ChapterIndex"].clone();
    v["DungeonIndex"] = room["DungeonIndex"].clone();
    v["SelectedRewardItemNo"] = json!(-1);
    v["Status"] = json!("PartyRoom");
    Ok(v)
}
async fn snapshot(db: &mut SqliteConnection, room: &Value) -> Result<Value> {
    let mut members = vec![];
    for (index, a) in ids_in(db, n(room, "RoomNo")).await?.into_iter().enumerate() {
        members.push(member(db, a, room, index).await?);
    }
    Ok(
        json!({"PartyRoom":room,"RaidIndex":room["RaidIndex"],"RaidLevel":room["RaidLevel"],"ChapterIndex":room["ChapterIndex"],"DungeonIndex":room["DungeonIndex"],"Difficulty":room["DungeonDifficulty"],"PartyMembers":members,"RewardItems":[],"Bannedids":[],"IsRepeatBattle":room["IsRepeatBattle"].as_bool().unwrap_or(false),"CreatedTime":time(now()),"MasterServerPrivateIp":"","MasterServerPort":0}),
    )
}

pub(crate) async fn send(
    s: &AppState,
    a: i64,
    target: i64,
    protocol: &str,
    mut content: Value,
) -> Result<()> {
    if !supported(protocol) || !content.is_object() {
        return Err(rule("InvalidMessage"));
    }
    let mut db = s.db.begin().await?;
    // Obtain the SQLite writer before reading membership/decks, avoiding lost updates.
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(a)
        .execute(&mut *db)
        .await?;
    let mut messages: Vec<(i64, i64, String, Value)> = vec![];
    if protocol == "JoinPartyReq" {
        let id = number(&content, "RoomNo");
        let raw: Option<String> = sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=?")
            .bind(id)
            .fetch_optional(&mut *db)
            .await?;
        let room: Value = read_json(&raw.ok_or_else(|| rule("RoomNotExist"))?)?;
        if n(&room, "MasterAccountId") != target
            || target == a
            || n(&room, "Status") != 0
            || n(&room, "Opened") != 1
            || content["Accepted"] != true
        {
            return Err(rule("InvalidRoomInfo"));
        }
        let members = ids_in(&mut db, id).await?;
        let current: Option<i64> =
            sqlx::query_scalar("SELECT room FROM battle_room_members WHERE account=?")
                .bind(a)
                .fetch_optional(&mut *db)
                .await?;
        if current.is_some_and(|room| room != id) {
            return Err(rule("AlreadyJoined"));
        }
        if members.len() as i64 >= n(&room, "Capacity") && !members.contains(&a) {
            return Err(rule("InvalidRoomInfo"));
        }
        let send_key = number(&content, "SendKey");
        if send_key <= 0 {
            return Err(rule("InvalidMessage"));
        }
        content = json!({"Invitee":member(&mut db,a,&room,members.len()).await?,"Accepted":true,"RoomNo":id,"Result":"Success","SendKey":send_key});
        put(
            &mut db,
            a,
            "party_pending",
            id,
            &json!({"Master":target,"SendKey":send_key,"Time":now()}),
        )
        .await?;
        messages.push((target, a, protocol.into(), content));
    } else {
        let (id, family, mut room) = membership(&mut db, a).await?;
        let master = n(&room, "MasterAccountId");
        let mut recipients = ids_in(&mut db, id).await?;
        if protocol == "JoinPartyRes" {
            let pending = get(&mut db, target, "party_pending", id).await?;
            if a != master
                || n(&pending, "Master") != a
                || now() - n(&pending, "Time") > 60
                || n(&pending, "SendKey") != number(&content, "SendKey")
            {
                return Err(rule("InvalidMessage"));
            }
            let accepted = content["Result"] == "Success" || content["Result"] == json!(1);
            if accepted {
                // A host can select heroes while alone: SendToAllMembers sends no
                // packet until another member exists. Recover only the host's
                // owned selection from its join response, never its supplied stats.
                if let Some(host) = content["PartyInfo"]["PartyMembers"]
                    .as_array()
                    .and_then(|members| members.iter().find(|v| number(v, "AccountId") == a))
                {
                    let def = if family == "raid" {
                        row(
                            s,
                            "Raid",
                            &[
                                ("Index", n(&room, "RaidIndex")),
                                ("Level", n(&room, "RaidLevel")),
                            ],
                        )?
                    } else {
                        row(
                            s,
                            "PartyDungeon",
                            &[
                                ("ChapterIndex", n(&room, "ChapterIndex")),
                                ("DungeonIndex", n(&room, "DungeonIndex")),
                            ],
                        )?
                    };
                    let mut host_state = member(&mut db, a, &room, 0).await?;
                    let mut selected = BTreeSet::new();
                    for field in ["DeckHeros", "SubDeckHeros"] {
                        let deck = host[field]
                            .as_object()
                            .ok_or_else(|| rule("InvalidMessage"))?;
                        let total_max = if field == "DeckHeros" {
                            n(def, "DeckCount").max(n(def, "MainPartyCount"))
                        } else {
                            n(def, "SubPartyCount")
                        };
                        let cap = n(def, "DeckCountPerPlayer").min(total_max);
                        let mut total = deck.len() as i64;
                        for other in &recipients {
                            if *other != a {
                                total += get(&mut db, *other, "party_member", id).await?[field]
                                    .as_object()
                                    .map_or(0, |deck| deck.len())
                                    as i64;
                            }
                        }
                        if deck.len() as i64 > cap || total > total_max {
                            return Err(rule("NotMatchHeroIndices"));
                        }
                        host_state[field] = json!({});
                        for value in deck.values() {
                            let hero = number(value, "HeroIndex");
                            if hero <= 0 || hero > i32::MAX as i64 || !selected.insert(hero) {
                                return Err(rule("DuplicatedHero"));
                            }
                            for other in &recipients {
                                if *other != a
                                    && get(&mut db, *other, "party_member", id).await?[field]
                                        [hero.to_string()]
                                    .is_object()
                                {
                                    return Err(rule("DuplicatedHero"));
                                }
                            }
                            let owned = special::cached_hero(&mut db, a, hero as i32).await?;
                            if n(&owned, "Level") < n(def, "ReqHeroLevel") {
                                return Err(rule("NotAvailableHero"));
                            }
                            host_state[field][hero.to_string()] = owned;
                        }
                    }
                    host_state["IsBattleReady"] = json!(false);
                    put(&mut db, a, "party_member", id, &host_state).await?;
                }
                room = rooms::join(&mut db, target, id, &family).await?;
                recipients = ids_in(&mut db, id).await?;
                content = json!({"Result":"Success","SendKey":pending["SendKey"],"MemberIndex":recipients.iter().position(|v|*v==target).unwrap_or(0),"PartyInfo":snapshot(&mut db,&room).await?});
            } else {
                content =
                    json!({"Result":"InvalidParty","SendKey":pending["SendKey"],"PartyInfo":null});
            }
            sqlx::query(
                "DELETE FROM battle_state WHERE account=? AND kind='party_pending' AND idx=?",
            )
            .bind(target)
            .bind(id)
            .execute(&mut *db)
            .await?;
            messages.push((target, a, protocol.into(), content.clone()));
            if accepted {
                for receiver in recipients.into_iter().filter(|v| *v != a && *v != target) {
                    messages.push((
                        receiver,
                        a,
                        "JoinPartyNotice".into(),
                        json!({"PartyInfo":content["PartyInfo"]}),
                    ));
                }
            }
        } else {
            if n(&room, "Status") != 0 {
                return Err(rule("InvalidRoomInfo"));
            }
            let mut state = member(
                &mut db,
                a,
                &room,
                recipients.iter().position(|v| *v == a).unwrap_or(0),
            )
            .await?;
            match protocol {
                "PartyDeckHeroInfo" => {
                    let hero = number(&content, "HeroIndex");
                    let deck = number(&content, "DeckIndex");
                    if hero <= 0 || hero > i32::MAX as i64 || !(0..=1).contains(&deck) {
                        return Err(rule("InvalidMessage"));
                    }
                    let field = if deck == 0 {
                        "DeckHeros"
                    } else {
                        "SubDeckHeros"
                    };
                    let other = if deck == 0 {
                        "SubDeckHeros"
                    } else {
                        "DeckHeros"
                    };
                    let remove = content["HeroInfo"].is_null()
                        || number(&content["HeroInfo"], "HeroIndex") == 0;
                    let key = hero.to_string();
                    if remove {
                        state[field].as_object_mut().unwrap().remove(&key);
                        content = json!({"HeroIndex":hero,"HeroInfo":null,"DeckIndex":deck});
                    } else {
                        let def = if family == "raid" {
                            row(
                                s,
                                "Raid",
                                &[
                                    ("Index", n(&room, "RaidIndex")),
                                    ("Level", n(&room, "RaidLevel")),
                                ],
                            )?
                        } else {
                            row(
                                s,
                                "PartyDungeon",
                                &[
                                    ("ChapterIndex", n(&room, "ChapterIndex")),
                                    ("DungeonIndex", n(&room, "DungeonIndex")),
                                ],
                            )?
                        };
                        let total_max = if deck == 0 {
                            n(def, "MainPartyCount").max(n(def, "DeckCount"))
                        } else {
                            n(def, "SubPartyCount")
                        };
                        let max = n(def, "DeckCountPerPlayer").min(total_max);
                        if (!state[field][&key].is_object()
                            && state[field].as_object().unwrap().len() as i64 >= max)
                            || state[other][&key].is_object()
                        {
                            return Err(rule("NotMatchHeroIndices"));
                        }
                        let mut total = state[field].as_object().unwrap().len() as i64;
                        for other_id in &recipients {
                            if *other_id != a {
                                let other_state =
                                    get(&mut db, *other_id, "party_member", id).await?;
                                if other_state[field][&key].is_object() {
                                    return Err(rule("DuplicatedHero"));
                                }
                                total +=
                                    other_state[field].as_object().map_or(0, |v| v.len()) as i64;
                            }
                        }
                        if !state[field][&key].is_object() && total >= total_max {
                            return Err(rule("NotMatchHeroIndices"));
                        }
                        let owned = special::cached_hero(&mut db, a, hero as i32).await?;
                        if n(&owned, "Level") < n(def, "ReqHeroLevel") {
                            return Err(rule("NotAvailableHero"));
                        }
                        state[field][&key] = owned.clone();
                        content = json!({"HeroIndex":hero,"HeroInfo":owned,"DeckIndex":deck});
                    }
                    state["IsBattleReady"] = json!(false);
                    for receiver in &recipients {
                        if *receiver != a {
                            messages.push((
                                *receiver,
                                a,
                                "PartyRoomReady".into(),
                                json!({"IsReady":false}),
                            ));
                        }
                    }
                    put(&mut db, a, "party_member", id, &state).await?;
                }
                "PartyRoomReady" => {
                    let ready = content["IsReady"]
                        .as_bool()
                        .ok_or_else(|| rule("InvalidMessage"))?;
                    if ready && state["DeckHeros"].as_object().unwrap().is_empty() {
                        return Err(rule("EmptyHeroIndices"));
                    }
                    state["IsBattleReady"] = json!(ready);
                    put(&mut db, a, "party_member", id, &state).await?;
                    content = json!({"IsReady":ready});
                }
                "JoinPartyNotice" => {
                    if a != master {
                        return Err(rule("WrongMember"));
                    }
                    content = json!({"PartyInfo":snapshot(&mut db,&room).await?});
                }
                "PartyRoomRepeat" => {
                    if a != master {
                        return Err(rule("WrongMember"));
                    }
                    let repeat = content["IsRepeat"]
                        .as_bool()
                        .ok_or_else(|| rule("InvalidMessage"))?;
                    room["IsRepeatBattle"] = json!(repeat);
                    sqlx::query("UPDATE battle_rooms SET data=? WHERE id=?")
                        .bind(room.to_string())
                        .bind(id)
                        .execute(&mut *db)
                        .await?;
                    content = json!({"IsRepeat":repeat,"Result":"NONE"});
                }
                "ChangePartyRoomMaster" => {
                    if master != number(&content, "NewMasterAccountId") {
                        return Err(rule("WrongMember"));
                    }
                    content = json!({"NewMasterAccountId":master});
                }
                "ChangePartyRoomInfo" => {
                    if a != master {
                        return Err(rule("WrongMember"));
                    }
                    content = json!({"RaidLevel":room["RaidLevel"],"GoalIndex":room["GoalIndex"],"AffixIndices":room["AffixIndices"],"IsViolateBanRule":false});
                }
                "LeaveParty" => {
                    let req = Request([("RoomNo".into(), id.to_string())].into_iter().collect());
                    rooms::execute(
                        &mut db,
                        s,
                        a,
                        &req,
                        &format!(
                            "{family}/leave_{}_room",
                            if family == "raid" {
                                "raid"
                            } else {
                                "party_dungeon"
                            }
                        ),
                    )
                    .await?;
                    content = json!({});
                }
                _ => return Err(rule("InvalidMessage")),
            }
            sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?")
                .bind(now())
                .bind(a)
                .execute(&mut *db)
                .await?;
            for receiver in recipients.into_iter().filter(|v| *v != a) {
                messages.push((receiver, a, protocol.into(), content.clone()));
            }
        }
    }
    db.commit().await?;
    for (receiver, sender, name, body) in messages {
        s.chat.notify(receiver, sender, &name, body);
    }
    Ok(())
}

pub(crate) async fn reconnect(s: &AppState, a: i64) -> Result<()> {
    let mut db = s.db.begin().await?;
    let Ok((id, _, room)) = membership(&mut db, a).await else {
        return Ok(());
    };
    let info = snapshot(&mut db, &room).await?;
    let members = ids_in(&mut db, id).await?;
    sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?")
        .bind(now())
        .bind(a)
        .execute(&mut *db)
        .await?;
    db.commit().await?;
    s.chat.notify(
        a,
        n(&room, "MasterAccountId"),
        "JoinPartyNotice",
        json!({"PartyInfo":info}),
    );
    for m in info["PartyMembers"].as_array().into_iter().flatten() {
        let sender = n(m, "AccountId");
        if sender == a {
            continue;
        }
        for (deck, field) in [(0, "DeckHeros"), (1, "SubDeckHeros")] {
            for hero in m[field].as_object().into_iter().flat_map(|m| m.values()) {
                s.chat.notify(
                    a,
                    sender,
                    "PartyDeckHeroInfo",
                    json!({"HeroIndex":hero["HeroIndex"],"HeroInfo":hero,"DeckIndex":deck}),
                );
            }
        }
        if members.contains(&sender) {
            s.chat.notify(
                a,
                sender,
                "PartyRoomReady",
                json!({"IsReady":m["IsBattleReady"]}),
            );
        }
    }
    Ok(())
}

pub(crate) async fn after_room_request(
    s: &AppState,
    a: i64,
    r: &Request,
    action: &str,
) -> Result<()> {
    if action.starts_with("ping_") || action.starts_with("search_") || action.starts_with("create_")
    {
        return Ok(());
    }
    let id = int(r, "RoomNo")?;
    if id <= 0 {
        return Ok(());
    }
    let mut db = s.db.begin().await?;
    let raw: Option<String> = sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=?")
        .bind(id)
        .fetch_optional(&mut *db)
        .await?;
    let Some(raw) = raw else {
        return Ok(());
    };
    let room: Value = read_json(&raw)?;
    let ids = ids_in(&mut db, id).await?;
    let info = snapshot(&mut db, &room).await?;
    db.commit().await?;
    if action.starts_with("leave_") {
        let target = r.number("AccountId", a)?;
        let target = if target == 0 { a } else { target };
        if target != a && !ids.contains(&target) {
            s.chat.notify(target,a,"KickedRaidMemberRes",json!({"KickedAccountIds":[target],"ReplaceMasterAccountId":room["MasterAccountId"]}));
        }
    }
    for receiver in ids {
        if action.starts_with("leave_") {
            let target = r.number("AccountId", a)?;
            let target = if target == 0 { a } else { target };
            s.chat.notify(receiver,a,"KickedRaidMemberRes",json!({"KickedAccountIds":[target],"ReplaceMasterAccountId":room["MasterAccountId"]}));
        }
        if action.starts_with("delegate_") || action.starts_with("leave_") {
            s.chat.notify(
                receiver,
                a,
                "ChangePartyRoomMaster",
                json!({"NewMasterAccountId":room["MasterAccountId"]}),
            );
        }
        s.chat.notify(
            receiver,
            n(&room, "MasterAccountId"),
            "JoinPartyNotice",
            json!({"PartyInfo":info}),
        );
    }
    Ok(())
}

pub(crate) async fn disconnected(s: &AppState, a: i64) -> Result<()> {
    let mut db = s.db.begin().await?;
    let Ok((id, _, room)) = membership(&mut db, a).await else {
        return Ok(());
    };
    let mut state = member(&mut db, a, &room, 0).await?;
    state["IsBattleReady"] = json!(false);
    put(&mut db, a, "party_member", id, &state).await?;
    let ids = ids_in(&mut db, id).await?;
    db.commit().await?;
    for receiver in ids.into_iter().filter(|id| *id != a) {
        s.chat
            .notify(receiver, a, "PartyRoomReady", json!({"IsReady":false}));
    }
    Ok(())
}
