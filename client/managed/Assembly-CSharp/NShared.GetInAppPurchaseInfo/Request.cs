namespace NShared.GetInAppPurchaseInfo;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/get_inapp_purchase_info";

	public EPurchasePlatform Platform { get; set; }

	public string DeveloperPayload { get; set; }

	public string ProductId { get; set; }

	public string TransactionId { get; set; }
}
