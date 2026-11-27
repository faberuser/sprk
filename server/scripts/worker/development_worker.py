"""Assemble the Linux battle worker for an unreleased branch (development).

A release-built image gets its worker from that release. A development image
has no release: this builds the same worker a release of this checkout would
produce, without publishing anything:

    pinned runtime + pinned baseline release (client_baseline.py)
    + client/overrides/files + game and helper code compiled from client/managed

That is exactly how build_client_release.py composes a client, so battles on a
development server run the branch's code and tables. Players' game files come
from the client updates the operator uploads to that server separately.
"""
import argparse
import json
from pathlib import Path
import sys
import tempfile

# Allow both direct execution and package imports from any working directory.
if __package__ in (None, ""):
    sys.path.insert(0, str(Path(__file__).resolve().parents[3]))

from client.scripts.release.build_client_release import apply_overrides, install_managed
from client.scripts.release.client_baseline import BASELINE
from server.scripts.worker.linux_worker import REPO, assemble, battle_unused, download


def main():
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--output", type=Path, default=REPO / "build-input/worker")
    parser.add_argument("--runtime", type=Path, help="worker-runtime.tar.gz (default: download the pinned one)")
    parser.add_argument("--baseline", type=Path, help="baseline client-update.tar.gz (default: download)")
    parser.add_argument("--pin", type=Path, default=REPO / "config/worker-runtime.json")
    parser.add_argument("--public-key", type=Path, default=REPO / "config/client-update-public.pem")
    args = parser.parse_args()
    pin = json.loads(args.pin.read_text())
    with tempfile.TemporaryDirectory(prefix="sprk-dev-worker-") as folder:
        temporary = Path(folder)
        runtime = args.runtime or download(pin["repository"], pin["tag"], pin["asset"], temporary)
        baseline = args.baseline or download(BASELINE["repository"], BASELINE["tag"], BASELINE["asset"], temporary)
        # Verifies the runtime pin and the baseline's signature before anything is changed.
        assemble(args.output, runtime, baseline, pin, args.public_key, BASELINE["tag"])
        applied = [path for path in apply_overrides(REPO / "client/overrides/files", args.output)]
        # Keep the image as small as a release-built one.
        for relative in applied:
            if battle_unused(relative):
                (args.output / relative).unlink(missing_ok=True)
        assemblies = install_managed(args.output, temporary / "managed")
    print(f"Development worker at {args.output}: baseline {BASELINE['tag']}, "
          f"{len(applied)} override files, {len(assemblies)} assemblies built from client/managed")


if __name__ == "__main__":
    main()
