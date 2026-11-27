//! Download queue: limits how many players download at once, and how many
//! connections each of them uses.
//!
//! The bandwidth limiter (`throttle`) splits the rate fairly but lets every
//! player in, and the game's patch downloader opens 30 connections per player
//! (the launcher 4). This queue keeps the server's load bounded instead:
//!
//! - `SPRK_UPDATES_MAX_CLIENTS`: players downloading at once. Requests from
//!   other players wait, first come first served, until one of them finishes.
//!   A player keeps its slot while it has a download open and for a few seconds
//!   after, so the files the game fetches one after another do not requeue it.
//! - `SPRK_UPDATES_MAX_CONNECTIONS_PER_CLIENT`: downloads one player has open at
//!   once; its other requests wait for one of them to end. With the bandwidth
//!   limiter on, a player's total speed stays the same.
//!
//! A waiting request receives no response yet, which neither client treats as
//! an error. The game, however, gives up on a file after 300 seconds (waiting
//! included) and stops the whole download after three failures, so a request
//! never waits longer than `SPRK_UPDATES_QUEUE_MAX_WAIT_SECONDS` (default 120)
//! for a slot: past that it is let in over the limit. A player whose request
//! was dropped keeps its place for a few seconds so its retry is not sent to
//! the back. Channel/release manifests and `LastBuildVersion.txt` are small
//! checks, not downloads, and are never queued.
//!
//! Players are told apart by IP address, as in the bandwidth limiter.
use super::throttle::{client_key, parse_client_ip_header};
use axum::{
    body::Body,
    extract::{Request, State},
    http::HeaderName,
    middleware::{self, Next},
    response::Response,
    Router,
};
use futures_util::StreamExt;
use std::{
    collections::{HashMap, VecDeque},
    sync::{Arc, Mutex},
    time::Duration,
};
use tokio::{
    sync::{OwnedSemaphorePermit, Semaphore},
    time::Instant,
};

/// How long a player keeps its slot after its last download ends.
const IDLE_GRACE: Duration = Duration::from_secs(5);
/// How long a player keeps its queue position with no request waiting.
const QUEUE_GRACE: Duration = Duration::from_secs(15);
/// How often a waiting request checks for a free slot.
const RECHECK: Duration = Duration::from_millis(500);
const DEFAULT_MAX_WAIT_SECONDS: u64 = 120;
/// Leaves the game time to transfer the file within its 300-second limit.
const MAX_MAX_WAIT_SECONDS: u64 = 240;

#[derive(Debug, Clone, PartialEq)]
pub(super) struct Config {
    pub max_clients: Option<usize>,
    pub max_connections_per_client: Option<usize>,
    pub max_wait: Duration,
    pub client_ip_header: Option<HeaderName>,
}

impl Config {
    pub fn from_env() -> anyhow::Result<Option<Self>> {
        let var = |name| std::env::var(name).ok();
        Self::parse(
            var("SPRK_UPDATES_MAX_CLIENTS").as_deref(),
            var("SPRK_UPDATES_MAX_CONNECTIONS_PER_CLIENT").as_deref(),
            var("SPRK_UPDATES_QUEUE_MAX_WAIT_SECONDS").as_deref(),
            var("SPRK_UPDATES_CLIENT_IP_HEADER").as_deref(),
        )
    }

    fn parse(
        clients: Option<&str>,
        connections: Option<&str>,
        max_wait: Option<&str>,
        header: Option<&str>,
    ) -> anyhow::Result<Option<Self>> {
        let max_clients = count("SPRK_UPDATES_MAX_CLIENTS", clients)?;
        let max_connections_per_client = count("SPRK_UPDATES_MAX_CONNECTIONS_PER_CLIENT", connections)?;
        let seconds = match max_wait.map(str::trim).filter(|v| !v.is_empty()) {
            None => DEFAULT_MAX_WAIT_SECONDS,
            Some(value) => value
                .parse()
                .ok()
                .filter(|v| (1..=MAX_MAX_WAIT_SECONDS).contains(v))
                .ok_or_else(|| {
                    anyhow::anyhow!(
                        "SPRK_UPDATES_QUEUE_MAX_WAIT_SECONDS must be 1 to {MAX_MAX_WAIT_SECONDS}, got {value:?}"
                    )
                })?,
        };
        let client_ip_header = parse_client_ip_header(header)?;
        if max_clients.is_none() && max_connections_per_client.is_none() {
            return Ok(None);
        }
        Ok(Some(Self {
            max_clients,
            max_connections_per_client,
            max_wait: Duration::from_secs(seconds),
            client_ip_header,
        }))
    }
}

