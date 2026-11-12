namespace NShared.OpenGuildBuilding;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/open_guild_building";
}
