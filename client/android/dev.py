#!/usr/bin/env python3
"""Everyday Android commands (the HybridCLR build in client/android/HybridBootstrap).

    python client/android/dev.py content    (original APK: ../sprk-android/base-apk/kings-raid-4.88.8.apk, or --apk)
                            one-time / after a Windows player-data or repacker change:
                            reference bundles, original APK data, content repack
    python client/android/dev.py assets     build ../sprk-android/build: original Android bundles plus
                                            bundles converted from ../sprk-client (needs content)
    python client/android/dev.py signing    one-time: create the release signing key
                                            (../sprk-android/signing; copy that folder to every
                                            machine that builds APKs, and back it up)
    python client/android/dev.py build      compile the game code and build the APK. The game is
                                            compiled into the APK (IL2CPP, like the original game);
                                            --interpreted loads it with HybridCLR instead (slower,
                                            but `hot` works). --release-code <manifest URL> builds
                                            from a published release's game DLLs: an APK for players
                                            must run exactly the server's code, or it asks them to
                                            download a new APK
    python client/android/dev.py test       build, install on the device, push missing device
                                            data, publish new asset versions when the assets or
                                            client/overrides/files changed (client/dev.py assets;
                                            --no-assets to skip), forward the local server, start the game
                                            (--picker: choose the server on the device, as
                                            SprkLauncher does on Windows)
    python client/android/dev.py hot        compile, push the game DLLs, publish new asset versions
                                            as test does, restart the game (no APK rebuild; the fast
                                            loop; needs an APK built with --interpreted)
    python client/android/dev.py connect    restore adb port forwarding to the local server

By default the game plays on the local server (start_server.bat) through `adb reverse`.
With --picker the APK starts with a server list (Local plus servers added on the device). The device defaults to BlueStacks (HD-Adb.exe,
emulator-5554); pass --adb/--serial for a phone. Unity 6000.0.53f1 is found through
Unity Hub (Hub must be running and signed in for batch builds) or --unity / SPRK_UNITY.
See client/README.md (Android) for setup; docs/android.md lists open work.
"""
import argparse
import base64
import hashlib
import json
import os
from pathlib import Path
import secrets
import shutil
import subprocess
import sys
import urllib.parse
import urllib.request

REPO = Path(__file__).resolve().parents[2]
PROJECT = REPO / "client/android/HybridBootstrap"
ARTIFACTS = REPO.parent / "sprk-android/bootstrap"
# Pinned original Android inputs, next to the repository (game content; never committed).
ORIGINAL_APK = REPO.parent / "sprk-android" / "base-apk" / "kings-raid-4.88.8.apk"
APK = ARTIFACTS / "apk/sprk-hybrid-probe.apk"
UNITY_VERSION = "6000.0.53f1"
DEFAULT_ADB = "C:/Program Files/BlueStacks_nxt/HD-Adb.exe"
# Release signing (SprkAndroidBuild.ApplySigning): outside the repository, never committed.
SIGNING = REPO.parent / "sprk-android" / "signing"
MODE_FILE = PROJECT / "HotUpdate" / "mode.txt"
MANAGED = Path("King's Raid_Data") / "Managed"


def run(cmd, **kw):
    print("+", " ".join(str(c) for c in cmd), flush=True)
    subprocess.run([str(c) for c in cmd], check=True, cwd=REPO, **kw)


def find_unity(explicit: str | None) -> Path:
    for candidate in (explicit, os.environ.get("SPRK_UNITY")):
        if candidate:
            return Path(candidate)
    registry = Path(os.path.expandvars("%APPDATA%/UnityHub/editors-v2.json"))
    if registry.is_file():
        for editor in json.loads(registry.read_text(encoding="utf-8")).get("data", []):
            if editor.get("version") == UNITY_VERSION and editor.get("location"):
                return Path(editor["location"][0])
    raise SystemExit(f"Unity {UNITY_VERSION} not found; install it with Unity Hub or pass --unity")


def find_bash() -> Path:
    # Git Bash, not the WSL launcher that a bare `bash` may resolve to.
    exec_path = subprocess.run(["git", "--exec-path"], capture_output=True, text=True, check=True).stdout.strip()
    bash = Path(exec_path).parents[2] / "bin/bash.exe"
    if not bash.is_file():
        raise SystemExit(f"Git Bash not found at {bash}")
    return bash


def script(args, name: str, *extra):
    run([find_bash(), "--noprofile", "--norc", PROJECT / name, *extra, args.adb, args.serial])


