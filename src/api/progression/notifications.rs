//! Common native response fields refresh mission UI after successful gameplay mutations.
use super::*;
use axum::{
    body::{to_bytes, Body},
    extract::Request as HttpRequest,
    middleware::Next,
    response::Response,
};

/// The native QuestManager queues every completed record it receives, even when
/// unchanged. Login initializes its full state; later responses must be deltas.
/// Keep the baseline per session so clients/accounts never consume each other's updates.
pub(crate) fn subquest_updates(state: &AppState, session_key: &str, infos: &Value) -> Vec<Value> {
    let Some(session) = state.sessions.get(session_key) else { return Vec::new(); };
    let mut delivered = session.delivered_sub_quests.lock().unwrap_or_else(|e| e.into_inner());
    let mut changed = Vec::new();
    for info in infos.as_array().into_iter().flatten() {
        let id = n(info, "SubQuestIndex");
        if delivered.get(&id) != Some(info) {
            changed.push(info.clone());
            delivered.insert(id, info.clone());
        }
    }
    changed
}

pub async fn notify(State(state): State<AppState>, request: HttpRequest, next: Next) -> Response {
    let path = request.uri().path();
    // Confirmed reads and cosmetic edits do not advance gameplay progress.
    // Reward claims and unknown mutations still refresh missions.
    if crate::api::system::performance::observation(path) || crate::api::system::performance::cosmetic(path) {
        return next.run(request).await;
    }
    let family = path.split('/').nth(1).unwrap_or("");
    if !matches!(
        family,
        "hero"
            | "monthly_hero"
            | "hero_storage_slot"
            | "item"
            | "equip"
            | "campaign"
            | "sweep"
            | "shop"
            | "friend"
            | "mail"
            | "tutorial"
            | "hero_inn"
            | "guild"
            | "npc"
            | "valance"
            | "equip_storage_slot"
            | "raid" | "tower" | "eclipse" | "ordeal_arena" | "arena" | "match"
            | "equip_gacha" | "soul_weapon" | "party_dungeon"
    ) {
        return next.run(request).await;
    }
    let acknowledged = request.headers().get("x-sprk-progression")
        .and_then(|v|v.to_str().ok()).unwrap_or("").to_owned();
    let headers = request.headers().clone();
    let (parts, body) = request.into_parts();
    let Ok(bytes) = to_bytes(body, 2 * 1024 * 1024).await else {
        return Response::builder().status(413).body(Body::empty()).unwrap();
    };
    let identity = Request::parse(&bytes).ok().and_then(|r| {
        let account = r.account(&state).ok()?;
        let key = r.0.get("SessionKey").or(r.0.get("SessionId"))?.clone();
        Some((account, key))
    });
    let response = next
        .run(HttpRequest::from_parts(parts, Body::from(bytes)))
        .await;
    let Some((account, session_key)) = identity else {
        return response;
    };
    if !response.status().is_success() {
        return response;
    }
    let (mut parts, body) = response.into_parts();
    let bytes = match to_bytes(body, 16 * 1024 * 1024).await {
        Ok(v) => v,
        Err(_) => return Response::builder().status(500).body(Body::empty()).unwrap(),
    };
    let mut value: Value = match serde_json::from_slice(&bytes) {
        Ok(v) => v,
        Err(_) => return Response::from_parts(parts, Body::from(bytes)),
    };
    if value["BaseResult"] != "Success"
        || value["Result"] != "Success"
        || value.get("Message").is_some()
        // Explicit endpoint snapshots can be required by their own callback
        // (e.g. achievement/list replaces its manager). Only suppress common
        // middleware fields and the verified lobby base-response fields.
        || value.get("AchievementInfos").is_some()
    {
        return Response::from_parts(parts, Body::from(bytes));
    }
    let updates = super::cache::get(&state, &session_key, account).await;
    match updates {
        Ok(updates) => {
            // Section clients must not use the combined token: a lobby refresh
            // can replace just one section independently of that combined state.
            if headers.get("x-sprk-sections").is_some_and(|v| v == "1") {
                for (wire, field) in SNAPSHOT_FIELDS { value[wire] = updates[field].clone(); }
                conditional_sections(&mut value, &session_key, &headers);
            } else {
                append_snapshots(&mut value, &updates, &session_key, &acknowledged);
            }
            value["ReservedSubQuestInfos"] = json!(subquest_updates(&state, &session_key, &updates["SubQuestInfos"]));
            match serde_json::to_vec(&value) {
                Ok(bytes) => {
                    parts.headers.remove(axum::http::header::CONTENT_LENGTH);
                    Response::from_parts(parts, Body::from(bytes))
                }
                Err(_) => Response::from_parts(parts, Body::from(bytes)),
            }
        }
        Err(e) => {
            tracing::warn!(%e,"Could not append progression notification");
            Response::from_parts(parts, Body::from(bytes))
        }
    }
}

