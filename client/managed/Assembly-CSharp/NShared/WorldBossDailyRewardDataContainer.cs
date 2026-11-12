using NShared.NJit;

namespace NShared;

public class WorldBossDailyRewardDataContainer : JitDataContainerDefault<int, WorldBossRewardData>
{
	public const string TABLENAME = "WorldBossDailyRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldBossDailyRewardTable");
}
