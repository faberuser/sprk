namespace NShared.CheckPurchaseOneStore;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/check_purchase_one_store";

	public string DeveloperPayload { get; set; }

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string OrderId { get; set; }

	public string PurchaseToken { get; set; }

	public string CurrencyCode { get; set; }

	public float Price { get; set; }

	public byte Resume { get; set; }
}
