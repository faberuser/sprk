use axum::{
    extract::Request,
    middleware::Next,
    response::Response,
    http::StatusCode,
};

/// Middleware to decrypt incoming requests if they're encrypted
/// Currently passes through - encryption is handled per-endpoint
pub async fn decrypt_request(
    request: Request,
    next: Next,
) -> Result<Response, StatusCode> {
    // For now, pass through - encryption will be handled per-endpoint
    // as we need the session key from the request itself
    Ok(next.run(request).await)
}

/// Record the player a request acts for, so `database::begin` can take that
/// player's account lock in each of its transactions: one player's
/// transactions run one at a time while different players run in parallel.
/// Requests without a valid session (and
/// GETs) run without an acting account.
pub async fn acting_account(
    axum::extract::State(state): axum::extract::State<crate::state::AppState>,
    request: Request,
    next: Next,
) -> Response {
    if request.method() != axum::http::Method::POST {
        return next.run(request).await;
    }
    let (parts, body) = request.into_parts();
    // Same limit as the handlers' own body extractors.
    let Ok(bytes) = axum::body::to_bytes(body, 2 * 1024 * 1024).await else {
        return Response::builder().status(StatusCode::PAYLOAD_TOO_LARGE).body(axum::body::Body::empty()).unwrap();
    };
    let account = super::request::Request::parse(&bytes).ok()
        .and_then(|r| r.account(&state).ok())
        .filter(|a| *a > 0);
    let path = parts.uri.path().to_owned();
    let request = Request::from_parts(parts, axum::body::Body::from(bytes));
    match account {
        Some(account) => {
            let response = crate::database::ACTING_ACCOUNT.scope(account, next.run(request)).await;
            repair_gem_response(&state, account, &path, response).await
        }
        None => next.run(request).await,
    }
}

const GEM_RESULT: &[u8] = br#""CurrencyType":"Gem""#;

/// The native client applies a Gem CurrencyResultInfo3 from its
/// NewSysGem/NewPayGem Field/Value pairs and ignores NewValue, so a Gem result
/// without them sets both ruby partitions to 0 until the next ruby result or a
/// relog. Every endpoint should build Gem results with `item::money`; this
/// repairs any that do not from the stored balance and logs the endpoint.
async fn repair_gem_response(state: &crate::state::AppState, account: i64, path: &str, response: Response) -> Response {
    let (mut parts, body) = response.into_parts();
    let Ok(bytes) = axum::body::to_bytes(body, 64 * 1024 * 1024).await else {
        return Response::builder().status(StatusCode::INTERNAL_SERVER_ERROR).body(axum::body::Body::empty()).unwrap();
    };
    let unchanged = |parts, bytes| Response::from_parts(parts, axum::body::Body::from(bytes));
    if !bytes.windows(GEM_RESULT.len()).any(|w| w == GEM_RESULT) {
        return unchanged(parts, bytes);
    }
    let Ok(mut value) = serde_json::from_slice::<serde_json::Value>(&bytes) else {
        return unchanged(parts, bytes);
    };
    let broken = count_broken_gem_results(&value);
    if broken == 0 {
        return unchanged(parts, bytes);
    }
    let balance: Result<(i64, i64), _> = crate::db::query_as("SELECT gem,pay_gem FROM user_info WHERE account_id=$1")
        .bind(account).fetch_one(&state.db).await;
    let Ok((sys, pay)) = balance else {
        return unchanged(parts, bytes);
    };
    tracing::warn!(path, count = broken, "Gem currency result without NewSysGem/NewPayGem; repaired");
    repair_gem_results(&mut value, sys, pay);
    match serde_json::to_vec(&value) {
        Ok(fixed) => {
            parts.headers.remove(axum::http::header::CONTENT_LENGTH);
            Response::from_parts(parts, axum::body::Body::from(fixed))
        }
        Err(_) => unchanged(parts, bytes),
    }
}

