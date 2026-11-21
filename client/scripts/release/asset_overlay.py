#!/usr/bin/env python3
"""The asset overlay of a client release: the game assets the release carries, per platform.

A release carries the patch-folder files SPRK changed (client/overrides/files and the
baseline's own changes). The update service applies them to the asset store at startup
(server/src/updates/overlay.rs): when one differs from a platform's active asset
version, it writes the next version with those files replaced, so players get edited
assets without an upload (docs/game-assets.md).

* StandaloneWindows: the release's own files (from "release": the bytes are in the
  release, at patch_prefix + name).
* Android: every bundle among them converted (convert_bundles.py), BundlePack.bin and
  the platform-neutral data (PlainAsset, ...) as they are; the bytes are in
  client-asset-overlay.tar.gz at Android/<name>. Each names its
  Windows source; the service applies it only when that Windows file changed.

The overlay JSON is signed like the release manifest.
"""
from __future__ import annotations

from concurrent.futures import ProcessPoolExecutor
import json
import os
from pathlib import Path
import shutil
import sys
import tarfile

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_assets import PATCH_PREFIX, code_path, excluded, hash_file
from client.scripts.release.publish_client_update import safe_path, sign_manifest

REPO = Path(__file__).resolve().parents[3]
CONTENT = REPO / "client/android/HybridBootstrap/content"
SHADER_MAP = CONTENT / "shader-map.json"
OVERLAY_MANIFEST = "client-asset-overlay.json"
OVERLAY_ARCHIVE = "client-asset-overlay.tar.gz"
WINDOWS, ANDROID = "StandaloneWindows", "Android"
BUNDLES = "Assetbundle/"


def android_tools():
    sys.path.insert(0, str(CONTENT))
    import build_android_assets  # noqa: E402  (needs UnityPy: client/scripts/requirements-client.txt)
    return build_android_assets


def windows_files(client: Path, manifest: dict) -> list[dict]:
    """Release files the game's patch downloader serves (publish_assets.py's selection)."""
    files = []
    for item in manifest["files"]:
        path = item["path"]
        if not path.startswith(PATCH_PREFIX) or excluded(path, PATCH_PREFIX) or code_path(path):
            continue
        info = hash_file(client / safe_path(path))
        if (info["length"], info["sha256"]) != (item["size"], item["sha256"]):
            raise ValueError(f"Release file differs from its manifest: {path}")
        files.append({"name": path[len(PATCH_PREFIX):], "size": info["length"], "sha256": info["sha256"],
                      "md5": info["md5"], "crc": info["crc"]})
    return files


def android_files(client: Path, windows: list[dict], output: Path, shader_map: dict, jobs: int, log=print) -> list[dict]:
    tools = android_tools()
    plan = []  # (windows name, android name, converted)
    for item in windows:
        if not item["name"].startswith(BUNDLES):
            # Platform-neutral data (PlainAsset, ...): the Android set lists it too
            # (build_android_assets.py), so its changes reach that list as they are.
            plan.append((item["name"], item["name"], False))
            continue
        rel = item["name"][len(BUNDLES):]
        if rel.startswith(tools.SKIPPED_PREFIXES) or rel in tools.ANDROID_ONLY:
            continue
        plan.append((item["name"], item["name"], rel not in tools.NEUTRAL))
    todo = []
    for source, name, convert in plan:
        target = output / ANDROID / name
        if convert:
            todo.append((str(client / PATCH_PREFIX / source), str(target)))
        else:
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(client / PATCH_PREFIX / source, target)
    log(f"Converting {len(todo)} bundles for Android")
    failures = []
    if todo:
        arguments = (*zip(*todo), [shader_map] * len(todo))
        if jobs > 1:
            with ProcessPoolExecutor(max_workers=jobs) as pool:
                results = list(pool.map(tools.convert_one, *arguments))
        else:
            results = list(map(tools.convert_one, *arguments))
        failures = [(target, result) for target, result in results if isinstance(result, str)]
    if failures:
        raise ValueError(f"{len(failures)} bundles could not be converted, first: {failures[0]}")
    files = []
    for source, name, _ in plan:
        info = hash_file(output / ANDROID / name)
        files.append({"name": name, "size": info["length"], "sha256": info["sha256"], "md5": info["md5"],
                      "crc": info["crc"], "source": source})
    return files


def build(client: Path, manifest: dict, output: Path, signing_key: Path, shader_map: Path = SHADER_MAP,
          jobs: int | None = None, log=print) -> tuple[Path, Path]:
    """Write client-asset-overlay.json (signed) and client-asset-overlay.tar.gz into output."""
    output.mkdir(parents=True, exist_ok=True)
    staging = output / ".overlay"
    if staging.exists():
        raise ValueError(f"Overlay staging exists: {staging}")
    try:
        windows = windows_files(client, manifest)
        android = android_files(client, windows, staging, json.loads(shader_map.read_text(encoding="utf-8")),
                                jobs or max(1, os.cpu_count() or 1), log)
        payload = {
            "schema_version": 1,
            "kind": "asset-overlay",
            "release": manifest["version"],
            "platforms": {
                WINDOWS: {"from": "release", "patch_prefix": PATCH_PREFIX, "files": windows},
                ANDROID: {"from": "archive", "patch_prefix": "", "files": android},
            },
        }
        signed = sign_manifest(payload, signing_key)
        archive_path, manifest_path = output / OVERLAY_ARCHIVE, output / OVERLAY_MANIFEST
        with tarfile.open(archive_path, "w:gz", format=tarfile.PAX_FORMAT) as bundle:
            for item in android:
                path = staging / ANDROID / item["name"]
                member = tarfile.TarInfo(f"{ANDROID}/{item['name']}")
                member.size = item["size"]
                member.mode = 0o644
                with path.open("rb") as source:
                    bundle.addfile(member, source)
        if archive_path.stat().st_size > 2 * 1024**3:
            raise ValueError("Overlay archive exceeds startup sync's 2 GiB limit")
        manifest_path.write_text(json.dumps(signed, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        log(f"Asset overlay: {len(windows)} Windows files, {len(android)} Android files "
            f"({archive_path.stat().st_size / 2**20:.1f} MiB archive)")
        return archive_path, manifest_path
    finally:
        shutil.rmtree(staging, ignore_errors=True)