/// A positive count; empty or `0` means no limit.
fn count(name: &str, value: Option<&str>) -> anyhow::Result<Option<usize>> {
    match value.map(str::trim).filter(|v| !v.is_empty() && *v != "0") {
        None => Ok(None),
        Some(value) => value
            .parse()
            .map(Some)
            .map_err(|_| anyhow::anyhow!("{name} must be a whole number, got {value:?}")),
    }
}

pub(super) fn layer(router: Router, config: Config) -> Router {
    tracing::info!(
        max_clients = ?config.max_clients,
        max_connections_per_client = ?config.max_connections_per_client,
        max_wait_seconds = config.max_wait.as_secs(),
        client_ip_header = ?config.client_ip_header,
        "Queueing client update downloads"
    );
    router.layer(middleware::from_fn_with_state(Queue::new(config), queue))
}

#[derive(Clone)]
pub(super) struct Queue(Arc<Shared>);

struct Shared {
    config: Config,
    state: Mutex<QueueState>,
}

#[derive(Default)]
struct QueueState {
    active: HashMap<String, Active>,
    waiting: VecDeque<Waiting>,
    next_id: u64,
}

struct Active {
    /// Requests admitted and not yet finished, including those waiting for a
    /// connection.
    requests: usize,
    idle_since: Option<Instant>,
    connections: Option<Arc<Semaphore>>,
}

struct Waiting {
    /// Tells a later entry for the same player apart from this one.
    id: u64,
    key: String,
    /// Requests currently waiting; 0 while a dropped request may retry.
    requests: usize,
    last_seen: Instant,
}

impl QueueState {
    fn purge(&mut self, now: Instant) {
        self.active.retain(|_, active| {
            active.requests > 0 || active.idle_since.is_some_and(|t| now - t < IDLE_GRACE)
        });
        self.waiting
            .retain(|waiting| waiting.requests > 0 || now - waiting.last_seen < QUEUE_GRACE);
    }

    fn stop_waiting(&mut self, id: u64, now: Instant) {
        if let Some(waiting) = self.waiting.iter_mut().find(|w| w.id == id) {
            waiting.requests -= 1;
            waiting.last_seen = now;
        }
    }
}

/// A request in the queue; leaves it when dropped.
struct Waiter<'a> {
    queue: &'a Queue,
    key: &'a str,
    /// The queue entry this request is counted in.
    queued: Option<u64>,
}

impl Drop for Waiter<'_> {
    fn drop(&mut self) {
        if let Some(id) = self.queued {
            self.queue
                .0
                .state
                .lock()
                .unwrap()
                .stop_waiting(id, Instant::now());
        }
    }
}

/// An admitted request; holds the player's slot until dropped.
struct Admission {
    queue: Queue,
    key: String,
    permit: Option<OwnedSemaphorePermit>,
}

impl Drop for Admission {
    fn drop(&mut self) {
        self.permit.take();
        let mut state = self.queue.0.state.lock().unwrap();
        if let Some(active) = state.active.get_mut(&self.key) {
            active.requests -= 1;
            if active.requests == 0 {
                active.idle_since = Some(Instant::now());
            }
        }
    }
}

impl Queue {
    pub fn new(config: Config) -> Self {
        Self(Arc::new(Shared {
            config,
            state: Mutex::default(),
        }))
    }

    /// Waits for a slot, then for one of the player's connections.
    async fn admit(&self, key: String) -> Admission {
        let started = Instant::now();
        let mut waiter = Waiter {
            queue: self,
            key: &key,
            queued: None,
        };
        let connections = loop {
            if let Some(connections) = self.try_enter(&mut waiter, started) {
                break connections;
            }
            tokio::time::sleep(RECHECK).await;
        };
        drop(waiter);
        // Built before waiting, so a dropped request still releases its slot.
        let mut admission = Admission {
            queue: self.clone(),
            key,
            permit: None,
        };
        if let Some(connections) = connections {
            admission.permit = Some(connections.acquire_owned().await.expect("never closed"));
        }
        admission
    }