/// A client acknowledges only a snapshot it has received. Lost responses and
/// new sessions therefore cause a full resend. Achievements replace native state,
/// so never send a partial AchievementInfos array.
const SNAPSHOT_FIELDS: [(&str, &str); 4] = [
    ("AchievementInfos", "AchievementInfos"),
    ("ReservedMainQuestInfo", "MainQuestInfo"),
    ("ReservedClearMissionInfos", "ClearMissionInfos"),
    ("ReservedPlayRecordInfos", "PlayRecordInfos"),
];

fn normalized(mut value: Value, play_records: bool) -> Value {
    if play_records {
        for record in value.as_array_mut().into_iter().flatten() {
            if let Some(record) = record.as_object_mut() { record.remove("UpdatedTime"); }
        }
    }
    value
}

/// Omission preserves native state; explicit empty lists retain native semantics.
/// Tokens are acknowledged after OnBaseResponse has applied the exact reply.
pub(crate) fn conditional_sections(value: &mut Value, session: &str, headers: &axum::http::HeaderMap) {
    use sha2::{Digest, Sha256};
    if !headers.get("x-sprk-sections").is_some_and(|v| v == "1") { return; }
    for (wire, name, header) in [
        ("AchievementInfos", "SprkAchievementToken", "x-sprk-achievements"),
        ("ReservedMainQuestInfo", "SprkMainQuestToken", "x-sprk-main-quest"),
        ("ReservedClearMissionInfos", "SprkClearMissionToken", "x-sprk-clear-missions"),
        ("ReservedPlayRecordInfos", "SprkPlayRecordToken", "x-sprk-play-records"),
    ] {
        let Some(snapshot) = value.get(wire) else { continue; };
        let mut digest = Sha256::new();
        digest.update(session.as_bytes());
        digest.update(wire.as_bytes());
        digest.update(serde_json::to_vec(&normalized(snapshot.clone(), wire == "ReservedPlayRecordInfos")).unwrap());
        let token = hex::encode(digest.finalize());
        if headers.get(header).and_then(|h| h.to_str().ok()) == Some(&token) {
            value.as_object_mut().unwrap().remove(wire);
        } else {
            value[name] = json!(token);
        }
    }
}

