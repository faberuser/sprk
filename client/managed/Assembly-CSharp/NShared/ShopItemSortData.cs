using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopItemSortData : IDataKeySelector<int>, IShopItemFilterInfo
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public int SortingOrder { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	[Key(4)]
	public EShopItemType ShopItemType { get; set; }

	[Key(5)]
	public EShopSortType ShopSortType { get; set; }

	int IDataKeySelector<int>.Key => Index;

	EShopItemType IShopItemFilterInfo.ShopItemType => ShopItemType;

	EShopSortType IShopItemFilterInfo.ShopSortType => ShopSortType;
}
