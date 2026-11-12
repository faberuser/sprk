using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopItemFilterData : IDataKeySelector<int>, IShopItemFilterInfo
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string ItemCode { get; set; }

	[Key(2)]
	public EShopItemType ShopItemType { get; set; }

	int IDataKeySelector<int>.Key => Index;

	EShopItemType IShopItemFilterInfo.ShopItemType => ShopItemType;

	EShopSortType IShopItemFilterInfo.ShopSortType => EShopSortType.None;

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + "ShopItemType=" + ShopItemType;
	}
}
