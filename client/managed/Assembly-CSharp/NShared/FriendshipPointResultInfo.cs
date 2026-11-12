namespace NShared;

[JsonSerializable(Javascript = true)]
public class FriendshipPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.FriendshipPoint;

	public long AddValue { get; set; }

	public long AddDailyAccValue { get; set; }

	public long NewValue { get; set; }

	public long NewDailyAccValue { get; set; }
}
