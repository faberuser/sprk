//! Asset object mirror: redirects game asset downloads to a copy of
//! `assets/objects/` in object storage (such as Cloudflare R2 behind a custom
//! domain), so the bytes leave from there instead of this server.
//!
//! `SPRK_ASSETS_MIRROR_URL` is the public URL of the mirrored objects folder;
//! an object is downloaded from `<url>/<sha256>`. Something else uploads the
//! objects (the `assets-mirror` service in docker-compose.yml). This server
//! keeps its own store as the source of truth and redirects a request only
//! after a `HEAD` to the mirror has found that object; until then, and
//! whenever the mirror cannot be reached, it serves the file itself. A newly
//! activated asset version therefore works before it has been uploaded.
//!
//! Objects are immutable and named by hash, so a found object is remembered
//! for the life of the process; a missing one is checked again after a minute.
//! The game's downloader (UnityWebRequest) follows the redirect, keeping
//! `Range` requests.
use axum::{
    body::Body,
    http::{header, HeaderValue, StatusCode},
    response::Response,
};
use dashmap::DashMap;
use std::time::Duration;
use tokio::time::Instant;

/// How long a missing object is served locally before the mirror is asked again.
const RECHECK_MISSING: Duration = Duration::from_secs(60);

pub(super) struct Mirror {
    base: String,
    client: reqwest::Client,
    /// `None`: found on the mirror. `Some(t)`: missing when checked at `t`.
    known: DashMap<String, Option<Instant>>,
}

impl Mirror {
    pub fn from_env() -> anyhow::Result<Option<Self>> {
        Self::new(std::env::var("SPRK_ASSETS_MIRROR_URL").ok().as_deref())
    }

    fn new(url: Option<&str>) -> anyhow::Result<Option<Self>> {
        let Some(url) = url.map(str::trim).filter(|v| !v.is_empty()) else {
            return Ok(None);
        };
        let parsed = reqwest::Url::parse(url)
            .map_err(|_| anyhow::anyhow!("SPRK_ASSETS_MIRROR_URL is not a URL: {url:?}"))?;
        anyhow::ensure!(
            parsed.scheme() == "https" || parsed.host_str().is_some_and(|h| h == "127.0.0.1" || h == "localhost"),
            "SPRK_ASSETS_MIRROR_URL must use https: {url:?}"
        );
        anyhow::ensure!(
            parsed.query().is_none() && parsed.fragment().is_none(),
            "SPRK_ASSETS_MIRROR_URL must not have a query or fragment: {url:?}"
        );
        let client = reqwest::Client::builder()
            .user_agent("sprk-assets-mirror-check")
            .redirect(reqwest::redirect::Policy::none())
            .connect_timeout(Duration::from_secs(5))
            .timeout(Duration::from_secs(10))
            .build()?;
        tracing::info!(url, "Redirecting game asset downloads to the mirror");
        Ok(Some(Self {
            base: url.trim_end_matches('/').to_owned(),
            client,
            known: DashMap::new(),
        }))
    }

    #[cfg(test)]
    pub fn new_for_tests(url: &str) -> Self {
        Self::new(Some(url)).unwrap().unwrap()
    }

    fn url(&self, sha256: &str) -> String {
        format!("{}/{sha256}", self.base)
    }

    /// A redirect to the mirror's copy, or `None` to serve the object locally.
    pub async fn redirect(&self, sha256: &str) -> Option<Response> {
        self.has(sha256).await.then(|| {
            Response::builder()
                .status(StatusCode::FOUND)
                .header(header::LOCATION, self.url(sha256))
                // The target is immutable, but whether it is mirrored is not
                // known to caches in between; let every request come here.
                .header(header::CACHE_CONTROL, HeaderValue::from_static("no-store"))
                .body(Body::empty())
                .expect("valid redirect")
        })
    }

    async fn has(&self, sha256: &str) -> bool {
        match self.known.get(sha256).map(|entry| *entry) {
            Some(None) => return true,
            Some(Some(checked)) if checked.elapsed() < RECHECK_MISSING => return false,
            _ => {}
        }
        let found = match self.client.head(self.url(sha256)).send().await {
            Ok(response) => response.status().is_success(),
            Err(error) => {
                tracing::debug!(%error, "Asset mirror check failed; serving locally");
                false
            }
        };
        self.known
            .insert(sha256.to_owned(), (!found).then(Instant::now));
        found
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use axum::{extract::Path, routing::head, Router};

    const PRESENT: &str = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    const MISSING: &str = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";

    #[test]
    fn parses_configuration() {
        assert!(Mirror::new(None).unwrap().is_none());
        assert!(Mirror::new(Some(" ")).unwrap().is_none());
        let mirror = Mirror::new(Some("https://assets.example.com/assets/objects/")).unwrap().unwrap();
        assert_eq!(mirror.url(PRESENT), format!("https://assets.example.com/assets/objects/{PRESENT}"));
        for invalid in ["assets.example.com", "http://assets.example.com/", "https://a.example.com/?x=1"] {
            assert!(Mirror::new(Some(invalid)).is_err(), "{invalid}");
        }
    }

    async fn mirror_server() -> (String, Arc<AtomicUsize>) {
        let checks = Arc::new(AtomicUsize::new(0));
        let counted = checks.clone();
        let app = Router::new().route(
            "/objects/{sha}",
            head(move |Path(sha): Path<String>| {
                counted.fetch_add(1, Ordering::SeqCst);
                async move { if sha == PRESENT { StatusCode::OK } else { StatusCode::NOT_FOUND } }
            }),
        );
        let listener = tokio::net::TcpListener::bind("127.0.0.1:0").await.unwrap();
        let address = listener.local_addr().unwrap();
        tokio::spawn(async move { axum::serve(listener, app).await.unwrap() });
        (format!("http://127.0.0.1:{}/objects/", address.port()), checks)
    }

    use std::sync::{
        atomic::{AtomicUsize, Ordering},
        Arc,
    };

    #[tokio::test]
    async fn redirects_only_mirrored_objects_and_remembers() {
        let (url, checks) = mirror_server().await;
        let mirror = Mirror::new(Some(&url)).unwrap().unwrap();
        let response = mirror.redirect(PRESENT).await.unwrap();
        assert_eq!(response.status(), StatusCode::FOUND);
        assert_eq!(response.headers()[header::LOCATION], format!("{url}{PRESENT}"));
        assert!(mirror.redirect(MISSING).await.is_none());
        assert!(mirror.redirect(PRESENT).await.is_some());
        assert!(mirror.redirect(MISSING).await.is_none());
        assert_eq!(checks.load(Ordering::SeqCst), 2, "results are remembered");
    }

    #[tokio::test]
    async fn unreachable_mirror_serves_locally() {
        let listener = std::net::TcpListener::bind("127.0.0.1:0").unwrap();
        let port = listener.local_addr().unwrap().port();
        drop(listener);
        let mirror = Mirror::new(Some(&format!("http://127.0.0.1:{port}/objects/"))).unwrap().unwrap();
        assert!(mirror.redirect(PRESENT).await.is_none());
    }
}
