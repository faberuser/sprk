namespace NShared.G2R_GetServerRanker;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public MatchRankerInfo[] Rankers { get; set; }
}
