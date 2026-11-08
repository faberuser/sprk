//! Party Retry votes for cooperative PvE raids and story party dungeons.
//!
//! Any player still in the battle may propose a Retry over the battle socket.
//! Every other player still in the battle must accept within the vote window.
//! Players who exited to the party room are not asked and stay in the room
//! while the others restart; the room master must still be playing. A single No,
//! a timeout, a departure or the battle ending cancels the vote for everyone.
//! Once accepted, the current run is released as a defeat (victory-only entry
//! holds are returned) and the room master's original start is replayed in
//! the same transaction, so a failed restart leaves the current battle intact.
//! Competitive runs (Guild Conquest, arena) never reach this module.
use super::*;

pub(super) const PREFIX: &str = "sprk-retry:";
pub(super) const VOTE_SECONDS: u64 = 15;
const MESSAGE_TYPE: &str = "SprkRetry";

/// Handle a `sprk-retry:` battle message. Returns whether it was accepted.
pub(super) async fn message(s: &AppState, run: &mut Run, a: i64, text: &str) -> Result<bool> {
    let command = text.strip_prefix(PREFIX).ok_or_else(|| rule("InvalidValue"))?;
    if !eligible(run) || !run.members.contains(&a) || run.cancelled.contains(&a) || run.departed.contains(&a) {
        send_to(s, run, a, "Cancel|0|Unavailable|".into());
        return Ok(false);
    }
    let (verb, vote) = command.split_once(':').unwrap_or((command, "0"));
    let vote: i64 = vote.parse().map_err(|_| rule("InvalidValue"))?;
    match verb {
        "propose" => propose(s, run, a).await,
        "yes" => accept(s, run, a, vote).await,
        "no" => {
            if current(run) != Some(vote) || vote == 0 { return Ok(false); }
            cancel_vote(s, run, "Declined", &nick(run, a));
            Ok(true)
        }
        _ => Err(rule("InvalidValue")),
    }
}

fn eligible(run: &Run) -> bool {
    run.payload["PartyCampaign"] == true && run.payload["ArenaMatch"] != true
        && run.result.is_none() && run.started && run.room > 0
}

fn current(run: &Run) -> Option<i64> {
    run.payload["RetryVote"]["Id"].as_i64()
}

pub(super) fn nick(run: &Run, a: i64) -> String {
    run.payload["Simulation"]["AccountInfos"].as_array().into_iter().flatten()
        .find(|info| number(&info["UserInfo"], "AccountId") == a)
        .and_then(|info| info["UserInfo"]["Nick"].as_str())
        .unwrap_or("A player").replace(['|', '\n', '\r'], " ")
}

/// Players who must agree: participants still in the room and the battle.
/// Players who exited to the party room are neither asked nor restarted.
async fn electorate(s: &AppState, run: &Run) -> Result<Vec<i64>> {
    let room: Vec<i64> = sqlx::query_scalar("SELECT account FROM battle_room_members WHERE room=? ORDER BY joined,account")
        .bind(run.room).fetch_all(&s.db).await?;
    Ok(room.into_iter().filter(|a| run.members.contains(a) && !run.cancelled.contains(a) && !run.departed.contains(a)).collect())
}

fn packet(run: &Run, content: String) -> Value {
    json!({"SenderId":0,"GroupType":"None","ReceiverIds":run.members,"Type":MESSAGE_TYPE,"Content":content})
}

fn send_all(s: &AppState, run: &Run, content: String) {
    s.conquest.notify(run, "BattleMessageNot", packet(run, content));
}

fn send_to(s: &AppState, run: &Run, a: i64, content: String) {
    if let Some(peer) = s.conquest.peers.get(&a) {
        if peer.run == run.id { s.conquest.deliver(&peer, "BattleMessageNot", packet(run, content)); }
    }
}

async fn propose(s: &AppState, run: &mut Run, a: i64) -> Result<bool> {
    if current(run).is_some() {
        send_to(s, run, a, "Cancel|0|Busy|".into());
        return Ok(false);
    }
    let voters = electorate(s, run).await?;
    // The restart replays the room master's start, so the master must still
    // be in the battle (a master who exited is waiting in the party room).
    if !voters.contains(&a) || !voters.contains(&run.master) || run.payload["MasterRequest"].is_null() {
        send_to(s, run, a, "Cancel|0|Unavailable|".into());
        return Ok(false);
    }
    let id = number(&run.payload, "RetryVoteSeq") + 1;
    run.payload["RetryVoteSeq"] = json!(id);
    run.payload["RetryVote"] = json!({"Id":id,"Proposer":a,"Yes":[a]});
    if voters.len() == 1 { return finish(s, run).await; }
    send_all(s, run, format!("Vote|{id}|{a}|{VOTE_SECONDS}|{}", nick(run, a)));
    let (s, run_id) = (s.clone(), run.id.clone());
    crate::database::spawn_labelled("task:battle-retry-vote", async move {
        tokio::time::sleep(std::time::Duration::from_secs(VOTE_SECONDS)).await;
        if let Err(error) = expire(&s, &run_id, id).await {
            tracing::warn!(%error, "Party retry vote expiry failed");
        }
    });
    Ok(true)
}

async fn accept(s: &AppState, run: &mut Run, a: i64, vote: i64) -> Result<bool> {
    if current(run) != Some(vote) || vote == 0 { return Ok(false); }
    let mut yes: BTreeSet<i64> = read_value(run.payload["RetryVote"]["Yes"].clone())?;
    yes.insert(a);
    run.payload["RetryVote"]["Yes"] = json!(yes);
    let voters = electorate(s, run).await?;
    if voters.iter().all(|v| yes.contains(v)) { return finish(s, run).await; }
    Ok(true)
}

