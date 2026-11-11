//! Client housekeeping calls that carry no gameplay state.
use crate::{api::system::request::Request, error::Result, state::AppState};
use axum::{body::Bytes, extract::State, Json};
use serde_json::{json, Value};

/// `user/ping`: the native client's periodic heartbeat (no parameters besides
/// the common T1/T2/TEMP/SessionKey fields). It marks the session active for
/// online status. Like the former fallback reply, it never fails: an expired
/// session is reported by the next gameplay request instead.
pub async fn ping(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    if let Ok(req) = Request::parse(&body) {
        let _ = req.account(&state);
    }
    Ok(Json(json!({"BaseResult":"Success","Result":"Success",
        "ServerTime":state.server_time_str(),"ServerUTCTime":state.server_utc_time_str()})))
}

/// `user/set_push_notification`: the server sends no push notifications, so the
/// toggle (PushType, Status) is acknowledged without being stored.
pub async fn set_push_notification(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    Request::parse(&body)?.account(&state)?;
    Ok(Json(json!({"BaseResult":"Success","Result":"Success"})))
}

/// `push_reward/get_push_reward`: polled about once a minute. The server
/// offers no push rewards, so it answers the empty success the fallback gave,
/// without logging an unimplemented-endpoint warning per poll.
pub async fn get_push_reward(State(state): State<AppState>, body: Bytes) -> Result<Json<Value>> {
    if let Ok(req) = Request::parse(&body) {
        let _ = req.account(&state);
    }
    Ok(Json(json!({"BaseResult":"Success","Result":"Success"})))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn heartbeat_and_push_settings_reply_like_native_success() {
        let db = sqlx::sqlite::SqlitePoolOptions::new().max_connections(1).connect("sqlite::memory:").await.unwrap();
        let state = AppState::new(db, crate::tables::GameTables::empty());
        state.create_session("live".into(), 7, String::new());
        let before = state.get_session("live").unwrap().last_activity;
        let pinged = ping(State(state.clone()), Bytes::from_static(b"SessionKey=live&T1=1&T2=601&TEMP=5")).await.unwrap().0;
        assert_eq!((pinged["BaseResult"].as_str(), pinged["Result"].as_str()), (Some("Success"), Some("Success")));
        assert!(state.get_session("live").unwrap().last_activity >= before);
        // A stale heartbeat stays harmless, as before.
        assert_eq!(ping(State(state.clone()), Bytes::from_static(b"SessionKey=gone")).await.unwrap().0["Result"], "Success");
        let push = set_push_notification(State(state.clone()), Bytes::from_static(b"SessionKey=live&PushType=GuildRaid&Status=False")).await.unwrap().0;
        assert_eq!(push["Result"], "Success");
        assert!(set_push_notification(State(state.clone()), Bytes::from_static(b"SessionKey=gone&PushType=Event&Status=True")).await.is_err());
        for key in ["live", "gone"] {
            let reward = get_push_reward(State(state.clone()), Bytes::from(format!("SessionKey={key}"))).await.unwrap().0;
            assert_eq!(reward, json!({"BaseResult":"Success","Result":"Success"}));
        }
    }
}
