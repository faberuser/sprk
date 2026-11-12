namespace NShared.NWorldBoss;

[JsonSerializable(Javascript = true)]
public class EventWorldBossInfo : IPersistentHpContentInfo
{
	public int Index { get; set; }

	public int Season { get; set; }

	public int OrderNo { get; set; }

	public int BossLevel { get; set; }

	public long MonsterHp0 { get; set; }

	public long MaxHp { get; set; }

	public long TotalDamage { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string NextOpenTime { get; set; }

	public string RewardedTime { get; set; }

	public string CreatedTime { get; set; }

	public string BattleCompletedTime { get; set; }

	public int DailyBossKillCount { get; set; }

	int IPersistentHpContentInfo.GetChapterIndex()
	{
		return GameRoot.Instance.EventWorldBossDataContainer.GetData(Index, BossLevel)?.ChapterIndex ?? 0;
	}

	int IPersistentHpContentInfo.GetDungeonIndex()
	{
		return GameRoot.Instance.EventWorldBossDataContainer.GetData(Index, BossLevel)?.DungeonIndex ?? 0;
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
