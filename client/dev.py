#!/usr/bin/env python3
"""Everyday client development commands.

    python client/dev.py build      compile the game code (no game folder changes)
    python client/dev.py test       build, install code and client/overrides/files into a separate
                                    test copy of the game, publish new asset versions when the
                                    assets changed (--no-assets to skip), start it
    python client/dev.py install    build, install code and overrides into your real game,
                                    publish a local release and, when the assets changed, new
                                    Windows and Android asset versions (--no-assets to skip)
    python client/dev.py assets     update the test copy's data and overrides, publish new Windows
                                    and Android asset versions when they changed (client/android/dev.py
                                    hot and test run this)
    python client/dev.py clean      delete the test copy

`test` never touches your real game and always plays on the local server
(start_server.bat). `install` publishes a local
release afterwards, so SprkLauncher on the Local server keeps your build instead
of restoring the published one.
"""
import argparse
from datetime import datetime
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import urllib.request

REPO = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(REPO))

from client.scripts.build import sandbox_client as sandbox
from client.scripts.build.build_managed_client import MANAGED, compile_managed, install

GAME = REPO.parent / "sprk-client"
TEST_GAME = REPO.parent / "sprk-client-test"
WORK = REPO / "artifacts/client-build"
OVERRIDES = REPO / "client/overrides/files"
UPDATES = REPO / "client-updates"
SIGNING_KEY = UPDATES / "update-signing/private.pem"
ASSETS = UPDATES / "assets"
ANDROID_GAME = REPO.parent / "sprk-android"
ANDROID_ORIGINALS = REPO.parent / "sprk-apk" / "Assetbundle"
SHADER_MAP = REPO / "client/android/HybridBootstrap/content/shader-map.json"
LOCAL_HOST = "http://127.0.0.1:8080/host.json"
LOCAL_MANIFEST = "http://127.0.0.1:8081/updates/stable/manifest.json"


def report_overrides(changed: list[str], game: Path) -> None:
    if changed:
        print(f"Applied {len(changed)} changed override(s) from client/overrides/files to {game}:")
        for path in changed[:10]:
            print("  " + path)
        if len(changed) > 10:
            print(f"  ... and {len(changed) - 10} more")


def require_local_server() -> None:
    try:
        with urllib.request.urlopen("http://127.0.0.1:8080/health", timeout=3) as response:
            if response.status == 200:
                return
    except OSError:
        pass
    raise SystemExit("The local server is not running. Start start_server.bat first.")


def build(game: Path) -> dict[str, Path]:
    if not (game / MANAGED).is_dir():
        raise SystemExit(f"Game folder not found: {game}. Pass --game <folder>.")
    if WORK.exists():
        shutil.rmtree(WORK)
    return compile_managed(game / MANAGED, WORK)


def prepare_test_copy(game: Path, code: bool) -> None:
    """Create or update the test copy: code (optional), the real game's data, then the overrides."""
    if TEST_GAME.exists():
        sandbox.require_closed(TEST_GAME)  # a running test copy locks its files
    if not TEST_GAME.exists():
        print(f"Creating the test copy {TEST_GAME} (first time only, about a minute)...")
        sandbox.create(game, TEST_GAME)
    elif not sandbox.is_sandbox(TEST_GAME):
        raise SystemExit(f"{TEST_GAME} exists but is not a test copy made by client/dev.py.")
    else:
        sandbox.upgrade(game, TEST_GAME)
    if code:
        install(build(game), TEST_GAME)
    # Start from the real game's data, then apply the tracked table/asset overrides.
    sandbox.refresh_data(game, TEST_GAME)
    report_overrides(sandbox.apply_overrides(OVERRIDES, TEST_GAME), TEST_GAME)


def test(args) -> None:
    # Unity allows one running copy.
    sandbox.require_closed(args.game)
    if TEST_GAME.exists():
        sandbox.require_closed(TEST_GAME)
    require_local_server()
    prepare_test_copy(args.game, code=not args.no_build)
    # Before the start: the game downloads the store's asset versions and would
    # replace overridden files that are not published.
    if not args.no_assets:
        publish_asset_versions(TEST_GAME)
    # Started directly, not through SprkLauncher: the launcher would replace the
    # test DLLs with the published ones. Pass the two addresses it would pass.
    environment = dict(os.environ, SPRK_HOST_URL=LOCAL_HOST, SPRK_UPDATE_MANIFEST_URL=LOCAL_MANIFEST)
    subprocess.Popen([str(TEST_GAME / "King's Raid.exe")], cwd=TEST_GAME, env=environment)
    print("Started the test copy on the local server. Rerun this command after each change.")


