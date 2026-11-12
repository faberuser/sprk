namespace NShared.InitPurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/init_purchase";

	public EPurchasePlatform Platform { get; set; }

	public string Channel { get; set; }

	public string PurchaseInfos { get; set; }

	public string SteamId { get; set; }
}
