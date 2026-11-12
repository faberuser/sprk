namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerPurchaseTimeDurationInfo : IAccountAttrKey<PurchaseInfoKey>
{
	PurchaseInfoKey IAccountAttrKey<PurchaseInfoKey>.Key => new PurchaseInfoKey(ShopIndex, Index);

	public int ShopIndex { get; set; }

	public int Index { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }

	public int PurchasedCount { get; set; }

	public string MailSentTime { get; set; }

	public int AttendanceGroupIndex { get; set; }
}
