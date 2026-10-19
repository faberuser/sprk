//! Fetch exactly the client release pinned to this server image, before listening.
use anyhow::{ensure, Context, Result};
use base64::{engine::general_purpose::STANDARD, Engine};
use flate2::read::GzDecoder;
use serde::Deserialize;
use sha2::{Digest, Sha256};
use std::{
    collections::{HashMap, HashSet},
    fs::{self, File},
    io::{Read, Write},
    path::{Path, PathBuf},
    process::Command,
    time::Duration,
};
use tokio::io::AsyncWriteExt;

const MANIFEST_ASSET: &str = "client-update-manifest.json";
const ARCHIVE_ASSET: &str = "client-update.tar.gz";
const JSON_LIMIT: u64 = 4 * 1024 * 1024;
const ARCHIVE_LIMIT: u64 = 2 * 1024 * 1024 * 1024;

pub(super) struct Config {
    repo: String,
    tag: String,
    public_key: String,
}

impl Config {
    pub fn from_env() -> Result<Option<Self>> {
        let tag = std::env::var("SPRK_UPDATES_GITHUB_TAG").unwrap_or_default();
        if tag.is_empty() {
            return Ok(None);
        }
        let repo = std::env::var("SPRK_UPDATES_GITHUB_REPO")
            .context("SPRK_UPDATES_GITHUB_REPO is required with a pinned client release")?;
        let parts: Vec<_> = repo.split('/').collect();
        ensure!(
            parts.len() == 2 && parts.iter().all(|p| super::valid_identifier(p)),
            "Expected GitHub owner/repository"
        );
        ensure!(
            tag.strip_prefix("client-")
                .is_some_and(super::valid_identifier)
                && super::valid_identifier(&tag),
            "Client release tag must be client-<version>"
        );
        let public_key = match std::env::var("SPRK_UPDATES_PUBLIC_KEY")
            .ok()
            .filter(|key| !key.is_empty())
        {
            Some(key) => key,
            None => {
                let path = std::env::var("SPRK_UPDATES_PUBLIC_KEY_FILE")
                    .unwrap_or_else(|_| "config/client-update-public.pem".into());
                fs::read_to_string(path).context("Unable to read the client release public key")?
            }
        };
        ensure!(
            public_key.contains("-----BEGIN PUBLIC KEY-----"),
            "Expected an RSA public PEM key"
        );
        Ok(Some(Self {
            repo,
            tag,
            public_key,
        }))
    }
}

#[derive(Deserialize)]
struct Release {
    tag_name: String,
    draft: bool,
    prerelease: bool,
    assets: Vec<Asset>,
}

#[derive(Deserialize)]
struct Asset {
    name: String,
    browser_download_url: String,
    size: u64,
    state: String,
}

#[derive(Deserialize)]
struct Manifest {
    schema_version: u32,
    version: String,
    channel: String,
    published_at: chrono::DateTime<chrono::Utc>,
    files: Vec<ManagedFile>,
    deleted_files: Vec<String>,
}

#[derive(Deserialize)]
struct ManagedFile {
    path: String,
    size: u64,
    sha256: String,
    url: String,
}

// Removes only this attempt's staging directory; retention is handled separately.
struct Temporary(PathBuf);
impl Drop for Temporary {
    fn drop(&mut self) {
        let _ = fs::remove_dir_all(&self.0);
    }
}

fn openssl() -> PathBuf {
    let git = PathBuf::from("C:/Program Files/Git/usr/bin/openssl.exe");
    if cfg!(windows) && git.exists() {
        git
    } else {
        "openssl".into()
    }
}

