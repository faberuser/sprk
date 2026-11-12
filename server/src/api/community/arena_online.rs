//! Durable Victory/Honor queues and atomic, service-owned settlement.
use super::*;
use crate::api::battle::cooperative;

// The original Unity marshalers encode creature integers as JSON strings.
fn native_number(v:&Value,key:&str)->i64 {
    v[key].as_i64().or_else(||v[key].as_str().and_then(|v|v.parse().ok())).unwrap_or(0)
}

pub(crate) async fn pair(
    db: &mut PgConnection, s: &AppState, a: i64, id: i64, mine: &Value,
) -> Result<Option<Value>> {
    let Some(worker) = cooperative::arena_worker(s).await else { return Ok(None); };
    let kind=n(mine,"Kind");
    let friendly=n(mine,"DuelType")==1;
    let cutoff = now() - settings(s, "ArenaQueueStaleSeconds", 15).clamp(6, 120);
    let candidates: Vec<(i64, i64, String)> = crate::db::query_as(
        "SELECT id,account,data FROM arena_runs WHERE kind=$1 AND status='waiting' AND account!=$2 AND started>=$3 AND COALESCE(last_poll,started)>=$4 AND season=$5 ORDER BY started,id LIMIT 100")
        .bind(kind).bind(a).bind(now()-settings(s,"ArenaMatchExpirySeconds",900)).bind(cutoff)
        .bind(season(s).0).fetch_all(&mut *db).await?;
    if !friendly && n(&match_tickets(db,s,a,kind,0).await?,"NewValue") < 1 {
        return Err(rule("NotEnoughSword"));
    }
    let mut mine_validated=false;
    for (other_id, b, _) in candidates {
        // Being logged in alone is insufficient: candidates must also have a
        // recently polled queue entry. Cancelled/bot/in-battle rows never qualify.
        if !s.sessions.iter().any(|v|v.account_id==b) { continue; }
        // Arena requests and settlements hold the arena queue lock, so the
        // candidate's entry and swords cannot change under us; re-read the
        // entry in case an earlier pairing in this scan's snapshot took it.
        let Some(raw): Option<String> = crate::db::query_scalar("SELECT data FROM arena_runs WHERE id=$1 AND account=$2 AND status='waiting'")
            .bind(other_id).bind(b).fetch_optional(&mut *db).await? else { continue; };
        let other: Value = parse(&raw)?;
        if friendly != (n(&other,"DuelType")==1) {continue;}
        if friendly && (n(mine,"GroupId")!=n(&other,"GroupId") || n(mine,"InvitedOpponent")!=b || n(&other,"InvitedOpponent")!=a) {continue;}
        if n(&other,"Expires") < now() || (!friendly && n(&match_tickets(db,s,b,kind,0).await?,"NewValue") < 1) { continue; }
        // Validate a collection only when an actual opponent is available, not
        // on every queue poll (Honor can register up to 200 Heroes).
        if !mine_validated {
            if !eligible(db,a,mine).await? {
                cancel_waiter(db,a,id,mine).await?;
                return Ok(Some(canceled_response(db,s,a,kind).await?));
            }
            mine_validated=true;
        }
        if !eligible(db,b,&other).await? {
            cancel_waiter(db,b,other_id,&other).await?;
            if friendly {return Ok(Some(canceled_response(db,s,a,kind).await?));}
            continue;
        }
        let uid = id.min(other_id);
        let mut accounts = vec![super::registered_account(db,id,mine).await?,super::registered_account(db,other_id,&other).await?];
        if id > other_id { accounts.swap(0,1); }
        accounts[0]["TeamType"] = json!("Left");
        accounts[1]["TeamType"] = json!("Right");
        let info = json!({"Uid":uid,"ArenaType":if kind==1{"BanPick"}else{"Normal"},"SeasonIndex":season(s).0,
            "AccountInfos":accounts,"BattleServerAddress":s.conquest.address,"BattleServerPort":s.conquest.port,
            "ChapterIndex":1000,"DungeonIndex":if kind==1{2}else{1},"DungeonDifficulty":"Easy","OnlineGameSpeedRatio":100,"ExtraData":null,"CanCheat":false});
        let mut response = Value::Null;
        for (row_id, account_id, saved, enemy) in [(id,a,mine,b),(other_id,b,&other,a)] {
            let mut data = saved.clone();
            let wait = json!({"BaseResult":"Success","Result":"Success","MatchInfo":info,
                "MatchedNpcInfo":null,"RelayBattleServerAddress":"","RelayBattleServerPort":0,
                "SwordResult":match_tickets(db,s,account_id,kind,0).await?,"BattleInfo":battle_info_kind(db,s,account_id,kind).await?});
            // Honor's MatchInfo repeats both ~600 KB registrations; store it
            // without them (re-polls rebuild them from arena_snapshots).
            data["Wait"] = wait.clone();
            if kind==1 { data["Wait"]["MatchInfo"]["AccountInfos"] = json!([]); }
            data["Opponent"] = json!(enemy);
            data["OnlineMatchUid"] = json!(uid);
            data["ServiceOwned"] = json!(true);
            data["OnlineCharged"] = json!(false);
            data["Started"] = json!(now());
            let updated = crate::db::query("UPDATE arena_runs SET status='battle',data=$1 WHERE id=$2 AND status='waiting'")
                .bind(data.to_string()).bind(row_id).execute(&mut *db).await?.rows_affected();
            // Both queue entries are locked above; a changed one aborts the pairing.
            if updated != 1 { return Err(rule("MatchingAborted")); }
            if account_id==a {response=wait;}
        }
        cooperative::create_arena(s, worker, info);
        return Ok(Some(response));
    }
    Ok(None)
}

