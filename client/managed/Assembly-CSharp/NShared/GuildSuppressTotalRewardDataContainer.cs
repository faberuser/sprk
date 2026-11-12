using NShared.NJit;

namespace NShared;

public class GuildSuppressTotalRewardDataContainer : JitDataContainerDefault<int, GuildSuppressRewardData>
{
	public const string TABLENAME = "GuildSuppressTotalRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressTotalRewardTable");
}
