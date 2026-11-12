using NShared.NJit;

namespace NShared;

public class SoulWeaponUpgradeDataContainer : JitDataContainerDefault<int, SoulWeaponUpgradeData>
{
	public const string TABLENAME = "SoulWeaponUpgradeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponUpgradeTable");

	public int MaxGrade { get; private set; }

	protected override void OnEndLoad_Final()
	{
		foreach (SoulWeaponUpgradeData allDatum in GetAllData())
		{
			if (allDatum.GETKEY() > MaxGrade)
			{
				MaxGrade = allDatum.GETKEY();
			}
		}
	}
}
