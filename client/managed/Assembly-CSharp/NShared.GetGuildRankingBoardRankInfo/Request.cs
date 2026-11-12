namespace NShared.GetGuildRankingBoardRankInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_ranking_board/get_guild_ranking_board_rank_info";

	public GuildRankingBoardType BoardType { get; set; }

	public GuildRankingType RankingType { get; set; }
}
