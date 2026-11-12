namespace NShared;

[JsonSerializable(Javascript = true)]
public class CurrencyResultInfo2 : ICurrencyResultInfo
{
	public CurrencyType CurrencyType { get; set; }

	public long AddValue { get; set; }

	public long NewValue { get; set; }
}
