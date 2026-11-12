namespace NShared.GetMultiplayReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/get_multiplay_reward";

	public long MasterAccountId { get; set; }

	public string MasterServerIp { get; set; }

	public int MasterServerPort { get; set; }
}
