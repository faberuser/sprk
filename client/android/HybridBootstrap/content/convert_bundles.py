#!/usr/bin/env python3
"""Convert Windows asset bundles of the game into Android asset bundles.

Used by build_android_assets.py for bundles that SPRK changed on Windows or that the
original Android set lacks (including the whole Low tier). Like repack_content.py, the
objects are reused unchanged except for what is platform-specific:

* the target platform of every serialized file (Windows 19 -> Android 13);
* textures in desktop formats (DXT/BC) are transcoded to ETC2, which every OpenGL ES 3
  device decodes in hardware (Unity would otherwise decompress them in software), and
  uncompressed colour textures (the Windows LowLargeSpec UI atlases are 4096x4096 RGBA32)
  are compressed to ETC2 too, as the original Android build did; textures scripts read
  (readable) and small ones stay as they are;
* shaders: references into the shared shader bundle (CAB-Shader) are kept, because the
  Android CAB-Shader has the same shaders under the same path IDs. Unity built-in
  shaders that a bundle embeds as D3D builds are redirected to GLES builds: the legacy
  ones to the player's unity_builtin_extra, Standard and Particles/Standard Unlit to
  sprk-shaders.unity3d (shader-map.json from repack_content.py); the D3D copies are
  removed.

usage: python client/android/HybridBootstrap/content/convert_bundles.py <windows.unity3d> <android.unity3d>
       [--shader-map client/android/HybridBootstrap/content/shader-map.json]
"""
from __future__ import annotations

import argparse
import copy
import json
from pathlib import Path

import UnityPy
from PIL import Image

# Bump when the conversion changes, so build_android_assets.py converts again.
VERSION = 2
ANDROID_PLATFORM = 13
SHADER_CAB = "CAB-sprkshaders"
BUILTIN_EXTRA = "Resources/unity_builtin_extra"
# unity_builtin_extra path IDs (Unity 6000.0.53f1); the APK always includes them.
BUILTIN_SHADER_IDS = {
    "Unlit/Texture": 10752,
    "Unlit/Transparent": 10750,
    "Legacy Shaders/Transparent/Diffuse": 30,
    "Legacy Shaders/Transparent/VertexLit": 34,
}
# TextureFormat values.
DXT1, DXT5, BC6H, BC7, BC4, BC5, DXT1_CRUNCHED, DXT5_CRUNCHED = 10, 12, 24, 25, 26, 27, 28, 29
RGB24, RGBA32, ARGB32, BGRA32 = 3, 4, 5, 14
ETC2_RGB, ETC2_RGBA8 = 45, 47
DESKTOP_FORMATS = {DXT1, DXT5, BC6H, BC7, BC4, BC5, DXT1_CRUNCHED, DXT5_CRUNCHED}
UNCOMPRESSED_COLOUR = {RGB24, RGBA32, ARGB32, BGRA32}
# Uncompressed textures at least this large (either side) are compressed.
COMPRESS_MIN_SIZE = 256


def arc(cab: str) -> str:
    return f"archive:/{cab}/{cab}"


def shader_name(obj) -> str:
    try:
        return obj.read().m_ParsedForm.m_Name
    except Exception:
        return obj.read().m_Name


def has_alpha(img: Image.Image) -> bool:
    return img.mode in ("RGBA", "LA") and img.getchannel("A").getextrema() != (255, 255)


STREAMING_TYPES = ("Texture2D", "Mesh", "AudioClip", "VideoClip", "Font")


def streams_of(obj) -> set[str]:
    """Names of the bundle's stream files (.resS/.resource) an object reads from."""
    tree = obj.read_typetree()
    out = set()
    for key in ("m_StreamData", "m_Resource", "m_ExternalResources"):
        value = tree.get(key) or {}
        path = value.get("path") or value.get("m_Source")
        if path:
            out.add(path.split("/")[-1])
    return out


def transcode(obj, stats: dict) -> bool:
    tex = obj.read()
    fmt = int(tex.m_TextureFormat)
    if tex.m_Width == 0 or tex.m_Height == 0:
        return False
    uncompressed = (fmt in UNCOMPRESSED_COLOUR and not getattr(tex, "m_IsReadable", False)
                    and max(tex.m_Width, tex.m_Height) >= COMPRESS_MIN_SIZE)
    if fmt not in DESKTOP_FORMATS and not uncompressed:
        return False
    img = tex.image
    # Single/dual-channel and HDR data (BC4/5/6H: masks, normal maps) stay exact.
    if fmt in (BC4, BC5, BC6H):
        target = RGBA32
    else:
        target = ETC2_RGBA8 if has_alpha(img) else ETC2_RGB
    mips = max(1, int(getattr(tex, "m_MipCount", 1) or 1))
    tex.set_image(img, target_format=target, mipmap_count=mips)
    tex.save()
    stats["textures"] = stats.get("textures", 0) + 1
    return True


