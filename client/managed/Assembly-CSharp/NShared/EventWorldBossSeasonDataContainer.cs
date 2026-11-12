using NShared.NJit;

namespace NShared;

public class EventWorldBossSeasonDataContainer : JitDataContainerDefault<int, EventWorldBossSeasonData>
{
	public const string TABLENAME = "EventWorldBossSeasonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EventWorldBossSeasonTable");
}
