//! Bandwidth limit for update downloads, shared fairly between players.
//!
//! Updates and the game usually share one upload link. Unlimited downloads fill
//! it and delay game traffic, and because TCP shares bandwidth per connection,
//! the game's patch downloader (30 parallel connections) takes almost all of it
//! from a launcher (4 connections). This limiter caps the total rate
//! (`SPRK_UPDATES_MAX_MBPS`) and splits it evenly between the clients
//! downloading at that moment, however many connections each one opens.
//!
//! Clients are told apart by IP address: the peer address, or the first value
//! of `SPRK_UPDATES_CLIENT_IP_HEADER` (such as `CF-Connecting-IP` or
//! `X-Forwarded-For`) when a trusted reverse proxy sets it. Behind a proxy
//! without that header every player looks like one client: the total cap still
//! applies, but the split is per connection again.
use axum::{
    body::{Body, Bytes},
    extract::{ConnectInfo, Request, State},
    http::HeaderName,
    middleware::{self, Next},
    response::Response,
    Router,
};
use dashmap::DashMap;
use futures_util::{stream, StreamExt};
use std::{
    net::SocketAddr,
    sync::{Arc, Mutex},
    time::{Duration, Instant},
};

/// Pieces a response is sent in; smaller pieces keep the rate smooth.
const PIECE: usize = 16 * 1024;
/// Burst a client may send after idling, in seconds of its share.
const BURST_SECONDS: f64 = 0.25;

#[derive(Debug, Clone, PartialEq)]
pub(super) struct Config {
    /// Total bytes per second for all downloads.
    pub bytes_per_second: f64,
    pub client_ip_header: Option<HeaderName>,
}

impl Config {
    pub fn from_env() -> anyhow::Result<Option<Self>> {
        let rate = std::env::var("SPRK_UPDATES_MAX_MBPS").ok();
        let header = std::env::var("SPRK_UPDATES_CLIENT_IP_HEADER").ok();
        Self::parse(rate.as_deref(), header.as_deref())
    }

    fn parse(rate: Option<&str>, header: Option<&str>) -> anyhow::Result<Option<Self>> {
        let Some(rate) = rate.map(str::trim).filter(|v| !v.is_empty() && *v != "0") else {
            return Ok(None);
        };
        let mbps: f64 = rate
            .parse()
            .ok()
            .filter(|v: &f64| v.is_finite() && *v > 0.0)
            .ok_or_else(|| anyhow::anyhow!("SPRK_UPDATES_MAX_MBPS must be a positive number of megabits per second, got {rate:?}"))?;
        Ok(Some(Self {
            bytes_per_second: mbps * 1_000_000.0 / 8.0,
            client_ip_header: parse_client_ip_header(header)?,
        }))
    }
}

/// `SPRK_UPDATES_CLIENT_IP_HEADER`, shared with the download queue.
pub(super) fn parse_client_ip_header(header: Option<&str>) -> anyhow::Result<Option<HeaderName>> {
    match header.map(str::trim).filter(|v| !v.is_empty()) {
        Some(name) => Ok(Some(HeaderName::try_from(name).map_err(|_| {
            anyhow::anyhow!("SPRK_UPDATES_CLIENT_IP_HEADER is not a valid header name: {name:?}")
        })?)),
        None => Ok(None),
    }
}

pub(super) fn layer(router: Router, config: Config) -> Router {
    tracing::info!(
        mbps = config.bytes_per_second * 8.0 / 1_000_000.0,
        client_ip_header = ?config.client_ip_header,
        "Limiting client update download bandwidth"
    );
    router.layer(middleware::from_fn_with_state(Limiter::new(config), limit))
}

#[derive(Clone)]
pub(super) struct Limiter(Arc<Shared>);

struct Shared {
    config: Config,
    clients: DashMap<String, Arc<Client>>,
}

struct Client {
    /// Response bodies currently streaming to this client.
    downloads: Mutex<usize>,
    bucket: Mutex<Bucket>,
}

struct Bucket {
    /// May go negative: a debt the client waits off before its next piece.
    tokens: f64,
    updated: Instant,
}

impl Limiter {
    pub fn new(config: Config) -> Self {
        Self(Arc::new(Shared {
            config,
            clients: DashMap::new(),
        }))
    }

    /// Registers a download; the client counts as active until the guard drops.
    fn start(&self, key: String) -> Download {
        let client = self
            .0
            .clients
            .entry(key.clone())
            .or_insert_with(|| {
                Arc::new(Client {
                    downloads: Mutex::new(0),
                    bucket: Mutex::new(Bucket {
                        tokens: 0.0,
                        updated: Instant::now(),
                    }),
                })
            })
            .clone();
        // Counted while the map entry is held, so removal cannot race it.
        *client.downloads.lock().unwrap() += 1;
        Download {
            limiter: self.clone(),
            key,
            client,
        }
    }

    fn share(&self) -> f64 {
        self.0.config.bytes_per_second / self.0.clients.len().max(1) as f64
    }
}

struct Download {
    limiter: Limiter,
    key: String,
    client: Arc<Client>,
}

impl Download {
    /// How long to wait before sending `bytes` more.
    fn debit(&self, bytes: usize) -> Duration {
        let rate = self.limiter.share();
        let mut bucket = self.client.bucket.lock().unwrap();
        let now = Instant::now();
        let burst = (rate * BURST_SECONDS).max(PIECE as f64);
        let elapsed = now.duration_since(bucket.updated).as_secs_f64();
        bucket.tokens = (bucket.tokens + elapsed * rate).min(burst) - bytes as f64;
        bucket.updated = now;
        if bucket.tokens >= 0.0 {
            Duration::ZERO
        } else {
            Duration::from_secs_f64(-bucket.tokens / rate)
        }
    }
}

