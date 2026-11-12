using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildBuildingDataKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Level;

	public GuildBuildingDataKey(int index, int level)
	{
		Index = index;
		Level = level;
	}

	public bool Equals(GuildBuildingDataKey other)
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
		return $"{GetType().Name}({Index}, {Level})";
	}
}
