using NShared.NJit;

namespace NShared;

public class EquipUpgradePriceDataContainer : JitDataContainerDefault<EquipUpgradePriceDataKey, EquipUpgradePriceData>
{
	public const string TABLENAME = "EquipUpgradePriceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipUpgradePriceTable");

	public int GetReqGold(int tier, int detailIndex, int level)
	{
		return GetData(new EquipUpgradePriceDataKey(tier, detailIndex, level))?.ReqGold ?? 0;
	}
}
