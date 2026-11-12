using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class PushRewardDataContainer : JitDataContainerDefault<int, PushRewardData>
{
	public const string TABLENAME = "PushRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PushRewardTable");

	public List<PushRewardData> GetDataList(int dayOfWeek)
	{
		IList<PushRewardData> allData = GetAllData();
		List<PushRewardData> list = new List<PushRewardData>();
		list.Clear();
		PushRewardData pushRewardData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			pushRewardData = allData[i];
			if (pushRewardData != null && dayOfWeek == pushRewardData.DayOfTheWeek)
			{
				list.Add(pushRewardData);
			}
		}
		return list;
	}
}
