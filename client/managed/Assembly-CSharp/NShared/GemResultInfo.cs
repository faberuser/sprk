namespace NShared;

[JsonSerializable(Javascript = true)]
public class GemResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.Gem;

	long ICurrencyResultInfo.NewValue => checked(NewSysGem + NewPayGem);

	public long AddValue { get; set; }

	public byte FirstDiffClear { get; set; }

	public long NewSysGem { get; set; }

	public long NewPayGem { get; set; }

	public long Sequence { get; set; }

	public static GemResultInfo Create(CurrencyResultInfo3 info)
	{
		return new GemResultInfo
		{
			AddValue = info.AddValue,
			Sequence = info.GetFieldValue("Sequence"),
			NewSysGem = info.GetFieldValue("NewSysGem"),
			NewPayGem = info.GetFieldValue("NewPayGem")
		};
	}
}
