using NShared.NJit;

namespace NShared;

public class InfinityRewardDataContainer : JitDataContainerDefault<int, InfinityRewardData>
{
	public const string TABLENAME = "InfinityRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("InfinityRewardTable");
}
