namespace NShared.G2R_DestroyGuild;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_destroy_guild";
}
