namespace NShared.GetServerRanker;

[Response]
public class Response : GenericResponse<ResultType>
{
	public MatchRankerInfo[] Rankers { get; set; }
}
