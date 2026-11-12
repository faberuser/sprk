namespace NShared.ExtendRunePage;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "hero/extend_rune_page";

	public int HeroIndex { get; set; }

	public int RunePage { get; set; }
}
