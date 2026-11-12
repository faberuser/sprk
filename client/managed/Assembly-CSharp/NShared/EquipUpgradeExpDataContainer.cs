using NShared.NJit;
using NVespa;

namespace NShared;

public class EquipUpgradeExpDataContainer : JitDataContainerDefault<EquipUpgradeExpDataKey, EquipUpgradeExpData>
{
	public const string TABLENAME = "EquipUpgradeExpTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipUpgradeExpTable");

	public int GetGiveExp(EquipItemData equipItemData, EquipItemInfo equipItemInfo)
	{
		return GetGiveExp(equipItemData.PartType, equipItemData.Tier, equipItemData.DetailIndex, equipItemInfo.Level);
	}

	public int GetGiveExp(EquipPartType partType, int tier, int detailIndex, int level)
	{
		EquipUpgradeExpData data = GetData(new EquipUpgradeExpDataKey(partType, tier, detailIndex, level));
		if (data == null)
		{
			VespaDebug.LogError("Equip upgrade exp not found : (" + partType.ToString() + ", Tier=" + tier + ", DetailIndex=" + detailIndex + ", Level=" + level + ")");
			return 0;
		}
		return data.GiveExp;
	}

	public int GetReqExp(EquipItemData equipItemData, EquipItemInfo equipItemInfo)
	{
		return GetReqExp(equipItemData.PartType, equipItemData.Tier, equipItemData.DetailIndex, equipItemInfo.Level);
	}

	public int GetReqExp(EquipPartType partType, int tier, int detailIndex, int level)
	{
		EquipUpgradeExpData data = GetData(new EquipUpgradeExpDataKey(partType, tier, detailIndex, level));
		if (data == null)
		{
			VespaDebug.LogError("Equip upgrade exp not found : (" + partType.ToString() + ", Tier=" + tier + ", DetailIndex=" + detailIndex + ", Level=" + level + ")");
			return 0;
		}
		return data.ReqExp;
	}
}
