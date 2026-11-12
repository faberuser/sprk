using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class RaidMultiInfoDataContainer : JitDataContainerDefault<int, RaidMultiInfoData>
{
	public const string TABLENAME = "RaidMultiInfoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RaidMultiInfoTable");

	public List<RaidMultiInfoData> GetRaidMultiInfoDataListByGroupIndex(int groupIndex)
	{
		IList<RaidMultiInfoData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<RaidMultiInfoData> list = new List<RaidMultiInfoData>();
		list.Clear();
		RaidMultiInfoData raidMultiInfoData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			raidMultiInfoData = allData[i];
			if (raidMultiInfoData != null && raidMultiInfoData.GroupIndex == groupIndex)
			{
				list.Add(raidMultiInfoData);
			}
		}
		return list;
	}
}
