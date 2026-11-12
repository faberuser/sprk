namespace NShared.ResetAllCustomizingCostumes;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/reset_all_customizing_costumes";

	public int HeroIndex { get; set; }
}
