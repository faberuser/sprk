using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildRaidBonusDungeonDataKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[Key(2)]
	public int BonusGroup;

	public GuildRaidBonusDungeonDataKey(int chapterIndex, int dungeonIndex, int bonusGroup)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		BonusGroup = bonusGroup;
	}

	public bool Equals(GuildRaidBonusDungeonDataKey other)
	{
		if (ChapterIndex == other.ChapterIndex && DungeonIndex == other.DungeonIndex)
		{
			return BonusGroup == other.BonusGroup;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex, BonusGroup);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {DungeonIndex}, {BonusGroup})";
	}
}
