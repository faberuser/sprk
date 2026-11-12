namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressPlayInfo : IPersistentHpContentInfo
{
	public int SessionIndex { get; set; }

	public string ServerGroup { get; set; }

	public int GuildId { get; set; }

	public int Level { get; set; }

	public int SeasonIndex { get; set; }

	public int RaidIndex { get; set; }

	public long MonsterHp0 { get; set; }

	public long TotalDamage { get; set; }

	public bool KilledByMe { get; set; }

	public GuildSuppressPlayInfo()
	{
	}

	public GuildSuppressPlayInfo(int raidIndex, int level)
	{
		RaidIndex = raidIndex;
		Level = level;
	}

	public bool IsDead()
	{
		if (MonsterHp0 == -1)
		{
			return true;
		}
		return false;
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
