using NShared.NJit;

namespace NShared;

public class GodkingTrialGroupDataContainer : JitDataContainerDefault<int, GodkingTrialGroupData>
{
	public const string TABLENAME = "GodkingTrialGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GodkingTrialGroupTable");
}
