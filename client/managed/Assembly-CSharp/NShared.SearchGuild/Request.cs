namespace NShared.SearchGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/search_guild";

	public string name { get; set; }
}
