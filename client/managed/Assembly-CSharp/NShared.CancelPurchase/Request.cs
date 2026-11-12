namespace NShared.CancelPurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/cancel_purchase";

	public string ProductId { get; set; }
}
