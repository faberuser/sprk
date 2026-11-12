namespace NShared.SetCostume;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/set_costume";

	public int HeroIndex { get; set; }

	public int CostumeIndex { get; set; }
}
