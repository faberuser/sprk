using NShared.NJit;

namespace NShared;

public class SoulWeaponEtherRateDataContainer : JitDataContainerDefault<SoulWeaponEtherRateKey, SoulWeaponEtherRateData>
{
	public const string TABLENAME = "SoulWeaponEtherRateTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponEtherRateTable");
}
