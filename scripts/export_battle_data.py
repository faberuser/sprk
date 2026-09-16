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

def keyed_schema(client, name):
    path = client / 'NShared' / (name + '.cs')
    if not path.exists(): return {}
    text = path.read_text(encoding='utf-8-sig')
    schema = {}
    base = re.search(r'public class '+re.escape(name)+r'\s*:\s*(\w+)', text)
    if base and base[1] != name: schema.update(keyed_schema(client, base[1]))
    for index, typ, key in re.findall(r'\[Key\((\d+)\)\]\s*public ([\w\[\]]+) (\w+)\s*\{\s*get', text):
        schema[int(index)] = (key, typ)
    return schema

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('decoded', type=Path)
    parser.add_argument('jit', type=Path)
    parser.add_argument('client', type=Path)
    parser.add_argument('--profile', choices=['battle', 'arena-guild'], default='battle')
    args = parser.parse_args()
    arena_guild = args.profile == 'arena-guild'
    names = ('''MatchTier MatchSeason MatchReward MatchBanPickSeason LuckyArenaReward
GuildLevel GuildPenalty GuildContributeReward GuildAttendanceReward GuildSkill GuildSkillLevel GuildBuilding
GuildRaidChapter GuildRaidDungeon GuildRaidChapterReward GuildRaidBonusDungeon GuildRaidDummyDungeon
GuildArenaTier GuildArenaSeason GuildArenaSeasonReward GuildArenaServerBuffReward
GuildSuppressChapter GuildSuppressDungeon GuildSuppressSession GuildSuppressReward GuildSuppressTotalReward
GuildSuppressGlobalReward GuildSuppressServerReward'''.split() if arena_guild else TABLES)
    families = ('guild match global_arena guild_raid guild_arena guild_suppress guild_ranking_board'.split() if arena_guild else FAMILIES)
    data = {}
    for name in names:
        rows = json.loads((args.decoded / (name+'Table.json')).read_text(encoding='utf-8'))
        pool = string_pool(args.jit / (name+'Table.jit'))
        schema_name = {'MatchBanPickSeason':'MatchSeason', 'GuildRaidDummyDungeon':'GuildRaidDungeon',
                       'GuildSuppressGlobalReward':'GuildSuppressReward', 'GuildSuppressServerReward':'GuildSuppressReward',
                       'GuildSuppressTotalReward':'GuildSuppressReward'}.get(name, name)
        schema = keyed_schema(args.client, schema_name+'Data') if arena_guild else {}
        types = {key:typ for key,typ in schema.values()}
        for i,row in enumerate(rows):
            if isinstance(row, list) and schema:
                row = {schema.get(j, (f'field_{j}', ''))[0]:v for j,v in enumerate(row)}
                rows[i] = row
            if not isinstance(row, dict):
                raise ValueError('Unnamed columns require an explicit mapping: '+name)
            for key in list(row):
                if key.startswith('field_') and key[6:].isdigit() and int(key[6:]) in schema:
                    row[schema[int(key[6:])][0]] = row.pop(key)
            for key, value in list(row.items()):
                if types.get(key) == 'string' and isinstance(value, int): row[key] = pool[value]
                elif types.get(key) == 'string[]' and isinstance(value,list): row[key] = [pool[v] for v in value]
                elif 'Code' in key and isinstance(value, int): row[key] = pool[value]
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
        route = re.search(r'"((?:'+'|'.join(families)+r')/\w+'+('' if arena_guild else '|match/get_season_info')+r')"', text)
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
    target = Path(__file__).resolve().parents[1]/'tables'/('ArenaGuildSupport.json' if arena_guild else 'BattleSupport.json')
    target.write_text(json.dumps({'Tables':data,'Contracts':contracts,'Enums':enums}, separators=(',',':'))+'\n',encoding='utf-8')
    print(f'Exported {len(data)} tables and {len(contracts)} contracts')

if __name__ == '__main__': main()
