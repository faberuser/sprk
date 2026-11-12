using NShared.NJit;

namespace NShared;

public class SoulWeaponReinforceDataContainer : JitDataContainerDefault<int, SoulWeaponReinforceData>
{
	public const string TABLENAME = "SoulWeaponReinforceTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponReinforceTable");
}
