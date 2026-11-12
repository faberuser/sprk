namespace NShared.ApplyHeroRunePage;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/apply_hero_rune_page";

	public int HeroIndex { get; set; }

	public int RunePage { get; set; }
}
