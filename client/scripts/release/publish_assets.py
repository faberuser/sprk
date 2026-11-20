#!/usr/bin/env python3
"""Publish a client's game assets for the game's own patch downloader.

SprkLauncher (Windows) and the Android bootstrap update code: Managed DLLs,
tables, localization, video and client/overrides. Everything else in the patch
folder (asset bundles, PlainAsset, LoginPresentation, ...) is downloaded by the
game (NGameAsset.NPatch.PatchDownloader) from the update service, filtered by
quality tier and language. This tool stores each file once, by hash, and writes
a version the service serves under /updates/assets/<platform>/:

    <store>/objects/<sha256>                    file bytes, stored once
    <store>/<platform>/versions/<n>/patch.json  the game's patch list (Raw archives)
    <store>/<platform>/versions/<n>/index.json  archive name -> sha256
    <store>/<platform>/versions/<n>/set.json    path, size, sha256 (kept objects)
    <store>/<platform>/current.txt              active version, replaced last

Upload the store with sync_assets.py. The game server points clients at it when
SPRK_ASSETS_CDN_URL is set (docs/game-assets.md).
"""
from __future__ import annotations

import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import sys
import zlib

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_client_update import DEFAULT_INCLUDES, ignored

REPO = Path(__file__).resolve().parents[3]
PATCH_PREFIX = "King's Raid_Data/Documents/Patch/StandaloneWindows/"
# What every code release installs (the launcher or the Android bootstrap).
# Files a code release also carries (an overridden bundle) stay in the game's
# list with the same content: the bundle loader only loads files its patch list
# names, and a download of one rewrites the same bytes.
CODE_PATHS = tuple(DEFAULT_INCLUDES) + ("King's Raid_Data/sprk-privacy.json",)
# The launcher's own files and local tools are never part of the game.
EXCLUDED_CLIENT = {"sprklauncher.exe", "sprk-launcher.json", "sprk-launcher-error.log", "reset_position.ps1"}
# Relative to the patch folder: the game's own patch records (written by the game
# from the server's list), and data that changes together with the code
# (installed by code releases with its integrity metadata).
EXCLUDED_PATCH_PREFIXES = ("PatchInfo/", "TableData/", "TableJit/", "LocalizationJit/")
EXCLUDED_PATCH_NAMES = {"patch.json", "patch.bjs"}
PLATFORM = re.compile(r"[A-Za-z0-9][A-Za-z0-9_.-]{0,63}")
CHUNK = 1 << 20
MAX_LENGTH = 2**31 - 1  # AssetInfo.Length is an int on the client.


def valid_name(name: str) -> bool:
    """The update server's path rules (updates.rs valid_file_path)."""
    return bool(name) and all(
        part
        and part not in (".", "..")
        and not part.startswith(".")
        and not part.endswith((".", " "))
        and not any(ord(c) < 32 or c in '\\:<>"|?*' for c in part)
        for part in name.split("/")
    )


def excluded(name: str, patch_prefix: str) -> bool:
    if name.lower() in EXCLUDED_CLIENT or name.endswith(".part"):
        return True
    if not name.startswith(patch_prefix):
        return False
    inner = name[len(patch_prefix):]
    return (inner in EXCLUDED_PATCH_NAMES or inner.startswith(EXCLUDED_PATCH_PREFIXES)
            or (inner.startswith("SPRK-Native") and "/" not in inner))


def code_path(name: str) -> bool:
    return any(name == path or name.startswith(path + "/") for path in CODE_PATHS)


def original_metadata(path: Path | None) -> dict[str, dict]:
    """Download category/priority/time per entry from the game's original patch.json."""
    if path is None or not path.exists():
        return {}
    data = json.loads(path.read_text(encoding="utf-8"))
    metadata = {}
    for archive in data.get("Archives", []):
        for asset in archive.get("AssetInfos", []):
            metadata[asset["EntryName"]] = {
                "Cat": str(asset.get("Cat", archive.get("Cat", "0"))),
                "Pri": str(asset.get("Pri", archive.get("Pri", "0"))),
                "Time": str(asset.get("Time", "0")),
            }
    return metadata


