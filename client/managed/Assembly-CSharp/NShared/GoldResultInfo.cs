namespace NShared;

[JsonSerializable(Javascript = true)]
public class GoldResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.Gold;

	public long AddValue { get; set; }

	public long AddBoosterValue { get; set; }

	public long NewValue { get; set; }

	public long Sequence { get; set; }

	public static GoldResultInfo Create(CurrencyResultInfo3 info)
	{
		return new GoldResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue,
			Sequence = info.GetFieldValue("Sequence"),
			AddBoosterValue = info.GetFieldValue("AddBoosterValue")
		};
	}
}
