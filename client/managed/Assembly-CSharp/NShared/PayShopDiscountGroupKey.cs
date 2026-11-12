using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PayShopDiscountGroupKey
{
	[Key(0)]
	public int GroupIndex;

	[Key(1)]
	public int Step;

	public PayShopDiscountGroupKey(int groupIndex, int step)
	{
		GroupIndex = groupIndex;
		Step = step;
	}

	public bool Equals(PayShopDiscountGroupKey other)
	{
		if (GroupIndex == other.GroupIndex)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(GroupIndex, Step);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({GroupIndex}, {Step})";
	}
}
