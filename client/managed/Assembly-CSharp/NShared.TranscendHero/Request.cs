namespace NShared.TranscendHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/transcend_hero";

	public int HeroIndex { get; set; }
}
