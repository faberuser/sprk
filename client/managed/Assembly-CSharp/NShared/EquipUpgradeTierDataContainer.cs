using NShared.NJit;

namespace NShared;

public class EquipUpgradeTierDataContainer : JitDataContainerDefault<string, EquipUpgradeTierData>
{
	public const string TABLENAME = "EquipUpgradeTierTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipUpgradeTierTable");

	public EquipUpgradeTierData GetData(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data == null)
		{
			return null;
		}
		return GetData(data.Code);
	}
}
