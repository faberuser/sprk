"""Restore three stripped native Team Level buff behaviours from matching pet layouts.

The native classes have identical serialized fields. Keep the Team Level artwork,
hierarchy, events and object IDs, and remap every copied local object reference.
"""
import argparse
import copy
import hashlib
import struct
from pathlib import Path

import UnityPy


def restore(asset, scripts, output=None):
    file = next(iter(UnityPy.load(str(asset)).files.values()))
    globals_file = next(iter(UnityPy.load(str(scripts)).files.values()))
    script_ids = {o.read().m_ClassName: o.path_id for o in globals_file.objects.values()
                  if o.type.name == "MonoScript"}
    global_id = next(i + 1 for i, e in enumerate(file.externals)
                     if e.path.endswith("globalgamemanagers.assets"))
    names = {o.path_id: o.read().m_Name for o in file.objects.values()
             if o.type.name == "GameObject"}

    def components(go):
        return [file.objects[c.component.m_PathID] for c in file.objects[go].read().m_Component]

    def hierarchy(go, path=""):
        result = {path: go}
        transform = next(o for o in components(go) if o.type.name in ("Transform", "RectTransform"))
        for child in transform.read().m_Children:
            child_go = child.read().m_GameObject.m_PathID
            child_path = path + "/" + names[child_go]
            if child_path in result:
                raise ValueError("Ambiguous hierarchy path")
            result.update(hierarchy(child_go, child_path))
        return result

    def signature(obj):
        if obj.type.name != "MonoBehaviour":
            return obj.type.name
        return struct.unpack_from("<iq", obj.get_raw_data(), 16)

    original = {k: (o.type_id, o.get_raw_data()) for k, o in file.objects.items()}
    changed = set()
    for suffix in ("Popup", "GroupComponent", "Component"):
        pet_name, team_name = "PetTotalBuff" + suffix, "TeamLevelTotalBuff" + suffix
        pet_go, = [k for k, v in names.items() if v == pet_name]
        team_go, = [k for k, v in names.items() if v == team_name]
        source, = [o for o in components(pet_go)
                   if signature(o) == (global_id, script_ids[pet_name])]
        target, = [o for o in components(team_go) if o.type.name == "MonoBehaviour"
                   and signature(o) in ((0, 0), (global_id, script_ids[team_name]))]
        source_paths, target_paths = hierarchy(pet_go), hierarchy(team_go)
        if source_paths.keys() != target_paths.keys():
            raise ValueError("Native pet/team hierarchy differs")
        mapping = {source.path_id: target.path_id}
        for path, source_go in source_paths.items():
            target_go = target_paths[path]
            mapping[source_go] = target_go
            for component in components(source_go):
                if component.path_id == source.path_id:
                    continue
                matches = [o for o in components(target_go) if signature(o) == signature(component)]
                if len(matches) == 1:
                    mapping[component.path_id] = matches[0].path_id
        raw = bytearray(source.get_raw_data())
        sizes = {"Popup": 176, "GroupComponent": 104, "Component": 68}
        fields = {"Popup": [0, 84, 140, 152, 164],
                  "GroupComponent": [0, 32, 44, 56, 68, 80, 92],
                  "Component": [0, 32, 44, 56]}
        if len(raw) != sizes[suffix]:
            raise ValueError("Unknown native serialized buff layout")
        references = []
        for offset in fields[suffix]:
            fid, pid = struct.unpack_from("<iq", raw, offset)
            if fid == 0 and pid:
                if pid not in mapping:
                    raise ValueError(f"Unmapped native field reference {pid}")
                references.append((offset, mapping[pid]))
        for offset, pid in references:
            struct.pack_into("<iq", raw, offset, 0, pid)
        struct.pack_into("<iq", raw, 16, global_id, script_ids[team_name])
        if target.get_raw_data() == raw:
            continue
        if len(target.get_raw_data()) != 32 or signature(target) != (0, 0):
            raise ValueError("Unknown Team Level behaviour; refusing to overwrite")
        changed.add(target.path_id)
        # Each stripped type belongs to this one behaviour only.
        if sum(o.type_id == target.type_id for o in file.objects.values()) != 1:
            raise ValueError("Stripped serialized type is shared")
        native_type = copy.copy(source.serialized_type)
        native_type.script_type_index = len(file.script_types)
        native_type.script_id = hashlib.md5(team_name.encode()).digest()
        identifier = copy.copy(file.script_types[source.serialized_type.script_type_index])
        identifier.local_serialized_file_index = global_id
        identifier.local_identifier_in_file = script_ids[team_name]
        file.script_types.append(identifier)
        file.types[target.type_id] = native_type
        target.serialized_type = native_type
        target.set_raw_data(bytes(raw))

    if output is None:
        if changed:
            raise ValueError("Team Level buff prefab restoration is missing")
    else:
        Path(output).write_bytes(file.save() if changed else Path(asset).read_bytes())
        reread = next(iter(UnityPy.load(str(output)).files.values()))
        if reread.objects.keys() != original.keys():
            raise ValueError("Asset object IDs changed")
        for key, (type_id, data) in original.items():
            if key not in changed and (reread.objects[key].type_id != type_id
                                      or reread.objects[key].get_raw_data() != data):
                raise ValueError(f"Unrelated asset object changed: {key}")
        restore(output, scripts)
    print("Verified all three native Team Level buff prefab bindings; unrelated objects preserved.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("asset", type=Path)
    parser.add_argument("scripts", type=Path)
    parser.add_argument("output", type=Path, nargs="?")
    args = parser.parse_args()
    restore(args.asset, args.scripts, args.output)
