namespace NShared.GetTowerNpcInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CachedAccountInfo[] TowerNpcInfos { get; set; }
}
