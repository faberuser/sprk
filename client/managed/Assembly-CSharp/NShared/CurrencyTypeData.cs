using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CurrencyTypeData : IDataKeySelector<CurrencyType>
{
	[Key(0)]
	public CurrencyType CurrencyType { get; set; }

	[Key(1)]
	public string StaminaType { get; set; }

	[Key(2)]
	public string UserStatType { get; set; }

	[Key(3)]
	public int Width { get; set; }

	[Key(4)]
	public string SpriteName { get; set; }

	[Key(5)]
	public string EffectCode { get; set; }

	[Key(6)]
	public string ButtonAction { get; set; }

	[Key(7)]
	public int DailyLimitValue { get; set; }

	[Key(8)]
	public string Condition1 { get; set; }

	[Key(9)]
	public string Condition2 { get; set; }

	[Key(10)]
	public string Condition3 { get; set; }

	[Key(11)]
	public string Condition4 { get; set; }

	[Key(12)]
	public string Condition5 { get; set; }

	[Key(13)]
	public int ExchangeCostValue { get; set; }

	[Key(14)]
	public int MaxValue { get; set; }

	CurrencyType IDataKeySelector<CurrencyType>.Key => CurrencyType;

	public List<string> GetConditonList()
	{
		List<string> list = new List<string>();
		if (Condition1 != null && Condition1 != string.Empty)
		{
			list.Add(Condition1);
		}
		if (Condition2 != null && Condition2 != string.Empty)
		{
			list.Add(Condition2);
		}
		if (Condition3 != null && Condition3 != string.Empty)
		{
			list.Add(Condition3);
		}
		if (Condition4 != null && Condition4 != string.Empty)
		{
			list.Add(Condition4);
		}
		if (Condition5 != null && Condition5 != string.Empty)
		{
			list.Add(Condition5);
		}
		if (list.Count() > 0)
		{
			return list;
		}
		return null;
	}
}
