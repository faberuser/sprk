namespace NShared.AdminCheckConsume;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "cashshop/admin_check_consume";

	public string DeveloperPayload { get; set; }

	public string ProductId { get; set; }

	public string OrderId { get; set; }
}