fn verify_manifest(raw: &[u8], public_key: &str, scratch: &Path) -> Result<Manifest> {
    let outer: serde_json::Value = serde_json::from_slice(raw)?;
    let signature = &outer["signature"];
    ensure!(
        signature["algorithm"] == "RSA-SHA256",
        "Signed RSA-SHA256 manifest required"
    );
    let payload = STANDARD.decode(
        signature["payload"]
            .as_str()
            .context("Missing signed payload")?,
    )?;
    let value = STANDARD.decode(signature["value"].as_str().context("Missing signature")?)?;
    fs::write(scratch.join("public.pem"), public_key)?;
    fs::write(scratch.join("payload.json"), &payload)?;
    fs::write(scratch.join("signature.bin"), value)?;
    let output = Command::new(openssl())
        .current_dir(scratch)
        .args([
            "dgst",
            "-sha256",
            "-verify",
            "public.pem",
            "-signature",
            "signature.bin",
            "payload.json",
        ])
        .output()
        .context("OpenSSL is required to verify client releases")?;
    ensure!(
        output.status.success(),
        "Client manifest signature verification failed"
    );
    let signed: serde_json::Value = serde_json::from_slice(&payload)?;
    let mut convenience = outer.clone();
    convenience
        .as_object_mut()
        .context("Invalid manifest")?
        .remove("signature");
    ensure!(
        signed == convenience,
        "Manifest fields differ from the signed payload"
    );
    let manifest: Manifest = serde_json::from_value(signed)?;
    ensure!(
        manifest.schema_version == 1 && manifest.channel == "stable",
        "Expected schema 1 stable manifest"
    );
    ensure!(
        super::valid_identifier(&manifest.version),
        "Invalid release version"
    );
    ensure!(!manifest.files.is_empty(), "Empty client release");
    let mut paths = HashSet::new();
    for item in &manifest.files {
        ensure!(
            super::valid_file_path(&item.path) && paths.insert(item.path.to_lowercase()),
            "Unsafe or duplicate managed path"
        );
        ensure!(
            item.sha256.len() == 64
                && item
                    .sha256
                    .bytes()
                    .all(|c| c.is_ascii_hexdigit() && !c.is_ascii_uppercase()),
            "Invalid SHA-256"
        );
        let prefix = format!("/updates/releases/{}/files/", manifest.version);
        let decoded = urlencoding::decode(
            item.url
                .strip_prefix(&prefix)
                .context("Incorrect release file URL")?,
        )?;
        ensure!(decoded == item.path, "Release URL does not match its path");
    }
    for path in &manifest.deleted_files {
        ensure!(
            super::valid_file_path(path) && !paths.contains(&path.to_lowercase()),
            "Invalid deletion path"
        );
    }
    Ok(manifest)
}

async fn download(client: &reqwest::Client, url: &str, path: &Path, limit: u64) -> Result<()> {
    let mut response = client.get(url).send().await?.error_for_status()?;
    ensure!(
        response.content_length().unwrap_or(0) <= limit,
        "Download exceeds size limit"
    );
    let mut file = tokio::fs::File::create(path).await?;
    let mut size = 0u64;
    while let Some(chunk) = response.chunk().await? {
        size = size
            .checked_add(chunk.len() as u64)
            .context("Download too large")?;
        ensure!(size <= limit, "Download exceeds size limit");
        file.write_all(&chunk).await?;
    }
    file.sync_all().await?;
    Ok(())
}

fn asset<'a>(release: &'a Release, name: &str, repo: &str, tag: &str) -> Result<&'a Asset> {
    let found: Vec<_> = release
        .assets
        .iter()
        .filter(|a| a.name == name && a.state == "uploaded")
        .collect();
    ensure!(
        found.len() == 1,
        "Release needs exactly one uploaded {name}"
    );
    let asset = found[0];
    ensure!(
        asset.browser_download_url
            == format!("https://github.com/{repo}/releases/download/{tag}/{name}"),
        "Unexpected GitHub asset URL"
    );
    Ok(asset)
}

