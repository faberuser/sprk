//! Apply the asset overlay of the pinned client release to the asset store at startup.
//!
//! CI attaches `client-asset-overlay.json` (signed) and `client-asset-overlay.tar.gz`
//! to each client release (client/scripts/release/asset_overlay.py). The overlay lists
//! the game assets the release carries, per platform: Windows files are the release's
//! own, Android files are bundles converted in CI. When one differs from a platform's
//! active asset version, the next version is written with those files replaced, so an
//! edited asset reaches players without uploading the store. An Android file is
//! applied only when its Windows source changed, so conversions made elsewhere (by
//! `client/android/dev.py assets`) are never replaced by equivalent ones.
//!
//! Failures never stop the update service: the active versions stay as they are.
use super::github::{self, Release};
use anyhow::{bail, ensure, Context, Result};
use flate2::read::GzDecoder;
use serde::Deserialize;
use sha2::{Digest, Sha256};
use std::{
    collections::{BTreeMap, HashMap, HashSet},
    fs::{self, File},
    io::{Read, Write},
    path::{Path, PathBuf},
};

pub(super) const OVERLAY_MANIFEST: &str = "client-asset-overlay.json";
pub(super) const OVERLAY_ARCHIVE: &str = "client-asset-overlay.tar.gz";

#[derive(Deserialize)]
pub(super) struct Overlay {
    schema_version: u32,
    kind: String,
    release: String,
    platforms: BTreeMap<String, Platform>,
}

#[derive(Deserialize)]
struct Platform {
    from: String,
    patch_prefix: String,
    files: Vec<Entry>,
}

#[derive(Deserialize, Clone)]
struct Entry {
    name: String,
    size: u64,
    sha256: String,
    md5: String,
    crc: u32,
    #[serde(default)]
    source: Option<String>,
}

fn is_hex(value: &str, length: usize) -> bool {
    value.len() == length && value.bytes().all(|c| c.is_ascii_digit() || (b'a'..=b'f').contains(&c))
}

/// Check an overlay's payload against the release it belongs to: `release` files must
/// be exactly the release's files (path -> size, sha256).
pub(super) fn parse(
    payload: serde_json::Value,
    version: &str,
    release_files: &HashMap<String, (u64, String)>,
) -> Result<Overlay> {
    let overlay: Overlay = serde_json::from_value(payload)?;
    ensure!(
        overlay.schema_version == 1 && overlay.kind == "asset-overlay",
        "Expected a schema 1 asset overlay"
    );
    ensure!(overlay.release == version, "Asset overlay belongs to another release");
    for (name, platform) in &overlay.platforms {
        ensure!(
            name != "objects" && super::valid_identifier(name),
            "Invalid overlay platform {name}"
        );
        ensure!(
            platform.patch_prefix.is_empty() || platform.patch_prefix.ends_with('/'),
            "Invalid patch prefix"
        );
        let mut names = HashSet::new();
        for entry in &platform.files {
            ensure!(
                super::valid_file_path(&entry.name) && names.insert(entry.name.to_lowercase()),
                "Unsafe or duplicate overlay file"
            );
            ensure!(
                is_hex(&entry.sha256, 64) && is_hex(&entry.md5, 32) && entry.size <= i32::MAX as u64,
                "Invalid overlay file {}",
                entry.name
            );
            match platform.from.as_str() {
                "release" => ensure!(
                    release_files.get(&format!("{}{}", platform.patch_prefix, entry.name))
                        == Some(&(entry.size, entry.sha256.clone())),
                    "Overlay file {} is not in the release",
                    entry.name
                ),
                "archive" => {}
                other => bail!("Unknown overlay source {other}"),
            }
        }
    }
    Ok(overlay)
}

/// What one platform needs: the files that differ from its active version.
struct Change<'a> {
    platform: &'a str,
    spec: &'a Platform,
    active: u32,
    files: Vec<&'a Entry>,
}

fn read_json(path: &Path) -> Result<serde_json::Value> {
    serde_json::from_slice(&fs::read(path)?).with_context(|| format!("Unreadable {}", path.display()))
}

fn active_version(platform_dir: &Path) -> Option<u32> {
    let text = fs::read_to_string(platform_dir.join("current.txt")).ok()?;
    let version: u32 = text.trim().parse().ok()?;
    platform_dir.join("versions").join(version.to_string()).is_dir().then_some(version)
}

