namespace NShared.GiveRewardMaxClosenessHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/give_reward_max_closeness_hero";

	public int[] HeroIndices { get; set; }
}
