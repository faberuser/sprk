using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ClientCustomizingPivotDataKey
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public AccessoryPartPivotType PivotType;

	public ClientCustomizingPivotDataKey(int heroIndex, AccessoryPartPivotType pivotType)
	{
		HeroIndex = heroIndex;
		PivotType = pivotType;
	}

	public bool Equals(ClientCustomizingPivotDataKey other)
	{
		if (HeroIndex == other.HeroIndex)
		{
			return PivotType == other.PivotType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, PivotType);
	}

	public override string ToString()
	{
		return $"HeroIndex={HeroIndex} PivotType={PivotType}";
	}
}
