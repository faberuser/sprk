#!/usr/bin/env python3
"""Repack the game's built-in player content as Android-loadable asset bundles.

The Windows player data (Unity 6000.0.53f1, the same version as the Android
bootstrap player) holds the built-in scenes, globalgamemanagers.assets (MonoScripts)
and resources.assets. Its serialized layouts match the Android player exactly, so the
objects are reused unchanged except for:

* shader references: Windows shaders are D3D-only, so every PPtr to a Windows shader
  is redirected by name to an OpenGL ES build of the same shader. Those come from the
  original Android client's Unity_2018/Shader.unity3d, extended with the shaders that
  the 2018 Android player kept in its own player data (same 2018.4 Shader layout);
* file references: player file names become archive:/ paths inside the new bundles;
* the target platform field.

Outputs (never commit them; they are game content):
  sprk-shaders.unity3d   2018 shader bundle, CAB-sprkshaders
  sprk-ggm.unity3d       MonoScripts etc., CAB-sprkggm
  sprk-resources.unity3d resources.assets, CAB-sprkres
  sprk-scenes.unity3d    streamed scene bundle (BuildPlayer-<Scene>)
  resources-map.json     Resources container for patch_player_resources.py

usage: python client/android/HybridBootstrap/content/repack_content.py
"""
from __future__ import annotations

import argparse
import copy
import json
import os
import sys
from pathlib import Path

import UnityPy
import UnityPy.helpers.Tpk
from UnityPy.files.ObjectReader import ObjectReader
from UnityPy.helpers.TypeTreeGenerator import TypeTreeGenerator
from UnityPy.helpers import TypeTreeHelper
from UnityPy.streams import EndianBinaryReader

# UnityPy's native reader ignores the alignment flags of corrected MonoBehaviour trees
# (it reads 3 bytes short after every bool); the Python reader honours them.
TypeTreeHelper.read_typetree_boost = None

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from serialized_writer import NEW_OBJECTS, SavedNode  # noqa: E402

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[3]  # sprk/
WORKSPACE = REPO.parent

UNITY_VERSION = "6000.0.53f1"
ANDROID_PLATFORM = 13
SHADER_CAB = "CAB-sprkshaders"
GGM_CAB = "CAB-sprkggm"
RES_CAB = "CAB-sprkres"
# Built-in shaders the Android player carries in its own builtin_extra (always-included).
BUILTIN_EXTRA = "Resources/unity_builtin_extra"
BUILTIN_SHADER_IDS = {"Unlit/Texture": 10752, "Unlit/Transparent": 10750}
DROPPED_SHADERS = {"Vuplex/Android Web Shader", "Vuplex/Default Web Shader"}
# Unity built-in shaders that Windows asset bundles embed (D3D builds). Their GLES 2018
# builds come from original Android bundles that embed them too, and go into the shared
# shader bundle; convert_bundles.py redirects converted bundles to them (shader-map.json).
EMBEDDED_BUILTIN = {
    "Standard": "Creature_Boss_DarkElfCrescent_Facial.unity3d",
    "Particles/Standard Unlit": "Effect_Skill_Hero_Cleo_Re_Skill4_Debuff.unity3d",
}


def arc(cab: str, node: str | None = None) -> str:
    return f"archive:/{cab}/{node or cab}"


def log(*a):
    print(*a, flush=True)


def raw_node(data: bytes) -> EndianBinaryReader:
    node = EndianBinaryReader(data)
    node.flags = 0
    return node


def walk_pptrs(x, out, path=""):
    if isinstance(x, dict):
        if len(x) == 2 and "m_FileID" in x and "m_PathID" in x:
            out.append((path, x))
            return
        for k, v in x.items():
            walk_pptrs(v, out, path + "." + k)
    elif isinstance(x, list):
        for i, v in enumerate(x):
            walk_pptrs(v, out, path + "[]")


def walk_strings(x, fn):
    """Replace string leaves in place via fn(str) -> str|None. Returns True if changed."""
    changed = False
    if isinstance(x, dict):
        for k, v in x.items():
            if isinstance(v, str):
                n = fn(v)
                if n is not None and n != v:
                    x[k] = n
                    changed = True
            else:
                changed |= walk_strings(v, fn)
    elif isinstance(x, list):
        for i, v in enumerate(x):
            if isinstance(v, str):
                n = fn(v)
                if n is not None and n != v:
                    x[i] = n
                    changed = True
            else:
                changed |= walk_strings(v, fn)
    return changed