fn active_index(platform_dir: &Path, version: u32) -> Result<HashMap<String, String>> {
    let index = read_json(&platform_dir.join("versions").join(version.to_string()).join("index.json"))?;
    index["files"]
        .as_object()
        .context("Invalid index.json")?
        .iter()
        .map(|(name, sha)| Ok((name.clone(), sha.as_str().context("Invalid index.json")?.to_owned())))
        .collect()
}

/// The files to write: release-sourced platforms first, then the others, whose
/// entries follow a release-sourced change of their `source`.
fn plan<'a>(assets: &Path, overlay: &'a Overlay) -> Result<Vec<Change<'a>>> {
    let mut changes = Vec::new();
    let mut changed_sources = HashSet::new();
    let ordered = overlay
        .platforms
        .iter()
        .filter(|(_, p)| p.from == "release")
        .chain(overlay.platforms.iter().filter(|(_, p)| p.from != "release"));
    for (name, spec) in ordered {
        let platform_dir = assets.join(name);
        let Some(active) = active_version(&platform_dir) else {
            tracing::info!(platform = %name, "No active asset version; asset overlay not applied");
            continue;
        };
        let index = active_index(&platform_dir, active)?;
        let files: Vec<_> = spec
            .files
            .iter()
            .filter(|entry| index.get(&entry.name) != Some(&entry.sha256))
            .filter(|entry| {
                spec.from == "release" || entry.source.as_ref().is_none_or(|s| changed_sources.contains(s))
            })
            .collect();
        if spec.from == "release" {
            changed_sources.extend(files.iter().map(|e| e.name.clone()));
        }
        if !files.is_empty() {
            changes.push(Change { platform: name, spec, active, files });
        }
    }
    Ok(changes)
}

/// Copy a file into the object store under its verified hash.
fn store_object(source: &Path, objects: &Path, entry: &Entry) -> Result<()> {
    let target = objects.join(&entry.sha256);
    if fs::metadata(&target).is_ok_and(|m| m.is_file() && m.len() == entry.size) {
        return Ok(());
    }
    let temporary = objects.join(format!(".{}.{}.tmp", entry.sha256, std::process::id()));
    let result = (|| {
        let mut reader = File::open(source)?;
        let mut writer = File::create(&temporary)?;
        let mut digest = Sha256::new();
        let mut buffer = vec![0u8; 1 << 20];
        let mut size = 0u64;
        loop {
            let read = reader.read(&mut buffer)?;
            if read == 0 {
                break;
            }
            digest.update(&buffer[..read]);
            writer.write_all(&buffer[..read])?;
            size += read as u64;
        }
        writer.sync_all()?;
        ensure!(
            size == entry.size && hex::encode(digest.finalize()) == entry.sha256,
            "{} does not match the overlay",
            entry.name
        );
        fs::rename(&temporary, &target)?;
        Ok(())
    })();
    if result.is_err() {
        let _ = fs::remove_file(&temporary);
    }
    result
}

fn archive_entry(entry: &Entry, version: u32, meta: Option<&serde_json::Value>) -> serde_json::Value {
    let field = |key: &str| {
        meta.and_then(|m| m.get(key)).and_then(|v| v.as_str()).unwrap_or("0").to_owned()
    };
    let common = serde_json::json!({
        "Cat": field("Cat"), "Pri": field("Pri"), "Time": field("Time"),
        "Length": entry.size.to_string(), "Md5": entry.md5, "Crc": entry.crc.to_string(),
    });
    let mut archive = common.clone();
    let object = archive.as_object_mut().expect("object");
    object.insert("Name".into(), entry.name.clone().into());
    object.insert("SrcVersion".into(), version.to_string().into());
    object.insert("Zip".into(), "False".into());
    object.insert("ArchiveType".into(), "Raw".into());
    let mut info = common;
    info.as_object_mut().expect("object").insert("EntryName".into(), entry.name.clone().into());
    object.insert("AssetInfos".into(), serde_json::json!([info]));
    archive
}

