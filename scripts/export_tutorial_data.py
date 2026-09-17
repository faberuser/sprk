"""Export tutorial support data from a locally decoded client table directory.

Usage: python scripts/export_tutorial_data.py PATH_TO_TableJit PATH_TO_TutorialTable.jit
The exporter uses only the Python standard library; no client files are modified.
"""
import argparse
import json
import struct
from pathlib import Path


def string_pool(path):
    data = path.read_bytes()
    offset = 0
    for _ in range(2):
        size = struct.unpack_from('<I', data, offset)[0]
        offset += 4 + size
    count = struct.unpack_from('<I', data, offset)[0]
    offset += 4
    offsets = struct.unpack_from('<' + 'I' * count, data, offset)
    container = data[offset + 4 * count + offsets[-1]:]
    count = struct.unpack_from('<I', container)[0]
    result = []
    for index in range(count):
        pos = 4 + 4 * count + struct.unpack_from('<I', container, 4 + 4 * index)[0]
        tag = container[pos]
        pos += 1
        if tag == 192:
            result.append('')
            continue
        if 160 <= tag <= 191:
            size = tag - 160
        else:
            width = {217: 1, 218: 2, 219: 4}[tag]
            size = int.from_bytes(container[pos:pos + width], 'big')
            pos += width
        result.append(container[pos:pos + size].decode('utf-8'))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('tutorial_jit', type=Path)
    parser.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1] / 'tables')
    args = parser.parse_args()
    def read(name):
        return json.loads((args.decoded / (name + '.json')).read_text(encoding='utf-8'))
    pool = string_pool(args.tutorial_jit)
    tutorials = read('TutorialTable')
    reward_indices = {row['RewardIndex'] for row in tutorials if row['RewardIndex']}
    rewards = read('RewardTable')
    reward_pool = read('RewardTableStringPool')
    codes = {'HERO_KASEL'}
    custom_indices = set()
    for reward in rewards:
        if reward['Index'] in reward_indices:
            for drop in reward['field_15']:
                codes.add(reward_pool[drop[1][0]])
                if drop[8]:
                    custom_indices.add(drop[8])
    for row in tutorials:
        action = [pool[i].strip() for i in (row['RewardAction'] or [])]
        if action and action[0] == 'AddCustomEquipItem':
            custom_indices.add(int(action[1]))
    creatures = {row['Index']: row for row in read('CreatureTable')}
    item_pool = read('ItemTableStringPool')
    items = {}
    for row in read('ItemTable'):
        code = item_pool[row['Code']]
        if code not in codes:
            continue
        item = {'Kind': 'Equip' if row['Type'] in (1, 52) else 'Item'}
        if row['Type'] == 15:
            values = [int(item_pool[i]) for i in row['Value']]
            creature = creatures[values[0]]
            item = {'Kind': 'Hero', 'HeroIndex': values[0],
                    'Star': values[1] if len(values) > 1 else creature['StartHeroStar'],
                    'Level': values[2] if len(values) > 2 else max(1, creature['StartHeroLevel']),
                    'Transcend': values[3] if len(values) > 3 else 0}
        items[str(row['Index'])] = item
    tutorial_dungeons = {tuple(row['RewardDungeonIndex']) for row in tutorials if row['RewardDungeonIndex']}
    difficulties = {}
    for wave in read('CampaignWaveTable'):
        node = (wave['ChapterIndex'], wave['DungeonIndex'])
        if node in tutorial_dungeons and 'WaveIndex' in wave and not wave.get('Scenario', False):
            key = f'{node[0]}:{node[1]}'
            difficulties[key] = min(difficulties.get(key, wave['Difficulty']), wave['Difficulty'])
    support = {
        'DungeonDifficulties': difficulties,
        'Items': items,
        'HeroLevels': read('CreatureLevelTable'),
        'HeroStars': [{k: row[k] for k in ('Star', 'Transcended', 'GetHeroTeamExp', 'MaxHeroLevel')}
                      for row in read('CreatureStarTable')],
        'TeamLevels': [{k: row[k] for k in ('Level', 'LocalExp')} for row in read('TeamLevelTable')],
        'CustomEquipment': {str(row['Index']): row for row in read('CustomEquipItemTable')
                            if row['Index'] in custom_indices},
    }
    args.output.mkdir(parents=True, exist_ok=True)
    for name, data in [('TutorialStringPool', pool), ('TutorialSupport', support)]:
        (args.output / (name + '.json')).write_text(json.dumps(data, indent=2, ensure_ascii=False) + '\n', encoding='utf-8')


if __name__ == '__main__':
    main()
