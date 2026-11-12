namespace NShared.GetPushReward;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PushRewardInfo PushRewardInfo { get; set; }
}
