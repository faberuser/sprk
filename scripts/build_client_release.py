#!/usr/bin/env python3
"""Build a signed client release in an isolated workspace from pinned CI inputs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tarfile
import tempfile

from publish_client_update import IDENTIFIER, ignored, publish, safe_path
from client_update_bundle import package, verified_manifest
from client_baseline import BASELINE

REPO = Path(__file__).resolve().parent.parent


def file_hash(path):
    digest = hashlib.sha256()
    with path.open("rb") as source:
        for chunk in iter(lambda: source.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def extract_baseline(archive: Path, client: Path, sha256: str, public_key: Path) -> dict:
    if not re.fullmatch(r"[0-9a-f]{64}", sha256) or file_hash(archive) != sha256:
        raise ValueError("Baseline archive does not match its pinned SHA-256")
    if client.exists():
        raise ValueError("Client workspace must be fresh")
    client.mkdir(parents=True)
    with tarfile.open(archive) as bundle:
        members = bundle.getmembers()
        if len({member.name for member in members}) != len(members) or not all(m.isfile() for m in members):
            raise ValueError("Baseline must contain unique regular files only")
        metadata_entry = bundle.getmember("baseline.json")
        if metadata_entry.size > 8 * 1024**2:
            raise ValueError("Baseline metadata too large")
        metadata = json.load(bundle.extractfile(metadata_entry))
        if metadata["schema_version"] != 1:
            raise ValueError("Unsupported baseline schema")
        previous = metadata["previous_manifest"]
        verified_manifest(json.dumps(previous).encode(), public_key)
        expected = {}
        casefolded = set()
        for item in metadata["files"]:
            relative = safe_path(item["path"])
            if relative.lower() in casefolded:
                raise ValueError("Duplicate baseline file")
            casefolded.add(relative.lower())
            expected["files/" + relative] = item
        if set(expected) | {"baseline.json"} != {m.name for m in members}:
            raise ValueError("Baseline has missing or unexpected files")
        for name, item in expected.items():
            member = bundle.getmember(name)
            if member.size != item["size"]:
                raise ValueError("Incorrect baseline member size")
            target = client / item["path"]
            target.parent.mkdir(parents=True, exist_ok=True)
            with bundle.extractfile(member) as source, target.open("xb") as output:
                shutil.copyfileobj(source, output)
            if file_hash(target) != item["sha256"]:
                raise ValueError(f"Baseline file hash mismatch: {item['path']}")
        # Preserve every previously managed file. The baseline hash also pins
        # support data added from signed baked metadata at preparation time.
        for item in previous["files"]:
            seed = expected.get("files/" + item["path"])
            if not seed or (seed["sha256"], seed["size"]) != (item["sha256"], item["size"]):
                raise ValueError("Baseline does not preserve the signed distribution")
    return metadata


def apply_overrides(source: Path, client: Path) -> list[str]:
    added = []
    if source.exists():
        for path in sorted(source.rglob("*")):
            if path.resolve(strict=True) != path.absolute():
                raise ValueError("Symlink/junction in client overrides")
            if path.is_file():
                relative = safe_path(path.relative_to(source).as_posix())
                destination = client / relative
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(path, destination)
                added.append(relative)
    return added


def managed_build_files(client: Path) -> list[str]:
    """Select the complete prepared client, including newly generated assets."""
    return [p.relative_to(client).as_posix() for p in sorted(client.rglob("*"))
            if p.is_file() and not ignored(p)]


def previous_release(repo: str, destination: Path, public_key: Path) -> bytes | None:
    result = subprocess.run(["gh", "api", f"repos/{repo}/releases?per_page=100"], check=True, capture_output=True)
    releases = json.loads(result.stdout)
    candidates = [r for r in releases if not r["draft"] and not r["prerelease"]
                  and r["tag_name"].startswith("client-")
                  and any(a["name"] == "client-update-manifest.json" for a in r["assets"])]
    if not candidates:
        return None
    latest = max(candidates, key=lambda r: r["published_at"])
    destination.mkdir()
    subprocess.run(["gh", "release", "download", latest["tag_name"], "--repo", repo,
                    "--pattern", "client-update-manifest.json", "--dir", str(destination)], check=True)
    raw = (destination / "client-update-manifest.json").read_bytes()
    verified_manifest(raw, public_key)
    return raw


def build(baseline: Path, work: Path, version: str, signing_key: Path,
          public_key: Path, previous: bytes | None = None) -> tuple[Path, Path]:
    if not IDENTIFIER.fullmatch(version) or not IDENTIFIER.fullmatch("client-" + version):
        raise ValueError("Invalid client release version")
    if work.exists():
        raise ValueError("Build workspace already exists; choose a new path")
    work.mkdir(parents=True)
    client = work / "client"
    metadata = extract_baseline(baseline, client, BASELINE["sha256"], public_key)
    apply_overrides(REPO / "client-overrides/files", client)
    tools = work / "tools"
    subprocess.run(["dotnet", "build", str(REPO / "scripts/DllPatcher"), "--configuration", "Release", "--output", str(tools)], check=True)
    subprocess.run(["dotnet", str(tools / "DllPatcher.dll"), str(client), "--build-release"], check=True)
    output = work / "updates"
    (output / "stable").mkdir(parents=True)
    previous = previous or (json.dumps(metadata["previous_manifest"]) + "\n").encode()
    verified_manifest(previous, public_key)
    (output / "stable/manifest.json").write_bytes(previous)
    # The isolated client contains only baseline + override + patcher output.
    # Include new assets automatically, and let the publisher carry removals
    # forward when patcher code deletes an obsolete file.
    includes = managed_build_files(client)
    manifest = publish(client, output, version, "stable", includes, signing_key)
    release = output / "releases" / version
    if (release / "manifest.json").read_bytes() != (output / "stable/manifest.json").read_bytes():
        raise ValueError("Published manifests do not match")
    print(f"Verified new managed snapshot: {len(manifest['files'])} files; {len(manifest['deleted_files'])} deletion paths")
    return package(output, version, public_key, work / "assets")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", type=Path, help="Use a local archive instead of downloading")
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--version", required=True)
    parser.add_argument("--signing-key", type=Path, help="Local key; CI uses CLIENT_UPDATE_SIGNING_KEY environment secret")
    parser.add_argument("--public-key", type=Path, default=REPO / "config/client-update-public.pem")
    parser.add_argument("--repo", default="faberuser/sprk")
    parser.add_argument("--publish", action="store_true")
    args = parser.parse_args()
    # Do not propagate the private key's environment value to patcher processes.
    secret = os.environ.pop("CLIENT_UPDATE_SIGNING_KEY", None)
    with tempfile.TemporaryDirectory(prefix="sprk-client-ci-") as temporary:
        temporary = Path(temporary)
        key = args.signing_key
        if key is None:
            if not secret:
                parser.error("Provide --signing-key or the CLIENT_UPDATE_SIGNING_KEY environment secret")
            key = temporary / "signing.pem"
            key.write_text(secret, encoding="utf-8")
            key.chmod(0o600)
        baseline = args.baseline
        if baseline is None:
            pin = BASELINE
            subprocess.run(["gh", "release", "download", pin["tag"], "--repo", pin["repository"],
                            "--pattern", pin["asset"], "--dir", str(temporary)], check=True)
            baseline = temporary / pin["asset"]
        previous = previous_release(args.repo, temporary / "previous", args.public_key) if args.publish else None
        archive, manifest = build(baseline.resolve(strict=True), args.work.resolve(), args.version,
                                  key.resolve(strict=True), args.public_key.resolve(strict=True), previous)
        tag = "client-" + args.version
        if args.publish:
            subprocess.run(["gh", "release", "create", tag, str(archive), str(manifest), "--repo", args.repo,
                            "--target", os.environ["GITHUB_SHA"], "--draft", "--title", f"Client {args.version}",
                            "--notes", f"Built from server/patcher commit {os.environ['GITHUB_SHA']}."], check=True)
            subprocess.run(["gh", "release", "edit", tag, "--repo", args.repo, "--draft=false", "--latest=false"], check=True)
        if "GITHUB_OUTPUT" in os.environ:
            with open(os.environ["GITHUB_OUTPUT"], "a") as output:
                output.write(f"tag={tag}\n")
        print(f"Client release: {tag}")


if __name__ == "__main__":
    main()
