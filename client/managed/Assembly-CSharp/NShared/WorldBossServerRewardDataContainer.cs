using NShared.NJit;

namespace NShared;

public class WorldBossServerRewardDataContainer : JitDataContainerDefault<int, WorldBossRewardData>
{
	public const string TABLENAME = "WorldBossServerRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossServerRewardTable");
}
