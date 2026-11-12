"""Shorten only the opaque Soul Weapon advancement flash, preserving the bundle.

The native particle lives for 1.5 seconds and holds near-full alpha until about
66% of that lifetime. A 0.35-second lifetime covers the 1.9-second background
swap, then reveals the already-running cinematic. No gameplay timing changes.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path

import UnityPy

FLASH = "Effect_UI_Ascension_AdvanceEffect_Flash"
LIFETIME = 0.35


def inspect(data):
    env = UnityPy.load(data)
    names = {o.path_id: o.read().m_Name for o in env.objects if o.type.name == "GameObject"}
    targets = [o for o in env.objects if o.type.name == "ParticleSystem"
               and names.get(o.read_typetree()["m_GameObject"]["m_PathID"]) == FLASH]
    if len(targets) != 1:
        raise ValueError("Expected exactly one advancement flash particle")
    return env, targets[0]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("asset", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    original = args.asset.read_bytes()
    env, target = inspect(original)
    before = target.read_typetree()
    life = before["InitialModule"]["startLifetime"]
    if life["minMaxState"] != 0 or min(abs(life["scalar"] - v) for v in [1.5, LIFETIME]) > 1e-5:
        raise ValueError("Unexpected advancement flash lifetime; review the source asset")
    expected = copy.deepcopy(before)
    expected["InitialModule"]["startLifetime"]["scalar"] = LIFETIME
    expected["useUnscaledTime"] = True
    changed = abs(life["scalar"] - LIFETIME) > 1e-5 or not before["useUnscaledTime"]
    if args.output is None and changed:
        raise ValueError("Advancement flash still has the long/scaled white hold")
    output = original
    if args.output is not None and changed:
        untouched = {(o.assets_file.name, o.path_id): o.get_raw_data()
                     for o in env.objects if o != target}
        target.save_typetree(expected)
        output = env.file.save(packer="lz4")
        checked, flash = inspect(output)
        actual = flash.read_typetree()
        # Unity serializes the lifetime as float32.
        if abs(actual["InitialModule"]["startLifetime"]["scalar"] - LIFETIME) > 1e-5:
            raise ValueError("Flash lifetime did not survive native serialization")
        expected["InitialModule"]["startLifetime"]["scalar"] = actual["InitialModule"]["startLifetime"]["scalar"]
        if actual != expected:
            raise ValueError("Unintended advancement particle changes")
        after = {(o.assets_file.name, o.path_id): o.get_raw_data()
                 for o in checked.objects if o != flash}
        if after != untouched:
            raise ValueError("Unrelated bundle objects changed")
    if args.output is not None:
        args.output.write_bytes(output)
    print(json.dumps({"flash": FLASH, "lifetime_seconds": LIFETIME,
                      "unscaled_time": True, "changed": changed,
                      "sha256": hashlib.sha256(output).hexdigest(),
                      "verification": "only flash lifetime/clock changed; other particle fields and bundle objects preserved"}))


if __name__ == "__main__":
    main()
