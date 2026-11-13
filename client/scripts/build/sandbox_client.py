"""Separate test copy of the game, used by `client/dev.py test`.

The copy holds the small player files (about 1.2 GB) and the game's data
folders (TableJit, LocalizationJit, TableData, PlainAsset, ... about 0.5 GB),
and links only the large media folders of the real game (Assetbundle, Video,
LoginPresentation, about 18 GB) through junctions. Table and text overrides
from client/overrides/files are applied to the copy, never through a link, so
the real game is never modified.
"""
import filecmp
import os
from pathlib import Path
import shutil
import stat
import subprocess

DATA = "King's Raid_Data"
ASSETS = DATA + "/Documents"
NATIVE = ASSETS + "/Patch/StandaloneWindows"
# Large, read-only media folders shared with the real game through junctions.
LINKED = {"Assetbundle", "Video", "LoginPresentation"}
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


def junction(target: Path, link: Path) -> None:
    # A junction needs no administrator rights, unlike a directory symlink.
    subprocess.run(["cmd", "/c", "mklink", "/J", str(link), str(target)], check=True, capture_output=True)


def link_assets(client: Path, sandbox: Path) -> None:
    """Copy the data folders of Documents and link only the large media folders."""
    def mirror(source: Path, target: Path, native: bool) -> None:
        target.mkdir(parents=True, exist_ok=True)
        for entry in source.iterdir():
            if native and entry.name in LINKED and entry.is_dir():
                junction(entry, target / entry.name)
            elif entry.is_dir():
                if (client / NATIVE).resolve() == entry.resolve() or (client / NATIVE).resolve().is_relative_to(entry.resolve()):
                    mirror(entry, target / entry.name, (client / NATIVE).resolve() == entry.resolve())
                else:
                    shutil.copytree(entry, target / entry.name)
            else:
                shutil.copy2(entry, target / entry.name)
    mirror(client / ASSETS, sandbox / ASSETS, False)


def create(client: Path, sandbox: Path) -> None:
    """Copy the player and data, link the media; compiled DLLs are installed separately."""
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
    link_assets(client, sandbox)


def upgrade(client: Path, sandbox: Path) -> None:
    """Older test copies linked all of Documents; give them their own data folders."""
    assets = sandbox / ASSETS
    if is_link(assets):
        os.rmdir(assets)  # removes the junction only, never the linked assets
        print("Upgrading the test copy: copying the game's data folders (about 0.5 GB, once)...")
        link_assets(client.resolve(strict=True), sandbox)


def linked_part(root: Path, target: Path) -> Path | None:
    """The first link between root and target (exclusive of root), if any."""
    current = root
    for part in target.relative_to(root).parts:
        current = current / part
        if is_link(current):
            return current
    return None


def refresh_data(client: Path, sandbox: Path) -> int:
    """Copy changed files of the game's copied data folders into the test copy."""
    copied = 0
    source_root = client / NATIVE
    for folder, directories, files in os.walk(source_root):
        directories[:] = [d for d in directories if not (Path(folder) == source_root and d in LINKED)]
        for name in files:
            source = Path(folder) / name
            target = sandbox / NATIVE / source.relative_to(source_root)
            if linked_part(sandbox, target) is not None:
                continue
            if not target.exists() or not filecmp.cmp(source, target, shallow=True):
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(source, target)
                copied += 1
    return copied


def apply_overrides(overrides: Path, game: Path) -> list[str]:
    """Copy client/overrides/files into a game folder; refuse to write through a link."""
    files = sorted(p for p in overrides.rglob("*") if p.is_file())
    targets = []
    for source in files:
        target = game / source.relative_to(overrides)
        link = linked_part(game, target)
        if link is not None:
            raise SystemExit(f"Override {source.relative_to(overrides)} lies inside the linked folder {link}; "
                             "it cannot be tested in the test copy without changing the real game.")
        targets.append((source, target))
    changed = []
    for source, target in targets:
        if not target.exists() or not filecmp.cmp(source, target, shallow=False):
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(source, target)
            changed.append(source.relative_to(overrides).as_posix())
    return changed


def remove(sandbox: Path) -> None:
    """Delete a test copy without ever following its links into the real assets."""
    sandbox = Path(os.path.abspath(sandbox))
    if not is_sandbox(sandbox):
        raise SystemExit(f"{sandbox} is not a test copy made by client/dev.py; nothing removed.")
    require_closed(sandbox)
    for link in links_inside(sandbox):
        if not link.is_relative_to(sandbox / ASSETS):
            raise SystemExit(f"Refusing to delete: unexpected link {link}")
        os.rmdir(link)  # removes the junction only, never the linked assets
    remaining = links_inside(sandbox)
    if remaining:
        raise SystemExit(f"Refusing to delete: unexpected links remain: {remaining}")
    shutil.rmtree(sandbox)