/// A Gem currency *result* (it has NewValue) the native client cannot read.
fn broken_gem_result(v: &serde_json::Map<String, serde_json::Value>) -> bool {
    v.get("CurrencyType").and_then(|t| t.as_str()) == Some("Gem")
        && v.contains_key("NewValue")
        && !["Field1", "Field2", "Field3", "Field4"].iter()
            .any(|f| v.get(*f).and_then(|n| n.as_str()) == Some("NewSysGem"))
}

fn count_broken_gem_results(value: &serde_json::Value) -> usize {
    match value {
        serde_json::Value::Object(map) => usize::from(broken_gem_result(map))
            + map.values().map(count_broken_gem_results).sum::<usize>(),
        serde_json::Value::Array(items) => items.iter().map(count_broken_gem_results).sum(),
        _ => 0,
    }
}

pub(crate) fn repair_gem_results(value: &mut serde_json::Value, sys: i64, pay: i64) {
    match value {
        serde_json::Value::Object(map) => {
            if broken_gem_result(map) {
                map.insert("NewValue".into(), (sys + pay).into());
                map.insert("Field1".into(), "NewSysGem".into());
                map.insert("Value1".into(), sys.into());
                map.insert("Field2".into(), "NewPayGem".into());
                map.insert("Value2".into(), pay.into());
            }
            for v in map.values_mut() { repair_gem_results(v, sys, pay); }
        }
        serde_json::Value::Array(items) => for v in items { repair_gem_results(v, sys, pay); },
        _ => {}
    }
}

#[cfg(test)]
mod gem_tests {
    use serde_json::json;
    #[test]
    fn gem_results_without_partitions_are_repaired_and_valid_ones_kept() {
        let mut v = json!({"CurrencyResults":[
            {"CurrencyType":"Gem","AddValue":50,"NewValue":150},
            {"CurrencyType":"Gem","AddValue":-5,"NewValue":7,"Field1":"NewSysGem","Value1":3,"Field2":"NewPayGem","Value2":4},
            {"CurrencyType":"Gold","AddValue":1,"NewValue":2}],
            "Rewards":[{"CurrencyType":"Gem","Amount":10}]});
        super::repair_gem_results(&mut v, 100, 50);
        assert_eq!(v["CurrencyResults"][0], json!({"CurrencyType":"Gem","AddValue":50,"NewValue":150,"Field1":"NewSysGem","Value1":100,"Field2":"NewPayGem","Value2":50}));
        assert_eq!(v["CurrencyResults"][1]["Value1"], 3, "valid results keep their own snapshot");
        assert!(v["CurrencyResults"][2].get("Field1").is_none());
        assert!(v["Rewards"][0].get("Field1").is_none(), "reward definitions are not results");
    }

    #[tokio::test]
    async fn player_responses_never_carry_an_unreadable_gem_result() {
        use tower::ServiceExt;
        let s = crate::api::community::tests::setup().await;
        let u = crate::api::community::tests::login(&s, "gem-repair").await;
        let a = u["UserInfo"]["AccountId"].as_i64().unwrap();
        crate::db::query("UPDATE user_info SET gem=120,pay_gem=30 WHERE account_id=$1").bind(a).execute(&s.db).await.unwrap();
        let app = axum::Router::new()
            .route("/broken", axum::routing::post(|| async {
                axum::Json(json!({"Result":"Success","CurrencyResults":[{"CurrencyType":"Gem","AddValue":20,"NewValue":150}]}))
            }))
            .layer(axum::middleware::from_fn_with_state(s.clone(), super::acting_account))
            .with_state(s.clone());
        let body = format!("SessionKey={}", u["UserInfo"]["SessionKey"].as_str().unwrap());
        let response = app.oneshot(axum::http::Request::post("/broken")
            .header("content-type", "application/x-www-form-urlencoded")
            .body(axum::body::Body::from(body)).unwrap()).await.unwrap();
        let bytes = axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap();
        let v: serde_json::Value = serde_json::from_slice(&bytes).unwrap();
        let gem = &v["CurrencyResults"][0];
        assert_eq!((&gem["Field1"], &gem["Value1"], &gem["Field2"], &gem["Value2"], &gem["NewValue"]),
            (&json!("NewSysGem"), &json!(120), &json!("NewPayGem"), &json!(30), &json!(150)), "{v}");
    }
}
