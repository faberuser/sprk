"""Shared signed-manifest verification and release packaging for client CI tools."""
import base64
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tarfile
import tempfile

from publish_client_update import IDENTIFIER, find_openssl, safe_path


def verified_manifest(raw: bytes, public_key: Path) -> dict:
    outer = json.loads(raw)
    signature = outer.get("signature", {})
    if signature.get("algorithm") != "RSA-SHA256":
        raise ValueError("A signed RSA-SHA256 release is required")
    payload = base64.b64decode(signature["payload"], validate=True)
    value = base64.b64decode(signature["value"], validate=True)
    with tempfile.TemporaryDirectory(prefix="sprk-verify-") as temporary:
        root = Path(temporary)
        (root / "payload").write_bytes(payload)
        (root / "signature").write_bytes(value)
        subprocess.run([
            find_openssl(), "dgst", "-sha256", "-verify", str(public_key.resolve(strict=True)),
            "-signature", str(root / "signature"), str(root / "payload"),
        ], check=True, capture_output=True)
    signed = json.loads(payload)
    if signed != {k: v for k, v in outer.items() if k != "signature"}:
        raise ValueError("Manifest fields differ from signed payload")
    if signed["schema_version"] != 1 or signed["channel"] != "stable":
        raise ValueError("Expected schema 1 stable release")
    if not IDENTIFIER.fullmatch(signed["version"]) or not signed["files"]:
        raise ValueError("Invalid or empty release")
    return signed


def package(root: Path, version: str, public_key: Path, output: Path) -> tuple[Path, Path]:
    if not IDENTIFIER.fullmatch(version):
        raise ValueError("Invalid release version")
    release = (root / "releases" / version).resolve(strict=True)
    raw = (release / "manifest.json").read_bytes()
    manifest = verified_manifest(raw, public_key)
    if manifest["version"] != version:
        raise ValueError("Release directory does not match signed version")
    # Validate everything before producing an uploadable archive.
    selected = []
    paths = set()
    for item in manifest["files"]:
        relative = safe_path(item["path"])
        if relative.lower() in paths:
            raise ValueError("Duplicate managed path")
        paths.add(relative.lower())
        path = release / "files" / relative
        if path.resolve(strict=True) != path:
            raise ValueError(f"Symlink/junction in release: {relative}")
        selected.append((path, item))
    for deleted in manifest["deleted_files"]:
        if safe_path(deleted).lower() in paths:
            raise ValueError("Deleted file is also managed")
    output.mkdir(parents=True, exist_ok=True)
    archive_path = output / "client-update.tar.gz"
    manifest_path = output / "client-update-manifest.json"
    with tempfile.TemporaryDirectory(prefix=".bundle-", dir=output) as temporary:
        staging = Path(temporary)
        archive = staging / archive_path.name
        with tarfile.open(archive, "w:gz", format=tarfile.PAX_FORMAT) as bundle:
            import io
            member = tarfile.TarInfo("manifest.json")
            member.size = len(raw)
            member.mode = 0o644
            bundle.addfile(member, io.BytesIO(raw))
            for path, item in selected:
                # Hash the exact open file that will be copied into the archive.
                with path.open("rb") as source:
                    before = path.stat()
                    digest = hashlib.sha256()
                    for chunk in iter(lambda: source.read(1024 * 1024), b""):
                        digest.update(chunk)
                    if before.st_size != item["size"] or digest.hexdigest() != item["sha256"]:
                        raise ValueError(f"Release size/hash mismatch: {item['path']}")
                    source.seek(0)
                    member = tarfile.TarInfo("files/" + item["path"])
                    member.size = item["size"]
                    member.mode = 0o644
                    bundle.addfile(member, source)
                    after = path.stat()
                    if (before.st_size, before.st_mtime_ns) != (after.st_size, after.st_mtime_ns):
                        raise ValueError(f"Release changed during packaging: {item['path']}")
        if archive.stat().st_size > 2 * 1024**3:
            raise ValueError("Archive exceeds startup sync's 2 GiB limit")
        (staging / manifest_path.name).write_bytes(raw)
        os.replace(archive, archive_path)
        os.replace(staging / manifest_path.name, manifest_path)
    return archive_path, manifest_path
