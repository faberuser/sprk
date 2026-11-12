namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressInfo : IPersistentHpContentInfo
{
	public int SessionIndex { get; set; }

	public int GuidId { get; set; }

	public int GroupIndex { get; set; }

	public string AppliedTime { get; set; }

	public int RaidIndex { get; set; }

	public int Level { get; set; }

	public long MonsterHp0 { get; set; }

	public long TotalDamage { get; set; }

	public string ServerGroup { get; set; }

	public GuildSuppressInfo()
	{
	}

	public GuildSuppressInfo(int raidIndex, int level)
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
