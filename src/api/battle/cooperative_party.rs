//! Ordinary raid/story parties share the authenticated native battle worker.
use super::*;

pub(crate) async fn begin_party(
    db: &mut SqliteConnection,
    s: &AppState,
    a: i64,
    r: &Request,
) -> Result<Value> {
    let room_id = r.number("RaidRoomNo", 0)?.max(r.number("MultiRoomNo", 0)?);
    let (family, raw): (String, String) = sqlx::query_as("SELECT family,data FROM battle_rooms WHERE id=? AND EXISTS(SELECT 1 FROM battle_room_members WHERE room=? AND account=?)")
        .bind(room_id).bind(room_id).bind(a).fetch_optional(&mut *db).await?.ok_or_else(|| rule("RoomNotExist"))?;
    let mut room: Value = read_json(&raw)?;
    let master = n(&room, "MasterAccountId");
    if r.number("MultiplayMasterId", 0)? != master
        || int(r, "ChapterIndex")? != n(&room, "ChapterIndex")
        || int(r, "DungeonIndex")? != n(&room, "DungeonIndex")
        || int(r, "RaidIndex")? != n(&room, "RaidIndex")
        || int(r, "RaidLevel")? != n(&room, "RaidLevel")
    {
        return Err(rule("InvalidRoomInfo"));
    }
    if family != "raid" && n(&room,"DungeonDifficulty") != campaign::difficulty(r)? + 1 {
        return Err(rule("InvalidDiff"));
    }
    if let Some(id) = room["CoopRunId"].as_str() {
        let saved: Option<(String,String)> = sqlx::query_as("SELECT entry,begin_response FROM battle_runs WHERE account=? AND json_extract(entry,'$.CoopRunId')=?")
            .bind(a).bind(id).fetch_optional(&mut *db).await?;
        let (raw,out) = saved.ok_or_else(|| rule("AlreadyOnBattleHero"))?;
        let mut entry: Value = read_json(&raw)?;
        let mut out: Value = read_json(&out)?;
        if campaign::difficulty(r)? != n(&entry,"DungeonDifficulty") {
            return Err(rule("InvalidDiff"));
        }
        let use_stamina = boolean(r,"UseStamina",true)?;
        if entry["PartyResourceChoicePending"] == true {
            let heroes = ids(r,"HeroIndices",32)?;
            let expected: Vec<i64> = read_value(entry["Heroes"].clone())?;
            if heroes.iter().copied().collect::<BTreeSet<_>>() != expected.iter().copied().collect()
                || heroes.len() != expected.len() {
                return Err(rule("NotMatchHeroIndices"));
            }
            if entry["CoopCharged"] == true { return Err(rule("NotCompletedBattle")); }
            // The host creates the shared run before guests submit BeginCampaign.
            // Only that guest's authenticated request can choose its entry cost.
            if use_stamina {
                let d = campaign::dungeon(s,r)?;
                let cost = campaign::stamina_cost(d,campaign::difficulty(r)?);
                let stamina = super::super::entry_costs::charge(db,s,a,&mut entry,n(d,"ReqStaminaType"),cost).await?;
                if !stamina.is_null() { out["StaminaResult"] = stamina; }
            }
            entry["PartyUseStamina"] = json!(use_stamina);
            entry["PartyResourceChoicePending"] = json!(false);
            entry["Request"]["UseStamina"] = json!(use_stamina.to_string());
            entry["Request"]["ItemBoosterCount"] = json!(if use_stamina {int(r,"ItemBoosterCount")?} else {0}.to_string());
            super::super::loot_booster::prepare(db,s,a,r,&mut entry,&mut out,use_stamina).await?;
            sqlx::query("UPDATE battle_runs SET entry=?,begin_response=? WHERE account=?")
                .bind(entry.to_string()).bind(out.to_string()).bind(a).execute(&mut *db).await?;
        } else if entry["PartyUseStamina"] != json!(use_stamina) {
            return Err(rule("InvalidValue"));
        } else if use_stamina && entry["Request"]["ItemBoosterCount"].as_str().unwrap_or("0") != r.number("ItemBoosterCount",0)?.to_string() {
            return Err(rule("InvalidValue"));
        }
        return Ok(out);
    }
    if a != master || n(&room, "Status") != 0 {
        return Err(rule("WrongMember"));
    }
    if !boolean(r,"UseStamina",true)? { return Err(rule("InvalidValue")); }
    let def = if family == "raid" {
        let def = row(
            s,
            "Raid",
            &[
                ("Index", n(&room, "RaidIndex")),
                ("Level", n(&room, "RaidLevel")),
            ],
        )?;
        if !matches!(n(def, "Type"), 0..=3) {
            return Err(rule("BattleServerNotFound"));
        }
        def
    } else {
        if n(&room, "DungeonDifficulty") != campaign::difficulty(r)? + 1 {
            return Err(rule("InvalidDiff"));
        }
        row(
            s,
            "PartyDungeon",
            &[
                ("ChapterIndex", n(&room, "ChapterIndex")),
                ("DungeonIndex", n(&room, "DungeonIndex")),
            ],
        )?
    };
    let worker_id = s
        .conquest
        .worker
        .read()
        .await
        .as_ref()
        .ok_or_else(|| rule("BattleServerNotFound"))?
        .id
        .clone();
    let members: Vec<i64> = sqlx::query_scalar(
        "SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account",
    )
    .bind(room_id)
    .fetch_all(&mut *db)
    .await?;
    if ids(r, "MultiplayMemberIds", 8)?
        .into_iter()
        .collect::<BTreeSet<_>>()
        != members.iter().copied().collect()
        || members.len() as i64 > n(&room, "Capacity")
    {
        return Err(rule("WrongMember"));
    }
    let selected = ids(r, "HeroIndices", 32)?;
    let owners: Vec<Value> = read_json(r.text("AccountIds")).map_err(|_| rule("WrongMember"))?;
    if selected.len() != owners.len()
        || owners.iter().any(|v| {
            v.as_i64()
                .or_else(|| v.as_str().and_then(|v| v.parse().ok()))
                != Some(a)
        })
    {
        return Err(rule("NotMatchHeroIndices"));
    }
    let id = uuid::Uuid::new_v4().to_string();
    let mut accounts = vec![];
    let mut all = BTreeSet::new();
    let mut entries = vec![];
    for account in &members {
        let active: bool = sqlx::query_scalar(
            "SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=0)",
        )
        .bind(account)
        .fetch_one(&mut *db)
        .await?;
        if active {
            return Err(rule("AlreadyOnBattleHero"));
        }
        let state = get(db, *account, "party_member", room_id).await?;
        if *account != master && state["IsBattleReady"] != true {
            return Err(rule("WrongMember"));
        }
        let own: Vec<i64> = if *account == master {
            selected.clone()
        } else {
            state["DeckHeros"]
                .as_object()
                .ok_or_else(|| rule("NotMatchHeroIndices"))?
                .keys()
                .map(|v| v.parse().map_err(|_| rule("NotMatchHeroIndices")))
                .collect::<Result<_>>()?
        };
        if own.is_empty() || own.len() as i64 > n(def, "DeckCountPerPlayer") {
            return Err(rule("NotMatchHeroIndices"));
        }
        if state["SubDeckHeros"]
            .as_object()
            .is_some_and(|v| !v.is_empty())
        {
            return Err(rule("NotMatchHeroIndices"));
        }
        for hero in &own {
            if !all.insert(*hero) {
                return Err(rule("DuplicatedHero"));
            }
        }
        let mut fields = r.0.clone();
        for key in [
            "RaidRoomNo",
            "MultiRoomNo",
            "MultiplayMasterId",
            "MultiplayMemberIds",
            "MultiplayObserverIds",
            "MultiplayMasterServerIp",
            "MultiplayMasterServerPort",
            "AccountIds",
        ] {
            fields.remove(key);
        }
        fields.insert("HeroIndices".into(), json!(own).to_string());
        fields.insert("AiHeroIndices".into(), "[]".into());
        fields.insert("GroupHeroIndices".into(), "[]".into());
        fields.insert("DeltaHeroIndices".into(), "[]".into());
        fields.insert("ZetaHeroIndices".into(), "[]".into());
        let use_stamina = *account == master;
        fields.insert("UseStamina".into(), use_stamina.to_string());
        if !use_stamina { fields.insert("ItemBoosterCount".into(), "0".into()); }
        let local = Request(fields);
        let mut out = campaign::begin_party_member(db, s, *account, &local, use_stamina).await?;
        let raw: String = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?")
            .bind(account)
            .fetch_one(&mut *db)
            .await?;
        let mut entry: Value = read_json(&raw)?;
        entry["CoopRunId"] = json!(id);
        entry["CoopMaster"] = json!(master);
        entry["CoopCharged"] = json!(false);
        entry["PartyCampaign"] = json!(true);
        entry["CoopRoom"] = json!(room_id);
        entry["PartyResourceChoicePending"] = json!(!use_stamina);
        let mut info = conquest::combat_buffs(db, s, *account).await?;
        let user = sqlx::query("SELECT a.nick,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id=?")
            .bind(account).fetch_one(&mut *db).await?;
        info["UserInfo"] = json!({"AccountId":account,"Nick":user.get::<String,_>("nick"),"TeamLevel":user.get::<i64,_>("team_level"),"AvatarHeroIndex":user.get::<i64,_>("avatar_hero_index")});
        let mut heroes = serde_json::Map::new();
        for hero in &own {
            let owned = special::cached_hero(db, *account, *hero as i32).await?;
            if n(&owned, "Level") < n(def, "ReqHeroLevel") {
                return Err(rule("NotAvailableHero"));
            }
            heroes.insert(hero.to_string(), owned);
        }
        info["HeroInfos"] = json!(heroes);
        info["AiHeroInfos"] = json!({});
        info["GroupHeroInfos"] = json!({});
        accounts.push(info);
        out["BattleServerAddress"] = json!(s.conquest.address);
        out["BattleServerPort"] = json!(s.conquest.port);
        out["MasterServerAddress"] = json!(s.conquest.address);
        out["MasterServerPort"] = json!(s.conquest.port);
        out["MultiplayRewardItems"] = json!([]);
        entries.push((*account, entry, out));
    }
    if all.len() as i64 > n(def, "MainPartyCount").max(n(def, "DeckCount")) {
        return Err(rule("NotMatchHeroIndices"));
    }
    let result = entries.iter().find(|v| v.0 == a).unwrap().2.clone();
    for (account, entry, out) in entries {
        sqlx::query("UPDATE battle_runs SET run_id=?,entry=?,begin_response=? WHERE account=?")
            .bind(format!("{id}:{account}"))
            .bind(entry.to_string())
            .bind(out.to_string())
            .bind(account)
            .execute(&mut *db)
            .await?;
    }
    room["Status"] = json!(2);
    room["CoopRunId"] = json!(id);
    sqlx::query("UPDATE battle_rooms SET data=?,updated=? WHERE id=?")
        .bind(room.to_string())
        .bind(now())
        .bind(room_id)
        .execute(&mut *db)
        .await?;
    let payload = json!({"RunId":id,"PartyCampaign":true,"BossHp":0,"BossIndex":0,"Seed":now()%i32::MAX as i64,
        "RaidIndex":room["RaidIndex"],"RaidLevel":room["RaidLevel"],
        "Simulation":{"Uid":id,"ChapterIndex":room["ChapterIndex"],"DungeonIndex":room["DungeonIndex"],"DungeonDifficulty":campaign::difficulty(r)?,"AccountInfos":accounts}});
    s.conquest.runs.insert(
        id.clone(),
        Arc::new(Mutex::new(Run {
            id,
            worker: worker_id,
            master,
            room: room_id,
            members,
            payload,
            entered: BTreeSet::new(),
            ready: BTreeSet::new(),
            sent: BTreeSet::new(),
            departed: BTreeSet::new(),
            offline: BTreeSet::new(),
            logs: vec![],
            pending_actions: vec![],
            result: None,
            started: false,
            wave_started: false,
            prepared: false,
            created: now(),
            time_ms: 0,
        })),
    );
    Ok(result)
}

