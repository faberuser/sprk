using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class CraftSortDataContainer : JitDataContainerDefault<CraftSortKey, CraftSortData>
{
	public const string TABLENAME = "CraftSortTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CraftSortTable");

	public CraftSortData[] GetCategoryData(CraftSortCategory category)
	{
		List<CraftSortData> list = new List<CraftSortData>();
		foreach (CraftSortData allDatum in GetAllData())
		{
			if (allDatum.Category == category)
			{
				list.Add(allDatum);
			}
		}
		list.Sort((CraftSortData l, CraftSortData r) => l.Value.CompareTo(r.Value));
		return list.ToArray();
	}
}
