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

#[tokio::test]
async fn illustration_model_is_immutable_separate_and_read_only() {
    let fixture = Fixture::new().await;
    let directory = fixture.0.join("illustration-models/realesrgan-anime-20220424-v1");
    tokio::fs::create_dir_all(&directory).await.unwrap();
    tokio::fs::write(directory.join("model.sprkmodel"), b"model-data").await.unwrap();
    tokio::fs::write(directory.join("metadata.json"), b"{}").await.unwrap();
    let app = fixture.router().await;
    let uri = "/updates/models/realesrgan-anime-20220424-v1/model.sprkmodel";
    let response = request(&app, Method::GET, uri, Some("bytes=0-4")).await;
    assert_eq!(response.status(), StatusCode::PARTIAL_CONTENT);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "public, max-age=31536000, immutable");
    assert_eq!(to_bytes(response.into_body(), 1024).await.unwrap().as_ref(), b"model");
    assert_eq!(request(&app, Method::HEAD, uri, None).await.status(), StatusCode::OK);
    assert_eq!(request(&app, Method::POST, uri, None).await.status(), StatusCode::METHOD_NOT_ALLOWED);
    assert_eq!(request(&app, Method::GET, "/updates/models/realesrgan-anime-20220424-v1/private.pem", None).await.status(), StatusCode::NOT_FOUND);
    assert_eq!(request(&app, Method::GET, "/updates/models/unknown/model.sprkmodel", None).await.status(), StatusCode::NOT_FOUND);
    assert_eq!(request(&app, Method::GET, "/updates/models/realesrgan-anime-20220424-v1/metadata.json", None).await.status(), StatusCode::OK);
    let directory = fixture.0.join("illustration-models/remacri-4x-v1");
    tokio::fs::create_dir_all(&directory).await.unwrap();
    tokio::fs::write(directory.join("model.sprkmodel"), b"remacri-model").await.unwrap();
    let response = request(&app, Method::GET, "/updates/models/remacri-4x-v1/model.sprkmodel", Some("bytes=0-6")).await;
    assert_eq!(response.status(), StatusCode::PARTIAL_CONTENT);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "public, max-age=31536000, immutable");
    assert_eq!(to_bytes(response.into_body(), 1024).await.unwrap().as_ref(), b"remacri");
    assert_eq!(request(&app, Method::GET, "/updates/models/remacri-4x-v1/private.pem", None).await.status(), StatusCode::NOT_FOUND);
}

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

const BUNDLE_SHA: &str = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

async fn write_assets(root: &FsPath, index: &str) {
    let version = root.join("assets/StandaloneWindows/versions/2");
    tokio::fs::create_dir_all(&version).await.unwrap();
    tokio::fs::create_dir_all(root.join("assets/objects")).await.unwrap();
    tokio::fs::write(root.join("assets/StandaloneWindows/current.txt"), b"2").await.unwrap();
    tokio::fs::write(version.join("patch.json"), br#"{"Archives":[]}"#).await.unwrap();
    tokio::fs::write(version.join("index.json"), index).await.unwrap();
    tokio::fs::write(root.join("assets/objects").join(BUNDLE_SHA), b"bundle-bytes").await.unwrap();
}

#[tokio::test]
async fn assets_serve_version_patch_and_archives_by_hash() {
    let fixture = Fixture::new().await;
    write_assets(
        &fixture.0,
        &format!(r#"{{"files":{{"Assetbundle/[English]/A b.unity3d":"{BUNDLE_SHA}"}}}}"#),
    )
    .await;
    let app = fixture.router().await;
    let base = "/updates/assets/StandaloneWindows";

    // The client appends a random query string to defeat caches.
    let response = request(&app, Method::GET, &format!("{base}/LastBuildVersion.txt?123"), None).await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "no-store");
    assert_eq!(to_bytes(response.into_body(), 64).await.unwrap().as_ref(), b"2");

    let response = request(&app, Method::GET, &format!("{base}/2/patch.json?5"), None).await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "public, max-age=31536000, immutable");

    let archive = format!("{base}/2/Assetbundle/%5BEnglish%5D/A%20b.unity3d");
    let response = request(&app, Method::GET, &archive, Some("bytes=7-11")).await;
    assert_eq!(response.status(), StatusCode::PARTIAL_CONTENT);
    assert_eq!(response.headers()[header::CONTENT_RANGE], "bytes 7-11/12");
    assert_eq!(to_bytes(response.into_body(), 64).await.unwrap().as_ref(), b"bytes");
    assert_eq!(request(&app, Method::HEAD, &archive, None).await.status(), StatusCode::OK);
    assert_eq!(request(&app, Method::POST, &archive, None).await.status(), StatusCode::METHOD_NOT_ALLOWED);

    for missing in [
        format!("{base}/2/Assetbundle/unknown.unity3d"),
        format!("{base}/3/patch.json"),
        format!("{base}/0/patch.json"),
        format!("{base}/x/patch.json"),
        format!("{base}/2/index.json"),
        format!("{base}/2/../current.txt"),
        "/updates/assets/Android/LastBuildVersion.txt".into(),
        "/updates/assets/objects/LastBuildVersion.txt".into(),
        format!("/updates/assets/objects/2/{BUNDLE_SHA}"),
    ] {
        assert_eq!(request(&app, Method::GET, &missing, None).await.status(), StatusCode::NOT_FOUND, "{missing}");
    }
}

