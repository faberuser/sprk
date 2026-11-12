using NShared.NJit;

namespace NShared;

public class PetIncubatorDataContainer : JitDataContainerDefault<int, PetIncubatorData>
{
	public const string TABLENAME = "PetIncubatorTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetIncubatorTable");
}
