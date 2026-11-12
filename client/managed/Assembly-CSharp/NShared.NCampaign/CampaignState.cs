using System.Collections.Generic;
using System.Linq;

namespace NShared.NCampaign;

public abstract class CampaignState : BattleModeState<CampaignState>
{
	protected void AddHeroExtraSkill(List<BattleCreatureCreateInfo> creatureCreateInfoList, CampaignWaveData waveData)
	{
		List<ExtraSkillInfo> list = new List<ExtraSkillInfo>();
		foreach (BattleCreatureCreateInfo creatureCreateInfo in creatureCreateInfoList)
		{
			if (creatureCreateInfo.AddExtraSkill != null && creatureCreateInfo.AddExtraSkill.Length != 0)
			{
				list.AddRange(creatureCreateInfo.AddExtraSkill);
			}
			list.AddRangeFromIndex(waveData.AddHeroExtraSkill);
			creatureCreateInfo.AddExtraSkill = list.ToArray();
			list.Clear();
		}
	}

	protected int GetWaveAreaMaxMx(CampaignWaveData waveData, int groupIndex)
	{
		if (groupIndex == 0)
		{
			return waveData.AreaPositionMx + waveData.AreaSizeMx / 2;
		}
		return waveData.AreaPositionMx2 + waveData.AreaSizeMx2 / 2;
	}

	protected void AddAllyWaveMember(CampaignWaveData waveData, int groupIndex, List<BattleCreatureCreateInfo> groupedCreatureCreateInfoList)
	{
		List<CampaignWaveData.CreatureDetailInfo> creatureList = waveData.GetCreatureList(includeBoss: true, useGroupFilter: true, groupIndex);
		if (creatureList == null || creatureList.Count == 0)
		{
			return;
		}
		for (int i = 0; i < creatureList.Count; i++)
		{
			CampaignWaveData.CreatureDetailInfo waveCreature = creatureList[i];
			if (waveCreature.IsAlly && waveCreature.IsWaveMember && !groupedCreatureCreateInfoList.Any((BattleCreatureCreateInfo e) => e.HeroInfo.HeroIndex == waveCreature.Index))
			{
				groupedCreatureCreateInfoList.Add(BattleCreatureCreateInfo.Create(waveCreature, 0));
			}
		}
	}
}
