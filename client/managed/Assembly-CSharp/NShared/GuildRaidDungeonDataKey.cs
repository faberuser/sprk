using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildRaidDungeonDataKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[Key(2)]
	public int Step;

	public GuildRaidDungeonDataKey(int chapterIndex, int dungeonIndex, int step)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Step = step;
	}

	public bool Equals(GuildRaidDungeonDataKey other)
	{
		if (ChapterIndex == other.ChapterIndex && DungeonIndex == other.DungeonIndex)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex, Step);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {DungeonIndex}, {Step})";
	}
}
