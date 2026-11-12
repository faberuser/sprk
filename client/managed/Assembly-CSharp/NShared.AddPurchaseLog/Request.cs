namespace NShared.AddPurchaseLog;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/add_purchase_log";

	public string PurchaseStatus { get; set; }

	public string StatusDescription { get; set; }

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string PurchaseToken { get; set; }

	public string OrderId { get; set; }

	public string CurrencyCode { get; set; }

	public string Price { get; set; }

	public string Platform { get; set; }

	public string Channel { get; set; }

	public string DeveloperPayload { get; set; }

	public byte Resume { get; set; }
}
