namespace NShared.HeroLimitBreakLevelUp;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/hero_limit_break_level_up";

	public int HeroIndex { get; set; }
}
