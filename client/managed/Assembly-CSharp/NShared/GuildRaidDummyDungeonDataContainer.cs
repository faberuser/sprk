using NShared.NJit;

namespace NShared;

public class GuildRaidDummyDungeonDataContainer : JitDataContainerDefault<ChapterDungeonKey2, GuildRaidDummyDungeonData>
{
	public const string TABLENAME = "GuildRaidDummyDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildRaidDummyDungeonTable");
}
