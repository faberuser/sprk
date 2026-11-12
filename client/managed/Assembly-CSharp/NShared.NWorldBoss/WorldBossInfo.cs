namespace NShared.NWorldBoss;

[JsonSerializable(Javascript = true)]
public class WorldBossInfo : IPersistentHpContentInfo
{
	public int Index { get; set; }

	public int Season { get; set; }

	public long MonsterHp0 { get; set; }

	public long MaxHp { get; set; }

	public long TotalDamage { get; set; }

	public WorldBossStatus Status { get; set; }

	public string CreatedTime { get; set; }

	public string EndedTime { get; set; }

	public string RewardedTime { get; set; }

	public string DailyRewardedTime { get; set; }

	public int DailyRewarding { get; set; }

	int IPersistentHpContentInfo.GetChapterIndex()
	{
		return GameRoot.Instance.WorldBossDataContainer.GetData(Index)?.ChapterIndex ?? 0;
	}

	int IPersistentHpContentInfo.GetDungeonIndex()
	{
		return GameRoot.Instance.WorldBossDataContainer.GetData(Index)?.DungeonIndex ?? 0;
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
		return MaxHp;
	}

	int IPersistentHpContentInfo.GetMaxMp(int waveSlotIndex)
	{
		return 0;
	}
}
