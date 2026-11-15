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
                let info = state.get_session(&session_key);
                conditional_sections_with_deltas(&mut value, &session_key, &headers,
                    info.as_ref().map(|s| &*s.login_sections), info.as_ref().map(|s| &*s.delivered_achievements));
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
/// new sessions therefore cause a full resend. The native client merges
/// AchievementInfos by index; only a client that asks for it
/// (`x-sprk-achievement-delta`) is sent just the changed entries.
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
#[cfg(test)]
pub(crate) fn conditional_sections(value: &mut Value, session: &str, headers: &axum::http::HeaderMap) {
    conditional_sections_after_login(value, session, headers, None)
}

pub(crate) fn section_token(session: &str, wire: &str, snapshot: &Value) -> String {
    use sha2::{Digest, Sha256};
    let mut digest = Sha256::new();
    digest.update(session.as_bytes());
    digest.update(wire.as_bytes());
    digest.update(serde_json::to_vec(&normalized(snapshot.clone(), wire == "ReservedPlayRecordInfos")).unwrap());
    hex::encode(digest.finalize())
}

/// Record that the login reply carried this session's full achievement list,
/// so the first follow-up replies need not resend it (~100 KB per login).
pub(crate) fn remember_login_sections(state: &AppState, session: &str, achievements: &Value) {
    if let Some(info) = state.sessions.get(session) {
        let token = section_token(session, "AchievementInfos", achievements);
        remember_achievements(&info.delivered_achievements, &token, achievements);
        info.login_sections.lock().unwrap_or_else(|e| e.into_inner()).insert("AchievementInfos".into(), token);
    }
}

/// Achievement lists recently delivered to one session: (section token, a
/// digest of every entry by AchievementIndex), oldest first.
pub(crate) type DeliveredAchievements = std::collections::VecDeque<(String, std::sync::Arc<std::collections::BTreeMap<i64, [u8; 16]>>)>;
/// A client acknowledges the newest list it applied; a few older ones cover
/// replies that were lost or are still in flight.
const DELIVERED_ACHIEVEMENTS_KEPT: usize = 4;

/// Each entry's digest by AchievementIndex; None when an entry has no index.
fn achievement_digests(snapshot: &Value) -> Option<std::collections::BTreeMap<i64, [u8; 16]>> {
    use sha2::{Digest, Sha256};
    snapshot.as_array()?.iter().map(|entry| {
        let digest = Sha256::digest(serde_json::to_vec(entry).ok()?);
        Some((entry["AchievementIndex"].as_i64()?, digest[..16].try_into().ok()?))
    }).collect()
}

fn remember_achievements(delivered: &std::sync::Mutex<DeliveredAchievements>, token: &str, snapshot: &Value) {
    let Some(digests) = achievement_digests(snapshot) else { return; };
    let mut delivered = delivered.lock().unwrap_or_else(|e| e.into_inner());
    if delivered.iter().any(|(t, _)| t == token) { return; }
    delivered.push_back((token.to_owned(), std::sync::Arc::new(digests)));
    while delivered.len() > DELIVERED_ACHIEVEMENTS_KEPT { delivered.pop_front(); }
}

/// The entries of `snapshot` that differ from the list delivered as `held`,
/// or None (send everything) when that list is unknown or lost an entry.
fn achievement_delta(delivered: &std::sync::Mutex<DeliveredAchievements>, held: &str, snapshot: &Value) -> Option<Vec<Value>> {
    let base = delivered.lock().unwrap_or_else(|e| e.into_inner()).iter().find(|(t, _)| t == held)?.1.clone();
    let current = achievement_digests(snapshot)?;
    if base.keys().any(|index| !current.contains_key(index)) { return None; }
    Some(snapshot.as_array()?.iter()
        .filter(|entry| entry["AchievementIndex"].as_i64().is_some_and(|index| base.get(&index) != current.get(&index)))
        .cloned().collect())
}

/// `login` holds tokens delivered by this session's login reply. A client
/// without its own token for a section is treated as holding that baseline,
/// until the section is acknowledged or sent in full again (a full resend may
/// be lost, so after it only the client's acknowledgement counts).
#[cfg(test)]
pub(crate) fn conditional_sections_after_login(value: &mut Value, session: &str, headers: &axum::http::HeaderMap,
    login: Option<&std::sync::Mutex<std::collections::BTreeMap<String, String>>>) {
    conditional_sections_with_deltas(value, session, headers, login, None)
}

