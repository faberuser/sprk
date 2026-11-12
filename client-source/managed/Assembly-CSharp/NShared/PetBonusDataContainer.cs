using NShared.NJit;

namespace NShared;

public class PetBonusDataContainer : JitDataContainerDefault<int, PetBonusData>
{
	public const string TABLENAME = "PetBonusTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetBonusTable");
}
