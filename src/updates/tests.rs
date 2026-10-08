use super::*;
use axum::{
    body::{to_bytes, Body},
    http::{Method, Request as HttpRequest},
};
use tower::ServiceExt;

struct Fixture(PathBuf);
impl Fixture {
    async fn new() -> Self {
        let root = std::env::temp_dir().join(format!("sprk-updates-{}", uuid::Uuid::new_v4()));
        tokio::fs::create_dir_all(root.join("stable"))
            .await
            .unwrap();
        tokio::fs::create_dir_all(root.join("releases/1.0.3/files/King's Raid_Data/Managed"))
            .await
            .unwrap();
        tokio::fs::write(root.join("stable/manifest.json"), br#"{"version":"1.0.3"}"#)
            .await
            .unwrap();
        tokio::fs::write(
            root.join("releases/1.0.3/manifest.json"),
            br#"{"version":"1.0.3"}"#,
        )
        .await
        .unwrap();
        tokio::fs::write(
            root.join("releases/1.0.3/files/King's Raid_Data/Managed/SprkDispatch.dll"),
            b"0123456789",
        )
        .await
        .unwrap();
        Self(root)
    }
    async fn router(&self) -> Router {
        crate::service_router(None, Some(routes(&self.0).await.unwrap()))
    }
}
impl Drop for Fixture {
    fn drop(&mut self) {
        let _ = std::fs::remove_dir_all(&self.0);
    }
}

const FILE: &str = "/updates/releases/1.0.3/files/King%27s%20Raid_Data/Managed/SprkDispatch.dll";

async fn request(app: &Router, method: Method, uri: &str, range: Option<&str>) -> Response {
    let mut request = HttpRequest::builder().method(method).uri(uri);
    if let Some(range) = range {
        request = request.header(header::RANGE, range);
    }
    app.clone()
        .oneshot(request.body(Body::empty()).unwrap())
        .await
        .unwrap()
}

#[tokio::test]
async fn downloads_support_resume_head_and_correct_cache_policy() {
    let fixture = Fixture::new().await;
    let app = fixture.router().await;
    let response = request(&app, Method::GET, "/updates/stable/manifest.json", None).await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "no-store");
    assert_eq!(response.headers()[header::CONTENT_TYPE], "application/json");

    let response = request(&app, Method::GET, FILE, Some("bytes=3-6")).await;
    assert_eq!(response.status(), StatusCode::PARTIAL_CONTENT);
    assert_eq!(response.headers()[header::CONTENT_RANGE], "bytes 3-6/10");
    assert_eq!(
        response.headers()[header::CACHE_CONTROL],
        "public, max-age=31536000, immutable"
    );
    assert_eq!(
        to_bytes(response.into_body(), 1024).await.unwrap().as_ref(),
        b"3456"
    );

    let response = request(&app, Method::HEAD, FILE, None).await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(response.headers()[header::CONTENT_LENGTH], "10");
    assert!(to_bytes(response.into_body(), 1024)
        .await
        .unwrap()
        .is_empty());
    let response = request(&app, Method::GET, FILE, Some("bytes=100-")).await;
    assert_eq!(response.status(), StatusCode::RANGE_NOT_SATISFIABLE);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "no-store");

    let response = request(
        &app,
        Method::GET,
        "/updates/releases/1.0.3/manifest.json",
        None,
    )
    .await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(
        response.headers()[header::CACHE_CONTROL],
        "public, max-age=31536000, immutable"
    );
}

#[tokio::test]
async fn rejects_traversal_private_paths_missing_files_and_writes() {
    let fixture = Fixture::new().await;
    tokio::fs::write(fixture.0.join("secret.key"), b"must not escape")
        .await
        .unwrap();
    tokio::fs::write(fixture.0.join("releases/1.0.3/files/.secret"), b"hidden")
        .await
        .unwrap();
    let app = fixture.router().await;
    for path in [
        "/updates/%2e%2e/manifest.json",
        "/updates/releases/%2e%2e/manifest.json",
        "/updates/releases/1.0.3/files/%2e%2e/%2e%2e/%2e%2e/secret.key",
        "/updates/releases/1.0.3/files/foo%5c..%5csecret.key",
        "/updates/releases/1.0.3/files/C%3a/secret.key",
        "/updates/releases/1.0.3/files/.secret",
        "/updates/releases/1.0.3/files/missing.dll",
        "/updates/releases/1.0.3/files/King%27s%20Raid_Data/Managed",
        "/updates/.staging/manifest.json",
        "/updates/secret.key",
    ] {
        assert_eq!(
            request(&app, Method::GET, path, None).await.status(),
            StatusCode::NOT_FOUND,
            "{path}"
        );
    }
    assert_eq!(
        request(&app, Method::POST, FILE, None).await.status(),
        StatusCode::METHOD_NOT_ALLOWED
    );
}

#[tokio::test]
async fn update_routes_stay_outside_game_middleware_and_fallback() {
    let fixture = Fixture::new().await;
    let game = Router::new()
        .route("/host.json", get(|| async { "game" }))
        .fallback(|| async { "game fallback" })
        .layer(axum::middleware::from_fn(
            |_: Request, _: axum::middleware::Next| async { StatusCode::UNAUTHORIZED },
        ));
    let all = crate::service_router(Some(game.clone()), Some(routes(&fixture.0).await.unwrap()));
    assert_eq!(
        request(&all, Method::GET, FILE, None).await.status(),
        StatusCode::OK
    );
    assert_eq!(
        request(&all, Method::GET, "/updates/missing", None)
            .await
            .status(),
        StatusCode::NOT_FOUND
    );
    assert_eq!(
        request(&all, Method::GET, "/host.json", None)
            .await
            .status(),
        StatusCode::UNAUTHORIZED
    );
    let game_only = crate::service_router(Some(game), None);
    assert_eq!(
        request(&game_only, Method::GET, FILE, None).await.status(),
        StatusCode::NOT_FOUND
    );
    let updates_only = fixture.router().await;
    assert_eq!(
        request(&updates_only, Method::GET, "/host.json", None)
            .await
            .status(),
        StatusCode::NOT_FOUND
    );
    assert_eq!(
        request(&updates_only, Method::GET, "/health", None)
            .await
            .status(),
        StatusCode::OK
    );
}

#[tokio::test]
async fn update_storage_must_exist_and_be_a_directory() {
    let fixture = Fixture::new().await;
    assert!(routes(&fixture.0.join("does-not-exist")).await.is_err());
    assert!(routes(&fixture.0.join("stable/manifest.json"))
        .await
        .is_err());
}

#[cfg(unix)]
#[tokio::test]
async fn symlinks_cannot_expose_files_outside_update_storage() {
    let fixture = Fixture::new().await;
    let outside = Fixture::new().await;
    std::os::unix::fs::symlink(
        outside.0.join("stable/manifest.json"),
        fixture.0.join("releases/1.0.3/files/escape.json"),
    )
    .unwrap();
    let app = fixture.router().await;
    assert_eq!(
        request(
            &app,
            Method::GET,
            "/updates/releases/1.0.3/files/escape.json",
            None
        )
        .await
        .status(),
        StatusCode::NOT_FOUND
    );
}
