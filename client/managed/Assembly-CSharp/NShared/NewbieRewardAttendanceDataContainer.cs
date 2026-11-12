using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NewbieRewardAttendanceDataContainer : JitDataContainerDefault<int, NewbieRewardAttendanceData>
{
	public const string TABLENAME = "NewbieRewardAttendanceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewbieRewardAttendanceTable");

	public bool IsNewbieRewardAttendanceData(int attendanceIndex)
	{
		bool result = false;
		IList<NewbieRewardAttendanceData> allData = GetAllData();
		if (allData == null)
		{
			return result;
		}
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardAttendanceData newbieRewardAttendanceData = allData[i];
			if (newbieRewardAttendanceData != null && newbieRewardAttendanceData.Index == attendanceIndex)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public NewbieRewardAttendanceData GetDataByTabType(NewbieRewardType tabType)
	{
		IList<NewbieRewardAttendanceData> allData = GetAllData();
		if (allData == null)
		{
			return null;
		}
		NewbieRewardAttendanceData result = null;
		for (int i = 0; i < allData.Count; i++)
		{
			NewbieRewardAttendanceData newbieRewardAttendanceData = allData[i];
			if (newbieRewardAttendanceData != null && newbieRewardAttendanceData.TabType == tabType)
			{
				result = newbieRewardAttendanceData;
				break;
			}
		}
		return result;
	}
}
