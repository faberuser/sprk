namespace NShared;

[JsonSerializable(Javascript = true)]
public class RaidPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.RaidPoint;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static RaidPointResultInfo Create(CurrencyResultInfo3 info)
	{
		return new RaidPointResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
