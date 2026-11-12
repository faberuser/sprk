using NShared.NJit;

namespace NShared;

public class GuildRaidBonusDungeonDataContainer : JitDataContainerDefault<GuildRaidBonusDungeonDataKey, GuildRaidBonusDungeonData>
{
	public const string TABLENAME = "GuildRaidBonusDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildRaidBonusDungeonTable");
}
