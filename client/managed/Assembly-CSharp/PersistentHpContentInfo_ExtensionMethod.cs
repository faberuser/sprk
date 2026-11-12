using System.Collections.Generic;
using NShared;
using NVespa;

public static class PersistentHpContentInfo_ExtensionMethod
{
	public static ResultCreatureInfo[] CreateResultCreatureInfos(this IPersistentHpContentInfo info)
	{
		if (info == null)
		{
			return null;
		}
		int chapterIndex = info.GetChapterIndex();
		int dungeonIndex = info.GetDungeonIndex();
		int difficulty = info.GetDifficulty();
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, (DungeonDifficultyType)difficulty, scenario: false, 1);
		if (data == null)
		{
			VespaDebug.LogError("WaveData not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex + " difficulty=" + difficulty);
			return null;
		}
		List<CampaignWaveData.CreatureDetailInfo> creatureList = data.GetCreatureList();
		if (creatureList == null || creatureList.Count == 0)
		{
			return null;
		}
		List<ResultCreatureInfo> list = null;
		for (int i = 0; i < creatureList.Count; i++)
		{
			CampaignWaveData.CreatureDetailInfo creatureDetailInfo = creatureList[i];
			int waveSlotIndex = creatureDetailInfo.WaveSlotIndex;
			bool flag = info.GetMaxHp(waveSlotIndex) == 0;
			long monsterRealHp = info.GetMonsterRealHp(waveSlotIndex, -99L);
			if (!(monsterRealHp == -99 && flag))
			{
				long num = info.GetMaxHp(waveSlotIndex);
				int maxMp = info.GetMaxMp(waveSlotIndex);
				if (num > 0 && monsterRealHp > 0 && monsterRealHp > num)
				{
					num = monsterRealHp;
				}
				int index = creatureDetailInfo.Index;
				int teamId = 1;
				string key = ResultCreatureInfo.GetKey(index, waveSlotIndex, teamId);
				ResultCreatureInfo item = new ResultCreatureInfo
				{
					Index = index,
					Key = key,
					Hp = monsterRealHp,
					Mp = 0,
					MaxHp = num,
					MaxMp = maxMp,
					TeamId = teamId
				};
				list = list ?? new List<ResultCreatureInfo>();
				list.Add(item);
			}
		}
		return list?.ToArray();
	}

	public static long GetMonsterRealHp(this IPersistentHpContentInfo info, int waveSlotIndex, long maxHp)
	{
		if (info == null)
		{
			return maxHp;
		}
		long savedMonsterHp = info.GetSavedMonsterHp(waveSlotIndex);
		return savedMonsterHp switch
		{
			-1L => 0L, 
			0L => maxHp, 
			_ => savedMonsterHp, 
		};
	}
}
