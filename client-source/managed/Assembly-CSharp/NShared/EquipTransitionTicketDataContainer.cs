using NShared.NJit;

namespace NShared;

public class EquipTransitionTicketDataContainer : JitDataContainerDefault<int, EquipTransitionTicketData>
{
	public const string TABLENAME = "EquipTransitionTicketTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EquipTransitionTicketTable");
}