/// The active version with the changed files replaced, as the next version.
/// `metadata` gives download categories for files new to this platform (by source).
fn write_version(
    assets: &Path,
    change: &Change,
    source_of: &dyn Fn(&str, &Entry) -> PathBuf,
    metadata: &HashMap<String, serde_json::Value>,
) -> Result<u32> {
    let platform_dir = assets.join(change.platform);
    let lock = platform_dir.join(".publish-lock");
    File::options()
        .write(true)
        .create_new(true)
        .open(&lock)
        .context("An asset publish is running")?;
    struct Unlock(PathBuf);
    impl Drop for Unlock {
        fn drop(&mut self) {
            let _ = fs::remove_file(&self.0);
        }
    }
    let _unlock = Unlock(lock);

    let objects = assets.join("objects");
    fs::create_dir_all(&objects)?;
    for entry in &change.files {
        store_object(&source_of(change.platform, entry), &objects, entry)?;
    }
    let versions = platform_dir.join("versions");
    let active = versions.join(change.active.to_string());
    let next = fs::read_dir(&versions)?
        .filter_map(|e| e.ok()?.file_name().to_str()?.parse::<u32>().ok())
        .max()
        .unwrap_or(0)
        + 1;

    // One file at a time: a parsed patch list of ~33,000 files is about 100 MB, and the
    // update container's default limit is 256 MB.
    let staging = versions.join(format!(".staging-{next}-{}", std::process::id()));
    fs::create_dir(&staging)?;
    let written = (|| {
        let write = |name: &str, value: &serde_json::Value| -> Result<()> {
            let mut file = File::create(staging.join(name))?;
            serde_json::to_writer(std::io::BufWriter::new(&mut file), value)?;
            file.sync_all()?;
            Ok(())
        };
        let mut set = read_json(&active.join("set.json"))?;
        set["version"] = format!("assets-{next}").into();
        let set_files = set["files"].as_array_mut().context("Invalid set.json")?;
        let mut positions: HashMap<String, usize> = HashMap::new();
        for (position, item) in set_files.iter().enumerate() {
            positions.insert(item["path"].as_str().context("Invalid set.json")?.to_owned(), position);
        }
        for entry in &change.files {
            let path = format!("{}{}", change.spec.patch_prefix, entry.name);
            let item = serde_json::json!({"path": path, "size": entry.size, "sha256": entry.sha256});
            match positions.get(&path) {
                Some(&position) => set_files[position] = item,
                None => set_files.push(item),
            }
        }
        write("set.json", &set)?;
        drop(set);

        let mut patch = read_json(&active.join("patch.json"))?;
        let archives = patch["Archives"].as_array_mut().context("Invalid patch.json")?;
        let mut positions: HashMap<String, usize> = HashMap::new();
        for (position, item) in archives.iter().enumerate() {
            positions.insert(item["Name"].as_str().context("Invalid patch.json")?.to_owned(), position);
        }
        for entry in &change.files {
            match positions.get(&entry.name) {
                Some(&position) => {
                    let meta = archives[position].clone();
                    archives[position] = archive_entry(entry, next, Some(&meta));
                }
                None => {
                    let meta = metadata.get(entry.source.as_deref().unwrap_or(&entry.name));
                    archives.push(archive_entry(entry, next, meta));
                }
            }
        }
        write("patch.json", &patch)?;
        drop(patch);

        let mut index = read_json(&active.join("index.json"))?;
        let index_files = index["files"].as_object_mut().context("Invalid index.json")?;
        for entry in &change.files {
            index_files.insert(entry.name.clone(), entry.sha256.clone().into());
        }
        write("index.json", &index)?;
        fs::rename(&staging, versions.join(next.to_string()))?;
        Ok::<_, anyhow::Error>(())
    })();
    if let Err(error) = written {
        let _ = fs::remove_dir_all(&staging);
        return Err(error);
    }
    let pending = platform_dir.join(format!(".current.txt.{}.tmp", std::process::id()));
    fs::write(&pending, next.to_string())?;
    fs::rename(&pending, platform_dir.join("current.txt"))?;
    Ok(next)
}

