namespace NShared.GiveRewardHeroInnRoulette;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/give_reward_hero_inn_roulette";

	public int RouletteIndex { get; set; }
}
