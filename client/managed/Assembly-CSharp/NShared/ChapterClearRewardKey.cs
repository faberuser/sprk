using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ChapterClearRewardKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Step;

	public ChapterClearRewardKey(int index, int step)
	{
		Index = index;
		Step = step;
	}

	public bool Equals(ChapterClearRewardKey other)
	{
		if (Index == other.Index)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Step);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {Step})";
	}
}
