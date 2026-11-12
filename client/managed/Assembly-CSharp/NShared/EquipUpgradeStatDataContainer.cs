using NShared.NContentsDefine;
using NShared.NJit;

namespace NShared;

public class EquipUpgradeStatDataContainer : JitDataContainerDefault<EquipUpgradeStatKey, EquipUpgradeStatData>
{
	public const string TABLENAME = "EquipUpgradeStatTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipUpgradeStatTable");

	public EquipUpgradeStatData GetData(int star, int detailIndex, int level)
	{
		star = NShared.NContentsDefine.SoulWeaponLimitBreak.ExceptionalMaxStar(star);
		if (star > 5)
		{
			star = 5;
		}
		return GetData(new EquipUpgradeStatKey(star, detailIndex, level));
	}
}