def hash_file(path: Path) -> dict:
    md5, sha256, crc, length = hashlib.md5(), hashlib.sha256(), 0, 0
    with path.open("rb") as stream:
        while chunk := stream.read(CHUNK):
            md5.update(chunk)
            sha256.update(chunk)
            crc = zlib.crc32(chunk, crc)
            length += len(chunk)
    return {"md5": md5.hexdigest(), "sha256": sha256.hexdigest(), "crc": crc & 0xFFFFFFFF, "length": length}


def store_object(source: Path, objects: Path, sha256: str) -> bool:
    """Copy source into the object store under its hash. Returns True if added."""
    target = objects / sha256
    if target.exists():
        return False
    temporary = objects / f".{sha256}.{os.getpid()}.tmp"
    digest = hashlib.sha256()
    try:
        with source.open("rb") as reader, temporary.open("wb") as writer:
            while chunk := reader.read(CHUNK):
                digest.update(chunk)
                writer.write(chunk)
        if digest.hexdigest() != sha256:
            raise RuntimeError(f"{source} changed while publishing")
        os.replace(temporary, target)
    finally:
        temporary.unlink(missing_ok=True)
    return True


def collect(client: Path, patch_prefix: str, includes: list[str]) -> list[str]:
    names, invalid = [], []
    for path in sorted(client.rglob("*")):
        relative = path.relative_to(client)
        if relative.parts[0].startswith("."):
            continue  # .sprk-launcher and other tool state
        if path.is_symlink():
            raise ValueError(f"Symlink/junction in client: {path}")
        if not path.is_file() or ignored(path):
            continue
        name = relative.as_posix()
        if not name.startswith(patch_prefix) or excluded(name, patch_prefix) or code_path(name):
            continue
        if includes and not any(name.startswith(prefix) for prefix in includes):
            continue
        (names if valid_name(name) else invalid).append(name)
    if invalid:
        raise ValueError(f"File names the server cannot serve: {invalid[:5]}")
    return names


def existing_versions(platform_dir: Path) -> list[int]:
    versions = platform_dir / "versions"
    if not versions.is_dir():
        return []
    return sorted(int(p.name) for p in versions.iterdir() if p.is_dir() and p.name.isdigit())


