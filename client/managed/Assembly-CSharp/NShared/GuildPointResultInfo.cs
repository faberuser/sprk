namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.GuildPoint;

	public long AddValue { get; set; }

	public long AddDailyAccValue { get; set; }

	public long NewValue { get; set; }

	public long NewDailyAccValue { get; set; }
}
