namespace NShared.G2R_GetWorldRanker;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public MatchRankerInfo[] Rankers { get; set; }
}
