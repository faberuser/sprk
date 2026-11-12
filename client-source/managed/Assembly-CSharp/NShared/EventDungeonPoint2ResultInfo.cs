namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventDungeonPoint2ResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.EventDungeonPoint2;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static EventDungeonPoint2ResultInfo Create(CurrencyResultInfo3 info)
	{
		return new EventDungeonPoint2ResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
