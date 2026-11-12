using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow;

public class NormalMonsterArchiveData
{
	public int MonsterIndex;

	public DungeonDifficultyType MainDifficulty;

	public Dictionary<DungeonDifficultyType, MonsterHeroInfo> MonsterInfos = new Dictionary<DungeonDifficultyType, MonsterHeroInfo>();

	public NormalMonsterArchiveData(int monsterIndex, DungeonDifficultyType mainDifficulty)
	{
		MonsterIndex = monsterIndex;
		MainDifficulty = mainDifficulty;
	}

	public void AddMonsterInfo(DungeonDifficultyType difficulty, MonsterHeroInfo monInfo)
	{
		if (MonsterInfos == null)
		{
			return;
		}
		if (MonsterInfos.ContainsKey(difficulty))
		{
			if (!MonsterInfos[difficulty].IsBoss && monInfo.IsBoss)
			{
				MonsterInfos[difficulty] = monInfo;
			}
			else if ((!MonsterInfos[difficulty].IsBoss || monInfo.IsBoss) && MonsterInfos[difficulty].HeroInfo.Level < monInfo.HeroInfo.Level)
			{
				MonsterInfos[difficulty] = monInfo;
			}
		}
		else
		{
			MonsterInfos.Add(difficulty, monInfo);
		}
	}

	public MonsterHeroInfo GetMainMonsterInfo()
	{
		if (!MonsterInfos.ContainsKey(MainDifficulty))
		{
			return null;
		}
		return MonsterInfos[MainDifficulty];
	}

	public MonsterHeroInfo GetMonsterInfo(DungeonDifficultyType difficulty)
	{
		if (!MonsterInfos.ContainsKey(difficulty))
		{
			return null;
		}
		return MonsterInfos[difficulty];
	}
}
