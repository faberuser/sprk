namespace NShared.BeginPurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/begin_purchase";

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public EPurchasePlatform Platform { get; set; }

	public string Channel { get; set; }

	public string ProductName { get; set; }
}
