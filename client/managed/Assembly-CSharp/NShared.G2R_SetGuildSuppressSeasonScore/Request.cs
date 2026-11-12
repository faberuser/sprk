namespace NShared.G2R_SetGuildSuppressSeasonScore;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_set_guild_suppress_season_score";
}
