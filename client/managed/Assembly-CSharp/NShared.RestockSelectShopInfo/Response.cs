namespace NShared.RestockSelectShopInfo;

[Response]
public class Response : GenericResponse<ResultType>
{
	public SelectShopInfo SelectShopInfo { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }
}
