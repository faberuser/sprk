namespace NShared.ApplyForMultiplayReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/apply_for_multiplay_reward";

	public long MasterAccountId { get; set; }

	public string MasterServerIp { get; set; }

	public int MasterServerPort { get; set; }

	public int RewardItemNo { get; set; }
}
