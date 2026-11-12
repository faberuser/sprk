namespace NShared;

[JsonSerializable(Javascript = true)]
public class WorldBossPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.WorldBossPoint;

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public static WorldBossPointResultInfo Create(CurrencyResultInfo3 info)
	{
		return new WorldBossPointResultInfo
		{
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
	}
}