def unity(args, method: str, log: str):
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    log_path = ARTIFACTS / log
    result = subprocess.run([str(find_unity(args.unity)), "-batchmode", "-quit", "-projectPath", str(PROJECT),
                             "-buildTarget", "Android", "-executeMethod", method, "-logFile", str(log_path)], cwd=REPO)
    text = log_path.read_text(encoding="utf-8", errors="replace") if log_path.exists() else ""
    if result.returncode != 0 or "error CS" in text:
        raise SystemExit(f"Unity {method} failed (exit {result.returncode}); see {log_path}")


def publish_assets(args):
    # The game downloads the local asset store's versions at start (docs/game-assets.md),
    # so edited bundles and overrides reach the device through a new version.
    if not args.no_assets:
        run([sys.executable, REPO / "client/dev.py", "assets"])


def fetch(url: str, timeout: int) -> bytes:
    # Cloudflare in front of the update host rejects urllib's default User-Agent (error 1010).
    request = urllib.request.Request(url, headers={"User-Agent": "sprk-dev/1.0"})
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.read()


def release_code(manifest_url: str) -> Path:
    """The game DLLs (Assembly-CSharp, Sprk*) of a published release, hash-checked and
    signature-checked like the APK's updater does, in ARTIFACTS/release-code."""
    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import padding

    outer = json.loads(fetch(manifest_url, 60))
    signature = outer.get("signature") or {}
    if not signature.get("payload"):
        raise SystemExit(f"{manifest_url} is not signed")
    payload = base64.b64decode(signature["payload"])
    key = serialization.load_pem_public_key((REPO / "config/client-update-public.pem").read_bytes())
    key.verify(base64.b64decode(signature["value"]), payload, padding.PKCS1v15(), hashes.SHA256())
    release = json.loads(payload)
    out = ARTIFACTS / "release-code" / MANAGED
    if out.exists():
        shutil.rmtree(out)
    out.mkdir(parents=True)
    prefix = MANAGED.as_posix() + "/"
    count = 0
    for f in release["files"]:
        name = f["path"][len(prefix):] if f["path"].startswith(prefix) else ""
        if "/" in name or not name.endswith(".dll") or not (name == "Assembly-CSharp.dll" or name.startswith("Sprk")):
            continue
        data = fetch(urllib.parse.urljoin(manifest_url, f["url"]), 300)
        if len(data) != f["size"] or hashlib.sha256(data).hexdigest() != f["sha256"]:
            raise SystemExit(f"{f['path']}: does not match the release manifest")
        (out / name).write_bytes(data)
        count += 1
    print(f"release {release['version']}: {count} game assemblies")
    return out.parents[1]


def compile_code(args=None):
    mode = "interpreted" if args is not None and args.interpreted else "aot"
    if args is not None and args.release_code:
        code = release_code(args.release_code)
        run([sys.executable, PROJECT / "sync_inputs.py", "--mode", mode, "--code", code])
        return
    run([sys.executable, REPO / "client/dev.py", "build"])
    run([sys.executable, PROJECT / "sync_inputs.py", "--mode", mode])


def cmd_content(args):
    args.apk = args.apk or ORIGINAL_APK
    if not Path(args.apk).is_file():
        raise SystemExit(f"content needs the original kings-raid-4.88.8 APK ({ORIGINAL_APK}, or --apk)")
    run([sys.executable, REPO / "client/dev.py", "build"])
    run([sys.executable, PROJECT / "sync_inputs.py"])
    unity(args, "SprkContentTemplates.Build", "templates.log")
    run([sys.executable, PROJECT / "content/extract_apk_data.py", args.apk])
    run([sys.executable, PROJECT / "content/repack_content.py"])
    print("Content repacked. Run `build` (the APK embeds the new Resources map) and `test`.")


def cmd_assets(args):
    # ../sprk-android/build: original Android bundles plus bundles converted from ../sprk-client.
    cmd = [sys.executable, PROJECT / "content/build_android_assets.py"]
    if args.only:
        cmd += ["--only", args.only]
    run(cmd)


def cmd_build(args):
    if not (ARTIFACTS / "content/resources-map.json").is_file():
        raise SystemExit("No repacked content; run `content --apk ...` first")
    if not (SIGNING / "release.json").is_file() and not os.environ.get("SPRK_ANDROID_SIGNING"):
        raise SystemExit(f"No release signing key in {SIGNING}; run `signing` once, or copy that folder from the other build machine")
    compile_code(args)
    unity(args, "SprkAndroidBuild.Build", "build.log")
    print(f"APK: {APK}")


