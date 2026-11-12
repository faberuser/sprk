namespace NShared.CompletePurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/complete_purchase";

	public string ProductId { get; set; }

	public string PurchaseToken { get; set; }

	public string OrderId { get; set; }

	public byte Resume { get; set; }

	public EPurchasePlatform Platform { get; set; }

	public string Channel { get; set; }

	public string DeveloperPayload { get; set; }

	public int ProductIndex { get; set; }

	public string CurrencyCode { get; set; }

	public string Price { get; set; }

	public string ExtraInfo { get; set; }
}
