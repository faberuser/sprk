namespace NShared.LinkAccount;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/link_account";

	public LoginMethodType LoginMethod { get; set; }

	public string LinkedId { get; set; }

	public bool ForceLink { get; set; }

	public string LoginVerificationInfo { get; set; }

	public long AccountId { get; set; }
}
