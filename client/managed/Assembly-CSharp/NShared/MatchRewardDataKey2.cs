using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct MatchRewardDataKey2
{
	[Key(0)]
	public int TierIndex;

	[Key(1)]
	public int MinRank;

	public MatchRewardDataKey2(int tierIndex, int minRank)
	{
		TierIndex = tierIndex;
		MinRank = minRank;
	}

	public bool Equals(MatchRewardDataKey2 other)
	{
		if (TierIndex == other.TierIndex)
		{
			return MinRank == other.MinRank;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(TierIndex, MinRank);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({TierIndex}, {MinRank})";
	}
}
