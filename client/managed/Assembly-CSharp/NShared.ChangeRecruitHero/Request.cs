namespace NShared.ChangeRecruitHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/change_recruit_hero";

	public int HeroIndex { get; set; }
}
