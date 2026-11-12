using NShared.NJit;

namespace NShared;

public class WorldBossGlobalRewardDataContainer : JitDataContainerDefault<int, WorldBossRewardData>
{
	public const string TABLENAME = "WorldBossGlobalRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossGlobalRewardTable");
}
