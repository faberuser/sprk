"""Export equipment/hero extension rules using each table's own string pool."""
import argparse
import json
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from export_tutorial_data import string_pool

TABLES = '''AwakenStone EquipAwakenPoint EquipAwakenPrice EquipAwakenRatio EquipItemDetail
EquipOptionChange EquipOptionUpgrade EquipEnchantOptionItem EquipEnchantableCondition
EquipUpgradeExp EquipUpgradePrice EquipUpgradeTier EquipItemBreak ArtifactRestore
RuneItem SoulWeaponUpgrade SoulWeaponUpgradePrice SoulWeaponEther SoulWeaponEtherRate
SoulWeaponReinforce SoulWeaponOption SoulWeaponAbilityTicket SoulWeaponAbilityReward
SoulStoneRestore SoulBreak ValanceCraft ValanceAwaken ValanceIdentified ValanceEnchant
ValanceEnchantOptionCount ClassBuff TeamLevelBuffOption TeamLevelBonusBuffOption
NPCFriendlyPoint NPCFriendlyPointItem FlaskItem ShardFlaskItem PunishmentRune
PunishmentRuneItemCount PunishmentRuneItemBreak PunishmentRuneOption
EquipTransitionTicket CreatureTranscendOption Costume HairCostume WeaponCostume
AccessoryCostume AccessorySelectItem PetSelectItem RecipeItem StorageSlotExtend'''.split()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    args = parser.parse_args()
    def read(name):
        return json.loads((args.decoded / (name + 'Table.json')).read_text(encoding='utf-8'))
    def pool(name):
        return string_pool(args.jit / (name + 'Table.jit'))
    strings = pool('Item')
    codes = {strings[r['Code']]: r['Index'] for r in read('Item')}
    data = {}
    for name in TABLES + ['EquipItem']:
        if not (args.decoded / (name + 'Table.json')).exists():
            print('Missing:', name)
            continue
        strings = pool(name)
        rows = []
        for source in read(name):
            r = {k: v for k, v in source.items() if not k.startswith('field_')}
            if name == 'ClassBuff':
                r.update(CreatureType=source['field_0'], ClassBuffIndex=source['field_1'], SkillIndex=source['field_2'])
            if name == 'SoulBreak':
                r['Rewards'] = [{'ItemIndex': codes.get(strings[v[0]], 0), 'Min': v[1], 'Max': v[2]} for v in source['field_3']]
            if name == 'RuneItem':
                r['Tag'] = [strings[i] for i in source.get('Tag') or []]
            if name == 'ValanceEnchant':
                parts = 'None Weapon Armor Accessory SecondGear Artifact Orb Treasure'.split()
                subtypes = 'SpecialWeapon Spear Sword Dagger Bow Cannon Staff HeavyArmor MediumArmor LightArmor Ring Earring Necklace Bracelet SpecialTreasure SpecialTreasure_1 SpecialTreasure_2 SpecialTreasure_3 SpecialTreasure_4 Treasure None'.split()
                r['PartType'] = parts.index(strings[source['PartType']])
                r['SubType'] = subtypes.index(strings[source['SubType']])
            for key, value in list(r.items()):
                if 'Code' in key and isinstance(value, int):
                    r[key] = strings[value]
                    if 'ItemCode' in key:
                        r[key.replace('ItemCode', 'ItemIndex')] = codes.get(r[key], 0)
                elif 'Code' in key and isinstance(value, list):
                    r[key] = [strings[i] for i in value]
            if name == 'EquipItem':
                r['ItemIndex'] = codes.get(r['EquipCode'][0], 0)
            for key in ['ArtifactItemGroup', 'SoulStoneItemGroup']:
                if key in r: r[key] = strings[r[key]]
            rows.append(r)
        data[name] = rows
    target = Path(__file__).resolve().parents[1] / 'tables' / 'ExtensionSupport.json'
    target.write_text(json.dumps(data, separators=(',', ':')) + '\n', encoding='utf-8')
    print({k: len(v) for k, v in data.items()})


if __name__ == '__main__':
    main()
