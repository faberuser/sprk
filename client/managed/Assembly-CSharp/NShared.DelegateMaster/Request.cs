namespace NShared.DelegateMaster;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/delegate_master";

	public long DelegateAccountId { get; set; }
}
