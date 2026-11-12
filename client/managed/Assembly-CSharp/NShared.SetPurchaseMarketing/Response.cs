namespace NShared.SetPurchaseMarketing;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PurchaseMarketingResultInfo[] PurchaseMarketingResult { get; set; }
}
