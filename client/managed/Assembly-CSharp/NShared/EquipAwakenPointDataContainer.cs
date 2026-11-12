using NShared.NJit;

namespace NShared;

public class EquipAwakenPointDataContainer : JitDataContainerDefault<int, EquipAwakenPointData>
{
	public const string TABLENAME = "EquipAwakenPointTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipAwakenPointTable");
}
