namespace NShared.DoHeroFriendly;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/do_hero_friendly";

	public int HeroIndex { get; set; }

	public FriendlyActionType Action { get; set; }
}
