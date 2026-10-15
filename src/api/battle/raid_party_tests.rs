use super::*;
use crate::api::account::user;
use crate::api::battle::tests::{account, call, prepared_dispatch, setup};

async fn unlock(s: &AppState, a: i64) {
    for (chapter,dungeon) in [(7,12),(8,26),(9,19),(9,23),(97,5),(10,9),(10,30)] {
        put(&mut s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(chapter,dungeon),
            &json!({"ChapterIndex":chapter,"DungeonIndex":dungeon,"FirstRewardedDiff":15,"MaxStar":33})).await.unwrap();
    }
}

async fn fixture(raid: i64, level: i64) -> (AppState, Vec<Value>, Vec<String>) {
    fixture_for_players(raid,level,3).await
}

async fn fixture_for_players(raid: i64, level: i64, players: usize) -> (AppState, Vec<Value>, Vec<String>) {
    let (s,host)=setup().await;
    let mut users=vec![host];
    for name in ["field-guest-a","field-guest-b"].into_iter().take(players-1) {
        users.push(json!(user::test_login(State(s.clone()),Bytes::from(format!("LoginId={name}"))).await.unwrap().0));
    }
    let def=row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap().clone();
    let total=n(&def,"DeckCount");
    let counts=match (players,total) {
        (2,6)=>vec![3,3],(2,7)=>vec![4,3],
        (3,5)=>vec![2,2,1],(3,6)=>vec![2,2,2],(3,7)=>vec![3,2,2],
        _=>panic!("Unexpected raid size")
    };
    let members:Vec<_>=users.iter().map(account).collect();
    let mut next=1;
    let mut rosters=vec![];
    for (i,user) in users.iter().enumerate() {
        let a=account(user);unlock(&s,a).await;
        put(&mut s.db.acquire().await.unwrap(),a,"raid",raid,&json!({"RaidIndex":raid,"RaidLevel":level})).await.unwrap();
        // Highest Technomagic tiers allow only one hero of each class.
        let heroes:Vec<i64>=(next..next+counts[i]).map(|h|if raid>=201 && h==5 {10}else{h}).collect();next+=counts[i];
        for hero in &heroes {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star,transcend) VALUES(?,?,?,100,5,5)")
                .bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
        rosters.push(heroes);
    }
    sqlx::query("UPDATE heroes SET level=100,star=5,transcend=5").execute(&s.db).await.unwrap();
    let created=call(&s,&users[0],"raid/create_raid_room",&format!("RaidIndex={raid}&RaidLevel={level}&Opened=1")).await;
    assert_eq!(created["Result"],"Success","raid {raid}: {created}");
    let room=n(&created,"RoomNo");
    for user in &users[1..] {
        let joined=call(&s,user,"raid/join_raid_room",&format!("RoomNo={room}")).await;
        assert_eq!(joined["Result"],"Success","raid {raid}: {joined}");
    }
    let mut requests=vec![];
    for (i,user) in users.iter().enumerate() {
        let a=account(user);
        let deck:serde_json::Map<_,_>=rosters[i].iter().map(|hero|(hero.to_string(),json!({"HeroIndex":hero}))).collect();
        put(&mut s.db.acquire().await.unwrap(),a,"party_member",room,&json!({"DeckHeros":deck,"SubDeckHeros":{},"IsBattleReady":true})).await.unwrap();
        requests.push(format!("ChapterIndex={}&DungeonIndex={}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={level}&RaidRoomNo={room}&MultiplayMasterId={}&MultiplayMemberIds={}&HeroIndices={}&AccountIds={}",
            n(&def,"ChapterIndex"),n(&def,"DungeonIndex"),members[0],json!(members),json!(rosters[i]),json!(vec![a;rosters[i].len()])));
    }
    (s,users,requests)
}

async fn fake_worker(s: &AppState) -> mpsc::Receiver<Packet> {
    let (tx,rx)=mpsc::channel(100);
    *s.conquest.worker.write().await=Some(Worker{id:"raid-worker".into(),sender:tx,arena_version:0});rx
}

