"""Repair missing Black Field Raid projectile materials using its retained trail material.
Requires UnityPy. Keeps an original backup; changes only the known null slots.
The replacement is an approximation, not a recovered original DarkAura material.
"""
import argparse
from pathlib import Path
import UnityPy

NAME = "Effect_Skill_Monster_Guardian_RottenBoulderGolem_Attack1_Projectile.unity3d"
AURA = "Effect_Monster_Guardian_RoettenBoulderGolem_Attack1_Projectile_DarkAura"
TRAIL = "Effect_Monster_Guardian_RoettenBoulderGolem_Attack1_Projectile_Trail"

def repair(path):
    env = UnityPy.load(path.read_bytes())
    names = {o.path_id: o.read_typetree()["m_Name"] for o in env.objects if o.type.name == "GameObject"}
    materials = {o.read_typetree()["m_Name"]: o.path_id for o in env.objects if o.type.name == "Material"}
    replacement = materials[TRAIL]
    changed = 0
    for obj in env.objects:
        if obj.type.name != "ParticleSystemRenderer": continue
        data = obj.read_typetree()
        if names.get(data["m_GameObject"]["m_PathID"]) != AURA: continue
        slots = data["m_Materials"]
        if not slots or any(slot["m_PathID"] != 0 for slot in slots): continue
        data["m_Materials"] = [{"m_FileID": 0, "m_PathID": replacement} for _ in slots]
        obj.save_typetree(data)
        changed += 1
    if changed:
        backup = path.with_suffix(path.suffix + ".before-field-material")
        if not backup.exists(): backup.write_bytes(path.read_bytes())
        temp = path.with_suffix(path.suffix + ".repaired")
        temp.write_bytes(env.file.save())
        checked = UnityPy.load(temp.read_bytes())
        for obj in checked.objects:
            if obj.type.name != "ParticleSystemRenderer": continue
            data = obj.read_typetree()
            if names.get(data["m_GameObject"]["m_PathID"]) == AURA:
                assert all(v["m_FileID"] == 0 and v["m_PathID"] == replacement for v in data["m_Materials"])
        temp.replace(path)
    print(f"{path.parent.name}: {changed} renderer repaired")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("client", type=Path)
    args = parser.parse_args()
    root = args.client / "King's Raid_Data/Documents/Patch/StandaloneWindows/Assetbundle"
    for quality in ("OriginalSpec", "MidSpec", "LowSpec"):
        repair(root / quality / NAME)
