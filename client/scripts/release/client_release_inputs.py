"""Fingerprint client inputs and select only verified, complete releases for reuse."""
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile

from client.scripts.release.client_update_bundle import verified_manifest

REPO = Path(__file__).resolve().parents[3]
# Everything that can change the published client: source, asset overrides,
# the build and release tooling, the SDK pin and the deployment workflow.
# Git excludes generated/ignored binaries, workspaces and signing secrets.
INPUTS = ["client/managed", "client/localization", "client/overrides", "client/scripts/build", "client/scripts/release",
          "client/global.json", "config/client-update-public.pem",
          ".github/workflows/build-and-deploy.yml", ".gitattributes"]
REQUIRED_ASSETS = {"client-update-manifest.json", "client-update.tar.gz"}


def fingerprint(repo: Path = REPO) -> str:
    names = subprocess.check_output(
        ["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "--", *INPUTS], cwd=repo)
    digest = hashlib.sha256(b"sprk-client-build-inputs-v1\0")
    for name in sorted(set(names.decode("utf-8").split("\0")) - {""}):
        path = repo / name
        if not path.exists():
            continue  # A tracked deletion must change the fingerprint too.
        if path.is_symlink() or path.resolve(strict=True) != path.absolute():
            raise ValueError(f"Linked client build input: {name}")
        # Git checkouts on Windows and Linux may use different line endings.
        # Hash the exact bytes used by the build; selection and build share a checkout.
        payload = path.read_bytes()
        digest.update(name.encode("utf-8") + b"\0")
        digest.update(len(payload).to_bytes(8, "big"))
        digest.update(payload)
    return digest.hexdigest()


def complete(release: dict) -> bool:
    return (not release["draft"] and not release["prerelease"]
            and re.fullmatch(r"client-[A-Za-z0-9][A-Za-z0-9_.-]*", release["tag_name"]) is not None
            and REQUIRED_ASSETS <= {a["name"] for a in release["assets"] if a["state"] == "uploaded"})


def release_manifest(repo: str, release: dict, public_key: Path) -> dict:
    if not complete(release):
        raise ValueError("Expected a complete published client release")
    tag = release["tag_name"]
    with tempfile.TemporaryDirectory(prefix="sprk-reuse-") as folder:
        subprocess.run(["gh", "release", "download", tag, "--repo", repo,
                        "--pattern", "client-update-manifest.json", "--dir", folder], check=True)
        manifest = verified_manifest((Path(folder) / "client-update-manifest.json").read_bytes(), public_key)
    if tag != "client-" + manifest["version"]:
        raise ValueError("Tag differs from the signed manifest version")
    return manifest


def select(repo: str, public_key: Path, inputs_hash: str, tag: str = "", force: bool = False) -> str:
    if tag and (len(tag) > 128 or not re.fullmatch(r"client-[A-Za-z0-9][A-Za-z0-9_.-]*", tag)):
        raise ValueError("Expected a client-<version> release tag")
    if tag and force:
        raise ValueError("Cannot combine a release override with a forced client build")
    if tag:
        release = json.loads(subprocess.check_output(["gh", "api", f"repos/{repo}/releases/tags/{tag}"]))
        release_manifest(repo, release, public_key)
        return tag
    if force:
        return ""
    releases = json.loads(subprocess.check_output(["gh", "api", f"repos/{repo}/releases?per_page=100"]))
    candidates = [r for r in releases if complete(r)]
    if not candidates:
        return ""
    # Never roll back to an older matching release: the latest snapshot carries
    # cumulative deletion paths that must survive subsequent client updates.
    latest = max(candidates, key=lambda r: r["published_at"])
    manifest = release_manifest(repo, latest, public_key)
    return latest["tag_name"] if manifest.get("build_inputs_sha256") == inputs_hash else ""


def main():
    inputs_hash = fingerprint()
    tag = select(os.environ["RELEASE_REPOSITORY"], REPO / "config/client-update-public.pem", inputs_hash,
                 os.environ.get("INPUT_TAG", ""), os.environ.get("FORCE_CLIENT_BUILD", "false").lower() == "true")
    with open(os.environ["GITHUB_OUTPUT"], "a") as output:
        output.write(f"tag={tag}\n")
    print(f"Client inputs: {inputs_hash}; " + (f"reuse {tag}" if tag else "build a new signed release"))
