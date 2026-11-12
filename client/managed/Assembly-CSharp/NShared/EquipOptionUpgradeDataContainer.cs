using NShared.NJit;

namespace NShared;

public class EquipOptionUpgradeDataContainer : JitDataContainerDefault<EquipOptionUpgradeDataKey, EquipOptionUpgradeData>
{
	public const string TABLENAME = "EquipOptionUpgradeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipOptionUpgradeTable");

	public EquipOptionUpgradeData GetData(int tier, int detailIndex)
	{
		return GetData(new EquipOptionUpgradeDataKey(tier, detailIndex));
	}
}
