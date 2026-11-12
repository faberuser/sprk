using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct RewardItemIconTypeDataKey
{
	[Key(0)]
	public RewardItemIconType IconType;

	[Key(1)]
	public CurrencyType CurrencyType;

	public RewardItemIconTypeDataKey(RewardItemIconType iconType, CurrencyType currencyType)
	{
		IconType = iconType;
		CurrencyType = currencyType;
	}

	public bool Equals(RewardItemIconTypeDataKey other)
	{
		if (IconType == other.IconType)
		{
			return CurrencyType == other.CurrencyType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(IconType, CurrencyType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({IconType}, {CurrencyType})";
	}
}