pub(super) async fn settle_party(s: &AppState, run: &Run, body: &Value) -> Result<()> {
    let creatures = body["Creatures"]
        .as_array()
        .ok_or_else(|| rule("InvalidValue"))?;
    let mut db = s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?")
        .bind(run.master)
        .execute(&mut *db)
        .await?;
    for account in &run.members {
        let (completed, raw): (i64, String) =
            sqlx::query_as("SELECT completed,entry FROM battle_runs WHERE account=?")
                .bind(account)
                .fetch_one(&mut *db)
                .await?;
        let entry: Value = read_json(&raw)?;
        if entry["CoopRunId"] != run.id || entry["CoopCharged"] != true
            || entry["PartyResourceChoicePending"] == true {
            return Err(rule("InvalidValue"));
        }
        if completed != 0 {
            continue;
        }
        let alive: Vec<_> = creatures
            .iter()
            .filter(|v| {
                number(v, "TeamId") == 0
                    && number(v, "AccountId") == *account
                    && number(v, "Hp") > 0
            })
            .map(|v| json!(number(v, "Index")))
            .collect();
        let mut fields = entry["Request"].clone();
        fields["Completed"] = body["Win"].clone();
        fields["Star"] = json!(if body["Win"] == true { 3 } else { 0 });
        fields["PureBattleTime"] = json!(number(body, "TimeMs") / 1000);
        // A party victory still counts for a participant whose own heroes died.
        if !alive.is_empty() {
            fields["AliveHeroIndices"] = json!(alive);
        }
        let mut out = campaign::end_authoritative(&mut db, s, *account, &request(fields)).await?;
        // Native EndCampaign gates the reward screen on this participant list,
        // even when all drops were awarded individually and the shared pool is empty.
        out["MultiplayMemberIds"] = json!(run.members);
        out["MultiplayRewardItems"] = json!([]);
        sqlx::query("UPDATE battle_runs SET entry=json_set(entry,'$.PartyEndResponse',json(?)) WHERE account=?")
            .bind(out.to_string()).bind(account).execute(&mut *db).await?;
    }
    sqlx::query("UPDATE battle_rooms SET data=json_remove(json_set(data,'$.Status',0),'$.CoopRunId'),updated=? WHERE id=? AND json_extract(data,'$.CoopRunId')=?")
        .bind(now()).bind(run.room).bind(&run.id).execute(&mut *db).await?;
    sqlx::query("UPDATE battle_room_members SET updated=? WHERE room=?")
        .bind(now()).bind(run.room).execute(&mut *db).await?;
    db.commit().await?;
    for account in &run.members {
        if let Err(error)=super::super::party_messages::replay_rewards(s,*account).await {
            tracing::warn!(%error,"Party reward summary notification failed after settlement");
        }
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::account::user;
    use crate::api::battle::tests::{account, call, setup};

    async fn fixture() -> (AppState, Vec<Value>, String) {
        let (s, u) = setup().await;
        let v = json!(
            user::test_login(
                State(s.clone()),
                Bytes::from_static(b"LoginId=party-follower")
            )
            .await
            .unwrap()
            .0
        );
        let users = vec![u, v];
        let members: Vec<_> = users.iter().map(account).collect();
        let room = json!({"RoomNo":1,"MasterAccountId":members[0],"RaidIndex":0,"RaidLevel":0,"ChapterIndex":1,"DungeonIndex":1,"DungeonDifficulty":2,"Status":0,"Capacity":4,"Opened":0});
        sqlx::query("INSERT INTO battle_rooms(id,family,master,data,updated) VALUES(1,'party_dungeon',?,?,?)")
            .bind(members[0]).bind(room.to_string()).bind(now()).execute(&s.db).await.unwrap();
        for (i, a) in members.iter().enumerate() {
            let hero = i as i64 + 1;
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)").bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
            sqlx::query(
                "INSERT INTO battle_room_members(room,account,joined,updated) VALUES(1,?,?,?)",
            )
            .bind(a)
            .bind(now())
            .bind(now())
            .execute(&s.db)
            .await
            .unwrap();
            put(&mut s.db.acquire().await.unwrap(),*a,"party_member",1,&json!({"IsBattleReady":true,"DeckHeros":{hero.to_string():{"HeroIndex":hero}},"SubDeckHeros":{}})).await.unwrap();
        }
        let args = format!("ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&RaidRoomNo=1&MultiplayMasterId={}&MultiplayMemberIds={}&HeroIndices=[1]&AccountIds=[{}]",members[0],json!(members),members[0]);
        (s, users, args)
    }
    async fn attach_worker(s: &AppState) -> mpsc::Receiver<Packet> {
        let (tx, rx) = mpsc::channel(100);
        *s.conquest.worker.write().await = Some(Worker {
            id: "party-worker".into(),
            sender: tx,
        });
        rx
    }
    fn guest_args(users: &[Value], args: &str, use_stamina: bool) -> String {
        format!("{}&UseStamina={use_stamina}",args.replace("HeroIndices=[1]", "HeroIndices=[2]").replace(
            &format!("AccountIds=[{}]",account(&users[0])),
            &format!("AccountIds=[{}]",account(&users[1]))))
    }
    #[tokio::test]
    async fn party_loot_booster_uses_each_players_own_confirmed_choice() {
        use crate::api::battle::{entry_costs,loot_booster};
        let (s,users,args)=fixture().await;let _worker=attach_worker(&s).await;
        let host=account(&users[0]);let guest=account(&users[1]);
        sqlx::query("UPDATE user_info SET gem=100 WHERE account_id=?").bind(guest).execute(&s.db).await.unwrap();
        // Only the host enables x2. A guest is provisional until it authenticates.
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&format!("{args}&ItemBoosterCount=1")).await["Result"],"Success");
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),host,"Currency",4).await.unwrap(),20);
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),guest,"Currency",4).await.unwrap(),0);
        let own=guest_args(&users,&args,true);
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&format!("{own}&ItemBoosterCount=0")).await["Result"],"Success");
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign",&format!("{own}&ItemBoosterCount=1")).await["Result"],"Success","A retry cannot change the saved booster choice");
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),guest,"Currency",4).await.unwrap(),0);
        recover_orphaned(&s).await.unwrap();
        // An unpaid guest cannot acquire loot or spend rubies, even with a stale checked flag.
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success");
        let free=guest_args(&users,&args,false);
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&format!("{free}&ItemBoosterCount=1")).await["Result"],"Success");
        let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(guest).fetch_one(&s.db).await.unwrap();
        assert!(!loot_booster::active(&read_json::<Value>(&raw).unwrap()));
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),guest,"Currency",4).await.unwrap(),0);
        recover_orphaned(&s).await.unwrap();
        // Guest-only x2 is independent of the host's unchecked choice.
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success");
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&format!("{own}&ItemBoosterCount=2")).await["Result"],"Success");
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),guest,"Currency",4).await.unwrap(),20);
        assert_eq!(entry_costs::held(&mut s.db.acquire().await.unwrap(),host,"Currency",4).await.unwrap(),0);
        recover_orphaned(&s).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
    }
    #[tokio::test]
    async fn party_resource_choice_is_authenticated_and_cannot_bypass_solo_costs() {
        let (s,users,args)=fixture().await;let _worker=attach_worker(&s).await;
        let guest=account(&users[1]);
        sqlx::query("UPDATE user_info SET stamina=0 WHERE account_id=?").bind(guest).execute(&s.db).await.unwrap();
        assert_ne!(call(&s,&users[0],"campaign/begin_campaign",&format!("{args}&UseStamina=false")).await["Result"],"Success");
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success","An unpaid guest must not need the host's stamina cost");
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        let (tx,_rx)=mpsc::channel(10);
        assert!(player_packet_for_run(&s,guest,&id,"BattleEnterReq",json!({"RequestId":1}),&tx).await.is_err(),"Guests must confirm their own options before entering combat");
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,true)).await["Result"],"Success","Paid participation still requires stamina");
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,false).replace("HeroIndices=[2]","HeroIndices=[1]")).await["Result"],"Success");
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,false).replace("DungeonDifficulty=1","DungeonDifficulty=0")).await["Result"],"Success");
        let free=guest_args(&users,&args,false);
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&free).await["Result"],"Success");
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&free).await["Result"],"Success");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds WHERE account=?").bind(guest).fetch_one(&s.db).await.unwrap(),0);
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,true)).await["Result"],"Success","Retry cannot change a confirmed choice");
        recover_orphaned(&s).await.unwrap();
        assert_ne!(call(&s,&users[1],"campaign/begin_campaign","ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&HeroIndices=[2]&UseStamina=false").await["Result"],"Success","Solo requests cannot use the party-only free option");
    }
    #[tokio::test]
    async fn disconnected_story_party_releases_holds_and_allows_another_battle() {
        use crate::websocket::tests::{server,Client};
        let (s,users,args)=fixture().await;let _worker=attach_worker(&s).await;
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success");
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        let server=server(s.clone()).await;let master=account(&users[0]);let mut clients=vec![];
        for user in &users {
            let mut client=Client::connect(&server,"battle").await;
            client.send("BattleLoginReq",json!({"AccountId":account(user),"SessionKey":user["UserInfo"]["SessionKey"],"MasterId":master,"RequestId":1})).await;
            assert_eq!(client.recv("BattleLoginRes").await["Result"],"Success");clients.push(client);
        }
        let holds=sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap();assert!(holds>0);
        let guest=clients.pop().unwrap();drop(guest);
        tokio::time::sleep(std::time::Duration::from_millis(100)).await;
        cancel_abandoned(&s,&id,None).await.unwrap();assert!(s.conquest.runs.contains_key(&id),"A connected participant keeps the run alive");
        drop(clients);
        tokio::time::timeout(std::time::Duration::from_secs(6),async {
            while s.conquest.runs.contains_key(&id) {tokio::time::sleep(std::time::Duration::from_millis(25)).await;}
        }).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=0").fetch_one(&s.db).await.unwrap(),0);
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success");
    }
    #[tokio::test]
    async fn party_campaign_requires_worker_and_ready_owned_decks() {
        let (s, users, args) = fixture().await;
        assert_eq!(
            call(&s, &users[0], "campaign/begin_campaign", &args).await["Result"],
            "BattleServerNotFound"
        );
        assert!(s.conquest.runs.is_empty());
        let _worker = attach_worker(&s).await;
        assert_ne!(
            call(&s, &users[1], "campaign/begin_campaign", &args).await["Result"],
            "Success"
        );
        assert_ne!(
            call(
                &s,
                &users[0],
                "campaign/begin_campaign",
                &args.replace("HeroIndices=[1]", "HeroIndices=[2]")
            )
            .await["Result"],
            "Success"
        );
        sqlx::query("UPDATE battle_state SET data=json_set(data,'$.IsBattleReady',json('false')) WHERE account=? AND kind='party_member'")
            .bind(account(&users[1])).execute(&s.db).await.unwrap();
        assert_ne!(
            call(&s, &users[0], "campaign/begin_campaign", &args).await["Result"],
            "Success"
        );
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_runs")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
    }
    #[tokio::test]
    async fn party_campaign_shared_start_authoritative_settlement_and_retry() {
        party_reward_settlement(true).await;
    }
    #[tokio::test]
    async fn party_guest_without_resource_spent_receives_no_loot() {
        party_reward_settlement(false).await;
    }
    async fn party_reward_settlement(guest_uses_stamina: bool) {
        let (mut s, users, args) = fixture().await;
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).battle).tables.get_mut("CampaignDungeon").unwrap()
            .iter_mut().find(|d|n(d,"ChapterIndex")==1 && n(d,"DungeonIndex")==1).unwrap()["FirstClearGem_Normal"]=json!(137);
        let mut worker = attach_worker(&s).await;
        let message_server=crate::websocket::tests::server(s.clone()).await;
        let mut chat_clients=vec![];
        for user in &users {
            let mut client=crate::websocket::tests::Client::connect(&message_server,"chat").await;
            client.send("LoginReq",json!({"AccountId":account(user),"SessionKey":user["UserInfo"]["SessionKey"],"RequestId":1})).await;
            assert_eq!(client.recv("LoginRes").await["Result"],"Success");chat_clients.push(client);
        }
        let begun = call(&s, &users[0], "campaign/begin_campaign", &args).await;
        assert_eq!(begun["Result"], "Success", "{begun}");
        let guest=account(&users[1]);
        let guest_currency_before:(i64,i64)=sqlx::query_as("SELECT gold,gem FROM user_info WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap();
        let guest_items_before:Vec<(i64,i64)>=sqlx::query_as("SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index").bind(guest).fetch_all(&s.db).await.unwrap();
        let guest_equipment_before:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap();
        let guest_mail_before:i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds WHERE account=?").bind(guest).fetch_one(&s.db).await.unwrap(),0);
        let stamina_before:Vec<(i64,i64)>=sqlx::query_as("SELECT account_id,stamina FROM user_info ORDER BY account_id").fetch_all(&s.db).await.unwrap();
        assert_eq!(
            call(
                &s,
                &users[1],
                "campaign/begin_campaign",
                &guest_args(&users,&args,guest_uses_stamina)
            )
            .await["Result"],
            "Success"
        );
        assert_ne!(
            call(
                &s,
                &users[0],
                "campaign/end_campaign",
                "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&Star=3"
            )
            .await["Result"],
            "Success"
        );
        let id = s.conquest.runs.iter().next().unwrap().key().clone();
        let mut receivers = vec![];
        for u in &users {
            let a = account(u);
            let (tx, _rx) = mpsc::channel(100);
            s.conquest.peers.insert(
                a,
                Peer {
                    id: a.to_string(),
                    run: id.clone(),
                    sender: tx.clone(),
                },
            );
            receivers.push(_rx);
            player_packet(&s, a, "BattleEnterReq", json!({"RequestId":1}), &tx)
                .await
                .unwrap();
        }
        let (name, payload) = worker.recv().await.unwrap();
        assert_eq!(name, "WorkerPrepare");
        assert_eq!(
            payload["Simulation"]["AccountInfos"]
                .as_array()
                .unwrap()
                .len(),
            2
        );
        for u in &users {
            let tx = s.conquest.peers.get(&account(u)).unwrap().sender.clone();
            player_packet(
                &s,
                account(u),
                "BattleStartReq",
                json!({"RequestId":2}),
                &tx,
            )
            .await
            .unwrap();
        }
        assert_eq!(worker.recv().await.unwrap().0, "WorkerStart");
        worker_packet(&s,"party-worker","WorkerPacket",json!({"RunId":id,"Name":"WaveReadyNot","Body":{"WaveIndex":2}})).await.unwrap();
        for u in &users {
            let tx=s.conquest.peers.get(&account(u)).unwrap().sender.clone();
            player_packet(&s,account(u),"BattleStartReq",json!({"RequestId":3}),&tx).await.unwrap();
        }
        assert_eq!(worker.recv().await.unwrap().0,"WorkerStart");
        worker_packet(&s,"party-worker","WorkerResult",json!({"RunId":id,"Win":true,"Creatures":[{"TeamId":0,"AccountId":account(&users[0]),"Index":1,"Hp":10},{"TeamId":0,"AccountId":account(&users[1]),"Index":2,"Hp":10}],"TimeMs":10000,"MaxWaveIndex":1})).await.unwrap();
        for client in &mut chat_clients {
            let mut summaries=std::collections::BTreeMap::new();
            for _ in 0..40 {
                let message=client.recv("MessageNot").await;
                if message["Type"]=="MemberResultsRes" {
                    let sender=message["SenderId"].as_i64().unwrap();
                    let summary:Value=read_json(message["Content"].as_str().unwrap()).unwrap();
                    summaries.insert(sender,summary);
                    if summaries.len()==users.len() {break;}
                }
            }
            assert_eq!(summaries.len(),users.len(),"Every client needs both participants' Loot summaries, including itself");
            for user in &users {
                let a=account(user);let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(a).fetch_one(&s.db).await.unwrap();
                let entry:Value=read_json(&raw).unwrap();let out=&entry["PartyEndResponse"];
                assert_eq!(summaries[&a]["UseStamina"],json!(a==account(&users[0]) || guest_uses_stamina));
                for field in ["HeroExpResults","CurrencyResults","ItemResults","EquipItemInfos","FlaskResults"] {
                    assert_eq!(summaries[&a][field],out[field]);
                }
                assert_eq!(summaries[&a]["ItemResults2"],json!([]));
            }
        }
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=1")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            2
        );
        let ending = "ChapterIndex=1&DungeonIndex=1&DungeonDifficulty=1&Completed=true&Star=3";
        let cost=campaign::stamina_cost(row(&s,"CampaignDungeon",&[("ChapterIndex",1),("DungeonIndex",1)]).unwrap(),1);
        for (account,before) in &stamina_before {
            let after:i64=sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?").bind(account).fetch_one(&s.db).await.unwrap();
            assert_eq!(after,before-if *account==guest && !guest_uses_stamina {0}else{cost});
            let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(account).fetch_one(&s.db).await.unwrap();
            let entry:Value=read_json(&raw).unwrap();
            let out=&entry["PartyEndResponse"];
            if *account==guest && !guest_uses_stamina {
                for field in ["CurrencyResults","ItemResults","EquipItemInfos","FlaskResults","FlaskItemResults"] {
                    assert_eq!(out[field],json!([]),"Free guests must not receive {field}");
                }
                assert_eq!(sqlx::query_as::<_,(i64,i64)>("SELECT gold,gem FROM user_info WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap(),guest_currency_before);
                assert_eq!(sqlx::query_as::<_,(i64,i64)>("SELECT item_index,count FROM items WHERE account_id=? ORDER BY item_index").bind(guest).fetch_all(&s.db).await.unwrap(),guest_items_before);
                assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap(),guest_equipment_before);
                assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE account_id=?").bind(guest).fetch_one(&s.db).await.unwrap(),guest_mail_before,"No hidden overflow rewards");
                assert_eq!(out["CampaignResults"][0]["LootRewardedDiff"],0);
            } else {
                assert!(out["CurrencyResults"].as_array().unwrap().iter().any(|v|v["CurrencyType"]=="Gold" && n(v,"AddValue")>0),"Paid participants still receive gold");
            }
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT clear_count FROM campaign_progress WHERE account_id=? AND chapter_id=1 AND dungeon_id=1").bind(account).fetch_one(&s.db).await.unwrap(),1,"Free participation still credits the clear");
        }
        let balances:Vec<(i64,i64)>=sqlx::query_as("SELECT account_id,gold FROM user_info ORDER BY account_id").fetch_all(&s.db).await.unwrap();
        let equipment_count=sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items").fetch_one(&s.db).await.unwrap();
        let first = call(&s, &users[0], "campaign/end_campaign", ending).await;
        assert_eq!(first["Result"], "Success");
        assert_eq!(first["MultiplayMemberIds"], json!(users.iter().map(account).collect::<Vec<_>>()));
        assert_eq!(first["MultiplayRewardItems"], json!([]));
        assert_eq!(
            first,
            call(&s, &users[0], "campaign/end_campaign", ending).await
        );
        let guest_end=call(&s,&users[1],"campaign/end_campaign",ending).await;
        assert_eq!(guest_end["Result"],"Success");
        assert_eq!(call(&s,&users[1],"campaign/end_campaign",ending).await,guest_end);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT clear_count FROM campaign_progress WHERE account_id=? AND chapter_id=1 AND dungeon_id=1").bind(account(&users[0])).fetch_one(&s.db).await.unwrap(),1);
        assert_eq!(sqlx::query_as::<_,(i64,i64)>("SELECT account_id,gold FROM user_info ORDER BY account_id").fetch_all(&s.db).await.unwrap(),balances,"Summary/result replay must not award gold again");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items").fetch_one(&s.db).await.unwrap(),equipment_count,"Summary/result replay must not award equipment again");
        for (account,before) in stamina_before {
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info WHERE account_id=?").bind(account).fetch_one(&s.db).await.unwrap(),before-if account==guest && !guest_uses_stamina {0}else{cost},"Result replay must not charge again");
        }
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
        if !guest_uses_stamina {
            let mut db=s.db.begin().await.unwrap();
            let paid=request(json!({"ChapterIndex":1,"DungeonIndex":1,"DungeonDifficulty":1}));
            let out=campaign::complete(&mut db,&s,guest,&paid,&[2],3,1).await.unwrap();
            assert!(out["CurrencyResults"].as_array().unwrap().iter().any(|v|v["CurrencyType"]=="Gem" && n(v,"AddValue")==137),"First-clear loot remains available after a free clear");
            let out=campaign::complete(&mut db,&s,guest,&paid,&[2],3,1).await.unwrap();
            assert!(!out["CurrencyResults"].as_array().unwrap().iter().any(|v|v["CurrencyType"]=="Gem" && n(v,"AddValue")>0),"The deferred first reward is granted only once");
            db.commit().await.unwrap();
        }
    }
    #[tokio::test]
    async fn party_campaign_worker_loss_and_restart_release_holds() {
        let (s, users, args) = fixture().await;
        let _worker = attach_worker(&s).await;
        assert_eq!(
            call(&s, &users[0], "campaign/begin_campaign", &args).await["Result"],
            "Success"
        );
        assert!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap()
                > 0
        );
        recover_orphaned(&s).await.unwrap();
        assert_eq!(
            sqlx::query_scalar::<_, i64>("SELECT COUNT(*) FROM battle_entry_holds")
                .fetch_one(&s.db)
                .await
                .unwrap(),
            0
        );
        assert_eq!(
            sqlx::query_scalar::<_, i64>(
                "SELECT json_extract(data,'$.Status') FROM battle_rooms WHERE id=1"
            )
            .fetch_one(&s.db)
            .await
            .unwrap(),
            0
        );
        recover_orphaned(&s).await.unwrap();
    }
    #[tokio::test]
    async fn dragon_party_uses_worker_route_and_keeps_direct_solo_guard() {
        let (s,users,_) = fixture().await;
        let _worker = attach_worker(&s).await;
        sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
        sqlx::query("UPDATE battle_rooms SET family='raid',data=json_set(data,'$.RaidIndex',1,'$.RaidLevel',7,'$.ChapterIndex',901,'$.DungeonIndex',7,'$.DungeonDifficulty',0,'$.Capacity',2)").execute(&s.db).await.unwrap();
        let args = format!("ChapterIndex=901&DungeonIndex=7&DungeonDifficulty=0&RaidIndex=1&RaidLevel=7&RaidRoomNo=1&MultiplayMasterId={}&MultiplayMemberIds=[{},{}]&HeroIndices=[1]&AccountIds=[{}]",account(&users[0]),account(&users[0]),account(&users[1]),account(&users[0]));
        let direct = "ChapterIndex=901&DungeonIndex=7&DungeonDifficulty=0&RaidIndex=1&RaidLevel=7&HeroIndices=[1]&ServiceOwned=true";
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",direct).await["Result"],"ContentsDisabled");
        let begun = call(&s,&users[0],"campaign/begin_campaign",&args).await;
        assert_eq!(begun["Result"],"Success","{begun}");
        let id = s.conquest.runs.iter().next().unwrap().key().clone();
        let handle = s.conquest.runs.get(&id).unwrap().clone();
        let run = handle.lock().await;
        assert_eq!(run.payload["Simulation"]["ChapterIndex"],901);
        assert_eq!(run.payload["Simulation"]["DungeonIndex"],7);
    }
    #[tokio::test]
    async fn dragon_party_clear_unlocks_next_stage_and_preserves_each_members_higher_progress() {
        let (s,users,_) = fixture().await;
        let _worker = attach_worker(&s).await;
        sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
        sqlx::query("UPDATE battle_rooms SET family='raid',data=json_set(data,'$.RaidIndex',1,'$.RaidLevel',7,'$.ChapterIndex',901,'$.DungeonIndex',7,'$.DungeonDifficulty',0,'$.Capacity',2)").execute(&s.db).await.unwrap();
        put(&mut s.db.acquire().await.unwrap(),account(&users[1]),"raid",1,&json!({"RaidIndex":1,"RaidLevel":16})).await.unwrap();
        let args=format!("ChapterIndex=901&DungeonIndex=7&DungeonDifficulty=0&RaidIndex=1&RaidLevel=7&RaidRoomNo=1&MultiplayMasterId={}&MultiplayMemberIds=[{},{}]&HeroIndices=[1]&AccountIds=[{}]",account(&users[0]),account(&users[0]),account(&users[1]),account(&users[0]));
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args).await["Result"],"Success");
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,false)).await["Result"],"Success");
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        sqlx::query("UPDATE battle_runs SET entry=json_set(entry,'$.CoopCharged',json('true'))").execute(&s.db).await.unwrap();
        let handle=s.conquest.runs.get(&id).unwrap().clone();let mut run=handle.lock().await;run.started=true;drop(run);
        worker_packet(&s,"party-worker","WorkerResult",json!({"RunId":id,"Win":true,"Creatures":[{"TeamId":0,"AccountId":account(&users[0]),"Index":1,"Hp":10},{"TeamId":0,"AccountId":account(&users[1]),"Index":2,"Hp":10}],"TimeMs":10000,"MaxWaveIndex":1})).await.unwrap();
        let end="ChapterIndex=901&DungeonIndex=7&DungeonDifficulty=0&Completed=true&Star=3";
        for (user,expected) in users.iter().zip([8,16]) {
            let response=call(&s,user,"campaign/end_campaign",end).await;
            assert_eq!(response["Result"],"Success");assert_eq!(response["CompletedRaidInfo"]["RaidLevel"],expected);
            if account(user)==account(&users[1]) {
                for field in ["CurrencyResults","ItemResults","EquipItemInfos"] {assert_eq!(response[field],json!([]));}
            }
            assert_eq!(call(&s,user,"campaign/end_campaign",end).await,response);
            assert_eq!(get(&mut s.db.acquire().await.unwrap(),account(user),"raid",1).await.unwrap()["RaidLevel"],expected);
        }
    }
    #[tokio::test]
    async fn full_login_leaves_idle_room_but_keeps_active_native_battle() {
        let (s, users, _) = fixture().await;
        // FirstLobby also uses this snapshot. Returning from battle must keep
        // the existing PartyManager's room; only a full user login leaves it.
        super::super::login(&s, account(&users[1])).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_room_members WHERE account=?")
            .bind(account(&users[1])).fetch_one(&s.db).await.unwrap(),1);
        super::super::party_messages::new_login(&s, account(&users[1]))
            .await
            .unwrap();
        assert_eq!(
            sqlx::query_scalar::<_, i64>(
                "SELECT COUNT(*) FROM battle_room_members WHERE account=?"
            )
            .bind(account(&users[1]))
            .fetch_one(&s.db)
            .await
            .unwrap(),
            0
        );
        sqlx::query("UPDATE battle_rooms SET data=json_set(data,'$.Status',2) WHERE id=1")
            .execute(&s.db)
            .await
            .unwrap();
        super::super::party_messages::new_login(&s, account(&users[0]))
            .await
            .unwrap();
        assert_eq!(
            sqlx::query_scalar::<_, i64>(
                "SELECT COUNT(*) FROM battle_room_members WHERE account=?"
            )
            .bind(account(&users[0]))
            .fetch_one(&s.db)
            .await
            .unwrap(),
            1
        );
    }

    #[tokio::test]
    #[ignore = "Requires the patched native Unity worker in CONQUEST_NATIVE_EXECUTABLE"]
    async fn native_story_party_combat_and_settlement() {
        let _ = tracing_subscriber::fmt().with_max_level(tracing::Level::WARN).try_init();
        use tokio::io::{AsyncBufReadExt, BufReader};
        struct Native(std::process::Child);
        impl Drop for Native { fn drop(&mut self) { let _=self.0.kill(); let _=self.0.wait(); } }
        let (mut s,users,args) = fixture().await;
        sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
        let key = uuid::Uuid::new_v4().to_string(); s.battle_service_key = Arc::new(Some(key.clone()));
        let listener = TcpListener::bind("127.0.0.1:0").await.unwrap(); let port=listener.local_addr().unwrap().port();
        let server=tokio::spawn(serve(listener,s.clone()));
        let executable = std::path::PathBuf::from(std::env::var("CONQUEST_NATIVE_EXECUTABLE").expect("Native worker executable")).canonicalize().unwrap();
        let mut command=std::process::Command::new(&executable);
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join("target/party-followup/native-story.log"))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1").env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] { use std::os::windows::process::CommandExt; command.creation_flags(0x08000000); }
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(90),async {
            while s.conquest.worker.read().await.is_none() { tokio::time::sleep(std::time::Duration::from_millis(100)).await; }
        }).await.expect("Native worker did not authenticate");
        let begun=call(&s,&users[0],"campaign/begin_campaign",&args).await; assert_eq!(begun["Result"],"Success","{begun}");
        assert_eq!(call(&s,&users[1],"campaign/begin_campaign",&guest_args(&users,&args,false)).await["Result"],"Success");
        let mut clients=vec![];
        for u in &users {
            let socket=TcpStream::connect(("127.0.0.1",port)).await.unwrap(); let (reader,mut writer)=socket.into_split(); let mut reader=BufReader::new(reader);
            writer.write_all(&encode_packet("BattleLoginReq",&json!({"AccountId":account(u),"MasterId":account(&users[0]),"SessionKey":u["UserInfo"]["SessionKey"],"RequestId":1}))).await.unwrap();
            let mut line=String::new(); reader.read_line(&mut line).await.unwrap(); assert_eq!(decode_packet(line.as_bytes()).unwrap().0,"BattleLoginRes");
            writer.write_all(&encode_packet("BattleEnterReq",&json!({"RequestId":2}))).await.unwrap();
            clients.push(tokio::spawn(async move {
                let mut logs=vec![]; let mut request_id=3; let mut ended=false;
                loop {
                    let mut line=String::new(); let count=tokio::time::timeout(std::time::Duration::from_secs(120),reader.read_line(&mut line)).await.unwrap().unwrap(); assert!(count>0,"Native battle connection closed");
                    let (name,body)=decode_packet(line.as_bytes()).unwrap();
                    if name=="BattleLogNot" { logs.push(body.clone()); }
                    if name=="WaveReadyNot" {
                        println!("Native wave ready: {}",body["WaveIndex"]);
                        writer.write_all(&encode_packet("BattleStartReq",&json!({"RequestId":request_id}))).await.unwrap(); request_id+=1;
                        writer.write_all(&encode_packet("BattleWaveResumeReq",&json!({"RequestId":request_id}))).await.unwrap(); request_id+=1;
                    }
                    if name=="WaveEndNot" { println!("Native wave ended: {}",body["WaveIndex"]); ended=true; }
                    if ended && (name=="WaveEndNot" || name=="PingRes") {
                        tokio::time::sleep(std::time::Duration::from_millis(200)).await;
                        writer.write_all(&encode_packet("PingReq",&json!({"RequestId":request_id,"BattleStopped":true}))).await.unwrap(); request_id+=1;
                    }
                    if name=="BattleResultNot" { return (body,logs); }
                }
            }));
        }
        let mut results=vec![];
        for client in clients { results.push(tokio::time::timeout(std::time::Duration::from_secs(180),client).await.unwrap().unwrap()); }
        assert_eq!(results[0],results[1]); assert_eq!(results[0].0["Win"],true);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=1").fetch_one(&s.db).await.unwrap(),2);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
        server.abort();
    }
}
