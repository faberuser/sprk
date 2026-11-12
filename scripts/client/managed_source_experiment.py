"""Isolated managed-source feasibility experiment; never installs or publishes.

The local installation is a candidate reference, not proof of runtime parity.
All outputs live in a new workspace; reruns must use another workspace.
"""
import argparse
from collections import Counter
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys

REPO = Path(__file__).resolve().parents[2]
ILSPY = "9.1.0.7988"
DATA = "King's Raid_Data"


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def write_json(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def run(command, log, cwd=REPO):
    with log.open("w", encoding="utf-8") as output:
        return subprocess.run([str(x) for x in command], cwd=cwd, stdout=output,
                              stderr=subprocess.STDOUT, check=False).returncode


def repair_null_discards(source):
    """Give decompiled ldnull/pop discards a type without adding side effects."""
    repairs = []
    for path in sorted(source.rglob("*.cs")):
        original = path.read_bytes()
        updated, count = re.subn(rb"(?m)^(\s*)_ = null;(?=\r?$)", rb"\1_ = (object)null;", original)
        if count:
            path.write_bytes(updated)
            repairs.append({"path": path.relative_to(source).as_posix(), "count": count,
                            "rule": "typed-null-discard", "before_sha256": hashlib.sha256(original).hexdigest(),
                            "after_sha256": digest(path)})
    return repairs


def record_compile(work, result):
    diagnostic = work / "output/Assembly-CSharp.dll.diagnostics.txt"
    errors = Counter(re.findall(r": error (CS\d+):", diagnostic.read_text(encoding="utf-8-sig"))) if diagnostic.exists() else {}
    write_json(work / "gate.json", {"decision": "continue investigation", "compiled": result == 0,
                                   "errors_by_code": dict(sorted(errors.items())),
                                   "contract_parity": "not run", "runtime_parity": "not run",
                                   "release_eligible": False})


def prepare(client, work):
    client = client.resolve(strict=True)
    work = work.resolve()
    if work == client or client in work.parents or work in client.parents:
        raise ValueError("Experiment workspace must be separate from the installation")
    managed = client / DATA / "Managed"
    if not (managed / "Assembly-CSharp.dll").is_file():
        raise ValueError("Client has no Assembly-CSharp.dll")
    # Refuse existing directories, including empty ones: exports/repairs are immutable inputs.
    work.mkdir(parents=True, exist_ok=False)
    refs = work / "references"
    refs.mkdir()
    write_json(work / "gate.json", {"decision": "experiment incomplete", "release_eligible": False})
    inputs = []
    for source in sorted(managed.glob("*.dll")):
        before = digest(source)
        target = refs / source.name
        shutil.copyfile(source, target)
        if digest(target) != before or digest(source) != before:
            raise ValueError(f"Input changed during snapshot: {source.name}")
        inputs.append({"path": f"{DATA}/Managed/{source.name}", "sha256": before,
                       "size": target.stat().st_size})
    native = []
    for source in [client / "UnityPlayer.dll", client / "King's Raid.exe",
                   *sorted((client / DATA / "Plugins").rglob("*.dll")),
                   *sorted((client / "MonoBleedingEdge").rglob("*.dll"))]:
        if source.is_file():
            native.append({"path": source.relative_to(client).as_posix(), "sha256": digest(source)})
    tracked = [*sorted((REPO / "scripts/ManagedSource").glob("*.cs*")), Path(__file__),
               REPO / "scripts/DllPatcher/BuildRelease.cs", REPO / "scripts/DllPatcher/Program.cs",
               REPO / "scripts/release/client_baseline.py"]
    write_json(work / "inputs.json", {
        "schema": 1, "reference_status": "local candidate; runtime parity unverified",
        "git_revision": subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO, text=True).strip(),
        "git_status": subprocess.check_output(["git", "status", "--porcelain"], cwd=REPO, text=True),
        "sdk": subprocess.check_output(["dotnet", "--version"], text=True).strip(),
        "ilspycmd": ILSPY, "managed": inputs, "native": native,
        "tool_inputs": {p.relative_to(REPO).as_posix(): digest(p) for p in tracked}})
    # A discovery index, deliberately not an assertion of migrated ownership.
    rows = []
    for relative in ("scripts/DllPatcher/BuildRelease.cs", "scripts/DllPatcher/Program.cs"):
        for number, line in enumerate((REPO / relative).read_text(encoding="utf-8-sig").splitlines(), 1):
            if re.search(r"\b(?:Patch|Install|Restore|Remove|Verify|Test)\w*\(", line):
                rows.append({"source": relative, "line": number, "operation": line.strip(),
                             "owner": "unreviewed", "migration": "legacy only"})
    write_json(work / "transformation-index.json", rows)
    return refs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--client", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--inventory-only", action="store_true")
    args = parser.parse_args()
    work = args.work.resolve()
    refs = prepare(args.client, work)
    project = REPO / "scripts/ManagedSource/ManagedSource.csproj"
    tool = ["dotnet", "run", "--project", project, "--property:RestoreLockedMode=true", "--"]
    if run([*tool, "inventory", refs, work / "inventory.json"], work / "inventory.log"):
        raise RuntimeError(f"Inventory failed; see {work / 'inventory.log'}")
    if args.inventory_only:
        return 0
    tools = work / "tools"
    if run(["dotnet", "tool", "install", "ilspycmd", "--version", ILSPY, "--tool-path", tools], work / "tool-install.log"):
        raise RuntimeError("Pinned decompiler installation failed; see tool-install.log")
    executable = tools / ("ilspycmd.exe" if os.name == "nt" else "ilspycmd")
    original = work / "export-original"
    if run([executable, "-p", "-o", original, "-r", refs, refs / "Assembly-CSharp.dll"], work / "export.log"):
        raise RuntimeError("Decompilation failed; see export.log")
    write_json(work / "export-hashes.json", {p.relative_to(original).as_posix(): digest(p)
                                           for p in sorted(original.rglob("*")) if p.is_file()})
    repaired = work / "source"
    shutil.copytree(original, repaired)
    write_json(work / "repairs.json", repair_null_discards(repaired))
    (work / "output").mkdir()
    result = run([*tool, "compile", repaired, refs, work / "output/Assembly-CSharp.dll"], work / "compile.log")
    record_compile(work, result)
    print(f"Experiment: {work}; compilation {'passed' if result == 0 else 'failed'}; parity gate remains closed")
    return result


if __name__ == "__main__":
    sys.exit(main())