async fn eligible(db:&mut PgConnection,a:i64,data:&Value)->Result<bool> {
    let heroes:Vec<i64>=data["Heroes"].as_array().into_iter().flatten().filter_map(Value::as_i64).collect();
    match owned(db,a,&heroes).await {
        Ok(())=>Ok(true),
        Err(ServerError::InvalidRequest(_))=>Ok(false),
        Err(e)=>Err(e),
    }
}

async fn cancel_waiter(db:&mut PgConnection,a:i64,id:i64,data:&Value)->Result<()> {
    if n(data,"DuelType")==1 {
        super::super::duel::cancel_waiting(db,a,n(data,"InvitedOpponent"),n(data,"GroupId")).await
    } else {
        crate::db::query("UPDATE arena_runs SET status='canceled',data=sprk_json_set(data,'$.QueueCancelReason','HeroUnavailable') WHERE id=$1 AND account=$2 AND status='waiting'")
            .bind(id).bind(a).execute(db).await?;
        Ok(())
    }
}

async fn canceled_response(db:&mut PgConnection,s:&AppState,a:i64,kind:i64)->Result<Value> {
    Ok(json!({"BaseResult":"Success","Result":"Canceled","SwordResult":match_tickets(db,s,a,kind,0).await?}))
}

pub(crate) async fn charge(db:&mut PgConnection,s:&AppState,uid:i64)->Result<()> {
    let rows:Vec<(i64,String)>=crate::db::query_as("SELECT account,data FROM arena_runs WHERE status='battle' AND sprk_json_int(data,'$.OnlineMatchUid')=$1 ORDER BY id")
        .bind(uid).fetch_all(&mut *db).await?;
    if rows.len()!=2 {return Err(rule("MatchingAborted"));}
    for (a,raw) in rows {
        let data:Value=parse(&raw)?;
        if n(&data,"Season")!=season(s).0 {return Err(rule("MatchingAborted"));}
        if data["OnlineCharged"]!=true {
            if n(&data,"DuelType")!=1 {match_tickets(db,s,a,n(&data,"Kind"),-1).await?;}
            crate::db::query("UPDATE arena_runs SET data=sprk_json_set(data,'$.OnlineCharged',('true')::jsonb) WHERE +account=$1 AND status='battle' AND sprk_json_int(data,'$.OnlineMatchUid')=$2")
                .bind(a).bind(uid).execute(&mut *db).await?;
        }
    }
    Ok(())
}

