using NShared.NJit;

namespace NShared;

public class TeamLevelCommonOptionOrderDataContainer : JitDataContainerDefault<OperationType, TeamLevelCommonOptionOrderData>
{
	public const string TABLENAME = "TeamLevelCommonOptionOrderTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TeamLevelCommonOptionOrderTable");
}
