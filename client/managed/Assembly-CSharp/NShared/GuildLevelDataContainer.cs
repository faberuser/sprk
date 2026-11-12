using NShared.NJit;

namespace NShared;

public class GuildLevelDataContainer : JitDataContainerDefault<int, GuildLevelData>
{
	public const string TABLENAME = "GuildLevelTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildLevelTable");
}
