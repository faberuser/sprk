namespace NShared.ExtendHeroSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/extend_hero_skill";

	public int HeroIndex { get; set; }

	public int SkillIndex { get; set; }

	public int SkillExtend { get; set; }
}
