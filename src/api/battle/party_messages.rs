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
            | "ChangePartyDungeonRoomInfo"
            | "LeaveParty"
            | "PartyBattleStart"
            | "ChangePartyMemberStatus"
            | "PartyMemberPing"
    )
}
fn number(v: &Value, key: &str) -> i64 {
    v[key]
        .as_i64()
        .or_else(|| v[key].as_str().and_then(|v| v.parse().ok()))
        .unwrap_or(0)
}
fn settings_notice(room:&Value,raid:bool,reset:bool)->(&'static str,Value) {
    if raid {
        ("ChangePartyRoomInfo",json!({"RaidLevel":room["RaidLevel"],"GoalIndex":room["GoalIndex"],"AffixIndices":room["AffixIndices"],"IsViolateBanRule":false}))
    }else{
        ("ChangePartyDungeonRoomInfo",json!({"ChapterIndex":room["ChapterIndex"],"DungeonIndex":room["DungeonIndex"],
            "Difficulty":n(room,"DungeonDifficulty").to_string(),"DungeonType":n(room,"DungeonType").to_string(),"IsResetSquard":reset}))
    }
}
fn boolean(v: &Value, key: &str) -> Result<bool> {
    match &v[key] {
        Value::Bool(value) => Ok(*value),
        Value::String(value) if value == "True" || value == "true" => Ok(true),
        Value::String(value) if value == "False" || value == "false" => Ok(false),
        _ => Err(rule("InvalidMessage")),
    }
}

