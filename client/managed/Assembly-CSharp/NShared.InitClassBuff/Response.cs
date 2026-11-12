namespace NShared.InitClassBuff;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ClassBuffPointInfo[] ClassBuffPointInfos { get; set; }

	public ClassBuffInfo[] ClassBuffInfos { get; set; }
}
