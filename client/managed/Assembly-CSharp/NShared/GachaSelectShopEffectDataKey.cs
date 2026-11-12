using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GachaSelectShopEffectDataKey
{
	[Key(0)]
	public string ItemGroupCode;

	[Key(1)]
	public string ItemCode;

	public GachaSelectShopEffectDataKey(string itemGroupCode, string itemCode)
	{
		ItemGroupCode = itemGroupCode;
		ItemCode = itemCode;
	}

	public bool Equals(GachaSelectShopEffectDataKey other)
	{
		if (ItemGroupCode == other.ItemGroupCode)
		{
			return ItemCode == other.ItemCode;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ItemGroupCode, ItemCode);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ItemGroupCode}, {ItemCode})";
	}
}
