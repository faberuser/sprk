namespace NShared.ReceivePushReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "push_reward/receive_push_reward";
}
