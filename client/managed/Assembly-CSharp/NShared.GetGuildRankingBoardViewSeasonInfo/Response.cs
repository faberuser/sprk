namespace NShared.GetGuildRankingBoardViewSeasonInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public int ViewSeasonIndex { get; set; }
}
