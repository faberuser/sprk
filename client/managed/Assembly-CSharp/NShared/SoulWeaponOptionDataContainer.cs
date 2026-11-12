using NShared.NJit;

namespace NShared;

public class SoulWeaponOptionDataContainer : JitDataContainerDefault<CreatureTagType, SoulWeaponOptionData>
{
	public const string TABLENAME = "SoulWeaponOptionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponOptionTable");
}
