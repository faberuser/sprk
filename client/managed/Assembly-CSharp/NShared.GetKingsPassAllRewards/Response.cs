namespace NShared.GetKingsPassAllRewards;

[Response]
public class Response : GenericResponse<ResultType>
{
	public RewardResultInfo[] RewardsResults { get; set; }

	public KingsPassMission[] KingsPassMissionInfos { get; set; }
}
