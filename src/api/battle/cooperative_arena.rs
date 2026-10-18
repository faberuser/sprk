//! Arena uses the same authenticated native worker and ordered-log transport.
use super::*;
use crate::api::community::arena::online;

pub(crate) async fn arena_worker(s:&AppState)->Option<String> {
    let guard=s.conquest.worker.read().await;
    guard.as_ref().filter(|w|w.arena_version>=1 && !w.sender.is_closed()).map(|w|w.id.clone())
}

pub(crate) fn create_arena(s:&AppState,worker:String,info:Value) {
    let uid=number(&info,"Uid");
    let id=format!("arena:{uid}");
    let members=info["AccountInfos"].as_array().unwrap().iter().map(|v|number(&v["UserInfo"],"AccountId")).collect::<Vec<_>>();
    // ArenaContext.GetDifficulty hardcodes Easy (0). Normal (1) selects the
    // PvE DefeatAllEnemy rules instead of Victory's MoreCreatureAlive rules,
    // changing combat state and ending the worker while the replay still fights.
    let mut payload=json!({"RunId":id,"ArenaMatch":true,"MatchUid":uid,"MatchInfo":info,"BossHp":0,"BossIndex":0,"Seed":rand::random::<u32>()%i32::MAX as u32,
        "Simulation":{"Uid":uid.to_string(),"ChapterIndex":1000,"DungeonIndex":1,"DungeonDifficulty":0,"RepeatCount":1,"RemainCount":1,"AccountInfos":info["AccountInfos"]}});
    if info["ArenaType"]=="BanPick" {payload["Draft"]=honor::initial(&members);}
    s.conquest.runs.insert(id.clone(),Arc::new(Mutex::new(Run{id,worker,master:members[0],room:0,members,payload,
        entered:BTreeSet::new(),ready:BTreeSet::new(),sent:BTreeSet::new(),departed:BTreeSet::new(),offline:BTreeSet::new(),cancelled:BTreeSet::new(),
        logs:vec![],pending_actions:vec![],result:None,started:false,wave_started:false,prepared:false,created:now(),time_ms:0})));
}

pub(super) async fn charge(s:&AppState,run:&Run)->Result<()> {
    let mut db=s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(run.master).execute(&mut *db).await?;
    online::charge(&mut db,s,number(&run.payload,"MatchUid")).await?;
    db.commit().await?;
    Ok(())
}

pub(super) fn player_result(run:&Run,a:i64,result:&Value)->Value {
    if run.payload["ArenaMatch"]!=true {return result.clone();}
    let alive=result["Creatures"].as_array().into_iter().flatten().filter(|v|number(v,"AccountId")==a && number(v,"Hp")>0)
        .map(|v|json!(number(v,"Index"))).collect::<Vec<_>>();
    json!({"Win":number(result,"WinnerAccountId")==a,"Star":0,"Dropped":result["Canceled"]==true || result["GiveUp"]==true,
        "KillCount":{},"AliveHeroIndices":alive,"MaxWaveIndex":1})
}

pub(super) async fn finish(s:&AppState,run:&mut Run,body:&Value,give_up:Option<i64>)->Result<()> {
    if run.result.is_some() {return Ok(());}
    let winner = if let Some(loser)=give_up {
        *run.members.iter().find(|v|**v!=loser).ok_or_else(||rule("WrongMember"))?
    } else {
        // A native time-limit draw is cancelled without score/reward changes.
        if body["Winner"].as_str().is_some_and(|v|!matches!(v,"Left"|"Right")) {return cancel(s,run).await;}
        run.members[if body["Win"]==true {0}else{1}]
    };
    let uid=number(&run.payload,"MatchUid");
    let mut db=s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(run.master).execute(&mut *db).await?;
    let valid:bool=sqlx::query_scalar("SELECT count(*)=2 FROM arena_runs WHERE status='battle' AND json_extract(data,'$.OnlineMatchUid')=? AND json_extract(data,'$.Season')=?")
        .bind(uid).bind(crate::api::community::arena::season(s).0).fetch_one(&mut *db).await?;
    if !valid {
        drop(db);
        if sync_finished(s,run).await? {return Ok(());}
        return cancel(s,run).await;
    }
    online::finish(&mut db,s,uid,winner,body,give_up).await?;
    db.commit().await?;
    run.result=Some(json!({"WinnerAccountId":winner,"Creatures":body["Creatures"],"GiveUp":give_up.is_some()}));
    // ArenaContext waits for the replay to finish after BattleResultNot. Do
    // not wait for a post-wave heartbeat: the native play loop can stop those
    // at the end, leaving an otherwise settled match frozen on screen.
    let delivered=notify_result(s,run,run.result.as_ref().unwrap());
    run.sent.extend(delivered);
    if give_up.is_some() {
        let _=s.conquest.send_worker("WorkerCancel",json!({"RunId":run.id})).await;
    }
    Ok(())
}

