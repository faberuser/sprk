using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewbieRewardProductMissionDataContainer : JitDataContainerDefault<int, NewbieRewardProductMissionData>
{
	public const string TABLENAME = "NewbieRewardProductMissionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewbieRewardProductMissionTable");

	public NewbieRewardProductMissionData[] GetMissionList(int groupIndex)
	{
		IList<NewbieRewardProductMissionData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		List<NewbieRewardProductMissionData> list = new List<NewbieRewardProductMissionData>();
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardProductMissionData newbieRewardProductMissionData = allData[i];
			if (newbieRewardProductMissionData != null && newbieRewardProductMissionData.GroupIndex == groupIndex)
			{
				list.Add(newbieRewardProductMissionData);
			}
		}
		return list.ToArray();
	}
}
