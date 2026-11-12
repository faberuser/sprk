namespace NShared.GetGuildRankingBoardRankerList;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_ranking_board/get_guild_ranking_board_ranker_list";

	public GuildRankingBoardType BoardType { get; set; }

	public GuildRankingType RankingType { get; set; }

	public int PageNo { get; set; }
}
