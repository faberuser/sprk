namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventGiftPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.EventGiftPoint;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static EventGiftPointResultInfo Create(CurrencyResultInfo3 info)
	{
		return new EventGiftPointResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
