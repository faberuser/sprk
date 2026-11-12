namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerFirstPurchaseInfo : IAccountAttrKey<PurchaseInfoKey>
{
	PurchaseInfoKey IAccountAttrKey<PurchaseInfoKey>.Key => new PurchaseInfoKey(ShopIndex, Index);

	public int ShopIndex { get; set; }

	public int Index { get; set; }

	public string PurchasedTime { get; set; }

	public int Count { get; set; }

	public string PurchaseCountResetTime { get; set; }
}
