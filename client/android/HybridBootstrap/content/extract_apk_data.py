#!/usr/bin/env python3
"""Extract the original Android client's player data (assets/bin/Data) for repack_content.py.

The 2018 Android player kept shaders that its Shader.unity3d bundle lacks; the repacker
adds those to the GLES shader bundle. Only the player data is extracted (no Managed code),
and sharedassets0.assets is re-joined from its .splitN parts.

usage: python client/android/HybridBootstrap/content/extract_apk_data.py <kings-raid-4.88.8.apk>
       [--out ../sprk-android/bootstrap/apk-data]
"""
import argparse
import re
import zipfile
from pathlib import Path

REPO = Path(__file__).resolve().parents[4]
PREFIX = "assets/bin/Data/"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("apk", type=Path)
    ap.add_argument("--out", type=Path, default=REPO.parent / "sprk-android" / "bootstrap" / "apk-data")
    args = ap.parse_args()
    out = args.out
    out.mkdir(parents=True, exist_ok=True)
    splits: dict[str, dict[int, bytes]] = {}
    count = 0
    with zipfile.ZipFile(args.apk) as apk:
        for name in apk.namelist():
            if not name.startswith(PREFIX) or "/Managed/" in name or name.endswith("/"):
                continue
            rel = name[len(PREFIX):]
            m = re.fullmatch(r"(.+)\.split(\d+)", rel)
            if m:
                splits.setdefault(m.group(1), {})[int(m.group(2))] = apk.read(name)
                continue
            target = out / rel
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(apk.read(name))
            count += 1
    for rel, parts in splits.items():
        with open(out / rel, "wb") as f:
            for i in sorted(parts):
                f.write(parts[i])
        count += 1
    print(f"extracted {count} files -> {out}")


if __name__ == "__main__":
    main()
