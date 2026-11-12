using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ShopItemSortGroupData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EShopItemFilterGroupType GroupType { get; set; }

	[Key(2)]
	public int SortingOrder { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
