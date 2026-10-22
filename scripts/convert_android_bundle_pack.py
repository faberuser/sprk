"""Convert the archived indexed BJS pack to the APK's native JsonBin v2 format.

Uses the local Windows client's native serializer, without launching the game or
changing its files. The complete value tree is decoded again and compared.
"""

import argparse
import hashlib
import io
import json
import math
from pathlib import Path
import struct
import subprocess

ROOT = Path(__file__).resolve().parent.parent
SOURCE_HASH = "fc5310c4f426d32fe9f574cbc2bf9d26f0c1b302fdb00a58b0833ce99fca7c15"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def bjs(data):
    stream = io.BytesIO(data)

    def read(count):
        result = stream.read(count)
        if len(result) != count:
            raise ValueError("Truncated BJS record")
        return result

    def integer():
        return struct.unpack("<i", read(4))[0]

    if integer() != 20160919:
        raise ValueError("Unexpected BJS version")
    pool = [read(integer()).decode("utf-16le") for _ in range(integer())]

    def string():
        index = integer()
        if not 0 <= index <= len(pool):
            raise ValueError("Invalid BJS string index")
        return pool[index - 1] if index else ""

    def value():
        tag = read(1)[0]
        if tag == 0:
            return None
        if tag == 10:
            result = {}
            for _ in range(integer()):
                key = string()
                if key in result:
                    raise ValueError("Duplicate BJS field")
                result[key] = value()
            return result
        if tag == 20:
            return [value() for _ in range(integer())]
        if tag == 30:
            return string()
        if tag == 40:
            return bool(read(1)[0])
        formats = {50: ("<d", 8), 60: ("<q", 8), 61: ("<i", 4), 62: ("<h", 2)}
        if tag in formats:
            fmt, length = formats[tag]
            return struct.unpack(fmt, read(length))[0]
        raise ValueError(f"Unknown BJS tag: {tag}")

    result = value()
    if stream.tell() != len(data):
        raise ValueError("Trailing BJS data")
    return result


def compare(original, decoded, path="root"):
    count = 1
    if isinstance(original, dict):
        if not isinstance(decoded, dict) or original.keys() != decoded.keys():
            raise ValueError(f"Roundtrip fields differ: {path}")
        for key in original:
            count += compare(original[key], decoded[key], path + "/" + key)
    elif isinstance(original, list):
        if not isinstance(decoded, list) or len(original) != len(decoded):
            raise ValueError(f"Roundtrip array differs: {path}")
        for index, (left, right) in enumerate(zip(original, decoded)):
            count += compare(left, right, path + "/" + str(index))
    elif isinstance(original, float):
        # The game's native packed format stores float32 values.
        if not math.isclose(original, decoded, rel_tol=2e-6, abs_tol=1e-6):
            raise ValueError(f"Roundtrip float differs: {path}")
    elif original != decoded:
        raise ValueError(f"Roundtrip value differs: {path}")
    return count


def convert(source, managed, output):
    data = source.read_bytes()
    if sha(data) != SOURCE_HASH:
        raise ValueError("Expected untouched archived build1558 Android BundlePack.bin")
    output.mkdir(parents=True, exist_ok=False)
    header_size, record_count = struct.unpack_from("<ii", data)
    stream = io.BytesIO(data)
    stream.seek(8)
    records = {}
    for _ in range(record_count):
        length, = struct.unpack("<i", stream.read(4))
        key = stream.read(length).decode("utf-16le")
        offset, = struct.unpack("<i", stream.read(4))
        if key in records or not header_size <= offset < len(data):
            raise ValueError("Invalid indexed pack record")
        records[key] = offset
    if stream.tell() != header_size:
        raise ValueError("Invalid indexed pack header")
    offsets = sorted(set(records.values()) | {header_size, len(data)})
    ends = dict(zip(offsets, offsets[1:]))

    def record(offset):
        return bjs(data[offset:ends[offset]])

    index = record(header_size)
    root = {"BundleFileInfo": index["BundleFileInfo"]}
    for category in ["Creature", "Effect", "BattleFieldMap2", "WorldMap", "Ngui", "NguiAtlas"]:
        root[category] = {key: record(records[key]) for key in index[category]}
    for category in ["CameraData_Json", "SoundTable_Json", "BgmTable_Json"]:
        root[category] = record(records[category])
    root["NguiAsset"] = {}
    duplicate_sprites = 0
    for key, atlas in root["NguiAtlas"].items():
        for sprite in atlas["SpriteList"]:
            path = atlas["Prefix"] + sprite
            link = {"BundlePath": key, "Name": sprite}
            if path in root["NguiAsset"] and root["NguiAsset"][path] != link:
                # Archive atlases contain shared sprite aliases. Preserve pack
                # declaration order, with the later declaration taking priority.
                duplicate_sprites += 1
            root["NguiAsset"][path] = link
    tree = output / "BundlePack.json"
    tree.write_text(json.dumps(root, ensure_ascii=False, separators=(",", ":"), allow_nan=False), encoding="utf-8")
    packed = output / "BundlePack.binv2"
    project = ROOT / "scripts/android/pack-converter/PackConverter.csproj"
    subprocess.run(["dotnet", "run", "--project", str(project),
                    "--artifacts-path", str(output / "dotnet"), "--",
                    str(managed), str(tree), str(packed)], check=True)
    decoded = json.loads(Path(str(packed) + ".roundtrip.json").read_text(encoding="utf-8"))
    values = compare(root, decoded)
    report = {
        "source_sha256": sha(data), "indexed_records": record_count,
        "native_serializer_assembly_sha256": sha((managed / "Assembly-CSharp.dll").read_bytes()),
        "output_sha256": sha(packed.read_bytes()), "output_bytes": packed.stat().st_size,
        "sprite_links": len(root["NguiAsset"]), "roundtrip_values_verified": values,
        "duplicate_sprite_aliases_last_declaration_wins": duplicate_sprites,
        "float_relative_tolerance": 2e-6, "float_absolute_tolerance": 1e-6,
        "localization_changed": False,
    }
    (output / "conversion-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--source", type=Path, default=ROOT.parent / "Pre-Doomsday/com.vespainteractive.KingsRaid/files/Documents/Patch/Android/Assetbundle/BundlePack.bin")
    parser.add_argument("--managed", type=Path, default=ROOT.parent / "sprk-client/King's Raid_Data/Managed")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    convert(args.source.resolve(strict=True), args.managed.resolve(strict=True), args.output.resolve())
