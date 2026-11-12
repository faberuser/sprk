namespace NShared.UpgradeHeroSkill;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/upgrade_hero_skill";

	public int HeroIndex { get; set; }

	public int[] SkillIndices { get; set; }

	public int[] TargetLevels { get; set; }
}
