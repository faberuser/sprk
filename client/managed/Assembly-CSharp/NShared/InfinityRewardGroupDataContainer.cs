using NShared.NJit;

namespace NShared;

public class InfinityRewardGroupDataContainer : JitDataContainerDefault<int, InfinityRewardGroupData>
{
	public const string TABLENAME = "InfinityRewardGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("InfinityRewardGroupTable");
}
