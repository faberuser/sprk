using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipEnchantableConditionKey
{
	[Key(0)]
	public int TierIndex;

	[Key(1)]
	public int DetailIndex;

	public EquipEnchantableConditionKey(int tierIndex, int detailIndex)
	{
		TierIndex = tierIndex;
		DetailIndex = detailIndex;
	}

	public bool Equals(EquipEnchantableConditionKey other)
	{
		if (TierIndex == other.TierIndex)
		{
			return DetailIndex == other.DetailIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(TierIndex, DetailIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({TierIndex}, {DetailIndex})";
	}
}
