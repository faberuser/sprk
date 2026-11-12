namespace NShared.GetStarterPickupEquipGachaLastReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public StarterPickupEquipGachaInfo[] StarterPickupEquipGachaInfos { get; set; }

	public RewardResultInfo RewardResult { get; set; }
}
