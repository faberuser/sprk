using NShared.NJit;

namespace NShared;

public class PetAwakenDataContainer : JitDataContainerDefault<PetAwakenDataKey, PetAwakenData>
{
	public const string TABLENAME = "PetAwakenTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetAwakenTable");

	public PetAwakenData GetData(int petIndex, int star)
	{
		return GetData(new PetAwakenDataKey(petIndex, star));
	}
}
