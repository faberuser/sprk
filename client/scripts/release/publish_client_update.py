#!/usr/bin/env python3
"""Publish a complete client file manifest and immutable release snapshot.

Requires Python 3.10+. Uses only the standard library; no game process is run.
"""
import argparse
import base64
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path, PurePosixPath
import re
import shutil
import subprocess
import tempfile
from urllib.parse import quote


DEFAULT_INCLUDES = [
    "King's Raid_Data/app.info",
    "King's Raid_Data/boot.config",
    "King's Raid_Data/globalgamemanagers",
    "King's Raid_Data/Managed",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/TableData",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/LocalizationJit",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/Video",
]
UNMANAGED_LIST = Path(__file__).with_name("unmanaged_files.txt")
# Asset set pinned by publish_assets.py: files stored once in the update
# server's object store and listed, not copied, in every release.
DEFAULT_ASSET_SET = Path(__file__).resolve().parents[2] / "assets" / "StandaloneWindows.json"
ASSET_URL_PREFIX = "/updates/assets/objects/"
SHA256 = re.compile(r"[0-9a-f]{64}\Z")
IDENTIFIER = re.compile(r"[A-Za-z0-9][A-Za-z0-9_.-]{0,127}\Z")


def safe_path(value: str) -> str:
    # Match the download server's cross-platform path restrictions.
    parts = value.split("/")
    if not value or any(
        not part or part.startswith(".") or part.endswith((".", " "))
        or any(ord(c) < 32 or ord(c) == 127 or c in '\\:<>"|?*' for c in part)
        for part in parts
    ):
        raise ValueError(f"Unsafe client-relative path: {value!r}")
    return PurePosixPath(value).as_posix()


def load_unmanaged(path: Path = UNMANAGED_LIST) -> list[str]:
    """Paths releases stop managing without deleting players' copies (see the list file)."""
    if not path.is_file():
        return []
    entries = []
    for line in path.read_text(encoding="utf-8").splitlines():
        line = line.strip()
        if not line or line.startswith("#"):
            continue
        folder = line.endswith("/")
        entries.append(safe_path(line.rstrip("/")) + ("/" if folder else ""))
    return entries


def is_unmanaged(path: str, unmanaged) -> bool:
    return any(path == entry or (entry.endswith("/") and path.startswith(entry)) for entry in unmanaged)


def ignored(path: Path) -> bool:
    name = path.name.lower()
    return ".backup" in name or ".before-" in name or ".before_" in name or name.endswith(
        (".patched", ".eye-patched", ".staged", "-staged", ".log", ".tmp", ".bak")
    )


def is_asset_entry(item: dict) -> bool:
    return item.get("url", "").startswith(ASSET_URL_PREFIX)


def load_asset_set(path: Path | None) -> tuple[str | None, list[dict]]:
    """The pinned asset set's version and files ({path, size, sha256})."""
    if path is None or not path.exists():
        return None, []
    data = json.loads(path.read_text(encoding="utf-8"))
    if data.get("schema_version") != 1 or not IDENTIFIER.fullmatch(data.get("version", "")):
        raise ValueError(f"Unsupported asset set: {path}")
    files, seen = [], set()
    for item in data["files"]:
        relative = safe_path(item["path"])
        if relative.lower() in seen or not SHA256.fullmatch(item["sha256"]) or type(item["size"]) is not int or item["size"] < 0:
            raise ValueError(f"Invalid asset set entry: {item['path']}")
        seen.add(relative.lower())
        files.append({"path": relative, "size": item["size"], "sha256": item["sha256"]})
    return data["version"], files


def collect_files(client: Path, includes: list[str]) -> list[Path]:
    selected = set()
    for include in includes:
        relative = safe_path(include.replace("\\", "/"))
        source = client / relative
        if not source.exists():
            raise ValueError(f"Included client path does not exist: {source}")
        candidates = [source] if source.is_file() else source.rglob("*")
        for candidate in candidates:
            # Do not follow symlinks or junctions into unrelated files.
            resolved = candidate.resolve(strict=True)
            if candidate.is_symlink() or resolved != candidate.absolute():
                raise ValueError(f"Symlink/junction in client update input: {candidate}")
            if candidate.is_file() and not ignored(candidate):
                safe_path(candidate.relative_to(client).as_posix())
                selected.add(candidate)
    if not selected:
        raise ValueError("No client files were selected")
    return sorted(selected)


def complete_snapshot_includes(client: Path, includes: list[str], previous_files=(), removals=()) -> list[str]:
    """With the default selection, keep managing every file the previous release managed.

    Releases also carry files outside DEFAULT_INCLUDES (asset bundles, Unity data
    files, integrity metadata). Dropping one from a later release would make the
    launcher delete it from players' installations, so they are carried forward
    while they still exist in the client. Use --delete to retire one on purpose.
    """
    selected = list(includes)
    if not set(DEFAULT_INCLUDES).issubset(selected):
        return selected
    for relative in sorted(previous_files):
        if relative in removals or not (client / relative).is_file():
            continue
        if not any(relative == path or relative.startswith(path + "/") for path in selected):
            selected.append(relative)
    return selected


