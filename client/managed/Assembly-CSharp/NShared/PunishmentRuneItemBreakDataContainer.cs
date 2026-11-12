using NShared.NJit;

namespace NShared;

public class PunishmentRuneItemBreakDataContainer : JitDataContainerDefault<int, PunishmentRuneItemBreakData>
{
	public const string TABLENAME = "PunishmentRuneItemBreakTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PunishmentRuneItemBreakTable");
}
