using NShared.NJit;

namespace NShared;

public class OrdealArenaEventDataContainer : JitDataContainerDefault<int, OrdealArenaEventData>
{
	public const string TABLENAME = "OrdealArenaEventTable";

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaEventTable");
}
