"""Exercise the real native Continuous Battle prefab in an isolated Unity client."""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
import uuid

REPO = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO))
from scripts.client.verify_graphics_options import prepare
from scripts.automation.client_automation import request


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--unpatched", action="store_true", help="Negative control: require the original prefab crash")
    args = parser.parse_args()
    client = REPO.parent / "sprk-client"
    root = REPO / "target/tower-continuous-validation" / uuid.uuid4().hex[:12]
    prepare(client, root, installed=True)
    managed = root / "King's Raid_Data/Managed"
    if not args.unpatched:
        shutil.copy2(client / "King's Raid_Data/Managed/Assembly-CSharp.dll.continuous-battle-staged", managed / "Assembly-CSharp.dll")
    shutil.copy2(client / "King's Raid_Data/Managed/SprkAutomation.dll.staged", managed / "SprkAutomation.dll")
    bridge = root / "automation"
    env = dict(os.environ, SPRK_AUTOMATION_DIR=str(bridge), SPRK_AUTOMATION_PROFILE="tower-continuous-" + root.name)
    for key in list(env):
        if key.endswith("_TEST"):
            env.pop(key)
    startup = subprocess.STARTUPINFO()
    startup.dwFlags |= subprocess.STARTF_USESHOWWINDOW
    startup.wShowWindow = 4
    proc = subprocess.Popen([str(next(root.glob("SprkTest-graphics-*.exe"))), "-screen-fullscreen", "0", "-screen-width", "640", "-screen-height", "480", "-logFile", str(root / "client.log")], cwd=root, env=env, startupinfo=startup)
    print(root, flush=True)
    try:
        for _ in range(200):
            if bridge.exists() or proc.poll() is not None:
                break
            time.sleep(.1)
        time.sleep(4)
        try:
            reply = request(bridge, "tower-continuous-probe", timeout=25)
        except TimeoutError:
            if not args.unpatched:
                raise
            code = proc.wait(timeout=15)
            log = (root / "client.log").read_text(encoding="utf-8", errors="replace")
            if code == 0 or "out of bounds" not in log or "Crash!!!" not in log:
                raise AssertionError("Negative control did not reproduce the prefab deserialization crash")
            print("PASS: unpatched native popup reproduced the fatal out-of-bounds crash", flush=True)
            return
        if args.unpatched:
            raise AssertionError("Negative control unexpectedly survived")
        (root / "report.json").write_text(json.dumps(reply, indent=2), encoding="utf-8")
        print(reply["materials"][-1], flush=True)
        request(bridge, "quit")
        assert proc.wait(timeout=15) == 0
    finally:
        if proc.poll() is None:
            proc.terminate()
            proc.wait(timeout=15)


if __name__ == "__main__":
    main()
