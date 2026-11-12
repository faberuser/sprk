using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipGachaClientDataKey
{
	[Key(0)]
	public int GachaIndex;

	[Key(1)]
	public HighGachaCategoryType CategoryType;

	public EquipGachaClientDataKey(int gachaIndex, HighGachaCategoryType categoryType)
	{
		GachaIndex = gachaIndex;
		CategoryType = categoryType;
	}

	public bool Equals(EquipGachaClientDataKey other)
	{
		if (GachaIndex == other.GachaIndex)
		{
			return CategoryType == other.CategoryType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(GachaIndex, CategoryType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({GachaIndex}, {CategoryType})";
	}
}
