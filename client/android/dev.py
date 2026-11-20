#!/usr/bin/env python3
"""Everyday Android commands (the HybridCLR build in client/android/HybridBootstrap).

    python client/android/dev.py content    (original APK: ../sprk-apk/kings-raid-4.88.8.apk, or --apk)
                            one-time / after a Windows player-data or repacker change:
                            reference bundles, original APK data, content repack
    python client/android/dev.py assets     build ../sprk-android: original Android bundles plus
                                            bundles converted from ../sprk-client (needs content)
    python client/android/dev.py build      compile the game code and build the APK
    python client/android/dev.py test       build, install on the device, push missing device
                                            data, publish new asset versions when the assets or
                                            client/overrides/files changed (client/dev.py assets;
                                            --no-assets to skip), forward the local server, start the game
                                            (--picker: choose the server on the device, as
                                            SprkLauncher does on Windows)
    python client/android/dev.py hot        compile, push the game DLLs, publish new asset versions
                                            as test does, restart the game (no APK rebuild; the fast loop)
    python client/android/dev.py connect    restore adb port forwarding to the local server

By default the game plays on the local server (start_server.bat) through `adb reverse`.
With --picker the APK starts with a server list (Local plus servers added on the device). The device defaults to BlueStacks (HD-Adb.exe,
emulator-5554); pass --adb/--serial for a phone. Unity 6000.0.53f1 is found through
Unity Hub (Hub must be running and signed in for batch builds) or --unity / SPRK_UNITY.
See client/README.md (Android) for setup; docs/android.md lists open work.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

REPO = Path(__file__).resolve().parents[2]
PROJECT = REPO / "client/android/HybridBootstrap"
ARTIFACTS = REPO / "artifacts/android-bootstrap"
# Pinned original Android inputs, next to the repository (game content; never committed).
ORIGINAL_APK = REPO.parent / "sprk-apk" / "kings-raid-4.88.8.apk"
APK = ARTIFACTS / "apk/sprk-hybrid-probe.apk"
UNITY_VERSION = "6000.0.53f1"
DEFAULT_ADB = "C:/Program Files/BlueStacks_nxt/HD-Adb.exe"


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


def compile_code():
    run([sys.executable, REPO / "client/dev.py", "build"])
    run([sys.executable, PROJECT / "sync_inputs.py"])


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
    # ../sprk-android: original Android bundles plus bundles converted from ../sprk-client.
    cmd = [sys.executable, PROJECT / "content/build_android_assets.py"]
    if args.only:
        cmd += ["--only", args.only]
    run(cmd)


def cmd_build(args):
    if not (ARTIFACTS / "content/resources-map.json").is_file():
        raise SystemExit("No repacked content; run `content --apk ...` first")
    compile_code()
    unity(args, "SprkAndroidBuild.Build", "build.log")
    print(f"APK: {APK}")


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
    run([sys.executable, REPO / "client/dev.py", "build"])
    script(args, "push_hot.sh")
    publish_assets(args)
    script(args, "run_probe.sh")


def cmd_connect(args):
    script(args, "connect_local.sh")


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("command", choices=["content", "assets", "build", "test", "hot", "connect"])
    ap.add_argument("--only", help="assets: only bundles whose path starts with this (testing)")
    ap.add_argument("--apk", help=f"original Android APK (content; default {ORIGINAL_APK})")
    ap.add_argument("--unity", help=f"Unity {UNITY_VERSION} Unity.exe (default: from Unity Hub)")
    ap.add_argument("--adb", default=DEFAULT_ADB)
    ap.add_argument("--serial", default="emulator-5554")
    ap.add_argument("--no-build", action="store_true", help="test: install the existing APK")
    ap.add_argument("--picker", action="store_true", help="test: show the server picker at launch instead of using Local")
    ap.add_argument("--no-assets", action="store_true", help="test, hot: do not publish asset versions")
    args = ap.parse_args()
    if not shutil.which(args.adb) and not Path(args.adb).is_file():
        raise SystemExit(f"adb not found: {args.adb}")
    {"content": cmd_content, "assets": cmd_assets, "build": cmd_build, "test": cmd_test, "hot": cmd_hot,
     "connect": cmd_connect}[args.command](args)


if __name__ == "__main__":
    main()
