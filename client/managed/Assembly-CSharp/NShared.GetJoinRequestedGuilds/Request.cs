namespace NShared.GetJoinRequestedGuilds;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/get_join_requested_guilds";
}