fn append_snapshots(value: &mut Value, updates: &Value, session: &str, acknowledged: &str) {
    use sha2::{Digest, Sha256};
    let mut digest = Sha256::new();
    digest.update(session.as_bytes());
    for (_, field) in SNAPSHOT_FIELDS {
        let snapshot = normalized(updates[field].clone(), field == "PlayRecordInfos");
        digest.update(serde_json::to_vec(&snapshot).unwrap());
    }
    let token = hex::encode(digest.finalize());
    if token != acknowledged {
        for (wire, field) in SNAPSHOT_FIELDS {
            value[wire] = updates[field].clone();
        }
    }
    value["SprkProgressionToken"] = json!(token);
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn section_acknowledgements_isolate_changes_and_recover_missing_empty_and_foreign_state() {
        let original = json!({"AchievementInfos":[{"Index":1}],"ReservedMainQuestInfo":{"Step":1},"ReservedClearMissionInfos":[],"ReservedPlayRecordInfos":[{"RecordIndex":1,"RecordValue":"1","UpdatedTime":"first"}]});
        let mut headers=axum::http::HeaderMap::new();headers.insert("x-sprk-sections","1".parse().unwrap());
        let mut first=original.clone();conditional_sections(&mut first,"session",&headers);
        for (token,header) in [("SprkAchievementToken","x-sprk-achievements"),("SprkMainQuestToken","x-sprk-main-quest"),("SprkClearMissionToken","x-sprk-clear-missions"),("SprkPlayRecordToken","x-sprk-play-records")] {
            headers.insert(header,first[token].as_str().unwrap().parse().unwrap());
        }
        let mut repeated=original.clone();repeated["ReservedPlayRecordInfos"][0]["UpdatedTime"]=json!("later");
        conditional_sections(&mut repeated,"session",&headers);
        for (wire,_) in SNAPSHOT_FIELDS {assert!(repeated.get(wire).is_none(),"{wire}");}
        let mut changed=original.clone();changed["ReservedMainQuestInfo"]["Step"]=json!(2);
        conditional_sections(&mut changed,"session",&headers);
        assert_eq!(changed["ReservedMainQuestInfo"]["Step"],2);assert!(changed.get("AchievementInfos").is_none());
        let mut empty=original.clone();empty["AchievementInfos"]=json!([]);conditional_sections(&mut empty,"session",&headers);
        assert_eq!(empty["AchievementInfos"],json!([]));
        let mut foreign=original.clone();conditional_sections(&mut foreign,"other-session",&headers);
        assert!(foreign["AchievementInfos"].is_array());
        headers.remove("x-sprk-achievements");let mut lost=original.clone();conditional_sections(&mut lost,"session",&headers);
        assert_eq!(lost["AchievementInfos"],original["AchievementInfos"]);
        let mut legacy=original.clone();conditional_sections(&mut legacy,"session",&axum::http::HeaderMap::new());assert_eq!(legacy,original);
        println!("Section fixture bytes: full={}, acknowledged={}",serde_json::to_vec(&first).unwrap().len(),serde_json::to_vec(&repeated).unwrap().len());
    }
    #[test]
    fn acknowledged_snapshots_resend_changes_losses_and_session_changes() {
        let mut updates=json!({"AchievementInfos":[{"AchievementIndex":1,"Achievement":2},{"AchievementIndex":2,"Achievement":3}],
            "MainQuestInfo":{"Step":1},"ClearMissionInfos":[],
            "PlayRecordInfos":[{"RecordIndex":1,"RecordValue":"2","UpdatedTime":"2026-10-26 12:00:00"}]});
        let mut first=json!({});append_snapshots(&mut first,&updates,"session","");
        let token=first["SprkProgressionToken"].as_str().unwrap();
        let mut repeated=json!({});append_snapshots(&mut repeated,&updates,"session",token);
        assert!(repeated.get("AchievementInfos").is_none());
        assert!(serde_json::to_vec(&repeated).unwrap().len()<serde_json::to_vec(&first).unwrap().len());
        let mut lost=json!({});append_snapshots(&mut lost,&updates,"session","");assert_eq!(lost,first);
        let mut new_session=json!({});append_snapshots(&mut new_session,&updates,"other",token);
        assert_eq!(new_session["AchievementInfos"],first["AchievementInfos"]);
        updates["PlayRecordInfos"][0]["UpdatedTime"]=json!("2026-10-26 12:00:01");
        let mut later=json!({});append_snapshots(&mut later,&updates,"session",token);
        assert!(later.get("AchievementInfos").is_none());
        assert_eq!(later["SprkProgressionToken"],token);
        let mut later_lost=json!({});append_snapshots(&mut later_lost,&updates,"session","");
        assert_eq!(later_lost["ReservedPlayRecordInfos"],updates["PlayRecordInfos"]);
        updates["PlayRecordInfos"][0]["RecordValue"]=json!("3");
        let mut progress=json!({});append_snapshots(&mut progress,&updates,"session",token);
        assert_ne!(progress["SprkProgressionToken"],token);
        assert_eq!(progress["ReservedPlayRecordInfos"],updates["PlayRecordInfos"]);
        updates["PlayRecordInfos"][0]["RecordValue"]=json!("2");
        updates["AchievementInfos"][0]["Achievement"]=json!(4);
        let mut changed=json!({});append_snapshots(&mut changed,&updates,"session",token);
        assert_eq!(changed["AchievementInfos"].as_array().unwrap().len(),2);
        assert_ne!(changed["SprkProgressionToken"],token);
    }
}
