using NShared.NJit;

namespace NShared;

public class AttendanceUrlDataContainer : JitDataContainerDefault<AttendanceUrlDataKey, AttendanceUrlData>
{
	public const string TABLENAME = "AttendanceUrlTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AttendanceUrlTable");

	public AttendanceUrlData GetData(int index, string language)
	{
		return GetData(new AttendanceUrlDataKey(index, language));
	}
}