async fn arcdim_fixture(raid:i64,level:i64,host_second:bool)->(AppState,Vec<Value>,Vec<String>) {
    let (s,host)=setup().await;
    let guest=json!(user::test_login(State(s.clone()),Bytes::from_static(b"LoginId=arcdim-guest")).await.unwrap().0);
    let users=vec![host,guest];let members:Vec<_>=users.iter().map(account).collect();
    let def=row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap();
    for (index,user) in users.iter().enumerate() {
        let a=account(user);unlock(&s,a).await;
        sqlx::query("UPDATE user_info SET stamina=10000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        put(&mut s.db.acquire().await.unwrap(),a,"raid",raid,&json!({"RaidIndex":raid,"RaidLevel":level})).await.unwrap();
        for h in index as i64*4+1..=index as i64*4+4 {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star,transcend) VALUES(?,?,?,100,5,5)")
                .bind(a).bind(h).bind(h).execute(&s.db).await.unwrap();
        }
    }
    sqlx::query("UPDATE heroes SET level=100,star=5,transcend=5").execute(&s.db).await.unwrap();
    let room=call(&s,&users[0],"raid/create_raid_room",&format!("RaidIndex={raid}&RaidLevel={level}&Opened=1")).await;
    assert_eq!(room["Result"],"Success","{room}");let room=n(&room,"RoomNo");
    assert_eq!(call(&s,&users[1],"raid/join_raid_room",&format!("RoomNo={room}")).await["Result"],"Success");
    let mut args=vec![];
    for (index,user) in users.iter().enumerate() {
        let second=(index==0)==host_second;
        let heroes:Vec<_>=(index as i64*4+1..=index as i64*4+4).collect();
        let main=if second {vec![]}else{heroes.clone()};let sub=if second {heroes.clone()}else{vec![]};
        let deck:serde_json::Map<_,_>=heroes.iter().map(|h|(h.to_string(),json!({"HeroIndex":h}))).collect();
        put(&mut s.db.acquire().await.unwrap(),account(user),"party_member",room,
            &json!({"IsBattleReady":false,"DeckHeros":if second {json!({})}else{json!(deck)},"SubDeckHeros":if second {json!(deck)}else{json!({})}})).await.unwrap();
        args.push(format!("ChapterIndex={}&DungeonIndex={}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={level}&RaidRoomNo={room}&MultiplayMasterId={}&MultiplayMemberIds={}&HeroIndices={}&AccountIds={}&DeltaHeroIndices={}&ZetaHeroIndices={}&AiHeroIndices={}",
            n(def,"ChapterIndex"),n(def,"DungeonIndex"),members[0],json!(members),json!(heroes),json!(vec![account(user);4]),json!(main),json!(sub),json!(main)));
    }
    (s,users,args)
}

#[tokio::test]
async fn arcdim_ready_and_battle_start_preserve_both_wave_teams() {
    for raid in [401,402] {for level in [1,3] {for host_second in [false,true] {
        let (s,users,args)=arcdim_fixture(raid,level,host_second).await;let _worker=fake_worker(&s).await;
        let host=account(&users[0]);let guest=account(&users[1]);
        assert_ne!(call(&s,&users[0],"campaign/begin_campaign",&args[0]).await["Result"],"Success");
        super::super::party_messages::send(&s,guest,host,"PartyRoomReady",json!({"IsReady":true})).await.unwrap();
        for (index,user) in users.iter().enumerate() {
            let start=call(&s,user,"campaign/begin_campaign",&args[index]).await;
            if start["Result"]!="Success" {
                let req=Request::parse(args[index].as_bytes()).unwrap();
                let reason=campaign::begin(&mut s.db.acquire().await.unwrap(),&s,account(user),&req).await;
                panic!("Arcdim {raid}/{level}, host second={host_second}: {reason:?}");
            }
            assert_eq!(start["Result"],"Success","Arcdim {raid}/{level}, host second={host_second}: {start}");
        }
        let handle=s.conquest.runs.iter().next().unwrap().value().clone();let run=handle.lock().await;
        let accounts=run.payload["Simulation"]["AccountInfos"].as_array().unwrap();assert_eq!(accounts.len(),2);
        for (index,info) in accounts.iter().enumerate() {
            assert_eq!(info["HeroInfos"].as_object().unwrap().len(),4);
            assert_eq!(info["GroupHeroInfos"].as_object().unwrap().len(),if (index==0)==host_second {4}else{0});
            let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(account(&users[index])).fetch_one(&s.db).await.unwrap();
            let entry:Value=read_json(&raw).unwrap();assert_eq!(entry["Heroes"].as_array().unwrap().len(),4);
            assert_eq!(entry["Request"]["GroupHeroIndices"],json!(if (index==0)==host_second {(index as i64*4+1..=index as i64*4+4).collect::<Vec<_>>()}else{vec![]}).to_string());
        }
    }}}
}

