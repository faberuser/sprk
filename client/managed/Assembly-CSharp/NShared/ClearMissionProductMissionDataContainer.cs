using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ClearMissionProductMissionDataContainer : JitDataContainerDefault<int, ClearMissionProductMissionData>
{
	public const string TABLENAME = "ClearMissionProductMissionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ClearMissionProductMissionTable");

	public ClearMissionProductMissionData[] GetMissionList(int productGroupIndex)
	{
		IList<ClearMissionProductMissionData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<ClearMissionProductMissionData> list = new List<ClearMissionProductMissionData>();
		for (int i = 0; i < allData.Count; i++)
		{
			ClearMissionProductMissionData clearMissionProductMissionData = allData[i];
			if (clearMissionProductMissionData != null && clearMissionProductMissionData.ProductGroupIndex == productGroupIndex)
			{
				list.Add(clearMissionProductMissionData);
			}
		}
		return list.ToArray();
	}
}