# Unity's type tree spelling of value types that C# names differently.
UNITY_STRUCT_NAMES = {
    "Color": "ColorRGBA", "Color32": "ColorRGBA", "Vector2": "Vector2f", "Vector3": "Vector3f",
    "Vector4": "Vector4f", "Quaternion": "Quaternionf", "Rect": "Rectf", "Matrix4x4": "Matrix4x4f",
    "Bounds": "AABB", "LayerMask": "BitField", "Vector2Int": "int2_storage", "Vector3Int": "int3_storage",
}
PRIMITIVE_SIZES = {
    "bool": 1, "char": 1, "UInt8": 1, "SInt8": 1, "SInt16": 2, "UInt16": 2, "short": 2, "unsigned short": 2,
    "int": 4, "unsigned int": 4, "SInt32": 4, "UInt32": 4, "float": 4, "Type*": 4,
    "SInt64": 8, "UInt64": 8, "long long": 8, "unsigned long long": 8, "double": 8,
}
ALIGN = 0x4000


def _engine_classes() -> set[str]:
    from UnityPy.enums import ClassIDType
    return {c.name for c in ClassIDType}


ENGINE_CLASSES = _engine_classes()


def normalize_node(node, monobehaviour: bool = False):
    """Make a TPK or generated type tree match what Unity itself writes.

    Unity compares a bundle's tree with the runtime type field by field. Anything that
    differs (type names, array flags, sizes) makes it convert the field instead of
    reading it; converting a PPtr whose declared type differs dereferences the target,
    which fails while loading ("Recursive Serialization is not supported") and leaves
    the script missing. TPK trees only lack TypeFlags; generated MonoBehaviour trees
    also lack sizes and versions and use C# spellings.
    """
    if monobehaviour:
        node.m_Type = "MonoBehaviour"  # Unity names the root by the native class
        node.m_MetaFlag = 0x8000
    for n in node.traverse():
        t = n.m_Type
        kids = n.m_Children or []
        # The generator types an array field by its element (e.g. string[] -> "string"
        # holding an Array of strings); Unity writes such fields as "vector".
        if monobehaviour and len(kids) == 1 and kids[0].m_Type == "Array" and t not in ("vector", "staticvector", "map", "set"):
            elem = kids[0].m_Children[1] if len(kids[0].m_Children) > 1 else None
            if not (t == "string" and elem is not None and elem.m_Type == "char"):
                n.m_Type = t = "vector"
        if t in UNITY_STRUCT_NAMES and monobehaviour:
            n.m_Type = t = UNITY_STRUCT_NAMES[t]
        n.m_TypeFlags = 1 if t in ("Array", "TypelessData") else 0
        if not n.m_Version:
            n.m_Version = 1
        if n.m_MetaFlag is None:
            n.m_MetaFlag = 0
        if getattr(n, "m_RefTypeHash", None) is None:
            n.m_RefTypeHash = 0

    def size(n) -> int:
        kids = n.m_Children or []
        child_sizes = [size(c) for c in kids]
        if n.m_Type in PRIMITIVE_SIZES and not kids:
            result = PRIMITIVE_SIZES[n.m_Type]
        elif n.m_Type.startswith("PPtr<"):
            result = 12
        elif not kids or n.m_TypeFlags or n.m_Type in ("string", "vector", "map", "set", "staticvector") or -1 in child_sizes:
            result = -1
        else:
            total = 0
            for c, cs in zip(kids, child_sizes):
                total += cs
                if c.m_MetaFlag & ALIGN:
                    total = (total + 3) & ~3
            result = total
        if not n.m_ByteSize or monobehaviour:
            n.m_ByteSize = result
        return n.m_ByteSize

    size(node)
    if monobehaviour:
        node.m_ByteSize = -1
    for i, n in enumerate(node.traverse()):
        n.m_Index = i
    return node


