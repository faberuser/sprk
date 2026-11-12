namespace NShared.BuyHeroTranscendSkillPage;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/buy_hero_transcend_skill_page";

	public int HeroIndex { get; set; }

	public int PageIndex { get; set; }

	public int BuyGem { get; set; }
}
