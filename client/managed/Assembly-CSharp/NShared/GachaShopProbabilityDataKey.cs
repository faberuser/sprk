using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GachaShopProbabilityDataKey
{
	[Key(0)]
	public int GachaIndex;

	[Key(1)]
	public HighGachaCategoryType GachaType;

	public GachaShopProbabilityDataKey(int gachaIndex, HighGachaCategoryType gachaType)
	{
		GachaIndex = gachaIndex;
		GachaType = gachaType;
	}

	public bool Equals(GachaShopProbabilityDataKey other)
	{
		if (GachaIndex == other.GachaIndex)
		{
			return GachaType == other.GachaType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(GachaIndex, GachaType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({GachaIndex}, {GachaType})";
	}
}
