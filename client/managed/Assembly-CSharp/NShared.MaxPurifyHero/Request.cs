namespace NShared.MaxPurifyHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/max_purify_hero";

	public int HeroIndex { get; set; }
}
