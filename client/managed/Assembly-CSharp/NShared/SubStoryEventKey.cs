using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SubStoryEventKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Sequence;

	public SubStoryEventKey(int index, int sequence)
	{
		Index = index;
		Sequence = sequence;
	}

	public bool Equals(SubStoryEventKey other)
	{
		if (Index == other.Index)
		{
			return Sequence == other.Sequence;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Sequence);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {Sequence})";
	}
}
