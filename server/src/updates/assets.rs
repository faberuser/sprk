//! Game asset downloads for the client's own patch downloader
//! (`NGameAsset.NPatch.PatchDownloader`), served from `<updates dir>/assets/`.
//!
//! The client is given `<base>/<platform>/` as its CDN URL and requests:
//!
//! - `LastBuildVersion.txt`: the active asset version (an integer)
//! - `<version>/patch.json`: the game's archive list for that version
//! - `<version>/<archive name>`: one archive (a raw file for `Raw` archives)
//!
//! Storage written by `client/scripts/release/publish_assets.py`:
//!
//! ```text
//! assets/objects/<sha256>                      file bytes, stored once
//! assets/<platform>/current.txt                active version
//! assets/<platform>/versions/<n>/patch.json    served to the client
//! assets/<platform>/versions/<n>/index.json    {"files": {archive name: sha256}}
//! ```
//!
//! Release manifests list pinned assets with the URL
//! `/updates/assets/objects/<sha256>`, so the Windows launcher installs them
//! with the code release; the game downloads nothing there.
//!
//! Objects and versions are immutable once written; publishing a version
//! adds new objects and replaces `current.txt` last.
use super::{io_status, serve, valid_file_path, valid_identifier, UpdateStorage};
use axum::{
    extract::{Path, Request, State},
    http::StatusCode,
    response::Response,
    routing::get,
    Router,
};
use std::{
    collections::HashMap,
    path::{Path as FsPath, PathBuf},
    sync::{Arc, Mutex},
};

#[derive(Clone)]
pub(super) struct AssetStorage {
    files: UpdateStorage,
    indexes: Arc<Mutex<HashMap<(String, u32), Arc<HashMap<String, String>>>>>,
}

/// Parsed indexes kept in memory; a server normally serves one or two versions.
const MAX_CACHED_INDEXES: usize = 4;

pub(super) fn routes(files: UpdateStorage) -> Router {
    Router::new()
        .route("/objects/{sha256}", get(object))
        .route("/{platform}/LastBuildVersion.txt", get(last_build_version))
        .route("/{platform}/{version}/{*archive}", get(archive))
        .fallback(|| async { StatusCode::NOT_FOUND })
        .with_state(AssetStorage {
            files,
            indexes: Arc::default(),
        })
}

fn parse_version(value: &str) -> Option<u32> {
    if value.is_empty() || value.len() > 9 || !value.bytes().all(|c| c.is_ascii_digit()) {
        return None;
    }
    value.parse().ok().filter(|v| *v > 0)
}

fn valid_sha256(value: &str) -> bool {
    value.len() == 64 && value.bytes().all(|c| matches!(c, b'0'..=b'9' | b'a'..=b'f'))
}

fn platform_dir(platform: &str) -> PathBuf {
    FsPath::new("assets").join(platform)
}

async fn object(
    State(storage): State<AssetStorage>,
    Path(sha256): Path<String>,
    request: Request,
) -> Result<Response, StatusCode> {
    if !valid_sha256(&sha256) {
        return Err(StatusCode::NOT_FOUND);
    }
    serve(
        &storage.files,
        FsPath::new("assets").join("objects").join(sha256),
        request,
        true,
    )
    .await
}

async fn last_build_version(
    State(storage): State<AssetStorage>,
    Path(platform): Path<String>,
    request: Request,
) -> Result<Response, StatusCode> {
    if !valid_identifier(&platform) || platform == "objects" {
        return Err(StatusCode::NOT_FOUND);
    }
    serve(
        &storage.files,
        platform_dir(&platform).join("current.txt"),
        request,
        false,
    )
    .await
}

async fn archive(
    State(storage): State<AssetStorage>,
    Path((platform, version, archive)): Path<(String, String, String)>,
    request: Request,
) -> Result<Response, StatusCode> {
    let version = parse_version(&version).ok_or(StatusCode::NOT_FOUND)?;
    if !valid_identifier(&platform) || platform == "objects" || !valid_file_path(&archive) {
        return Err(StatusCode::NOT_FOUND);
    }
    let version_dir = platform_dir(&platform)
        .join("versions")
        .join(version.to_string());
    if archive == "patch.json" {
        return serve(&storage.files, version_dir.join("patch.json"), request, true).await;
    }
    let index = storage.index(&platform, version, &version_dir).await?;
    let sha256 = index.get(&archive).ok_or(StatusCode::NOT_FOUND)?;
    serve(
        &storage.files,
        FsPath::new("assets").join("objects").join(sha256),
        request,
        true,
    )
    .await
}

impl AssetStorage {
    async fn index(
        &self,
        platform: &str,
        version: u32,
        version_dir: &FsPath,
    ) -> Result<Arc<HashMap<String, String>>, StatusCode> {
        let key = (platform.to_owned(), version);
        if let Some(index) = self.indexes.lock().unwrap().get(&key) {
            return Ok(index.clone());
        }
        let path = self.files.0.join(version_dir).join("index.json");
        let path = tokio::fs::canonicalize(path).await.map_err(io_status)?;
        if !path.starts_with(self.files.0.as_ref()) {
            return Err(StatusCode::NOT_FOUND);
        }
        let bytes = tokio::fs::read(&path).await.map_err(io_status)?;
        let index = parse_index(&bytes).map_err(|error| {
            tracing::error!(%error, path = %path.display(), "Invalid asset index");
            StatusCode::INTERNAL_SERVER_ERROR
        })?;
        let index = Arc::new(index);
        let mut cache = self.indexes.lock().unwrap();
        if cache.len() >= MAX_CACHED_INDEXES {
            cache.clear();
        }
        cache.insert(key, index.clone());
        Ok(index)
    }
}

fn parse_index(bytes: &[u8]) -> anyhow::Result<HashMap<String, String>> {
    #[derive(serde::Deserialize)]
    struct Index {
        files: HashMap<String, String>,
    }
    let index: Index = serde_json::from_slice(bytes)?;
    if let Some((name, _)) = index.files.iter().find(|(_, sha)| !valid_sha256(sha)) {
        anyhow::bail!("invalid sha256 for {name}");
    }
    Ok(index.files)
}
