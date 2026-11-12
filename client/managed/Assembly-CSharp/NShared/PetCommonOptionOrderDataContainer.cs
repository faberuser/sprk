using NShared.NJit;

namespace NShared;

public class PetCommonOptionOrderDataContainer : JitDataContainerDefault<OperationType, PetCommonOptionOrderData>
{
	public const string TABLENAME = "PetCommonOptionOrderTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetCommonOptionOrderTable");
}
