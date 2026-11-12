namespace NShared.LearnHeroTranscendSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/learn_hero_transcend_skill";

	public int HeroIndex { get; set; }

	public int TranscendSkill { get; set; }

	public int SkillIndex { get; set; }
}
