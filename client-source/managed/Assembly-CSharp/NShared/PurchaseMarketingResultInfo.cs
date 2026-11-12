namespace NShared;

[JsonSerializable(Javascript = true)]
public class PurchaseMarketingResultInfo
{
	public int PurchaseIndex { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public string PurchasedTime { get; set; }

	public int Count { get; set; }

	public byte Active { get; set; }

	public byte IsOpenPopup { get; set; }
}
