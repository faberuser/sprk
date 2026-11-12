using NShared.NJit;

namespace NShared;

public class AttendanceRewardInfoDataContainer : JitDataContainerDefault<AttendanceRewardInfoKey, AttendanceRewardInfoData>
{
	public const string TABLENAME = "AttendanceRewardInfoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AttendanceRewardInfoTable");

	public AttendanceRewardInfoData GetData(int index, int day, int no)
	{
		return GetData(new AttendanceRewardInfoKey(index, day, no));
	}
}
