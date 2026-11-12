namespace NShared.SetEclipseDeck;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "eclipse/set_eclipse_deck";

	public string HeroInfos { get; set; }
}
