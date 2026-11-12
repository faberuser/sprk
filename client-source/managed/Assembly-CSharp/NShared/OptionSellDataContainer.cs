using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class OptionSellDataContainer : JitDataContainerDefault<OptionSellKey, OptionSellData>
{
	public const string TABLENAME = "OptionSellTable";

	public override TablePathDefine PathDefine => new TablePathDefine("OptionSellTable");

	public OptionSellData[] GetCategoryData(OptionSellCategory category)
	{
		List<OptionSellData> list = new List<OptionSellData>();
		foreach (OptionSellData allDatum in GetAllData())
		{
			if (allDatum.Category == category)
			{
				list.Add(allDatum);
			}
		}
		list.Sort((OptionSellData l, OptionSellData r) => l.Value.CompareTo(r.Value));
		return list.ToArray();
	}
}
