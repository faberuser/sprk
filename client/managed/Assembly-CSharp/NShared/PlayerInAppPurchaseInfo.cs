namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerInAppPurchaseInfo : IAccountAttrKey<string>
{
	string IAccountAttrKey<string>.Key => DeveloperPayload;

	public string DeveloperPayload { get; set; }

	public string Platform { get; set; }

	public string CreatedTime { get; set; }

	public string PurchaseStatus { get; set; }

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string OrderId { get; set; }

	public string PurchaseToken { get; set; }

	public string CurrencyCode { get; set; }

	public float Price { get; set; }
}
