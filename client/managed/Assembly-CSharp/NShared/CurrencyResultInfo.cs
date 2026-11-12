namespace NShared;

[JsonSerializable(Javascript = true)]
public class CurrencyResultInfo
{
	public RewardType CurrencyType { get; set; }

	public string ResultString { get; set; }
}
