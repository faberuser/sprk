namespace NShared.ResumePurchase;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "payment/resume_purchase";

	public EPurchasePlatform Platform { get; set; }

	public string Channel { get; set; }

	public string DeveloperPayload { get; set; }

	public string ProductId { get; set; }

	public string Token { get; set; }

	public string OrderId { get; set; }

	public string ExtraInfo { get; set; }
}
