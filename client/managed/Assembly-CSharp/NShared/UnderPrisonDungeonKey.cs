using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct UnderPrisonDungeonKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int Diff;

	public UnderPrisonDungeonKey(int chapterIndex, int diff)
	{
		ChapterIndex = chapterIndex;
		Diff = diff;
	}

	public bool Equals(UnderPrisonDungeonKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return Diff == other.Diff;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, Diff);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {Diff})";
	}
}
