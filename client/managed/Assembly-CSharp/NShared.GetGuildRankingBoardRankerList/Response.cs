namespace NShared.GetGuildRankingBoardRankerList;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRankingBoardRankerInfo[] RankerInfos { get; set; }

	public int ViewSeasonIndex { get; set; }
}
