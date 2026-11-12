using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct OrdealArenaRewardAreaDataKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public MatchTierType MatchTierType;

	public OrdealArenaRewardAreaDataKey(int index, MatchTierType matchTierType)
	{
		Index = index;
		MatchTierType = matchTierType;
	}

	public bool Equals(OrdealArenaRewardAreaDataKey other)
	{
		if (Index == other.Index)
		{
			return MatchTierType == other.MatchTierType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, MatchTierType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Index}, {MatchTierType})";
	}
}
