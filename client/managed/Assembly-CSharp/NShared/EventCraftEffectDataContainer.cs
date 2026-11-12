using NShared.NJit;

namespace NShared;

public class EventCraftEffectDataContainer : JitDataContainerDefault<int, EventCraftEffectData>
{
	public const string TABLENAME = "EventCraftEffectTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EventCraftEffectTable");
}