def cmd_signing(args):
    # A PKCS12 keystore (what `keytool -genkeypair -storetype PKCS12` makes), without needing a JDK.
    import datetime
    from cryptography import x509
    from cryptography.hazmat.primitives import hashes, serialization
    from cryptography.hazmat.primitives.asymmetric import rsa
    from cryptography.hazmat.primitives.serialization import pkcs12
    from cryptography.x509.oid import NameOID

    config = SIGNING / "release.json"
    if config.is_file():
        raise SystemExit(f"{config} already exists; it signs every APK players have, never replace it")
    SIGNING.mkdir(parents=True, exist_ok=True)
    keystore = SIGNING / "sprk-release.keystore"
    password = secrets.token_urlsafe(24)  # PKCS12: one password for the store and the key
    key = rsa.generate_private_key(public_exponent=65537, key_size=4096)
    name = x509.Name([x509.NameAttribute(NameOID.COMMON_NAME, "SPRK")])
    now = datetime.datetime.now(datetime.timezone.utc)
    cert = (x509.CertificateBuilder().subject_name(name).issuer_name(name).public_key(key.public_key())
            .serial_number(x509.random_serial_number()).not_valid_before(now)
            .not_valid_after(now + datetime.timedelta(days=10000))
            .sign(key, hashes.SHA256()))
    keystore.write_bytes(pkcs12.serialize_key_and_certificates(
        b"sprk", key, cert, None, serialization.BestAvailableEncryption(password.encode())))
    config.write_text(json.dumps({"keystore": keystore.name, "storePassword": password, "alias": "sprk",
                                  "keyPassword": password}, indent=2) + "\n", encoding="utf-8")
    print(f"Release key created in {SIGNING}. Back up that folder and copy it to every machine that builds APKs:\n"
          "a lost key means players must uninstall (losing their downloaded data) to install the next APK.")


def cmd_test(args):
    if not args.no_build:
        cmd_build(args)
    if args.serial.startswith("emulator"):
        run([args.adb, "connect", "127.0.0.1:5555"])
    run([args.adb, "-s", args.serial, "install", "-r", APK])
    # The APK carries the current DLLs; drop overrides left by `hot`.
    run([args.adb, "-s", args.serial, "shell", "rm", "-rf", "/sdcard/Android/data/com.sprk.hybridprobe/files/sprk/hot"])
    script(args, "push_patch.sh")
    publish_assets(args)
    # Unattended runs skip the server picker and use Local; --picker shows it on launch.
    env = {**os.environ, "SPRK_SERVER_PICKER": "1" if args.picker else "0"}
    run([find_bash(), "--noprofile", "--norc", PROJECT / "push_content.sh", "Login", args.adb, args.serial], env=env)
    script(args, "run_probe.sh")


def cmd_hot(args):
    if not MODE_FILE.is_file() or MODE_FILE.read_text(encoding="utf-8").strip() != "interpreted":
        raise SystemExit("The last APK has the game compiled in, which ignores pushed DLLs; "
                         "use `test`, or `test --interpreted` once to use `hot`")
    run([sys.executable, REPO / "client/dev.py", "build"])
    script(args, "push_hot.sh")
    publish_assets(args)
    script(args, "run_probe.sh")


def cmd_connect(args):
    script(args, "connect_local.sh")


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("command", choices=["content", "assets", "signing", "build", "test", "hot", "connect"])
    ap.add_argument("--interpreted", action="store_true",
                    help="build, test: run the game code in HybridCLR's interpreter (slower; enables `hot`)")
    ap.add_argument("--release-code", metavar="MANIFEST_URL",
                    help="build, test: compile a published release's game DLLs instead of a local build "
                         "(e.g. https://<server>/updates/stable/manifest.json)")
    ap.add_argument("--only", help="assets: only bundles whose path starts with this (testing)")
    ap.add_argument("--apk", help=f"original Android APK (content; default {ORIGINAL_APK})")
    ap.add_argument("--unity", help=f"Unity {UNITY_VERSION} Unity.exe (default: from Unity Hub)")
    ap.add_argument("--adb", default=DEFAULT_ADB)
    ap.add_argument("--serial", default="emulator-5554")
    ap.add_argument("--no-build", action="store_true", help="test: install the existing APK")
    ap.add_argument("--picker", action="store_true", help="test: show the server picker at launch instead of using Local")
    ap.add_argument("--no-assets", action="store_true", help="test, hot: do not publish asset versions")
    args = ap.parse_args()
    if args.command in ("test", "hot", "connect") and not shutil.which(args.adb) and not Path(args.adb).is_file():
        raise SystemExit(f"adb not found: {args.adb}")
    {"content": cmd_content, "assets": cmd_assets, "signing": cmd_signing, "build": cmd_build, "test": cmd_test,
     "hot": cmd_hot, "connect": cmd_connect}[args.command](args)


if __name__ == "__main__":
    main()
