using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EventDungeonGroupKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int GroupIndex;

	public EventDungeonGroupKey(int chapterIndex, int groupIndex)
	{
		ChapterIndex = chapterIndex;
		GroupIndex = groupIndex;
	}

	public bool Equals(EventDungeonGroupKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return GroupIndex == other.GroupIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, GroupIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {GroupIndex})";
	}
}
