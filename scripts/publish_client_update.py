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
    "King's Raid_Data/Managed",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/TableData",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/TableJit",
    "King's Raid_Data/Documents/Patch/StandaloneWindows/LocalizationJit",
]
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


def ignored(path: Path) -> bool:
    name = path.name.lower()
    return ".backup" in name or name.endswith((".patched", ".eye-patched", ".log", ".tmp", ".bak"))


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


def publish(client: Path, output: Path, version: str, channel: str, includes: list[str], signing_key: Path | None = None, deleted_files: list[str] | None = None) -> dict:
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
    selected = list(includes)
    if privacy:
        selected += privacy.get("required_files", []) + ["King's Raid_Data/sprk-privacy.json"]
    sources = collect_files(client, selected)
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
        previous_files = {safe_path(item["path"]) for item in previous.get("files", [])}
        removed = {safe_path(path) for path in previous.get("deleted_files", [])}
        explicit_removals = {safe_path(path) for path in (deleted_files or []) + privacy.get("deleted_files", [])}
        removed.update(explicit_removals)
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
            current_files = {item["path"] for item in files}
            if explicit_removals & current_files:
                raise ValueError("Explicit deletion also included in release: " + ", ".join(sorted(explicit_removals & current_files)))
            manifest = {
                "schema_version": 1,
                "version": version,
                "channel": channel,
                "published_at": datetime.now(timezone.utc).isoformat(),
                "files": files,
                "deleted_files": sorted((removed | (previous_files - current_files)) - current_files),
            }
            if signing_key is not None:
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
    args = parser.parse_args()
    try:
        manifest = publish(args.client, args.output, args.version, args.channel, args.include or DEFAULT_INCLUDES, args.signing_key, args.delete)
    except (OSError, ValueError, KeyError, TypeError, subprocess.CalledProcessError) as error:
        parser.exit(1, f"Publish failed: {error}\n")
    size = sum(item["size"] for item in manifest["files"])
    print(f"Published {args.version} to {args.channel}: {len(manifest['files'])} files, {size:,} bytes, {len(manifest['deleted_files'])} removed paths")
    print(f"Manifest: {args.output / args.channel / 'manifest.json'}")


if __name__ == "__main__":
    main()
