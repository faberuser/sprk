using NShared.NJit;

namespace NShared;

public class GuildArenaServerBuffRewardDataContainer : JitDataContainerDefault<int, GuildArenaServerBuffRewardData>
{
	public const string TABLENAME = "GuildArenaServerBuffRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildArenaServerBuffRewardTable");
}
