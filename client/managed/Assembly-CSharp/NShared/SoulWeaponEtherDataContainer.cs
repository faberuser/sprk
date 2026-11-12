using NShared.NJit;

namespace NShared;

public class SoulWeaponEtherDataContainer : JitDataContainerDefault<string, SoulWeaponEtherData>
{
	public const string TABLENAME = "SoulWeaponEtherTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponEtherTable");
}