def install_real(args) -> None:
    from client.scripts.release.publish_client_update import DEFAULT_INCLUDES, publish
    sandbox.require_closed(args.game)
    if TEST_GAME.exists():
        sandbox.require_closed(TEST_GAME)
    install(build(args.game), args.game)
    report_overrides(sandbox.apply_overrides(OVERRIDES, args.game), args.game)
    previous_path = UPDATES / "stable/manifest.json"
    previous = json.loads(previous_path.read_text(encoding="utf-8")) if previous_path.is_file() else {"files": []}
    version = datetime.now().strftime("local-%Y%m%d-%H%M%S")
    key = SIGNING_KEY if SIGNING_KEY.is_file() else None
    # Overrides outside the default selection (a changed asset bundle) are code
    # files of the release, as in CI.
    overrides = [p.relative_to(OVERRIDES).as_posix() for p in sorted(OVERRIDES.rglob("*")) if p.is_file()]
    manifest = publish(args.game, UPDATES, version, "stable", DEFAULT_INCLUDES + overrides, key)
    before = {f["path"]: f["sha256"] for f in previous["files"]}
    after = {f["path"]: f["sha256"] for f in manifest["files"]}
    changed = sorted(p for p in after if before.get(p) != after[p])
    # Files the launcher will delete; paths merely no longer listed stay installed.
    removed = sorted(set(manifest["deleted_files"]) - set(previous.get("deleted_files", [])))
    print(f"Published local release {version}: {len(changed)} changed or new files, {len(removed)} removed.")
    for path in (changed + removed)[:15]:
        print("  " + path)
    if len(changed) + len(removed) > 15:
        print(f"  ... and {len(changed) + len(removed) - 15} more")
    if key is None:
        print("This release is unsigned (no signing key). In SprkLauncher > Manage Servers, edit Local and untick\n"
              "'Use the launcher's default public key' once, or the launcher will refuse it.")
    if not args.no_assets:
        publish_asset_versions(args.game)
    print("Start the game with SprkLauncher on the Local server (start_server.bat running).")


def publish_asset_versions(game: Path) -> None:
    """New asset versions in client-updates/assets for whatever changed (docs/game-assets.md)."""
    from client.scripts.release.publish_assets import PATCH_PREFIX, publish as publish_assets
    published = []
    result = publish_assets(game, ASSETS, "StandaloneWindows", if_changed=True)
    if result["version"] is not None:
        published.append(f"StandaloneWindows {result['version']}")
    # Android: convert the changed Windows bundles into ../sprk-android, then publish it.
    missing = [str(p) for p in (ANDROID_GAME, ANDROID_ORIGINALS, SHADER_MAP) if not p.exists()]
    if missing:
        print("Android assets not published; missing " + ", ".join(missing)
              + " (python client/android/dev.py content, then assets, once).")
    else:
        subprocess.run([sys.executable, str(REPO / "client/android/HybridBootstrap/content/build_android_assets.py"),
                        "--windows", str(game / PATCH_PREFIX)], check=True, cwd=REPO)
        result = publish_assets(ANDROID_GAME, ASSETS, "Android", patch_prefix="",
                                metadata_path=game / PATCH_PREFIX / "patch.json", if_changed=True)
        if result["version"] is not None:
            published.append(f"Android {result['version']}")
    if published:
        print(f"Published local asset versions: {', '.join(published)} (the local server serves them).\n"
              "Changes from client/overrides/files reach players with the merge (the release's asset overlay);\n"
              "upload client-updates/assets only for other asset changes (docs/game-assets.md).")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--game", type=Path, default=GAME, help="Your game folder (default: ../sprk-client)")
    commands = parser.add_subparsers(dest="command", required=True)
    commands.add_parser("build", help="Compile the game code into artifacts/client-build")
    tester = commands.add_parser("test", help="Build and start a separate test copy of the game on the local server")
    tester.add_argument("--no-build", action="store_true", help="Start the test copy without rebuilding")
    tester.add_argument("--no-assets", action="store_true", help="Do not publish asset versions")
    installer = commands.add_parser("install", help="Build, install into your real game, publish a local release "
                                                    "and new asset versions")
    installer.add_argument("--no-assets", action="store_true", help="Do not publish asset versions")
    commands.add_parser("assets", help="Update the test copy's data and overrides and publish new asset versions "
                                       "(no game folder of yours changes)")
    commands.add_parser("clean", help="Delete the test copy")
    args = parser.parse_args()
    args.game = args.game.resolve()
    if args.command == "build":
        outputs = build(args.game)
        print(f"Compiled {len(outputs)} assemblies into {WORK / 'client-code'}")
    elif args.command == "test":
        test(args)
    elif args.command == "install":
        install_real(args)
    elif args.command == "assets":
        prepare_test_copy(args.game, code=False)
        publish_asset_versions(TEST_GAME)
    elif TEST_GAME.exists():
        sandbox.remove(TEST_GAME)  # refuses while anything runs from it
        print(f"Deleted {TEST_GAME}; your real game was not touched.")
    else:
        print("No test copy to delete.")


if __name__ == "__main__":
    main()
