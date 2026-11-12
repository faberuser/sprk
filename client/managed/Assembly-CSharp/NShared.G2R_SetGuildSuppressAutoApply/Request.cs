namespace NShared.G2R_SetGuildSuppressAutoApply;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_guild_suppress_auto_apply";
}