def new_object(sf, template_obj: ObjectReader, path_id: int, data: bytes) -> ObjectReader:
    """Clone template_obj's type into sf and add an object with the given raw data."""
    typ = template_obj.serialized_type
    idx = next((i for i, t in enumerate(sf.types) if t.class_id == typ.class_id and t.script_type_index == typ.script_type_index), None)
    if idx is None:
        sf.types.append(copy.copy(typ))
        idx = len(sf.types) - 1
    obj = copy.copy(template_obj)
    obj.assets_file = sf
    obj.path_id = path_id
    obj.type_id = idx
    obj.serialized_type = sf.types[idx]
    obj.data = data
    NEW_OBJECTS.add(id(obj))  # appended after the original data by save_serialized
    sf.objects[path_id] = obj
    return obj


def serialize_with(template_obj: ObjectReader, tree: dict, nodes=None) -> bytes:
    clone = copy.copy(template_obj)
    clone.data = None
    clone.save_typetree(tree, nodes=nodes)
    return clone.data


def clone_node(n):
    from UnityPy.helpers.TypeTreeNode import TypeTreeNode
    c = TypeTreeNode(m_Level=n.m_Level, m_Type=n.m_Type, m_Name=n.m_Name, m_ByteSize=n.m_ByteSize, m_Version=n.m_Version)
    for k in ("m_TypeFlags", "m_MetaFlag", "m_Index", "m_RefTypeHash"):
        setattr(c, k, getattr(n, k, None))
    c.m_Children = [clone_node(x) for x in n.m_Children]
    return c


BASE_FIELDS = ["m_GameObject", "m_Enabled", "m_Script", "m_Name"]


def script_node(o, version):
    """Unity's MonoBehaviour tree for o's script.

    The generator's script fields match what Unity writes, but its base fields do not:
    m_Name lacks its Array child and the alignment flags sit on the wrong fields, which
    shifts m_Script by three bytes (Unity then reports "The referenced script (Unknown)
    on this Behaviour is missing" and UnityPy misparses every field). So the base comes
    from the exact Unity 6 MonoBehaviour tree and only the script fields are generated.
    """
    gen = o._get_typetree_node()
    if [c.m_Name for c in gen.m_Children[:4]] != BASE_FIELDS:
        raise ValueError("unexpected generated base fields")
    root = clone_node(UnityPy.helpers.Tpk.get_typetree_node(114, version))
    if [c.m_Name for c in root.m_Children] != BASE_FIELDS:
        raise ValueError("unexpected MonoBehaviour base tree")
    root.m_Children += [clone_node(c) for c in gen.m_Children[4:]]
    return normalize_node(root, monobehaviour=True)


def base_script_node(version):
    return normalize_node(clone_node(UnityPy.helpers.Tpk.get_typetree_node(114, version)), monobehaviour=True)


# ----------------------------------------------------------------------------- shaders

def shader_name(obj) -> str:
    try:
        return obj.read().m_ParsedForm.m_Name
    except Exception:
        return obj.read().m_Name


