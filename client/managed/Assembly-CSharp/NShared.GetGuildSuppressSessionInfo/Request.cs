namespace NShared.GetGuildSuppressSessionInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_suppress/get_guild_suppress_session_info";
}
