using NShared.NJit;

namespace NShared;

public class ItemBreakDataContainer : JitDataContainerDefault<int, ItemBreakData>
{
	public const string TABLENAME = "ItemBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ItemBreakTable");
}
