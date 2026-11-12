namespace NShared.GetMatchRanker;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchRankerInfo[] MatchRankers { get; set; }
}