fn notify_result(s:&AppState,run:&Run,result:&Value)->BTreeSet<i64> {
    let mut delivered=BTreeSet::new();
    for a in &run.members {
        if let Some(peer)=s.conquest.peers.get(a) {
            if peer.run==run.id && peer.sender.try_send(("BattleResultNot".into(),player_result(run,*a,result))).is_ok() {
                delivered.insert(*a);
            }
        }
    }
    delivered
}

pub(super) async fn depart(s:&AppState,run:&mut Run,a:i64)->Result<()> {
    if run.payload["Draft"].is_object() && !run.started {
        // Leaving an accepted draft is a forfeit. Charge once even though the
        // combat loading barrier has not been reached yet.
        charge(s,run).await?;
        return finish(s,run,&json!({"Creatures":[],"TimeMs":0}),Some(a)).await;
    }
    if !run.started {return cancel(s,run).await;}
    finish(s,run,&json!({"Creatures":[],"TimeMs":run.time_ms}),Some(a)).await
}

pub(super) async fn cancel(s:&AppState,run:&Run)->Result<()> {
    let mut db=s.db.begin().await?;
    sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(run.master).execute(&mut *db).await?;
    online::cancel(&mut db,s,number(&run.payload,"MatchUid")).await?;
    db.commit().await?;
    let _=notify_result(s,run,&json!({"Canceled":true}));
    let _=s.conquest.send_worker("WorkerCancel",json!({"RunId":run.id})).await;
    s.conquest.runs.remove(&run.id);
    Ok(())
}

/// Called after the HTTP cancellation transaction commits, so lock ordering
/// stays run -> DB throughout the battle transport.
pub(crate) async fn http_cancelled(s:&AppState,a:i64)->Result<()> {
    let handles:Vec<_>=s.conquest.runs.iter().map(|v|v.value().clone()).collect();
    for handle in handles {
        let mut run=handle.lock().await;
        if run.payload["ArenaMatch"]==true && run.members.contains(&a) && run.result.is_none() {
            sync_finished(s,&mut run).await?;
        }
    }
    Ok(())
}

