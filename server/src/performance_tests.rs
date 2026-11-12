use super::*;
use axum::{
    body::{to_bytes, Body},
    http::Request,
    Json,
};
use serde_json::{json, Value};
use tower::ServiceExt;

#[tokio::test]
async fn http_compression_negotiates_and_preserves_json_and_timing() {
    let payload = json!({"Heroes":vec![json!({"HeroIndex":1,"Level":100,"Transcended":5});100]});
    let expected = serde_json::to_vec(&payload).unwrap();
    let app = http_router(
        Router::new()
            .route("/snapshot", get(move || async move { Json(payload) }))
            .route(
                "/small",
                get(|| async { Json(json!({"Result":"Success"})) }),
            ),
    );
    for (path, accept, compressed) in [
        ("/snapshot", "gzip", true),
        ("/snapshot", "identity", false),
        ("/snapshot", "gzip;q=0", false),
        ("/small", "gzip", false),
    ] {
        let response = app
            .clone()
            .oneshot(
                Request::get(path)
                    .header("accept-encoding", accept)
                    .body(Body::empty())
                    .unwrap(),
            )
            .await
            .unwrap();
        assert!(response.headers().contains_key("server-timing"));
        assert_eq!(
            response.headers().get("content-encoding").is_some(),
            compressed
        );
        let bytes = to_bytes(response.into_body(), usize::MAX).await.unwrap();
        let decoded = if compressed {
            use std::io::Read;
            assert!(bytes.len() < expected.len() / 4);
            let mut data = Vec::new();
            flate2::read::GzDecoder::new(&bytes[..])
                .read_to_end(&mut data)
                .unwrap();
            data
        } else {
            bytes.to_vec()
        };
        let value: Value = serde_json::from_slice(&decoded).unwrap();
        if path == "/snapshot" {
            assert_eq!(decoded, expected);
        } else {
            assert_eq!(value["Result"], "Success");
        }
    }
}
