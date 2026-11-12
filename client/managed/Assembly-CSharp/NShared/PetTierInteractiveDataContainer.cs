using NShared.NJit;

namespace NShared;

public class PetTierInteractiveDataContainer : JitDataContainerDefault<int, PetTierInteractiveData>
{
	public const string TABLENAME = "PetTierInteractiveTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PetTierInteractiveTable");

	public int DefaultAddHappinessPoint => 10;

	public int GetAddHappinessPoint(int grade, PetActionType petActionType)
	{
		PetTierInteractiveData data = GetData(grade);
		if (data == null)
		{
			return DefaultAddHappinessPoint;
		}
		return petActionType switch
		{
			PetActionType.Feed => data.PetFeedAddHappinessPoint, 
			PetActionType.Play => data.PetPlayAddHappinesPoint, 
			PetActionType.Pat => data.PetPatAddHappinessPoint, 
			_ => DefaultAddHappinessPoint, 
		};
	}
}
