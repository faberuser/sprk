namespace NShared.LearnHeroSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/learn_hero_skill";

	public int HeroIndex { get; set; }

	public int SkillIndex { get; set; }
}
