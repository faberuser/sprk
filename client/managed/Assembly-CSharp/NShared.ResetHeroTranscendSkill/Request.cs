namespace NShared.ResetHeroTranscendSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/reset_hero_transcend_skill";

	public int HeroIndex { get; set; }
}
