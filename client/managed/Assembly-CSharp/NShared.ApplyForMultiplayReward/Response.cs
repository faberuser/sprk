namespace NShared.ApplyForMultiplayReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public bool RewardCalculated { get; set; }
}
