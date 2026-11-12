namespace NShared.InitPurchase;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerPurchaseInfo[] ResumePurchaseInfos { get; set; }

	public PlayerPurchaseInfo[] CompletedPurchaseInfos { get; set; }

	public PlayerPurchaseInfo[] CSPurchaseInfos { get; set; }
}
