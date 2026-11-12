namespace NShared.RefreshNode;

[Response]
public class Response : GenericResponse<ResultType>
{
	public OrdealNodeInfo OrdealNodeInfos { get; set; }

	public int RemainRefreshCount { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }
}
