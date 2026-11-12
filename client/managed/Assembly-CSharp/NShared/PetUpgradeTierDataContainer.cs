using NShared.NJit;

namespace NShared;

public class PetUpgradeTierDataContainer : JitDataContainerDefault<int, PetUpgradeTierData>
{
	public const string TABLENAME = "PetUpgradeTierTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetUpgradeTierTable");
}
