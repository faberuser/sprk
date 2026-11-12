namespace NShared.ExecIOSPayment;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/exec_ios_payment";

	public string ProductId { get; set; }

	public int ProductIndex { get; set; }

	public string TransactionId { get; set; }

	public string DeveloperPayload { get; set; }

	public string Receipt { get; set; }

	public string CurrencyCode { get; set; }

	public float Price { get; set; }

	public byte Resume { get; set; }
}
