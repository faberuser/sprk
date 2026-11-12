using NShared.NJit;

namespace NShared;

public class GuildSuppressGlobalRewardDataContainer : JitDataContainerDefault<int, GuildSuppressRewardData>
{
	public const string TABLENAME = "GuildSuppressGlobalRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressGlobalRewardTable");
}
