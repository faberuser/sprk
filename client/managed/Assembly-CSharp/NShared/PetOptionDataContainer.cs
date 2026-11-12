using NShared.NJit;

namespace NShared;

public class PetOptionDataContainer : JitDataContainerDefault<int, PetOptionData>
{
	public const string TABLENAME = "PetOptionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetOptionTable");
}