    /// Admits the request, or queues it and returns `None`.
    fn try_enter(&self, waiter: &mut Waiter, started: Instant) -> Option<Option<Arc<Semaphore>>> {
        let config = &self.0.config;
        let now = Instant::now();
        let mut state = self.0.state.lock().unwrap();
        state.purge(now);
        let key = waiter.key;
        if !state.active.contains_key(key) {
            let free = config
                .max_clients
                .map_or(usize::MAX, |max| max.saturating_sub(state.active.len()));
            let ahead = state
                .waiting
                .iter()
                .take_while(|w| w.key != key)
                .filter(|w| w.requests > 0)
                .count();
            let waited = now - started;
            if ahead >= free && waited < config.max_wait {
                match state.waiting.iter_mut().find(|w| w.key == key) {
                    Some(waiting) if waiter.queued == Some(waiting.id) => {}
                    Some(waiting) => {
                        waiting.requests += 1;
                        waiting.last_seen = now;
                        waiter.queued = Some(waiting.id);
                    }
                    None => {
                        let id = state.next_id;
                        state.next_id += 1;
                        state.waiting.push_back(Waiting {
                            id,
                            key: key.to_owned(),
                            requests: 1,
                            last_seen: now,
                        });
                        waiter.queued = Some(id);
                    }
                }
                return None;
            }
            if ahead >= free {
                tracing::warn!(
                    waited_seconds = waited.as_secs(),
                    active = state.active.len(),
                    "Download queue wait limit reached; admitting a player over the limit"
                );
            } else if waiter.queued.is_some() {
                tracing::debug!(
                    waited_ms = waited.as_millis() as u64,
                    still_waiting = state.waiting.iter().filter(|w| w.requests > 0).count() - 1,
                    "Player admitted from the download queue"
                );
            }
            // The player's other waiting requests are admitted with it.
            state.waiting.retain(|w| w.key != key);
            state.active.insert(
                key.to_owned(),
                Active {
                    requests: 0,
                    idle_since: None,
                    connections: config
                        .max_connections_per_client
                        .map(|n| Arc::new(Semaphore::new(n))),
                },
            );
        }
        waiter.queued = None;
        let active = state.active.get_mut(key).expect("just inserted");
        active.requests += 1;
        active.idle_since = None;
        Some(active.connections.clone())
    }
}

/// Manifests and the asset version are small checks, not downloads.
fn is_download(path: &str) -> bool {
    !(path.ends_with("/manifest.json") || path.ends_with("/LastBuildVersion.txt"))
}

async fn queue(State(queue): State<Queue>, request: Request, next: Next) -> Response {
    if !is_download(request.uri().path()) {
        return next.run(request).await;
    }
    let key = client_key(queue.0.config.client_ip_header.as_ref(), &request);
    let admission = queue.admit(key).await;
    let response = next.run(request).await;
    // The slot is held until the body has been sent or the client goes away.
    let (parts, body) = response.into_parts();
    let body = body.into_data_stream().map(move |chunk| {
        let _held = &admission;
        chunk
    });
    Response::from_parts(parts, Body::from_stream(body))
}

#[cfg(test)]
mod tests {
    use super::*;
    use axum::{http::Request as HttpRequest, routing::get};
    use tokio::task::JoinHandle;
    use tower::ServiceExt;

    fn config(clients: Option<usize>, connections: Option<usize>) -> Config {
        Config {
            max_clients: clients,
            max_connections_per_client: connections,
            max_wait: Duration::from_secs(120),
            client_ip_header: Some(HeaderName::from_static("x-forwarded-for")),
        }
    }

    fn spawn(queue: &Queue, key: &str) -> JoinHandle<Admission> {
        let (queue, key) = (queue.clone(), key.to_owned());
        tokio::spawn(async move { queue.admit(key).await })
    }

    /// Lets spawned requests run until they wait, `seconds` of paused time later.
    async fn pass(seconds: f64) {
        tokio::time::sleep(Duration::from_secs_f64(seconds)).await;
        for _ in 0..10 {
            tokio::task::yield_now().await;
        }
    }

    #[test]
    fn parses_configuration() {
        assert_eq!(Config::parse(None, None, None, None).unwrap(), None);
        assert_eq!(Config::parse(Some("0"), Some(" "), Some("60"), None).unwrap(), None);
        let parsed = Config::parse(Some("20"), Some("6"), None, Some("CF-Connecting-IP")).unwrap().unwrap();
        assert_eq!(parsed.max_clients, Some(20));
        assert_eq!(parsed.max_connections_per_client, Some(6));
        assert_eq!(parsed.max_wait, Duration::from_secs(120));
        assert_eq!(parsed.client_ip_header.unwrap().as_str(), "cf-connecting-ip");
        let parsed = Config::parse(None, Some("4"), Some("30"), None).unwrap().unwrap();
        assert_eq!((parsed.max_clients, parsed.max_wait), (None, Duration::from_secs(30)));
        for invalid in ["-1", "many", "1.5"] {
            assert!(Config::parse(Some(invalid), None, None, None).is_err(), "{invalid}");
            assert!(Config::parse(None, Some(invalid), None, None).is_err(), "{invalid}");
        }
        for invalid in ["0", "241", "soon"] {
            assert!(Config::parse(Some("5"), None, Some(invalid), None).is_err(), "{invalid}");
        }
    }

    #[test]
    fn queues_only_downloads() {
        assert!(is_download("/assets/StandaloneWindows/3/bundle.unity3d"));
        assert!(is_download("/releases/1.0.0/files/King's Raid_Data/Managed/Assembly-CSharp.dll"));
        assert!(is_download("/assets/StandaloneWindows/3/patch.json"));
        assert!(!is_download("/stable/manifest.json"));
        assert!(!is_download("/releases/1.0.0/manifest.json"));
        assert!(!is_download("/assets/StandaloneWindows/LastBuildVersion.txt"));
    }

