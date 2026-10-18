//! Durable Victory/Honor queues and atomic, service-owned settlement.
use super::*;
use crate::api::battle::cooperative;

// The original Unity marshalers encode creature integers as JSON strings.
fn native_number(v:&Value,key:&str)->i64 {
    v[key].as_i64().or_else(||v[key].as_str().and_then(|v|v.parse().ok())).unwrap_or(0)
}

pub(crate) async fn pair(
    db: &mut SqliteConnection, s: &AppState, a: i64, id: i64, mine: &Value,
) -> Result<Option<Value>> {
    let Some(worker) = cooperative::arena_worker(s).await else { return Ok(None); };
    let kind=n(mine,"Kind");
    let cutoff = now() - settings(s, "ArenaQueueStaleSeconds", 15).clamp(6, 120);
    let candidates: Vec<(i64, i64, String)> = sqlx::query_as(
        "SELECT id,account,data FROM arena_runs WHERE kind=? AND status='waiting' AND account!=? AND started>=? AND COALESCE(json_extract(data,'$.LastPoll'),started)>=? AND json_extract(data,'$.Season')=? ORDER BY started,id LIMIT 100")
        .bind(kind).bind(a).bind(now()-settings(s,"ArenaMatchExpirySeconds",900)).bind(cutoff)
        .bind(season(s).0).fetch_all(&mut *db).await?;
    if n(&match_tickets(db,s,a,kind,0).await?,"NewValue") < 1 {
        return Err(rule("NotEnoughSword"));
    }
    for (other_id, b, raw) in candidates {
        // Being logged in alone is insufficient: candidates must also have a
        // recently polled queue entry. Cancelled/bot/in-battle rows never qualify.
        if !s.sessions.iter().any(|v|v.account_id==b) { continue; }
        let other: Value = parse(&raw)?;
        if n(&other,"Expires") < now() || n(&match_tickets(db,s,b,kind,0).await?,"NewValue") < 1 { continue; }
        let uid = id.min(other_id);
        let mut accounts = vec![mine["Register"]["AccountInfo"].clone(),other["Register"]["AccountInfo"].clone()];
        if id > other_id { accounts.swap(0,1); }
        accounts[0]["TeamType"] = json!("Left");
        accounts[1]["TeamType"] = json!("Right");
        let info = json!({"Uid":uid,"ArenaType":if kind==1{"BanPick"}else{"Normal"},"SeasonIndex":season(s).0,
            "AccountInfos":accounts,"BattleServerAddress":s.conquest.address,"BattleServerPort":s.conquest.port,
            "ChapterIndex":1000,"DungeonIndex":1,"DungeonDifficulty":"Easy","OnlineGameSpeedRatio":100,"ExtraData":null,"CanCheat":false});
        let mut response = Value::Null;
        for (row_id, account_id, saved, enemy) in [(id,a,mine,b),(other_id,b,&other,a)] {
            let mut data = saved.clone();
            let wait = json!({"BaseResult":"Success","Result":"Success","MatchInfo":info,
                "MatchedNpcInfo":null,"RelayBattleServerAddress":"","RelayBattleServerPort":0,
                "SwordResult":match_tickets(db,s,account_id,kind,0).await?,"BattleInfo":battle_info_kind(db,s,account_id,kind).await?});
            data["Wait"] = wait.clone();
            data["Opponent"] = json!(enemy);
            data["OnlineMatchUid"] = json!(uid);
            data["ServiceOwned"] = json!(true);
            data["OnlineCharged"] = json!(false);
            data["Started"] = json!(now());
            sqlx::query("UPDATE arena_runs SET status='battle',data=? WHERE id=? AND status='waiting'")
                .bind(data.to_string()).bind(row_id).execute(&mut *db).await?;
            if account_id==a {response=wait;}
        }
        cooperative::create_arena(s, worker, info);
        return Ok(Some(response));
    }
    Ok(None)
}

pub(crate) async fn charge(db:&mut SqliteConnection,s:&AppState,uid:i64)->Result<()> {
    let rows:Vec<(i64,String)>=sqlx::query_as("SELECT account,data FROM arena_runs WHERE status='battle' AND json_extract(data,'$.OnlineMatchUid')=? ORDER BY id")
        .bind(uid).fetch_all(&mut *db).await?;
    if rows.len()!=2 {return Err(rule("MatchingAborted"));}
    for (a,raw) in rows {
        let data:Value=parse(&raw)?;
        if n(&data,"Season")!=season(s).0 {return Err(rule("MatchingAborted"));}
        if data["OnlineCharged"]!=true {
            match_tickets(db,s,a,n(&data,"Kind"),-1).await?;
            sqlx::query("UPDATE arena_runs SET data=json_set(data,'$.OnlineCharged',json('true')) WHERE account=? AND status='battle' AND json_extract(data,'$.OnlineMatchUid')=?")
                .bind(a).bind(uid).execute(&mut *db).await?;
        }
    }
    Ok(())
}

