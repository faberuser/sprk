using NShared.NJit;

namespace NShared;

public class AccumulateLoginRewardDataContainer : JitDataContainerDefault<int, AccumulateLoginRewardData>
{
	public const string TABLENAME = "AccumulateLoginRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AccumulateLoginRewardTable");
}
