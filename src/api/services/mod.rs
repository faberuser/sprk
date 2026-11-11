//! Client supporting APIs and authenticated battle-service integration.
use crate::{
    api::{
        inventory::item::{self, n, rule},
        system::request::Request,
    },
    error::{Result, ServerError},
    state::AppState,
};
use axum::{
    body::Bytes,
    extract::{OriginalUri, State},
    http::HeaderMap,
    Json,
};
use serde_json::{json, Value};
use sqlx::PgConnection;
use crate::db::RowExt;
mod internal;
pub(crate) mod records;
#[cfg(test)]
mod tests;
pub fn routes(t: &crate::tables::BattleTable) -> axum::Router<AppState> {
    let mut r = axum::Router::new();
    for path in t.contracts.keys() {
        r = r.route(&format!("/{path}"), axum::routing::post(handle));
    }
    r.route("/battle/recover", axum::routing::post(handle))
}
async fn handle(
    State(s): State<AppState>,
    OriginalUri(uri): OriginalUri,
    headers: HeaderMap,
    body: Bytes,
) -> Result<Json<Value>> {
    execute_request(&s, uri.path().trim_start_matches('/'), &headers, body)
        .await
        .map(Json)
}
pub(crate) async fn execute_request(
    s: &AppState,
    path: &str,
    headers: &HeaderMap,
    body: Bytes,
) -> Result<Value> {
    let mut r = if matches!(path,"replay/save_replay"|"internal/b2m_save_replay") {
        Request::parse_with_integer_arrays(&body,&["AccountIds"])?
    } else {Request::parse(&body)?};
    if path == "user/get_stamina_infos" {
        let pairs: Vec<(String, String)> =
            serde_urlencoded::from_bytes(&body).map_err(|_| rule("Fail"))?;
        let values: Vec<_> = pairs
            .into_iter()
            .filter(|(k, _)| k == "StaminaTypes")
            .map(|(_, v)| v)
            .collect();
        if values.len() > 1 {
            r.0.insert("StaminaTypes".into(), json!(values).to_string());
        }
    }
    let service = path.starts_with("internal/");
    let a = if service {
        internal::authenticate(s, headers)?;
        r.number("AccountId", 0)?
    } else {
        r.account(s)?
    };
    if let Some(fields) = s
        .tables
        .services
        .contracts
        .get(path)
        .and_then(|v| v["Request"].as_object())
    {
        for (k, t) in fields {
            if let Some(e) = t.as_str().and_then(|v| s.tables.services.enums.get(v)) {
                if let Some(v) = r.0.get_mut(k) {
                    if let Some(i) = e.get(v) {
                        *v = i.to_string();
                    }
                }
            }
        }
    }
    let mut tx = crate::database::begin(&s.db).await?;
    // Serialize the account's read-modify-write sequences, including service requests.
    crate::postgres::lock_account(&mut *tx, a).await?;
    let result = if service {
        internal::execute(&mut tx, s, a, &r, headers, path).await
    } else if path.starts_with("user/") {
        crate::api::account::stamina::execute(&mut tx, s, a, &r, path).await
    } else if path == "battle/recover" {
        internal::recover(&mut tx, a).await
    } else {
        records::execute(&mut tx, s, a, &r, path, false).await
    };
    match result {
        Ok(v) => {
            tx.commit().await?;
            let mut out = item::success();
            if let Some(m) = v.as_object() {
                for (k, v) in m {
                    out[k] = v.clone();
                }
            }
            Ok(out)
        }
        Err(ServerError::InvalidRequest(code)) => {
            tx.rollback().await?;
            let allowed = s
                .tables
                .services
                .contracts
                .get(path)
                .and_then(|v| v["Results"].as_array())
                .is_some_and(|v| v.contains(&json!(code)));
            Ok(json!({"BaseResult":"Success","Result":if allowed{code}else{"Fail".into()}}))
        }
        Err(e) => Err(e),
    }
}
fn now() -> i64 {
    chrono::Utc::now().timestamp()
}
fn setting(s: &AppState, key: &str, default: i64) -> i64 {
    s.tables.services.rules[key].as_i64().unwrap_or(default)
}

pub(crate) async fn login(s: &AppState, a: i64) -> Result<Value> {
    let recovered = internal::recover(&mut *s.db.acquire().await?, a).await?;
    Ok(json!({"ServiceBattle":recovered["Battle"]}))
}
