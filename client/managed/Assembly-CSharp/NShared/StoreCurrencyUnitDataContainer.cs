using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class StoreCurrencyUnitDataContainer : JitDataContainerDefault<int, StoreCurrencyUnitData>
{
	public const string TABLENAME = "StoreCurrencyUnitTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StoreCurrencyUnitTable");

	public List<StoreCurrencyUnitData> GetDatas(CurrencyChangeType type)
	{
		List<StoreCurrencyUnitData> list = null;
		foreach (StoreCurrencyUnitData allDatum in GetAllData())
		{
			if (allDatum.ChangeType == type)
			{
				if (list == null)
				{
					list = new List<StoreCurrencyUnitData>();
				}
				list.Add(allDatum);
			}
		}
		return list;
	}
}
