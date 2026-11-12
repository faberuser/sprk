namespace NShared.ResetHeroTranscendSkillPage;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/reset_hero_transcend_skill_page";

	public int HeroIndex { get; set; }

	public int PageIndex { get; set; }
}
