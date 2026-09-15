"""Export mail reward item types from decoded client TableJit JSON (stdlib only)."""
import argparse
import json
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1] / 'tables' / 'ItemRewardMetadata.json')
    args = parser.parse_args()
    def read(name):
        return json.loads((args.decoded / (name + '.json')).read_text(encoding='utf-8'))
    pool = read('ItemTableStringPool')
    creatures = {row['Index']: row for row in read('CreatureTable')}
    stars = {(r['Star'], r['Transcended']): r['AllOwnedRewardIndex'] for r in read('CreatureStarTable')}
    items = {}
    for row in read('ItemTable'):
        item = {'Kind': 'Equip' if row['Type'] == 1 else 'Item'}
        if row['Type'] == 15:
            values = [int(pool[i]) for i in row['Value']]
            creature = creatures[values[0]]
            item = {'Kind': 'Hero', 'HeroIndex': values[0],
                    'Star': values[1] if len(values) > 1 else creature['StartHeroStar'],
                    'Level': values[2] if len(values) > 2 else max(1, creature['StartHeroLevel']),
                    'Transcend': values[3] if len(values) > 3 else 0}
            item['DuplicateRewardIndex'] = stars[(item['Star'], item['Transcend'])]
        items[str(row['Index'])] = item
    args.output.write_text(json.dumps(items, separators=(',', ':')) + '\n', encoding='utf-8')
    print(f'Exported {len(items)} item definitions to {args.output}')


if __name__ == '__main__':
    main()