/// Download categories (Cat, Pri, Time) of the release-sourced platforms' active
/// versions, for the sources of the files that `changes` adds.
fn release_metadata(assets: &Path, overlay: &Overlay, changes: &[Change]) -> HashMap<String, serde_json::Value> {
    let wanted: HashSet<&str> = changes
        .iter()
        .flat_map(|c| c.files.iter())
        .map(|e| e.source.as_deref().unwrap_or(&e.name))
        .collect();
    let mut metadata = HashMap::new();
    for (name, _) in overlay.platforms.iter().filter(|(_, p)| p.from == "release") {
        let platform_dir = assets.join(name);
        let Some(active) = active_version(&platform_dir) else { continue };
        let Ok(patch) = read_json(&platform_dir.join("versions").join(active.to_string()).join("patch.json")) else {
            continue;
        };
        for archive in patch["Archives"].as_array().into_iter().flatten() {
            if let Some(name) = archive["Name"].as_str().filter(|n| wanted.contains(n)) {
                let meta = serde_json::json!({"Cat": archive["Cat"], "Pri": archive["Pri"], "Time": archive["Time"]});
                metadata.insert(name.to_owned(), meta);
            }
        }
    }
    metadata
}

/// Unpack the overlay archive's files (`<platform>/<name>`) into `destination`.
fn unpack(archive: &Path, destination: &Path, overlay: &Overlay) -> Result<()> {
    let mut expected: HashMap<String, u64> = overlay
        .platforms
        .iter()
        .filter(|(_, p)| p.from == "archive")
        .flat_map(|(platform, p)| p.files.iter().map(move |e| (format!("{platform}/{}", e.name), e.size)))
        .collect();
    let mut tar = tar::Archive::new(GzDecoder::new(File::open(archive)?));
    for entry in tar.entries()? {
        let mut entry = entry?;
        ensure!(entry.header().entry_type().is_file(), "Only regular archive files are allowed");
        let name = entry.path()?.to_str().context("Non UTF-8 archive path")?.to_owned();
        ensure!(super::valid_file_path(&name), "Unsafe archive path");
        let size = expected.remove(&name).context("Unexpected or duplicate overlay file")?;
        ensure!(entry.size() == size, "Incorrect overlay member size");
        let path = destination.join(&name);
        fs::create_dir_all(path.parent().context("Invalid member parent")?)?;
        let mut output = File::create(&path)?;
        std::io::copy(&mut entry, &mut output)?;
    }
    ensure!(expected.is_empty(), "Overlay archive is missing files");
    Ok(())
}

fn verify_file(path: &Path, entry: &Entry) -> Result<()> {
    let mut file = File::open(path)?;
    let mut digest = Sha256::new();
    let mut buffer = vec![0u8; 1 << 20];
    loop {
        let read = file.read(&mut buffer)?;
        if read == 0 {
            break;
        }
        digest.update(&buffer[..read]);
    }
    ensure!(
        hex::encode(digest.finalize()) == entry.sha256,
        "{} does not match the overlay",
        entry.name
    );
    Ok(())
}

/// Plan and write; `archive` provides the unpacked overlay archive when needed.
pub(super) async fn apply<F, Fut>(root: &Path, release_dir: &Path, overlay: &Overlay, work: &Path, archive: F) -> Result<Vec<(String, u32)>>
where
    F: FnOnce(PathBuf) -> Fut,
    Fut: std::future::Future<Output = Result<()>>,
{
    let assets = root.join("assets");
    if !assets.is_dir() {
        tracing::info!("No asset store; asset overlay not applied");
        return Ok(Vec::new());
    }
    ensure!(!assets.join(".sync-lock").exists(), "An asset upload is in progress");
    let changes = plan(&assets, overlay)?;
    let unpacked = work.join("overlay");
    if changes.iter().any(|c| c.spec.from == "archive") {
        let path = work.join(OVERLAY_ARCHIVE);
        archive(path.clone()).await?;
        fs::create_dir_all(&unpacked)?;
        unpack(&path, &unpacked, overlay)?;
        for change in changes.iter().filter(|c| c.spec.from == "archive") {
            for entry in &change.files {
                verify_file(&unpacked.join(change.platform).join(&entry.name), entry)?;
            }
        }
    }
    let metadata = release_metadata(&assets, overlay, &changes);
    let source_of = |platform: &str, entry: &Entry| -> PathBuf {
        let spec = &overlay.platforms[platform];
        if spec.from == "release" {
            release_dir.join("files").join(format!("{}{}", spec.patch_prefix, entry.name))
        } else {
            unpacked.join(platform).join(&entry.name)
        }
    };
    // Platforms that follow a release-sourced one are written first: if writing stops
    // between them, the release-sourced change is still pending and is retried with
    // them at the next startup.
    let mut written = Vec::new();
    for change in changes.iter().rev() {
        let version = write_version(&assets, change, &source_of, &metadata)?;
        tracing::info!(platform = %change.platform, version, files = change.files.len(), "Asset overlay applied");
        written.push((change.platform.to_owned(), version));
    }
    if changes.is_empty() {
        tracing::info!("Asset overlay already matches the active asset versions");
    }
    Ok(written)
}

