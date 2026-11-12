namespace NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_guild_suppress_board_server_total_ranking_info";
}
