namespace NShared.G2R_SetGuildSuppressPlayInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_guild_suppress_play_info";
}
