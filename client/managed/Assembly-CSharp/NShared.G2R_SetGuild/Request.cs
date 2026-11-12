namespace NShared.G2R_SetGuild;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_guild";
}
