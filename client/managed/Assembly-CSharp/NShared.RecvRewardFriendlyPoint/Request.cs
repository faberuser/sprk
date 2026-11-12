namespace NShared.RecvRewardFriendlyPoint;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "npc/recv_reward_friendly_point";

	public int HeroIndex { get; set; }

	public int Step { get; set; }
}
