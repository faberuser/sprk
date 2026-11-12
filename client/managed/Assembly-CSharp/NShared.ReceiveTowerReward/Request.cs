namespace NShared.ReceiveTowerReward;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "campaign/receive_tower_reward";

	public int TowerIndex { get; set; }
}
