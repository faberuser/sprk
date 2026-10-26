"""Restore Maria Cos21 body/face materials in a Windows bundle from the archive.

Output bundles belong in client-overrides/files for the normal release builder.
Only empty body/face material arrays and missing material/texture objects change.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path

import UnityPy
from PIL import Image


def restore(installed, archive, output, low=False):
    env = UnityPy.load(str(installed))
    original = next(o.assets_file for o in env.objects if o.type.name == "Material")
    source_env = UnityPy.load(str(archive))
    source = {o.path_id: o for o in source_env.objects}
    before = {k: o.get_raw_data() for k, o in original.objects.items()}
    templates = {t: next(o for o in original.objects.values() if o.type.name == t)
                 for t in ("Material", "Texture2D")}
    repaired = set()
    materials = set()
    renderers = [o for o in original.objects.values() if o.type.name == "SkinnedMeshRenderer"]
    for obj in renderers:
        data = obj.read()
        name = data.m_GameObject.read().m_Name.strip()
        if name not in ("Hero_Maria_Cos21Legend_Body", "Hero_Maria_Cos21Legend_Facial_A", "Hero_Maria_Cos21Legend_Facial_B"):
            continue
        expected = source[obj.path_id].read_typetree()["m_Materials"]
        tree = obj.read_typetree()
        if tree["m_Materials"] and tree["m_Materials"] != expected:
            raise ValueError("Unexpected nonempty native renderer materials")
        if tree["m_Materials"] != expected:
            tree["m_Materials"] = expected
            obj.save_typetree(tree)
            repaired.add(obj.path_id)
        materials.update(p["m_PathID"] for p in expected)

    def add(path_id, kind):
        obj = copy.copy(templates[kind])
        obj.path_id = path_id
        obj.set_raw_data(templates[kind].get_raw_data())
        original.objects[path_id] = obj
        return obj

    textures = set()
    for path_id in sorted(materials):
        if path_id in original.objects:
            continue
        obj = add(path_id, "Material")
        tree = templates["Material"].read_typetree()
        tree.update(source[path_id].read_typetree())
        # The Windows shader pack retains the standard hero shader. The removed
        # legend skin/fabric shader variant is replaced just as for Jane Cos21.
        tree["m_Shader"] = {"m_FileID": 1, "m_PathID": 6624712413963325431}
        obj.save_typetree(tree)
        for _, tex in tree["m_SavedProperties"]["m_TexEnvs"]:
            pointer = tex["m_Texture"]
            if pointer["m_FileID"] == 0 and pointer["m_PathID"]:
                textures.add(pointer["m_PathID"])
    expected_images = {}
    for path_id in sorted(textures):
        if path_id in original.objects:
            continue
        old = source[path_id].read()
        image = old.image.convert("RGBA")
        if low:
            image = image.resize((max(1, image.width // 2), max(1, image.height // 2)), Image.Resampling.LANCZOS)
        obj = add(path_id, "Texture2D")
        data = obj.read()
        data.m_Name = old.m_Name
        data.m_TextureSettings = copy.deepcopy(old.m_TextureSettings)
        data.m_ColorSpace = old.m_ColorSpace
        data.set_image(image, target_format=4, mipmap_count=1)
        data.save()
        expected_images[path_id] = image.tobytes()

    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_bytes(env.file.save(packer="lz4"))
    reread_env = UnityPy.load(str(output))
    reread = {o.path_id: o for o in reread_env.objects}
    for path_id, raw in before.items():
        if path_id not in repaired and reread[path_id].get_raw_data() != raw:
            raise ValueError(f"Unrelated object changed: {path_id}")
    for path_id, pixels in expected_images.items():
        if reread[path_id].read().image.convert("RGBA").tobytes() != pixels:
            raise ValueError("Recovered texture pixels differ")
    for obj in reread.values():
        if obj.type.name == "SkinnedMeshRenderer":
            data = obj.read()
            if len(data.m_Materials) != len(data.m_Mesh.read().m_SubMeshes):
                raise ValueError("Renderer still has missing material slots")
            for material in data.m_Materials:
                mat = material.read()
                for _, tex in mat.m_SavedProperties.m_TexEnvs:
                    if tex.m_Texture.m_PathID and tex.m_Texture.m_FileID == 0:
                        texture = tex.m_Texture.read()
                        if texture.image.width != texture.m_Width:
                            raise ValueError("Invalid texture dependency")
    return {"input_sha256": hashlib.sha256(installed.read_bytes()).hexdigest(),
            "archive_sha256": hashlib.sha256(archive.read_bytes()).hexdigest(),
            "output_sha256": hashlib.sha256(output.read_bytes()).hexdigest(),
            "restored_renderers": len(repaired), "restored_textures": len(expected_images),
            "preserved_objects": len(before) - len(repaired)}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("installed", type=Path)
    parser.add_argument("archive", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--low", action="store_true")
    args = parser.parse_args()
    print(json.dumps(restore(args.installed, args.archive, args.output, args.low), indent=2))
