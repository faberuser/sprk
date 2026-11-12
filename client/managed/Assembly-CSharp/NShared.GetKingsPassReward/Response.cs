namespace NShared.GetKingsPassReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public RewardResultInfo RewardsResult { get; set; }

	public KingsPassMission KingsPassMissionInfo { get; set; }
}
