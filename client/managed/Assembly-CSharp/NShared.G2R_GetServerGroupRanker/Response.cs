namespace NShared.G2R_GetServerGroupRanker;

[Response]
public class Response
{
	public ResultType Result { get; set; }

	public ServerGroupRankerInfo[] Rankers { get; set; }
}