#[tokio::test]
async fn assets_reject_index_with_invalid_hash() {
    let fixture = Fixture::new().await;
    write_assets(&fixture.0, r#"{"files":{"Assetbundle/a.unity3d":"../../stable/manifest.json"}}"#).await;
    let app = fixture.router().await;
    let response = request(&app, Method::GET, "/updates/assets/StandaloneWindows/2/Assetbundle/a.unity3d", None).await;
    assert_eq!(response.status(), StatusCode::INTERNAL_SERVER_ERROR);
}

#[tokio::test]
async fn assets_serve_objects_by_hash() {
    let fixture = Fixture::new().await;
    write_assets(&fixture.0, r#"{"files":{}}"#).await;
    let app = fixture.router().await;

    let object = format!("/updates/assets/objects/{BUNDLE_SHA}");
    let response = request(&app, Method::GET, &object, Some("bytes=0-5")).await;
    assert_eq!(response.status(), StatusCode::PARTIAL_CONTENT);
    assert_eq!(response.headers()[header::CACHE_CONTROL], "public, max-age=31536000, immutable");
    assert_eq!(to_bytes(response.into_body(), 64).await.unwrap().as_ref(), b"bundle");

    for missing in [
        "/updates/assets/objects/ABC".to_string(),
        format!("/updates/assets/objects/{}", BUNDLE_SHA.to_uppercase()),
        "/updates/assets/objects/manifest.json".into(),
        "/updates/assets/Android/manifest.json".into(),
    ] {
        assert_eq!(request(&app, Method::GET, &missing, None).await.status(), StatusCode::NOT_FOUND, "{missing}");
    }
}

#[tokio::test]
async fn assets_redirect_to_the_mirror_once_mirrored() {
    use axum::routing::head;
    // A mirror that has the bundle; any other object is missing.
    let mirror = Router::new().route(
        "/objects/{sha}",
        head(|axum::extract::Path(sha): axum::extract::Path<String>| async move {
            if sha == BUNDLE_SHA { StatusCode::OK } else { StatusCode::NOT_FOUND }
        }),
    );
    let listener = tokio::net::TcpListener::bind("127.0.0.1:0").await.unwrap();
    let port = listener.local_addr().unwrap().port();
    tokio::spawn(async move { axum::serve(listener, mirror).await.unwrap() });
    let mirror_url = format!("http://127.0.0.1:{port}/objects/");

    let fixture = Fixture::new().await;
    let other = "c".repeat(64);
    write_assets(
        &fixture.0,
        &format!(r#"{{"files":{{"Assetbundle/[English]/A b.unity3d":"{BUNDLE_SHA}","Assetbundle/new.unity3d":"{other}"}}}}"#),
    )
    .await;
    tokio::fs::write(fixture.0.join("assets/objects").join(&other), b"not uploaded yet").await.unwrap();
    let mirror = mirror::Mirror::new_for_tests(&mirror_url);
    let app = crate::service_router(None, Some(routes_with(&fixture.0, Some(Arc::new(mirror))).await.unwrap()));
    let base = "/updates/assets/StandaloneWindows";

    let response = request(&app, Method::GET, &format!("{base}/2/Assetbundle/%5BEnglish%5D/A%20b.unity3d"), None).await;
    assert_eq!(response.status(), StatusCode::FOUND);
    assert_eq!(response.headers()[header::LOCATION], format!("{mirror_url}{BUNDLE_SHA}"));
    let response = request(&app, Method::GET, &format!("/updates/assets/objects/{BUNDLE_SHA}"), None).await;
    assert_eq!(response.status(), StatusCode::FOUND);

    // Not on the mirror yet: served here.
    let response = request(&app, Method::GET, &format!("{base}/2/Assetbundle/new.unity3d"), None).await;
    assert_eq!(response.status(), StatusCode::OK);
    assert_eq!(to_bytes(response.into_body(), 64).await.unwrap().as_ref(), b"not uploaded yet");
    // Version files and unknown names never go to the mirror.
    assert_eq!(request(&app, Method::GET, &format!("{base}/2/patch.json"), None).await.status(), StatusCode::OK);
    assert_eq!(request(&app, Method::GET, &format!("{base}/LastBuildVersion.txt"), None).await.status(), StatusCode::OK);
    assert_eq!(request(&app, Method::GET, &format!("{base}/2/unknown.unity3d"), None).await.status(), StatusCode::NOT_FOUND);
}
