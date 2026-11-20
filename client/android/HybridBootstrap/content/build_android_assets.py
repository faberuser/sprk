#!/usr/bin/env python3
"""Build the maintained Android asset set (../sprk-android) from the Windows client.

The Windows client (../sprk-client) is the source of truth for game assets. For every
asset bundle it has:

* unchanged from the original game (its MD5 matches the original Windows patch list)
  and present in the original Android set (../sprk-apk/Assetbundle): the original
  Android bundle is used;
* otherwise (changed by SPRK, or missing on Android, including the whole Low tier):
  the Windows bundle is converted (convert_bundles.py).

BundlePack.bin, the bundle index, is platform-neutral and comes from Windows (the
Android one predates content the tables use). The shader bundle the game loads
(Unity_2018/Shader.unity3d) is the original Android GLES one.

Incremental: a bundle is converted again only when its source or the converter changed
(state in ../sprk-android/.sprk-android.json). Unchanged originals are hard links into
../sprk-apk where possible. Never edit ../sprk-android by hand; run this again after
changing Windows assets, then publish it with
`publish_assets.py --client ../sprk-android --platform Android --patch-prefix ""`.

usage: python client/android/HybridBootstrap/content/build_android_assets.py [--jobs N] [--only PREFIX]
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import sys
from concurrent.futures import ProcessPoolExecutor, as_completed
from pathlib import Path

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]
WORKSPACE = REPO.parent
sys.path.insert(0, str(HERE))
import convert_bundles  # noqa: E402

WINDOWS_PATCH = WORKSPACE / "sprk-client" / "King's Raid_Data" / "Documents" / "Patch" / "StandaloneWindows"
# Files the game loads from the original Android set as they are.
ANDROID_ONLY = ("Unity_2018/Shader.unity3d", "Shader.unity3d")
# Windows-only shader bundles (D3D); Android uses ANDROID_ONLY instead.
SKIPPED_PREFIXES = ("Unity_2018/", "Unity_2020/")
NEUTRAL = ("BundlePack.bin",)


def md5(path: Path) -> str:
    digest = hashlib.md5()
    with path.open("rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest()


def fingerprint(path: Path) -> list:
    st = path.stat()
    return [st.st_size, st.st_mtime_ns]


def place(source: Path, target: Path) -> None:
    """Hard link (same volume) or copy, replacing the target atomically."""
    target.parent.mkdir(parents=True, exist_ok=True)
    temporary = target.with_name(f".{target.name}.{os.getpid()}.tmp")
    temporary.unlink(missing_ok=True)
    try:
        os.link(source, temporary)
    except OSError:
        shutil.copy2(source, temporary)
    os.replace(temporary, target)


def convert_one(source: str, target: str, shader_map: dict) -> tuple[str, dict | str]:
    try:
        data, stats = convert_bundles.convert(Path(source).read_bytes(), shader_map)
        t = Path(target)
        t.parent.mkdir(parents=True, exist_ok=True)
        temporary = t.with_name(f".{t.name}.{os.getpid()}.tmp")
        temporary.write_bytes(data)
        os.replace(temporary, t)
        return target, stats
    except Exception as e:  # reported per bundle; the others still finish
        return target, f"{type(e).__name__}: {e}"


def main() -> None:
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--windows", type=Path, default=WINDOWS_PATCH, help="Windows patch folder")
    ap.add_argument("--original", type=Path, default=WORKSPACE / "sprk-apk" / "Assetbundle", help="original Android bundles")
    ap.add_argument("--out", type=Path, default=WORKSPACE / "sprk-android", help="Android asset set (patch-folder layout)")
    ap.add_argument("--shader-map", type=Path, default=REPO / "artifacts/android-bootstrap/content/shader-map.json")
    ap.add_argument("--jobs", type=int, default=max(1, (os.cpu_count() or 2) - 2))
    ap.add_argument("--only", help="limit to bundle paths starting with this (testing)")
    args = ap.parse_args()

    win_bundles = args.windows / "Assetbundle"
    out_bundles = args.out / "Assetbundle"
    if not args.shader_map.exists():
        raise SystemExit(f"{args.shader_map} is missing: run `python client/android/dev.py content` first")
    shader_map = json.loads(args.shader_map.read_text(encoding="utf-8"))
    shader_key = hashlib.sha256(json.dumps(shader_map, sort_keys=True).encode()).hexdigest()[:16]
    state_path = args.out / ".sprk-android.json"
    state = json.loads(state_path.read_text(encoding="utf-8")) if state_path.exists() else {}
    files = state.setdefault("files", {})
    md5_cache = state.setdefault("windows_md5", {})

    original_md5 = {}
    for archive in json.loads((args.windows / "patch.json").read_text(encoding="utf-8"))["Archives"]:
        for asset in archive["AssetInfos"]:
            original_md5[asset["EntryName"]] = asset["Md5"].lower()

    plan: dict[str, tuple[str, Path]] = {}  # rel (under Assetbundle/) -> (kind, source)
    for path in sorted(win_bundles.rglob("*.unity3d")):
        rel = path.relative_to(win_bundles).as_posix()
        if rel.startswith(SKIPPED_PREFIXES) or rel in ANDROID_ONLY:
            continue
        if args.only and not rel.startswith(args.only):
            continue
        fp = fingerprint(path)
        cached = md5_cache.get(rel)
        digest = cached[1] if cached and cached[0] == fp else md5(path)
        md5_cache[rel] = [fp, digest]
        unchanged = original_md5.get("Assetbundle/" + rel) == digest
        if unchanged and (args.original / rel).is_file():
            plan[rel] = ("android", args.original / rel)
        else:
            plan[rel] = ("convert", path)
    if not args.only:
        for rel in ANDROID_ONLY:
            plan[rel] = ("android", args.original / rel)
        for rel in NEUTRAL:
            plan[rel] = ("windows", win_bundles / rel)

    todo, linked, kept = [], 0, 0
    for rel, (kind, source) in plan.items():
        target = out_bundles / rel
        key = [kind, fingerprint(source)] + ([convert_bundles.VERSION, shader_key] if kind == "convert" else [])
        if files.get(rel) == key and target.is_file():
            kept += 1
            continue
        if kind == "convert":
            todo.append((rel, source, target, key))
        else:
            place(source, target)
            files[rel] = key
            linked += 1
    if not args.only:
        stale = [rel for rel in files if rel not in plan]
        for rel in stale:
            (out_bundles / rel).unlink(missing_ok=True)
            del files[rel]
    else:
        stale = []

    print(f"{len(plan)} files: {kept} up to date, {linked} from originals/Windows, {len(todo)} to convert, {len(stale)} removed",
          flush=True)
    failures, totals = [], {}
    if todo:
        with ProcessPoolExecutor(max_workers=args.jobs) as pool:
            jobs = {pool.submit(convert_one, str(src), str(tgt), shader_map): (rel, key) for rel, src, tgt, key in todo}
            for done, future in enumerate(as_completed(jobs), 1):
                rel, key = jobs[future]
                _, result = future.result()
                if isinstance(result, str):
                    failures.append((rel, result))
                    files.pop(rel, None)
                else:
                    files[rel] = key
                    for k, v in result.items():
                        totals[k] = totals.get(k, 0) + v
                if done % 250 == 0 or done == len(todo):
                    print(f"  converted {done}/{len(todo)}", flush=True)
                    state_path.write_text(json.dumps(state), encoding="utf-8")
    args.out.mkdir(parents=True, exist_ok=True)
    state_path.write_text(json.dumps(state), encoding="utf-8")
    print("conversion totals:", totals)
    if failures:
        print(f"{len(failures)} bundles could not be converted:")
        for rel, error in failures[:30]:
            print(f"  {rel}: {error}")
        sys.exit(1)


if __name__ == "__main__":
    main()
