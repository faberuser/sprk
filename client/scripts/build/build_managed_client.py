#!/usr/bin/env python3
"""Compile Assembly-CSharp.dll and every Sprk*.dll from client/managed.

Inputs are the tracked C# source and the game's own runtime DLLs (Unity, Mono
and third-party libraries), which are read from an installed client or an
extracted client release and checked against client/managed/references.lock.json.
No previously built game or helper DLL is used.

    python client/scripts/build/build_managed_client.py
    python client/scripts/build/build_managed_client.py --install ../sprk-client
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import time

REPO = Path(__file__).resolve().parents[3]
CLIENT = REPO / "client"
SOURCE = CLIENT / "managed"
LOCK = SOURCE / "references.lock.json"
GAME = "Assembly-CSharp"
MANAGED = "King's Raid_Data/Managed"
MARKER = "result.json"


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def projects() -> dict[str, Path]:
    """Every directory of client/managed is one assembly named after it."""
    found = {path.stem: path for path in sorted(SOURCE.glob("*/*.csproj"))}
    for name, path in found.items():
        if path.parent.name != name:
            raise ValueError(f"Project file must be named after its directory: {path}")
    if GAME not in found:
        raise ValueError(f"Missing {GAME} project")
    return found


def stage_references(source: Path, destination: Path, lock: dict, built=()) -> None:
    """Copy exactly the locked runtime DLLs; never a game or helper assembly."""
    for item in lock["files"]:
        name = item["path"]
        if Path(name).name != name or "\\" in name or Path(name).stem in built:
            raise ValueError(f"Invalid runtime reference: {name}")
    for item in lock["files"]:
        path = source / item["path"]
        if not path.is_file() or path.stat().st_size != item["size"] or digest(path) != item["sha256"]:
            raise ValueError(f"Runtime reference missing or changed: {path}")
    destination.mkdir(parents=True, exist_ok=False)
    for item in lock["files"]:
        shutil.copyfile(source / item["path"], destination / item["path"])


def bootstrap_project(folder: Path) -> Path:
    """One throwaway assembly holding the game and all helpers.

    The game calls the helpers and the helpers call the game. Compiling all of
    it together once gives the first helper pass something to reference, so
    the circular dependency never needs a previously built DLL.
    """
    folder.mkdir(parents=True)
    source = SOURCE.as_posix()
    project = folder / f"{GAME}.csproj"
    # MSBuild only applies these settings early enough from a Directory.Build file.
    for name in ("Directory.Build.props", "Directory.Build.targets"):
        (folder / name).write_text(f'<Project>\n  <Import Project="{source}/{name}" />\n</Project>\n', encoding="utf-8")
    project.write_text(f"""<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="{source}/*/**/*.cs" Exclude="{source}/*/Properties/**;{source}/*/obj/**;{source}/*/bin/**" />
    <Compile Include="{source}/{GAME}/Properties/**/*.cs" />
  </ItemGroup>
