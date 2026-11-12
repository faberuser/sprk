namespace NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/g2r_get_guild_suppress_board_server_total_ranking_list";
}