def build_shader_bundle(android_bundle: Path, android_player: Path, needed: set[str], out: Path,
                        embedded_sources: list[Path] = ()) -> dict[str, int]:
    env = UnityPy.load(str(android_bundle))
    bundle = next(iter(env.files.values()))
    cab_name, sf = next((k, v) for k, v in bundle.files.items() if hasattr(v, "objects"))
    by_name: dict[str, int] = {}
    shader_template = None
    for pid, o in sf.objects.items():
        if o.type.name == "Shader":
            by_name.setdefault(shader_name(o), pid)
            shader_template = shader_template or o
    log(f"shader bundle: {len(by_name)} shaders in {android_bundle.name}")

    # Shaders the 2018 Android player kept in its own data (one serialized file per resource).
    player_index: dict[str, tuple[Path, int]] = {}
    for f in sorted(android_player.iterdir()):
        if f.is_file() and (len(f.name) == 32 or f.name.startswith("sharedassets") and f.suffix == ".assets" or f.name == "globalgamemanagers.assets"):
            try:
                penv = UnityPy.load(str(f))
            except Exception:
                continue
            for psf in penv.files.values():
                if not hasattr(psf, "objects"):
                    continue
                for pid, o in psf.objects.items():
                    if o.type.name == "Shader":
                        player_index.setdefault(shader_name(o), (f, pid, o, psf))
    # Built-in shaders embedded in original Android bundles (same 2018.4 Shader layout).
    for f in embedded_sources:
        for bf in UnityPy.load(str(f)).files.values():
            for psf in getattr(bf, "files", {}).values():
                if not hasattr(psf, "objects"):
                    continue
                for pid, o in psf.objects.items():
                    if o.type.name == "Shader":
                        player_index.setdefault(shader_name(o), (f, pid, o, psf))

    ext_paths = [e.path for e in sf.externals]

    def ext_index(path: str) -> int:
        low = [p.lower() for p in ext_paths]
        if path.lower() in low:
            return low.index(path.lower()) + 1
        e = copy.copy(sf.externals[0]) if sf.externals else None
        if e is None:
            raise RuntimeError("shader bundle has no externals to clone")
        e.path = path
        sf.externals.append(e)
        ext_paths.append(path)
        return len(ext_paths)

    next_pid = max(sf.objects) + 1
    added: list[int] = []

    def ensure(name: str) -> int | None:
        nonlocal next_pid
        if name in by_name:
            return by_name[name]
        if name not in player_index:
            return None
        f, src_pid, src, psf = player_index[name]
        pid = next_pid
        next_pid += 1
        by_name[name] = pid
        obj = new_object(sf, shader_template, pid, src.get_raw_data())
        # Same 2018.4 Shader layout (identical type hash); parse with the bundle's tree.
        tree = src.read_typetree(nodes=shader_template._get_typetree_node(), check_read=True)
        refs = []
        walk_pptrs(tree, refs)
        src_ext = [e.path for e in psf.externals]
        for _, p in refs:
            if p["m_FileID"] == 0 and p["m_PathID"] == 0:
                continue
            target = src_ext[p["m_FileID"] - 1] if p["m_FileID"] > 0 else None
            if target and ("unity default resources" in target.lower() or "unity_builtin_extra" in target.lower()):
                p["m_FileID"] = ext_index(target)
                continue
            # Another player shader: resolve by name.
            tf = android_player / target if target else f
            dep = None
            for n, (pf, ppid, _, _) in player_index.items():
                if pf == tf and ppid == p["m_PathID"]:
                    dep = n
                    break
            dep_pid = ensure(dep) if dep else None
            p["m_FileID"], p["m_PathID"] = (0, dep_pid) if dep_pid else (0, 0)
        obj.data = serialize_with(obj, tree)
        added.append(pid)
        return pid

    missing = []
    for n in sorted(needed):
        if ensure(n) is None:
            missing.append(n)
    if missing:
        log(f"shader bundle: no Android build for {missing}")

    ab = next(o for o in sf.objects.values() if o.type.name == "AssetBundle")
    tree = ab.read_typetree()
    for i, pid in enumerate(added):
        tree["m_Container"].append((f"assets/sprk/shaders/{pid}.shader", {"preloadIndex": len(tree["m_PreloadTable"]), "preloadSize": 1, "asset": {"m_FileID": 0, "m_PathID": pid}}))
        tree["m_PreloadTable"].append({"m_FileID": 0, "m_PathID": pid})
    tree["m_Name"] = "sprk-shaders"
    if "m_AssetBundleName" in tree:
        tree["m_AssetBundleName"] = "sprk-shaders"
    ab.save_typetree(tree)

    bundle.files = {SHADER_CAB if k == cab_name else k.replace(cab_name, SHADER_CAB): v for k, v in bundle.files.items()}
    out.write_bytes(bundle.save(packer="none"))
    log(f"shader bundle: +{len(added)} player shaders -> {out.name} ({out.stat().st_size >> 20} MB)")
    return by_name


# ----------------------------------------------------------------------------- player files

