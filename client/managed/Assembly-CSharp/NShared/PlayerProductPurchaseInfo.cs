namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerProductPurchaseInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ProductIndex;

	public int ProductIndex { get; set; }

	public int PurchasedCount { get; set; }

	public string PurchasedTime { get; set; }

	public string PurchaseCountResetTime { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }

	public string MailSentTime { get; set; }

	[NonDbSchema]
	public int ActionValue { get; set; }
}
