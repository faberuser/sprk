using NShared.NJit;

namespace NShared.NWorldMap;

public class EventDataContainer : JitDataContainerDefault<int, EventData>
{
	public const string TABLENAME = "WorldMapEquipTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WorldMapEquipTable");
}