</Project>
""", encoding="utf-8")
    return project


def solution(path: Path, members: list[Path]) -> Path:
    lines = "".join(f'  <Project Path="{member.as_posix()}" />\n' for member in members)
    path.write_text(f"<Solution>\n{lines}</Solution>\n", encoding="utf-8")
    return path


def compile_managed(references: Path, work: Path, dotnet: str = "dotnet") -> dict[str, Path]:
    """Build into a new `work` directory and return {assembly name: DLL path}."""
    references, work = references.resolve(strict=True), work.resolve()
    if work == SOURCE or SOURCE in work.parents or work in SOURCE.parents:
        raise ValueError("Work directory must not overlap client/managed")
    host = shutil.which(dotnet)
    if not host:
        raise ValueError(f"dotnet not found: {dotnet}")
    found = projects()
    helpers = [path for name, path in found.items() if name != GAME]
    helper_names = [path.stem for path in helpers]
    work.mkdir(parents=True, exist_ok=False)
    staged, logs = work / "references", work / "logs"
    stage_references(references, staged, json.loads(LOCK.read_text(encoding="utf-8")), built=set(found))
    logs.mkdir()
    # client/global.json selects the SDK, so every command runs from client/.
    sdk = subprocess.check_output([host, "--version"], cwd=CLIENT, text=True).strip()

    def build(step: str, target: Path, names: list[str]) -> None:
        """Compile `target`, then make its DLLs the references of later steps."""
        root = work / "steps" / step
        command = [host, "build", str(target), "-c", "Release", "--nologo", "-v", "quiet",
                   "-p:UseSharedCompilation=false", f"-p:ManagedReferenceDirectory={staged.as_posix()}",
                   f"-p:SprkBuildRoot={root.as_posix()}"]
        log = logs / f"{step}.log"
        start = time.perf_counter()
        with log.open("w", encoding="utf-8") as stream:
            code = subprocess.run(command, cwd=CLIENT, stdout=stream, stderr=subprocess.STDOUT).returncode
        if code:
            lines = log.read_text(encoding="utf-8", errors="replace").splitlines()
            errors = list(dict.fromkeys(line for line in lines if ": error " in line))
            print("\n".join(errors[:40]), file=sys.stderr)
            raise RuntimeError(f"Compilation failed in step '{step}'; full log: {log}")
        for name in names:
            shutil.copyfile(root / "bin" / f"{name}.dll", staged / f"{name}.dll")
        print(f"Built {step} ({time.perf_counter() - start:.0f}s)", flush=True)

    # 1. Everything in one assembly, only to break the game <-> helper cycle.
    build("bootstrap", bootstrap_project(work / "steps/bootstrap/project"), [GAME])
    # 2. Helpers against that assembly; 3. the game against those helpers.
    build("helpers-first", solution(work / "helpers-first.slnx", helpers), helper_names)
    build("game-first", found[GAME], [GAME])
    # 4. Final helpers and game, each compiled only against real assemblies.
    build("helpers", solution(work / "helpers.slnx", helpers), helper_names)
    build("game", found[GAME], [GAME])

    package = work / "client-code" / MANAGED
    package.mkdir(parents=True)
    outputs = {}
    for name in found:
        outputs[name] = package / f"{name}.dll"
        shutil.copyfile(staged / f"{name}.dll", outputs[name])
    (work / MARKER).write_text(json.dumps({
        "sdk": sdk,
        "assemblies": {name: {"size": path.stat().st_size, "sha256": digest(path)} for name, path in outputs.items()},
    }, indent=2) + "\n", encoding="utf-8")
    return outputs


def install(outputs: dict[str, Path], client: Path) -> None:
    """Replace the game and helper DLLs of an installed client or player folder."""
    managed = client.resolve(strict=True) / MANAGED
    if not (managed / "UnityEngine.dll").is_file():
        raise ValueError(f"Not a King's Raid client: {client}")
    try:
        for path in outputs.values():
            shutil.copyfile(path, managed / path.name)
    except PermissionError as error:
        raise RuntimeError("Close the game before installing; its DLLs are locked") from error
    print(f"Installed {len(outputs)} assemblies into {managed}")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--references", type=Path, default=REPO.parent / "sprk-client" / MANAGED,
                        help="Directory holding the game's runtime DLLs (default: ../sprk-client's Managed folder)")
    parser.add_argument("--work", type=Path, default=REPO / "artifacts/client-build",
                        help="Build directory; a previous build in it is replaced (default: artifacts/client-build)")
    parser.add_argument("--install", type=Path, metavar="CLIENT",
                        help="Copy the compiled DLLs into this client folder after a successful build")
    parser.add_argument("--dotnet", default="dotnet", help=".NET SDK host matching client/global.json")
    args = parser.parse_args()
    work = args.work.resolve()
    if work.exists():
        # Only ever delete a directory this script created.
        if any(work.iterdir()) and not ((work / MARKER).is_file() or (work / "references").is_dir()):
            parser.error(f"--work exists and is not a previous client build: {work}")
        shutil.rmtree(work)
    start = time.perf_counter()
    outputs = compile_managed(args.references, work, args.dotnet)
    print(f"Compiled {len(outputs)} assemblies in {time.perf_counter() - start:.0f}s: {work / 'client-code'}")
    if args.install:
        install(outputs, args.install)


if __name__ == "__main__":
    main()
