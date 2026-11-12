namespace NShared.GetKingsPassReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "kings_pass/get_kings_pass_reward";

	public int SeasonIndex { get; set; }

	public int MissionIndex { get; set; }
}
