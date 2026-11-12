namespace NShared.G2R_GetGuildSuppressGlobalServerTotalRankInfo;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public GuildRankingBoardServerTotalRankingInfo Info { get; set; }
}
