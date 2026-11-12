using NShared.NJit;

namespace NShared;

public class MazeClearRewardDataContainer : JitDataContainerDefault<int, MazeClearRewardData>
{
	public const string TABLENAME = "MazeClearRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MazeClearRewardTable");
}
