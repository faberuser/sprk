using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CurrencyTypeDataContainer : JitDataContainerDefault<CurrencyType, CurrencyTypeData>
{
	public const string TABLENAME = "CurrencyTypeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CurrencyTypeTable");

	public CurrencyTypeData GetCurrencyData(CurrencyType currency)
	{
		List<CurrencyTypeData> list = new List<CurrencyTypeData>();
		foreach (CurrencyTypeData allDatum in GetAllData())
		{
			if (allDatum.CurrencyType == currency)
			{
				list.Add(allDatum);
			}
		}
		if (list.Count > 0)
		{
			return list[0];
		}
		return null;
	}
}
