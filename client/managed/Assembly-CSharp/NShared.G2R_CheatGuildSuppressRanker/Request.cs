namespace NShared.G2R_CheatGuildSuppressRanker;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_cheat_guild_suppress_ranker";
}
