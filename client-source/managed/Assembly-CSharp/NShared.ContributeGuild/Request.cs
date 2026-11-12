namespace NShared.ContributeGuild;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/contribute_guild";
}
