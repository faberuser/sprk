using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct LuckyArenaRewardDataKey
{
	[Key(0)]
	public LuckyMatchRewardType TierIndex;

	[Key(1)]
	public int ShowIndex;

	public LuckyArenaRewardDataKey(LuckyMatchRewardType tierIndex, int showIndex)
	{
		TierIndex = tierIndex;
		ShowIndex = showIndex;
	}

	public bool Equals(LuckyArenaRewardDataKey other)
	{
		if (TierIndex == other.TierIndex)
		{
			return ShowIndex == other.ShowIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(TierIndex, ShowIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({TierIndex}, {ShowIndex})";
	}
}
