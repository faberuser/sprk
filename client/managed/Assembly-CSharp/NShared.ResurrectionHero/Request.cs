namespace NShared.ResurrectionHero;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "ordeal_arena/resurrection_hero";

	public int HeroIndex { get; set; }
}
