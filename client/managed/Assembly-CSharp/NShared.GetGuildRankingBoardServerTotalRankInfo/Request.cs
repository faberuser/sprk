namespace NShared.GetGuildRankingBoardServerTotalRankInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_ranking_board/get_guild_ranking_board_server_total_rank_info";

	public GuildRankingBoardType BoardType { get; set; }

	public GuildRankingType RankingType { get; set; }

	public int ViewSeasonIndex { get; set; }

	public long GuildId { get; set; }
}
