namespace NShared;

[JsonSerializable(Javascript = true)]
public class ShopListItemInfo : IAccountAttrKey<ShopItemKey>
{
	ShopItemKey IAccountAttrKey<ShopItemKey>.Key => new ShopItemKey(ShopIndex, ListNo);

	public int ShopIndex { get; set; }

	public int ListNo { get; set; }

	public string ItemCode { get; set; }

	public int Count { get; set; }

	public byte Sold { get; set; }

	public int PurchasableCount { get; set; }

	public int Purchased { get; set; }

	public int Star { get; set; }
}
