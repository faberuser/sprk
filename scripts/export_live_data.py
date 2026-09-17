"""Export non-cash shop, event, summon, and pet rules from the supplied client."""
import argparse
import json
import re
from pathlib import Path
from export_battle_data import keyed_schema
from export_tutorial_data import string_pool

TABLES = '''NewEquipGacha EquipGachaClient GachaSelectItem GachaSelectShop
GachaShopProbability StepUpGachaReward EventCraft EventRoulette EventStep EventForge
PetEgg PetIncubator PetIncubatorSlot PetAwaken PetTierInteractive PetUpgradeTier
PetAdventure PetAdventureReward PetBonus PetOption NewPayShopProduct PayShopDiscount
PayShopDiscountGroup CurrencyType PurchaseDungeon'''.split()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    parser.add_argument('client', type=Path)
    args = parser.parse_args()
    data = {}
    for name in TABLES + ['Creature']:
        rows = json.loads((args.decoded / (name+'Table.json')).read_text(encoding='utf-8'))
        schema = keyed_schema(args.client, name+'Data')
        pool = string_pool(args.jit / (name+'Table.jit'))
        types = {key: typ for key, typ in schema.values()}
        out = []
        for raw in rows:
            row = {schema.get(i, (f'field_{i}', ''))[0]: v for i, v in enumerate(raw)} if isinstance(raw, list) else dict(raw)
            for key in list(row):
                if key.startswith('field_') and key[6:].isdigit() and int(key[6:]) in schema:
                    row[schema[int(key[6:])][0]] = row.pop(key)
            if name == 'Creature' and not 7100000 <= row['Index'] < 7500000:
                continue
            for key, value in list(row.items()):
                if types.get(key) == 'string' and isinstance(value, int):
                    row[key] = pool[value]
                elif types.get(key) == 'string[]' and isinstance(value, list):
                    row[key] = [pool[v] for v in value]
            out.append(row)
        data['Pet' if name == 'Creature' else name] = out
    shared = args.client / 'NShared'
    enums = {}
    for p in shared.glob('*.cs'):
        text = p.read_text(encoding='utf-8-sig')
        if not re.search(r'public enum '+re.escape(p.stem)+r'\b', text):
            continue
        number = 0
        values = {}
        for line in text.splitlines():
            member = re.match(r'^\s*(\w+)(?:\s*=\s*(-?\d+))?,?\s*$', line)
            if member:
                number = int(member[2]) if member[2] else number
                values[member[1]] = number
                number += 1
        enums[p.stem] = values
    contracts = {}
    for p in shared.glob('*/Request.cs'):
        text = p.read_text(encoding='utf-8-sig')
        route = re.search(r'"((?:shop|equip_gacha|event|event_step|event_equip|pet)/\w+|item/(?:event_craft_item|reward_event_roulette))"', text)
        if not route:
            continue
        fields = lambda t: {key: typ for typ, key in re.findall(r'public ([\w.<>\[\], ?]+) (\w+)\s*\{\s*get', t)}
        contracts[route[1]] = {
            'Request': fields(text),
            'Response': fields(p.with_name('Response.cs').read_text(encoding='utf-8-sig')),
            'Results': re.findall(r'^\s*(\w+)(?:\s*=\s*\d+)?,?\s*$', p.with_name('ResultType.cs').read_text(encoding='utf-8-sig'), re.M),
        }
    keep = {typ for c in contracts.values() for typ in c['Request'].values()}
    keep.update(['CurrencyType', 'ShopCostType', 'PetMiscType', 'PetStatusType', 'CreatureGrade', 'EventStepType', 'GachaType', 'GachaSubType'])
    enums = {k: v for k, v in enums.items() if k in keep}
    target = Path(__file__).resolve().parents[1] / 'tables/LiveSupport.json'
    target.write_text(json.dumps({'Tables': data, 'Contracts': contracts, 'Enums': enums}, separators=(',', ':'))+'\n', encoding='utf-8')
    print(f'Exported {len(data)} table groups, {len(data["Pet"])} pets, {len(contracts)} contracts')


if __name__ == '__main__':
    main()
