//! Native pre-Doomsday Honor ban/pick, on the authenticated arena transport.
//! Phase definitions are imported verbatim from build 1558 MatchBanPickState.
use super::*;

const STATES: [&str; 11] = ["None", "PHASE1_ALL_BAN_ALL", "PHASE2_1P_PICK_1P",
    "PHASE3_2P_PICK_2P", "PHASE4_1P_PICK_1P", "PHASE5_2P_PICK_2P",
    "PHASE6_1P_PICK_1P", "PHASE7_2P_PICK_2P", "PHASE8_ALL_BAN_EACH_PICK",
    "PHASE9_ALL_WAIT_SETTING", "PHASE10_BATTLE_START"];

pub(super) fn initial(members:&[i64])->Value {
    json!({"State":0,"Ready":[],"Submitted":{},"Deadline":0,"Accounts":members.iter()
        .map(|a|json!({"AccountId":a,"PickHeroIndices":[],"BanHeroIndices":[]})).collect::<Vec<_>>()})
}
pub(super) fn drafting(run:&Run)->bool {
    run.payload["Draft"].is_object() && number(&run.payload["Draft"],"State")<10 && run.result.is_none()
}
fn phase(run:&Run)->usize {number(&run.payload["Draft"],"State") as usize}
fn definition<'a>(s:&'a AppState,run:&Run)->Result<&'a Value> {
    s.tables.arena_guild.find("MatchBanPickState", &[("State",phase(run) as i64)])
        .ok_or_else(||rule("BanPickStateNotFound"))
}
fn snapshot(run:&Run)->Value {
    json!({"RemainTimeSec":(number(&run.payload["Draft"],"Deadline")-now()).max(0),
        "BanPickState":STATES[phase(run)],"BanPickAccountInfo":run.payload["Draft"]["Accounts"]})
}
fn actors(s:&AppState,run:&Run)->Result<Vec<usize>> {
    Ok(match number(definition(s,run)?,"TeamType") {0=>vec![0],1=>vec![1],_=>vec![0,1]})
}
fn indices(v:&Value)->Vec<i64> {v.as_array().into_iter().flatten().map(|v|v.as_i64().unwrap_or(0)).collect()}
fn available(s:&AppState,run:&Run,side:usize)->Vec<i64> {
    let draft=&run.payload["Draft"];
    if phase(run)==1 {
        // All-hero opening bans may target a released hero neither player owns.
        return s.tables.hero_shop.heroes.iter().filter(|(_,v)|n(v,"OpenType")!=0 && n(v,"OpenType")!=2)
            .map(|(id,_)|i64::from(*id)).collect::<BTreeSet<_>>().into_iter().collect();
    }
    if phase(run)==8 {return indices(&draft["Accounts"][1-side]["PickHeroIndices"]);}
    let used=draft["Accounts"].as_array().unwrap().iter().flat_map(|a|
        indices(&a["PickHeroIndices"]).into_iter().chain(indices(&a["BanHeroIndices"]))).collect::<BTreeSet<_>>();
    run.payload["MatchInfo"]["AccountInfos"][side]["HeroInfos"].as_object().unwrap().keys()
        .filter_map(|v|v.parse::<i64>().ok()).filter(|id|!used.contains(id)).collect::<BTreeSet<_>>().into_iter().collect()
}
fn select(s:&AppState,run:&mut Run,side:usize,heroes:&[i64])->Result<&'static str> {
    if !actors(s,run)?.contains(&side) {return Ok("NotCorrectTeamType");}
    let key=run.members[side].to_string();
    if let Some(saved)=run.payload["Draft"]["Submitted"].get(&key) {
        return Ok(if indices(saved)==heroes {"Success"}else{"NotCorrectState"});
    }
    if heroes.len()!=number(definition(s,run)?,"SelectableCount4Deck") as usize || heroes.iter().collect::<BTreeSet<_>>().len()!=heroes.len() {
        return Ok("NotCorrectHeroCount");
    }
    let allowed=available(s,run,side);
    if heroes.iter().any(|v|!allowed.contains(v)) {return Ok("NotCorrectHero");}
    let pick=definition(s,run)?["IsPick"]==true;
    let target=if pick {side}else{1-side};
    let field=if pick {"PickHeroIndices"}else{"BanHeroIndices"};
    // Final bans remove one of the opponent's five picks; four remain.
    if phase(run)==8 {
        run.payload["Draft"]["Accounts"][target]["PickHeroIndices"].as_array_mut().unwrap().retain(|v|!heroes.contains(&v.as_i64().unwrap()));
    }
    run.payload["Draft"]["Accounts"][target][field].as_array_mut().unwrap().extend(heroes.iter().map(|v|json!(v)));
    run.payload["Draft"]["Submitted"][key]=json!(heroes);
    Ok("Success")
}
fn enter(s:&AppState,run:&mut Run,next:usize)->Result<()> {
    run.payload["Draft"]["State"]=json!(next);
    run.payload["Draft"]["Submitted"]=json!({});
    let seconds=number(definition(s,run)?,"WaitTime4Deck");
    run.payload["Draft"]["Deadline"]=json!(now()+seconds);
    run.created=now();
    let mut notice=snapshot(run);notice["HeroIndices"]=json!([]);
    s.conquest.notify(run,"BattleBanPickNextNot",notice);
    Ok(())
}
async fn advance(s:&AppState,run:&mut Run)->Result<()> {
    if (1..=8).contains(&phase(run)) && actors(s,run)?.iter().all(|side|
        run.payload["Draft"]["Submitted"].get(run.members[*side].to_string()).is_some()) {
        enter(s,run,phase(run)+1)?;
    }
    Ok(())
}
async fn complete(s:&AppState,run:&mut Run)->Result<()> {
    let mut accounts=run.payload["MatchInfo"]["AccountInfos"].clone();
    let mut db=s.db.begin().await?;
    // Persist the exact four combat heroes before exposing the loading phase.
    for (side,a) in run.members.iter().enumerate() {
        let heroes=indices(&run.payload["Draft"]["Accounts"][side]["PickHeroIndices"]);
        if heroes.len()!=4 {return Err(rule("InvalidHero"));}
        accounts[side]["HeroInfos"].as_object_mut().unwrap().retain(|id,_|id.parse::<i64>().is_ok_and(|id|heroes.contains(&id)));
        accounts[side]["LeaderHeroIndex"]=json!(heroes[0]);
        sqlx::query("UPDATE arena_runs SET data=json_set(data,'$.Heroes',json(?),'$.Leader',?) WHERE account=? AND status='battle' AND json_extract(data,'$.OnlineMatchUid')=?")
            .bind(json!(heroes).to_string()).bind(heroes[0]).bind(a).bind(number(&run.payload,"MatchUid")).execute(&mut *db).await?;
    }
    db.commit().await?;
    run.payload["MatchInfo"]["AccountInfos"]=accounts.clone();
    run.payload["Simulation"]["AccountInfos"]=accounts.clone();
    run.payload["Draft"]["State"]=json!(10);
    run.created=now();
    s.conquest.notify(run,"BattleBanPickEndNot",json!({"AccountInfos":accounts,"BanPickAccountInfo":run.payload["Draft"]["Accounts"]}));
    Ok(())
}
pub(super) async fn tick(s:&AppState,run:&mut Run)->Result<()> {
    if !drafting(run) || phase(run)==0 {return Ok(());}
    let deadline=number(&run.payload["Draft"],"Deadline");
    if phase(run)==9 {
        if now()>=deadline {complete(s,run).await?;}
    } else if now()>deadline+15 {
        // Let the native UI auto-pick first, including its animation time.
        // Server fallback also bounds stalled or malicious draft clients.
        let count=number(definition(s,run)?,"SelectableCount4Deck") as usize;
        for side in actors(s,run)? {
            if run.payload["Draft"]["Submitted"].get(run.members[side].to_string()).is_none() {
                let heroes=available(s,run,side).into_iter().take(count).collect::<Vec<_>>();
                if select(s,run,side,&heroes)?!="Success" {return arena::cancel(s,run).await;}
            }
        }
        advance(s,run).await?;
    }
    Ok(())
}
pub(super) async fn packet(s:&AppState,run:&mut Run,a:i64,name:&str,body:&Value,tx:&Sender)->Result<()> {
    let response=name.strip_suffix("Req").map(|v|format!("{v}Res")).ok_or_else(||rule("InvalidRequest"))?;
    let mut reply=if run.payload["Draft"].is_object(){snapshot(run)}else{json!({})};
    reply["RequestId"]=body["RequestId"].clone();reply["Result"]=json!("Success");
    let side=run.members.iter().position(|v|*v==a).ok_or_else(||rule("WrongMember"))?;
    if !drafting(run) {
        reply["Result"]=json!(match name {
            "BattleBanPickStartReq"|"BattleBanPickInfoReq"=>"BanPickStateNotFound",
            "BattleBanPickReq"=>"NotCorrectState",
            _=>"Fail",
        });
    }
    else {match name {
        "BattleBanPickStartReq"=>{
            let ready=run.payload["Draft"]["Ready"].as_array_mut().unwrap();
            if !ready.contains(&json!(a)) {ready.push(json!(a));}
        }
        "BattleBanPickInfoReq"=>{}
        "BattleBanPickReq"=>{
            let requested=body["BanPickState"].as_str().and_then(|v|STATES.iter().position(|s|*s==v))
                .or_else(||body["BanPickState"].as_u64().map(|v|v as usize));
            if requested!=Some(phase(run)) || !(1..=8).contains(&phase(run)) {reply["Result"]=json!("NotCorrectState");}
            else {
                let raw=body["HeroIndices"].as_array();
                let heroes=raw.map(|v|v.iter().map(|v|v.as_i64().or_else(||v.as_str().and_then(|v|v.parse().ok())).unwrap_or(0)).collect::<Vec<_>>()).unwrap_or_default();
                reply["Result"]=json!(select(s,run,side,&heroes)?);
                reply["BanPickAccountInfo"]=run.payload["Draft"]["Accounts"].clone();
            }
        }
        "BattleBanPickGiveUpReq"=>{}
        _=>{reply["Result"]=json!("Fail");}
    }}
    let success=reply["Result"]=="Success";
    tx.send((response,reply)).await.map_err(|_|rule("InvalidValue"))?;
    if success && drafting(run) {
        if name=="BattleBanPickGiveUpReq" {return arena::depart(s,run,a).await;}
        if phase(run)==0 && run.payload["Draft"]["Ready"].as_array().unwrap().len()==2 {enter(s,run,1)?;}
        else if phase(run)>0 && name=="BattleBanPickStartReq" {
            let mut notice=snapshot(run);notice["HeroIndices"]=json!([]);
            tx.send(("BattleBanPickNextNot".into(),notice)).await.map_err(|_|rule("InvalidValue"))?;
        }
        else {advance(s,run).await?;}
    }
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup,login,call};
    use crate::websocket::tests::{server,Client};

    async fn fixture()->(AppState,Vec<Value>,mpsc::Receiver<Packet>) {
        let s=setup().await;
        let users=vec![login(&s,"honor-a").await,login(&s,"honor-b").await];
        for u in &users {
            for hero in 1..=16 {
                sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,100,5)")
                    .bind(number(&u["UserInfo"],"AccountId")).bind(hero).bind(hero).execute(&s.db).await.unwrap();
            }
        }
        let (tx,rx)=mpsc::channel(100);
        *s.conquest.worker.write().await=Some(Worker{id:"honor-worker".into(),sender:tx,arena_version:1});
        (s,users,rx)
    }
    async fn register(s:&AppState,u:&Value,kind:&str) {
        let heroes=if kind=="BanPick"{json!((1..=16).collect::<Vec<_>>()).to_string()}else{"[1]".into()};
        let out=call(s,u,"match/register_match",&format!("ArenaType={kind}&HeroIndices={heroes}&LeaderHeroIndex=1")).await;
        assert_eq!(out["Result"],"Success","{out}");
    }
    async fn matched(s:&AppState,users:&[Value])->Value {
        for u in users {register(s,u,"BanPick").await;}
        let out=call(s,&users[0],"match/wait_match","PlayOfflineMatch=true").await;
        assert_eq!(out["Result"],"Success","{out}");
        assert_eq!(out["MatchInfo"]["ArenaType"],"BanPick");
        out
    }
    async fn balance(s:&AppState,u:&Value)->(i64,i64) {
        sqlx::query_as("SELECT sword,sword2 FROM user_info WHERE account_id=?")
            .bind(number(&u["UserInfo"],"AccountId")).fetch_one(&s.db).await.unwrap()
    }
    async fn draft(s:&AppState,users:&[Value],matched:&Value)->(crate::websocket::tests::Server,Vec<Client>) {
        let server=server(s.clone()).await;
        let uid=number(&matched["MatchInfo"],"Uid");
        let mut clients=vec![];
        for u in users {
            let mut c=Client::connect(&server,"battle").await;
            c.send("BattleLoginReq",json!({"RequestId":1,"AccountId":u["UserInfo"]["AccountId"],"SessionKey":u["UserInfo"]["SessionKey"],"MatchUid":uid})).await;
            assert_eq!(c.recv("BattleLoginRes").await["Result"],"Success");
            c.recv("BattleBanPickStartNot").await;
            c.send("BattleBanPickStartReq",json!({"RequestId":2})).await;
            assert_eq!(c.recv("BattleBanPickStartRes").await["Result"],"Success");
            clients.push(c);
        }
        for c in &mut clients {assert_eq!(c.recv("BattleBanPickNextNot").await["BanPickState"],STATES[1]);}
        (server,clients)
    }
    async fn finish_draft(s:&AppState,clients:&mut [Client],uid:i64) {
        let handle=s.conquest.runs.get(&format!("arena:{uid}")).unwrap().clone();
        for state in 1..=8 {
            let sides={let run=handle.lock().await;assert_eq!(phase(&run),state);actors(s,&run).unwrap()};
            for side in sides {
                let heroes={let run=handle.lock().await;let count=number(definition(s,&run).unwrap(),"SelectableCount4Deck") as usize;
                    available(s,&run,side).into_iter().take(count).collect::<Vec<_>>()};
                clients[side].send("BattleBanPickReq",json!({"RequestId":10+state,"BanPickState":STATES[state],"HeroIndices":heroes})).await;
                assert_eq!(clients[side].recv("BattleBanPickRes").await["Result"],"Success");
            }
            for c in clients.iter_mut() {assert_eq!(c.recv("BattleBanPickNextNot").await["BanPickState"],STATES[state+1]);}
        }
        {handle.lock().await.payload["Draft"]["Deadline"]=json!(now()-1);}
        clients[0].send("PingReq",json!({"RequestId":30})).await;
        for c in clients.iter_mut() {
            let end=c.recv("BattleBanPickEndNot").await;
            for account in end["AccountInfos"].as_array().unwrap() {assert_eq!(account["HeroInfos"].as_object().unwrap().len(),4);}
        }
        clients[0].recv("PingRes").await;
    }
    #[tokio::test]
    async fn honor_queue_never_pairs_with_victory_or_falls_back_to_a_bot() {
        let (s,users,_worker)=fixture().await;
        register(&s,&users[0],"BanPick").await;register(&s,&users[1],"Normal").await;
        let wait=call(&s,&users[0],"match/wait_match","PlayOfflineMatch=true").await;
        assert_eq!(wait["Result"],"WaitMore");assert!(wait["MatchedNpcInfo"].is_null());
        let before=balance(&s,&users[0]).await;
        call(&s,&users[0],"match/cancel_match","").await;
        assert_eq!(balance(&s,&users[0]).await,before);
        assert!(s.conquest.runs.is_empty());
    }
    #[tokio::test]
    async fn honor_native_protocol_validates_drafts_and_settles_in_its_own_partition() {
        let (s,users,mut worker)=fixture().await;
        let matched=matched(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let before=[balance(&s,&users[0]).await,balance(&s,&users[1]).await];
        let (_server,mut clients)=draft(&s,&users,&matched).await;
        // Forged phase, duplicate choices and unowned/invalid heroes are errors,
        // not disconnects that could forfeit the other player.
        for (state,heroes,result) in [(2,json!([1]),"NotCorrectState"),(1,json!([1,1]),"NotCorrectHeroCount"),(1,json!([999999]),"NotCorrectHero")] {
            clients[0].send("BattleBanPickReq",json!({"RequestId":3,"BanPickState":STATES[state],"HeroIndices":heroes})).await;
            assert_eq!(clients[0].recv("BattleBanPickRes").await["Result"],result);
        }
        clients[0].send("BattleStartReq",json!({"RequestId":4})).await;clients[0].recv("BattleStartRes").await;
        assert!(worker.try_recv().is_err(),"A draft cannot start the combat engine");
        finish_draft(&s,&mut clients,uid).await;
        for c in &mut clients {c.send("BattleEnterReq",json!({"RequestId":31})).await;c.recv("BattleEnterRes").await;}
        let prepare=worker.recv().await.unwrap();assert_eq!(prepare.0,"WorkerPrepare");
        for a in prepare.1["Simulation"]["AccountInfos"].as_array().unwrap() {assert_eq!(a["HeroInfos"].as_object().unwrap().len(),4);}
        for c in &mut clients {c.send("BattleStartReq",json!({"RequestId":32})).await;c.recv("BattleStartRes").await;}
        assert_eq!(worker.recv().await.unwrap().0,"WorkerStart");
        for (i,u) in users.iter().enumerate() {assert_eq!(balance(&s,u).await,(before[i].0,before[i].1-1));}
        let result=json!({"RunId":format!("arena:{uid}"),"Winner":"Left","Win":true,"Creatures":[],"TimeMs":10000});
        worker_packet(&s,"honor-worker","WorkerResult",result.clone()).await.unwrap();
        worker_packet(&s,"honor-worker","WorkerResult",result).await.unwrap();
        for (i,u) in users.iter().enumerate() {
            let out=call(&s,u,"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(out["MatchResult"]["ArenaType"],"BanPick");
            assert_eq!(out["BattleBanPickInfo"]["MatchScore"],if i==0{1020}else{990});
            assert_eq!(out["MatchResult"]["SwordResult"]["Type"],"Sword2");
            let normal=call(&s,u,"match/get_match_rank","").await;
            assert_eq!(normal["BattleInfo"]["MatchScore"],1000);
        }
    }
    #[tokio::test]
    async fn honor_draft_forfeit_and_restart_are_idempotent() {
        let (s,users,_worker)=fixture().await;
        let matched=matched(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=draft(&s,&users,&matched).await;
        let before=balance(&s,&users[0]).await;
        clients[0].send("BattleBanPickGiveUpReq",json!({"RequestId":3})).await;
        assert_eq!(clients[0].recv("BattleBanPickGiveUpRes").await["Result"],"Success");
        clients[0].recv("BattleResultNot").await;clients[1].recv("BattleResultNot").await;
        assert_eq!(balance(&s,&users[0]).await,(before.0,before.1-1));
        recover_orphaned(&s).await.unwrap();recover_orphaned(&s).await.unwrap();
        let out=call(&s,&users[0],"match/get_match_result",&format!("MatchUid={uid}")).await;
        assert_eq!(out["MatchResult"]["GiveUp"],true);assert_eq!(out["MatchResult"]["NewMatchScore"],990);
        assert_eq!(balance(&s,&users[0]).await,(before.0,before.1-1));
    }
    #[tokio::test]
    async fn honor_timer_completes_legal_teams_and_restart_cancels_without_charging() {
        let (s,users,_worker)=fixture().await;
        let matched=matched(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let handle=s.conquest.runs.get(&format!("arena:{uid}")).unwrap().clone();
        let before=[balance(&s,&users[0]).await,balance(&s,&users[1]).await];
        {
            let mut run=handle.lock().await;
            enter(&s,&mut run,1).unwrap();
            for expected in 1..=9 {
                assert_eq!(phase(&run),expected);
                run.payload["Draft"]["Deadline"]=json!(now()-16);
                tick(&s,&mut run).await.unwrap();
            }
            assert_eq!(phase(&run),10);
            let left=indices(&run.payload["Draft"]["Accounts"][0]["PickHeroIndices"]);
            let right=indices(&run.payload["Draft"]["Accounts"][1]["PickHeroIndices"]);
            assert_eq!(left.len(),4);assert_eq!(right.len(),4);
            assert!(left.iter().all(|id|!right.contains(id)));
        }
        recover_orphaned(&s).await.unwrap();recover_orphaned(&s).await.unwrap();
        for (i,u) in users.iter().enumerate() {
            assert_eq!(balance(&s,u).await,before[i]);
            let out=call(&s,u,"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(out["MatchResult"]["Canceled"],true);
            assert_eq!(out["BattleBanPickInfo"]["MatchScore"],1000);
        }
    }
    #[tokio::test]
    #[ignore = "requires SPRK_HONOR_NATIVE_EXECUTABLE pointing to a patched Unity worker"]
    async fn honor_native_worker_completes_the_drafted_four_vs_four_battle() {
        use futures_util::StreamExt;
        struct Native(std::process::Child);
        impl Drop for Native {fn drop(&mut self){let _=self.0.kill();let _=self.0.wait();}}
        let (mut s,users,_rx)=fixture().await;
        *s.conquest.worker.write().await=None;
        sqlx::query("UPDATE heroes SET level=1,star=1 WHERE account_id=?")
            .bind(number(&users[1]["UserInfo"],"AccountId")).execute(&s.db).await.unwrap();
        let key=uuid::Uuid::new_v4().to_string();s.battle_service_key=Arc::new(Some(key.clone()));
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let port=listener.local_addr().unwrap().port();
        let task=tokio::spawn(serve(listener,s.clone()));
        let executable=std::path::PathBuf::from(std::env::var("SPRK_HONOR_NATIVE_EXECUTABLE").unwrap()).canonicalize().unwrap();
        std::fs::create_dir_all("target/honor-research").unwrap();
        let mut command=std::process::Command::new(&executable);
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join("target/honor-research/native-worker.log"))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1")
            .env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] {use std::os::windows::process::CommandExt;command.creation_flags(0x08000000);}
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(60),async {
            while arena_worker(&s).await.is_none(){tokio::time::sleep(std::time::Duration::from_millis(100)).await;}
        }).await.expect("Native Honor worker did not authenticate");
        let matched=matched(&s,&users).await;let uid=number(&matched["MatchInfo"],"Uid");
        let (_server,mut clients)=draft(&s,&users,&matched).await;
        finish_draft(&s,&mut clients,uid).await;
        let mut tasks=vec![];
        for mut c in clients {
            c.send("BattleEnterReq",json!({"RequestId":31})).await;
            tasks.push(tokio::spawn(async move {
                let mut buffer=vec![];let mut logs=vec![];
                loop {
                    let frame=c.socket.next().await.unwrap().unwrap();
                    match frame {
                        tokio_tungstenite::tungstenite::Message::Binary(bytes)=>buffer.extend(bytes),
                        tokio_tungstenite::tungstenite::Message::Text(text)=>buffer.extend(text.as_bytes()),
                        _=>continue,
                    }
                    while let Some(end)=buffer.iter().position(|v|*v==b'\n') {
                        let frame=buffer.drain(..=end).collect::<Vec<_>>();let (name,body)=decode_packet(&frame).unwrap();
                        if name=="WaveReadyNot" {
                            c.send("BattleStartReq",json!({"RequestId":32})).await;
                            c.send("BattleWaveResumeReq",json!({"RequestId":33})).await;
                        }
                        if name=="BattleLogNot" {logs.push(body.clone());}
                        if name=="BattleResultNot" {return (logs,body);}
                    }
                }
            }));
        }
        let mut results=vec![];
        for task in tasks {results.push(tokio::time::timeout(std::time::Duration::from_secs(120),task).await.unwrap().unwrap());}
        assert!(!results[0].0.is_empty());assert_eq!(results[0].0,results[1].0);
        assert_eq!(results[0].1["Win"],true);assert_eq!(results[1].1["Win"],false);
        for (i,u) in users.iter().enumerate() {
            let out=call(&s,u,"match/get_match_result",&format!("MatchUid={uid}")).await;
            assert_eq!(out["MatchResult"]["Win"],i==0);
            assert_eq!(out["BattleBanPickInfo"]["SeasonWin"],if i==0{1}else{0});
        }
        std::fs::write("target/honor-research/native-results.json",json!(results).to_string()).unwrap();
        task.abort();
    }
}
