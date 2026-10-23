"""Prepare the pinned Unity runtime and signed client snapshot for Linux/Wine."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import tarfile
import tempfile

from client_update_bundle import verified_manifest
from publish_client_update import ignored, safe_path

REPO = Path(__file__).resolve().parent.parent
DATA = "King's Raid_Data"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def runtime_files(client):
    # Explicit engine/scene inputs only: never collect accounts, launcher
    # configuration, logs, saved games or the 18 GB downloaded asset cache.
    files = [client / "King's Raid.exe", client / "UnityPlayer.dll"]
    for directory in ("MonoBleedingEdge", DATA + "/Resources"):
        files.extend(p for p in (client / directory).rglob("*") if p.is_file() and not ignored(p))
    for path in (client / DATA).iterdir():
        if path.is_file() and (path.name in ("app.info", "RuntimeInitializeOnLoads.json", "ScriptingAssemblies.json")
                              or re.fullmatch(r"level\d+", path.name)
                              or re.fullmatch(r"(?:resources|sharedassets\d+|globalgamemanagers)\.(?:assets(?:\.resS)?|resource)", path.name)):
            files.append(path)
    return sorted(files)


def seed(client, archive, pin):
    client = client.resolve(strict=True)
    files = runtime_files(client)
    for path in files:
        if path.resolve(strict=True) != path:
            raise ValueError("Runtime inputs must not be links")
    archive.parent.mkdir(parents=True, exist_ok=True)
    with archive.open("xb") as output, gzip.GzipFile(fileobj=output, mode="wb", filename="", mtime=0) as compressed:
        with tarfile.open(fileobj=compressed, mode="w", format=tarfile.PAX_FORMAT) as bundle:
            for path in files:
                item = tarfile.TarInfo(safe_path(path.relative_to(client).as_posix()))
                item.size = path.stat().st_size
                item.mode = 0o644
                with path.open("rb") as source:
                    bundle.addfile(item, source)
    sha = digest(archive)
    value = {"repository": "faberuser/sprk", "tag": "worker-runtime-" + sha[:16],
             "asset": "worker-runtime.tar.gz", "sha256": sha}
    pin.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")
    print(f"Prepared {archive}: {len(files)} runtime files, SHA-256 {sha}")
    print(f"Publish this immutable asset as {value['tag']} before CI builds the image.")


def members(bundle):
    result = {}
    seen = set()
    for item in bundle.getmembers():
        name = safe_path(item.name)
        if not item.isfile() or name.lower() in seen:
            raise ValueError("Archive must contain unique regular files only")
        seen.add(name.lower())
        result[name] = item
    return result


def extract_runtime(archive, client, sha):
    if not re.fullmatch(r"[0-9a-f]{64}", sha) or digest(archive) != sha:
        raise ValueError("Runtime archive SHA-256 mismatch")
    with tarfile.open(archive) as bundle:
        entries = members(bundle)
        for name, item in entries.items():
            target = client / name
            target.parent.mkdir(parents=True, exist_ok=True)
            with bundle.extractfile(item) as source, target.open("xb") as output:
                shutil.copyfileobj(source, output)


def apply_release(archive, client, public_key, tag):
    with tarfile.open(archive) as bundle:
        entries = members(bundle)
        if entries["manifest.json"].size > 8 * 1024**2:
            raise ValueError("Manifest too large")
        raw = bundle.extractfile(entries["manifest.json"]).read()
        manifest = verified_manifest(raw, public_key)
        if tag != "client-" + manifest["version"]:
            raise ValueError("Release tag differs from signed version")
        expected = {}
        casefolded = set()
        for item in manifest["files"]:
            path = safe_path(item["path"])
            if path.lower() in casefolded:
                raise ValueError("Duplicate signed path")
            casefolded.add(path.lower())
            expected["files/" + path] = item
        if set(entries) != set(expected) | {"manifest.json"}:
            raise ValueError("Release has missing or unexpected files")
        deleted = [safe_path(p) for p in manifest["deleted_files"]]
        if any(p.lower() in casefolded for p in deleted):
            raise ValueError("Deleted file is also managed")
        for name, item in expected.items():
            if entries[name].size != item["size"]:
                raise ValueError("Release size mismatch")
            target = client / item["path"]
            target.parent.mkdir(parents=True, exist_ok=True)
            with bundle.extractfile(entries[name]) as source, target.open("wb") as output:
                shutil.copyfileobj(source, output)
            if digest(target) != item["sha256"]:
                raise ValueError("Release hash mismatch: " + item["path"])
        for name in deleted:
            (client / name).unlink(missing_ok=True)
        return raw


def download(repo, tag, asset, directory):
    subprocess.run(["gh", "release", "download", tag, "--repo", repo,
                    "--pattern", asset, "--dir", str(directory)], check=True)
    return directory / asset


def assemble(output, runtime, release, pin, public_key, tag):
    if output.exists():
        raise ValueError("Worker destination must be fresh")
    output.parent.mkdir(parents=True, exist_ok=True)
    # A failed verification never leaves a buildable worker directory behind.
    with tempfile.TemporaryDirectory(prefix=".worker-", dir=output.parent) as folder:
        client = Path(folder) / "client"
        client.mkdir()
        extract_runtime(runtime, client, pin["sha256"])
        raw = apply_release(release, client, public_key, tag)
        for name in ("King's Raid.exe", "UnityPlayer.dll", "MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll",
                     DATA + "/globalgamemanagers", DATA + "/Managed/SprkConquest.dll"):
            if not (client / name).is_file():
                raise ValueError("Worker runtime missing " + name)
        (client / "client-update-manifest.json").write_bytes(raw)
        client.rename(output)
    print(f"Verified Linux worker: {tag} at {output}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest="command", required=True)
    seed_args = commands.add_parser("seed")
    seed_args.add_argument("--client", type=Path, required=True)
    seed_args.add_argument("--archive", type=Path, required=True)
    seed_args.add_argument("--pin", type=Path, default=REPO / "config/worker-runtime.json")
    build = commands.add_parser("build")
    build.add_argument("--output", type=Path, default=REPO / "build-input/worker")
    build.add_argument("--runtime", type=Path)
    build.add_argument("--release", type=Path)
    build.add_argument("--repo", default="faberuser/sprk")
    build.add_argument("--tag", required=True)
    build.add_argument("--pin", type=Path, default=REPO / "config/worker-runtime.json")
    build.add_argument("--public-key", type=Path, default=REPO / "config/client-update-public.pem")
    args = parser.parse_args()
    if args.command == "seed":
        seed(args.client, args.archive, args.pin)
        return
    pin = json.loads(args.pin.read_text())
    with tempfile.TemporaryDirectory(prefix="sprk-worker-download-") as folder:
        temporary = Path(folder)
        runtime = args.runtime or download(pin["repository"], pin["tag"], pin["asset"], temporary)
        release = args.release or download(args.repo, args.tag, "client-update.tar.gz", temporary)
        assemble(args.output, runtime, release, pin, args.public_key, args.tag)


if __name__ == "__main__":
    main()
