//! Authenticated native Conquest battle transport. A separate trusted Unity
//! worker runs the original combat engine; players receive ordered native logs.
use super::*;
use crate::api::community::{chat::{decode_packet,encode_packet},conquest};
use dashmap::DashMap;
use std::sync::Arc;
use tokio::{io::{AsyncReadExt,AsyncWriteExt},net::{TcpListener,TcpStream},sync::{mpsc,Mutex,RwLock}};

type Packet=(String,Value);
type Sender=mpsc::Sender<Packet>;
struct Worker {id:String,sender:Sender}
struct Peer {id:String,run:String,sender:Sender}
pub(crate) struct BattleHub {
    worker:RwLock<Option<Worker>>,
    peers:DashMap<i64,Peer>,
    runs:DashMap<String,Arc<Mutex<Run>>>,
    pub port:u16,
    pub address:String,
}
impl Default for BattleHub {
    fn default()->Self {Self{worker:RwLock::new(None),peers:DashMap::new(),runs:DashMap::new(),
        port:std::env::var("BATTLE_PORT").ok().and_then(|v|v.parse().ok()).unwrap_or(9002),
        address:crate::websocket::public_url("battle")}}
}
struct Run {
    id:String,worker:String,master:i64,room:i64,members:Vec<i64>,payload:Value,
    entered:BTreeSet<i64>,ready:BTreeSet<i64>,sent:BTreeSet<i64>,
    logs:Vec<Packet>,result:Option<Value>,started:bool,prepared:bool,created:i64,time_ms:i64,
}
impl BattleHub {
    async fn send_worker(&self,name:&str,body:Value)->Result<()> {
        let worker=self.worker.read().await;
        let worker=worker.as_ref().ok_or_else(||rule("BattleServerNotFound"))?;
        worker.sender.try_send((name.into(),body)).map_err(|_|rule("BattleServerNotFound"))
    }
    fn notify(&self,run:&Run,name:&str,body:Value) {
        for a in &run.members {
            if let Some(peer)=self.peers.get(a) {if peer.run==run.id {let _=peer.sender.try_send((name.into(),body.clone()));}}
        }
    }
}

