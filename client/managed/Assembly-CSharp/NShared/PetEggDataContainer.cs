using NShared.NJit;

namespace NShared;

public class PetEggDataContainer : JitDataContainerDefault<int, PetEggData>
{
	public const string TABLENAME = "PetEggTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetEggTable");
}
