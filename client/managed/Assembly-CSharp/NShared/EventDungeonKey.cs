using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EventDungeonKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	public EventDungeonKey(int chapterIndex, int dungeonIndex)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
	}

	public bool Equals(EventDungeonKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return DungeonIndex == other.DungeonIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex);
	}

	public override string ToString()
	{
		return $"ChapterIndex={ChapterIndex} DungeonIndex={DungeonIndex}";
	}
}