pub(super) async fn sync(root: PathBuf, config: Config) -> Result<()> {
    tracing::info!(repo = %config.repo, tag = %config.tag, "Checking pinned client release before startup");
    // The OS releases this lock even if startup is killed during a download.
    // Publishing happens on the workstation; do not run that publisher here.
    ensure!(
        !root.join(".publish-lock").exists(),
        "A client publisher lock exists on the sync host"
    );
    let lock = File::options()
        .read(true)
        .write(true)
        .create(true)
        .truncate(false)
        .open(root.join(".github-sync.lock"))?;
    lock.try_lock()
        .context("Another client startup sync is running")?;
    // Only this reserved directory is reused, under the exclusive OS lock.
    // A process killed mid-download must not leave an ever-growing set of blobs.
    let work = root.join(".github-sync-staging");
    if work.exists() {
        ensure!(
            fs::canonicalize(&work)? == work && fs::symlink_metadata(&work)?.is_dir(),
            "Unsafe sync staging directory"
        );
        fs::remove_dir_all(&work)?;
    }
    fs::create_dir(&work).context("Client update storage must be writable")?;
    let _work = Temporary(work.clone());
    let cached = root.join("releases").join(
        config
            .tag
            .strip_prefix("client-")
            .context("Invalid pinned tag")?,
    );
    if cached.exists() {
        let raw = fs::read(cached.join("manifest.json"))?;
        let manifest = verify_manifest(&raw, &config.public_key, &work)?;
        ensure!(
            config.tag == format!("client-{}", manifest.version),
            "Cached manifest does not match pinned tag"
        );
        verify_files(&cached, &manifest)?;
        promote(&root, &work, &raw)?;
        cleanup_after_activation(&root, &manifest.version, &config.public_key, &work);
        tracing::info!(version = %manifest.version, "Verified cached pinned client release; no download needed");
        return Ok(());
    }
    let client = reqwest::Client::builder()
        .user_agent("sprk-client-update-sync")
        .https_only(true)
        .connect_timeout(Duration::from_secs(15))
        .timeout(Duration::from_secs(900))
        .build()?;
    download(
        &client,
        &format!(
            "https://api.github.com/repos/{}/releases/tags/{}",
            config.repo, config.tag
        ),
        &work.join("github.json"),
        JSON_LIMIT,
    )
    .await?;
    let release: Release = serde_json::from_slice(&fs::read(work.join("github.json"))?)?;
    ensure!(
        !release.draft && !release.prerelease && release.tag_name == config.tag,
        "Expected published stable GitHub release"
    );
    let manifest_asset = asset(&release, MANIFEST_ASSET, &config.repo, &config.tag)?;
    let archive_asset = asset(&release, ARCHIVE_ASSET, &config.repo, &config.tag)?;
    download(
        &client,
        &manifest_asset.browser_download_url,
        &work.join("manifest.json"),
        JSON_LIMIT,
    )
    .await?;
    let raw = fs::read(work.join("manifest.json"))?;
    let manifest = verify_manifest(&raw, &config.public_key, &work)?;
    ensure!(
        config.tag == format!("client-{}", manifest.version),
        "GitHub tag does not match signed release version"
    );
    let destination = root.join("releases").join(&manifest.version);
    if destination.exists() {
        ensure!(
            fs::read(destination.join("manifest.json"))? == raw,
            "Existing immutable release differs from GitHub"
        );
        // Also detect corrupt or incomplete storage; don't trust a manifest alone.
        verify_files(&destination, &manifest)?;
    } else {
        ensure!(
            archive_asset.size <= ARCHIVE_LIMIT,
            "Release archive exceeds 2 GiB limit"
        );
        download(
            &client,
            &archive_asset.browser_download_url,
            &work.join(ARCHIVE_ASSET),
            archive_asset.size,
        )
        .await?;
        ensure!(
            fs::metadata(work.join(ARCHIVE_ASSET))?.len() == archive_asset.size,
            "Incomplete archive download"
        );
        unpack(
            &work.join(ARCHIVE_ASSET),
            &work.join("release"),
            &raw,
            &manifest,
        )?;
        fs::create_dir_all(root.join("releases"))?;
        fs::rename(work.join("release"), &destination)?;
    }
    promote(&root, &work, &raw)?;
    cleanup_after_activation(&root, &manifest.version, &config.public_key, &work);
    tracing::info!(version = %manifest.version, "Pinned client release verified and ready; starting update service");
    Ok(())
}

fn cleanup_after_activation(root: &Path, active: &str, public_key: &str, scratch: &Path) {
    // A cleanup failure must not take a successfully verified update offline.
    if let Err(error) = prune_releases(root, active, public_key, scratch) {
        tracing::warn!(%error, "Client release cleanup could not finish");
    }
}

