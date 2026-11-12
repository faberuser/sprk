using NShared.NJit;

namespace NShared;

public class PunishmentRuneItemCountDataContainer : JitDataContainerDefault<int, PunishmentRuneItemCountData>
{
	public const string TABLENAME = "PunishmentRuneItemCountTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRuneItemCountTable");
}
