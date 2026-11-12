namespace NShared;

[JsonSerializable(Javascript = true)]
public class PvpCoinResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.PvpCoin;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static PvpCoinResultInfo Create(CurrencyResultInfo3 info)
	{
		return new PvpCoinResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