def replace_file(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_name(f".{path.name}.{os.getpid()}.tmp")
    with temporary.open("wb") as writer:
        writer.write(data)
        writer.flush()
        os.fsync(writer.fileno())
    os.replace(temporary, path)


def encode_set(asset_set: dict) -> bytes:
    """One file per line, so versions compare as readable diffs."""
    lines = [json.dumps(item, ensure_ascii=False, separators=(",", ":")) for item in asset_set["files"]]
    header = {k: v for k, v in asset_set.items() if k != "files"}
    head = json.dumps(header, ensure_ascii=False, separators=(",", ":"))[:-1]
    return (head + ',"files":[\n' + ",\n".join(lines) + "\n]}\n").encode("utf-8")


def publish(
    client: Path,
    store: Path,
    platform: str,
    *,
    patch_prefix: str = PATCH_PREFIX,
    version: int | None = None,
    includes: list[str] | None = None,
    activate: bool = True,
    metadata_path: Path | None = None,
    workers: int = 4,
    log=print,
) -> dict:
    if not PLATFORM.fullmatch(platform) or platform == "objects":
        raise ValueError(f"Invalid platform: {platform}")
    client = client.resolve(strict=True)
    platform_dir = store / platform
    objects = store / "objects"
    objects.mkdir(parents=True, exist_ok=True)
    (platform_dir / "versions").mkdir(parents=True, exist_ok=True)
    lock = platform_dir / ".publish-lock"
    try:
        lock_fd = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    except FileExistsError:
        raise RuntimeError(f"Another publish is running (or crashed): {lock}") from None
    try:
        versions = existing_versions(platform_dir)
        version = version if version is not None else (versions[-1] + 1 if versions else 1)
        if version <= 0 or version in versions:
            raise ValueError(f"Version {version} is invalid or already published")
        names = collect(client, patch_prefix, includes or [])
        if not names:
            raise ValueError("No files selected")
        metadata = original_metadata(metadata_path or client / patch_prefix / "patch.json")
        log(f"Hashing {len(names)} files")
        with ThreadPoolExecutor(max_workers=workers) as pool:
            hashes = list(pool.map(lambda n: hash_file(client / n), names))
        archives, index, files, added, total = [], {}, [], 0, 0
        for name, info in zip(names, hashes):
            if store_object(client / name, objects, info["sha256"]):
                added += 1
            total += info["length"]
            files.append({"path": name, "size": info["length"], "sha256": info["sha256"]})
            if not name.startswith(patch_prefix):
                continue
            # The game's patch downloader addresses files relative to its patch folder.
            entry = name[len(patch_prefix):]
            if info["length"] > MAX_LENGTH:
                raise ValueError(f"{name} is larger than the game downloader supports")
            meta = metadata.get(entry, {"Cat": "0", "Pri": "0", "Time": "0"})
            common = {"Cat": meta["Cat"], "Pri": meta["Pri"], "Length": str(info["length"]),
                      "Md5": info["md5"], "Crc": str(info["crc"]), "Time": meta["Time"]}
            archives.append({
                "Name": entry, "SrcVersion": str(version), "Zip": "False", "ArchiveType": "Raw",
                "AssetInfos": [{"EntryName": entry, **common}], **common,
            })
            index[entry] = info["sha256"]
        asset_set = {"schema_version": 1, "platform": platform, "version": f"assets-{version}", "files": files}
        set_bytes = encode_set(asset_set)

        staging = platform_dir / "versions" / f".staging-{version}-{os.getpid()}"
        staging.mkdir()
        try:
            (staging / "set.json").write_bytes(set_bytes)
            (staging / "patch.json").write_text(
                json.dumps({"Archives": archives}, ensure_ascii=False, separators=(",", ":")), encoding="utf-8")
            (staging / "index.json").write_text(
                json.dumps({"files": index}, ensure_ascii=False, indent=0), encoding="utf-8")
            os.replace(staging, platform_dir / "versions" / str(version))
        except BaseException:
            shutil.rmtree(staging, ignore_errors=True)
            raise
        if activate:
            replace_file(platform_dir / "current.txt", str(version).encode("ascii"))
        log(f"Published {platform} asset set assets-{version}: {len(names)} files, "
            f"{total / 2**30:.2f} GiB, {added} new objects"
            + ("" if activate else " (not activated)"))
        return {"version": version, "files": len(names), "bytes": total, "new_objects": added}
    finally:
        os.close(lock_fd)
        lock.unlink(missing_ok=True)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--client", type=Path, default=Path("../sprk-client"), help="Client folder")
    parser.add_argument("--store", type=Path, default=Path("client-updates/assets"))
    parser.add_argument("--platform", default="StandaloneWindows", help="Platform key, as in host.json")
    parser.add_argument("--patch-prefix", default=PATCH_PREFIX,
                        help="Client-relative patch folder for the game's patch.json ('' = whole folder)")
    parser.add_argument("--version", type=int, help="Asset version (default: latest + 1)")
    parser.add_argument("--include", action="append", default=[], help="Only paths with this prefix (repeatable)")
    parser.add_argument("--no-activate", action="store_true", help="Publish without switching current.txt")
    parser.add_argument("--metadata", type=Path,
                        help="Original patch list for download categories (default: the patch folder's patch.json); "
                             "the Android set uses the Windows client's")
    args = parser.parse_args()
    publish(
        args.client,
        args.store,
        args.platform,
        patch_prefix=args.patch_prefix,
        version=args.version,
        includes=args.include,
        activate=not args.no_activate,
        metadata_path=args.metadata,
    )


if __name__ == "__main__":
    main()
