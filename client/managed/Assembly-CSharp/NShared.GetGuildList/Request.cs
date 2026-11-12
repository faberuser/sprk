namespace NShared.GetGuildList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_guildlist";
}
