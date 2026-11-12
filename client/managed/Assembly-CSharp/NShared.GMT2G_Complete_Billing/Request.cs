namespace NShared.GMT2G_Complete_Billing;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/gmt2g_complete_billing";

	public long AccountId { get; set; }

	public string Uid { get; set; }
}