/// `conditional_sections_after_login`; with `delivered` (the session's recent
/// achievement lists) and a client that merges partial lists
/// (`x-sprk-achievement-delta: 1`), a changed achievement list carries only
/// the entries that differ from the list the client holds: the one it
/// acknowledged, or else its login list. The token is always the full list's,
/// so applying the reply leaves the client holding exactly that list.
pub(crate) fn conditional_sections_with_deltas(value: &mut Value, session: &str, headers: &axum::http::HeaderMap,
    login: Option<&std::sync::Mutex<std::collections::BTreeMap<String, String>>>,
    delivered: Option<&std::sync::Mutex<DeliveredAchievements>>) {
    if !headers.get("x-sprk-sections").is_some_and(|v| v == "1") { return; }
    let deltas = delivered.filter(|_| headers.get("x-sprk-achievement-delta").is_some_and(|v| v == "1"));
    for (wire, name, header) in [
        ("AchievementInfos", "SprkAchievementToken", "x-sprk-achievements"),
        ("ReservedMainQuestInfo", "SprkMainQuestToken", "x-sprk-main-quest"),
        ("ReservedClearMissionInfos", "SprkClearMissionToken", "x-sprk-clear-missions"),
        ("ReservedPlayRecordInfos", "SprkPlayRecordToken", "x-sprk-play-records"),
    ] {
        let Some(snapshot) = value.get(wire) else { continue; };
        let token = section_token(session, wire, snapshot);
        let acknowledged = headers.get(header).and_then(|h| h.to_str().ok());
        let mut baseline = login.map(|m| m.lock().unwrap_or_else(|e| e.into_inner()));
        // The list the client holds: acknowledged, or else the login reply's.
        let held = acknowledged.map(str::to_owned).or_else(|| baseline.as_ref().and_then(|b| b.get(wire).cloned()));
        let delivered = baseline.as_ref().and_then(|b| b.get(wire)).is_some_and(|t| *t == token);
        if acknowledged.is_some() { if let Some(b) = baseline.as_mut() { b.remove(wire); } }
        if acknowledged == Some(token.as_str()) {
            value.as_object_mut().unwrap().remove(wire);
        } else if acknowledged.is_none() && delivered {
            // Keep the token so the client acknowledges the login baseline.
            value.as_object_mut().unwrap().remove(wire);
            value[name] = json!(token);
        } else {
            if let Some(b) = baseline.as_mut() { b.remove(wire); }
            if let (Some(delivered), "AchievementInfos") = (deltas, wire) {
                let snapshot = value[wire].clone();
                if let Some(changed) = held.and_then(|held| achievement_delta(delivered, &held, &snapshot)) {
                    value[wire] = json!(changed);
                }
                remember_achievements(delivered, &token, &snapshot);
            }
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
    /// The client merges AchievementInfos by index (`AccountAttrManager.Set`).
    fn merge(held: &Value, reply: &Value) -> Value {
        let mut list: Vec<Value> = held.as_array().unwrap().clone();
        for entry in reply.as_array().unwrap() {
            match list.iter_mut().find(|e| e["AchievementIndex"] == entry["AchievementIndex"]) {
                Some(old) => *old = entry.clone(),
                None => list.push(entry.clone()),
            }
        }
        json!(list)
    }
    /// The client asks for achievement deltas on every request.
    #[test]
    fn the_client_opts_in_to_achievement_deltas() {
        let source = include_str!("../../../../client/managed/SprkNetworkPerformance/SprkNetworkPerformance.cs");
        assert!(source.contains("dictionary[\"X-SPRK-Achievement-Delta\"] = \"1\";"));
    }
    #[test]
    fn opted_in_clients_get_only_changed_achievements_relative_to_the_list_they_hold() {
        let list = |progress: [i64; 3]| json!((1..=3).map(|i| json!({"AchievementIndex":i,"LastStep":0,"Achievement":progress[i as usize - 1]})).collect::<Vec<_>>());
        let (v1, v2, v3) = (list([0, 0, 0]), list([0, 5, 0]), list([0, 5, 9]));
        let token = |v: &Value| section_token("session", "AchievementInfos", v);
        let login = std::sync::Mutex::new(std::collections::BTreeMap::from([("AchievementInfos".to_string(), token(&v1))]));
        let delivered = std::sync::Mutex::new(DeliveredAchievements::default());
        remember_achievements(&delivered, &token(&v1), &v1);
        let mut headers = axum::http::HeaderMap::new();
        headers.insert("x-sprk-sections", "1".parse().unwrap());
        headers.insert("x-sprk-achievement-delta", "1".parse().unwrap());
        let reply = |snapshot: &Value, headers: &axum::http::HeaderMap| {
            let mut value = json!({"AchievementInfos": snapshot});
            conditional_sections_with_deltas(&mut value, "session", headers, Some(&login), Some(&delivered));
            value
        };
        // Holding the login list (nothing acknowledged yet): one changed entry.
        let first = reply(&v2, &headers);
        assert_eq!(first["AchievementInfos"], json!([v2[1]]));
        assert_eq!(first["SprkAchievementToken"], json!(token(&v2)), "the token is the full list's");
        assert_eq!(merge(&v1, &first["AchievementInfos"]), v2);
        // Acknowledged v2: only what changed since v2.
        headers.insert("x-sprk-achievements", token(&v2).parse().unwrap());
        let second = reply(&v3, &headers);
        assert_eq!(second["AchievementInfos"], json!([v3[2]]));
        assert_eq!(merge(&v2, &second["AchievementInfos"]), v3);
        // That reply was lost: the client still acknowledges v2 and gets the same delta.
        assert_eq!(reply(&v3, &headers)["AchievementInfos"], second["AchievementInfos"]);
        // An unknown list, a list without an entry it held, or a client that
        // did not opt in gets everything.
        headers.insert("x-sprk-achievements", "0".repeat(64).parse().unwrap());
        assert_eq!(reply(&v3, &headers)["AchievementInfos"], v3);
        headers.insert("x-sprk-achievements", token(&v3).parse().unwrap());
        let shorter = json!([v3[0], v3[1]]);
        assert_eq!(reply(&shorter, &headers)["AchievementInfos"], shorter);
        let mut legacy = headers.clone();
        legacy.remove("x-sprk-achievement-delta");
        legacy.insert("x-sprk-achievements", token(&v2).parse().unwrap());
        assert_eq!(reply(&v1, &legacy)["AchievementInfos"], v1);
        // Only a few recent lists are kept.
        for step in 10..20 { remember_achievements(&delivered, &format!("{step:064}"), &v1); }
        headers.insert("x-sprk-achievements", token(&v2).parse().unwrap());
        assert_eq!(reply(&v3, &headers)["AchievementInfos"], v3);
        assert!(delivered.lock().unwrap().len() <= DELIVERED_ACHIEVEMENTS_KEPT);
    }
    #[test]
    fn login_baseline_omits_resend_until_acknowledged_or_replaced() {
        let achievements = json!([{"AchievementIndex":1,"LastStep":2}]);
        let token = section_token("session", "AchievementInfos", &achievements);
        let login = std::sync::Mutex::new(std::collections::BTreeMap::from([("AchievementInfos".to_string(), token.clone())]));
        let mut headers = axum::http::HeaderMap::new();
        headers.insert("x-sprk-sections", "1".parse().unwrap());
        // First reply after login: no client token, unchanged snapshot.
        let mut first = json!({"AchievementInfos":achievements.clone()});
        conditional_sections_after_login(&mut first, "session", &headers, Some(&login));
        assert!(first.get("AchievementInfos").is_none());
        assert_eq!(first["SprkAchievementToken"], json!(token));
        // Another session never uses this baseline.
        let mut foreign = json!({"AchievementInfos":achievements.clone()});
        conditional_sections_after_login(&mut foreign, "other", &headers, Some(&login));
        assert_eq!(foreign["AchievementInfos"], achievements);
        // Legacy clients without section support always receive full data.
        let mut legacy = json!({"AchievementInfos":achievements.clone()});
        conditional_sections_after_login(&mut legacy, "session", &axum::http::HeaderMap::new(), Some(&login));
        assert_eq!(legacy["AchievementInfos"], achievements);
        // A changed snapshot is sent in full and ends the baseline, so a lost
        // reply cannot leave the client relying on stale login data.
        let changed = json!([{"AchievementIndex":1,"LastStep":3}]);
        let mut full = json!({"AchievementInfos":changed.clone()});
        conditional_sections_after_login(&mut full, "session", &headers, Some(&login));
        assert_eq!(full["AchievementInfos"], changed);
        let mut reverted = json!({"AchievementInfos":achievements.clone()});
        conditional_sections_after_login(&mut reverted, "session", &headers, Some(&login));
        assert_eq!(reverted["AchievementInfos"], achievements, "baseline must not survive a full resend");
        // Acknowledgement also retires the baseline; the token path continues.
        let login = std::sync::Mutex::new(std::collections::BTreeMap::from([("AchievementInfos".to_string(), token.clone())]));
        headers.insert("x-sprk-achievements", token.parse().unwrap());
        let mut acked = json!({"AchievementInfos":achievements.clone()});
        conditional_sections_after_login(&mut acked, "session", &headers, Some(&login));
        assert!(acked.get("AchievementInfos").is_none());
        assert!(login.lock().unwrap().is_empty());
    }
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
