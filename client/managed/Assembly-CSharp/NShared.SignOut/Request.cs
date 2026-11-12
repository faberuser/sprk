namespace NShared.SignOut;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/sign_out";

	public bool ForceSignOut { get; set; }
}
