using NShared.NJit;

namespace NShared;

public class GuildSuppressServerRewardDataContainer : JitDataContainerDefault<int, GuildSuppressRewardData>
{
	public const string TABLENAME = "GuildSuppressServerRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressServerRewardTable");
}