pub(crate) async fn finish(db:&mut SqliteConnection,s:&AppState,uid:i64,winner:i64,body:&Value,give_up:Option<i64>)->Result<()> {
    let rows:Vec<(i64,String)>=sqlx::query_as("SELECT account,data FROM arena_runs WHERE status='battle' AND json_extract(data,'$.OnlineMatchUid')=? ORDER BY id")
        .bind(uid).fetch_all(&mut *db).await?;
    if rows.is_empty() {return Ok(());} // duplicate worker delivery
    if rows.len()!=2 || !rows.iter().any(|(a,_)|*a==winner) {return Err(rule("MatchingAborted"));}
    for (a,raw) in rows {
        let data:Value=parse(&raw)?;
        if data["OnlineCharged"]!=true {return Err(rule("MatchingAborted"));}
        let alive=body["Creatures"].as_array().into_iter().flatten()
            .filter(|c|native_number(c,"AccountId")==a && native_number(c,"Hp")>0 && data["Heroes"].as_array().is_some_and(|h|h.contains(&json!(native_number(c,"Index")))))
            .map(|c|json!(native_number(c,"Index"))).collect::<Vec<_>>();
        let req=Request([("ArenaType".into(),n(&data,"Kind").to_string()),("Win".into(),if a==winner{"1"}else{"0"}.into()),
            ("PlayTime".into(),(native_number(body,"TimeMs")/1000).max(0).to_string()),("AliveHeroIndices".into(),json!(alive).to_string())].into());
        service_result(db,s,a,&req).await?;
        sqlx::query("UPDATE arena_runs SET data=json_set(data,'$.End.MatchResult.MatchUid',?,'$.End.MatchResult.GiveUp',json(?)) WHERE account=? AND status='complete' AND json_extract(data,'$.OnlineMatchUid')=?")
            .bind(uid).bind(if give_up==Some(a){"true"}else{"false"}).bind(a).bind(uid).execute(&mut *db).await?;
    }
    Ok(())
}

pub(crate) async fn cancel_account(db:&mut SqliteConnection,s:&AppState,a:i64)->Result<bool> {
    let raw:Option<String>=sqlx::query_scalar("SELECT data FROM arena_runs WHERE account=? AND status='battle' AND json_extract(data,'$.OnlineMatchUid') IS NOT NULL")
        .bind(a).fetch_optional(&mut *db).await?;
    if let Some(raw)=raw {
        let data:Value=parse(&raw)?;let uid=n(&data,"OnlineMatchUid");
        // Once combat has started, leaving is a forfeit. It cannot refund a
        // losing match or erase the other participant's win.
        if (data["OnlineCharged"]==true || n(&data,"Kind")==1) && n(&data,"Season")==season(s).0 {
            if data["OnlineCharged"]!=true {charge(db,s,uid).await?;}
            finish(db,s,uid,n(&data,"Opponent"),&json!({"Creatures":[],"TimeMs":(now()-n(&data,"Started"))*1000}),Some(a)).await?;
        } else {cancel(db,s,uid).await?;}
        return Ok(true);
    }
    Ok(false)
}

pub(crate) async fn cancel(db:&mut SqliteConnection,s:&AppState,uid:i64)->Result<()> {
    let rows:Vec<(i64,i64,String)>=sqlx::query_as("SELECT id,account,data FROM arena_runs WHERE status='battle' AND json_extract(data,'$.OnlineMatchUid')=?")
        .bind(uid).fetch_all(&mut *db).await?;
    for (id,a,raw) in rows {
        let mut data:Value=parse(&raw)?;
        let kind=n(&data,"Kind");
        if data["OnlineCharged"]==true {match_tickets(db,s,a,kind,1).await?;data["OnlineCharged"]=json!(false);}
        let info=battle_info_kind(db,s,a,kind).await?;
        data["End"]=json!({"BaseResult":"Success","Result":"Success",
            "MatchResult":{"MatchUid":uid,"ArenaType":if kind==1{"BanPick"}else{"Normal"},"Canceled":true,"Win":false,"GiveUp":false,
            "GainedMatchScore":0,"NewMatchScore":info["MatchScore"],"PrevTierIndex":info["TierIndex"],"NewTierIndex":info["TierIndex"],
            "PrevTierRank":info["Rank"],"NewTierRank":info["Rank"],"PrevTotalRank":info["Rank"],"NewTotalRank":info["Rank"],
            "SwordResult":match_tickets(db,s,a,kind,0).await?,"CurrencyResults":[],"ItemResults":[],"HeroExpResults":[]}});
        data["End"][if kind==1{"BattleBanPickInfo"}else{"BattleInfo"}]=info;
        sqlx::query("UPDATE arena_runs SET status='complete',data=? WHERE id=?")
            .bind(data.to_string()).bind(id).execute(&mut *db).await?;
    }
    Ok(())
}

pub(crate) async fn recover(db:&mut SqliteConnection,s:&AppState)->Result<()> {
    let ids:Vec<i64>=sqlx::query_scalar("SELECT DISTINCT json_extract(data,'$.OnlineMatchUid') FROM arena_runs WHERE status='battle' AND json_extract(data,'$.OnlineMatchUid') IS NOT NULL")
        .fetch_all(&mut *db).await?;
    for uid in ids {cancel(db,s,uid).await?;}
    sqlx::query("UPDATE arena_runs SET status='canceled' WHERE status='waiting'").execute(&mut *db).await?;
    Ok(())
}
