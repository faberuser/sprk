namespace NShared.AbandonMultiplayReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	public virtual string ServiceName => "campaign/abandon_multiplay_reward";

	public long MasterAccountId { get; set; }

	public string MasterServerIp { get; set; }

	public int MasterServerPort { get; set; }
}
