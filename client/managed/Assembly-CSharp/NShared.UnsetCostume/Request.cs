namespace NShared.UnsetCostume;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/unset_costume";

	public int HeroIndex { get; set; }
}