fn prune_releases(root: &Path, active: &str, public_key: &str, scratch: &Path) -> Result<usize> {
    ensure!(super::valid_identifier(active), "Invalid active release");
    let releases = root.join("releases");
    ensure!(
        fs::canonicalize(&releases)? == releases,
        "Unsafe releases directory"
    );
    let active_path = releases.join(active);
    ensure!(
        fs::canonicalize(&active_path)? == active_path && active_path.is_dir(),
        "Unsafe active release directory"
    );
    let mut previous = Vec::new();
    for entry in fs::read_dir(&releases)? {
        let entry = entry?;
        let path = entry.path();
        let Some(version) = entry.file_name().to_str().map(str::to_owned) else {
            continue;
        };
        if version == active || !super::valid_identifier(&version) || !entry.file_type()?.is_dir() {
            continue;
        }
        // Never follow a symlink/junction or treat unrelated directories as releases.
        if fs::canonicalize(&path)? != path {
            continue;
        }
        let candidate = (|| -> Result<Manifest> {
            let manifest_path = path.join("manifest.json");
            ensure!(
                fs::canonicalize(&manifest_path)? == manifest_path,
                "Unsafe release manifest"
            );
            ensure!(
                fs::metadata(&manifest_path)?.len() <= JSON_LIMIT,
                "Release manifest too large"
            );
            let manifest = verify_manifest(&fs::read(manifest_path)?, public_key, scratch)?;
            ensure!(
                manifest.version == version,
                "Release folder does not match its signed manifest"
            );
            Ok(manifest)
        })();
        match candidate {
            Ok(manifest) => previous.push((manifest.published_at, version, path)),
            Err(error) => {
                tracing::warn!(release = %version, %error, "Leaving unrecognized release directory untouched")
            }
        }
    }
    // Release names contain commit hashes, so use authenticated publication time.
    previous.sort_by(|a, b| b.0.cmp(&a.0).then_with(|| b.1.cmp(&a.1)));
    let mut removed = 0;
    for (_, version, path) in previous.into_iter().skip(2) {
        // Recheck the absolute target immediately before recursive deletion.
        ensure!(
            path.parent() == Some(releases.as_path()) && fs::canonicalize(&path)? == path,
            "Unsafe cleanup target"
        );
        fs::remove_dir_all(&path)
            .with_context(|| format!("Unable to remove old release {version}"))?;
        removed += 1;
        tracing::info!(release = %version, "Removed old client release");
    }
    tracing::info!(active, removed, "Client release cleanup complete; retaining active and up to two most recent other releases");
    Ok(removed)
}

fn verify_files(root: &Path, manifest: &Manifest) -> Result<()> {
    for item in &manifest.files {
        let path = root.join("files").join(&item.path);
        ensure!(
            fs::canonicalize(&path)? == path,
            "Symlink in stored release"
        );
        let mut file = File::open(&path)?;
        ensure!(
            file.metadata()?.len() == item.size,
            "Incorrect size for {}",
            item.path
        );
        let mut digest = Sha256::new();
        let mut buffer = [0u8; 64 * 1024];
        loop {
            let size = file.read(&mut buffer)?;
            if size == 0 {
                break;
            }
            digest.update(&buffer[..size]);
        }
        ensure!(
            hex::encode(digest.finalize()) == item.sha256,
            "Incorrect hash for {}",
            item.path
        );
    }
    Ok(())
}

fn unpack(archive: &Path, destination: &Path, raw: &[u8], manifest: &Manifest) -> Result<()> {
    fs::create_dir(destination)?;
    let mut expected: HashMap<String, u64> = manifest
        .files
        .iter()
        .map(|f| (format!("files/{}", f.path), f.size))
        .collect();
    expected.insert("manifest.json".into(), raw.len() as u64);
    let mut archive = tar::Archive::new(GzDecoder::new(File::open(archive)?));
    for entry in archive.entries()? {
        let mut entry = entry?;
        ensure!(
            entry.header().entry_type().is_file(),
            "Only regular archive files are allowed"
        );
        let name = entry
            .path()?
            .to_str()
            .context("Non UTF-8 archive path")?
            .to_owned();
        ensure!(super::valid_file_path(&name), "Unsafe archive path");
        let size = expected
            .remove(&name)
            .context("Unexpected or duplicate archive file")?;
        ensure!(entry.size() == size, "Incorrect archive member size");
        let path = destination.join(&name);
        fs::create_dir_all(path.parent().context("Invalid member parent")?)?;
        let mut output = File::create(&path)?;
        std::io::copy(&mut entry, &mut output)?;
        output.sync_all()?;
    }
    ensure!(expected.is_empty(), "Archive is missing managed files");
    ensure!(
        fs::read(destination.join("manifest.json"))? == raw,
        "Archive manifest differs from signed asset"
    );
    verify_files(destination, manifest)
}

