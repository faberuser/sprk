using NShared.NJit;

namespace NShared;

public class EquipItemStarDataContainer : JitDataContainerDefault<int, EquipItemStarData>
{
	public const string TABLENAME = "EquipItemStarTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipItemStarTable");
}
