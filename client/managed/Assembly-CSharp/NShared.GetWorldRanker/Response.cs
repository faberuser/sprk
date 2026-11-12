namespace NShared.GetWorldRanker;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchRankerInfo[] Rankers { get; set; }
}