pub(super) async fn sync_finished(s:&AppState,run:&mut Run)->Result<bool> {
    let rows:Vec<(i64,String)>=sqlx::query_as("SELECT account,data FROM arena_runs WHERE status='complete' AND json_extract(data,'$.OnlineMatchUid')=?")
        .bind(number(&run.payload,"MatchUid")).fetch_all(&s.db).await?;
    if rows.len()!=2 {return Ok(false);}
    let mut winner=0;let mut canceled=false;
    for (a,raw) in rows {
        let data:Value=read_json(&raw)?;
        canceled |= data["End"]["MatchResult"]["Canceled"]==true;
        if data["End"]["MatchResult"]["Win"]==true {winner=a;}
    }
    if canceled {cancel(s,run).await?;} else {
        run.result=Some(json!({"WinnerAccountId":winner,"Creatures":[],"GiveUp":true}));
        let delivered=notify_result(s,run,run.result.as_ref().unwrap());
        run.sent.extend(delivered);
        let _=s.conquest.send_worker("WorkerCancel",json!({"RunId":run.id})).await;
    }
    Ok(true)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup,login,call};
    use crate::websocket::tests::{server,Client};

    async fn fixture()->(AppState,Vec<Value>,mpsc::Receiver<Packet>) {
        let s=setup().await;
        let users=vec![login(&s,"arena-live-a").await,login(&s,"arena-live-b").await,login(&s,"arena-live-c").await];
        let (tx,rx)=mpsc::channel(100);
        *s.conquest.worker.write().await=Some(Worker{id:"arena-worker".into(),sender:tx,arena_version:1});
        (s,users,rx)
    }
    async fn register(s:&AppState,u:&Value) {
        let out=call(s,u,"match/register_match","ArenaType=Normal&HeroIndices=1&LeaderHeroIndex=1").await;
        assert_eq!(out["Result"],"Success","{out}");
    }
    async fn pair(s:&AppState,users:&[Value])->Value {
        register(s,&users[0]).await;register(s,&users[1]).await;
        let matched=call(s,&users[0],"match/wait_match","PlayOfflineMatch=true").await;
        assert_eq!(matched["Result"],"Success","{matched}");
        matched
    }
    async fn sword(s:&AppState,u:&Value)->i64 {
        sqlx::query_scalar("SELECT sword FROM user_info WHERE account_id=?").bind(number(&u["UserInfo"],"AccountId")).fetch_one(&s.db).await.unwrap()
    }
    async fn transport(s:&AppState,users:&[Value],matched:&Value)->(crate::websocket::tests::Server,Vec<Client>) {
        let server=server(s.clone()).await;
        let uid=number(&matched["MatchInfo"],"Uid");
        let mut clients=vec![];
        for u in users.iter().take(2) {
            let mut c=Client::connect(&server,"battle").await;
            c.send("BattleLoginReq",json!({"AccountId":u["UserInfo"]["AccountId"],"SessionKey":u["UserInfo"]["SessionKey"],"MatchUid":uid,"RequestId":1})).await;
            assert_eq!(c.recv("BattleLoginRes").await["Result"],"Success");
            clients.push(c);
        }
        (server,clients)
    }
    async fn start(_s:&AppState,clients:&mut [Client],worker:&mut mpsc::Receiver<Packet>,uid:i64) {
        for c in clients.iter_mut() {
            c.send("BattleEnterReq",json!({"RequestId":2})).await;
            assert_eq!(c.recv("BattleEnterRes").await["Result"],"Success");
        }
        let prepare=worker.recv().await.unwrap();assert_eq!(prepare.0,"WorkerPrepare");
        assert_eq!(prepare.1["ArenaMatch"],true);
        assert_eq!(prepare.1["MatchInfo"]["DungeonDifficulty"],"Easy");
        assert_eq!(prepare.1["Simulation"]["DungeonDifficulty"],0,
            "Worker must use ArenaContext's Easy ruleset, not the Normal PvE rules");
        let accounts=prepare.1["MatchInfo"]["AccountInfos"].as_array().unwrap();
        assert_eq!(accounts[0]["TeamType"],"Left");assert_eq!(accounts[1]["TeamType"],"Right");
        assert_eq!(accounts[0]["HeroInfos"],accounts[1]["HeroInfos"]); // shared hero IDs are valid on opposing teams
        for c in clients.iter_mut() {
            c.send("BattleStartReq",json!({"RequestId":3})).await;
            c.recv("BattleStartRes").await;
        }
        assert_eq!(worker.recv().await.unwrap(),("WorkerStart".into(),json!({"RunId":format!("arena:{uid}")})));
    }

    #[tokio::test]
    async fn arena_reservations_are_forwarded_while_the_countdown_is_paused() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        let id=format!("arena:{uid}");
        clients[0].send("ReserveOnSkillReq",json!({"RequestId":2,"CreatureUid":1,"SkillSlotIndex":1})).await;
        clients[0].recv("BaseCreatureActionRes").await;
        assert!(worker.try_recv().is_err(),"An unloaded worker cannot resolve creature ownership yet");
        for c in &mut clients {
            c.send("BattleEnterReq",json!({"RequestId":3})).await;c.recv("BattleEnterRes").await;
        }
        assert_eq!(worker.recv().await.unwrap().0,"WorkerPrepare");
        assert!(worker.try_recv().is_err());
        worker_packet(&s,"arena-worker","WorkerPacket",json!({"RunId":id,"Name":"WaveReadyNot","Body":{"WaveIndex":0,"TotalWaveIndex":1}})).await.unwrap();
        for c in &mut clients {c.recv("WaveReadyNot").await;}
        let (name,action)=worker.recv().await.unwrap();
        assert_eq!(name,"WorkerCommand");assert_eq!(action["Name"],"ReserveOnSkillReq");
        for (request,name,slot) in [(4,"ReserveOffSkillReq",0),(5,"ReserveOnSkillReq",2)] {
            clients[0].send(name,json!({"RequestId":request,"CreatureUid":1,"SkillSlotIndex":slot})).await;
            clients[0].recv("BaseCreatureActionRes").await;
            let (kind,action)=worker.recv().await.unwrap();assert_eq!(kind,"WorkerCommand");assert_eq!(action["Name"],name);
        }
        let handle=s.conquest.runs.get(&id).unwrap().clone();
        {
            let run=handle.lock().await;
            assert!(!run.started && !run.wave_started && run.pending_actions.is_empty());
        }
        assert!(worker.try_recv().is_err(),"Reservations must not start combat or advance its clock");
        for c in &mut clients {c.send("BattleStartReq",json!({"RequestId":6})).await;c.recv("BattleStartRes").await;}
        assert_eq!(worker.recv().await.unwrap().0,"WorkerStart");
        assert!(worker.try_recv().is_err(),"Countdown inputs must not be applied twice at start");
    }

    #[tokio::test]
    async fn arena_clock_progresses_without_actions_or_replay_history_growth() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        let id=format!("arena:{uid}");
        for time in [100,200,5000,10000] {
            let packet=json!({"BattleLogs":[],"ServerTimeMs":time});
            worker_packet(&s,"arena-worker","WorkerPacket",json!({"RunId":id,"Name":"BattleLogNot","Body":packet})).await.unwrap();
            for c in &mut clients {
                assert_eq!(c.recv("BattleLogNot").await,packet);
                c.send("PingReq",json!({"RequestId":time,"ClientTimeMs":time})).await;
                assert_eq!(c.recv("PingRes").await["ServerTimeMs"],time);
            }
        }
        let run=s.conquest.runs.get(&id).unwrap().clone();
        assert!(run.lock().await.logs.is_empty(),"Clock pulses must not exhaust the retained combat-log limit");
        let packet=json!({"BattleLogs":[{"No":"1","Type":"CreatureReserveOnSkill","GlobalTimeMs":"10100","Data":"{}"}],"ServerTimeMs":10100});
        worker_packet(&s,"arena-worker","WorkerPacket",json!({"RunId":id,"Name":"BattleLogNot","Body":packet})).await.unwrap();
        for c in &mut clients {assert_eq!(c.recv("BattleLogNot").await,packet);}
        assert_eq!(run.lock().await.logs,vec![("BattleLogNot".into(),packet)]);
    }

    #[tokio::test]
    async fn arena_online_first_shared_match_and_atomic_results() {
        let (s,users,mut worker)=fixture().await;
        let before=[sword(&s,&users[0]).await,sword(&s,&users[1]).await];
        let matched=pair(&s,&users).await;
        let uid=number(&matched["MatchInfo"],"Uid");
        assert!(matched["MatchedNpcInfo"].is_null());
        let again=call(&s,&users[1],"match/wait_match","PlayOfflineMatch=false").await;
        assert_eq!(again["MatchInfo"],matched["MatchInfo"]);
        assert_eq!(call(&s,&users[0],"match/wait_match","PlayOfflineMatch=true").await,matched);
        for i in 0..2 {assert_eq!(sword(&s,&users[i]).await,before[i]);}
        let forged=call(&s,&users[0],"match/set_offline_match_result","Win=1&PlayTime=1&AliveHeroIndices=1").await;
        assert_ne!(forged["Result"],"Success");
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        for i in 0..2 {assert_eq!(sword(&s,&users[i]).await,before[i]-1);}
        let body=json!({"RunId":format!("arena:{uid}"),"Win":false,"Winner":"Right","TimeMs":10000,"MaxWaveIndex":1,
            "Creatures":[{"Index":"1","AccountId":users[0]["UserInfo"]["AccountId"],"TeamId":"0","Hp":"0"},
                         {"Index":"1","AccountId":users[1]["UserInfo"]["AccountId"],"TeamId":"1","Hp":"100"}]});
        worker_packet(&s,"arena-worker","WorkerResult",body.clone()).await.unwrap();
        worker_packet(&s,"arena-worker","WorkerResult",body).await.unwrap();
        for (i,c) in clients.iter_mut().enumerate() {
            c.send("PingReq",json!({"RequestId":4,"BattleStopped":true})).await;
            let result=c.recv("BattleResultNot").await;
            assert_eq!(result["Win"],i==1);
            assert_eq!(result["AliveHeroIndices"],if i==1{json!([1])}else{json!([])});
            c.recv("PingRes").await;
            let result=call(&s,&users[i],"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(result["Result"],"Success","{result}");
            assert_eq!(result["MatchResult"]["MatchUid"],uid);
            assert_eq!(result["MatchResult"]["NewMatchScore"],if i==1{1020}else{990});
            assert_eq!(sword(&s,&users[i]).await,before[i]-1);
        }
        let scores:Vec<(i64,i64)>=sqlx::query_as("SELECT wins,losses FROM arena_scores WHERE account IN (?,?) ORDER BY account")
            .bind(number(&users[0]["UserInfo"],"AccountId")).bind(number(&users[1]["UserInfo"],"AccountId")).fetch_all(&s.db).await.unwrap();
        assert_eq!(scores,vec![(0,1),(1,0)]);
    }

    #[tokio::test]
    async fn arena_terminal_result_is_pushed_without_a_final_heartbeat_and_replayed_on_reconnect() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        let id=format!("arena:{uid}");
        for c in &mut clients {
            c.send("PingReq",json!({"RequestId":4,"ClientTimeMs":71000,"ReplayWaveIndex":1,"BattleStopped":false})).await;
            c.recv("PingRes").await;
        }
        worker_packet(&s,"arena-worker","WorkerPacket",json!({"RunId":id,"Name":"WaveEndNot","Body":{"WaveIndex":1,"Winner":"Left"}})).await.unwrap();
        for c in &mut clients {c.recv("WaveEndNot").await;}
        let result=json!({"RunId":id,"Win":true,"Winner":"Left","TimeMs":71000,"Creatures":[]});
        worker_packet(&s,"arena-worker","WorkerResult",result.clone()).await.unwrap();
        worker_packet(&s,"arena-worker","WorkerResult",result).await.unwrap();
        // Native arena can finish with no further PingReq at all. Waiting for
        // BattleStopped=true used to leave both clients on the frozen field.
        for (i,c) in clients.iter_mut().enumerate() {
            assert_eq!(c.recv("BattleResultNot").await["Win"],i==0);
            c.send("PingReq",json!({"RequestId":5,"BattleStopped":true,"ReplayWaveIndex":1})).await;
            c.recv("PingRes").await; // no duplicate result
            let saved=call(&s,&users[i],"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(saved["MatchResult"]["Win"],i==0);
            assert_eq!(saved["MatchResult"]["GiveUp"],false);
        }
        let mut old=clients.remove(0);
        old.socket.close(None).await.unwrap();old.closed().await;
        let a=number(&users[0]["UserInfo"],"AccountId");
        tokio::time::timeout(std::time::Duration::from_secs(5),async {
            while s.conquest.peers.contains_key(&a) {tokio::time::sleep(std::time::Duration::from_millis(10)).await;}
        }).await.unwrap();
        let mut again=Client::connect(&server,"battle").await;
        again.send("BattleLoginReq",json!({"AccountId":a,"SessionKey":users[0]["UserInfo"]["SessionKey"],"MatchUid":uid,"RequestId":6})).await;
        again.recv("BattleLoginRes").await;
        again.send("BattleEnterReq",json!({"RequestId":7})).await;
        again.recv("WaveEndNot").await;
        assert_eq!(again.recv("BattleResultNot").await["Win"],true);
        again.recv("BattleEnterRes").await;
        again.send("PingReq",json!({"RequestId":8,"BattleStopped":true})).await;
        again.recv("PingRes").await;
    }

    #[tokio::test]
    async fn arena_emoticons_reach_both_players_without_disconnect_or_forfeit() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        let before=[sword(&s,&users[0]).await,sword(&s,&users[1]).await];
        let outsider=login(&s,"arena-emote-outsider").await;
        let other_users=vec![users[2].clone(),outsider];
        let other_match=pair(&s,&other_users).await;
        let (_other_server,mut other_clients)=transport(&s,&other_users,&other_match).await;
        let receivers=users.iter().take(2).map(|u|u["UserInfo"]["AccountId"].clone()).collect::<Vec<_>>();
        for sender in 0..2 {
            clients[sender].send("BattleMessageReq",json!({"RequestId":"10","GroupType":"None","ReceiverIds":null,"Message":"1",
                "SenderId":other_users[0]["UserInfo"]["AccountId"]})).await;
            for c in &mut clients {
                let notification=c.recv("BattleMessageNot").await;
                assert_eq!(notification,json!({"SenderId":users[sender]["UserInfo"]["AccountId"],"GroupType":"None",
                    "ReceiverIds":receivers,"Type":"Emoticon","Content":"1"}));
            }
            let response=clients[sender].recv("BattleMessageRes").await;
            assert_eq!(response["RequestId"],"10");assert_eq!(response["Result"],"Success");
        }
        for invalid in [Value::Null,json!(1),json!(""),json!("0"),json!("-1"),json!("2147483648"),json!("hello"),json!("9".repeat(100))] {
            clients[0].send("BattleMessageReq",json!({"RequestId":11,"Message":invalid})).await;
            assert_eq!(clients[0].recv("BattleMessageRes").await["Result"],"Fail");
        }
        // Any accidental cross-match notification or disconnect fails these
        // heartbeat checks; neither match may be changed by the emotes.
        for c in clients.iter_mut().chain(other_clients.iter_mut()) {
            c.send("PingReq",json!({"RequestId":12,"ClientTimeMs":123})).await;
            assert_eq!(c.recv("PingRes").await["ClientTimeMs"],123);
        }
        assert!(worker.try_recv().is_err(),"Emotes must not command or stop the combat worker");
        let rows:Vec<(String,i64,i64)>=sqlx::query_as("SELECT r.status,s.wins,s.losses FROM arena_runs r JOIN arena_scores s ON s.account=r.account AND s.kind=0 AND s.season=json_extract(r.data,'$.Season') WHERE json_extract(r.data,'$.OnlineMatchUid')=? ORDER BY r.id")
            .bind(uid).fetch_all(&s.db).await.unwrap();
        assert_eq!(rows,vec![("battle".into(),0,0),("battle".into(),0,0)]);
        for i in 0..2 {assert_eq!(sword(&s,&users[i]).await,before[i]);}
        let body=json!({"RunId":format!("arena:{uid}"),"Win":false,"Winner":"Right","TimeMs":10000,"Creatures":[]});
        worker_packet(&s,"arena-worker","WorkerResult",body).await.unwrap();
        for (i,u) in users.iter().take(2).enumerate() {
            let result=call(&s,u,"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(result["MatchResult"]["Win"],i==1);
            assert_eq!(result["MatchResult"]["GiveUp"],false);
            assert_eq!(result["MatchResult"]["Canceled"],false);
        }
    }

    #[tokio::test]
    async fn arena_queue_stale_cancelled_and_worker_unavailable_fall_back() {
        let (s,users,_worker)=fixture().await;
        register(&s,&users[0]).await;register(&s,&users[1]).await;
        sqlx::query("UPDATE arena_runs SET data=json_set(data,'$.LastPoll',?) WHERE account=?")
            .bind(now()-30).bind(number(&users[1]["UserInfo"],"AccountId")).execute(&s.db).await.unwrap();
        assert_eq!(call(&s,&users[0],"match/wait_match","PlayOfflineMatch=false").await["Result"],"WaitMore");
        call(&s,&users[1],"match/cancel_match","").await;
        let bot=call(&s,&users[0],"match/wait_match","PlayOfflineMatch=true").await;
        assert_eq!(bot["MatchedNpcInfo"]["IsNpc"],true);
        register(&s,&users[1]).await;register(&s,&users[2]).await;
        s.conquest.worker.write().await.as_mut().unwrap().arena_version=0;
        let bot=call(&s,&users[1],"match/wait_match","PlayOfflineMatch=true").await;
        assert_eq!(bot["MatchedNpcInfo"]["IsNpc"],true);
        assert!(bot["MatchInfo"].is_null());
    }

    #[tokio::test]
    async fn arena_concurrent_polls_pair_once_and_reject_other_account_login() {
        let (s,users,_worker)=fixture().await;
        for u in &users {register(&s,u).await;}
        let (a,b)=tokio::join!(call(&s,&users[0],"match/wait_match","PlayOfflineMatch=true"),call(&s,&users[1],"match/wait_match","PlayOfflineMatch=true"));
        assert_eq!(a["MatchInfo"],b["MatchInfo"]);
        assert_eq!(a["Result"],"Success");
        let third=call(&s,&users[2],"match/wait_match","PlayOfflineMatch=false").await;
        assert_eq!(third["Result"],"WaitMore");
        let server=server(s.clone()).await;
        let mut c=Client::connect(&server,"battle").await;
        c.send("BattleLoginReq",json!({"AccountId":users[2]["UserInfo"]["AccountId"],"SessionKey":users[2]["UserInfo"]["SessionKey"],"MatchUid":a["MatchInfo"]["Uid"],"RequestId":1})).await;
        c.closed().await;
        assert_eq!(s.conquest.runs.len(),1);
    }

    #[tokio::test]
    async fn arena_cancellation_refunds_once_and_restart_recovers_matches() {
        let (s,users,_worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let before=[sword(&s,&users[0]).await,sword(&s,&users[1]).await];
        {let mut db=s.db.acquire().await.unwrap();online::charge(&mut db,&s,uid).await.unwrap();}
        let handle=s.conquest.runs.get(&format!("arena:{uid}")).unwrap().clone();
        cancel(&s,&*handle.lock().await).await.unwrap();
        call(&s,&users[0],"match/cancel_match","").await;
        for i in 0..2 {
            assert_eq!(sword(&s,&users[i]).await,before[i]);
            let out=call(&s,&users[i],"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(out["MatchResult"]["Canceled"],true);
            assert_eq!(out["MatchResult"]["GainedMatchScore"],0);
        }
        assert!(s.conquest.runs.is_empty());
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        {let mut db=s.db.acquire().await.unwrap();online::charge(&mut db,&s,uid).await.unwrap();}
        recover_orphaned(&s).await.unwrap();recover_orphaned(&s).await.unwrap();
        for i in 0..2 {assert_eq!(sword(&s,&users[i]).await,before[i]);}
    }

    #[tokio::test]
    async fn arena_quit_menu_acknowledges_and_settles_once_without_disconnect() {
        for quitter in 0..2 {
            let (s,users,mut worker)=fixture().await;
            let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
            let (_server,mut clients)=transport(&s,&users,&matched).await;
            start(&s,&mut clients,&mut worker,uid).await;
            let swords=[sword(&s,&users[0]).await,sword(&s,&users[1]).await];
            // The original Quit button sends only RequestId. Account IDs in
            // any extra fields must not let it forfeit on behalf of an opponent.
            clients[quitter].send("BattleGiveUpReq",json!({"RequestId":"4","AccountId":users[1-quitter]["UserInfo"]["AccountId"]})).await;
            for (i,c) in clients.iter_mut().enumerate() {
                let result=c.recv("BattleResultNot").await;
                assert_eq!(result["Win"],i!=quitter);
                if i==quitter {
                    assert_eq!(c.recv("BattleGiveUpRes").await,json!({"RequestId":"4","Result":"Success"}));
                }
            }
            assert_eq!(worker.recv().await.unwrap(),("WorkerCancel".into(),json!({"RunId":format!("arena:{uid}")})));
            clients[quitter].send("BattleGiveUpReq",json!({"RequestId":5})).await;
            assert_eq!(clients[quitter].recv("BattleGiveUpRes").await["Result"],"Success");
            worker_packet(&s,"arena-worker","WorkerResult",json!({"RunId":format!("arena:{uid}"),"Win":quitter==0,"Winner":if quitter==0{"Left"}else{"Right"},"Creatures":[],"TimeMs":10000})).await.unwrap();
            assert!(worker.try_recv().is_err(),"Duplicate Quit/result packets must not settle or cancel twice");
            for (i,c) in clients.iter_mut().enumerate() {
                c.send("PingReq",json!({"RequestId":6,"ClientTimeMs":7000})).await;
                assert_eq!(c.recv("PingRes").await["ClientTimeMs"],7000,"Both sockets remain usable after intentional Quit");
                let saved=call(&s,&users[i],"match/get_match_result",&format!("MatchUid={uid}")).await;
                assert_eq!(saved["MatchResult"]["Win"],i!=quitter);
                assert_eq!(saved["MatchResult"]["GiveUp"],i==quitter);
                assert_eq!(saved["MatchResult"]["NewMatchScore"],if i==quitter{990}else{1020});
                assert_eq!(sword(&s,&users[i]).await,swords[i]);
            }
        }
    }

    #[tokio::test]
    async fn arena_withdrawal_settles_a_single_forfeit() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        clients[1].send("WithdrawBattleReq",json!({"RequestId":4})).await;
        assert_eq!(clients[1].recv("BattleResultNot").await["Win"],false);
        clients[1].recv("WithdrawBattleRes").await;
        assert_eq!(clients[0].recv("BattleResultNot").await["Win"],true);
        let out=call(&s,&users[1],"match/get_match_result",&format!("MatchUid={uid}")).await;
        assert_eq!(out["MatchResult"]["GiveUp"],true);
        assert_eq!(out["MatchResult"]["NewMatchScore"],990);
    }

    #[tokio::test]
    async fn arena_http_cancel_after_start_is_a_forfeit() {
        let (s,users,mut worker)=fixture().await;
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let before=sword(&s,&users[1]).await;
        let (_server,mut clients)=transport(&s,&users,&matched).await;
        start(&s,&mut clients,&mut worker,uid).await;
        call(&s,&users[1],"match/cancel_match","").await;
        assert_eq!(clients[0].recv("BattleResultNot").await["Win"],true);
        assert_eq!(clients[1].recv("BattleResultNot").await["Win"],false);
        assert_eq!(sword(&s,&users[1]).await,before-1);
        let out=call(&s,&users[1],"match/get_match_result",&format!("MatchUid={uid}")).await;
        assert_eq!(out["MatchResult"]["GiveUp"],true);
        assert_eq!(out["MatchResult"]["Canceled"],false);
    }

    #[tokio::test]
    #[ignore = "requires SPRK_ARENA_NATIVE_EXECUTABLE pointing to a patched Unity worker"]
    async fn arena_native_worker_runs_opposing_teams_with_the_same_hero() {
        use tokio::io::{AsyncBufReadExt,BufReader};
        struct Native(std::process::Child);
        impl Drop for Native {fn drop(&mut self){let _=self.0.kill();let _=self.0.wait();}}
        let (mut s,users,_rx)=fixture().await;
        *s.conquest.worker.write().await=None;
        let key=uuid::Uuid::new_v4().to_string();s.battle_service_key=Arc::new(Some(key.clone()));
        sqlx::query("UPDATE heroes SET level=100,star=5,transcend=5 WHERE account_id=?")
            .bind(number(&users[1]["UserInfo"],"AccountId")).execute(&s.db).await.unwrap();
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let port=listener.local_addr().unwrap().port();
        let task=tokio::spawn(serve(listener,s.clone()));
        let executable=std::path::PathBuf::from(std::env::var("SPRK_ARENA_NATIVE_EXECUTABLE").expect("SPRK_ARENA_NATIVE_EXECUTABLE")).canonicalize().unwrap();
        std::fs::create_dir_all("target/arena-lab").unwrap();
        let mut command=std::process::Command::new(&executable);
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join("target/arena-lab/native-worker.log"))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1")
            .env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] {use std::os::windows::process::CommandExt;command.creation_flags(0x08000000);}
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(60),async {
            while arena_worker(&s).await.is_none(){tokio::time::sleep(std::time::Duration::from_millis(100)).await;}
        }).await.expect("Native arena worker did not authenticate");
        let matched=pair(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let mut clients=vec![];
        for (side,u) in users.iter().take(2).enumerate() {
            let socket=TcpStream::connect(("127.0.0.1",port)).await.unwrap();let (reader,mut writer)=socket.into_split();
            let mut reader=BufReader::new(reader);
            writer.write_all(&encode_packet("BattleLoginReq",&json!({"AccountId":u["UserInfo"]["AccountId"],"SessionKey":u["UserInfo"]["SessionKey"],"MatchUid":uid,"RequestId":1}))).await.unwrap();
            let mut line=String::new();reader.read_line(&mut line).await.unwrap();assert_eq!(decode_packet(line.as_bytes()).unwrap().0,"BattleLoginRes");
            writer.write_all(&encode_packet("BattleEnterReq",&json!({"RequestId":2}))).await.unwrap();
            clients.push(tokio::spawn(async move {
                let mut request_id=3;let mut logs=vec![];let mut reserved=false;
                loop {
                    let mut line=String::new();let count=tokio::time::timeout(std::time::Duration::from_secs(60),reader.read_line(&mut line)).await.unwrap().unwrap();
                    assert!(count>0,"Native arena socket closed before result");
                    let (name,body)=decode_packet(line.as_bytes()).unwrap();
                    if name=="BattleLogNot" {
                        logs.push(body.clone());
                        if !reserved && body["BattleLogs"].as_array().unwrap().iter().any(|log| {
                            log["Type"]=="CreatureReserveOnSkill" && serde_json::from_str::<Value>(log["Data"].as_str().unwrap())
                                .is_ok_and(|data|number(&data,"CreatureUid")==side as i64+1)
                        }) {
                            assert_eq!(number(&body,"ServerTimeMs"),0,"Reservation must be replayed before countdown starts combat");
                            reserved=true;
                            writer.write_all(&encode_packet("BattleStartReq",&json!({"RequestId":request_id}))).await.unwrap();request_id+=1;
                            writer.write_all(&encode_packet("BattleWaveResumeReq",&json!({"RequestId":request_id}))).await.unwrap();request_id+=1;
                        }
                    }
                    if name=="WaveReadyNot" {
                        writer.write_all(&encode_packet("ReserveOnSkillReq",&json!({"RequestId":request_id,"CreatureUid":side+1,"SkillSlotIndex":1,"ManualTargetUid":0,"AutoReserved":false}))).await.unwrap();request_id+=1;
                    }
                    // Arena must receive its result as a notification, without
                    // manufacturing a final heartbeat after replay completion.
                    if name=="BattleResultNot" {assert!(reserved);return (logs,body);}
                }
            }));
        }
        let mut results=vec![];
        for client in clients {results.push(tokio::time::timeout(std::time::Duration::from_secs(120),client).await.unwrap().unwrap());}
        assert!(!results[0].0.is_empty());assert_eq!(results[0].0,results[1].0);
        assert!(results[0].0.iter().any(|packet|packet["BattleLogs"].as_array().is_some_and(|logs|logs.is_empty()) && number(packet,"ServerTimeMs")>0),
            "Native worker must publish advancing time between combat actions");
        let logs=results[0].0.iter().flat_map(|packet|packet["BattleLogs"].as_array().unwrap()).collect::<Vec<_>>();
        let ready:Value=serde_json::from_str(logs.iter().find(|log|log["Type"]=="WaveReady").unwrap()["Data"].as_str().unwrap()).unwrap();
        let end:Value=serde_json::from_str(logs.iter().find(|log|log["Type"]=="WaveEnd").unwrap()["Data"].as_str().unwrap()).unwrap();
        assert_eq!(ready["Difficulty"],"Easy","Native worker and arena replay must use the same ruleset");
        assert_eq!(end["_winDecider"],"MoreCreatureAlive","Victory must use its native PvP victory rules");
        assert_eq!(results[0].1["Win"],false);assert_eq!(results[1].1["Win"],true);
        for (i,u) in users.iter().take(2).enumerate() {
            let result=call(&s,u,"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(result["MatchResult"]["Win"],i==1);
            assert_eq!(result["MatchResult"]["Canceled"],false);
        }
        std::fs::write("target/arena-lab/native-results.json",json!(results).to_string()).unwrap();
        task.abort();
    }
}
