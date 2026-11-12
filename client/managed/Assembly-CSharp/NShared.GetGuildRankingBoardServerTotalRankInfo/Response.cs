namespace NShared.GetGuildRankingBoardServerTotalRankInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRankingBoardServerTotalRankingInfo RankInfo { get; set; }
}