fn promote(root: &Path, work: &Path, raw: &[u8]) -> Result<()> {
    fs::create_dir_all(root.join("stable"))?;
    let pending = work.join("stable.json");
    let mut file = File::create(&pending)?;
    file.write_all(raw)?;
    file.sync_all()?;
    drop(file);
    // Windows rename cannot replace an existing file. Use the same atomic
    // replacement primitive as the Python publisher on that platform.
    #[cfg(windows)]
    {
        use std::os::windows::ffi::OsStrExt;
        #[link(name = "kernel32")]
        extern "system" {
            fn MoveFileExW(source: *const u16, target: *const u16, flags: u32) -> i32;
        }
        let source: Vec<_> = pending.as_os_str().encode_wide().chain(Some(0)).collect();
        let target: Vec<_> = root
            .join("stable/manifest.json")
            .as_os_str()
            .encode_wide()
            .chain(Some(0))
            .collect();
        if unsafe { MoveFileExW(source.as_ptr(), target.as_ptr(), 1 | 8) } == 0 {
            return Err(std::io::Error::last_os_error().into());
        }
    }
    #[cfg(not(windows))]
    fs::rename(pending, root.join("stable/manifest.json"))?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use serde_json::json;

    struct Fixture {
        root: PathBuf,
        public: String,
        raw: Vec<u8>,
    }

    impl Fixture {
        fn new() -> Self {
            let path =
                std::env::temp_dir().join(format!("sprk-github-test-{}", uuid::Uuid::new_v4()));
            fs::create_dir(&path).unwrap();
            let root = fs::canonicalize(path).unwrap();
            assert!(Command::new(openssl())
                .current_dir(&root)
                .args([
                    "genpkey",
                    "-algorithm",
                    "RSA",
                    "-pkeyopt",
                    "rsa_keygen_bits:2048",
                    "-out"
                ])
                .arg("test-private.pem")
                .output()
                .unwrap()
                .status
                .success());
            let public = Command::new(openssl())
                .current_dir(&root)
                .args(["pkey", "-pubout", "-in", "test-private.pem"])
                .output()
                .unwrap();
            assert!(public.status.success());
            let public = String::from_utf8(public.stdout).unwrap();
            let path = "King's Raid_Data/Managed/sprk.dll";
            let mut payload = json!({
                "schema_version": 1, "version": "test-1", "channel": "stable",
                "published_at": "2026-10-19T00:00:00Z",
                "files": [{"path": path, "size": 5, "sha256": hex::encode(Sha256::digest(b"hello")),
                    "url": "/updates/releases/test-1/files/King%27s%20Raid_Data/Managed/sprk.dll"}],
                "deleted_files": []
            });
            let bytes = serde_json::to_vec(&payload).unwrap();
            fs::write(root.join("test-payload"), &bytes).unwrap();
            let signature = Command::new(openssl())
                .current_dir(&root)
                .args([
                    "dgst",
                    "-sha256",
                    "-sign",
                    "test-private.pem",
                    "test-payload",
                ])
                .output()
                .unwrap();
            assert!(
                signature.status.success(),
                "{}",
                String::from_utf8_lossy(&signature.stderr)
            );
            payload["signature"] = json!({"algorithm":"RSA-SHA256", "payload":STANDARD.encode(bytes), "value":STANDARD.encode(signature.stdout)});
            let raw = serde_json::to_vec(&payload).unwrap();
            Self { root, public, raw }
        }

        fn manifest(&self) -> Manifest {
            verify_manifest(&self.raw, &self.public, &self.root).unwrap()
        }

        fn old_release(&self, version: &str, day: u32) -> PathBuf {
            let mut payload: serde_json::Value = serde_json::from_slice(&self.raw).unwrap();
            payload.as_object_mut().unwrap().remove("signature");
            payload["version"] = json!(version);
            payload["published_at"] = json!(format!("2026-10-{day:02}T00:00:00Z"));
            payload["files"][0]["url"] = json!(format!(
                "/updates/releases/{version}/files/King%27s%20Raid_Data/Managed/sprk.dll"
            ));
            let bytes = serde_json::to_vec(&payload).unwrap();
            fs::write(self.root.join("retention-payload"), &bytes).unwrap();
            let signature = Command::new(openssl())
                .current_dir(&self.root)
                .args([
                    "dgst",
                    "-sha256",
                    "-sign",
                    "test-private.pem",
                    "retention-payload",
                ])
                .output()
                .unwrap();
            assert!(signature.status.success());
            payload["signature"] = json!({"algorithm":"RSA-SHA256", "payload":STANDARD.encode(bytes), "value":STANDARD.encode(signature.stdout)});
            let directory = self.root.join("releases").join(version);
            fs::create_dir_all(&directory).unwrap();
            fs::write(
                directory.join("manifest.json"),
                serde_json::to_vec(&payload).unwrap(),
            )
            .unwrap();
            fs::write(directory.join("snapshot"), b"old release data").unwrap();
            directory
        }

        fn archive(&self, name: &str, content: &[u8], extra: Option<tar::EntryType>) -> PathBuf {
            let path = self.root.join(name);
            let writer = flate2::write::GzEncoder::new(
                File::create(&path).unwrap(),
                flate2::Compression::fast(),
            );
            let mut archive = tar::Builder::new(writer);
            for (name, data) in [
                ("manifest.json", self.raw.as_slice()),
                ("files/King's Raid_Data/Managed/sprk.dll", content),
            ] {
                let mut header = tar::Header::new_gnu();
                header.set_size(data.len() as u64);
                header.set_mode(0o644);
                header.set_cksum();
                archive.append_data(&mut header, name, data).unwrap();
            }
            if let Some(kind) = extra {
                let mut header = tar::Header::new_gnu();
                header.set_entry_type(kind);
                header.set_size(0);
                header.set_mode(0o644);
                if kind.is_symlink() {
                    header.set_link_name("/tmp/unrelated").unwrap();
                }
                header.set_cksum();
                archive
                    .append_data(&mut header, "unexpected", &b""[..])
                    .unwrap();
            }
            archive.into_inner().unwrap().finish().unwrap();
            path
        }
    }

    impl Drop for Fixture {
        fn drop(&mut self) {
            let _ = fs::remove_dir_all(&self.root);
        }
    }

    #[test]
    fn signed_bundle_roundtrip_and_atomic_manifest_replacement() {
        let f = Fixture::new();
        let manifest = f.manifest();
        let archive = f.archive("valid.tar.gz", b"hello", None);
        let destination = f.root.join("release");
        unpack(&archive, &destination, &f.raw, &manifest).unwrap();
        fs::create_dir(f.root.join("stable")).unwrap();
        fs::write(f.root.join("stable/manifest.json"), b"previous").unwrap();
        promote(&f.root, &f.root, &f.raw).unwrap();
        assert_eq!(
            fs::read(f.root.join("stable/manifest.json")).unwrap(),
            f.raw
        );
    }

    #[test]
    fn signature_and_unsigned_convenience_tampering_are_rejected() {
        let f = Fixture::new();
        let mut raw: serde_json::Value = serde_json::from_slice(&f.raw).unwrap();
        raw["version"] = json!("forged");
        assert!(verify_manifest(&serde_json::to_vec(&raw).unwrap(), &f.public, &f.root).is_err());
        raw = serde_json::from_slice(&f.raw).unwrap();
        raw["signature"]["value"] = json!(STANDARD.encode([0u8; 256]));
        assert!(verify_manifest(&serde_json::to_vec(&raw).unwrap(), &f.public, &f.root).is_err());
    }

    #[test]
    fn corrupt_extra_and_link_members_never_promote_stable() {
        let f = Fixture::new();
        let manifest = f.manifest();
        fs::create_dir(f.root.join("stable")).unwrap();
        fs::write(f.root.join("stable/manifest.json"), b"previous").unwrap();
        for (name, bytes, extra) in [
            ("corrupt", b"wrong".as_slice(), None),
            ("extra", b"hello".as_slice(), Some(tar::EntryType::Regular)),
            ("link", b"hello".as_slice(), Some(tar::EntryType::Symlink)),
        ] {
            let archive = f.archive(&format!("{name}.tar.gz"), bytes, extra);
            assert!(unpack(&archive, &f.root.join(name), &f.raw, &manifest).is_err());
            assert_eq!(
                fs::read(f.root.join("stable/manifest.json")).unwrap(),
                b"previous"
            );
        }
    }

    #[test]
    fn cleanup_keeps_active_even_on_rollback_and_two_newest_publication_dates() {
        let f = Fixture::new();
        let active = f.old_release("active-oldest", 1);
        let oldest = f.old_release("z-old", 2);
        let stale = f.old_release("b-stale", 3);
        let second = f.old_release("m-second", 4);
        let newest = f.old_release("a-newest", 5);
        assert_eq!(
            prune_releases(&f.root, "active-oldest", &f.public, &f.root).unwrap(),
            2
        );
        assert!(active.exists() && second.exists() && newest.exists());
        assert!(!oldest.exists() && !stale.exists());
        assert_eq!(
            prune_releases(&f.root, "active-oldest", &f.public, &f.root).unwrap(),
            0
        );
    }

    #[test]
    fn cleanup_leaves_unrecognized_folders_and_link_targets_untouched() {
        let f = Fixture::new();
        f.old_release("active", 5);
        f.old_release("previous-1", 4);
        f.old_release("previous-2", 3);
        let stale = f.old_release("stale", 2);
        let unrelated = f.root.join("releases/unrelated");
        fs::create_dir(&unrelated).unwrap();
        fs::write(unrelated.join("notes"), b"keep").unwrap();
        let invalid = f.old_release("invalid-signature", 1);
        let mut raw: serde_json::Value =
            serde_json::from_slice(&fs::read(invalid.join("manifest.json")).unwrap()).unwrap();
        raw["signature"]["value"] = json!(STANDARD.encode([0u8; 256]));
        fs::write(
            invalid.join("manifest.json"),
            serde_json::to_vec(&raw).unwrap(),
        )
        .unwrap();
        let outside = f.root.join("outside");
        fs::create_dir(&outside).unwrap();
        fs::write(outside.join("sentinel"), b"keep").unwrap();
        let linked = f.root.join("releases/linked");
        #[cfg(windows)]
        let link_result = std::os::windows::fs::symlink_dir(&outside, &linked);
        #[cfg(not(windows))]
        let link_result = std::os::unix::fs::symlink(&outside, &linked);
        assert_eq!(
            prune_releases(&f.root, "active", &f.public, &f.root).unwrap(),
            1
        );
        assert!(!stale.exists());
        assert!(
            unrelated.join("notes").exists()
                && invalid.exists()
                && outside.join("sentinel").exists()
        );
        if link_result.is_ok() {
            assert!(linked.exists());
        }
    }

    #[tokio::test]
    async fn cached_pin_starts_offline_and_corruption_fails_closed() {
        let f = Fixture::new();
        let archive = f.archive("valid.tar.gz", b"hello", None);
        fs::create_dir(f.root.join("releases")).unwrap();
        let release = f.root.join("releases/test-1");
        unpack(&archive, &release, &f.raw, &f.manifest()).unwrap();
        let config = || Config {
            repo: "does-not-exist/not-used".into(),
            tag: "client-test-1".into(),
            public_key: f.public.clone(),
        };
        fs::create_dir(f.root.join(".github-sync-staging")).unwrap();
        fs::write(
            f.root.join(".github-sync-staging/interrupted-download"),
            b"partial",
        )
        .unwrap();
        f.old_release("previous-1", 18);
        f.old_release("previous-2", 17);
        let stale = f.old_release("stale", 16);
        sync(f.root.clone(), config()).await.unwrap();
        assert!(!stale.exists());
        assert!(!f.root.join(".github-sync-staging").exists());
        assert!(!f.root.join(".publish-lock").exists());
        fs::write(
            release.join("files/King's Raid_Data/Managed/sprk.dll"),
            b"wrong",
        )
        .unwrap();
        let keep_on_failure = f.old_release("keep-on-failure", 15);
        assert!(sync(f.root.clone(), config()).await.is_err());
        assert!(keep_on_failure.exists());
        assert_eq!(
            fs::read(f.root.join("stable/manifest.json")).unwrap(),
            f.raw
        );
        assert!(!f.root.join(".publish-lock").exists());
    }

    #[tokio::test]
    async fn downloads_reject_http_errors_and_oversized_responses() {
        let f = Fixture::new();
        let router = axum::Router::new()
            .route("/ok", axum::routing::get(|| async { "hello" }))
            .route(
                "/fail",
                axum::routing::get(|| async { axum::http::StatusCode::SERVICE_UNAVAILABLE }),
            );
        let listener = tokio::net::TcpListener::bind("127.0.0.1:0").await.unwrap();
        let address = listener.local_addr().unwrap();
        let server = tokio::spawn(async move { axum::serve(listener, router).await.unwrap() });
        let client = reqwest::Client::new();
        let destination = f.root.join("download");
        download(&client, &format!("http://{address}/ok"), &destination, 5)
            .await
            .unwrap();
        assert_eq!(fs::read(&destination).unwrap(), b"hello");
        assert!(
            download(&client, &format!("http://{address}/ok"), &destination, 4)
                .await
                .is_err()
        );
        assert!(download(
            &client,
            &format!("http://{address}/fail"),
            &destination,
            100
        )
        .await
        .is_err());
        server.abort();
    }
}