/// Invite All uses Friend/Guild groups with no receiver IDs. Resolve those
/// groups from persisted membership, and build the invite from the host's room.
pub(crate) async fn invite(s: &AppState, a: i64, group: &Value, targets: &Value) -> Result<()> {
    let mut db = s.db.begin().await?;
    let (id, family, room) = membership(&mut db, a).await?;
    let members = ids_in(&mut db, id).await?;
    if n(&room, "MasterAccountId") != a || n(&room, "Status") != 0
        || members.len() as i64 >= n(&room, "Capacity") {
        return Err(rule("InvalidRoomInfo"));
    }
    let direct = group.is_null() || group == "None" || group == 0 || group == "0";
    let recipients: Vec<i64> = if direct {
        targets.as_array().filter(|v| !v.is_empty() && v.len() <= 50)
            .ok_or_else(|| rule("InvalidMessage"))?.iter()
            .map(|v| v.as_i64().or_else(|| v.as_str().and_then(|v| v.parse().ok()))
                .filter(|id| *id > 0).ok_or_else(|| rule("InvalidMessage")))
            .collect::<Result<_>>()?
    } else if group == "Friend" || group == 4 || group == "4" {
        sqlx::query_scalar("SELECT CASE WHEN account_id=? THEN friend_account_id ELSE account_id END FROM friends WHERE status='accepted' AND (account_id=? OR friend_account_id=?)")
            .bind(a).bind(a).bind(a).fetch_all(&mut *db).await?
    } else if group == "Guild" || group == 3 || group == "3" {
        sqlx::query_scalar("SELECT account_id FROM guild_members WHERE guild_id=(SELECT guild_id FROM guild_members WHERE account_id=?) AND account_id!=?")
            .bind(a).bind(a).fetch_all(&mut *db).await?
    } else { return Err(rule("InvalidMessage")); };
    let nick: String = sqlx::query_scalar("SELECT nick FROM accounts WHERE account_id=?")
        .bind(a).fetch_one(&mut *db).await?;
    let content = json!({"InvitorName":nick,"RoomNo":id,"RaidIndex":if family=="raid" {room["RaidIndex"].clone()} else {json!(0)},
        "RaidLevel":if family=="raid" {room["RaidLevel"].clone()} else {json!(0)},"BannedIds":[],"IsSendOne":direct,
        "dungeonType":n(&room,"DungeonType").to_string(),"chapterIndex":room["ChapterIndex"],"dungeonIndex":room["DungeonIndex"],
        "difficulty":n(&room,"DungeonDifficulty").to_string(),"AffixIndices":room["AffixIndices"]});
    let recipients: BTreeSet<_> = recipients.into_iter().filter(|target| *target != a && !members.contains(target)).collect();
    for target in &recipients {
        put(&mut db, *target, "party_invite", id, &json!({"Master":a,"Time":now()})).await?;
    }
    db.commit().await?;
    for target in recipients {
        s.chat.notify(target, a, "InvitePartyReq", content.clone());
    }
    Ok(())
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
    if !v["Status"].is_string() { v["Status"] = json!("PartyRoom"); }
    Ok(v)
}
async fn snapshot(db: &mut SqliteConnection, room: &Value) -> Result<Value> {
    let mut members = vec![];
    for (index, a) in ids_in(db, n(room, "RoomNo")).await?.into_iter().enumerate() {
        members.push(member(db, a, room, index).await?);
    }
    Ok(
        json!({"PartyRoom":rooms::wire_room(room),"RaidIndex":room["RaidIndex"],"RaidLevel":room["RaidLevel"],"ChapterIndex":room["ChapterIndex"],"DungeonIndex":room["DungeonIndex"],"Difficulty":n(room,"DungeonDifficulty").to_string(),"PartyMembers":members,"RewardItems":[],"Bannedids":[],"IsRepeatBattle":room["IsRepeatBattle"].as_bool().unwrap_or(false),"CreatedTime":time(now()),"MasterServerPrivateIp":"","MasterServerPort":0}),
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
        // Native declines omit RoomNo and Invitee; there is no join to authorize.
        if !boolean(&content, "Accepted")? { return Ok(()); }
        let id = number(&content, "RoomNo");
        let raw: Option<(String,String)> = sqlx::query_as("SELECT family,data FROM battle_rooms WHERE id=?")
            .bind(id)
            .fetch_optional(&mut *db)
            .await?;
        let (family,raw)=raw.ok_or_else(||rule("RoomNotExist"))?;
        let room: Value = read_json(&raw)?;
        let invitation = get(&mut db, a, "party_invite", id).await?;
        let invited = n(&invitation, "Master") == target && now() - n(&invitation, "Time") <= 120;
        if n(&room, "MasterAccountId") != target
            || target == a
            || n(&room, "Status") != 0
            || (n(&room, "Opened") != 1 && !invited)
            || !boolean(&content, "Accepted")?
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
        if let Err(error)=rooms::require_join_unlock(&mut db,s,a,&room,&family).await {
            if !matches!(error,ServerError::InvalidRequest(_)) {return Err(error);}
            // A real JoinPartyRes clears the guest's native pending join and
            // waiting popup. A bare MessageRes failure would leave it waiting.
            sqlx::query("DELETE FROM battle_state WHERE account=? AND kind='party_pending' AND idx=?")
                .bind(a).bind(id).execute(&mut *db).await?;
            db.commit().await?;
            s.chat.notify(a,target,"JoinPartyRes",json!({"Result":"DungeonLocked","SendKey":send_key,"PartyInfo":null}));
            return Ok(());
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
            let mut accepted = content["Result"] == "Success" || content["Result"] == json!(1);
            let mut locked=false;
            if accepted {
                if let Err(error)=rooms::require_join_unlock(&mut db,s,target,&room,&family).await {
                    if !matches!(error,ServerError::InvalidRequest(_)) {return Err(error);}
                    accepted=false;locked=true;
                }
            }
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
                            if family=="raid" && n(def,"Type")==16 {n(def,"MainPartyCount")} else {n(def, "DeckCount").max(n(def, "MainPartyCount"))}
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
                    if family=="raid" && n(def,"Type")==16 && selected.len() as i64>n(def,"DeckCountPerPlayer") {
                        return Err(rule("NotMatchHeroIndices"));
                    }
                    host_state["IsBattleReady"] = json!(false);
                    put(&mut db, a, "party_member", id, &host_state).await?;
                }
                room = rooms::join(&mut db, s, target, id, &family).await?;
                recipients = ids_in(&mut db, id).await?;
                content = json!({"Result":"Success","SendKey":pending["SendKey"],"MemberIndex":recipients.iter().position(|v|*v==target).unwrap_or(0),"PartyInfo":snapshot(&mut db,&room).await?});
            } else {
                content =
                    json!({"Result":if locked {"DungeonLocked"}else{"InvalidParty"},"SendKey":pending["SendKey"],"PartyInfo":null});
            }
            sqlx::query(
                "DELETE FROM battle_state WHERE account=? AND kind='party_pending' AND idx=?",
            )
            .bind(target)
            .bind(id)
            .execute(&mut *db)
            .await?;
            messages.push((target, a, protocol.into(), content.clone()));
            if locked {
                // The native host optimistically added the guest before its
                // response reached us. Remove that uncommitted UI member too.
                for receiver in &recipients {
                    messages.push((*receiver,a,"KickedRaidMemberRes".into(),json!({"KickedAccountIds":[target],"ReplaceMasterAccountId":master})));
                }
            }
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
        } else if protocol=="PartyBattleStart" {
            content=super::cooperative::party_start(&mut db,s,a,&room).await?;
            for receiver in recipients.into_iter().filter(|v|*v!=a) {
                messages.push((receiver,a,protocol.into(),content.clone()));
            }
        } else if protocol == "PartyMemberPing" {
            if target != master { return Err(rule("WrongMember")); }
            sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?")
                .bind(now()).bind(a).execute(&mut *db).await?;
        } else if protocol == "ChangePartyMemberStatus" {
            let status = match content["Status"].as_str() {
                Some("PartyRoom" | "0") => "PartyRoom",
                Some("Battle" | "1") => "Battle",
                Some("EndCampaign" | "2") => "EndCampaign",
                _ => match content["Status"].as_i64() {
                    Some(0) => "PartyRoom", Some(1) => "Battle", Some(2) => "EndCampaign",
                    _ => return Err(rule("InvalidMessage")),
                },
            };
            let complete: bool = sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=1)")
                .bind(a).fetch_one(&mut *db).await?;
            if status == "Battle" && n(&room,"Status") != 2 || status == "EndCampaign" && !complete {
                return Err(rule("InvalidRoomInfo"));
            }
            let mut state = member(&mut db,a,&room,recipients.iter().position(|v|*v==a).unwrap_or(0)).await?;
            state["Status"] = json!(status); put(&mut db,a,"party_member",id,&state).await?;
            sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?")
                .bind(now()).bind(a).execute(&mut *db).await?;
            for receiver in recipients.into_iter().filter(|v|*v != a) {
                messages.push((receiver,a,protocol.into(),json!({"Status":status})));
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
                            if family=="raid" && n(def,"Type")==16 {n(def,"MainPartyCount")} else {n(def, "MainPartyCount").max(n(def, "DeckCount"))}
                        } else {
                            n(def, "SubPartyCount")
                        };
                        let max = n(def, "DeckCountPerPlayer").min(total_max);
                        let own_total=state[field].as_object().unwrap().len()+state[other].as_object().unwrap().len();
                        if family=="raid" && n(def,"Type")==16 && !state[field][&key].is_object()
                            && own_total as i64>=n(def,"DeckCountPerPlayer") {return Err(rule("NotMatchHeroIndices"));}
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
                    let ready = boolean(&content, "IsReady")?;
                    let second_team=family=="raid" && !state["SubDeckHeros"].as_object().unwrap().is_empty()
                        && n(row(s,"Raid",&[("Index",n(&room,"RaidIndex")),("Level",n(&room,"RaidLevel"))])?,"Type")==16;
                    if ready && state["DeckHeros"].as_object().unwrap().is_empty() && !second_team {
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
                    let repeat = boolean(&content, "IsRepeat")?;
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
                    content = settings_notice(&room,true,false).1;
                }
                "ChangePartyDungeonRoomInfo" => {
                    if a != master || family == "raid" { return Err(rule("WrongMember")); }
                    let reset=boolean(&content,"IsResetSquard")?;
                    if reset {
                        for (index, account) in recipients.iter().enumerate() {
                            let mut state=member(&mut db,*account,&room,index).await?;
                            state["DeckHeros"]=json!({});state["SubDeckHeros"]=json!({});state["IsBattleReady"]=json!(false);
                            put(&mut db,*account,"party_member",id,&state).await?;
                        }
                    }
                    content=settings_notice(&room,false,reset).1;
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

/// Native story-party Loot reads MemberInfo populated by MemberResultsRes,
/// rather than the personal EndCampaign response. These are display summaries
/// of committed rewards, never an additional award or a player-supplied report.
pub(crate) async fn replay_rewards(s: &AppState, a: i64) -> Result<()> {
    let mut db=s.db.acquire().await?;
    let own:Option<(i64,String)>=sqlx::query_as("SELECT m.room,r.entry FROM battle_room_members m JOIN battle_runs r ON r.account=m.account WHERE m.account=? AND r.completed=1")
        .bind(a).fetch_optional(&mut *db).await?;
    let Some((room,raw))=own else {return Ok(());};
    let entry:Value=read_json(&raw)?;
    if entry["PartyCampaign"]!=true || number(&entry,"CoopRoom")!=room || !entry["PartyEndResponse"].is_object() {return Ok(());}
    let Some(run)=entry["CoopRunId"].as_str() else {return Ok(());};
    let rows:Vec<(i64,String)>=sqlx::query_as("SELECT r.account,r.entry FROM battle_runs r JOIN battle_room_members m ON m.account=r.account WHERE m.room=? AND r.completed=1 AND json_extract(r.entry,'$.CoopRunId')=? AND json_extract(r.entry,'$.CoopRoom')=?")
        .bind(room).bind(run).bind(room).fetch_all(&mut *db).await?;
    drop(db);
    for (sender,raw) in rows {
        let entry:Value=read_json(&raw)?;
        let out=&entry["PartyEndResponse"];
        if !out.is_object() {continue;}
        let use_stamina=entry["PartyUseStamina"].as_bool()
            .unwrap_or_else(||out["StaminaResult"]["AddValue"].as_i64().unwrap_or(0)<0);
        let mut summary=json!({"UseStamina":use_stamina});
        for field in ["HeroExpResults","FlaskResults","FlaskItemResults","CurrencyResults","ItemResults","EquipItemInfos","EquipItemBoostInfos",
            "CurrencyResults2","ItemResults2","EquipItemInfos2","EquipItemBoostInfos2"] {
            summary[field]=out[field].as_array().map_or_else(||json!([]),|values|json!(values));
        }
        s.chat.notify(a,sender,"MemberResultsRes",summary);
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
    replay_rewards(s,a).await
}

/// A full game login creates a fresh PartyManager. JoinPartyNotice can update
/// an existing room, but cannot initialize that manager or open its window.
/// Leave idle membership here; a chat reconnect with the same session retains it.
pub(crate) async fn new_login(s: &AppState, a: i64) -> Result<()> {
    let mut db = s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(a).execute(&mut *db).await?;
    let Ok((id, family, room)) = membership(&mut db, a).await else { return Ok(()); };
    if n(&room, "Status") == 2 { return Ok(()); }
    rooms::remove_account(&mut db, a).await?;
    db.commit().await?;
    let req = Request([("RoomNo".into(), id.to_string())].into_iter().collect());
    after_room_request(s, a, &req, &format!("leave_{family}_room")).await
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
    if action.starts_with("leave_") {
        let target=r.number("AccountId",a)?;
        let target=if target==0 {a}else{target};
        crate::api::battle::cooperative::room_departed(s,target,id).await?;
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
    if action.starts_with("change_") {
        let raid=action=="change_raid_room_info";
        let (name,body)=settings_notice(&room,raid,false);
        db.commit().await?;
        // JoinPartyNotice only adds missing members in the native client.
        // Publish the committed settings even if the companion chat message
        // raced ahead of HTTP and was rebuilt from the previous room state.
        for receiver in ids {s.chat.notify(receiver,n(&room,"MasterAccountId"),name,body.clone());}
        return Ok(());
    }
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
