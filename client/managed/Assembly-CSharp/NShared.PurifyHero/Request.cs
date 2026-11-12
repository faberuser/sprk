namespace NShared.PurifyHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/purify_hero";

	public int HeroIndex { get; set; }
}
