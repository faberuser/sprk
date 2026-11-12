namespace NShared.G2R_SetGuildSuppressGroupScore;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_guild_suppress_group_score";
}
