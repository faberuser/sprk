using System.Collections.Generic;
using System.Linq;
using NShared.NJit;

namespace NShared;

public class CampaignDungeonDataContainer : JitDataContainerDefault<ChapterDungeonKey2, CampaignDungeonData>
{
	public const string TABLENAME = "CampaignDungeonTable";

	private const int MaxStar = 3;

	private JitDictionary<int, int> _originalDungeonDictionary = new JitDictionary<int, int>();

	private JitDictionary<int, List<CampaignDungeonData>> _sortedLists_by_ChapterIndex = new JitDictionary<int, List<CampaignDungeonData>>();

	private JitDictionary<BattleType, List<CampaignDungeonData>> _lists_by_BattleType = new JitDictionary<BattleType, List<CampaignDungeonData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignDungeonTable");

	public CampaignDungeonDataContainer()
	{
		base.Packer.RegisterAndBind(_originalDungeonDictionary);
		base.Packer.RegisterAndBind(_sortedLists_by_ChapterIndex);
		base.Packer.RegisterAndBind(_lists_by_BattleType);
	}

	protected override void OnEndLoadFromJsonData()
	{
		foreach (CampaignDungeonData item in base.BaseList.JsonLoad_GetOrNew())
		{
			if (item.FirstDungeonIndex > 0 && item.FirstDungeonIndex != item.DungeonIndex)
			{
				_originalDungeonDictionary.JsonLoad_Get().Remove(item.FirstDungeonIndex);
				_originalDungeonDictionary.JsonLoad_Get().Add(item.FirstDungeonIndex, item.DungeonIndex);
			}
			_sortedLists_by_ChapterIndex.JsonLoad_AddToListValue(item.ChapterIndex, item);
			_lists_by_BattleType.JsonLoad_AddToListValue(item.BattleType, item);
		}
		_sortedLists_by_ChapterIndex.JsonLoad_SortListValue((CampaignDungeonData lhs, CampaignDungeonData rhs) => lhs.DungeonIndex.CompareTo(rhs.DungeonIndex));
	}

	public int GetOriginalDungeonIndex(int linkedDungeonIndex)
	{
		return _originalDungeonDictionary.GetOrDefault(linkedDungeonIndex, linkedDungeonIndex);
	}

	public CampaignDungeonData GetData(int chapterIndex, int dungeonIndex)
	{
		return GetData(new ChapterDungeonKey2(chapterIndex, dungeonIndex));
	}

