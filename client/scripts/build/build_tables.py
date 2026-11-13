#!/usr/bin/env python3
"""Regenerate table and localization overrides from client/tables patches.

Tables (.jit) and localization have no source form. Their edits are kept as
readable JSON patches under client/tables/ and applied to the pinned baseline
release with client/tools/JitTables, which uses the game's own table code. The
results are written, in final form, to client/overrides/files with updated
SPRK-Native*.json metadata. See docs/jit-tables.md.

    python client/scripts/build/build_tables.py            # regenerate overrides
    python client/scripts/build/build_tables.py --check    # fail if overrides are stale

Needs the .NET SDK, `gh` (first run only, to download the baseline) and staged
managed references (run `python client/dev.py build` once).
"""
import argparse
import hashlib
import json
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import tempfile

if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.client_baseline import BASELINE

REPO = Path(__file__).resolve().parents[3]
PATCHES = REPO / "client/tables"
TOOL = REPO / "client/tools/JitTables"
NATIVE = "King's Raid_Data/Documents/Patch/StandaloneWindows"
OVERRIDES = REPO / "client/overrides/files" / NATIVE
# Records the overrides this script owns, so a removed patch removes its table.
GENERATED = REPO / "client/overrides/generated-tables.json"
CACHE = REPO / "artifacts/table-baseline"
WANTED = ("TableJit/", "LocalizationJit/", "SPRK-NativeData.json", "SPRK-NativeLocalization.json")


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def baseline_input() -> Path:
    """Pristine TableJit/LocalizationJit of the pinned baseline, extracted once."""
    folder = CACHE / BASELINE["tag"]
    extracted = folder / "native"
    marker = folder / "extracted.json"
    if marker.exists() and json.loads(marker.read_text(encoding="utf-8")).get("sha256") == BASELINE["sha256"]:
        return extracted
    archive = folder / BASELINE["asset"]
    if not archive.exists() or digest(archive) != BASELINE["sha256"]:
        folder.mkdir(parents=True, exist_ok=True)
        archive.unlink(missing_ok=True)
        subprocess.run(["gh", "release", "download", BASELINE["tag"], "--repo", BASELINE["repository"],
                        "--pattern", BASELINE["asset"], "--dir", str(folder)], check=True)
    if digest(archive) != BASELINE["sha256"]:
        raise ValueError("Baseline archive does not match its pinned SHA-256")
    if extracted.exists():
        shutil.rmtree(extracted)
    prefix = "files/" + NATIVE + "/"
    with tarfile.open(archive) as bundle:
        for member in bundle:
            if not member.isfile() or not member.name.startswith(prefix):
                continue
            relative = member.name[len(prefix):]
            if not relative.startswith(WANTED) or ".." in relative.split("/"):
                continue
            target = extracted / relative
            target.parent.mkdir(parents=True, exist_ok=True)
            with bundle.extractfile(member) as source, target.open("wb") as output:
                shutil.copyfileobj(source, output)
    marker.write_text(json.dumps({"sha256": BASELINE["sha256"]}) + "\n", encoding="utf-8")
    return extracted


def tool() -> list[str]:
    subprocess.run(["dotnet", "build", str(TOOL), "-c", "Release", "-v", "q", "-nologo"], check=True, cwd=REPO / "client")
    return ["dotnet", str(TOOL / "bin/Release/net10.0/JitTables.dll")]


def generate(output: Path) -> list[str]:
    """Build every patch into output; return the generated relative paths."""
    source = baseline_input()
    for name in ("SPRK-NativeData.json", "SPRK-NativeLocalization.json"):
        # Start metadata from the tracked override when there is one, so other
        # override entries are kept; otherwise from the baseline.
        tracked = OVERRIDES / name
        if tracked.exists():
            shutil.copyfile(tracked, output / name)
    subprocess.run([*tool(), "build", str(source), str(PATCHES), str(output)], check=True)
    return sorted(p.relative_to(output).as_posix() for p in output.rglob("*.jit"))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--check", action="store_true", help="Compare instead of writing; exit 1 when stale")
    args = parser.parse_args()
    previous = json.loads(GENERATED.read_text(encoding="utf-8"))["files"] if GENERATED.exists() else []
    with tempfile.TemporaryDirectory(prefix="sprk-tables-") as temporary:
        output = Path(temporary)
        files = generate(output)
        metadata = [p.name for p in output.glob("SPRK-Native*.json")]
        stale = [f for f in files + metadata
                 if not (OVERRIDES / f).exists() or digest(OVERRIDES / f) != digest(output / f)]
        stale += [f for f in previous if f not in files and (OVERRIDES / f).exists()]
        if args.check:
            for name in stale:
                print("stale:", name)
            return 1 if stale else 0
        for name in previous:
            if name not in files:
                (OVERRIDES / name).unlink(missing_ok=True)
                print("removed", name)
        for name in files + metadata:
            target = OVERRIDES / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(output / name, target)
        GENERATED.write_text(json.dumps({"files": files}, indent=2) + "\n", encoding="utf-8")
        print(f"Wrote {len(files)} generated files and {len(metadata)} metadata files to {OVERRIDES.relative_to(REPO)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
