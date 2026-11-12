namespace NShared.GetPushReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "push_reward/get_push_reward";
}
