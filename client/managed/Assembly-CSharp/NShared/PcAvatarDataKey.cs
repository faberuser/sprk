using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PcAvatarDataKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int Index;

	public PcAvatarDataKey(int chapterIndex, int index)
	{
		ChapterIndex = chapterIndex;
		Index = index;
	}

	public bool Equals(PcAvatarDataKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, Index);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {Index})";
	}
}
