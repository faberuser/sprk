namespace NShared.AbandonMultiplayReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool RewardCalculated { get; set; }
}
