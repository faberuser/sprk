using NShared.NJit;

namespace NShared;

public class GuildSuppressRewardDataContainer : JitDataContainerDefault<int, GuildSuppressRewardData>
{
	public const string TABLENAME = "GuildSuppressRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressRewardTable");
}
