namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventDungeonPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.EventDungeonPoint;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public long DayValue { get; set; }

	public static EventDungeonPointResultInfo Create(CurrencyResultInfo3 info)
	{
		return new EventDungeonPointResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue,
			DayValue = info.GetFieldValue("DayValue")
		};
	}
}
