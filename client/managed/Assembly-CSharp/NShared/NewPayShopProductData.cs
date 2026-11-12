using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewPayShopProductData
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string GoogleId { get; set; }

	[Key(2)]
	public string OneStoreId { get; set; }

	[Key(3)]
	public string AppleId { get; set; }

	[Key(4)]
	public string UDPStoreId { get; set; }

	[Key(5)]
	public bool ExcludeConsoleRequest { get; set; }

	[Key(6)]
	public NewPayShopProductCategoryType Category { get; set; }

	[Key(7)]
	public ProductType ProductType { get; set; }

	[Key(8)]
	public string[] RewardAnyMiscTypes { get; set; }

	[Key(9)]
	public string ParentAnyMiscType { get; set; }
}
