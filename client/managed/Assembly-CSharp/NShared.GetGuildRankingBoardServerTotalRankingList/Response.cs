namespace NShared.GetGuildRankingBoardServerTotalRankingList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRankingBoardServerTotalRankingInfo[] RankingInfos { get; set; }
}
