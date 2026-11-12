namespace NShared;

[JsonSerializable(Javascript = true)]
public class LuaPointResultInfo : ICurrencyResultInfo
{
	CurrencyType ICurrencyResultInfo.CurrencyType => CurrencyType.LuaPoint;

	public long AddValue { get; set; }

	public long NewValue { get; set; }
}