    #[tokio::test(start_paused = true)]
    async fn waits_until_a_player_finishes() {
        let queue = Queue::new(config(Some(1), None));
        let a = queue.admit("a".into()).await;
        let also_a = queue.admit("a".into()).await; // a player's requests share its slot
        let b = spawn(&queue, "b");
        pass(30.0).await;
        assert!(!b.is_finished());
        drop(a);
        pass(10.0).await;
        assert!(!b.is_finished(), "a still has a download open");
        drop(also_a);
        pass(1.0).await;
        assert!(!b.is_finished(), "a keeps its slot between files");
        pass(IDLE_GRACE.as_secs_f64()).await;
        assert!(b.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn first_come_first_served() {
        let queue = Queue::new(config(Some(1), None));
        let a = queue.admit("a".into()).await;
        let b = spawn(&queue, "b");
        pass(1.0).await;
        let c = spawn(&queue, "c");
        pass(1.0).await;
        drop(a);
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        assert!(b.is_finished());
        assert!(!c.is_finished());
        drop(b.await.unwrap());
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        assert!(c.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn a_retry_keeps_its_place() {
        let queue = Queue::new(config(Some(1), None));
        let a = queue.admit("a".into()).await;
        let b = spawn(&queue, "b");
        pass(1.0).await;
        let c = spawn(&queue, "c");
        pass(1.0).await;
        b.abort(); // the request timed out; the client retries a second later
        pass(1.0).await;
        let b = spawn(&queue, "b");
        pass(1.0).await;
        drop(a);
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        assert!(b.is_finished());
        assert!(!c.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn an_abandoned_place_is_given_up() {
        let queue = Queue::new(config(Some(1), None));
        let a = queue.admit("a".into()).await;
        let b = spawn(&queue, "b");
        pass(1.0).await;
        let c = spawn(&queue, "c");
        pass(1.0).await;
        b.abort();
        pass(1.0).await;
        drop(a);
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        assert!(c.is_finished(), "nobody is waiting for b");
    }

    #[tokio::test(start_paused = true)]
    async fn never_waits_past_the_limit() {
        let queue = Queue::new(Config {
            max_wait: Duration::from_secs(10),
            ..config(Some(1), None)
        });
        let _a = queue.admit("a".into()).await;
        let b = spawn(&queue, "b");
        pass(9.0).await;
        assert!(!b.is_finished());
        pass(2.0).await;
        assert!(b.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn limits_connections_per_player() {
        let queue = Queue::new(config(None, Some(2)));
        let first = queue.admit("a".into()).await;
        let _second = queue.admit("a".into()).await;
        let third = spawn(&queue, "a");
        let _other = queue.admit("b".into()).await; // players have separate limits
        pass(60.0).await;
        assert!(!third.is_finished());
        drop(first);
        pass(0.0).await;
        assert!(third.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn a_dropped_request_releases_its_slot() {
        let queue = Queue::new(config(Some(1), Some(1)));
        let a = queue.admit("a".into()).await;
        let waiting_for_connection = spawn(&queue, "a");
        pass(1.0).await;
        waiting_for_connection.abort();
        drop(a);
        let b = spawn(&queue, "b");
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        assert!(b.is_finished());
    }

    #[tokio::test(start_paused = true)]
    async fn holds_the_slot_until_the_body_is_sent() {
        let router = layer(
            Router::new()
                .route("/file", get(|| async { "data" }))
                .route("/stable/manifest.json", get(|| async { "{}" })),
            config(Some(1), None),
        );
        let request = |path: &str, ip: &str| {
            HttpRequest::get(path)
                .header("x-forwarded-for", ip)
                .body(Body::empty())
                .unwrap()
        };
        let a = router.clone().oneshot(request("/file", "1.1.1.1")).await.unwrap();
        let b = {
            let router = router.clone();
            tokio::spawn(async move { router.oneshot(request("/file", "2.2.2.2")).await.unwrap() })
        };
        pass(30.0).await;
        assert!(!b.is_finished());
        // Manifests are never queued.
        let manifest = router.clone().oneshot(request("/stable/manifest.json", "3.3.3.3")).await.unwrap();
        assert_eq!(axum::body::to_bytes(manifest.into_body(), usize::MAX).await.unwrap(), "{}");
        assert_eq!(axum::body::to_bytes(a.into_body(), usize::MAX).await.unwrap(), "data");
        pass(IDLE_GRACE.as_secs_f64() + 1.0).await;
        let b = b.await.unwrap();
        assert_eq!(axum::body::to_bytes(b.into_body(), usize::MAX).await.unwrap(), "data");
    }
}
