namespace NShared.GetServerGroupRanker;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ServerGroupRankerInfo[] Rankers { get; set; }
}
