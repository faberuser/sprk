"""Separate test copy of the game, used by `client/dev.py test`.

The copy holds the small player files (about 1.2 GB) and links the 19 GB
King's Raid_Data/Documents folder of the real game through a junction, so the
real game is never modified.
"""
import os
from pathlib import Path
import shutil
import stat
import subprocess

DATA = "King's Raid_Data"
ASSETS = DATA + "/Documents"
MARKER = ".sprk-sandbox"
PLAYER_FILES = ["King's Raid.exe", "UnityPlayer.dll", "WinPixEventRuntime.dll", "MonoBleedingEdge", "D3D12"]


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


def processes_in(folder: Path) -> list[str]:
    """Running processes whose executable is inside `folder`. Fails closed."""
    command = ("$f = $env:SPRK_CHECK_DIR.TrimEnd('\\') + '\\'; "
               "Get-Process | Where-Object { $_.Path -and $_.Path.StartsWith($f, [StringComparison]::OrdinalIgnoreCase) } | "
               "ForEach-Object { '{0} (PID {1})' -f $_.ProcessName, $_.Id }")
    result = subprocess.run(["powershell", "-NoProfile", "-NonInteractive", "-Command", command],
                            env=dict(os.environ, SPRK_CHECK_DIR=os.path.abspath(folder)),  # abspath also removes '..'
                            capture_output=True, text=True)
    if result.returncode != 0:
        raise SystemExit(f"Could not check for running programs in {folder}; nothing was changed.\n{result.stderr.strip()}")
    return [line.strip() for line in result.stdout.splitlines() if line.strip()]


def require_closed(folder: Path) -> None:
    running = processes_in(folder)
    if running:
        raise SystemExit(f"Close these first, they run from {folder}: {', '.join(running)}")


def is_sandbox(path: Path) -> bool:
    return (path / MARKER).is_file()


def create(client: Path, sandbox: Path) -> None:
    """Copy the player and link the assets; compiled DLLs are installed separately."""
    client = client.resolve(strict=True)
    if not (client / "King's Raid.exe").is_file() or not (client / ASSETS).is_dir():
        raise SystemExit(f"Not a game folder: {client}")
    if sandbox.exists():
        raise SystemExit(f"{sandbox} already exists and is not a test copy made by client/dev.py.")
    (sandbox / DATA).mkdir(parents=True)
    (sandbox / MARKER).write_text(f"Test copy of {client}. Remove with: python client/dev.py clean\n", encoding="utf-8")
    for name in PLAYER_FILES:
        source = client / name
        (shutil.copytree if source.is_dir() else shutil.copy2)(source, sandbox / name)
    for source in (client / DATA).iterdir():
        if source.name != "Documents":
            (shutil.copytree if source.is_dir() else shutil.copy2)(source, sandbox / DATA / source.name)
    # A junction needs no administrator rights, unlike a directory symlink.
    subprocess.run(["cmd", "/c", "mklink", "/J", str(sandbox / ASSETS), str(client / ASSETS)],
                   check=True, capture_output=True)


def remove(sandbox: Path) -> None:
    """Delete a test copy without ever following its link into the real assets."""
    sandbox = Path(os.path.abspath(sandbox))
    if not is_sandbox(sandbox):
        raise SystemExit(f"{sandbox} is not a test copy made by client/dev.py; nothing removed.")
    require_closed(sandbox)
    assets = sandbox / ASSETS
    if is_link(assets):
        os.rmdir(assets)  # removes the junction only, never the linked assets
    remaining = links_inside(sandbox)
    if remaining:
        raise SystemExit(f"Refusing to delete: unexpected links remain: {remaining}")
    shutil.rmtree(sandbox)
