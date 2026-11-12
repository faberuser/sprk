using NShared.NJit;

namespace NShared;

public class AttendanceInfoDataContainer : JitDataContainerDefault<int, AttendanceInfoData>
{
	public const string TABLENAME = "AttendanceInfoTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AttendanceInfoTable");
}
