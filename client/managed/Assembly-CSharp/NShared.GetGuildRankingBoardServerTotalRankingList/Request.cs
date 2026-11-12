namespace NShared.GetGuildRankingBoardServerTotalRankingList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_ranking_board/get_guild_ranking_board_server_total_ranking_list";

	public int PageNo { get; set; }

	public int ViewSeasonIndex { get; set; }
}