#[tokio::test]
async fn arcdim_team_change_is_authenticated_broadcast_and_replayed() {
    let (s,users,args)=arcdim_fixture(401,1,false).await;let _worker=fake_worker(&s).await;
    let host=account(&users[0]);let guest=account(&users[1]);
    super::super::party_messages::send(&s,guest,host,"PartyRoomReady",json!({"IsReady":true})).await.unwrap();
    for (i,user) in users.iter().enumerate() {assert_eq!(call(&s,user,"campaign/begin_campaign",&args[i]).await["Result"],"Success");}
    let id=s.conquest.runs.iter().next().unwrap().key().clone();let mut receivers=vec![];
    for user in &users {
        let a=account(user);let(tx,rx)=mpsc::channel(100);
        s.conquest.peers.insert(a,Peer{id:a.to_string(),run:id.clone(),sender:tx});receivers.push(rx);
    }
    let notification=json!({"RunId":id,"Name":"SetNextTeamNot","Body":{"ChatperIndex":10,"DungeonIndex":3001,"DeckIndex":1}});
    assert!(worker_packet(&s,"another-worker","WorkerPacket",notification.clone()).await.is_err());
    for (key,value) in [("DeckIndex",2),("DungeonIndex",3101),("ChatperIndex",9)] {
        let mut invalid=notification.clone();invalid["Body"][key]=json!(value);
        assert!(worker_packet(&s,"raid-worker","WorkerPacket",invalid).await.is_err());
    }
    assert!(receivers.iter_mut().all(|rx|rx.try_recv().is_err()));
    worker_packet(&s,"raid-worker","WorkerPacket",notification.clone()).await.unwrap();
    for rx in &mut receivers {assert_eq!(rx.recv().await.unwrap(),("SetNextTeamNot".into(),notification["Body"].clone()));}
    let(tx,mut rx)=mpsc::channel(100);
    player_packet(&s,guest,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();
    assert_eq!(rx.recv().await.unwrap(),("SetNextTeamNot".into(),notification["Body"].clone()));
}

#[tokio::test]
async fn arcdim_party_wins_unlock_next_stage_for_both_players_without_regressing() {
    for raid in [401,402] {for (level,previous,expected,won) in [(1,1,2,true),(2,2,3,true),(3,3,3,true),(1,3,3,true),(1,1,1,false)] {
        let(s,users,args)=arcdim_fixture(raid,level,false).await;let _worker=fake_worker(&s).await;
        for user in &users {
            put(&mut s.db.acquire().await.unwrap(),account(user),"raid",raid,&json!({"RaidIndex":raid,"RaidLevel":previous})).await.unwrap();
        }
        super::super::party_messages::send(&s,account(&users[1]),account(&users[0]),"PartyRoomReady",json!({"IsReady":true})).await.unwrap();
        for(i,user) in users.iter().enumerate() {assert_eq!(call(&s,user,"campaign/begin_campaign",&args[i]).await["Result"],"Success");}
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        let mut receivers=vec![];
        for user in &users {
            let a=account(user);let(tx,rx)=mpsc::channel(100);receivers.push(rx);
            s.conquest.peers.insert(a,Peer{id:a.to_string(),run:id.clone(),sender:tx.clone()});
            player_packet(&s,a,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();
            player_packet(&s,a,"BattleStartReq",json!({"RequestId":2}),&tx).await.unwrap();
        }
        // Native Arcdim replaces Delta's team: only Zeta's heroes remain.
        let creatures:Vec<_>=(5..=8).map(|h|json!({"TeamId":0,"AccountId":account(&users[1]),"Index":h,"Hp":10})).collect();
        let result=json!({"RunId":id,"Win":won,"Creatures":creatures,"TimeMs":60000,"MaxWaveIndex":2});
        worker_packet(&s,"raid-worker","WorkerResult",result.clone()).await.unwrap();
        for user in &users {
            let a=account(user);let mut db=s.db.acquire().await.unwrap();
            let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(a).fetch_one(&mut *db).await.unwrap();
            let entry:Value=read_json(&raw).unwrap();
            if won {
                assert_eq!(entry["PartyEndResponse"]["CompletedRaidInfo"]["RaidIndex"],raid);
                assert_eq!(entry["PartyEndResponse"]["CompletedRaidInfo"]["RaidLevel"],expected,"Arcdim {raid}, stage {level}, previous {previous}");
            } else {assert!(entry["PartyEndResponse"]["CompletedRaidInfo"].is_null());}
            assert_eq!(get(&mut db,a,"raid",raid).await.unwrap()["RaidLevel"],expected);
            let next=row(&s,"Raid",&[("Index",raid),("Level",expected)]).unwrap();
            super::super::dungeons::require_raid_join_unlock(&mut db,&s,a,next).await.unwrap();
        }
        worker_packet(&s,"raid-worker","WorkerResult",result).await.unwrap();
        for user in &users {assert_eq!(get(&mut s.db.acquire().await.unwrap(),account(user),"raid",raid).await.unwrap()["RaidLevel"],expected);}
    }}
}

#[tokio::test]
#[ignore = "Requires CONQUEST_NATIVE_EXECUTABLE; clears Delta and Zeta with both party accounts"]
async fn native_arcdim_coop_transitions_to_zeta_and_settles_both_players() {
    struct Native(std::process::Child);
    impl Drop for Native {fn drop(&mut self){let _=self.0.kill();let _=self.0.wait();}}
    for raid in [401,402] {
        let (mut s,users,args)=arcdim_fixture(raid,1,false).await;
        super::super::party_messages::send(&s,account(&users[1]),account(&users[0]),"PartyRoomReady",json!({"IsReady":true})).await.unwrap();
        let key=uuid::Uuid::new_v4().to_string();s.battle_service_key=Arc::new(Some(key.clone()));
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let port=listener.local_addr().unwrap().port();
        let server=tokio::spawn(serve(listener,s.clone()));
        let executable=std::path::PathBuf::from(std::env::var("CONQUEST_NATIVE_EXECUTABLE").unwrap()).canonicalize().unwrap();
        let mut command=std::process::Command::new(&executable);
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join(format!("target/arcdim-ready-fix/native-{raid}.log")))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1").env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] {use std::os::windows::process::CommandExt;command.creation_flags(0x08000000);}
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(90),async {
            while s.conquest.worker.read().await.is_none() {tokio::time::sleep(std::time::Duration::from_millis(100)).await;}
        }).await.expect("Arcdim worker did not authenticate");
        for (index,user) in users.iter().enumerate() {
            let start=call(&s,user,"campaign/begin_campaign",&args[index]).await;
            assert_eq!(start["Result"],"Success","{start}");
        }
        let id=s.conquest.runs.iter().next().unwrap().key().clone();let mut clients=vec![];
        {
            // This isolated fixture uses unequipped heroes. Give only its
            // trusted simulation sufficient stats to clear both native bosses.
            let handle=s.conquest.runs.get(&id).unwrap().value().clone();let mut run=handle.lock().await;
            for info in run.payload["Simulation"]["AccountInfos"].as_array_mut().unwrap() {
                info["ExtraStatDatas"]=json!([
                    {"OptionType":"ModMaxHp","OptionValue":1000000000},
                    {"OptionType":"ModAttackPower","OptionValue":100000000},
                    {"OptionType":"ModDefensePower","OptionValue":100000000}]);
            }
        }
        let guest=account(&users[1]);
        for user in &users {
            let a=account(user);let(tx,mut rx)=mpsc::channel(1000);
            s.conquest.peers.insert(a,Peer{id:a.to_string(),run:id.clone(),sender:tx.clone()});let state=s.clone();
            clients.push(tokio::spawn(async move {
                player_packet(&state,a,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();
                player_packet(&state,a,"BattleStartReq",json!({"RequestId":2}),&tx).await.unwrap();
                let mut heroes=BTreeSet::new();let mut entered=BTreeSet::new();let mut waves=BTreeSet::new();let mut switched=false;
                let mut wave=0;let mut heartbeat=tokio::time::interval(std::time::Duration::from_secs(1));
                loop {
                    let(name,body)=tokio::select! {
                        packet=rx.recv()=>packet.unwrap(),
                        _=heartbeat.tick()=>{
                            player_packet(&state,a,"PingReq",json!({"RequestId":4,"BattleStopped":true,"ReplayWaveIndex":wave}),&tx).await.unwrap();continue;
                        }
                    };
                    if name=="SetNextTeamNot" {
                        assert!(!switched);assert_eq!(number(&body,"DeckIndex"),1);switched=true;
                    }
                    if name=="WaveReadyNot" {
                        wave=number(&body,"WaveIndex");waves.insert(wave);
                        if wave==2 {assert!(switched,"Zeta must follow the team-change notification");}
                        println!("Arcdim {raid} player {a}: native wave {wave} ready");
                        player_packet(&state,a,"BattleStartReq",json!({"RequestId":2}),&tx).await.unwrap();
                        player_packet(&state,a,"BattleWaveResumeReq",json!({"RequestId":3}),&tx).await.unwrap();
                    }
                    if name=="BattleLogNot" {
                        for log in body["BattleLogs"].as_array().into_iter().flatten() {
                            let data:Value=if let Some(raw)=log["Data"].as_str() {serde_json::from_str(raw).unwrap()}else{log["Data"].clone()};
                            if log["Type"]=="PlayerEnter" {entered.insert(number(&data["PlayerCreateInfo"],"Id"));}
                            for hero in data["CreatureCreateInfos"].as_array().into_iter().flatten() {
                                let index=number(&hero["HeroInfo"],"HeroIndex");
                                if index>0 {
                                    assert_eq!(number(hero,"GroupIndex"),0,"The second team must wait for its wave");
                                    heroes.insert(index);
                                }
                            }
                            if log["Type"]=="CreatureCreate" {
                                let index=number(&data["CreateInfo"]["HeroInfo"],"HeroIndex");
                                if index>0 && index<1000 {
                                    assert!(switched || wave==1);
                                    assert!((5..=8).contains(&index),"Only the reserved team replaces Delta's team");
                                    assert_eq!(number(&data,"PlayerId"),guest,"Zeta heroes retain their actual owner");
                                    assert_eq!(number(&data["CreateInfo"],"GroupIndex"),0,"Wave teams share the native battle area");
                                    heroes.insert(index);
                                }
                            }
                        }
                    }
                    if name=="BattleResultNot" {
                        assert_eq!(entered.len(),2);assert!(switched);
                        assert_eq!(waves,BTreeSet::from([1,2]));
                        assert_eq!(heroes,(1..=8).collect());
                        assert_eq!(body["Win"],true);assert_eq!(number(&body,"MaxWaveIndex"),2);
                        return body;
                    }
                }
            }));
        }
        let mut results=vec![];
        for client in clients {results.push(tokio::time::timeout(std::time::Duration::from_secs(180),client).await.expect("Arcdim native transition timed out").unwrap());}
        assert_eq!(results[0],results[1]);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=1").fetch_one(&s.db).await.unwrap(),2);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
        println!("Native Arcdim {raid}: Delta cleared, Zeta team created with correct ownership, both players settled");
        server.abort();
    }
}

#[tokio::test]
async fn two_player_field_and_technomagic_parties_fill_the_total_hero_cap() {
    for (raid,levels,cap,total) in [(32,vec![1,2],3,6),(33,vec![1,2],3,6),(34,vec![1,2],4,7),
        (201,vec![1,9],4,7),(202,vec![1,9],4,7),(203,vec![1,9],4,7)] {
        for level in levels {
            let (s,users,args)=fixture_for_players(raid,level,2).await;
            let _worker=fake_worker(&s).await;
            let def=row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap();
            assert_eq!(n(def,"DeckCountPerPlayer"),cap);
            assert_eq!(n(def,"DeckCount"),total);
            for (i,user) in users.iter().enumerate() {
                let result=call(&s,user,"campaign/begin_campaign",&args[i]).await;
                assert_eq!(result["Result"],"Success","two-player raid {raid}/{level}: {result}");
            }
            let run=s.conquest.runs.iter().next().unwrap().value().clone();let run=run.lock().await;
            let accounts=run.payload["Simulation"]["AccountInfos"].as_array().unwrap();
            assert_eq!(accounts.len(),2);
            assert_eq!(accounts[0]["HeroInfos"].as_object().unwrap().len() as i64,cap);
            assert_eq!(accounts.iter().map(|a|a["HeroInfos"].as_object().unwrap().len() as i64).sum::<i64>(),total);
        }
    }
}

#[tokio::test]
async fn field_and_technomagic_coop_accept_full_rosters_and_enforce_caps() {
    for (raid,level) in [(31,1),(32,1),(33,1),(34,1),(201,1),(202,1),(203,1),(31,2),(34,2),(201,9),(202,9),(203,9)] {
        let (s,users,args)=fixture(raid,level).await;let _worker=fake_worker(&s).await;
        let total=n(row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap(),"DeckCount");
        // A full combined roster is permitted; each account remains bounded.
        let begun=call(&s,&users[0],"campaign/begin_campaign",&args[0]).await;
        assert_eq!(begun["Result"],"Success","raid {raid}/{level}: {begun}");
        for i in 1..users.len() {
            let begun=call(&s,&users[i],"campaign/begin_campaign",&args[i]).await;
            assert_eq!(begun["Result"],"Success","guest, raid {raid}: {begun}");
        }
        let run=s.conquest.runs.iter().next().unwrap().value().clone();let run=run.lock().await;
        assert_eq!(run.payload["Simulation"]["AccountInfos"].as_array().unwrap().len(),3);
        let count:usize=run.payload["Simulation"]["AccountInfos"].as_array().unwrap().iter()
            .map(|a|a["HeroInfos"].as_object().unwrap().len()).sum();
        assert_eq!(count as i64,total);
        let id=run.id.clone();
        let creatures:Vec<_>=run.payload["Simulation"]["AccountInfos"].as_array().unwrap().iter().flat_map(|a| {
            a["HeroInfos"].as_object().unwrap().values().map(|h|json!({"TeamId":0,"AccountId":a["UserInfo"]["AccountId"],"Index":h["HeroIndex"],"Hp":10})).collect::<Vec<_>>()
        }).collect();
        drop(run);
        let mut receivers=vec![];
        for user in &users {
            let a=account(user);let (tx,rx)=mpsc::channel(100);receivers.push(rx);
            s.conquest.peers.insert(a,Peer{id:a.to_string(),run:id.clone(),sender:tx.clone()});
            player_packet(&s,a,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();
            player_packet(&s,a,"BattleStartReq",json!({"RequestId":2}),&tx).await.unwrap();
        }
        let report=json!({"RunId":id,"Win":true,"Creatures":creatures,"TimeMs":60000,"MaxWaveIndex":1});
        worker_packet(&s,"raid-worker","WorkerResult",report.clone()).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=1").fetch_one(&s.db).await.unwrap(),3);
        let count_before:i64=sqlx::query_scalar("SELECT COUNT(*) FROM equip_items").fetch_one(&s.db).await.unwrap();
        for user in &users {
            let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(account(user)).fetch_one(&s.db).await.unwrap();
            let entry:Value=read_json(&raw).unwrap();let ended=&entry["PartyEndResponse"];
            assert_eq!(ended["CompletedRaidInfo"]["RaidIndex"],raid);
            assert_eq!(ended["CompletedRaidInfo"]["RaidLevel"],(level+1).min(if raid<100 {2}else{9}));
            assert!(!ended["ItemResults"].as_array().unwrap().is_empty() || !ended["EquipItemInfos"].as_array().unwrap().is_empty(),"raid {raid} must award loot");
        }
        worker_packet(&s,"raid-worker","WorkerResult",report).await.unwrap();
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM equip_items").fetch_one(&s.db).await.unwrap(),count_before);
    }
    for (raid,over_player,over_total) in [(31,false,true),(32,true,false),(34,false,true),(201,true,false),(203,false,true)] {
        let (s,users,args)=fixture(raid,1).await;let _worker=fake_worker(&s).await;
        let a=account(&users[2]);
        let def=row(&s,"Raid",&[("Index",raid),("Level",1)]).unwrap();
        let count=if over_player {n(def,"DeckCountPerPlayer")+1}else{3};
        let deck:serde_json::Map<_,_>=(20..20+count).map(|h|(h.to_string(),json!({"HeroIndex":h}))).collect();
        for hero in 20..20+count {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                .bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
        put(&mut s.db.acquire().await.unwrap(),a,"party_member",1,&json!({"DeckHeros":deck,"SubDeckHeros":{},"IsBattleReady":true})).await.unwrap();
        let before:Vec<i64>=sqlx::query_scalar("SELECT stamina FROM user_info ORDER BY account_id").fetch_all(&s.db).await.unwrap();
        assert_eq!(call(&s,&users[0],"campaign/begin_campaign",&args[0]).await["Result"],"NotMatchHeroIndices","raid {raid}, total={over_total}");
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs").fetch_one(&s.db).await.unwrap(),0);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info ORDER BY account_id").fetch_all(&s.db).await.unwrap(),before);
    }
}

#[tokio::test]
async fn field_and_technomagic_dispatch_infers_solo_raid_and_settles_flat_rosters_once() {
    for raid in [41,42,43,44,301,302,303] {
        for level in [1,if raid<100 {2}else{9}] {
            let (s,u)=setup().await;let a=account(&u);unlock(&s,a).await;
            sqlx::query("UPDATE user_info SET stamina=10000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
            for hero in (1..=10).chain([21,40]) {
                sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                    .bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
            }
            sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
            let def=row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap();
            let chapter=n(def,"ChapterIndex");let dungeon=n(def,"DungeonIndex");
            let data=row(&s,"CampaignDungeon",&[("ChapterIndex",chapter),("DungeonIndex",dungeon)]).unwrap();
            let cap=n(data,"DispatchDeckCount");assert_eq!(cap,if raid==41 {4}else if raid<100 {6}else{7});
            let cost=campaign::stamina_cost(data,0);
            let heroes:Vec<_>=(1..=cap).map(|h|if raid>=300 && h==5 {10}else{h}).collect();
            let wire=heroes.iter().map(|h|format!("HeroIndices={h}")).collect::<Vec<_>>().join("&");
            let args=format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&Difficulty=0&RepeatCount=2&DeckIndex=1&{wire}");
            put(&mut s.db.acquire().await.unwrap(),a,"raid",n(def,"ClearRaidIndex"),
                &json!({"RaidIndex":def["ClearRaidIndex"],"RaidLevel":level})).await.unwrap();
            // Dispatch requires a clear of this exact stage, not just its unlock.
            assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
            put(&mut s.db.acquire().await.unwrap(),a,"dungeon",campaign::key(chapter,dungeon),
                &json!({"ChapterIndex":chapter,"DungeonIndex":dungeon,"FirstRewardedDiff":1,"MaxStar":3})).await.unwrap();
            assert_ne!(call(&s,&u,"dispatch/start_dispatch",&format!("{args}&HeroIndices={}",cap+1)).await["Result"],"Success");
            if raid>=300 {
                let invalid=format!("ChapterIndex={chapter}&DungeonIndex={dungeon}&Difficulty=0&RepeatCount=2&DeckIndex=1&HeroIndices=[1,5,21,40,2,3,4]");
                assert_ne!(call(&s,&u,"dispatch/start_dispatch",&invalid).await["Result"],"Success","Four warriors must fail the stage's class limit");
            }
            sqlx::query("UPDATE heroes SET level=1 WHERE account_id=? AND hero_index=1").bind(a).execute(&s.db).await.unwrap();
            assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
            sqlx::query("UPDATE heroes SET level=100 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
            let stamina=||sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db);
            assert_eq!(stamina().await.unwrap(),10000);
            let start=call(&s,&u,"dispatch/start_dispatch",&format!("{args}&RaidIndex=999&RaidLevel=999")).await;
            if start["Result"]!="Success" {
                let request=Request::parse(args.as_bytes()).unwrap();
                let reason=dispatch::execute(&mut s.db.acquire().await.unwrap(),&s,a,&request,"start_dispatch").await;
                panic!("raid dispatch {raid}/{level}: {reason:?}");
            }
            assert_eq!(start["Result"],"Success","raid dispatch {raid}/{level}: {start}");
            assert_eq!(start["DispatchBattleInfo"]["Request"]["RaidIndex"],raid.to_string());
            assert_eq!(start["DispatchBattleInfo"]["Request"]["RaidLevel"],level.to_string());
            assert_eq!(start["DispatchBattleInfo"]["HeroIndices"],json!(heroes).to_string());
            assert_eq!(stamina().await.unwrap(),10000-2*cost);
            let slot=n(&start["DispatchBattleInfo"],"SlotIndex");let slot_args=format!("SlotIndex={slot}");
            assert_ne!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
            assert_eq!(call(&s,&u,"dispatch/cancel_dispatch",&slot_args).await["Result"],"Success");
            assert_eq!(stamina().await.unwrap(),10000);
            assert_eq!(call(&s,&u,"dispatch/start_dispatch",&args).await["Result"],"Success");
            prepared_dispatch(&s,&u,slot,&[1,0]).await;
            let mut run=get(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot).await.unwrap();
            run["FinishTimestamp"]=json!(now()-1);
            put(&mut s.db.acquire().await.unwrap(),a,"dispatch",slot,&run).await.unwrap();
            let finished=call(&s,&u,"dispatch/request_complete_dispatch",&slot_args).await;
            assert_eq!(finished["Result"],"Success","dispatch collection {raid}/{level}: {finished}");
            assert_eq!(finished["DispatchBattleInfo"]["WinCount"],1);
            assert_eq!(finished["DispatchBattleInfo"]["LoseCount"],1);
            assert_eq!(stamina().await.unwrap(),10000-cost);
            let mails:i64=sqlx::query_scalar("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='Dispatch equipment rewards'").bind(a).fetch_one(&s.db).await.unwrap();
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mail_notifications q JOIN mails m ON m.mail_id=q.mail_id WHERE m.account_id=? AND m.title='Dispatch equipment rewards'").bind(a).fetch_one(&s.db).await.unwrap(),mails);
            assert!(mails>0 || finished["ItemResults"].as_array().is_some_and(|v|!v.is_empty()),
                "Dispatch {raid}/{level} must grant item rewards or mail rolled equipment: {finished}");
            assert_ne!(call(&s,&u,"dispatch/request_complete_dispatch",&slot_args).await["Result"],"Success");
            assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM mails WHERE account_id=? AND title='Dispatch equipment rewards'").bind(a).fetch_one(&s.db).await.unwrap(),mails);
            assert_eq!(call(&s,&u,"dispatch/get_dispatch_list","").await["DispatchBattleInfos"],json!([]));
        }
    }
}

#[tokio::test]
async fn field_and_technomagic_solo_subparty_is_counted_once() {
    for (raid,total,sub) in [(41,4,0),(42,6,2),(43,6,2),(44,7,3),(301,7,3),(302,7,3),(303,7,3)] {
        let (s,u)=setup().await;let a=account(&u);unlock(&s,a).await;
        for hero in 1..=10 {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                .bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
        sqlx::query("UPDATE heroes SET level=100").execute(&s.db).await.unwrap();
        sqlx::query("UPDATE user_info SET stamina=1000 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
        for level in [1,if raid<100 {2}else{9}] {
            let def=row(&s,"Raid",&[("Index",raid),("Level",level)]).unwrap();
            put(&mut s.db.acquire().await.unwrap(),a,"raid",n(def,"ClearRaidIndex"),
                &json!({"RaidIndex":def["ClearRaidIndex"],"RaidLevel":level})).await.unwrap();
            let prefix=format!("ChapterIndex={}&DungeonIndex={}&DungeonDifficulty=0&RaidIndex={raid}&RaidLevel={level}",n(def,"ChapterIndex"),n(def,"DungeonIndex"));
            let heroes:Vec<_>=(1..=total).map(|id|if raid>=300 && id==5 {10}else{id}).collect();
            let support=heroes[(total-sub) as usize..].to_vec();
            // RaidManagement uses AI support (not wave groups), and WWWForm
            // repeats the array keys. Exercise exactly that native request.
            let wire=heroes.iter().map(|id|format!("HeroIndices={id}"))
                .chain(support.iter().map(|id|format!("AiHeroIndices={id}"))).collect::<Vec<_>>().join("&");
            let entry=format!("{prefix}&{wire}");
            let before:i64=sqlx::query_scalar("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap();
            for invalid in [
                format!("{prefix}&HeroIndices={}&AiHeroIndices={}",json!((1..=total+1).collect::<Vec<_>>()),json!(support)),
                format!("{prefix}&HeroIndices={}&AiHeroIndices=[99]",json!(heroes)),
                format!("{entry}&GroupHeroIndices=[1]"),
            ] {
                assert_eq!(call(&s,&u,"campaign/begin_campaign",&invalid).await["Result"],"NotMatchHeroIndices","{invalid}");
                assert_eq!(sqlx::query_scalar::<_,i64>("SELECT stamina FROM user_info WHERE account_id=?").bind(a).fetch_one(&s.db).await.unwrap(),before);
            }
            if sub>0 {
                for invalid in [
                    format!("{prefix}&HeroIndices={}&AiHeroIndices=[]",json!(heroes)),
                    format!("{prefix}&HeroIndices={}&AiHeroIndices={}",json!(heroes),json!(&heroes[(total-sub-1) as usize..])),
                ] {
                    assert_eq!(call(&s,&u,"campaign/begin_campaign",&invalid).await["Result"],"NotMatchHeroIndices");
                }
                let invalid=format!("{entry}&AiHeroIndices={}",support[0]);
                assert_eq!(call(&s,&u,"campaign/begin_campaign",&invalid).await["Result"],"DuplicatedHero");
            }
            let result=call(&s,&u,"campaign/begin_campaign",&entry).await;
            assert_eq!(result["Result"],"Success","raid {raid}/{level}: {result}");
            let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(a).fetch_one(&s.db).await.unwrap();
            let stored:Value=read_json(&raw).unwrap();assert_eq!(stored["Heroes"],json!(heroes));
            let end=format!("{prefix}&Completed=true&Star=3&AliveHeroIndices=[1]");
            let ended=call(&s,&u,"campaign/end_campaign",&end).await;
            assert_eq!(ended["Result"],"Success","raid {raid}/{level}: {ended}");
            assert_eq!(ended["CompletedRaidInfo"]["RaidIndex"],def["ClearRaidIndex"]);
            assert_eq!(ended["CompletedRaidInfo"]["RaidLevel"],if raid<100 {2}else{(level+1).min(9)});
        }
    }
}

#[tokio::test]
async fn technomagic_class_limit_counts_support_once_and_all_coop_members_together() {
    let (s,_)=setup().await;
    for (heroes,group,valid) in [("[1,5,2,3,21,4,6]","[21,4,6]",true),("[1,5,2,3,21,4,40]","[21,4,40]",false)] {
        let req=Request::parse(format!("ChapterIndex=10&DungeonIndex=2001&RaidIndex=301&RaidLevel=1&HeroIndices={heroes}&AiHeroIndices={group}").as_bytes()).unwrap();
        assert_eq!(restrictions::party(&s,&req,10103).is_ok(),valid);
    }
    let (s,users,args)=fixture(201,1).await;let _worker=fake_worker(&s).await;
    for (i,roster) in [vec![1,5,2],vec![21,40],vec![3,4]].iter().enumerate() {
        let a=account(&users[i]);
        for hero in roster {
            sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                .bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();
        }
        let deck:serde_json::Map<_,_>=roster.iter().map(|h|(h.to_string(),json!({"HeroIndex":h}))).collect();
        put(&mut s.db.acquire().await.unwrap(),a,"party_member",1,&json!({"IsBattleReady":true,"DeckHeros":deck,"SubDeckHeros":{}})).await.unwrap();
    }
    let result=call(&s,&users[0],"campaign/begin_campaign",&args[0].replace("HeroIndices=[1,2,3]","HeroIndices=[1,5,2]")).await;
    assert_eq!(result["Result"],"NotMatchHeroIndices","Four distinct warriors across owners violate the three-per-class raid rule");
    assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs").fetch_one(&s.db).await.unwrap(),0);
}

#[tokio::test]
#[ignore = "Requires CONQUEST_NATIVE_EXECUTABLE; runs real native field and Technomagic battles"]
async fn native_field_and_technomagic_coop_wave_and_settlement() {
    struct Native(std::process::Child);
    impl Drop for Native {fn drop(&mut self){let _=self.0.kill();let _=self.0.wait();}}
    let indices=std::env::var("SPRK_RAID_TEST_INDEX").ok().map(|s|vec![s.parse::<i64>().unwrap()])
        .unwrap_or_else(||vec![31,32,33,34,201,202,203]);
    for raid in indices {
        let (mut s,users,args)=fixture(raid,1).await;
        let key=uuid::Uuid::new_v4().to_string();s.battle_service_key=Arc::new(Some(key.clone()));
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let port=listener.local_addr().unwrap().port();
        let server=tokio::spawn(serve(listener,s.clone()));
        let executable=std::path::PathBuf::from(std::env::var("CONQUEST_NATIVE_EXECUTABLE").unwrap()).canonicalize().unwrap();
        let mut command=std::process::Command::new(&executable);
        std::fs::create_dir_all("target/raid-party-fix").unwrap();
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join(format!("target/raid-party-fix/native-{raid}.log")))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1").env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] {use std::os::windows::process::CommandExt;command.creation_flags(0x08000000);}
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(90),async {
            while s.conquest.worker.read().await.is_none() {tokio::time::sleep(std::time::Duration::from_millis(100)).await;}
        }).await.expect("Native worker did not authenticate");
        for (i,user) in users.iter().enumerate() {
            let begun=call(&s,user,"campaign/begin_campaign",&args[i]).await;
            assert_eq!(begun["Result"],"Success","raid {raid}: {begun}");
        }
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        let mut tasks=vec![];
        for user in &users {
            let a=account(user);let (tx,mut rx)=mpsc::channel(1000);
            s.conquest.peers.insert(a,Peer{id:a.to_string(),run:id.clone(),sender:tx.clone()});
            let state=s.clone();
            tasks.push(tokio::spawn(async move {
                player_packet(&state,a,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();
                let mut heartbeat=tokio::time::interval(std::time::Duration::from_secs(1));
                let mut waves=0;let mut logs=0;
                loop {
                    let packet=tokio::select! {
                        value=rx.recv()=>value.unwrap(),
                        _=heartbeat.tick()=>{
                            player_packet(&state,a,"PingReq",json!({"RequestId":2,"BattleStopped":true}),&tx).await.unwrap();continue;
                        }
                    };
                    if packet.0=="WaveReadyNot" {
                        waves+=1;
                        player_packet(&state,a,"BattleStartReq",json!({"RequestId":3}),&tx).await.unwrap();
                        player_packet(&state,a,"BattleWaveResumeReq",json!({"RequestId":4}),&tx).await.unwrap();
                    }
                    if packet.0=="BattleLogNot" {logs+=1;}
                    if packet.0=="BattleResultNot" {assert!(waves>0 && logs>0);return packet.1;}
                }
            }));
        }
        let mut results=vec![];
        for task in tasks {
            results.push(tokio::time::timeout(std::time::Duration::from_secs(240),task).await.expect("Native raid timed out").unwrap());
        }
        assert!(results.windows(2).all(|r|r[0]==r[1]));
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=1").fetch_one(&s.db).await.unwrap(),3);
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_entry_holds").fetch_one(&s.db).await.unwrap(),0);
        println!("Native raid {raid}: combat logs and authoritative settlement for all three accounts; win={}",results[0]["Win"]);
        server.abort();
    }
}
