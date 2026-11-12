namespace NShared.HeroLimitBreakExpUp;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/hero_limit_break_exp_up";

	public int HeroIndex { get; set; }

	public int[] ExpItemIndices { get; set; }

	public int[] ExpItemCounts { get; set; }
}
