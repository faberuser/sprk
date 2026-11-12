using NShared.NJit;

namespace NShared;

public class EventRouletteDataContainer : JitDataContainerDefault<int, EventRouletteData>
{
	public const string TABLENAME = "EventRouletteTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EventRouletteTable");
}
