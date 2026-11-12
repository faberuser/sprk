using NShared.NJit;

namespace NShared;

public class PunishmentRuneOptionDataContainer : JitDataContainerDefault<int, PunishmentRuneOptionData>
{
	public const string TABLENAME = "PunishmentRuneOptionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRuneOptionTable");
}
