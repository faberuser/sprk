namespace NShared.GetGuildRankingBoardRankInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRankingBoardRankerInfo RankInfo { get; set; }

	public int TotalRankerCount { get; set; }
}