	public IList<CampaignDungeonData> GetAllChapterData(int chapterIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex));
	}

	public CampaignDungeonData[] GetConquestDungeonData(int chapterIndex)
	{
		return (from v in __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex))
			where v.BattleType == BattleType.Conquest
			select v).ToArray();
	}

	public IList<CampaignDungeonData> GetAllDataOfBattleType(BattleType battleType, int chapterIndex = 0)
	{
		IList<CampaignDungeonData> emptyIfNull = __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_lists_by_BattleType.GetOrDefault(battleType));
		if (chapterIndex == 0)
		{
			return emptyIfNull;
		}
		return emptyIfNull.Where((CampaignDungeonData v) => chapterIndex == v.ChapterIndex).ToArray();
	}

	public CampaignDungeonData GetTrialDungeonData(int heroIndex, int heroStar, int heroTranscend = 0)
	{
		HeroAwakeData data = GameRoot.Instance.HeroAwakeDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return null;
		}
		int[] dungeonIndex = data.GetDungeonIndex(heroStar, heroTranscend);
		if (dungeonIndex == null || dungeonIndex.Length < 2)
		{
			return null;
		}
		return GetData(dungeonIndex[0], dungeonIndex[1]);
	}

	public string GetFieldName(int chapterIndex, int dungeonIndex)
	{
		return GetData(chapterIndex, dungeonIndex)?.BattleField;
	}

	public bool IsScenarioDungeon(int chapterIndex, int dungeonIndex)
	{
		return GetData(chapterIndex, dungeonIndex)?.Scenario ?? false;
	}

	public bool IsLastDungeonInChapter(int chapterIndex, int dungeonIndex)
	{
		IList<CampaignDungeonData> allChapterData = GetAllChapterData(chapterIndex);
		if (__Extension_Method_ICollection__.COUNT(allChapterData) > 0)
		{
			CampaignDungeonData campaignDungeonData = allChapterData[__Extension_Method_ICollection__.COUNT(allChapterData) - 1];
			if (campaignDungeonData != null && campaignDungeonData.DungeonIndex == dungeonIndex)
			{
				return true;
			}
		}
		return false;
	}

	public CreatureData GetBossCreatureData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, out CampaignWaveData bossWaveData, out CampaignWaveData.CreatureDetailInfo bossWaveDetailInfo, out HeroInfo bossHeroInfo)
	{
		bossWaveData = null;
		bossWaveDetailInfo = null;
		bossHeroInfo = null;
		if (GetData(chapterIndex, dungeonIndex) == null)
		{
			return null;
		}
		int num = 0;
		int num2 = 1;
		while (true)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, num2);
			if (data == null)
			{
				break;
			}
			if (data.BossCreatureIndex != 0)
			{
				num = data.BossCreatureIndex;
				bossWaveData = data;
				break;
			}
			num2++;
		}
		if (num == 0)
		{
			return null;
		}
		if (!bossWaveData.GetCreatureHeroInfo((CampaignWaveData.CreatureDetailInfo e) => e.IsBoss, out bossWaveDetailInfo, out bossHeroInfo))
		{
			return null;
		}
		return GameRoot.Instance.CreatureDataContainer.GetData(num);
	}

	public CreatureData GetBossCreatureData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, out CampaignWaveData bossWaveData)
	{
		CampaignWaveData.CreatureDetailInfo bossWaveDetailInfo;
		HeroInfo bossHeroInfo;
		return GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario, out bossWaveData, out bossWaveDetailInfo, out bossHeroInfo);
	}

	public CreatureData GetBossCreatureData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario)
	{
		CampaignWaveData bossWaveData;
		return GetBossCreatureData(chapterIndex, dungeonIndex, difficulty, scenario, out bossWaveData);
	}

	public CreatureData GetBossCreatureData(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, int waveIndex, out CampaignWaveData bossWaveData, out CampaignWaveData.CreatureDetailInfo bossWaveDetailInfo, out HeroInfo bossHeroInfo)
	{
		bossWaveData = null;
		bossWaveDetailInfo = null;
		bossHeroInfo = null;
		if (GetData(chapterIndex, dungeonIndex) == null)
		{
			return null;
		}
		int num = 0;
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, waveIndex);
		if (data == null)
		{
			return null;
		}
		if (data.BossCreatureIndex != 0)
		{
			num = data.BossCreatureIndex;
			bossWaveData = data;
		}
		if (num == 0)
		{
			return null;
		}
		if (!bossWaveData.GetCreatureHeroInfo((CampaignWaveData.CreatureDetailInfo e) => e.IsBoss, out bossWaveDetailInfo, out bossHeroInfo))
		{
			return null;
		}
		return GameRoot.Instance.CreatureDataContainer.GetData(num);
	}

	public List<int> GetBattleDungeonIndices(int chapterIndex)
	{
		return (from v in __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex))
			where v.CheckClearChapter > 0
			select v.DungeonIndex).ToList();
	}

	public int GetTotalStar(int chapterIndex)
	{
		return GetBattleDungeonIndices(chapterIndex).Count * 3;
	}

	public List<int> GetDungeonIndexByChapterIndex(int chapterIndex)
	{
		return (from v in __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_sortedLists_by_ChapterIndex.GetOrDefault(chapterIndex))
			where v.BattleType != BattleType.SubStory && v.BattleType != BattleType.SubStoryBoss
			where v.CheckClearChapter > 0 || v.BattleDefineCode.Contains("MultiOnly")
			select v.DungeonIndex).ToList();
	}

	public int GetTotalStarByChapterClearReward(int chapterIndex)
	{
		return GetDungeonIndexByChapterIndex(chapterIndex).Count * 3;
	}
}
