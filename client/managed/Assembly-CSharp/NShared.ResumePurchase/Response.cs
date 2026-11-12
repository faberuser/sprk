namespace NShared.ResumePurchase;

[Response]
public class Response : GenericResponse<ResultType>
{
	public PlayerInAppPurchaseInfo InAppPurchaseInfo { get; set; }

	public string Nick { get; set; }

	public bool SignedOut { get; set; }
}