fn number(v:&Value,key:&str)->i64 {v[key].as_i64().or_else(||v[key].as_str().and_then(|v|v.parse().ok())).unwrap_or(0)}
fn request(fields:Value)->Request {
    Request(fields.as_object().unwrap().iter().map(|(k,v)|(k.clone(),v.as_str().map(str::to_owned).unwrap_or_else(||v.to_string()))).collect())
}
pub(super) async fn begin(db:&mut SqliteConnection,s:&AppState,a:i64,r:&Request)->Result<Value> {
    let room_id=r.number("RaidRoomNo",0)?;
    let raw:String=sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=? AND family='raid' AND EXISTS(SELECT 1 FROM battle_room_members WHERE room=? AND account=?)")
        .bind(room_id).bind(room_id).bind(a).fetch_optional(&mut *db).await?.ok_or_else(||rule("RoomNotExist"))?;
    let mut room:Value=read_json(&raw)?;let master=n(&room,"MasterAccountId");
    if r.number("MultiplayMasterId",0)?!=master || n(&room,"RaidIndex")!=90001 || n(&room,"RaidLevel")!=r.number("RaidLevel",0)? {
        return Err(rule("InvalidRoomInfo"));
    }
    if let Some(id) = room["ConquestRunId"].as_str() {
        let saved: Option<(String, String)> =
            sqlx::query_as("SELECT entry,begin_response FROM battle_runs WHERE account=?")
                .bind(a)
                .fetch_optional(&mut *db)
                .await?;
        if let Some((entry, response)) = saved {
            if read_json::<Value>(&entry)?["CoopRunId"] == id {
                return read_json(&response);
            }
        }
        return Err(rule("AlreadyOnBattleHero"));
    }
    if a!=master || n(&room,"Status")!=0 {return Err(rule("WrongMember"));}
    let worker_id={let worker=s.conquest.worker.read().await;worker.as_ref().ok_or_else(||rule("BattleServerNotFound"))?.id.clone()};
    let members:Vec<i64>=sqlx::query_scalar("SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account")
        .bind(room_id).fetch_all(&mut *db).await?;
    if members.is_empty() || members.len()>3 || ids(r,"MultiplayMemberIds",3)?.into_iter().collect::<BTreeSet<_>>() != members.iter().copied().collect() {
        return Err(rule("WrongMember"));
    }
    let guild:i64=sqlx::query_scalar("SELECT guild_id FROM guild_members WHERE account_id=?").bind(a).fetch_one(&mut *db).await?;
    let selected=ids(r,"HeroIndices",3)?;
    // AccountIds pairs with HeroIndices and legitimately repeats for each of
    // a player's heroes. Hero-index uniqueness must not be applied to owners.
    let owner_values:Vec<Value>=read_json(r.text("AccountIds")).map_err(|_|rule("WrongMember"))?;
    let owners:Vec<i64>=owner_values.iter().map(|v|v.as_i64().or_else(||v.as_str().and_then(|v|v.parse().ok())).ok_or_else(||rule("WrongMember"))).collect::<Result<_>>()?;
    // The original PartyManager sends only the requester's deck. Followers'
    // decks come from their ready state, never from the host's request.
    if selected.len()!=owners.len() || selected.is_empty() || owners.iter().any(|v|*v!=master) {return Err(rule("NotMatchHeroIndices"));}
    let id=uuid::Uuid::new_v4().to_string();let mut accounts=vec![];let mut entries=vec![];let mut all=BTreeSet::new();
    let def=row(s,"Raid",&[("Index",90001),("Level",n(&room,"RaidLevel"))])?;
    for member in &members {
        let same:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM guild_members WHERE account_id=? AND guild_id=?)").bind(member).bind(guild).fetch_one(&mut *db).await?;
        if !same {return Err(rule("WrongMember"));}
        let active:bool=sqlx::query_scalar("SELECT EXISTS(SELECT 1 FROM battle_runs WHERE account=? AND completed=0)").bind(member).fetch_one(&mut *db).await?;
        if active {return Err(rule("AlreadyOnBattleHero"));}
        let state=get(db,*member,"party_member",room_id).await?;
        let own:Vec<i64>=if *member==master {selected.clone()}else {
            state["DeckHeros"].as_object().ok_or_else(||rule("WrongMember"))?.keys()
                .map(|key|key.parse::<i64>().map_err(|_|rule("NotMatchHeroIndices"))).collect::<Result<_>>()?
        };
        if own.is_empty() || own.len()>3 {return Err(rule("NotMatchHeroIndices"));}
        if *member!=master && state["IsBattleReady"]!=true {
            return Err(rule("WrongMember"));
        }
        let req=request(json!({"RaidIndex":90001,"RaidLevel":n(&room,"RaidLevel"),"ChapterIndex":9900,"DungeonIndex":n(&room,"RaidLevel"),
            "HeroIndices":own,"AiHeroIndices":[],"DungeonDifficulty":r.number("DungeonDifficulty",0)?}));
        let ban=conquest::validate(db,s,*member,&req).await?;
        let mut heroes=serde_json::Map::new();
        for hero in &own {
            if !all.insert(*hero) {return Err(rule("DuplicatedHero"));}
            super::dispatch::ensure_available(db,*member,&[*hero],None).await?;
            let h=special::cached_hero(db,*member,*hero as i32).await?;
            if n(&h,"Level")<n(def,"ReqHeroLevel") {return Err(rule("NotAvailableHero"));}
            ensure_ban(s,ban,*hero)?;
            heroes.insert(hero.to_string(),h);
        }
        // Validate balances before anyone is charged. The actual debit occurs
        // together, only once all players have loaded the combat scene.
        if n(&conquest::tickets(db,s,*member,0).await?,"NewValue")<1 {return Err(rule("NotEnoughDungeonKey"));}
        let user=sqlx::query("SELECT a.nick,u.team_level,u.avatar_hero_index FROM accounts a JOIN user_info u USING(account_id) WHERE a.account_id=?")
            .bind(member).fetch_one(&mut *db).await?;
        let mut account=conquest::combat_buffs(db,s,*member).await?;
        account["UserInfo"]=json!({"AccountId":member,"Nick":user.get::<String,_>("nick"),"TeamLevel":user.get::<i64,_>("team_level"),"AvatarHeroIndex":user.get::<i64,_>("avatar_hero_index")});
        account["HeroInfos"]=json!(heroes);account["AiHeroInfos"]=json!({});account["GroupHeroInfos"]=json!({});accounts.push(account);
        let mut entry=json!({"RunId":id,"CoopRunId":id,"CoopMaster":master,"CoopCharged":false,"ServiceOwned":true,
            "ChapterIndex":9900,"DungeonIndex":n(&room,"RaidLevel"),"DungeonDifficulty":r.number("DungeonDifficulty",0)?,"Heroes":own,"Request":req.0});
        let mut out=response(s,"campaign/begin_campaign");conquest::enter(db,s,*member,&req,&mut entry,&mut out).await?;
        out["BattleServerAddress"]=json!(s.conquest.address);out["BattleServerPort"]=json!(s.conquest.port);
        out["MasterServerAddress"]=json!(s.conquest.address);out["MasterServerPort"]=json!(s.conquest.port);
        out["StaminaResult"]=conquest::tickets(db,s,*member,0).await?;out["MultiplayRewardItems"]=json!([]);
        entries.push((*member,entry,out));
    }
    if all.len()>7 {return Err(rule("NotMatchHeroIndices"));}
    let payload=json!({"RunId":id,"BossHp":entries[0].1["ConquestBossHp"],"BossIndex":entries[0].1["ConquestBossIndex"],"Seed":(now()%i32::MAX as i64),
        "Simulation":{"Uid":id,"ChapterIndex":9900,"DungeonIndex":n(&room,"RaidLevel"),"DungeonDifficulty":r.number("DungeonDifficulty",0)?,"RepeatCount":1,"RemainCount":1,"AccountInfos":accounts}});
    let result=entries.iter().find(|v|v.0==a).unwrap().2.clone();
    for (member,entry,out) in entries {
        sqlx::query("INSERT INTO battle_runs(account,run_id,started,completed,entry,begin_response) VALUES(?,?,?,0,?,?) ON CONFLICT(account) DO UPDATE SET run_id=excluded.run_id,started=excluded.started,completed=0,entry=excluded.entry,begin_response=excluded.begin_response")
            .bind(member).bind(format!("{id}:{member}")).bind(now()).bind(entry.to_string()).bind(out.to_string()).execute(&mut *db).await?;
    }
    room["Status"]=json!(2);room["ConquestRunId"]=json!(id);
    sqlx::query("UPDATE battle_rooms SET data=?,updated=? WHERE id=?").bind(room.to_string()).bind(now()).bind(room_id).execute(&mut *db).await?;
    s.conquest.runs.insert(id.clone(),Arc::new(Mutex::new(Run{id,worker:worker_id,master,room:room_id,members,payload,
        entered:BTreeSet::new(),ready:BTreeSet::new(),sent:BTreeSet::new(),logs:vec![],result:None,started:false,prepared:false,created:now(),time_ms:0})));
    Ok(result)
}
fn ensure_ban(s:&AppState,ban:i64,hero:i64)->Result<()> {
    let code=s.tables.battle.find("BattleHero",&[("Index",hero)]).and_then(|v|v["CodeName"].as_str()).unwrap_or("");
    if s.tables.arena_guild.rows("BanRule").iter().any(|v|n(v,"Index")==ban && v["BanValue2"].as_str().unwrap_or("").split(',').any(|name|name==code)) {
        return Err(rule("NotAvailableHero"));
    }
    Ok(())
}

