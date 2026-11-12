namespace NShared.RewardEventRoulette;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "item/reward_event_roulette";

	public int RouletteIndex { get; set; }

	public int RouletteCount { get; set; }
}
