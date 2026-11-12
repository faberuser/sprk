namespace NShared;

[JsonSerializable(Javascript = true)]
public class ShopItemInfo
{
	public int ShopIndex { get; set; }

	public int ListNo { get; set; }

	public byte Sold { get; set; }

	public int Purchased { get; set; }

	public int PurchasedTime { get; set; }
}
