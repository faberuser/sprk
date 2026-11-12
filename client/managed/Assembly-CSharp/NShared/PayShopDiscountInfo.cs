namespace NShared;

[JsonSerializable(Javascript = true)]
public class PayShopDiscountInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ProductIndex;

	public int ProductIndex { get; set; }

	public int PurchasedCount { get; set; }

	public int Step { get; set; }

	public int DiscountPrice { get; set; }

	public int DefaultPrice { get; set; }

	public int ServerLimitedCount { get; set; }
}
