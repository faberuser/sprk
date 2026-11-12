namespace NShared.G2R_GetGuildSuppressCurrentSessionInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_guild_suppress_current_session_info";
}