pub(crate) async fn party_start(db:&mut SqliteConnection,s:&AppState,a:i64,room:&Value)->Result<Value> {
    let id=room["ConquestRunId"].as_str().ok_or_else(||rule("InvalidRoomInfo"))?;
    if !s.conquest.runs.contains_key(id) {return Err(rule("BattleServerNotFound"));}
    if n(room,"MasterAccountId")!=a {return Err(rule("WrongMember"));}
    let members:Vec<i64>=sqlx::query_scalar("SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account").bind(n(room,"RoomNo")).fetch_all(&mut *db).await?;
    let mut heroes=vec![];let mut owners=vec![];
    for member in &members {
        let raw:String=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(member).fetch_one(&mut *db).await?;
        let entry:Value=read_json(&raw)?;
        for hero in entry["Heroes"].as_array().unwrap() {heroes.push(hero.clone());owners.push(json!(member));}
    }
    Ok(json!({"BattleServerAddress":s.conquest.address,"BattleServerPort":s.conquest.port,"MasterServerAddress":s.conquest.address,"MasterServerPort":s.conquest.port,
        "RaidIndex":90001,"RaidLevel":room["RaidLevel"],"RewardItems":[],"OnetimeBooster":false,"HeroIndices":heroes,"AccountIds":owners}))
}

