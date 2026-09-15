"""Export hero progression and shop rules, resolving each JIT's own string pool."""
import argparse
import json
import re
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from export_tutorial_data import string_pool


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    parser.add_argument('source', type=Path, help='Assembly-CSharp directory containing NShared')
    parser.add_argument('--output', type=Path, default=Path(__file__).resolve().parents[1] / 'tables/HeroShopSupport.json')
    args = parser.parse_args()
    def read(name):
        return json.loads((args.decoded / (name + '.json')).read_text(encoding='utf-8'))
    def pool(name):
        return string_pool(args.jit / (name + '.jit'))
    strings = pool('ItemTable')
    items = read('ItemTable')
    codes = {strings[r['Code']]: r['Index'] for r in items}
    data = {'Items': {str(r['Index']): dict({k:v for k,v in r.items() if k in ('Index','Type','NotForSale') or k.startswith('Buy')}, Code=strings[r['Code']]) for r in items}}
    data['Heroes'] = {str(r['Index']): r for r in read('CreatureTable') if r['PlayableCharacter'] and (1 <= r['Index'] <= 102 or r['Index'] == 111)}
    # Match the existing private-server visibility patch, without opening scenario heroes.
    for r in data['Heroes'].values():
        r['Buyable'] = r['OpenType'] == 3 or r['OpenType'] == 0
    costume_strings = pool('CostumeTable')
    data['Costumes'] = {str(r['CostumeIndex']): r for r in read('CostumeTable')}
    for r in data['Costumes'].values():
        r['Buyable'] = not r['IsDefault'] and r['CostumeType'] == 1 and r['ProductIndex'] == 0 and (not r['BonusCostumeIndices'] or r['MainIndex'] == r['CostumeIndex']) and (r['ReqBuyGem'] > 0 or r['ReqBuyGold'] > 0 or r['ReqBuyMileage'] > 0)
    for r in data['Costumes'].values():
        r['Group'] = costume_strings[r['Group']]
        for i in range(1,4):
            r[f'AbilityValue{i}'] = [costume_strings[x] for x in r[f'AbilityValue{i}'] or []]
    data['Prices'] = read('CreatureStarPriceTable')
    data['Stars'] = read('CreatureStarTable')
    data['EquipStarPrices'] = read('EquipItemStarTable')
    strings = pool('HeroBonusTable')
    data['HeroBonuses'] = read('HeroBonusTable')
    for r in data['HeroBonuses']:
        for i in range(1, 6):
            r[f'BonusValue{i}'] = [strings[x] for x in r[f'BonusValue{i}'] or []]
    data['SkillPrices'] = read('SkillPriceTable')
    data['Shops'] = {str(r['Index']): r for r in read('ShopTable')}
    strings = pool('ShopItemTable')
    data['ShopItems'] = [dict(r, ItemIndex=codes.get(strings[r['ItemCode']], 0), ItemCode=strings[r['ItemCode']]) for r in read('ShopItemTable')]
    for name,key in [('UpgradeBookItemTable','Books'),('HeroLimitBreakExpItemTable','LimitExpItems'),('HeroGrowthItemTable','GrowthItems'),('MultipleHeroSelectItemTable','MultiHeroItems'),('CostumeSelectItemTable','CostumeSelectors')]:
        strings=pool(name)
        data[key]=[dict(r, ItemIndex=codes[strings[r['ItemCode']]]) for r in read(name)]
    strings=pool('CostumeSelectItemTable')
    for r in data['CostumeSelectors']:
        r['CostumeCategories']=[strings[x] for x in r['CostumeCategories'] or []]
    strings=pool('CostumeGroupTable')
    data['CostumeGroups']={strings[r['GroupName']]:strings[r['SelectItemGroup']] for r in read('CostumeGroupTable')}
    for name,key in [('CreatureAwakeChallengeItemTable','Challenges'),('HeroAwakeTable','Awake'),('HeroLimitBreakTable','LimitBreaks')]:
        strings=pool(name)
        rows=read(name)
        for r in rows:
            for k in list(r):
                if 'ItemCode' in k:
                    r[k.replace('ItemCode','ItemIndex')]=codes.get(strings[r[k]],0)
        data[key]=rows
    strings=pool('HeroPresetStorageSlotTable')
    data['PresetSlots']={strings[r['StorageKey']]:dict(r, StorageKey=strings[r['StorageKey']]) for r in read('HeroPresetStorageSlotTable') if r.get('StorageKey')}
    strings=pool('ConstantTable')
    data['Constants']={strings[r['Key']]:strings[r['Value']] for r in read('ConstantTable')}
    data['Results']={}
    for d in (args.source/'NShared').iterdir():
        req=d/'Request.cs'; result=d/'ResultType.cs'
        if not req.is_file() or not result.is_file(): continue
        match=re.search(r'return "([^"]+)"',req.read_text(encoding='utf-8-sig'))
        if match:
            text=result.read_text(encoding='utf-8-sig')
            data['Results'][match[1].split('/')[-1]]=re.findall(r'^\s*([A-Za-z][A-Za-z0-9_]*)\s*(?:,|=\s*\d+,?)?\s*$',text,re.M)
    args.output.write_text(json.dumps(data,separators=(',',':'))+'\n',encoding='utf-8')
    print({k:len(v) for k,v in data.items()})


if __name__=='__main__':
    main()
