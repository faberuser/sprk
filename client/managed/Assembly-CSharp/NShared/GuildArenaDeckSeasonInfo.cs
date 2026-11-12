using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaDeckSeasonInfo : IPersistentHpContentInfo
{
	public int SeasonIndex { get; set; }

	public string ServerGroup { get; set; }

	public int GuidId { get; set; }

	public long AccountId { get; set; }

	public int DeckIndex { get; set; }

	public string HeroIndices { get; set; }

	public string SkillSlotIndices { get; set; }

	[NonDbSchema]
	public MatchedAccountInfo MatchedNpcInfo { get; set; }

	[Obsolete("CachedHeroInfo 사용하지 말고 MatchedNpcInfo 사용하자. 데이터베이스에 text로 저장되는 변수. JSON.string 형으로 저장한다.")]
	public string CachedAccountInfo { get; set; }

	public string AppliedTime { get; set; }

	public int RaidIndex { get; set; }

	public int Level { get; set; }

	public long MonsterHp0 { get; set; }

	public long TotalDamage { get; set; }

	public GuildArenaDeckSeasonInfo()
	{
	}

	public GuildArenaDeckSeasonInfo(int raidIndex, int level)
	{
		RaidIndex = raidIndex;
		Level = level;
	}

	int IPersistentHpContentInfo.GetChapterIndex()
	{
		return GameRoot.Instance.RaidDataContainer.GetData(RaidIndex, Level)?.ChapterIndex ?? 0;
	}

	int IPersistentHpContentInfo.GetDungeonIndex()
	{
		return GameRoot.Instance.RaidDataContainer.GetData(RaidIndex, Level)?.DungeonIndex ?? 0;
	}

	int IPersistentHpContentInfo.GetDifficulty()
	{
		return 0;
	}

	long IPersistentHpContentInfo.GetSavedMonsterHp(int waveSlotIndex)
	{
		if (waveSlotIndex == 0)
		{
			return MonsterHp0;
		}
		return 0L;
	}

	long IPersistentHpContentInfo.GetMaxHp(int waveSlotIndex)
	{
		return MonsterHp0;
	}

	int IPersistentHpContentInfo.GetMaxMp(int waveSlotIndex)
	{
		return 0;
	}
}
