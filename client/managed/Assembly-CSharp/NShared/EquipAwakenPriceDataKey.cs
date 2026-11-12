using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipAwakenPriceDataKey
{
	[Key(0)]
	public int DetailIndex;

	[Key(1)]
	public int Star;

	[Key(2)]
	public int Tier;

	public EquipAwakenPriceDataKey(int detailIndex, int star, int tier)
	{
		DetailIndex = detailIndex;
		Star = star;
		Tier = tier;
	}

	public bool Equals(EquipAwakenPriceDataKey other)
	{
		if (DetailIndex == other.DetailIndex && Star == other.Star)
		{
			return Tier == other.Tier;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(DetailIndex, Star, Tier);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({DetailIndex}, {Star}, {Tier})";
	}
}
