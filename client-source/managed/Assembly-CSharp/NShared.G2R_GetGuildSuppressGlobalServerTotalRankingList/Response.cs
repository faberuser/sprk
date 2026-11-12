namespace NShared.G2R_GetGuildSuppressGlobalServerTotalRankingList;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildRankingBoardServerTotalRankingInfo[] Rankers { get; set; }
}
