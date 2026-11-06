"""Move only Lavril S3's three effect positions to its native projectile centre.

Splice BJS value records so all unrelated bytes, node links and string-pool
indices remain unchanged. No combat tables or skill behaviour are rewritten.
"""
import argparse
import copy
import io
from pathlib import Path
import struct


class Bjs:
    def __init__(self, data):
        self.data = data
        self.stream = io.BytesIO(data)
        self.spans = {}
        assert self.integer() == 20160919, 'Unsupported BJS version'
        self.pool = [self.read(self.integer()).decode('utf-16le') for _ in range(self.integer())]
        self.value = self.parse(())
        assert self.stream.tell() == len(data), 'Trailing BJS data'

    def read(self, count):
        value = self.stream.read(count)
        assert len(value) == count, 'Truncated BJS'
        return value

    def integer(self):
        return struct.unpack('<i', self.read(4))[0]

    def string(self):
        index = self.integer()
        assert 0 <= index <= len(self.pool)
        return self.pool[index - 1] if index else ''

    def parse(self, path):
        start = self.stream.tell()
        tag = self.read(1)[0]
        if tag == 0:
            value = None
        elif tag == 10:
            value = {}
            for _ in range(self.integer()):
                key = self.string()
                assert key not in value
                value[key] = self.parse(path + (key,))
        elif tag == 20:
            value = [self.parse(path + (i,)) for i in range(self.integer())]
        elif tag == 30:
            value = self.string()
        elif tag == 40:
            value = bool(self.read(1)[0])
        else:
            fmt, count = {50: ('<d', 8), 60: ('<q', 8), 61: ('<i', 4), 62: ('<h', 2)}[tag]
            value = struct.unpack(fmt, self.read(count))[0]
        self.spans[path] = (start, self.stream.tell())
        return value


def patch(data):
    original = Bjs(data)
    matches = [(i, t) for i, t in enumerate(original.value['TreeArray'])
               if t['Body']['Code'] == 'BeginOperation_5430_3']
    assert len(matches) == 1, 'Expected one S3 projectile-creation effect tree'
    tree_index, tree = matches[0]
    nodes = tree['Body']['NodeArray']
    by_id = {n['NodeId']: (i, n['Contents']) for i, n in enumerate(nodes)}
    assert by_id[100][1]['SkillIndex'] == 5430 and by_id[100][1]['OperationNo'] == 3
    template_index, template = by_id[129]
    assert template['Code'] == 'Skill.Hero.Lavril.Skill1.Casting'
    prefix = ('TreeArray', tree_index, 'Body', 'NodeArray')
    source_path = prefix + (template_index, 'Contents', 'TransformModifier', 'Position')
    position = template['TransformModifier']['Position']
    assert position['ValuePath'] == 'Vector3/Calculate/Add'
    assert position['Body'][0]['Body'][0]['Body']['ArgumentContainer'][0]['Body']['Name'] == 'Myself'
    assert position['Body'][1]['Body'][0]['Body'][0]['Body']['Name'] == 'Myself'
    distance_path = source_path + ('Body', 1, 'Body', 1, 'Body', 'Derived')
    old_distance = position['Body'][1]['Body'][1]['Body']['Derived']
    assert old_distance in (5.0, 6.0), 'Unexpected field offset'
    start, end = original.spans[source_path]
    a, z = original.spans[distance_path]
    assert data[a] == 50 and z - a == 9
    replacement = data[start:a] + bytes([50]) + struct.pack('<d', 6.0) + data[z:end]
    expected = copy.deepcopy(original.value)
    expected_position = copy.deepcopy(position)
    expected_position['Body'][1]['Body'][1]['Body']['Derived'] = 6.0
    edits = []
    for node_id, code in [(128, 'Skill.Hero.Lavril.Skill3.Loop'),
                          (129, 'Skill.Hero.Lavril.Skill1.Casting'),
                          (136, 'Skill.Hero.LavrilCos20GloryLeague.Skill3.Loop')]:
        index, contents = by_id[node_id]
        assert contents['type'] == 'Effect/CreateAssetEffect' and contents['Code'] == code
        old = contents['TransformModifier']['Position']
        if node_id != 129:
            myself = {'ValuePath': 'Uid/Variable', 'Body': {'Name': 'Myself', 'ArgumentContainer': None}}
            # Build1558 (restored node tree): caster origin + facing * 5m.
            build1558 = {'ValuePath': 'Vector3/Calculate/Add', 'Body': [
                {'ValuePath': 'Vector3/Operation/GetPosition', 'Body': [myself]},
                {'ValuePath': 'Vector3/Calculate/Multiply', 'Body': [
                    {'ValuePath': 'Vector3/Operation/GetDirection', 'Body': [myself]},
                    {'ValuePath': 'Float/Constant', 'Body': {'Derived': 5.0, 'ArgumentContainer': None}}]}]}
            assert old == expected_position or old == build1558 or old == {
                'ValuePath': 'Vector3/Operation/GetPosition',
                'Body': [{'ValuePath': 'Uid/Variable', 'Body': {'Name': 'Target', 'ArgumentContainer': None}}]}
        path = prefix + (index, 'Contents', 'TransformModifier', 'Position')
        edits.append((*original.spans[path], replacement))
        expected['TreeArray'][tree_index]['Body']['NodeArray'][index]['Contents']['TransformModifier']['Position'] = copy.deepcopy(expected_position)
    result = data
    for a, z, value in sorted(edits, reverse=True):
        result = result[:a] + value + result[z:]
    decoded = Bjs(result)
    assert decoded.value == expected, 'Unrelated node-tree data changed'
    assert decoded.pool == original.pool, 'String pool changed'
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('source', type=Path)
    parser.add_argument('--output', type=Path)
    args = parser.parse_args()
    data = args.source.read_bytes()
    result = patch(data)
    assert patch(result) == result, 'Patch is not idempotent'
    if args.output:
        args.output.write_bytes(result)
    else:
        assert data == result, 'Lavril circle is not repaired'
    print('PASS: Lavril S3 normal/costume circles and impact use caster + facing * 6m; complete BJS readback, only three visual positions changed; idempotence.')


if __name__ == '__main__':
    main()