pub(crate) async fn serve(listener:TcpListener,s:AppState)->std::io::Result<()> {
    let cleanup=s.clone();tokio::spawn(async move {
        let mut interval=tokio::time::interval(std::time::Duration::from_secs(15));
        loop {interval.tick().await;
            let handles:Vec<_>=cleanup.conquest.runs.iter().map(|v|v.value().clone()).collect();
            for handle in handles {let run=handle.lock().await;
                if run.result.is_some() && now()-run.created>1800 {cleanup.conquest.runs.remove(&run.id);}
                else if run.result.is_none() && now()-run.created>if run.started {1200}else{120} {
                    if let Err(error)=cancel(&cleanup,&run).await {tracing::warn!(%error,"Conquest cleanup failed");}
                }
            }
        }
    });
    loop {let (socket,_)=listener.accept().await?;let s=s.clone();tokio::spawn(async move {
        if let Err(error)=connection(socket,s).await {tracing::debug!(%error,"Conquest connection closed");}
    });}
}
// A process restart cannot resume a native worker's in-memory engine. Recover
// only unfinished Conquest service runs, atomically and once; ordinary runs and
// already committed scores are left intact.
pub(crate) async fn recover_orphaned(s:&AppState)->Result<()> {
    let mut db=s.db.begin().await?;
    sqlx::query("UPDATE battle_runs SET started=started WHERE completed=0 AND json_extract(entry,'$.CoopRunId') IS NOT NULL").execute(&mut *db).await?;
    let entries:Vec<(i64,String)>=sqlx::query_as("SELECT account,entry FROM battle_runs WHERE completed=0 AND json_extract(entry,'$.CoopRunId') IS NOT NULL").fetch_all(&mut *db).await?;
    for (a,raw) in entries {
        let entry:Value=read_json(&raw)?;
        if entry["CoopCharged"]==true {
            sqlx::query("UPDATE community_state SET data=json_set(data,'$.Count',json_extract(data,'$.Count')+1) WHERE owner=? AND kind='guild_conquest_keys' AND idx=0").bind(a).execute(&mut *db).await?;
        }
        sqlx::query("UPDATE battle_runs SET completed=1,entry=json_set(entry,'$.CoopCancelled',json('true')) WHERE account=? AND completed=0").bind(a).execute(&mut *db).await?;
    }
    sqlx::query("UPDATE battle_rooms SET data=json_remove(json_set(data,'$.Status',0),'$.ConquestRunId') WHERE json_extract(data,'$.ConquestRunId') IS NOT NULL").execute(&mut *db).await?;
    db.commit().await?;Ok(())
}
async fn connection(socket:TcpStream,s:AppState)->Result<()> {
    socket.set_nodelay(true).map_err(|_|rule("InvalidValue"))?;
    connection_stream(socket,s,true).await
}
pub(crate) async fn connection_stream<S>(socket:S,s:AppState,allow_worker:bool)->Result<()>
where S: tokio::io::AsyncRead + tokio::io::AsyncWrite + Unpin + Send + 'static {
    let (mut reader,mut writer)=tokio::io::split(socket);
    let (tx,mut rx)=mpsc::channel::<Packet>(1024);let connection_id=uuid::Uuid::new_v4().to_string();
    let writer_task=tokio::spawn(async move {
        while let Some((name,body))=rx.recv().await {if writer.write_all(&encode_packet(&name,&body)).await.is_err() {break;}}
    });
    let mut buffer=vec![];let mut chunk=[0_u8;16384];let mut account=0;let mut worker=false;
    let outcome:Result<()>=async {
        loop {
            let count=tokio::time::timeout(std::time::Duration::from_secs(90),reader.read(&mut chunk)).await.map_err(|_|rule("SessionExpired"))?.map_err(|_|rule("InvalidValue"))?;
            if count==0 {return Ok(());}
            buffer.extend_from_slice(&chunk[..count]);if buffer.len()>8*1024*1024 {return Err(rule("InvalidValue"));}
            while let Some(end)=buffer.iter().position(|v|*v==b'\n') {
                let frame:Vec<_>=buffer.drain(..=end).collect();let (name,body)=decode_packet(&frame)?;
                if account==0 && !worker {
                    if name=="WorkerLogin" {
                        if !allow_worker {return Err(rule("Unauthorized"));}
                        let key=s.battle_service_key.as_ref().as_ref().ok_or_else(||rule("Unauthorized"))?;
                        let supplied=body["Key"].as_str().unwrap_or("");
                        if key.len()!=supplied.len() || !key.as_bytes().iter().zip(supplied.as_bytes()).fold(0_u8,|v,(a,b)|v|(a^b)).eq(&0) {return Err(rule("Unauthorized"));}
                        let mut active=s.conquest.worker.write().await;
                        if active.is_some() {return Err(rule("AlreadyOnBattleHero"));}
                        *active=Some(Worker{id:connection_id.clone(),sender:tx.clone()});worker=true;
                        tx.send(("WorkerLoginRes".into(),json!({"Result":"Success"}))).await.map_err(|_|rule("InvalidValue"))?;
                    } else if name=="BattleLoginReq" {
                        let session=s.get_session(body["SessionKey"].as_str().unwrap_or("")).ok_or_else(||rule("SessionExpired"))?;
                        account=session.account_id;
                        if account<=0 || account!=number(&body,"AccountId") {return Err(rule("WrongMember"));}
                        let mut db=s.db.acquire().await?;
                        let entry:Option<String>=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=?").bind(account).fetch_optional(&mut *db).await?;
                        let entry:Value=read_json(&entry.ok_or_else(||rule("MultiplayInfoNotFound"))?)?;
                        let id=entry["CoopRunId"].as_str().ok_or_else(||rule("MultiplayInfoNotFound"))?;
                        if n(&entry,"CoopMaster")!=number(&body,"MasterId") {return Err(rule("WrongMember"));}
                        let handle=s.conquest.runs.get(id).ok_or_else(||rule("BattleServerNotFound"))?.clone();let run=handle.lock().await;
                        if !run.members.contains(&account) {return Err(rule("WrongMember"));}
                        if s.conquest.peers.contains_key(&account) {return Err(rule("AlreadyOnBattleHero"));}
                        s.conquest.peers.insert(account,Peer{id:connection_id.clone(),run:id.into(),sender:tx.clone()});
                        tx.send(("BattleLoginRes".into(),json!({"Result":"Success","RequestId":body["RequestId"]}))).await.map_err(|_|rule("InvalidValue"))?;
                    } else {return Err(rule("Unauthorized"));}
                } else if worker {worker_packet(&s,&connection_id,&name,body).await?;}
                else {player_packet(&s,account,&name,body,&tx).await?;}
            }
        }
    }.await;
    if worker {
        let mut current=s.conquest.worker.write().await;
        if current.as_ref().is_some_and(|v|v.id==connection_id) {*current=None;}
        drop(current);
        let handles:Vec<_>=s.conquest.runs.iter().map(|v|v.value().clone()).collect();
        for handle in handles {let run=handle.lock().await;if run.worker==connection_id && run.result.is_none() {let _=cancel(&s,&run).await;}}
    } else if account>0 {s.conquest.peers.remove_if(&account,|_,v|v.id==connection_id);}
    writer_task.abort();outcome
}
async fn player_packet(s:&AppState,a:i64,name:&str,body:Value,tx:&Sender)->Result<()> {
    let id=s.conquest.peers.get(&a).ok_or_else(||rule("WrongMember"))?.run.clone();
    let handle=s.conquest.runs.get(&id).ok_or_else(||rule("BattleServerNotFound"))?.clone();let mut run=handle.lock().await;
    let response_name=if matches!(name,"ReserveOnSkillReq"|"ReserveOffSkillReq"|"FireNextSkillReq"|"SetManualTargetReq"|"SetManualMoveReq") {"BaseCreatureActionRes".into()}else{name.strip_suffix("Req").map(|n|format!("{n}Res")).ok_or_else(||rule("InvalidRequest"))?};
    let mut reply=json!({"Result":"Success","RequestId":body["RequestId"]});
    match name {
        "BattleEnterReq" => {
            // Replay only this participant's current battle, never another room.
            for (name,body) in &run.logs {tx.send((name.clone(),body.clone())).await.map_err(|_|rule("InvalidValue"))?;}
            run.entered.insert(a);
            if run.entered.len()==run.members.len() && !run.prepared {
                s.conquest.send_worker("WorkerPrepare",run.payload.clone()).await?;run.prepared=true;
            }
        }
        "BattleStartReq" => {
            run.ready.insert(a);
            if !run.started && run.prepared && run.ready.len()==run.members.len() {
                let mut db=s.db.begin().await?;sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(a).execute(&mut *db).await?;
                for member in &run.members {
                    conquest::tickets(&mut db,s,*member,1).await?;
                    sqlx::query("UPDATE battle_runs SET entry=json_set(entry,'$.CoopCharged',json('true')) WHERE account=? AND completed=0 AND json_extract(entry,'$.CoopRunId')=?")
                        .bind(member).bind(&id).execute(&mut *db).await?;
                }
                db.commit().await?;s.conquest.send_worker("WorkerStart",json!({"RunId":id})).await?;run.started=true;
            }
        }
        "BattleWaveResumeReq"=>{s.conquest.send_worker("WorkerResume",json!({"RunId":id})).await?;}
        "PingReq"|"PingIdleReq"=> {
            reply["ClientTimeMs"]=body["ClientTimeMs"].clone();reply["ServerTimeMs"]=json!(run.time_ms);
            sqlx::query("UPDATE battle_room_members SET updated=? WHERE account=?").bind(now()).bind(a).execute(&s.db).await?;
            if body["BattleStopped"]==true && !run.sent.contains(&a) {
                if let Some(result)=run.result.clone() {tx.try_send(("BattleResultNot".into(),result)).map_err(|_|rule("InvalidValue"))?;run.sent.insert(a);}
            }
        }
        "ReserveOnSkillReq"|"ReserveOffSkillReq"|"FireNextSkillReq"|"SetManualTargetReq"|"SetManualMoveReq" => {
            if !run.started || run.result.is_some() {return Err(rule("InvalidValue"));}
            if name=="ReserveOnSkillReq" && ![1,2,3,4,10].contains(&number(&body,"SkillSlotIndex")) {return Err(rule("InvalidValue"));}
            s.conquest.send_worker("WorkerCommand",json!({"RunId":id,"AccountId":a,"Name":name,"Body":body})).await?;
        }
        "WithdrawBattleReq" => {
            if run.started && run.result.is_none() {
                s.conquest.send_worker("WorkerWithdraw",json!({"RunId":id,"AccountId":a})).await?;
            }
        }
        "BattleLeaveReq"|"BattleEndReq"=>{},
        _=>return Err(rule("InvalidRequest")),
    }
    tx.send((response_name,reply)).await.map_err(|_|rule("InvalidValue"))?;Ok(())
}
async fn worker_packet(s:&AppState,worker:&str,name:&str,body:Value)->Result<()> {
    if name=="WorkerPing" {return Ok(());}
    let id=body["RunId"].as_str().ok_or_else(||rule("InvalidValue"))?;
    let Some(handle)=s.conquest.runs.get(id).map(|v|v.value().clone()) else {return Ok(());};let mut run=handle.lock().await;
    if run.worker!=worker {return Err(rule("Unauthorized"));}
    if run.result.is_some() {return Ok(());}
    if name=="WorkerPacket" {
        let name=body["Name"].as_str().ok_or_else(||rule("InvalidValue"))?;
        if !matches!(name,"BattleLogNot"|"WaveReadyNot"|"WaveEndNot") {return Err(rule("InvalidValue"));}
        let packet=body["Body"].clone();
        if name=="BattleLogNot" {run.time_ms=number(&packet,"ServerTimeMs");}
        if run.logs.len()>20000 {return Err(rule("InvalidValue"));}
        run.logs.push((name.into(),packet.clone()));s.conquest.notify(&run,name,packet);
    } else if name=="WorkerResult" {
        if !run.started {return Err(rule("InvalidValue"));}
        #[cfg(test)]
        if let Some(path)=s.tables.arena_guild.rules["NativeConquestCapturePath"].as_str() {std::fs::write(path,body.to_string()).unwrap();}
        settle(s,&run,&body).await?;
        let alive=body["Creatures"].as_array().ok_or_else(||rule("InvalidValue"))?.iter().filter(|v|number(v,"TeamId")==0 && number(v,"Hp")>0).map(|v|json!(number(v,"Index"))).collect::<Vec<_>>();
        let result=json!({"Win":body["Win"],"Star":if body["Win"]==true {number(&run.payload["Simulation"],"DungeonDifficulty")*10+3}else{0},"Dropped":false,"KillCount":{},"AliveHeroIndices":alive,"MaxWaveIndex":body["MaxWaveIndex"]});
        // Results wait for each native replay thread to stop (authenticated
        // Ping/PingIdle acknowledgement), so UI never reads a live instance.
        run.result=Some(result);
    } else {return Err(rule("InvalidRequest"));}
    Ok(())
}
async fn settle(s:&AppState,run:&Run,body:&Value)->Result<()> {
    let creatures=body["Creatures"].as_array().ok_or_else(||rule("InvalidValue"))?;
    let boss=number(&run.payload,"BossIndex");let hp=number(&run.payload,"BossHp");
    let boss_report=creatures.iter().find(|v|number(v,"TeamId")==1 && number(v,"Index")==boss).ok_or_else(||rule("InvalidValue"))?;
    let end_hp=number(boss_report,"Hp");
    if end_hp<0 || end_hp>hp {return Err(rule("InvalidValue"));}
    let contributions=body["AccountDamage"].as_object().ok_or_else(||rule("InvalidValue"))?;
    if contributions.len()!=run.members.len() {return Err(rule("InvalidValue"));}
    let total=run.members.iter().try_fold(0_i64,|sum,a| {
        let damage=contributions.get(&a.to_string()).and_then(Value::as_i64).ok_or_else(||rule("InvalidValue"))?;if damage<0 {return Err(rule("InvalidValue"));}
        sum.checked_add(damage).ok_or_else(||rule("InvalidValue"))
    })?;
    let lost_hp=if number(&run.payload["Simulation"],"DungeonIndex")==3 {number(boss_report,"GivedDamage")} else {hp-end_hp};
    if lost_hp<0 {return Err(rule("InvalidValue"));}let mut assigned=0_i64;
    let mut db=s.db.begin().await?;sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(run.master).execute(&mut *db).await?;
    for (index,a) in run.members.iter().enumerate() {
        let (completed,raw):(i64,String)=sqlx::query_as("SELECT completed,entry FROM battle_runs WHERE account=?").bind(a).fetch_one(&mut *db).await?;
        let mut entry:Value=read_json(&raw)?;
        if entry["CoopRunId"]!=run.id || entry["CoopCharged"]!=true {return Err(rule("InvalidValue"));}
        if completed>0 {continue;}
        let own=creatures.iter().filter(|v|number(v,"TeamId")==0 && number(v,"AccountId")==*a).cloned().collect::<Vec<_>>();
        let sum=contributions[&a.to_string()].as_i64().unwrap();
        let portion=if index==run.members.len()-1 {lost_hp-assigned}else if total>0 {(lost_hp as i128*sum as i128/total as i128) as i64}else{lost_hp/run.members.len() as i64};assigned+=portion;
        let mut report=own;report.push(json!({"Index":boss,"TeamId":1,"Hp":if number(&entry,"ConquestLevel")==3 {end_hp}else{hp-portion},"GivedDamage":portion}));
        let request=request(json!({"CreatureInfoString":json!(report).to_string(),"Completed":body["Win"],"PureBattleTime":number(body,"TimeMs")/1000}));
        let mut out=response(s,"campaign/end_campaign");conquest::finish(&mut db,s,*a,&request,&entry,&mut out).await?;
        out["CampaignResults"]=json!([]);entry["ConquestEndResponse"]=out;
        sqlx::query("UPDATE battle_runs SET completed=1,entry=? WHERE account=? AND completed=0").bind(entry.to_string()).bind(a).execute(&mut *db).await?;
    }
    sqlx::query("UPDATE battle_rooms SET data=json_remove(json_set(data,'$.Status',0),'$.ConquestRunId'),updated=? WHERE id=? AND json_extract(data,'$.ConquestRunId')=?")
        .bind(now()).bind(run.room).bind(&run.id).execute(&mut *db).await?;
    db.commit().await?;Ok(())
}
async fn cancel(s:&AppState,run:&Run)->Result<()> {
    let mut db=s.db.begin().await?;sqlx::query("UPDATE accounts SET last_login=last_login WHERE account_id=?").bind(run.master).execute(&mut *db).await?;
    for a in &run.members {
        let raw:Option<String>=sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND json_extract(entry,'$.CoopRunId')=?").bind(a).bind(&run.id).fetch_optional(&mut *db).await?;
        if let Some(raw)=raw {
            let entry:Value=read_json(&raw)?;
            if entry["CoopCharged"]==true {
                sqlx::query("UPDATE community_state SET data=json_set(data,'$.Count',json_extract(data,'$.Count')+1) WHERE owner=? AND kind='guild_conquest_keys' AND idx=0").bind(a).execute(&mut *db).await?;
            }
            sqlx::query("UPDATE battle_runs SET completed=1,entry=json_set(entry,'$.CoopCancelled',json('true')) WHERE account=?").bind(a).execute(&mut *db).await?;
        }
    }
    sqlx::query("UPDATE battle_rooms SET data=json_remove(json_set(data,'$.Status',0),'$.ConquestRunId'),updated=? WHERE id=? AND json_extract(data,'$.ConquestRunId')=?")
        .bind(now()).bind(run.room).bind(&run.id).execute(&mut *db).await?;db.commit().await?;
    s.conquest.notify(run,"BattleEndNot",json!({"Reason":"MasterLogOff"}));
    let _=s.conquest.send_worker("WorkerCancel",json!({"RunId":run.id})).await;s.conquest.runs.remove(&run.id);Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::api::community::tests::{setup,login,call,create};
    async fn fixture()->(AppState,Vec<Value>,i64,mpsc::Receiver<Packet>,Vec<mpsc::Receiver<Packet>>) {
        fixture_level(1).await
    }
    async fn fixture_level(level:i64)->(AppState,Vec<Value>,i64,mpsc::Receiver<Packet>,Vec<mpsc::Receiver<Packet>>) {
        let mut s=setup().await;
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).arena_guild).rules["GuildConquestTestTime"]=json!(1767628800+3600);
        let mut users=vec![];
        for i in 0..3 {users.push(login(&s,&format!("conquest-coop-{i}")).await);}
        let g=create(&s,&users[0],"CoopGuild",1).await;
        for u in users.iter().skip(1) {assert_eq!(call(&s,u,"guild/request_join_guild",&format!("GuildId={g}")).await["Result"],"Success");}
        assert_eq!(call(&s,&users[0],"guild_suppress/apply_guild_suppress","").await["Result"],"Success");
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).arena_guild).rules["GuildConquestTestTime"]=json!(1767628800+50*3600);
        let members=users.iter().map(|u|n(&u["UserInfo"],"AccountId")).collect::<Vec<_>>();
        let room=json!({"RoomNo":1,"MasterAccountId":members[0],"RaidIndex":90001,"RaidLevel":level,"ChapterIndex":9900,"DungeonIndex":level,"Status":0,"Capacity":3,"Opened":1});
        sqlx::query("INSERT INTO battle_rooms(id,family,master,data,updated) VALUES(1,'raid',?,?,?)").bind(members[0]).bind(room.to_string()).bind(now()).execute(&s.db).await.unwrap();
        for (i,a) in members.iter().enumerate() {
            for hero in 2..=4 {sqlx::query("INSERT OR IGNORE INTO heroes(account_id,hero_id,hero_index,level,star) VALUES(?,?,?,1,2)").bind(a).bind(hero).bind(hero).execute(&s.db).await.unwrap();}
            sqlx::query("UPDATE heroes SET level=100,star=5,transcend=5 WHERE account_id=?").bind(a).execute(&s.db).await.unwrap();
            sqlx::query("INSERT INTO battle_room_members(room,account,joined,updated) VALUES(1,?,?,?)").bind(a).bind(now()).bind(now()).execute(&s.db).await.unwrap();
            let mut db=s.db.acquire().await.unwrap();
            put(&mut db,*a,"party_member",1,&json!({"IsBattleReady":true,"DeckHeros":{(i+1).to_string():{"HeroIndex":i+1}},"SubDeckHeros":{}})).await.unwrap();
        }
        let (worker,rx)=mpsc::channel(100);*s.conquest.worker.write().await=Some(Worker{id:"test-worker".into(),sender:worker});
        let mut db=s.db.begin().await.unwrap();
        let args=request(json!({"RaidRoomNo":1,"RaidIndex":90001,"RaidLevel":level,"ChapterIndex":9900,"DungeonIndex":level,"DungeonDifficulty":0,
            "MultiplayMasterId":members[0],"MultiplayMemberIds":members,"HeroIndices":[1,4],"AccountIds":[members[0],members[0]]}));
        let begun=begin(&mut db,&s,members[0],&args).await.unwrap();assert_eq!(begun["StaminaResult"]["NewValue"],2);db.commit().await.unwrap();
        let id=s.conquest.runs.iter().next().unwrap().key().clone();let mut receivers=vec![];
        for a in &members {let (tx,rx)=mpsc::channel(100);s.conquest.peers.insert(*a,Peer{id:a.to_string(),run:id.clone(),sender:tx});receivers.push(rx);}
        (s,users,g,rx,receivers)
    }
    async fn start(s:&AppState)->String {
        let id=s.conquest.runs.iter().next().unwrap().key().clone();let handle=s.conquest.runs.get(&id).unwrap().clone();
        let members=handle.lock().await.members.clone();
        for a in &members {let tx=s.conquest.peers.get(a).unwrap().sender.clone();player_packet(s,*a,"BattleEnterReq",json!({"RequestId":1}),&tx).await.unwrap();}
        for a in &members {let tx=s.conquest.peers.get(a).unwrap().sender.clone();player_packet(s,*a,"BattleStartReq",json!({"RequestId":2}),&tx).await.unwrap();}
        id
    }
    #[tokio::test]
    async fn websocket_three_player_battle_and_reconnect() {
        use crate::websocket::tests::{server, Client};
        let (s,users,_,mut worker,_clients)=fixture().await;
        s.conquest.peers.clear();
        let server=server(s.clone()).await;
        let master=users[0]["UserInfo"]["AccountId"].clone();
        let auth=|u:&Value|json!({"AccountId":u["UserInfo"]["AccountId"],"SessionKey":u["UserInfo"]["SessionKey"],"MasterId":master,"RequestId":1});
        let mut clients=vec![];
        for u in &users {
            let mut c=Client::connect(&server,"battle").await;
            c.send("BattleLoginReq",auth(u)).await;
            assert_eq!(c.recv("BattleLoginRes").await["Result"],"Success");
            c.send("BattleEnterReq",json!({"RequestId":2})).await;
            assert_eq!(c.recv("BattleEnterRes").await["Result"],"Success");
            clients.push(c);
        }
        assert_eq!(worker.recv().await.unwrap().0,"WorkerPrepare");
        for c in &mut clients {
            c.send("BattleStartReq",json!({"RequestId":3})).await;
            assert_eq!(c.recv("BattleStartRes").await["Result"],"Success");
        }
        assert_eq!(worker.recv().await.unwrap().0,"WorkerStart");
        let id=s.conquest.runs.iter().next().unwrap().key().clone();
        worker_packet(&s,"test-worker","WorkerPacket",json!({"RunId":id,"Name":"BattleLogNot","Body":{"ServerTimeMs":42,"Logs":[]}})).await.unwrap();
        for c in &mut clients {assert_eq!(c.recv("BattleLogNot").await["ServerTimeMs"],42);}
        clients[0].send("PingReq",json!({"RequestId":4,"ClientTimeMs":123})).await;
        assert_eq!(clients[0].recv("PingRes").await["ClientTimeMs"],123);
        let lost=clients.pop().unwrap(); drop(lost);
        let account=users[2]["UserInfo"]["AccountId"].as_i64().unwrap();
        tokio::time::timeout(std::time::Duration::from_secs(5),async {
            while s.conquest.peers.contains_key(&account) {tokio::time::sleep(std::time::Duration::from_millis(10)).await;}
        }).await.unwrap();
        let mut again=Client::connect(&server,"battle").await;
        again.send("BattleLoginReq",auth(&users[2])).await;
        assert_eq!(again.recv("BattleLoginRes").await["Result"],"Success");
        again.send("BattleEnterReq",json!({"RequestId":5})).await;
        assert_eq!(again.recv("BattleLogNot").await["ServerTimeMs"],42);
        assert_eq!(again.recv("BattleEnterRes").await["Result"],"Success");
        assert!(worker.try_recv().is_err(),"reconnect must not start the battle twice");
    }
    #[tokio::test]
    #[ignore = "Exports a trusted fixture for the isolated native engine integration test"]
    async fn conquest_native_worker_fixture() {
        let (s,_,_,_worker,_clients)=fixture().await;
        let path=std::env::var("CONQUEST_FIXTURE_PATH").expect("CONQUEST_FIXTURE_PATH is required");
        let handle=s.conquest.runs.iter().next().unwrap().value().clone();
        std::fs::write(path,handle.lock().await.payload.to_string()).unwrap();
    }
    #[tokio::test]
    #[ignore = "Requires the patched native Unity worker executable in CONQUEST_NATIVE_EXECUTABLE"]
    async fn conquest_native_three_player_tcp_battle() {
        use tokio::io::{AsyncBufReadExt,BufReader};
        struct Native(std::process::Child);
        impl Drop for Native {fn drop(&mut self){let _=self.0.kill();let _=self.0.wait();}}
        let level=std::env::var("CONQUEST_NATIVE_LEVEL").ok().and_then(|v|v.parse().ok()).unwrap_or(1);
        let (mut s,users,g,_worker,_clients)=fixture_level(level).await;
        Arc::make_mut(&mut Arc::make_mut(&mut s.tables).arena_guild).rules["NativeConquestCapturePath"]=json!(format!("target/conquest-lab/native-result-{level}.json"));
        let key=uuid::Uuid::new_v4().to_string();s.battle_service_key=Arc::new(Some(key.clone()));
        *s.conquest.worker.write().await=None;s.conquest.peers.clear();
        let listener=TcpListener::bind("127.0.0.1:0").await.unwrap();let port=listener.local_addr().unwrap().port();
        let server=tokio::spawn(serve(listener,s.clone()));
        let executable=std::env::var("CONQUEST_NATIVE_EXECUTABLE").expect("CONQUEST_NATIVE_EXECUTABLE");
        let executable=std::path::Path::new(&executable).canonicalize().unwrap();
        let mut command=std::process::Command::new(&executable);
        command.current_dir(executable.parent().unwrap()).args(["-batchmode","-nographics","-logFile"])
            .arg(std::env::current_dir().unwrap().join("target/conquest-lab/native-tcp.log"))
            .env("SPRK_CONQUEST_WORKER_KEY",key).env("SPRK_CONQUEST_WORKER_HOST","127.0.0.1").env("SPRK_CONQUEST_WORKER_PORT",port.to_string());
        #[cfg(windows)] {use std::os::windows::process::CommandExt;command.creation_flags(0x08000000);}
        let _native=Native(command.spawn().unwrap());
        tokio::time::timeout(std::time::Duration::from_secs(90),async {
            while s.conquest.worker.read().await.is_none(){tokio::time::sleep(std::time::Duration::from_millis(100)).await;}
        }).await.expect("Native worker did not authenticate");
        let handle=s.conquest.runs.iter().next().unwrap().value().clone();
        handle.lock().await.worker=s.conquest.worker.read().await.as_ref().unwrap().id.clone();
        let initial_hp=number(&handle.lock().await.payload,"BossHp");
        let master=n(&users[0]["UserInfo"],"AccountId");let mut clients=vec![];
        for user in &users {
            let socket=TcpStream::connect(("127.0.0.1",port)).await.unwrap();let (reader,mut writer)=socket.into_split();let mut reader=BufReader::new(reader);
            writer.write_all(&encode_packet("BattleLoginReq",&json!({"AccountId":n(&user["UserInfo"],"AccountId"),"MasterId":master,"SessionKey":user["UserInfo"]["SessionKey"],"RequestId":1}))).await.unwrap();
            let mut line=String::new();reader.read_line(&mut line).await.unwrap();assert_eq!(decode_packet(line.as_bytes()).unwrap().0,"BattleLoginRes");
            writer.write_all(&encode_packet("BattleEnterReq",&json!({"RequestId":2}))).await.unwrap();
            clients.push(tokio::spawn(async move {
                let mut logs=vec![];let mut request_id=3;let mut ended=false;
                loop {
                    let mut line=String::new();let count=tokio::time::timeout(std::time::Duration::from_secs(60),reader.read_line(&mut line)).await.unwrap().unwrap();assert!(count>0,"Battle transport closed before result");
                    let (name,body)=decode_packet(line.as_bytes()).unwrap();
                    if name=="BattleLogNot" {logs.push(body.clone());}
                    if name=="WaveReadyNot" {
                        writer.write_all(&encode_packet("BattleStartReq",&json!({"RequestId":request_id}))).await.unwrap();request_id+=1;
                        writer.write_all(&encode_packet("BattleWaveResumeReq",&json!({"RequestId":request_id}))).await.unwrap();request_id+=1;
                    }
                    if name=="WaveEndNot" {ended=true;}
                    if ended && (name=="WaveEndNot" || name=="PingRes") {
                        tokio::time::sleep(std::time::Duration::from_millis(100)).await;
                        writer.write_all(&encode_packet("PingReq",&json!({"RequestId":request_id,"BattleStopped":true,"ClientTimeMs":0}))).await.unwrap();request_id+=1;
                    }
                    if name=="BattleResultNot" {return(logs,body);}
                }
            }));
        }
        let mut completed=vec![];
        for client in clients {completed.push(tokio::time::timeout(std::time::Duration::from_secs(90),client).await.unwrap().unwrap());}
        assert!(!completed[0].0.is_empty());assert_eq!(completed[0],completed[1]);assert_eq!(completed[1],completed[2]);
        let total:i64=sqlx::query_scalar("SELECT SUM(score) FROM guild_battle_scores WHERE guild_id=? AND kind='suppress'").bind(g).fetch_one(&s.db).await.unwrap();assert!(total>0,"Native battle must deal nonzero damage");
        let board=call(&s,&users[0],"guild_suppress/get_guild_suppress_status_board","").await;
        assert_eq!(board["GuildSuppressStatusBoardInfos"][0]["GuildSuppressPlayInfos"][(level-1) as usize]["MonsterHp0"],if level==3 {initial_hp}else{initial_hp-total});
        for user in users {let mut db=s.db.acquire().await.unwrap();assert_eq!(conquest::tickets(&mut db,&s,n(&user["UserInfo"],"AccountId"),0).await.unwrap()["NewValue"],1);}
        server.abort();
    }
    #[tokio::test]
    async fn conquest_coop_settles_shared_hp_once_and_scores_each_participant() {
        let (s,users,g,mut worker,_clients)=fixture().await;let id=start(&s).await;
        assert_eq!(worker.recv().await.unwrap().0,"WorkerPrepare");assert_eq!(worker.recv().await.unwrap().0,"WorkerStart");
        let handle=s.conquest.runs.get(&id).unwrap().clone();let run=handle.lock().await;let hp=number(&run.payload,"BossHp");drop(run);
        let mut creatures=vec![json!({"Index":11301,"TeamId":1,"Hp":hp-600})];
        for (i,u) in users.iter().enumerate() {
            creatures.push(json!({"Index":i+1,"TeamId":0,"AccountId":n(&u["UserInfo"],"AccountId"),"Hp":0,"GivedDamage":0,"DealtDamage":(i+1)*100}));
            let a=n(&u["UserInfo"],"AccountId");let mut db=s.db.acquire().await.unwrap();assert_eq!(conquest::tickets(&mut db,&s,a,0).await.unwrap()["NewValue"],1);
        }
        let damage=users.iter().enumerate().map(|(i,u)|(n(&u["UserInfo"],"AccountId").to_string(),json!((i+1)*100))).collect::<serde_json::Map<_,_>>();
        let result=json!({"RunId":id,"Win":false,"Creatures":creatures,"AccountDamage":damage,"TimeMs":10000,"MaxWaveIndex":1});
        worker_packet(&s,"forged-worker","WorkerResult",result.clone()).await.unwrap_err();
        worker_packet(&s,"test-worker","WorkerResult",result.clone()).await.unwrap();worker_packet(&s,"test-worker","WorkerResult",result).await.unwrap();
        let total:i64=sqlx::query_scalar("SELECT SUM(score) FROM guild_battle_scores WHERE guild_id=? AND kind='suppress'").bind(g).fetch_one(&s.db).await.unwrap();assert_eq!(total,600);
        for (i,u) in users.iter().enumerate() {
            let a=n(&u["UserInfo"],"AccountId");let score:i64=sqlx::query_scalar("SELECT score FROM guild_battle_scores WHERE account=? AND kind='suppress'").bind(a).fetch_one(&s.db).await.unwrap();assert_eq!(score,(i as i64+1)*100);
            let end=crate::api::battle::execute_request(&s,"campaign/end_campaign",Bytes::from(format!("SessionKey={}&ChapterIndex=9900&DungeonIndex=1&DungeonDifficulty=0&Completed=false",u["UserInfo"]["SessionKey"].as_str().unwrap()))).await.unwrap();
            assert_eq!(end["Result"],"Success","{end}");assert_eq!(end["GuildSuppressScoreInfo"]["TotalDamage"],score);
        }
        let board=call(&s,&users[0],"guild_suppress/get_guild_suppress_status_board","").await;
        assert_eq!(board["GuildSuppressStatusBoardInfos"][0]["GuildSuppressPlayInfos"][0]["MonsterHp0"],hp-600);
    }
    #[tokio::test]
    async fn conquest_coop_service_failure_restores_only_its_consumed_tickets_once() {
        let (s,users,_,_worker,_clients)=fixture().await;let id=start(&s).await;let handle=s.conquest.runs.get(&id).unwrap().clone();let run=handle.lock().await;
        cancel(&s,&run).await.unwrap();cancel(&s,&run).await.unwrap();
        for u in users {let mut db=s.db.acquire().await.unwrap();assert_eq!(conquest::tickets(&mut db,&s,n(&u["UserInfo"],"AccountId"),0).await.unwrap()["NewValue"],2);}
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM guild_battle_scores WHERE kind='suppress'").fetch_one(&s.db).await.unwrap(),0);
    }
    #[tokio::test]
    async fn conquest_coop_restart_recovers_once_without_changing_completed_scores() {
        let (s,users,_,_worker,_clients)=fixture().await;start(&s).await;
        recover_orphaned(&s).await.unwrap();recover_orphaned(&s).await.unwrap();
        for u in users {let mut db=s.db.acquire().await.unwrap();assert_eq!(conquest::tickets(&mut db,&s,n(&u["UserInfo"],"AccountId"),0).await.unwrap()["NewValue"],2);}
        assert_eq!(sqlx::query_scalar::<_,i64>("SELECT COUNT(*) FROM battle_runs WHERE completed=0").fetch_one(&s.db).await.unwrap(),0);
    }
}
