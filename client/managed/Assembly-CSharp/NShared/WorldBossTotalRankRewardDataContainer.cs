using NShared.NJit;

namespace NShared;

public class WorldBossTotalRankRewardDataContainer : JitDataContainerDefault<int, WorldBossRewardData>
{
	public const string TABLENAME = "WorldBossTotalRankRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossTotalRankRewardTable");
}
