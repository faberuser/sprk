namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerPurchaseInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => ProductId;

	public string Platform { get; set; }

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string PurchaseToken { get; set; }

	public string OrderId { get; set; }

	public string PurchaseStatus { get; set; }

	public string PurchaseTime { get; set; }
}
