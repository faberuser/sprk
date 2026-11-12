namespace NShared.G2R_DestroyGuildBasicInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_destroy_guild_basic_info";
}
