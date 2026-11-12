namespace NShared.GetLoginDailyReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "logindaily/get_logindaily_reward";

	public int LoginDailyIndex { get; set; }
}
