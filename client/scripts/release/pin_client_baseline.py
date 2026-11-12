#!/usr/bin/env python3
"""Pin a published client release as the build's asset and runtime baseline.

Rarely needed: asset changes normally go in client/overrides/files. Use this to
fold accumulated overrides into a newer release, then delete the overrides it
already contains.

    python client/scripts/release/pin_client_baseline.py --tag client-<version>
"""
import argparse
import json
from pathlib import Path
import subprocess
import sys
import tempfile

# Allow both direct execution and package imports from any working directory.
if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.build_client_release import extract_baseline, file_hash
from client.scripts.release.client_release_inputs import REQUIRED_ASSETS, complete

REPO = Path(__file__).resolve().parents[3]
ASSET = "client-update.tar.gz"


def write_pin(path: Path, repository: str, tag: str, sha256: str) -> None:
    header = path.read_text(encoding="utf-8").split("BASELINE = ", 1)[0]
    pin = {"repository": repository, "tag": tag, "asset": ASSET, "sha256": sha256}
    path.write_text(header + "BASELINE = " + json.dumps(pin, indent=4) + "\n", encoding="utf-8")


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--tag", required=True, help="Published client-<version> release to pin")
    parser.add_argument("--repo", default="faberuser/sprk")
    parser.add_argument("--public-key", type=Path, default=REPO / "config/client-update-public.pem")
    parser.add_argument("--pin-file", type=Path, default=Path(__file__).with_name("client_baseline.py"))
    args = parser.parse_args()
    release = json.loads(subprocess.check_output(["gh", "api", f"repos/{args.repo}/releases/tags/{args.tag}"]))
    if not complete(release):
        parser.error(f"{args.tag} is not a complete published client release ({', '.join(sorted(REQUIRED_ASSETS))})")
    with tempfile.TemporaryDirectory(prefix="sprk-baseline-") as temporary:
        folder = Path(temporary)
        subprocess.run(["gh", "release", "download", args.tag, "--repo", args.repo,
                        "--pattern", ASSET, "--dir", str(folder)], check=True)
        archive = folder / ASSET
        sha256 = file_hash(archive)
        # Refuse to pin anything the release build itself would reject.
        extract_baseline(archive, folder / "client", sha256, args.public_key.resolve(strict=True))
    write_pin(args.pin_file, args.repo, args.tag, sha256)
    print(f"Pinned {args.tag} ({sha256}) in {args.pin_file}")


if __name__ == "__main__":
    main()
