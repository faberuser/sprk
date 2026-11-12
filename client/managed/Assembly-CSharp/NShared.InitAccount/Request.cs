namespace NShared.InitAccount;

[Request]
public class Request : IServiceRequest
{
	string IServiceRequest.ServiceName => "internal/init_account";

	public long AccountId { get; set; }
}
