namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidDungeonInfo : IPersistentHpContentInfo
{
	public int GuildId { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int Step { get; set; }

	public string OpenedTime { get; set; }

	public int FinishMode { get; set; }

	public string CompletedTime { get; set; }

	public long ClearMemberId { get; set; }

	public long MonsterHp0 { get; set; }

	public int IsOngoing { get; set; }

	public int BonusDungeonOpenStep { get; set; }

	public int BonusChapterIndex { get; set; }

	public int BonusDungeonIndex { get; set; }

	public int IsBonusDungeon { get; set; }

	public int IsDummyDungeon { get; set; }

	public int CurrentChapterIndex => ChapterIndex;

	public int CurrentDungeonIndex => DungeonIndex;

	public int CurrentDungeonStep
	{
		get
		{
			if (CheckBonusDungeon)
			{
				return BonusDungeonOpenStep;
			}
			return Step;
		}
	}

	public bool CheckBonusDungeon
	{
		get
		{
			if (IsOngoing != 1)
			{
				return false;
			}
			if (IsBonusDungeon == 1)
			{
				return true;
			}
			return false;
		}
	}

	public bool CheckDummyDungeon
	{
		get
		{
			if (IsOngoing != 1)
			{
				return false;
			}
			if (IsDummyDungeon == 1)
			{
				return true;
			}
			return false;
		}
	}

	public bool CheckFinishMode
	{
		get
		{
			if (IsOngoing != 1)
			{
				return false;
			}
			if (FinishMode != 1)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsDead(int waveIndex = 0)
	{
		if (MonsterHp0 <= -1)
		{
			return true;
		}
		return false;
	}

	int IPersistentHpContentInfo.GetChapterIndex()
	{
		return ChapterIndex;
	}

	int IPersistentHpContentInfo.GetDungeonIndex()
	{
		return DungeonIndex;
	}

	int IPersistentHpContentInfo.GetDifficulty()
	{
		return 1;
	}

	long IPersistentHpContentInfo.GetSavedMonsterHp(int waveSlotIndex)
	{
		return MonsterHp0;
	}

	long IPersistentHpContentInfo.GetMaxHp(int waveSlotIndex)
	{
		return 0L;
	}

	int IPersistentHpContentInfo.GetMaxMp(int waveSlotIndex)
	{
		return 0;
	}
}
