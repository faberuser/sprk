using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class InspectUserDataContainer : JitDataContainerDefault<InspectUserKey, InspectUserData>
{
	public const string TABLENAME = "InspectUserTable";

	public override TablePathDefine PathDefine => new TablePathDefine("InspectUserTable");

	public InspectUserData[] GetCategoryData(InspectUserCategory category)
	{
		List<InspectUserData> list = new List<InspectUserData>();
		foreach (InspectUserData allDatum in GetAllData())
		{
			if (allDatum.Category == category)
			{
				list.Add(allDatum);
			}
		}
		list.Sort((InspectUserData l, InspectUserData r) => l.Type.CompareTo(r.Type));
		return list.ToArray();
	}
}
