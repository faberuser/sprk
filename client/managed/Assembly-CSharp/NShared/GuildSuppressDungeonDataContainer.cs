using NShared.NJit;

namespace NShared;

public class GuildSuppressDungeonDataContainer : JitDataContainerDefault<int, GuildSuppressDungeonData>
{
	public const string TABLENAME = "GuildSuppressDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildSuppressDungeonTable");
}
