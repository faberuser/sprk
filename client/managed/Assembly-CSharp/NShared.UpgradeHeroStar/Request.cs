namespace NShared.UpgradeHeroStar;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/upgrade_hero_star";

	public int HeroIndex { get; set; }
}
