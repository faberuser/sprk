//! Public, read-only release downloads, independent of all game state.
use axum::{
    extract::{Path, Request, State},
    http::{header, HeaderValue, StatusCode},
    response::Response,
    routing::get,
    Router,
};
use std::{
    path::{Path as FsPath, PathBuf},
    sync::Arc,
};
use tower_http::services::ServeFile;

#[derive(Clone)]
struct UpdateStorage(Arc<PathBuf>);

pub async fn routes_from_env() -> anyhow::Result<Router> {
    let root = std::env::var("SPRK_UPDATES_DIR").unwrap_or_else(|_| "client-updates".into());
    routes(FsPath::new(&root)).await
}

pub async fn routes(root: &FsPath) -> anyhow::Result<Router> {
    use anyhow::Context;
    let root = tokio::fs::canonicalize(root).await.with_context(|| {
        format!(
            "SPRK_UPDATES_DIR must be an existing directory: {}",
            root.display()
        )
    })?;
    anyhow::ensure!(
        tokio::fs::metadata(&root).await?.is_dir(),
        "SPRK_UPDATES_DIR is not a directory"
    );
    tracing::info!(directory = %root.display(), "Serving client updates");
    Ok(Router::new()
        .route("/:channel/manifest.json", get(channel_manifest))
        .route("/releases/:release/manifest.json", get(release_manifest))
        .route("/releases/:release/files/*file_path", get(release_file))
        .fallback(|| async { StatusCode::NOT_FOUND })
        .with_state(UpdateStorage(Arc::new(root))))
}

fn valid_identifier(value: &str) -> bool {
    value.len() <= 128
        && value
            .as_bytes()
            .first()
            .is_some_and(u8::is_ascii_alphanumeric)
        && value
            .bytes()
            .all(|c| c.is_ascii_alphanumeric() || matches!(c, b'-' | b'_' | b'.'))
}

// Validate the decoded URL using rules that are safe on both Windows and Linux.
fn valid_file_path(value: &str) -> bool {
    !value.is_empty()
        && value.split('/').all(|part| {
            !part.is_empty()
                && part != "."
                && part != ".."
                && !part.starts_with('.')
                && !part.ends_with(['.', ' '])
                && !part.chars().any(|c| {
                    c.is_control() || matches!(c, '\\' | ':' | '<' | '>' | '"' | '|' | '?' | '*')
                })
        })
}

async fn channel_manifest(
    State(storage): State<UpdateStorage>,
    Path(channel): Path<String>,
    request: Request,
) -> Result<Response, StatusCode> {
    if !valid_identifier(&channel) || channel.eq_ignore_ascii_case("releases") {
        return Err(StatusCode::NOT_FOUND);
    }
    serve(
        &storage,
        FsPath::new(&channel).join("manifest.json"),
        request,
        false,
    )
    .await
}

async fn release_manifest(
    State(storage): State<UpdateStorage>,
    Path(release): Path<String>,
    request: Request,
) -> Result<Response, StatusCode> {
    if !valid_identifier(&release) {
        return Err(StatusCode::NOT_FOUND);
    }
    serve(
        &storage,
        FsPath::new("releases").join(release).join("manifest.json"),
        request,
        true,
    )
    .await
}

async fn release_file(
    State(storage): State<UpdateStorage>,
    Path((release, file_path)): Path<(String, String)>,
    request: Request,
) -> Result<Response, StatusCode> {
    if !valid_identifier(&release) || !valid_file_path(&file_path) {
        return Err(StatusCode::NOT_FOUND);
    }
    let path = FsPath::new("releases")
        .join(release)
        .join("files")
        .join(file_path);
    serve(&storage, path, request, true).await
}

async fn serve(
    storage: &UpdateStorage,
    relative: PathBuf,
    request: Request,
    immutable: bool,
) -> Result<Response, StatusCode> {
    let candidate = storage.0.join(relative);
    let path = match tokio::fs::canonicalize(candidate).await {
        Ok(path) => path,
        Err(error) => return Err(io_status(error)),
    };
    // Reject symlinks/junctions escaping the mounted release directory.
    if !path.starts_with(storage.0.as_ref()) {
        return Err(StatusCode::NOT_FOUND);
    }
    if !tokio::fs::metadata(&path)
        .await
        .map_err(io_status)?
        .is_file()
    {
        return Err(StatusCode::NOT_FOUND);
    }
    // ServeFile streams bounded chunks, and implements GET/HEAD, Range, and
    // conditional requests. Avoid compression: hashes refer to exact file bytes.
    let mut response = ServeFile::new(path)
        .try_call(request)
        .await
        .map_err(io_status)?
        .map(axum::body::Body::new);
    let cache_control = if immutable && response.status().is_success() {
        "public, max-age=31536000, immutable"
    } else {
        "no-store"
    };
    response.headers_mut().insert(
        header::CACHE_CONTROL,
        HeaderValue::from_static(cache_control),
    );
    response.headers_mut().insert(
        header::HeaderName::from_static("x-content-type-options"),
        HeaderValue::from_static("nosniff"),
    );
    Ok(response)
}

fn io_status(error: std::io::Error) -> StatusCode {
    match error.kind() {
        std::io::ErrorKind::NotFound | std::io::ErrorKind::PermissionDenied => {
            StatusCode::NOT_FOUND
        }
        _ => {
            tracing::error!(%error, "Unable to read client update file");
            StatusCode::INTERNAL_SERVER_ERROR
        }
    }
}

#[cfg(test)]
mod tests;
