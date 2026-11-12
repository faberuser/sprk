using NShared.NJit;

namespace NShared;

public class EquipSetBonusDataContainer : JitDataContainerDefault<int, EquipSetBonusData>
{
	public const string TABLENAME = "EquipSetBonusTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipSetBonusTable");
}