impl Drop for Download {
    fn drop(&mut self) {
        let mut downloads = self.client.downloads.lock().unwrap();
        *downloads -= 1;
        if *downloads == 0 {
            drop(downloads);
            self.limiter
                .0
                .clients
                .remove_if(&self.key, |_, client| *client.downloads.lock().unwrap() == 0);
        }
    }
}

/// The player a request belongs to: its IP address (see the module comment).
pub(super) fn client_key(client_ip_header: Option<&HeaderName>, request: &Request) -> String {
    if let Some(name) = client_ip_header {
        if let Some(value) = request
            .headers()
            .get(name)
            .and_then(|v| v.to_str().ok())
            .and_then(|v| v.split(',').next())
            .map(str::trim)
            .filter(|v| !v.is_empty())
        {
            return value.to_owned();
        }
    }
    request
        .extensions()
        .get::<ConnectInfo<SocketAddr>>()
        .map(|info| info.0.ip().to_string())
        .unwrap_or_default()
}

async fn limit(State(limiter): State<Limiter>, request: Request, next: Next) -> Response {
    let key = client_key(limiter.0.config.client_ip_header.as_ref(), &request);
    let response = next.run(request).await;
    // Headers (Content-Length, ranges) are untouched; only the body is paced.
    let (parts, body) = response.into_parts();
    let download = Arc::new(limiter.start(key));
    let pieces = body.into_data_stream().flat_map(|chunk| {
        stream::iter(match chunk {
            Ok(bytes) => split(bytes).into_iter().map(Ok).collect::<Vec<_>>(),
            Err(error) => vec![Err(error)],
        })
    });
    let paced = pieces.then(move |piece| {
        let download = download.clone();
        async move {
            if let Ok(bytes) = &piece {
                let wait = download.debit(bytes.len());
                if !wait.is_zero() {
                    tokio::time::sleep(wait).await;
                }
            }
            piece
        }
    });
    Response::from_parts(parts, Body::from_stream(paced))
}

fn split(mut bytes: Bytes) -> Vec<Bytes> {
    let mut pieces = Vec::with_capacity(bytes.len().div_ceil(PIECE));
    while bytes.len() > PIECE {
        pieces.push(bytes.split_to(PIECE));
    }
    if !bytes.is_empty() {
        pieces.push(bytes);
    }
    pieces
}

#[cfg(test)]
mod tests {
    use super::*;
    use axum::{http::Request as HttpRequest, routing::get};
    use tower::ServiceExt;

    fn config(mbps: f64) -> Config {
        Config {
            bytes_per_second: mbps * 1_000_000.0 / 8.0,
            client_ip_header: Some(HeaderName::from_static("x-forwarded-for")),
        }
    }

    #[test]
    fn parses_configuration() {
        assert_eq!(Config::parse(None, None).unwrap(), None);
        assert_eq!(Config::parse(Some(" "), None).unwrap(), None);
        assert_eq!(Config::parse(Some("0"), None).unwrap(), None);
        let parsed = Config::parse(Some("80"), Some("CF-Connecting-IP")).unwrap().unwrap();
        assert_eq!(parsed.bytes_per_second, 10_000_000.0);
        assert_eq!(parsed.client_ip_header.unwrap().as_str(), "cf-connecting-ip");
        for invalid in ["-5", "fast", "NaN", "inf"] {
            assert!(Config::parse(Some(invalid), None).is_err(), "{invalid}");
        }
        assert!(Config::parse(Some("10"), Some("bad header")).is_err());
    }

    #[test]
    fn rate_is_shared_per_client_not_per_connection() {
        let limiter = Limiter::new(config(8.0));
        let a: Vec<_> = (0..30).map(|_| limiter.start("a".into())).collect();
        assert_eq!(limiter.share(), 1_000_000.0);
        let b = limiter.start("b".into());
        assert_eq!(limiter.share(), 500_000.0);
        drop(a);
        assert_eq!(limiter.share(), 1_000_000.0);
        drop(b);
        assert!(limiter.0.clients.is_empty());
    }

    #[test]
    fn debt_becomes_a_wait() {
        let limiter = Limiter::new(config(8.0)); // 1 MB/s
        let download = limiter.start("a".into());
        let wait = download.debit(500_000);
        assert!(wait > Duration::from_millis(450) && wait <= Duration::from_millis(500), "{wait:?}");
    }

    #[test]
    fn splits_into_pieces() {
        let sizes: Vec<_> = split(Bytes::from(vec![0; PIECE * 2 + 5])).iter().map(Bytes::len).collect();
        assert_eq!(sizes, [PIECE, PIECE, 5]);
        assert!(split(Bytes::new()).is_empty());
    }

    #[tokio::test]
    async fn paces_body_and_keeps_headers() {
        let body = vec![7u8; 200_000];
        let served = body.clone();
        let router = layer(
            Router::new().route(
                "/file",
                get(move || async move {
                    Response::builder()
                        .header("content-length", served.len())
                        .body(Body::from(served))
                        .unwrap()
                }),
            ),
            config(8.0), // 1 MB/s: 200 KB takes about 0.2 s after the burst
        );
        let start = Instant::now();
        let response = router
            .oneshot(HttpRequest::get("/file").header("x-forwarded-for", "1.2.3.4, 10.0.0.1").body(Body::empty()).unwrap())
            .await
            .unwrap();
        assert_eq!(response.headers()["content-length"], "200000");
        let bytes = axum::body::to_bytes(response.into_body(), usize::MAX).await.unwrap();
        assert_eq!(bytes.as_ref(), body.as_slice());
        let elapsed = start.elapsed();
        assert!(elapsed >= Duration::from_millis(150), "{elapsed:?}");
    }
}
