#!/usr/bin/env python3
"""Create or remove a separate game folder for testing source-built DLLs.

The sandbox copies the small player files (about 1.2 GB), links the 19 GB
King's Raid_Data/Documents folder of the real game through a junction, and
installs DLLs compiled from client/managed. The real game is never modified.

    python client/scripts/build/sandbox_client.py create
    python client/scripts/build/build_managed_client.py --install ../sprk-client-test   # after code changes
    python client/scripts/build/sandbox_client.py remove

Start it with "Play (local server).bat" inside the sandbox, never with
SprkLauncher: the launcher would restore the published DLLs.
"""
import argparse
import os
from pathlib import Path
import shutil
import stat
import subprocess
import sys

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.build.build_managed_client import MANAGED, REPO, compile_managed, install

DATA = "King's Raid_Data"
ASSETS = DATA + "/Documents"
MARKER = ".sprk-sandbox"
PLAYER_FILES = ["King's Raid.exe", "UnityPlayer.dll", "WinPixEventRuntime.dll", "MonoBleedingEdge", "D3D12"]
LAUNCHERS = {
    "Play (local server).bat": ("http://127.0.0.1:8080/host.json",
                                "Connects only to the local server started by start_server.bat."),
    "Play (public server).bat": ("https://play.krinfo.net/host.json",
                                 "Uses the PUBLIC server with your real account. Prefer the local server."),
}


def is_link(path: Path) -> bool:
    """True for a symlink or an NTFS junction, without following it."""
    try:
        info = os.lstat(path)
    except FileNotFoundError:
        return False
    return stat.S_ISLNK(info.st_mode) or bool(getattr(info, "st_file_attributes", 0) & stat.FILE_ATTRIBUTE_REPARSE_POINT)


def links_inside(root: Path) -> list[Path]:
    found = []
    for folder, directories, files in os.walk(root):
        for name in directories + files:
            path = Path(folder) / name
            if is_link(path):
                found.append(path)
        directories[:] = [d for d in directories if not is_link(Path(folder) / d)]
    return found


def create(client: Path, sandbox: Path) -> None:
    client = client.resolve(strict=True)
    if not (client / "King's Raid.exe").is_file() or not (client / ASSETS).is_dir():
        raise SystemExit(f"Not a game folder: {client}")
    if sandbox.exists():
        raise SystemExit(f"{sandbox} exists. Rebuild into it with build_managed_client.py --install, or remove it first.")
    (sandbox / DATA).mkdir(parents=True)
    (sandbox / MARKER).write_text(f"Test copy of {client}. Remove with sandbox_client.py remove.\n", encoding="utf-8")
    for name in PLAYER_FILES:
        source = client / name
        (shutil.copytree if source.is_dir() else shutil.copy2)(source, sandbox / name)
    for source in (client / DATA).iterdir():
        if source.name != "Documents":
            (shutil.copytree if source.is_dir() else shutil.copy2)(source, sandbox / DATA / source.name)
    # A junction needs no administrator rights, unlike a directory symlink.
    subprocess.run(["cmd", "/c", "mklink", "/J", str(sandbox / ASSETS), str(client / ASSETS)],
                   check=True, capture_output=True)
    for name, (host, note) in LAUNCHERS.items():
        (sandbox / name).write_text(
            "@echo off\r\n"
            f"REM {note}\r\n"
            "REM Do not use SprkLauncher here: it would replace the test DLLs with the published ones.\r\n"
            f'set "SPRK_HOST_URL={host}"\r\n'
            "cd /d \"%~dp0\"\r\n"
            'start "" "%~dp0King\'s Raid.exe"\r\n', encoding="ascii")
    outputs = compile_managed(client / MANAGED, REPO / "artifacts/client-build-sandbox")
    install(outputs, sandbox)
    shutil.rmtree(REPO / "artifacts/client-build-sandbox")
    print(f"Sandbox ready: {sandbox}\nStart the local server, then run \"Play (local server).bat\".")


def remove(sandbox: Path) -> None:
    sandbox = sandbox.absolute()
    if not (sandbox / MARKER).is_file():
        raise SystemExit(f"{sandbox} is not a sandbox created by this script; nothing removed.")
    assets = sandbox / ASSETS
    if is_link(assets):
        os.rmdir(assets)  # removes the junction only, never the linked assets
    remaining = links_inside(sandbox)
    if remaining:
        raise SystemExit(f"Refusing to delete: unexpected links remain: {remaining}")
    shutil.rmtree(sandbox)
    print(f"Removed {sandbox}; the real game folder was not touched.")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("action", choices=["create", "remove"])
    parser.add_argument("--client", type=Path, default=REPO.parent / "sprk-client", help="Real game folder (default: ../sprk-client)")
    parser.add_argument("--sandbox", type=Path, default=REPO.parent / "sprk-client-test", help="Test folder (default: ../sprk-client-test)")
    args = parser.parse_args()
    if args.action == "create":
        create(args.client, args.sandbox)
    else:
        remove(args.sandbox)


if __name__ == "__main__":
    main()