/// Fetch the pinned release's overlay and apply it. Releases built before overlays
/// existed have none; that is not an error.
pub(super) async fn sync(
    root: &Path,
    client: &reqwest::Client,
    release: &Release,
    repo: &str,
    tag: &str,
    public_key: &str,
    version: &str,
    release_files: &HashMap<String, (u64, String)>,
    work: &Path,
) -> Result<()> {
    if !release.assets.iter().any(|a| a.name == OVERLAY_MANIFEST) {
        tracing::info!("Client release has no asset overlay");
        return Ok(());
    }
    let manifest_asset = github::asset(release, OVERLAY_MANIFEST, repo, tag)?;
    let raw_path = work.join(OVERLAY_MANIFEST);
    github::fetch(client, &github::routes(manifest_asset, repo), &raw_path, github::JSON_LIMIT * 16, &github::RETRY_DELAYS).await?;
    let payload = github::verify_signed(&fs::read(&raw_path)?, public_key, work)?;
    let overlay = parse(payload, version, release_files)?;
    let archive_asset = github::asset(release, OVERLAY_ARCHIVE, repo, tag)?;
    ensure!(archive_asset.size <= github::ARCHIVE_LIMIT, "Overlay archive exceeds 2 GiB limit");
    let release_dir = root.join("releases").join(version);
    apply(root, &release_dir, &overlay, work, |path| async move {
        github::fetch(client, &github::routes(archive_asset, repo), &path, archive_asset.size, &github::RETRY_DELAYS).await?;
        ensure!(fs::metadata(&path)?.len() == archive_asset.size, "Incomplete overlay download");
        Ok(())
    })
    .await?;
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;
    use serde_json::json;

    const PREFIX: &str = "King's Raid_Data/Documents/Patch/StandaloneWindows/";

    struct Store(PathBuf);
    impl Drop for Store {
        fn drop(&mut self) {
            let _ = fs::remove_dir_all(&self.0);
        }
    }

    fn sha(data: &[u8]) -> String {
        hex::encode(Sha256::digest(data))
    }

    fn entry(name: &str, data: &[u8], source: Option<&str>) -> serde_json::Value {
        let mut value = json!({"name": name, "size": data.len(), "sha256": sha(data),
            "md5": format!("{:032x}", data.len()), "crc": 7});
        if let Some(source) = source {
            value["source"] = source.into();
        }
        value
    }

    /// A store with one active version per platform (A and B, Cat 3 for A), and a
    /// release holding the Windows files.
    fn fixture() -> (Store, PathBuf) {
        let root = std::env::temp_dir().join(format!("sprk-overlay-test-{}", uuid::Uuid::new_v4()));
        let store = Store(root.clone());
        for (platform, prefix, data) in [("StandaloneWindows", PREFIX, b"win" as &[u8]), ("Android", "", b"and")] {
            let version = root.join("assets").join(platform).join("versions/4");
            fs::create_dir_all(&version).unwrap();
            let names = ["Assetbundle/A.unity3d", "PlainAsset/B.txt"];
            let set: Vec<_> = names.iter().map(|n| json!({"path": format!("{prefix}{n}"), "size": 3, "sha256": sha(data)})).collect();
            let patch: Vec<_> = names.iter().map(|n| json!({"Name": n, "Cat": if n.ends_with("unity3d") { "3" } else { "0" },
                "Pri": "0", "Time": "0", "Length": "3", "Md5": "x", "Crc": "0", "SrcVersion": "4", "Zip": "False",
                "ArchiveType": "Raw", "AssetInfos": [{"EntryName": n}]})).collect();
            let index: serde_json::Map<_, _> = names.iter().map(|n| (n.to_string(), sha(data).into())).collect();
            fs::write(version.join("set.json"), json!({"version": "assets-4", "files": set}).to_string()).unwrap();
            fs::write(version.join("patch.json"), json!({"Archives": patch}).to_string()).unwrap();
            fs::write(version.join("index.json"), json!({"files": index}).to_string()).unwrap();
            fs::write(root.join("assets").join(platform).join("current.txt"), "4").unwrap();
        }
        let release = root.join("releases/r1");
        for (name, data) in [("Assetbundle/A.unity3d", b"win" as &[u8]), ("Assetbundle/New.unity3d", b"new"), ("PlainAsset/B.txt", b"changed")] {
            let path = release.join("files").join(format!("{PREFIX}{name}"));
            fs::create_dir_all(path.parent().unwrap()).unwrap();
            fs::write(path, data).unwrap();
        }
        (store, release)
    }

    fn overlay(release: &Path, android: Vec<serde_json::Value>) -> Overlay {
        let windows = vec![
            entry("Assetbundle/A.unity3d", b"win", None),
            entry("Assetbundle/New.unity3d", b"new", None),
            entry("PlainAsset/B.txt", b"changed", None),
        ];
        let mut files = HashMap::new();
        for item in &windows {
            let name = format!("{PREFIX}{}", item["name"].as_str().unwrap());
            let data = fs::read(release.join("files").join(&name)).unwrap();
            files.insert(name, (data.len() as u64, sha(&data)));
        }
        parse(json!({"schema_version": 1, "kind": "asset-overlay", "release": "r1", "platforms": {
            "StandaloneWindows": {"from": "release", "patch_prefix": PREFIX, "files": windows},
            "Android": {"from": "archive", "patch_prefix": "", "files": android},
        }}), "r1", &files).unwrap()
    }

    fn archive(path: &Path, files: &[(&str, &[u8])]) {
        let mut tar = tar::Builder::new(flate2::write::GzEncoder::new(File::create(path).unwrap(), Default::default()));
        for (name, data) in files {
            let mut header = tar::Header::new_gnu();
            header.set_size(data.len() as u64);
            header.set_mode(0o644);
            header.set_cksum();
            tar.append_data(&mut header, name, *data).unwrap();
        }
        tar.into_inner().unwrap().finish().unwrap();
    }

    fn version(root: &Path, platform: &str, n: u32, file: &str) -> serde_json::Value {
        read_json(&root.join("assets").join(platform).join("versions").join(n.to_string()).join(file)).unwrap()
    }

    #[tokio::test]
    async fn writes_next_versions_for_changed_files_only() {
        let (store, release) = fixture();
        let root = store.0.clone();
        // A.unity3d is unchanged on Windows, so its (different) Android conversion is
        // not applied; New.unity3d is new on both.
        let overlay = overlay(&release, vec![
            entry("Assetbundle/A.unity3d", b"converted-a", Some("Assetbundle/A.unity3d")),
            entry("Assetbundle/New.unity3d", b"converted-new", Some("Assetbundle/New.unity3d")),
        ]);
        let work = root.join("work");
        fs::create_dir(&work).unwrap();
        let written = apply(&root, &release, &overlay, &work, |path| async move {
            archive(&path, &[("Android/Assetbundle/A.unity3d", b"converted-a"), ("Android/Assetbundle/New.unity3d", b"converted-new")]);
            Ok(())
        })
        .await
        .unwrap();
        assert_eq!(written, vec![("Android".into(), 5), ("StandaloneWindows".into(), 5)]);
        let index = version(&root, "StandaloneWindows", 5, "index.json");
        assert_eq!(index["files"]["PlainAsset/B.txt"], sha(b"changed"));
        assert_eq!(index["files"]["Assetbundle/New.unity3d"], sha(b"new"));
        let android = version(&root, "Android", 5, "index.json");
        assert_eq!(android["files"]["Assetbundle/A.unity3d"], sha(b"and"));
        assert_eq!(android["files"]["Assetbundle/New.unity3d"], sha(b"converted-new"));
        let patch = version(&root, "StandaloneWindows", 5, "patch.json");
        let b = patch["Archives"].as_array().unwrap().iter().find(|a| a["Name"] == "PlainAsset/B.txt").unwrap();
        assert_eq!((b["Length"].as_str(), b["SrcVersion"].as_str(), b["AssetInfos"][0]["EntryName"].as_str()),
                   (Some("7"), Some("5"), Some("PlainAsset/B.txt")));
        let set = version(&root, "Android", 5, "set.json");
        assert_eq!(set["version"], "assets-5");
        assert_eq!(set["files"].as_array().unwrap().len(), 3);
        for (platform, n) in [("StandaloneWindows", "5"), ("Android", "5")] {
            assert_eq!(fs::read_to_string(root.join("assets").join(platform).join("current.txt")).unwrap(), n);
        }
        assert_eq!(fs::read(root.join("assets/objects").join(sha(b"converted-new"))).unwrap(), b"converted-new");
        assert!(!root.join("assets/StandaloneWindows/.publish-lock").exists());

        // Applying it again changes nothing and needs no archive.
        let again = apply(&root, &release, &overlay, &work, |_| async { bail!("not needed") }).await.unwrap();
        assert!(again.is_empty());
    }

    #[tokio::test]
    async fn new_android_files_take_the_windows_download_category() {
        let (store, release) = fixture();
        let root = store.0.clone();
        // Windows already has New.unity3d (Cat 3), Android does not.
        let windows_patch = root.join("assets/StandaloneWindows/versions/4/patch.json");
        let mut patch = read_json(&windows_patch).unwrap();
        patch["Archives"].as_array_mut().unwrap().push(json!({"Name": "Assetbundle/New.unity3d", "Cat": "3", "Pri": "2", "Time": "9"}));
        fs::write(&windows_patch, patch.to_string()).unwrap();
        let overlay = overlay(&release, vec![entry("Assetbundle/New.unity3d", b"converted-new", Some("Assetbundle/New.unity3d"))]);
        let work = root.join("work");
        fs::create_dir(&work).unwrap();
        apply(&root, &release, &overlay, &work, |path| async move {
            archive(&path, &[("Android/Assetbundle/New.unity3d", b"converted-new")]);
            Ok(())
        })
        .await
        .unwrap();
        let patch = version(&root, "Android", 5, "patch.json");
        let new = patch["Archives"].as_array().unwrap().iter().find(|a| a["Name"] == "Assetbundle/New.unity3d").unwrap();
        assert_eq!((new["Cat"].as_str(), new["Pri"].as_str(), new["Time"].as_str()), (Some("3"), Some("2"), Some("9")));
    }

    #[tokio::test]
    async fn a_corrupt_archive_file_leaves_the_active_versions() {
        let (store, release) = fixture();
        let root = store.0.clone();
        let overlay = overlay(&release, vec![entry("Assetbundle/New.unity3d", b"converted-new", Some("Assetbundle/New.unity3d"))]);
        let work = root.join("work");
        fs::create_dir(&work).unwrap();
        let result = apply(&root, &release, &overlay, &work, |path| async move {
            archive(&path, &[("Android/Assetbundle/New.unity3d", b"CONVERTED-NEW")]);
            Ok(())
        })
        .await;
        assert!(result.is_err());
        for platform in ["StandaloneWindows", "Android"] {
            let dir = root.join("assets").join(platform);
            assert_eq!(fs::read_to_string(dir.join("current.txt")).unwrap(), "4");
            assert!(!dir.join("versions/5").exists());
            assert!(!dir.join(".publish-lock").exists());
        }
    }

    #[test]
    fn rejects_release_files_the_release_does_not_have() {
        let payload = json!({"schema_version": 1, "kind": "asset-overlay", "release": "r1", "platforms": {
            "StandaloneWindows": {"from": "release", "patch_prefix": PREFIX, "files": [entry("PlainAsset/B.txt", b"x", None)]}}});
        assert!(parse(payload.clone(), "r1", &HashMap::new()).is_err());
        assert!(parse(payload, "r2", &HashMap::new()).is_err());
        let unsafe_name = json!({"schema_version": 1, "kind": "asset-overlay", "release": "r1", "platforms": {
            "Android": {"from": "archive", "patch_prefix": "", "files": [entry("../x", b"x", None)]}}});
        assert!(parse(unsafe_name, "r1", &HashMap::new()).is_err());
    }
}
