"""Remove application telemetry code, SDK files, assets, and Unity service destinations.

Requires .NET 8 and UnityPy. Recovery copies and reports stay outside the client.
Run after any patch that restores the original client DLL or serialized assets.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import re
from pathlib import Path
import shutil
import struct
import subprocess

REPO = Path(__file__).resolve().parents[2]


def telemetry_assembly(name):
    return name.startswith(("Firebase.", "Unity.Services.")) or name in {
        "UnityEngine.UnityAnalyticsModule", "UnityEngine.UnityAnalyticsCommonModule",
        "UnityEngine.PerformanceReportingModule", "UnityEngine.CrashReportingModule",
        "UnityEngine.UnityConnectModule",
    }


def inside(root, relative):
    target = (root / relative).resolve()
    if not target.is_relative_to(root.resolve()):
        raise ValueError(f"Path leaves the expected directory: {relative}")
    return target


def load_asset(path):
    import UnityPy

    env = UnityPy.load(str(path))
    return next(iter(env.files.values()))


def stage_assets(data, stage, manifest):
    removed_types = set(manifest["removedTypes"])
    scripts = load_asset(data / "globalgamemanagers.assets")
    removed_ids = set()
    for obj in list(scripts.objects.values()):
        if obj.type.name != "MonoScript":
            continue
        tree = obj.read_typetree()
        assembly = tree["m_AssemblyName"].removesuffix(".dll")
        namespace = tree["m_Namespace"]
        name = (namespace + "." if namespace else "") + tree["m_ClassName"]
        if telemetry_assembly(assembly) or assembly + ":" + name in removed_types:
            removed_ids.add(obj.path_id)
            del scripts.objects[obj.path_id]
    (stage / "globalgamemanagers.assets").write_bytes(scripts.save())
    manifest["modifiedFiles"].append("globalgamemanagers.assets")

    resources = load_asset(data / "resources.assets")
    script_files = {i + 1 for i, ext in enumerate(resources.externals)
                    if str(ext.path).endswith("globalgamemanagers.assets")}
    removed_objects = set()
    removed_roots = set()
    for obj in resources.objects.values():
        if obj.type.name == "MonoBehaviour":
            raw = obj.get_raw_data()
            file_id, path_id = struct.unpack_from("<iq", raw, 16)
            if file_id in script_files and path_id in removed_ids:
                removed_objects.add(obj.path_id)
                go_file, go_id = struct.unpack_from("<iq", raw)
                if go_file != 0:
                    raise ValueError("Telemetry component belongs to an external GameObject")
                removed_roots.add(go_id)
    # The only components in the shipped client are dedicated Adjust/AdjustPurchase prefabs.
    # Refuse to remove a mixed-purpose object rather than stripping unrelated gameplay.
    for go_id in removed_roots:
        go = resources.objects[go_id].read_typetree()
        if go["m_Name"] not in {"Adjust", "AdjustPurchase", "GaHiddenWebView", "GaHiddenWebViewPrefab"}:
            raise ValueError(f"Unexpected telemetry GameObject: {go['m_Name']}")
        removed_objects.add(go_id)
        for component in go["m_Component"]:
            pointer = component["component"]
            if pointer["m_FileID"] != 0:
                raise ValueError("External telemetry prefab component")
            obj = resources.objects[pointer["m_PathID"]]
            if obj.type.name == "Transform":
                tree = obj.read_typetree()
                if tree["m_Children"]:
                    raise ValueError("Telemetry prefab has unexpected children")
                removed_objects.add(obj.path_id)
            elif obj.path_id not in removed_objects:
                raise ValueError("Telemetry prefab contains a non-telemetry component")
    for path_id in removed_objects:
        del resources.objects[path_id]
    if removed_objects:
        (stage / "resources.assets").write_bytes(resources.save())
        manifest["modifiedFiles"].append("resources.assets")

    settings = load_asset(data / "globalgamemanagers")
    external_scripts = {i + 1 for i, ext in enumerate(settings.externals)
                        if str(ext.path).endswith("globalgamemanagers.assets")}
    external_resources = {i + 1 for i, ext in enumerate(settings.externals)
                          if str(ext.path).endswith("resources.assets")}

    def deleted_pointer(pointer):
        return ((pointer["m_FileID"] in external_scripts and pointer["m_PathID"] in removed_ids) or
                (pointer["m_FileID"] in external_resources and pointer["m_PathID"] in removed_objects))

    for obj in settings.objects.values():
        if obj.type.name == "UnityConnectSettings":
            tree = obj.read_typetree()
            for key in ("m_EventOldUrl", "m_EventUrl", "m_ConfigUrl", "m_DashboardUrl"):
                tree[key] = ""
            tree["m_Enabled"] = False
            for key, value in tree.items():
                if isinstance(value, dict):
                    if "m_Enabled" in value:
                        value["m_Enabled"] = False
                    if "m_InitializeOnStartup" in value:
                        value["m_InitializeOnStartup"] = False
                    if "m_EventUrl" in value:
                        value["m_EventUrl"] = ""
            obj.save_typetree(tree)
        elif obj.type.name == "PlayerSettings":
            # Unity 6 has an extra opaque field absent from UnityPy's generated tree.
            # Edit only known string records in the raw object, preserving all other bytes.
            tree = obj.read_typetree(check_read=False)
            raw = obj.get_raw_data()
            for key in ("cloudProjectId", "organizationId"):
                value = tree.get(key, "").encode("utf-8")
                if not value:
                    continue
                record = struct.pack("<I", len(value)) + value + b"\0" * (-len(value) % 4)
                if raw.count(record) != 1:
                    raise ValueError(f"Cannot uniquely locate PlayerSettings.{key}")
                raw = raw.replace(record, b"\0" * 4)
            obj.set_raw_data(raw)
        elif obj.type.name == "MonoManager":
            tree = obj.read_typetree()
            tree["m_Scripts"] = [pointer for pointer in tree["m_Scripts"] if not deleted_pointer(pointer)]
            obj.save_typetree(tree)
        elif obj.type.name == "ResourceManager":
            tree = obj.read_typetree()
            tree["m_Container"] = [(name, pointer) for name, pointer in tree["m_Container"]
                                   if not deleted_pointer(pointer)]
            tree["m_DependentAssets"] = [entry for entry in tree["m_DependentAssets"]
                                         if not deleted_pointer(entry["m_Object"])]
            for entry in tree["m_DependentAssets"]:
                entry["m_Dependencies"] = [p for p in entry["m_Dependencies"] if not deleted_pointer(p)]
            obj.save_typetree(tree)
    (stage / "globalgamemanagers").write_bytes(settings.save())
    manifest["modifiedFiles"].append("globalgamemanagers")
    manifest["removedScriptCount"] = len(removed_ids)
    manifest["removedPrefabObjectCount"] = len(removed_objects)


def verify_assets(stage):
    asset = load_asset(stage / "globalgamemanagers")
    settings = next(o for o in asset.objects.values() if o.type.name == "UnityConnectSettings").read_typetree()
    assert not settings["m_Enabled"]
    assert all(not value for key, value in settings.items() if key.endswith("Url"))
    assert not settings["UnityAnalyticsSettings"]["m_InitializeOnStartup"]
    assert not settings["CrashReportingSettings"]["m_EventUrl"]
    scripts = load_asset(stage / "globalgamemanagers.assets")
    for obj in scripts.objects.values():
        if obj.type.name == "MonoScript":
            assert not telemetry_assembly(obj.read_typetree()["m_AssemblyName"].removesuffix(".dll"))
    if (stage / "resources.assets").exists():
        load_asset(stage / "resources.assets")  # Validate serialized directory after prefab removal.


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("client", type=Path)
    parser.add_argument("--stage-only", action="store_true")
    parser.add_argument("--patcher", type=Path, help="Use an already built DllPatcher.dll (CI)")
    args = parser.parse_args()
    client = args.client.resolve()
    data = client / "King's Raid_Data"
    stamp = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S-%f")
    audit = REPO / "target" / "telemetry-removal" / stamp
    stage = audit / "staged"
    patcher = (["dotnet", str(args.patcher.resolve(strict=True))] if args.patcher else
               ["dotnet", "run", "--project", str(REPO / "scripts/DllPatcher"), "--"])
    subprocess.run([*patcher, str(client), "--remove-telemetry", "--stage-only", "--output", str(stage)], check=True)
    manifest = json.loads((stage / "manifest.json").read_text())
    stage_assets(data, stage, manifest)
    verify_assets(stage)
    subprocess.run([*patcher, str(client), "--verify-no-telemetry", "--overlay", str(stage)], check=True)
    for name in ("UnityCrashHandler32.exe", "UnityCrashHandler64.exe"):
        if (client / name).exists():
            manifest["removedFiles"].append("../" + name)
    # Old patch snapshots are not part of the playable client and can reintroduce telemetry.
    for folder in (data, data / "Managed"):
        for path in folder.iterdir():
            if not path.is_file():
                continue
            if re.search(r"(?:\.assets|\.dll)\.(?:backup|before[-_]|.*staged|patched|bak)", path.name):
                manifest["removedFiles"].append(str(path.relative_to(data)))
    policy_path = data / "sprk-privacy.json"
    policy = json.loads(policy_path.read_text()) if policy_path.exists() else {}
    deleted = set(policy.get("deleted_files", []))
    deleted.update(inside(client, str(Path("King's Raid_Data") / path)).relative_to(client).as_posix()
                   for path in manifest["removedFiles"])
    required = ["King's Raid_Data/" + name for name in (
        "globalgamemanagers", "globalgamemanagers.assets", "resources.assets",
        "RuntimeInitializeOnLoads.json", "ScriptingAssemblies.json")]
    (stage / "sprk-privacy.json").write_text(json.dumps({
        "schema_version": 1, "required_files": required, "deleted_files": sorted(deleted)
    }, indent=2))
    manifest["modifiedFiles"].append("sprk-privacy.json")
    manifest["hashes"] = {path: hashlib.sha256(inside(stage, path).read_bytes()).hexdigest()
                          for path in manifest["modifiedFiles"]}
    (audit / "report.json").write_text(json.dumps(manifest, indent=2))
    if args.stage_only:
        print(f"Validated telemetry removal, staged at {stage}")
        return
    backup = audit / "backup"
    for relative in manifest["modifiedFiles"]:
        old = inside(data, relative)
        recovery = inside(backup / "King's Raid_Data", relative)
        recovery.parent.mkdir(parents=True, exist_ok=True)
        if old.exists():
            shutil.copy2(old, recovery)
    for relative in manifest["removedFiles"]:
        old = inside(client, str(Path("King's Raid_Data") / relative))
        recovery = inside(backup, old.relative_to(client))
        if old.exists():
            recovery.parent.mkdir(parents=True, exist_ok=True)
            shutil.move(str(old), recovery)
    try:
        for relative in manifest["modifiedFiles"]:
            shutil.copy2(inside(stage, relative), inside(data, relative))
    except Exception:
        for path in (backup / "King's Raid_Data").rglob("*"):
            if path.is_file():
                target = inside(client, path.relative_to(backup))
                target.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(path, target)
        for name in ("UnityCrashHandler32.exe", "UnityCrashHandler64.exe"):
            if (backup / name).exists():
                shutil.copy2(backup / name, client / name)
        raise
    # Remove obsolete symbol files and patch snapshots containing deleted telemetry code.
    legacy_stage = data / "telemetry-removal-staged"
    if legacy_stage.exists():
        shutil.move(str(inside(data, "telemetry-removal-staged")), backup / "legacy-telemetry-staging")
    print(f"Removed telemetry: {len(manifest['removedTypes'])} types, "
          f"{len(manifest['removedFiles'])} files, {manifest['removedScriptCount']} serialized scripts.")
    print(f"Report and recovery copies: {audit}")


if __name__ == "__main__":
    main()
