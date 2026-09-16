"""Export dungeon rules and native HTTP contracts from the supplied client extraction."""
import argparse
import json
import re
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from export_tutorial_data import string_pool

TABLES = '''CampaignChapter CampaignDungeon Tower TowerFloor DOWDungeon UnderPrisonDungeon
TreasureHouseInfo TreasureHouseDungeon GodkingTrialGroup GodkingTrial DispatchState PartyDungeon
Raid RaidMultiInfo PunishmentRaid PunishmentRaidReward PunishmentRaidTrigger ShakmehDungeon ShakmehBoss
EclipseStart EclipseReward OrdealArenaTier OrdealArenaNode OrdealArenaEvent OrdealArenaBuff
OrdealArenaRewardArea OrdealArenaRewardRating WorldBoss EventWorldBoss EventWorldBossSeason
ChallengeRaid WorldBossDailyAchievement WorldBossReward WorldBossScoreReward ChallengeRaidReward
ChallengeRaidClearReward EventDungeon EventDungeonGroup SelectReward BanRule CurrencyType PunishmentGroup'''.split()
FAMILIES = '''campaign sweep dispatch maze_tower dow_dungeon under_prison treasure_house godking_trial
eclipse ordeal_arena punishment_raid shakmeh_dungeon party_dungeon raid world_boss event_world_boss'''.split()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    parser.add_argument('client', type=Path)
    args = parser.parse_args()
    data = {}
    for name in TABLES:
        rows = json.loads((args.decoded / (name+'Table.json')).read_text(encoding='utf-8'))
        pool = string_pool(args.jit / (name+'Table.jit'))
        for row in rows:
            if not isinstance(row, dict):
                raise ValueError('Unnamed columns require an explicit mapping: '+name)
            for key, value in list(row.items()):
                if 'Code' in key and isinstance(value, int): row[key] = pool[value]
                elif 'Code' in key and isinstance(value, list): row[key] = [pool[v] for v in value]
                elif (name == 'BanRule' and key.startswith('BanValue') or name == 'TowerFloor' and key == 'OpenTime') and isinstance(value,int): row[key] = pool[value]
        data[name] = rows
    creatures=json.loads((args.decoded/'CreatureTable.json').read_text(encoding='utf-8'))
    pool=string_pool(args.jit/'CreatureTable.jit')
    data['BattleHero']=[{'Index':r['Index'],'CodeName':pool[r['CodeName']],'TagType':r['TagType'],'AttrType':r['AttrType']} for r in creatures if r.get('PlayableCharacter')]
    contracts = {}
    enums = {}
    enum_sources = {}
    for p in (args.client/'NShared').rglob('*.cs'):
        text=p.read_text(encoding='utf-8-sig')
        if re.search(r'public enum '+re.escape(p.stem)+r'\b',text): enum_sources.setdefault(p.stem,text)
    for p in (args.client/'NShared').glob('*/Request.cs'):
        text = p.read_text(encoding='utf-8-sig')
        route = re.search(r'"((?:'+'|'.join(FAMILIES)+r')/\w+|match/get_season_info)"', text)
        if not route: continue
        result = p.with_name('ResultType.cs').read_text(encoding='utf-8-sig')
        response = p.with_name('Response.cs').read_text(encoding='utf-8-sig')
        request_fields=dict((key,typ) for typ,key in re.findall(r'public ([\w.<>\[\], ?]+) (\w+)\s*\{\s*get', text))
        for typ in request_fields.values():
            if typ not in enum_sources or typ in enums: continue
            number=0; values={}
            for line in enum_sources[typ].splitlines():
                member=re.match(r'^\s*(\w+)(?:\s*=\s*(-?\d+))?,?\s*$',line)
                if member:
                    number=int(member[2]) if member[2] else number
                    values[member[1]]=number;number+=1
            enums[typ]=values
        contracts[route[1]] = {
            'Request': request_fields,
            'Results': re.findall(r'^\s*(\w+)(?:\s*=\s*\d+)?,?\s*$', result, re.M),
            'Response': dict((key,typ) for typ,key in re.findall(r'public ([\w.<>\[\], ?]+) (\w+)\s*\{\s*get', response))
        }
    target = Path(__file__).resolve().parents[1]/'tables'/'BattleSupport.json'
    target.write_text(json.dumps({'Tables':data,'Contracts':contracts,'Enums':enums}, separators=(',',':'))+'\n',encoding='utf-8')
    print(f'Exported {len(data)} tables and {len(contracts)} contracts')

if __name__ == '__main__': main()
