using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewbieRewardTapOpenDataContainer : JitDataContainerDefault<int, NewbieRewardTapOpenData>
{
	public const string TABLENAME = "NewbieRewardTapOpenTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewbieRewardTapOpenTable");

	public NewbieRewardTapOpenData GetDataByRewardType(NewbieRewardType rewardType)
	{
		IList<NewbieRewardTapOpenData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		NewbieRewardTapOpenData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardTapOpenData newbieRewardTapOpenData = allData[i];
			if (newbieRewardTapOpenData != null && newbieRewardTapOpenData.TabType == rewardType)
			{
				result = newbieRewardTapOpenData;
			}
		}
		return result;
	}
}