def copy_and_hash(source: Path, destination: Path) -> tuple[int, str]:
    destination.parent.mkdir(parents=True, exist_ok=True)
    digest = hashlib.sha256()
    size = 0
    before = source.stat()
    with source.open("rb") as reader, destination.open("xb") as writer:
        for chunk in iter(lambda: reader.read(1024 * 1024), b""):
            writer.write(chunk)
            digest.update(chunk)
            size += len(chunk)
        writer.flush()
        os.fsync(writer.fileno())
    after = source.stat()
    if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns) or size != before.st_size:
        raise ValueError(f"Client file changed during publishing: {source}")
    return size, digest.hexdigest()


def find_openssl() -> str:
    executable = shutil.which("openssl")
    git_openssl = Path("C:/Program Files/Git/usr/bin/openssl.exe")
    if executable:
        return executable
    if git_openssl.exists():
        return str(git_openssl)
    raise ValueError("OpenSSL is required for signed releases; install it or add it to PATH")


def sign_manifest(manifest: dict, private_key: Path) -> dict:
    payload = (json.dumps(manifest, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    result = subprocess.run(
        [find_openssl(), "dgst", "-sha256", "-sign", str(private_key.resolve(strict=True))],
        input=payload, capture_output=True, check=True,
    )
    # Retain the convenience fields for publishing/removal bookkeeping. The
    # launcher verifies and uses only the authenticated payload when a key is pinned.
    return {**manifest, "signature": {
        "algorithm": "RSA-SHA256",
        "payload": base64.b64encode(payload).decode("ascii"),
        "value": base64.b64encode(result.stdout).decode("ascii"),
    }}


def publish(client: Path, output: Path, version: str, channel: str, includes: list[str], signing_key: Path | None = None, deleted_files: list[str] | None = None, build_inputs_sha256: str | None = None, unmanaged: list[str] | None = None, asset_set: Path | None = None, drop_assets: bool = False) -> dict:
    if not IDENTIFIER.fullmatch(version) or not IDENTIFIER.fullmatch(channel) or channel.lower() == "releases":
        raise ValueError("Version/channel must start with a letter or digit and contain only letters, digits, '.', '_', '-' (max 128 characters); channel cannot be 'releases'")
    client = client.resolve(strict=True)
    if not client.is_dir():
        raise ValueError("Client must be a directory")
    output = output.resolve()
    if output == client or output.is_relative_to(client):
        raise ValueError("Output directory must be outside the client directory")
    privacy_path = client / "King's Raid_Data/sprk-privacy.json"
    privacy = json.loads(privacy_path.read_text(encoding="utf-8")) if privacy_path.exists() else {}
    if privacy and privacy.get("schema_version") != 1:
        raise ValueError("Unsupported client privacy policy schema")
    unmanaged = load_unmanaged() if unmanaged is None else unmanaged
    output.mkdir(parents=True, exist_ok=True)
    # Serialize publishers so two releases cannot race to replace a channel.
    lock = output / ".publish-lock"
    try:
        lock.mkdir()
    except FileExistsError:
        raise ValueError(f"Another publisher is running (lock: {lock}). Remove the lock only if its publisher has stopped.") from None
    try:
        release = output / "releases" / version
        if release.exists():
            raise ValueError(f"Release {version} already exists; use a new version")
        channel_dir = output / channel
        previous_path = channel_dir / "manifest.json"
        previous = json.loads(previous_path.read_text(encoding="utf-8")) if previous_path.exists() else {}
        # Carry removals forward so clients can skip intermediate versions.
        previous_paths = {safe_path(item["path"]) for item in previous.get("files", [])}
        # Only code files are carried forward as files to copy; assets come
        # from the pinned asset set (and are deleted when it drops them).
        previous_files = {safe_path(item["path"]) for item in previous.get("files", []) if not is_asset_entry(item)}
        # Without the asset set, every previously listed asset would become a
        # deletion and launchers would remove the Unity player and bundles.
        if asset_set is None and not drop_assets and any(is_asset_entry(item) for item in previous.get("files", [])):
            raise ValueError("The previous release lists the asset set; pass it (client/assets/StandaloneWindows.json), "
                             "or --no-asset-set to delete those files from installations on purpose")
        removed = {safe_path(path) for path in previous.get("deleted_files", [])}
        explicit_removals = {safe_path(path) for path in (deleted_files or []) + privacy.get("deleted_files", [])}
        removed.update(explicit_removals)
        conflicts = sorted(path for path in explicit_removals if is_unmanaged(path, unmanaged))
        if conflicts:
            raise ValueError("Cannot both delete and stop managing: " + ", ".join(conflicts))
        selected = complete_snapshot_includes(client, includes, previous_files, explicit_removals)
        # Native table migrations update their integrity metadata alongside the JIT
        # files. Keep it in subsequent releases too, even when using default includes.
        for name in ("SPRK-NativeData.json", "SPRK-NativeCombat.json", "SPRK-NativeLocalization.json"):
            path = "King's Raid_Data/Documents/Patch/StandaloneWindows/" + name
            if (client / path).is_file():
                selected.append(path)
        if privacy:
            selected += privacy.get("required_files", []) + ["King's Raid_Data/sprk-privacy.json"]
        # Unmanaged files leave the release but are never marked deleted.
        sources = [source for source in collect_files(client, selected)
                   if not is_unmanaged(source.relative_to(client).as_posix(), unmanaged)]
        if not sources:
            raise ValueError("No client files were selected")
        with tempfile.TemporaryDirectory(prefix=".publish-", dir=output) as staging_name:
            staging = Path(staging_name)
            snapshot = staging / "release"
            snapshot.mkdir()
            files = []
            for source in sources:
                path = source.relative_to(client).as_posix()
                size, sha256 = copy_and_hash(source, snapshot / "files" / path)
                files.append({
                    "path": path, "size": size, "sha256": sha256,
                    "url": f"/updates/releases/{version}/files/{quote(path, safe='/')}",
                })
            asset_version, assets = load_asset_set(asset_set)
            code_paths = {path.lower() for path in (item["path"] for item in files)}
            # A file this release builds or overrides wins over the asset set.
            for item in assets:
                if item["path"].lower() in code_paths or item["path"] in explicit_removals or is_unmanaged(item["path"], unmanaged):
                    continue
                files.append({**item, "url": ASSET_URL_PREFIX + item["sha256"]})
            current_files = {item["path"] for item in files}
            if explicit_removals & current_files:
                raise ValueError("Explicit deletion also included in release: " + ", ".join(sorted(explicit_removals & current_files)))
            manifest = {
                "schema_version": 1,
                "version": version,
                "channel": channel,
                "published_at": datetime.now(timezone.utc).isoformat(),
                "files": files,
                "deleted_files": sorted(path for path in (removed | (previous_paths - current_files)) - current_files
                                        if not is_unmanaged(path, unmanaged)),
            }
            if asset_version is not None:
                manifest["asset_set"] = asset_version
            if signing_key is not None:
                if build_inputs_sha256 is not None:
                    if not re.fullmatch(r"[0-9a-f]{64}", build_inputs_sha256):
                        raise ValueError("Invalid client build input fingerprint")
                    manifest["build_inputs_sha256"] = build_inputs_sha256
                manifest = sign_manifest(manifest, signing_key)
            payload = (json.dumps(manifest, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
            (snapshot / "manifest.json").write_bytes(payload)
            release.parent.mkdir(parents=True, exist_ok=True)
            # The complete immutable release appears before the channel changes.
            snapshot.rename(release)
            channel_dir.mkdir(parents=True, exist_ok=True)
            pending = staging / "manifest.json"
            with pending.open("wb") as writer:
                writer.write(payload)
                writer.flush()
                os.fsync(writer.fileno())
            os.replace(pending, previous_path)
            return manifest
    finally:
        lock.rmdir()


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--client", type=Path, required=True, help="Patched client root")
    parser.add_argument("--output", type=Path, default=Path("client-updates"), help="SPRK_UPDATES_DIR directory")
    parser.add_argument("--version", required=True, help="New immutable release identifier")
    parser.add_argument("--channel", default="stable")
    parser.add_argument("--include", action="append", help="Client-relative file/directory; repeat to replace the default selection")
    parser.add_argument("--signing-key", type=Path, help="RSA private key PEM; use for hosted launcher updates")
    parser.add_argument("--delete", action="append", help="Client-relative obsolete file to remove, including files not previously managed; repeat as needed")
    parser.add_argument("--asset-set", type=Path, default=DEFAULT_ASSET_SET,
                        help="Pinned asset set listed in the release (default: client/assets/StandaloneWindows.json)")
    parser.add_argument("--no-asset-set", action="store_true",
                        help="Publish code files only; files the previous release listed from the asset set become deletions")
    parser.add_argument("--unmanaged-list", type=Path, default=UNMANAGED_LIST,
                        help="Files releases stop managing without deleting them (default: unmanaged_files.txt next to this script)")
    args = parser.parse_args()
    try:
        manifest = publish(args.client, args.output, args.version, args.channel, args.include or DEFAULT_INCLUDES, args.signing_key, args.delete,
                           unmanaged=load_unmanaged(args.unmanaged_list),
                           asset_set=None if args.no_asset_set else args.asset_set, drop_assets=args.no_asset_set)
    except (OSError, ValueError, KeyError, TypeError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Publish failed: {error}\n")
    code = [item for item in manifest["files"] if not is_asset_entry(item)]
    assets = [item for item in manifest["files"] if is_asset_entry(item)]
    size = sum(item["size"] for item in code)
    print(f"Published {args.version} to {args.channel}: {len(code)} files, {size:,} bytes, {len(manifest['deleted_files'])} removed paths")
    if assets:
        objects = args.output / "assets" / "objects"
        missing = sum(1 for item in assets if not (objects / item["sha256"]).is_file())
        print(f"Lists asset set {manifest.get('asset_set')}: {len(assets)} files, {sum(i['size'] for i in assets):,} bytes"
              + (f"; WARNING: {missing} objects are missing from {objects}" if missing else ""))
    print(f"Manifest: {args.output / args.channel / 'manifest.json'}")


if __name__ == "__main__":
    main()
