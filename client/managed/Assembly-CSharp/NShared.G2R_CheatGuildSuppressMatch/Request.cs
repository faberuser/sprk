namespace NShared.G2R_CheatGuildSuppressMatch;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_cheat_guild_suppress_match";
}
