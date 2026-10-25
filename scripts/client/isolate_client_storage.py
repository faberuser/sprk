"""Change only native PlayerSettings company bytes; retain opaque Unity 6 fields."""
import argparse
from pathlib import Path
import struct
import UnityPy


def isolated_payload(original):
    if len(original) != 808 or original[36:52] != struct.pack("<I",10)+b"masangsoft\0\0":
        raise ValueError("Unsupported native PlayerSettings fingerprint")
    return original[:36]+struct.pack("<I",4)+b"sprk"+original[52:]


def transform(asset, output, fingerprint):
    original=Path(fingerprint).read_bytes(); isolated=isolated_payload(original)
    env=UnityPy.load(str(asset)); file=next(iter(env.files.values()))
    objects={key:(obj.type_id,obj.get_raw_data()) for key,obj in file.objects.items()}
    settings=[obj for obj in file.objects.values() if obj.type.name=="PlayerSettings"]
    if len(settings)!=1:raise ValueError("Expected one PlayerSettings object")
    obj=settings[0]; raw=obj.get_raw_data(); desired=None
    for pattern,flag in [(original,648),(isolated,640)]:
        for enabled in (0,1):
            candidate=bytearray(pattern);candidate[flag]=enabled
            if raw==candidate:
                desired=bytearray(isolated);desired[640]=enabled
    if desired is None:raise ValueError("Unknown native PlayerSettings; refusing schema-based rewrite")
    if raw==desired:
        Path(output).write_bytes(Path(asset).read_bytes())
    else:
        obj.set_raw_data(bytes(desired));Path(output).write_bytes(file.save())
    reread=next(iter(UnityPy.load(str(output)).files.values()))
    if set(reread.objects)!=set(objects):raise ValueError("Object IDs changed")
    for key,(type_id,data) in objects.items():
        expected=bytes(desired) if key==obj.path_id else data
        other=reread.objects[key]
        if other.type_id!=type_id or other.get_raw_data()!=expected:raise ValueError("Unrelated object bytes changed")
    print("Verified company sprk, unchanged product, frame timing, other PlayerSettings fields and all unrelated asset objects.")


if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("asset",type=Path);p.add_argument("output",type=Path);p.add_argument("fingerprint",type=Path)
    a=p.parse_args();transform(a.asset,a.output,a.fingerprint)
