using NShared.NJit;

namespace NShared;

public class TeamLevelBonusBuffOptionDataContainer : JitDataContainerDefault<int, TeamLevelBonusBuffOptionData>
{
	public const string TABLENAME = "TeamLevelBonusBuffOptionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TeamLevelBonusBuffOptionTable");
}
