namespace NShared.G2R_GetGuildSuppressPlayInfos;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_guild_suppress_play_infos";
}
