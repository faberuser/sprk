using NShared.NJit;

namespace NShared;

public class TeamLevelBuffOptionDataContainer : JitDataContainerDefault<int, TeamLevelBuffOptionData>
{
	public const string TABLENAME = "TeamLevelBuffOptionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TeamLevelBuffOptionTable");
}
