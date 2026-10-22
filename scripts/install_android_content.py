"""Install and hash-verify a separately published, signed Android client snapshot."""

import argparse
import hashlib
import json
from pathlib import Path
import shlex
import shutil
import subprocess

from client_update_bundle import verified_manifest
from publish_client_update import safe_path


def digest(path):
    result = hashlib.sha256()
    with path.open("rb") as source:
        for chunk in iter(lambda: source.read(1024 * 1024), b""):
            result.update(chunk)
    return result.hexdigest()


def install(release, public_key, adb, serial, report_dir, verify_only=False):
    release = release.resolve(strict=True)
    manifest = verified_manifest((release / "manifest.json").read_bytes(), public_key)
    if manifest["deleted_files"]:
        raise ValueError("Android snapshot installer does not apply deletions")
    files = []
    prefix = "files/Documents/Patch/Android/"
    for item in manifest["files"]:
        relative = safe_path(item["path"])
        if relative != "SPRK.apk" and not relative.startswith(prefix):
            raise ValueError(f"Unexpected Android snapshot path: {relative}")
        source = release / "files" / relative
        if source.resolve(strict=True) != source or not source.is_file():
            raise ValueError(f"Invalid snapshot file: {relative}")
        if source.stat().st_size != item["size"] or digest(source) != item["sha256"]:
            raise ValueError(f"Snapshot hash mismatch: {relative}")
        files.append((source, item))
    if not any(item["path"] == "SPRK.apk" for _, item in files):
        raise ValueError("Android snapshot must include its signed SPRK.apk")
    print(f"Verified signature and {len(files)} local snapshot files.", flush=True)
    report_dir.mkdir(parents=True, exist_ok=True)
    base = [str(adb), "-s", serial]

    def run(*args, timeout=30, output=None):
        result = subprocess.run(base + list(args), stdout=output or subprocess.PIPE,
                                stderr=subprocess.STDOUT, timeout=timeout)
        if result.returncode:
            detail = result.stdout.decode("utf-8", "replace")[-1500:] if result.stdout else "see installation log"
            raise RuntimeError(f"ADB {args[0]} failed ({result.returncode}): {detail}")
        return result.stdout.decode("utf-8", "replace") if result.stdout else ""

    run("shell", "am", "force-stop", "com.vespainteractive.KingsRaid")
    destination = "/sdcard/Android/data/com.vespainteractive.KingsRaid/files/Documents/Patch/Android"
    patch = release / "files/files/Documents/Patch/Android"
    data_files = [(source, item) for source, item in files if item["path"].startswith(prefix)]
    if not data_files:
        raise ValueError("Snapshot has no Android content")
    if not verify_only:
        with (report_dir / "install.log").open("wb") as log:
            run("install", "-r", str(release / "files/SPRK.apk"), timeout=120, output=log)
            # BlueStacks' bundled ADB 1.0.36 cannot create these directories via
            # sync secure_mkdirs. The shell can; precreate in short command batches.
            directories = {destination}
            for source, _ in data_files:
                current = source.parent
                while current != patch:
                    directories.add(destination + "/" + current.relative_to(patch).as_posix())
                    current = current.parent
            directories = sorted(directories)
            for i in range(0, len(directories), 16):
                run("shell", "mkdir -p " + " ".join(shlex.quote(p) for p in directories[i:i + 16]))
            print(f"Restoring {len(data_files)} content files; progress goes to install.log.", flush=True)
            run("push", str(patch), destination.rsplit("/", 1)[0] + "/", timeout=1800, output=log)
    package_paths = [line.removeprefix("package:").strip()
                     for line in run("shell", "pm", "path", "com.vespainteractive.KingsRaid").splitlines()
                     if line.startswith("package:")]
    if len(package_paths) != 1 or not package_paths[0].startswith("/data/app/"):
        raise ValueError("Expected one installed base APK")
    apk_item = next(item for _, item in files if item["path"] == "SPRK.apk")
    if run("shell", "sha256sum", package_paths[0]).split()[0] != apk_item["sha256"]:
        raise ValueError("Installed Android APK differs from the signed snapshot")
    checksum_file = report_dir / "device-checksums.sha256"
    checksum_file.write_text("".join(
        item["sha256"] + "  " + destination + "/" + item["path"][len(prefix):] + "\n"
        for _, item in data_files), encoding="utf-8", newline="\n")
    remote_checksum = "/sdcard/Download/sprk-content-" + manifest["version"] + ".sha256"
    run("push", str(checksum_file), remote_checksum)
    print("Verifying every restored content file on Android…", flush=True)
    with (report_dir / "device-hashes.log").open("wb") as log:
        run("shell", "sha256sum", "-c", remote_checksum, timeout=1800, output=log)
    checked = (report_dir / "device-hashes.log").read_text(encoding="utf-8", errors="replace")
    if sum(line.rstrip().endswith(": OK") for line in checked.splitlines()) != len(data_files):
        raise ValueError("Android checksum output did not confirm every file")
    report = {
        "version": manifest["version"], "manifest_signature_verified": True,
        "local_files_verified": len(files), "device_content_files_verified": len(data_files),
        "installed_apk_verified": True,
        "device_content_bytes": sum(item["size"] for _, item in data_files),
        "serial": serial, "verify_only": verify_only,
    }
    (report_dir / "installation-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    root = Path(__file__).resolve().parent.parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--release", type=Path, required=True)
    parser.add_argument("--public-key", type=Path, default=root / "config/client-update-public.pem")
    parser.add_argument("--adb", type=Path, default=Path(shutil.which("adb") or "C:/Program Files/BlueStacks_nxt/HD-Adb.exe"))
    parser.add_argument("--serial", default="emulator-5554")
    parser.add_argument("--report-dir", type=Path, required=True)
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    install(args.release, args.public_key, args.adb, args.serial, args.report_dir, args.verify_only)
