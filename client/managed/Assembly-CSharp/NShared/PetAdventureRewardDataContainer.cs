using NShared.NJit;

namespace NShared;

public class PetAdventureRewardDataContainer : JitDataContainerDefault<int, PetAdventureRewardData>
{
	public const string TABLENAME = "PetAdventureRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetAdventureRewardTable");
}