pub(crate) async fn finish(db:&mut PgConnection,s:&AppState,uid:i64,winner:i64,body:&Value,give_up:Option<i64>)->Result<()> {
    let rows:Vec<(i64,String)>=crate::db::query_as("SELECT account,data FROM arena_runs WHERE status='battle' AND sprk_json_int(data,'$.OnlineMatchUid')=$1 ORDER BY id")
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
            ("PlayTime".into(),(native_number(body,"TimeMs")/1000).max(0).to_string()),("AliveHeroIndices".into(),json!(alive).to_string()),
            ("GiveUp".into(),if give_up==Some(a){"1"}else{"0"}.into())].into());
        // Settlement writes MatchUid and GiveUp into End with the result
        // itself: patching them afterwards rewrote each ~1.5 MB Honor row again.
        service_result(db,s,a,&req).await?;
        if n(&data,"DuelType")==1 {
            crate::db::query("UPDATE friendly_duels SET status='complete' WHERE group_id=$1")
                .bind(n(&data,"GroupId")).execute(&mut *db).await?;
        }
    }
    Ok(())
}

pub(crate) async fn cancel_account(db:&mut PgConnection,s:&AppState,a:i64)->Result<bool> {
    let raw:Option<String>=crate::db::query_scalar("SELECT data FROM arena_runs WHERE account=$1 AND status='battle' AND sprk_json_int(data,'$.OnlineMatchUid') IS NOT NULL")
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

pub(crate) async fn cancel(db:&mut PgConnection,s:&AppState,uid:i64)->Result<()> {
    let rows:Vec<(i64,i64,String)>=crate::db::query_as("SELECT id,account,data FROM arena_runs WHERE status='battle' AND sprk_json_int(data,'$.OnlineMatchUid')=$1")
        .bind(uid).fetch_all(&mut *db).await?;
    for (id,a,raw) in rows {
        let mut data:Value=parse(&raw)?;
        let kind=n(&data,"Kind");
        if data["OnlineCharged"]==true {if n(&data,"DuelType")!=1 {match_tickets(db,s,a,kind,1).await?;}data["OnlineCharged"]=json!(false);}
        let info=battle_info_kind(db,s,a,kind).await?;
        data["End"]=json!({"BaseResult":"Success","Result":"Success",
            "MatchResult":{"MatchUid":uid,"ArenaType":if kind==1{"BanPick"}else{"Normal"},"Canceled":true,"Win":false,"GiveUp":false,
            "GainedMatchScore":0,"NewMatchScore":info["MatchScore"],"PrevTierIndex":info["TierIndex"],"NewTierIndex":info["TierIndex"],
            "PrevTierRank":info["Rank"],"NewTierRank":info["Rank"],"PrevTotalRank":info["Rank"],"NewTotalRank":info["Rank"],
            "SwordResult":match_tickets(db,s,a,kind,0).await?,"CurrencyResults":[],"ItemResults":[],"HeroExpResults":[]}});
        data["End"][if kind==1{"BattleBanPickInfo"}else{"BattleInfo"}]=info;
        crate::db::query("UPDATE arena_runs SET status='complete',data=$1 WHERE id=$2")
            .bind(data.to_string()).bind(id).execute(&mut *db).await?;
        if n(&data,"DuelType")==1 {
            crate::db::query("UPDATE friendly_duels SET status='canceled' WHERE group_id=$1")
                .bind(n(&data,"GroupId")).execute(&mut *db).await?;
        }
    }
    Ok(())
}

pub(crate) async fn recover(db:&mut PgConnection,s:&AppState)->Result<()> {
    let ids:Vec<i64>=crate::db::query_scalar("SELECT DISTINCT sprk_json_int(data,'$.OnlineMatchUid') FROM arena_runs WHERE status='battle' AND sprk_json_int(data,'$.OnlineMatchUid') IS NOT NULL")
        .fetch_all(&mut *db).await?;
    for uid in ids {cancel(db,s,uid).await?;}
    crate::db::query("UPDATE arena_runs SET status='canceled' WHERE status='waiting'").execute(&mut *db).await?;
    Ok(())
}
