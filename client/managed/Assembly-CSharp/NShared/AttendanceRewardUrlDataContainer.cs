using NShared.NJit;

namespace NShared;

public class AttendanceRewardUrlDataContainer : JitDataContainerDefault<AttendanceRewardUrlKey, AttendanceRewardUrlData>
{
	public const string TABLENAME = "AttendanceRewardUrlTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AttendanceRewardUrlTable");

	public AttendanceRewardUrlData GetData(int index, int day, string language)
	{
		return GetData(new AttendanceRewardUrlKey(index, day, language));
	}
}