def convert(data: bytes, shader_map: dict[str, int]) -> tuple[bytes, dict]:
    """Converted bundle bytes and statistics; raises if something cannot be converted."""
    env = UnityPy.load(data)
    bundle = next(iter(env.files.values()))
    stats: dict = {}
    # Streams still read after the conversion (UnityPy returns an object's original data
    # until the bundle is saved, so they are collected from the unchanged objects).
    used: set[str] = set()
    for sf in [f for f in bundle.files.values() if hasattr(f, "objects")]:
        sf._m_target_platform = ANDROID_PLATFORM
        for o in list(sf.objects.values()):
            if o.type.name not in STREAMING_TYPES:
                continue
            if not (o.type.name == "Texture2D" and transcode(o, stats)):
                used |= streams_of(o)
        local_shaders = {pid: shader_name(o) for pid, o in sf.objects.items() if o.type.name == "Shader"}
        if local_shaders:
            redirect_shaders(sf, local_shaders, shader_map, stats)
    for name in [n for n in bundle.files if n.endswith(".resS") and n not in used]:
        del bundle.files[name]
        stats["dropped streams"] = stats.get("dropped streams", 0) + 1
    return bundle.save(packer="lz4"), stats


def redirect_shaders(sf, local: dict[int, str], shader_map: dict[str, int], stats: dict) -> None:
    def new_external():
        if sf.externals:
            return copy.copy(sf.externals[0])
        # A bundle without any external reference: build one (UnityPy's constructor
        # only reads from a file).
        from UnityPy.files.SerializedFile import FileIdentifier
        e = object.__new__(FileIdentifier)
        e.temp_empty = ""
        return e

    def add_ext(path: str) -> int:
        for i, e in enumerate(sf.externals):
            if e.path.lower() == path.lower():
                return i + 1
        e = new_external()
        guid = b"\x00" * 8 + b"\x0f" + b"\x00" * 7 if path == BUILTIN_EXTRA else b"\x00" * 16
        e.path, e.type, e.guid = path, 0, guid
        sf.externals.append(e)
        return len(sf.externals)

    target: dict[int, tuple[int, int]] = {}
    for pid, name in local.items():
        if name in BUILTIN_SHADER_IDS:
            target[pid] = (add_ext(BUILTIN_EXTRA), BUILTIN_SHADER_IDS[name])
        elif name in shader_map:
            target[pid] = (add_ext(arc(SHADER_CAB)), shader_map[name])
        else:
            raise ValueError(f"no GLES build for embedded shader {name!r}")

    def fix(p: dict) -> bool:
        if p.get("m_FileID") == 0 and p.get("m_PathID") in target:
            p["m_FileID"], p["m_PathID"] = target[p["m_PathID"]]
            return True
        return False

    for o in list(sf.objects.values()):
        if o.type.name == "Material":
            tree = o.read_typetree()
            if fix(tree["m_Shader"]):
                o.save_typetree(tree)
                stats["materials"] = stats.get("materials", 0) + 1
        elif o.type.name == "AssetBundle":
            tree = o.read_typetree()
            changed = False
            for p in tree["m_PreloadTable"]:
                changed |= fix(p)
            for _, info in tree["m_Container"]:
                changed |= fix(info["asset"])
            if changed:
                o.save_typetree(tree)
    for pid in local:
        del sf.objects[pid]
    stats["embedded shaders"] = stats.get("embedded shaders", 0) + len(local)


def main() -> None:
    here = Path(__file__).resolve()
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("source", type=Path)
    ap.add_argument("out", type=Path)
    ap.add_argument("--shader-map", type=Path,
                    default=here.parent / "shader-map.json")
    args = ap.parse_args()
    shader_map = json.loads(args.shader_map.read_text(encoding="utf-8")) if args.shader_map.exists() else {}
    data, stats = convert(args.source.read_bytes(), shader_map)
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_bytes(data)
    print(f"{args.out}: {len(data):,} bytes {stats}")


if __name__ == "__main__":
    main()
