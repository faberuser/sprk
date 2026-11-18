"""Serialized-file writer that keeps Unity's object data order.

UnityPy's SerializedFile.save() lays object data out in path-ID order with 8-byte
padding. Unity 6 rejects such files inside asset bundles ("not compatible with this
newer version of the Unity runtime") even when every byte of metadata and object
data is identical. This writer reproduces Unity's layout instead: objects keep their
original relative data order (new objects are appended), each starts on a 16-byte
boundary, and the object table stays sorted by path ID.
"""
from UnityPy.streams import EndianBinaryWriter

# id() of ObjectReaders created by the repacker (ObjectReader has __slots__).
NEW_OBJECTS: set[int] = set()


def prune_types(sf):
    """Drop type entries no object uses (e.g. after removing the D3D shaders).

    Unity rejects a bundle whose type table lists classes without objects
    ("not compatible with this newer version of the Unity runtime").
    """
    used = sorted({o.type_id for o in sf.objects.values()})
    if len(used) == len(sf.types):
        return
    remap = {old: new for new, old in enumerate(used)}
    sf.types = [sf.types[i] for i in used]
    for o in sf.objects.values():
        o.type_id = remap[o.type_id]
        o.serialized_type = sf.types[o.type_id]


def save_serialized(sf) -> bytes:
    prune_types(sf)
    header = sf.header
    assert header.version >= 22, "only the Unity 2020+ (v22) header is supported"
    meta = EndianBinaryWriter(endian=header.endian)
    data = EndianBinaryWriter(endian=header.endian)

    meta.write_string_to_null(sf.unity_version)
    meta.write_int(sf._m_target_platform)
    meta.write_boolean(sf._enable_type_tree)
    meta.write_int(len(sf.types))
    for typ in sf.types:
        typ.write(sf, meta, False)

    # Data: original order first (by original offset), then objects added later.
    def data_key(o):
        return (1, o.path_id) if id(o) in NEW_OBJECTS else (0, o.byte_start)

    placed = {}
    for o in sorted(sf.objects.values(), key=data_key):
        if o.data is not None:
            blob = o.data
        else:
            o.reset()
            blob = o.reader.read(o.byte_size)
        data.align_stream(16)
        placed[o.path_id] = (data.Position, len(blob))
        data.write(blob)

    meta.write_int(len(sf.objects))
    for o in sorted(sf.objects.values(), key=lambda x: x.path_id):
        offset, size = placed[o.path_id]
        meta.align_stream(4)
        meta.write_long(o.path_id)
        meta.write_long(offset)
        meta.write_u_int(size)
        meta.write_int(o.type_id)

    meta.write_int(len(sf.script_types))
    for st in sf.script_types:
        st.write(header, meta)
    meta.write_int(len(sf.externals))
    for e in sf.externals:
        e.write(header, meta)
    meta.write_int(len(sf.ref_types or []))
    for rt in sf.ref_types or []:
        rt.write(sf, meta, True)
    meta.write_string_to_null(sf.userInformation or "")

    header_size = 48
    metadata_size = meta.Length
    data_offset = header_size + metadata_size
    data_offset += (16 - data_offset % 16) % 16
    file_size = data_offset + data.Length

    out = EndianBinaryWriter()  # header fields are big-endian
    out.write_u_int(0)
    out.write_u_int(0)
    out.write_u_int(header.version)
    out.write_u_int(0)
    out.write_boolean(header.endian == ">")
    out.write_bytes(header.reserved)
    out.write_u_int(metadata_size)
    out.write_long(file_size)
    out.write_long(data_offset)
    out.write_long(sf.unknown)
    out.write_bytes(meta.bytes)
    out.align_stream(16)
    out.write_bytes(data.bytes)
    return out.bytes


class SavedNode:
    """Bundle node wrapper so BundleFile.save() embeds our bytes unchanged."""

    def __init__(self, sf, flags: int = 4):
        self.sf = sf
        self.flags = flags

    def save(self) -> bytes:
        return save_serialized(self.sf)