class Repacker:
    def __init__(self, win: Path, shader_pids: dict[str, int], gen: TypeTreeGenerator, scenes: list[str]):
        self.win = win
        self.shader_pids = shader_pids
        self.gen = gen
        self.scenes = scenes  # BuildSettings order, scene names
        self.files: dict[str, object] = {}
        self.win_shaders: dict[tuple[str, int], str] = {}
        self.stats: dict[str, int] = {}
        # (file, path ID) -> new path ID. Unity looks for the AssetBundle object at a fixed
        # path ID (1 in an asset bundle, 2 in a scene bundle's first .sharedAssets), so
        # whatever the player file kept there moves and every reference to it follows.
        self.relocate: dict[tuple[str, int], int] = {}
        self.node_cache: dict[tuple[str, int], object] = {}

    def node_for(self, name: str, o):
        """Corrected script tree for a MonoBehaviour (None: engine type, default tree;
        False: no usable script tree). Validated on the first object of each type."""
        if o.type.name != "MonoBehaviour":
            return None
        key = (name, o.type_id)
        if key not in self.node_cache:
            try:
                node = script_node(o, o.assets_file.version)
                o.read_typetree(nodes=node)  # must consume the object exactly
            except Exception:
                node = False
            self.node_cache[key] = node
        return self.node_cache[key]

    def reserve(self, name: str, path_id: int):
        sf = self.files[name]
        if path_id in sf.objects:
            self.relocate[(name, path_id)] = max(sf.objects) + 1

    # Player file name -> archive path of the serialized file that replaces it.
    def target_of(self, name: str) -> str | None:
        n = name.split("/")[-1].lower()
        if n == "globalgamemanagers.assets":
            return arc(GGM_CAB)
        if n == "resources.assets":
            return arc(RES_CAB)
        if n.startswith("sharedassets") and n.endswith(".assets"):
            i = int(n[len("sharedassets"):-len(".assets")])
            return arc(f"BuildPlayer-{self.scenes[i]}", f"BuildPlayer-{self.scenes[i]}.sharedAssets")
        return None

    def stream_target(self, value: str) -> str | None:
        n = value.split("/")[-1]
        low = n.lower()
        table = {"resources.assets.ress": arc(RES_CAB, RES_CAB + ".resS"),
                 "resources.resource": arc(RES_CAB, RES_CAB + ".resource"),
                 "globalgamemanagers.assets.ress": arc(GGM_CAB, GGM_CAB + ".resS")}
        if low in table:
            return table[low]
        for i, s in enumerate(self.scenes):
            node = f"BuildPlayer-{s}.sharedAssets"
            if low == f"sharedassets{i}.assets.ress":
                return arc(f"BuildPlayer-{s}", node + ".resS")
            if low == f"sharedassets{i}.resource":
                return arc(f"BuildPlayer-{s}", node + ".resource")
        return None

    def load(self, names: list[str]):
        for n in names:
            env = UnityPy.load(str(self.win / n))
            env.typetree_generator = self.gen
            sf = next(iter(env.files.values()))
            self.files[n] = sf
            for pid, o in sf.objects.items():
                if o.type.name == "Shader":
                    self.win_shaders[(n, pid)] = shader_name(o)

    def convert(self, name: str):
        sf = self.files[name]
        sf._m_target_platform = ANDROID_PLATFORM
        old_ext = [e.path for e in sf.externals]
        # Redirect targets are appended as new externals; existing ones are renamed only
        # after the walk, because MonoBehaviour parsing follows them to the MonoScripts.
        tmpl_ext = sf.externals[0]

        def add_ext(path: str) -> int:
            for i, e in enumerate(sf.externals):
                if e.path.lower() == path.lower():
                    return i + 1
            e = copy.copy(tmpl_ext)
            # Built-in resource files are identified by fixed GUIDs; archive paths use zero.
            guid = b"\x00" * 8 + b"\x0f" + b"\x00" * 7 if path == BUILTIN_EXTRA else b"\x00" * 16
            e.path, e.type, e.guid = path, 0, guid
            sf.externals.append(e)
            return len(sf.externals)

        shader_fid = add_ext(arc(SHADER_CAB))
        builtin_fid = add_ext(BUILTIN_EXTRA)
        names = [name] + [p.split("/")[-1] for p in old_ext]

        changed = errors = 0
        for pid, o in list(sf.objects.items()):
            t = o.type.name
            if t in ("Shader", "MonoScript"):
                continue
            try:
                node = self.node_for(name, o)
                if node is False:
                    raise ValueError("no script tree")
                tree = o.read_typetree(nodes=node)
            except Exception:
                errors += 1
                continue
            dirty = False
            refs = []
            walk_pptrs(tree, refs)
            for path, p in refs:
                fid = p["m_FileID"]
                if fid >= len(names):
                    continue
                key = (names[fid], p["m_PathID"])
                if key in self.relocate:
                    p["m_PathID"] = self.relocate[key]
                    dirty = True
                    continue
                sname = self.win_shaders.get(key)
                if sname is None:
                    continue
                if sname in BUILTIN_SHADER_IDS:
                    p["m_FileID"], p["m_PathID"] = builtin_fid, BUILTIN_SHADER_IDS[sname]
                elif sname in self.shader_pids:
                    p["m_FileID"], p["m_PathID"] = shader_fid, self.shader_pids[sname]
                else:
                    p["m_FileID"], p["m_PathID"] = 0, 0
                    self.stats["dropped shader refs"] = self.stats.get("dropped shader refs", 0) + 1
                dirty = True
            dirty |= walk_strings(tree, self.stream_target)
            if dirty:
                raw = o.get_raw_data()
                if serialize_with(o, o.read_typetree(nodes=node), node) != raw:
                    raise RuntimeError(f"{name}:{pid} {t}: type tree does not round-trip; refusing to rewrite")
                o.save_typetree(tree, nodes=node)
                changed += 1
        self.embed_type_trees(name, sf)
        for (fname, old), new in self.relocate.items():
            if fname == name:
                o = sf.objects.pop(old)
                o.path_id = new
                sf.objects[new] = o
        for e in sf.externals[: len(old_ext)]:
            t = self.target_of(e.path)
            if t:
                e.path, e.type, e.guid = t, 0, b"\x00" * 16
        # The D3D shader objects are never referenced any more.
        removed = 0
        for pid in [pid for pid, o in sf.objects.items() if o.type.name == "Shader"]:
            del sf.objects[pid]
            removed += 1
        log(f"{name}: rewrote {changed} objects, removed {removed} shaders, {errors} unparsed")

    def embed_type_trees(self, name: str, sf):
        """Write type trees so Unity reads the objects with its field-matching (safe) reader.

        Without them Unity requires the stored type and script hashes to equal the
        runtime ones and rejects the bundle ("not compatible with this newer version",
        "references scripts that are not compatible"). The trees are the ones every
        object was just parsed with: UnityPy's Unity 6 engine trees, and trees generated
        from the game's managed assemblies for each MonoBehaviour script.
        """
        by_type: dict[int, ObjectReader] = {}
        for o in sf.objects.values():
            by_type.setdefault(o.type_id, o)
        fallback = 0
        for idx, typ in enumerate(sf.types):
            if typ.node is None:
                o = by_type.get(idx)
                if typ.class_id == 114:
                    node = self.node_for(name, o) if o is not None else False
                    if node is False:
                        # Script whose fields cannot be generated: base fields only.
                        node = base_script_node(sf.version)
                        fallback += 1
                else:
                    node = normalize_node(UnityPy.helpers.Tpk.get_typetree_node(typ.class_id, sf.version))
                typ.node = node
            if typ.type_dependencies is None:
                typ.type_dependencies = ()
        for typ in sf.ref_types or []:
            if typ.node is None:
                raise RuntimeError(f"{name}: SerializeReference type without a tree")
        sf._enable_type_tree = True
        if fallback:
            log(f"{name}: {fallback} script types without generated fields (base fields only)")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--windows-data", default=str(WORKSPACE / "sprk-client" / "King's Raid_Data"))
    ap.add_argument("--android-data", default=str(REPO / "artifacts" / "android-bootstrap" / "apk-data"),
                    help="assets/bin/Data extracted from the original 4.88.8 APK")
    ap.add_argument("--android-shaders", default=str(WORKSPACE / "sprk-apk" / "Assetbundle" / "Unity_2018" / "Shader.unity3d"))
    ap.add_argument("--templates", default=str(REPO / "artifacts" / "android-bootstrap" / "templates"))
    ap.add_argument("--out", default=str(REPO / "artifacts" / "android-bootstrap" / "content"))
    args = ap.parse_args()

    win = Path(args.windows_data)
    out = Path(args.out)
    out.mkdir(parents=True, exist_ok=True)

    gen = TypeTreeGenerator(UNITY_VERSION)
    gen.load_local_dll_folder(str(win / "Managed"))

    ggm = UnityPy.load(str(win / "globalgamemanagers"))
    ggm_sf = next(iter(ggm.files.values()))
    build = res_mgr = None
    for o in ggm.objects:
        if o.type.name == "BuildSettings":
            build = o.read_typetree(check_read=False)
        elif o.type.name == "ResourceManager":
            res_mgr = o.read_typetree()
    scenes = [Path(s).stem for s in build["scenes"]]
    log("scenes:", scenes)

    names = ["globalgamemanagers.assets", "resources.assets"]
    names += [f"sharedassets{i}.assets" for i in range(len(scenes))] + [f"level{i}" for i in range(len(scenes))]
    rp = Repacker(win, {}, gen, scenes)
    rp.load(names)
    # The player's own preload list is meaningless in a bundle; its slot (path ID 1) is
    # where Unity expects the AssetBundle object.
    ggm_assets = rp.files["globalgamemanagers.assets"]
    for pid in [pid for pid, o in ggm_assets.objects.items() if o.type.name == "PreloadData"]:
        del ggm_assets.objects[pid]
    rp.reserve("globalgamemanagers.assets", 1)
    rp.reserve("resources.assets", 1)
    rp.reserve("sharedassets0.assets", 2)  # 1 is the scene's PreloadData, as in Unity's layout
    needed = set(rp.win_shaders.values()) - set(BUILTIN_SHADER_IDS) - DROPPED_SHADERS
    android_bundles = Path(args.android_shaders).parent.parent
    rp.shader_pids = build_shader_bundle(Path(args.android_shaders), Path(args.android_data), needed | set(EMBEDDED_BUILTIN),
                                         out / "sprk-shaders.unity3d", [android_bundles / f for f in EMBEDDED_BUILTIN.values()])
    # Shader name -> path ID in sprk-shaders.unity3d, for convert_bundles.py.
    (out / "shader-map.json").write_text(json.dumps(rp.shader_pids, indent=0, sort_keys=True), encoding="utf-8")
    for n in names:
        rp.convert(n)
    if rp.stats:
        log("stats:", rp.stats)

    tmpl_assets = UnityPy.load(str(Path(args.templates) / "template_assets"))
    tmpl_scenes = UnityPy.load(str(Path(args.templates) / "template_scenes"))
    ab_assets = next(o for o in tmpl_assets.objects if o.type.name == "AssetBundle")
    ab_scenes = next(o for o in tmpl_scenes.objects if o.type.name == "AssetBundle")
    bundle = next(iter(tmpl_assets.files.values()))

    def read_side(n: str) -> bytes | None:
        p = win / n
        return p.read_bytes() if p.exists() else None

    def asset_bundle_tree(name: str, container=(), preload=()) -> dict:
        t = ab_assets.read_typetree()
        t.update(m_Name=name, m_Container=list(container), m_PreloadTable=list(preload), m_Dependencies=[], m_SceneHashes=[])
        if "m_AssetBundleName" in t:
            t["m_AssetBundleName"] = name
        return t

    def save_bundle(path: Path, nodes: dict):
        bundle.files = nodes
        path.write_bytes(bundle.save(packer="none"))
        log(f"wrote {path.name} ({path.stat().st_size >> 20} MB)")

    # globalgamemanagers.assets: MonoScripts and a few shared objects.
    sf = rp.files["globalgamemanagers.assets"]
    # A player loads every MonoScript at startup; a bundle only loads what its container
    # lists. The bootstrap loads all of these before any MonoBehaviour is deserialized
    # (otherwise: "Recursive Serialization is not supported" and missing scripts).
    scripts = sorted(pid for pid, o in sf.objects.items() if o.type.name == "MonoScript")
    container = [(f"monoscript/{pid}", {"preloadIndex": i, "preloadSize": 1, "asset": {"m_FileID": 0, "m_PathID": pid}}) for i, pid in enumerate(scripts)]
    preload = [{"m_FileID": 0, "m_PathID": pid} for pid in scripts]
    new_object(sf, ab_assets, 1, serialize_with(ab_assets, asset_bundle_tree("sprk-ggm", container, preload)))
    nodes = {GGM_CAB: SavedNode(sf)}
    if (d := read_side("globalgamemanagers.assets.resS")) is not None:
        nodes[GGM_CAB + ".resS"] = raw_node(d)
    save_bundle(out / "sprk-ggm.unity3d", nodes)

    # resources.assets + the Resources container (also exported for the player patch).
    ggm_ext = ["globalgamemanagers"] + [e.path.split("/")[-1] for e in ggm_sf.externals]
    shader_lookup = rp.win_shaders

    def resolve(p) -> dict | None:
        fname = ggm_ext[p["m_FileID"]] if p["m_FileID"] < len(ggm_ext) else None
        if fname is None:
            return None
        sname = shader_lookup.get((fname, p["m_PathID"]))
        if sname is not None:
            if sname in rp.shader_pids:
                return {"file": arc(SHADER_CAB), "pathID": rp.shader_pids[sname]}
            if sname in BUILTIN_SHADER_IDS:
                return {"file": BUILTIN_EXTRA, "pathID": BUILTIN_SHADER_IDS[sname]}
            return None
        target = rp.target_of(fname) or fname
        return {"file": target, "pathID": rp.relocate.get((fname, p["m_PathID"]), p["m_PathID"])}

    entries, dropped = [], 0
    for path, p in res_mgr["m_Container"]:
        r = resolve(p)
        if r is None:
            dropped += 1
            continue
        entries.append({"path": path, **r})
    deps = []
    for d in res_mgr.get("m_DependentAssets", []):
        o = resolve(d["m_Object"])
        if o is None:
            continue
        ds = [x for x in (resolve(q) for q in d["m_Dependencies"]) if x is not None]
        deps.append({"object": o, "dependencies": ds})
    (out / "resources-map.json").write_text(json.dumps({"container": entries, "dependents": deps,
                                                            "shaders": [{"name": n, "file": arc(SHADER_CAB), "pathID": pid} for n, pid in sorted(rp.shader_pids.items())]}, indent=0))
    log(f"resources map: {len(entries)} entries ({dropped} dropped), {len(deps)} dependency lists")

    sf = rp.files["resources.assets"]
    local = [(e["path"], e["pathID"]) for e in entries if e["file"] == arc(RES_CAB)]
    container = [(path, {"preloadIndex": 0, "preloadSize": 0, "asset": {"m_FileID": 0, "m_PathID": pid}}) for path, pid in local]
    new_object(sf, ab_assets, 1, serialize_with(ab_assets, asset_bundle_tree("sprk-resources", container)))
    nodes = {RES_CAB: SavedNode(sf)}
    for side, node in (("resources.assets.resS", ".resS"), ("resources.resource", ".resource")):
        if (d := read_side(side)) is not None:
            nodes[RES_CAB + node] = raw_node(d)
    save_bundle(out / "sprk-resources.unity3d", nodes)
    del nodes, sf

    # Scenes: BuildPlayer-<Scene> (levelN) + BuildPlayer-<Scene>.sharedAssets (sharedassetsN).
    nodes = {}
    for i, s in enumerate(scenes):
        shared = rp.files[f"sharedassets{i}.assets"]
        level = rp.files[f"level{i}"]
        if i == 0:
            t = ab_scenes.read_typetree()
            t.update(m_Name="sprk-scenes", m_PreloadTable=[], m_Dependencies=[], m_SceneHashes=[],
                     m_Container=[(sc, {"preloadIndex": 0, "preloadSize": 0, "asset": {"m_FileID": 0, "m_PathID": 0}}) for sc in build["scenes"]])
            if "m_AssetBundleName" in t:
                t["m_AssetBundleName"] = "sprk-scenes"
            new_object(shared, ab_scenes, 2, serialize_with(ab_scenes, t))
        node = f"BuildPlayer-{s}"
        nodes[node + ".sharedAssets"] = SavedNode(shared)
        nodes[node] = SavedNode(level)
        for side, suffix in ((f"sharedassets{i}.assets.resS", ".sharedAssets.resS"), (f"sharedassets{i}.resource", ".sharedAssets.resource")):
            if (d := read_side(side)) is not None:
                nodes[node + suffix] = raw_node(d)
    save_bundle(out / "sprk-scenes.unity3d", nodes)
    log("done ->", out)


if __name__ == "__main__":
    sys.exit(main())
