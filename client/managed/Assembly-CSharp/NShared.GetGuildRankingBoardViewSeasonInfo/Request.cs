namespace NShared.GetGuildRankingBoardViewSeasonInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild_ranking_board/get_guild_ranking_view_season_info";

	public GuildRankingBoardType BoardType { get; set; }
}
