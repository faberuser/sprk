using NShared.NJit;

namespace NShared;

public class SoulWeaponUpgradePriceDataContainer : JitDataContainerDefault<SoulWeaponUpgradePriceKey, SoulWeaponUpgradePriceData>
{
	public const string TABLENAME = "SoulWeaponUpgradePriceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponUpgradePriceTable");

	public SoulWeaponUpgradePriceData GetData(int itemIndex, int grade)
	{
		return GetData(new SoulWeaponUpgradePriceKey(itemIndex, grade));
	}

	public SoulWeaponUpgradePriceData GetDataByLiberation(int itemIndex, int grade)
	{
		return GetData(itemIndex, grade);
	}
}
