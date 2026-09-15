//! Common native response fields refresh mission UI after successful gameplay mutations.
use super::*;
use axum::{
    body::{to_bytes, Body},
    extract::Request as HttpRequest,
    middleware::Next,
    response::Response,
};

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
    ) {
        return next.run(request).await;
    }
    let (parts, body) = request.into_parts();
    let Ok(bytes) = to_bytes(body, 2 * 1024 * 1024).await else {
        return Response::builder().status(413).body(Body::empty()).unwrap();
    };
    let account = Request::parse(&bytes)
        .ok()
        .and_then(|r| r.account(&state).ok());
    let response = next
        .run(HttpRequest::from_parts(parts, Body::from(bytes)))
        .await;
    let Some(account) = account else {
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
                ("ReservedSubQuestInfos", "SubQuestInfos"),
                ("ReservedMainQuestInfo", "MainQuestInfo"),
                ("ReservedClearMissionInfos", "ClearMissionInfos"),
            ] {
                value[wire] = updates[field].clone();
            }
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
