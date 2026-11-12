using NShared.NJit;

namespace NShared;

public class GuildContributeRewardDataContainer : JitDataContainerDefault<int, GuildContributeRewardData>
{
	public const string TABLENAME = "GuildContributeRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildContributeRewardTable");
}
