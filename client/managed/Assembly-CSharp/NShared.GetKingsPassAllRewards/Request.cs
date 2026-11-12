namespace NShared.GetKingsPassAllRewards;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "kings_pass/get_kings_pass_all_rewards";

	public int SeasonIndex { get; set; }
}
