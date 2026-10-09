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
    let family = path.split('/').nth(1).unwrap_or("");
    if !matches!(
        family,
        "hero"
            | "hero_storage_slot"
            | "item"
            | "equip"
            | "campaign"
            | "shop"
            | "friend"
            | "mail"
            | "tutorial"
            | "hero_inn"
            | "guild"
            | "npc"
            | "valance"
            | "equip_storage_slot"
    ) {
        return next.run(request).await;
    }
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
            for (wire, field) in [
                ("AchievementInfos", "AchievementInfos"),
                ("ReservedMainQuestInfo", "MainQuestInfo"),
                ("ReservedClearMissionInfos", "ClearMissionInfos"),
            ] {
                value[wire] = updates[field].clone();
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
