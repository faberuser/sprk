"""Resolve attendance, achievement, and quest rules from each client JIT pool."""
import argparse
import json
import re
import sys
from pathlib import Path
sys.dont_write_bytecode = True
from export_tutorial_data import string_pool

def main():
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('decoded', type=Path)
    p.add_argument('jit', type=Path)
    p.add_argument('source', type=Path)
    a = p.parse_args()
    def enum(name):
        text = (a.source/'NShared'/(name+'.cs')).read_text(encoding='utf-8-sig')
        return re.findall(r'^\s*([A-Za-z][A-Za-z0-9_]*)\s*,?\s*$', text, re.M)
    def rows(name, fields=(), arrays=()):
        data = json.loads((a.decoded/(name+'.json')).read_text(encoding='utf-8'))
        pool = string_pool(a.jit/(name+'.jit'))
        for r in data:
            for k in fields: r[k] = pool[r[k]]
            for k in arrays: r[k] = [pool[x] for x in r[k] or []]
        return data
    data = {}
    data['Achievements'] = rows('AchievementTable', ['ReqValue','CheckType','RewardCode','Reward2Code','EventName','HandlerName'], ['OpenCondition','HandlerCondition'])
    kinds = enum('AchievementReqType')
    rewards = enum('AchievementRewardType')
    for r in data['Achievements']:
        r['Kind'] = kinds[r['ReqType']]
        r['RewardKind'] = rewards[r['RewardType']]
        r['Reward2Kind'] = rewards[r['Reward2Type']]
    data['SubQuests'] = rows('SubQuestTable', arrays=['StateValue'])
    data['MainQuests'] = rows('MainQuestTable', arrays=['StateValue'])
    data['ClearMissions'] = rows('ClearMissionProductMissionTable')
    kinds = enum('QuestActionType')
    for group in ['SubQuests','MainQuests','ClearMissions']:
        for r in data[group]: r['Kind'] = kinds[r['StateType']]
    data['ClearProducts'] = rows('ClearMissionProductTable', arrays=['MissionOpenCondition'])
    data['NewbieMissions'] = rows('NewbieRewardProductMissionTable', arrays=['Condition'])
    data['NewbieProducts'] = rows('NewbieRewardProductTable')
    data['LoginRewards'] = rows('AccumulateLoginRewardTable')
    data['ChapterRewards'] = rows('ChapterClearRewardTable', ['RewardItemCode'])
    data['AttendanceInfo'] = rows('AttendanceInfoTable', arrays=['ConditionValue1'])
    data['Chapters'] = rows('CampaignChapterTable')
    data['MissionCategories'] = rows('MissionCategoryTable')
    data['MissionVisuals'] = rows('MissionVisualTable')
    (Path(__file__).resolve().parents[1]/'tables/ProgressionSupport.json').write_text(json.dumps(data,separators=(',',':'))+'\n',encoding='utf-8')
    print({k:len(v) for k,v in data.items()})

if __name__ == '__main__': main()
