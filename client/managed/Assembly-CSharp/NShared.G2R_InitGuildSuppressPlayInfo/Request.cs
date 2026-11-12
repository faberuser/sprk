namespace NShared.G2R_InitGuildSuppressPlayInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_init_guild_suppress_play_info";
}
