namespace NShared.RecruitHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/recruit_hero";

	public int HeroIndex { get; set; }
}
