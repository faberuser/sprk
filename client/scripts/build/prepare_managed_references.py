"""Package verified compile-time dependencies separately from the maintained source."""
import argparse
import json
from pathlib import Path
import tempfile
import zipfile

if __package__ in (None, ""):
    import sys
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))
from client.scripts.build.build_managed_client import SOURCE, digest, stage_references


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--references", required=True, type=Path)
    parser.add_argument("--output", required=True, type=Path)
    args = parser.parse_args()
    manifest = json.loads((SOURCE / "dependencies.lock.json").read_text(encoding="utf-8"))
    with tempfile.TemporaryDirectory(prefix="sprk-api-") as temp:
        staged = Path(temp) / "references"
        stage_references(args.references.resolve(strict=True), staged, manifest)
        args.output.parent.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(args.output, "x", compression=zipfile.ZIP_DEFLATED) as archive:
            for path in sorted(staged.iterdir()):
                info = zipfile.ZipInfo(path.name, date_time=(1980, 1, 1, 0, 0, 0))
                info.compress_type = zipfile.ZIP_DEFLATED
                archive.writestr(info, path.read_bytes())
    print(f"{args.output}: SHA-256 {digest(args.output)}")


if __name__ == "__main__":
    main()
