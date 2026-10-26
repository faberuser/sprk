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
        || value.get("AchievementInfos").is_some()
    {
        return Response::from_parts(parts, Body::from(bytes));
    }
    let updates = async {
        let mut tx = state.db.begin().await?;
        item::init(&mut tx, &state, account).await?;
        touch_day(&mut tx, account).await?;
        let updates = snapshot(&mut tx, &state, account).await?;
        tx.commit().await?;
        Ok::<_, ServerError>(updates)
    }
    .await;
    match updates {
        Ok(updates) => {
            append_snapshots(&mut value, &updates, &session_key, &acknowledged);
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
fn append_snapshots(value: &mut Value, updates: &Value, session: &str, acknowledged: &str) {
    use sha2::{Digest, Sha256};
    let mut digest = Sha256::new();
    digest.update(session.as_bytes());
    let fields = [
        ("AchievementInfos", "AchievementInfos"),
        ("ReservedMainQuestInfo", "MainQuestInfo"),
        ("ReservedClearMissionInfos", "ClearMissionInfos"),
        ("ReservedPlayRecordInfos", "PlayRecordInfos"),
    ];
    for (_, field) in fields {
        let mut snapshot = updates[field].clone();
        if field == "PlayRecordInfos" {
            // These timestamps describe snapshot generation, not progress changes.
            // Keep them on the wire without invalidating an acknowledged state.
            for record in snapshot.as_array_mut().into_iter().flatten() {
                if let Some(record) = record.as_object_mut() {
                    record.remove("UpdatedTime");
                }
            }
        }
        digest.update(serde_json::to_vec(&snapshot).unwrap());
    }
    let token = hex::encode(digest.finalize());
    if token != acknowledged {
        for (wire, field) in fields {
            value[wire] = updates[field].clone();
        }
    }
    value["SprkProgressionToken"] = json!(token);
}

#[cfg(test)]
mod tests {
    use super::*;
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
