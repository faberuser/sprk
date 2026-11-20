#!/usr/bin/env python3
"""Copy a local asset store to the update server's storage, in a safe order.

    python client/scripts/release/sync_assets.py --store client-updates/assets --dest <SPRK_UPDATES_DIR>/assets

<dest> is the host's update storage (for example /srv/sprk/client-updates/assets
on the Docker host, or a network share mounted on the publishing machine).

1. `.sync-lock` is created; the update service skips asset cleanup meanwhile.
2. Missing objects are copied and verified against their hash.
3. Missing version folders are copied (staged, then renamed into place).
4. Each platform's current.txt is replaced.
5. The lock is removed.

Existing objects and versions are never overwritten. Players get the new
version at their next login. For a remote host without a mounted path, rsync in the
same order: objects/ first, then */versions/, then */current.txt.
"""
from __future__ import annotations

import argparse
import hashlib
import os
from pathlib import Path
import shutil
import sys

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.publish_assets import PLATFORM, replace_file

CHUNK = 1 << 20
SHA256_HEX = set("0123456789abcdef")


def copy_object(source: Path, target_dir: Path) -> bool:
    target = target_dir / source.name
    if target.exists():
        return False
    temporary = target_dir / f".{source.name}.{os.getpid()}.tmp"
    digest = hashlib.sha256()
    try:
        with source.open("rb") as reader, temporary.open("wb") as writer:
            while chunk := reader.read(CHUNK):
                digest.update(chunk)
                writer.write(chunk)
            writer.flush()
            os.fsync(writer.fileno())
        if digest.hexdigest() != source.name:
            raise ValueError(f"Object does not match its hash: {source}")
        os.replace(temporary, target)
    finally:
        temporary.unlink(missing_ok=True)
    return True


def sync(store: Path, dest: Path, log=print) -> dict:
    store = store.resolve(strict=True)
    dest.mkdir(parents=True, exist_ok=True)
    lock = dest / ".sync-lock"
    try:
        lock_fd = os.open(lock, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    except FileExistsError:
        raise RuntimeError(f"Another sync is running (or crashed): {lock}") from None
    try:
        objects = dest / "objects"
        objects.mkdir(exist_ok=True)
        sources = [p for p in (store / "objects").iterdir()
                   if p.is_file() and len(p.name) == 64 and set(p.name) <= SHA256_HEX]
        added = sum(copy_object(p, objects) for p in sources)
        log(f"Objects: {added} copied, {len(sources) - added} already present")
        versions_added = 0
        platforms = [p for p in store.iterdir() if p.is_dir() and p.name != "objects" and PLATFORM.fullmatch(p.name)]
        for platform in platforms:
            target_versions = dest / platform.name / "versions"
            target_versions.mkdir(parents=True, exist_ok=True)
            for version in sorted((platform / "versions").iterdir() if (platform / "versions").is_dir() else []):
                if not version.is_dir() or not version.name.isdigit() or (target_versions / version.name).exists():
                    continue
                staging = target_versions / f".staging-{version.name}-{os.getpid()}"
                shutil.copytree(version, staging)
                os.replace(staging, target_versions / version.name)
                versions_added += 1
            current = platform / "current.txt"
            if current.is_file():
                replace_file(dest / platform.name / "current.txt", current.read_bytes())
        log(f"Versions: {versions_added} copied")
        return {"objects": added, "versions": versions_added}
    finally:
        os.close(lock_fd)
        lock.unlink(missing_ok=True)


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--store", type=Path, default=Path("client-updates/assets"), help="Local asset store")
    parser.add_argument("--dest", type=Path, required=True, help="The server's <SPRK_UPDATES_DIR>/assets")
    args = parser.parse_args()
    if args.store.resolve() == args.dest.resolve():
        parser.exit(1, "Store and destination are the same folder\n")
    sync(args.store, args.dest)


if __name__ == "__main__":
    main()
