#!/usr/bin/env python3
"""Point the Android player's Resources table at the repacked content bundles.

Resources.Load resolves names through the player's ResourceManager (in
globalgamemanagers). This fills it with the game's Resources container produced by
repack_content.py, with every entry referencing an object inside an archive:/ bundle
file. Unity dereferences those lazily, so Resources.Load / LoadAsync work unchanged
once the bootstrap has loaded the bundles, and the game code needs no Android path.

Run by SprkAndroidBuild on the generated Gradle project:
  python -I patch_player_resources.py <unityLibrary>/src/main/assets/bin/Data/globalgamemanagers <resources-map.json>
"""
import copy
import json
import sys

import UnityPy


def main(ggm_path: str, map_path: str) -> int:
    data = json.load(open(map_path, encoding="utf-8"))
    env = UnityPy.load(ggm_path)
    sf = next(iter(env.files.values()))
    rm = next(o for o in sf.objects.values() if o.type.name == "ResourceManager")
    tree = rm.read_typetree()
    if tree["m_Container"]:
        print(f"patch_player_resources: ResourceManager already has {len(tree['m_Container'])} entries; leaving it")
        return 0

    template = sf.externals[0]
    index = {e.path.lower(): i + 1 for i, e in enumerate(sf.externals)}

    def fid(path: str) -> int:
        key = path.lower()
        if key not in index:
            e = copy.copy(template)
            e.path, e.type, e.guid = path, 0, b"\x00" * 16
            sf.externals.append(e)
            index[key] = len(sf.externals)
        return index[key]

    def pptr(r):
        return {"m_FileID": fid(r["file"]), "m_PathID": r["pathID"]}

    tree["m_Container"] = [(e["path"], pptr(e)) for e in data["container"]]
    tree["m_DependentAssets"] = [{"m_Object": pptr(d["object"]), "m_Dependencies": [pptr(x) for x in d["dependencies"]]} for d in data["dependents"]]
    rm.save_typetree(tree)

    # Shader.Find resolves names through the ScriptMapper; a player only lists its own
    # (built-in / always-included) shaders there. Add the GLES shader bundle by name.
    sm = next(o for o in sf.objects.values() if o.type.name == "ScriptMapper")
    smt = sm.read_typetree()
    entries = smt["m_Shaders"]["m_ObjectToName"]
    resolved = {name for ptr, name in entries if ptr["m_PathID"] != 0}
    entries[:] = [(ptr, name) for ptr, name in entries if ptr["m_PathID"] != 0 or name not in {x["name"] for x in data.get("shaders", [])}]
    added = 0
    for x in data.get("shaders", []):
        if x["name"] not in resolved:
            entries.append((pptr(x), x["name"]))
            added += 1
    sm.save_typetree(smt)
    # Serialize fully before opening for write: UnityPy reads the source lazily.
    data = sf.save()
    with open(ggm_path, "wb") as f:
        f.write(data)
    print(f"patch_player_resources: {len(tree['m_Container'])} resources, {len(tree['m_DependentAssets'])} dependency lists, "
          f"{added} shader names, {len(sf.externals)} externals -> {ggm_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main(*sys.argv[1:3]))
