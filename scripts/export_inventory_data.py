"""Export inventory rules from decoded JSON and original JIT string pools (stdlib only)."""
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from export_tutorial_data import string_pool


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    parser.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1] / 'tables' / 'InventorySupport.json')
    args = parser.parse_args()
    def read(name):
        return json.loads((args.decoded / (name + '.json')).read_text(encoding='utf-8'))
    def pool(name):
        return string_pool(args.jit / (name + '.jit'))
    item_pool = pool('ItemTable')
    items = read('ItemTable')
    codes = {item_pool[r['Code']]: r['Index'] for r in items}
    booster_pool = pool('BoosterItemTable')
    booster_codes = {booster_pool[r['ItemCode']]: r['BoosterItemIndex'] for r in read('BoosterItemTable')}
    data = {'Items': {str(r['Index']): {k: r[k] for k in ['Type', 'SellGold', 'NotForSale', 'Breakable', 'ItemMaxCap', 'Grade']} for r in items}}
    for name, key in [('PotionItemTable', 'Potions'), ('PackageItemTable', 'Packages'), ('SelectItemTable', 'Selectors'), ('WeaponUniqueSelectItemTable', 'EquipmentSelectors'), ('HeroSelectItemTable', 'HeroSelectors'), ('BoosterItemTable', 'Boosters')]:
        strings = pool(name)
        mapped = {}
        for row in read(name):
            code = strings[row['ItemCode']]
            if name == 'BoosterItemTable':
                code = next((c for c, index in codes.items() if index == row['BoosterItemIndex']), code)
            if code not in codes:
                continue  # Unreleased definitions cannot be owned.
            if name == 'PotionItemTable':
                row['ActionSubValue'] = strings[row['ActionSubValue']]
                row['BoosterCodes'] = [booster_codes[strings[i]] for i in row['BoosterCodes'] or []]
            mapped[str(codes[code])] = row
        data[key] = mapped
    strings = pool('CraftItemTable')
    data['Crafts'] = {}
    for row in read('CraftItemTable'):
        row['ItemIndex'] = codes[strings[row['ItemCode']]]
        row['Materials'] = [{'ItemIndex': codes[strings[row[f'MaterialItemCode{i}']]], 'Count': row[f'MaterialItemCount{i}']} for i in range(1, 9) if row[f'MaterialItemCount{i}'] > 0]
        data['Crafts'][str(row['CraftIndex'])] = row
    data['BreakRewards'] = {str(r['ItemIndex']): r['RewardIndex'] for r in read('ItemBreakTable')}
    strings = pool('RuneItemBreakTable')
    data['RuneBreaks'] = {str(r['Grade']): [{'ItemIndex': codes[strings[r[f'ItemCode{i}']]], 'Min': r[f'MinItemCount{i}'], 'Max': r[f'MaxItemCount{i}']} for i in range(1, 5) if r[f'MaxItemCount{i}'] > 0] for r in read('RuneItemBreakTable')}
    strings = pool('EquipItemTable')
    details = {r['DetailIndex']: r for r in read('EquipItemDetailTable')}
    data['Equipment'] = {str(codes[strings[r['EquipCode'][0]]]): dict({'OptionCount': details[r['DetailIndex']]['OptionCount']}, **{k: r[k] for k in ['OptionIndex', 'OptionGroupIndex', 'EnableDuplicationOption', 'MinRuneCount', 'MaxRuneCount', 'UniqueOptionIndex', 'UniqueOptionGroupIndex']}) for r in read('EquipItemTable') if strings[r['EquipCode'][0]] in codes}
    data['OptionGroups'] = {str(r['Index']): r['OptionIndex'] for r in read('EquipOptionGroupTable')}
    data['Options'] = {str(r['Index']): {'Steps': r['OptionValueStep'], 'Ratio': r['Ratio'], 'Type': r['OptionType']} for r in read('EquipOptionTable')}
    data['Extensions'] = read('InventoryExtendTable')
    data['CraftInstantPrices'] = read('CraftInstantGemTable')
    data['TeamLevels'] = read('TeamLevelTable')
    data['CustomEquipment'] = {str(r['Index']): r for r in read('CustomEquipItemTable')}
    strings = pool('ConstantTable')
    wanted = ['CraftSlotCountMin', 'CraftSlotCountMax', 'EquipItemMaxCount', 'EquipCHESTMaxCount'] + [f'CraftSlotIndexOpen{i}' for i in range(1, 10)]
    data['Constants'] = {strings[r['Key']]: int(strings[r['Value']]) for r in read('ConstantTable') if strings[r['Key']] in wanted}
    args.output.write_text(json.dumps(data, separators=(',', ':')) + '\n', encoding='utf-8')
    print({k: len(v) for k, v in data.items()})


if __name__ == '__main__':
    main()
