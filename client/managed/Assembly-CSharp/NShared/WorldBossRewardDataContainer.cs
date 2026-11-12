using NShared.NJit;

namespace NShared;

public class WorldBossRewardDataContainer : JitDataContainerDefault<int, WorldBossRewardData>
{
	public const string TABLENAME = "WorldBossRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossRewardTable");
}