pub(super) async fn expire(s: &AppState, id: &str, vote: i64) -> Result<()> {
    let Some(handle) = s.conquest.runs.get(id).map(|v| v.value().clone()) else { return Ok(()); };
    let mut run = handle.lock().await;
    if current(&run) != Some(vote) { return Ok(()); }
    let yes: BTreeSet<i64> = read_value(run.payload["RetryVote"]["Yes"].clone())?;
    let waiting: Vec<String> = electorate(s, &run).await?.into_iter()
        .filter(|a| !yes.contains(a)).map(|a| nick(&run, a)).collect();
    cancel_vote(s, &mut run, "Timeout", &waiting.join(", "));
    Ok(())
}

/// Cancel an open vote, if any, and tell every participant why.
pub(super) fn cancel_vote(s: &AppState, run: &mut Run, reason: &str, who: &str) {
    let Some(id) = current(run) else { return; };
    run.payload["RetryVote"] = Value::Null;
    send_all(s, run, format!("Cancel|{id}|{reason}|{who}"));
}

async fn finish(s: &AppState, run: &mut Run) -> Result<bool> {
    let id = current(run).unwrap_or(0);
    let voters: BTreeSet<i64> = electorate(s, run).await?.into_iter().collect();
    match restart(s, run, &voters).await {
        Ok(start) => {
            run.payload["RetryVote"] = Value::Null;
            send_all(s, run, format!("Restart|{id}|{start}"));
            let _ = s.conquest.send_worker("WorkerCancel", json!({"RunId":run.id})).await;
            // Keep the old run unreachable: its result can no longer settle.
            run.result = Some(json!({"Retried":true}));
            s.conquest.runs.remove(&run.id);
            Ok(true)
        }
        Err(error) => {
            tracing::info!(%error, run=%run.id, "Party retry restart rejected");
            let reason = match &error { ServerError::InvalidRequest(code) => code.replace(['|', '\n', '\r'], " "), _ => "Failed".into() };
            cancel_vote(s, run, "Failed", &reason);
            Ok(false)
        }
    }
}

/// Release the current run and start the same room again atomically.
/// Returns the native `PartyBattleStart` body every client follows.
async fn restart(s: &AppState, run: &Run, voters: &BTreeSet<i64>) -> Result<Value> {
    if !voters.contains(&run.master) { return Err(rule("WrongMember")); }
    let mut db = s.db.begin().await?;
    let raw: Option<String> = sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=?")
        .bind(run.room).fetch_optional(&mut *db).await?;
    let mut room: Value = read_json(&raw.ok_or_else(|| rule("RoomNotExist"))?)?;
    if room["CoopRunId"] != json!(run.id) || n(&room, "MasterAccountId") != run.master {
        return Err(rule("WrongMember"));
    }
    for a in &run.members {
        let raw: Option<String> = sqlx::query_scalar("SELECT entry FROM battle_runs WHERE account=? AND completed=0 AND json_extract(entry,'$.CoopRunId')=?")
            .bind(a).bind(&run.id).fetch_optional(&mut *db).await?;
        let Some(raw) = raw else { continue; };
        let mut entry: Value = read_json(&raw)?;
        // A retry is a defeat for cost purposes: victory-only holds are released.
        super::super::entry_costs::settle(&mut db, s, *a, &mut entry, false, &mut json!({})).await?;
        sqlx::query("UPDATE battle_runs SET completed=1,entry=json_set(entry,'$.CoopCancelled',json('true'),'$.CoopRetried',json('true')) WHERE account=?")
            .bind(a).execute(&mut *db).await?;
    }
    room["Status"] = json!(0);
    room.as_object_mut().unwrap().remove("CoopRunId");
    sqlx::query("UPDATE battle_rooms SET data=?,updated=? WHERE id=?")
        .bind(room.to_string()).bind(now()).bind(run.room).execute(&mut *db).await?;
    for member in voters.iter().filter(|a| **a != run.master) {
        // Every remaining player voted Yes, which is their readiness.
        let mut state = get(&mut db, *member, "party_member", run.room).await?;
        state["IsBattleReady"] = json!(true);
        put(&mut db, *member, "party_member", run.room, &state).await?;
    }
    let mut fields: std::collections::HashMap<String, String> = read_value(run.payload["MasterRequest"].clone())?;
    fields.insert("MultiplayMemberIds".into(), json!(voters).to_string());
    let request = Request(fields);
    super::party::begin_party_for(&mut db, s, run.master, &request, Some(voters)).await?;
    let raw: String = sqlx::query_scalar("SELECT data FROM battle_rooms WHERE id=?")
        .bind(run.room).fetch_one(&mut *db).await?;
    let mut room: Value = read_json(&raw)?;
    room["RoomNo"] = json!(run.room);
    let next = room["CoopRunId"].as_str().map(str::to_owned);
    let start = match super::party_start(&mut db, s, run.master, &room).await {
        Ok(start) => start,
        Err(error) => {
            if let Some(next) = next { s.conquest.runs.remove(&next); }
            return Err(error);
        }
    };
    if let Err(error) = db.commit().await {
        if let Some(next) = next { s.conquest.runs.remove(&next); }
        return Err(error.into());
    }
    Ok(start)
}
