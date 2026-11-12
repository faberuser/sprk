namespace NShared;

[JsonSerializable(Javascript = true)]
public class MileageResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.Mileage;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static MileageResultInfo Create(CurrencyResultInfo3 info)
	{
		return new MileageResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
