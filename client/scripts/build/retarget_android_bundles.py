#!/usr/bin/env python3
"""Turn Android asset bundles that ship in the Windows asset set into Windows bundles.

A few bundles under ``Documents/Patch/StandaloneWindows/Assetbundle`` are byte-identical
to the Android originals (build target 13, Unity 5.6/2018 instead of 2020.3). The Windows
player refuses them ("created for another build target"), so their effects or models never
appear. This keeps every object as it is except what is platform-specific:

* the target platform of each serialized file (Android 13 -> StandaloneWindows64 19);
* textures in mobile-only formats (ETC/ETC2/ASTC/PVRTC) are decoded and stored
  uncompressed (RGB24, or RGBA32 when they have alpha). These are small effect
  textures, so the size cost is minor and no quality is lost;
* preload entries into unity_builtin_extra that no object references are removed
  (older builds left them in; the 2020 player has no objects under those path IDs and
  fails to load the assets whose preload range includes them).

It also serves to restore a Windows bundle from the Android original when the Windows
rebuild lost assets (Xanadus' Skill1_Projectile has no _Smoke object on Windows).

Shader references need no change: materials point into the shared CAB-Shader bundle,
which has the same shaders under the same path IDs on both platforms (checked by
``--check-shaders``).

usage: python client/scripts/build/retarget_android_bundles.py <android.unity3d> <out.unity3d>
       [--check-shaders <Assetbundle/Unity_2020/Shader.unity3d> <Android Shader.unity3d>]
Requires UnityPy (pip install UnityPy).
"""
from __future__ import annotations

import argparse
from pathlib import Path

import UnityPy
from UnityPy.enums import BuildTarget, TextureFormat

WINDOWS = BuildTarget.StandaloneWindows64
MOBILE_FORMATS = {
    name for name in TextureFormat.__members__ if name.startswith(("ETC", "EAC", "ASTC", "PVRTC"))
}


def serialized_files(env):
    for bundle in env.files.values():
        for item in getattr(bundle, "files", {}).values():
            if hasattr(item, "target_platform"):
                yield item


def shader_names(path: Path) -> dict[int, str]:
    out = {}
    for obj in UnityPy.load(str(path)).objects:
        if obj.type.name == "Shader":
            data = obj.read()
            try:
                out[obj.path_id] = data.m_ParsedForm.m_Name
            except Exception:
                out[obj.path_id] = data.m_Name
    return out


def check_shaders(bundle: Path, windows_shader: Path, android_shader: Path) -> None:
    windows, android = shader_names(windows_shader), shader_names(android_shader)
    for obj in UnityPy.load(str(bundle)).objects:
        if obj.type.name != "Material":
            continue
        ref = obj.read_typetree()["m_Shader"]
        if ref["m_FileID"] == 0:
            continue
        name, windows_name = android.get(ref["m_PathID"]), windows.get(ref["m_PathID"])
        if windows_name is None:
            raise SystemExit(f"{bundle.name}: shader path ID {ref['m_PathID']} ({name}) is not in the Windows shader bundle")
        if name is not None and windows_name != name:
            raise SystemExit(f"{bundle.name}: shader {name} has another path ID on Windows")


def references(tree, out: set) -> None:
    if isinstance(tree, dict):
        if "m_FileID" in tree and "m_PathID" in tree:
            out.add((tree["m_FileID"], tree["m_PathID"]))
        for value in tree.values():
            references(value, out)
    elif isinstance(tree, list):
        for value in tree:
            references(value, out)


def drop_stale_builtin_preloads(env) -> list[str]:
    """Removes preload entries into unity_builtin_extra that no object references.

    Older builds list a few unity_builtin_extra objects (path IDs that do not exist in
    the 2020 player) in the preload range of some assets. Unity then fails to load
    those assets (LoadAsset returns null), although nothing in them uses the objects.
    """
    changes = []
    for item in serialized_files(env):
        builtin = {i + 1 for i, ext in enumerate(item.externals) if ext.path.lower().endswith("unity_builtin_extra")}
        if not builtin:
            continue
        used: set = set()
        bundle_obj = None
        for obj in item.objects.values():
            if obj.type.name == "AssetBundle":
                bundle_obj = obj
                continue
            references(obj.read_typetree(), used)
        if bundle_obj is None:
            continue
        tree = bundle_obj.read_typetree()
        table = tree["m_PreloadTable"]
        keep = [
            not (ref["m_FileID"] in builtin and (ref["m_FileID"], ref["m_PathID"]) not in used)
            for ref in table
        ]
        if all(keep):
            continue
        before = [0]
        for flag in keep:
            before.append(before[-1] + int(flag))
        for _, info in tree["m_Container"]:
            start, size = info["preloadIndex"], info["preloadSize"]
            info["preloadIndex"] = before[start]
            info["preloadSize"] = before[start + size] - before[start]
        dropped = [ref for ref, flag in zip(table, keep) if not flag]
        tree["m_PreloadTable"] = [ref for ref, flag in zip(table, keep) if flag]
        bundle_obj.save_typetree(tree)
        changes.append("dropped unused builtin preloads " + ", ".join(str(ref["m_PathID"]) for ref in dropped))
    return changes


def retarget(source: Path, target: Path) -> list[str]:
    env = UnityPy.load(str(source))
    changes = []
    for item in serialized_files(env):
        if int(item.target_platform) != int(WINDOWS):
            changes.append(f"target {int(item.target_platform)} -> {int(WINDOWS)}")
            # UnityPy writes the raw header field and only re-serializes changed files.
            item.target_platform = WINDOWS
            item._m_target_platform = int(WINDOWS)
            item.mark_changed()
    changes += drop_stale_builtin_preloads(env)
    for obj in env.objects:
        if obj.type.name != "Texture2D":
            continue
        texture = obj.read()
        fmt = TextureFormat(texture.m_TextureFormat)
        if fmt.name not in MOBILE_FORMATS:
            continue
        image = texture.image
        alpha = image.mode in ("RGBA", "LA") and image.getchannel("A").getextrema() != (255, 255)
        new = TextureFormat.RGBA32 if alpha else TextureFormat.RGB24
        texture.set_image(image.convert("RGBA" if alpha else "RGB"), target_format=new)
        texture.save()
        changes.append(f"{texture.m_Name}: {fmt.name} -> {new.name}")
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_bytes(env.file.save(packer="lz4"))
    for item in serialized_files(UnityPy.load(str(target))):
        if int(item.target_platform) != int(WINDOWS):
            raise SystemExit(f"{target}: target platform was not rewritten")
    return changes


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("source", type=Path)
    parser.add_argument("target", type=Path)
    parser.add_argument("--check-shaders", nargs=2, type=Path, metavar=("WINDOWS_SHADER", "ANDROID_SHADER"))
    args = parser.parse_args()
    if args.check_shaders:
        check_shaders(args.source, *args.check_shaders)
    for change in retarget(args.source, args.target):
        print(f"{args.target.name}: {change}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
