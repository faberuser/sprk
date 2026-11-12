using NShared.NJit;

namespace NShared;

public class SoulWeaponAbilityTicketDataContainer : JitDataContainerDefault<string, SoulWeaponAbilityTicketData>
{
	public const string TABLENAME = "SoulWeaponAbilityTicketTable";

	public override TablePathDefine PathDefine => new TablePathDefine("SoulWeaponAbilityTicketTable");
}
