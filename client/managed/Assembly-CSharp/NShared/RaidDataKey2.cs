using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct RaidDataKey2
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Level;

	public RaidDataKey2(int index, int level)
	{
		Index = index;
		Level = level;
	}

	public bool Equals(RaidDataKey2 other)
	{
		if (Index == other.Index)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Level);
	}

	public override string ToString()
	{
		return $"Index={Index} Level={Level}";
	}
}
