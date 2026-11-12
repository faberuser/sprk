"""Compile maintained game/helper source and launcher, excluding player/assets.

No decompiler or IL patcher is invoked. Pinned helper APIs bootstrap the circular
references, then every shipping game/helper DLL is compiled from source.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import zipfile

REPO = Path(__file__).resolve().parents[3]
SOURCE = REPO / "client"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def stage_references(inputs, destination, manifest):
    """Validate all inputs before copying; the original game DLL is never needed."""
    for item in manifest["files"]:
        name = item["path"]
        if Path(name).name != name or "/" in name or "\\" in name or name == "Assembly-CSharp.dll":
            raise ValueError(f"Invalid dependency path: {name}")
    if inputs.is_file():
        with zipfile.ZipFile(inputs) as archive:
            expected = {item["path"]: item for item in manifest["files"]}
            names = archive.namelist()
            if len(names) != len(set(names)) or set(names) != set(expected):
                raise ValueError("Reference archive must contain exactly the pinned dependency files")
            for info in archive.infolist():
                if info.file_size != expected[info.filename]["size"]:
                    raise ValueError(f"Dependency size mismatch: {info.filename}")
            destination.mkdir(parents=True, exist_ok=False)
            for name, item in expected.items():
                target = destination / name
                with archive.open(name) as source, target.open("xb") as output:
                    shutil.copyfileobj(source, output)
                if digest(target) != item["sha256"]:
                    raise ValueError(f"Pinned API dependency changed: {name}")
        return
    for item in manifest["files"]:
        path = inputs / item["path"]
        if not path.is_file() or digest(path) != item["sha256"]:
            raise ValueError(f"Pinned API dependency missing or changed: {path}")
    destination.mkdir(parents=True, exist_ok=False)
    for item in manifest["files"]:
        target = destination / item["path"]
        shutil.copyfile(inputs / item["path"], target)
        if digest(target) != item["sha256"]:
            raise ValueError(f"Dependency changed while copying: {target}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--references", type=Path, required=True, help="Directory or ZIP containing hash-pinned API DLLs")
    parser.add_argument("--work", type=Path, required=True, help="New isolated build directory")
    parser.add_argument("--dotnet", default="dotnet", help=".NET 10.0.100 host executable")
    parser.add_argument("--managed-only", action="store_true", help="Skip the already maintained launcher")
    args = parser.parse_args()
    inputs = args.references.resolve(strict=True)
    work = args.work.resolve()
    if work == inputs or work in inputs.parents or inputs in work.parents:
        raise ValueError("Work directory must not overlap dependency inputs")
    if work == SOURCE or SOURCE in work.parents or work in SOURCE.parents:
        raise ValueError("Work directory must not overlap maintained source")
    host = shutil.which(args.dotnet)
    if not host:
        raise ValueError(f"dotnet host not found: {args.dotnet}")
    host = str(Path(host).resolve())
    sdk = subprocess.check_output([host, "--version"], cwd=SOURCE, text=True).strip()
    if sdk != "10.0.100":
        raise ValueError(f"Expected SDK 10.0.100, got {sdk}")
    manifest = json.loads((SOURCE / "dependencies.lock.json").read_text(encoding="utf-8"))
    work.mkdir(parents=True, exist_ok=False)
    save(work / "result.json", {"status": "building", "runtime_parity": "not tested", "release_eligible": False})
    references = work / "references"
    stage_references(inputs, references, manifest)
    logs = work / "logs"
    logs.mkdir()
    projects = sorted((SOURCE / "managed").glob("*/*.csproj"))
    game = next(p for p in projects if p.stem == "Assembly-CSharp")
    helpers = [p for p in projects if p != game]

    def build(project, phase):
        output = work / phase / project.stem
        command = [host, "build", str(project), "-c", "Release", "-t:Rebuild", "-v", "quiet", "--nologo",
                   "-p:UseSharedCompilation=false", "-p:BuildInParallel=false", "-maxcpucount:1",
                   f"-p:ManagedReferenceDirectory={references.as_posix()}", "-o", str(output)]
        log = logs / f"{phase}-{project.stem}.log"
        with log.open("w", encoding="utf-8") as stream:
            result = subprocess.run(command, cwd=SOURCE, stdout=stream, stderr=subprocess.STDOUT)
        if result.returncode:
            print(f"FAILED {project.stem}: {log}", flush=True)
            return None
        print(f"Built {phase}/{project.stem}", flush=True)
        return output / (project.stem + ".dll")

    def require(result):
        if result is None:
            raise RuntimeError(f"Compilation failed; inspect {logs}")
        return result

    # Bootstrap compiles the game using pinned companion APIs, never an original game template.
    seed = require(build(game, "bootstrap"))
    shutil.copyfile(seed, references / seed.name)
    with ThreadPoolExecutor(max_workers=3) as pool:
        compiled = list(pool.map(lambda p: build(p, "helpers"), helpers))
    for result in compiled:
        result = require(result)
        shutil.copyfile(result, references / result.name)
    # Recompile the game with only newly source-built companions.
    final_game = require(build(game, "final"))
    shutil.copyfile(final_game, references / final_game.name)
    # Check all helpers against the final game API (including private members exposed by recovery).
    with ThreadPoolExecutor(max_workers=3) as pool:
        final_helpers = list(pool.map(lambda p: build(p, "final"), helpers))
    rebuilt = [final_game, *[require(p) for p in final_helpers]]
    package = work / "client-code"
    managed = package / "King's Raid_Data/Managed"
    managed.mkdir(parents=True)
    for result in rebuilt:
        shutil.copyfile(result, managed / result.name)
    if not args.managed_only:
        output = work / "launcher"
        with (logs / "launcher.log").open("w", encoding="utf-8") as stream:
            subprocess.run([host, "publish", str(REPO / "client/launcher/SprkLauncher/SprkLauncher.csproj"),
                            "-c", "Release", "-r", "win-x64", "--self-contained", "true", "-o", str(output)],
                           cwd=SOURCE, stdout=stream, stderr=subprocess.STDOUT, check=True)
        shutil.copyfile(output / "SprkLauncher.exe", package / "SprkLauncher.exe")
    source_files = [p for p in SOURCE.rglob("*") if p.is_file() and not {"bin", "obj", "__pycache__"}.intersection(p.relative_to(SOURCE).parts)]
    source_files.append(Path(__file__).resolve())
    save(work / "build-inputs.json", {"sdk": sdk, "files": {p.relative_to(REPO).as_posix(): digest(p) for p in sorted(source_files)}})
    save(work / "result.json", {
        "status": "compiled", "managed_assemblies": len(rebuilt), "launcher": not args.managed_only,
        "runtime_parity": "not tested", "release_eligible": False,
        "files": {p.relative_to(package).as_posix(): digest(p) for p in sorted(package.rglob("*")) if p.is_file()}})
    print(f"Compiled client code: {package}")


if __name__ == "__main__":
    main()
